using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.SubSystems;

public class AnimationData
{
    private readonly IFileManager files;
    private readonly StructureFile structure;

    public AnimationData(
        IFileManager fileManager,
        StructureFile structureFile)
    {
        this.files = fileManager;
        this.structure = structureFile;
    }

    public ValueTask<bool> InitAnimationSystem()
    {
        int cnt1, cnt2;
        string sFilename;
        STRUCTURE_FILE_REF? pStructureFileRef;

        this.LoadAnimationStateInstructions();

        this.InitAnimationSurfacesPerBodytype();

        if (!this.LoadAnimationProfiles())
        {
            //return (SET_ERROR("Problems initializing Animation Profiles"));
            return ValueTask.FromResult(false);
        }

        // OK, Load all animation structures.....
        for (cnt1 = 0; cnt1 < (int)SoldierBodyTypes.TOTALBODYTYPES; cnt1++)
        {
            for (cnt2 = 0; cnt2 < (int)StructData.NUM_STRUCT_IDS; cnt2++)
            {
                sFilename = this.gAnimStructureDatabase[cnt1, cnt2].Filename;

                if (this.files.FileExists(sFilename))
                {
                    pStructureFileRef = this.structure.LoadStructureFile(sFilename);
                    if (pStructureFileRef == null)
                    {
                        // SET_ERROR("Animation structure file load failed - %s", sFilename);
                    }

                    this.gAnimStructureDatabase[cnt1, cnt2].pStructureFileRef = pStructureFileRef;
                }
            }
        }

        return ValueTask.FromResult(true);
    }

    private bool LoadAnimationProfiles()
    {
        return true;
    }

    private void InitAnimationSurfacesPerBodytype()
    {
    }

    private void LoadAnimationStateInstructions()
    {
    }

    public AnimationStructureType[,] gAnimStructureDatabase = new AnimationStructureType[(int)SoldierBodyTypes.TOTALBODYTYPES, (int)StructData.NUM_STRUCT_IDS]
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

public enum SoldierBodyTypes
{
    REGMALE = 0,
    BIGMALE,
    STOCKYMALE,
    REGFEMALE,
    ADULTFEMALEMONSTER,
    AM_MONSTER,
    YAF_MONSTER,
    YAM_MONSTER,
    LARVAE_MONSTER,
    INFANT_MONSTER,
    QUEENMONSTER,
    FATCIV,
    MANCIV,
    MINICIV,
    DRESSCIV,
    HATKIDCIV,
    KIDCIV,
    CRIPPLECIV,

    COW,
    CROW,
    BLOODCAT,

    ROBOTNOWEAPON,

    HUMVEE,
    TANK_NW,
    TANK_NE,
    ELDORADO,
    ICECREAMTRUCK,
    JEEP,

