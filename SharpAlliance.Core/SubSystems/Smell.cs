using System;
using SharpAlliance.Core.Managers;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

public class Smell
{
    /*
 * Smell & Blood system
 * 
 * Smell and blood trails decay as time passes.  
 * 
 *             Decay Rate        Maximum Strength    Decay Time: Min Max (for biggest volume)
 *
 * Smell       1 per turn              31                         31  31
 * Blood    1 every 1-3 turns           7                          7  21
 *
 * Smell has a much finer resolution so that creatures which track by smell
 * can do so effectively.
 */

    /*
     * Time for some crazy-ass macros!
     * The smell byte is spit as follows:
     * O \
     * O  \
     * O   \ Smell
     * O   / Strength (only on ground)
     * O  /
     * O /
     * O >   Type of blood on roof
     * O >   Type of smell/blood on ground
     *
     * The blood byte is split as follows:
     * O \
     * O  > Blood quantity on roof
     * O /
     * O \ 
     * O  > Blood quantity on ground
     * O /
     * O \  Blood decay
     * O /  time (roof and ground decay together)
     */

    /*
     * In these defines,
     * s indicates the smell byte, b indicates the blood byte
     */

    // LUT for which graphic to use based on strength
    //															 0  1,  2,  3,  4,  5,  6, 7
    public static int[] ubBloodGraphicLUT = { 3, 3, 2, 2, 1, 1, 0, 0 };

    public static int SMELL_TYPE_BITS(int s) => (s & 0x03);
    public static int BLOOD_ROOF_TYPE(int s) => (s & 0x02);
    public static int BLOOD_FLOOR_TYPE(int s) => (s & 0x01);
    public static int BLOOD_ROOF_STRENGTH(int b) => (b & 0xE0);
    public static int BLOOD_FLOOR_STRENGTH(int b) => ((b & 0x1C) >> 2);
    public static int BLOOD_DELAY_TIME(int b) => (b & 0x03);
    public static bool NO_BLOOD_STRENGTH(int b) => ((b & 0xFC) == 0);
    public static int SMELL_TYPE(int s) => (s & 0x01);
    public static int SMELL_STRENGTH(int s) => ((s & 0xFC) >> SMELL_TYPE_NUM_BITS);

    public static void DECAY_SMELL_STRENGTH(int s)
    {
        int ubStrength = Smell.SMELL_STRENGTH((s));
        ubStrength--;
        ubStrength <<= Globals.SMELL_TYPE_NUM_BITS;
        (s) = SMELL_TYPE_BITS((s)) | ubStrength;
    }

    // s = smell byte
    // ns = new strength
    // ntf = new type on floor
    // Note that the first part of the macro is designed to
    // preserve the type value for the blood on the roof
    public static void SET_SMELL(int smell, int newStrength, int newTypeFloor)
    {
        (smell) = (BLOOD_ROOF_TYPE(smell)) | SMELL_TYPE(newTypeFloor) | (newStrength << Globals.SMELL_TYPE_NUM_BITS);
    }

    public static void DECAY_BLOOD_DELAY_TIME(int b)
    {
        (b)--;
    }

    public static void SET_BLOOD_FLOOR_STRENGTH(int b, int nb)
    {
        (b) = ((nb) << 2) | ((b) & 0xE3);
    }

    public static void SET_BLOOD_ROOF_STRENGTH(int b, int nb)
    {
        (b) = BLOOD_FLOOR_STRENGTH((nb)) << 5 | ((b) & 0x1F);
    }

    public static void DECAY_BLOOD_FLOOR_STRENGTH(int b)
    {
        int ubFloorStrength;
        ubFloorStrength = BLOOD_FLOOR_STRENGTH((b));
        ubFloorStrength--;
        SET_BLOOD_FLOOR_STRENGTH(b, ubFloorStrength);
    }

