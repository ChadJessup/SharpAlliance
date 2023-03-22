﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core;
using SharpAlliance.Core.SubSystems;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public partial class Globals
{
    public static int[] gusPlottedPath = new int[256];
    public static int[] gusMapPathingData = new int[256];
    public static int gusPathDataSize;
    public static bool gfPlotToAvoidPlayerInfuencedSectors = false;

#define MAXTRAILTREE	(4096)
#define MAXpathQ			(512)
#define MAP_WIDTH 18
#define MAP_LENGTH MAP_WIDTH*MAP_WIDTH

    //#define EASYWATERCOST	TRAVELCOST_FLAT / 2
    //#define ISWATER(t)	(((t)==TRAVELCOST_KNEEDEEP) || ((t)==TRAVELCOST_DEEPWATER))
    //#define NOPASS (TRAVELCOST_OBSTACLE)
    //#define VEINCOST TRAVELCOST_FLAT     //actual cost for bridges and doors and such
    //#define ISVEIN(v) ((v==TRAVELCOST_VEINMID) || (v==TRAVELCOST_VEINEND))
#define TRAILCELLTYPE int

    static path_t pathQB[MAXpathQ];
    static int totAPCostB[MAXpathQ];
    static int gusPathShown, gusAPtsToMove;
    static int gusMapMovementCostsB[MAP_LENGTH][MAXDIR];
static TRAILCELLTYPE trailCostB[MAP_LENGTH];
    static trail_t trailStratTreeB[MAXTRAILTREE];
    short trailStratTreedxB = 0;

#define QHEADNDX (0)
#define QPOOLNDX (MAXpathQ-1)

#define pathQNotEmpty (pathQB[QHEADNDX].nextLink!=QHEADNDX)
#define pathFound (pathQB[ pathQB[QHEADNDX].nextLink ].location == sDestination)
#define pathNotYetFound (!pathFound)

#define REMQUENODE(ndx)							\
{	pathQB[pathQB[ndx].prevLink].nextLink = pathQB[ndx].nextLink;	\
	pathQB[pathQB[ndx].nextLink].prevLink = pathQB[ndx].prevLink;	\
}

#define INSQUENODEPREV(newNode,curNode)				\
{	pathQB[newNode].nextLink = curNode;			\
	pathQB[newNode].prevLink = pathQB[curNode].prevLink;	\
	pathQB[pathQB[curNode].prevLink].nextLink = newNode;	\
	pathQB[curNode].prevLink = newNode;			\
}

#define INSQUENODE(newNode,curNode)				\
{
    pathQB[newNode].prevLink = curNode;			\
	pathQB[newNode].NextLink = pathQB[curNode].nextLink;	\
	pathQB[pathQB[curNode].nextLink].prevLink = newNode;	\
	pathQB[curNode].nextLink = newNode;			\
}


#define DELQUENODE(ndx)                     			\
{
    REMQUENODE(ndx);                        		\
	INSQUENODEPREV(ndx, QPOOLNDX);           		\
	pathQB[ndx].location = -1;				\
}



#define NEWQUENODE                          			\
if (queRequests < QPOOLNDX)                       \
		qNewNdx = queRequests++;			\

    else                            \
	{                                       		\
		qNewNdx = pathQB[QPOOLNDX].nextLink;		\
		REMQUENODE(qNewNdx);				\
	}

#define ESTIMATE0	((dx>dy) ?       (dx)      :       (dy))
#define ESTIMATE1	((dx<dy) ? ((dx*14)/10+dy) : ((dy*14)/10+dx) )
#define ESTIMATE2	FLATCOST*( (dx<dy) ? ((dx*14)/10+dy) : ((dy*14)/10+dx) )
#define ESTIMATEn	((int)(FLATCOST*sqrt(dx*dx+dy*dy)))
#define ESTIMATE ESTIMATE1


#define REMAININGCOST(ndx)					\
(                               \
	(locY = pathQB[ndx].location / MAP_WIDTH),			\
	(locX = pathQB[ndx].location % MAP_WIDTH),			\
	(dy = abs(iDestY - locY)),					\
	(dx = abs(iDestX - locX)),					\
	ESTIMATE						\
)


#define MAXCOST (99900)
#define TOTALCOST(ndx) (pathQB[ndx].costSoFar + pathQB[ndx].costToGo)
#define XLOC(a) (a%MAP_WIDTH)
#define YLOC(a) (a/MAP_WIDTH)
#define LEGDISTANCE(a,b) ( abs( XLOC(b)-XLOC(a) ) + abs( YLOC(b)-YLOC(a) ) )
#define FARTHER(ndx,NDX) ( LEGDISTANCE(pathQB[ndx].location,sDestination) > LEGDISTANCE(pathQB[NDX].location,sDestination) )

#define FLAT_STRATEGIC_TRAVEL_TIME		60

#define QUESEARCH(ndx,NDX)						\
{									\
	int k = TOTALCOST(ndx);					\
	NDX = pathQB[QHEADNDX].nextLink;					\
	while (NDX && (k > TOTALCOST(NDX)))              \
		NDX = pathQB[NDX].nextLink;				\
	while (NDX && (k == TOTALCOST(NDX)) && FARTHER(ndx, NDX))    \
		NDX = pathQB[NDX].nextLink;				\
}
}

struct path_s
{
    int nextLink;           //2
    int prevLink;           //2
    int location;           //2
    int costSoFar;          //4
    int costToGo;           //4
    int pathNdx;            //2
};

struct trail_s
{
    short nextLink;
    short diStratDelta;
};

public class StrategicPathing
{
    INSERTION_CODE[] ubFromMapDirToInsertionCode =
    {
        INSERTION_CODE.SOUTH,			//NORTH_STRATEGIC_MOVE
    	INSERTION_CODE.WEST,			//EAST_STRATEGIC_MOVE
    	INSERTION_CODE.NORTH,			//SOUTH_STRATEGIC_MOVE
    	INSERTION_CODE.EAST				//WEST_STRATEGIC_MOVE
    };




    int queRequests;


    int diStratDelta[8] =
    {
    -MAP_WIDTH,        //N
	1-MAP_WIDTH,       //NE
	1,                //E
	1+MAP_WIDTH,       //SE
	MAP_WIDTH,         //S
	MAP_WIDTH-1,       //SW
	-1,               //W
	-MAP_WIDTH-1       //NW
};


    extern int GetTraversability(int sStartSector, int sEndSector);



    // this will find if a shortest strategic path

