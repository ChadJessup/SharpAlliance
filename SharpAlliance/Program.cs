using System.IO;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Veldrid.SPIRV;
using System.Text;
using SharpDX;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using SharpAlliance.Platform;
using System.Runtime.CompilerServices;
using SharpAlliance.Platform.Interfaces;
using SharpAlliance.Core;

namespace SharpAlliance
{
    /// <summary>
    /// This executable is the one that most closely matches the original Jagged Alliance 2
    /// game. It uses DirectDraw, raw inputs, etc.
    /// 
    /// Think of this as an example for how to implement other versions of SharpAlliance.
    /// </summary>
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            // By default, GamePlatform will look for SharpAlliance.json for configuration options,
            // but let's add per-machine configuration as well (and commandline args),
            // since I dev this on multiple machines...and need examples.
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile($"SharpAlliance.{Environment.MachineName}.json", optional: true)
                .AddCommandLine(args);

            // pass configuration to GamePlatformBuilder...
            var platformBuilder = new GamePlatformBuilder(configurationBuilder)
                // These are the components that most closely match the original source
                // Video/Audio/Input, etc
                .AddOriginalComponents();

            // Initialize the game platform as a whole, which returns a game context
            // containing 
            using var context = platformBuilder.Build();


            return await Task.FromResult(0);
        }
    }
 
    public static class StandardSharpAllianceExtensions
    {
        public static IGamePlatformBuilder AddOriginalComponents(this IGamePlatformBuilder builder)
        {
            builder.AddLibraryManager<LibraryFileManager>();
            return builder;
        }
    }
}
