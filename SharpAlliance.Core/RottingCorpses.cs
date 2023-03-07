using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.SubSystems;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class RottingCorpses
{
    public static void DecayRottingCorpseAIWarnings()
    {
        int cnt;
        ROTTING_CORPSE? pCorpse;

        for (cnt = 0; cnt < giNumRottingCorpse; cnt++)
        {
            pCorpse = (gRottingCorpse[cnt]);

            if (pCorpse.fActivated && pCorpse.def.ubAIWarningValue > 0)
            {
                pCorpse.def.ubAIWarningValue--;
            }
        }

    }
}

public enum RottingCorpseDefines
{
    NO_CORPSE,
    SMERC_JFK,
    SMERC_BCK,
    SMERC_FWD,
    SMERC_DHD,
    SMERC_PRN,
    SMERC_WTR,
    SMERC_FALL,
    SMERC_FALLF,

    MMERC_JFK,
    MMERC_BCK,
    MMERC_FWD,
    MMERC_DHD,
    MMERC_PRN,
    MMERC_WTR,
    MMERC_FALL,
    MMERC_FALLF,

    FMERC_JFK,
    FMERC_BCK,
    FMERC_FWD,
    FMERC_DHD,
    FMERC_PRN,
    FMERC_WTR,
    FMERC_FALL,
    FMERC_FALLF,

    // CIVS
    M_DEAD1,
    K_DEAD1,
    H_DEAD1,
    FT_DEAD1,
    S_DEAD1,
    W_DEAD1,
    C_DEAD1,
    M_DEAD2,
    K_DEAD2,
    H_DEAD2,

    FT_DEAD2,
    S_DEAD2,
    W_DEAD2,
    C_DEAD2,
    BLOODCAT_DEAD,
    COW_DEAD,
    ADULTMONSTER_DEAD,
    INFANTMONSTER_DEAD,
    LARVAEMONSTER_DEAD,
    ROTTING_STAGE2,

    TANK1_DEAD,
    TANK2_DEAD,
    HUMMER_DEAD,
    ICECREAM_DEAD,
    QUEEN_MONSTER_DEAD,
    ROBOT_DEAD,
    BURNT_DEAD,
    EXPLODE_DEAD,

    NUM_CORPSES,

}

public class ROTTING_CORPSE_DEFINITION
{
    public int ubType;
    public int ubBodyType;
    public int sGridNo;
    public double dXPos;
    public double dYPos;
    public int sHeightAdjustment;

    public PaletteRepID? HeadPal;           // Palette reps
    public PaletteRepID? PantsPal;
    public PaletteRepID? VestPal;
    public PaletteRepID? SkinPal;

    public int bDirection;
    public int uiTimeOfDeath;
    public int usFlags;
    public int bLevel;
    public int bVisible;
    public int bNumServicingCrows;
    public int ubProfile;
    public bool fHeadTaken;
    public int ubAIWarningValue;
    public int[] ubFiller = new int[12];
}


public class ROTTING_CORPSE
{
    public ROTTING_CORPSE_DEFINITION def;
    public bool fActivated;
    public bool fAttractCrowsOnlyWhenOnScreen;
    public ANITILE? pAniTile;
    public SGPPaletteEntry? p8BPPPalette;
    public int p16BPPPalette;
    public int[] pShades = new int[NUM_CORPSE_SHADES];
    public int sGraphicNum;
    public int iCachedTileID;
    public double dXPos;
    public double dYPos;
    public int iID;
}