    static bool fPreviousPlotDirectPath = false;     // don't save
    int FindStratPath(int sStart, int sDestination, int sMvtGroupNumber, bool fTacticalTraversal)
    {
        int iCnt, ndx, insertNdx, qNewNdx;
        int iDestX, iDestY, locX, locY, dx, dy;
        int sSectorX, sSectorY;
        int newLoc, curLoc;
        TRAILCELLTYPE curCost, newTotCost, nextCost;
        int sOrigination;
        int iCounter = 0;
        bool fPlotDirectPath = false;
        GROUP* pGroup;

        // ******** Fudge by Bret (for now), curAPcost is never initialized in this function, but should be!
        // so this is just to keep things happy!

        // for player groups only!
        pGroup = GetGroup((int)sMvtGroupNumber);
        if (pGroup.fPlayer)
        {
            // if player is holding down SHIFT key, find the shortest route instead of the quickest route!
            if (_KeyDown(SHIFT))
            {
                fPlotDirectPath = true;
            }


            if (fPlotDirectPath != fPreviousPlotDirectPath)
            {
                // must redraw map to erase the previous path...
                fMapPanelDirty = true;
                fPreviousPlotDirectPath = fPlotDirectPath;
            }
        }


        queRequests = 2;

        //initialize the ai data structures
        memset(trailStratTreeB, 0, sizeof(trailStratTreeB));
        memset(trailCostB, 255, sizeof(trailCostB));

        //memset(trailCostB,255*PATHFACTOR,MAP_LENGTH);
        memset(pathQB, 0, sizeof(pathQB));

        // FOLLOWING LINE COMMENTED OUT ON MARCH 7/97 BY IC
        memset(gusMapPathingData, ((int)sStart), sizeof(gusMapPathingData));
        trailStratTreedxB = 0;

        //set up common info
        sOrigination = sStart;


        iDestY = (sDestination / MAP_WIDTH);
        iDestX = (sDestination % MAP_WIDTH);


        // if origin and dest is water, then user wants to stay in water!
        // so, check and set waterToWater flag accordingly



        //setup Q
        pathQB[QHEADNDX].location = sOrigination;
        pathQB[QHEADNDX].nextLink = 1;
        pathQB[QHEADNDX].prevLink = 1;
        pathQB[QHEADNDX].costSoFar = MAXCOST;

        pathQB[QPOOLNDX].nextLink = QPOOLNDX;
        pathQB[QPOOLNDX].prevLink = QPOOLNDX;

        //setup first path record
        pathQB[1].nextLink = QHEADNDX;
        pathQB[1].prevLink = QHEADNDX;
        pathQB[1].location = sOrigination;
        pathQB[1].pathNdx = 0;
        pathQB[1].costSoFar = 0;
        pathQB[1].costToGo = REMAININGCOST(1);


        trailStratTreedxB = 0;
        trailCostB[sOrigination] = 0;
        ndx = pathQB[QHEADNDX].nextLink;
        pathQB[ndx].pathNdx = trailStratTreedxB;
        trailStratTreedxB++;


        do
        {
            //remove the first and best path so far from the que
            ndx = pathQB[QHEADNDX].nextLink;
            curLoc = pathQB[ndx].location;
            curCost = pathQB[ndx].costSoFar;
            // = totAPCostB[ndx];
            DELQUENODE((int)ndx);

            if (trailCostB[curLoc] < curCost)
                continue;


            //contemplate a new path in each direction
            for (iCnt = 0; iCnt < 8; iCnt += 2)
            {
                newLoc = curLoc + diStratDelta[iCnt];


                // are we going off the map?
                if ((newLoc % MAP_WORLD_X == 0) || (newLoc % MAP_WORLD_X == MAP_WORLD_X - 1) || (newLoc / MAP_WORLD_X == 0) || (newLoc / MAP_WORLD_X == MAP_WORLD_X - 1))
                {
                    // yeppers
                    continue;
                }

                if (gfPlotToAvoidPlayerInfuencedSectors && newLoc != sDestination)
                {
                    sSectorX = (int)(newLoc % MAP_WORLD_X);
                    sSectorY = (int)(newLoc / MAP_WORLD_X);

                    if (IsThereASoldierInThisSector(sSectorX, sSectorY, 0))
                    {
                        continue;
                    }
                    if (GetNumberOfMilitiaInSector(sSectorX, sSectorY, 0))
                    {
                        continue;
                    }
                    if (!OkayForEnemyToMoveThroughSector((int)SECTOR(sSectorX, sSectorY)))
                    {
                        continue;
                    }
                }

                // are we plotting path or checking for existance of one?
                if (sMvtGroupNumber != 0)
                {
                    if (iHelicopterVehicleId != -1)
                    {
                        nextCost = GetTravelTimeForGroup((int)(SECTOR((curLoc % MAP_WORLD_X), (curLoc / MAP_WORLD_X))), (int)(iCnt / 2), (int)sMvtGroupNumber);
                        if (nextCost != 0xffffffff && sMvtGroupNumber == pVehicleList[iHelicopterVehicleId].ubMovementGroup)
                        {
                            // is a heli, its pathing is determined not by time (it's always the same) but by total cost
                            // Skyrider will avoid uncontrolled airspace as much as possible...
                            if (StrategicMap[curLoc].fEnemyAirControlled == true)
                            {
                                nextCost = COST_AIRSPACE_UNSAFE;
                            }
                            else
                            {
                                nextCost = COST_AIRSPACE_SAFE;
                            }
                        }
                    }
                    else
                    {
                        nextCost = GetTravelTimeForGroup((int)(SECTOR((curLoc % MAP_WORLD_X), (curLoc / MAP_WORLD_X))), (int)(iCnt / 2), (int)sMvtGroupNumber);
                    }
                }
                else
                {
                    nextCost = GetTravelTimeForFootTeam((int)(SECTOR(curLoc % MAP_WORLD_X, curLoc / MAP_WORLD_X)), (int)(iCnt / 2));
                }

                if (nextCost == 0xffffffff)
                {
                    continue;
                }


                // if we're building this path due to a tactical traversal exit, we have to force the path to the next sector be
                // in the same direction as the traversal, even if it's not the shortest route, otherwise pathing can crash!  This
                // can happen in places where the long way around to next sector is actually shorter: e.g. D5 to D6.  ARM
                if (fTacticalTraversal)
                {
                    // if it's the first sector only (no cost yet)
                    if (curCost == 0 && (newLoc == sDestination))
                    {
                        if (GetTraversability((int)(SECTOR(curLoc % 18, curLoc / 18)), (int)(SECTOR(newLoc % 18, newLoc / 18))) != GROUNDBARRIER)
                        {
                            nextCost = 0;
                        }
                    }
                }
                else
                {
                    if (fPlotDirectPath)
                    {
                        // use shortest route instead of faster route
                        nextCost = FLAT_STRATEGIC_TRAVEL_TIME;
                    }
                }

                /*
                // Commented out by CJC Feb 4 1999... causing errors!

                //make the destination look very attractive
                if( ( newLoc == sDestination ) )
                {
                    if( GetTraversability( ( int )( SECTOR( curLoc % 18, curLoc / 18 ) ), ( int ) ( SECTOR( newLoc %18,  newLoc / 18 ) ) ) != GROUNDBARRIER )
                    {
                        nextCost = 0;
                    }
                }
                */
                //if (_KeyDown(CTRL_DOWN) && nextCost < TRAVELCOST_VEINEND)
                newTotCost = curCost + nextCost;
                if (newTotCost < trailCostB[newLoc])
                {
                    NEWQUENODE;

                    if (qNewNdx == QHEADNDX)
                    {
                        return (0);
                    }


                    if (qNewNdx == QPOOLNDX)
                    {
                        return (0);
                    }

                    //make new path to current location
                    trailStratTreeB[trailStratTreedxB].nextLink = pathQB[ndx].pathNdx;
                    trailStratTreeB[trailStratTreedxB].diStratDelta = (int)iCnt;
                    pathQB[qNewNdx].pathNdx = trailStratTreedxB;
                    trailStratTreedxB++;


                    if (trailStratTreedxB >= MAXTRAILTREE)
                    {
                        return (0);
                    }

                    pathQB[qNewNdx].location = (int)newLoc;
                    pathQB[qNewNdx].costSoFar = newTotCost;
                    pathQB[qNewNdx].costToGo = REMAININGCOST(qNewNdx);
                    trailCostB[newLoc] = newTotCost;
                    //do a sorted que insert of the new path
                    QUESEARCH(qNewNdx, insertNdx);
                    INSQUENODEPREV((int)qNewNdx, (int)insertNdx);
                }
            }
        }
        while (pathQNotEmpty && pathNotYetFound);
        // work finished. Did we find a path?
        if (pathFound)
        {
            int z, _z, _nextLink; //,tempgrid;

            _z = 0;
            z = pathQB[pathQB[QHEADNDX].nextLink].pathNdx;

            while (z)
            {
                _nextLink = trailStratTreeB[z].nextLink;
                trailStratTreeB[z].nextLink = _z;
                _z = z;
                z = _nextLink;
            }

            // if this function was called because a solider is about to embark on an actual route
            // (as opposed to "test" path finding (used by cursor, etc), then grab all pertinent
            // data and copy into soldier's database


            z = _z;

            for (iCnt = 0; z && (iCnt < MAX_PATH_LIST_SIZE); iCnt++)
            {
                gusMapPathingData[iCnt] = trailStratTreeB[z].diStratDelta;

                z = trailStratTreeB[z].nextLink;
            }

            gusPathDataSize = (int)iCnt;


            // return path length : serves as a "successful" flag and a path length counter
            return (iCnt);
        }
        // failed miserably, report...
        return (0);
    }


