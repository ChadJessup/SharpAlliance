using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SharpAlliance.Core.SubSystems;

public class MercHiring
{
}

public class MERC_HIRE_STRUCT
{
    public int  ubProfileID;
    public int  sSectorX;
    public int  sSectorY;
    public int bSectorZ;
    public int  iTotalContractLength;
    public bool fCopyProfileItemsOver;
    public int  uiTimeTillMercArrives;
    public INSERTION_CODE ubInsertionCode;
    public int  usInsertionData;
    public bool fUseLandingZoneForArrival;
}

