// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace InfiniLore.Permissions.Generators;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
/// <summary>
///     This struct contains metadata for a class including its name, namespace,
///     and a list of properties related to permissions. It also indicates
///     whether the output should be obfuscated and whether property names should
///     be converted to uppercase.
/// </summary>
public readonly struct PermissionsStoreDto(string className, string nameSpace, PermissionsPropertyDto[] properties, bool obfuscate, bool toUpperCase, bool parsePrefix) {
    /// <summary>
    ///     Represents the name of the class to be generated in the permissions' repository.
    /// </summary>
    public string ClassName { get; } = className;

    /// <summary>
    ///     Gets the namespace of the permissions repository class.
    ///     This property represents the namespace under which the generated permissions repository class will be placed.
    /// </summary>
    public string Namespace { get; } = nameSpace;

    /// <summary>
    ///     Gets the array of <see cref="PermissionsPropertyDto" /> which define the properties
    ///     of the Permissions Repo. These properties may be subject to additional processing
    ///     such as obfuscation or case formatting based on the repo's settings.
    /// </summary>
    public PermissionsPropertyDto[] Properties { get; } = properties;

    /// <summary>
    ///     Gets a value indicating whether the output should be obfuscated.
    ///     When this property is set to true, the generated permissions may be altered for obscurity
    ///     using methods like hashing, which can help in hiding the actual permission names from the output.
    /// </summary>
    public bool ObfuscateOutput { get; } = obfuscate;

    /// <summary>
    ///     Gets a value indicating whether the properties should be converted to upper case.
    /// </summary>
    public bool ToUpperCase { get; } = toUpperCase;


    /// <summary>
    ///     Indicates whether a prefix should be parsed when processing
    ///     the properties related to permissions in the metadata struct.
    /// </summary>
    public bool ParsePrefix { get; } = parsePrefix;
}
