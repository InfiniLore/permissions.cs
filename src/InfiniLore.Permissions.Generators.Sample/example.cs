// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace InfiniLore.Permissions.Generators.Sample;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[PermissionsRepo(GeneratorFlags.ParsePrefix)]
public static partial class Permissions {
    private const string DataUser = nameof(DataUser);
    
    [Prefix(DataUser)] public static partial string LorescopesRead { get; }
    [Prefix(DataUser)] public static partial string LorescopesWrite { get; }
    [Prefix(DataUser)] public static partial string LorescopesDelete { get; }

    internal static partial string AccountRead { get; }
}