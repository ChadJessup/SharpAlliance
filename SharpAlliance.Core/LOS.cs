using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class LOS
{
    public static int SoldierTo3DLocationLineOfSightTest(SOLDIERTYPE? pStartSoldier, int sGridNo, int bLevel, int bCubeLevel, int ubTileSightLimit, int bAware)
    {
        float dStartZPos, dEndZPos;
        int sXPos, sYPos;
        int ubTargetID;
        SOLDIERTYPE? pTarget;
        bool fOk;

        CHECKF(pStartSoldier);

        fOk = CalculateSoldierZPos(pStartSoldier, LOS_POS, out dStartZPos);
        CHECKF(fOk);

        if (bCubeLevel > 0)
        {
            dEndZPos = ((float)(bCubeLevel + bLevel * PROFILE_Z_SIZE) - 0.5f) * HEIGHT_UNITS_PER_INDEX;
            dEndZPos += CONVERT_PIXELS_TO_HEIGHTUNITS(gpWorldLevelData[sGridNo].sHeight);
        }
        else
        {
            ubTargetID = WhoIsThere2(sGridNo, bLevel);
            if (ubTargetID != NOBODY)
            {
                pTarget = MercPtrs[ubTargetID];
                // there's a merc there; do a soldier-to-soldier test
                return (SoldierToSoldierLineOfSightTest(pStartSoldier, pTarget, ubTileSightLimit, bAware));
            }
            // else... assume standing height
            dEndZPos = STANDING_LOS_POS + bLevel * HEIGHT_UNITS;
            // add in ground height
            dEndZPos += CONVERT_PIXELS_TO_HEIGHTUNITS(gpWorldLevelData[sGridNo].sHeight);
        }

        IsometricUtils.ConvertGridNoToXY(sGridNo, out sXPos, out sYPos);
        sXPos = sXPos * CELL_X_SIZE + (CELL_X_SIZE / 2);
        sYPos = sYPos * CELL_Y_SIZE + (CELL_Y_SIZE / 2);

        return (LineOfSightTest((FLOAT)CenterX(pStartSoldier->sGridNo), (FLOAT)CenterY(pStartSoldier->sGridNo), dStartZPos, (FLOAT)sXPos, (FLOAT)sYPos, dEndZPos, ubTileSightLimit, gubTreeSightReduction[ANIM_STAND], bAware, 0, FALSE, NULL));
    }
}
