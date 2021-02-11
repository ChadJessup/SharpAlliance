using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using Veldrid;

namespace SharpAlliance.Core.Screens
{
    public class CreditsScreen : IScreen
    {
        private readonly GuiManager gui;
        private readonly IClockManager clock;
        private readonly GameContext context;
        private readonly IVideoManager video;
        private readonly IInputManager inputs;
        private readonly IFileManager files;
        private string guiCreditFacesKey;
        private string guiCreditBackGroundImageKey;
        private bool gfCreditsScreenEntry;
        private bool gfCreditsScreenExit;
        private ScreenName guiCreditsExitScreen;
        private bool gfCrdtHaveRenderedFirstFrameToSaveBuffer;
        private FontStyle guiCreditScreenActiveFont;
        private FontColor gubCreditScreenActiveColor;
        private FontStyle guiCreditScreenTitleFont;
        private FontColor gubCreditScreenTitleColor;
        private int guiCrdtNodeScrollSpeed;
        private CreditRenderFlag gubCreditScreenRenderFlags;
        private int giCurrentlySelectedFace;
        private bool gfPauseCreditScreen;
        private TextJustifies gubCrdtJustification;
        private int guiCurrentCreditRecord;
        private uint guiCrdtLastTimeUpdatingNode;
        private int guiGapBetweenCreditSections;
        private int guiGapBetweenCreditNodes;
        private int guiGapTillReadNextCredit;
        private MouseRegion[] gCrdtMouseRegions = new MouseRegion[(int)PeopleInCredits.NUM_PEOPLE_IN_CREDITS];

        private List<CRDT_NODE> gCrdtNodes = new();

        public bool IsInitialized { get; set; }
        public ScreenState State { get; set; }

        public CreditsScreen(
            GameContext gameContext,
            IVideoManager videoManager,
            IInputManager inputManager,
            IClockManager clockManager,
            IFileManager fileManager,
            GuiManager guiManager)
        {
            this.gui = guiManager;
            this.clock = clockManager;
            this.context = gameContext;
            this.video = videoManager;
            this.inputs = inputManager;
            this.files = fileManager;

            Array.Fill(this.gCrdtMouseRegions, null);
        }

        public ValueTask Activate()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask Deactivate()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask<ScreenName> Handle()
        {

            if (this.gfCreditsScreenEntry)
            {
                if (!this.EnterCreditsScreen())
                {
                    this.gfCreditsScreenEntry = false;
                    this.gfCreditsScreenExit = true;
                }
                else
                {
                    this.gfCreditsScreenEntry = false;
                    this.gfCreditsScreenExit = false;
                }

                this.gubCreditScreenRenderFlags = CreditRenderFlag.CRDT_RENDER_ALL;
            }

            this.GetCreditScreenUserInput();
            this.HandleCreditScreen();

            // render buttons marked dirty	
            //	MarkButtonsDirty( );
            //	RenderButtons( ); 

            // render help
            //	RenderFastHelp( );
            //	RenderButtonsFastHelp( );


            //ExecuteBaseDirtyRectQueue();
            //EndFrameBufferRender();


            if (this.gfCreditsScreenExit)
            {
                this.ExitCreditScreen();
                this.gfCreditsScreenEntry = true;
                this.gfCreditsScreenExit = false;
            }

            return ValueTask.FromResult(this.guiCreditsExitScreen);
        }

        private void ExitCreditScreen()
        {
            //Blit the background image
            //	DeleteVideoSurfaceFromIndex( guiCreditBackGroundImage );
            //DeleteVideoObjectFromIndex(guiCreditBackGroundImage);

            //DeleteVideoObjectFromIndex(guiCreditFaces);


            //ShutDown Credit link list
            this.ShutDownCreditList();

            for (PeopleInCredits uiCnt = 0; uiCnt < PeopleInCredits.NUM_PEOPLE_IN_CREDITS; uiCnt++)
            {
                this.inputs.Mouse.MSYS_RemoveRegion(ref this.gCrdtMouseRegions[(int)uiCnt]);
            }

            /*
                //close the text file
                FileClose( ghFile );
                ghFile = 0;
            */
        }

        private void ShutDownCreditList()
        {
            throw new NotImplementedException();
        }

