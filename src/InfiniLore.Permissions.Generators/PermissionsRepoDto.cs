// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
namespace InfiniLore.Permissions.Generators;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public readonly struct PermissionsRepoDto(string className, string nameSpace, PermissionsPropertyDto[] properties, bool obfuscate, bool toUpperCase){
    public string ClassName { get; } = className;
    public string Namespace { get; } = nameSpace;
    public PermissionsPropertyDto[] Properties { get; } = properties;
    
    // Based on GeneratorFlags.cs
    public bool ObfuscateOutput { get; } = obfuscate;
    public bool ToUpperCase { get; } = toUpperCase;
}