    public static void DECAY_BLOOD_ROOF_STRENGTH(int b)
    {
        int ubRoofStrength;
        ubRoofStrength = BLOOD_ROOF_STRENGTH((b));
        ubRoofStrength--;
        SET_BLOOD_FLOOR_STRENGTH(b, ubRoofStrength);
    }

    public static void SET_BLOOD_DELAY_TIME(int b)
    {
        (b) = BLOOD_DELAY_TIME((int)Globals.Random.Next(Globals.BLOOD_DELAY_MAX) + 1) | (b & 0xFC);
    }

    public static void SET_BLOOD_FLOOR_TYPE(int s, int ntg)
    {
        (s) = BLOOD_FLOOR_TYPE(ntg) | (s & 0xFE);
    }

    public static void SET_BLOOD_ROOF_TYPE(int s, int ntr)
    {
        (s) = BLOOD_ROOF_TYPE(ntr) | (s & 0xFD);
    }

    void RemoveBlood(int sGridNo, int bLevel)
    {
        Globals.gpWorldLevelData[sGridNo].ubBloodInfo = 0;

        Globals.gpWorldLevelData[sGridNo].uiFlags |= MAPELEMENTFLAGS.REEVALUATEBLOOD;

        UpdateBloodGraphics(sGridNo, bLevel);
    }


    public static void DecaySmells()
    {
        foreach (MAP_ELEMENT? pMapElement in Globals.gpWorldLevelData)
        {
            if (pMapElement.ubSmellInfo > 0)
            {
                // decay smell strength!
                DECAY_SMELL_STRENGTH(pMapElement.ubSmellInfo);
                // if the strength left is 0, wipe the whole byte to clear the type
                if (SMELL_STRENGTH(pMapElement.ubSmellInfo) == 0)
                {
                    pMapElement.ubSmellInfo = 0;
                }
            }
        }
    }


    void DecayBlood()
    {
        foreach (MAP_ELEMENT? pMapElement in Globals.gpWorldLevelData)
        {
            if (pMapElement.ubBloodInfo > 0)
            {
                // delay blood timer!
                DECAY_BLOOD_DELAY_TIME(pMapElement.ubBloodInfo);
                if (BLOOD_DELAY_TIME(pMapElement.ubBloodInfo) == 0)
                {
                    // Set re-evaluate flag
                    pMapElement.uiFlags |= MAPELEMENTFLAGS.REEVALUATEBLOOD;

                    // reduce the floor blood strength if it is above zero
                    if (BLOOD_FLOOR_STRENGTH(pMapElement.ubBloodInfo) > 0)
                    {
                        DECAY_BLOOD_FLOOR_STRENGTH(pMapElement.ubBloodInfo);
                    }

                    if (BLOOD_FLOOR_STRENGTH(pMapElement.ubBloodInfo) == 0)
                    {
                        // delete the blood graphic on the floor!
                        // then
                        if (NO_BLOOD_STRENGTH(pMapElement.ubBloodInfo))
                        {
                            // wipe the whole byte to zero
                            pMapElement.ubBloodInfo = 0;
                        }
                    }
                }
                // reduce the roof blood strength if it is above zero
                if (BLOOD_ROOF_STRENGTH(pMapElement.ubBloodInfo) > 0)
                {

                    DECAY_BLOOD_ROOF_STRENGTH(pMapElement.ubBloodInfo);

                    if (BLOOD_ROOF_STRENGTH(pMapElement.ubBloodInfo) == 0)
                    {
                        // delete the blood graphic on the roof!
                        if (NO_BLOOD_STRENGTH(pMapElement.ubBloodInfo))
                        {
                            // wipe the whole byte to zero
                            pMapElement.ubBloodInfo = 0;
                        }
                    }
                }

                // if any blood remaining, reset time
                if (pMapElement.ubBloodInfo > 0)
                {
                    SET_BLOOD_DELAY_TIME(pMapElement.ubBloodInfo);
                }
            }
            // end of blood handling
        }

        // now go on to the next gridno
    }


