using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Accounting.Form
{
    public partial class FrmJurnal
    {
        private readonly object overlaySync = new();
        private IOverlaySplashScreenHandle? overlayHandle;
        private int overlayDepth;

        private CancellationTokenSource? periodeReloadCts;
        private CancellationTokenSource? monthlySearchCts;
        private CancellationTokenSource? yearlySearchCts;
        private CancellationTokenSource? aisReloadCts;
        private CancellationTokenSource? hrisReloadCts;
        private CancellationTokenSource? payrollUmumReloadCts;

        private const int DefaultDebounceDelayMs = 300;

        private IDisposable BeginGlobalLoadingScope()
        {
            lock (overlaySync)
            {
                overlayDepth++;

                if (overlayDepth == 1 && !IsDisposed && IsHandleCreated)
                {
                    overlayHandle = SplashScreenManager.ShowOverlayForm(this);
                    overlayHandle.QueueFocus(IntPtr.Zero);
                }
            }

            return new GlobalLoadingScope(this);
        }

        private void EndGlobalLoadingScope()
        {
            lock (overlaySync)
            {
                if (overlayDepth <= 0)
                {
                    return;
                }

                overlayDepth--;
                if (overlayDepth > 0 || overlayHandle == null)
                {
                    return;
                }

                try
                {
                    SplashScreenManager.CloseOverlayForm(overlayHandle);
                }
                catch
                {
                    // Ignore overlay close failures during teardown.
                }
                finally
                {
                    overlayHandle = null;
                }
            }
        }

        private void DisposeLoadingInfrastructure()
        {
            CancelAndDispose(ref periodeReloadCts);
            CancelAndDispose(ref monthlySearchCts);
            CancelAndDispose(ref yearlySearchCts);
            CancelAndDispose(ref aisReloadCts);
            CancelAndDispose(ref hrisReloadCts);
            CancelAndDispose(ref payrollUmumReloadCts);

            lock (overlaySync)
            {
                overlayDepth = 0;
                if (overlayHandle == null)
                {
                    return;
                }

                try
                {
                    SplashScreenManager.CloseOverlayForm(overlayHandle);
                }
                catch
                {
                    // Ignore overlay close failures during teardown.
                }
                finally
                {
                    overlayHandle = null;
                }
            }
        }

        private void ScheduleDebounced(
            ref CancellationTokenSource? cancellationField,
            Func<CancellationToken, Task> operation,
            string operationName,
            int delayMs = DefaultDebounceDelayMs)
        {
            CancelAndDispose(ref cancellationField);

            CancellationTokenSource nextCts = new();
            cancellationField = nextCts;
            _ = ExecuteDebouncedAsync(nextCts, operation, operationName, delayMs);
        }

        private async Task ExecuteDebouncedAsync(
            CancellationTokenSource cancellation,
            Func<CancellationToken, Task> operation,
            string operationName,
            int delayMs)
        {
            // Capture the token up front. A subsequent debounced call disposes this
            // CancellationTokenSource via CancelAndDispose; reading cancellation.Token
            // after that point would throw ObjectDisposedException.
            CancellationToken token = cancellation.Token;

            try
            {
                await Task.Delay(delayMs, token);
                await operation(token);
            }
            catch (OperationCanceledException)
            {
                // Expected during quick user input changes.
            }
            catch (ObjectDisposedException)
            {
                // The CancellationTokenSource was disposed by a newer debounced call; treat as cancelled.
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                {
                    XtraMessageBox.Show($"An error occurred while {operationName}: {ex.Message}", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Hand);
                }
            }
            finally
            {
                cancellation.Dispose();
            }
        }

        private static void CancelAndDispose(ref CancellationTokenSource? cancellation)
        {
            if (cancellation == null)
            {
                return;
            }

            try
            {
                if (!cancellation.IsCancellationRequested)
                {
                    cancellation.Cancel();
                }
            }
            catch (ObjectDisposedException)
            {
                // Already disposed.
            }
            finally
            {
                cancellation.Dispose();
                cancellation = null;
            }
        }

        private sealed class GlobalLoadingScope : IDisposable
        {
            private FrmJurnal? owner;

            public GlobalLoadingScope(FrmJurnal owner)
            {
                this.owner = owner;
            }

            public void Dispose()
            {
                FrmJurnal? localOwner = Interlocked.Exchange(ref owner, null);
                localOwner?.EndGlobalLoadingScope();
            }
        }
    }
}
