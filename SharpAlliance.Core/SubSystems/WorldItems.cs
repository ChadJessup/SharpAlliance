using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SharpAlliance.Core.SubSystems;

public class WorldItems
{
}

public struct WORLDITEM
{
    public bool fExists;
    public int sGridNo;
    public int ubLevel;
    public OBJECTTYPE o;
    public int usFlags;
    public int bRenderZHeightAboveLevel;
    public int bVisible;

    //This is the chance associated with an item or a trap not-existing in the world.  The reason why 
    //this is reversed (10 meaning item has 90% chance of appearing, is because the order that the map 
    //is saved, we don't know if the version is older or not until after the items are loaded and added.
    //Because this value is zero in the saved maps, we can't change it to 100, hence the reversal method.
    //This check is only performed the first time a map is loaded.  Later, it is entirely skipped.
    public int ubNonExistChance;
}

public struct WORLDBOMB
{
    public bool fExists;
    public int iItemIndex;
}

