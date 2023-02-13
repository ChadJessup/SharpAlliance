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

    public bool gfApplyChangesToTempFile { get; set; } = false;

    public void AddRemoveObjectToMapTempFile(int uiMapIndex, ushort usIndex)
    {
        MODIFY_MAP Map;
        int uiType;
        ushort usSubIndex;

        if (!gfApplyChangesToTempFile)
        {
            return;
        }

        if (this.overhead.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.LOADING_SAVED_GAME))
        {
            return;
        }

        GetTileType(usIndex, out uiType);
        GetSubIndexFromTileIndex(usIndex, out usSubIndex);

        //memset(&Map, 0, sizeof(MODIFY_MAP));

        Map.usGridNo = (UINT16)uiMapIndex;
        //	Map.usIndex		= usIndex;
        Map.usImageType = (UINT16)uiType;
        Map.usSubImageIndex = usSubIndex;

        Map.ubType = SLM_REMOVE_OBJECT;

        SaveModifiedMapStructToMapTempFile(out Map, gWorldSectorX, gWorldSectorY, gbWorldSectorZ);
    }
}

struct MODIFY_MAP
{
    ushort usGridNo;                //The gridno the graphic will be applied to
    ushort usImageType;         //graphic index
    ushort usSubImageIndex;     //
                                //	UINT16	usIndex;
    byte ubType;                       // the layer it will be applied to

    byte ubExtra;					// Misc. variable used to strore arbritary values
}
