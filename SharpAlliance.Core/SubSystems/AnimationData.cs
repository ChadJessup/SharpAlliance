using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Platform.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class AnimationData
{
    private readonly IFileManager files;
    private readonly StructureFile structure;
    private static IVideoManager video;
    private StructureInternals structureInternals;

    public AnimationData(
        IFileManager fileManager,
        StructureInternals structureInternals,
        IVideoManager videoManager,
        StructureFile structureFile)
    {
        this.structureInternals = structureInternals;
        video = videoManager;
        this.files = fileManager;
        this.structure = structureFile;
    }

    public ValueTask<bool> InitAnimationSystem()
    {
        SoldierBodyTypes cnt1;
        StructData cnt2;
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
        for (cnt1 = 0; cnt1 < SoldierBodyTypes.TOTALBODYTYPES; cnt1++)
        {
            for (cnt2 = 0; cnt2 < StructData.NUM_STRUCT_IDS; cnt2++)
            {
                sFilename = gAnimStructureDatabase[cnt1][cnt2].Filename;

                if (this.files.FileExists(sFilename))
                {
                    pStructureFileRef = this.structureInternals.LoadStructureFile(sFilename);
                    if (pStructureFileRef == null)
                    {
                        // SET_ERROR("Animation structure file load failed - %s", sFilename);
                    }

                    gAnimStructureDatabase[cnt1][cnt2].pStructureFileRef = pStructureFileRef;
                }
            }
        }
        return ValueTask.FromResult(true);
    }

    public static bool UnLoadAnimationSurface(int usSoldierID, AnimationSurfaceTypes usSurfaceIndex)
    {
        // Decrement usage flag, only if this soldier has it currently tagged
        if (gbAnimUsageHistory[usSurfaceIndex][usSoldierID] > 0)
        {
            // Decrement usage count
            Messages.AnimDebugMsg(string.Format("Surface Database: Decrementing Usage %d ( Soldier %d )", usSurfaceIndex, usSoldierID));
            gAnimSurfaceDatabase[usSurfaceIndex].bUsageCount--;
            // Set history for particular sodlier
            gbAnimUsageHistory[usSurfaceIndex][usSoldierID] = 0;

        }
        else
        {
            // Return warning that we have not actually loaded the surface previously
            Messages.AnimDebugMsg(string.Format("Surface Database: WARNING!!! Soldier has tried to unlock surface that he has not locked."));
            return false;
        }

        Messages.AnimDebugMsg(string.Format("Surface Database: MercUsage: %d, Global Uasage: %d", gbAnimUsageHistory[usSurfaceIndex][usSoldierID], gAnimSurfaceDatabase[usSurfaceIndex].bUsageCount));

        // Check for < 0
        if (gAnimSurfaceDatabase[usSurfaceIndex].bUsageCount < 0)
        {
            gAnimSurfaceDatabase[usSurfaceIndex].bUsageCount = 0;
        }


        // Check if count has reached zero and delet if so
        if (gAnimSurfaceDatabase[usSurfaceIndex].bUsageCount == 0)
        {
            Messages.AnimDebugMsg(string.Format("Surface Database: Unloading Surface: %d", usSurfaceIndex));

            CHECKF(gAnimSurfaceDatabase[usSurfaceIndex].hVideoObject != null);

            video.DeleteVideoObject(gAnimSurfaceDatabase[usSurfaceIndex].hVideoObject);
            gAnimSurfaceDatabase[usSurfaceIndex].hVideoObject = null;
        }

        return true;
    }

    private bool LoadAnimationProfiles()
    {
        return true;
    }

    private static STRUCTURE_FILE_REF? InternalGetAnimationStructureRef(int usSoldierID, AnimationSurfaceTypes usSurfaceIndex, AnimationStates usAnimState, bool fUseAbsolute)
    {
        StructData bStructDataType;

        if (usSurfaceIndex == INVALID_ANIMATION_SURFACE)
        {
            return null;
        }

        bStructDataType = gAnimSurfaceDatabase[usSurfaceIndex].bStructDataType;

        if (bStructDataType == StructData.NO_STRUCT)
        {
            return null;
        }

        // ATE: Alright - we all hate exception coding but ness here...
        // return STANDING struct for these - which start standing but end prone
        // CJC August 14 2002: added standing burst hit to this list
        if ((usAnimState == AnimationStates.FALLFORWARD_FROMHIT_STAND || usAnimState == AnimationStates.GENERIC_HIT_STAND ||
                 usAnimState == AnimationStates.FALLFORWARD_FROMHIT_CROUCH || usAnimState == AnimationStates.STANDING_BURST_HIT) && !fUseAbsolute)
        {
            return gAnimStructureDatabase[MercPtrs[usSoldierID].ubBodyType][StructData.S_STRUCT].pStructureFileRef;
        }

        return gAnimStructureDatabase[MercPtrs[usSoldierID].ubBodyType][bStructDataType].pStructureFileRef;
    }

    public static STRUCTURE_FILE_REF? GetAnimationStructureRef(int usSoldierID, AnimationSurfaceTypes usSurfaceIndex, AnimationStates usAnimState)
    {
        return InternalGetAnimationStructureRef(usSoldierID, usSurfaceIndex, usAnimState, false);
    }

    private void InitAnimationSurfacesPerBodytype()
    {
    }

    private void LoadAnimationStateInstructions()
    {
    }

    internal static void ZeroAnimSurfaceCounts()
    {
        foreach (var cnt in Enum.GetValues<AnimationSurfaceTypes>())
        {
            gAnimSurfaceDatabase[cnt].bUsageCount = 0;
            gAnimSurfaceDatabase[cnt].hVideoObject = null;
        }

        foreach(var ast in  Enum.GetValues<AnimationSurfaceTypes>())
        {
            gbAnimUsageHistory[ast] = [];
        }
    }
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
public enum AnimationSurfaceTypes : ushort
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

    INVALID_ANIMATION = 0xFFF0,
    FOUND_INVALID_ANIMATION = 0xFFF1,
    INVALID_ANIMATION_SURFACE = 32000,
}

public class AnimationStructureType
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
public class AnimationSurfaceType
{
    public int ubName;
    public string Filename;
    public StructData bStructDataType;
    public int ubFlags;
    public int uiNumDirections;
    public int uiNumFramesPerDir;
    public HVOBJECT? hVideoObject;
    public int Unused;
    public int bUsageCount;
    public NPCID bProfile;
}
