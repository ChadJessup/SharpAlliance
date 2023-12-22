using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Platform;
using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Screens;

public class CreditsScreen : IScreen
{
    private readonly GuiManager gui;
    private readonly IClockManager clock;
    private readonly GameContext context;
    private readonly IInputManager inputs;
    private readonly IFileManager files;
    private readonly FontSubSystem fonts;
    private readonly IVideoManager video;

    public bool IsInitialized { get; set; }
    public ScreenState State { get; set; }

    public CreditsScreen(
        GameContext gameContext,
        IInputManager inputManager,
        IClockManager clockManager,
        IFileManager fileManager,
        IVideoManager videoManager,
        GuiManager guiManager,
        FontSubSystem fontSubSystem)
    {
        this.fonts = fontSubSystem;
        this.video = videoManager;
        this.gui = guiManager;
        this.clock = clockManager;
        this.context = gameContext;
        this.inputs = inputManager;
        this.files = fileManager;

        Array.Fill(Globals.gCrdtMouseRegions, null);
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

        if (Globals.gfCreditsScreenEntry)
        {
            if (!this.EnterCreditsScreen())
            {
                Globals.gfCreditsScreenEntry = false;
                Globals.gfCreditsScreenExit = true;
            }
            else
            {
                Globals.gfCreditsScreenEntry = false;
                Globals.gfCreditsScreenExit = false;
            }

            Globals.gubCreditScreenRenderFlags = CreditRenderFlag.CRDT_RENDER_ALL;
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


        if (Globals.gfCreditsScreenExit)
        {
            this.ExitCreditScreen();
            Globals.gfCreditsScreenEntry = true;
            Globals.gfCreditsScreenExit = false;
        }

        return ValueTask.FromResult(Globals.guiCreditsExitScreen);
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
            MouseSubSystem.MSYS_RemoveRegion(Globals.gCrdtMouseRegions[(int)uiCnt]);
        }

        /*
            //close the text file
            FileClose( ghFile );
            ghFile = 0;
        */
    }

    private void ShutDownCreditList()
    {
        for (int i = 0; i < Globals.gCrdtNodes.Count; i++)
        {
            Globals.gCrdtNodes.RemoveAt(i);
        }
    }

    private void HandleCreditScreen()
    {
        if (Globals.gubCreditScreenRenderFlags == CreditRenderFlag.CRDT_RENDER_ALL)
        {
            this.RenderCreditScreen();
            Globals.gubCreditScreenRenderFlags = CreditRenderFlag.CRDT_RENDER_NONE;
        }


        //Handle the Credit linked list
        this.HandleCreditNodes();

        //Handle the blinkng eyes
        this.HandleCreditEyeBlinking();


        //is it time to get a new node
        if (Globals.gCrdtLastAddedNode == null || (CRDT_START_POS_Y - (Globals.gCrdtLastAddedNode.sPos.Y + Globals.gCrdtLastAddedNode.sHeightOfString - 16)) >= Globals.guiGapTillReadNextCredit)
        {
            //if there are no more credits in the file
            if (!this.GetNextCreditFromTextFile() && Globals.gCrdtLastAddedNode == null)
            {
                this.SetCreditsExitScreen(ScreenName.MAINMENU_SCREEN);
            }
        }

        // RestoreExternBackgroundRect(CRDT_NAME_LOC_X, CRDT_NAME_LOC_Y, CRDT_NAME_LOC_WIDTH, (int)CRDT_NAME_LOC_HEIGHT);

        if (Globals.giCurrentlySelectedFace != -1)
        {
            FontSubSystem.DrawTextToScreen(EnglishText.gzCreditNames[Globals.giCurrentlySelectedFace], new(CRDT_NAME_LOC_X, CRDT_NAME_LOC_Y), CRDT_NAME_LOC_WIDTH, CRDT_NAME_FONT, FontColor.FONT_MCOLOR_WHITE, 0, TextJustifies.INVALIDATE_TEXT | TextJustifies.CENTER_JUSTIFIED);
            FontSubSystem.DrawTextToScreen(EnglishText.gzCreditNameTitle[Globals.giCurrentlySelectedFace], new(CRDT_NAME_LOC_X, CRDT_NAME_TITLE_LOC_Y), CRDT_NAME_LOC_WIDTH, CRDT_NAME_FONT, FontColor.FONT_MCOLOR_WHITE, 0, TextJustifies.INVALIDATE_TEXT | TextJustifies.CENTER_JUSTIFIED);
            FontSubSystem.DrawTextToScreen(EnglishText.gzCreditNameFunny[Globals.giCurrentlySelectedFace], new(CRDT_NAME_LOC_X, CRDT_NAME_FUNNY_LOC_Y), CRDT_NAME_LOC_WIDTH, CRDT_NAME_FONT, FontColor.FONT_MCOLOR_WHITE, 0, TextJustifies.INVALIDATE_TEXT | TextJustifies.CENTER_JUSTIFIED);
        }
    }