    void DecayBloodAndSmells(int uiTime)
    {
        int uiCheckTime;

        if (!gfWorldLoaded)
        {
            return;
        }

        // period between checks, in game seconds
        switch (giTimeCompressMode)
        {
            // in time compression, let this happen every 5 REAL seconds
            case TIME_COMPRESS.TIME_COMPRESS_5MINS: // rate of 300 seconds per real second
                uiCheckTime = 5 * 300;
                break;
            case TIME_COMPRESS.TIME_COMPRESS_30MINS: // rate of 1800 seconds per real second
                uiCheckTime = 5 * 1800;
                break;
            case TIME_COMPRESS.TIME_COMPRESS_60MINS: // rate of 3600 seconds per real second
            case TIME_COMPRESS.TIME_SUPER_COMPRESS: // should not be used but just in frigging case...
                uiCheckTime = 5 * 3600;
                break;
            default: // not compressing
                uiCheckTime = 100;
                break;
        }

        // ok so "uiDecayBloodLastUpdate" is a bit of a misnomer now
        if ((uiTime - gTacticalStatus.uiDecayBloodLastUpdate) > uiCheckTime)
        {
            gTacticalStatus.uiDecayBloodLastUpdate = uiTime;
            DecayBlood();
            DecaySmells();
        }
    }

    void DropSmell(SOLDIERTYPE? pSoldier)
    {
        MAP_ELEMENT? pMapElement;
        int ubOldSmell;
        int ubOldStrength;
        int ubSmell;
        int ubStrength;

        /*
         *  Here we are creating a new smell on the ground.  If there is blood in
         *  the tile, it overrides dropping smells of any type
         */

        if (pSoldier.bLevel == 0)
        {
            pMapElement = (gpWorldLevelData[pSoldier.sGridNo]);
            if (pMapElement.ubBloodInfo > 0)
            {
                // blood here, don't drop any smell
                return;
            }

            if (pSoldier.bNormalSmell > pSoldier.bMonsterSmell)
            {
                ubStrength = pSoldier.bNormalSmell - pSoldier.bMonsterSmell;
                ubSmell = HUMAN;
            }
            else
            {
                ubStrength = pSoldier.bMonsterSmell - pSoldier.bNormalSmell;
                if (ubStrength == 0)
                {
                    // don't drop any smell
                    return;
                }
                ubSmell = CREATURE_ON_FLOOR;
            }

            if (pMapElement.ubSmellInfo > 0)
            {
                // smell already exists here; check to see if it's the same or not

                ubOldSmell = SMELL_TYPE(pMapElement.ubSmellInfo);
                ubOldStrength = SMELL_STRENGTH(pMapElement.ubSmellInfo);
                if (ubOldSmell == ubSmell)
                {
                    // same smell; increase the strength to the bigger of the two strengths,
                    // plus 1/5 of the smaller
                    ubStrength = Math.Max(ubStrength, ubOldStrength) + Math.Min(ubStrength, ubOldStrength) / 5;
                    ubStrength = Math.Max(ubStrength, SMELL_STRENGTH_MAX);
                }
                else
                {
                    // different smell; we muddy the smell by reducing the smell strength
                    if (ubOldStrength > ubStrength)
                    {
                        ubOldStrength -= ubStrength / 3;
                        SET_SMELL(pMapElement.ubSmellInfo, ubOldStrength, ubOldSmell);
                    }
                    else
                    {
                        ubStrength -= ubOldStrength / 3;
                        if (ubStrength > 0)
                        {
                            SET_SMELL(pMapElement.ubSmellInfo, ubStrength, ubSmell);
                        }
                        else
                        {
                            // smell reduced to 0 - wipe all info on it!
                            pMapElement.ubSmellInfo = 0;
                        }
                    }
                }
            }
            else
            {
                // the simple case, dropping a smell in a location where there is none
                SET_SMELL(pMapElement.ubSmellInfo, ubStrength, ubSmell);
            }
        }
        // otherwise skip dropping smell
    }


