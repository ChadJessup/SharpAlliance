using Microsoft.Extensions.Logging;

namespace SharpAlliance.Platform
{
    public static class LoggingEventId
    {
        public static EventId MEMORY_MANAGER = new(0, nameof(MEMORY_MANAGER));
        public static EventId FILE_MANAGER = new(1, nameof(FILE_MANAGER));
        public static EventId DATABASE_MANAGER = new(2, nameof(DATABASE_MANAGER));
        public static EventId GAME = new(3, nameof(GAME));
        public static EventId SGP = new(4, nameof(SGP));
        public static EventId VIDEO = new(5, nameof(VIDEO));
        public static EventId INPUT = new(6, nameof(INPUT));
        public static EventId STACK_CONTAINERS = new(7, nameof(STACK_CONTAINERS));
        public static EventId LIST_CONTAINERS = new(8, nameof(LIST_CONTAINERS));
        public static EventId QUEUE_CONTAINERS = new(9, nameof(QUEUE_CONTAINERS));
        public static EventId PRILIST_CONTAINERS = new(10, nameof(PRILIST_CONTAINERS));
        public static EventId HIMAGE = new(11, nameof(HIMAGE));
        public static EventId ORDLIST_CONTAINERS = new(12, nameof(ORDLIST_CONTAINERS));
        public static EventId ThreeDENGINE = new(13, nameof(ThreeDENGINE));
        public static EventId VIDEOOBJECT = new(14, nameof(VIDEOOBJECT));
        public static EventId FONT_HANDLER = new(15, nameof(FONT_HANDLER));
        public static EventId VIDEOSURFACE = new(16, nameof(VIDEOSURFACE));
        public static EventId MouseSystem = new(17, nameof(MouseSystem));
        public static EventId BUTTON_HANDLER = new(18, nameof(BUTTON_HANDLER));
        public static EventId MUTEX = new(19, nameof(MUTEX));
        public static EventId BLIT_QUEUE = new(20, nameof(BLIT_QUEUE));

        // TODO: Move to SharpAlliance.Core
        public static EventId JA2 = new(21, nameof(JA2));
        public static EventId JA2OPPLIST = new(22, nameof(JA2OPPLIST));
        public static EventId JA2AI = new(23, nameof(JA2AI));
    }
}
