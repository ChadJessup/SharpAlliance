using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{
    // the delay for a group about to arrive
    public const int ABOUT_TO_ARRIVE_DELAY = 5;

    // is the bottom of the map panel dirty?
    public static bool gfUsePersistantPBI;
    public static int[] gubVehicleMovementGroups = new int[MAX_VEHICLES];
    public static bool gfDelayAutoResolveStart = false;
    public static bool gfRandomizingPatrolGroup = false;

    //Doesn't require text localization.  This is for debug strings only.
    public static Traversability[] gszTerrain =
    {
        Traversability.TOWN, Traversability.ROAD, Traversability.PLAINS, Traversability.SAND, Traversability.SPARSE, Traversability.DENSE, Traversability.SWAMP,
        Traversability.WATER, Traversability.HILLS, Traversability.GROUNDBARRIER, Traversability.NS_RIVER, Traversability.EW_RIVER, Traversability.EDGEOFWORLD
    };

    public static bool gfUndergroundTacticalTraversal = false;

    // remembers which player group is the Continue/Stop prompt about?  No need to save as long as you can't save while prompt ON
    public static GROUP? gpGroupPrompting = null;
    public static GROUP? gpInitPrebattleGroup = null;

    public static int[] uniqueIDMask = { 0, 0, 0, 0, 0, 0, 0, 0 };


    // waiting for input from user
    public static bool gfWaitingForInput = false;
}

