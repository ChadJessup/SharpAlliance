using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using Veldrid;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Managers;

public class ScreenManager : IScreenManager
{
    // GLOBAL FOR PAL EDITOR 
    // TODO: Move out of global scope once pieces are all in place.
    public static byte CurrentPalette = 0;
    public static bool FirstTime = true;

    private Dictionary<ScreenName, Type> ScreenTypes { get; set; } = new();
    private readonly GameContext context;
    private readonly ILogger<ScreenManager> logger;
    private Task currentScreenTask;

    public ScreenManager(ILogger<ScreenManager> logger, GameContext context)
    {
        this.context = context;
        this.logger = logger;
    }

    public Dictionary<ScreenName, IScreen> Screens { get; set; } = new();
    public Dictionary<Type, ScreenName> ScreenNames { get; set; } = new();

    public static IScreen CurrentScreen { get; private set; }
    public ScreenName CurrentScreenName { get; private set; }
    public bool IsInitialized { get; private set; }
    public IScreen guiPendingScreen { get; set; } = NullScreen.Instance;

    public ValueTask<bool> Initialize()
    {
        this.context.Services.GetRequiredService<FontSubSystem>().Initialize();
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

    public static void Draw(IVideoManager videoManager)
    {
        CurrentScreen.Draw(videoManager);
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

        if (CurrentScreen is not null)
        {
            await CurrentScreen.Deactivate();
        }

        currentScreenTask = screen.Activate().AsTask();
        CurrentScreen = screen;

        return CurrentScreen;
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
        Console.WriteLine($"Setting next pending screen: {pendingScreen}");
        this.guiPendingScreen = await this.GetScreen(pendingScreen, activate: false);
    }
}

public enum ScreenState
{
    Unknown = 0,
    Inactive,
    Initializing,
    Active,
    ShuttingDown,
}
