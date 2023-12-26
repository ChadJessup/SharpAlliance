using System.Collections.Generic;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{

    public const string ANIMPROFILEFILENAME = "BINARYDATA\\JA2PROF.DAT";
    public static int gubNumAnimProfiles = 0;

    public static Dictionary<AnimationSurfaceTypes, List<int>> gbAnimUsageHistory = new();// [NUMANIMATIONSURFACETYPES][MAX_NUM_SOLDIERS];
    public static Dictionary<NPCID, ANIM_PROF> gpAnimProfiles = new();

    // BODY TYPES
    // RGM = Regular Male
    // (RG) = Body desc ( Regular - RG, Short Stocky ( SS ), etc
    // (M) = Sex, Male, female
    public static bool IS_MERC_BODY_TYPE(SOLDIERTYPE p) => (p.ubBodyType <= SoldierBodyTypes.REGFEMALE) ? true : false;
    public static bool IS_CIV_BODY_TYPE(SOLDIERTYPE p) => (p.ubBodyType >= SoldierBodyTypes.FATCIV) && (p.ubBodyType <= SoldierBodyTypes.CRIPPLECIV);


    public static Dictionary<AnimationSurfaceTypes, AnimationSurfaceType> gAnimSurfaceDatabase = new();
    public static Dictionary<SoldierBodyTypes, Dictionary<StructData, AnimationStructureType>> gAnimStructureDatabase = new()
    {
        {
            SoldierBodyTypes.REGMALE,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\M_STAND.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\M_PRONE.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\M_FALL.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        },
        {
            SoldierBodyTypes.BIGMALE,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\M_STAND.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\M_PRONE.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\M_FALL.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        },
        {
            SoldierBodyTypes.STOCKYMALE,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\M_STAND.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\M_PRONE.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\M_FALL.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        },
        {
            // Reg Female
            SoldierBodyTypes.REGFEMALE,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\M_STAND.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\M_PRONE.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\M_FALL.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        },
        {
            // Adult female creature
            SoldierBodyTypes.ADULTFEMALEMONSTER,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        },
        {
            SoldierBodyTypes.AM_MONSTER,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        },
        {
            // Young Adult female creature
            SoldierBodyTypes.YAF_MONSTER,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        },
        {
            // Young Adult male creature
            SoldierBodyTypes.YAM_MONSTER,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        },
        {
            // larvea creature
            SoldierBodyTypes.LARVAE_MONSTER,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\L_BREATH.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\L_BREATH.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\L_BREATH.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\L_BREATH.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\L_BREATH.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        },
        {
            // infant creature
            SoldierBodyTypes.INFANT_MONSTER,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\I_BREATH.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\I_BREATH.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\I_BREATH.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\I_BREATH.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\I_BREATH.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\I_BREATH.JSD") },
            }
        },
        {
            // Queen creature
            SoldierBodyTypes.QUEENMONSTER,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\Q_READY.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\Q_READY.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\Q_READY.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\Q_READY.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\Q_READY.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        },
        {
            // Fat civ
            SoldierBodyTypes.FATCIV,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\M_STAND.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\M_PRONE.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\M_FALL.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        },
        {
            // man civ
            SoldierBodyTypes.MANCIV,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\M_STAND.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\M_PRONE.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\M_FALL.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        },
        {
            // miniskirt civ
            SoldierBodyTypes.MINICIV,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\M_STAND.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\M_PRONE.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\M_FALL.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        },
        {
            // dress civ
            SoldierBodyTypes.DRESSCIV,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\M_STAND.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\M_PRONE.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\M_FALL.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        },
        {
            // kid civ
            SoldierBodyTypes.KIDCIV,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\K_STAND.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\K_CROUCH.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\K_CROUCH.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\M_PRONE.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\M_PRONE.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_PRONE.JSD") },
            }
        },
        {
            // hat kid civ
            SoldierBodyTypes.HATKIDCIV,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\K_STAND.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\K_CROUCH.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\K_CROUCH.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\M_PRONE.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\M_PRONE.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_PRONE.JSD") },
            }
        },
        {
            // cripple civ
            SoldierBodyTypes.CRIPPLECIV,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\M_FALL.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        },
        {
            // cow
            SoldierBodyTypes.COW,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\CW_BREATH.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\CW_BREATH.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\CW_BREATH.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\CW_BREATH.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\CW_BREATH.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        },
        {
            // crow
            SoldierBodyTypes.CROW,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\CR_STAND.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\CR_CROUCH.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\CR_PRONE.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\CR_PRONE.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\CR_PRONE.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        },
        {
            // CAT
            SoldierBodyTypes.BLOODCAT,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\CT_BREATH.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\CT_BREATH.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\CT_BREATH.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\CT_BREATH.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\CT_BREATH.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        },
        {
            // ROBOT1
            SoldierBodyTypes.ROBOTNOWEAPON,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\J_R_BRET.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\J_R_BRET.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\J_R_BRET.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\J_R_BRET.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\J_R_BRET.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\J_R_BRET.JSD") },
            }
        },
        {
            // vech 1
            SoldierBodyTypes.HUMVEE,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\HMMV.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\HMMV.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\HMMV.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\HMMV.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\HMMV.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        },
        {
            // tank 1
            SoldierBodyTypes.TANK_NW,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\TNK_SHT.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\TNK_SHT.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\TNK_SHT.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\TNK_SHT.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\TNK_SHT.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        },
        {
            // tank 2
            SoldierBodyTypes.TANK_NE,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\TNK2_ROT.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\TNK2_ROT.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\TNK2_ROT.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\TNK2_ROT.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\TNK2_ROT.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        }, {
            //ELDORADO
            SoldierBodyTypes.ELDORADO,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\HMMV.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\HMMV.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\HMMV.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\HMMV.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\HMMV.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        },
        {
            //ICECREAMTRUCK
            SoldierBodyTypes.ICECREAMTRUCK,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\HMMV.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\HMMV.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\HMMV.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\HMMV.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\HMMV.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        },
        {
            //JEEP
            SoldierBodyTypes.JEEP,
            new()
            {
                { StructData.S_STRUCT,          new("ANIMS\\STRUCTDATA\\HMMV.JSD") },
                { StructData.C_STRUCT,          new("ANIMS\\STRUCTDATA\\HMMV.JSD") },
                { StructData.P_STRUCT,          new("ANIMS\\STRUCTDATA\\HMMV.JSD") },
                { StructData.F_STRUCT,          new("ANIMS\\STRUCTDATA\\HMMV.JSD") },
                { StructData.FB_STRUCT,         new("ANIMS\\STRUCTDATA\\HMMV.JSD") },
                { StructData.DEFAULT_STRUCT,    new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") },
            }
        }
    };
}
