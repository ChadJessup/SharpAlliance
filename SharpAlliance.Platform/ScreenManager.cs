using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Platform
{
    public class ScreenManager : IScreenManager
    {
        private Dictionary<string, Type> ScreenTypes { get; set; } = new();
        private readonly GameContext context;
        private Task currentScreenTask;

        public ScreenManager(GameContext context)
        {
            this.context = context;
        }

        public Dictionary<string, IScreen> Screens { get; set; } = new();
        public IScreen CurrentScreen { get; private set; }

        public bool Initialize()
        {
            return true;
        }

        public bool ScreenExists(string screenName) => this.Screens.ContainsKey(screenName);

        public async ValueTask<IScreen> GetScreen(string screenName, bool activate = false)
        {
            if (!this.Screens.TryGetValue(screenName, out var screen))
            {
                throw new ArgumentException($"Screen {screenName} not found.");
            }

            if (activate)
            {
                return await this.ActivateScreen(screen);
            }

            return screen;
        }

        public ValueTask<IScreen> ActivateScreen(string screenName)
        {
            if (!this.Screens.TryGetValue(screenName, out var screen))
            {
                if (!this.ScreenTypes.TryGetValue(screenName, out var screenType))
                {
                    throw new ArgumentException($"Screen {screenName} doesn't exists.");
                }

                screen = ActivatorUtilities.GetServiceOrCreateInstance(this.context.Services, screenType) as IScreen;
            }

            if (screen is null)
            {
                throw new NullReferenceException($"{screenName} was null for some reason?");
            }

            return this.ActivateScreen(screen);
        }

        public ValueTask<IScreen> ActivateScreen(IScreen screen)
        {
            if (!screen.IsInitialized)
            {
                screen.IsInitialized = screen.Initialize();
            }

            this.currentScreenTask = screen.Activate().AsTask();
            this.CurrentScreen = screen;

            return ValueTask.FromResult(this.CurrentScreen);
        }

        public IScreenManager AddScreen<TScreen>(string screenName)
            where TScreen : IScreen
        {
            this.ScreenTypes.TryAdd(screenName, typeof(TScreen));

            return this;
        }

        public ValueTask<int> HandleCurrentScreen()
        {
            if (this.CurrentScreen != null)
            {
                return this.CurrentScreen.Handle();
            }

            return ValueTask.FromResult(-1);
        }

        public void Dispose()
        {
            foreach (var screen in this.Screens.Values)
            {
                screen.Dispose();
            }
        }
    }

    public interface IScreen : IDisposable
    {
        bool Initialize();

        bool IsInitialized { get; set; }
        ValueTask Activate();

        ValueTask<int> Handle();
    }

    public enum ScreenState
    {
        Unknown = 0,
        Inactive,
        Initializing,
        Active,
        ShuttingDown,
    }
}
