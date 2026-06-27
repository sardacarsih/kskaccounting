namespace Accounting.Services
{
    public enum LabaRugiReadinessFailure
    {
        None,
        MissingJournal,
        NotBalanced
    }

    public sealed class LabaRugiReadinessResult
    {
        private LabaRugiReadinessResult(bool isReady, LabaRugiReadinessFailure failure, string message, decimal selisih)
        {
            IsReady = isReady;
            Failure = failure;
            Message = message;
            Selisih = selisih;
        }

        public bool IsReady { get; }
        public LabaRugiReadinessFailure Failure { get; }
        public string Message { get; }
        public decimal Selisih { get; }

        public static LabaRugiReadinessResult Ready()
        {
            return new LabaRugiReadinessResult(true, LabaRugiReadinessFailure.None, string.Empty, 0m);
        }

        public static LabaRugiReadinessResult MissingJournal()
        {
            return new LabaRugiReadinessResult(false, LabaRugiReadinessFailure.MissingJournal, "Belum ada transaksi jurnal", 0m);
        }

        public static LabaRugiReadinessResult NotBalanced(string periode, decimal selisih)
        {
            string message = $"Laporan Laba/Rugi tidak dapat dibuat karena periode {periode} belum balance. Selisih: {selisih:n2}";
            return new LabaRugiReadinessResult(false, LabaRugiReadinessFailure.NotBalanced, message, selisih);
        }
    }
}