    TOTALBODYTYPES
}

// DEFINES FOR ANIMATION PROFILES
[Flags]
public enum TILE_FLAG
{
    FEET = 0x0001,
    MID = 0x0002,
    HEAD = 0x0004,
    CANMOVE = 0x0008,
    NORTH_HALF = 0x0010,
    SOUTH_HALF = 0x0020,
    WEST_HALF = 0x0040,
    EAST_HALF = 0x0080,
    TOP_HALF = 0x0100,
    BOTTOM_HALF = 0x0200,
}

// Enumerations for struct data
public enum StructData
{
    S_STRUCT,
    C_STRUCT,
    P_STRUCT,
    F_STRUCT,
    FB_STRUCT,
    DEFAULT_STRUCT,
    NUM_STRUCT_IDS,
    NO_STRUCT = 120,
}

// Enumerations
// This enumeration defines the graphic image file per animation

// RGM = Regular Male
// (RG) = Body desc ( Regular - RG, Short Stocky ( SS ), etc
// (M) = Sex, Male, female
public enum AnimationSurfaceTypes
{
    RGMBASICWALKING = 0,
    RGMSTANDING,
    RGMCROUCHING,
    RGMSNEAKING,
    RGMRUNNING,
    RGMPRONE,
    RGMSTANDAIM,
    RGMHITHARD,
    RGMHITSTAND,
    RGMHITHARDBLOOD,
    RGMCROUCHAIM,
    RGMHITFALLBACK,
    RGMROLLOVER,
    RGMCLIMBROOF,
    RGMFALL,
    RGMFALLF,
    RGMHITCROUCH,
    RGMHITPRONE,
    RGMHOPFENCE,
    RGMPUNCH,
    RGMNOTHING_STD,
    RGMNOTHING_WALK,
    RGMNOTHING_RUN,
    RGMNOTHING_SWAT,
    RGMNOTHING_CROUCH,
    RGMHANDGUN_S_SHOT,
    RGMHANDGUN_C_SHOT,
    RGMHANDGUN_PRONE,
    RGMDIE_JFK,
    RGMOPEN,
    RGMPICKUP,
    RGMSTAB,
    RGMSLICE,
    RGMCSTAB,
    RGMMEDIC,
    RGMDODGE,
    RGMSTANDDWALAIM,
    RGMRAISE,
    RGMTHROW,
    RGMLOB,
    RGMKICKDOOR,
    RGMRHIT,
    RGM_SQUISH,
    RGM_LOOK,
    RGM_PULL,
    RGM_SPIT,
    RGMWATER_R_WALK,
    RGMWATER_R_STD,
    RGMWATER_N_WALK,
    RGMWATER_N_STD,
    RGMWATER_DIE,
    RGMWATER_N_AIM,
    RGMWATER_R_AIM,
    RGMWATER_DBLSHT,
    RGMWATER_TRANS,
    RGMDEEPWATER_TRED,
    RGMDEEPWATER_SWIM,
    RGMDEEPWATER_DIE,
    RGMMCLIMB,
    RGMHELIDROP,
    RGMLOWKICK,
    RGMNPUNCH,
    RGMSPINKICK,
    RGMSLEEP,
    RGMSHOOT_LOW,
    RGMCDBLSHOT,
    RGMHURTSTANDINGN,
    RGMHURTSTANDINGR,
    RGMHURTWALKINGN,
    RGMHURTWALKINGR,
    RGMHURTTRANS,
    RGMTHROWKNIFE,
    RGMBREATHKNIFE,
    RGMPISTOLBREATH,
    RGMCOWER,
    RGMROCKET,
    RGMMORTAR,
    RGMSIDESTEP,
    RGMDBLBREATH,
    RGMPUNCHLOW,
    RGMPISTOLSHOOTLOW,
    RGMWATERTHROW,
    RGMRADIO,
    RGMCRRADIO,
    RGMBURN,
    RGMDWPRONE,
    RGMDRUNK,
    RGMPISTOLDRUNK,
    RGMCROWBAR,
    RGMJUMPOVER,

    BGMWALKING,
    BGMSTANDING,
    BGMCROUCHING,
    BGMSNEAKING,
    BGMRUNNING,
    BGMPRONE,
    BGMSTANDAIM,
    BGMHITHARD,
    BGMHITSTAND,
    BGMHITHARDBLOOD,
    BGMCROUCHAIM,
    BGMHITFALLBACK,
    BGMROLLOVER,
    BGMCLIMBROOF,
    BGMFALL,
    BGMFALLF,
    BGMHITCROUCH,
    BGMHITPRONE,
    BGMHOPFENCE,
    BGMPUNCH,
    BGMNOTHING_STD,
    BGMNOTHING_WALK,
    BGMNOTHING_RUN,
    BGMNOTHING_SWAT,
    BGMNOTHING_CROUCH,
    BGMHANDGUN_S_SHOT,
    BGMHANDGUN_C_SHOT,
    BGMHANDGUN_PRONE,
    BGMDIE_JFK,
    BGMOPEN,
    BGMPICKUP,
    BGMSTAB,
    BGMSLICE,
    BGMCSTAB,
    BGMMEDIC,
    BGMDODGE,
    BGMSTANDDWALAIM,
    BGMRAISE,
    BGMTHROW,
    BGMLOB,
    BGMKICKDOOR,
    BGMRHIT,
    BGMSTANDAIM2,
    BGMFLEX,
    BGMSTRECH,
    BGMSHOEDUST,
    BGMHEADTURN,
    BGMWATER_R_WALK,
    BGMWATER_R_STD,
    BGMWATER_N_WALK,
    BGMWATER_N_STD,
    BGMWATER_DIE,
    BGMWATER_N_AIM,
    BGMWATER_R_AIM,
    BGMWATER_DBLSHT,
    BGMWATER_TRANS,
    BGMDEEPWATER_TRED,
    BGMDEEPWATER_SWIM,
    BGMDEEPWATER_DIE,
    BGMHELIDROP,
    BGMSLEEP,
    BGMSHOOT_LOW,
    BGMTHREATENSTAND,
    BGMCDBLSHOT,
    BGMHURTSTANDINGN,
    BGMHURTSTANDINGR,
    BGMHURTWALKINGN,
    BGMHURTWALKINGR,
    BGMHURTTRANS,
    BGMTHROWKNIFE,
    BGMBREATHKNIFE,
    BGMPISTOLBREATH,
    BGMCOWER,
    BGMRAISE2,
    BGMROCKET,
    BGMMORTAR,
    BGMSIDESTEP,
    BGMDBLBREATH,
    BGMPUNCHLOW,
    BGMPISTOLSHOOTLOW,
    BGMWATERTHROW,
    BGMWALK2,
    BGMRUN2,
    BGMIDLENECK,
    BGMCROUCHTRANS,
    BGMRADIO,
    BGMCRRADIO,
    BGMDWPRONE,
    BGMDRUNK,
    BGMPISTOLDRUNK,
    BGMCROWBAR,
    BGMJUMPOVER,