    void InternalDropBlood(int sGridNo, int bLevel, int ubType, int ubStrength, int bVisible)
    {
        MAP_ELEMENT? pMapElement;
        int ubOldStrength = 0;
        int ubNewStrength = 0;

        /*
         * Dropping some blood;
         * We can check the type of blood by consulting the type in the smell byte
         */

        // If we are in water...
        if (WorldManager.GetTerrainType(sGridNo) == TerrainTypeDefines.DEEP_WATER
            || WorldManager.GetTerrainType(sGridNo) == TerrainTypeDefines.LOW_WATER
            || WorldManager.GetTerrainType(sGridNo) == TerrainTypeDefines.MED_WATER)
        {
            return;
        }

        // ATE: Send warning if dropping blood nowhere....
        if (sGridNo == NOWHERE)
        {
            // ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_TESTVERSION, "Attempting to drop blood NOWHERE");
            return;
        }

        // ensure max strength is okay
        ubStrength = Math.Min(ubStrength, BLOOD_STRENGTH_MAX);

        pMapElement = (Globals.gpWorldLevelData[sGridNo]);
        if (bLevel == 0)
        {
            // dropping blood on ground
            ubOldStrength = BLOOD_FLOOR_STRENGTH(pMapElement.ubBloodInfo);
            if (ubOldStrength > 0)
            {
                // blood already there... we'll leave the decay time as it is
                if (BLOOD_FLOOR_TYPE(pMapElement.ubBloodInfo) == ubType)
                {
                    // combine blood strengths!
                    ubNewStrength = Math.Min((ubOldStrength + ubStrength), BLOOD_STRENGTH_MAX);

                    SET_BLOOD_FLOOR_STRENGTH(pMapElement.ubBloodInfo, ubNewStrength);
                }
                else
                {
                    // replace the existing blood if more is being dropped than exists
                    if (ubStrength > ubOldStrength)
                    {
                        // replace!
                        SET_BLOOD_FLOOR_STRENGTH(pMapElement.ubBloodInfo, ubStrength);
                    }
                    // else we don't drop anything at all
                }
            }
            else
            {
                // no blood on the ground yet, so drop this amount!
                // set decay time 
                SET_BLOOD_DELAY_TIME(pMapElement.ubBloodInfo);
                SET_BLOOD_FLOOR_STRENGTH(pMapElement.ubBloodInfo, ubStrength);
                // NB blood floor type stored in smell byte!
                SET_BLOOD_FLOOR_TYPE(pMapElement.ubSmellInfo, ubType);
            }
        }
        else
        {
            // dropping blood on roof
            ubOldStrength = BLOOD_ROOF_STRENGTH(pMapElement.ubBloodInfo);
            if (ubOldStrength > 0)
            {
                // blood already there... we'll leave the decay time as it is
                if (BLOOD_ROOF_TYPE(pMapElement.ubSmellInfo) == ubType)
                {
                    // combine blood strengths!
                    ubNewStrength = Math.Max(ubOldStrength, ubStrength) + 1;
                    // make sure the strength is legal		
                    ubNewStrength = Math.Max(ubNewStrength, BLOOD_STRENGTH_MAX);
                    SET_BLOOD_ROOF_STRENGTH(pMapElement.ubBloodInfo, ubNewStrength);
                }
                else
                {
                    // replace the existing blood if more is being dropped than exists
                    if (ubStrength > ubOldStrength)
                    {
                        // replace!
                        SET_BLOOD_ROOF_STRENGTH(pMapElement.ubBloodInfo, ubStrength);
                    }
                    // else we don't drop anything at all
                }
            }
            else
            {
                // no blood on the roof yet, so drop this amount!
                // set decay time 
                SET_BLOOD_DELAY_TIME(pMapElement.ubBloodInfo);
                SET_BLOOD_ROOF_STRENGTH(pMapElement.ubBloodInfo, ubNewStrength);
                SET_BLOOD_ROOF_TYPE(pMapElement.ubSmellInfo, ubType);
            }
        }

        // Turn on flag...
        pMapElement.uiFlags |= MAPELEMENT_REEVALUATEBLOOD;

        if (bVisible != -1)
        {
            UpdateBloodGraphics(sGridNo, bLevel);
        }

    }


