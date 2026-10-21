using System.Net;
using System.Net.Http.Json;
using LoanManagementSystem.Application.DTOs;
using LoanManagementSystem.Application.Interfaces;

namespace LoanManagementSystem.Infrastructure.Clients;

public class EmiModuleClient : IEmiModuleClient
{
    private readonly HttpClient _http;

    public EmiModuleClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<EmiDetailsDto?> GetEmiAsync(
        int emiId,
        CancellationToken ct = default)
    {
        using var response = await _http.GetAsync(
            $"api/emis/{emiId}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<EmiDetailsDto>(
                cancellationToken: ct);
    }

    public async Task<ApplyEmiPaymentResponseDto?> ApplyPaymentAsync(
        ApplyEmiPaymentDto dto,
        CancellationToken cancellationToken = default)
    {
        var request = new ApplyEmiPaymentRequestDto
        {
            PaymentId = dto.PaymentId,
            EmiId = dto.EmiId,
            Amount = dto.Amount,
            IdempotencyKey = dto.IdempotencyKey
        };

        return await ApplyPaymentAsync(request, cancellationToken);
    }

    public async Task<ApplyEmiPaymentResponseDto?> ApplyPaymentAsync(
        ApplyEmiPaymentRequestDto request,
        CancellationToken ct = default)
    {
        using var response = await _http.PostAsJsonAsync(
            "api/emis/allocate-payment", request, ct);

        // 409 means Jousha rejected an allocation,
        // for example because the EMI has insufficient balance.
        if (response.StatusCode == HttpStatusCode.Conflict)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<ApplyEmiPaymentResponseDto>(
                cancellationToken: ct);
    }

    public async Task<ApplyEmiPaymentResponseDto?> GetAllocationAsync(
        string idempotencyKey,
        CancellationToken ct = default)
    {
        using var response = await _http.GetAsync(
            $"api/emis/allocations/{Uri.EscapeDataString(idempotencyKey)}",
            ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<ApplyEmiPaymentResponseDto>(
                cancellationToken: ct);
    }

    public async Task<EmiOutstandingDto> GetOutstandingAsync(
        int loanId,
        CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<EmiOutstandingDto>(
            $"api/emis/outstanding/{loanId}", ct)
            ?? throw new InvalidOperationException(
                "EMI outstanding response was empty.");
    }
}