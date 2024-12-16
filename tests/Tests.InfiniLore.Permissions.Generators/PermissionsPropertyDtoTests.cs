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
using Xunit;

namespace Tests.InfiniLore.Permissions.Generators;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
[TestSubject(typeof(PermissionsPropertyDto))]
public class PermissionsPropertyDtoTests {
    [Fact]
    public void FromPropertyDeclarationSyntax_ShouldConvertCorrectly() {
        const string code = """
            public class TestClass
            {
                [Prefix("Test")]
                public string SampleProperty { get; set; }
            }
            """;
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
        PropertyDeclarationSyntax propertyNode = syntaxTree.GetRoot()
            .DescendantNodes().OfType<PropertyDeclarationSyntax>()
            .First();

        PermissionsPropertyDto dto = PermissionsPropertyDto.FromPropertyDeclarationSyntax(propertyNode);

        Assert.Equal("public", dto.AccessModifier);
        Assert.Equal(string.Empty, dto.StaticPrefix);
        Assert.Equal("SampleProperty", dto.PropertyName);
        Assert.Equal("Test.sample.property", dto.PermissionName);
    }

    [Fact]
    public void ToPropertyString_ShouldReturnCorrectFormat() {
        var dto = new PermissionsPropertyDto {
            AccessModifier = "public",
            StaticPrefix = "",
            PropertyName = "SampleProperty",
            PermissionName = "sample.permission"
        };

        const string expected = "public partial string SampleProperty { get => \"sample.permission\"; }";
        Assert.Equal(expected, dto.ToPropertyString());
    }

    [Theory]
    [InlineData("sample.permission", "aihuI")]
    public void ObfuscatePermissionName_ShouldObfuscateCorrectly(string input, string expectedOutput) {
        var dto = new PermissionsPropertyDto {
            PermissionName = input
        };

        using var hasher = SHA256.Create();
        dto.ObfuscatePermissionName(hasher);

        Assert.NotEmpty(dto.PermissionName);
        Assert.Equal(5, dto.PermissionName.Length);
        Assert.Equal(expectedOutput, dto.PermissionName); // Obfuscated should be the same across runs.
    }

    [Theory]
    [InlineData("sample.permission", "SAMPLE.PERMISSION")]
    [InlineData("test.permission", "TEST.PERMISSION")]
    [InlineData("mixed.Case.Permission", "MIXED.CASE.PERMISSION")]
    [InlineData("another.test.case", "ANOTHER.TEST.CASE")]
    [InlineData("UpperCaseAlready", "UPPERCASEALREADY")]
    public void ToUpperCase_ShouldConvertToUpperCorrectly(string input, string expectedOutput) {
        var dto = new PermissionsPropertyDto {
            PermissionName = input
        };

        dto.ToUpperInvariant();

        Assert.Equal(expectedOutput, dto.PermissionName);
    }

    [Theory]
    [InlineData("SampleProperty", "sample.property")]
    [InlineData("Sample.Property", "sample.property")]
    [InlineData("SampleTwo.Property", "sample.two.property")]
    [InlineData("DataUsers.duckies.public", "data.users.duckies.public")]
    [InlineData("DataUsersDuckiesPublic", "data.users.duckies.public")]
    public void ParsePrefix_ShouldConvertToPeriodSeparated(string input, string expectedOutput) {
        var dto = new PermissionsPropertyDto {
            PermissionName = input
        };

        dto.ParsePrefix();

        Assert.Equal(expectedOutput, dto.PermissionName);
    }

    [Theory]
    [InlineData("SampleProperty", "sample.property")]
    [InlineData("Sample.Property", "sample.property")]
    [InlineData("SampleTwo.Property", "sample.two.property")]
    [InlineData("DataUsers.duckies.public", "data.users.duckies.public")]
    public void ToPeriodSeperated_ShouldConvertCamelCaseToPeriodSeparated(string input, string expectedOutput) {
        string result = PermissionsPropertyDto.ToPeriodSeperated(input);
        Assert.Equal(expectedOutput, result);
    }
}