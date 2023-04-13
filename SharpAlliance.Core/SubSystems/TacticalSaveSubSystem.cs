using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class TacticalSaveSubSystem : IDisposable
{
    private readonly ILogger<TacticalSaveSubSystem> logger;

    public TacticalSaveSubSystem(ILogger<TacticalSaveSubSystem> logger)
    {
        this.logger = logger;
    }

    public void Dispose()
    {
    }

    public static bool ReSetSectorFlag(int sMapX, MAP_ROW sMapY, int bMapZ, SF uiFlagToSet)
    {
        if (bMapZ == 0)
        {
            SectorInfo[SECTORINFO.SECTOR(sMapX, sMapY)].uiFlags &= ~(uiFlagToSet);
        }
        else
        {
            ReSetUnderGroundSectorFlag(sMapX, sMapY, bMapZ, uiFlagToSet);
        }

        return (true);
    }

    public static bool SaveCurrentSectorsInformationToTempItemFile()
    {
        bool fShouldBeInMeanwhile = false;
        if (gfWasInMeanwhile)
        { //Don't save a temp file for the meanwhile scene map.
            gfWasInMeanwhile = false;
            return true;
        }
        else if (Meanwhile.AreInMeanwhile())
        {
            gfInMeanwhile = false;
            fShouldBeInMeanwhile = true;
        }

        //If we havent been to tactical yet
        if ((gWorldSectorX == 0) && (gWorldSectorY == 0))
        {
            return (true);
        }


        //Save the Blood, smell and the revealed status for map elements
//        SaveBloodSmellAndRevealedStatesFromMapToTempFile();

        // handle all reachable before save
        HandleAllReachAbleItemsInTheSector(gWorldSectorX, gWorldSectorY, gbWorldSectorZ);

        //Save the Items to the the file
//        if (!SaveWorldItemsToTempItemFile(gWorldSectorX, gWorldSectorY, gbWorldSectorZ, guiNumWorldItems, gWorldItems))
//        {
//            //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, "SaveCurrentSectorsInformationToTempItemFile:  failed in SaveWorldItemsToTempItemFile()");
//            return (false);
//        }

        //Save the rotting corpse array to the temp rotting corpse file
//        if (!SaveRottingCorpsesToTempCorpseFile(gWorldSectorX, gWorldSectorY, gbWorldSectorZ))
//        {
////            DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("SaveCurrentSectorsInformationToTempItemFile:  failed in SaveRottingCorpsesToTempCorpseFile()"));
//            return (false);
//        }

        //save the Doortable array to the temp door map file 
//        if (!SaveDoorTableToDoorTableTempFile(gWorldSectorX, gWorldSectorY, gbWorldSectorZ))
//        {
////            DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("SaveCurrentSectorsInformationToTempItemFile:  failed in SaveDoorTableToDoorTableTempFile()"));
//            return (false);
//        }

        //save the 'revealed'status of the tiles
//        if (!SaveRevealedStatusArrayToRevealedTempFile(gWorldSectorX, gWorldSectorY, gbWorldSectorZ))
//        {
////            DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("SaveCurrentSectorsInformationToTempItemFile:  failed in SaveRevealedStatusArrayToRevealedTempFile()"));
//            return (false);
//        }

        //save the door open status to the saved game file
//        if (!SaveDoorStatusArrayToDoorStatusTempFile(gWorldSectorX, gWorldSectorY, gbWorldSectorZ))
//        {
////            DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("SaveCurrentSectorsInformationToTempItemFile:  failed in SaveDoorStatusArrayToDoorStatusTempFile()"));
//            return (false);
//        }

        //Save the enemies to the temp file
//        if (!NewWayOfSavingEnemyAndCivliansToTempFile(gWorldSectorX, gWorldSectorY, gbWorldSectorZ, true, false))
//        {
////            DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("SaveCurrentSectorsInformationToTempItemFile:  failed in NewWayOfSavingEnemyAndCivliansToTempFile( Enemy, Creature Team )"));
//            return (false);
//        }

        //Save the civilian info to the temp file
//        if (!NewWayOfSavingEnemyAndCivliansToTempFile(gWorldSectorX, gWorldSectorY, gbWorldSectorZ, false, false))
//        {
////            DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("SaveCurrentSectorsInformationToTempItemFile:  failed in NewWayOfSavingEnemyAndCivliansToTempFile( Civ Team )"));
//            return (false);
//        }

        //Save the smoke effects info to the temp file
        if (!SmokeEffects.SaveSmokeEffectsToMapTempFile(gWorldSectorX, gWorldSectorY, gbWorldSectorZ))
        {
//            DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("SaveCurrentSectorsInformationToTempItemFile:  failed in SaveSmokeEffectsToMapTempFile"));
            return (false);
        }

        //Save the smoke effects info to the temp file
        if (!LightEffects.SaveLightEffectsToMapTempFile(gWorldSectorX, gWorldSectorY, gbWorldSectorZ))
        {
//            DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("SaveCurrentSectorsInformationToTempItemFile:  failed in SaveLightEffectsToMapTempFile"));
            return (false);
        }


        //ttt

        //Save any other info here


        //Save certain information from the NPC's soldier structure to the Merc structure
        SaveNPCInformationToProfileStruct();

        //Save the time the player was last in the sector
        SetLastTimePlayerWasInSector();


        if (fShouldBeInMeanwhile)
        {
            gfInMeanwhile = true;
        }

        return (true);
    }

    private static bool HandleAllReachAbleItemsInTheSector(int secX, MAP_ROW secY, int secZ)
    {
        return false;
    }

    private static void SaveNPCInformationToProfileStruct()
    {

    }

    private static void SetLastTimePlayerWasInSector()
    {
        if (gbWorldSectorZ == 0)
        {
            SectorInfo[SECTORINFO.SECTOR(gWorldSectorX, gWorldSectorY)].uiTimeCurrentSectorWasLastLoaded = GameClock.GetWorldTotalMin();
        }
        else if (gbWorldSectorZ > 0)
        {
            UNDERGROUND_SECTORINFO? pTempNode = gpUndergroundSectorInfoHead;

            pTempNode = gpUndergroundSectorInfoHead;

            //loop through and look for the right underground sector
            while (pTempNode is not null)
            {
                if ((pTempNode.ubSectorX == gWorldSectorX)
                    && (pTempNode.ubSectorY == gWorldSectorY)
                    && (pTempNode.ubSectorZ == gbWorldSectorZ))
                {
                    //set the flag indicating that ther is a temp item file exists for the sector
                    pTempNode.uiTimeCurrentSectorWasLastLoaded = GameClock.GetWorldTotalMin();
                    return; //break out
                }
                pTempNode = pTempNode.next;
            }
        }
    }

    public static bool ReSetUnderGroundSectorFlag(int sSectorX, MAP_ROW sSectorY, int ubSectorZ, SF uiFlagToSet)
    {
        UNDERGROUND_SECTORINFO? pTempNode = gpUndergroundSectorInfoHead;

        pTempNode = gpUndergroundSectorInfoHead;

        //loop through and look for the right underground sector
        while (pTempNode is not null)
        {
            if ((pTempNode.ubSectorX == sSectorX)
                && (pTempNode.ubSectorY == sSectorY)
                && (pTempNode.ubSectorZ == ubSectorZ))
            {
                //set the flag indicating that ther is a temp item file exists for the sector
                pTempNode.uiFlags &= ~(uiFlagToSet);

                return (true);
            }
            pTempNode = pTempNode.next;
        }

        return (false);
    }

    public static bool SetSectorFlag(int sMapX, MAP_ROW sMapY, int bMapZ, SF uiFlagToSet)
    {
        if (uiFlagToSet == SF.ALREADY_VISITED)
        {
            // do certain things when particular sectors are visited
            if ((sMapX == TIXA_SECTOR_X) && (sMapY == TIXA_SECTOR_Y))
            {
                // Tixa prison (not seen until Tixa visited)
                SectorInfo[SEC.J9].uiFacilitiesFlags |= SFCF.PRISON;
            }

            if ((sMapX == GUN_RANGE.X) && (sMapY == GUN_RANGE.Y) && (bMapZ == GUN_RANGE.Z))
            {
                // Alma shooting range (not seen until sector visited)
                SectorInfo[SEC.H13].uiFacilitiesFlags |= SFCF.GUN_RANGE;
                SectorInfo[SEC.H14].uiFacilitiesFlags |= SFCF.GUN_RANGE;
                SectorInfo[SEC.I13].uiFacilitiesFlags |= SFCF.GUN_RANGE;
                SectorInfo[SEC.I14].uiFacilitiesFlags |= SFCF.GUN_RANGE;
            }

            if (!GetSectorFlagStatus(sMapX, sMapY, bMapZ, SF.ALREADY_VISITED))
            {
                // increment daily counter of sectors visited
                gStrategicStatus.ubNumNewSectorsVisitedToday++;
                if (gStrategicStatus.ubNumNewSectorsVisitedToday == NEW_SECTORS_EQUAL_TO_ACTIVITY)
                {
                    // visited enough to count as an active day
                    StrategicStatus.UpdateLastDayOfPlayerActivity(GameClock.GetWorldDay());
                }
            }
        }

        if (bMapZ == 0)
        {
            SectorInfo[SECTORINFO.SECTOR(sMapX, sMapY)].uiFlags |= uiFlagToSet;
        }
        else
        {
            TacticalSaveSubSystem.SetUnderGroundSectorFlag(sMapX, sMapY, bMapZ, uiFlagToSet);
        }

        return (true);
    }

    public static bool SetUnderGroundSectorFlag(int sSectorX, MAP_ROW sSectorY, int ubSectorZ, SF uiFlagToSet)
    {
        UNDERGROUND_SECTORINFO? pTempNode = gpUndergroundSectorInfoHead;

        pTempNode = gpUndergroundSectorInfoHead;

        //loop through and look for the right underground sector
        while (pTempNode is not null)
        {
            if ((pTempNode.ubSectorX == sSectorX)
                && (pTempNode.ubSectorY == sSectorY)
                && (pTempNode.ubSectorZ == ubSectorZ))
            {
                //set the flag indicating that ther is a temp item file exists for the sector
                pTempNode.uiFlags |= uiFlagToSet;

                return (true);
            }
            pTempNode = pTempNode.next;
        }

        return (false);
    }

    public static bool GetSectorFlagStatus(int sMapX, MAP_ROW sMapY, int bMapZ, SF uiFlagToSet)
    {
        if (bMapZ == 0)
        {
            return ((SectorInfo[SECTORINFO.SECTOR(sMapX, sMapY)].uiFlags.HasFlag(uiFlagToSet)));
        }
        else
        {
            return ((GetUnderGroundSectorFlagStatus(sMapX, sMapY, bMapZ, uiFlagToSet)));
        }
    }

    public static bool GetUnderGroundSectorFlagStatus(int sSectorX, MAP_ROW sSectorY, int ubSectorZ, SF uiFlagToCheck)
    {
        UNDERGROUND_SECTORINFO? pTempNode = gpUndergroundSectorInfoHead;

        pTempNode = gpUndergroundSectorInfoHead;

        //loop through and look for the right underground sector
        while (pTempNode is not null)
        {
            if ((pTempNode.ubSectorX == sSectorX)
                && (pTempNode.ubSectorY == sSectorY)
                && (pTempNode.ubSectorZ == ubSectorZ))
            {
                //set the flag indicating that ther is a temp item file exists for the sector
                if (pTempNode.uiFlags.HasFlag(uiFlagToCheck))
                {
                    return (true);
                }
                else
                {
                    return (false);
                }
            }

            pTempNode = pTempNode.next;
        }

        return (false);
    }

    public static void GetMapTempFileName(SF uiType, string pMapName, int sMapX, MAP_ROW sMapY, int bMapZ)
    {
        string zTempName;

        //Convert the current sector location into a file name
        StrategicMap.GetMapFileName(sMapX, sMapY, bMapZ, out zTempName, false, false);

        switch (uiType)
        {
            case SF.ITEM_TEMP_FILE_EXISTS:
                pMapName = sprintf("%s\\i_%s", MAPS_DIR, zTempName);
                break;

            case SF.ROTTING_CORPSE_TEMP_FILE_EXISTS:
                pMapName = sprintf("%s\\r_%s", MAPS_DIR, zTempName);
                break;

            case SF.MAP_MODIFICATIONS_TEMP_FILE_EXISTS:
                pMapName = sprintf("%s\\m_%s", MAPS_DIR, zTempName);
                break;

            case SF.DOOR_TABLE_TEMP_FILES_EXISTS:
                pMapName = sprintf("%s\\d_%s", MAPS_DIR, zTempName);
                break;

            case SF.REVEALED_STATUS_TEMP_FILE_EXISTS:
                pMapName = sprintf("%s\\v_%s", MAPS_DIR, zTempName);
                break;

            case SF.DOOR_STATUS_TEMP_FILE_EXISTS:
                pMapName = sprintf("%s\\ds_%s", MAPS_DIR, zTempName);
                break;

            case SF.ENEMY_PRESERVED_TEMP_FILE_EXISTS:
                pMapName = sprintf("%s\\e_%s", MAPS_DIR, zTempName);
                break;

            case SF.CIV_PRESERVED_TEMP_FILE_EXISTS:
                // NB save game version 0 is "saving game"
                if ((gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.LOADING_SAVED_GAME))
                    && guiSaveGameVersion != 0 && guiSaveGameVersion < 78)
                {
                    pMapName = sprintf("%s\\c_%s", MAPS_DIR, zTempName);
                }
                else
                {
                    pMapName = sprintf("%s\\cc_%s", MAPS_DIR, zTempName);
                }
                break;

            case SF.SMOKE_EFFECTS_TEMP_FILE_EXISTS:
                pMapName = sprintf("%s\\sm_%s", MAPS_DIR, zTempName);
                break;

            case SF.LIGHTING_EFFECTS_TEMP_FILE_EXISTS:
                pMapName = sprintf("%s\\l_%s", MAPS_DIR, zTempName);
                break;

            default:
                Debug.Assert(false);
                break;
        }
    }

    internal void InitTacticalSave(bool fCreateTempDir)
    {
    }
}
