using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Application.Interfaces;
using LoanManagementSystem.Domain.Entities;
using LoanManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LoanManagementSystem.Infrastructure.Services;

public class EmiAllocationService : IEmiAllocationService
{
    private readonly ApplicationDbContext _db;
    private readonly IEmiModuleClient _emiClient;

    public EmiAllocationService(
        ApplicationDbContext db,
        IEmiModuleClient emiClient)
    {
        _db = db;
        _emiClient = emiClient;
    }

    public async Task<EmiAllocationResultDto> AllocateAsync(
        AllocateEmiPaymentDto dto,
        CancellationToken ct = default)
    {
        if (dto.PaymentId <= 0 || dto.EmiId <= 0)
            throw new ArgumentException(
                "PaymentId and EmiId must be valid.");

        var payment = await _db.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.PaymentId == dto.PaymentId, ct);

        if (payment == null)
            throw new KeyNotFoundException(
                "Payment not found.");

        if (payment.PaymentStatus != "Completed")
            throw new InvalidOperationException(
                "Only verified completed payments can be allocated.");

        if (payment.Amount <= 0)
            throw new InvalidOperationException(
                "Payment amount must be positive.");

        if (payment.EmiId.HasValue &&
            payment.EmiId.Value != dto.EmiId)
        {
            throw new InvalidOperationException(
                "Payment belongs to a different EMI.");
        }

        var allocation = await _db.EmiPaymentAllocations
            .FirstOrDefaultAsync(
                x => x.PaymentId == dto.PaymentId, ct);

        if (allocation != null)
        {
            if (allocation.EmiId != dto.EmiId)
                throw new InvalidOperationException(
                    "Payment is already assigned to another EMI.");

            if (allocation.Status == "Applied")
                return await BuildResultAsync(allocation, ct);

            if (allocation.Status != "Pending")
                throw new InvalidOperationException(
                    "Allocation requires manual review.");

            // A previous request may have succeeded remotely.
            // Query Jousha before attempting to apply it again.
            var remote = await _emiClient.GetAllocationAsync(
                allocation.IdempotencyKey, ct);

            if (remote != null)
            {
                ValidateRemote(remote, allocation);

                allocation.Status = "Applied";
                allocation.AppliedAt = DateTime.UtcNow;
                allocation.LastError = null;

                await _db.SaveChangesAsync(ct);

                return ToResult(allocation, remote);
            }

            // Pending allocations are not automatically re-sent
            // by this endpoint. A dedicated reconciler must retry
            // with the same idempotency key after confirming the
            // remote result is absent.
            throw new InvalidOperationException(
                "Allocation is pending reconciliation.");
        }

        var emi = await _emiClient.GetEmiAsync(dto.EmiId, ct);

        if (emi == null)
            throw new KeyNotFoundException("EMI not found.");

        if (emi.LoanId != payment.LoanId)
            throw new InvalidOperationException(
                "EMI does not belong to the payment loan.");

        if (payment.Amount > emi.RemainingAmount)
            throw new InvalidOperationException(
                "Payment exceeds the remaining EMI amount.");

        allocation = new EmiPaymentAllocation
        {
            PaymentId = payment.PaymentId,
            CustomerId = payment.CustomerId,
            LoanId = payment.LoanId,
            EmiId = dto.EmiId,
            Amount = payment.Amount,
            IdempotencyKey = $"loan-payment-{payment.PaymentId}",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _db.EmiPaymentAllocations.Add(allocation);

        // The unique PaymentId index protects this reservation.
        // If a concurrent request already reserved the payment,
        // this insert fails rather than applying it twice.
        await _db.SaveChangesAsync(ct);

        ApplyEmiPaymentResponseDto? applied;

        try
        {
            applied = await _emiClient.ApplyPaymentAsync(
                new ApplyEmiPaymentRequestDto
                {
                    PaymentId = payment.PaymentId,
                    EmiId = dto.EmiId,
                    Amount = payment.Amount,
                    IdempotencyKey = allocation.IdempotencyKey
                },
                ct);
        }
        catch (HttpRequestException)
        {
            // Remote result is unknown: keep Pending.
            throw new InvalidOperationException(
                "EMI API unavailable or returned an error. " +
                "Allocation is pending reconciliation.");
        }
        catch (TaskCanceledException)
        {
            // Timeout: remote operation might have succeeded.
            throw new InvalidOperationException(
                "EMI API timed out. " +
                "Allocation is pending reconciliation.");
        }

        if (applied == null)
        {
            allocation.Status = "Rejected";
            allocation.LastError = "EMI module rejected allocation.";

            await _db.SaveChangesAsync(ct);

            throw new InvalidOperationException(
                "EMI module rejected the allocation.");
        }

        ValidateRemote(applied, allocation);

        allocation.Status = "Applied";
        allocation.AppliedAt = DateTime.UtcNow;
        allocation.LastError = null;

        await _db.SaveChangesAsync(ct);

        return ToResult(allocation, applied);
    }

    public Task<EmiOutstandingDto> GetOutstandingAsync(
        int loanId,
        CancellationToken ct = default)
    {
        if (loanId <= 0)
            throw new ArgumentException("Invalid LoanId.");

        return _emiClient.GetOutstandingAsync(loanId, ct);
    }

    private async Task<EmiAllocationResultDto> BuildResultAsync(
        EmiPaymentAllocation allocation,
        CancellationToken ct)
    {
        var remote = await _emiClient.GetAllocationAsync(
            allocation.IdempotencyKey, ct);

        if (remote == null)
            throw new InvalidOperationException(
                "Remote allocation status unavailable.");

        ValidateRemote(remote, allocation);

        return ToResult(allocation, remote);
    }

    private static void ValidateRemote(
        ApplyEmiPaymentResponseDto remote,
        EmiPaymentAllocation allocation)
    {
        if (remote.EmiId != allocation.EmiId ||
            remote.AppliedAmount != allocation.Amount)
        {
            throw new InvalidOperationException(
                "EMI allocation response mismatch. " +
                "Manual reconciliation required.");
        }
    }

    private static EmiAllocationResultDto ToResult(
        EmiPaymentAllocation allocation,
        ApplyEmiPaymentResponseDto remote)
    {
        return new EmiAllocationResultDto
        {
            AllocationId = allocation.Id,
            PaymentId = allocation.PaymentId,
            EmiId = allocation.EmiId,
            AllocatedAmount = allocation.Amount,
            AllocationStatus = allocation.Status,
            EmiStatus = remote.Status,
            RemainingEmiAmount = remote.RemainingAmount
        };
    }
}