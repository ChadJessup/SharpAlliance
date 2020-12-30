using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharpAlliance.Platform.Interfaces;

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
        public IConfiguration Configuration { get; init; }
        public ILibraryManager LibraryManager { get; init; }
        public IVideoManager VideoManager { get; init; }
        public IInputManager InputManager { get; init; }
        public IFileManager FileManager { get; init; }
        public ISoundManager SoundManager { get; init; }

        public bool Initialize()
        {
            var success = true;

            success &= this.LibraryManager.Initialize();
            success &= this.VideoManager.Initialize();
            success &= this.InputManager.Initialize();
            success &= this.FileManager.Initialize();
            success &= this.SoundManager.Initialize();

            return success;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.LibraryManager?.Dispose();
                    this.VideoManager?.Dispose();
                    this.InputManager?.Dispose();
                    this.FileManager?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
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
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