        private void HandleCreditScreen()
        {
            if (this.gubCreditScreenRenderFlags == CreditRenderFlag.CRDT_RENDER_ALL)
            {
                this.RenderCreditScreen();
                this.gubCreditScreenRenderFlags = CreditRenderFlag.CRDT_RENDER_NONE;
            }


            //Handle the Credit linked list
            this.HandleCreditNodes();

            //Handle the blinkng eyes
            this.HandleCreditEyeBlinking();


            //is it time to get a new node
            if (this.gCrdtLastAddedNode == null || (CRDT_START_POS_Y - (this.gCrdtLastAddedNode.sPos.Y + this.gCrdtLastAddedNode.sHeightOfString - 16)) >= this.guiGapTillReadNextCredit)
            {
                //if there are no more credits in the file
                if (!this.GetNextCreditFromTextFile() && this.gCrdtLastAddedNode == null)
                {
                    this.SetCreditsExitScreen(ScreenName.MAINMENU_SCREEN);
                }
            }

            // RestoreExternBackgroundRect(CRDT_NAME_LOC_X, CRDT_NAME_LOC_Y, CRDT_NAME_LOC_WIDTH, (INT16)CRDT_NAME_LOC_HEIGHT);

            if (this.giCurrentlySelectedFace != -1)
            {
                this.video.DrawTextToScreen(EnglishText.gzCreditNames[this.giCurrentlySelectedFace], CRDT_NAME_LOC_X, CRDT_NAME_LOC_Y, CRDT_NAME_LOC_WIDTH, CRDT_NAME_FONT, FontColor.FONT_MCOLOR_WHITE, 0, false, TextJustifies.INVALIDATE_TEXT | TextJustifies.CENTER_JUSTIFIED);
                this.video.DrawTextToScreen(EnglishText.gzCreditNameTitle[this.giCurrentlySelectedFace], CRDT_NAME_LOC_X, CRDT_NAME_TITLE_LOC_Y, CRDT_NAME_LOC_WIDTH, CRDT_NAME_FONT, FontColor.FONT_MCOLOR_WHITE, 0, false, TextJustifies.INVALIDATE_TEXT | TextJustifies.CENTER_JUSTIFIED);
                this.video.DrawTextToScreen(EnglishText.gzCreditNameFunny[this.giCurrentlySelectedFace], CRDT_NAME_LOC_X, CRDT_NAME_FUNNY_LOC_Y, CRDT_NAME_LOC_WIDTH, CRDT_NAME_FONT, FontColor.FONT_MCOLOR_WHITE, 0, false, TextJustifies.INVALIDATE_TEXT | TextJustifies.CENTER_JUSTIFIED);
            }
        }

        private bool GetNextCreditFromTextFile()
        {
            bool fDone = false;
            uint uiStringWidth = 20;
            string zOriginalString;
            string zString = string.Empty;
            string zCodes;
            string? pzNewCode = null;
            uint uiCodeType = 0;
            CreditNodeType uiNodeType = 0;
            uint uiStartLoc = 0;
            CRDT_FLAG__ uiFlags = 0;

            //Get the current Credit record
            uiStartLoc = CREDITS_LINESIZE * (uint)this.guiCurrentCreditRecord;
            if (!this.files.LoadEncryptedDataFromFile(CRDT_NAME_OF_CREDIT_FILE, out zOriginalString, uiStartLoc, CREDITS_LINESIZE))
            {
                //there are no more credits
                return false;
            }

            //Increment to the next crdit record
            this.guiCurrentCreditRecord++;


            //if there are no codes in the string
            if (zOriginalString[0] != CRDT_START_CODE)
            {
                //copy the string
                zString = zOriginalString;
                uiNodeType = CreditNodeType.CRDT_NODE_DEFAULT;
            }
            else
            {
                int uiSizeOfCodes = 0;
                int uiSizeOfSubCode = 0;
                int pzEndCode = 0;
                int uiDistanceIntoCodes = 0;

                //Retrive all the codes from the string
                pzEndCode = zOriginalString.IndexOf(CRDT_END_CODE, 0);


                //Make a string for the codes
                zCodes = zOriginalString;

                //end the setence after the codes
                // zCodes[pzEndCode - zOriginalString + 1] = '\0';

                //Get the size of the codes
                uiSizeOfCodes = pzEndCode + 1;

                //
                //check to see if there is a string, or just codes
                //

                //if the string is the same size as the codes
                if (zOriginalString.Length == uiSizeOfCodes)
                {
                    //there is no string, just codes
                    uiNodeType = CreditNodeType.CRDT_NODE_NONE;
                }

                //else there is a string aswell
                else
                {
                    //copy the main string
                    // zString = zOriginalString[uiSizeOfCodes];

                    uiNodeType = CreditNodeType.CRDT_NODE_DEFAULT;
                }

                //get rid of the start code delimeter
                uiDistanceIntoCodes = 1;

                uiFlags = 0;

                //loop through the string of codes to get all the control codes out
                while (uiDistanceIntoCodes < uiSizeOfCodes)
                {
                    //Determine what kind of code it is, and handle it
                    uiFlags |= this.GetAndHandleCreditCodeFromCodeString(zCodes[uiDistanceIntoCodes]);

                    //get the next code from the string of codes, returns NULL when done
                    pzNewCode = this.GetNextCreditCode(zCodes[uiDistanceIntoCodes], out uiSizeOfSubCode);

                    //if we are done getting the sub codes
                    if (pzNewCode == null)
                    {
                        uiDistanceIntoCodes = uiSizeOfCodes;
                    }
                    else
                    {
                        //else increment by the size of the code
                        uiDistanceIntoCodes += uiSizeOfSubCode;
                    }
                }
            }

            if (uiNodeType != CreditNodeType.CRDT_NODE_NONE)
            {
                //add the node to the list
                this.AddCreditNode(uiNodeType, uiFlags, zString);
            }

            //if any processing of the flags need to be done
            this.HandleCreditFlags(uiFlags);

            return true;
        }

