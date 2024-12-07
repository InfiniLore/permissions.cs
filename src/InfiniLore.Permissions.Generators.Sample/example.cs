// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace InfiniLore.Permissions.Generators.Sample;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[PermissionsRepo]
public static partial class Permissions {
    [Prefix("data.users")] public static partial string LorescopesRead { get; }
    [Prefix("data.users")] public static partial string LorescopesWrite { get; }
    [Prefix("data.users")] public static partial string LorescopesDelete { get; }

    internal static partial string AccountRead { get; }
}
