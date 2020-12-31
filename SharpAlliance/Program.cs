using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SharpAlliance.Core;
using SharpAlliance.Core.Screens;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance
{
    /// <summary>
    /// This executable is the one that most closely matches the original Jagged Alliance 2
    /// game. It uses Direct2d, raw inputs, etc.
    /// 
    /// Think of this as an example for how to implement other versions of SharpAlliance.
    /// </summary>
    public class Program
    {
        private CancellationTokenSource cts = new CancellationTokenSource();

        public static async Task<int> Main(string[] args)
        {
            var program = new Program();

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
                .AddVideoManager<DirectDrawVideoManager>()
                .AddOtherComponents();

            // Initialize the game platform as a whole, which returns a game context
            // containing platform components for core game logic to use.
            using var context = platformBuilder.Build();

            // Register all our screens...
            RegisterGameScreens(context);

            // Call some game specific start up and splash screen!
            var splashScreen = await context.ScreenManager.ActivateScreen(ScreenNames.SplashScreen);

            // The rest is up to game-specific logic, pass the context into a loop and go.
            await Task.WhenAll(program.GameLoop(context, program.cts.Token));

            return await Task.FromResult(0);
        }

        private static void RegisterGameScreens(GameContext context)
        {
            var sm = context.ScreenManager;

            sm.AddScreen<SplashScreen>(ScreenNames.SplashScreen);
        }

        public Task<int> GameLoop(GameContext context, CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {

            }

            return Task.FromResult(0);
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
