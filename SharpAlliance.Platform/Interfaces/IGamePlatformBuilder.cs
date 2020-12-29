namespace SharpAlliance.Platform.Interfaces
{
    public interface IGamePlatformBuilder
    {
        GameContext Build();
        IGamePlatformBuilder AddDependency<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;
    }
}
