using System;
using System.Threading;
using System.Threading.Tasks;
using Accounting.FixedAssets.Application.Contracts;
using Accounting.FixedAssets.Application.Models;
using Accounting.FixedAssets.Domain;

namespace Accounting.FixedAssets.Application.Services;

public sealed class FixedAssetLifecycleAppService
{
    private static readonly FixedAssetTransactionType[] ApprovalMandatoryTypes =
    new[]
    {
        FixedAssetTransactionType.Revaluation,
        FixedAssetTransactionType.FullDisposal,
        FixedAssetTransactionType.Sale,
        FixedAssetTransactionType.WriteOff
    };

    private readonly IFixedAssetLifecycleRepository _repository;
    private readonly IPeriodLockService _periodLockService;

    public FixedAssetLifecycleAppService(IFixedAssetLifecycleRepository repository, IPeriodLockService periodLockService)
    {
        _repository = repository;
        _periodLockService = periodLockService;
    }

    public async Task<long> CreateTransactionAsync(FixedAssetTransactionCreateRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);

        AccountingPeriod period = AccountingPeriod.Parse(request.Period);
        bool isLocked = await _periodLockService.IsPeriodLockedAsync(request.IdData, period.PeriodeString, cancellationToken).ConfigureAwait(false);
        if (isLocked)
        {
            throw new InvalidOperationException($"Periode {period.PeriodeString} sudah locked.");
        }

        bool exists = await _repository.ExistsActiveAssetAsync(request.IdData, request.AssetId, cancellationToken).ConfigureAwait(false);
        if (!exists)
        {
            throw new InvalidOperationException($"Asset {request.AssetId} tidak ditemukan/ tidak aktif.");
        }

        string docType = request.TransactionType switch
        {
            FixedAssetTransactionType.Improvement => "FA-IMP",
            FixedAssetTransactionType.Revaluation => "FA-REV",
            FixedAssetTransactionType.FullDisposal => "FA-DSP",
            FixedAssetTransactionType.Sale => "FA-SAL",
            FixedAssetTransactionType.WriteOff => "FA-WOF",
            FixedAssetTransactionType.Transfer => "FA-TRF",
            _ => "FA-TRX"
        };

        string docNo = await _repository.GenerateDocumentNoAsync(request.IdData, docType, period, cancellationToken).ConfigureAwait(false);
        long trxId = await _repository.CreateTransactionAsync(request, docNo, cancellationToken).ConfigureAwait(false);

        if (Array.Exists(ApprovalMandatoryTypes, x => x == request.TransactionType))
        {
            await _repository.SubmitForApprovalAsync(request.IdData, trxId, request.UserId, cancellationToken).ConfigureAwait(false);
        }

        return trxId;
    }

    public Task ApproveAsync(ApprovalActionRequest request, CancellationToken cancellationToken = default)
    {
        ValidateApprovalRequest(request);
        return _repository.ApproveAsync(request, cancellationToken);
    }

    public Task RejectAsync(ApprovalActionRequest request, CancellationToken cancellationToken = default)
    {
        ValidateApprovalRequest(request);
        return _repository.RejectAsync(request, cancellationToken);
    }

    public Task<LifecyclePostingActionResult> PostApprovedAsync(LifecyclePostingActionRequest request, CancellationToken cancellationToken = default)
    {
        ValidatePostingActionRequest(request);
        return _repository.PostApprovedAsync(request, cancellationToken);
    }

    public Task ReversePostedAsync(LifecyclePostingActionRequest request, CancellationToken cancellationToken = default)
    {
        ValidatePostingActionRequest(request);
        return _repository.ReversePostedAsync(request, cancellationToken);
    }

    private static void ValidateCreateRequest(FixedAssetTransactionCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdData))
        {
            throw new ArgumentException("IDDATA wajib diisi.", nameof(request.IdData));
        }

        if (request.AssetId <= 0)
        {
            throw new ArgumentException("AssetId tidak valid.", nameof(request.AssetId));
        }

        if (string.IsNullOrWhiteSpace(request.Period))
        {
            throw new ArgumentException("Periode wajib diisi.", nameof(request.Period));
        }

        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            throw new ArgumentException("UserId wajib diisi.", nameof(request.UserId));
        }

        if (request.AmountBase < 0m)
        {
            throw new ArgumentException("AmountBase tidak boleh negatif.", nameof(request.AmountBase));
        }

        if (request.TransactionType == FixedAssetTransactionType.Improvement && request.AmountBase <= 0m)
        {
            throw new ArgumentException("Improvement amount harus lebih besar dari nol.", nameof(request.AmountBase));
        }

        if (request.TransactionType == FixedAssetTransactionType.Revaluation
            && !request.OldAmountBase.HasValue
            && !request.NewAmountBase.HasValue)
        {
            throw new ArgumentException("Revaluation wajib mengisi old/new amount minimal salah satu.", nameof(request.NewAmountBase));
        }
    }

    private static void ValidateApprovalRequest(ApprovalActionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdData))
        {
            throw new ArgumentException("IDDATA wajib diisi.", nameof(request.IdData));
        }

        if (request.TransactionId <= 0)
        {
            throw new ArgumentException("TransactionId tidak valid.", nameof(request.TransactionId));
        }

        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            throw new ArgumentException("UserId wajib diisi.", nameof(request.UserId));
        }

        if (string.IsNullOrWhiteSpace(request.RoleCode))
        {
            throw new ArgumentException("RoleCode wajib diisi.", nameof(request.RoleCode));
        }
    }

    private static void ValidatePostingActionRequest(LifecyclePostingActionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdData))
        {
            throw new ArgumentException("IDDATA wajib diisi.", nameof(request.IdData));
        }

        if (request.TransactionId <= 0)
        {
            throw new ArgumentException("TransactionId tidak valid.", nameof(request.TransactionId));
        }

        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            throw new ArgumentException("UserId wajib diisi.", nameof(request.UserId));
        }
    }
}
