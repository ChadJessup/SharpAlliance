using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharpAlliance.Platform.Interfaces;
using SharpAlliance.Platform.NullManagers;

namespace SharpAlliance.Platform
{
    public class GameContext : IDisposable
    {
        private readonly ILogger<GameContext>? logger;

        // protect against double dispose.
        private bool disposedValue;

        public GameContext(
            ILogger<GameContext>? logger,
            IServiceProvider services,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.Services = services;
            this.Configuration = configuration;

            this.logger?.LogDebug($"Initialized {nameof(GameContext)}");
        }

        public IServiceProvider Services { get; }
        public IConfiguration Configuration { get; }
        public IGameLogic GameLogic { get; set; }
        public ILibraryManager LibraryManager { get; set; }
        public IVideoManager VideoManager { get; set; } = new NullVideoManager();
        public IInputManager InputManager { get; set; } = new NullInputManager();
        public IFileManager FileManager { get; set; } = new NullFileManager();
        public ISoundManager SoundManager { get; set; } = new NullSoundManager();
        public IScreenManager ScreenManager { get; set; } = new NullScreenManager();
        public ITimerManager TimerManager { get; set; } = new TimerManager();
        public IClockManager ClockManager { get; set; } = new ClockManager();

        public async Task<int> StartGameLoop(CancellationToken token = default)
        {
            if (this.GameLogic is null)
            {
                throw new NullReferenceException("GameLogic must be provided!");
            }

            var result = await Task.Run(() => this.GameLogic?.GameLoop(token));

            return result;
        }

        public async Task<bool> Initialize()
        {
            var success = true;

            success &= await this.GameLogic.Initialize();;
            success &= await this.LibraryManager.Initialize();
            success &= await this.VideoManager.Initialize();
            success &= await this.InputManager.Initialize();
            success &= await this.FileManager.Initialize();
            success &= await this.SoundManager.Initialize();
            success &= await this.ScreenManager.Initialize();
            success &= await this.TimerManager.Initialize();
            success &= await this.ClockManager.Initialize();

            return success;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.GameLogic?.Dispose();
                    this.LibraryManager?.Dispose();
                    this.VideoManager.Dispose();
                    this.InputManager.Dispose();
                    this.FileManager.Dispose();
                    this.SoundManager.Dispose();
                    this.ScreenManager.Dispose();
                    this.TimerManager.Dispose();
                    this.ClockManager.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this.disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~GameContext()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
