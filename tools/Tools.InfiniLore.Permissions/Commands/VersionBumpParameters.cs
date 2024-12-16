// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CliArgsParser;

namespace Tools.InfiniLore.Permissions.Commands;

// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
public class VersionBumpParameters : ICommandParameters {
    [ArgValue("root")] [Description("The root directory of the project to update")]
    public string Root { get; set; } = "../../../../../";
    
    [ArgValue("section")] [Description("The section of the version to bump. One of: Major, Minor, Patch")]
    public string? SectionStringValue { get; set; }
    
    [ArgFlag("push-to-remote")] [Description("Push the changes to the remote repository")]
    public bool PushToRemote { get; set; }
    
    public VersionSection Section => Enum.Parse<VersionSection>(SectionStringValue ?? "None", ignoreCase:true);
}

public enum VersionSection {
    None = 0,
    Major,
    Minor,
    Patch,
    Addendum
}
