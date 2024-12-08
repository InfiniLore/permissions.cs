// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace InfiniLore.Permissions.Generators.Sample;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[PermissionsStore(GeneratorFlags.ParsePrefix)]
public static partial class Permissions {
    private const string DataUser = nameof(DataUser);
    private const string Data = nameof(Data);
    private const string User = nameof(User);
    
    [Prefix(DataUser)] public static partial string LorescopesRead { get; }
    [Prefix(DataUser)] private static partial string LorescopesWrite { get; }
    [Prefix(DataUser)] internal static partial string LorescopesDelete { get; }
    
    [Prefix(Data, User)] public static partial string SomethingRead { get; }
    [Prefix(Data, User), Prefix(Data, User)] public static partial string SomethingReadA { get; }
    [Prefix(Data, User)] [Prefix(Data, User)] public static partial string SomethingReadB { get; }

    internal static partial string AccountRead { get; }
}