        private CRDT_FLAG__ GetAndHandleCreditCodeFromCodeString(char v)
        {
            throw new NotImplementedException();
        }

        private string? GetNextCreditCode(char v, out int uiSizeOfSubCode)
        {
            throw new NotImplementedException();
        }

        private void AddCreditNode(CreditNodeType uiNodeType, CRDT_FLAG__ uiFlags, string zString)
        {
            throw new NotImplementedException();
        }

        private void HandleCreditFlags(CRDT_FLAG__ uiFlags)
        {
            if (uiFlags.HasFlag(CRDT_FLAG__.TITLE))
            {
            }

            if (uiFlags.HasFlag(CRDT_FLAG__.START_SECTION))
            {
                //		guiCrdtTimeTillReadNextCredit = guiCrdtDelayBetweenNodes;
                this.guiGapTillReadNextCredit = this.guiGapBetweenCreditNodes;
            }

            if (uiFlags.HasFlag(CRDT_FLAG__.END_SECTION))
            {
                //		guiCrdtTimeTillReadNextCredit = guiCrdtDelayBetweenCreditSection;
                this.guiGapTillReadNextCredit = this.guiGapBetweenCreditSections;
            }
        }

        private void HandleCreditEyeBlinking()
        {
        }

        private void HandleCreditNodes()
        {
            long uiCurrentTime = this.clock.GetJA2Clock();

            if (!this.gCrdtNodes.Any())
            {
                return;
            }

            //if the screen is paused, exit
            if (this.gfPauseCreditScreen)
            {
                return;
            }

            if (!(this.clock.GetJA2Clock() - this.guiCrdtLastTimeUpdatingNode > this.guiCrdtNodeScrollSpeed))
            {
                return;
            }

            var toDelete = new List<CRDT_NODE>();

            //loop through all the nodes
            foreach (var node in this.gCrdtNodes)
            {
                this.HandleCurrentCreditNode(node);

                //if the node is to be deleted
                if (node.fDelete)
                {
                    //delete the node
                    toDelete.Add(node);
                }
            }

            for (int i = 0; i < toDelete.Count; i++)
            {
                this.DeleteNode(toDelete[i]);
            }
            //	RestoreExternBackgroundRect( CRDT_TEXT_START_LOC, 0, CRDT_WIDTH_OF_TEXT_AREA, CRDT_LINE_NODE_DISAPPEARS_AT );

            this.guiCrdtLastTimeUpdatingNode = this.clock.GetJA2Clock();
        }

        private void DeleteNode(CRDT_NODE node)
        {
        }

        private void HandleCurrentCreditNode(CRDT_NODE node)
        {
        }

        private void RenderCreditScreen()
        {
            HVOBJECT hPixHandle = this.video.GetVideoObject(this.guiCreditBackGroundImageKey);
            this.video.BltVideoObject(hPixHandle, 0, 0, 0, 0);
            /*
                HVSURFACE hVSurface;

                GetVideoSurface( &hVSurface, guiCreditBackGroundImage );
                BltVideoSurfaceToVideoSurface( ghFrameBuffer, hVSurface, 0, 0, 0, 0, NULL );
            */
            if (!this.gfCrdtHaveRenderedFirstFrameToSaveBuffer)
            {
                this.gfCrdtHaveRenderedFirstFrameToSaveBuffer = true;

                //blit everything to the save buffer ( cause the save buffer can bleed through )
                this.video.BlitBufferToBuffer(0, 0, 640, 480);

                this.gui.Buttons.UnmarkButtonsDirty();
            }

            this.video.InvalidateScreen();
        }