    Path BuildAStrategicPath(Path pPath, int iStartSectorNum, int iEndSectorNum, int sMvtGroupNumber, bool fTacticalTraversal /*, bool fTempPath */ )
    {

        int iCurrentSectorNum;
        int iDelta = 0;
        int iPathLength;
        int iCount = 0;
        Path pNode = null;
        Path pNewNode = null;
        Path pDeleteNode = null;
        bool fFlag = false;
        Path pHeadOfPathList = pPath;
        int iOldDelta = 0;
        iCurrentSectorNum = iStartSectorNum;


        if (pNode == null)
        {
            // start new path list
            pNode = MemAlloc(sizeof(PathSt));
            /*
               if ( _KeyDown( CTRL ))
                     pNode.fSpeed=SLOW_MVT;
                 else
            */
            pNode.fSpeed = NORMAL_MVT;
            pNode.uiSectorId = iStartSectorNum;
            pNode.pNext = null;
            pNode.pPrev = null;
            pNode.uiEta = GetWorldTotalMin();
            pHeadOfPathList = pNode;
        }

        if (iEndSectorNum < MAP_WORLD_X - 1)
            return null;

        iPathLength = ((int)FindStratPath(((int)iStartSectorNum), ((int)iEndSectorNum), sMvtGroupNumber, fTacticalTraversal));
        while (iPathLength > iCount)
        {
            switch (gusMapPathingData[iCount])
            {
                case (NORTH):
                    iDelta = NORTH_MOVE;
                    break;
                case (SOUTH):
                    iDelta = SOUTH_MOVE;
                    break;
                case (EAST):
                    iDelta = EAST_MOVE;
                    break;
                case (WEST):
                    iDelta = WEST_MOVE;
                    break;
            }
            iCount++;
            // create new node
            iCurrentSectorNum += iDelta;

            if (!AddSectorToPathList(pHeadOfPathList, (int)iCurrentSectorNum))
            {
                pNode = pHeadOfPathList;
                // intersected previous node, delete path to date
                if (!pNode)
                    return null;
                while (pNode.pNext)
                    pNode = pNode.pNext;
                // start backing up 
                while (pNode.uiSectorId != (int)iStartSectorNum)
                {
                    pDeleteNode = pNode;
                    pNode = pNode.pPrev;
                    pNode.pNext = null;
                    MemFree(pDeleteNode);
                }
                return null;
            }


            // for strategic mvt events
            // we are at the new node, check if previous node was a change in deirection, ie change in delta..add waypoint
            // if -1, do not
            /*
            if( iOldDelta != 0 )
            {
                if( iOldDelta != iDelta )
                {
                    // ok add last waypt
                    if( fTempPath == false )
                    {
                        // change in direction..add waypoint
                        AddWaypointToGroup( ( int )sMvtGroupNumber, ( int )( ( iCurrentSectorNum - iDelta ) % MAP_WORLD_X ), ( int )( ( iCurrentSectorNum - iDelta ) / MAP_WORLD_X ) );
                    }
                }
            }
            */
            iOldDelta = iDelta;


            pHeadOfPathList = pNode;
            if (!pNode)
                return null;
            while (pNode.pNext)
                pNode = pNode.pNext;

        }

        pNode = pHeadOfPathList;

        if (!pNode)
            return null;
        while (pNode.pNext)
            pNode = pNode.pNext;

        if (!pNode.pPrev)
        {
            MemFree(pNode);
            pHeadOfPathList = null;
            pPath = pHeadOfPathList;
            return false;
        }

        /*
        // ok add last waypt
        if( fTempPath == false )
        {
            // change in direction..add waypoint
            AddWaypointToGroup( ( int )sMvtGroupNumber, ( int )( iCurrentSectorNum% MAP_WORLD_X ), ( int )( iCurrentSectorNum / MAP_WORLD_X ) );
      }
        */

        pPath = pHeadOfPathList;
        return pPath;

    }





    bool AddSectorToPathList(Path pPath, int uiSectorNum)
    {
        Path pNode = null;
        Path pTempNode = null;
        Path pHeadOfList = pPath;
        pNode = pPath;

        if (uiSectorNum < MAP_WORLD_X - 1)
            return false;

        if (pNode == null)
        {
            pNode = MemAlloc(sizeof(PathSt));

            // Implement EtaCost Array as base EtaCosts of sectors
            // pNode.uiEtaCost=EtaCost[uiSectorNum];
            pNode.uiSectorId = uiSectorNum;
            pNode.uiEta = GetWorldTotalMin();
            pNode.pNext = null;
            pNode.pPrev = null;
            /*
                 if ( _KeyDown( CTRL ))
                       pNode.fSpeed=SLOW_MVT;
                   else
            */
            pNode.fSpeed = NORMAL_MVT;


            return true;
        }
        else
        {
            //if (pNode.uiSectorId==uiSectorNum)
            //	  return false;
            while (pNode.pNext)
            {
                //  if (pNode.uiSectorId==uiSectorNum)
                //	  return false;
                pNode = pNode.pNext;

            }

            pTempNode = MemAlloc(sizeof(PathSt));
            pTempNode.uiEta = 0;
            pNode.pNext = pTempNode;
            pTempNode.uiSectorId = uiSectorNum;
            pTempNode.pPrev = pNode;
            pTempNode.pNext = null;
            /*
                  if ( _KeyDown( CTRL ))
                   pTempNode.fSpeed=SLOW_MVT;
                  else 
            */
            pTempNode.fSpeed = NORMAL_MVT;
            pNode = pTempNode;

        }
        pPath = pHeadOfList;
        return true;
    }



