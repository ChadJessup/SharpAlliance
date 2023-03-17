using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class World
{
    private readonly ILogger<World> logger;

    public World(ILogger<World> logger)
    {
        this.logger = logger;
    }

    public static void AddTileToRecompileArea(int sGridNo)
    {
        int sCheckGridNo;
        int sCheckX;
        int sCheckY;

        // Set flag to wipe and recompile MPs in this tile
        if (sGridNo < 0 || sGridNo >= WORLD_MAX)
        {
            return;
        }

        Globals.gpWorldLevelData[sGridNo].ubExtFlags[0] |= MAPELEMENT_EXT.RECALCULATE_MOVEMENT;

        // check Top/Left of recompile region
        sCheckGridNo = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc((int)WorldDirections.NORTHWEST));
        sCheckX = sCheckGridNo % WORLD_COLS;
        sCheckY = sCheckGridNo / WORLD_COLS;
        if (sCheckX < Globals.gsRecompileAreaLeft)
        {
            Globals.gsRecompileAreaLeft = sCheckX;
        }
        if (sCheckY < Globals.gsRecompileAreaTop)
        {
            Globals.gsRecompileAreaTop = sCheckY;
        }

        // check Bottom/Right
        sCheckGridNo = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc((int)WorldDirections.SOUTHEAST));
        sCheckX = sCheckGridNo % WORLD_COLS;
        sCheckY = sCheckGridNo / WORLD_COLS;
        if (sCheckX > Globals.gsRecompileAreaRight)
        {
            Globals.gsRecompileAreaRight = sCheckX;
        }
        if (sCheckY > Globals.gsRecompileAreaBottom)
        {
            Globals.gsRecompileAreaBottom = sCheckY;
        }
    }

    public void TrashWorld()
    {
    }

    public ValueTask<bool> InitializeWorld()
    {
        return ValueTask.FromResult(true);
    }
}