        private void GetCreditScreenUserInput()
        {
            while (this.inputs.DequeueEvent(out var Event))
            {
                var keyEvent = Event!.KeyEvents.LastOrDefault();

                if (keyEvent.Down)
                {
                    switch (keyEvent.Key)
                    {
                        case Key.Escape:
                            //Exit out of the screen
                            this.SetCreditsExitScreen(ScreenName.MAINMENU_SCREEN);
                            break;
                    }
                }
            }
        }

        private void SetCreditsExitScreen(ScreenName screen)
        {
            this.gfCreditsScreenExit = true;

            this.guiCreditsExitScreen = screen;
        }

        private bool EnterCreditsScreen()
        {
            this.video.AddVideoObject("INTERFACE\\Credits.sti", out this.guiCreditBackGroundImageKey);
            this.video.AddVideoObject("INTERFACE\\Credit Faces.sti", out this.guiCreditFacesKey);

            //Initialize the root credit node

            this.guiCreditsExitScreen = ScreenName.CREDIT_SCREEN;
            this.gfCrdtHaveRenderedFirstFrameToSaveBuffer = false;

            this.guiCreditScreenActiveFont = FontStyle.FONT12ARIAL;
            this.gubCreditScreenActiveColor = FontColor.FONT_MCOLOR_DKWHITE;
            this.guiCreditScreenTitleFont = FontStyle.FONT14ARIAL;
            this.gubCreditScreenTitleColor = FontColor.FONT_MCOLOR_RED;
            //	guiCreditScreenActiveDisplayFlags = LEFT_JUSTIFIED;
            this.guiCrdtNodeScrollSpeed = CRDT_NODE_DELAY_AMOUNT;
            this.gubCrdtJustification = TextJustifies.CENTER_JUSTIFIED;
            this.guiCurrentCreditRecord = 0;

            //	guiCrdtTimeTillReadNextCredit = CRDT_DELAY_BN_SECTIONS;
            //	guiCrdtDelayBetweenCreditSection = CRDT_DELAY_BN_SECTIONS;
            //	guiCrdtDelayBetweenNodes = CRDT_DELAY_BN_NODES;

            this.guiCrdtLastTimeUpdatingNode = this.clock.GetJA2Clock();


            this.guiGapBetweenCreditSections = CRDT_SPACE_BN_SECTIONS;
            this.guiGapBetweenCreditNodes = CRDT_SPACE_BN_NODES;
            this.guiGapTillReadNextCredit = CRDT_SPACE_BN_NODES;


            for (PeopleInCredits uiCnt = 0; uiCnt < PeopleInCredits.NUM_PEOPLE_IN_CREDITS; uiCnt++)
            {
                // Make a mouse region
                if (this.gCrdtMouseRegions[(int)uiCnt] is null)
                {
                    this.gCrdtMouseRegions[(int)uiCnt] = new MouseRegion(uiCnt.ToString());
                }

                this.inputs.Mouse.MSYS_DefineRegion(
                    this.gCrdtMouseRegions[(int)uiCnt],
                     new(
                        this.gCreditFaces[(int)uiCnt].sX,
                        this.gCreditFaces[(int)uiCnt].sY,
                        this.gCreditFaces[(int)uiCnt].sX + this.gCreditFaces[(int)uiCnt].sWidth,
                        this.gCreditFaces[(int)uiCnt].sY + this.gCreditFaces[(int)uiCnt].sHeight),
                    MSYS_PRIORITY.NORMAL,
                    Cursor.WWW,
                    this.SelectCreditFaceMovementRegionCallBack,
                    this.SelectCreditFaceRegionCallBack);

                // Add region
                //this.inputs.Mouse.MSYS_AddRegion(&gCrdtMouseRegions[uiCnt]);

                this.inputs.Mouse.MSYS_SetRegionUserData(this.gCrdtMouseRegions[(int)uiCnt], 0, (int)uiCnt);
            }


            //Test Node
            {
                //		AddCreditNode( CRDT_NODE_DEFAULT, L"This is a test" );
            }

            /*
                //open the credit text file
                ghFile = FileOpen( CRDT_NAME_OF_CREDIT_FILE, FILE_ACCESS_READ | FILE_OPEN_EXISTING, FALSE );
                if( !ghFile )
                { 
                    return( FALSE );
                }
            */

            this.giCurrentlySelectedFace = -1;
            this.gfPauseCreditScreen = false;

            this.InitCreditEyeBlinking();

            return true;
        }

