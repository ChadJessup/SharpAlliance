using System;
using System.IO;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Managers;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class SaveLoadMap
{
    private readonly ILogger<SaveLoadMap> logger;
    private readonly Overhead overhead;

    public SaveLoadMap(
        ILogger<SaveLoadMap> logger,
        Overhead overhead)
    {
        this.logger = logger;
        this.overhead = overhead;
    }

    public static void ApplyMapChangesToMapTempFile(bool fAddToMap)
    {
        gfApplyChangesToTempFile = fAddToMap;
    }

    public static void AddObjectToMapTempFile(int uiMapIndex, TileIndexes usIndex)
    {
        MODIFY_MAP Map;
        TileTypeDefines uiType;
        int? usSubIndex;

        if (!gfApplyChangesToTempFile)
        {
            return;
        }

        if (gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.LOADING_SAVED_GAME))
        {
            return;
        }

        TileDefine.GetTileType(usIndex, out uiType);
        TileDefine.GetSubIndexFromTileIndex(usIndex, out usSubIndex);

        Map = new()
        {
            usGridNo = uiMapIndex,
            //	usIndex		= usIndex,
            usImageType = uiType,
            usSubImageIndex = usSubIndex,

            ubType = SLM.OBJECT,
        };

        SaveModifiedMapStructToMapTempFile(Map, gWorldSectorX, gWorldSectorY, gbWorldSectorZ);
    }

    public static void AddWindowHitToMapTempFile(int uiMapIndex)
    {
        MODIFY_MAP Map = new()
        {
            usGridNo = uiMapIndex,
            ubType = SLM.WINDOW_HIT,
        };

        SaveModifiedMapStructToMapTempFile(Map, gWorldSectorX, gWorldSectorY, gbWorldSectorZ);
    }

    public static bool SaveModifiedMapStructToMapTempFile(MODIFY_MAP pMap, int sSectorX, MAP_ROW sSectorY, int bSectorZ)
    {
        string zMapName;
        Stream hFile;
        int uiNumBytesWritten;

        //Convert the current sector location into a file name
        //	GetMapFileName( sSectorX, sSectorY, bSectorZ, zTempName, FALSE );

        //add the 'm' for 'Modifed Map' to the front of the map name
        //	sprintf( zMapName, "%s\\m_%s", MAPS_DIR, zTempName);

        GetMapTempFileName(SF.MAP_MODIFICATIONS_TEMP_FILE_EXISTS, zMapName, sSectorX, sSectorY, bSectorZ);

        //Open the file for writing, Create it if it doesnt exist
        hFile = FileManager.FileOpen(zMapName, FILE_ACCESS_WRITE | FILE_OPEN_ALWAYS, FALSE);
        if (hFile == 0)
        {
            //Error opening map modification file
            return (false);
        }

        //Move to the end of the file
        FileSeek(hFile, 0, FILE_SEEK_FROM_END);


        FileWrite(hFile, pMap, sizeof(MODIFY_MAP), out uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(MODIFY_MAP))
        {
            //Error Writing size of array to disk
            FileClose(hFile);
            return (false);
        }

        FileClose(hFile);

        SetSectorFlag(sSectorX, sSectorY, bSectorZ, SF.MAP_MODIFICATIONS_TEMP_FILE_EXISTS);

        return (true);
    }

    public static void AddRemoveObjectToMapTempFile(int uiMapIndex, TileIndexes usIndex)
    {
        MODIFY_MAP Map;
        TileTypeDefines uiType;
        int? usSubIndex;

        if (!Globals.gfApplyChangesToTempFile)
        {
            return;
        }

        if (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.LOADING_SAVED_GAME))
        {
            return;
        }

        TileDefine.GetTileType(usIndex, out uiType);
        TileDefine.GetSubIndexFromTileIndex(usIndex, out usSubIndex);

        //memset(&Map, 0, sizeof(MODIFY_MAP));

        Map = new MODIFY_MAP
        {
            usGridNo = uiMapIndex,
            //	Map.usIndex		= usIndex;
            usImageType = (int)uiType,
            usSubImageIndex = (int)usSubIndex!,

            ubType = SLM.REMOVE_OBJECT
        };

        // SaveModifiedMapStructToMapTempFile(out Map, Globals.gWorldSectorX, Globals.gWorldSectorY, Globals.gbWorldSectorZ);
    }
}

public struct MODIFY_MAP
{
    public int usGridNo;                //The gridno the graphic will be applied to
    public TileTypeDefines usImageType;         //graphic index
    public int? usSubImageIndex;     //
                                     //	ushort	usIndex;
    public SLM ubType;                       // the layer it will be applied to

    public byte ubExtra;					// Misc. variable used to strore arbritary values
}

//Used for the ubType in the MODIFY_MAP struct  
public enum SLM
{
    NONE,

    //Adding a map graphic
    LAND,
    OBJECT,
    STRUCT,
    SHADOW,
    MERC,                                       //Should never be used
    ROOF,
    ONROOF,
    TOPMOST,                                //Should never be used

    // For Removing
    REMOVE_LAND,
    REMOVE_OBJECT,
    REMOVE_STRUCT,
    REMOVE_SHADOW,
    REMOVE_MERC,                                        //Should never be used
    REMOVE_ROOF,
    REMOVE_ONROOF,
    REMOVE_TOPMOST,                             //Should never be used

    //Smell, or Blood is used
    BLOOD_SMELL,

    // Damage a particular struct
    DAMAGED_STRUCT,

    //Exit Grids
    EXIT_GRIDS,

    // State of Openable structs
    OPENABLE_STRUCT,

    // Modify window graphic & structure 
    WINDOW_HIT,
}