    private bool GetNextCreditFromTextFile()
    {
        bool fDone = false;
        uint uiStringWidth = 20;
        string zString = string.Empty;
        string zCodes;
        string? pzNewCode = null;
        uint uiCodeType = 0;
        CRDT_FLAG__ uiNodeType = 0;
        uint uiStartLoc = 0;
        CRDT_FLAG__ uiFlags = 0;

        //Get the current Credit record
        uiStartLoc = CREDITS_LINESIZE * (uint)Globals.guiCurrentCreditRecord;
        if (!files.LoadEncryptedDataFromFile(CRDT_NAME_OF_CREDIT_FILE, out string zOriginalString, uiStartLoc, CREDITS_LINESIZE))
        {
            //there are no more credits
            return false;
        }

        //Increment to the next crdit record
        Globals.guiCurrentCreditRecord++;

        try
        {
            //if there are no codes in the string
            if (zOriginalString.Length > 0 && zOriginalString[0] != CRDT_START_CODE)
            {
                //copy the string
                zString = zOriginalString;
                uiNodeType = CRDT_FLAG__.CRDT_NODE_DEFAULT;
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
                    uiNodeType = CRDT_FLAG__.CRDT_NODE_NONE;
                }

                //else there is a string aswell
                else
                {
                    //copy the main string
                    // zString = zOriginalString[uiSizeOfCodes];

                    uiNodeType = CRDT_FLAG__.CRDT_NODE_DEFAULT;
                }

                //get rid of the start code delimeter
                uiDistanceIntoCodes = 1;

                uiFlags = 0;

                //loop through the string of codes to get all the control codes out
                while (uiDistanceIntoCodes < uiSizeOfCodes)
                {
                    //Determine what kind of code it is, and handle it
                    uiFlags |= this.GetAndHandleCreditCodeFromCodeString(zCodes.Substring(uiDistanceIntoCodes));

                    //get the next code from the string of codes, returns null when done
                    pzNewCode = this.GetNextCreditCode(zCodes.Substring(uiDistanceIntoCodes), out uiSizeOfSubCode);

                    //if we are done getting the sub codes
                    if (pzNewCode == null)
                    {
                        uiDistanceIntoCodes = uiSizeOfCodes;
                    }
                    else
                    {
                        //else increment by the size of the code
                        uiDistanceIntoCodes += uiSizeOfSubCode + 1;
                    }
                }
            }
        }
        catch(Exception e)
        {

        }

        if (uiNodeType != CRDT_FLAG__.CRDT_NODE_NONE)
        {
            //add the node to the list
            this.AddCreditNode(uiNodeType, uiFlags, zString);
        }

        //if any processing of the flags need to be done
        this.HandleCreditFlags(uiFlags);

