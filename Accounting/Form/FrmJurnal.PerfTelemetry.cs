using Serilog;
using System;
using System.Diagnostics;
using System.Threading;

namespace Accounting.Form
{
    public partial class FrmJurnal
    {
        private static readonly bool PerfTelemetryEnabled =
            !string.Equals(Environment.GetEnvironmentVariable("ACCOUNTING_DISABLE_JURNAL_PERF_LOG"), "1", StringComparison.OrdinalIgnoreCase);

        private IDisposable BeginPerfMeasurement(string operation, Func<string>? detailsFactory = null)
        {
            if (!PerfTelemetryEnabled)
            {
                return NoopDisposable.Instance;
            }

            return new PerfScope(operation, detailsFactory);
        }

        private sealed class PerfScope : IDisposable
        {
            private readonly string operation;
            private readonly Func<string>? detailsFactory;
            private readonly Stopwatch stopwatch;
            private int disposed;

            public PerfScope(string operation, Func<string>? detailsFactory)
            {
                this.operation = operation;
                this.detailsFactory = detailsFactory;
                stopwatch = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref disposed, 1) != 0)
                {
                    return;
                }

                stopwatch.Stop();

                string details = string.Empty;
                if (detailsFactory != null)
                {
                    try
                    {
                        details = detailsFactory.Invoke() ?? string.Empty;
                    }
                    catch (Exception ex)
                    {
                        details = $"details_error={ex.Message}";
                    }
                }

                if (string.IsNullOrWhiteSpace(details))
                {
                    Log.Information("PERF {Operation} elapsed_ms={ElapsedMs}", operation, stopwatch.ElapsedMilliseconds);
                    return;
                }

                Log.Information("PERF {Operation} elapsed_ms={ElapsedMs} details={Details}", operation, stopwatch.ElapsedMilliseconds, details);
            }
        }

        private sealed class NoopDisposable : IDisposable
        {
            public static readonly NoopDisposable Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
