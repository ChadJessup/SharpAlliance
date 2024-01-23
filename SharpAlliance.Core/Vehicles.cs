using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class Vehicles
{
    public static void SetVehicleSectorValues(int iVehId, int ubSectorX, MAP_ROW ubSectorY)
    {
        pVehicleList[iVehId].sSectorX = ubSectorX;
        pVehicleList[iVehId].sSectorY = ubSectorY;

        gMercProfiles[pVehicleList[iVehId].ubProfileID].sSectorX = ubSectorX;
        gMercProfiles[pVehicleList[iVehId].ubProfileID].sSectorY = ubSectorY;

    }

    public static void InitVehicles()
    {
        GROUP? pGroup = null;

        for (int cnt = 0; cnt < MAX_VEHICLES; cnt++)
        {
            // create mvt groups
            gubVehicleMovementGroups[cnt] = StrategicMovement.CreateNewVehicleGroupDepartingFromSector(1, MAP_ROW.A, cnt);

            // Set persistent....
            pGroup = StrategicMovement.GetGroup(gubVehicleMovementGroups[cnt]);
            pGroup.fPersistant = true;
        }
    }

    internal static void ClearOutVehicleList()
    {
        // empty out the vehicle list
        if (pVehicleList.Any())
        {
            for (int iCounter = 0; iCounter < ubNumberOfVehicles; iCounter++)
            {
                // if there is a valid vehicle
                if (pVehicleList[iCounter].fValid)
                {
                    //if the vehicle has a valid path
                    if (pVehicleList[iCounter].pMercPath is not null)
                    {
                        //toast the vehicle path
                        pVehicleList[iCounter].pMercPath = StrategicPathing.ClearStrategicPathList(ref pVehicleList[iCounter].pMercPath, 0);
                    }
                }
            }

            MemFree(pVehicleList);
            pVehicleList.Clear();
            ubNumberOfVehicles = 0;
        }

        /*	
            // empty out the vehicle list
            if( pVehicleList )
            {
                MemFree( pVehicleList );
                pVehicleList = NULL;
                ubNumberOfVehicles = 0;
            }
        */
    }

    internal static bool IsThisVehicleAccessibleToSoldier(SOLDIERTYPE? pSoldier, int iId)
    {
        if (pSoldier == null)
        {
            return false;
        }

        if ((iId >= ubNumberOfVehicles) || (iId < 0))
        {
            return (false);
        }

        // now check if vehicle is valid
        if (pVehicleList[iId].fValid == false)
        {
            return (false);
        }

        // if the soldier or the vehicle is between sectors
        if (pSoldier.fBetweenSectors || pVehicleList[iId].fBetweenSectors)
        {
            return (false);
        }

        // any sector values off?
        if ((pSoldier.sSectorX != pVehicleList[iId].sSectorX) ||
                (pSoldier.sSectorY != pVehicleList[iId].sSectorY) ||
                (pSoldier.bSectorZ != pVehicleList[iId].sSectorZ))
        {
            return (false);
        }

        // if vehicle is not ok to use then return false
        if (!Vehicles.OKUseVehicle(pVehicleList[iId].ubProfileID))
        {
            return (false);
        }

        return (true);
    }

    private static bool OKUseVehicle(NPCID ubProfile)
    {
        if (ubProfile == NPCID.PROF_HUMMER)
        {
            return (Facts.CheckFact(FACT.OK_USE_HUMMER, NO_PROFILE));
        }
        else if (ubProfile == NPCID.PROF_ICECREAM)
        {
            return (Facts.CheckFact(FACT.OK_USE_ICECREAM, NO_PROFILE));
        }
        else if (ubProfile == NPCID.PROF_HELICOPTER)
        {
            // don't allow mercs to get inside vehicle if it's grounded (enemy controlled, Skyrider owed money, etc.)
            return (MapScreenHelicopter.CanHelicopterFly());
        }
        else
        {
            return (true);
        }
    }

    internal static bool IsEnoughSpaceInVehicle(int iID)
    {
        // find if vehicle is valid
        if (VehicleIdIsValid(iID) == false)
        {
            return (false);
        }

        if (GetNumberInVehicle(iID) == iSeatingCapacities[pVehicleList[iID].ubVehicleType])
        {
            return (false);
        }

        return (true);
    }

    private static bool VehicleIdIsValid(int iID)
    {
        throw new NotImplementedException();
    }

    private static int GetNumberInVehicle(int iID)
    {
        throw new NotImplementedException();
    }

    internal static void PutSoldierInVehicle(SOLDIERTYPE? pSoldier, int iVehicleID)
    {
        throw new NotImplementedException();
    }

    // the mvt groups associated with vehcile types
    public VehicleTypes[] iMvtTypes =
    {
        VehicleTypes.CAR,   // eldorado
    	VehicleTypes.CAR,   // hummer
    	VehicleTypes.CAR,   // ice cream truck
    	VehicleTypes.CAR,   // jeep
    	VehicleTypes.CAR,   // tank
    	VehicleTypes.AIR,  // helicopter
    };

    public static int[] iSeatingCapacities =
    {
        6, // eldorado
    	6, // hummer
    	6, // ice cream truck
    	6, // jeep
    	6, // tank
    	6, // helicopter
    };


    public SoundDefine[] iEnterVehicleSndID =
    {
        SoundDefine.S_VECH1_INTO,
        SoundDefine.S_VECH1_INTO,
        SoundDefine.S_VECH1_INTO,
        SoundDefine.S_VECH1_INTO,
        SoundDefine.S_VECH1_INTO,
        SoundDefine.S_VECH1_INTO,
    };

    public SoundDefine[] iMoveVehicleSndID =
    {
        SoundDefine.S_VECH1_MOVE,
        SoundDefine.S_VECH1_MOVE,
        SoundDefine.S_VECH1_MOVE,
        SoundDefine.S_VECH1_MOVE,
        SoundDefine.S_VECH1_MOVE,
        SoundDefine.S_VECH1_MOVE,
    };

    public NPCID[] ubVehicleTypeProfileID =
    {
        NPCID.PROF_ELDERODO,
        NPCID.PROF_HUMMER,
        NPCID.PROF_ICECREAM,
        NPCID.NPC164,
        NPCID.NPC164,
        NPCID.PROF_HELICOPTER
    };
}

