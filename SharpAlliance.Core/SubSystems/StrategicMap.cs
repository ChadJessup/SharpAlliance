using System;
using System.Threading.Tasks;
using SharpAlliance.Core.Screens;

namespace SharpAlliance.Core.SubSystems;

public class StrategicMap
{
    public ValueTask<bool> InitStrategicMovementCosts()
    {
        return ValueTask.FromResult(true);
    }

    public ValueTask<bool> InitStrategicEngine()
    {
        return ValueTask.FromResult(true);
    }
}

public class StrategicMapElement
{
    public int [] UNUSEDuiFootEta = new int[4];          // eta/mvt costs for feet 
    public int [] UNUSEDuiVehicleEta = new int[4];       // eta/mvt costs for vehicles 
    public int [] uiBadFootSector = new int[4];    // blocking mvt for foot
    public int [] uiBadVehicleSector = new int[4]; // blocking mvt from vehicles
    public TOWNS bNameId;
    public bool fEnemyControlled;   // enemy controlled or not
    public bool fEnemyAirControlled;
    public bool UNUSEDfLostControlAtSomeTime;
    public int bSAMCondition; // SAM Condition .. 0 - 100, just like an item's status
    public int[] bPadding = new int[20];
}
