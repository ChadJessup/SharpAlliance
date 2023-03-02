using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class AI
{
}

// NB THESE THREE FLAGS SHOULD BE REMOVED FROM CODE

public enum AIDEFINES
{
    
    AI_RTP_OPTION_CAN_RETREAT = 0x01,
    AI_RTP_OPTION_CAN_SEEK_COVER = 0x02,
    AI_RTP_OPTION_CAN_HELP = 0x04,
    AI_CAUTIOUS = 0x08,
    AI_HANDLE_EVERY_FRAME = 0x10,
    AI_ASLEEP = 0x20,
    AI_LOCK_DOOR_INCLUDES_CLOSE = 0x40,
    AI_CHECK_SCHEDULE = 0x80,
};

public enum QUOTE_ACTION_ID //QuoteActionType
{
    CHECKFORDEST = 1,
    TURNTOWARDSPLAYER,
    DRAWGUN,
    LOWERGUN,
    TRAVERSE_EAST,
    TRAVERSE_SOUTH,
    TRAVERSE_WEST,
    TRAVERSE_NORTH,
}
