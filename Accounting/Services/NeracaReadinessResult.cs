namespace Accounting.Services
{
    public enum NeracaReadinessFailure
    {
        None,
        MissingJournal,
        NotBalanced
    }

    public sealed class NeracaReadinessResult
    {
        private NeracaReadinessResult(bool isReady, NeracaReadinessFailure failure, string message, decimal selisih)
        {
            IsReady = isReady;
            Failure = failure;
            Message = message;
            Selisih = selisih;
        }

        public bool IsReady { get; }
        public NeracaReadinessFailure Failure { get; }
        public string Message { get; }
        public decimal Selisih { get; }

        public static NeracaReadinessResult Ready()
        {
            return new NeracaReadinessResult(true, NeracaReadinessFailure.None, string.Empty, 0m);
        }

        public static NeracaReadinessResult MissingJournal()
        {
            return new NeracaReadinessResult(false, NeracaReadinessFailure.MissingJournal, "Belum ada transaksi jurnal", 0m);
        }

        public static NeracaReadinessResult NotBalanced(string periode, decimal selisih)
        {
            string message = $"Laporan Neraca tidak dapat dibuat karena periode {periode} belum balance. Selisih: {selisih:n2}";
            return new NeracaReadinessResult(false, NeracaReadinessFailure.NotBalanced, message, selisih);
        }
    }
}