    RGFWALKING,
    RGFSTANDING,
    RGFCROUCHING,
    RGFSNEAKING,
    RGFRUNNING,
    RGFPRONE,
    RGFSTANDAIM,
    RGFHITHARD,
    RGFHITSTAND,
    RGFHITHARDBLOOD,
    RGFCROUCHAIM,
    RGFHITFALLBACK,
    RGFROLLOVER,
    RGFCLIMBROOF,
    RGFFALL,
    RGFFALLF,
    RGFHITCROUCH,
    RGFHITPRONE,
    RGFHOPFENCE,
    RGFPUNCH,
    RGFNOTHING_STD,
    RGFNOTHING_WALK,
    RGFNOTHING_RUN,
    RGFNOTHING_SWAT,
    RGFNOTHING_CROUCH,
    RGFHANDGUN_S_SHOT,
    RGFHANDGUN_C_SHOT,
    RGFHANDGUN_PRONE,
    RGFDIE_JFK,
    RGFOPEN,
    RGFPICKUP,
    RGFSTAB,
    RGFSLICE,
    RGFCSTAB,
    RGFMEDIC,
    RGFDODGE,
    RGFSTANDDWALAIM,
    RGFRAISE,
    RGFTHROW,
    RGFLOB,
    RGFKICKDOOR,
    RGFRHIT,
    RGFCLEAN,
    RGFKICKSN,
    RGFALOOK,
    RGFWIPE,
    RGFWATER_R_WALK,
    RGFWATER_R_STD,
    RGFWATER_N_WALK,
    RGFWATER_N_STD,
    RGFWATER_DIE,
    RGFWATER_N_AIM,
    RGFWATER_R_AIM,
    RGFWATER_DBLSHT,
    RGFWATER_TRANS,
    RGFDEEPWATER_TRED,
    RGFDEEPWATER_SWIM,
    RGFDEEPWATER_DIE,
    RGFHELIDROP,
    RGFSLEEP,
    RGFSHOOT_LOW,
    RGFCDBLSHOT,
    RGFHURTSTANDINGN,
    RGFHURTSTANDINGR,
    RGFHURTWALKINGN,
    RGFHURTWALKINGR,
    RGFHURTTRANS,
    RGFTHROWKNIFE,
    RGFBREATHKNIFE,
    RGFPISTOLBREATH,
    RGFCOWER,
    RGFROCKET,
    RGFMORTAR,
    RGFSIDESTEP,
    RGFDBLBREATH,
    RGFPUNCHLOW,
    RGFPISTOLSHOOTLOW,
    RGFWATERTHROW,
    RGFRADIO,
    RGFCRRADIO,
    RGFSLAP,
    RGFDWPRONE,
    RGFDRUNK,
    RGFPISTOLDRUNK,
    RGFCROWBAR,
    RGFJUMPOVER,

    AFMONSTERSTANDING,
    AFMONSTERWALKING,
    AFMONSTERATTACK,
    AFMONSTERCLOSEATTACK,
    AFMONSTERSPITATTACK,
    AFMONSTEREATING,
    AFMONSTERDIE,
    AFMUP,
    AFMJUMP,
    AFMMELT,

    LVBREATH,
    LVDIE,
    LVWALK,

