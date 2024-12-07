// ---------------------------------------------------------------------------------------------------------------------
// Imports
// ---------------------------------------------------------------------------------------------------------------------
using CliArgsParser;
using Microsoft.Extensions.DependencyInjection;
using Tools.InfiniLore.Permissions.Commands;

namespace Tools.InfiniLore.Permissions;
// ---------------------------------------------------------------------------------------------------------------------
// Code
// ---------------------------------------------------------------------------------------------------------------------
internal static class Program {
    public async static Task Main(string[] args) {
        IServiceCollection serviceCollection = new ServiceCollection();

        serviceCollection.AddCliArgsParser(configuration =>
            configuration
                .SetConfig(new CliArgsParserConfig {
                    Overridable = true,
                    GenerateShortNames = true
                })
                .AddFromType<VersionBumpCommands>()
        );

        ServiceProvider provider = serviceCollection.BuildServiceProvider();

        var argsParser = provider.GetRequiredService<IArgsParser>();
        await argsParser.ParseAsyncLinear(args);
    }
}
