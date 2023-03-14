using System;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int END_FACE_OVERLAY_DELAY = 2000;

    // GLOBAL FOR FACES LISTING
    public static FACETYPE[] gFacesData = new FACETYPE[NUM_FACE_SLOTS];
    public static int guiNumFaces = 0;
    public static bool gfInItemPickupMenu;

    public static RPC_SMALL_FACE_VALUES[] gRPCSmallFaceValues =
    {
        new(9,  8,  8,  24),		// MIGUEL		( 57 )
    	new(8,  8,  7,  24),		// CARLOS		( 58 )
    	new(10, 8,  8,  26),		// IRA			( 59 )
    	new( 7, 8,  7,  26),		// DIMITRI	( 60 )
    	new( 6, 7,  7,  23),		// DEVIN		( 61 )
    	new( 0, 0,  0,   0),		// THE RAT	( 62 )
    	new( 8, 7,  8,  23),		//					( 63 )
    	new( 8, 8,  8,  22),		// SLAY			( 64 )
    	new( 0, 0,  0,   0),		//					( 65 )
    	new( 9, 4,  7,  22),		// DYNAMO		( 66 )
    	new( 8, 8,  8,  25),		// SHANK		( 67 )
    	new( 4, 6,  5,  22),		// IGGY			( 68 )
    	new( 8, 9,  7,  25),		// VINCE		( 69 )
    	new( 4, 7,  5,  25),		// CONRAD		( 70 )
    	new( 9, 7,  8,  22),		// CARL			( 71 )
    	new( 9, 7,  9,  25),		// MADDOG		( 72 )
    	new( 0, 0,  0,   0),		//					( 73 )	
    	new( 0, 0,  0,   0),		//					( 74 )
    	new( 9, 3,  8,  23),		// MARIA		( 88 )
    	new( 9, 3,  8,  25),		// JOEY			( 90 )
    	new(11, 7,  9,  24),		// SKYRIDER	( 97 )
    	new(9,  5,  7,  23),		// Miner	( 106 )
    	new( 6, 4,  6,  24),		// JOHN					( 118 )
    	new( 12,4,  10, 24),			//					( 119 )
    	new(8,  6,  8,  23),		// Miner	( 148 )
    	new(6,  5,  6,  23),		// Miner	( 156 )
    	new(13, 7,  11, 24),		// Miner	( 157 )
    	new(9,  7,  8,  22),		// Miner	( 158 )
    };

    public static NPCID[] gubRPCSmallFaceProfileNum =
    {
        (NPCID)57, // entry 0
    	(NPCID)58,
        (NPCID)59,
        (NPCID)60,
        (NPCID)61,
        (NPCID)62,
        (NPCID)63,
        (NPCID)64,
        (NPCID)65,
        (NPCID)66, // entry 9
    	(NPCID)67,
        (NPCID)68,
        (NPCID)69,
        (NPCID)70,
        (NPCID)71,
        (NPCID)72,
        (NPCID)73,
        (NPCID)74,
        (NPCID)88,
        (NPCID)90, // entry 19
    	(NPCID)97,
        (NPCID)106,
        (NPCID)118,
        (NPCID)119,
        (NPCID)148, // entry 24
    	(NPCID)156,
        (NPCID)157,
        (NPCID)158,
    };

    public static int ubRPCNumSmallFaceValues = 28;
    public static int gusSMCurrentMerc;
    public static bool gfSMDisableForItems;
    public static bool gfRerenderInterfaceFromHelpText;

    // Defines
    public const int NUM_FACE_SLOTS = 50;
    public const uint FACE_AUTO_DISPLAY_BUFFER = 0xFFFFF000;
    public const uint FACE_AUTO_RESTORE_BUFFER = 0xFFFFFF00;
    public const uint FACE_NO_RESTORE_BUFFER = 0xFFFFFFF0;

    // duration for talking
    public const int FINAL_TALKING_DURATION = 2000;
}

// FLAGS....
[Flags]
public enum FACE
{
    DESTROY_OVERLAY = 0x00000000,                     // A face may contain a video overlay
    BIGFACE = 0x00000001,                    // A BIGFACE instead of small face
    POTENTIAL_KEYWAIT = 0x00000002,                       // If the option is set, will not stop face until key pressed
    PCTRIGGER_NPC = 0x00000004,                       // This face has to trigger an NPC after being done
    INACTIVE_HANDLED_ELSEWHERE = 0x00000008,                      // This face has been setup and any disable should be done
                                                                  // Externally
    TRIGGER_PREBATTLE_INT = 0x00000010,
    SHOW_WHITE_HILIGHT = 0x00000020,                      // Show highlight around face
    FORCE_SMALL = 0x00000040,                     // force to small face	
    MODAL = 0x00000080,                       // make game modal
    MAKEACTIVE_ONCE_DONE = 0x00000100,
    SHOW_MOVING_HILIGHT = 0x00000200,
    REDRAW_WHOLE_FACE_NEXT_FRAME = 0x00000400,                        // Redraw the complete face next frame

