using DreamTeam.Modules.Identity;
using DreamTeam.Modules.Multitenancy;
using System.Reflection;

namespace Generic.Tests.Architecture;

/// <summary>
/// Discovers all DreamTeam module assemblies for use in generic architecture tests.
/// Kept modules per the FDS: Identity, Multitenancy (dormant until v4), Files.
/// The Forms module (MVP-1) will be added when it lands.
/// </summary>
internal static class ModuleAssemblyDiscovery
{
    private static readonly Assembly[] _cached = Discover();

    public static Assembly[] GetModuleAssemblies() => _cached;

    private static Assembly[] Discover()
    {
        string baseDir = AppContext.BaseDirectory;

        var moduleFiles = Directory.GetFiles(baseDir, "DreamTeam.Modules.*.dll")
            .Where(f => !f.EndsWith(".Contracts.dll", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var assemblies = new List<Assembly>();

        foreach (var file in moduleFiles)
        {
            try
            {
                var assemblyName = AssemblyName.GetAssemblyName(file);
                assemblies.Add(Assembly.Load(assemblyName));
            }
#pragma warning disable CA1031
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Skip unreadable / not-managed assemblies.
            }
#pragma warning restore CA1031
        }

        return assemblies
            .OrderBy(a => a.GetName().Name, StringComparer.Ordinal)
            .ToArray();
    }
}