public class StrategicMovementCosts
{
    int[][] gubEncryptionArray3 =
{
    new[]
    {
        250,224,3,197,156,209,110,
        159,75,119,221,42,212,180,
        223,115,13,246,173,221,211,
        148,3,78,214,195,102,155,
        5,128,5,204,42,72,240,
        65,177,242,226,81,255,139,
        70,150,95,124,203,83,248
    },
    new[]
    {
    234,33,49,205,144,43,212,
    44,249,86,116,150,112,80,
    244,150,120,207,182,110,50,
    179,160,41,114,31,130,253,
    243,221,106,120,118,181,252,
    103,30,238,119,10,242,187,
    99,99,210,197,153,71,176
    },
    new[]
    {
    137,180,252,121,200,124,8,
    111,186,110,245,102,71,247,
    195,157,232,115,191,169,136,
    138,98,54,253,14,34,248,
    106,226,167,185,48,19,112,
    183,175,155,66,76,150,34,
    114,38,225,8,126,236,96
    },
    new[]
    {
    16,220,169,218,40,146,208,
    171,96,114,57,235,189,141,
    227,252,238,194,231,160,128,
    231,91,85,175,137,143,46,
    106,176,119,234,149,173,154,
    114,52,93,90,126,142,222,
    243,157,223,56,7,82,175
    },
    new[]
    {
    214,11,122,112,113,118,195,
    111,55,4,85,186,203,217,
    125,14,7,2,128,91,236,
    239,85,23,213,142,125,198,
    74,130,186,4,118,41,195,
    123,188,1,212,1,94,239,
    40,89,169,57,55,203,169
    },
    new[]
    {
    107,224,17,213,57,56,188,
    177,120,49,183,211,64,230,
    226,84,84,171,122,18,226,
    165,77,205,198,31,112,139,
    65,93,107,58,110,22,144,
    19,97,87,140,177,42,4,
    192,72,174,177,138,11,166
    },
    new[]
    {
    9,11,33,144,120,134,56,
    4,91,241,26,37,93,204,
    71,167,75,221,9,34,10,
    219,253,35,235,183,134,199,
    28,130,28,63,91,151,1,
    3,187,36,59,41,166,150,
    104,162,205,29,72,186,83
    },
    new[]
    {
    196,130,182,208,71,167,231,
    133,59,210,10,82,151,243,
    72,199,77,48,187,1,229,
    90,194,112,224,238,252,108,
    206,154,86,168,215,178,231,
    84,179,166,10,22,174,219,
    127,163,226,226,243,31,38
    },
    new[]
    {
    151,198,6,214,2,99,210,
    26,142,255,31,154,133,47,
    111,116,53,2,99,76,220,
    32,138,65,181,182,139,192,
    37,121,215,223,133,181,173,
    70,135,166,142,83,146,1,
    243,133,54,3,113,189,13
    },
    new[]
    {
    218,245,135,17,201,119,232,
    222,199,170,217,219,28,16,
    3,178,162,3,204,38,47,
    12,104,170,218,33,215,196,
    149,34,158,166,210,45,34,
    243,172,26,99,60,84,204,
    28,18,35,143,222,62,46
    },
    new[]
    {
    60,75,3,168,92,248,239,
    242,4,39,47,181,156,203,
    212,206,79,31,30,121,87,
    53,27,131,225,189,185,224,
    197,139,173,133,179,233,43,
    197,57,111,229,53,35,75,
    91,56,162,191,210,60,204
    },
    new[]
    {
    204,16,230,187,172,49,5,
    6,62,173,174,199,231,242,
    88,238,27,145,67,3,252,
    116,22,44,104,24,248,161,
    191,68,19,63,190,51,179,
    124,223,155,19,121,99,175,
    236,86,157,100,225,151,149
    },
    new[]
    {
    20,225,193,156,236,144,244,
    233,27,222,169,213,53,207,
    99,209,213,167,118,171,224,
    107,166,60,107,5,215,26,
    193,227,130,90,118,110,40,
    15,9,41,122,128,4,213,
    119,214,25,121,36,43,50
    },
    new[]
    {
    145,47,181,236,88,31,32,
    115,104,90,150,49,168,172,
    179,101,188,142,221,234,236,
    228,41,88,211,109,94,201,
    158,144,56,104,73,210,109,
    23,168,157,173,64,144,150,
    18,68,3,56,48,116,165
    },
    new[]
    {
    244,90,27,112,128,36,134,
    214,150,207,139,84,223,171,
    128,173,54,7,27,180,4,
    201,54,253,233,84,240,76,
    115,170,33,14,5,159,140,
    205,195,253,229,225,165,86,
    11,58,114,131,107,165,215
    },
    new[]
    {
    127,30,93,91,165,158,58,
    91,236,151,103,207,65,207,
    224,16,142,150,170,76,137,
    179,3,245,230,90,117,207,
    4,1,32,217,158,175,10,
    214,182,171,214,154,51,253,
    189,234,95,204,17,14,207
    },
    new[]
    {
    251,51,223,223,24,80,138,
    60,244,179,168,186,1,21,
    12,239,194,171,206,186,121,
    108,254,72,86,66,135,179,
    75,154,160,214,228,28,109,
    100,31,230,13,217,190,45,
    212,123,22,131,225,202,182
    },
    new[]
    {
    185,198,186,9,155,133,18,
    53,111,146,55,105,127,17,
    220,228,159,10,193,193,233,
    209,13,3,157,84,98,206,
    113,120,76,80,52,103,3,
    69,15,214,66,155,70,31,
    44,43,203,79,226,242,132
    },
    new[]
    {
    243,234,219,137,211,230,117,
    77,78,213,164,239,148,89,
    188,164,131,43,255,119,66,
    78,239,81,106,25,124,145,
    243,179,114,20,144,27,54,
    248,181,69,49,9,19,129,
    246,21,163,160,145,26,21
    },
    new[]
    {
    19,244,140,188,119,3,162,
    214,207,50,237,66,223,44,
    37,110,211,126,117,193,202,
    185,39,26,89,15,255,186,
    152,204,45,61,223,196,18,
    230,196,12,213,241,104,9,
    2,33,192,82,18,67,223
    },
    new[]
    {
    74,68,234,227,249,134,5,
    155,29,216,149,124,210,253,
    70,1,251,206,7,6,169,
    11,110,69,164,249,34,121,
    124,192,237,83,24,179,204,
    195,70,140,154,203,57,204,
    154,84,113,52,162,44,11
    },
    new[]
    {
    149,12,210,227,237,40,13,
    145,9,125,242,172,155,114,
    134,79,24,170,101,90,40,
    201,183,100,21,213,235,222,
    1,235,97,78,63,140,139,
    41,175,36,176,69,106,21,
    222,78,151,1,31,62,206
    },
    new[]
    {
    111,142,87,207,172,114,135,
    240,251,218,183,28,227,230,
    7,172,200,86,82,11,141,
    106,27,97,114,183,48,49,
    236,5,27,61,172,200,203,
    128,129,90,113,165,107,124,
    2,196,116,74,95,198,166
    },
    new[]
    {
    36,157,67,183,185,88,56,
    196,189,140,108,182,108,4,
    207,158,104,168,192,176,19,
    219,132,39,248,42,196,176,
    100,106,126,180,172,179,32,
    32,102,40,67,229,250,6,
    212,3,207,255,251,39,137
    },
    new[]
    {
    75,159,202,137,103,226,221,
    61,6,107,208,82,34,206,
    43,111,163,245,105,131,160,
    221,86,66,164,127,159,241,
    252,63,209,15,117,177,134,
    241,155,33,226,253,211,145,
    55,122,105,182,231,179,227
    },
    new[]
    {
    157,96,103,188,105,64,44,
    218,9,130,220,208,31,209,
    165,84,23,196,202,232,165,
    52,185,56,150,110,141,11,
    65,114,137,84,121,247,180,
    97,83,114,27,129,147,201,
    227,59,40,2,192,121,117
    },
    new[]
    {
    141,213,168,224,119,181,65,
    98,40,127,183,126,248,200,
    61,116,77,83,91,13,104,
    56,217,205,187,161,226,238,
    229,156,224,248,17,35,26,
    72,247,255,100,102,62,145,
    12,135,83,17,77,255,163
    },
    new[]
    {
    114,95,19,65,117,142,233,
    198,248,84,19,166,59,238,
    91,165,4,102,92,171,109,
    125,153,177,72,137,125,255,
    201,156,23,103,141,9,230,
    198,139,174,164,127,20,8,
    55,25,105,110,215,204,24
    },
    new[]
    {
    158,164,46,157,212,125,174,
    116,154,138,38,34,169,58,
    43,99,220,22,105,253,182,
    66,163,101,91,9,182,186,
    147,53,45,66,185,174,198,
    244,21,25,133,42,145,223,
    147,19,91,117,172,252,72
    },
    new[]
    {
    49,66,21,133,143,27,168,
    148,62,162,138,247,194,151,
    175,153,19,96,160,84,252,
    176,202,168,181,193,91,4,
    91,206,171,158,213,18,227,
    101,224,241,223,225,148,168,
    252,160,86,4,213,6,111
    },
    new[]
    {
    144,151,17,65,208,251,3,
    77,204,130,87,4,157,7,
    28,165,66,66,8,17,95,
    85,91,208,59,252,247,77,
    146,111,174,109,148,149,48,
    134,177,171,170,239,125,216,
    120,18,77,240,230,76,226
    },
    new[]
    {
    210,134,132,192,156,253,190,
    117,63,210,141,138,131,45,
    185,81,35,254,244,69,17,
    145,239,66,118,235,177,58,
    145,10,125,173,254,99,41,
    155,144,176,54,26,63,107,
    135,92,92,2,13,83,139
    },
    new[]
    {
    51,60,163,170,147,164,49,
    58,161,146,230,89,121,242,
    4,248,134,113,158,82,65,
    18,148,65,101,47,159,144,
    148,39,206,229,233,148,16,
    64,113,112,11,203,242,240,
    255,1,19,113,237,186,66
    },
    new[]
    {
    89,159,78,103,56,246,78,
    204,4,21,252,53,204,162,
    14,168,189,244,222,214,188,
    53,154,156,141,90,137,154,
    195,28,5,79,102,155,54,
    192,149,251,61,20,11,162,
    196,30,206,82,172,93,1
    },
    new[]
    {
    226,222,85,249,190,223,200,
    178,240,60,187,187,232,97,
    207,164,185,5,211,32,8,
    168,23,210,90,85,110,5,
    12,44,92,46,148,220,104,
    161,95,153,5,51,231,168,
    13,54,84,34,77,166,72
    },
    new[]
    {
    252,15,213,37,242,26,114,
    115,99,46,77,163,196,100,
    157,235,193,113,53,117,144,
    72,105,138,167,8,22,7,
    97,184,138,186,169,200,185,
    7,73,199,135,77,234,79,
    143,149,114,153,47,242,186
    },
    new[]
    {
    187,60,9,83,243,54,78,
    90,20,70,81,255,107,243,
    177,221,63,217,7,159,51,
    56,113,50,168,185,8,252,
    138,74,218,63,120,74,198,
    59,206,5,205,40,123,185,
    46,167,40,14,241,178,153
    },
    new[]
    {
    75,41,175,215,50,141,196,
    250,196,198,238,44,224,253,
    14,195,247,8,102,7,200,
    205,196,115,107,61,202,22,
    142,105,139,229,44,24,255,
    154,171,123,119,239,174,72,
    160,219,106,222,45,158,228
    },
    new[]
    {
    201,188,54,248,57,37,25,
    96,199,162,200,176,46,20,
    27,160,39,217,196,100,58,
    103,23,127,168,47,95,229,
    39,234,244,187,179,238,89,
    154,37,140,111,160,190,49,
    56,56,126,62,22,213,80
    },
    new[]
    {
    81,12,160,241,248,231,70,
    171,127,226,220,168,223,151,
    45,22,115,217,54,204,131,
    100,66,186,63,198,114,191,
    69,158,2,56,67,137,48,
    242,216,196,25,192,64,253,
    95,93,232,65,242,229,139
    },
    new[]
    {
    134,221,148,217,202,95,252,
    95,61,51,127,170,99,97,
    40,82,194,103,179,250,244,
    25,250,229,172,5,102,45,
    149,205,194,61,150,45,7,
    167,96,27,110,234,204,213,
    117,58,248,57,20,234,161
    },
    new[]
    {
    38,213,157,169,107,23,175,
    84,238,15,28,30,134,243,
    88,168,69,218,79,201,159,
    159,4,16,64,125,5,223,
    214,149,64,121,210,33,68,
    249,64,123,162,195,195,200,
    107,77,238,103,118,198,207
    },
    new[]
    {
    232,120,145,34,201,147,8,
    220,158,104,126,144,240,77,
    8,89,132,187,230,206,52,
    139,46,181,45,26,125,223,
    181,244,93,1,55,20,46,
    220,205,75,29,161,7,5,
    34,193,17,215,109,50,25
    },
    new[]
    {
    25,89,86,245,1,51,123,
    253,111,240,58,28,252,69,
    144,241,90,250,19,53,165,
    34,9,11,197,1,207,136,
    105,56,90,29,184,34,29,
    30,96,214,85,38,248,211,
    231,131,125,190,194,106,204
    },
    new[]
    {
    61,15,48,227,80,24,43,
    221,58,41,146,86,89,88,
    250,64,248,115,177,207,134,
    12,182,142,54,217,120,46,
    111,96,32,51,32,37,151,
    15,72,90,11,200,212,66,
    17,187,46,58,64,154,125
    },
    new[]
    {
    176,94,60,25,239,233,78,
    19,10,51,143,104,187,179,
    159,185,176,236,250,20,228,
    122,71,189,152,144,122,121,
    149,165,253,58,50,118,92,
    202,216,34,158,78,119,147,
    232,32,175,242,105,5,20
    },
    new[]
    {
    88,62,37,83,109,101,204,
    176,66,65,101,138,12,229,
    157,97,249,172,65,38,232,
    47,177,45,30,73,118,158,
    209,49,230,186,172,61,84,
    202,3,116,192,24,3,129,
    135,189,122,24,1,172,139
    },
    new[]
    {
    115,137,193,238,244,237,60,
    4,136,178,113,108,224,44,
    23,96,32,227,245,129,17,
    62,100,83,120,217,93,33,
    161,164,138,122,190,26,26,
    17,48,159,188,27,71,132,
    155,5,167,136,166,149,216
    },
    new[]
    {
    124,10,86,29,212,50,96,
    40,191,32,87,212,177,122,
    184,100,207,41,78,103,73,
    208,226,235,2,23,9,255,
    153,233,21,34,48,194,23,
    194,249,39,252,94,6,68,
    157,81,56,5,229,1,239
    },
    new[]
    {
    109,209,104,83,161,130,167,
    172,101,12,168,226,109,80,
    124,120,101,130,117,14,239,
    162,172,222,143,156,249,47,
    182,69,250,40,239,237,75,
    18,96,198,112,106,145,201,
    171,208,196,95,49,54,187
    },
    new[]
    {
    125,238,86,66,116,112,229,
    80,35,251,120,41,196,128,
    141,64,28,109,190,69,41,
    7,139,44,39,89,183,137,
    4,83,178,29,23,51,255,
    218,62,204,31,93,41,202,
    220,250,247,133,158,120,253
    },
    new[]
    {
    117,124,147,199,242,198,81,
    46,74,212,97,166,187,160,
    98,132,139,36,127,115,172,
    244,19,206,38,12,210,29,
    201,63,54,94,83,86,145,
    105,132,61,162,21,95,76,
    244,88,13,24,242,35,139
    },
    new[]
    {
    191,252,45,196,59,89,93,
    15,158,95,25,209,189,162,
    46,60,61,146,124,209,115,
    74,54,193,42,248,209,175,
    155,184,122,14,184,40,48,
    143,46,158,66,212,21,89,
    120,234,207,110,136,175,12
    },
    new[]
    {
    239,25,187,91,249,22,224,
    99,40,115,213,19,41,56,
    53,221,222,229,82,112,215,
    23,12,215,126,112,44,146,
    209,173,116,133,9,253,233,
    75,235,96,117,211,69,72,
    120,209,63,49,107,230,5
    },
    new[]
    {
    98,24,14,131,155,143,55,
    150,221,114,139,140,10,153,
    84,73,144,203,3,226,232,
    129,64,28,254,91,143,128,
    99,100,112,138,96,179,122,
    168,183,133,108,113,69,98,
    167,230,45,116,11,32,225
    },
    new[]
    {
    153,169,41,171,77,85,127,
    241,6,111,247,245,26,2,
    97,66,194,143,211,123,90,
    150,228,211,108,60,176,209,
    165,35,7,167,82,207,143,
    205,104,166,75,33,202,249,
    58,54,206,10,136,19,166
    },
    new[]
    {
    146,29,30,194,190,208,94,
    195,8,67,217,18,255,127,
    64,188,106,114,153,172,177,
    98,54,195,32,99,153,14,
    221,5,133,140,46,33,255,
    187,212,29,98,102,47,125,
    80,232,235,19,180,106,219
    },
};