    /*
    bool TravelBetweenSectorsIsBlockedFromVehicle( int sSourceSector, int sDestSector )
    {
        int sDelta;

        sDelta = sDestSector - sSourceSector;

        switch( sDelta )
        {
            case( 0 ):
                return( true );
            break;
            case( - MAP_WORLD_Y ):
              return( StrategicMap[ sSourceSector ].uiBadVehicleSector[ 0 ] );
            break;
            case( MAP_WORLD_Y):
            return( StrategicMap[ sSourceSector ].uiBadVehicleSector[ 2 ] );
        break; 
            case( 1 ):
                return ( StrategicMap[ sSourceSector ].uiBadVehicleSector[ 1 ] );
            break;
            case( -1 ):
                return ( StrategicMap[ sSourceSector ].uiBadVehicleSector[ 3 ] );
            break;
        }

        return( false );
    }



    bool SectorIsBlockedFromVehicleExit( int sSourceSector, int bToDirection  )
    {

        if( StrategicMap[ sSourceSector ].uiBadVehicleSector[ bToDirection ] )
        {
            return ( true );
        }
        else
        {
            return ( false );
        }

    }



    bool TravelBetweenSectorsIsBlockedFromFoot( int sSourceSector, int sDestSector )
    {
        int sDelta;

        sDelta = sDestSector - sSourceSector;

        switch( sDelta )
        {
            case( 0 ):
                return( true );
            break;
            case( - MAP_WORLD_Y ):
              return( StrategicMap[ sSourceSector ].uiBadFootSector[ 0 ] );
            break;
            case( MAP_WORLD_Y):
            return( StrategicMap[ sSourceSector ].uiBadFootSector[ 2 ] );
        break; 
            case( 1 ):
                return ( StrategicMap[ sSourceSector ].uiBadFootSector[ 1 ] );
            break;
            case( -1 ):
                return ( StrategicMap[ sSourceSector ].uiBadFootSector[ 3 ] );
            break;
        }

        return( false );
    }


    bool SectorIsBlockedFromFootExit( int sSourceSector, int bToDirection )
    {
        if( StrategicMap[ sSourceSector ].uiBadFootSector[ bToDirection ]  )
        {
            return ( true );
        }
        else
        {
            return ( false );
        }

    }



    bool CanThisMercMoveToThisSector( SOLDIERTYPE *pSoldier ,int sX, int sY )
    {
        // this fucntion will return if this merc ( pSoldier ), can move to sector sX, sY
      bool fOkToMoveFlag = false;


        return fOkToMoveFlag;
    }



    void SetThisMercsSectorXYToTheseValues( SOLDIERTYPE *pSoldier ,int sX, int sY, int ubFromDirection )
    {
      // will move a merc ( pSoldier )to a sector sX, sY

        // Ok, update soldier control pointer values
        pSoldier.sSectorX = sX;
        pSoldier.sSectorY = sY;

        // Set insertion code....
        pSoldier.ubStrategicInsertionCode = ubFromMapDirToInsertionCode[ ubFromDirection ];

        // Are we the same as our current sector
        if ( gWorldSectorX == sX && gWorldSectorY == sY && !gbWorldSectorZ )
        {
            // Add this poor bastard!
            UpdateMercInSector( pSoldier, sX, sY, 0 );
        }
        // Were we in sector?
        else if ( pSoldier.bInSector )
        {
            RemoveSoldierFromTacticalSector( pSoldier, true );

            // Remove from tactical team UI
            RemovePlayerFromTeamSlotGivenMercID( pSoldier.ubID );

        }

        return;
    }
    */



    Path AppendStrategicPath(Path pNewSection, Path pHeadOfPathList)
    {
        // will append a new section onto the end of the head of list, then return the head of the new list

        Path pNode = pHeadOfPathList;
        Path pPastNode = null;
        // move to end of original section

        if (pNewSection == null)
        {
            return pHeadOfPathList;
        }


        // is there in fact a list to append to
        if (pNode)
        {
            // move to tail of old list
            while (pNode.pNext)
            {
                // next node in list
                pNode = pNode.pNext;
            }

            // make sure the 2 are not the same

            if (pNode.uiSectorId == pNewSection.uiSectorId)
            {
                // are the same, remove head of new list
                pNewSection = RemoveHeadFromStrategicPath(pNewSection);
            }

            // append onto old list
            pNode.pNext = pNewSection;
            pNewSection.pPrev = pNode;

        }
        else
        {
            // head of list becomes head of new section
            pHeadOfPathList = pNewSection;
        }

        // return head of new list
        return (pHeadOfPathList);
    }

    Path ClearStrategicPathList(Path pHeadOfPath, int sMvtGroup)
    {
        // will clear out a strategic path and return head of list as null
        Path pNode = pHeadOfPath;
        Path pDeleteNode = pHeadOfPath;

        // is there in fact a path?
        if (pNode == null)
        {
            // no path, leave
            return (pNode);
        }

        // clear list
        while (pNode.pNext)
        {
            // set up delete node
            pDeleteNode = pNode;

            // move to next node
            pNode = pNode.pNext;

            // delete delete node
            MemFree(pDeleteNode);
        }

        // clear out last node
        MemFree(pNode);

        pNode = null;
        pDeleteNode = null;

        if ((sMvtGroup != -1) && (sMvtGroup != 0))
        {
            // clear this groups mvt pathing
            RemoveGroupWaypoints((int)sMvtGroup);
        }

        return (pNode);
    }


    Path ClearStrategicPathListAfterThisSector(Path pHeadOfPath, int sX, int sY, int sMvtGroup)
    {
        // will clear out a strategic path and return head of list as null
        Path pNode = pHeadOfPath;
        Path pDeleteNode = pHeadOfPath;
        int sSector = 0;
        int sCurrentSector = -1;



        // is there in fact a path?
        if (pNode == null)
        {
            // no path, leave
            return (pNode);
        }


        // get sector value
        sSector = sX + (sY * MAP_WORLD_X);

        // go to end of list
        pNode = MoveToEndOfPathList(pNode);

        // get current sector value
        sCurrentSector = (int)pNode.uiSectorId;

        // move through list
        while ((pNode) && (sSector != sCurrentSector))
        {

            // next value
            pNode = pNode.pPrev;

            // get current sector value
            if (pNode != null)
            {
                sCurrentSector = (int)pNode.uiSectorId;
            }
        }

        // did we find the target sector?
        if (pNode == null)
        {
            // nope, leave
            return (pHeadOfPath);
        }


        // we want to KEEP the target sector, not delete it, so advance to the next sector
        pNode = pNode.pNext;

        // is nothing left?
        if (pNode == null)
        {
            // that's it, leave
            return (pHeadOfPath);
        }


        // if we're NOT about to clear the head (there's a previous entry)
        if (pNode.pPrev)
        {
            // set next for tail to null
            pNode.pPrev.pNext = null;
        }
        else
        {
            // clear head, return null
            pHeadOfPath = ClearStrategicPathList(pHeadOfPath, sMvtGroup);

            return (null);
        }

        // clear list
        while (pNode.pNext)
        {
            // set up delete node
            pDeleteNode = pNode;

            // move to next node
            pNode = pNode.pNext;

            // check if we are clearing the head of the list
            if (pDeleteNode == pHeadOfPath)
            {
                // null out head
                pHeadOfPath = null;
            }

            // delete delete node
            MemFree(pDeleteNode);
        }


        // clear out last node
        MemFree(pNode);
        pNode = null;
        pDeleteNode = null;

        return (pHeadOfPath);
    }

