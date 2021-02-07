using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Screens;
using SharpAlliance.Platform;

namespace SharpAlliance.Core.Managers
{
    public class ScreenManager : IScreenManager
    {
        // GLOBAL FOR PAL EDITOR 
        // TODO: Move out of global scope once pieces are all in place.
        public static byte CurrentPalette = 0;
        public static int guiBackgroundRect;
        public static bool gfExitPalEditScreen = false;
        public static bool gfExitDebugScreen = false;
        public static bool gfInitRect = true;
        public static bool FirstTime = true;
        public static bool gfDoneWithSplashScreen = false;

        private Dictionary<ScreenName, Type> ScreenTypes { get; set; } = new();
        private readonly GameContext context;
        private Task currentScreenTask;

        public ScreenManager(GameContext context)
        {
            this.context = context;
        }

        public Dictionary<ScreenName, IScreen> Screens { get; set; } = new();
        public Dictionary<Type, ScreenName> ScreenNames { get; set; } = new();

        public IScreen CurrentScreen { get; private set; }
        public ScreenName CurrentScreenName { get; private set; }
        public bool IsInitialized { get; private set; }
        public IScreen guiPendingScreen { get; set; } = NullScreen.Instance;

        public ValueTask<bool> Initialize()
        {
            this.IsInitialized = true;
            return ValueTask.FromResult(true);
        }

        public bool ScreenExists(ScreenName screenName) => this.Screens.ContainsKey(screenName);

        public async ValueTask<TScreen> GetScreen<TScreen>(ScreenName screenName, bool activate = false) where TScreen : IScreen
            => (TScreen)await this.GetScreen(screenName, activate);

        public async ValueTask<IScreen> GetScreen(ScreenName screenName, bool activate = false)
        {
            Type? screenType = null;
            if (!this.Screens.TryGetValue(screenName, out var screen)
                && !this.ScreenTypes.TryGetValue(screenName, out screenType))
            {
                throw new ArgumentException($"Screen {screenName} not found.");
            }

            if (screen is not null)
            {
                if (activate)
                {
                    return await this.ActivateScreen(screen);
                }

                return screen;
            }

            if (screenType is not null)
            {
                screen = ActivatorUtilities.GetServiceOrCreateInstance(this.context.Services, screenType) as IScreen;
            }

            if (screen is null)
            {
                throw new ArgumentException($"Screen {screenName} not found.");
            }

            this.Screens.Add(screenName, screen);

            if (activate)
            {
                return await this.ActivateScreen(screen);
            }

            return screen;
        }

        public ValueTask<IScreen> ActivateScreen(ScreenName screenName)
        {
            if (!this.Screens.TryGetValue(screenName, out var screen))
            {
                if (!this.ScreenTypes.TryGetValue(screenName, out var screenType))
                {
                    throw new ArgumentException($"Screen {screenName} doesn't exists.");
                }

                screen = ActivatorUtilities.GetServiceOrCreateInstance(this.context.Services, screenType) as IScreen;
                this.Screens.Add(screenName, screen!);
                this.ScreenNames.TryAdd(screenType, screenName);
            }

            if (screen is null)
            {
                throw new NullReferenceException($"{screenName} was null for some reason?");
            }

            return this.ActivateScreen(screen);
        }

        public async ValueTask<IScreen> ActivateScreen(IScreen screen)
        {
            if (!screen.IsInitialized)
            {
                screen.IsInitialized = await screen.Initialize();
            }

            this.currentScreenTask = screen.Activate().AsTask();
            this.CurrentScreen = screen;

            return this.CurrentScreen;
        }

        public IScreenManager AddScreen<TScreen>(ScreenName screenName)
            where TScreen : IScreen
        {
            this.ScreenTypes.TryAdd(screenName, typeof(TScreen));

            return this;
        }

        public void Dispose()
        {
            foreach (var screen in this.Screens.Values)
            {
                screen.Dispose();
            }
        }

        public void EndMapScreen(bool v)
        {
        }

        public void ExitLaptop()
        {
        }

        public async ValueTask SetPendingNewScreen(ScreenName pendingScreen)
        {
            this.guiPendingScreen = await this.GetScreen(pendingScreen, activate: false);
        }
    }

    public interface IScreen : IDisposable
    {
        ValueTask<bool> Initialize();

        bool IsInitialized { get; set; }
        ValueTask Activate();

        ValueTask<ScreenName> Handle();

        ScreenState State { get; set; }
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
