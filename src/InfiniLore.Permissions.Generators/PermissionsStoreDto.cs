// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
public readonly struct PermissionsStoreDto(ClassDeclarationSyntax classDeclaration, ISymbol classSymbol, string className, string nameSpace, PermissionsPropertyDto[] properties, bool obfuscate, bool toUpperCase, bool parsePrefix) {
    /// <summary>
    ///     Gets the syntax node representing the class declaration, allowing access
    ///     to the structural syntax details of the class related to permissions.
    /// </summary>
    public ClassDeclarationSyntax ClassDeclaration { get; } = classDeclaration;

    /// <summary>
    ///     Gets the symbol representing the class, providing access to the
    ///     semantic information associated with the class as defined within
    ///     the source code.
    /// </summary>
    public ISymbol ClassSymbol { get; } = classSymbol;
    
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

    /// <summary>
    ///     Checks if the class declaration does not contain a 'partial' keyword and reports a diagnostic warning if it is not partial.
    /// </summary>
    /// <param name="context">The context for reporting diagnostics during source generation.</param>
    /// <returns>Returns true if the class is not partial and a diagnostic warning is reported; otherwise, false.</returns>
    public bool TryReportErrorIfNotPartial(SourceProductionContext context) {
        if (ClassDeclaration.Members.Any(SyntaxKind.PartialKeyword)) return false;

        context.ReportDiagnostic(Diagnostic.Create(
            Rules.NonPartialClassWarning,
            ClassDeclaration.GetLocation(),
            ClassSymbol.Name
        ));
        return true;
    }
}
