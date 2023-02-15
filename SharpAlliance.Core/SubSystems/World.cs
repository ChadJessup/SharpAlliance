using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SharpAlliance.Core.SubSystems
{
    public class World
    {
        public const int LANDHEAD = 0;
        public const int MAXDIR = 8;
        public const int WORLD_TILE_X = 40;
        public const int WORLD_TILE_Y = 20;
        public const int WORLD_COLS = 160;
        public const int WORLD_ROWS = 160;
        public const int WORLD_COORD_COLS = 1600;
        public const int WORLD_COORD_ROWS = 1600;
        public const int WORLD_MAX = 25600;

        private readonly ILogger<World> logger;
        private readonly Globals globals;
        private readonly IsometricUtils isometricUtils;

        public World(
            ILogger<World> logger,
            Globals globals,
            IsometricUtils isometricUtils)
        {
            this.logger = logger;
            this.globals = globals;
            this.isometricUtils = isometricUtils;
        }

        public int gsRecompileAreaTop { get; set; } = 0;
        public int gsRecompileAreaLeft { get; set; } = 0;
        public int gsRecompileAreaRight { get; set; } = 0;
        public int gsRecompileAreaBottom { get; set; } = 0;

        public void AddTileToRecompileArea(int sGridNo)
        {
            int sCheckGridNo;
            int sCheckX;
            int sCheckY;

            // Set flag to wipe and recompile MPs in this tile
            if (sGridNo < 0 || sGridNo >= WORLD_MAX)
            {
                return;
            }

            this.globals.gpWorldLevelData[sGridNo].ubExtFlags[0] |= MAPELEMENT_EXT_RECALCULATE_MOVEMENT;

            // check Top/Left of recompile region
            sCheckGridNo = this.isometricUtils.NewGridNo(sGridNo, this.isometricUtils.DirectionInc((int)WorldDirections.NORTHWEST));
            sCheckX = sCheckGridNo % WORLD_COLS;
            sCheckY = sCheckGridNo / WORLD_COLS;
            if (sCheckX < gsRecompileAreaLeft)
            {
                gsRecompileAreaLeft = sCheckX;
            }
            if (sCheckY < gsRecompileAreaTop)
            {
                gsRecompileAreaTop = sCheckY;
            }

            // check Bottom/Right
            sCheckGridNo = this.isometricUtils.NewGridNo(sGridNo, this.isometricUtils.DirectionInc((int)WorldDirections.SOUTHEAST));
            sCheckX = sCheckGridNo % WORLD_COLS;
            sCheckY = sCheckGridNo / WORLD_COLS;
            if (sCheckX > gsRecompileAreaRight)
            {
                gsRecompileAreaRight = sCheckX;
            }
            if (sCheckY > gsRecompileAreaBottom)
            {
                gsRecompileAreaBottom = sCheckY;
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
}
