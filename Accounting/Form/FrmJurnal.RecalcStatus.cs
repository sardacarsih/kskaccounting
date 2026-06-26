using Accounting.BusinessLayer;
using DevExpress.XtraEditors;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Accounting.Form
{
    // Lightweight, non-blocking monitor for the async saldo recalculation job that
    // is queued after a jurnal is saved/updated/deleted. Its primary purpose is to
    // surface a permanently FAILED recalc to the user who made the change, so a
    // wrong balance is never left silent. DONE simply stops the watch.
    public partial class FrmJurnal
    {
        private readonly Timer recalcStatusTimer = new() { Interval = 2000 };
        private long? monitoredRecalcJobId;
        private DateTime monitoredRecalcStartUtc;
        private bool isRecalcStatusCheckInProgress;
        private bool recalcStatusWired;
        private const int RecalcStatusTimeoutSeconds = 180;

        private void MonitorRecalcJob(long jobId)
        {
            if (jobId <= 0)
            {
                return;
            }

            if (!recalcStatusWired)
            {
                recalcStatusTimer.Tick += RecalcStatusTimer_Tick;
                FormClosed += (_, _) =>
                {
                    recalcStatusTimer.Stop();
                    recalcStatusTimer.Dispose();
                };
                recalcStatusWired = true;
            }

            monitoredRecalcJobId = jobId;
            monitoredRecalcStartUtc = DateTime.UtcNow;
            recalcStatusTimer.Start();
        }

        private async void RecalcStatusTimer_Tick(object? sender, EventArgs e)
        {
            if (isRecalcStatusCheckInProgress)
            {
                return;
            }

            if (!monitoredRecalcJobId.HasValue)
            {
                recalcStatusTimer.Stop();
                return;
            }

            if ((DateTime.UtcNow - monitoredRecalcStartUtc).TotalSeconds > RecalcStatusTimeoutSeconds)
            {
                recalcStatusTimer.Stop();
                long timedOutJobId = monitoredRecalcJobId.Value;
                monitoredRecalcJobId = null;
                Log.Warning("COA RecalcWatch watch_timeout form=FrmJurnal job_id={JobId} timeout_seconds={TimeoutSeconds}", timedOutJobId, RecalcStatusTimeoutSeconds);
                return;
            }

            isRecalcStatusCheckInProgress = true;
            try
            {
                long jobId = monitoredRecalcJobId.Value;
                RekalkulasiJobStatusSnapshot? status = await Task.Run(() => JurnalInputOperationService.GetRekalkulasiJobStatus(jobId));
                if (status == null)
                {
                    return;
                }

                string normalizedStatus = (status.Status ?? string.Empty).Trim().ToUpperInvariant();
                if (normalizedStatus == "DONE")
                {
                    recalcStatusTimer.Stop();
                    monitoredRecalcJobId = null;
                    Log.Information("COA RecalcWatch status_done form=FrmJurnal job_id={JobId}", jobId);
                    return;
                }

                if (normalizedStatus == "FAILED")
                {
                    recalcStatusTimer.Stop();
                    monitoredRecalcJobId = null;
                    Log.Warning("COA RecalcWatch status_failed form=FrmJurnal job_id={JobId} error={LastError}", jobId, status.LastError);
                    if (!IsDisposed)
                    {
                        XtraMessageBox.Show(
                            "Perhitungan ulang saldo COA untuk jurnal yang baru disimpan GAGAL diproses.\n\n" +
                            "Saldo pada periode ini mungkin belum akurat. Silakan jalankan ulang rekalkulasi " +
                            "(atau hubungi admin) sebelum menggunakan laporan periode tersebut.",
                            "Rekalkulasi Saldo Gagal",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "COA RecalcWatch status_check_exception form=FrmJurnal");
            }
            finally
            {
                isRecalcStatusCheckInProgress = false;
            }
        }
    }
}
