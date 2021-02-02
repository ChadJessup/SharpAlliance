using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Platform
{
    public static class GamePlatformBuilderExtensions
    {
        public static IGamePlatformBuilder AddGameLogic<TGameLogic>(this IGamePlatformBuilder builder)
            where TGameLogic : class, IGameLogic
        {
            builder.AddDependency<IGameLogic, TGameLogic>();

            return builder;
        }
    }
}