    Path MoveToBeginningOfPathList(Path pList)
    {

        // move to beginning of this list

        // no list, return
        if (pList == null)
        {
            return (null);
        }

        // move to beginning of list
        while (pList.pPrev)
        {
            pList = pList.pPrev;
        }

        return (pList);

    }

    Path MoveToEndOfPathList(Path pList)
    {
        // move to end of list

        // no list, return
        if (pList == null)
        {
            return (null);
        }

        // move to beginning of list
        while (pList.pNext)
        {
            pList = pList.pNext;
        }

        return (pList);

    }


    Path RemoveTailFromStrategicPath(Path pHeadOfList)
    {
        // remove the tail section from the strategic path
        Path pNode = pHeadOfList;
        Path pLastNode = pHeadOfList;

        if (pNode == null)
        {
            // no list, leave
            return (null);
        }

        while (pNode.pNext)
        {
            pLastNode = pNode;
            pNode = pNode.pNext;
        }

        // end of list

        // set next to null
        pLastNode.pNext = null;

        // now remove old last node
        MemFree(pNode);

        // return head of new list
        return (pHeadOfList);

    }


    Path RemoveHeadFromStrategicPath(Path pList)
    {
        // move to head of list
        Path pNode = pList;
        Path pNewHead = pList;

        // check if there is a list
        if (pNode == null)
        {
            // no list, leave
            return (null);
        }

        // move to head of list
        while (pNode.pPrev)
        {
            // back one node
            pNode = pNode.pPrev;
        }

        // set up new head
        pNewHead = pNode.pNext;
        if (pNewHead)
        {
            pNewHead.pPrev = null;
        }

        // free old head
        MemFree(pNode);

        pNode = null;

        // return new head
        return (pNewHead);

    }


    Path RemoveSectorFromStrategicPathList(Path pList, int sX, int sY)
    {
        // find sector sX, sY ...then remove it
        int sSector = 0;
        int sCurrentSector = -1;
        Path pNode = pList;
        Path pPastNode = pList;

        // get sector value
        sSector = sX + (sY * MAP_WORLD_X);

        // check if there is a valid list 
        if (pNode == null)
        {
            return (pNode);
        }

        // get current sector value
        sCurrentSector = (int)pNode.uiSectorId;

        // move to end of list
        pNode = MoveToEndOfPathList(pNode);

        // move through list
        while ((pNode) && (sSector != sCurrentSector))
        {
            // set past node up
            pPastNode = pNode;

            // next value
            pNode = pNode.pPrev;

            // get current sector value
            sCurrentSector = (int)pNode.uiSectorId;
        }

        // no list left, sector not found
        if (pNode == null)
        {
            return (null);
        }

        // sector found...remove it
        pPastNode.pNext = pNode.pNext;

        // remove node
        MemFree(pNode);

        // set up prev for next
        pPastNode.pNext.pPrev = pPastNode;


        pPastNode = MoveToBeginningOfPathList(pPastNode);

        return (pPastNode);
    }

    int GetLastSectorIdInCharactersPath(SOLDIERTYPE? pCharacter)
    {
        // will return the last sector of the current path, or the current sector if there's no path
        int sLastSector = (pCharacter.sSectorX) + (pCharacter.sSectorY) * (MAP_WORLD_X);
        Path pNode = null;

        pNode = GetSoldierMercPathPtr(pCharacter);

        while (pNode)
        {
            sLastSector = (int)(pNode.uiSectorId);
            pNode = pNode.pNext;
        }

        return sLastSector;
    }

    // get id of last sector in vehicle path list
    int GetLastSectorIdInVehiclePath(int iId)
    {
        int sLastSector = -1;
        Path pNode = null;

        if ((iId >= ubNumberOfVehicles) || (iId < 0))
        {
            return (sLastSector);
        }
        // now check if vehicle is valid
        if (pVehicleList[iId].fValid == false)
        {
            return (sLastSector);
        }

        // get current last sector
        sLastSector = (pVehicleList[iId].sSectorX) + (pVehicleList[iId].sSectorY * MAP_WORLD_X);

        pNode = pVehicleList[iId].pMercPath;

        while (pNode)
        {
            sLastSector = (int)(pNode.uiSectorId);
            pNode = pNode.pNext;
        }

        return sLastSector;


    }



    Path CopyPaths(Path pSourcePath, Path pDestPath)
    {
        Path pDestNode = pDestPath;
        Path pCurNode = pSourcePath;
        // copies path from source to dest


        // null out dest path
        pDestNode = ClearStrategicPathList(pDestNode, -1);
        Debug.Assert(pDestNode == null);


        // start list off
        if (pCurNode != null)
        {
            pDestNode = MemAlloc(sizeof(PathSt));

            // set next and prev nodes
            pDestNode.pPrev = null;
            pDestNode.pNext = null;

            // copy sector value and times
            pDestNode.uiSectorId = pCurNode.uiSectorId;
            pDestNode.uiEta = pCurNode.uiEta;
            pDestNode.fSpeed = pCurNode.fSpeed;

            pCurNode = pCurNode.pNext;
        }

        while (pCurNode != null)
        {

            pDestNode.pNext = MemAlloc(sizeof(PathSt));

            // set next's previous to current
            pDestNode.pNext.pPrev = pDestNode;

            // set next's next to null
            pDestNode.pNext.pNext = null;

            // increment ptr
            pDestNode = pDestNode.pNext;

            // copy sector value and times
            pDestNode.uiSectorId = pCurNode.uiSectorId;
            pDestNode.uiEta = pCurNode.uiEta;
            pDestNode.fSpeed = pCurNode.fSpeed;

            pCurNode = pCurNode.pNext;
        }


        // move back to beginning fo list
        pDestNode = MoveToBeginningOfPathList(pDestNode);

        // return to head of path
        return (pDestNode);
    }

    int GetStrategicMvtSpeed(SOLDIERTYPE? pCharacter)
    {
        // will return the strategic speed of the character
        int iSpeed;

        // avg of strength and agility * percentage health..very simple..replace later

        iSpeed = (int)((pCharacter.bAgility + pCharacter.bStrength) / 2);
        iSpeed *= (int)((pCharacter.bLife));
        iSpeed /= (int)pCharacter.bLifeMax;

        return (iSpeed);
    }

