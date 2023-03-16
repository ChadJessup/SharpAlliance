using System;
using System.IO;
using System.Linq;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class Keys
{
    //This is the link to see if a door exists at a gridno.  
    public static DOOR? FindDoorInfoAtGridNo(int iMapIndex)
    {
        for (int i = 0; i < Globals.gubNumDoors; i++)
        {
            if (Globals.DoorTable[i].sGridNo == iMapIndex)
            {
                return Globals.DoorTable[i];
            }
        }
        return null;
    }

    // Returns a doors status value, null if not found
    public static DOOR_STATUS? GetDoorStatus(int sGridNo)
    {
        int ubCnt;
        STRUCTURE? pStructure;
        STRUCTURE? pBaseStructure;

        //if there is an array
        if (Globals.gpDoorStatus.Any())
        {
            // Find the base tile for the door structure and use that gridno
            pStructure = WorldStructures.FindStructure(sGridNo, STRUCTUREFLAGS.ANYDOOR);
            if (pStructure is not null)
            {
                pBaseStructure = WorldStructures.FindBaseStructure(pStructure);
            }
            else
            {
                pBaseStructure = null;
            }

            if (pBaseStructure == null)
            {
                return (null);
            }

            //Check to see if the user is adding an existing door
            for (ubCnt = 0; ubCnt < Globals.gubNumDoorStatus; ubCnt++)
            {
                //if this is the door
                if (Globals.gpDoorStatus[ubCnt].sGridNo == pBaseStructure.sGridNo)
                {
                    return ((Globals.gpDoorStatus[ubCnt]));
                }
            }
        }

        return (null);
    }


    public static bool AllMercsLookForDoor(int sGridNo, bool fUpdateValue)
    {
        int cnt, cnt2;
        WorldDirections[] bDirs = new WorldDirections[8] {
            WorldDirections.NORTH, WorldDirections.SOUTH, WorldDirections.EAST, 
                WorldDirections.WEST, WorldDirections.NORTHEAST, WorldDirections.NORTHWEST,
                WorldDirections.SOUTHEAST, WorldDirections.SOUTHWEST };
        SOLDIERTYPE? pSoldier;
        int sDistVisible;
        DOOR_STATUS? pDoorStatus;
        int usNewGridNo;

        // Get door
        pDoorStatus = GetDoorStatus(sGridNo);

        if (pDoorStatus == null)
        {
            return (false);
        }

        // IF IT'S THE SELECTED GUY, MAKE ANOTHER SELECTED!
        cnt = Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bFirstID;

        // look for all mercs on the same team, 
        for (pSoldier = Globals.MercPtrs[cnt]; cnt <= Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bLastID; cnt++)//, pSoldier++)
        {
            // ATE: Ok, lets check for some basic things here!
            if (pSoldier.bLife >= Globals.OKLIFE && pSoldier.sGridNo != Globals.NOWHERE && pSoldier.bActive && pSoldier.bInSector)
            {
                // is he close enough to see that gridno if he turns his head?
                sDistVisible = DistanceVisible(pSoldier, DIRECTION_IRRELEVANT, DIRECTION_IRRELEVANT, sGridNo, 0);

                if (IsometricUtils.PythSpacesAway(pSoldier.sGridNo, sGridNo) <= sDistVisible)
                {
                    // and we can trace a line of sight to his x,y coordinates?
                    // (taking into account we are definitely aware of this guy now)
                    if (SoldierTo3DLocationLineOfSightTest(pSoldier, sGridNo, 0, 0, (int)sDistVisible, true))
                    {
                        // Update status...
                        if (fUpdateValue)
                        {
                            InternalUpdateDoorsPerceivedValue(pDoorStatus);
                        }
                        return (true);
                    }
                }

                // Now try other adjacent gridnos...
                for (cnt2 = 0; cnt2 < 8; cnt2++)
                {
                    usNewGridNo = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc(bDirs[cnt2]));
                    sDistVisible = DistanceVisible(pSoldier, DIRECTION_IRRELEVANT, DIRECTION_IRRELEVANT, usNewGridNo, 0);

                    if (IsometricUtils.PythSpacesAway(pSoldier.sGridNo, usNewGridNo) <= sDistVisible)
                    {
                        // and we can trace a line of sight to his x,y coordinates?
                        // (taking into account we are definitely aware of this guy now)
                        if (SoldierTo3DLocationLineOfSightTest(pSoldier, usNewGridNo, 0, 0, (int)sDistVisible, true))
                        {
                            // Update status...
                            if (fUpdateValue)
                            {
                                InternalUpdateDoorsPerceivedValue(pDoorStatus);
                            }
                            return (true);
                        }
                    }
                }
            }
        }

        return (false);
    }


    public static bool MercLooksForDoors(SOLDIERTYPE? pSoldier, bool fUpdateValue)
    {
        int cnt, cnt2;
        int sDistVisible;
        int sGridNo;
        DOOR_STATUS? pDoorStatus;
        WorldDirections[] bDirs = new WorldDirections[8] { WorldDirections.NORTH,
            WorldDirections.SOUTH, WorldDirections.EAST, WorldDirections.WEST,
            WorldDirections.NORTHEAST, WorldDirections.NORTHWEST, WorldDirections.SOUTHEAST,
            WorldDirections.SOUTHWEST };

        int usNewGridNo;



        // Loop through all corpses....
        for (cnt = 0; cnt < Globals.gubNumDoorStatus; cnt++)
        {
            pDoorStatus = (Globals.gpDoorStatus[cnt]);

            if (!InternalIsPerceivedDifferentThanReality(pDoorStatus))
            {
                continue;
            }

            sGridNo = pDoorStatus.sGridNo;

            // is he close enough to see that gridno if he turns his head?
            sDistVisible = DistanceVisible(pSoldier, DIRECTION_IRRELEVANT, DIRECTION_IRRELEVANT, sGridNo, 0);

            if (IsometricUtils.PythSpacesAway(pSoldier.sGridNo, sGridNo) <= sDistVisible)
            {
                // and we can trace a line of sight to his x,y coordinates?
                // (taking into account we are definitely aware of this guy now)
                if (SoldierTo3DLocationLineOfSightTest(pSoldier, sGridNo, 0, 0, (int)sDistVisible, true))
                {
                    // OK, here... update perceived value....
                    if (fUpdateValue)
                    {
                        InternalUpdateDoorsPerceivedValue(pDoorStatus);

                        // Update graphic....
                        InternalUpdateDoorGraphicFromStatus(pDoorStatus, true, true);
                    }
                    return (true);
                }
            }

            // Now try other adjacent gridnos...
            for (cnt2 = 0; cnt2 < 8; cnt2++)
            {
                usNewGridNo = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc(bDirs[cnt2]));

                if (IsometricUtils.PythSpacesAway(pSoldier.sGridNo, usNewGridNo) <= sDistVisible)
                {
                    // and we can trace a line of sight to his x,y coordinates?
                    // (taking into account we are definitely aware of this guy now)
                    if (SoldierTo3DLocationLineOfSightTest(pSoldier, usNewGridNo, 0, 0, (int)sDistVisible, true))
                    {
                        // Update status...
                        if (fUpdateValue)
                        {
                            InternalUpdateDoorsPerceivedValue(pDoorStatus);

                            // Update graphic....
                            InternalUpdateDoorGraphicFromStatus(pDoorStatus, true, true);

                        }
                        return (true);
                    }
                }
            }

        }

        return (false);
    }

    void SyncronizeDoorStatusToStructureData(DOOR_STATUS? pDoorStatus)
    {
        STRUCTURE? pStructure, pBaseStructure;
        LEVELNODE? pNode;
        int sBaseGridNo = Globals.NOWHERE;

        // First look for a door structure here...
        pStructure = WorldStructures.FindStructure(pDoorStatus.sGridNo, STRUCTUREFLAGS.ANYDOOR);

        if (pStructure is not null)
        {
            pBaseStructure = WorldStructures.FindBaseStructure(pStructure);
            sBaseGridNo = pBaseStructure.sGridNo;
        }
        else
        {
            pBaseStructure = null;
        }

        if (pBaseStructure == null)
        {
		//ScreenMsg( FONT_MCOLOR_LTYELLOW, MSG_BETAVERSION, "Door structure data at %d was not found", pDoorStatus.sGridNo );
            return;
        }

        pNode = WorldStructures.FindLevelNodeBasedOnStructure(sBaseGridNo, pBaseStructure);
        if (pNode is null)
        {
            // ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_BETAVERSION, "Could not find levelnode from door structure at %d", pDoorStatus.sGridNo);
            return;
        }

        // ATE: OK let me explain something here:
        // One of the purposes of this function is to MAKE sure the door status MATCHES
        // the struct data value - if not - change ( REGARDLESS of perceived being used or not... )
        // 
        // Check for opened...
        if (pDoorStatus.ubFlags & DOOR_OPEN)
        {
            // IF closed.....
            if (!(pStructure.fFlags & STRUCTUREFLAGS.OPEN))
            {
                // Swap!
                SwapStructureForPartner(sBaseGridNo, pBaseStructure);
                RecompileLocalMovementCosts(sBaseGridNo);
            }
        }
        else
        {
            if ((pStructure.fFlags & STRUCTUREFLAGS.OPEN))
            {
                // Swap!
                SwapStructureForPartner(sBaseGridNo, pBaseStructure);
                RecompileLocalMovementCosts(sBaseGridNo);
            }
        }
    }

    void UpdateDoorGraphicsFromStatus(bool fUsePerceivedStatus, bool fDirty)
    {
        int cnt;
        DOOR_STATUS? pDoorStatus;

        for (cnt = 0; cnt < gubNumDoorStatus; cnt++)
        {
            pDoorStatus = (gpDoorStatus[cnt]);

            // ATE: Make sure door status flag and struct info are syncronized....
            SyncronizeDoorStatusToStructureData(pDoorStatus);

            InternalUpdateDoorGraphicFromStatus(pDoorStatus, fUsePerceivedStatus, fDirty);
        }
    }

    private static void InternalUpdateDoorGraphicFromStatus(DOOR_STATUS? pDoorStatus, bool fUsePerceivedStatus, bool fDirty)
    {
        STRUCTURE? pStructure, pBaseStructure;
        int cnt;
        bool fOpenedGraphic = false;
        LEVELNODE? pNode;
        bool fWantToBeOpen = false;
        bool fDifferent = false;
        int sBaseGridNo = Globals.NOWHERE;


        // OK, look at perceived status and adjust graphic
        // First look for a door structure here...
        pStructure = WorldStructures.FindStructure(pDoorStatus.sGridNo, STRUCTUREFLAGS.ANYDOOR);

        if (pStructure is not null)
        {
            pBaseStructure = WorldStructures.FindBaseStructure(pStructure);
            sBaseGridNo = pBaseStructure.sGridNo;
        }
        else
        {
            pBaseStructure = null;
        }

        if (pBaseStructure == null)
        {
		//ScreenMsg( FONT_MCOLOR_LTYELLOW, MSG_BETAVERSION, "Door structure data at %d was not found", pDoorStatus.sGridNo );
            return;
        }

        pNode = WorldStructures.FindLevelNodeBasedOnStructure(sBaseGridNo, pBaseStructure);
        if (pNode is null)
        {
            //ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_BETAVERSION, "Could not find levelnode from door structure at %d", pDoorStatus.sGridNo);
            return;
        }

        // Get status we want to chenge to.....
        if (fUsePerceivedStatus)
        {
            if (pDoorStatus.ubFlags.HasFlag(DOOR_STATUS_FLAGS.PERCEIVED_OPEN))
            {
                fWantToBeOpen = true;
            }
        }
        else
        {
            if (pDoorStatus.ubFlags.HasFlag(DOOR_STATUS_FLAGS.OPEN))
            {
                fWantToBeOpen = true;
            }
        }

        // First look for an opened door
        // get what it is now...
        cnt = 0;
        while (Globals.gClosedDoorList[cnt] != -1)
        {
            // IF WE ARE A SHADOW TYPE
            if (pNode.usIndex == Globals.gClosedDoorList[cnt])
            {
                fOpenedGraphic = true;
                break;
            }
            cnt++;
        };

        // OK, we either have an opened graphic, in which case we want to switch to the closed, or a closed
        // in which case we want to switch to opened...
        // adjust o' graphic


        // OK, we now need to test these things against the true structure data
        // we may need to only adjust the graphic here....
        if (fWantToBeOpen && (pStructure.fFlags & STRUCTUREFLAGS.OPEN))
        {
            bool fFound = false;
            // Adjust graphic....

            // Loop through and and find opened graphic for the closed one....
            cnt = 0;
            while (gOpenDoorList[cnt] != -1)
            {
                // IF WE ARE A SHADOW TYPE
                if (pNode.usIndex == gOpenDoorList[cnt])
                {
                    fFound = true;
                    break;
                }
                cnt++;
            };

            // OK, now use opened graphic.
            if (fFound)
            {
                pNode.usIndex = gClosedDoorList[cnt];

                if (fDirty)
                {
                    RenderWorld.InvalidateWorldRedundency();
                    RenderWorld.SetRenderFlags(RenderingFlags.FULL);
                }
            }

            return;
        }

        // If we want to be closed but structure is closed
        if (!fWantToBeOpen && !(pStructure.fFlags.HasFlag(STRUCTUREFLAGS.OPEN)))
        {
            bool fFound = false;
            // Adjust graphic....

            // Loop through and and find closed graphic for the opend one....
            cnt = 0;
            while (gClosedDoorList[cnt] != (TileDefines)(-1))
            {
                // IF WE ARE A SHADOW TYPE
                if (pNode.usIndex == gClosedDoorList[cnt])
                {
                    fFound = true;
                    break;
                }
                cnt++;
            };

            // OK, now use opened graphic.
            if (fFound)
            {
                pNode.usIndex = gOpenDoorList[cnt];

                if (fDirty)
                {
                    RenderWorld.InvalidateWorldRedundency();
                    RenderWorld.SetRenderFlags(RenderingFlags.FULL);
                }
            }

            return;
        }


        if (fOpenedGraphic && !fWantToBeOpen)
        {
            // Close the beast!
            fDifferent = true;
            pNode.usIndex = gOpenDoorList[cnt];
        }
        else if (!fOpenedGraphic && fWantToBeOpen)
        {
            // Find the closed door graphic and adjust....
            cnt = 0;
            while (gOpenDoorList[cnt] != -1)
            {
                // IF WE ARE A SHADOW TYPE
                if (pNode.usIndex == gOpenDoorList[cnt])
                {
                    // Open the beast!
                    fDifferent = true;
                    pNode.usIndex = gClosedDoorList[cnt];
                    break;
                }
                cnt++;
            };
        }

        if (fDifferent)
        {
            SwapStructureForPartner(sBaseGridNo, pBaseStructure);

            RecompileLocalMovementCosts(sBaseGridNo);

            if (fDirty)
            {
                RenderWorld.InvalidateWorldRedundency();
                RenderWorld.SetRenderFlags(RenderingFlags.FULL);
            }
        }
    }


    private static bool InternalIsPerceivedDifferentThanReality(DOOR_STATUS? pDoorStatus)
    {
        if ((pDoorStatus.ubFlags.HasFlag(DOOR_STATUS_FLAGS.PERCEIVED_NOTSET)))
        {
            return (true);
        }

        // Compare flags....
        if ((pDoorStatus.ubFlags & DOOR_STATUS_FLAGS.OPEN && pDoorStatus.ubFlags & DOOR_STATUS_FLAGS.PERCEIVED_OPEN) ||
                 (!(pDoorStatus.ubFlags & DOOR_STATUS_FLAGS.OPEN) && !(pDoorStatus.ubFlags & DOOR_STATUS_FLAGS.PERCEIVED_OPEN)))
        {
            return (false);
        }

        return (true);
    }

    private static void InternalUpdateDoorsPerceivedValue(DOOR_STATUS? pDoorStatus)
    {
        // OK, look at door, set perceived value the same as actual....
        if (pDoorStatus.ubFlags.HasFlag(DOOR_STATUS_FLAGS.OPEN))
        {
            InternalSetDoorPerceivedOpenStatus(pDoorStatus, true);
        }
        else
        {
            InternalSetDoorPerceivedOpenStatus(pDoorStatus, false);
        }
    }

    bool UpdateDoorStatusPerceivedValue(int sGridNo)
    {
        DOOR_STATUS? pDoorStatus = null;

        pDoorStatus = GetDoorStatus(sGridNo);
        CHECKF(pDoorStatus != null);

        InternalUpdateDoorsPerceivedValue(pDoorStatus);

        return (true);
    }


    bool IsDoorPerceivedOpen(int sGridNo)
    {
        DOOR_STATUS? pDoorStatus;

        pDoorStatus = GetDoorStatus(sGridNo);

        if (pDoorStatus is not null && pDoorStatus.ubFlags.HasFlag(DOOR_STATUS_FLAGS.PERCEIVED_OPEN))
        {
            return (true);
        }
        else
        {
            if (pDoorStatus is null)
            {
                // ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_TESTVERSION, "WARNING! Failed to find the Perceived Open Door Status on Gridno %s", sGridNo);
            }

            return (false);
        }
    }


    private static bool InternalSetDoorPerceivedOpenStatus(DOOR_STATUS? pDoorStatus, bool fPerceivedOpen)
    {
        if (fPerceivedOpen)
        {
            pDoorStatus.ubFlags |= DOOR_STATUS_FLAGS.PERCEIVED_OPEN;
        }
        else
        {
            pDoorStatus.ubFlags &= ~DOOR_STATUS_FLAGS.PERCEIVED_OPEN;
        }

        // Turn off perceived not set flag....
        pDoorStatus.ubFlags &= ~DOOR_STATUS_FLAGS.PERCEIVED_NOTSET;

        return (true);
    }


    bool SetDoorPerceivedOpenStatus(int sGridNo, bool fPerceivedOpen)
    {
        DOOR_STATUS? pDoorStatus = null;

        pDoorStatus = GetDoorStatus(sGridNo);

        CHECKF(pDoorStatus != null);

        return (InternalSetDoorPerceivedOpenStatus(pDoorStatus, fPerceivedOpen));

    }


    bool SetDoorOpenStatus(int sGridNo, bool fOpen)
    {
        DOOR_STATUS? pDoorStatus;

        pDoorStatus = GetDoorStatus(sGridNo);

        if (pDoorStatus)
        {
            if (fOpen)
            {
                pDoorStatus.ubFlags |= DOOR_STATUS_FLAGS.OPEN;
            }
            else
            {
                pDoorStatus.ubFlags &= ~DOOR_STATUS_FLAGS.OPEN;
            }
            return (true);
        }
        else
        {
            return (false);
        }

    }


    bool SaveDoorStatusArrayToDoorStatusTempFile(int sSectorX, int sSectorY, int bSectorZ)
    {
        string zMapName;
        Stream hFile;
        int uiNumBytesWritten;
        int ubCnt;

        // Turn off any DOOR BUSY flags....
        for (ubCnt = 0; ubCnt < gubNumDoorStatus; ubCnt++)
        {
            gpDoorStatus[ubCnt].ubFlags &= (~DOOR_STATUS_FLAGS.BUSY);
        }


        //Convert the current sector location into a file name
        //	GetMapFileName( sSectorX, sSectorY, bSectorZ, zTempName, false );

        //add the 'm' for 'Modifed Map' to the front of the map name
        //	sprintf( zMapName, "%s\\ds_%s", MAPS_DIR, zTempName);

        GetMapTempFileName(SF.DOOR_STATUS_TEMP_FILE_EXISTS, zMapName, sSectorX, sSectorY, bSectorZ);


        //Open the file for writing, Create it if it doesnt exist
        hFile = FileManager.FileOpen(zMapName, FILE_ACCESS_WRITE | FILE_OPEN_ALWAYS, false);
        if (hFile == 0)
        {
            //Error opening map modification file
            return (false);
        }


        //Save the number of elements in the door array
        FileManager.FileWrite(hFile, gubNumDoorStatus, sizeof(int), out uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int))
        {
            //Error Writing size of array to disk
            FileManager.FileClose(hFile);
            return (false);
        }

        //if there is some to save
        if (gubNumDoorStatus != 0)
        {
            //Save the door array
            FileManager.FileWrite(hFile, gpDoorStatus, (sizeof(DOOR_STATUS) * gubNumDoorStatus), out uiNumBytesWritten);
            if (uiNumBytesWritten != (sizeof(DOOR_STATUS) * gubNumDoorStatus))
            {
                //Error Writing size of array to disk
                FileManager.FileClose(hFile);
                return (false);
            }
        }

        FileManager.FileClose(hFile);

        //Set the flag indicating that there is a door status array
        SetSectorFlag(sSectorX, sSectorY, bSectorZ, SF.DOOR_STATUS_TEMP_FILE_EXISTS);

        return (true);
    }


    bool LoadDoorStatusArrayFromDoorStatusTempFile()
    {
        string zMapName;
        Stream hFile;
        int uiNumBytesRead;
        int ubLoop;

        //Convert the current sector location into a file name
        //	GetMapFileName( gWorldSectorX, gWorldSectorY, gbWorldSectorZ, zTempName, false );

        //add the 'm' for 'Modifed Map' to the front of the map name
        //	sprintf( zMapName, "%s\\ds_%s", MAPS_DIR, zTempName);

        GetMapTempFileName(SF.DOOR_STATUS_TEMP_FILE_EXISTS, zMapName, gWorldSectorX, gWorldSectorY, gbWorldSectorZ);

        //Get rid of the existing door array
        TrashDoorStatusArray();

        //Open the file for reading
        hFile = FileManager.FileOpen(zMapName, FILE_ACCESS_READ | FILE_OPEN_EXISTING, false);
        if (hFile == 0)
        {
            //Error opening map modification file,
            return (false);
        }


        // Load the number of elements in the door status array
        FileManager.FileRead(hFile, ref gubNumDoorStatus, sizeof(int), out uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int))
        {
            FileManager.FileClose(hFile);
            return (false);
        }

        if (gubNumDoorStatus == 0)
        {
            FileManager.FileClose(hFile);
            return (true);
        }


        //Allocate space for the door status array
        gpDoorStatus = MemAlloc(sizeof(DOOR_STATUS) * gubNumDoorStatus);
        if (gpDoorStatus == null)
        {
            //AssertMsg(0, "Error Allocating memory for the gpDoorStatus");
        }

        //memset(gpDoorStatus, 0, sizeof(DOOR_STATUS) * gubNumDoorStatus);


        // Load the number of elements in the door status array
        FileManager.FileRead(hFile, gpDoorStatus, (sizeof(DOOR_STATUS) * gubNumDoorStatus), out uiNumBytesRead);
        if (uiNumBytesRead != (sizeof(DOOR_STATUS) * gubNumDoorStatus))
        {
            FileManager.FileClose(hFile);
            return (false);
        }

        FileManager.FileClose(hFile);

        // the graphics will be updated later in the loading process.

        // set flags in map for containing a door status 
        for (ubLoop = 0; ubLoop < gubNumDoorStatus; ubLoop++)
        {
            gpWorldLevelData[gpDoorStatus[ubLoop].sGridNo].ubExtFlags[0] |= MAPELEMENT_EXT.DOOR_STATUS_PRESENT;
        }

        UpdateDoorGraphicsFromStatus(true, false);

        return (true);
    }


    bool SaveKeyTableToSaveGameFile(Stream hFile)
    {
        int uiNumBytesWritten = 0;


        // Save the KeyTable
        FileManager.FileWrite(hFile, KeyTable, sizeof(KEY) * NUM_KEYS, out uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(KEY) * NUM_KEYS)
        {
            return (false);
        }

        return (true);
    }

    bool LoadKeyTableFromSaveedGameFile(Stream hFile)
    {
        int uiNumBytesRead = 0;


        // Load the KeyTable
        FileManager.FileRead(hFile, KeyTable, sizeof(KEY) * NUM_KEYS, out uiNumBytesRead);
        if (uiNumBytesRead != sizeof(KEY) * NUM_KEYS)
        {
            return (false);
        }

        return (true);
    }



    void ExamineDoorsOnEnteringSector()
    {
        int cnt;
        DOOR_STATUS? pDoorStatus;
        SOLDIERTYPE? pSoldier;
        bool fOK = false;
        int bTownId;

        // OK, only do this if conditions are met....
        // If this is any omerta tow, don't do it...
        bTownId = GetTownIdForSector(gWorldSectorX, gWorldSectorY);

        if (bTownId == OMERTA)
        {
            return;
        }

        // Check time...
        if ((GameClock.GetWorldTotalMin() - gTacticalStatus.uiTimeSinceLastInTactical) < 30)
        {
            return;
        }

        // there is at least one human being in that sector.
        // check for civ
        cnt = gTacticalStatus.Team[ENEMY_TEAM].bFirstID;
        // look for all mercs on the same team, 
        for (pSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[LAST_TEAM].bLastID; cnt++, pSoldier++)
        {
            if (pSoldier.bActive)
            {
                if (pSoldier.bInSector)
                {
                    fOK = true;
                    break;
                }
            }
        }

        // Let's do it!
        if (fOK)
        {
            for (cnt = 0; cnt < gubNumDoorStatus; cnt++)
            {
                pDoorStatus = &(gpDoorStatus[cnt]);

                // Get status of door....
                if (pDoorStatus.ubFlags & DOOR_STATUS_FLAGS.OPEN)
                {
                    // If open, close!
                    HandleDoorChangeFromGridNo(null, pDoorStatus.sGridNo, true);
                }
            }
        }
    }

    void HandleDoorsChangeWhenEnteringSectorCurrentlyLoaded()
    {
        int cnt;
        DOOR_STATUS? pDoorStatus;
        SOLDIERTYPE? pSoldier;
        bool fOK = false;
        int iNumNewMercs = 0;
        TOWNS bTownId;

        // OK, only do this if conditions are met....

        // If this is any omerta tow, don't do it...
        bTownId = GetTownIdForSector(gWorldSectorX, gWorldSectorY);

        if (bTownId == TOWNS.OMERTA)
        {
            return;
        }

        // 1 ) there is at least one human being in that sector.
        // check for civ
        cnt = gTacticalStatus.Team[TEAM.ENEMY_TEAM].bFirstID;

        // Check time...
        if ((GameClock.GetWorldTotalMin() - gTacticalStatus.uiTimeSinceLastInTactical) < 30)
        {
            return;
        }

        // look for all mercs on the same team, 
        for (pSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[LAST_TEAM].bLastID; cnt++, pSoldier++)
        {
            if (pSoldier.bActive && pSoldier.bInSector)
            {
                fOK = true;
                break;
            }
        }

        // Loop through our team now....
        cnt = gTacticalStatus.Team[gbPlayerNum].bFirstID;
        for (pSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[gbPlayerNum].bLastID; cnt++, pSoldier++)
        {
            if (pSoldier.bActive && pSoldier.bInSector && gbMercIsNewInThisSector[cnt] > 0)
            {
                iNumNewMercs++;
            }
        }

        // ATE: Only do for newly added mercs....
        if (iNumNewMercs == 0)
        {
            return;
        }

        // Let's do it!
        if (fOK)
        {
            for (cnt = 0; cnt < gubNumDoorStatus; cnt++)
            {
                pDoorStatus = &(gpDoorStatus[cnt]);

                // Get status of door....
                if (pDoorStatus.ubFlags & DOOR_OPEN)
                {
                    // If open, close!
                    gfSetPerceivedDoorState = true;

                    HandleDoorChangeFromGridNo(null, pDoorStatus.sGridNo, true);

                    gfSetPerceivedDoorState = false;

                    AllMercsLookForDoor(pDoorStatus.sGridNo, true);

                    InternalUpdateDoorGraphicFromStatus(pDoorStatus, true, true);

                }
            }
        }
    }


    void DropKeysInKeyRing(SOLDIERTYPE? pSoldier, int sGridNo, int bLevel, int bVisible, bool fAddToDropList, int iDropListSlot, bool fUseUnLoaded)
    {
        int ubLoop;
        int ubItem;
        OBJECTTYPE Object;

        if (!(pSoldier.pKeyRing))
        {
            // no key ring!
            return;
        }
        for (ubLoop = 0; ubLoop < NUM_KEYS; ubLoop++)
        {
            ubItem = pSoldier.pKeyRing[ubLoop].ubKeyID;

            if (pSoldier.pKeyRing[ubLoop].ubNumber > 0)
            {
                CreateKeyObject(out Object, pSoldier.pKeyRing[ubLoop].ubNumber, ubItem);

                // Zero out entry
                pSoldier.pKeyRing[ubLoop].ubNumber = 0;
                pSoldier.pKeyRing[ubLoop].ubKeyID = INVALID_KEY_NUMBER;

                if (fAddToDropList)
                {
                    AddItemToLeaveIndex(out Object, iDropListSlot);
                }
                else
                {
                    if (pSoldier.sSectorX != gWorldSectorX || pSoldier.sSectorY != gWorldSectorY || pSoldier.bSectorZ != gbWorldSectorZ || fUseUnLoaded)
                    {
                        // Set flag for item...
                        AddItemsToUnLoadedSector(pSoldier.sSectorX, pSoldier.sSectorY, pSoldier.bSectorZ, sGridNo, 1, out Object, bLevel, WOLRD_ITEM_FIND_SWEETSPOT_FROM_GRIDNO | WORLD_ITEM_REACHABLE, 0, bVisible, false);
                    }
                    else
                    {
                        // Add to pool
                        AddItemToPool(sGridNo, out Object, bVisible, bLevel, 0, 0);
                    }
                }
            }
        }
    }
}

public struct KEY
{

    public int usItem;                      // index in item table for key
    public int fFlags;                       // flags...
    public int usSectorFound;       // where and
    public int usDateFound;			// when the key was found
}

public enum DoorTrapTypes
{
    NO_TRAP = 0,
    EXPLOSION,
    ELECTRIC,
    SIREN,
    SILENT_ALARM,
    BROTHEL_SIREN,
    SUPER_ELECTRIC,
    NUM_DOOR_TRAPS
}

[Flags]
public enum DOOR_TRAP
{
    STOPS_ACTION = 0x01,
    RECURRING = 0x02,
    SILENT = 0x04,
}

public struct DOORTRAP
{
    public DOOR_TRAP fFlags;    // stops action?  recurring trap?
}

//The status of the door, either open or closed
[Flags]
public enum DOOR_STATUS_FLAGS
{
    OPEN = 0x01,
    PERCEIVED_OPEN = 0x02,
    PERCEIVED_NOTSET = 0x04,
    BUSY = 0x08,
    HAS_TIN_CAN = 0x10,
}

public class DOOR_STATUS
{
    public int sGridNo;
    public DOOR_STATUS_FLAGS ubFlags;
}
