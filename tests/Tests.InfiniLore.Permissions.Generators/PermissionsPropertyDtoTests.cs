// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using InfiniLore.Permissions.Generators;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Tests.InfiniLore.Permissions.Generators;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[TestSubject(typeof(PermissionsPropertyDto))]
public class PermissionsPropertyDtoTests {
    [Test]
    public async Task FromPropertyDeclarationSyntax_ShouldConvertCorrectly() {
        const string code = """
            public class TestClass
            {
                [Prefix("Test")]
                public string SampleProperty { get; set; }
            }
            """;
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
        PropertyDeclarationSyntax propertyNode = (await syntaxTree.GetRootAsync())
            .DescendantNodes().OfType<PropertyDeclarationSyntax>()
            .First();

        PermissionsPropertyDto dto = PermissionsPropertyDto.FromPropertyDeclarationSyntax(propertyNode);
        
        await Assert.That(dto).IsNotNull();
        await Assert.That(dto.AccessModifier).IsEqualTo("public");
        await Assert.That(dto.StaticPrefix).IsEqualTo(string.Empty);
        await Assert.That(dto.PropertyName).IsEqualTo("SampleProperty");
        await Assert.That(dto.PermissionName).IsEqualTo("Test.sample.property");
    }

    [Test]
    public async Task ToPropertyString_ShouldReturnCorrectFormat() {
        var dto = new PermissionsPropertyDto {
            AccessModifier = "public",
            StaticPrefix = "",
            PropertyName = "SampleProperty",
            PermissionName = "sample.permission"
        };

        const string expected = "public partial string SampleProperty { get => \"sample.permission\"; }";
        
        await Assert.That(dto.ToPropertyString()).IsEqualTo(expected);
    }

    [Test]
    [Arguments("sample.permission", "aihuI")]
    public async Task ObfuscatePermissionName_ShouldObfuscateCorrectly(string input, string expectedOutput) {
        var dto = new PermissionsPropertyDto {
            PermissionName = input
        };

        using var hasher = SHA256.Create();
        dto.ObfuscatePermissionName(hasher);

        await Assert.That(dto.PermissionName).IsNotEmpty();
        await Assert.That(dto.PermissionName).IsEqualTo(expectedOutput);  // Obfuscated should be the same across runs.
        await Assert.That(dto.PermissionName.Length).IsEqualTo(5);
    }

    [Test]
    [Arguments("sample.permission", "SAMPLE.PERMISSION")]
    [Arguments("test.permission", "TEST.PERMISSION")]
    [Arguments("mixed.Case.Permission", "MIXED.CASE.PERMISSION")]
    [Arguments("another.test.case", "ANOTHER.TEST.CASE")]
    [Arguments("UpperCaseAlready", "UPPERCASEALREADY")]
    public async Task ToUpperCase_ShouldConvertToUpperCorrectly(string input, string expectedOutput) {
        var dto = new PermissionsPropertyDto {
            PermissionName = input
        };

        dto.ToUpperInvariant();

        await Assert.That(dto.PermissionName).IsNotEmpty();
        await Assert.That(dto.PermissionName).IsEqualTo(expectedOutput);
    }

    [Test]
    [Arguments("SampleProperty", "sample.property")]
    [Arguments("Sample.Property", "sample.property")]
    [Arguments("SampleTwo.Property", "sample.two.property")]
    [Arguments("DataUsers.duckies.public", "data.users.duckies.public")]
    [Arguments("DataUsersDuckiesPublic", "data.users.duckies.public")]
    public async Task ParsePrefix_ShouldConvertToPeriodSeparated(string input, string expectedOutput) {
        var dto = new PermissionsPropertyDto {
            PermissionName = input
        };

        dto.ParsePrefix();

        await Assert.That(dto.PermissionName).IsNotEmpty();
        await Assert.That(dto.PermissionName).IsEqualTo(expectedOutput);
    }

    [Test]
    [Arguments("SampleProperty", "sample.property")]
    [Arguments("Sample.Property", "sample.property")]
    [Arguments("SampleTwo.Property", "sample.two.property")]
    [Arguments("DataUsers.duckies.public", "data.users.duckies.public")]
    public async Task ToPeriodSeperated_ShouldConvertCamelCaseToPeriodSeparated(string input, string expectedOutput) {
        string result = PermissionsPropertyDto.ToPeriodSeperated(input);
        
        await Assert.That(result).IsNotEmpty();
        await Assert.That(result).IsEqualTo(expectedOutput);
    }
}