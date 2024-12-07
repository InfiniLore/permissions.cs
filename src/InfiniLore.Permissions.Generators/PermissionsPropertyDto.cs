// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace InfiniLore.Permissions.Generators;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
/// <summary>
///     Represents a Data Transfer Object (DTO) for permissions related to a property.
///     This class is used to encapsulate the various aspects of a property declaration
///     syntax, allowing for transformation or processing of property-based permission declarations.
/// </summary>
public class PermissionsPropertyDto {
    /// <summary>
    ///     Represents the access modifier of a property within the PermissionsPropertyDto class.
    /// </summary>
    private string AccessModifier { get; set; } = default!;

    /// <summary>
    ///     Represents the prefix indicating whether a property is static.
    ///     This property returns the string "static " if the property is marked as static, otherwise it returns an empty
    ///     string.
    /// </summary>
    private string StaticPrefix { get; set; } = default!;

    /// <summary>
    ///     Gets or sets the name of the property.
    ///     <see cref="PropertyName" /> is used to distinguish and handle various permissions effectively
    ///     within the <c>PermissionsPropertyDto</c> class. This property is part of the permissions
    ///     management system and plays a crucial role in mapping and modifying permission-related data
    ///     according to specific attributes and rules defined in the associated class.
    /// </summary>
    private string PropertyName { get; set; } = default!;

    /// <summary>
    ///     Gets or sets the name of the permission associated with the property.
    /// </summary>
    private string PermissionName { get; set; } = default!;

    // -----------------------------------------------------------------------------------------------------------------
    // Constructors
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Converts a given <see cref="PropertyDeclarationSyntax" /> into a <see cref="PermissionsPropertyDto" />.
    /// </summary>
    /// <param name="propertySyntax">The property declaration syntax that contains the property information to be converted.</param>
    /// <returns>
    ///     A <see cref="PermissionsPropertyDto" /> with details derived from the provided property declaration, including
    ///     access modifier, static prefix, property name, and permission name.
    /// </returns>
    public static PermissionsPropertyDto FromPropertyDeclarationSyntax(PropertyDeclarationSyntax propertySyntax) {
        // Grabs the possible prefix value from the optional attribute.
        AttributeSyntax[] propertyAttributes = propertySyntax.AttributeLists.SelectMany(list => list.Attributes).ToArray();
        AttributeSyntax? prefixSyntax = propertyAttributes.FirstOrDefault(attr => attr.Name.ToString().EndsWith("Prefix"));
        string prefix = prefixSyntax?.ArgumentList?.Arguments.FirstOrDefault()?.Expression.ToString().Replace("\"", "") ?? string.Empty;

        // Determines if the property is static.
        string staticPrefix = propertySyntax.Modifiers.Any(SyntaxKind.StaticKeyword) ? "static " : string.Empty;

        // Determines the access modifier.
        // Is an edge case that someone would want to define it as something else than public, but this way it is supported
        string accessModifier = propertySyntax.Modifiers switch {
            var modifiers when modifiers.Any(SyntaxKind.PrivateKeyword) => "private",
            var modifiers when modifiers.Any(SyntaxKind.ProtectedKeyword) => "protected",
            var modifiers when modifiers.Any(SyntaxKind.InternalKeyword) => "internal",
            _ => "public"
        };

        string propertyNameAsPermission = ToPeriodSeperated(propertySyntax.Identifier.ToString());

        // Actual permission name
        string permissionName = string.IsNullOrEmpty(prefix)
            ? propertyNameAsPermission
            : $"{prefix}.{propertyNameAsPermission}";

        return new PermissionsPropertyDto {
            AccessModifier = accessModifier,
            StaticPrefix = staticPrefix,
            PropertyName = propertySyntax.Identifier.ToString(),
            PermissionName = permissionName
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Converts the current instance of <see cref="PermissionsPropertyDto" /> to a string representation of a partial
    ///     string property declaration.
    /// </summary>
    /// <returns>
    ///     A string that contains the access modifier, static prefix if any, and the partial string property declaration with
    ///     the property name and permission name.
    /// </returns>
    public string ToPropertyString() => $"{AccessModifier} {StaticPrefix}partial string {PropertyName} {{ get => \"{PermissionName}\"; }}";

    /// <summary>
    ///     Obfuscates the permission name of the current instance using a specified hash algorithm and optionally converts it
    ///     to uppercase.
    /// </summary>
    /// <param name="hasher">The hash algorithm to use for obfuscating the permission name.</param>
    /// <param name="toUpperCase">
    ///     A boolean value indicating whether the obfuscated permission name should be converted to
    ///     uppercase.
    /// </param>
    public void ObfuscatePermissionName(HashAlgorithm hasher, bool toUpperCase) {
        byte[] bytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(PermissionName));
        string hash = Regex.Replace(Convert.ToBase64String(bytes), "[+/=]", string.Empty);

        // Set the PermissionName to the obfuscated version
        // Truncate to 5 characters
        PermissionName = hash.Length > 10
            ? hash.Substring(0, 5)
            : hash.PadRight(5, '0');
    }

    /// <summary>
    ///     Converts the permission name to upper case using the invariant culture.
    /// </summary>
    public void ToUpperCase() {
        PermissionName = PermissionName.ToUpperInvariant();
    }

    /// <summary>
    ///     Converts a camelCase or PascalCase string into a period-separated lowercase string.
    /// </summary>
    /// <param name="input">The input string in camelCase or PascalCase format.</param>
    /// <returns>
    ///     A string where each word is separated by periods and all characters are in lowercase.
    /// </returns>
    private static string ToPeriodSeperated(string input) =>
        string.Join(
            ".",
            Regex.Split(input, "(?<!^)(?=[A-Z])")
        ).ToLowerInvariant();

    /// <summary>
    ///     Parses the static prefix of the property and converts it to a period-separated format in lowercase.
    /// </summary>
    public void ParsePrefix() => PermissionName = ToPeriodSeperated(PermissionName);
}
