using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core.SubSystems;

public class QuestEngine
{
    private const int MAX_QUESTS = 30;
    private const int MAX_FACTS = 65536;
    private const int NUM_FACTS = 500;			//If you increase this number, add entries to the fact text list in QuestText.c

    public int[] gubQuest { get; } = new int[MAX_QUESTS];
    public int[] gubFact { get; } = new int[NUM_FACTS]; // this has to be updated when we figure out how many facts we have

    public int gsFoodQuestSectorX;
    public int gsFoodQuestSectorY;
}

public enum MapRows : int
{
    MAP_ROW_A = 1,
    MAP_ROW_B = 2,
    MAP_ROW_C = 3,
    MAP_ROW_D = 4,
    MAP_ROW_E = 5,
    MAP_ROW_F = 6,
    MAP_ROW_G = 7,
    MAP_ROW_H = 8,
    MAP_ROW_I = 9,
    MAP_ROW_J = 10,
    MAP_ROW_K = 11,
    MAP_ROW_L = 12,
    MAP_ROW_M = 13,
    MAP_ROW_N = 14,
    MAP_ROW_O = 15,
    MAP_ROW_P = 16,
}

public enum Quest
{
    DELIVER_LETTER = 0,
    FOOD_ROUTE,
    KILL_TERRORISTS,
    KINGPIN_IDOL,
    KINGPIN_MONEY,
    RUNAWAY_JOEY,
    RESCUE_MARIA,
    CHITZENA_IDOL,
    HELD_IN_ALMA,
    INTERROGATION,
    ARMY_FARM, // 10
    FIND_SCIENTIST,
    DELIVER_VIDEO_CAMERA,
    BLOODCATS,
    FIND_HERMIT,
    CREATURES,
    CHOPPER_PILOT,
    ESCORT_SKYRIDER,
    FREE_DYNAMO,
    ESCORT_TOURISTS,
    FREE_CHILDREN,    // 20
    LEATHER_SHOP_DREAM,
    KILL_DEIDRANNA = 25,
}
