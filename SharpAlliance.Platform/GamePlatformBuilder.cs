﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SharpAlliance.Platform.Interfaces;
using SharpAlliance.Platform.NullManagers;

namespace SharpAlliance.Platform
{
    /// <summary>
    /// Generic game platform.
    /// </summary>
    public class GamePlatformBuilder : IGamePlatformBuilder
    {
        public GamePlatformBuilder(IServiceCollection serviceCollection)
            : this(new ConfigurationBuilder().Build(), serviceCollection)
        { }

        public GamePlatformBuilder(IConfiguration configuration)
            : this(configuration, new ServiceCollection())
        { }

        public GamePlatformBuilder()
            : this(new ConfigurationBuilder().Build(), new ServiceCollection())
        { }

        public GamePlatformBuilder(
            IConfiguration? configuration = null,
            IServiceCollection? serviceCollection = null)
        {
            configuration ??= new ConfigurationBuilder().Build();

            // We want people to be able to provide their own configuration, and
            // overrides, so we'll take their configuration and build a larger configuration
            // on top of that. Provided values will override any defaults.
            this.Configuration = new ConfigurationBuilder()
                .AddJsonFile("SharpAlliance.json", optional: true)
                .AddIniFile("sgp.ini", optional: true) // JA2 used this...
                .AddConfiguration(configuration)
                .Build();

            serviceCollection ??= new ServiceCollection();
            this.Services = this.BuildDependencyTree(configuration, serviceCollection);
        }

        public GamePlatformBuilder(IConfigurationBuilder configurationBuilder)
            : this(configurationBuilder.Build(), new ServiceCollection())
        {
        }

        public GameContext? GameContext { get; set; }
        public IServiceCollection Services { get; set; }
        public IConfiguration Configuration { get; set; }

        public bool gfProgramIsRunning { get; set; } // Turn this to FALSE to exit program
        public int giStartMem { get; set; }
        public string gzCommandLine { get; set; }    // Command line given
        public int gbPixelDepth { get; set; }              // GLOBAL RUN-TIME SETTINGS
        public bool gfDontUseDDBlits { get; set; } // GLOBAL FOR USE OF DD BLITTING

        public IServiceCollection BuildDependencyTree(IConfiguration configuration, IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging();
            serviceCollection.AddOptions();
            serviceCollection.AddLocalization();

            serviceCollection.TryAddSingleton<GameContext>();
            serviceCollection.TryAddSingleton(configuration);
            serviceCollection.TryAddSingleton(serviceCollection);
            // Null Managers for when an implementation doesn't provide one.
            // We still inject something that won't crash or cause null ref exceptions.
            serviceCollection.TryAddSingleton<ISoundManager, NullSoundManager>();
            serviceCollection.TryAddSingleton<ISound2dManager, NullSoundManager>();
            serviceCollection.TryAddSingleton<ISound3dManager, NullSoundManager>();
            serviceCollection.TryAddSingleton<IMusicManager, NullMusicManager>();

            // These can be overwritten by custom implementations, but are fairly generic.
            serviceCollection.TryAddSingleton<ITimerManager, TimerManager>();

            return serviceCollection;
        }

        public GameContext Build()
        {
            // Since dependencies are expected to need GameContext, we'll build it ourselves.
            // This is also a bit easier to debug for people new to this pattern.
            // Full DI is used after this...
            var provider = this.Services.BuildServiceProvider();

            this.GameContext = provider.GetRequiredService<GameContext>();

            this.GameContext.FileManager = provider.GetRequiredService<IFileManager>();
            this.GameContext.SoundManager = provider.GetRequiredService<ISoundManager>();
            this.GameContext.TimerManager = provider.GetRequiredService<ITimerManager>();
            this.GameContext.ClockManager = provider.GetRequiredService<IClockManager>();
            this.GameContext.GameLogic = provider.GetRequiredService<IGameLogic>();
            this.GameContext.OSManager = provider.GetRequiredService<IOSManager>();
            this.GameContext.MusicManager = provider.GetRequiredService<IMusicManager>();

            // Purposefully block on initialize with .Result.
            var success = this.GameContext.Initialize().Result;

            if (!success)
            {

            }

            return this.GameContext;
        }

        public IGamePlatformBuilder AddDependency<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            this.Services.AddSingleton<TService, TImplementation>();

            return this;
        }
    }
}
