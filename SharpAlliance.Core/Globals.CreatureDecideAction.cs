using System.Collections.Generic;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{
    public static bool CAN_CALL(SOLDIERTYPE s) => s.ubBodyType != SoldierBodyTypes.BLOODCAT && s.ubBodyType != SoldierBodyTypes.LARVAE_MONSTER && s.ubBodyType != SoldierBodyTypes.INFANT_MONSTER;
    public static bool CAN_LISTEN_TO_CALL(SOLDIERTYPE s) => s.ubBodyType != SoldierBodyTypes.BLOODCAT && s.ubBodyType != SoldierBodyTypes.LARVAE_MONSTER;

    public const int FRENZY_THRESHOLD = 8;
    public const int MAX_EAT_DIST = 5;

    public static Dictionary<CALL, List<int>> gbCallPriority = new()
{
    { CALL.NONE, new() {0, 0, 0 } },//CALL_NONE
	{ CALL.SINGLE_PREY, new() {3, 5, 12} },//CALL_1_PREY
	{ CALL.MULTIPLE_PREY, new() {5, 9, 12} },//CALL_MULTIPLE_PREY
	{ CALL.ATTACKED, new() {4, 7, 12} },//CALL_ATTACKED
	{ CALL.CRIPPLED, new() {6, 9, 12} },//CALL_CRIPPLED
};

    public static Dictionary<CALL, int> gbHuntCallPriority = new()
    {
        { CALL.SINGLE_PREY, 4 }, //CALL_1_PREY
        { CALL.MULTIPLE_PREY, 5 }, //CALL_MULTIPLE_PREY
        { CALL.ATTACKED, 7 }, //CALL_ATTACKED
        { CALL.CRIPPLED, 8 },  //CALL_CRIPPLED
    };

    public const int PRIORITY_DECR_DISTANCE = 30;
    public const CALL CALL_1_OPPONENT = CALL.SINGLE_PREY;
    public const CALL CALL_MULTIPLE_OPPONENT = CALL.MULTIPLE_PREY;

}
