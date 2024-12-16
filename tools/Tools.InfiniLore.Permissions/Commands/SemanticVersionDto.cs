// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Tools.InfiniLore.Permissions.Commands;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public partial class SemanticVersionDto {
    private uint Major { get; set; }
    private uint Minor { get; set; }
    private uint Patch { get; set; }
    private uint? Preview { get; set; }
    
    [GeneratedRegex(@"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(-preview\.(?<preview>\d+))?$")]
    private static partial Regex FindVersionRegex { get; }
    
    // -----------------------------------------------------------------------------------------------------------------
    // Methods
    // -----------------------------------------------------------------------------------------------------------------
    public static bool TryParse(string version,[NotNullWhen(true)] out SemanticVersionDto? result) {
        Match match = FindVersionRegex.Match(version);
        if (!match.Success) {
            result = null;
            return false;
        }

        result = new SemanticVersionDto {
            Major = uint.Parse(match.Groups["major"].Value),
            Minor = uint.Parse(match.Groups["minor"].Value),
            Patch = uint.Parse(match.Groups["patch"].Value),
            Preview = match.Groups["preview"].Success ? uint.Parse(match.Groups["preview"].Value) : null
        };
        return true;
    }

    public void BumpVersion(VersionSection section) {
        switch (section) {
            case VersionSection.Major: {
                Major += 1;
                Minor = 0;
                Patch = 0;
                break;
            }

            case VersionSection.Minor: {
                Minor += 1;
                Patch = 0;
                Preview = null;
                break;
            }

            case VersionSection.Patch: {
                Patch += 1;
                Preview = null;
                break;
            }

            case VersionSection.Preview: {
                Preview ??= 0;
                Preview += 1;
                break;
            }

            // We don't care
            case VersionSection.None:
            default:
                break;
        }
    }
    
    public override string ToString() => Preview is not null 
        ? $"{Major}.{Minor}.{Patch}-preview.{Preview}" 
        : $"{Major}.{Minor}.{Patch}";
}
