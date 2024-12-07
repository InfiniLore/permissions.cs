// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using System;
using System.Security.Cryptography; 

namespace InfiniLore.Permissions.Generators;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
// Reason for being a class is because there could be post processors like the obfuscate feature which need to edit the
// generated code.
public class PermissionsPropertyDto {
    private string AccessModifier { get; set; } = default!;
    private string StaticPrefix { get; set; } = default!;
    private string PropertyName { get; set; } = default!;
    private string PermissionName { get; set; } = default!;

    // -----------------------------------------------------------------------------------------------------------------
    // Constructors
    // -----------------------------------------------------------------------------------------------------------------
    public static PermissionsPropertyDto FromPropertyDeclarationSyntax(PropertyDeclarationSyntax propertySyntax) {
        // Grabs the possible prefix value from the optional attribute.
        AttributeSyntax[] propertyAttributes = propertySyntax.AttributeLists.SelectMany(list => list.Attributes).ToArray();
        AttributeSyntax? prefixSyntax = propertyAttributes.FirstOrDefault(attr => attr.Name.ToString() == "Prefix");
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

        string propertyNameAsPermission = string.Join(
            ".",
            Regex.Split(propertySyntax.Identifier.ToString(), "(?<!^)(?=[A-Z])")
        ).ToLowerInvariant();
        
        // Actual permission name
        string permissionName = string.IsNullOrEmpty(prefix)
            ? propertyNameAsPermission
            : $"{prefix}.{propertyNameAsPermission}";
        
        return new PermissionsPropertyDto {
            AccessModifier = accessModifier,
            StaticPrefix = staticPrefix,
            PropertyName = propertySyntax.Identifier.ToString(),
            PermissionName = permissionName,
        };
    }

    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public string ToPropertyString() {
        return $"{AccessModifier} {StaticPrefix}partial string {PropertyName} {{ get => \"{PermissionName}\"; }}";
    }

    public void ObfuscatePermissionName(HashAlgorithm hasher, bool toUpperCase) {
        byte[] bytes = hasher.ComputeHash(System.Text.Encoding.UTF8.GetBytes(PermissionName));
        string hash = Regex.Replace(Convert.ToBase64String(bytes), "[+/=]", string.Empty);

        // Set the PermissionName to the obfuscated version
        // Truncate to 5 characters
        PermissionName =  hash.Length > 10
            ? hash.Substring(0, 5) 
            : hash.PadRight(5, '0'); 
    }
    
    public void ToUpperCase() {
        PermissionName = PermissionName.ToUpperInvariant();
    }
}