    /*
    void CalculateEtaForCharacterPath( SOLDIERTYPE *pCharacter )
    {
        Path pNode = null;
        int uiDeltaEta =0;
        int iMveDelta = 0;
        bool fInVehicle;

        // valid character
        if( pCharacter == null )
        {
            return;
        }

        // the rules change a little for people in vehicles
        if( pCharacter . bAssignment == VEHICLE )
        {
            fInVehicle = true;
        }

        if( ( pCharacter . pMercPath == null ) && ( fInVehicle == false ) )
        {
            return;
        }

        if( ( fInVehicle == true ) && ( VehicleIdIsValid( pCharacter . iVehicleId ) ) )
        {
            // valid vehicle, is there a path for it?
            if( pVehicleList[ iId ].pMercPath == null )
            {
                // nope
                return;
            }
        }


        // go through path list, calculating eta's based on previous sector eta, speed of mvt through sector, and eta cost of sector
        pNode = GetSoldierMercPathPtr( pCharacter );

        // while there are nodes, calculate eta
        while( pNode )
        {
            // first node, set eta to current time
            if( pNode . pPrev == null )
            {
                pNode . uiEta = GetWorldTotalMin( );
            }
            else
            {
                // get delta in sectors
                switch( pNode . uiSectorId - pNode . pPrev . uiSectorId )
                {
                case( NORTH_MOVE ):
                    iMveDelta = 0;
                    break;
                case( SOUTH_MOVE ):
                    iMveDelta = 2;
                    break;
                case( EAST_MOVE ):
                    iMveDelta = 1;
                    break;
                case( WEST_MOVE ):
                    iMveDelta = 3;
                    break;
                }

                if( fInVehicle == true )
                {
                    // which type

                }
                else
                {
                    // get delta..is the  sector ( mvt cost * modifier ) / ( character strategic speed * mvt speed )
                    uiDeltaEta = ( ( StrategicMap[ pNode . uiSectorId ].uiFootEta[ iMveDelta ] * FOOT_MVT_MODIFIER ) / ( GetStrategicMvtSpeed( pCharacter ) * ( pNode . fSpeed + 1 ) ) );
                }


                // next sector eta
                pNode . uiEta = pNode . pPrev . uiEta + ( uiDeltaEta );
            }
            pNode = pNode . pNext;
        }
        return;
    }
    */

    /*
    void MoveCharacterOnPath( SOLDIERTYPE *pCharacter )
    {
        // will move a character along a merc path
        Path pNode = null;
        Path pDeleteNode = null;


        // error check
        if( pCharacter == null )
        {
            return;
        }

        if( pCharacter . pMercPath == null )
        {
            return;
        }

        if( pCharacter . pMercPath . pNext == null )
        {
            // simply set eta to current time
            pCharacter . pMercPath . uiEta = GetWorldTotalMin( );
            return;
        }

        // set up node to beginning of path list
        pNode = pCharacter . pMercPath;


        // while there are nodes left with eta less than current time
        while( pNode . pNext . uiEta < GetWorldTotalMin( ) )
        {
            // delete node, move on
            pDeleteNode = pNode;

            // next node
            pNode = pNode . pNext;

            // delete delete node
            MemFree( pDeleteNode );

            // set up merc path to this sector
            pCharacter . pMercPath = pNode;

            // no where left to go
            if( pNode == null )
            {
                return;
            }


            // null out prev to beginning of merc path list
            pNode . pPrev = null;

            // set up new location
            pCharacter . sSectorX = ( int )( pNode . uiSectorId ) % MAP_WORLD_X ;
            pCharacter . sSectorY = ( int )( pNode . uiSectorId ) / MAP_WORLD_X;

            // dirty map panel
            fMapPanelDirty = true;

            if( pNode . pNext == null )
            {
                return;
            }
        }
    }


    void MoveTeamOnFoot( void )
    {
        // run through list of characters on player team, if on foot, move them
        SOLDIERTYPE *pSoldier, *pTeamSoldier;
      int cnt=0;

        // set psoldier as first in merc ptrs
        pSoldier = MercPtrs[0];	

        // go through list of characters, move characters
        for ( pTeamSoldier = MercPtrs[ cnt ]; cnt <= gTacticalStatus.Team[ pSoldier.bTeam ].bLastID; cnt++,pTeamSoldier++)
        {
            if ( pTeamSoldier.bActive )
            {
                MoveCharacterOnPath( pTeamSoldier );
            }
        }

        return;
    }
    */


    /*
    int GetEtaGivenRoute( Path pPath )
    {
        // will return the eta of a passed path in global time units, in minutes
        Path pNode = pPath;

        if( pPath == null )
        {
            return( GetWorldTotalMin( ) );
        }
        else if( pPath . pNext == null )
        {
            return( GetWorldTotalMin( ) );
        }
        else
        {
            // there is a path
            while( pNode . pNext )
            {
                // run through list
                pNode = pNode . pNext;
            }

            // have last sector, therefore the eta of the path
            return( pNode . uiEta );
        }

        // error
        return( 0 );
    }
    */


#if BETA_VERSION
void VerifyAllMercsInGroupAreOnSameSquad(GROUP* pGroup)
{
    PLAYERGROUP* pPlayer;
    SOLDIERTYPE? pSoldier;
    int bSquad = -1;

    // Let's choose somebody in group.....
    pPlayer = pGroup.pPlayerList;

    while (pPlayer != null)
    {
        pSoldier = pPlayer.pSoldier;
        Assert(pSoldier);

        if (pSoldier.bAssignment < ON_DUTY)
        {
            if (bSquad == -1)
            {
                bSquad = pSoldier.bAssignment;
            }
            else
            {
                // better be the same squad!
                Assert(pSoldier.bAssignment == bSquad);
            }
        }

        pPlayer = pPlayer.next;
    }

}
#endif



    void RebuildWayPointsForGroupPath(Path pHeadOfPath, int sMvtGroup)
    {
        int iDelta = 0;
        int iOldDelta = 0;
        bool fFirstNode = true;
        Path pNode = pHeadOfPath;
        GROUP* pGroup = null;
        WAYPOINT* wp = null;


        if ((sMvtGroup == -1) || (sMvtGroup == 0))
        {
            // invalid group...leave
            return;
        }

        pGroup = GetGroup((int)sMvtGroup);

        //KRIS!  Added this because it was possible to plot a new course to the same destination, and the
        //       group would add new arrival events without removing the existing one(s).
        DeleteStrategicEvent(EVENT_GROUP_ARRIVAL, sMvtGroup);

        RemoveGroupWaypoints((int)sMvtGroup);


        if (pGroup.fPlayer)
        {
#if BETA_VERSION
        VerifyAllMercsInGroupAreOnSameSquad(pGroup);
#endif

            // update the destination(s) in the team list
            fTeamPanelDirty = true;

            // update the ETA in character info
            fCharacterInfoPanelDirty = true;

            // allows assignments to flash right away if their subject moves away/returns (robot/vehicle being repaired), or
            // patient/doctor/student/trainer being automatically put on a squad via the movement menu.
            gfReEvaluateEveryonesNothingToDo = true;
        }


        // if group has no path planned at all
        if ((pNode == null) || (pNode.pNext == null))
        {
            // and it's a player group, and it's between sectors
            // NOTE: AI groups never reverse direction between sectors, Kris cheats & teleports them back to their current sector!
            if (pGroup.fPlayer && pGroup.fBetweenSectors)
            {
                // send the group right back to its current sector by reversing directions
                GroupReversingDirectionsBetweenSectors(pGroup, pGroup.ubSectorX, pGroup.ubSectorY, false);
            }

            return;
        }


        // if we're currently between sectors
        if (pGroup.fBetweenSectors)
        {
            // figure out which direction we're already going in  (Otherwise iOldDelta starts at 0)
            iOldDelta = CALCULATE_STRATEGIC_INDEX(pGroup.ubNextX, pGroup.ubNextY) - CALCULATE_STRATEGIC_INDEX(pGroup.ubSectorX, pGroup.ubSectorY);
        }

        // build a brand new list of waypoints, one for initial direction, and another for every "direction change" thereafter
        while (pNode.pNext)
        {
            iDelta = pNode.pNext.uiSectorId - pNode.uiSectorId;
            Debug.Assert(iDelta != 0);        // same sector should never repeat in the path list

            // Waypoints are only added at "pivot points" - whenever there is a change in orthogonal direction.
            // If we're NOT currently between sectors, iOldDelta will start off at 0, which means that the first node can't be
            // added as a waypoint.  This is what we want - for stationary mercs, the first node in a path is the CURRENT sector.
            if ((iOldDelta != 0) && (iDelta != iOldDelta))
            {
                // add this strategic sector as a waypoint
                AddWaypointStrategicIDToPGroup(pGroup, pNode.uiSectorId);
            }

            // remember this delta
            iOldDelta = iDelta;

            pNode = pNode.pNext;
            fFirstNode = false;
        }


        // there must have been at least one next node, or we would have bailed out on "no path" earlier
        Assert(!fFirstNode);

        // the final destination sector - always add a waypoint for it
        AddWaypointStrategicIDToPGroup(pGroup, pNode.uiSectorId);

        // at this point, the final sector in the path must be identical to this group's last waypoint
        wp = GetFinalWaypoint(pGroup);
        AssertMsg(wp, "Path exists, but no waypoints were added!  AM-0");
        AssertMsg(pNode.uiSectorId == (int)CALCULATE_STRATEGIC_INDEX(wp.x, wp.y), "Last waypoint differs from final path sector!  AM-0");


        // see if we've already reached the first sector in the path (we never actually left the sector and reversed back to it)
        if (pGroup.uiArrivalTime == GetWorldTotalMin())
        {
            // never really left.  Must set check for battle true in order for HandleNonCombatGroupArrival() to run!
            GroupArrivedAtSector(pGroup.ubGroupID, true, true);
        }
    }



