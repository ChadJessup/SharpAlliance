using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class SoldierAni
{
    public static bool OKFallDirection(SOLDIERTYPE pSoldier, int sGridNo, int bLevel, WorldDirections bTestDirection, AnimationStates usAnimState)
    {
//        STRUCTURE_FILE_REF* pStructureFileRef;
//        UINT16 usAnimSurface;
//
//        // How are the movement costs?
//        if (gubWorldMovementCosts[sGridNo][bTestDirection][bLevel] > TRAVELCOST_SHORE)
//        {
//            return (FALSE);
//        }
//
//        //NOT ok if in water....
//        if (GetTerrainType(sGridNo) == MED_WATER || GetTerrainType(sGridNo) == DEEP_WATER || GetTerrainType(sGridNo) == LOW_WATER)
//        {
//            return (FALSE);
//        }
//
//        // How are we for OK dest?
//        if (!NewOKDestination(pSoldier, sGridNo, true, bLevel))
//        {
//            return (FALSE);
//        }
//
//        usAnimSurface = DetermineSoldierAnimationSurface(pSoldier, usAnimState);
//        pStructureFileRef = GetAnimationStructureRef(pSoldier->ubID, usAnimSurface, usAnimState);
//
//        if (pStructureFileRef)
//        {
//            UINT16 usStructureID;
//            INT16 sTestGridNo;
//
//            // must make sure that structure data can be added in the direction of the target
//
//            usStructureID = pSoldier->ubID;
//
//            // Okay this is really SCREWY but it's due to the way this function worked before and must
//            // work now.  The function is passing in an adjacent gridno but we need to place the structure
//            // data in the tile BEFORE.  So we take one step back in the direction opposite to bTestDirection
//            // and use that gridno
//            sTestGridNo = NewGridNo(sGridNo, (UINT16)(DirectionInc(gOppositeDirection[bTestDirection])));
//
//            if (!OkayToAddStructureToWorld(sTestGridNo, bLevel, &(pStructureFileRef->pDBStructureRef[gOneCDirection[bTestDirection]]), usStructureID))
//            {
//                // can't go in that dir!
//                return (FALSE);
//            }
//        }
//
        return true;
    }
}
