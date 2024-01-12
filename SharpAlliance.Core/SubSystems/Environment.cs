using System;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class Environment
{
    // higher is darker, remember
    public const int NORMAL_LIGHTLEVEL_NIGHT = 12;
    public const int NORMAL_LIGHTLEVEL_DAY = 3;
    public const uint DAWN_START = (6 * 60 + 47);		//6:47AM
    public const int DAY_START = (7 * 60 + 5);		//7:05AM
    public const int DUSK_START = (20 * 60 + 57);	//8:57PM
    public const int NIGHT_START = (21 * 60 + 15);	//9:15PM
    public const uint DAWN_TO_DAY = (DAY_START - DAWN_START);
    public const int DAY_TO_DUSK = (DUSK_START - DAY_START);
    public const int DUSK_TO_NIGHT = (NIGHT_START - DUSK_START);
    public const uint NIGHT_TO_DAWN = (24 * 60 - NIGHT_START + DAWN_START);

    public const int DESERT_WARM_START = (8 * 60);
    public const int DESERT_HOT_START = (9 * 60);
    public const int DESERT_HOT_END = (17 * 60);
    public const int DESERT_WARM_END = (19 * 60);

    public const int GLOBAL_WARM_START = (9 * 60);
    public const int GLOBAL_HOT_START = (12 * 60);
    public const int GLOBAL_HOT_END = (14 * 60);
    public const int GLOBAL_WARM_END = (17 * 60);

    //Make sure you use 24 for end time hours and 0 for start time hours if
    //midnight is the hour you wish to use.
    public const int NIGHT_TIME_LIGHT_START_HOUR = 21;
    public const int NIGHT_TIME_LIGHT_END_HOUR = 7;
    public const int PRIME_TIME_LIGHT_START_HOUR = 21;
    public const int PRIME_TIME_LIGHT_END_HOUR = 24;


    public const int HOT_DAY_LIGHTLEVEL = 2;

    internal static void BuildDayLightLevels()
    {
        int uiLoop, uiHour;

        /*
        // Dawn; light 12
        AddEveryDayStrategicEvent( EVENT_CHANGELIGHTVAL, DAWNLIGHT_START, NORMAL_LIGHTLEVEL_NIGHT - 1 );

        // loop from light 12 down to light 4
        for (uiLoop = 1; uiLoop < 8; uiLoop++)
        {
            AddEveryDayStrategicEvent( EVENT_CHANGELIGHTVAL, DAWN_START + 15 * uiLoop,	NORMAL_LIGHTLEVEL_NIGHT - 1 - uiLoop );
        }
        */

        //Transition from night to day
        for (uiLoop = 0; uiLoop < 9; uiLoop++)
        {
            GameEvents.AddEveryDayStrategicEvent(EVENT.CHANGELIGHTVAL, (uint)(DAWN_START + 2 * uiLoop), NORMAL_LIGHTLEVEL_NIGHT - 1 - uiLoop);
        }

        // Add events for hot times
        GameEvents.AddEveryDayStrategicEvent(EVENT.TEMPERATURE_UPDATE, DESERT_WARM_START, TEMPERATURE.DESERT_WARM);
        GameEvents.AddEveryDayStrategicEvent(EVENT.TEMPERATURE_UPDATE, DESERT_HOT_START, TEMPERATURE.DESERT_HOT);
        GameEvents.AddEveryDayStrategicEvent(EVENT.TEMPERATURE_UPDATE, DESERT_HOT_END, TEMPERATURE.DESERT_WARM);
        GameEvents.AddEveryDayStrategicEvent(EVENT.TEMPERATURE_UPDATE, DESERT_WARM_END, TEMPERATURE.DESERT_COOL);

        GameEvents.AddEveryDayStrategicEvent(EVENT.TEMPERATURE_UPDATE, GLOBAL_WARM_START, TEMPERATURE.GLOBAL_WARM);
        GameEvents.AddEveryDayStrategicEvent(EVENT.TEMPERATURE_UPDATE, GLOBAL_HOT_START, TEMPERATURE.GLOBAL_HOT);
        GameEvents.AddEveryDayStrategicEvent(EVENT.TEMPERATURE_UPDATE, GLOBAL_HOT_END, TEMPERATURE.GLOBAL_WARM);
        GameEvents.AddEveryDayStrategicEvent(EVENT.TEMPERATURE_UPDATE, GLOBAL_WARM_END, TEMPERATURE.GLOBAL_COOL);

        /*
            // Twilight; light 5
            AddEveryDayStrategicEvent( EVENT_CHANGELIGHTVAL, TWILLIGHT_START, NORMAL_LIGHTLEVEL_DAY + 1 );

            // Dusk; loop from light 5 up to 12
            for (uiLoop = 1; uiLoop < 8; uiLoop++)
            {
                AddEveryDayStrategicEvent( EVENT_CHANGELIGHTVAL, DUSK_START + 15 * uiLoop, NORMAL_LIGHTLEVEL_DAY + 1 + uiLoop );
            }
        */

        //Transition from day to night
        for (uiLoop = 0; uiLoop < 9; uiLoop++)
        {
            GameEvents.AddEveryDayStrategicEvent(EVENT.CHANGELIGHTVAL, (uint)(DUSK_START + 2 * uiLoop), NORMAL_LIGHTLEVEL_DAY + 1 + uiLoop);
        }

        //Set up the scheduling for turning lights on and off based on the various types.
        uiHour = NIGHT_TIME_LIGHT_START_HOUR == 24 ? 0 : NIGHT_TIME_LIGHT_START_HOUR;
        GameEvents.AddEveryDayStrategicEvent(EVENT.TURN_ON_NIGHT_LIGHTS, (uint)uiHour * 60, 0);
        uiHour = NIGHT_TIME_LIGHT_END_HOUR == 24 ? 0 : NIGHT_TIME_LIGHT_END_HOUR;
        GameEvents.AddEveryDayStrategicEvent(EVENT.TURN_OFF_NIGHT_LIGHTS, (uint)uiHour * 60, 0);
        uiHour = PRIME_TIME_LIGHT_START_HOUR == 24 ? 0 : PRIME_TIME_LIGHT_START_HOUR;
        GameEvents.AddEveryDayStrategicEvent(EVENT.TURN_ON_PRIME_LIGHTS, (uint)uiHour * 60, 0);
        uiHour = PRIME_TIME_LIGHT_END_HOUR == 24 ? 0 : PRIME_TIME_LIGHT_END_HOUR;
        GameEvents.AddEveryDayStrategicEvent(EVENT.TURN_OFF_PRIME_LIGHTS, (uint)uiHour * 60, 0);
    }
}

[Flags]
public enum WEATHER_FORECAST
{
    SUNNY = 0x00000001,
    OVERCAST = 0x00000002,
    PARTLYSUNNY = 0x00000004,
    DRIZZLE = 0x00000008,
    SHOWERS = 0x00000010,
    THUNDERSHOWERS = 0x00000020,
}

public enum Temperatures
{
    COOL,
    WARM,
    HOT
}

public enum TEMPERATURE
{
    DESERT_COOL,
    DESERT_WARM,
    DESERT_HOT,
    GLOBAL_COOL,
    GLOBAL_WARM,
    GLOBAL_HOT,
}