    // clear strategic movement (mercpaths and waypoints) for this soldier, and his group (including its vehicles)
    void ClearMvtForThisSoldierAndGang(SOLDIERTYPE? pSoldier)
    {
        GROUP* pGroup = null;


        // check if valid grunt
        Assert(pSoldier);

        pGroup = GetGroup(pSoldier.ubGroupID);
        Assert(pGroup);

        // clear their strategic movement (mercpaths and waypoints)
        ClearMercPathsAndWaypointsForAllInGroup(pGroup);
    }



    bool MoveGroupFromSectorToSector(int ubGroupID, int sStartX, int sStartY, int sDestX, int sDestY)
    {
        Path pNode = null;

        // build the path
        pNode = BuildAStrategicPath(pNode, (int)CALCULATE_STRATEGIC_INDEX(sStartX, sStartY), (int)CALCULATE_STRATEGIC_INDEX(sDestX, sDestY), ubGroupID, false /*, false */ );

        if (pNode == null)
        {
            return (false);
        }

        pNode = MoveToBeginningOfPathList(pNode);

        // start movement to next sector
        RebuildWayPointsForGroupPath(pNode, ubGroupID);

        // now clear out the mess
        pNode = ClearStrategicPathList(pNode, -1);

        return (true);
    }


    bool MoveGroupFromSectorToSectorButAvoidLastSector(int ubGroupID, int sStartX, int sStartY, int sDestX, int sDestY)
    {
        Path pNode = null;

        // build the path
        pNode = BuildAStrategicPath(pNode, (int)CALCULATE_STRATEGIC_INDEX(sStartX, sStartY), (int)CALCULATE_STRATEGIC_INDEX(sDestX, sDestY), ubGroupID, false /*, false*/ );

        if (pNode == null)
        {
            return (false);
        }

        // remove tail from path
        pNode = RemoveTailFromStrategicPath(pNode);

        pNode = MoveToBeginningOfPathList(pNode);

        // start movement to next sector
        RebuildWayPointsForGroupPath(pNode, ubGroupID);

        // now clear out the mess
        pNode = ClearStrategicPathList(pNode, -1);

        return (true);
    }

    bool MoveGroupFromSectorToSectorButAvoidPlayerInfluencedSectors(int ubGroupID, int sStartX, int sStartY, int sDestX, int sDestY)
    {
        Path pNode = null;

        // init sectors with soldiers in them
        InitSectorsWithSoldiersList();

        // build the list of sectors with soldier in them
        BuildSectorsWithSoldiersList();

        // turn on the avoid flag
        gfPlotToAvoidPlayerInfuencedSectors = true;

        // build the path
        pNode = BuildAStrategicPath(pNode, (int)CALCULATE_STRATEGIC_INDEX(sStartX, sStartY), (int)CALCULATE_STRATEGIC_INDEX(sDestX, sDestY), ubGroupID, false /*, false */ );

        // turn off the avoid flag
        gfPlotToAvoidPlayerInfuencedSectors = false;

        if (pNode == null)
        {
            if (MoveGroupFromSectorToSector(ubGroupID, sStartX, sStartY, sDestX, sDestY) == false)
            {
                return (false);
            }
            else
            {
                return (true);
            }
        }

        pNode = MoveToBeginningOfPathList(pNode);

        // start movement to next sector
        RebuildWayPointsForGroupPath(pNode, ubGroupID);

        // now clear out the mess
        pNode = ClearStrategicPathList(pNode, -1);

        return (true);
    }

    bool MoveGroupFromSectorToSectorButAvoidPlayerInfluencedSectorsAndStopOneSectorBeforeEnd(int ubGroupID, int sStartX, int sStartY, int sDestX, int sDestY)
    {
        Path pNode = null;

        // init sectors with soldiers in them
        InitSectorsWithSoldiersList();

        // build the list of sectors with soldier in them
        BuildSectorsWithSoldiersList();

        // turn on the avoid flag
        gfPlotToAvoidPlayerInfuencedSectors = true;

        // build the path
        pNode = BuildAStrategicPath(pNode, (int)CALCULATE_STRATEGIC_INDEX(sStartX, sStartY), (int)CALCULATE_STRATEGIC_INDEX(sDestX, sDestY), ubGroupID, false /*, false */ );

        // turn off the avoid flag
        gfPlotToAvoidPlayerInfuencedSectors = false;

        if (pNode == null)
        {
            if (MoveGroupFromSectorToSectorButAvoidLastSector(ubGroupID, sStartX, sStartY, sDestX, sDestY) == false)
            {
                return (false);
            }
            else
            {
                return (true);
            }
        }

        // remove tail from path
        pNode = RemoveTailFromStrategicPath(pNode);

        pNode = MoveToBeginningOfPathList(pNode);

        // start movement to next sector
        RebuildWayPointsForGroupPath(pNode, ubGroupID);

        // now clear out the mess
        pNode = ClearStrategicPathList(pNode, -1);

        return (true);
    }


    /*
    bool MoveGroupToOriginalSector( int ubGroupID )
    {
        GROUP *pGroup;
        int ubDestX, ubDestY;
        pGroup = GetGroup( ubGroupID );
        ubDestX = ( pGroup.ubOriginalSector % 16 ) + 1;
        ubDestY = ( pGroup.ubOriginalSector / 16 ) + 1;
        MoveGroupFromSectorToSector( ubGroupID, pGroup.ubSectorX, pGroup.ubSectorY, ubDestX, ubDestY );

        return( true );
    }
    */


