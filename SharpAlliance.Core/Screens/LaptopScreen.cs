using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems.LaptopSubSystem;

namespace SharpAlliance.Core.Screens;

public class LaptopScreen : IScreen
{
    private readonly ILogger<LaptopScreen> logger;
    private readonly Laptop laptop;

    public bool IsInitialized { get; set; }
    public ScreenState State { get; set; }

    public LaptopScreen(ILogger<LaptopScreen> logger, Laptop laptop)
    {
        this.logger = logger;
        this.laptop = laptop;
    }

    public ValueTask Activate()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask<ScreenName> Handle()
    {
        return ValueTask.FromResult(ScreenName.LAPTOP_SCREEN);
    }

    public ValueTask<bool> Initialize()
    {
        Laptop.LaptopScreenInit();
        return ValueTask.FromResult(true);
    }

    public void Dispose()
    {
    }

    public ValueTask Deactivate()
    {
        return ValueTask.CompletedTask;
    }
}
