using System.Net.Http.Headers;

namespace LoanManagementSystem.API.Middleware;

public sealed class ForwardAuthorizationHandler(
    IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var authorization = httpContextAccessor.HttpContext?
            .Request.Headers.Authorization.ToString();

        if (!string.IsNullOrWhiteSpace(authorization) &&
            AuthenticationHeaderValue.TryParse(
                authorization,
                out var headerValue))
        {
            request.Headers.Authorization = headerValue;
        }

        return base.SendAsync(request, cancellationToken);
    }
}