    int GetLengthOfPath(Path pHeadPath)
    {
        int iLength = 0;
        Path pNode = pHeadPath;

        while (pNode)
        {
            pNode = pNode.pNext;
            iLength++;
        }

        return (iLength);
    }

    int GetLengthOfMercPath(SOLDIERTYPE? pSoldier)
    {
        Path pNode = null;
        int iLength = 0;

        pNode = GetSoldierMercPathPtr(pSoldier);
        iLength = GetLengthOfPath(pNode);
        return (iLength);
    }


    bool CheckIfPathIsEmpty(Path pHeadPath)
    {
        // no path
        if (pHeadPath == null)
        {
            return (true);
        }

        // nothing next either
        if (pHeadPath.pNext == null)
        {
            return (true);
        }

        return (false);
    }



    Path GetSoldierMercPathPtr(SOLDIERTYPE? pSoldier)
    {
        Path pMercPath = null;

        Debug.Assert(pSoldier);

        // IN a vehicle?
        if (pSoldier.bAssignment == VEHICLE)
        {
            pMercPath = pVehicleList[pSoldier.iVehicleId].pMercPath;
        }
        // IS a vehicle?
        else if (pSoldier.uiStatusFlags & SOLDIER_VEHICLE)
        {
            pMercPath = pVehicleList[pSoldier.bVehicleID].pMercPath;
        }
        else    // a person
        {
            pMercPath = pSoldier.pMercPath;
        }

        return (pMercPath);
    }



    Path GetGroupMercPathPtr(GROUP* pGroup)
    {
        Path pMercPath = null;
        int iVehicledId = -1;


        Assert(pGroup);

        // must be a player group!
        Assert(pGroup.fPlayer);

        if (pGroup.fVehicle)
        {
            iVehicledId = GivenMvtGroupIdFindVehicleId(pGroup.ubGroupID);
            Assert(iVehicledId != -1);

            pMercPath = pVehicleList[iVehicledId].pMercPath;
        }
        else
        {
            // value returned will be null if there's nobody in the group!
            if (pGroup.pPlayerList && pGroup.pPlayerList.pSoldier)
            {
                pMercPath = pGroup.pPlayerList.pSoldier.pMercPath;
            }
        }

        return (pMercPath);
    }



    int GetSoldierGroupId(SOLDIERTYPE? pSoldier)
    {
        int ubGroupId = 0;

        // IN a vehicle?
        if (pSoldier.bAssignment == VEHICLE)
        {
            ubGroupId = pVehicleList[pSoldier.iVehicleId].ubMovementGroup;
        }
        // IS a vehicle?
        else if (pSoldier.uiStatusFlags & SOLDIER_VEHICLE)
        {
            ubGroupId = pVehicleList[pSoldier.bVehicleID].ubMovementGroup;
        }
        else    // a person
        {
            ubGroupId = pSoldier.ubGroupID;
        }

        return (ubGroupId);
    }



    // clears this groups strategic movement (mercpaths and waypoints), include those in the vehicle structs(!)
    void ClearMercPathsAndWaypointsForAllInGroup(GROUP* pGroup)
    {
        PLAYERGROUP* pPlayer = null;
        SOLDIERTYPE? pSoldier = null;

        pPlayer = pGroup.pPlayerList;
        while (pPlayer)
        {
            pSoldier = pPlayer.pSoldier;

            if (pSoldier != null)
            {
                ClearPathForSoldier(pSoldier);
            }

            pPlayer = pPlayer.next;
        }

        // if it's a vehicle
        if (pGroup.fVehicle)
        {
            int iVehicleId = -1;
            VEHICLETYPE* pVehicle = null;

            iVehicleId = GivenMvtGroupIdFindVehicleId(pGroup.ubGroupID);
            Assert(iVehicleId != -1);

            pVehicle = &(pVehicleList[iVehicleId]);

            // clear the path for that vehicle
            pVehicle.pMercPath = ClearStrategicPathList(pVehicle.pMercPath, pVehicle.ubMovementGroup);
        }

        // clear the waypoints for this group too - no mercpath = no waypoints!
        RemovePGroupWaypoints(pGroup);
        // not used anymore
        //SetWayPointsAsCanceled( pCurrentMerc.ubGroupID );
    }



    // clears the contents of the soldier's mercpPath, as well as his vehicle path if he is a / or is in a vehicle
    void ClearPathForSoldier(SOLDIERTYPE? pSoldier)
    {
        VEHICLETYPE* pVehicle = null;


        // clear the soldier's mercpath
        pSoldier.pMercPath = ClearStrategicPathList(pSoldier.pMercPath, pSoldier.ubGroupID);

        // if a vehicle
        if (pSoldier.uiStatusFlags & SOLDIER_VEHICLE)
        {
            pVehicle = &(pVehicleList[pSoldier.bVehicleID]);
        }
        // or in a vehicle
        else if (pSoldier.bAssignment == VEHICLE)
        {
            pVehicle = &(pVehicleList[pSoldier.iVehicleId]);
        }

        // if there's an associate vehicle structure
        if (pVehicle != null)
        {
            // clear its mercpath, too
            pVehicle.pMercPath = ClearStrategicPathList(pVehicle.pMercPath, pVehicle.ubMovementGroup);
        }
    }



    void AddSectorToFrontOfMercPathForAllSoldiersInGroup(GROUP* pGroup, int ubSectorX, int ubSectorY)
    {
        PLAYERGROUP* pPlayer = null;
        SOLDIERTYPE? pSoldier = null;

        pPlayer = pGroup.pPlayerList;
        while (pPlayer)
        {
            pSoldier = pPlayer.pSoldier;

            if (pSoldier != null)
            {
                AddSectorToFrontOfMercPath(&(pSoldier.pMercPath), ubSectorX, ubSectorY);
            }

            pPlayer = pPlayer.next;
        }

        // if it's a vehicle
        if (pGroup.fVehicle)
        {
            int iVehicleId = -1;
            VEHICLETYPE* pVehicle = null;

            iVehicleId = GivenMvtGroupIdFindVehicleId(pGroup.ubGroupID);
            Assert(iVehicleId != -1);

            pVehicle = &(pVehicleList[iVehicleId]);

            // add it to that vehicle's path
            AddSectorToFrontOfMercPath(&(pVehicle.pMercPath), ubSectorX, ubSectorY);
        }
    }



    void AddSectorToFrontOfMercPath(Path* ppMercPath, int ubSectorX, int ubSectorY)
    {
        Path pNode = null;


        // allocate and hang a new node at the front of the path list
        pNode = MemAlloc(sizeof(PathSt));

        pNode.uiSectorId = CALCULATE_STRATEGIC_INDEX(ubSectorX, ubSectorY);
        pNode.pNext = *ppMercPath;
        pNode.pPrev = null;
        pNode.uiEta = GetWorldTotalMin();
        pNode.fSpeed = NORMAL_MVT;

        // if path wasn't null
        if (*ppMercPath != null)
        {
            // hang the previous pointer of the old head to the new head
            (*ppMercPath).pPrev = pNode;
        }

        *ppMercPath = pNode;
    }

}