    IBREATH,
    IWALK,
    IDIE,
    IEAT,
    IATTACK,

    QUEENMONSTERSTANDING,
    QUEENMONSTERREADY,
    QUEENMONSTERSPIT_SW,
    QUEENMONSTERSPIT_E,
    QUEENMONSTERSPIT_NE,
    QUEENMONSTERSPIT_S,
    QUEENMONSTERSPIT_SE,
    QUEENMONSTERDEATH,
    QUEENMONSTERSWIPE,

    FATMANSTANDING,
    FATMANWALKING,
    FATMANRUNNING,
    FATMANDIE,
    FATMANASS,
    FATMANACT,
    FATMANCOWER,
    FATMANDIE2,
    FATMANCOWERHIT,

    MANCIVSTANDING,
    MANCIVWALKING,
    MANCIVRUNNING,
    MANCIVDIE,
    MANCIVACT,
    MANCIVCOWER,
    MANCIVDIE2,
    MANCIVSMACKED,
    MANCIVPUNCH,
    MANCIVCOWERHIT,

    MINICIVSTANDING,
    MINICIVWALKING,
    MINICIVRUNNING,
    MINICIVDIE,
    MINISTOCKING,
    MINIACT,
    MINICOWER,
    MINIDIE2,
    MINICOWERHIT,

    DRESSCIVSTANDING,
    DRESSCIVWALKING,
    DRESSCIVRUNNING,
    DRESSCIVDIE,
    DRESSCIVACT,
    DRESSCIVCOWER,
    DRESSCIVDIE2,
    DRESSCIVCOWERHIT,

    HATKIDCIVSTANDING,
    HATKIDCIVWALKING,
    HATKIDCIVRUNNING,
    HATKIDCIVDIE,
    HATKIDCIVJFK,
    HATKIDCIVYOYO,
    HATKIDCIVACT,
    HATKIDCIVCOWER,
    HATKIDCIVDIE2,
    HATKIDCIVCOWERHIT,
    HATKIDCIVSKIP,

    KIDCIVSTANDING,
    KIDCIVWALKING,
    KIDCIVRUNNING,
    KIDCIVDIE,
    KIDCIVJFK,
    KIDCIVARMPIT,
    KIDCIVACT,
    KIDCIVCOWER,
    KIDCIVDIE2,
    KIDCIVCOWERHIT,
    KIDCIVSKIP,

    CRIPCIVSTANDING,
    CRIPCIVWALKING,
    CRIPCIVRUNNING,
    CRIPCIVBEG,
    CRIPCIVDIE,
    CRIPCIVDIE2,
    CRIPCIVKICK,

    COWSTANDING,
    COWWALKING,
    COWDIE,
    COWEAT,

    CROWWALKING,
    CROWFLYING,
    CROWEATING,
    CROWDYING,

    CATBREATH,
    CATWALK,
    CATRUN,
    CATREADY,
    CATHIT,
    CATDIE,
    CATSWIPE,
    CATBITE,

    ROBOTNWBREATH,
    ROBOTNWWALK,
    ROBOTNWHIT,
    ROBOTNWDIE,
    ROBOTNWSHOOT,

    HUMVEE_BASIC,
    HUMVEE_DIE,

    TANKNW_READY,
    TANKNW_SHOOT,
    TANKNW_DIE,

    TANKNE_READY,
    TANKNE_SHOOT,
    TANKNE_DIE,

    ELDORADO_BASIC,
    ELDORADO_DIE,

    ICECREAMTRUCK_BASIC,
    ICECREAMTRUCK_DIE,

    JEEP_BASIC,
    JEEP_DIE,

    BODYEXPLODE,

    NUMANIMATIONSURFACETYPES,
}

public struct AnimationStructureType
{
    public AnimationStructureType(string fileName)
    {
        this.Filename = fileName;
        this.pStructureFileRef = null;
    }

    public string Filename;
    public STRUCTURE_FILE_REF? pStructureFileRef;
}

// Struct for animation 'surface' information
public struct AnimationSurfaceType
{
    public int ubName;
    public string Filename;
    public int bStructDataType;
    public int ubFlags;
    public int uiNumDirections;
    public int uiNumFramesPerDir;
    public HVOBJECT hVideoObject;
    public int Unused;
    public int bUsageCount;
    public int bProfile;
}