    private static void InitStrategicRowA()
    {
        SECTORINFO pSector;

        pSector = SectorInfo[SEC.A1];
        pSector.ubTravelRating = 10;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TROPICS;

        pSector = SectorInfo[SEC.A2];
        pSector.ubTravelRating = 60;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.A3];
        pSector.ubTravelRating = 9;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS;

        pSector = SectorInfo[SEC.A4];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.EDGEOFWORLD;

        pSector = SectorInfo[SEC.A5];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.EDGEOFWORLD;

        pSector = SectorInfo[SEC.A6];
        pSector.ubTravelRating = 5;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.HILLS;

        pSector = SectorInfo[SEC.A7];
        pSector.ubTravelRating = 4;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.HILLS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.HILLS;

        pSector = SectorInfo[SEC.A8];
        pSector.ubTravelRating = 14;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.HILLS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.HILLS;

        pSector = SectorInfo[SEC.A9];
        pSector.ubTravelRating = 70;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.A10];
        pSector.ubTravelRating = 70;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.A11];
        pSector.ubTravelRating = 18;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.FARMLAND;

        pSector = SectorInfo[SEC.A12];
        pSector.ubTravelRating = 4;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.DENSE;

        pSector = SectorInfo[SEC.A13];
        pSector.ubTravelRating = 14;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE;

        pSector = SectorInfo[SEC.A14];
        pSector.ubTravelRating = 10;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS;

        pSector = SectorInfo[SEC.A15];
        pSector.ubTravelRating = 8;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.DENSE;

        pSector = SectorInfo[SEC.A16];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.EDGEOFWORLD;
    }

    private static void InitStrategicRowB()
    {
        SECTORINFO pSector;

        pSector = SectorInfo[SEC.B1];
        pSector.ubTravelRating = 10;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TROPICS;

        pSector = SectorInfo[SEC.B2];
        pSector.ubTravelRating = 60;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.B3];
        pSector.ubTravelRating = 6;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE;

        pSector = SectorInfo[SEC.B4];
        pSector.ubTravelRating = 4;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE;

        pSector = SectorInfo[SEC.B5];
        pSector.ubTravelRating = 15;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.FARMLAND;

        pSector = SectorInfo[SEC.B6];
        pSector.ubTravelRating = 15;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.FARMLAND;

        pSector = SectorInfo[SEC.B7];
        pSector.ubTravelRating = 10;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS;

        pSector = SectorInfo[SEC.B8];
        pSector.ubTravelRating = 20;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.FARMLAND;

        pSector = SectorInfo[SEC.B9];
        pSector.ubTravelRating = 70;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS_ROAD;

        pSector = SectorInfo[SEC.B10];
        pSector.ubTravelRating = 50;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE_ROAD;

        pSector = SectorInfo[SEC.B11];
        pSector.ubTravelRating = 50;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE_ROAD;

        pSector = SectorInfo[SEC.B12];
        pSector.ubTravelRating = 50;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.FARMLAND_ROAD;

        pSector = SectorInfo[SEC.B13];
        pSector.ubTravelRating = 85;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.B14];
        pSector.ubTravelRating = 15;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.WATER;

        pSector = SectorInfo[SEC.B15];
        pSector.ubTravelRating = 10;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SWAMP;

        pSector = SectorInfo[SEC.B16];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.DENSE;
    }

    private static void InitStrategicRowC()
    {
        SECTORINFO pSector;

        pSector = SectorInfo[SEC.C1];
        pSector.ubTravelRating = 8;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TROPICS;

        pSector = SectorInfo[SEC.C2];
        pSector.ubTravelRating = 40;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TROPICS_ROAD;

        pSector = SectorInfo[SEC.C3];
        pSector.ubTravelRating = 40;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS_ROAD;

        pSector = SectorInfo[SEC.C4];
        pSector.ubTravelRating = 20;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE;

        pSector = SectorInfo[SEC.C5];
        pSector.ubTravelRating = 80;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.C6];
        pSector.ubTravelRating = 75;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.C7];
        pSector.ubTravelRating = 45;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS_ROAD;

        pSector = SectorInfo[SEC.C8];
        pSector.ubTravelRating = 48;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE_ROAD;

        pSector = SectorInfo[SEC.C9];
        pSector.ubTravelRating = 80;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS_ROAD;

        pSector = SectorInfo[SEC.C10];
        pSector.ubTravelRating = 12;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE;

        pSector = SectorInfo[SEC.C11];
        pSector.ubTravelRating = 3;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.DENSE;

        pSector = SectorInfo[SEC.C12];
        pSector.ubTravelRating = 8;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE;

        pSector = SectorInfo[SEC.C13];
        pSector.ubTravelRating = 60;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.C14];
        pSector.ubTravelRating = 15;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.WATER;

        pSector = SectorInfo[SEC.C15];
        pSector.ubTravelRating = 7;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SWAMP;

        pSector = SectorInfo[SEC.C16];
        pSector.ubTravelRating = 5;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SWAMP;
    }

    private static void InitStrategicRowD()
    {
        SECTORINFO pSector;

        pSector = SectorInfo[SEC.D1];
        pSector.ubTravelRating = 7;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.D2];
        pSector.ubTravelRating = 7;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;//TROPICS_SAMSITE;

        pSector = SectorInfo[SEC.D3];
        pSector.ubTravelRating = 40;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS_ROAD;

        pSector = SectorInfo[SEC.D4];
        pSector.ubTravelRating = 12;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.HILLS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.HILLS;

        pSector = SectorInfo[SEC.D5];
        pSector.ubTravelRating = 49;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.D6];
        pSector.ubTravelRating = 50;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.FARMLAND;

        pSector = SectorInfo[SEC.D7];
        pSector.ubTravelRating = 45;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.DENSE_ROAD;

        pSector = SectorInfo[SEC.D8];
        pSector.ubTravelRating = 16;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.DENSE;

        pSector = SectorInfo[SEC.D9];
        pSector.ubTravelRating = 50;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS_ROAD;

        pSector = SectorInfo[SEC.D10];
        pSector.ubTravelRating = 11;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS;

        pSector = SectorInfo[SEC.D11];
        pSector.ubTravelRating = 5;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE;

        pSector = SectorInfo[SEC.D12];
        pSector.ubTravelRating = 11;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SWAMP;

        pSector = SectorInfo[SEC.D13];
        pSector.ubTravelRating = 60;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.D14];
        pSector.ubTravelRating = 12;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.WATER;

        pSector = SectorInfo[SEC.D15];
        pSector.ubTravelRating = 8;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.D16];
        pSector.ubTravelRating = 5;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.DENSE;
    }

    private static void InitStrategicRowE()
    {
        SECTORINFO pSector;

        pSector = SectorInfo[SEC.E1];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.E2];
        pSector.ubTravelRating = 10;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TROPICS;

        pSector = SectorInfo[SEC.E3];
        pSector.ubTravelRating = 45;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS_ROAD;

        pSector = SectorInfo[SEC.E4];
        pSector.ubTravelRating = 11;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.HILLS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.HILLS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.HILLS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.HILLS;

        pSector = SectorInfo[SEC.E5];
        pSector.ubTravelRating = 9;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.HILLS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.HILLS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.HILLS;

        pSector = SectorInfo[SEC.E6];
        pSector.ubTravelRating = 11;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS;

        pSector = SectorInfo[SEC.E7];
        pSector.ubTravelRating = 50;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.FARMLAND_ROAD;

        pSector = SectorInfo[SEC.E8];
        pSector.ubTravelRating = 15;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.FARMLAND;

        pSector = SectorInfo[SEC.E9];
        pSector.ubTravelRating = 56;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.FARMLAND_ROAD;

        pSector = SectorInfo[SEC.E10];
        pSector.ubTravelRating = 11;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SWAMP;

        pSector = SectorInfo[SEC.E11];
        pSector.ubTravelRating = 9;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE;

        pSector = SectorInfo[SEC.E12];
        pSector.ubTravelRating = 35;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE;

        pSector = SectorInfo[SEC.E13];
        pSector.ubTravelRating = 45;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.WATER;

        pSector = SectorInfo[SEC.E14];
        pSector.ubTravelRating = 8;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SWAMP;

        pSector = SectorInfo[SEC.E15];
        pSector.ubTravelRating = 8;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.DENSE_ROAD;

        pSector = SectorInfo[SEC.E16];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.EDGEOFWORLD;
    }

    private static void InitStrategicRowF()
    {
        SECTORINFO pSector;

        pSector = SectorInfo[SEC.F1];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.F2];
        pSector.ubTravelRating = 4;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.NS_RIVER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.COASTAL;

        pSector = SectorInfo[SEC.F3];
        pSector.ubTravelRating = 40;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.NS_RIVER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS_ROAD;

        pSector = SectorInfo[SEC.F4];
        pSector.ubTravelRating = 9;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.HILLS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.HILLS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.HILLS;

        pSector = SectorInfo[SEC.F5];
        pSector.ubTravelRating = 6;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.HILLS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.HILLS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.HILLS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.HILLS;

        pSector = SectorInfo[SEC.F6];
        pSector.ubTravelRating = 9;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.HILLS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.HILLS;

        pSector = SectorInfo[SEC.F7];
        pSector.ubTravelRating = 50;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.HILLS_ROAD;

        pSector = SectorInfo[SEC.F8];
        pSector.ubTravelRating = 60;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.F9];
        pSector.ubTravelRating = 65;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.F10];
        pSector.ubTravelRating = 15;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.FARMLAND;

        pSector = SectorInfo[SEC.F11];
        pSector.ubTravelRating = 9;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS;

        pSector = SectorInfo[SEC.F12];
        pSector.ubTravelRating = 40;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE_ROAD;

        pSector = SectorInfo[SEC.F13];
        pSector.ubTravelRating = 8;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE;

        pSector = SectorInfo[SEC.F14];
        pSector.ubTravelRating = 12;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS_ROAD;

        pSector = SectorInfo[SEC.F15];
        pSector.ubTravelRating = 3;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.DENSE_ROAD;

        pSector = SectorInfo[SEC.F16];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.EDGEOFWORLD;
    }

    private static void InitStrategicRowG()
    {
        SECTORINFO pSector;

        pSector = SectorInfo[SEC.G1];
        pSector.ubTravelRating = 60;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.G2];
        pSector.ubTravelRating = 7;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.G3];
        pSector.ubTravelRating = 55;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.COASTAL_ROAD;

        pSector = SectorInfo[SEC.G4];
        pSector.ubTravelRating = 65;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.HILLS_ROAD;

        pSector = SectorInfo[SEC.G5];
        pSector.ubTravelRating = 65;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.HILLS_ROAD;

        pSector = SectorInfo[SEC.G6];
        pSector.ubTravelRating = 55;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.HILLS_ROAD;

        pSector = SectorInfo[SEC.G7];
        pSector.ubTravelRating = 55;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.FARMLAND_ROAD;

        pSector = SectorInfo[SEC.G8];
        pSector.ubTravelRating = 65;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.G9];
        pSector.ubTravelRating = 65;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.G10];
        pSector.ubTravelRating = 50;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SAND_ROAD;

        pSector = SectorInfo[SEC.G11];
        pSector.ubTravelRating = 25;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SAND_ROAD;

        pSector = SectorInfo[SEC.G12];
        pSector.ubTravelRating = 55;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS_ROAD;

        pSector = SectorInfo[SEC.G13];
        pSector.ubTravelRating = 65;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS;

        pSector = SectorInfo[SEC.G14];
        pSector.ubTravelRating = 60;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS_ROAD;

        pSector = SectorInfo[SEC.G15];
        pSector.ubTravelRating = 16;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.DENSE;

        pSector = SectorInfo[SEC.G16];
        pSector.ubTravelRating = 4;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SWAMP;
    }

    private static void InitStrategicRowH()
    {
        SECTORINFO pSector;

        pSector = SectorInfo[SEC.H1];
        pSector.ubTravelRating = 67;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.H2];
        pSector.ubTravelRating = 55;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.H3];
        pSector.ubTravelRating = 65;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.H4];
        pSector.ubTravelRating = 8;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.DENSE;

        pSector = SectorInfo[SEC.H5];
        pSector.ubTravelRating = 10;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.HILLS;

        pSector = SectorInfo[SEC.H6];
        pSector.ubTravelRating = 60;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.HILLS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.HILLS_ROAD;

        pSector = SectorInfo[SEC.H7];
        pSector.ubTravelRating = 8;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.HILLS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS;

        pSector = SectorInfo[SEC.H8];
        pSector.ubTravelRating = 15;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.H9];
        pSector.ubTravelRating = 15;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS;

        pSector = SectorInfo[SEC.H10];
        pSector.ubTravelRating = 3;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SAND;

        pSector = SectorInfo[SEC.H11];
        pSector.ubTravelRating = 7;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SAND;

        pSector = SectorInfo[SEC.H12];
        pSector.ubTravelRating = 12;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS_ROAD;

        pSector = SectorInfo[SEC.H13];
        pSector.ubTravelRating = 65;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.H14];
        pSector.ubTravelRating = 65;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.H15];
        pSector.ubTravelRating = 12;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SWAMP;

        pSector = SectorInfo[SEC.H16];
        pSector.ubTravelRating = 3;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE;
    }

    private static void InitStrategicRowI()
    {
        SECTORINFO pSector;

        pSector = SectorInfo[SEC.I1];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.I2];
        pSector.ubTravelRating = 3;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.I3];
        pSector.ubTravelRating = 45;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.COASTAL_ROAD;

        pSector = SectorInfo[SEC.I4];
        pSector.ubTravelRating = 7;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.DENSE;

        pSector = SectorInfo[SEC.I5];
        pSector.ubTravelRating = 5;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE;

        pSector = SectorInfo[SEC.I6];
        pSector.ubTravelRating = 55;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.I7];
        pSector.ubTravelRating = 10;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE;

        pSector = SectorInfo[SEC.I8];
        pSector.ubTravelRating = 5;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.I9];
        pSector.ubTravelRating = 5;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SAND;

        pSector = SectorInfo[SEC.I10];
        pSector.ubTravelRating = 5;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SAND;

        pSector = SectorInfo[SEC.I11];
        pSector.ubTravelRating = 10;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS;

        pSector = SectorInfo[SEC.I12];
        pSector.ubTravelRating = 10;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS;

        pSector = SectorInfo[SEC.I13];
        pSector.ubTravelRating = 10;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.I14];
        pSector.ubTravelRating = 55;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.I15];
        pSector.ubTravelRating = 10;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE;

        pSector = SectorInfo[SEC.I16];
        pSector.ubTravelRating = 2;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE;
    }

    private static void InitStrategicRowJ()
    {
        SECTORINFO pSector;

        pSector = SectorInfo[SEC.J1];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.J2];
        pSector.ubTravelRating = 50;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.COASTAL_ROAD;

        pSector = SectorInfo[SEC.J3];
        pSector.ubTravelRating = 50;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.COASTAL_ROAD;

        pSector = SectorInfo[SEC.J4];
        pSector.ubTravelRating = 4;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SWAMP;

        pSector = SectorInfo[SEC.J5];
        pSector.ubTravelRating = 3;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SWAMP;

        pSector = SectorInfo[SEC.J6];
        pSector.ubTravelRating = 50;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE_ROAD;

        pSector = SectorInfo[SEC.J7];
        pSector.ubTravelRating = 6;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE;

        pSector = SectorInfo[SEC.J8];
        pSector.ubTravelRating = 10;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SAND;

        pSector = SectorInfo[SEC.J9];
        pSector.ubTravelRating = 80;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.J10];
        pSector.ubTravelRating = 10;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SAND;

        pSector = SectorInfo[SEC.J11];
        pSector.ubTravelRating = 15;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SAND;

        pSector = SectorInfo[SEC.J12];
        pSector.ubTravelRating = 10;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE;

        pSector = SectorInfo[SEC.J13];
        pSector.ubTravelRating = 12;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE;

        pSector = SectorInfo[SEC.J14];
        pSector.ubTravelRating = 50;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SPARSE;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.FARMLAND_ROAD;

        pSector = SectorInfo[SEC.J15];
        pSector.ubTravelRating = 10;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE;

        pSector = SectorInfo[SEC.J16];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.EDGEOFWORLD;
    }

    private static void InitStrategicRowK()
    {
        SECTORINFO pSector;

        pSector = SectorInfo[SEC.K1];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.K2];
        pSector.ubTravelRating = 55;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.COASTAL_ROAD;

        pSector = SectorInfo[SEC.K3];
        pSector.ubTravelRating = 4;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SWAMP;

        pSector = SectorInfo[SEC.K4];
        pSector.ubTravelRating = 45;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.K5];
        pSector.ubTravelRating = 15;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SWAMP;

        pSector = SectorInfo[SEC.K6];
        pSector.ubTravelRating = 60;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS_ROAD;

        pSector = SectorInfo[SEC.K7];
        pSector.ubTravelRating = 60;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SAND_ROAD;

        pSector = SectorInfo[SEC.K8];
        pSector.ubTravelRating = 55;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SAND_ROAD;

        pSector = SectorInfo[SEC.K9];
        pSector.ubTravelRating = 55;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SAND_ROAD;

        pSector = SectorInfo[SEC.K10];
        pSector.ubTravelRating = 55;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS_ROAD;

        pSector = SectorInfo[SEC.K11];
        pSector.ubTravelRating = 65;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS_ROAD;

        pSector = SectorInfo[SEC.K12];
        pSector.ubTravelRating = 70;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS_ROAD;

        pSector = SectorInfo[SEC.K13];
        pSector.ubTravelRating = 65;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE_ROAD;

        pSector = SectorInfo[SEC.K14];
        pSector.ubTravelRating = 50;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE_ROAD;

        pSector = SectorInfo[SEC.K15];
        pSector.ubTravelRating = 7;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.DENSE;

        pSector = SectorInfo[SEC.K16];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.EDGEOFWORLD;
    }

    private static void InitStrategicRowL()
    {
        SECTORINFO pSector;

        pSector = SectorInfo[SEC.L1];
        pSector.ubTravelRating = 4;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.COASTAL;

        pSector = SectorInfo[SEC.L2];
        pSector.ubTravelRating = 55;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.COASTAL_ROAD;

        pSector = SectorInfo[SEC.L3];
        pSector.ubTravelRating = 5;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SWAMP;

        pSector = SectorInfo[SEC.L4];
        pSector.ubTravelRating = 15;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SWAMP;

        pSector = SectorInfo[SEC.L5];
        pSector.ubTravelRating = 10;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.DENSE;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.DENSE;

        pSector = SectorInfo[SEC.L6];
        pSector.ubTravelRating = 65;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS_ROAD;

        pSector = SectorInfo[SEC.L7];
        pSector.ubTravelRating = 10;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS;

        pSector = SectorInfo[SEC.L8];
        pSector.ubTravelRating = 7;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SAND;

        pSector = SectorInfo[SEC.L9];
        pSector.ubTravelRating = 8;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS;

        pSector = SectorInfo[SEC.L10];
        pSector.ubTravelRating = 9;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE_ROAD;

        pSector = SectorInfo[SEC.L11];
        pSector.ubTravelRating = 17;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.L12];
        pSector.ubTravelRating = 55;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.L13];
        pSector.ubTravelRating = 18;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.COASTAL;

        pSector = SectorInfo[SEC.L14];
        pSector.ubTravelRating = 7;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SWAMP;

        pSector = SectorInfo[SEC.L15];
        pSector.ubTravelRating = 3;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.DENSE;

        pSector = SectorInfo[SEC.L16];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.EDGEOFWORLD;
    }

    private static void InitStrategicRowM()
    {
        SECTORINFO pSector;

        pSector = SectorInfo[SEC.M1];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.M2];
        pSector.ubTravelRating = 65;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.COASTAL_ROAD;

        pSector = SectorInfo[SEC.M3];
        pSector.ubTravelRating = 70;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SWAMP;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SWAMP_ROAD;

        pSector = SectorInfo[SEC.M4];
        pSector.ubTravelRating = 38;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.FARMLAND;

        pSector = SectorInfo[SEC.M5];
        pSector.ubTravelRating = 70;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.DENSE_ROAD;

        pSector = SectorInfo[SEC.M6];
        pSector.ubTravelRating = 65;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.DENSE_ROAD;

        pSector = SectorInfo[SEC.M7];
        pSector.ubTravelRating = 12;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS;

        pSector = SectorInfo[SEC.M8];
        pSector.ubTravelRating = 8;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.SAND;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS;

        pSector = SectorInfo[SEC.M9];
        pSector.ubTravelRating = 8;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.PLAINS;

        pSector = SectorInfo[SEC.M10];
        pSector.ubTravelRating = 7;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TROPICS_ROAD;

        pSector = SectorInfo[SEC.M11];
        pSector.ubTravelRating = 5;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.M12];
        pSector.ubTravelRating = 12;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.M13];
        pSector.ubTravelRating = 5;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SPARSE;

        pSector = SectorInfo[SEC.M14];
        pSector.ubTravelRating = 2;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.SWAMP;

        pSector = SectorInfo[SEC.M15];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.EDGEOFWORLD;

        pSector = SectorInfo[SEC.M16];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.EDGEOFWORLD;
    }

    private static void InitStrategicRowN()
    {
        SECTORINFO pSector;

        pSector = SectorInfo[SEC.N1];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.N2];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.N3];
        pSector.ubTravelRating = 80;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.N4];
        pSector.ubTravelRating = 80;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.N5];
        pSector.ubTravelRating = 80;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.N6];
        pSector.ubTravelRating = 40;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TROPICS_ROAD;

        pSector = SectorInfo[SEC.N7];
        pSector.ubTravelRating = 20;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.COASTAL_ROAD;

        pSector = SectorInfo[SEC.N8];
        pSector.ubTravelRating = 10;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.COASTAL_ROAD;

        pSector = SectorInfo[SEC.N9];
        pSector.ubTravelRating = 5;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TROPICS_ROAD;

        pSector = SectorInfo[SEC.N10];
        pSector.ubTravelRating = 5;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.ROAD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TROPICS_ROAD;

        pSector = SectorInfo[SEC.N11];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.N12];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.N13];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.EDGEOFWORLD;

        pSector = SectorInfo[SEC.N14];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.EDGEOFWORLD;

        pSector = SectorInfo[SEC.N15];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.EDGEOFWORLD;

        pSector = SectorInfo[SEC.N16];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.EDGEOFWORLD;
    }

    private static void InitStrategicRowO()
    {
        SECTORINFO pSector;

        pSector = SectorInfo[SEC.O1];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.O2];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.O3];
        pSector.ubTravelRating = 90;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.O4];
        pSector.ubTravelRating = 90;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.O5];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.O6];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.O7];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.O8];
        pSector.ubTravelRating = 5;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TROPICS;

        pSector = SectorInfo[SEC.O9];
        pSector.ubTravelRating = 5;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.PLAINS;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TROPICS;

        pSector = SectorInfo[SEC.O10];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.O11];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.O12];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.O13];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.O14];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.EDGEOFWORLD;

        pSector = SectorInfo[SEC.O15];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.EDGEOFWORLD;

        pSector = SectorInfo[SEC.O16];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.EDGEOFWORLD;
    }

    private static void InitStrategicRowP()
    {
        SECTORINFO pSector;

        pSector = SectorInfo[SEC.P1];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;
        //	pSector.ubTraversability[ StrategicMove.THROUGH ] = Traversability.WATER; //keep as water so we can teleport to demo maps.

        pSector = SectorInfo[SEC.P2];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.P3];
        pSector.ubTravelRating = 100;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.TOWN;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.TOWN;

        pSector = SectorInfo[SEC.P4];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.P5];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.P6];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.P7];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.P8];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.P9];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.P10];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.P11];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.P12];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.P13];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.GROUNDBARRIER;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.GROUNDBARRIER;

        pSector = SectorInfo[SEC.P14];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.EDGEOFWORLD;

        pSector = SectorInfo[SEC.P15];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.EDGEOFWORLD;

        pSector = SectorInfo[SEC.P16];
        pSector.ubTravelRating = 0;
        pSector.ubTraversability[StrategicMove.NORTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.EAST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.SOUTH] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.WEST] = Traversability.EDGEOFWORLD;
        pSector.ubTraversability[StrategicMove.THROUGH] = Traversability.EDGEOFWORLD;
    }

    public static void InitStrategicMovementCosts()
    {
        foreach (var sec in Enum.GetValues<SEC>())
        {
            SectorInfo[sec] = new();
        }

        InitStrategicRowA();
        InitStrategicRowB();
        InitStrategicRowC();
        InitStrategicRowD();
        InitStrategicRowE();
        InitStrategicRowF();
        InitStrategicRowG();
        InitStrategicRowH();
        InitStrategicRowI();
        InitStrategicRowJ();
        InitStrategicRowK();
        InitStrategicRowL();
        InitStrategicRowM();
        InitStrategicRowN();
        InitStrategicRowO();
        InitStrategicRowP();
    }

    Traversability GetTraversability(SEC sStartSector, int sEndSector)
    {
        StrategicMove ubDirection = 0;
        int sDifference = 0;

        // given start and end sectors
        sDifference = sEndSector - (int)sStartSector;

        if (sDifference == -1)
        {
            ubDirection = StrategicMove.WEST;
        }
        else if (sDifference == 1)
        {
            ubDirection = StrategicMove.EAST;
        }
        else if (sDifference == 16)
        {
            ubDirection = StrategicMove.SOUTH;
        }
        else
        {
            ubDirection = StrategicMove.NORTH;
        }


        return (SectorInfo[sStartSector].ubTraversability[ubDirection]);
    }

    bool SectorIsImpassable(SEC sSector)
    {
        // returns true if the sector is impassable in all directions
        return (SectorInfo[sSector].ubTraversability[StrategicMove.THROUGH] == Traversability.GROUNDBARRIER ||
            SectorInfo[sSector].ubTraversability[StrategicMove.THROUGH] == Traversability.EDGEOFWORLD);
    }
}
