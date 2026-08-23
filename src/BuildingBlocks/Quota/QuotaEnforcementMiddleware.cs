using System.Diagnostics;
using System.Globalization;
using Finbuckle.MultiTenant.Abstractions;
using DreamTeam.Framework.Shared.Multitenancy;
using DreamTeam.Framework.Shared.Quota;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DreamTeam.Framework.Quota;

/// <summary>
/// Per-request quota enforcement for <see cref="QuotaResource.ApiCalls"/>. Runs after auth and the
/// rate limiter so only authenticated, counted calls charge the meter. Skips health probes, the
/// root tenant, and unresolved tenants. Rejects with HTTP 429 + RFC 9457 ProblemDetails and a
/// <c>Retry-After</c> header; flags the request via the <c>DreamTeam.Quota.Rejected</c> HttpContext
/// item so a future audit implementation can tag the activity event without this middleware taking
/// a dependency on it.
/// </summary>
public sealed class QuotaEnforcementMiddleware : IMiddleware
{
    private const string QuotaExceededType = "https://datatracker.ietf.org/doc/html/rfc6585#section-4";

    private readonly IQuotaService _quotaService;
    private readonly IMultiTenantContextAccessor<AppTenantInfo> _tenantAccessor;
    private readonly QuotaOptions _options;
    private readonly ILogger<QuotaEnforcementMiddleware> _logger;
    private readonly TimeProvider _timeProvider;

    public QuotaEnforcementMiddleware(
        IQuotaService quotaService,
        IMultiTenantContextAccessor<AppTenantInfo> tenantAccessor,
        QuotaOptions options,
        ILogger<QuotaEnforcementMiddleware> logger,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(quotaService);
        ArgumentNullException.ThrowIfNull(tenantAccessor);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _quotaService = quotaService;
        _tenantAccessor = tenantAccessor;
        _options = options;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        if (!_options.Enabled || IsExempt(context))
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var tenantId = _tenantAccessor.MultiTenantContext?.TenantInfo?.Id;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var result = await _quotaService
            .CheckAndRecordAsync(tenantId, QuotaResource.ApiCalls, 1, context.RequestAborted)
            .ConfigureAwait(false);

        if (result.Allowed)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        await RejectAsync(context, tenantId, result).ConfigureAwait(false);
    }

    private async Task RejectAsync(HttpContext context, string tenantId, QuotaCheckResult result)
    {
        // Quota-rejected flag — historically read by the audit middleware to tag the activity event
        // with AuditTag.OutOfQuota. Audit module is removed in the FSH-strip; the flag is set here as
        // a string constant so future audit implementations can pick it up without changes to this file.
        context.Items["DreamTeam.Quota.Rejected"] = true;

        var retryAfterSeconds = CalculateRetryAfter(result.ResetAtUtc);
        if (retryAfterSeconds is > 0)
        {
            context.Response.Headers.RetryAfter = retryAfterSeconds.Value
                .ToString(CultureInfo.InvariantCulture);
        }

        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Quota Exceeded",
            Detail = $"Monthly quota for {result.Resource} has been reached ({result.CurrentUsage}/{result.Limit}).",
            Type = QuotaExceededType,
            Instance = context.Request.Path
        };

        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
        problem.Extensions["traceId"] = traceId;

        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? context.TraceIdentifier;
        problem.Extensions["correlationId"] = correlationId;
        problem.Extensions["resource"] = result.Resource.ToString();
        problem.Extensions["limit"] = result.Limit;
        problem.Extensions["currentUsage"] = result.CurrentUsage;
        if (result.ResetAtUtc is not null)
        {
            problem.Extensions["resetAtUtc"] = result.ResetAtUtc.Value.ToString("o", CultureInfo.InvariantCulture);
        }

        if (_logger.IsEnabled(LogLevel.Warning))
        {
            _logger.LogWarning(
                "Rejected request for tenant {TenantId} — {Resource} quota exceeded ({Current}/{Limit})",
                tenantId, result.Resource, result.CurrentUsage, result.Limit);
        }

        await context.Response.WriteAsJsonAsync(problem, context.RequestAborted).ConfigureAwait(false);
    }

    private int? CalculateRetryAfter(DateTimeOffset? resetAtUtc)
    {
        if (resetAtUtc is null)
        {
            return null;
        }

        var delta = resetAtUtc.Value - _timeProvider.GetUtcNow();
        return delta.TotalSeconds > 0 ? (int)Math.Ceiling(delta.TotalSeconds) : null;
    }

    private static bool IsExempt(HttpContext context)
    {
        var path = context.Request.Path;
        return path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/healthz", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/ready", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/live", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/alive", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/metrics", StringComparison.OrdinalIgnoreCase);
    }
}
