// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CodeOfChaos.GeneratorTools;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace InfiniLore.Permissions.Generators;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
/// <summary>
///     Generates source code for permission repository classes that have the <c>PermissionsStore</c> attribute applied.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class PermissionsStoreGenerator : IIncrementalGenerator {

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        IncrementalValueProvider<ImmutableArray<PermissionsStoreDto>> data = context.SyntaxProvider
            .CreateSyntaxProvider(
                IsRepoClassCandidate,
                GatherPermissionsStoreDto
            )
            .Collect();

        context.RegisterSourceOutput(context.CompilationProvider.Combine(data), GenerateSources);
    }

    /// <summary>
    ///     Evaluates whether a given syntax node qualifies as a candidate for a Permissions Repository class.
    ///     A class is considered a candidate if it is a partial class and is decorated with the "PermissionsStore" attribute.
    /// </summary>
    /// <param name="node">The syntax node to evaluate.</param>
    /// <param name="token">A cancellation token to observe while performing the operation.</param>
    /// <returns>True if the syntax node is a candidate for a Permissions Repository class; otherwise, false.</returns>
    internal static bool IsRepoClassCandidate(SyntaxNode node, CancellationToken token) =>
        node is ClassDeclarationSyntax { AttributeLists.Count: > 0, Modifiers: var modifiers } classDeclarationSyntax
            && modifiers.Any(SyntaxKind.PartialKeyword)

            // Check if any of the attribute lists have the attribute we are looking for
            // Users can put multiple attribute collection on one class, this ensure we check all of them
            && classDeclarationSyntax.AttributeLists.Any(attrList =>
                attrList.Attributes.Any(attr => attr.Name.ToString().Contains("PermissionsStore"))
            );

    /// <summary>
    ///     Gathers data from a specified class declaration and returns a <see cref="PermissionsStoreDto" />.
    ///     This involves extracting class-level and property-level information specifically for classes
    ///     annotated with the <c>PermissionsStoreAttribute</c>, and determining the options for obfuscation and case
    ///     conversion.
    /// </summary>
    /// <param name="syntaxContext">The context containing semantic information about the syntax node being analyzed.</param>
    /// <param name="token">A token to monitor for cancellation requests, typically issued by the calling thread.</param>
    /// <returns>
    ///     A <see cref="PermissionsStoreDto" /> containing the collected data for the symbol representing the class being
    ///     analyzed.
    /// </returns>
    private static PermissionsStoreDto GatherPermissionsStoreDto(GeneratorSyntaxContext syntaxContext, CancellationToken token) {
        // Because of the IsRepoClassCandidate method we can skip a whole load of null checks
        var classDeclaration = (ClassDeclarationSyntax)syntaxContext.Node;
        ISymbol classSymbol = ModelExtensions.GetDeclaredSymbol(syntaxContext.SemanticModel, classDeclaration)!;
        AttributeData repoAttribute = classSymbol.GetAttributes().First(attr => attr.AttributeClass?.Name == "PermissionsStoreAttribute");
        
        // Collect properties and convert to the DTO we eventually need
        PermissionsPropertyDto[] properties = classDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .Where(prop => prop is { Type: PredefinedTypeSyntax { Keyword.Text: "string" } })
            .Select(PermissionsPropertyDto.FromPropertyDeclarationSyntax)
            .ToArray();

        // Check enum flag parameter of the attribute
        int flagValue = (int)(repoAttribute.ConstructorArguments.FirstOrDefault().Value ?? -1);

        var permissionsStore = new PermissionsStoreDto(
            classDeclaration: classDeclaration,
            classSymbol : classSymbol,
            className: classSymbol.Name,
            nameSpace: classSymbol.ContainingNamespace.ToDisplayString(),
            properties: properties,
            obfuscate:                    (flagValue & 0b1) != 0,
            toUpperCase:                  (flagValue & 0b10) != 0,
            parsePrefix:                  (flagValue & 0b100) != 0,
            generateAllPermissionsMethod: (flagValue & 0b1000) != 0
        );

        // fix the properties depending on some repo details
        using var sha256 = SHA256.Create();
        foreach (PermissionsPropertyDto propertyDto in permissionsStore.Properties) {
            // Obfuscate the permission name if required
            if (permissionsStore.ParsePrefix) propertyDto.ParsePrefix();
            propertyDto.ToLowerInvariant(); // Only try and do to LowerInvariant after we've parsed the prefix, otherwise the ParsePrefix will fail
            if (permissionsStore.ObfuscateOutput) propertyDto.ObfuscatePermissionName(sha256);
            if (permissionsStore.ToUpperCase) propertyDto.ToUpperInvariant();
        }
        
        return permissionsStore;
    }

    /// <summary>
    ///     Generates source code files based on the provided permissions repository data and compilation context.
    /// </summary>
    /// <param name="context">The source production context used for adding the generated sources.</param>
    /// <param name="source">
    ///     A tuple containing the compilation and an immutable array of PermissionsStoreDto objects which
    ///     represent the repository data for which the source code is generated.
    /// </param>
    private static void GenerateSources(SourceProductionContext context, (Compilation compilation, ImmutableArray<PermissionsStoreDto> classDeclarations) source) {
        (_, ImmutableArray<PermissionsStoreDto> classDeclarations) = source;
        GeneratorStringBuilder builder = new();
        
        // Throws a diagnostic error if the class is not partial (exclude from the output)
        IEnumerable<PermissionsStoreDto> didNotThrowErrors = classDeclarations
            .Where(repo => !repo.TryReportErrorIfNotPartial(context));
        
        // Parse and correct 
        
        foreach (PermissionsStoreDto repoDto in didNotThrowErrors) {
            builder.AppendLine("// <auto-generated />")
                .AppendLine("using System.Collections.Generic;")
                .AppendLine($"namespace {repoDto.Namespace};")
                .AppendLine()
                .AppendLine($"public partial class {repoDto.ClassName} {{")
                
                // Generates the properties for the class
                .ForEachAppendLineIndented(repoDto.Properties, propertyDto => propertyDto.ToPropertyString())
                .AppendLine();
            

            if (repoDto.GenerateAllPermissionsMethod) {
                // Generate a property which holds an array of all of the property's values;
                builder.Indent(g => g
                    .AppendLine("public static IEnumerable<string> GetAllPermissions() {")
                    .ForEachAppendLineIndented(repoDto.Properties, propertyDto => propertyDto.ToYieldString())
                    .AppendLine("}")
                );
            }

            builder.AppendLine("}");
            context.AddSource($"{repoDto.ClassName}.g.cs", builder.ToStringAndClear());
        }
    }
}