        private void SelectCreditFaceRegionCallBack(ref MouseRegion pRegion, MouseCallbackReasons iReason)
        {
            if (iReason.HasFlag(MouseCallbackReasons.INIT))
            {
            }
            else if (iReason.HasFlag(MouseCallbackReasons.LBUTTON_UP))
            {
            }
            else if (iReason.HasFlag(MouseCallbackReasons.RBUTTON_UP))
            {
            }
        }

        private void InitCreditEyeBlinking()
        {

        }

        public void Draw(SpriteRenderer sr, GraphicsDevice gd, CommandList cl)
        {

        }

        private void SelectCreditFaceMovementRegionCallBack(ref MouseRegion pRegion, MouseCallbackReasons iReason)
        {
            if (iReason.HasFlag(MouseCallbackReasons.LOST_MOUSE))
            {
                this.giCurrentlySelectedFace = -1;
            }
            else if (iReason.HasFlag(MouseCallbackReasons.GAIN_MOUSE))
            {
                this.giCurrentlySelectedFace = this.inputs.Mouse.MSYS_GetRegionUserData(ref pRegion, 0);
            }
            else if (iReason.HasFlag(MouseCallbackReasons.MOVE))
            {
            }
        }

        public ValueTask<bool> Initialize()
        {
            this.gfCreditsScreenEntry = true;

            this.video.AddVideoObject("INTERFACE\\Credits.sti", out this.guiCreditBackGroundImageKey);
            this.video.AddVideoObject("INTERFACE\\Credit Faces.sti", out this.guiCreditFacesKey);

            return ValueTask.FromResult(true);
        }

        public void Dispose()
        {
        }

        //
        // Code tokens
        //
        //new codes:
        private const char CRDT_START_CODE = '@';
        private const string CRDT_SEPARATION_CODE = ",";
        private const string CRDT_END_CODE = ";";

        private const char CRDT_DELAY_BN_STRINGS_CODE = 'D';
        private const char CRDT_DELAY_BN_SECTIONS_CODE = 'B';
        private const char CRDT_SCROLL_SPEED = 'S';
        private const char CRDT_FONT_JUSTIFICATION = 'J';
        private const char CRDT_TITLE_FONT_COLOR = 'C';
        private const char CRDT_ACTIVE_FONT_COLOR = 'R';

        //Flags:
        private const char CRDT_TITLE = 'T';
        private const char CRDT_START_OF_SECTION = '{';
        private const char CRDT_END_OF_SECTION = '}';


        private const int CRDT_NAME_LOC_X = 375;
        private const int CRDT_NAME_LOC_Y = 420;
        private const int CRDT_NAME_TITLE_LOC_Y = 435;
        private const int CRDT_NAME_FUNNY_LOC_Y = 450;
        private const int CRDT_NAME_LOC_WIDTH = 260;
        private const int CRDT_NAME_LOC_HEIGHT = 41;// (CRDT_NAME_FUNNY_LOC_Y - CRDT_NAME_LOC_Y + GetFontHeight(CRDT_NAME_FONT ) )
        private const int CRDT_WIDTH_OF_TEXT_AREA = 210;
        private const int CRDT_TEXT_START_LOC = 10;
        private const int CRDT_SCROLL_PIXEL_AMOUNT = 1;
        private const int CRDT_NODE_DELAY_AMOUNT = 25;
        private const int CRDT_DELAY_BN_NODES = 750;
        private const int CRDT_DELAY_BN_SECTIONS = 2500;
        private const int CRDT_SPACE_BN_SECTIONS = 50;
        private const int CRDT_SPACE_BN_NODES = 12;
        private const int CRDT_START_POS_Y = 479;
        private const int CRDT_EYE_WIDTH = 30;
        private const int CRDT_EYE_HEIGHT = 12;
        private const int CRDT_EYES_CLOSED_TIME = 150;

        private const FontStyle CRDT_NAME_FONT = FontStyle.FONT12ARIAL;

        private const int CRDT_LINE_NODE_DISAPPEARS_AT = 0;//20

        private const string CRDT_NAME_OF_CREDIT_FILE = "BINARYDATA\\Credits.edt";

        private const uint CREDITS_LINESIZE = 80 * 2;

