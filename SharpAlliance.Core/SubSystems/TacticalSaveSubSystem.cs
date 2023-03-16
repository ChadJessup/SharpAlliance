using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;
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
