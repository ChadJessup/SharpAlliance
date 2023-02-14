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
        TileTypeDefines uiType;
        ushort? usSubIndex;

        if (!gfApplyChangesToTempFile)
        {
            return;
        }

        if (this.overhead.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.LOADING_SAVED_GAME))
        {
            return;
        }

        TileDefine.GetTileType(usIndex, out uiType);
        TileDefine.GetSubIndexFromTileIndex(usIndex, out usSubIndex);

        //memset(&Map, 0, sizeof(MODIFY_MAP));

        Map = new MODIFY_MAP
        {
            usGridNo = (ushort)uiMapIndex,
            //	Map.usIndex		= usIndex;
            usImageType = (ushort)uiType,
            usSubImageIndex = usSubIndex,

            ubType = SLM_REMOVE_OBJECT
        };

        SaveModifiedMapStructToMapTempFile(out Map, gWorldSectorX, gWorldSectorY, gbWorldSectorZ);
    }
}

public struct MODIFY_MAP
{
    public ushort usGridNo;                //The gridno the graphic will be applied to
    public ushort usImageType;         //graphic index
    public ushort usSubImageIndex;     //
                                //	ushort	usIndex;
    public byte ubType;                       // the layer it will be applied to

    public byte ubExtra;					// Misc. variable used to strore arbritary values
}