        public struct CDRT_FACE
        {
            public int sX;
            public int sY;
            public int sWidth;
            public int sHeight;

            public int sEyeX;
            public int sEyeY;

            public int sMouthX;
            public int sMouthY;

            public int sBlinkFreq;
            public int uiLastBlinkTime;
            public int uiEyesClosedTime;

            public CDRT_FACE(
                int sX,
                int sY,
                int width,
                int height,
                int eyeX,
                int eyeY,
                int mouthX,
                int mouthY,
                int blinkFreq,
                int lastBlinkTime,
                int eyeClosedTime)
            {
                this.sX = sX;
                this.sY = sY;
                this.sWidth = width;
                this.sHeight = height;
                this.sEyeX = eyeX;
                this.sEyeY = eyeY;
                this.sMouthX = mouthX;
                this.sMouthY = mouthY;
                this.sBlinkFreq = blinkFreq;
                this.uiLastBlinkTime = lastBlinkTime;
                this.uiEyesClosedTime = eyeClosedTime;
            }
        }

        private CDRT_FACE[] gCreditFaces = new CDRT_FACE[]
        {
        //  x	    y	    		w		h			
	    new(298, 137,           37, 49, 310, 157,       304, 170,   2500, 0, 0),											//Camfield
	    new(348, 137,           43, 47, 354, 153,       354, 153,   3700, 0, 0),											//Shawn
	    new(407, 132,           30, 50, 408, 151,       410, 164,   3000, 0, 0),											//Kris
	    new(443, 131,           30, 50, 447, 151,       446, 161,   4000, 0, 0),											//Ian
	    new(487, 136,           43, 50, 493, 155,       493, 155,   3500, 0, 0),											//Linda
	    new(529, 145,           43, 50, 536, 164,       536, 164,   4000, 0, 0),											//Eric
	    new(581, 132,           43, 48, 584, 150,       583, 161,   3500, 0, 0),											//Lynn
	    new(278, 211,           36, 51, 283, 232,       283, 241,   3700, 0, 0),											//Norm
	    new(319, 210,           34, 49, 323, 227,       320, 339,   4000, 0, 0),											//George
	    new(358, 211,           38, 49, 364, 226,       361, 239,   3600, 0, 0),											//Andrew Stacey
	    new(396, 200,           42, 50, 406, 220,       403, 230,   4600, 0, 0),											//Scott
	    new(444, 202,           43, 51, 452, 220,       452, 231,   2800, 0, 0),											//Emmons
	    new(493, 188,           36, 51, 501, 207,       499, 217,   4500, 0, 0),											//Dave
	    new(531, 199,           47, 56, 541, 221,       540, 232,   4000, 0, 0),											//Alex
	    new(585, 196,           39, 49, 593, 218,       593, 228,   3500, 0, 0),											//Joey
};
        private CRDT_NODE? gCrdtLastAddedNode;
    }

    //local Defines
    public enum CreditRenderFlag
    {
        CRDT_RENDER_NONE,
        CRDT_RENDER_ALL,
    }

    //type of credits
    public enum CreditNodeType
    {
        CRDT_NODE_NONE,
        CRDT_NODE_DEFAULT,      // scrolls up and off the screen
    };
    //flags for the credits
    //Flags:
    [Flags]
    public enum CRDT_FLAG__
    {
        TITLE = 0x00000001,
        START_SECTION = 0x00000002,
        END_SECTION = 0x00000004,
    }

    public enum PeopleInCredits
    {
        CRDT_CAMFIELD,
        CRDT_SHAWN,
        CRDT_KRIS,
        CRDT_IAN,
        CRDT_LINDA,
        CRDT_ERIC,
        CRDT_LYNN,
        CRDT_NORM,
        CRDT_GEORGE,
        CRDT_STACEY,
        CRDT_SCOTT,
        CRDT_EMMONS,
        CRDT_DAVE,
        CRDT_ALEX,
        CRDT_JOEY,

        NUM_PEOPLE_IN_CREDITS,
    };

    public class CRDT_NODE
    {
        public int uiType;          //the type of node
        public string pString;        //string for the node if the node contains a string
        public int uiFlags;     //various flags
        public Point sPos;            //position of the node on the screen if the node is displaying stuff
        public Point sOldPos;         //position of the node on the screen if the node is displaying stuff
        public int sHeightOfString;      //The height of the displayed string
        public bool fDelete;        //Delete this loop
        public long uiLastTime;  // The last time the node was udated
        public int uiVideoSurfaceImage;
    }
}
