using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class MercHiring
{
}

public class MERC_HIRE_STRUCT
{
    public NPCID  ubProfileID;
    public int  sSectorX;
    public MAP_ROW sSectorY;
    public int bSectorZ;
    public int  iTotalContractLength;
    public bool fCopyProfileItemsOver;
    public int  uiTimeTillMercArrives;
    public INSERTION_CODE ubInsertionCode;
    public int  usInsertionData;
    public bool fUseLandingZoneForArrival;
}

