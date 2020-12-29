using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SharpAlliance.Core;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;

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
            IGamePlatformBuilder platformBuilder = new GamePlatformBuilder(configurationBuilder);

            // Adding components that as closely match Jagged Alliance 2's internals as possible.
            // These components can have other components injected in when instantiated.
            platformBuilder
                .AddLibraryManager<LibraryFileManager>()
                .AddInputManager<InputManager>()
                .AddVideoManager<VideoManager>()
                .AddOtherComponents();

            // Initialize the game platform as a whole, which returns a game context
            // containing platform components for core game logic to use.
            using var context = platformBuilder.Build();


            return await Task.FromResult(0);
        }
    }

    public static class StandardSharpAllianceExtensions
    {
        public static IGamePlatformBuilder AddOtherComponents(this IGamePlatformBuilder builder)
        {
            return builder;
        }
    }
}
