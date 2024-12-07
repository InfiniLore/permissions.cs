// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using InfiniLore.Permissions.Generators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace InfiniLore.Permissions.Generators;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
/// <summary>
///     Generates source code for permission repository classes that have the <c>PermissionsRepo</c> attribute applied.
/// </summary>
[Generator(LanguageNames.CSharp)]
public class PermissionsRepoGenerator : IIncrementalGenerator {

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        IncrementalValueProvider<ImmutableArray<PermissionsRepoDto>> data = context.SyntaxProvider
            .CreateSyntaxProvider(
                IsRepoClassCandidate,
                GatherPermissionsRepoDto
            )
            .Collect();

        context.RegisterSourceOutput(context.CompilationProvider.Combine(data), GenerateSources);
    }

    /// <summary>
    ///     Evaluates whether a given syntax node qualifies as a candidate for a Permissions Repository class.
    ///     A class is considered a candidate if it is a partial class and is decorated with the "PermissionsRepo" attribute.
    /// </summary>
    /// <param name="node">The syntax node to evaluate.</param>
    /// <param name="token">A cancellation token to observe while performing the operation.</param>
    /// <returns>True if the syntax node is a candidate for a Permissions Repository class; otherwise, false.</returns>
    private static bool IsRepoClassCandidate(SyntaxNode node, CancellationToken token) =>
        node is ClassDeclarationSyntax { AttributeLists.Count: > 0, Modifiers: var modifiers } classDeclarationSyntax
        && modifiers.Any(SyntaxKind.PartialKeyword)

        // Check if any of the attribute lists have the attribute we are looking for
        // Users can put multiple attribute collection on one class, this ensure we check all of them
        && classDeclarationSyntax.AttributeLists.Any(attrList =>
            attrList.Attributes.Any(attr => attr.Name.ToString().Contains("PermissionsRepo"))
        );

    /// <summary>
    ///     Gathers data from a specified class declaration and returns a <see cref="PermissionsRepoDto" />.
    ///     This involves extracting class-level and property-level information specifically for classes
    ///     annotated with the <c>PermissionsRepoAttribute</c>, and determining the options for obfuscation and case
    ///     conversion.
    /// </summary>
    /// <param name="syntaxContext">The context containing semantic information about the syntax node being analyzed.</param>
    /// <param name="token">A token to monitor for cancellation requests, typically issued by the calling thread.</param>
    /// <returns>
    ///     A <see cref="PermissionsRepoDto" /> containing the collected data for the symbol representing the class being
    ///     analyzed.
    /// </returns>
    private static PermissionsRepoDto GatherPermissionsRepoDto(GeneratorSyntaxContext syntaxContext, CancellationToken token) {
        // Because of the IsRepoClassCandidate method we can skip a whole load of null checks
        var classDeclaration = (ClassDeclarationSyntax)syntaxContext.Node;
        ISymbol classSymbol = ModelExtensions.GetDeclaredSymbol(syntaxContext.SemanticModel, classDeclaration)!;
        AttributeData repoAttribute = classSymbol.GetAttributes().First(attr => attr.AttributeClass?.Name == "PermissionsRepoAttribute");

        // Collect properties and convert to the DTO we eventually need
        PermissionsPropertyDto[] properties = classDeclaration.Members
            .OfType<PropertyDeclarationSyntax>()
            .Where(prop => prop is { Type: PredefinedTypeSyntax { Keyword.Text: "string" } })
            .Select(PermissionsPropertyDto.FromPropertyDeclarationSyntax)
            .ToArray();

        // Check enum flag parameter of the attribute
        int flagValue = (int)(repoAttribute.ConstructorArguments.FirstOrDefault().Value ?? -1);

        return new PermissionsRepoDto(
            classSymbol.Name,
            classSymbol.ContainingNamespace.ToDisplayString(),
            properties,
            (flagValue & 0b1) != 0,
            (flagValue & 0b10) != 0,
            (flagValue & 0b100) != 0
        );
    }

    /// <summary>
    ///     Generates source code files based on the provided permissions repository data and compilation context.
    /// </summary>
    /// <param name="context">The source production context used for adding the generated sources.</param>
    /// <param name="source">
    ///     A tuple containing the compilation and an immutable array of PermissionsRepoDto objects which
    ///     represent the repository data for which the source code is generated.
    /// </param>
    private static void GenerateSources(SourceProductionContext context, (Compilation compilation, ImmutableArray<PermissionsRepoDto> Right) source) {
        (_, ImmutableArray<PermissionsRepoDto> classDeclarations) = source;
        StringBuilder builder = new();
        using var sha256 = SHA256.Create();

        foreach (PermissionsRepoDto repoDto in classDeclarations) {
            builder.AppendLine("// <auto-generated />")
                .AppendLine($"namespace {repoDto.Namespace};")
                .AppendLine()
                .AppendLine($"public partial class {repoDto.ClassName} {{");

            foreach (PermissionsPropertyDto propertyDto in repoDto.Properties) {
                // Obfuscate the permission name if required
                if (repoDto.ParsePrefix) propertyDto.ParsePrefix();
                if (repoDto.ObfuscateOutput) propertyDto.ObfuscatePermissionName(sha256, repoDto.ToUpperCase);
                if (repoDto.ToUpperCase) propertyDto.ToUpperCase();

                // Add the property to the class
                builder.IndentLine(1, propertyDto.ToPropertyString());
            }

            builder.AppendLine("}");
            context.AddSource($"{repoDto.ClassName}.g.cs", builder.ToString());
            builder.Clear();
        }
    }
}
