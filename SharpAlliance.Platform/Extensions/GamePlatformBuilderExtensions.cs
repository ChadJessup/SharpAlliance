using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Platform
{
    public static class GamePlatformBuilderExtensions
    {
        public static IGamePlatformBuilder AddInputManager<TInputManager>(this IGamePlatformBuilder builder)
            where TInputManager : class, IInputManager
        {
            builder.AddDependency<IInputManager, TInputManager>();

            return builder;

        }

        public static IGamePlatformBuilder AddVideoManager<TVideoManager>(this IGamePlatformBuilder builder)
            where TVideoManager : class, IVideoManager
        {
            builder.AddDependency<IVideoManager, TVideoManager>();

            return builder;

        }

        public static IGamePlatformBuilder AddLibraryManager<TLibraryManager>(this IGamePlatformBuilder builder)
            where TLibraryManager : class, ILibraryManager
        {
            builder.AddDependency<ILibraryManager, TLibraryManager>();

            return builder;
        }
    }
}