    // chad: technically won't fit in with the bitfields above, but shouldn't(?) cause any problems.
    FACE_DRAW_TEXT_OVER = 2,
    FACE_ERASE_TEXT_OVER = 1,
    FACE_NO_TEXT_OVER = 0,
}

public class AUDIO_GAP
{
    public int uiStart;
    public int uiEnd;
    public AUDIO_GAP? pNext;
};

public class AudioGapList
{
    // This is a structure that will contain data about the gaps in a particular
    // wave file

    int size; /* the number of entries in the list of AUDIO_GAPs itself*/
    int current_time;
    // Pointer to head and current entry of gap list
    AUDIO_GAP? pHead;
    AUDIO_GAP? pCurrent;

    bool audio_gap_active;

}

public class FACETYPE
{
    public int iID;
    public bool fAllocated;                                         //Allocated or not
    public bool fTalking;                                               //Set to true if face is talking ( can be sitting for user input to esc )
    public bool fAnimatingTalking;                          // Set if the face is animating right now
    public bool fDisabled;                                          // Not active
    public bool fValidSpeech;
    public bool fStartFrame;                                        // Flag for the first start frame
    public bool fInvalidAnim;
    public FACE uiFlags;                                             // Basic flags 
    public int uiTalkingDuration;                           // A delay based on text length for how long to talk if no speech
    public uint uiTalkingTimer;                                  // A timer to handle delay when no speech file
    public int uiTalkingFromVeryBeginningTimer;// Timer from very beginning of talking...	
    public bool fFinishTalking;                                 // A flag to indicate we want to delay after speech done
    public int iVideoOverlay;                                    // Value for video overlay ( not used too much )
    public int uiSoundID;                                           // Sound ID if one being played
    public int ubSoldierID;                                      // SoldierID if one specified
    public NPCID ubCharacterNum;                                   // Profile ID num
    public int usFaceX;                                             // X location to render face
    public int usFaceY;                                             // Y location to render face
    public int usFaceWidth;
    public int usFaceHeight;
    public Surfaces uiAutoDisplayBuffer;                     // Display buffer for face
    public Surfaces uiAutoRestoreBuffer;                     // Restore buffer
    public bool fAutoRestoreBuffer;                         // Flag to indicate our own restorebuffer or not
    public bool fAutoDisplayBuffer;                         // Flag to indicate our own display buffer or not
    public bool fDisplayTextOver;                               // Boolean indicating to display text on face
    public bool fOldDisplayTextOver;                        // OLD Boolean indicating to display text on face
    public bool fCanHandleInactiveNow;
    public int[] zDisplayText = new int[30];                         // String of text that can be displayed
    public int usEyesX;
    public int usEyesY;
    public int usEyesOffsetX;
    public int usEyesOffsetY;
    public int usEyesWidth;
    public int usEyesHeight;
    public int usMouthX;
    public int usMouthY;
    public int usMouthOffsetX;
    public int usMouthOffsetY;
    public int usMouthWidth;
    public int usMouthHeight;
    public int sEyeFrame;
    public int ubEyeWait;
    public uint uiEyelast;
    public int uiEyeDelay;
    public int uiBlinkFrequency;
    public int uiExpressionFrequency;
    public int uiStopOverlayTimer;
    public Expression ubExpression;
    public int bOldSoldierLife;
    public int bOldActionPoints;
    public bool fOldHandleUIHatch;
    public bool fOldShowHighlight;
    public int bOldAssignment;
    public int ubOldServiceCount;
    public int ubOldServicePartner;
    public bool fOldShowMoveHilight;
    public int sMouthFrame;
    public int uiMouthlast;
    public int uiMouthDelay;
    public uint uiLastBlink;
    public uint uiLastExpression;
    public int uiVideoObject;
    public int uiUserData1;
    public int uiUserData2;
    public bool fCompatibleItems;
    public bool fOldCompatibleItems;
    public bool bOldStealthMode;
    public int bOldOppCnt;
    public AudioGapList GapList;
}

public enum Expression
{
    NO_EXPRESSION = 0,
    BLINKING = 1,
    ANGRY = 2,
    SURPRISED = 3,
}
