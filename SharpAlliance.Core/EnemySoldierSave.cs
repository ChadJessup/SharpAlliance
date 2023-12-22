using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform.Interfaces;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public partial class Globals
{

}

public class EnemySoldierSave
{
    private readonly ILogger<EnemySoldierSave> logger;
    private static IFileManager files;

    public EnemySoldierSave(
        ILogger<EnemySoldierSave> logger,
        IFileManager fileManager)
    {
        this.logger = logger;
        files = fileManager;
    }

    //If we are saving a game and we are in the sector, we will need to preserve the links between the 
    //soldiers and the soldier init list.  Otherwise, the temp file will be deleted.
    public static bool NewWayOfSavingEnemyAndCivliansToTempFile(
        int sSectorX,
        MAP_ROW sSectorY,
        int bSectorZ,
        bool fEnemy,
        bool fValidateOnly)
    {
        SOLDIERINITNODE? curr;
        SOLDIERTYPE? pSoldier;
        int i;
        int slots = 0;
        uint uiTimeStamp;
        Stream hfile;
        //	CHAR8		zTempName[ 128 ];
        string zMapName = string.Empty;// [128];
        SEC ubSectorID;
        int usCheckSum;
        TEAM ubStartID = 0;
        TEAM ubEndID = 0;

        //if we are saving the enemy info to the enemy temp file
        if (fEnemy)
        {
            ubStartID = ENEMY_TEAM;
            ubEndID = CREATURE_TEAM;
        }

        //else its the civilian team
        else
        {
            ubStartID = CIV_TEAM;
            ubEndID = CIV_TEAM;
        }



        //STEP ONE:  Prep the soldiers for saving...

        //modify the map's soldier init list to reflect the changes to the member's still alive...
        for (i = gTacticalStatus.Team[ubStartID].bFirstID; i <= gTacticalStatus.Team[ubEndID].bLastID; i++)
        {
            pSoldier = MercPtrs[i];

            //make sure the person is active, alive, in the sector, and is not a profiled person
            if (pSoldier.IsActive /*&& pSoldier.bInSector*/
                && pSoldier.bLife > 0
                && pSoldier.ubProfile == NO_PROFILE)
            { //soldier is valid, so find the matching soldier init list entry for modification.
                curr = gSoldierInitHead;
                while (curr is not null && curr.pSoldier != pSoldier)
                {
                    curr = curr.next;
                }

                if (curr is not null && curr.pSoldier == pSoldier && pSoldier.ubProfile == NO_PROFILE)
                { //found a match.  

                    if (!fValidateOnly)
                    {
                        if (!(gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.LOADING_SAVED_GAME)))
                        {
                            if (curr.pDetailedPlacement is null)
                            { //need to upgrade the placement to detailed placement
                                curr.pBasicPlacement.fDetailedPlacement = true;
                                curr.pDetailedPlacement = new SOLDIERCREATE_STRUCT();
                            }

                            //Copy over the data of the soldier.
                            curr.pDetailedPlacement.ubProfile = NO_PROFILE;
                            curr.pDetailedPlacement.bLife = pSoldier.bLife;
                            curr.pDetailedPlacement.bLifeMax = pSoldier.bLifeMax;
                            curr.pDetailedPlacement.bAgility = pSoldier.bAgility;
                            curr.pDetailedPlacement.bDexterity = pSoldier.bDexterity;
                            curr.pDetailedPlacement.bExpLevel = pSoldier.bExpLevel;
                            curr.pDetailedPlacement.bMarksmanship = pSoldier.bMarksmanship;
                            curr.pDetailedPlacement.bMedical = pSoldier.bMedical;
                            curr.pDetailedPlacement.bMechanical = pSoldier.bMechanical;
                            curr.pDetailedPlacement.bExplosive = pSoldier.bExplosive;
                            curr.pDetailedPlacement.bLeadership = pSoldier.bLeadership;
                            curr.pDetailedPlacement.bStrength = pSoldier.bStrength;
                            curr.pDetailedPlacement.bWisdom = pSoldier.bWisdom;
                            curr.pDetailedPlacement.bAttitude = pSoldier.bAttitude;
                            curr.pDetailedPlacement.bOrders = pSoldier.bOrders;
                            curr.pDetailedPlacement.bMorale = pSoldier.bMorale;
                            curr.pDetailedPlacement.bAIMorale = pSoldier.bAIMorale;
                            curr.pDetailedPlacement.bBodyType = pSoldier.ubBodyType;
                            curr.pDetailedPlacement.ubCivilianGroup = pSoldier.ubCivilianGroup;
                            curr.pDetailedPlacement.ubScheduleID = pSoldier.ubScheduleID;
                            curr.pDetailedPlacement.fHasKeys = pSoldier.bHasKeys;
                            curr.pDetailedPlacement.sSectorX = pSoldier.sSectorX;
                            curr.pDetailedPlacement.sSectorY = pSoldier.sSectorY;
                            curr.pDetailedPlacement.bSectorZ = pSoldier.bSectorZ;
                            curr.pDetailedPlacement.ubSoldierClass = pSoldier.ubSoldierClass;
                            curr.pDetailedPlacement.bTeam = pSoldier.bTeam;
                            curr.pDetailedPlacement.bDirection = pSoldier.bDirection;

                            //we don't want the player to think that all the enemies start in the exact position when we 
                            //left the map, so randomize the start locations either current position or original position.
                            if (PreRandom(2) > 0)
                            { //use current position
                                curr.pDetailedPlacement.fOnRoof = pSoldier.bLevel;
                                curr.pDetailedPlacement.sInsertionGridNo = pSoldier.sGridNo;
                            }
                            else
                            { //use original position
                                curr.pDetailedPlacement.fOnRoof = curr.pBasicPlacement.fOnRoof;
                                curr.pDetailedPlacement.sInsertionGridNo = curr.pBasicPlacement.usStartingGridNo;
                            }

                            wprintf(curr.pDetailedPlacement.name, pSoldier.name);

                            //Copy patrol points
                            curr.pDetailedPlacement.bPatrolCnt = pSoldier.bPatrolCnt;
                            curr.pDetailedPlacement.sPatrolGrid = pSoldier.usPatrolGrid;

                            //copy colors for soldier based on the body type.
                            sprintf(curr.pDetailedPlacement.HeadPal, pSoldier.HeadPal);
                            sprintf(curr.pDetailedPlacement.VestPal, pSoldier.VestPal);
                            sprintf(curr.pDetailedPlacement.SkinPal, pSoldier.SkinPal);
                            sprintf(curr.pDetailedPlacement.PantsPal, pSoldier.PantsPal);
                            sprintf(curr.pDetailedPlacement.MiscPal, pSoldier.MiscPal);

                            //copy soldier's inventory
                            curr.pDetailedPlacement.Inv = pSoldier.inv.Values.ToArray();
                        }
                    }

                    //DONE, now increment the counter, so we know how many there are.
                    slots++;
                }
            }
        }

