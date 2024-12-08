// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using Microsoft.CodeAnalysis;

namespace InfiniLore.Permissions.Generators;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public static class Rules {
    public static readonly DiagnosticDescriptor NonPartialClassWarning = new(
        id: "ILPM001", 
        title: "Class with PermissionsStore attribute must be partial", 
        messageFormat: "Class '{0}' should be declared as partial to support PermissionsStore generation",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
