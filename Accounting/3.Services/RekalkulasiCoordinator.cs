using Serilog;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Accounting.BusinessLayer
{
    internal readonly record struct RekalkulasiRequestKey(string IdData, string Periode, double JurnalId);

    internal readonly record struct RekalkulasiWorkItem(
        RekalkulasiRequestKey Key,
        int GlMonth,
        int GlYear,
        string UserId,
        DateTime EnqueuedAtUtc);

    internal sealed class RekalkulasiCoordinator
    {
        private readonly ConcurrentDictionary<RekalkulasiRequestKey, RekalkulasiWorkItem> pendingByKey = new();
        private readonly ConcurrentDictionary<RekalkulasiRequestKey, byte> inProgressKeys = new();
        private readonly ConcurrentQueue<RekalkulasiRequestKey> queue = new();
        private readonly SemaphoreSlim signal = new(0);
        private readonly Func<RekalkulasiWorkItem, Task> processor;

        public RekalkulasiCoordinator(Func<RekalkulasiWorkItem, Task> processor, int workerCount)
        {
            this.processor = processor ?? throw new ArgumentNullException(nameof(processor));

            int sanitizedWorkerCount = Math.Max(1, workerCount);
            for (int i = 0; i < sanitizedWorkerCount; i++)
            {
                int workerId = i + 1;
                _ = Task.Run(() => WorkerLoop(workerId));
            }
        }

        public void Enqueue(RekalkulasiWorkItem workItem)
        {
            bool isNewKey = pendingByKey.TryAdd(workItem.Key, workItem);
            if (!isNewKey)
            {
                pendingByKey[workItem.Key] = workItem;
                Log.Information(
                    "PERF RekalkulasiCoordinator dedup_hit key={Key} pending_count={PendingCount}",
                    workItem.Key,
                    pendingByKey.Count);
                return;
            }

            queue.Enqueue(workItem.Key);
            signal.Release();

            Log.Information(
                "PERF RekalkulasiCoordinator queued key={Key} queue_depth={QueueDepth} pending_count={PendingCount}",
                workItem.Key,
                queue.Count,
                pendingByKey.Count);
        }

        private async Task WorkerLoop(int workerId)
        {
            while (true)
            {
                await signal.WaitAsync().ConfigureAwait(false);
                if (!queue.TryDequeue(out RekalkulasiRequestKey key))
                {
                    continue;
                }

                if (inProgressKeys.ContainsKey(key))
                {
                    Log.Information("PERF RekalkulasiCoordinator skipped_stale key={Key} reason=in_progress", key);
                    continue;
                }

                if (!pendingByKey.TryRemove(key, out RekalkulasiWorkItem workItem))
                {
                    continue;
                }

                if (!inProgressKeys.TryAdd(key, 0))
                {
                    pendingByKey[key] = workItem;
                    continue;
                }

                try
                {
                    Log.Information("PERF RekalkulasiCoordinator started key={Key} worker={WorkerId}", key, workerId);
                    await processor(workItem).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "PERF RekalkulasiCoordinator failed key={Key} worker={WorkerId}", key, workerId);
                }
                finally
                {
                    inProgressKeys.TryRemove(key, out _);

                    if (pendingByKey.ContainsKey(key))
                    {
                        queue.Enqueue(key);
                        signal.Release();
                        Log.Information("PERF RekalkulasiCoordinator requeued_latest key={Key}", key);
                    }
                }
            }
        }
    }
}
