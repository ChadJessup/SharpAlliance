using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Screens;

namespace SharpAlliance.Core.SubSystems;

public class StrategicMines
{
}

public enum MINE
{
    SAN_MONA = 0,
    DRASSEN,
    ALMA,
    CAMBRIA,
    CHITZENA,
    GRUMM,

    MAX_NUMBER_OF_MINES,
}

public enum MINER
{
    FRED = 0,
    MATT,
    OSWALD,
    CALVIN,
    CARL,
    NUM_HEAD_MINERS,
}

// the strategic mine structures
public struct MINE_LOCATION_TYPE
{
    public MINE_LOCATION_TYPE(int sectorX, int sectorY, TOWNS town)
    {
        this.sSectorX = sectorX;
        this.sSectorY = sectorY;
        this.bAssociatedTown = town;
    }

    public int sSectorX;                     // x value of sector mine is in
    public int sSectorY;                     // y value of sector mine is in
    public TOWNS bAssociatedTown;			// associated town of this mine
}
