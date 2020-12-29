using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Platform
{
    public static class GamePlatformBuilderExtensions
    {
        public static IGamePlatformBuilder AddLibraryManager<TLibraryManager>(this IGamePlatformBuilder builder)
            where TLibraryManager : class, ILibraryManager
        {
            builder.AddDependency<ILibraryManager, TLibraryManager>();

            return builder;
        }
    }
}