    void DropBlood(SOLDIERTYPE? pSoldier, int ubStrength, int bVisible)
    {
        int ubType;
        int ubOldStrength = 0;
        int ubNewStrength = 0;

        /*
         * Dropping some blood;
         * We can check the type of blood by consulting the type in the smell byte
         */

        // figure out the type of blood that we're dropping
        if (pSoldier.uiStatusFlags & SOLDIER.MONSTER)
        {
            if (pSoldier.bLevel == 0)
            {
                ubType = CREATURE_ON_FLOOR;
            }
            else
            {
                ubType = CREATURE_ON_ROOF;
            }
        }
        else
        {
            ubType = 0;
        }


        InternalDropBlood(pSoldier.sGridNo, pSoldier.bLevel, ubType, ubStrength, bVisible);
    }



    void UpdateBloodGraphics(int sGridNo, int bLevel)
    {
        MAP_ELEMENT? pMapElement;
        int bValue;
        int usIndex, usNewIndex;

        // OK, based on level, type, display graphics for blood
        pMapElement = (Globals.gpWorldLevelData[sGridNo]);

        // CHECK FOR BLOOD OPTION
        if (!GameSettings.fOptions[TOPTION.BLOOD_N_GORE])
        {
            return;
        }

        if (pMapElement.uiFlags.HasFlag(MAPELEMENTFLAGS.REEVALUATEBLOOD))
        {

            // Turn off flag!
            pMapElement.uiFlags &= (~MAPELEMENTFLAGS.REEVALUATEBLOOD);

            // Ground
            if (bLevel == 0)
            {
                bValue = BLOOD_FLOOR_STRENGTH(pMapElement.ubBloodInfo);

                // OK, remove tile graphic if one exists....
                if (TypeRangeExistsInObjectLayer(sGridNo, TileTypeDefines.HUMANBLOOD, TileTypeDefines.CREATUREBLOOD, out usIndex))
                {
                    //This has been removed and it is handled by the ubBloodInfo level when restoring a saved game.
                    //Set a flag indicating that the following changes are to go the the maps temp file
                    //ApplyMapChangesToMapTempFile( TRUE );

                    // Remove
                    RemoveObject(sGridNo, usIndex);

                    //ApplyMapChangesToMapTempFile( FALSE );
                }

                // OK, pick new one. based on strength and randomness

                if (bValue > 0)
                {
                    usIndex = (int)((Globals.Random.Next(4) * 4) + ubBloodGraphicLUT[bValue]);

                    if (BLOOD_FLOOR_TYPE(pMapElement.ubSmellInfo) == 0)
                    {
                        GetTileIndexFromTypeSubIndex(HUMANBLOOD, (int)(usIndex + 1), out usNewIndex);
                    }
                    else
                    {
                        GetTileIndexFromTypeSubIndex(CREATUREBLOOD, (int)(usIndex + 1), out usNewIndex);
                    }

                    //This has been removed and it is handled by the ubBloodInfo level when restoring a saved game.
                    //Set a flag indicating that the following changes are to go the the maps temp file
                    //ApplyMapChangesToMapTempFile( TRUE );

                    // Add!
                    AddObjectToHead(sGridNo, usNewIndex);

                    //ApplyMapChangesToMapTempFile( FALSE );


                    // Update rendering!
                    pMapElement.uiFlags |= MAPELEMENT_REDRAW;
                    RenderWorld.SetRenderFlags(RenderingFlags.MARKED);

                }
            }
            // Roof
            else
            {
            }
        }
    }
}
