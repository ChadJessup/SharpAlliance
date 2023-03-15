using System.Collections.Generic;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{
    public static List<ANIM_PROF> gpAnimProfiles = new();
    public static int gubNumAnimProfiles;

    public static AnimationSurfaceType[] gAnimSurfaceDatabase = new AnimationSurfaceType[(int)AnimationSurfaceTypes.NUMANIMATIONSURFACETYPES];
    public static AnimationStructureType[,] gAnimStructureDatabase = new AnimationStructureType[(int)SoldierBodyTypes.TOTALBODYTYPES, (int)StructData.NUM_STRUCT_IDS]
    {
        {
            // Normal Male
            new("ANIMS\\STRUCTDATA\\M_STAND.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),
            new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
            new("ANIMS\\STRUCTDATA\\M_FALL.JSD"),
            new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),// default
        }, {
            // Big male
            new("ANIMS\\STRUCTDATA\\M_STAND.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),
            new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
            new("ANIMS\\STRUCTDATA\\M_FALL.JSD"),
            new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),// default
        }, {
            // Stocky male
            new("ANIMS\\STRUCTDATA\\M_STAND.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),
            new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
            new("ANIMS\\STRUCTDATA\\M_FALL.JSD"),
            new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),// default
        }, {
            // Reg Female
            new("ANIMS\\STRUCTDATA\\M_STAND.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),
            new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
            new("ANIMS\\STRUCTDATA\\M_FALL.JSD"),
            new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),// default
        }, {
            // Adult female creature
            new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
            new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
            new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
            new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
            new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),// default
        }, {
            // Adult male creature
            new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
            new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
            new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
            new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
            new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),// default
        }, {
            // Young Adult female creature
            new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
            new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
            new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
            new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
            new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),// default
        }, {
            // Young Adult male creature
            new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
            new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
            new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
            new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
            new("ANIMS\\STRUCTDATA\\MN_BREAT.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),// default
        }, {
            // larvea creature
            new("ANIMS\\STRUCTDATA\\L_BREATH.JSD"),
            new("ANIMS\\STRUCTDATA\\L_BREATH.JSD"),
            new("ANIMS\\STRUCTDATA\\L_BREATH.JSD"),
            new("ANIMS\\STRUCTDATA\\L_BREATH.JSD"),
            new("ANIMS\\STRUCTDATA\\L_BREATH.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),// default
        }, {
            // infant creature
            new("ANIMS\\STRUCTDATA\\I_BREATH.JSD"),
            new("ANIMS\\STRUCTDATA\\I_BREATH.JSD"),
            new("ANIMS\\STRUCTDATA\\I_BREATH.JSD"),
            new("ANIMS\\STRUCTDATA\\I_BREATH.JSD"),
            new("ANIMS\\STRUCTDATA\\I_BREATH.JSD"),
            new("ANIMS\\STRUCTDATA\\I_BREATH.JSD"),// default
        }, {
            // Queen creature
            new("ANIMS\\STRUCTDATA\\Q_READY.JSD"),
            new("ANIMS\\STRUCTDATA\\Q_READY.JSD"),
            new("ANIMS\\STRUCTDATA\\Q_READY.JSD"),
            new("ANIMS\\STRUCTDATA\\Q_READY.JSD"),
            new("ANIMS\\STRUCTDATA\\Q_READY.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),// default
        }, {
            // Fat civ
            new("ANIMS\\STRUCTDATA\\M_STAND.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),
            new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
            new("ANIMS\\STRUCTDATA\\M_FALL.JSD"),
            new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"), // default
        }, {
            // man civ
            new("ANIMS\\STRUCTDATA\\M_STAND.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),
            new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
            new("ANIMS\\STRUCTDATA\\M_FALL.JSD"),
            new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"), // default
        }, {
            // miniskirt civ
            new("ANIMS\\STRUCTDATA\\M_STAND.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),
            new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
            new("ANIMS\\STRUCTDATA\\M_FALL.JSD"),
            new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"), // default
        }, {
            // dress civ
            new("ANIMS\\STRUCTDATA\\M_STAND.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),
            new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
            new("ANIMS\\STRUCTDATA\\M_FALL.JSD"),
            new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"), // default
        }, {
            // kid civ
            new("ANIMS\\STRUCTDATA\\K_STAND.JSD"),
            new("ANIMS\\STRUCTDATA\\K_CROUCH.JSD"),
            new("ANIMS\\STRUCTDATA\\K_CROUCH.JSD"),
            new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
            new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
            new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
        }, {
            // hat kid civ
            new("ANIMS\\STRUCTDATA\\K_STAND.JSD"),
            new("ANIMS\\STRUCTDATA\\K_CROUCH.JSD"),
            new("ANIMS\\STRUCTDATA\\K_CROUCH.JSD"),
            new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
            new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
            new("ANIMS\\STRUCTDATA\\M_PRONE.JSD"),
        }, {
            // cripple civ
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"),
            new("ANIMS\\STRUCTDATA\\M_FALL.JSD"),
            new("ANIMS\\STRUCTDATA\\M_FALLBACK.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"), // default
        }, {
            // cow
            new("ANIMS\\STRUCTDATA\\CW_BREATH.JSD"),
            new("ANIMS\\STRUCTDATA\\CW_BREATH.JSD"),
            new("ANIMS\\STRUCTDATA\\CW_BREATH.JSD"),
            new("ANIMS\\STRUCTDATA\\CW_BREATH.JSD"),
            new("ANIMS\\STRUCTDATA\\CW_BREATH.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"), // default
        }, {
            // crow
            new("ANIMS\\STRUCTDATA\\CR_STAND.JSD"),
            new("ANIMS\\STRUCTDATA\\CR_CROUCH.JSD"),
            new("ANIMS\\STRUCTDATA\\CR_PRONE.JSD"),
            new("ANIMS\\STRUCTDATA\\CR_PRONE.JSD"),
            new("ANIMS\\STRUCTDATA\\CR_PRONE.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"), // default
        }, {
            // CAT
            new("ANIMS\\STRUCTDATA\\CT_BREATH.JSD"),
            new("ANIMS\\STRUCTDATA\\CT_BREATH.JSD"),
            new("ANIMS\\STRUCTDATA\\CT_BREATH.JSD"),
            new("ANIMS\\STRUCTDATA\\CT_BREATH.JSD"),
            new("ANIMS\\STRUCTDATA\\CT_BREATH.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"), // default
        }, {
            // ROBOT1
            new("ANIMS\\STRUCTDATA\\J_R_BRET.JSD"),
            new("ANIMS\\STRUCTDATA\\J_R_BRET.JSD"),
            new("ANIMS\\STRUCTDATA\\J_R_BRET.JSD"),
            new("ANIMS\\STRUCTDATA\\J_R_BRET.JSD"),
            new("ANIMS\\STRUCTDATA\\J_R_BRET.JSD"),
            new("ANIMS\\STRUCTDATA\\J_R_BRET.JSD"), // default
        }, {
            // vech 1
            new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
            new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
            new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
            new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
            new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") // default
        }, {
            // tank 1
            new("ANIMS\\STRUCTDATA\\TNK_SHT.JSD"),
            new("ANIMS\\STRUCTDATA\\TNK_SHT.JSD"),
            new("ANIMS\\STRUCTDATA\\TNK_SHT.JSD"),
            new("ANIMS\\STRUCTDATA\\TNK_SHT.JSD"),
            new("ANIMS\\STRUCTDATA\\TNK_SHT.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") // default
        }, {
            // tank 2
            new("ANIMS\\STRUCTDATA\\TNK2_ROT.JSD"),
            new("ANIMS\\STRUCTDATA\\TNK2_ROT.JSD"),
            new("ANIMS\\STRUCTDATA\\TNK2_ROT.JSD"),
            new("ANIMS\\STRUCTDATA\\TNK2_ROT.JSD"),
            new("ANIMS\\STRUCTDATA\\TNK2_ROT.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD"), // default
        }, {
            //ELDORADO
            new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
            new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
            new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
            new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
            new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") // default
        }, {
            //ICECREAMTRUCK
            new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
            new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
            new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
            new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
            new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") // default
        }, {
            //JEEP
            new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
            new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
            new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
            new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
            new("ANIMS\\STRUCTDATA\\HMMV.JSD"),
            new("ANIMS\\STRUCTDATA\\M_CROUCH.JSD") // default
        }
    };
}
