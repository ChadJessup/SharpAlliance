using Microsoft.Extensions.Logging;

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

    public static void AddRemoveObjectToMapTempFile(int uiMapIndex, int usIndex)
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

        SaveModifiedMapStructToMapTempFile(out Map, Globals.gWorldSectorX, Globals.gWorldSectorY, Globals.gbWorldSectorZ);
    }
}

public struct MODIFY_MAP
{
    public int usGridNo;                //The gridno the graphic will be applied to
    public int usImageType;         //graphic index
    public int usSubImageIndex;     //
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