        return true;
    }

    private CRDT_FLAG__ GetAndHandleCreditCodeFromCodeString(string pzCode)
    {
        //if the code is to change the delay between strings
        if (pzCode[0] == CRDT_DELAY_BN_STRINGS_CODE)
        {
            //Get the delay from the string
            int uiNewDelay = int.Parse(pzCode.Substring(1, pzCode.IndexOf(CRDT_SEPARATION_CODE) - 1));
            //swscanf(pzCode[1], "%d%*s", out int uiNewDelay);

            Globals.guiGapBetweenCreditNodes = uiNewDelay;

            return CRDT_FLAG__.CRDT_NODE_NONE;
        }

        //if the code is to change the delay between sections strings
        else if (pzCode[0] == CRDT_DELAY_BN_SECTIONS_CODE)
        {

            //Get the delay from the string
            var idx = pzCode.IndexOf(CRDT_SEPARATION_CODE);
            if (idx == -1)
            {
                idx = pzCode.IndexOf(CRDT_END_CODE);
            }

            int uiNewDelay = int.Parse(pzCode.Substring(1, idx - 1));
            //swscanf(&pzCode[1], "%d%*s", uiNewDelay);

            // guiCrdtDelayBetweenCreditSection = uiNewDelay;
            Globals.guiGapBetweenCreditSections = uiNewDelay;

            return CRDT_FLAG__.CRDT_NODE_NONE;
        }


        else if (pzCode[0] == CRDT_SCROLL_SPEED)
        {
            int uiScrollSpeed = int.Parse(pzCode.Substring(1, pzCode.IndexOf(CRDT_SEPARATION_CODE) - 1));

            //Get the delay from the string
            Globals.guiCrdtNodeScrollSpeed = uiScrollSpeed;

            return CRDT_FLAG__.CRDT_NODE_NONE;
        }

        else if (pzCode[0] == CRDT_FONT_JUSTIFICATION)
        {
            int uiJustification = int.Parse(pzCode.Substring(1, pzCode.IndexOf(CRDT_SEPARATION_CODE) - 1));

            //Get the delay from the string
            // swscanf(&pzCode[1], "%d%*s", &uiJustification);

            //get the justification
            switch (uiJustification)
            {
                case 0:
                    Globals.gubCrdtJustification = TextJustifies.LEFT_JUSTIFIED;
                    break;
                case 1:
                    Globals.gubCrdtJustification = TextJustifies.CENTER_JUSTIFIED;
                    break;
                case 2:
                    Globals.gubCrdtJustification = TextJustifies.RIGHT_JUSTIFIED;
                    break;
                default:
                    break;
                    // Assert(0);
            }

            return CRDT_FLAG__.CRDT_NODE_NONE;
        }

        else if (pzCode[0] == CRDT_TITLE_FONT_COLOR)
        {
            //Get the new color for the title
            Globals.gubCreditScreenTitleColor = Enum.Parse<FontColor>(pzCode.Substring(1, pzCode.IndexOf(CRDT_SEPARATION_CODE) - 1));
            //swscanf(&pzCode[1], "%d%*s", &gubCreditScreenTitleColor);

            return CRDT_FLAG__.CRDT_NODE_NONE;
        }

        else if (pzCode[0] == CRDT_ACTIVE_FONT_COLOR)
        {
            //Get the new color for the active text
            Globals.gubCreditScreenActiveColor = Enum.Parse<FontColor>(pzCode.Substring(1, pzCode.IndexOf(CRDT_SEPARATION_CODE) - 1));
            // swscanf(&pzCode[1], "%d%*s", &gubCreditScreenActiveColor);

            return CRDT_FLAG__.CRDT_NODE_NONE;
        }


        //else its the title code
        else if (pzCode[0] == CRDT_TITLE)
        {
            return CRDT_FLAG__.TITLE;
        }

        //else its the title code
        else if (pzCode[0] == CRDT_START_OF_SECTION)
        {
            return CRDT_FLAG__.START_SECTION;
        }

        //else its the title code
        else if (pzCode[0] == CRDT_END_OF_SECTION)
        {
            return CRDT_FLAG__.END_SECTION;
        }

        //else its an error
        else
        {
            // Assert(0);
        }

        return CRDT_FLAG__.CRDT_NODE_NONE;
    }

    private string? GetNextCreditCode(string pString, out int pSizeOfCode)
    {
        string? pzNewCode = null;
        char currentChar;
        int uiSizeOfCode = 0;
        int idx = 0;
        //get the new subcode out
        var codeSeparationIdx = pString.IndexOf(CRDT_SEPARATION_CODE);

        if (codeSeparationIdx != -1)
        {
            pzNewCode = pString.Substring(codeSeparationIdx);
            currentChar = pzNewCode[idx];
        }
        else
        {
            pzNewCode = null;
        }


        //if there is no separation code, then there must be an end code
        if (pzNewCode == null)
        {
            //pzNewCode = wcsstr( pString, CRDT_END_CODE );

            //we are done
            pzNewCode = null;
        }
        else
        {
            //get rid of separeation code
            currentChar = pzNewCode[++idx];


            //calc size of sub string
            uiSizeOfCode = pString.Length - pzNewCode.Length;
        }

        pSizeOfCode = uiSizeOfCode;
        return pzNewCode;
    }

    private bool AddCreditNode(CRDT_FLAG__ uiType, CRDT_FLAG__ uiFlags, string pString)
    {
        CRDT_NODE? pNodeToAdd = null;
        CRDT_NODE? pTemp = null;
        int uiSizeOfString = (pString.Length + 2) * 2;
        FontStyle uiFontToUse;
        FontColor uiColorToUse;

        //if
        if (uiType == CRDT_FLAG__.CRDT_NODE_NONE)
        {
            //Assert( 0 );
            return true;
        }

        pNodeToAdd = new();

        //Determine the font and the color to use
        if (uiFlags.HasFlag(CRDT_FLAG__.TITLE))
        {
            uiFontToUse = Globals.guiCreditScreenTitleFont;
            uiColorToUse = Globals.gubCreditScreenTitleColor;
        }
        /*
            else if (.uiFlags.HasFlag(CRDT_FLAG__START_SECTION ))
            {
                uiFontToUse = ;
                uiColorToUse = ;
            }
            else if (.uiFlags.HasFlag(CRDT_FLAG__END_SECTION ))
            {
                uiFontToUse = ;
                uiColorToUse = ;
            }
        */
        else
        {
            uiFontToUse = Globals.guiCreditScreenActiveFont;
            uiColorToUse = Globals.gubCreditScreenActiveColor;
        }

        //
        // Set some default data
        // 

        //the type of the node
        pNodeToAdd.uiType = uiType;

        //any flags that are added
        pNodeToAdd.uiFlags = uiFlags;

        //the starting left position for the it
        pNodeToAdd.sPos.X = CRDT_TEXT_START_LOC;

        //copy the string into the node
        pNodeToAdd.pString = pString;

        //Calculate the height of the string
        pNodeToAdd.sHeightOfString = FontSubSystem.DisplayWrappedString(
            new(0, 0),
            CRDT_WIDTH_OF_TEXT_AREA,
            2,
            uiFontToUse,
            uiColorToUse,
            pNodeToAdd.pString,
            0,
            TextJustifies.DONT_DISPLAY_TEXT) + 1;

        //starting y position on the screen
        pNodeToAdd.sPos.Y = CRDT_START_POS_Y;

        //	pNodeToAdd.uiLastTime = GetJA2Clock();

        //if the node can have something to display, Create a surface for it
        if (pNodeToAdd.uiType == CRDT_FLAG__.CRDT_NODE_DEFAULT)
        {
            VSURFACE_DESC vs_desc = new()
            {
                // Create a background video surface to blt the face onto
                fCreateFlags = VSurfaceCreateFlags.VSURFACE_CREATE_DEFAULT | VSurfaceCreateFlags.VSURFACE_SYSTEM_MEM_USAGE,
                usWidth = CRDT_WIDTH_OF_TEXT_AREA,
                usHeight = pNodeToAdd.sHeightOfString,
                ubBitDepth = 16
            };

//            if (video.TryCreateVideoSurface(vs_desc, out pNodeToAdd.uiVideoSurfaceImage))
//            {
//                return false;
//            }

            //Set transparency
            video.SetVideoSurfaceTransparency(pNodeToAdd.uiVideoSurfaceImage, new Rgba32(0, 0, 0));

            //fill the surface with a transparent color

            //set the font dest buffer to be the surface
            this.fonts.SetFontDestBuffer(pNodeToAdd.uiVideoSurfaceImage, 0, 0, CRDT_WIDTH_OF_TEXT_AREA, pNodeToAdd.sHeightOfString, false);

            //write the string onto the surface
            FontSubSystem.DisplayWrappedString(new SixLabors.ImageSharp.Point(0, 1), CRDT_WIDTH_OF_TEXT_AREA, 2, uiFontToUse, uiColorToUse, pNodeToAdd.pString, 0, Globals.gubCrdtJustification);

            //reset the font dest buffer
            this.fonts.SetFontDestBuffer(SurfaceType.FRAME_BUFFER, 0, 0, 640, 480, false);
        }

        //
        // Insert the node into the list
        //

        //Add the new node to the list
        Globals.gCrdtNodes.Add(pNodeToAdd);
        Globals.gCrdtLastAddedNode = pNodeToAdd;

        return true;
    }

    private void HandleCreditFlags(CRDT_FLAG__ uiFlags)
    {
        if (uiFlags.HasFlag(CRDT_FLAG__.TITLE))
        {
        }

        if (uiFlags.HasFlag(CRDT_FLAG__.START_SECTION))
        {
            //		guiCrdtTimeTillReadNextCredit = guiCrdtDelayBetweenNodes;
            Globals.guiGapTillReadNextCredit = Globals.guiGapBetweenCreditNodes;
        }

        if (uiFlags.HasFlag(CRDT_FLAG__.END_SECTION))
        {
            //		guiCrdtTimeTillReadNextCredit = guiCrdtDelayBetweenCreditSection;
            Globals.guiGapTillReadNextCredit = Globals.guiGapBetweenCreditSections;
        }
    }

    private void HandleCreditEyeBlinking()
    {
    }

    private void HandleCreditNodes()
    {
        long uiCurrentTime = Globals.GetJA2Clock();

        if (!Globals.gCrdtNodes.Any())
        {
            return;
        }

        //if the screen is paused, exit
        if (Globals.gfPauseCreditScreen)
        {
            return;
        }

        if (!(Globals.GetJA2Clock() - Globals.guiCrdtLastTimeUpdatingNode > Globals.guiCrdtNodeScrollSpeed))
        {
            return;
        }

        var toDelete = new List<CRDT_NODE>();

        //loop through all the nodes
        foreach (var node in Globals.gCrdtNodes)
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

        Globals.guiCrdtLastTimeUpdatingNode = Globals.GetJA2Clock();
    }

    private void DeleteNode(CRDT_NODE node)
    {
    }

    private void HandleCurrentCreditNode(CRDT_NODE node)
    {
    }

    private void RenderCreditScreen()
    {
        HVOBJECT hPixHandle = video.GetVideoObject(Globals.guiCreditBackGroundImageKey);
        video.BltVideoObject(hPixHandle, 0, 0, 0, 0);
        /*
            HVSURFACE hVSurface;

            GetVideoSurface( &hVSurface, guiCreditBackGroundImage );
            BltVideoSurfaceToVideoSurface( ghFrameBuffer, hVSurface, 0, 0, 0, 0, null );
        */
        if (!Globals.gfCrdtHaveRenderedFirstFrameToSaveBuffer)
        {
            Globals.gfCrdtHaveRenderedFirstFrameToSaveBuffer = true;

            //blit everything to the save buffer ( cause the save buffer can bleed through )
            video.BlitBufferToBuffer(SurfaceType.RENDER_BUFFER, SurfaceType.SAVE_BUFFER, 0, 0, 640, 480);

            ButtonSubSystem.UnmarkButtonsDirty();
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
        Globals.gfCreditsScreenExit = true;

        Globals.guiCreditsExitScreen = screen;
    }

    private bool EnterCreditsScreen()
    {
        this.video.GetVideoObject("INTERFACE\\Credits.sti", out Globals.guiCreditBackGroundImageKey);
        this.video.GetVideoObject("INTERFACE\\Credit Faces.sti", out Globals.guiCreditFacesKey);

        //Initialize the root credit node

        Globals.guiCreditsExitScreen = ScreenName.CREDIT_SCREEN;
        Globals.gfCrdtHaveRenderedFirstFrameToSaveBuffer = false;

        Globals.guiCreditScreenActiveFont = FontStyle.FONT12ARIAL;
        Globals.gubCreditScreenActiveColor = FontColor.FONT_MCOLOR_DKWHITE;
        Globals.guiCreditScreenTitleFont = FontStyle.FONT14ARIAL;
        Globals.gubCreditScreenTitleColor = FontColor.FONT_MCOLOR_RED;
        //	guiCreditScreenActiveDisplayFlags = LEFT_JUSTIFIED;
        Globals.guiCrdtNodeScrollSpeed = CRDT_NODE_DELAY_AMOUNT;
        Globals.gubCrdtJustification = TextJustifies.CENTER_JUSTIFIED;
        Globals.guiCurrentCreditRecord = 0;

        //	guiCrdtTimeTillReadNextCredit = CRDT_DELAY_BN_SECTIONS;
        //	guiCrdtDelayBetweenCreditSection = CRDT_DELAY_BN_SECTIONS;
        //	guiCrdtDelayBetweenNodes = CRDT_DELAY_BN_NODES;

        Globals.guiCrdtLastTimeUpdatingNode = Globals.GetJA2Clock();


        Globals.guiGapBetweenCreditSections = CRDT_SPACE_BN_SECTIONS;
        Globals.guiGapBetweenCreditNodes = CRDT_SPACE_BN_NODES;
        Globals.guiGapTillReadNextCredit = CRDT_SPACE_BN_NODES;


        for (PeopleInCredits uiCnt = 0; uiCnt < PeopleInCredits.NUM_PEOPLE_IN_CREDITS; uiCnt++)
        {
            // Make a mouse region
            if (Globals.gCrdtMouseRegions[(int)uiCnt] is null)
            {
                Globals.gCrdtMouseRegions[(int)uiCnt] = new MOUSE_REGION(uiCnt.ToString());
            }

            MouseSubSystem.MSYS_DefineRegion(
                Globals.gCrdtMouseRegions[(int)uiCnt],
                 new(
                    Globals.gCreditFaces[(int)uiCnt].sX,
                    Globals.gCreditFaces[(int)uiCnt].sY,
                    Globals.gCreditFaces[(int)uiCnt].sX + Globals.gCreditFaces[(int)uiCnt].sWidth,
                    Globals.gCreditFaces[(int)uiCnt].sY + Globals.gCreditFaces[(int)uiCnt].sHeight),
                MSYS_PRIORITY.NORMAL,
                CURSOR.WWW,
                this.SelectCreditFaceMovementRegionCallBack,
                this.SelectCreditFaceRegionCallBack);

            // Add region
            //MouseSubSystem.MSYS_AddRegion(&gCrdtMouseRegions[uiCnt]);

            MouseSubSystem.MSYS_SetRegionUserData(Globals.gCrdtMouseRegions[(int)uiCnt], 0, (int)uiCnt);
        }


        //Test Node
        {
            //		AddCreditNode( CRDT_NODE_DEFAULT, "This is a test" );
        }

        /*
            //open the credit text file
            ghFile = FileOpen( CRDT_NAME_OF_CREDIT_FILE, FILE_ACCESS_READ | FILE_OPEN_EXISTING, false );
            if( !ghFile )
            { 
                return( false );
            }
        */

        Globals.giCurrentlySelectedFace = -1;
        Globals.gfPauseCreditScreen = false;

        this.InitCreditEyeBlinking();

        return true;
    }

    private void SelectCreditFaceRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
        {
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
        }
    }

    private void InitCreditEyeBlinking()
    {

    }

    public void Draw(IVideoManager videoManager)
    {

    }

    private void SelectCreditFaceMovementRegionCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
        {
            Globals.giCurrentlySelectedFace = -1;
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.GAIN_MOUSE))
        {
            Globals.giCurrentlySelectedFace = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.MOVE))
        {
        }
    }

    public ValueTask<bool> Initialize()
    {
        Globals.gfCreditsScreenEntry = true;

        this.video.GetVideoObject("INTERFACE\\Credits.sti", out Globals.guiCreditBackGroundImageKey);
        this.video.GetVideoObject("INTERFACE\\Credit Faces.sti", out Globals.guiCreditFacesKey);

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
}

//local Defines
public enum CreditRenderFlag
{
    CRDT_RENDER_NONE,
    CRDT_RENDER_ALL,
}

//flags for the credits
//Flags:

[Flags]
public enum CRDT_FLAG__
{
    CRDT_NODE_NONE = 0x00000000,
    // scrolls up and off the screen
    CRDT_NODE_DEFAULT = 0x00000001,
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
    public CRDT_FLAG__ uiType;          //the type of node
    public string pString;        //string for the node if the node contains a string
    public CRDT_FLAG__ uiFlags;     //various flags
    public Point sPos;            //position of the node on the screen if the node is displaying stuff
    public Point sOldPos;         //position of the node on the screen if the node is displaying stuff
    public int sHeightOfString;      //The height of the displayed string
    public bool fDelete;        //Delete this loop
    public long uiLastTime;  // The last time the node was udated
    public SurfaceType uiVideoSurfaceImage;
}