/*
// location of crits based on facing
int bInternalCritHitsByLocation[ NUMBER_OF_EXTERNAL_HIT_LOCATIONS_ON_VEHICLE ][ NUMBER_OF_INTERNAL_HIT_LOCATIONS_IN_VEHICLE ]={
    { ENGINE_HIT_LOCATION, ENGINE_HIT_LOCATION, CREW_COMPARTMENT_HIT_LOCATION,CREW_COMPARTMENT_HIT_LOCATION, RF_TIRE_HIT_LOCATION, LF_TIRE_HIT_LOCATION }, // front
    { ENGINE_HIT_LOCATION, LF_TIRE_HIT_LOCATION, CREW_COMPARTMENT_HIT_LOCATION, CREW_COMPARTMENT_HIT_LOCATION, LR_TIRE_HIT_LOCATION, GAS_TANK_HIT_LOCATION}, // left side
    { ENGINE_HIT_LOCATION, RF_TIRE_HIT_LOCATION, CREW_COMPARTMENT_HIT_LOCATION, CREW_COMPARTMENT_HIT_LOCATION, RR_TIRE_HIT_LOCATION, GAS_TANK_HIT_LOCATION}, // right side
    { CREW_COMPARTMENT_HIT_LOCATION, CREW_COMPARTMENT_HIT_LOCATION, CREW_COMPARTMENT_HIT_LOCATION, RR_TIRE_HIT_LOCATION, LR_TIRE_HIT_LOCATION, GAS_TANK_HIT_LOCATION }, // rear
    { ENGINE_HIT_LOCATION, RF_TIRE_HIT_LOCATION, LF_TIRE_HIT_LOCATION, RR_TIRE_HIT_LOCATION,LR_TIRE_HIT_LOCATION, GAS_TANK_HIT_LOCATION,}, // bottom side
    { ENGINE_HIT_LOCATION, ENGINE_HIT_LOCATION, ENGINE_HIT_LOCATION, CREW_COMPARTMENT_HIT_LOCATION, CREW_COMPARTMENT_HIT_LOCATION, GAS_TANK_HIT_LOCATION }, // top
};
*/

