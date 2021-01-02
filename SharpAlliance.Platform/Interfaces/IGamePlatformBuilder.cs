using Microsoft.Extensions.DependencyInjection;

namespace SharpAlliance.Platform.Interfaces
{
    public interface IGamePlatformBuilder
    {
        IServiceCollection Services { get; }
        GameContext Build();
        IGamePlatformBuilder AddDependency<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;
    }
}
