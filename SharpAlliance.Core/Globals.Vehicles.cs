using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int MAX_VEHICLES = 10;

    //extern STR16 sCritLocationStrings[];

    //extern int bInternalCritHitsByLocation[NUMBER_OF_EXTERNAL_HIT_LOCATIONS_ON_VEHICLE][ NUMBER_OF_INTERNAL_HIT_LOCATIONS_IN_VEHICLE ];

    //extern int sVehicleOrigArmorValues[NUMBER_OF_TYPES_OF_VEHICLES][NUMBER_OF_INTERNAL_HIT_LOCATIONS_IN_VEHICLE];

    // the list of vehicles
    public static List<VEHICLETYPE> pVehicleList = new();

    // number of vehicle slots on the list
    public static int ubNumberOfVehicles = 0;

    //ATE: These arrays below should all be in a large LUT which contains
    // static info for each vehicle....

    public static Items[] sVehicleArmourType =
    {
    Items.KEVLAR_VEST,			// El Dorado
	Items.SPECTRA_VEST,			// Hummer
	Items.KEVLAR_VEST,			// Ice cream truck
	Items.KEVLAR_VEST,			// Jeep
	Items.SPECTRA_VEST,			// Tank - do we want this?
	Items.KEVLAR_VEST,			// Helicopter
};


    /*
    int sVehicleExternalOrigArmorValues[ NUMBER_OF_TYPES_OF_VEHICLES ][ NUMBER_OF_INTERNAL_HIT_LOCATIONS_IN_VEHICLE ]={
        { 100,100,100,100,100,100 }, // helicopter
        { 500,500,500,500,500,500 }, // hummer
    };
    */

    /*
    // external armor values
    int sVehicleInternalOrigArmorValues[ NUMBER_OF_TYPES_OF_VEHICLES ][ NUMBER_OF_INTERNAL_HIT_LOCATIONS_IN_VEHICLE ]={
        { 250,250,250,250,250,250 }, // eldorado
        { 250,250,250,250,250,250 }, // hummer
        { 250,250,250,250,250,250 }, // ice cream
        { 250,250,250,250,250,250 }, // feep
        { 850,850,850,850,850,850 }, // tank
        { 50,50,50,50,50,50 }, // helicopter
    };
    */

    // ap cost per crit
    public const int COST_PER_ENGINE_CRIT = 15;
    public const int COST_PER_TIRE_HIT = 5;
}