// original armor values for vehicles
/*
    ELDORADO_CAR = 0,
    HUMMER,
    ICE_CREAM_TRUCK,
    JEEP_CAR,
    TANK_CAR,
    HELICOPTER,
*/

// type of vehicles
public enum TypesOfVehicles
{
    ELDORADO_CAR = 0,
    HUMMER,
    ICE_CREAM_TRUCK,
    JEEP_CAR,
    TANK_CAR,
    HELICOPTER,
    NUMBER_OF_TYPES_OF_VEHICLES,
};


// external armor hit locations
public enum EXTERNAL_HIT_LOCATIONS
{
    FRONT_EXTERNAL_HIT_LOCATION,
    LEFT_EXTERNAL_HIT_LOCATION,
    RIGHT_EXTERNAL_HIT_LOCATION,
    REAR_EXTERNAL_HIT_LOCATION,
    BOTTOM_EXTERNAL_HIT_LOCATION,
    TOP_EXTERNAL_HIT_LOCATION,
    NUMBER_OF_EXTERNAL_HIT_LOCATIONS_ON_VEHICLE,
};

// internal critical hit locations
public enum HIT_LOCATION
{
    ENGINE_HIT_LOCATION,
    CREW_COMPARTMENT_HIT_LOCATION,
    RF_TIRE_HIT_LOCATION,
    LF_TIRE_HIT_LOCATION,
    RR_TIRE_HIT_LOCATION,
    LR_TIRE_HIT_LOCATION,
    GAS_TANK_HIT_LOCATION,
    NUMBER_OF_INTERNAL_HIT_LOCATIONS_IN_VEHICLE,
};

// struct for vehicles
public class VEHICLETYPE
{
    public Path? pMercPath;  // vehicle's stategic path list
    public int ubMovementGroup; // the movement group this vehicle belongs to
    public int ubVehicleType; // type of vehicle 
    public int sSectorX;   // X position on the Stategic Map
    public MAP_ROW sSectorY;   // Y position on the Stategic Map
    public int sSectorZ;
    public bool fBetweenSectors;  // between sectors?
    public int sGridNo;   // location in tactical
    public List<SOLDIERTYPE> pPassengers = new();
    public int ubDriver;
    public EXTERNAL_HIT_LOCATIONS[] sInternalHitLocations = new EXTERNAL_HIT_LOCATIONS[(int)EXTERNAL_HIT_LOCATIONS.NUMBER_OF_EXTERNAL_HIT_LOCATIONS_ON_VEHICLE];
    public int sArmourType;
    public EXTERNAL_HIT_LOCATIONS[] sExternalArmorLocationsStatus = new EXTERNAL_HIT_LOCATIONS[(int)EXTERNAL_HIT_LOCATIONS.NUMBER_OF_EXTERNAL_HIT_LOCATIONS_ON_VEHICLE];
    public HIT_LOCATION[] sCriticalHits = new HIT_LOCATION[(int)HIT_LOCATION.NUMBER_OF_INTERNAL_HIT_LOCATIONS_IN_VEHICLE];
    public int iOnSound;
    public int iOffSound;
    public int iMoveSound;
    public int iOutOfSound;
    public bool fFunctional;
    public bool fDestroyed;
    public int iMovementSoundID;
    public NPCID ubProfileID;
    public bool fValid;
}

// vehicle/human path structure
public class Path
{
    public uint uiSectorId;
    public uint uiEta;
    public bool fSpeed;
    public Path? pNext;
	public Path? pPrev;
};