        if (slots == 0)
        {
            if (fEnemy)
            {
                //No need to save anything, so return successfully
                RemoveEnemySoldierTempFile(sSectorX, sSectorY, bSectorZ);
                return true;
            }
            else
            {
                //No need to save anything, so return successfully
                RemoveCivilianTempFile(sSectorX, sSectorY, bSectorZ);
                return (true);
            }

        }

        if (fValidateOnly)
        {
            return (true);
        }

        //STEP TWO:  Set up the temp file to write to.

        //Convert the current sector location into a file name
        //GetMapFileName( sSectorX, sSectorY, bSectorZ, zTempName, false );

        if (fEnemy)
        {
            //add the 'e' for 'Enemy preserved' to the front of the map name
            //sprintf( zMapName, "%s\\e_%s", MAPS_DIR, zTempName);
            TacticalSaveSubSystem.GetMapTempFileName(SF.ENEMY_PRESERVED_TEMP_FILE_EXISTS, zMapName, sSectorX, sSectorY, bSectorZ);
        }
        else
        {
            //add the 'e' for 'Enemy preserved' to the front of the map name
            //sprintf( zMapName, "%s\\c_%s", MAPS_DIR, zTempName);
            TacticalSaveSubSystem.GetMapTempFileName(SF.CIV_PRESERVED_TEMP_FILE_EXISTS, zMapName, sSectorX, sSectorY, bSectorZ);
        }

