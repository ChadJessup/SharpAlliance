using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharpAlliance.Core;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using Vortice;

namespace SharpAlliance
{
    /// <summary>
    /// This executable is the one that most closely matches the original Jagged Alliance 2
    /// game. It uses Direct2d, raw inputs, etc.
    /// 
    /// Think of this as an example for how to implement other versions of SharpAlliance.
    /// </summary>
    public class Program //: Application
    {
        private static readonly CancellationTokenSource cts = new();

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
                .AddVideoManager<VeldridVideoManager>()
                .AddGameLogic<SharpAllianceGameLogic>()
                .AddOtherComponents();

            // Initialize the game platform as a whole, which returns a game context
            // containing platform components for core game logic to use.
            using var context = platformBuilder.Build();

            // The rest is up to game-specific logic, pass the context into a loop and go.
            var result = await context.StartGameLoop(cts.Token);

            return result;
        }

        public Program()// : base(true) { }
        { }
    }

    public static class StandardSharpAllianceExtensions
    {
        public static IGamePlatformBuilder AddLibraryManager<TLibraryManager>(this IGamePlatformBuilder builder)
            where TLibraryManager : class, ILibraryManager
        {
            builder.AddDependency<ILibraryManager, TLibraryManager>();

            return builder;
        }

        public static IGamePlatformBuilder AddOtherComponents(this IGamePlatformBuilder builder)
        {
            builder.AddDependency<IOSManager, WindowsSubSystem>();
            builder.AddDependency<IVideoObjectManager, VideoObjectManager>();

            builder.Services.AddSingleton<Globals>();
            builder.Services.AddSingleton<RenderWorld>();
            builder.Services.AddSingleton<FontSubSystem>();
            builder.Services.AddSingleton<MouseSubSystem>();
            builder.Services.AddSingleton<ButtonSubSystem>();
            builder.Services.AddSingleton<CursorSubSystem>();
            builder.Services.AddSingleton<SaveGameSubSystem>();
            builder.Services.AddSingleton<MessageBoxSubSystem>();
            builder.Services.AddSingleton<HelpScreenSubSystem>();
            builder.Services.AddSingleton<MapScreenInterfaceMap>();
            builder.Services.AddSingleton<MessageSubSystem>();


            return builder;
        }
    }
}
