using System.Collections.Concurrent;
using DreamTeam.Framework.Shared.Multitenancy;

namespace DreamTeam.Modules.Multitenancy.Services;

internal sealed class TenantInitialPasswordBuffer : ITenantInitialPasswordBuffer
{
    private readonly ConcurrentDictionary<string, string> _passwords = new(StringComparer.OrdinalIgnoreCase);

    public void Store(string tenantId, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        _passwords[tenantId] = password;
    }

    public string? TryConsume(string tenantId)
        => _passwords.TryRemove(tenantId, out var password) ? password : null;
}
