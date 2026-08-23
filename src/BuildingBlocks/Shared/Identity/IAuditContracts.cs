namespace DreamTeam.Framework.Shared.Identity;

// =============================================================================
// Security + activity audit contracts.
// The FSH starter kit's heavy Auditing module is intentionally removed; these
// lightweight contracts preserve the call sites in DreamTeam.Modules.Identity
// so a real implementation (event-sourcing-lite, per FDS docs/architecture-v1.md
// §Audit) can land in MVP-2+ without touching the Identity handlers again.
// Default no-op implementation: NoOpSecurityAudit / NoOpAuditClient below.
// =============================================================================

public enum ActivityKind { Command = 0, Query = 1, IntegrationEvent = 2, BackgroundJob = 3 }
public enum BodyCapture  { None = 0, Request = 1, Response = 2, Both = 3 }
public enum AuditSeverity { Information = 0, Warning = 1, Error = 2, Critical = 3 }
public enum SecurityAction
{
    PolicyFailed = 0,
    LoginSucceeded = 1,
    LoginFailed = 2,
    TokenIssued = 3,
    TokenRevoked = 4,
    ImpersonationStarted = 5,
    ImpersonationEnded = 6,
    PasswordChanged = 7,
    MfaEnabled = 8,
    MfaDisabled = 9,
}

public interface ISecurityAudit
{
    ValueTask LoginFailedAsync(
        string subjectIdOrName,
        string clientId,
        string reason,
        string ip,
        CancellationToken ct);

    ValueTask LoginSucceededAsync(
        string userId,
        string userName,
        string clientId,
        string ip,
        string userAgent,
        CancellationToken ct);

    ValueTask TokenIssuedAsync(
        string userId,
        string userName,
        string clientId,
        string tokenFingerprint,
        DateTimeOffset expiresUtc,
        CancellationToken ct);

    ValueTask TokenRevokedAsync(
        string userId,
        string clientId,
        string reason,
        CancellationToken ct);

    ValueTask ImpersonationStartedAsync(
        string actorUserId,
        string? actorTenantId,
        string targetUserId,
        string? targetTenantId,
        string clientId,
        string ip,
        string userAgent,
        string reason,
        CancellationToken ct);

    ValueTask ImpersonationEndedAsync(
        string actorUserId,
        string? actorTenantId,
        string targetUserId,
        string? targetTenantId,
        string clientId,
        string ip,
        string userAgent,
        string reason,
        CancellationToken ct);
}

public interface IAuditClient
{
    ValueTask WriteActivityAsync(
        ActivityKind kind,
        string name,
        int statusCode,
        long durationMs,
        BodyCapture captured,
        long requestSize,
        long responseSize,
        object? requestPreview,
        object? responsePreview,
        AuditSeverity severity,
        string source,
        CancellationToken ct);

    ValueTask WriteSecurityAsync(
        SecurityAction action,
        string? subjectId,
        string? reasonCode,
        IReadOnlyDictionary<string, object?>? claims,
        AuditSeverity severity,
        string source,
        CancellationToken ct);
}

public sealed class NoOpSecurityAudit : ISecurityAudit
{
    public ValueTask LoginFailedAsync(string subjectIdOrName, string clientId, string reason, string ip, CancellationToken ct) => ValueTask.CompletedTask;
    public ValueTask LoginSucceededAsync(string userId, string userName, string clientId, string ip, string userAgent, CancellationToken ct) => ValueTask.CompletedTask;
    public ValueTask TokenIssuedAsync(string userId, string userName, string clientId, string tokenFingerprint, DateTimeOffset expiresUtc, CancellationToken ct) => ValueTask.CompletedTask;
    public ValueTask TokenRevokedAsync(string userId, string clientId, string reason, CancellationToken ct) => ValueTask.CompletedTask;
    public ValueTask ImpersonationStartedAsync(string actorUserId, string? actorTenantId, string targetUserId, string? targetTenantId, string clientId, string ip, string userAgent, string reason, CancellationToken ct) => ValueTask.CompletedTask;
    public ValueTask ImpersonationEndedAsync(string actorUserId, string? actorTenantId, string targetUserId, string? targetTenantId, string clientId, string ip, string userAgent, string reason, CancellationToken ct) => ValueTask.CompletedTask;
}

public sealed class NoOpAuditClient : IAuditClient
{
    public ValueTask WriteActivityAsync(ActivityKind kind, string name, int statusCode, long durationMs, BodyCapture captured, long requestSize, long responseSize, object? requestPreview, object? responsePreview, AuditSeverity severity, string source, CancellationToken ct) => ValueTask.CompletedTask;
    public ValueTask WriteSecurityAsync(SecurityAction action, string? subjectId, string? reasonCode, IReadOnlyDictionary<string, object?>? claims, AuditSeverity severity, string source, CancellationToken ct) => ValueTask.CompletedTask;
}
