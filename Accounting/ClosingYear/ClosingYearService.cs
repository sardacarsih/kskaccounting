using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Oracle.ManagedDataAccess.Client;

namespace Accounting.ClosingYear;

public sealed partial class ClosingYearService(IClosingYearRepository repository)
{
    // ORA error codes raised by ACCT_CLOSING_YEAR_V2.CLOSE_YEAR. These are the stable contract between
    // the orchestrator and this mapper; message text is only a fallback for older/localized messages.
    private const int PeriodLockedOraCode = 20310;
    private const int NeracaNotBalancedOraCode = 20311;

    public ClosingYearResult CloseYear(ClosingYearRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return repository.CloseYear(request);
        }
        catch (Exception ex) when (TryMapBusinessException(request, ex, out ClosingYearResult result))
        {
            return result;
        }
    }

    private static bool TryMapBusinessException(ClosingYearRequest request, Exception exception, out ClosingYearResult result)
    {
        string message = exception.Message ?? string.Empty;
        string period = $"12/{request.Year:0000}";
        int oraCode = GetOraCode(exception, message);

        if (oraCode == PeriodLockedOraCode
            || message.Contains("PERIOD_LOCKED", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Telah Dikunci", StringComparison.OrdinalIgnoreCase))
        {
            result = ClosingYearResult.LockedPeriod(request.Year, period);
            return true;
        }

        if (oraCode == NeracaNotBalancedOraCode
            || message.Contains("NERACA_NOT_BALANCED", StringComparison.OrdinalIgnoreCase))
        {
            decimal selisih = ExtractDecimal(message);
            result = ClosingYearResult.NotBalanced(request.Year, period, selisih);
            return true;
        }

        result = default!;
        return false;
    }

    private static int GetOraCode(Exception exception, string message)
    {
        if (exception is OracleException oracleException)
        {
            return oracleException.Number;
        }

        Match match = OraCodePattern().Match(message);
        return match.Success && int.TryParse(match.Groups[1].Value, out int code) ? code : 0;
    }

    private static decimal ExtractDecimal(string message)
    {
        int markerIndex = message.IndexOf("NERACA_NOT_BALANCED", StringComparison.OrdinalIgnoreCase);
        // Prefer the value after the business marker. When the marker is absent, strip the leading
        // ORA-<code> token first so the error code is not mistaken for the selisih.
        string valueSource = markerIndex >= 0
            ? message[(markerIndex + "NERACA_NOT_BALANCED".Length)..]
            : OraCodePattern().Replace(message, string.Empty);
        Match match = DecimalPattern().Match(valueSource);
        if (!match.Success)
        {
            return 0m;
        }

        return decimal.TryParse(match.Value, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out decimal value)
            ? value
            : 0m;
    }

    [GeneratedRegex(@"[-+]?\d+(\.\d+)?")]
    private static partial Regex DecimalPattern();

    [GeneratedRegex(@"ORA-(\d+)")]
    private static partial Regex OraCodePattern();
}