        //Open the file for writing, Create it if it doesnt exist
        hfile = files.FileOpen(zMapName, FileAccess.Write, false);
        if (!hfile.CanWrite)
        {   //Error opening map modification file
            return false;
        }

        files.FileWrite(hfile, sSectorY, 2, out int uiNumBytesWritten);
        if (uiNumBytesWritten != 2)
        {
            goto FAIL_SAVE;
        }

        //STEP THREE:  Save the data

        //this works for both civs and enemies
//        SaveSoldierInitListLinks(hfile);

        files.FileWrite(hfile, sSectorX, 2, out uiNumBytesWritten);
        if (uiNumBytesWritten != 2)
        {
            goto FAIL_SAVE;
        }

        //This check may appear confusing.  It is intended to abort if the player is saving the game.  It is only 
        //supposed to preserve the links to the placement list, so when we finally do leave the level with enemies remaining,
        //we will need the links that are only added when the map is loaded, and are normally lost when restoring a save.
        if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.LOADING_SAVED_GAME))
        {
            slots = 0;
        }

        files.FileWrite(hfile, slots, 4, out uiNumBytesWritten);
        if (uiNumBytesWritten != 4)
        {
            goto FAIL_SAVE;
        }

        uiTimeStamp = GameClock.GetWorldTotalMin();
        files.FileWrite(hfile, uiTimeStamp, 4, out uiNumBytesWritten);
        if (uiNumBytesWritten != 4)
        {
            goto FAIL_SAVE;
        }

        files.FileWrite(hfile, bSectorZ, 1, out uiNumBytesWritten);
        if (uiNumBytesWritten != 1)
        {
            goto FAIL_SAVE;
        }

        if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.LOADING_SAVED_GAME))
        {
            //if we are saving the game, we don't need to preserve the soldier information, just 
            //preserve the links to the placement list.
            slots = 0;
            files.FileClose(hfile);

            if (fEnemy)
            {
                TacticalSaveSubSystem.SetSectorFlag(sSectorX, sSectorY, bSectorZ, SF.ENEMY_PRESERVED_TEMP_FILE_EXISTS);
            }
            else
            {
                TacticalSaveSubSystem.SetSectorFlag(sSectorX, sSectorY, bSectorZ, SF.CIV_PRESERVED_TEMP_FILE_EXISTS);
            }
            return true;
        }

        for (i = gTacticalStatus.Team[ubStartID].bFirstID; i <= gTacticalStatus.Team[ubEndID].bLastID; i++)
        {
            pSoldier = MercPtrs[i];
            // CJC: note that bInSector is not required; the civ could be offmap!
            if (pSoldier.IsActive /*&& pSoldier.bInSector*/ && pSoldier.bLife > 0)
            {
                //soldier is valid, so find the matching soldier init list entry for modification.
                curr = gSoldierInitHead;
                while (curr is not null && curr.pSoldier != pSoldier)
                {
                    curr = curr.next;
                }
                if (curr is not null && curr.pSoldier == pSoldier && pSoldier.ubProfile == NO_PROFILE)
                {
                    //found a match.  
                    files.FileWrite(hfile, curr.pDetailedPlacement, Marshal.SizeOf<SOLDIERCREATE_STRUCT>(), out uiNumBytesWritten);
                    if (uiNumBytesWritten != Marshal.SizeOf<SOLDIERCREATE_STRUCT>())
                    {
                        goto FAIL_SAVE;
                    }
                    //insert a checksum equation (anti-hack)
                    usCheckSum =
                        curr.pDetailedPlacement.bLife * 7 +
                        curr.pDetailedPlacement.bLifeMax * 8 -
                        curr.pDetailedPlacement.bAgility * 2 +
                        curr.pDetailedPlacement.bDexterity * 1 +
                        curr.pDetailedPlacement.bExpLevel * 5 -
                        curr.pDetailedPlacement.bMarksmanship * 9 +
                        curr.pDetailedPlacement.bMedical * 10 +
                        curr.pDetailedPlacement.bMechanical * 3 +
                        curr.pDetailedPlacement.bExplosive * 4 +
                        curr.pDetailedPlacement.bLeadership * 5 +
                        curr.pDetailedPlacement.bStrength * 7 +
                        curr.pDetailedPlacement.bWisdom * 11 +
                        curr.pDetailedPlacement.bMorale * 7 +
                        (int)curr.pDetailedPlacement.bAIMorale * 3 -
                        (int)curr.pDetailedPlacement.bBodyType * 7 +
                        4 * 6 +
                        curr.pDetailedPlacement.sSectorX * 7 -
                        (int)curr.pDetailedPlacement.ubSoldierClass * 4 +
                        (int)curr.pDetailedPlacement.bTeam * 7 +
                        (int)curr.pDetailedPlacement.bDirection * 5 +
                        curr.pDetailedPlacement.fOnRoof * 17 +
                        curr.pDetailedPlacement.sInsertionGridNo * 1 +
                        3;

                    files.FileWrite(hfile, usCheckSum, 2, out uiNumBytesWritten);
                    if (uiNumBytesWritten != 2)
                    {
                        goto FAIL_SAVE;
                    }
                }
            }
        }

        ubSectorID = SECTORINFO.SECTOR(sSectorX, sSectorY);
        files.FileWrite(hfile, ubSectorID, 1, out uiNumBytesWritten);
        if (uiNumBytesWritten != 1)
        {
            goto FAIL_SAVE;
        }

        files.FileClose(hfile);

        if (fEnemy)
        {
            TacticalSaveSubSystem.SetSectorFlag(sSectorX, sSectorY, bSectorZ, SF.ENEMY_PRESERVED_TEMP_FILE_EXISTS);
        }
        else
        {
            TacticalSaveSubSystem.SetSectorFlag(sSectorX, sSectorY, bSectorZ, SF.CIV_PRESERVED_TEMP_FILE_EXISTS);
        }

        return true;

    FAIL_SAVE:
        files.FileClose(hfile);
        return false;
    }

    public static void RemoveEnemySoldierTempFile(int sSectorX, MAP_ROW sSectorY, int bSectorZ)
    {
        string zMapName = string.Empty;// [128];
        if (TacticalSaveSubSystem.GetSectorFlagStatus(sSectorX, sSectorY, bSectorZ, SF.ENEMY_PRESERVED_TEMP_FILE_EXISTS))
        {
            //Delete any temp file that is here and toast the flag that say's one exists.
            TacticalSaveSubSystem.ReSetSectorFlag(sSectorX, sSectorY, bSectorZ, SF.ENEMY_PRESERVED_TEMP_FILE_EXISTS);

            //		GetMapFileName( gWorldSectorX, gWorldSectorY, gbWorldSectorZ, zTempName, FALSE );
            //add the 'e' for 'Enemy preserved' to the front of the map name
            //		sprintf( zMapName, "%s\\e_%s", MAPS_DIR, zTempName);

            TacticalSaveSubSystem.GetMapTempFileName(SF.ENEMY_PRESERVED_TEMP_FILE_EXISTS, zMapName, sSectorX, sSectorY, bSectorZ);

            //Delete the temp file.
            files.FileDelete(zMapName);
        }
    }

    public static void RemoveCivilianTempFile(int sSectorX, MAP_ROW sSectorY, int bSectorZ)
    {
        //CHAR8		zTempName[ 128 ];
        string zMapName = string.Empty;// [128];
        if (TacticalSaveSubSystem.GetSectorFlagStatus(sSectorX, sSectorY, bSectorZ, SF.CIV_PRESERVED_TEMP_FILE_EXISTS))
        {
            //Delete any temp file that is here and toast the flag that say's one exists.
            TacticalSaveSubSystem.ReSetSectorFlag(sSectorX, sSectorY, bSectorZ, SF.CIV_PRESERVED_TEMP_FILE_EXISTS);
            //GetMapFileName( gWorldSectorX, gWorldSectorY, gbWorldSectorZ, zTempName, FALSE );

            TacticalSaveSubSystem.GetMapTempFileName(SF.CIV_PRESERVED_TEMP_FILE_EXISTS, zMapName, sSectorX, sSectorY, bSectorZ);

            //Delete the temp file.
            files.FileDelete(zMapName);
        }
    }
}

