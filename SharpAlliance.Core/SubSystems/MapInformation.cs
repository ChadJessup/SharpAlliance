using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core.SubSystems;

public class MapInformation
{

}

public struct MAPCREATE_STRUCT
{
    //These are the mandatory entry points for a map.  If any of the values are -1, then that means that
    //the point has been specifically not used and that the map is not traversable to or from an adjacent 
    //sector in that direction.  The >0 value points must be validated before saving the map.  This is 
    //done by simply checking if those points are sittable by mercs, and that you can plot a path from 
    //these points to each other.  These values can only be set by the editor : mapinfo tab
    public int sNorthGridNo;
    public int sEastGridNo;
    public int sSouthGridNo;
    public int sWestGridNo;
    //This contains the number of individuals in the map.
    //Individuals include NPCs, enemy placements, creatures, civilians, rebels, and animals.
    public int  ubNumIndividuals;
    public int  ubMapVersion;
    public int  ubRestrictedScrollID;
    public int  ubEditorSmoothingType;  //normal, basement, or caves
    public int  sCenterGridNo;
    public int  sIsolatedGridNo;
    public int[] bPadding;// = new int[83];	//I'm sure lots of map info will be added
} //99 bytes

