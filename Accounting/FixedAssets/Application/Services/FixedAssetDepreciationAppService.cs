using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Accounting.FixedAssets.Application.Contracts;
using Accounting.FixedAssets.Application.Models;
using Accounting.FixedAssets.Domain;
using Accounting.FixedAssets.Domain.Entities;

namespace Accounting.FixedAssets.Application.Services;

public sealed class FixedAssetDepreciationAppService
{
    private readonly IFixedAssetRepository _repository;
    private readonly IFixedAssetJournalGateway _journalGateway;
    private readonly IPeriodLockService _periodLockService;

    public FixedAssetDepreciationAppService(
        IFixedAssetRepository repository,
        IFixedAssetJournalGateway journalGateway,
        IPeriodLockService periodLockService)
    {
        _repository = repository;
        _journalGateway = journalGateway;
        _periodLockService = periodLockService;
    }

    public async Task<DepreciationPreviewResult> PreviewAsync(DepreciationPreviewRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.IdData, request.Period, request.UserId);

        AccountingPeriod period = AccountingPeriod.Parse(request.Period);
        bool isLocked = await _periodLockService.IsPeriodLockedAsync(request.IdData, period.PeriodeString, cancellationToken).ConfigureAwait(false);
        if (isLocked)
        {
            throw new InvalidOperationException($"Periode {period.PeriodeString} sudah locked.");
        }

        IReadOnlyList<FixedAsset> assets = await _repository.GetDepreciableAssetsAsync(request.IdData, period, cancellationToken).ConfigureAwait(false);
        List<DepreciationPreviewLine> lines = new();

        foreach (FixedAsset asset in assets)
        {
            if (asset.IsDepreciationStopped || asset.DepreciationMethod == DepreciationMethod.NoDepreciation)
            {
                continue;
            }

            if (asset.EffectiveDepreciationStartDate > period.EndDate)
            {
                continue;
            }

            decimal depreciationAmount = DepreciationCalculator.CalculateMonthlyDepreciationAmount(asset);
            if (depreciationAmount <= 0m)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(asset.DepreciationExpenseAccount) || string.IsNullOrWhiteSpace(asset.AccumulatedDepreciationAccount))
            {
                throw new InvalidOperationException($"Account mapping depresiasi belum lengkap untuk asset {asset.AssetCode}.");
            }

            decimal closingNpv = Math.Round(asset.OpeningNetBookValue - depreciationAmount, 2, MidpointRounding.AwayFromZero);
            lines.Add(new DepreciationPreviewLine
            {
                AssetId = asset.AssetId,
                AssetCode = asset.AssetCode,
                Period = period.PeriodeString,
                PeriodStartDate = period.StartDate,
                PeriodEndDate = period.EndDate,
                OpeningNetBookValue = asset.OpeningNetBookValue,
                DepreciationAmount = depreciationAmount,
                ClosingNetBookValue = Math.Max(asset.ResidualValue, closingNpv),
                ResidualValue = asset.ResidualValue,
                DepreciationExpenseAccount = asset.DepreciationExpenseAccount,
                AccumulatedDepreciationAccount = asset.AccumulatedDepreciationAccount,
                Description = $"Depresiasi {asset.AssetCode} periode {period.PeriodeString}"
            });
        }

        decimal total = lines.Sum(x => x.DepreciationAmount);
        long? runId = null;
        if (request.PersistAsDraftRun && lines.Count > 0)
        {
            runId = await _repository.CreateDepreciationRunAsync(request.IdData, period, request.UserId, total, cancellationToken).ConfigureAwait(false);
            await _repository.SaveDepreciationRunLinesAsync(runId.Value, lines, request.UserId, cancellationToken).ConfigureAwait(false);
        }

        return new DepreciationPreviewResult
        {
            RunId = runId,
            Period = period.PeriodeString,
            Lines = lines,
            TotalAmount = total
        };
    }

    public async Task<DepreciationPostResult> PostAsync(DepreciationPostRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.IdData, request.Period, request.UserId);
        if (request.RunId <= 0)
        {
            throw new ArgumentException("RunId tidak valid.", nameof(request.RunId));
        }

        AccountingPeriod period = AccountingPeriod.Parse(request.Period);
        bool isLocked = await _periodLockService.IsPeriodLockedAsync(request.IdData, period.PeriodeString, cancellationToken).ConfigureAwait(false);
        if (isLocked)
        {
            throw new InvalidOperationException($"Posting dibatalkan. Periode {period.PeriodeString} sudah locked.");
        }

        DepreciationRunSnapshot? runSnapshot = await _repository
            .GetDepreciationRunSnapshotAsync(request.RunId, request.IdData, cancellationToken)
            .ConfigureAwait(false);
        if (runSnapshot is null)
        {
            throw new InvalidOperationException($"Run {request.RunId} tidak ditemukan.");
        }

        IReadOnlyList<DepreciationPreviewLine> lines = await _repository.GetDepreciationRunLinesAsync(request.RunId, request.IdData, cancellationToken).ConfigureAwait(false);
        if (lines.Count == 0)
        {
            throw new InvalidOperationException($"Run {request.RunId} tidak memiliki baris depresiasi.");
        }

        decimal total = lines.Sum(x => x.DepreciationAmount);
        if (string.Equals(runSnapshot.Status, "POSTED", StringComparison.OrdinalIgnoreCase))
        {
            return new DepreciationPostResult
            {
                RunId = request.RunId,
                NoJurnal = runSnapshot.NoJurnal,
                JurnalId = runSnapshot.JurnalId,
                PostedLineCount = lines.Count * 2,
                TotalAmount = total
            };
        }

        if (!string.Equals(runSnapshot.Status, "DRAFT", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Run {request.RunId} tidak dapat diposting karena status {runSnapshot.Status}.");
        }

        DepreciationJournalPostResponse journalResponse = await _journalGateway
            .PostDepreciationAsync(request.IdData, period, request.RunId, request.UserId, lines, cancellationToken)
            .ConfigureAwait(false);

        await _repository
            .MarkDepreciationRunPostedAsync(request.RunId, journalResponse.NoJurnal, journalResponse.JurnalId, request.UserId, cancellationToken)
            .ConfigureAwait(false);

        return new DepreciationPostResult
        {
            RunId = request.RunId,
            NoJurnal = journalResponse.NoJurnal,
            JurnalId = journalResponse.JurnalId,
            PostedLineCount = journalResponse.PostedLineCount,
            TotalAmount = total
        };
    }

    private static void ValidateRequest(string idData, string period, string userId)
    {
        if (string.IsNullOrWhiteSpace(idData))
        {
            throw new ArgumentException("IDDATA wajib diisi.", nameof(idData));
        }

        if (string.IsNullOrWhiteSpace(period))
        {
            throw new ArgumentException("Periode wajib diisi.", nameof(period));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("UserId wajib diisi.", nameof(userId));
        }
    }
}
