﻿using System;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Screens;
using SharpAlliance.Platform.Interfaces;
using SixLabors.ImageSharp;
using static SharpAlliance.Core.EnglishText;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems;

/////////////////////////////////////////////////////////////////////////////////////
//											UI SYSTEM DESCRIPTION																			 
//
//	The UI system here decouples event determination from event execution. IN other words,
//	first any user input is gathered and analysed for an event to happen. Once the event is determined,
//	it is then executed. For example, if the left mouse button is used to select a guy, it does not 
//  execute the code to selected the guy, rather it sets a flag to a particular event, in this case
//	the I_SELECT_MERC event is set. The code then executes this event after all input is analysed. In 
//  this way, more than one input method from the user will cause the came event to occur and hence no
//  duplication of code. Also, events have cetain charactoristics. The select merc event is executed just
//  once and then returns to the previous event. Most events are set to run continuously until new
//  input changes to another event. Other events have a 'SNAP-BACK' feature which snap the mouse back to
//  it's position before the event was executed.  Another issue is UI modes. In order to filter out input
//  depending on other flags, for example we do not want to cancel the confirm when a user moves to another 
//	tile unless we are in the 'confirm' mode.  This could be done by flags ( and in effect it is ) where
//  if staements are used, but here at input collection time, we can switch on our current mode to handle
//  input differently based on the mode. Doing it this way also allows us to group certain commands togther
//  like menu commands which are initialized and deleted in the same manner.
//
//	UI_EVENTS
/////////////
//
//	UI_EVENTS have flags to itendtify themselves with special charactoristics, a UI_MODE catagory which
//  signifies the UI mode this event will cause the system to move to. Also, a pointer to a handle function is
//  used to actually handle the particular event. UI_EVENTS also have a couple of param variables and a number
//  of boolean flags used during run-time to determine states of events.
//
////////////////////////////////////////////////////////////////////////////////////////////////

public enum UIEVENT
{
    SINGLEEVENT = 0x00000002,
    SNAPMOUSE = 0x00000008,
}

public class HandleUI
{
    private const int MAX_ON_DUTY_SOLDIERS = 6;
    // LOCAL DEFINES

    private const int GO_MOVE_ONE = 40;
    private const int GO_MOVE_TWO = 80;
    private const int GO_MOVE_THREE = 100;
    private readonly MessageBoxSubSystem messageBoxSubSystem;

    //extern bool gfExitDebugScreen;
    //extern byte gCurDebugPage;
    //extern bool gfGetNewPathThroughPeople;
    //extern bool Globals.gfIgnoreOnSelectedGuy;
    //extern bool gfInOpenDoorMenu;

    private readonly ILogger<HandleUI> logger;
    private readonly IInputManager inputs;
    private readonly IClockManager clock;
    private readonly Overhead overhead;
    private readonly Points points;
    private readonly Random rnd;
    private readonly GameSettings gGameSettings;
    private readonly RenderWorld renderWorld;
    private readonly CursorSubSystem cursors;
    private readonly PathAI pathAI;
    private readonly SoldierFind soldierFind;
    private IScreenManager screens;

    public HandleUI(
        ILogger<HandleUI> logger,
        IClockManager clock,
        Overhead overhead,
        IInputManager inputManager,
        CursorSubSystem cursorSubSystem,
        GameSettings gameSettings,
        Points points,
        PathAI pathAI,
        MessageBoxSubSystem messageBoxSubSystem,
        RenderWorld renderWorld,
        SoldierFind soldierFind,
        IScreenManager screenManager)
    {
        this.messageBoxSubSystem = messageBoxSubSystem;
        this.logger = logger;
        this.clock = clock;
        this.overhead = overhead;
        this.rnd = Globals.Random;
        this.points = points;
        this.inputs = inputManager;
        this.cursors = cursorSubSystem;
        this.pathAI = pathAI;
        this.gGameSettings = gameSettings;
        this.renderWorld = renderWorld;
        this.soldierFind = soldierFind;
        this.screens = screenManager;

        Globals.gEvents = new()
        {
            { UI_EVENT_DEFINES.I_DO_NOTHING, new(0, UI_MODE.IDLE_MODE, this.UIHandleIDoNothing, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.I_EXIT, new(0, UI_MODE.IDLE_MODE, this.UIHandleExit, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.I_NEW_MERC, new(UIEVENT.SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, this.UIHandleNewMerc, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.I_NEW_BADMERC, new(UIEVENT.SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, this.UIHandleNewBadMerc, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.I_SELECT_MERC, new(UIEVENT.SINGLEEVENT, UI_MODE.MOVE_MODE, this.UIHandleSelectMerc, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.I_ENTER_EDIT_MODE, new(UIEVENT.SINGLEEVENT, UI_MODE.MOVE_MODE, this.UIHandleEnterEditMode, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.I_ENTER_PALEDIT_MODE, new(UIEVENT.SINGLEEVENT, UI_MODE.MOVE_MODE, this.UIHandleEnterPalEditMode, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.I_ENDTURN, new(UIEVENT.SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, this.UIHandleEndTurn, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.I_TESTHIT, new(UIEVENT.SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, this.UIHandleTestHit, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.I_CHANGELEVEL, new(UIEVENT.SINGLEEVENT, UI_MODE.MOVE_MODE, this.UIHandleChangeLevel, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.I_ON_TERRAIN, new(UIEVENT.SINGLEEVENT, UI_MODE.IDLE_MODE, this.UIHandleIOnTerrain, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.I_CHANGE_TO_IDLE, new(UIEVENT.SINGLEEVENT, UI_MODE.IDLE_MODE, this.UIHandleIChangeToIdle, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.I_LOADLEVEL, new(UIEVENT.SINGLEEVENT, UI_MODE.IDLE_MODE, this.UIHandleILoadLevel, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.I_SOLDIERDEBUG, new(UIEVENT.SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, this.UIHandleISoldierDebug, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.I_LOSDEBUG, new(UIEVENT.SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, this.UIHandleILOSDebug, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.I_LEVELNODEDEBUG, new(UIEVENT.SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, this.UIHandleILevelNodeDebug, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.I_GOTODEMOMODE, new(UIEVENT.SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, this.UIHandleIGotoDemoMode, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.I_LOADFIRSTLEVEL, new(UIEVENT.SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, this.UIHandleILoadFirstLevel, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.I_LOADSECONDLEVEL, new(UIEVENT.SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, this.UIHandleILoadSecondLevel, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.I_LOADTHIRDLEVEL, new(UIEVENT.SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, this.UIHandleILoadThirdLevel, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.I_LOADFOURTHLEVEL, new(UIEVENT.SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, this.UIHandleILoadFourthLevel, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.I_LOADFIFTHLEVEL, new(UIEVENT.SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, this.UIHandleILoadFifthLevel, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.ET_ON_TERRAIN, new(0,UI_MODE.ENEMYS_TURN_MODE, this.UIHandleIETOnTerrain, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.ET_ENDENEMYS_TURN, new(UIEVENT.SINGLEEVENT,UI_MODE.MOVE_MODE, this.UIHandleIETEndTurn, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.M_ON_TERRAIN, new(0,UI_MODE.MOVE_MODE, UIHandleMOnTerrain, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.M_CHANGE_TO_ACTION, new(UIEVENT.SINGLEEVENT, UI_MODE.ACTION_MODE,                    this.UIHandleMChangeToAction, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.M_CHANGE_TO_HANDMODE, new(UIEVENT.SINGLEEVENT, UI_MODE.HANDCURSOR_MODE,            this.UIHandleMChangeToHandMode, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.M_CYCLE_MOVEMENT, new(UIEVENT.SINGLEEVENT, UI_MODE.MOVE_MODE,                      this.UIHandleMCycleMovement, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.M_CYCLE_MOVE_ALL, new(UIEVENT.SINGLEEVENT, UI_MODE.CONFIRM_MOVE_MODE,      this.UIHandleMCycleMoveAll, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.M_CHANGE_TO_ADJPOS_MODE, new(UIEVENT.SNAPMOUSE, UI_MODE.ADJUST_STANCE_MODE,     this.UIHandleMAdjustStanceMode, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.POPUP_DOMESSAGE, new(0, UI_MODE.POPUP_MODE, this.UIHandlePOPUPMSG, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.A_ON_TERRAIN, new(0, UI_MODE.ACTION_MODE, this.UIHandleAOnTerrain, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.A_CHANGE_TO_MOVE, new(UIEVENT.SINGLEEVENT, UI_MODE.MOVE_MODE,                      this.UIHandleAChangeToMove, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.A_CHANGE_TO_CONFIM_ACTION, new(UIEVENT.SINGLEEVENT, UI_MODE.CONFIRM_ACTION_MODE,    this.UIHandleAChangeToConfirmAction, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.A_END_ACTION, new(UIEVENT.SINGLEEVENT, UI_MODE.MOVE_MODE,                      this.UIHandleAEndAction, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.U_MOVEMENT_MENU, new(UIEVENT.SNAPMOUSE, UI_MODE.MENU_MODE,                      this.UIHandleMovementMenu, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.U_POSITION_MENU, new(UIEVENT.SNAPMOUSE, UI_MODE.MENU_MODE,                      this.UIHandlePositionMenu, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.C_WAIT_FOR_CONFIRM, new(0, UI_MODE.CONFIRM_MOVE_MODE,    this.UIHandleCWait, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.C_MOVE_MERC, new(UIEVENT.SINGLEEVENT, UI_MODE.MOVE_MODE,                      this.UIHandleCMoveMerc, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.C_ON_TERRAIN, new(0, UI_MODE.CONFIRM_MOVE_MODE,      this.UIHandleCOnTerrain, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.PADJ_ADJUST_STANCE, new(0, UI_MODE.MOVE_MODE,                      this.UIHandlePADJAdjustStance, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.CA_ON_TERRAIN, new(0, UI_MODE.CONFIRM_ACTION_MODE,    this.UIHandleCAOnTerrain, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.CA_MERC_SHOOT, new(UIEVENT.SINGLEEVENT, UI_MODE.ACTION_MODE,                    this.UIHandleCAMercShoot, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.CA_END_CONFIRM_ACTION, new(UIEVENT.SINGLEEVENT, UI_MODE.ACTION_MODE,                    this.UIHandleCAEndConfirmAction, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.HC_ON_TERRAIN, new(0, UI_MODE.HANDCURSOR_MODE,            this.UIHandleHCOnTerrain, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.G_GETTINGITEM, new(0, UI_MODE.GETTINGITEM_MODE,           this.UIHandleHCGettingItem, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.LC_ON_TERRAIN, new(0, UI_MODE.LOOKCURSOR_MODE,            this.UIHandleLCOnTerrain, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.LC_CHANGE_TO_LOOK, new(UIEVENT.SINGLEEVENT, UI_MODE.LOOKCURSOR_MODE,            this.UIHandleLCChangeToLook, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.LC_LOOK, new(UIEVENT.SINGLEEVENT, UI_MODE.MOVE_MODE,                      this.UIHandleLCLook, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.TA_TALKINGMENU, new(0,UI_MODE.TALKINGMENU_MODE,           this.UIHandleTATalkingMenu, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.T_ON_TERRAIN, new(0,UI_MODE.TALKCURSOR_MODE,            this.UIHandleTOnTerrain, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.T_CHANGE_TO_TALKING, new(UIEVENT.SINGLEEVENT,UI_MODE.TALKCURSOR_MODE,            this.UIHandleTChangeToTalking, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.LU_ON_TERRAIN, new(0,UI_MODE.LOCKUI_MODE,                    this.UIHandleLUIOnTerrain, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.LU_BEGINUILOCK, new(0,UI_MODE.LOCKUI_MODE,                    this.UIHandleLUIBeginLock, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.LU_ENDUILOCK, new(UIEVENT.SINGLEEVENT,UI_MODE.MOVE_MODE,                      UIHandleLUIEndLock, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.OP_OPENDOORMENU, new(0, UI_MODE.OPENDOOR_MENU_MODE,     this.UIHandleOpenDoorMenu, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.LA_ON_TERRAIN, new(0, UI_MODE.LOCKOURTURN_UI_MODE,    this.UIHandleLAOnTerrain, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.LA_BEGINUIOURTURNLOCK, new(0, UI_MODE.LOCKOURTURN_UI_MODE,    this.UIHandleLABeginLockOurTurn, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.LA_ENDUIOUTURNLOCK, new(UIEVENT.SINGLEEVENT, UI_MODE.MOVE_MODE,                      this.UIHandleLAEndLockOurTurn, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.EX_EXITSECTORMENU, new(0, UI_MODE.EXITSECTORMENU_MODE,    this.UIHandleEXExitSectorMenu, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.RB_ON_TERRAIN, new(0, UI_MODE.RUBBERBAND_MODE,            this.UIHandleRubberBandOnTerrain, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.JP_ON_TERRAIN, new(0, UI_MODE.JUMPOVER_MODE,              this.UIHandleJumpOverOnTerrain, false, false, 0, new int[] { 0, 0, 0 }) },
            { UI_EVENT_DEFINES.JP_JUMP, new(0, UI_MODE.MOVE_MODE, this.UIHandleJumpOver, false, false, 0, new int[] { 0, 0, 0 }) },
        };
    }

    // MAIN TACTICAL UI HANDLER
    static LEVELNODE? pOldIntTile = null;

    public static ScreenName HandleTacticalUI()
    {
        ScreenName ReturnVal = ScreenName.GAME_SCREEN;
        UI_EVENT_DEFINES uiNewEvent;
        LEVELNODE? pIntTile = null;


        // RESET FLAGS
        Globals.gfUIDisplayActionPoints = false;
        Globals.gfUIDisplayActionPointsInvalid = false;
        Globals.gfUIDisplayActionPointsBlack = false;
        Globals.gfUIDisplayActionPointsCenter = false;
        Globals.gfUIDoNotHighlightSelMerc = false;
        Globals.gfUIHandleSelection = Globals.NO_GUY_SELECTION;
        Globals.gfUIHandleSelectionAboveGuy = false;
        Globals.gfUIDisplayDamage = false;
        Globals.guiShowUPDownArrows = ARROWS.HIDE_UP | ARROWS.HIDE_DOWN;
        Globals.gfUIBodyHitLocation = false;
        Globals.gfUIIntTileLocation = false;
        Globals.gfUIIntTileLocation2 = false;
        //gfUIForceReExamineCursorData		= false;
        Globals.gfUINewStateForIntTile = false;
        Globals.gfUIShowExitExitGrid = false;
        Globals.gfUIOverItemPool = false;
        Globals.gfUIHandlePhysicsTrajectory = false;
        Globals.gfUIMouseOnValidCatcher = 0;
        Globals.gfIgnoreOnSelectedGuy = false;

        // Set old event value
        Globals.guiOldEvent = uiNewEvent = Globals.guiCurrentEvent;

        if (Globals.gfUIInterfaceSetBusy)
        {
            if ((Globals.GetJA2Clock() - Globals.guiUIInterfaceBusyTime) > 25000)
            {
                Globals.gfUIInterfaceSetBusy = false;

                //UNLOCK UI
                UnSetUIBusy((byte)Globals.gusSelectedSoldier);

                // Decrease global busy  counter...
                Globals.gTacticalStatus.ubAttackBusyCount = 0;
                //DebugMsg(TOPIC_JA2, DBG_LEVEL_3, "Setting attack busy count to 0 due to ending AI lock");

                Globals.guiPendingOverrideEvent = UI_EVENT_DEFINES.LU_ENDUILOCK;
                UIHandleLUIEndLock(null);
            }
        }

        if ((Globals.GetJA2Clock() - Globals.guiUIInterfaceSwapCursorsTime) > 1000)
        {
            Globals.gfOKForExchangeCursor = !Globals.gfOKForExchangeCursor;
            Globals.guiUIInterfaceSwapCursorsTime = Globals.GetJA2Clock();
        }

        // OK, do a check for on an int tile...
//        pIntTile = GetCurInteractiveTile();

        if (pIntTile != pOldIntTile)
        {
            Globals.gfUINewStateForIntTile = true;

            pOldIntTile = pIntTile;
        }

        if (Globals.guiPendingOverrideEvent == UI_EVENT_DEFINES.I_DO_NOTHING)
        {
            // When we are here, guiCurrentEvent is set to the last event
            // Within the input gathering phase, it may change

            // GATHER INPUT
            // Any new event will overwrite previous events. Therefore,
            // PRIOTITIES GO LIKE THIS:
            //						Mouse Movement
            //						Keyboard Polling
            //						Mouse Buttons
            //						Keyboard Queued Events ( will override always )

            // SWITCH ON INPUT GATHERING, DEPENDING ON MODE
            // IF WE ARE NOT IN COMBAT OR IN REALTIME COMBAT
            if (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.REALTIME) || !Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
            {
                // FROM MOUSE POSITION
//                GetRTMousePositionInput(out uiNewEvent);
                // FROM KEYBOARD POLLING
//                GetPolledKeyboardInput(out uiNewEvent);
                // FROM MOUSE CLICKS
//                GetRTMouseButtonInput(out uiNewEvent);
                // FROM KEYBOARD
//                GetKeyboardInput(out uiNewEvent);
            }
            else
            {
                // FROM MOUSE POSITION
//                GetTBMousePositionInput(out uiNewEvent);
                // FROM KEYBOARD POLLING
//                GetPolledKeyboardInput(out uiNewEvent);
                // FROM MOUSE CLICKS
//                GetTBMouseButtonInput(out uiNewEvent);
                // FROM KEYBOARD
//                GetKeyboardInput(out uiNewEvent);
            }
        }
        else
        {
            uiNewEvent = Globals.guiPendingOverrideEvent;
            Globals.guiPendingOverrideEvent = UI_EVENT_DEFINES.I_DO_NOTHING;
        }

//        if (HandleItemPickupMenu())
        {
            uiNewEvent = UI_EVENT_DEFINES.A_CHANGE_TO_MOVE;
        }

        // Set Current event to new one!
        Globals.guiCurrentEvent = uiNewEvent;

        //ATE: New! Get flags for over soldier or not...
        Globals.gfUIFullTargetFound = false;
        Globals.gfUISelectiveTargetFound = false;

        if (IsometricUtils.GetMouseMapPos(out int usMapPos))
        {
            // Look for soldier full
            if (SoldierFind.FindSoldier(usMapPos, out Globals.gusUIFullTargetID, out Globals.guiUIFullTargetFlags, SoldierFind.FINDSOLDIERSAMELEVEL(Globals.gsInterfaceLevel)))
            {
                Globals.gfUIFullTargetFound = true;
            }

            // Look for soldier selective
            if (SoldierFind.FindSoldier(usMapPos, out Globals.gusUISelectiveTargetID, out Globals.guiUISelectiveTargetFlags, SoldierFind.FINDSOLDIERSELECTIVESAMELEVEL(Globals.gsInterfaceLevel)))
            {
                Globals.gfUISelectiveTargetFound = true;
            }
        }

        // Check if current event has changed and clear event if so, to prepare it for execution
        // Clearing it does things like set first time flag, param variavles, etc
        if (uiNewEvent != Globals.guiOldEvent)
        {
            // Snap mouse back if it's that type
            if (Globals.gEvents[Globals.guiOldEvent].uiFlags.HasFlag(UIEVENT.SNAPMOUSE))
            {
//                SimulateMouseMovement((uint)Globals.gusSavedMouseX, (uint)Globals.gusSavedMouseY);
            }

            ClearEvent(Globals.gEvents[uiNewEvent]);
        }

        // Restore not scrolling from stance adjust....
        if (Globals.gOldUIMode == UI_MODE.ADJUST_STANCE_MODE)
        {
            Globals.gfIgnoreScrolling = false;
        }

        // IF this event is of type snap mouse, save position
        if (Globals.gEvents[uiNewEvent].uiFlags.HasFlag(UIEVENT.SNAPMOUSE) && Globals.gEvents[uiNewEvent].fFirstTime)
        {
            // Save mouse position
            Globals.gusSavedMouseX = Globals.gusMouseXPos;
            Globals.gusSavedMouseY = Globals.gusMouseYPos;
        }

        // HANDLE UI EVENT
        ReturnVal = Globals.gEvents[uiNewEvent].HandleEvent(Globals.gEvents[uiNewEvent]);

        if (Globals.gfInOpenDoorMenu)
        {
            return ReturnVal;
        }

        // Set first time flag to false, now that it has been executed
        Globals.gEvents[uiNewEvent].fFirstTime = false;

        // Check if UI mode has changed from previous event
        if (Globals.gEvents[uiNewEvent].ChangeToUIMode != Globals.gCurrentUIMode && (Globals.gEvents[uiNewEvent].ChangeToUIMode != UI_MODE.DONT_CHANGEMODE))
        {
            Globals.gEvents[uiNewEvent].uiMenuPreviousMode = Globals.gCurrentUIMode;

            Globals.gOldUIMode = Globals.gCurrentUIMode;

            Globals.gCurrentUIMode = Globals.gEvents[uiNewEvent].ChangeToUIMode;

            // CHANGE MODE - DO SPECIAL THINGS IF WE ENTER THIS MODE
            switch (Globals.gCurrentUIMode)
            {
                case UI_MODE.ACTION_MODE:
                    PathAI.ErasePath(true);
                    break;
            }
        }

        // Check if menu event is done and if so set to privious mode
        // This is needed to hook into the interface stuff which sets the fDoneMenu flag
        if (Globals.gEvents[uiNewEvent].fDoneMenu == true)
        {
            if (Globals.gCurrentUIMode == UI_MODE.MENU_MODE
                || Globals.gCurrentUIMode == UI_MODE.POPUP_MODE
                || Globals.gCurrentUIMode == UI_MODE.LOOKCURSOR_MODE)
            {
                Globals.gCurrentUIMode = Globals.gEvents[uiNewEvent].uiMenuPreviousMode;
            }
        }
        // Check to return to privious mode
        // If the event is a single event, return to previous
        if (Globals.gEvents[uiNewEvent].uiFlags.HasFlag(UIEVENT.SINGLEEVENT))
        {
            // ATE: OK - don't revert to single event if our mouse is not
            // in viewport - rather use m_on_t event
            if (Globals.gViewportRegion.uiFlags.HasFlag(MouseRegionFlags.IN_AREA))
            {
                Globals.guiCurrentEvent = Globals.guiOldEvent;
            }
            else
            {
                // ATE: Check first that some modes are met....
                if (Globals.gCurrentUIMode != UI_MODE.HANDCURSOR_MODE
                    && Globals.gCurrentUIMode != UI_MODE.LOOKCURSOR_MODE
                    && Globals.gCurrentUIMode != UI_MODE.TALKCURSOR_MODE)
                {
                    Globals.guiCurrentEvent = UI_EVENT_DEFINES.M_ON_TERRAIN;
                }
            }
        }

        // Donot display APs if not in combat
        if (!Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)
            | (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.REALTIME)))
        {
            Globals.gfUIDisplayActionPoints = false;
        }

        // Will set the cursor but only if different
        SetUIMouseCursor();

        // ATE: Check to reset selected guys....
        if (Globals.gTacticalStatus.fAtLeastOneGuyOnMultiSelect)
        {
            // If not in MOVE_MODE, CONFIRM_MOVE_MODE, RUBBERBAND_MODE, stop....
            if (Globals.gCurrentUIMode != UI_MODE.MOVE_MODE
                && Globals.gCurrentUIMode != UI_MODE.CONFIRM_MOVE_MODE
                && Globals.gCurrentUIMode != UI_MODE.RUBBERBAND_MODE
                && Globals.gCurrentUIMode != UI_MODE.ADJUST_STANCE_MODE
                && Globals.gCurrentUIMode != UI_MODE.TALKCURSOR_MODE
                && Globals.gCurrentUIMode != UI_MODE.LOOKCURSOR_MODE)
            {
                ResetMultiSelection();
            }
        }

        return ReturnVal;
    }

    static int sOldExitGridNo = (int)Globals.NOWHERE;
    static bool fOkForExit = false;

    public static void SetUIMouseCursor()
    {
        uint uiTraverseTimeInMinutes;
        bool fForceUpdateNewCursor = false;
        bool fUpdateNewCursor = true;

        // Check if we moved from confirm mode on exit arrows
        // If not in move mode, return!
        if (Globals.gCurrentUIMode == UI_MODE.MOVE_MODE)
        {
            if (Globals.gfUIConfirmExitArrows)
            {
                GetCursorMovementFlags(out MOUSE uiCursorFlags);

                if (uiCursorFlags.HasFlag(MOUSE.MOVING))
                {
                    Globals.gfUIConfirmExitArrows = false;
                }
            }

            if (Globals.gfUIShowExitEast)
            {
                Globals.gfUIDisplayActionPoints = false;
                PathAI.ErasePath(true);

                if (StrategicMap.OKForSectorExit(StrategicMove.EAST, 0, out uiTraverseTimeInMinutes) > 0)
                {
                    if (Globals.gfUIConfirmExitArrows)
                    {
                        Globals.guiNewUICursor = UICursorDefines.CONFIRM_EXIT_EAST_UICURSOR;
                    }
                    else
                    {
                        Globals.guiNewUICursor = UICursorDefines.EXIT_EAST_UICURSOR;
                    }
                }
                else
                {
                    Globals.guiNewUICursor = UICursorDefines.NOEXIT_EAST_UICURSOR;
                }

                if (Globals.gusMouseXPos < 635)
                {
                    Globals.gfUIShowExitEast = false;
                }
            }

            if (Globals.gfUIShowExitWest)
            {
                Globals.gfUIDisplayActionPoints = false;
                PathAI.ErasePath(true);

                if (StrategicMap.OKForSectorExit(StrategicMove.WEST, 0, out uiTraverseTimeInMinutes) > 0)
                {
                    if (Globals.gfUIConfirmExitArrows)
                    {
                        Globals.guiNewUICursor = UICursorDefines.CONFIRM_EXIT_WEST_UICURSOR;
                    }
                    else
                    {
                        Globals.guiNewUICursor = UICursorDefines.EXIT_WEST_UICURSOR;
                    }
                }
                else
                {
                    Globals.guiNewUICursor = UICursorDefines.NOEXIT_WEST_UICURSOR;
                }

                if (Globals.gusMouseXPos > 5)
                {
                    Globals.gfUIShowExitWest = false;
                }
            }

            if (Globals.gfUIShowExitNorth)
            {
                Globals.gfUIDisplayActionPoints = false;
                PathAI.ErasePath(true);

                if (StrategicMap.OKForSectorExit(StrategicMove.NORTH, 0, out uiTraverseTimeInMinutes) > 0)
                {
                    if (Globals.gfUIConfirmExitArrows)
                    {
                        Globals.guiNewUICursor = UICursorDefines.CONFIRM_EXIT_NORTH_UICURSOR;
                    }
                    else
                    {
                        Globals.guiNewUICursor = UICursorDefines.EXIT_NORTH_UICURSOR;
                    }
                }
                else
                {
                    Globals.guiNewUICursor = UICursorDefines.NOEXIT_NORTH_UICURSOR;
                }

                if (Globals.gusMouseYPos > 5)
                {
                    Globals.gfUIShowExitNorth = false;
                }
            }


            if (Globals.gfUIShowExitSouth)
            {
                Globals.gfUIDisplayActionPoints = false;
                PathAI.ErasePath(true);

                if (StrategicMap.OKForSectorExit(StrategicMove.SOUTH, 0, out uiTraverseTimeInMinutes) > 0)
                {
                    if (Globals.gfUIConfirmExitArrows)
                    {
                        Globals.guiNewUICursor = UICursorDefines.CONFIRM_EXIT_SOUTH_UICURSOR;
                    }
                    else
                    {
                        Globals.guiNewUICursor = UICursorDefines.EXIT_SOUTH_UICURSOR;
                    }
                }
                else
                {
                    Globals.guiNewUICursor = UICursorDefines.NOEXIT_SOUTH_UICURSOR;
                }

                if (Globals.gusMouseYPos < 478)
                {
                    Globals.gfUIShowExitSouth = false;

                    // Define region for viewport
                    MouseSubSystem.MSYS_RemoveRegion(Globals.gViewportRegion);

                    MouseSubSystem.MSYS_DefineRegion(
                        Globals.gViewportRegion,
                        new(0, 0, Globals.gsVIEWPORT_END_X, Globals.gsVIEWPORT_WINDOW_END_Y),
                        MSYS_PRIORITY.NORMAL,
                        CURSOR.VIDEO_NO_CURSOR,
                        MSYS_NO_CALLBACK,
                        MSYS_NO_CALLBACK);


                    // Adjust where we blit our cursor!
                    Globals.gsGlobalCursorYOffset = 0;
                    CursorSubSystem.SetCurrentCursorFromDatabase(CURSOR.NORMAL);
                }
                else
                {
                    if (Globals.gfScrollPending || Globals.gfScrollInertia != 0)
                    {

                    }
                    else
                    {
                        // Adjust viewport to edge of screen!
                        // Define region for viewport
                        MouseSubSystem.MSYS_RemoveRegion(Globals.gViewportRegion);
                        MouseSubSystem.MSYS_DefineRegion(Globals.gViewportRegion, new(0, 0, Globals.gsVIEWPORT_END_X, 480), MSYS_PRIORITY.NORMAL,
                                             CURSOR.VIDEO_NO_CURSOR, MSYS_NO_CALLBACK, MSYS_NO_CALLBACK);

                        Globals.gsGlobalCursorYOffset = 480 - Globals.gsVIEWPORT_WINDOW_END_Y;
                        CursorSubSystem.SetCurrentCursorFromDatabase(Globals.gUICursors[Globals.guiNewUICursor].usFreeCursorName);

                        Globals.gfViewPortAdjustedForSouth = true;

                    }
                }
            }
            else
            {
                if (Globals.gfViewPortAdjustedForSouth)
                {
                    // Define region for viewport
                    MouseSubSystem.MSYS_RemoveRegion(Globals.gViewportRegion);

                    MouseSubSystem.MSYS_DefineRegion(
                        Globals.gViewportRegion,
                        new(0, 0, Globals.gsVIEWPORT_END_X, Globals.gsVIEWPORT_WINDOW_END_Y),
                        MSYS_PRIORITY.NORMAL,
                        CURSOR.VIDEO_NO_CURSOR,
                        MSYS_NO_CALLBACK,
                        MSYS_NO_CALLBACK);

                    // Adjust where we blit our cursor!
                    Globals.gsGlobalCursorYOffset = 0;
                    CursorSubSystem.SetCurrentCursorFromDatabase(CURSOR.NORMAL);

                    Globals.gfViewPortAdjustedForSouth = false;
                }
            }

            if (Globals.gfUIShowExitExitGrid)
            {

                Globals.gfUIDisplayActionPoints = false;
                PathAI.ErasePath(true);

                if (IsometricUtils.GetMouseMapPos(out int usMapPos))
                {
                    if (Globals.gusSelectedSoldier != Globals.NOBODY && Globals.MercPtrs[Globals.gusSelectedSoldier].bLevel == 0)
                    {
                        // ATE: Is this place revealed?
                        if (!RenderFun.InARoom(usMapPos, out int ubRoomNum) || (RenderFun.InARoom(usMapPos, out ubRoomNum) && Globals.gpWorldLevelData[usMapPos].uiFlags.HasFlag(MAPELEMENTFLAGS.REVEALED)))
                        {
                            if (sOldExitGridNo != usMapPos)
                            {
                                fOkForExit = StrategicMap.OKForSectorExit((StrategicMove)(-1), usMapPos, out uiTraverseTimeInMinutes) > 0;
                                sOldExitGridNo = usMapPos;
                            }

                            if (fOkForExit)
                            {
                                if (Globals.gfUIConfirmExitArrows)
                                {
                                    Globals.guiNewUICursor = UICursorDefines.CONFIRM_EXIT_GRID_UICURSOR;
                                }
                                else
                                {
                                    Globals.guiNewUICursor = UICursorDefines.EXIT_GRID_UICURSOR;
                                }
                            }
                            else
                            {
                                Globals.guiNewUICursor = UICursorDefines.NOEXIT_GRID_UICURSOR;
                            }
                        }
                    }
                }
            }
            else
            {
                sOldExitGridNo = (int)Globals.NOWHERE;
            }

        }
        else
        {
            Globals.gsGlobalCursorYOffset = 0;
        }

        if (Globals.gfDisplayTimerCursor)
        {
            InterfaceCursors.SetUICursor(Globals.guiTimerCursorID);

            fUpdateNewCursor = false;

            if ((Globals.GetJA2Clock() - Globals.guiTimerLastUpdate) > Globals.guiTimerCursorDelay)
            {
                Globals.gfDisplayTimerCursor = false;

                // OK, timer may be different, update...
                fForceUpdateNewCursor = true;
                fUpdateNewCursor = true;
            }
        }

        if (fUpdateNewCursor)
        {
            if (!Globals.gfTacticalForceNoCursor)
            {
                if (Globals.guiNewUICursor != Globals.guiCurrentUICursor || fForceUpdateNewCursor)
                {
                    InterfaceCursors.SetUICursor(Globals.guiNewUICursor);

                    Globals.guiCurrentUICursor = Globals.guiNewUICursor;
                }
            }
        }
    }

    public static void HandleTacticalUILoseCursorFromOtherScreen()
    {
        InterfaceCursors.SetUICursor(0);

        Globals.gfTacticalForceNoCursor = true;

        PathAI.ErasePath(true);

        //((Globals.GameScreens[ScreenName.GAME_SCREEN].HandleScreen))();

        Globals.gfTacticalForceNoCursor = false;

        InterfaceCursors.SetUICursor(Globals.guiCurrentUICursor);
    }


    void SetUIKeyboardHook(/*UIKEYBOARD_HOOK KeyboardHookFnc*/)
    {
        //gUIKeyboardHook = KeyboardHookFnc;
    }


    public static void ClearEvent(UI_EVENT pUIEvent)
    {
        //memset(pUIEvent.uiParams, 0, sizeof(pUIEvent.uiParams) );
        pUIEvent.fDoneMenu = false;
        pUIEvent.fFirstTime = true;
        pUIEvent.uiMenuPreviousMode = 0;
    }

    void EndMenuEvent(UI_EVENT uiEvent)
    {
        uiEvent.fDoneMenu = true;
    }

    // HANDLER FUCNTIONS

    ScreenName UIHandleIDoNothing(UI_EVENT pUIEvent)
    {
        Globals.guiNewUICursor = UICursorDefines.NORMAL_SNAPUICURSOR;

        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleExit(UI_EVENT pUIEvent)
    {
        Globals.gfProgramIsRunning = false;
        return ScreenName.GAME_SCREEN;
    }

    static NPCID ubTemp = (NPCID)3;
    static int iSoldierCount = 0;

    ScreenName UIHandleNewMerc(UI_EVENT pUIEvent)
    {
        int bReturnCode = 0;
        SOLDIERTYPE? pSoldier;

        // Get Grid Corrdinates of mouse
        if (IsometricUtils.GetMouseMapPos(out int usMapPos))
        {
            ubTemp += 2;

            //memset(out HireMercStruct, 0, sizeof(MERC_HIRE_STRUCT));

            MERC_HIRE_STRUCT HireMercStruct = new()
            {
                ubProfileID = ubTemp,

                //DEF: temp
                sSectorX = Globals.gWorldSectorX,
                sSectorY = Globals.gWorldSectorY,
                bSectorZ = Globals.gbWorldSectorZ,
                ubInsertionCode = INSERTION_CODE.GRIDNO,
                usInsertionData = usMapPos,
                fCopyProfileItemsOver = true,
                iTotalContractLength = 7,

                //specify when the merc should arrive
                uiTimeTillMercArrives = 0
            };

            //if we succesfully hired the merc
//            bReturnCode = HireMerc(out HireMercStruct);

            if (bReturnCode == Globals.MERC_HIRE_FAILED)
            {
                Messages.ScreenMsg(FontColor.FONT_ORANGE, MSG.BETAVERSION, "Merc hire failed:  Either already hired or dislikes you.");
            }
            else if (bReturnCode == Globals.MERC_HIRE_OVER_20_MERCS_HIRED)
            {
                Messages.ScreenMsg(FontColor.FONT_ORANGE, MSG.BETAVERSION, "Can't hire more than 20 mercs.");
            }
            else
            {
                // Get soldier from profile
                pSoldier = SoldierProfileSubSystem.FindSoldierByProfileID(ubTemp, false);

//                MercArrivesCallback(pSoldier.ubID);
//                SelectSoldier(pSoldier.ubID, false, true);
            }
        }

        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleNewBadMerc(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE? pSoldier;
        uint usRandom;

        //Get map postion and place the enemy there.
        if (IsometricUtils.GetMouseMapPos(out int usMapPos))
        {
            // Are we an OK dest?
//            if (!IsLocationSittable(usMapPos, 0))
            {
                return ScreenName.GAME_SCREEN;
            }

            usRandom = (uint)Globals.Random.Next(10);
            if (usRandom < 4)
            {
//                pSoldier = TacticalCreateAdministrator();
            }
            else if (usRandom < 8)
            {
//                pSoldier = TacticalCreateArmyTroop();
            }
            else
            {
//                pSoldier = TacticalCreateEliteEnemy();
            }

            //Add soldier strategic info, so it doesn't break the counters!
            if (pSoldier is not null)
            {
                if (Globals.gbWorldSectorZ == 0)
                {
                    SECTORINFO? pSector = Globals.SectorInfo[SECTORINFO.SECTOR(Globals.gWorldSectorX, Globals.gWorldSectorY)];
                    switch (pSoldier.ubSoldierClass)
                    {
                        case SOLDIER_CLASS.ADMINISTRATOR:
                            pSector.ubNumAdmins++;
                            pSector.ubAdminsInBattle++;
                            break;
                        case SOLDIER_CLASS.ARMY:
                            pSector.ubNumTroops++;
                            pSector.ubTroopsInBattle++;
                            break;
                        case SOLDIER_CLASS.ELITE:
                            pSector.ubNumElites++;
                            pSector.ubElitesInBattle++;
                            break;
                    }
                }
                else
                {
                    UNDERGROUND_SECTORINFO? pSector = QueenCommand.FindUnderGroundSector(Globals.gWorldSectorX, Globals.gWorldSectorY, Globals.gbWorldSectorZ);
                    if (pSector is not null)
                    {
                        switch (pSoldier.ubSoldierClass)
                        {
                            case SOLDIER_CLASS.ADMINISTRATOR:
                                pSector.ubNumAdmins++;
                                pSector.ubAdminsInBattle++;
                                break;
                            case SOLDIER_CLASS.ARMY:
                                pSector.ubNumTroops++;
                                pSector.ubTroopsInBattle++;
                                break;
                            case SOLDIER_CLASS.ELITE:
                                pSector.ubNumElites++;
                                pSector.ubElitesInBattle++;
                                break;
                        }
                    }
                }

                pSoldier.ubStrategicInsertionCode = INSERTION_CODE.GRIDNO;
                pSoldier.usStrategicInsertionData = usMapPos;
//                UpdateMercInSector(pSoldier, Globals.gWorldSectorX, Globals.gWorldSectorY, Globals.gbWorldSectorZ);
//                AllTeamsLookForAll(NO_INTERRUPTS);
            }
        }
        return ScreenName.GAME_SCREEN;
    }


    ScreenName UIHandleEnterEditMode(UI_EVENT pUIEvent)
    {
        return ScreenName.EDIT_SCREEN;
    }

    ScreenName UIHandleEnterPalEditMode(UI_EVENT pUIEvent)
    {
        return ScreenName.PALEDIT_SCREEN;
    }

    ScreenName UIHandleEndTurn(UI_EVENT pUIEvent)
    {
        // CANCEL FROM PLANNING MODE!
//        if (InUIPlanMode())
//        {
//            EndUIPlan();
//        }

        // ATE: If we have an item pointer end it!
//        CancelItemPointer();

        //Messages.ScreenMsg( FONT_MCOLOR_LTYELLOW, MSG.INTERFACE, Globals.TacticalStr[ ENDING_TURN ] );

//        if (CheckForEndOfCombatMode(false))
        {
            // do nothing...
        }
//        else
        {
//            if (FileManager.FileExists("..\\AutoSave.pls") && CanGameBeSaved())
            {
                //Save the game
                Globals.guiPreviousOptionScreen = Globals.guiCurrentScreen;
//                SaveGame(SAVE__END_TURN_NUM, "End Turn Auto Save");
            }

            // End our turn!
//            EndTurn(Globals.gbPlayerNum + 1);
        }

        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleTestHit(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE? pSoldier = null;
        byte bDamage = 0;

        // CHECK IF WE'RE ON A GUY ( EITHER SELECTED, OURS, OR THEIRS
        if (Globals.gfUIFullTargetFound)
        {
            // Get Soldier
            Overhead.GetSoldier(out pSoldier, Globals.gusUIFullTargetID);

//            if (_KeyDown(SHIFT))
//            {
//                pSoldier.bBreath -= 30;
//
//                if (pSoldier.bBreath < 0)
//                {
//                    pSoldier.bBreath = 0;
//                }
//
//                bDamage = 1;
//            }
//            else
//            {
//                if (this.rnd.Next(2) > 0)
//                {
//                    bDamage = 20;
//                }
//                else
//                {
//                    bDamage = 25;
//                }
//            }

            Globals.gTacticalStatus.ubAttackBusyCount++;

            SoldierControl.EVENT_SoldierGotHit(pSoldier, Items.GLOCK_17, bDamage, 10, pSoldier.bDirection, 320, Globals.NOBODY, FIRE_WEAPON_NO_SPECIAL, pSoldier.bAimShotLocation, 0, (int)Globals.NOWHERE);

        }

        return ScreenName.GAME_SCREEN;
    }

    void ChangeInterfaceLevel(int sLevel)
    {
        // Only if different!
        if (sLevel == Globals.gsInterfaceLevel)
        {
            return;
        }

        Globals.gsInterfaceLevel = sLevel;

        if (Globals.gsInterfaceLevel == InterfaceLevel.I_ROOF_LEVEL)
        {
            Globals.gsRenderHeight += Globals.ROOF_LEVEL_HEIGHT;
            Globals.gTacticalStatus.uiFlags |= TacticalEngineStatus.SHOW_ALL_ROOFS;
            RenderWorld.InvalidateWorldRedundency();
        }
        else if (Globals.gsInterfaceLevel == 0)
        {
            Globals.gsRenderHeight -= Globals.ROOF_LEVEL_HEIGHT;
            Globals.gTacticalStatus.uiFlags &= ~TacticalEngineStatus.SHOW_ALL_ROOFS;
            RenderWorld.InvalidateWorldRedundency();
        }

        RenderWorld.SetRenderFlags(RenderingFlags.FULL);
        // Remove any interactive tiles we could be over!
        InteractiveTiles.BeginCurInteractiveTileCheck(INTILE_CHECK_SELECTIVE);
        Globals.gfPlotNewMovement = true;
        PathAI.ErasePath(false);
    }

    ScreenName UIHandleChangeLevel(UI_EVENT pUIEvent)
    {
        if (Globals.gsInterfaceLevel == 0)
        {
            this.ChangeInterfaceLevel(1);
        }
        else if (Globals.gsInterfaceLevel == InterfaceLevel.I_ROOF_LEVEL)
        {
            this.ChangeInterfaceLevel(0);
        }

        return ScreenName.GAME_SCREEN;
    }

    //extern void InternalSelectSoldier(int usSoldierID, bool fAcknowledge, bool fForceReselect, bool fFromUI);

    ScreenName UIHandleSelectMerc(UI_EVENT pUIEvent)
    {
        SquadEnum iCurrentSquad;

        // Get merc index at mouse and set current selection
        if (Globals.gfUIFullTargetFound)
        {
            iCurrentSquad = Squads.CurrentSquad();

//            InternalSelectSoldier(Globals.gusUIFullTargetID, true, false, true);

            // If different, display message
            if (Squads.CurrentSquad() != iCurrentSquad)
            {
                Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.INTERFACE, pMessageStrings[MSG.SQUAD_ACTIVE], ((int)Squads.CurrentSquad() + 1).ToString());
            }
        }

        return ScreenName.GAME_SCREEN;
    }

    static int sGridNoForItemsOver;
    static int bLevelForItemsOver;
    static uint uiItemsOverTimer;
    static bool fOverItems;

    public static ScreenName UIHandleMOnTerrain(UI_EVENT? pUIEvent)
    {
        bool fSetCursor = false;
        LEVELNODE? pIntNode;
        EXITGRID ExitGrid;
        int sIntTileGridNo;
        bool fContinue = true;

        if (!IsometricUtils.GetMouseMapPos(out int usMapPos))
        {
            return ScreenName.GAME_SCREEN;
        }

        Globals.gUIActionModeChangeDueToMouseOver = false;

        // If we are a vehicle..... just show an X
        if (Overhead.GetSoldier(out SOLDIERTYPE? pSoldier, Globals.gusSelectedSoldier))
        {
            if (OK_ENTERABLE_VEHICLE(pSoldier))
            {
                if (!UIHandleOnMerc(true))
                {
                    Globals.guiNewUICursor = UICursorDefines.FLOATING_X_UICURSOR;
                    return ScreenName.GAME_SCREEN;
                }
            }
        }

        // CHECK IF WE'RE ON A GUY ( EITHER SELECTED, OURS, OR THEIRS
        if (!UIHandleOnMerc(true))
        {
            // Are we over items...
            if (HandleItems.GetItemPool(usMapPos, out ITEM_POOL? pItemPool, Globals.gsInterfaceLevel)
                && ITEMPOOL_VISIBLE(pItemPool))
            {
                // Are we already in...
                if (fOverItems)
                {
                    // Is this the same level & gridno...
                    if (Globals.gsInterfaceLevel == bLevelForItemsOver && usMapPos == sGridNoForItemsOver)
                    {
                        // Check timer...
                        if ((Globals.GetJA2Clock() - uiItemsOverTimer) > 1500)
                        {
                            // Change to hand curso mode
                            Globals.guiPendingOverrideEvent = UI_EVENT_DEFINES.M_CHANGE_TO_HANDMODE;
                            Globals.gsOverItemsGridNo = usMapPos;
                            Globals.gsOverItemsLevel = Globals.gsInterfaceLevel;
                            fOverItems = false;
                        }
                    }
                    else
                    {
                        uiItemsOverTimer = Globals.GetJA2Clock();
                        bLevelForItemsOver = Globals.gsInterfaceLevel;
                        sGridNoForItemsOver = usMapPos;
                    }
                }
                else
                {
                    fOverItems = true;

                    uiItemsOverTimer = Globals.GetJA2Clock();
                    bLevelForItemsOver = Globals.gsInterfaceLevel;
                    sGridNoForItemsOver = usMapPos;
                }
            }
            else
            {
                fOverItems = false;
            }


            if (Overhead.GetSoldier(out pSoldier, Globals.gusSelectedSoldier))
            {

                if (pSoldier.sGridNo == (int)Globals.NOWHERE)
                {
                    int i = 0;
                }

//                if (GetExitGrid(usMapPos, out ExitGrid) && pSoldier.bLevel == 0)
//                {
//                    Globals.gfUIShowExitExitGrid = true;
//                }

                // ATE: Draw invalidc cursor if heights different
                if (Globals.gpWorldLevelData[usMapPos].sHeight != Globals.gpWorldLevelData[pSoldier.sGridNo].sHeight)
                {
                    // ERASE PATH
                    PathAI.ErasePath(true);

                    Globals.guiNewUICursor = UICursorDefines.FLOATING_X_UICURSOR;

                    return ScreenName.GAME_SCREEN;
                }
            }

            // DO SOME CURSOR POSITION FLAGS SETTING
            GetCursorMovementFlags(out MOUSE uiCursorFlags);

            if (Globals.gusSelectedSoldier != Globals.NO_SOLDIER)
            {
                // Get Soldier Pointer
                Overhead.GetSoldier(out pSoldier, Globals.gusSelectedSoldier);

                // Get interactvie tile node
//                pIntNode = GetCurInteractiveTileGridNo(out sIntTileGridNo);

                // Check were we are
                // CHECK IF WE CAN MOVE HERE
                // THIS IS JUST A CRUDE TEST FOR NOW
                if (pSoldier.bLife < Globals.OKLIFE)
                {
                    byte ubID;
                    // Show reg. cursor
                    // GO INTO IDLE MODE
                    // Globals.guiPendingOverrideEvent = I_CHANGE_TO_IDLE;
                    // Globals.gusSelectedSoldier = NO_SOLDIER;	
//                    ubID = FindNextActiveAndAliveMerc(pSoldier, false, false);

//                    if (ubID != Globals.NOBODY)
                    {
//                        SelectSoldier((int)ubID, false, false);
                    }
//                    else
                    {
                        Globals.gusSelectedSoldier = Globals.NO_SOLDIER;
                        // Change UI mode to outlact that we are selected
                        Globals.guiPendingOverrideEvent = UI_EVENT_DEFINES.I_ON_TERRAIN;
                    }
                }
//                else if ((UIOKMoveDestination(pSoldier, usMapPos) != 1) && pIntNode == null)
//                {
//                    // ERASE PATH
//                    PathAI.ErasePath(true);
//
//                    Globals.guiNewUICursor = UICursorDefines.CANNOT_MOVE_UICURSOR;
//
//                }
                else
                {
                    if (UIHandleInteractiveTilesAndItemsOnTerrain(pSoldier, usMapPos, false, true) == 0)
                    {
                        // Are we in combat?
                        if (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)
                            && Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED))
                        {
                            // If so, draw path, etc
                            fSetCursor = HandleUIMovementCursor(pSoldier, uiCursorFlags, usMapPos, 0);
                        }
                        else
                        {
                            // Donot draw path until confirm
                            fSetCursor = true;

                            // If so, draw path, etc
                            fSetCursor = HandleUIMovementCursor(pSoldier, uiCursorFlags, usMapPos, 0);

                            //ErasePath( true );
                        }
                    }
                    else
                    {
                        fSetCursor = true;
                    }
                }
            }
            else
            {
                // IF GUSSELECTEDSOLDIER != NOSOLDIER
                Globals.guiNewUICursor = UICursorDefines.NORMAL_SNAPUICURSOR;
            }
        }
        else
        {
            if (ValidQuickExchangePosition())
            {
                // Do new cursor!
                Globals.guiNewUICursor = UICursorDefines.EXCHANGE_PLACES_UICURSOR;
            }
        }


        {

            //if ( fSetCursor && guiNewUICursor != ENTER_VEHICLE_UICURSOR )
            if (fSetCursor && !Globals.gfBeginVehicleCursor)
            {
                SetMovementModeCursor(pSoldier);
            }
        }

        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleMovementMenu(UI_EVENT pUIEvent)
    {


        // Get soldier
        if (!Overhead.GetSoldier(out SOLDIERTYPE? pSoldier, Globals.gusSelectedSoldier))
        {
            return ScreenName.GAME_SCREEN;
        }

        // Popup Menu
        if (pUIEvent.fFirstTime)
        {
            //Pop-up menu
//            PopupMovementMenu(pUIEvent);

            // Change cusror to normal
            Globals.guiNewUICursor = UICursorDefines.NORMAL_FREEUICURSOR;

        }

        // Check for done flag
        if (pUIEvent.fDoneMenu)
        {
//            PopDownMovementMenu();

            // Excecute command, if user hit a button
            if (pUIEvent.uiParams[1] > 1)
            {
                if (pUIEvent.uiParams[2] == (int)MOVEMENT.MENU_LOOK)
                {
                    Globals.guiPendingOverrideEvent = UI_EVENT_DEFINES.LC_CHANGE_TO_LOOK;
                }
                else if (pUIEvent.uiParams[2] == (int)MOVEMENT.MENU_HAND)
                {
                    Globals.guiPendingOverrideEvent = UI_EVENT_DEFINES.HC_ON_TERRAIN;
                }
                else if (pUIEvent.uiParams[2] == (int)MOVEMENT.MENU_ACTIONC)
                {
                    Globals.guiPendingOverrideEvent = UI_EVENT_DEFINES.M_CHANGE_TO_ACTION;
                }
                else if (pUIEvent.uiParams[2] == (int)MOVEMENT.MENU_TALK)
                {
                    Globals.guiPendingOverrideEvent = UI_EVENT_DEFINES.T_CHANGE_TO_TALKING;
                }
                else
                {
                    // Change stance based on params!
                    switch ((MOVEMENT)pUIEvent.uiParams[0])
                    {
                        case MOVEMENT.MENU_RUN:

                            if (pSoldier.usUIMovementMode != AnimationStates.WALKING
                                && pSoldier.usUIMovementMode != AnimationStates.RUNNING)
                            {
                                this.UIHandleSoldierStanceChange(pSoldier.ubID, AnimationHeights.ANIM_STAND);
                                pSoldier.fUIMovementFast = true;
                            }
                            else
                            {
                                pSoldier.fUIMovementFast = true;
                                pSoldier.usUIMovementMode = AnimationStates.RUNNING;
                                Globals.gfPlotNewMovement = true;
                            }
                            break;

                        case MOVEMENT.MENU_WALK:

                            this.UIHandleSoldierStanceChange(pSoldier.ubID, AnimationHeights.ANIM_STAND);
                            break;

                        case MOVEMENT.MENU_SWAT:

                            this.UIHandleSoldierStanceChange(pSoldier.ubID, AnimationHeights.ANIM_CROUCH);
                            break;

                        case MOVEMENT.MENU_PRONE:

                            this.UIHandleSoldierStanceChange(pSoldier.ubID, AnimationHeights.ANIM_PRONE);
                            break;

                    }

                    Globals.guiPendingOverrideEvent = UI_EVENT_DEFINES.A_CHANGE_TO_MOVE;

                    //pSoldier.usUIMovementMode = (byte)pUIEvent.uiParams[ 0 ];
                }
            }
        }

        return ScreenName.GAME_SCREEN;
    }


    ScreenName UIHandlePositionMenu(UI_EVENT pUIEvent)
    {
        return ScreenName.GAME_SCREEN;
    }


    ScreenName UIHandleAOnTerrain(UI_EVENT pUIEvent)
    {
        //	int							sTargetXPos, sTargetYPos;

        if (!IsometricUtils.GetMouseMapPos(out int usMapPos))
        {
            return ScreenName.GAME_SCREEN;
        }

        if (Globals.gpItemPointer != null)
        {
            return ScreenName.GAME_SCREEN;
        }

        // Get soldier to determine range
        if (Overhead.GetSoldier(out SOLDIERTYPE? pSoldier, Globals.gusSelectedSoldier))
        {
            // ATE: Add stuff here to display a system message if we are targeting smeothing and
            //  are out of range.
            // Are we using a gun?
//            if (GetActionModeCursor(pSoldier) == TARGETCURS)
//            {
//                SetActionModeDoorCursorText();
//
//                // Yep, she's a gun.
//                // Are we in range?
//                if (!InRange(pSoldier, usMapPos))
//                {
//                    // Are we over a guy?
//                    if (Globals.gfUIFullTargetFound)
//                    {
//                        // No, ok display message IF this is the first time at this gridno
//                        if (Globals.gsOutOfRangeGridNo != Globals.MercPtrs[Globals.gusUIFullTargetID].sGridNo
//                            || Globals.gubOutOfRangeMerc != Globals.gusSelectedSoldier)
//                        {
//                            // Display
//                            Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.INTERFACE, Globals.TacticalStr[(int)STR.OUT_OF_RANGE_STRING]);
//
//                            //PlayJA2Sample( TARGET_OUT_OF_RANGE, RATE_11025, MIDVOLUME, 1, MIDDLEPAN );			              
//
//                            // Set
//                            Globals.gsOutOfRangeGridNo = Globals.MercPtrs[Globals.gusUIFullTargetID].sGridNo;
//                            Globals.gubOutOfRangeMerc = (byte)Globals.gusSelectedSoldier;
//                        }
//                    }
//                }
//
//            }

//            Globals.guiNewUICursor = GetProperItemCursor((byte)Globals.gusSelectedSoldier, pSoldier.inv[InventorySlot.HANDPOS].usItem, usMapPos, false);

            // Show UI ON GUY
            UIHandleOnMerc(false);

            // If we are in realtime, and in a stationary animation, follow!
            if (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.REALTIME)
                || !Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
            {
                if (Globals.gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.STATIONARY)
                    && pSoldier.ubPendingAction == MERC.NO_PENDING_ACTION)
                {
                    // Check if we have a shot waiting!
                    if (Globals.gUITargetShotWaiting)
                    {
                        Globals.guiPendingOverrideEvent = UI_EVENT_DEFINES.CA_MERC_SHOOT;
                    }

                    if (!Globals.gUITargetReady)
                    {
                        // Move to proper stance + direction!
                        // Convert our grid-not into an XY
                        //	ConvertGridNoToXY( usMapPos, &sTargetXPos, &sTargetYPos );

                        // Ready weapon
                        //		SoldierReadyWeapon( pSoldier, sTargetXPos, sTargetYPos, false );

                        Globals.gUITargetReady = true;
                    }
                }
                else
                {
                    Globals.gUITargetReady = false;
                }
            }
        }

        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleMChangeToAction(UI_EVENT pUIEvent)
    {
        Globals.gUITargetShotWaiting = false;

//        EndPhysicsTrajectoryUI();

        //guiNewUICursor = CONFIRM_MOVE_UICURSOR;

        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleMChangeToHandMode(UI_EVENT pUIEvent)
    {
        PathAI.ErasePath(false);

        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleAChangeToMove(UI_EVENT pUIEvent)
    {
        // Set merc glow back to normal
        // ( could have been set when in target cursor )
//        SetMercGlowNormal();

        // gsOutOfRangeGridNo = NOWHERE;

        Globals.gfPlotNewMovement = true;

        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleCWait(UI_EVENT pUIEvent)
    {
        bool fSetCursor;
        LEVELNODE? pInvTile = null;

        if (!IsometricUtils.GetMouseMapPos(out int usMapPos))
        {
            return ScreenName.GAME_SCREEN;
        }

        if (Overhead.GetSoldier(out SOLDIERTYPE? pSoldier, Globals.gusSelectedSoldier))
        {
//            pInvTile = GetCurInteractiveTile();

            if (pInvTile is not null && Globals.gpInvTileThatCausedMoveConfirm != pInvTile)
            {
                // Get out og this mode...
                Globals.guiPendingOverrideEvent = UI_EVENT_DEFINES.A_CHANGE_TO_MOVE;
                return ScreenName.GAME_SCREEN;
            }

            GetCursorMovementFlags(out MOUSE uiCursorFlags);

            if (pInvTile != null)
            {
                fSetCursor = HandleUIMovementCursor(pSoldier, uiCursorFlags, usMapPos, MOVEUI_TARGET.INTTILES);

                //Set UI CURSOR
                Globals.guiNewUICursor = InteractiveTiles.GetInteractiveTileCursor(Globals.guiNewUICursor, true);

                // Make red tile under spot... if we've previously found one...
                if (Globals.gfUIHandleShowMoveGrid > 0)
                {
                    Globals.gfUIHandleShowMoveGrid = 2;
                }

                return ScreenName.GAME_SCREEN;
            }

            // Display action points
            Globals.gfUIDisplayActionPoints = true;

            // Determine if we can afford!
//            if (!EnoughPoints(pSoldier, Globals.gsCurrentActionPoints, 0, false))
            {
                Globals.gfUIDisplayActionPointsInvalid = true;
            }

            this.SetConfirmMovementModeCursor(pSoldier, false);

            // If we are not in combat, draw path here!
            if (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.REALTIME) || !Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
            {
                //DrawUIMovementPath( pSoldier, usMapPos,  0 );
                fSetCursor = HandleUIMovementCursor(pSoldier, uiCursorFlags, usMapPos, 0);
            }

        }

        return ScreenName.GAME_SCREEN;
    }


    // NOTE, ONCE AT THIS FUNCTION, WE HAVE ASSUMED TO HAVE CHECKED FOR ENOUGH APS THROUGH
    // SelectedMercCanAffordMove
    ScreenName UIHandleCMoveMerc(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE? pSoldier;
        int sDestGridNo;
        int sActionGridNo;
        WorldDirections ubDirection = 0;
        int fAllMove;
        int bLoop;
        LEVELNODE? pIntTile;
        bool fOldFastMove;

        if (Globals.gusSelectedSoldier != Globals.NO_SOLDIER)
        {
            fAllMove = Globals.gfUIAllMoveOn;
            Globals.gfUIAllMoveOn = 0;

            if (!IsometricUtils.GetMouseMapPos(out int usMapPos))
            {
                return ScreenName.GAME_SCREEN;
            }

            // ERASE PATH
            PathAI.ErasePath(true);

            if (fAllMove > 0)
            {
                Globals.gfGetNewPathThroughPeople = true;

                // Loop through all mercs and make go!
                // TODO: Only our squad!
                for (bLoop = Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bFirstID, pSoldier = Globals.MercPtrs[bLoop]; bLoop <= Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bLastID; bLoop++)//, pSoldier++)
                {
                    if (Soldier.OK_CONTROLLABLE_MERC(pSoldier) && pSoldier.bAssignment == (Assignment)Squads.CurrentSquad() && !pSoldier.fMercAsleep)
                    {
                        // If we can't be controlled, returninvalid...
                        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.ROBOT))
                        {
                            if (!SoldierControl.CanRobotBeControlled(pSoldier))
                            {
                                continue;
                            }
                        }

//                        AdjustNoAPToFinishMove(pSoldier, false);

                        fOldFastMove = pSoldier.fUIMovementFast;

                        if (fAllMove == 2)
                        {
                            pSoldier.fUIMovementFast = true;
                            pSoldier.usUIMovementMode = AnimationStates.RUNNING;
                        }
                        else
                        {
                            pSoldier.fUIMovementFast = false;
//                            pSoldier.usUIMovementMode = GetMoveStateBasedOnStance(pSoldier, Globals.gAnimControl[pSoldier.usAnimState].ubEndHeight);
                        }

                        // Remove any previous actions
                        pSoldier.ubPendingAction = MERC.NO_PENDING_ACTION;


                        //if ( !( gTacticalStatus.uiFlags.HasFlag(INCOMBAT )) && ( Globals.gAnimControl[ pSoldier.usAnimState ].uiFlags.HasFlag(ANIM_MOVING )) )
                        //{
                        //	pSoldier.sRTPendingMovementGridNo = usMapPos;
                        //	pSoldier.usRTPendingMovementAnim  = pSoldier.usUIMovementMode;
                        //}
                        //else					
                        if (SoldierControl.EVENT_InternalGetNewSoldierPath(pSoldier, usMapPos, pSoldier.usUIMovementMode, 1, false))
                        {
//                            InternalDoMercBattleSound(pSoldier, BATTLE_SOUND.OK1, BATTLE_SND_LOWER_VOLUME);
                        }
                        else
                        {
                            Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.INTERFACE, Globals.TacticalStr[(int)STR.NO_PATH_FOR_MERC], pSoldier.name);
                        }

                        pSoldier.fUIMovementFast = fOldFastMove;

                    }
                }

                Globals.gfGetNewPathThroughPeople = false;

                // RESET MOVE FAST FLAG
                this.SetConfirmMovementModeCursor(pSoldier, true);

                Globals.gfUIAllMoveOn = 0;

            }
            else
            {
                // Get soldier
                if (Overhead.GetSoldier(out pSoldier, Globals.gusSelectedSoldier))
                {
                    // FOR REALTIME - DO MOVEMENT BASED ON STANCE!
                    if (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.REALTIME) || !Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
                    {
//                        pSoldier.usUIMovementMode = GetMoveStateBasedOnStance(pSoldier, Globals.gAnimControl[pSoldier.usAnimState].ubEndHeight);
                    }


                    sDestGridNo = usMapPos;


                    // Get structure info for in tile!
                    pIntTile = InteractiveTiles.GetCurInteractiveTileGridNoAndStructure(out int sIntTileGridNo, out STRUCTURE? pStructure);

                    // We should not have null here if we are given this flag...
                    if (pIntTile != null)
                    {
                        sActionGridNo = Overhead.FindAdjacentGridEx(pSoldier, sIntTileGridNo, ref ubDirection, out var _, false, true);
                        if (sActionGridNo != -1)
                        {
                            this.SetUIBusy(pSoldier.ubID);

                            // Set dest gridno
                            sDestGridNo = sActionGridNo;

                            // check if we are at this location
                            if (pSoldier.sGridNo == sDestGridNo)
                            {
                                InteractiveTiles.StartInteractiveObject(sIntTileGridNo, pStructure.usStructureID, pSoldier, ubDirection);
                                InteractiveTiles.InteractWithInteractiveObject(pSoldier, pStructure, ubDirection);
                                return ScreenName.GAME_SCREEN;
                            }
                        }
                        else
                        {
//                            Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, Globals.TacticalStr[NO_PATH]);
                            return ScreenName.GAME_SCREEN;
                        }
                    }

                    this.SetUIBusy(pSoldier.ubID);

                    if (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.REALTIME) || !Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
                    {
                        // RESET MOVE FAST FLAG
                        this.SetConfirmMovementModeCursor(pSoldier, true);

                        if (!Globals.gTacticalStatus.fAtLeastOneGuyOnMultiSelect)
                        {
                            pSoldier.fUIMovementFast = false;
                        }

                        //StartLooseCursor( usMapPos, 0 );
                    }

                    if (Globals.gTacticalStatus.fAtLeastOneGuyOnMultiSelect && pIntTile == null)
                    {
                        this.HandleMultiSelectionMove(sDestGridNo);
                    }
                    else
                    {
                        if (Globals.gUIUseReverse)
                        {
                            pSoldier.bReverse = true;
                        }
                        else
                        {
                            pSoldier.bReverse = false;
                        }

                        // Remove any previous actions
                        pSoldier.ubPendingAction = MERC.NO_PENDING_ACTION;

                        {
                            SoldierControl.EVENT_InternalGetNewSoldierPath(pSoldier, sDestGridNo, pSoldier.usUIMovementMode, 1, pSoldier.fNoAPToFinishMove);
                        }

                        if (pSoldier.usPathDataSize > 5)
                        {
//                            DoMercBattleSound(pSoldier, BATTLE_SOUND.OK1);
                        }

                        // HANDLE ANY INTERACTIVE OBJECTS HERE!
                        if (pIntTile != null)
                        {
//                            StartInteractiveObject(sIntTileGridNo, pStructure.usStructureID, pSoldier, out ubDirection);
                        }
                    }
                }
            }
        }
        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleMCycleMoveAll(UI_EVENT pUIEvent)
    {

        if (!Overhead.GetSoldier(out SOLDIERTYPE? pSoldier, Globals.gusSelectedSoldier))
        {
            return ScreenName.GAME_SCREEN;
        }

        if (Globals.gfUICanBeginAllMoveCycle)
        {
            Globals.gfUIAllMoveOn = 1;
            Globals.gfUICanBeginAllMoveCycle = false;
        }
        return ScreenName.GAME_SCREEN;
    }


    ScreenName UIHandleMCycleMovement(UI_EVENT pUIEvent)
    {
        bool fGoodMode = false;

        if (!Overhead.GetSoldier(out SOLDIERTYPE pSoldier, Globals.gusSelectedSoldier))
        {
            return ScreenName.GAME_SCREEN;
        }

        Globals.gfUIAllMoveOn = 0;

        if (pSoldier.ubBodyType == SoldierBodyTypes.ROBOTNOWEAPON)
        {
            pSoldier.usUIMovementMode = AnimationStates.WALKING;
            Globals.gfPlotNewMovement = true;
            return ScreenName.GAME_SCREEN;
        }

        do
        {
            // Cycle gmovement state
            if (pSoldier.usUIMovementMode == AnimationStates.RUNNING)
            {
                pSoldier.usUIMovementMode = AnimationStates.WALKING;
                if (SoldierControl.IsValidMovementMode(pSoldier, AnimationStates.WALKING))
                {
                    fGoodMode = true;
                }
            }
            else if (pSoldier.usUIMovementMode == AnimationStates.WALKING)
            {
                pSoldier.usUIMovementMode = AnimationStates.SWATTING;
                if (SoldierControl.IsValidMovementMode(pSoldier, AnimationStates.SWATTING))
                {
                    fGoodMode = true;
                }
            }
            else if (pSoldier.usUIMovementMode == AnimationStates.SWATTING)
            {
                pSoldier.usUIMovementMode = AnimationStates.CRAWLING;
                if (SoldierControl.IsValidMovementMode(pSoldier, AnimationStates.CRAWLING))
                {
                    fGoodMode = true;
                }
            }
            else if (pSoldier.usUIMovementMode == AnimationStates.CRAWLING)
            {
                pSoldier.fUIMovementFast = true;
                pSoldier.usUIMovementMode = AnimationStates.RUNNING;
                if (SoldierControl.IsValidMovementMode(pSoldier, AnimationStates.RUNNING))
                {
                    fGoodMode = true;
                }
            }

        } while (fGoodMode != true);

        Globals.gfPlotNewMovement = true;

        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleCOnTerrain(UI_EVENT pUIEvent)
    {
        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleMAdjustStanceMode(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE? pSoldier;
        bool fCheck = false;
        int iPosDiff;
        byte bNewDirection;

        // Change cusror to normal
        Globals.guiNewUICursor = UICursorDefines.NO_UICURSOR;


        if (pUIEvent.fFirstTime)
        {
            Globals.gusAnchorMouseY = Globals.gusMouseYPos;
            Globals.usOldMouseY = Globals.gusMouseYPos;
            Globals.ubNearHeigherLevel = false;
            Globals.ubNearLowerLevel = false;

            Globals.guiShowUPDownArrows = ARROWS.SHOW_DOWN_BESIDE | ARROWS.SHOW_UP_BESIDE;
            Globals.uiOldShowUPDownArrows = Globals.guiShowUPDownArrows;

            Globals.gbAdjustStanceDiff = 0;
            Globals.gbClimbID = 0;

            Globals.gfIgnoreScrolling = true;

            // Get soldier current height of animation
            if (Overhead.GetSoldier(out pSoldier, Globals.gusSelectedSoldier))
            {
                // IF we are on a basic level...(temp)
                if (pSoldier.bLevel == 0)
                {
//                    if (FindHeigherLevel(pSoldier, pSoldier.sGridNo, pSoldier.bDirection, out bNewDirection))
//                    {
//                        Globals.ubNearHeigherLevel = true;
//                    }
                }

                // IF we are higher...
                if (pSoldier.bLevel > 0)
                {
//                    if (FindLowerLevel(pSoldier, pSoldier.sGridNo, pSoldier.bDirection, out bNewDirection))
//                    {
//                        Globals.ubNearLowerLevel = true;
//                    }
                }

                switch (Globals.gAnimControl[pSoldier.usAnimState].ubEndHeight)
                {
                    case AnimationHeights.ANIM_STAND:
                        if (Globals.ubNearHeigherLevel)
                        {
                            Globals.ubUpHeight = 1;
                            Globals.ubDownDepth = 2;
                        }
                        else if (Globals.ubNearLowerLevel)
                        {
                            Globals.ubUpHeight = 0;
                            Globals.ubDownDepth = 3;
                        }
                        else
                        {
                            Globals.ubUpHeight = 0;
                            Globals.ubDownDepth = 2;
                        }
                        break;

                    case AnimationHeights.ANIM_CROUCH:
                        if (Globals.ubNearHeigherLevel)
                        {
                            Globals.ubUpHeight = 2;
                            Globals.ubDownDepth = 1;
                        }
                        else if (Globals.ubNearLowerLevel)
                        {
                            Globals.ubUpHeight = 1;
                            Globals.ubDownDepth = 2;
                        }
                        else
                        {
                            Globals.ubUpHeight = 1;
                            Globals.ubDownDepth = 1;
                        }
                        break;

                    case AnimationHeights.ANIM_PRONE:
                        if (Globals.ubNearHeigherLevel)
                        {
                            Globals.ubUpHeight = 3;
                            Globals.ubDownDepth = 0;
                        }
                        else if (Globals.ubNearLowerLevel)
                        {
                            Globals.ubUpHeight = 2;
                            Globals.ubDownDepth = 1;
                        }
                        else
                        {
                            Globals.ubUpHeight = 2;
                            Globals.ubDownDepth = 0;
                        }
                        break;
                }
            }
        }

        // Check if delta X has changed alot since last time
        iPosDiff = Math.Abs((int)(Globals.usOldMouseY - Globals.gusMouseYPos));

        //Globals.guiShowUPDownArrows = ARROWS.SHOW_DOWN_BESIDE | ARROWS.SHOW_UP_BESIDE;
        Globals.guiShowUPDownArrows = Globals.uiOldShowUPDownArrows;

        {
            if (Globals.gusAnchorMouseY > Globals.gusMouseYPos)
            {
                // Get soldier
                if (Overhead.GetSoldier(out pSoldier, Globals.gusSelectedSoldier))
                {
                    if (iPosDiff < GO_MOVE_ONE && Globals.ubUpHeight >= 1)
                    {
                        // Change arrows to move down arrow + show
                        //Globals.guiShowUPDownArrows = ARROWS.SHOW_UP_ABOVE_Y;
                        Globals.guiShowUPDownArrows = ARROWS.SHOW_DOWN_BESIDE | ARROWS.SHOW_UP_BESIDE;
                        Globals.gbAdjustStanceDiff = 0;
                        Globals.gbClimbID = 0;
                    }
                    else if (iPosDiff > GO_MOVE_ONE && iPosDiff < GO_MOVE_TWO && Globals.ubUpHeight >= 1)
                    {
                        //Globals.guiShowUPDownArrows = ARROWS.SHOW_UP_ABOVE_G;
                        if (Globals.ubUpHeight == 1 && Globals.ubNearHeigherLevel)
                        {
                            Globals.guiShowUPDownArrows = ARROWS.SHOW_UP_ABOVE_CLIMB;
                            Globals.gbClimbID = 1;
                        }
                        else
                        {
                            Globals.guiShowUPDownArrows = ARROWS.SHOW_UP_ABOVE_Y;
                            Globals.gbClimbID = 0;
                        }

                        Globals.gbAdjustStanceDiff = 1;
                    }
                    else if (iPosDiff >= GO_MOVE_TWO && iPosDiff < GO_MOVE_THREE && Globals.ubUpHeight >= 2)
                    {
                        if (Globals.ubUpHeight == 2 && Globals.ubNearHeigherLevel)
                        {
                            Globals.guiShowUPDownArrows = ARROWS.SHOW_UP_ABOVE_CLIMB;
                            Globals.gbClimbID = 1;
                        }
                        else
                        {
                            Globals.guiShowUPDownArrows = ARROWS.SHOW_UP_ABOVE_YY;
                            Globals.gbClimbID = 0;
                        }

                        Globals.gbAdjustStanceDiff = 2;
                    }
                    else if (iPosDiff >= GO_MOVE_THREE && Globals.ubUpHeight >= 3)
                    {
                        if (Globals.ubUpHeight == 3 && Globals.ubNearHeigherLevel)
                        {
                            Globals.guiShowUPDownArrows = ARROWS.SHOW_UP_ABOVE_CLIMB;
                            Globals.gbClimbID = 1;
                        }
                    }
                }

            }

            if (Globals.gusAnchorMouseY < Globals.gusMouseYPos)
            {

                // Get soldier
                if (Overhead.GetSoldier(out pSoldier, Globals.gusSelectedSoldier))
                {
                    if (iPosDiff < GO_MOVE_ONE && Globals.ubDownDepth >= 1)
                    {
                        // Change arrows to move down arrow + show
                        //Globals.guiShowUPDownArrows = ARROWS.SHOW_DOWN_BELOW_Y;
                        Globals.guiShowUPDownArrows = ARROWS.SHOW_DOWN_BESIDE | ARROWS.SHOW_UP_BESIDE;
                        Globals.gbAdjustStanceDiff = 0;
                        Globals.gbClimbID = 0;
                    }
                    else if (iPosDiff >= GO_MOVE_ONE && iPosDiff < GO_MOVE_TWO && Globals.ubDownDepth >= 1)
                    {
                        //						Globals.guiShowUPDownArrows = ARROWS.SHOW_DOWN_BELOW_G;
                        if (Globals.ubDownDepth == 1 && Globals.ubNearLowerLevel)
                        {
                            Globals.guiShowUPDownArrows = ARROWS.SHOW_DOWN_CLIMB;
                            Globals.gbClimbID = -1;
                        }
                        else
                        {
                            Globals.guiShowUPDownArrows = ARROWS.SHOW_DOWN_BELOW_Y;
                            Globals.gbClimbID = 0;
                        }

                        Globals.gbAdjustStanceDiff = -1;
                    }
                    else if (iPosDiff > GO_MOVE_TWO && iPosDiff < GO_MOVE_THREE && Globals.ubDownDepth >= 2)
                    {
                        //Globals.guiShowUPDownArrows = ARROWS.SHOW_DOWN_BELOW_GG;
                        if (Globals.ubDownDepth == 2 && Globals.ubNearLowerLevel)
                        {
                            Globals.guiShowUPDownArrows = ARROWS.SHOW_DOWN_CLIMB;
                            Globals.gbClimbID = -1;
                        }
                        else
                        {
                            Globals.guiShowUPDownArrows = ARROWS.SHOW_DOWN_BELOW_YY;
                            Globals.gbClimbID = 0;
                        }

                        Globals.gbAdjustStanceDiff = -2;
                    }
                    else if (iPosDiff > GO_MOVE_THREE && Globals.ubDownDepth >= 3)
                    {
                        //Globals.guiShowUPDownArrows = ARROWS.SHOW_DOWN_BELOW_GG;
                        if (Globals.ubDownDepth == 3 && Globals.ubNearLowerLevel)
                        {
                            Globals.guiShowUPDownArrows = ARROWS.SHOW_DOWN_CLIMB;
                            Globals.gbClimbID = -1;
                        }
                    }
                }
            }
        }

        Globals.uiOldShowUPDownArrows = Globals.guiShowUPDownArrows;

        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleAChangeToConfirmAction(UI_EVENT pUIEvent)
    {

        if (Overhead.GetSoldier(out SOLDIERTYPE pSoldier, Globals.gusSelectedSoldier))
        {
            //            HandleLeftClickCursor(pSoldier);
        }

        //        ResetBurstLocations();

        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleCAOnTerrain(UI_EVENT pUIEvent)
    {

        if (!IsometricUtils.GetMouseMapPos(out int usMapPos))
        {
            return ScreenName.GAME_SCREEN;
        }

        if (Overhead.GetSoldier(out SOLDIERTYPE pSoldier, Globals.gusSelectedSoldier))
        {
//            Globals.guiNewUICursor = GetProperItemCursor((byte)Globals.gusSelectedSoldier, pSoldier.inv[InventorySlot.HANDPOS].usItem, usMapPos, true);

            UIHandleOnMerc(false);
        }

        return ScreenName.GAME_SCREEN;
    }

    public void UIHandleMercAttack(SOLDIERTYPE? pSoldier, SOLDIERTYPE? pTargetSoldier, int usMapPos)
    {
        ITEM_HANDLE iHandleReturn = 0;
        int sTargetGridNo;
        int bTargetLevel;
        Items usItem;
        LEVELNODE? pIntNode = null;
        STRUCTURE? pStructure = null;
        int sGridNo = 0, sNewGridNo = 0;
        CURS ubItemCursor = 0;

        // get cursor
//        ubItemCursor = GetActionModeCursor(pSoldier);

        if (!Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)
            && pTargetSoldier is not null
            && Globals.Item[pSoldier.inv[InventorySlot.HANDPOS].usItem].usItemClass.HasFlag(IC.WEAPON))
        {
//            if (NPCFirstDraw(pSoldier, pTargetSoldier))
//            {
//                // go into turnbased for that person
//                AIMain.CancelAIAction(pTargetSoldier, 1);
//                AddToShouldBecomeHostileOrSayQuoteList(pTargetSoldier.ubID);
//                //MakeCivHostile( pTargetSoldier, 2 );
//                //TriggerNPCWithIHateYouQuote( pTargetSoldier.ubProfile );
//                return;
//            }
        }

        // Set aim time to one in UI
        pSoldier.bAimTime = pSoldier.bShownAimTime / 2;
        usItem = pSoldier.inv[InventorySlot.HANDPOS].usItem;

        // ATE: Check if we are targeting an interactive tile, and adjust gridno accordingly...
//        pIntNode = GetCurInteractiveTileGridNoAndStructure(out sGridNo, out pStructure);

        if (pTargetSoldier != null)
        {
            sTargetGridNo = pTargetSoldier.sGridNo;
            bTargetLevel = pTargetSoldier.bLevel;
        }
        else
        {
            sTargetGridNo = usMapPos;
            bTargetLevel = Globals.gsInterfaceLevel;

            if (pIntNode != null)
            {
                // Change gridno....
                sTargetGridNo = sGridNo;
            }
        }

        // here, change gridno if we're targeting ourselves....
        if (pIntNode != null)
        {
            // Are we in the same gridno?
            if (sGridNo == pSoldier.sGridNo && ubItemCursor != CURS.AIDCURS)
            {
                // Get orientation....
                sNewGridNo = pStructure.ubWallOrientation switch
                {
                    WallOrientation.OUTSIDE_TOP_LEFT or WallOrientation.INSIDE_TOP_LEFT => IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc(WorldDirections.SOUTH)),
                    WallOrientation.OUTSIDE_TOP_RIGHT or WallOrientation.INSIDE_TOP_RIGHT => IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc(WorldDirections.EAST)),
                    _ => sGridNo,
                };

                // Set target gridno to this one...
                sTargetGridNo = sNewGridNo;

                // now set target cube height
                // CJC says to hardcode this value :)
                pSoldier.bTargetCubeLevel = 2;
            }
            else
            {
                // ATE: Refine this a bit - if we have nobody as a target...
                if (pTargetSoldier == null)
                {
                    sTargetGridNo = sGridNo;
                }
            }
        }


        // Cannot be fire if we are already in a fire animation....
        // this is to stop the shooting trigger/happy duded from contiously pressing fire...
        if (!Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
        {
            if (Globals.gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.FIRE))
            {
                return;
            }
        }

        // If in turn-based mode - return to movement
        if (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
        {
            // Reset some flags for cont move...
            pSoldier.sFinalDestination = pSoldier.sGridNo;
            pSoldier.bGoodContPath = 0;
            //  Globals.guiPendingOverrideEvent = A_CHANGE_TO_MOVE;
        }


        if (pSoldier.bWeaponMode == WM.ATTACHED)
        {
//            iHandleReturn = HandleItem(pSoldier, sTargetGridNo, bTargetLevel, Items.UNDER_GLAUNCHER, true);
        }
        else
        {
//            iHandleReturn = HandleItem(pSoldier, sTargetGridNo, bTargetLevel, pSoldier.inv[InventorySlot.HANDPOS].usItem, true);
        }

        if (iHandleReturn < 0)
        {
            if (iHandleReturn == ITEM_HANDLE.RELOADING)
            {
                Globals.guiNewUICursor = UICursorDefines.ACTION_TARGET_RELOADING;
                return;
            }

            if (iHandleReturn == ITEM_HANDLE.NOROOM)
            {
                Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, pMessageStrings[MSG.CANT_FIRE_HERE]);
                return;
            }
        }


        if (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED) && !Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
        {
//            HandleUICursorRTFeedback(pSoldier);
        }

        Globals.gfUIForceReExamineCursorData = true;
    }

    void AttackRequesterCallback(MessageBoxReturnCode bExitValue)
    {
        if (bExitValue == MessageBoxReturnCode.MSG_BOX_RETURN_YES)
        {
            Globals.gTacticalStatus.ubLastRequesterTargetID = Globals.gpRequesterTargetMerc.ubProfile;

            this.UIHandleMercAttack(Globals.gpRequesterMerc, Globals.gpRequesterTargetMerc, Globals.gsRequesterGridNo);
        }
    }


    ScreenName UIHandleCAMercShoot(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE? pTSoldier = null;
        bool fDidRequester = false;

        if (Globals.gusSelectedSoldier != Globals.NO_SOLDIER)
        {

            if (!IsometricUtils.GetMouseMapPos(out int usMapPos))
            {
                return ScreenName.GAME_SCREEN;
            }

            // Get soldier
            if (Overhead.GetSoldier(out SOLDIERTYPE? pSoldier, Globals.gusSelectedSoldier))
            {
                // Get target guy...
                if (Globals.gfUIFullTargetFound)
                {
                    // Get target soldier, if one exists
                    pTSoldier = Globals.MercPtrs[Globals.gusUIFullTargetID];
                }


                if (pTSoldier != null)
                {
                    // If this is one of our own guys.....pop up requiester...
                    if ((pTSoldier.bTeam == Globals.gbPlayerNum || pTSoldier.bTeam == TEAM.MILITIA_TEAM)
                        && Globals.Item[pSoldier.inv[InventorySlot.HANDPOS].usItem].usItemClass != IC.MEDKIT
                        && pSoldier.inv[InventorySlot.HANDPOS].usItem != Items.GAS_CAN
                        && Globals.gTacticalStatus.ubLastRequesterTargetID != pTSoldier.ubProfile
                        && (pTSoldier.ubID != pSoldier.ubID))
                    {
                        string zStr = string.Empty;

                        Globals.gpRequesterMerc = pSoldier;
                        Globals.gpRequesterTargetMerc = pTSoldier;
                        Globals.gsRequesterGridNo = usMapPos;

                        fDidRequester = true;

                        // wprintf(zStr, Globals.TacticalStr[ATTACK_OWN_GUY_PROMPT], pTSoldier.name);

                        Rectangle? _ = null;

                        this.messageBoxSubSystem.DoMessageBox(
                            MessageBoxStyle.MSG_BOX_BASIC_STYLE,
                            zStr,
                            ScreenName.GAME_SCREEN,
                            MSG_BOX_FLAG.YESNO,
                            this.AttackRequesterCallback,
                            ref _);
                    }
                }

                if (!fDidRequester)
                {
                    this.UIHandleMercAttack(pSoldier, pTSoldier, usMapPos);
                }
            }
        }

        return ScreenName.GAME_SCREEN;
    }


    ScreenName UIHandleAEndAction(UI_EVENT pUIEvent)
    {

        // Get gridno at this location
        if (!IsometricUtils.GetMouseMapPos(out int usMapPos))
        {
            return ScreenName.GAME_SCREEN;
        }

        if (Overhead.GetSoldier(out SOLDIERTYPE? pSoldier, Globals.gusSelectedSoldier))
        {
            if (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.REALTIME)
                || !Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
            {
                if (Globals.gUITargetReady)
                {
                    // Move to proper stance + direction!
                    // Convert our grid-not into an XY
                    IsometricUtils.ConvertGridNoToXY(usMapPos, out int sTargetXPos, out int sTargetYPos);

                    // UNReady weapon
                    SoldierControl.SoldierReadyWeapon(pSoldier, sTargetXPos, sTargetYPos, true);

                    Globals.gUITargetReady = false;
                }

            }
        }
        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleCAEndConfirmAction(UI_EVENT pUIEvent)
    {

        if (Overhead.GetSoldier(out SOLDIERTYPE? pSoldier, Globals.gusSelectedSoldier))
        {
            //            HandleEndConfirmCursor(pSoldier);
        }

        return ScreenName.GAME_SCREEN;
    }


    ScreenName UIHandleIOnTerrain(UI_EVENT pUIEvent)
    {

        // Get gridno at this location
        if (!IsometricUtils.GetMouseMapPos(out int usMapPos))
        {
            return ScreenName.GAME_SCREEN;
        }


        if (!UIHandleOnMerc(true))
        {
            // Check if dest is OK
            //if ( !NewOKDestination( usMapPos, false ) || IsRoofVisible( usMapPos ) )
            ////{
            //	guiNewUICursor = CANNOT_MOVE_UICURSOR;				
            //}
            //else
            {
                Globals.guiNewUICursor = UICursorDefines.NORMAL_SNAPUICURSOR;
            }
        }

        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleIChangeToIdle(UI_EVENT pUIEvent)
    {
        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandlePADJAdjustStance(UI_EVENT pUIEvent)
    {
        AnimationHeights ubNewStance;
        bool fChangeStance = false;

        Globals.guiShowUPDownArrows = ARROWS.HIDE_UP | ARROWS.HIDE_DOWN;


        Globals.gfIgnoreScrolling = false;

        if (Globals.gusSelectedSoldier != Globals.NO_SOLDIER && Globals.gbAdjustStanceDiff != 0)
        {
            // Get soldier
            if (Overhead.GetSoldier(out SOLDIERTYPE? pSoldier, Globals.gusSelectedSoldier))
            {
                ubNewStance = this.GetAdjustedAnimHeight(Globals.gAnimControl[pSoldier.usAnimState].ubEndHeight, Globals.gbAdjustStanceDiff);

                if (Globals.gbClimbID == 1)
                {
//                    BeginSoldierClimbUpRoof(pSoldier);
                }
                else if (Globals.gbClimbID == -1)
                {
//                    BeginSoldierClimbDownRoof(pSoldier);
                }
                else
                {
                    // Set state to result
                    this.UIHandleSoldierStanceChange(pSoldier.ubID, ubNewStance);
                }

                // Once we have APs, we can safely reset nomove flag!
                // AdjustNoAPToFinishMove( pSoldier, false );

            }

        }
        return ScreenName.GAME_SCREEN;
    }


    AnimationHeights GetAdjustedAnimHeight(AnimationHeights ubAnimHeight, int bChange)
    {
        AnimationHeights ubNewAnimHeight = ubAnimHeight;

        if (ubAnimHeight == AnimationHeights.ANIM_STAND)
        {
            if (bChange == -1)
            {
                ubNewAnimHeight = AnimationHeights.ANIM_CROUCH;
            }
            if (bChange == -2)
            {
                ubNewAnimHeight = AnimationHeights.ANIM_PRONE;
            }
            if (bChange == 1)
            {
                ubNewAnimHeight = (AnimationHeights)50;
            }
        }
        else if (ubAnimHeight == AnimationHeights.ANIM_CROUCH)
        {
            if (bChange == 1)
            {
                ubNewAnimHeight = AnimationHeights.ANIM_STAND;
            }
            if (bChange == -1)
            {
                ubNewAnimHeight = AnimationHeights.ANIM_PRONE;
            }
            if (bChange == -2)
            {
                ubNewAnimHeight = (AnimationHeights)55;
            }
        }
        else if (ubAnimHeight == AnimationHeights.ANIM_PRONE)
        {
            if (bChange == -1)
            {
                ubNewAnimHeight = (AnimationHeights)55;
            }
            if (bChange == 1)
            {
                ubNewAnimHeight = AnimationHeights.ANIM_CROUCH;
            }
            if (bChange == 2)
            {
                ubNewAnimHeight = AnimationHeights.ANIM_STAND;
            }
        }

        return ubNewAnimHeight;
    }

    void HandleObjectHighlighting()
    {

        if (!IsometricUtils.GetMouseMapPos(out int usMapPos))
        {
            return;
        }

        // CHECK IF WE'RE ON A GUY ( EITHER SELECTED, OURS, OR THEIRS
        if (Globals.gfUIFullTargetFound)
        {
            // Get Soldier
            Overhead.GetSoldier(out SOLDIERTYPE? pSoldier, Globals.gusUIFullTargetID);

            // If an enemy, and in a given mode, highlight
            if (Globals.guiUIFullTargetFlags.HasFlag(FIND_SOLDIER_RESPONSES.ENEMY_MERC))
            {
                switch (Globals.gCurrentUIMode)
                {

                    case UI_MODE.CONFIRM_MOVE_MODE:
                    case UI_MODE.MENU_MODE:

                        break;

                    case UI_MODE.MOVE_MODE:
                    case UI_MODE.CONFIRM_ACTION_MODE:
                    //case ACTION_MODE:
                    case UI_MODE.IDLE_MODE:

                        // Set as selected
                        //pSoldier.pCurrentShade = pSoldier.pShades[ 1 ];
                        break;
                }
            }
            else if (Globals.guiUIFullTargetFlags.HasFlag(FIND_SOLDIER_RESPONSES.OWNED_MERC))
            {
                // Check for selected
                pSoldier.pCurrentShade = pSoldier.pShades[0];
                Globals.gfUIDoNotHighlightSelMerc = true;
            }
        }

    }

    void AdjustSoldierCreationStartValues()
    {
        int cnt;
        SOLDIERTYPE? pSoldier;


        cnt = Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bFirstID;
        Globals.guiCreateGuyIndex = (int)cnt;

        for (pSoldier = Globals.MercPtrs[cnt]; cnt <= Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bLastID; cnt++)//, pSoldier++)
        {
            if (!pSoldier.bActive)
            {
                Globals.guiCreateGuyIndex = (int)cnt;
                break;
            }
        }

        cnt = Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bLastID + 1;
        Globals.guiCreateBadGuyIndex = (int)cnt;

        for (pSoldier = Globals.MercPtrs[cnt]; cnt <= Globals.gTacticalStatus.Team[TEAM.LAST_TEAM].bLastID; cnt++)//, pSoldier++)
        {
            if (!pSoldier.bActive && cnt > Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bLastID)
            {
                Globals.guiCreateBadGuyIndex = (int)cnt;
                break;
            }
        }

    }

    bool SelectedMercCanAffordAttack()
    {
        int sTargetGridNo;
        bool fEnoughPoints = true;
        int sAPCost;
        CURS ubItemCursor = 0;
        Items usInHand;

        if (Globals.gusSelectedSoldier != Globals.NO_SOLDIER)
        {

            if (!IsometricUtils.GetMouseMapPos(out int usMapPos))
            {
                return true; // (ScreenName.GAME_SCREEN);
            }

            // Get soldier
            if (Overhead.GetSoldier(out SOLDIERTYPE? pSoldier, Globals.gusSelectedSoldier))
            {
                // LOOK IN GUY'S HAND TO CHECK LOCATION
                usInHand = pSoldier.inv[InventorySlot.HANDPOS].usItem;

                // Get cursor value
//                ubItemCursor = GetActionModeCursor(pSoldier);

                if (ubItemCursor == CURS.INVALIDCURS)
                {
                    return false;
                }

                if (ubItemCursor == CURS.BOMBCURS)
                {
                    // Check as...
//                    if (EnoughPoints(pSoldier, GetTotalAPsToDropBomb(pSoldier, usMapPos), 0, true))
//                    {
//                        return (true);
//                    }
                }
                else if (ubItemCursor == CURS.REMOTECURS)
                {
                    // Check as...
//                    if (EnoughPoints(pSoldier, GetAPsToUseRemote(pSoldier), 0, true))
//                    {
//                        return (true);
//                    }
                }
                else
                {
                    // Look for a soldier at this position
                    if (Globals.gfUIFullTargetFound)
                    {
                        // GetSoldier
                        Overhead.GetSoldier(out SOLDIERTYPE? pTargetSoldier, Globals.gusUIFullTargetID);
                        sTargetGridNo = pTargetSoldier.sGridNo;
                    }
                    else
                    {
                        sTargetGridNo = usMapPos;
                    }

                    sAPCost = this.points.CalcTotalAPsToAttack(pSoldier, sTargetGridNo, 1, (byte)(pSoldier.bShownAimTime / 2));

//                    if (EnoughPoints(pSoldier, sAPCost, 0, true))
//                    {
//                        return (true);
//                    }
//                    else
//                    {
//                        // Play curse....
//                        DoMercBattleSound(pSoldier, BATTLE_SOUND.CURSE1);
//                    }
                }
            }

        }

        return false;
    }


    bool SelectedMercCanAffordMove()
    {
        uint sAPCost = 0;
        int sBPCost = 0;
        LEVELNODE? pIntTile = null;

        // Get soldier
        if (Overhead.GetSoldier(out SOLDIERTYPE? pSoldier, Globals.gusSelectedSoldier))
        {
            if (!IsometricUtils.GetMouseMapPos(out int usMapPos))
            {
                return true;//(ScreenName.GAME_SCREEN);
            }


            // IF WE ARE OVER AN INTERACTIVE TILE, GIVE GRIDNO OF POSITION
//            pIntTile = GetCurInteractiveTile();

            if (pIntTile != null)
            {
                // CHECK APS
//                if (EnoughPoints(pSoldier, Globals.gsCurrentActionPoints, 0, true))
//                {
//                    return (true);
//                }
//                else
//                {
//                    return (false);
//                }
            }

            // Take the first direction!
//            sAPCost = PtsToMoveDirection(pSoldier, Globals.guiPathingData[0]);

//            sAPCost += GetAPsToChangeStance(pSoldier, Globals.gAnimControl[pSoldier.usUIMovementMode].ubHeight);

//            if (EnoughPoints(pSoldier, sAPCost, 0, true))
//            {
//                return (true);
//            }
//            else
//            {
//                // OK, remember where we were trying to get to.....
//                pSoldier.sContPathLocation = usMapPos;
//                pSoldier.bGoodContPath = 1;
//            }
        }

        return false;
    }

    void GetMercClimbDirection(int ubSoldierID, out bool pfGoDown, out bool pfGoUp)
    {
        byte bNewDirection;

        pfGoDown = false;
        pfGoUp = false;

        if (!Overhead.GetSoldier(out SOLDIERTYPE? pSoldier, ubSoldierID))
        {
            return;
        }

        // Check if we are close / can climb
        if (pSoldier.bLevel == 0)
        {
            // See if we are not in a building!
//            if (FindHeigherLevel(pSoldier, pSoldier.sGridNo, pSoldier.bDirection, out bNewDirection))
            {
                pfGoUp = true;
            }
        }

        // IF we are higher...
        if (pSoldier.bLevel > 0)
        {
//            if (FindLowerLevel(pSoldier, pSoldier.sGridNo, pSoldier.bDirection, out bNewDirection))
            {
                pfGoDown = true;
            }
        }
    }

    void RemoveTacticalCursor()
    {
        Globals.guiNewUICursor = UICursorDefines.NO_UICURSOR;
        PathAI.ErasePath(true);
    }

    ScreenName UIHandlePOPUPMSG(UI_EVENT pUIEvent)
    {
        return ScreenName.GAME_SCREEN;
    }


    ScreenName UIHandleHCOnTerrain(UI_EVENT pUIEvent)
    {

        if (!IsometricUtils.GetMouseMapPos(out int usMapPos))
        {
            return ScreenName.GAME_SCREEN;
        }

        if (!Overhead.GetSoldier(out SOLDIERTYPE? pSoldier, Globals.gusSelectedSoldier))
        {
            return ScreenName.GAME_SCREEN;
        }

        // If we are out of breath, no cursor...
        if (pSoldier.bBreath < Globals.OKBREATH && pSoldier.bCollapsed)
        {
            Globals.guiNewUICursor = UICursorDefines.INVALID_ACTION_UICURSOR;
        }
        else
        {
            if (Globals.gsOverItemsGridNo != Globals.NOWHERE
                && (usMapPos != Globals.gsOverItemsGridNo
                || Globals.gsInterfaceLevel != Globals.gsOverItemsLevel))
            {
                Globals.gsOverItemsGridNo = Globals.NOWHERE;
                Globals.guiPendingOverrideEvent = UI_EVENT_DEFINES.A_CHANGE_TO_MOVE;
            }
            else
            {
                Globals.guiNewUICursor = UICursorDefines.NORMALHANDCURSOR_UICURSOR;

                UIHandleInteractiveTilesAndItemsOnTerrain(pSoldier, usMapPos, true, false);
            }
        }

        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleHCGettingItem(UI_EVENT pUIEvent)
    {
        Globals.guiNewUICursor = UICursorDefines.NORMAL_FREEUICURSOR;

        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleTATalkingMenu(UI_EVENT pUIEvent)
    {
        Globals.guiNewUICursor = UICursorDefines.NORMAL_FREEUICURSOR;

        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleEXExitSectorMenu(UI_EVENT pUIEvent)
    {
        Globals.guiNewUICursor = UICursorDefines.NORMAL_FREEUICURSOR;

        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleOpenDoorMenu(UI_EVENT pUIEvent)
    {
        Globals.guiNewUICursor = UICursorDefines.NORMAL_FREEUICURSOR;

        return ScreenName.GAME_SCREEN;
    }

    void ToggleHandCursorMode(out UI_EVENT_DEFINES puiNewEvent)
    {
        // Toggle modes			
        if (Globals.gCurrentUIMode == UI_MODE.HANDCURSOR_MODE)
        {
            puiNewEvent = UI_EVENT_DEFINES.A_CHANGE_TO_MOVE;
        }
        else
        {
            puiNewEvent = UI_EVENT_DEFINES.M_CHANGE_TO_HANDMODE;
        }
    }

    void ToggleTalkCursorMode(out UI_EVENT_DEFINES puiNewEvent)
    {
        // Toggle modes			
        if (Globals.gCurrentUIMode == UI_MODE.TALKCURSOR_MODE)
        {
            puiNewEvent = UI_EVENT_DEFINES.A_CHANGE_TO_MOVE;
        }
        else
        {
            puiNewEvent = UI_EVENT_DEFINES.T_CHANGE_TO_TALKING;
        }
    }

    void ToggleLookCursorMode(UI_EVENT_DEFINES puiNewEvent)
    {
        // Toggle modes			
        if (Globals.gCurrentUIMode == UI_MODE.LOOKCURSOR_MODE)
        {
            Globals.guiPendingOverrideEvent = UI_EVENT_DEFINES.A_CHANGE_TO_MOVE;
            HandleTacticalUI();
        }
        else
        {
            Globals.guiPendingOverrideEvent = UI_EVENT_DEFINES.LC_CHANGE_TO_LOOK;
            HandleTacticalUI();
        }
    }

    public static bool UIHandleOnMerc(bool fMovementMode)
    {
        int usSoldierIndex;
        FIND_SOLDIER_RESPONSES uiMercFlags;
        bool fFoundMerc = false;

        if (!IsometricUtils.GetMouseMapPos(out int usMapPos))
        {
            return true;//(ScreenName.GAME_SCREEN);
        }

        //if ( fMovementMode )
        //{
        //	fFoundMerc			= gfUISelectiveTargetFound;
        //	usSoldierIndex	= gusUISelectiveTargetID;
        //	uiMercFlags			= guiUISelectiveTargetFlags;
        //}
        //else
        {
            fFoundMerc = Globals.gfUIFullTargetFound;
            usSoldierIndex = Globals.gusUIFullTargetID;
            uiMercFlags = Globals.guiUIFullTargetFlags;
        }

        // CHECK IF WE'RE ON A GUY ( EITHER SELECTED, OURS, OR THEIRS
        if (fFoundMerc)
        {
            // Get Soldier
            Overhead.GetSoldier(out SOLDIERTYPE? pSoldier, usSoldierIndex);

            if (uiMercFlags.HasFlag(FIND_SOLDIER_RESPONSES.OWNED_MERC))
            {
                // ATE: Check if this is an empty vehicle.....
                //if ( OK_ENTERABLE_VEHICLE( pSoldier ) && GetNumberInVehicle( pSoldier.bVehicleID ) == 0 )
                //{
                //	return( false );
                //}

                // If not unconscious, select
                if (!uiMercFlags.HasFlag(FIND_SOLDIER_RESPONSES.UNCONSCIOUS_MERC))
                {
                    if (fMovementMode)
                    {
                        // ERASE PATH
                        PathAI.ErasePath(true);

                        // Show cursor with highlight on selected merc
                        Globals.guiNewUICursor = UICursorDefines.NO_UICURSOR;

                        // IF selected, do selection one
                        if (uiMercFlags.HasFlag(FIND_SOLDIER_RESPONSES.SELECTED_MERC))
                        {
                            // Add highlight to guy in interface.c
                            Globals.gfUIHandleSelection = Globals.SELECTED_GUY_SELECTION;

                            if (Globals.gpItemPointer == null)
                            {
                                // Don't do this unless we want to

                                // Check if buddy is stationary!
                                if (Globals.gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.STATIONARY)
                                    || pSoldier.fNoAPToFinishMove)
                                {
                                    Globals.guiShowUPDownArrows = ARROWS.SHOW_DOWN_BESIDE | ARROWS.SHOW_UP_BESIDE;
                                }
                            }

                        }
                        else
                        {
                            //if ( ( uiMercFlags & ONDUTY_MERC ) && !( uiMercFlags & NOINTERRUPT_MERC ) )
                            if (!uiMercFlags.HasFlag(FIND_SOLDIER_RESPONSES.NOINTERRUPT_MERC))
                            {
                                // Add highlight to guy in interface.c
                                Globals.gfUIHandleSelection = Globals.NONSELECTED_GUY_SELECTION;
                            }
                            else
                            {
                                Globals.gfUIHandleSelection = Globals.ENEMY_GUY_SELECTION;
                                Globals.gfUIHandleSelectionAboveGuy = true;
                            }
                        }
                    }
                }

                // If not dead, show above guy!
                if (!uiMercFlags.HasFlag(FIND_SOLDIER_RESPONSES.DEAD_MERC))
                {
                    if (fMovementMode)
                    {
                        // ERASE PATH
                        PathAI.ErasePath(true);

                        // Show cursor with highlight on selected merc
                        Globals.guiNewUICursor = UICursorDefines.NO_UICURSOR;

                        Globals.gsSelectedGridNo = pSoldier.sGridNo;
                        Globals.gsSelectedLevel = pSoldier.bLevel;
                    }

                    Globals.gsSelectedGuy = usSoldierIndex;
                    Globals.gfUIHandleSelectionAboveGuy = true;
                }

            }
            else if ((uiMercFlags.HasFlag(FIND_SOLDIER_RESPONSES.ENEMY_MERC)
                || uiMercFlags.HasFlag(FIND_SOLDIER_RESPONSES.NEUTRAL_MERC))
                && uiMercFlags.HasFlag(FIND_SOLDIER_RESPONSES.VISIBLE_MERC))
            {
                // ATE: If we are a vehicle, let the mouse cursor be a wheel...
                if (OK_ENTERABLE_VEHICLE(pSoldier))
                {
                    return false;
                }
                else
                {
                    if (fMovementMode)
                    {

                        // Check if this guy is on the enemy team....
                        if (pSoldier.bNeutral == 0 && (pSoldier.bSide != Globals.gbPlayerNum))
                        {
                            Globals.gUIActionModeChangeDueToMouseOver = true;

                            Globals.guiPendingOverrideEvent = UI_EVENT_DEFINES.M_CHANGE_TO_ACTION;
                            // Return false
                            return false;
                        }
                        else
                        {
                            // ERASE PATH
                            PathAI.ErasePath(true);

                            // Show cursor with highlight on selected merc
                            Globals.guiNewUICursor = UICursorDefines.NO_UICURSOR;
                            // Show cursor with highlight
                            Globals.gfUIHandleSelection = Globals.ENEMY_GUY_SELECTION;
                            Globals.gsSelectedGridNo = pSoldier.sGridNo;
                            Globals.gsSelectedLevel = pSoldier.bLevel;
                        }
                    }

                    Globals.gfUIHandleSelectionAboveGuy = true;
                    Globals.gsSelectedGuy = usSoldierIndex;
                }
            }
            else
            {
                if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
                {
                    return false;
                }
            }
        }
        else
        {
            Globals.gfIgnoreOnSelectedGuy = false;

            return false;
        }

        return true;
    }

    ScreenName UIHandleILoadLevel(UI_EVENT pUIEvent)
    {
        return ScreenName.InitScreen;
    }

    ScreenName UIHandleISoldierDebug(UI_EVENT pUIEvent)
    {
        // Use soldier display pages
        //SetDebugRenderHook((RENDER_HOOK)DebugSoldierPage1, 0);
        //SetDebugRenderHook((RENDER_HOOK)DebugSoldierPage2, 1);
        //SetDebugRenderHook((RENDER_HOOK)DebugSoldierPage3, 2);
        //SetDebugRenderHook((RENDER_HOOK)DebugSoldierPage4, 3);
        //gCurDebugPage = 1;

        return ScreenName.DEBUG_SCREEN;
    }

    ScreenName UIHandleILOSDebug(UI_EVENT pUIEvent)
    {
//        SetDebugRenderHook((RENDER_HOOK)DebugStructurePage1, 0);
        return ScreenName.DEBUG_SCREEN;
    }

    ScreenName UIHandleILevelNodeDebug(UI_EVENT pUIEvent)
    {
//        SetDebugRenderHook((RENDER_HOOK)DebugLevelNodePage, 0);
        return ScreenName.DEBUG_SCREEN;
    }

    ScreenName UIHandleIETOnTerrain(UI_EVENT pUIEvent)
    {
        //guiNewUICursor = CANNOT_MOVE_UICURSOR;
        Globals.guiNewUICursor = UICursorDefines.NO_UICURSOR;

//        SetCurrentCursorFromDatabase(CURSOR.VIDEO_NO_CURSOR);

        return ScreenName.GAME_SCREEN;
    }


    void UIHandleSoldierStanceChange(int ubSoldierID, AnimationHeights bNewStance)
    {
        SOLDIERTYPE? pSoldier;

        pSoldier = Globals.MercPtrs[ubSoldierID];

        // Is this a valid stance for our position?
        if (!SoldierControl.IsValidStance(pSoldier, bNewStance))
        {
            if (pSoldier.bCollapsed && pSoldier.bBreath < Globals.OKBREATH)
            {
                Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, gzLateLocalizedString[4], pSoldier.name);
            }
            else
            {
                if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
                {
                    Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, Globals.TacticalStr[(int)STR.VEHICLES_NO_STANCE_CHANGE_STR]);
                }
                else if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.ROBOT))
                {
                    Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, Globals.TacticalStr[(int)STR.ROBOT_NO_STANCE_CHANGE_STR]);
                }
                else
                {
                    if (pSoldier.bCollapsed)
                    {
                        Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, pMessageStrings[MSG.CANT_CHANGE_STANCE], pSoldier.name);
                    }
                    else
                    {
                        Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, Globals.TacticalStr[(int)STR.CANNOT_STANCE_CHANGE_STR], pSoldier.name);
                    }
                }
            }

            return;
        }

        // IF turn-based - adjust stance now!
        if (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)
            && Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
        {
            pSoldier.fTurningFromPronePosition = 0;

            // Check if we have enough APS
            if (this.SoldierCanAffordNewStance(pSoldier, bNewStance))
            {
                // Adjust stance
                //ChangeSoldierStance( pSoldier, bNewStance );
                SoldierControl.SendChangeSoldierStanceEvent(pSoldier, bNewStance);

                pSoldier.sFinalDestination = pSoldier.sGridNo;
                pSoldier.bGoodContPath = 0;

            }
            else
            {
                return;
            }
        }

        // If realtime- change walking animation!
        if (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.REALTIME) || !Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
        {

            // If we are stationary, do something else!
            if (Globals.gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.STATIONARY))
            {
                // Change stance normally
                SoldierControl.SendChangeSoldierStanceEvent(pSoldier, bNewStance);
            }
            else
            {
                // Pick moving animation based on stance

                // LOCK VARIBLE FOR NO UPDATE INDEX...
//                pSoldier.usUIMovementMode = GetMoveStateBasedOnStance(pSoldier, bNewStance);

                if (pSoldier.usUIMovementMode == AnimationStates.CRAWLING && Globals.gAnimControl[pSoldier.usAnimState].ubEndHeight != AnimationHeights.ANIM_PRONE)
                {
                    pSoldier.usDontUpdateNewGridNoOnMoveAnimChange = Globals.LOCKED_NO_NEWGRIDNO;
                    pSoldier.bPathStored = false;
                }
                else
                {
                    pSoldier.usDontUpdateNewGridNoOnMoveAnimChange = 1;
                }

                SoldierControl.ChangeSoldierState(pSoldier, pSoldier.usUIMovementMode, 0, false);
            }
        }

        // Set UI value for soldier
        this.SetUIbasedOnStance(pSoldier, bNewStance);

        Globals.gfUIStanceDifferent = true;

        // ATE: If we are being serviced...stop...
        // InternalReceivingSoldierCancelServices( pSoldier, false );
        SoldierControl.InternalGivingSoldierCancelServices(pSoldier, false);
        //gfPlotNewMovement   = true;

    }

    ScreenName UIHandleIETEndTurn(UI_EVENT pUIEvent)
    {
        return ScreenName.GAME_SCREEN;
    }


    ScreenName UIHandleIGotoDemoMode(UI_EVENT pUIEvent)
    {
        return ScreenName.DEMO_EXIT_SCREEN;//(EnterTacticalDemoMode());
    }


    ScreenName UIHandleILoadFirstLevel(UI_EVENT pUIEvent)
    {
        Globals.gubCurrentScene = 0;
        return ScreenName.InitScreen;
    }

    ScreenName UIHandleILoadSecondLevel(UI_EVENT pUIEvent)
    {
        Globals.gubCurrentScene = 1;
        return ScreenName.InitScreen;
    }

    ScreenName UIHandleILoadThirdLevel(UI_EVENT pUIEvent)
    {
        Globals.gubCurrentScene = 2;
        return ScreenName.InitScreen;
    }

    ScreenName UIHandleILoadFourthLevel(UI_EVENT pUIEvent)
    {
        Globals.gubCurrentScene = 3;
        return ScreenName.InitScreen;
    }

    ScreenName UIHandleILoadFifthLevel(UI_EVENT pUIEvent)
    {
        Globals.gubCurrentScene = 4;
        return ScreenName.InitScreen;
    }

    private static bool fStationary = false;
    private static int usOldMouseXPos = 32000;
    private static int usOldMouseYPos = 32000;
    private static int usOldMapPos = 32000;
    private static MOUSE uiSameFrameCursorFlags;
    private static int uiOldFrameNumber = 99999;

    public static void GetCursorMovementFlags(out MOUSE puiCursorFlags)
    {

        // Check if this is the same frame as before, return already calculated value if so!
        if (uiOldFrameNumber == Globals.guiGameCycleCounter)
        {
            puiCursorFlags = uiSameFrameCursorFlags;
            return;
        }

        IsometricUtils.GetMouseMapPos(out int usMapPos);
        IsometricUtils.ConvertGridNoToXY(usMapPos, out int sXPos, out int sYPos);

        puiCursorFlags = 0;

        if (Globals.gusMouseXPos != usOldMouseXPos || Globals.gusMouseYPos != usOldMouseYPos)
        {
            puiCursorFlags |= MOUSE.MOVING;

            // IF CURSOR WAS PREVIOUSLY STATIONARY, MAKE THE ADDITIONAL CHECK OF GRID POS CHANGE
            if (fStationary && usOldMapPos == usMapPos)
            {
                puiCursorFlags |= MOUSE.MOVING_IN_TILE;
            }
            else
            {
                fStationary = false;
                puiCursorFlags |= MOUSE.MOVING_NEW_TILE;
            }
        }
        else
        {
            puiCursorFlags |= MOUSE.STATIONARY;
            fStationary = true;
        }

        usOldMapPos = usMapPos;
        usOldMouseXPos = Globals.gusMouseXPos;
        usOldMouseYPos = Globals.gusMouseYPos;

        uiOldFrameNumber = Globals.guiGameCycleCounter;
        uiSameFrameCursorFlags = puiCursorFlags;
    }

    static int usTargetID = Globals.NOBODY;
    static bool fTargetFound = false;

    public static bool HandleUIMovementCursor(SOLDIERTYPE? pSoldier, MOUSE uiCursorFlags, int usMapPos, MOVEUI_TARGET uiFlags)
    {
        bool fSetCursor = false;
        bool fCalculated = false;
        bool fTargetFoundAndLookingForOne = false;
        bool fIntTileFoundAndLookingForOne = false;

        // Determine if we can afford!
//        if (!EnoughPoints(pSoldier, Globals.gsCurrentActionPoints, 0, false))
        {
            Globals.gfUIDisplayActionPointsInvalid = true;
        }

        // Check if we're stationary
        if (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.REALTIME)
            || !Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)
            || Globals.gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.STATIONARY)
            || pSoldier.fNoAPToFinishMove
            || pSoldier.ubID >= Globals.MAX_NUM_SOLDIERS)
        {
            // If we are targeting a merc for some reason, don't go thorugh normal channels if we are on someone now
            if (uiFlags == MOVEUI_TARGET.MERCS || uiFlags == MOVEUI_TARGET.MERCSFORAID)
            {
                if (Globals.gfUIFullTargetFound != fTargetFound
                    || usTargetID != Globals.gusUIFullTargetID
                    || Globals.gfResetUIMovementOptimization)
                {
                    Globals.gfResetUIMovementOptimization = false;

                    // ERASE PATH
                    PathAI.ErasePath(true);

                    // Try and get a path right away
                    DrawUIMovementPath(pSoldier, usMapPos, uiFlags);
                }

                // Save for next time...
                fTargetFound = Globals.gfUIFullTargetFound;
                usTargetID = Globals.gusUIFullTargetID;

                if (fTargetFound)
                {
                    fTargetFoundAndLookingForOne = true;
                }
            }

            if (uiFlags == MOVEUI_TARGET.ITEMS)
            {
                Globals.gfUIOverItemPool = true;
                Globals.gfUIOverItemPoolGridNo = usMapPos;
            }
            else if (uiFlags == MOVEUI_TARGET.MERCSFORAID)
            {
                // Set values for AP display...
                Globals.gfUIDisplayActionPointsCenter = true;
            }

            // IF CURSOR IS MOVING
            if (uiCursorFlags.HasFlag(MOUSE.MOVING) || Globals.gfUINewStateForIntTile)
            {
                // SHOW CURSOR
                fSetCursor = true;

                // IF CURSOR WAS PREVIOUSLY STATIONARY, MAKE THE ADDITIONAL CHECK OF GRID POS CHANGE
                if ((uiCursorFlags.HasFlag(MOUSE.MOVING_NEW_TILE) && !fTargetFoundAndLookingForOne)
                    || Globals.gfUINewStateForIntTile)
                {
                    // ERASE PATH
                    PathAI.ErasePath(true);

                    // Reset counter
//                    RESETCOUNTER(PATHFINDCOUNTER);

                    Globals.gfPlotNewMovement = true;
                }

                if (uiCursorFlags.HasFlag(MOUSE.MOVING_IN_TILE))
                {
                    Globals.gfUIDisplayActionPoints = true;
                }
            }

            if (uiCursorFlags.HasFlag(MOUSE.STATIONARY))
            {
                // CURSOR IS STATIONARY
//                if (_KeyDown(SHIFT) && !Globals.gfPlotNewMovementNOCOST)
                {
                    Globals.gfPlotNewMovementNOCOST = true;
                    Globals.gfPlotNewMovement = true;
                }
//                if (!(_KeyDown(SHIFT)) && Globals.gfPlotNewMovementNOCOST)
                {
                    Globals.gfPlotNewMovementNOCOST = false;
                    Globals.gfPlotNewMovement = true;
                }


                // ONLY DIPSLAY PATH AFTER A DELAY
//                if (COUNTERDONE(PATHFINDCOUNTER))
                {
                    // Reset counter
//                    RESETCOUNTER(PATHFINDCOUNTER);

                    if (Globals.gfPlotNewMovement)
                    {
                        DrawUIMovementPath(pSoldier, usMapPos, uiFlags);

                        Globals.gfPlotNewMovement = false;
                    }
                }

                fSetCursor = true;

                // DISPLAY POINTS EVEN WITHOUT DELAY
                // ONLY IF GFPLOT NEW MOVEMENT IS false!
                if (!Globals.gfPlotNewMovement)
                {
                    if (Globals.gsCurrentActionPoints < 0
                        || Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.REALTIME)
                        || !Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
                    {
                        Globals.gfUIDisplayActionPoints = false;
                    }
                    else
                    {
                        Globals.gfUIDisplayActionPoints = true;

                        if (uiFlags == MOVEUI_TARGET.INTTILES)
                        {
                            // Set values for AP display...
                            Globals.gUIDisplayActionPointsOffX = 22;
                            Globals.gUIDisplayActionPointsOffY = 15;
                        }
                        if (uiFlags == MOVEUI_TARGET.BOMB)
                        {
                            // Set values for AP display...
                            Globals.gUIDisplayActionPointsOffX = 22;
                            Globals.gUIDisplayActionPointsOffY = 15;
                        }
                        else if (uiFlags == MOVEUI_TARGET.ITEMS)
                        {
                            // Set values for AP display...
                            Globals.gUIDisplayActionPointsOffX = 22;
                            Globals.gUIDisplayActionPointsOffY = 15;
                        }
                        else
                        {
                            switch (pSoldier.usUIMovementMode)
                            {
                                case AnimationStates.WALKING:

                                    Globals.gUIDisplayActionPointsOffY = 10;
                                    Globals.gUIDisplayActionPointsOffX = 10;
                                    break;

                                case AnimationStates.RUNNING:
                                    Globals.gUIDisplayActionPointsOffY = 15;
                                    Globals.gUIDisplayActionPointsOffX = 21;
                                    break;
                            }
                        }
                    }
                }
            }

        }
        else
        {
            // THE MERC IS MOVING
            // We're moving, erase path, change cursor
            PathAI.ErasePath(true);

            fSetCursor = true;

        }

        return fSetCursor;
    }

    public static byte DrawUIMovementPath(SOLDIERTYPE? pSoldier, int usMapPos, MOVEUI_TARGET uiFlags)
    {
        int sAPCost;
        BP sBPCost;
        int sActionGridNo = 0;
        STRUCTURE? pStructure;
        bool fOnInterTile = false;
        WorldDirections ubDirection = 0;
        //	ITEM_POOL					*pItemPool;
        int sAdjustedGridNo;
        int sIntTileGridNo;
        LEVELNODE? pIntTile = null;
        byte bReturnCode = 0;
        bool fPlot;
        byte ubMercID;

        if (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)
            && Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)
//            || _KeyDown(SHIFT)
            )
        {
            fPlot = Globals.PLOT;
        }
        else
        {
            fPlot = Globals.NO_PLOT;
        }

        sActionGridNo = usMapPos;
        sAPCost = 0;

        PathAI.ErasePath(false);

        // IF WE ARE OVER AN INTERACTIVE TILE, GIVE GRIDNO OF POSITION
        if (uiFlags == MOVEUI_TARGET.INTTILES)
        {
            // Get structure info for in tile!
//            pIntTile = GetCurInteractiveTileGridNoAndStructure(out sIntTileGridNo, out pStructure);

            // We should not have null here if we are given this flag...
//            if (pIntTile != null)
            {
//                sActionGridNo = Overhead.FindAdjacentGridEx(pSoldier, sIntTileGridNo, ref ubDirection, null, false, true);
                if (sActionGridNo == -1)
                {
//                    sActionGridNo = sIntTileGridNo;
                }
//                CalcInteractiveObjectAPs(sIntTileGridNo, pStructure, out sAPCost, out sBPCost);
                //sAPCost += UIPlotPath( pSoldier, sActionGridNo, NO_COPYROUTE, PLOT, TEMPORARY, (uint)pSoldier.usUIMovementMode, NOT_STEALTH, FORWARD, pSoldier.bActionPoints);
                sAPCost += PathAI.UIPlotPath(pSoldier, sActionGridNo, PlotPathDefines.NO_COPYROUTE, fPlot, PlotPathDefines.TEMPORARY, pSoldier.usUIMovementMode, PlotPathDefines.NOT_STEALTH, PlotPathDefines.FORWARD, pSoldier.bActionPoints);

                if (sActionGridNo != pSoldier.sGridNo)
                {
                    Globals.gfUIHandleShowMoveGrid = 1;
                    Globals.gsUIHandleShowMoveGridLocation = sActionGridNo;
                }

                // Add cost for stance change....
//                sAPCost += GetAPsToChangeStance(pSoldier, AnimationHeights.ANIM_STAND);
            }
//            else
            {
                sAPCost += PathAI.UIPlotPath(pSoldier, sActionGridNo, PlotPathDefines.NO_COPYROUTE, fPlot, PlotPathDefines.TEMPORARY, pSoldier.usUIMovementMode, PlotPathDefines.NOT_STEALTH, PlotPathDefines.FORWARD, pSoldier.bActionPoints);
            }
        }
        else if (uiFlags == MOVEUI_TARGET.WIREFENCE)
        {
//            sActionGridNo = Overhead.FindAdjacentGridEx(pSoldier, usMapPos, ref ubDirection, null, false, true);
            if (sActionGridNo == -1)
            {
                sAPCost = 0;
            }
            else
            {
//                sAPCost = GetAPsToCutFence(pSoldier);

                sAPCost += PathAI.UIPlotPath(pSoldier, sActionGridNo, PlotPathDefines.NO_COPYROUTE, fPlot, PlotPathDefines.TEMPORARY, pSoldier.usUIMovementMode, PlotPathDefines.NOT_STEALTH, PlotPathDefines.FORWARD, pSoldier.bActionPoints);

                if (sActionGridNo != pSoldier.sGridNo)
                {
                    Globals.gfUIHandleShowMoveGrid = 1;
                    Globals.gsUIHandleShowMoveGridLocation = sActionGridNo;
                }
            }
        }
        else if (uiFlags == MOVEUI_TARGET.JAR)
        {
//            sActionGridNo = Overhead.FindAdjacentGridEx(pSoldier, usMapPos, ref ubDirection, null, false, true);
            if (sActionGridNo == -1)
            {
                sActionGridNo = usMapPos;
            }

//            sAPCost = GetAPsToUseJar(pSoldier, sActionGridNo);

            sAPCost += PathAI.UIPlotPath(
                pSoldier,
                sActionGridNo,
                PlotPathDefines.NO_COPYROUTE,
                fPlot,
                PlotPathDefines.TEMPORARY,
                pSoldier.usUIMovementMode,
                PlotPathDefines.NOT_STEALTH,
                PlotPathDefines.FORWARD,
                pSoldier.bActionPoints);

            if (sActionGridNo != pSoldier.sGridNo)
            {
                Globals.gfUIHandleShowMoveGrid = 1;
                Globals.gsUIHandleShowMoveGridLocation = sActionGridNo;
            }
        }
        else if (uiFlags == MOVEUI_TARGET.CAN)
        {
            // Get structure info for in tile!
//            pIntTile = GetCurInteractiveTileGridNoAndStructure(out sIntTileGridNo, out pStructure);

            // We should not have null here if we are given this flag...
            if (pIntTile != null)
            {
//                sActionGridNo = Overhead.FindAdjacentGridEx(pSoldier, sIntTileGridNo, ref ubDirection, null, false, true);
                if (sActionGridNo != -1)
                {
                    sAPCost = AP.ATTACH_CAN;
                    sAPCost += PathAI.UIPlotPath(pSoldier, sActionGridNo, PlotPathDefines.NO_COPYROUTE, fPlot, PlotPathDefines.TEMPORARY, pSoldier.usUIMovementMode, PlotPathDefines.NOT_STEALTH, PlotPathDefines.FORWARD, pSoldier.bActionPoints);

                    if (sActionGridNo != pSoldier.sGridNo)
                    {
                        Globals.gfUIHandleShowMoveGrid = 1;
                        Globals.gsUIHandleShowMoveGridLocation = sActionGridNo;
                    }
                }
            }
            else
            {
                sAPCost += PathAI.UIPlotPath(pSoldier, sActionGridNo, PlotPathDefines.NO_COPYROUTE, fPlot, PlotPathDefines.TEMPORARY, pSoldier.usUIMovementMode, PlotPathDefines.NOT_STEALTH, PlotPathDefines.FORWARD, pSoldier.bActionPoints);
            }

        }
        else if (uiFlags == MOVEUI_TARGET.REPAIR)
        {
            // For repair, check if we are over a vehicle, then get gridnot to edge of that vehicle!
//            if (IsRepairableStructAtGridNo(usMapPos, out ubMercID) == 2)
            {
                int sNewGridNo = 0;

//                sNewGridNo = FindGridNoFromSweetSpotWithStructDataFromSoldier(pSoldier, pSoldier.usUIMovementMode, 5, out ubDirection, 0, Globals.MercPtrs[ubMercID]);

//                if (sNewGridNo != Globals.NOWHERE)
                {
                    usMapPos = sNewGridNo;
                }
            }

//            sActionGridNo = Overhead.FindAdjacentGridEx(pSoldier, usMapPos, ref ubDirection, null, false, true);
            if (sActionGridNo == -1)
            {
                sActionGridNo = usMapPos;
            }

//            sAPCost = GetAPsToBeginRepair(pSoldier);

            sAPCost += PathAI.UIPlotPath(pSoldier, sActionGridNo, PlotPathDefines.NO_COPYROUTE, fPlot, PlotPathDefines.TEMPORARY, pSoldier.usUIMovementMode, PlotPathDefines.NOT_STEALTH, PlotPathDefines.FORWARD, pSoldier.bActionPoints);

            if (sActionGridNo != pSoldier.sGridNo)
            {
                Globals.gfUIHandleShowMoveGrid = 1;
                Globals.gsUIHandleShowMoveGridLocation = sActionGridNo;
            }
        }
        else if (uiFlags == MOVEUI_TARGET.REFUEL)
        {
            // For repair, check if we are over a vehicle, then get gridnot to edge of that vehicle!
//            if (IsRefuelableStructAtGridNo(usMapPos, out ubMercID) == 2)
            {
                int sNewGridNo;

//                sNewGridNo = FindGridNoFromSweetSpotWithStructDataFromSoldier(pSoldier, pSoldier.usUIMovementMode, 5, out ubDirection, 0, Globals.MercPtrs[ubMercID]);

//                if (sNewGridNo != Globals.NOWHERE)
                {
//                    usMapPos = sNewGridNo;
                }
            }

//            sActionGridNo = Overhead.FindAdjacentGridEx(pSoldier, usMapPos, ref ubDirection, null, false, true);
            if (sActionGridNo == -1)
            {
                sActionGridNo = usMapPos;
            }

//            sAPCost = GetAPsToRefuelVehicle(pSoldier);

            sAPCost += PathAI.UIPlotPath(pSoldier, sActionGridNo, PlotPathDefines.NO_COPYROUTE, fPlot, PlotPathDefines.TEMPORARY, pSoldier.usUIMovementMode, PlotPathDefines.NOT_STEALTH, PlotPathDefines.FORWARD, pSoldier.bActionPoints);

            if (sActionGridNo != pSoldier.sGridNo)
            {
                Globals.gfUIHandleShowMoveGrid = 1;
                Globals.gsUIHandleShowMoveGridLocation = sActionGridNo;
            }
        }
        else if (uiFlags == MOVEUI_TARGET.MERCS)
        {
            int sGotLocation = Globals.NOWHERE;
            bool fGotAdjacent = false;

            // Check if we are on a target
            if (Globals.gfUIFullTargetFound)
            {
                int cnt;
                int sSpot;
                int ubGuyThere;

                for (cnt = 0; cnt < (int)WorldDirections.NUM_WORLD_DIRECTIONS; cnt++)
                {
                    sSpot = IsometricUtils.NewGridNo(pSoldier.sGridNo, IsometricUtils.DirectionInc(cnt));

                    // Make sure movement costs are OK....
                    if (Globals.gubWorldMovementCosts[sSpot, cnt, Globals.gsInterfaceLevel] >= TRAVELCOST.BLOCKED)
                    {
                        continue;
                    }


                    // Check for who is there...
                    ubGuyThere = WorldManager.WhoIsThere2(sSpot, pSoldier.bLevel);

                    if (ubGuyThere == Globals.MercPtrs[Globals.gusUIFullTargetID].ubID)
                    {
                        // We've got a guy here....
                        // Who is the one we want......
                        sGotLocation = sSpot;
                        sAdjustedGridNo = Globals.MercPtrs[Globals.gusUIFullTargetID].sGridNo;
                        ubDirection = (WorldDirections)cnt;
                        break;
                    }
                }

                if (sGotLocation == (int)Globals.NOWHERE)
                {
                    sActionGridNo = Overhead.FindAdjacentGridEx(pSoldier, Globals.MercPtrs[Globals.gusUIFullTargetID].sGridNo, ref ubDirection, out sAdjustedGridNo, true, false);

                    if (sActionGridNo == -1)
                    {
                        sGotLocation = (int)Globals.NOWHERE;
                    }
                    else
                    {
                        sGotLocation = sActionGridNo;
                    }
                    fGotAdjacent = true;
                }
            }
            else
            {
                sAdjustedGridNo = usMapPos;
                sGotLocation = sActionGridNo;
                fGotAdjacent = true;
            }

            if (sGotLocation != (int)Globals.NOWHERE)
            {
//                sAPCost += MinAPsToAttack(pSoldier, out sAdjustedGridNo, true);
                sAPCost += PathAI.UIPlotPath(
                    pSoldier,
                    sGotLocation,
                    PlotPathDefines.NO_COPYROUTE,
                    fPlot,
                    PlotPathDefines.TEMPORARY,
                    pSoldier.usUIMovementMode,
                    PlotPathDefines.NOT_STEALTH,
                    PlotPathDefines.FORWARD,
                    pSoldier.bActionPoints);

                if (sGotLocation != pSoldier.sGridNo && fGotAdjacent)
                {
                    Globals.gfUIHandleShowMoveGrid = 1;
                    Globals.gsUIHandleShowMoveGridLocation = sGotLocation;
                }
            }
        }
        else if (uiFlags == MOVEUI_TARGET.STEAL)
        {
            // Check if we are on a target
            if (Globals.gfUIFullTargetFound)
            {
                sActionGridNo = Overhead.FindAdjacentGridEx(
                    pSoldier, 
                    MercPtrs[gusUIFullTargetID].sGridNo, 
                    ref ubDirection, 
                    out sAdjustedGridNo, 
                    true, 
                    false);
                if (sActionGridNo == -1)
                {
                    sActionGridNo = sAdjustedGridNo;
                }
                sAPCost += AP.STEAL_ITEM;
                // CJC August 13 2002: take into account stance in AP prediction
                if (!PTR_STANDING(pSoldier))
                {
//                    sAPCost += GetAPsToChangeStance(pSoldier, AnimationHeights.ANIM_STAND);
                }
                sAPCost += PathAI.UIPlotPath(pSoldier, sActionGridNo, PlotPathDefines.NO_COPYROUTE, fPlot, PlotPathDefines.TEMPORARY, pSoldier.usUIMovementMode, PlotPathDefines.NOT_STEALTH, PlotPathDefines.FORWARD, pSoldier.bActionPoints);

                if (sActionGridNo != pSoldier.sGridNo)
                {
                    Globals.gfUIHandleShowMoveGrid = 1;
                    Globals.gsUIHandleShowMoveGridLocation = sActionGridNo;
                }
            }
        }
        else if (uiFlags == MOVEUI_TARGET.BOMB)
        {
//            sAPCost += GetAPsToDropBomb(pSoldier);
            sAPCost += PathAI.UIPlotPath(pSoldier, usMapPos, PlotPathDefines.NO_COPYROUTE, fPlot, PlotPathDefines.TEMPORARY, pSoldier.usUIMovementMode, PlotPathDefines.NOT_STEALTH, PlotPathDefines.FORWARD, pSoldier.bActionPoints);

            Globals.gfUIHandleShowMoveGrid = 1;
            Globals.gsUIHandleShowMoveGridLocation = usMapPos;
        }
        else if (uiFlags == MOVEUI_TARGET.MERCSFORAID)
        {
            if (Globals.gfUIFullTargetFound)
            {
                sActionGridNo = Overhead.FindAdjacentGridEx(pSoldier, Globals.MercPtrs[Globals.gusUIFullTargetID].sGridNo, ref ubDirection, out sAdjustedGridNo, true, false);

                // Try again at another gridno...
                if (sActionGridNo == -1)
                {
                    sActionGridNo = Overhead.FindAdjacentGridEx(pSoldier, usMapPos, ref ubDirection, out sAdjustedGridNo, true, false);

                    if (sActionGridNo == -1)
                    {
                        sActionGridNo = sAdjustedGridNo;
                    }
                }

//                sAPCost += GetAPsToBeginFirstAid(pSoldier);
                sAPCost += PathAI.UIPlotPath(pSoldier, sActionGridNo, PlotPathDefines.NO_COPYROUTE, fPlot, PlotPathDefines.TEMPORARY, pSoldier.usUIMovementMode, PlotPathDefines.NOT_STEALTH, PlotPathDefines.FORWARD, pSoldier.bActionPoints);
                if (sActionGridNo != pSoldier.sGridNo)
                {
                    Globals.gfUIHandleShowMoveGrid = 1;
                    Globals.gsUIHandleShowMoveGridLocation = sActionGridNo;
                }
            }
        }
        else if (uiFlags == MOVEUI_TARGET.ITEMS)
        {
            //if ( GetItemPool( usMapPos, &pItemPool, pSoldier.bLevel ) )
            {
                //if ( ITEMPOOL_VISIBLE( pItemPool ) )
                {
//                    sActionGridNo = AdjustGridNoForItemPlacement(pSoldier, sActionGridNo);

                    if (pSoldier.sGridNo != sActionGridNo)
                    {
                        sAPCost += PathAI.UIPlotPath(pSoldier, sActionGridNo, PlotPathDefines.NO_COPYROUTE, fPlot, PlotPathDefines.TEMPORARY, pSoldier.usUIMovementMode, PlotPathDefines.NOT_STEALTH, PlotPathDefines.FORWARD, pSoldier.bActionPoints);
                        if (sAPCost != 0)
                        {
                            sAPCost += (int)AP.PICKUP_ITEM;
                        }
                    }
                    else
                    {
                        sAPCost += (int)AP.PICKUP_ITEM;
                    }

                    if (sActionGridNo != pSoldier.sGridNo)
                    {
                        Globals.gfUIHandleShowMoveGrid = 1;
                        Globals.gsUIHandleShowMoveGridLocation = sActionGridNo;
                    }

                }
            }
        }
        else
        {
            sAPCost += PathAI.UIPlotPath(pSoldier, sActionGridNo, PlotPathDefines.NO_COPYROUTE, fPlot, PlotPathDefines.TEMPORARY, pSoldier.usUIMovementMode, PlotPathDefines.NOT_STEALTH, PlotPathDefines.FORWARD, pSoldier.bActionPoints);
        }

        if (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.SHOW_AP_LEFT))
        {
            Globals.gsCurrentActionPoints = pSoldier.bActionPoints - sAPCost;
        }
        else
        {
            Globals.gsCurrentActionPoints = sAPCost;
        }

        return bReturnCode;
    }


    bool UIMouseOnValidAttackLocation(SOLDIERTYPE pSoldier)
    {
        Items usInHand;
        bool fGuyHere = false;
        SOLDIERTYPE pTSoldier;
        CURS ubItemCursor = 0;

        if (!IsometricUtils.GetMouseMapPos(out int usMapPos))
        {
            return false;
        }

        // LOOK IN GUY'S HAND TO CHECK LOCATION
        usInHand = pSoldier.inv[InventorySlot.HANDPOS].usItem;

        // Get cursor value
//        ubItemCursor = GetActionModeCursor(pSoldier);

//        if (ubItemCursor == INVALIDCURS)
//        {
//            return (false);
//        }


//        if (ubItemCursor == WIRECUTCURS)
        {
//            if (IsCuttableWireFenceAtGridNo(usMapPos) && pSoldier.bLevel == 0)
//            {
//                return (true);
//            }
//            else
//            {
//                return (false);
//            }
        }

//        if (ubItemCursor == REPAIRCURS)
//        {
//            if (Globals.gfUIFullTargetFound)
//            {
//                usMapPos = Globals.MercPtrs[Globals.gusUIFullTargetID].sGridNo;
//            }
//
//            if (IsRepairableStructAtGridNo(usMapPos, null) && pSoldier.bLevel == 0)
//            {
//                return (true);
//            }
//            else
//            {
//                return (false);
//            }
//        }

        if (ubItemCursor == CURS.REFUELCURS)
        {
            if (Globals.gfUIFullTargetFound)
            {
                usMapPos = Globals.MercPtrs[Globals.gusUIFullTargetID].sGridNo;
            }

//            if (IsRefuelableStructAtGridNo(usMapPos, null) && pSoldier.bLevel == 0)
//            {
//                return (true);
//            }
//            else
//            {
//                return (false);
//            }
        }

        if (ubItemCursor == CURS.BOMBCURS)
        {
            if (usMapPos == pSoldier.sGridNo)
            {
                return true;
            }

            if (!Overhead.NewOKDestination(pSoldier, usMapPos, true, pSoldier.bLevel))
            {
                return false;
            }
        }

        // SEE IF THERE IS SOMEBODY HERE
        if (Globals.gfUIFullTargetFound && ubItemCursor != CURS.KNIFECURS)
        {
            fGuyHere = true;

            if (Globals.guiUIFullTargetFlags.HasFlag(FIND_SOLDIER_RESPONSES.SELECTED_MERC) && Globals.Item[usInHand].usItemClass != IC.MEDKIT)
            {
                return false;
            }
        }

//        if (!CanPlayerUseRocketRifle(pSoldier, true))
//        {
//            return (false);
//        }

        //if ( Item[ usInHand ].usItemClass == IC_BLADE && usInHand != THROWING_KNIFE )	
        //{
        //	if ( !fGuyHere )
        //	{
        //	return( false );
        //	}
        //}

        if (Globals.Item[usInHand].usItemClass == IC.PUNCH)
        {
            if (!fGuyHere)
            {
                return false;
            }
        }

        //if ( Item[ usInHand ].usItemClass == IC_BLADE )	
        //{
        //	if ( !fGuyHere )
        //	{
        //		return( false );
        //	}
        //}

        if (Globals.Item[usInHand].usItemClass == IC.MEDKIT)
        {
            if (!fGuyHere)
            {
                return false;
            }

            // IF a guy's here, chack if they need medical help!
            pTSoldier = Globals.MercPtrs[Globals.gusUIFullTargetID];

            // If we are a vehicle...
            if (pTSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE | SOLDIER.ROBOT))
            {
                Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, Globals.TacticalStr[(int)STR.CANNOT_DO_FIRST_AID_STR], pTSoldier.name);
                return false;
            }

            if (pSoldier.bMedical == 0)
            {
                Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, pMessageStrings[MSG.MERC_HAS_NO_MEDSKILL], pSoldier.name);
                return false;
            }

            if (pTSoldier.bBleeding == 0 && pTSoldier.bLife != pTSoldier.bLifeMax)
            {
                Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, Globals.gzLateLocalizedString[19], pTSoldier.name);
                return false;
            }

            if (pTSoldier.bBleeding == 0 && pTSoldier.bLife >= Globals.OKLIFE)
            {
                Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, Globals.TacticalStr[(int)STR.CANNOT_NO_NEED_FIRST_AID_STR], pTSoldier.name);
                return false;
            }

        }

        return true;
    }


    bool UIOkForItemPickup(SOLDIERTYPE? pSoldier, int sGridNo)
    {
        int sAPCost;

//        sAPCost = GetAPsToPickupItem(pSoldier, sGridNo);

//        if (sAPCost == 0)
//        {
//            Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, Globals.TacticalStr[(int)STR.NO_PATH]);
//        }
//        else
//        {
//            if (HandleItems.GetItemPool(sGridNo, out ITEM_POOL? pItemPool, pSoldier.bLevel))
//            {
//                //if ( !ITEMPOOL_VISIBLE( pItemPool ) )
//                {
//                    //		return( false );
//                }
//            }
//
//            if (EnoughPoints(pSoldier, sAPCost, 0, true))
//            {
//                return (true);
//            }
//        }

        return false;
    }


    bool SoldierCanAffordNewStance(SOLDIERTYPE? pSoldier, AnimationHeights ubDesiredStance)
    {
        AnimationHeights bCurrentHeight;
        int bAP = 0;
        int bBP = 0;

        bCurrentHeight = ubDesiredStance - (int)Globals.gAnimControl[pSoldier.usAnimState].ubEndHeight;

        // Now change to appropriate animation

        switch (bCurrentHeight)
        {
            case (AnimationHeights)(AnimationHeights.ANIM_STAND - AnimationHeights.ANIM_CROUCH):
            case (AnimationHeights)(AnimationHeights.ANIM_CROUCH - AnimationHeights.ANIM_STAND):

                bAP = AP.CROUCH;
                bBP = BP.CROUCH;
                break;

            case (AnimationHeights)(AnimationHeights.ANIM_STAND - AnimationHeights.ANIM_PRONE):
            case (AnimationHeights)(AnimationHeights.ANIM_PRONE - AnimationHeights.ANIM_STAND):

                bAP = AP.CROUCH + AP.PRONE;
                bBP = BP.CROUCH + BP.PRONE;
                break;

            case (AnimationHeights)(AnimationHeights.ANIM_CROUCH - AnimationHeights.ANIM_PRONE):
            case (AnimationHeights)(AnimationHeights.ANIM_PRONE - AnimationHeights.ANIM_CROUCH):

                bAP = AP.PRONE;
                bBP = BP.PRONE;
                break;

        }

        return true;//(EnoughPoints(pSoldier, bAP, bBP, true));
    }

    void SetUIbasedOnStance(SOLDIERTYPE? pSoldier, AnimationHeights bNewStance)
    {
        // Set UI based on our stance!
        switch (bNewStance)
        {
            case AnimationHeights.ANIM_STAND:
                pSoldier.usUIMovementMode = AnimationStates.WALKING;
                break;

            case AnimationHeights.ANIM_CROUCH:
                pSoldier.usUIMovementMode = AnimationStates.SWATTING;
                break;

            case AnimationHeights.ANIM_PRONE:
                pSoldier.usUIMovementMode = AnimationStates.CRAWLING;
                break;
        }

        // Set UI cursor!

    }


    public static void SetMovementModeCursor(SOLDIERTYPE pSoldier)
    {
        if (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED) && Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
        {
            if (OK_ENTERABLE_VEHICLE(pSoldier))
            {
                Globals.guiNewUICursor = UICursorDefines.MOVE_VEHICLE_UICURSOR;
            }
            else
            {
                // Change mouse cursor based on type of movement we want to do
                switch (pSoldier.usUIMovementMode)
                {
                    case AnimationStates.WALKING:
                        Globals.guiNewUICursor = UICursorDefines.MOVE_WALK_UICURSOR;
                        break;

                    case AnimationStates.RUNNING:
                        Globals.guiNewUICursor = UICursorDefines.MOVE_RUN_UICURSOR;
                        break;

                    case AnimationStates.SWATTING:
                        Globals.guiNewUICursor = UICursorDefines.MOVE_SWAT_UICURSOR;
                        break;

                    case AnimationStates.CRAWLING:
                        Globals.guiNewUICursor = UICursorDefines.MOVE_PRONE_UICURSOR;
                        break;
                }
            }
        }

        if (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.REALTIME) || !Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
        {
            if (Globals.gfUIAllMoveOn > 0)
            {
                Globals.guiNewUICursor = UICursorDefines.ALL_MOVE_REALTIME_UICURSOR;
            }
            else
            {
                //if ( pSoldier.fUIMovementFast )
                //{
                //	BeginDisplayTimedCursor( MOVE_RUN_REALTIME_UICURSOR, 300 );
                //}

                Globals.guiNewUICursor = UICursorDefines.MOVE_REALTIME_UICURSOR;
            }
        }

        Globals.guiNewUICursor = InteractiveTiles.GetInteractiveTileCursor(guiNewUICursor, false);

    }



    void SetConfirmMovementModeCursor(SOLDIERTYPE pSoldier, bool fFromMove)
    {
        if (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)
            && Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
        {
            if (Globals.gfUIAllMoveOn > 0)
            {
                if (OK_ENTERABLE_VEHICLE(pSoldier))
                {
                    Globals.guiNewUICursor = UICursorDefines.ALL_MOVE_VEHICLE_UICURSOR;
                }
                else
                {
                    // Change mouse cursor based on type of movement we want to do
                    switch (pSoldier.usUIMovementMode)
                    {
                        case AnimationStates.WALKING:
                            Globals.guiNewUICursor = UICursorDefines.ALL_MOVE_WALK_UICURSOR;
                            break;

                        case AnimationStates.RUNNING:
                            Globals.guiNewUICursor = UICursorDefines.ALL_MOVE_RUN_UICURSOR;
                            break;

                        case AnimationStates.SWATTING:
                            Globals.guiNewUICursor = UICursorDefines.ALL_MOVE_SWAT_UICURSOR;
                            break;

                        case AnimationStates.CRAWLING:
                            Globals.guiNewUICursor = UICursorDefines.ALL_MOVE_PRONE_UICURSOR;
                            break;
                    }
                }
            }
            else
            {
                if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
                {
                    Globals.guiNewUICursor = UICursorDefines.CONFIRM_MOVE_VEHICLE_UICURSOR;
                }
                else
                {
                    // Change mouse cursor based on type of movement we want to do
                    switch (pSoldier.usUIMovementMode)
                    {
                        case AnimationStates.WALKING:
                            Globals.guiNewUICursor = UICursorDefines.CONFIRM_MOVE_WALK_UICURSOR;
                            break;

                        case AnimationStates.RUNNING:
                            Globals.guiNewUICursor = UICursorDefines.CONFIRM_MOVE_RUN_UICURSOR;
                            break;

                        case AnimationStates.SWATTING:
                            Globals.guiNewUICursor = UICursorDefines.CONFIRM_MOVE_SWAT_UICURSOR;
                            break;

                        case AnimationStates.CRAWLING:
                            Globals.guiNewUICursor = UICursorDefines.CONFIRM_MOVE_PRONE_UICURSOR;
                            break;
                    }
                }
            }
        }

        if (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.REALTIME) || !Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
        {
            if (Globals.gfUIAllMoveOn > 0)
            {
                if (Globals.gfUIAllMoveOn == 2)
                {
                    this.BeginDisplayTimedCursor(UICursorDefines.MOVE_RUN_REALTIME_UICURSOR, 300);
                }
                else
                {
                    Globals.guiNewUICursor = UICursorDefines.ALL_MOVE_REALTIME_UICURSOR;
                }
            }
            else
            {
                if (pSoldier.fUIMovementFast && pSoldier.usAnimState == AnimationStates.RUNNING && fFromMove)
                {
                    this.BeginDisplayTimedCursor(UICursorDefines.MOVE_RUN_REALTIME_UICURSOR, 300);
                }

                Globals.guiNewUICursor = UICursorDefines.CONFIRM_MOVE_REALTIME_UICURSOR;
            }
        }

        Globals.guiNewUICursor = InteractiveTiles.GetInteractiveTileCursor(Globals.guiNewUICursor, true);
    }

    ScreenName UIHandleLCOnTerrain(UI_EVENT pUIEvent)
    {
        WorldDirections sFacingDir;

        Globals.guiNewUICursor = UICursorDefines.LOOK_UICURSOR;

        // Get soldier
        if (!Overhead.GetSoldier(out SOLDIERTYPE? pSoldier, Globals.gusSelectedSoldier))
        {
            return ScreenName.GAME_SCREEN;
        }

        Globals.gfUIDisplayActionPoints = true;

        Globals.gUIDisplayActionPointsOffX = 14;
        Globals.gUIDisplayActionPointsOffY = 7;


        // Get soldier
        if (!Overhead.GetSoldier(out pSoldier, Globals.gusSelectedSoldier))
        {
            return ScreenName.GAME_SCREEN;
        }

        IsometricUtils.GetMouseXY(out int sXPos, out int sYPos);

        // Get direction from mouse pos
        sFacingDir = SoldierControl.GetDirectionFromXY(sXPos, sYPos, pSoldier);

        // Set # of APs
        if (sFacingDir != pSoldier.bDirection)
        {
//            Globals.gsCurrentActionPoints = GetAPsToLook(pSoldier);
        }
        else
        {
            Globals.gsCurrentActionPoints = 0;
        }

        // Determine if we can afford!
//        if (!EnoughPoints(pSoldier, Globals.gsCurrentActionPoints, 0, false))
//        {
//            Globals.gfUIDisplayActionPointsInvalid = true;
//        }

        return ScreenName.GAME_SCREEN;

    }

    ScreenName UIHandleLCChangeToLook(UI_EVENT pUIEvent)
    {
        PathAI.ErasePath(true);

        return ScreenName.GAME_SCREEN;
    }


    bool MakeSoldierTurn(SOLDIERTYPE pSoldier, int sXPos, int sYPos)
    {
        WorldDirections sFacingDir;
        int sAPCost;

        // Get direction from mouse pos
        sFacingDir = SoldierControl.GetDirectionFromXY(sXPos, sYPos, pSoldier);

        if (sFacingDir != pSoldier.bDirection)
        {
//            sAPCost = GetAPsToLook(pSoldier);

            // Check AP cost...
//            if (!EnoughPoints(pSoldier, sAPCost, 0, true))
            {
                return false;
            }

            // ATE: make stationary if...
            if (pSoldier.fNoAPToFinishMove)
            {
                SoldierControl.SoldierGotoStationaryStance(pSoldier);
            }

            //DEF:  made it an event
            SoldierControl.SendSoldierSetDesiredDirectionEvent(pSoldier, sFacingDir);

            pSoldier.bTurningFromUI = true;

            // ATE: Hard-code here previous event to ui busy event...
            Globals.guiOldEvent = UI_EVENT_DEFINES.LA_BEGINUIOURTURNLOCK;

            return true;
        }

        return false;
    }


    ScreenName UIHandleLCLook(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE? pSoldier;
        int cnt;
        SOLDIERTYPE? pFirstSoldier = null;


        if (!IsometricUtils.GetMouseXY(out int sXPos, out int sYPos))
        {
            return ScreenName.GAME_SCREEN;
        }

        if (Globals.gTacticalStatus.fAtLeastOneGuyOnMultiSelect)
        {
            // OK, loop through all guys who are 'multi-selected' and
            cnt = Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bFirstID;
            for (pSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[gbPlayerNum].bLastID; cnt++)//, pSoldier++)
            {
                if (pSoldier.bActive && pSoldier.bInSector)
                {
                    if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MULTI_SELECTED))
                    {
                        this.MakeSoldierTurn(pSoldier, sXPos, sYPos);
                    }
                }
            }
        }
        else
        {
            // Get soldier
            if (!Overhead.GetSoldier(out pSoldier, Globals.gusSelectedSoldier))
            {
                return ScreenName.GAME_SCREEN;
            }

            if (this.MakeSoldierTurn(pSoldier, sXPos, sYPos))
            {
                this.SetUIBusy(pSoldier.ubID);
            }
        }
        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleTOnTerrain(UI_EVENT pUIEvent)
    {
        int uiRange;
        bool fValidTalkableGuy = false;
        int sTargetGridNo;
        int sDistVisible;

        // Get soldier
        if (!Overhead.GetSoldier(out SOLDIERTYPE? pSoldier, Globals.gusSelectedSoldier))
        {
            return ScreenName.GAME_SCREEN;
        }

        if (!IsometricUtils.GetMouseMapPos(out int usMapPos))
        {
            return ScreenName.GAME_SCREEN;
        }

        if (ValidQuickExchangePosition())
        {
            // Do new cursor!
            Globals.guiPendingOverrideEvent = UI_EVENT_DEFINES.M_ON_TERRAIN;
            return UIHandleMOnTerrain(pUIEvent);
        }

        sTargetGridNo = usMapPos;


        UIHandleOnMerc(false);


        //CHECK FOR VALID TALKABLE GUY HERE
        fValidTalkableGuy = IsValidTalkableNPCFromMouse(out int ubTargID, false, true, false);

        // USe cursor based on distance
        // Get distance away
        if (fValidTalkableGuy)
        {
            sTargetGridNo = Globals.MercPtrs[ubTargID].sGridNo;
        }

        uiRange = IsometricUtils.GetRangeFromGridNoDiff(pSoldier.sGridNo, sTargetGridNo);


        //ATE: Check if we have good LOS
        // is he close enough to see that gridno if he turns his head?
        sDistVisible = OppList.DistanceVisible(pSoldier, WorldDirections.DIRECTION_IRRELEVANT, WorldDirections.DIRECTION_IRRELEVANT, sTargetGridNo, pSoldier.bLevel);

//        if (uiRange <= NPC_TALK_RADIUS)
//        {
//            if (fValidTalkableGuy)
//            {
//                Globals.guiNewUICursor = UICursorDefines.TALK_A_UICURSOR;
//            }
//            else
//            {
//                Globals.guiNewUICursor = UICursorDefines.TALK_NA_UICURSOR;
//            }
//        }
//        else
//        {
//            if (fValidTalkableGuy)
//            {
//                //guiNewUICursor = TALK_OUT_RANGE_A_UICURSOR;		
//                Globals.guiNewUICursor = UICursorDefines.TALK_A_UICURSOR;
//            }
//            else
//            {
//                Globals.guiNewUICursor = UICursorDefines.TALK_OUT_RANGE_NA_UICURSOR;
//            }
//        }

        if (fValidTalkableGuy)
        {
            if (!LOS.SoldierTo3DLocationLineOfSightTest(pSoldier, sTargetGridNo, pSoldier.bLevel, 3, (byte)sDistVisible, 1))
            {
                //. ATE: Make range far, so we alternate cursors...
                Globals.guiNewUICursor = UICursorDefines.TALK_OUT_RANGE_A_UICURSOR;
            }
        }

        Globals.gfUIDisplayActionPoints = true;

        Globals.gUIDisplayActionPointsOffX = 8;
        Globals.gUIDisplayActionPointsOffY = 3;

        // Set # of APs
        Globals.gsCurrentActionPoints = 6;

        // Determine if we can afford!
//        if (!EnoughPoints(pSoldier, Globals.gsCurrentActionPoints, 0, false))
//        {
//            Globals.gfUIDisplayActionPointsInvalid = true;
//        }

        if (!Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
        {
            if (Globals.gfUIFullTargetFound)
            {
//                PauseRT(true);
            }
            else
            {
//                PauseRT(false);
            }
        }

        return ScreenName.GAME_SCREEN;

    }


    ScreenName UIHandleTChangeToTalking(UI_EVENT pUIEvent)
    {
        PathAI.ErasePath(true);

        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleLUIOnTerrain(UI_EVENT pUIEvent)
    {
        //guiNewUICursor = NO_UICURSOR;
        //	SetCurrentCursorFromDatabase( VIDEO_NO_CURSOR );

        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleLUIBeginLock(UI_EVENT pUIEvent)
    {
        // Don't let both versions of the locks to happen at the same time!
        // ( They are mutually exclusive )!
        this.UIHandleLAEndLockOurTurn(null);

        if (!Globals.gfDisableRegionActive)
        {
            Globals.gfDisableRegionActive = true;

            this.RemoveTacticalCursor();
            //SetCurrentCursorFromDatabase( VIDEO_NO_CURSOR );

            MouseSubSystem.MSYS_DefineRegion(Globals.gDisableRegion, new(0, 0, 640, 480), MSYS_PRIORITY.HIGHEST,
                                 CURSOR.WAIT, MSYS_NO_CALLBACK, MSYS_NO_CALLBACK);
            // Add region
            MouseSubSystem.MSYS_AddRegion(ref Globals.gDisableRegion);

            //Globals.guiPendingOverrideEvent = LOCKUI_MODE;

            // UnPause time!
            GameClock.PauseGame();
            GameClock.LockPauseState(16);
        }

        return ScreenName.GAME_SCREEN;
    }

    public static ScreenName UIHandleLUIEndLock(UI_EVENT? pUIEvent)
    {
        if (Globals.gfDisableRegionActive)
        {
            Globals.gfDisableRegionActive = false;

            // Add region
            MouseSubSystem.MSYS_RemoveRegion(Globals.gDisableRegion);
//            RefreshMouseRegions();

            //SetCurrentCursorFromDatabase( guiCurrentUICursor );

            Globals.guiForceRefreshMousePositionCalculation = true;
            UIHandleMOnTerrain(null);

            if (Globals.gViewportRegion.uiFlags.HasFlag(MouseRegionFlags.IN_AREA))
            {
//                SetCurrentCursorFromDatabase(Globals.gUICursors[Globals.guiNewUICursor].usFreeCursorName);
            }

            Globals.guiPendingOverrideEvent = UI_EVENT_DEFINES.M_ON_TERRAIN;
            HandleTacticalUI();

            // ATE: Only if NOT in conversation!
            if (!Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.ENGAGED_IN_CONV))
            {
                // UnPause time!
                GameClock.UnLockPauseState();
                GameClock.UnPauseGame();
            }
        }

        return ScreenName.GAME_SCREEN;
    }

    void CheckForDisabledRegionRemove()
    {
        if (Globals.gfDisableRegionActive)
        {
            Globals.gfDisableRegionActive = false;

            // Remove region
            MouseSubSystem.MSYS_RemoveRegion(Globals.gDisableRegion);

            GameClock.UnLockPauseState();
            GameClock.UnPauseGame();

        }

        if (Globals.gfUserTurnRegionActive)
        {
            Globals.gfUserTurnRegionActive = false;

            Globals.gfUIInterfaceSetBusy = false;

            // Remove region
            MouseSubSystem.MSYS_RemoveRegion(Globals.gUserTurnRegion);

            GameClock.UnLockPauseState();
            GameClock.UnPauseGame();
        }
    }

    ScreenName UIHandleLAOnTerrain(UI_EVENT pUIEvent)
    {
        //guiNewUICursor = NO_UICURSOR;
        //SetCurrentCursorFromDatabase( VIDEO_NO_CURSOR );

        return ScreenName.GAME_SCREEN;
    }

    void GetGridNoScreenXY(int sGridNo, out int? pScreenX, out int? pScreenY)
    {
        int sScreenX, sScreenY;
        int sOffsetX;
        int sOffsetY;

        IsometricUtils.ConvertGridNoToCellXY(sGridNo, out int sXPos, out int sYPos);

        // Get 'true' merc position
        sOffsetX = sXPos - Globals.gsRenderCenterX;
        sOffsetY = sYPos - Globals.gsRenderCenterY;

        IsometricUtils.FromCellToScreenCoordinates(sOffsetX, sOffsetY, out int sTempX_S, out var sTempY_S);

        sScreenX = ((Globals.gsVIEWPORT_END_X - Globals.gsVIEWPORT_START_X) / 2) + (int)sTempX_S;
        sScreenY = ((Globals.gsVIEWPORT_END_Y - Globals.gsVIEWPORT_START_Y) / 2) + (int)sTempY_S;

        // Adjust for offset position on screen
        sScreenX -= Globals.gsRenderWorldOffsetX;
        sScreenY -= Globals.gsRenderWorldOffsetY;
        sScreenY -= Globals.gpWorldLevelData[sGridNo].sHeight;

        // Adjust based on interface level

        // Adjust for render height
        sScreenY += Globals.gsRenderHeight;

        // Adjust y offset!
        sScreenY += Globals.WORLD_TILE_Y / 2;

        pScreenX = sScreenX;
        pScreenY = sScreenY;
    }

    void EndMultiSoldierSelection(bool fAcknowledge)
    {
        SOLDIERTYPE? pSoldier;
        int cnt;
        SOLDIERTYPE? pFirstSoldier = null;
        bool fSelectedSoldierInBatch = false;

        Globals.gTacticalStatus.fAtLeastOneGuyOnMultiSelect = false;

        // OK, loop through all guys who are 'multi-selected' and
        // check if our currently selected guy is amoung the
        // lucky few.. if not, change to a guy who is...
        cnt = Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bFirstID;
        for (pSoldier = Globals.MercPtrs[cnt]; cnt <= Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bLastID; cnt++)//, pSoldier++)
        {
            if (pSoldier.bActive && pSoldier.bInSector)
            {
                if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MULTI_SELECTED))
                {
                    Globals.gTacticalStatus.fAtLeastOneGuyOnMultiSelect = true;

                    if (pSoldier.ubID != Globals.gusSelectedSoldier && pFirstSoldier == null)
                    {
                        pFirstSoldier = pSoldier;
                    }

                    if (pSoldier.ubID == Globals.gusSelectedSoldier)
                    {
                        fSelectedSoldierInBatch = true;
                    }

                    if (!GameSettings.fOptions[TOPTION.MUTE_CONFIRMATIONS] && fAcknowledge)
                    {
//                        InternalDoMercBattleSound(pSoldier, BATTLE_SOUND.ATTN1, BATTLE_SND_LOWER_VOLUME);
                    }

                    if (pSoldier.fMercAsleep)
                    {
//                        PutMercInAwakeState(pSoldier);
                    }
                }
            }
        }

        // If here, select the first guy...
        if (pFirstSoldier != null && !fSelectedSoldierInBatch)
        {
//            SelectSoldier(pFirstSoldier.ubID, true, true);
        }

    }


    void StopRubberBandedMercFromMoving()
    {
        SOLDIERTYPE? pSoldier;
        int cnt;
        SOLDIERTYPE? pFirstSoldier = null;

        if (!Globals.gTacticalStatus.fAtLeastOneGuyOnMultiSelect)
        {
            return;
        }

        // OK, loop through all guys who are 'multi-selected' and
        // check if our currently selected guy is amoung the
        // lucky few.. if not, change to a guy who is...
        cnt = Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bFirstID;
        for (pSoldier = Globals.MercPtrs[cnt]; cnt <= Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bLastID; cnt++)//, pSoldier++)
        {
            if (pSoldier.bActive && pSoldier.bInSector)
            {
                if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MULTI_SELECTED))
                {
                    pSoldier.fDelayedMovement = 0;
                    pSoldier.sFinalDestination = pSoldier.sGridNo;
//                    StopSoldier(pSoldier);
                }
            }
        }

    }


    void EndRubberBanding()
    {
        if (Globals.gRubberBandActive)
        {
//            FreeMouseCursor();
            Globals.gfIgnoreScrolling = false;

            this.EndMultiSoldierSelection(true);

            Globals.gRubberBandActive = false;
        }
    }


    bool HandleMultiSelectionMove(int sDestGridNo)
    {
        SOLDIERTYPE? pSoldier;
        int cnt;
        bool fAtLeastOneMultiSelect = false;
        bool fMoveFast = false;

        // OK, loop through all guys who are 'multi-selected' and
        // Make them move....

        // Do a loop first to see if the selected guy is told to go fast...
        Globals.gfGetNewPathThroughPeople = true;

        cnt = Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bFirstID;
        for (pSoldier = Globals.MercPtrs[cnt]; cnt <= Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bLastID; cnt++)//, pSoldier++)
        {
            if (pSoldier.bActive && pSoldier.bInSector)
            {
                if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MULTI_SELECTED))
                {
                    if (pSoldier.ubID == Globals.gusSelectedSoldier)
                    {
                        fMoveFast = pSoldier.fUIMovementFast;
                        break;
                    }
                }
            }
        }

        cnt = Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bFirstID;
        for (pSoldier = Globals.MercPtrs[cnt]; cnt <= Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bLastID; cnt++)//, pSoldier++)
        {
            if (pSoldier.bActive && pSoldier.bInSector)
            {
                if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MULTI_SELECTED))
                {
                    // If we can't be controlled, returninvalid...
                    if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.ROBOT))
                    {
                        if (!SoldierControl.CanRobotBeControlled(pSoldier))
                        {
                            continue;
                        }
                    }

                    pSoldier.fUIMovementFast = fMoveFast;
//                    pSoldier.usUIMovementMode = GetMoveStateBasedOnStance(pSoldier, Globals.gAnimControl[pSoldier.usAnimState].ubEndHeight);

                    pSoldier.fUIMovementFast = false;

                    if (Globals.gUIUseReverse)
                    {
                        pSoldier.bReverse = true;
                    }
                    else
                    {
                        pSoldier.bReverse = false;
                    }

                    // Remove any previous actions
                    pSoldier.ubPendingAction = MERC.NO_PENDING_ACTION;


                    if (SoldierControl.EVENT_InternalGetNewSoldierPath(pSoldier, sDestGridNo, pSoldier.usUIMovementMode, 1, pSoldier.fNoAPToFinishMove))
                    {
//                        InternalDoMercBattleSound(pSoldier, BATTLE_SOUND.OK1, BATTLE_SND_LOWER_VOLUME);
                    }
                    else
                    {
//                        Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.INTERFACE, Globals.TacticalStr[NO_PATH_FOR_MERC], pSoldier.name);
                    }

                    fAtLeastOneMultiSelect = true;
                }
            }
        }

        Globals.gfGetNewPathThroughPeople = false;

        return fAtLeastOneMultiSelect;
    }


    public static void ResetMultiSelection()
    {
        SOLDIERTYPE? pSoldier;
        int cnt;

        // OK, loop through all guys who are 'multi-selected' and
        // Make them move....

        cnt = Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bFirstID;
        for (pSoldier = Globals.MercPtrs[cnt]; cnt <= Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bLastID; cnt++)//, pSoldier++)
        {
            if (pSoldier.bActive && pSoldier.bInSector)
            {
                if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.MULTI_SELECTED))
                {
                    pSoldier.uiStatusFlags &= ~SOLDIER.MULTI_SELECTED;
                }
            }
        }

        Globals.gTacticalStatus.fAtLeastOneGuyOnMultiSelect = false;
    }

    ScreenName UIHandleRubberBandOnTerrain(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE? pSoldier;
        int cnt;
        int? sScreenX, sScreenY;
        int iTemp;
        Rectangle aRect = new(); // gRubberBandRect when appropriate
        bool fAtLeastOne = false;

        Globals.guiNewUICursor = UICursorDefines.NO_UICURSOR;
        //SetCurrentCursorFromDatabase( VIDEO_NO_CURSOR );

        Globals.gRubberBandRect.Width = Globals.gusMouseXPos;
        Globals.gRubberBandRect.Height = Globals.gusMouseYPos;

        aRect = new(
            Globals.gRubberBandRect.X,
            Globals.gRubberBandRect.Y,
            Globals.gRubberBandRect.Width,
            Globals.gRubberBandRect.Height);

        // Copy into temp rect
        // memcpy(out aRect, &gRubberBandRect, sizeof(gRubberBandRect));

        if (aRect.Width < aRect.Left)
        {
            iTemp = aRect.Left;
            aRect.X = aRect.Right;
            aRect.Width = aRect.X + iTemp;
        }

        if (aRect.Bottom < aRect.Top)
        {
            iTemp = aRect.Top;
            aRect.Y = aRect.Bottom;
            aRect.Height = iTemp + aRect.Y;
        }

        // ATE:Check at least for one guy that's in point!
        cnt = Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bFirstID;
        for (pSoldier = Globals.MercPtrs[cnt]; cnt <= Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bLastID; cnt++)//, pSoldier++)
        {
            // Check if this guy is OK to control....
            if (Soldier.OK_CONTROLLABLE_MERC(pSoldier) && !pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE | SOLDIER.PASSENGER | SOLDIER.DRIVER))
            {
                // Get screen pos of gridno......
                this.GetGridNoScreenXY(pSoldier.sGridNo, out sScreenX, out sScreenY);

                // ATE: If we are in a hiehger interface level, subttrasct....
                if (Globals.gsInterfaceLevel == 1)
                {
                    sScreenY -= 50;
                }

                if (IsometricUtils.IsPointInScreenRect(sScreenX, sScreenY, aRect))
                {
                    fAtLeastOne = true;
                }
            }
        }

        if (!fAtLeastOne)
        {
            return ScreenName.GAME_SCREEN;
        }

        // ATE: Now loop through our guys and see if any fit!
        cnt = Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bFirstID;
        for (pSoldier = Globals.MercPtrs[cnt]; cnt <= Globals.gTacticalStatus.Team[Globals.gbPlayerNum].bLastID; cnt++)//, pSoldier++)
        {
            // Check if this guy is OK to control....
            if (Soldier.OK_CONTROLLABLE_MERC(pSoldier) && !pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE | SOLDIER.PASSENGER | SOLDIER.DRIVER))
            {
                //                if (!_KeyDown(ALT))
                //                {
                //                    pSoldier.uiStatusFlags &= (~SOLDIER.MULTI_SELECTED);
                //                }

                // Get screen pos of gridno......
                this.GetGridNoScreenXY(pSoldier.sGridNo, out sScreenX, out sScreenY);

                // ATE: If we are in a hiehger interface level, subttrasct....
                if (Globals.gsInterfaceLevel == 1)
                {
                    sScreenY -= 50;
                }

//                if (IsPointInScreenRect(sScreenX, sScreenY, out aRect))
                {
                    // Adjust this guy's flag...
                    pSoldier.uiStatusFlags |= SOLDIER.MULTI_SELECTED;
                }
            }
        }

        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleJumpOverOnTerrain(UI_EVENT pUIEvent)
    {

        // Here, first get map screen
        if (!Overhead.GetSoldier(out SOLDIERTYPE? pSoldier, Globals.gusSelectedSoldier))
        {
            return ScreenName.GAME_SCREEN;
        }

        if (!IsometricUtils.GetMouseMapPos(out int usMapPos))
        {
            return ScreenName.GAME_SCREEN;
        }

        if (!this.IsValidJumpLocation(pSoldier, usMapPos, false))
        {
            Globals.guiPendingOverrideEvent = UI_EVENT_DEFINES.M_ON_TERRAIN;
            return ScreenName.GAME_SCREEN;
        }

        // Display APs....
//        Globals.gsCurrentActionPoints = GetAPsToJumpOver(pSoldier);

        Globals.gfUIDisplayActionPoints = true;
        Globals.gfUIDisplayActionPointsCenter = true;

        Globals.guiNewUICursor = UICursorDefines.JUMP_OVER_UICURSOR;

        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleJumpOver(UI_EVENT pUIEvent)
    {
        WorldDirections bDirection = 0;

        // Here, first get map screen
        if (!Overhead.GetSoldier(out SOLDIERTYPE? pSoldier, Globals.gusSelectedSoldier))
        {
            return ScreenName.GAME_SCREEN;
        }

        if (!IsometricUtils.GetMouseMapPos(out int usMapPos))
        {
            return ScreenName.GAME_SCREEN;
        }

        if (!this.IsValidJumpLocation(pSoldier, usMapPos, false))
        {
            return ScreenName.GAME_SCREEN;
        }

        this.SetUIBusy(pSoldier.ubID);

        // OK, Start jumping!
        // Remove any previous actions
        pSoldier.ubPendingAction = MERC.NO_PENDING_ACTION;

        // Get direction to goto....
//        bDirection = GetDirectionFromGridNo(usMapPos, pSoldier);


        pSoldier.fDontChargeTurningAPs = true;
        SoldierControl.EVENT_InternalSetSoldierDesiredDirection(pSoldier, bDirection, false, pSoldier.usAnimState);
        pSoldier.fTurningUntilDone = true;
        // ATE: Reset flag to go back to prone...
        //pSoldier.fTurningFromPronePosition = TURNING_FROM_PRONE_OFF;
        pSoldier.usPendingAnimation = AnimationStates.JUMP_OVER_BLOCKING_PERSON;


        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleLABeginLockOurTurn(UI_EVENT pUIEvent)
    {
        // Don't let both versions of the locks to happen at the same time!
        // ( They are mutually exclusive )!
        UIHandleLUIEndLock(null);

        if (!Globals.gfUserTurnRegionActive)
        {
            Globals.gfUserTurnRegionActive = true;

            Globals.gfUIInterfaceSetBusy = true;
            Globals.guiUIInterfaceBusyTime = Globals.GetJA2Clock();

            //guiNewUICursor = NO_UICURSOR;
            //SetCurrentCursorFromDatabase( VIDEO_NO_CURSOR );

            MouseSubSystem.MSYS_DefineRegion(Globals.gUserTurnRegion, new Rectangle(0, 0, 640, 480), MSYS_PRIORITY.HIGHEST,
                                 CURSOR.WAIT, MSYS_NO_CALLBACK, MSYS_NO_CALLBACK);
            // Add region
            MouseSubSystem.MSYS_AddRegion(ref Globals.gUserTurnRegion);

            //Globals.guiPendingOverrideEvent = LOCKOURTURN_UI_MODE;

            PathAI.ErasePath(true);

            // Pause time!
            GameClock.PauseGame();
            GameClock.LockPauseState(17);
        }

        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleLAEndLockOurTurn(UI_EVENT? pUIEvent)
    {
        if (Globals.gfUserTurnRegionActive)
        {
            Globals.gfUserTurnRegionActive = false;

            Globals.gfUIInterfaceSetBusy = false;

            // Add region
            MouseSubSystem.MSYS_RemoveRegion(Globals.gUserTurnRegion);
//            RefreshMouseRegions();
            //SetCurrentCursorFromDatabase( guiCurrentUICursor );

            Globals.gfPlotNewMovement = true;

            Globals.guiForceRefreshMousePositionCalculation = true;
            UIHandleMOnTerrain(null);

            if (Globals.gViewportRegion.uiFlags.HasFlag(MouseRegionFlags.IN_AREA))
            {
                CursorSubSystem.SetCurrentCursorFromDatabase(Globals.gUICursors[Globals.guiNewUICursor].usFreeCursorName);
            }
            Globals.guiPendingOverrideEvent = UI_EVENT_DEFINES.M_ON_TERRAIN;
            HandleTacticalUI();

//            TurnOffTeamsMuzzleFlashes(Globals.gbPlayerNum);

            // UnPause time!
            GameClock.UnLockPauseState();
            GameClock.UnPauseGame();
        }

        return ScreenName.GAME_SCREEN;
    }

    public static bool IsValidTalkableNPCFromMouse(out int pubSoldierID, bool fGive, bool fAllowMercs, bool fCheckCollapsed)
    {
        // Check if there is a guy here to talk to!
        if (Globals.gfUIFullTargetFound)
        {
            pubSoldierID = Globals.gusUIFullTargetID;
            return IsValidTalkableNPC(Globals.gusUIFullTargetID, fGive, fAllowMercs, fCheckCollapsed);
        }

        pubSoldierID = 0;
        return false;
    }

    public static bool IsValidTalkableNPC(int ubSoldierID, bool fGive, bool fAllowMercs, bool fCheckCollapsed)
    {
        SOLDIERTYPE pSoldier = Globals.MercPtrs[ubSoldierID];
        bool fValidGuy = false;

        if (Globals.gusSelectedSoldier != Globals.NOBODY)
        {
            if (AM_A_ROBOT(Globals.MercPtrs[Globals.gusSelectedSoldier]))
            {
                return false;
            }
        }

        // CHECK IF ACTIVE!
        if (!pSoldier.bActive)
        {
            return false;
        }

        // CHECK IF DEAD
        if (pSoldier.bLife == 0)
        {
            return false;
        }

        if (pSoldier.bCollapsed && fCheckCollapsed)
        {
            return false;
        }

        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.VEHICLE))
        {
            return false;
        }


        // IF BAD GUY - CHECK VISIVILITY
        if (pSoldier.bTeam != (TEAM)Globals.gbPlayerNum)
        {
            if (pSoldier.bVisible == -1 && !Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.SHOW_ALL_MERCS))
            {
                return false;
            }
        }

        if (pSoldier.ubProfile != NPCID.NO_PROFILE && pSoldier.ubProfile >= Globals.FIRST_RPC && !RPC_RECRUITED(pSoldier) && !AM_AN_EPC(pSoldier))
        {
            fValidGuy = true;
        }

        // Check for EPC...
        if (pSoldier.ubProfile != NPCID.NO_PROFILE && (Globals.gCurrentUIMode == UI_MODE.TALKCURSOR_MODE || fGive) && AM_AN_EPC(pSoldier))
        {
            fValidGuy = true;
        }

        // ATE: We can talk to our own teammates....
        if (pSoldier.bTeam == Globals.gbPlayerNum && fAllowMercs)
        {
            fValidGuy = true;
        }

//        if (GetCivType(pSoldier) != CIV_TYPE_NA && !fGive)
//        {
//            fValidGuy = true;
//        }

        // Alright, let's do something special here for robot...
        if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.ROBOT))
        {
            if (fValidGuy == true && !fGive)
            {
                // Can't talk to robots!
                fValidGuy = false;
            }
        }

        // OK, check if they are stationary or not....
        // Do some checks common to all..
        if (fValidGuy)
        {
            if (Globals.gAnimControl[pSoldier.usAnimState].uiFlags.HasFlag(ANIM.MOVING) && !Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
            {
                return false;
            }

            return true;
        }

        return false;
    }


    bool HandleTalkInit()
    {
        int sAPCost;
        int uiRange;
        int sGoodGridNo;
        byte ubNewDirection;
        QUOTE ubQuoteNum = 0;
        byte ubDiceRoll;
        int sDistVisible;
        int sActionGridNo;
        WorldDirections ubDirection = 0;

        // Get soldier
        if (!Overhead.GetSoldier(out SOLDIERTYPE? pSoldier, Globals.gusSelectedSoldier))
        {
            return false;
        }

        if (!IsometricUtils.GetMouseMapPos(out int usMapPos))
        {
            return false;
        }

        // Check if there is a guy here to talk to!
        if (Globals.gfUIFullTargetFound)
        {
            // Is he a valid NPC?
            if (IsValidTalkableNPC(Globals.gusUIFullTargetID, false, true, false))
            {
                Overhead.GetSoldier(out SOLDIERTYPE? pTSoldier, Globals.gusUIFullTargetID);

                if (pTSoldier.ubID != pSoldier.ubID)
                {
                    //ATE: Check if we have good LOS
                    // is he close enough to see that gridno if he turns his head?
                    sDistVisible = OppList.DistanceVisible(pSoldier, WorldDirections.DIRECTION_IRRELEVANT, WorldDirections.DIRECTION_IRRELEVANT, pTSoldier.sGridNo, pTSoldier.bLevel);

                    // Check LOS!
                    if (!LOS.SoldierTo3DLocationLineOfSightTest(pSoldier, pTSoldier.sGridNo, pTSoldier.bLevel, 3, (byte)sDistVisible, 1))
                    {
                        if (pTSoldier.ubProfile != NPCID.NO_PROFILE)
                        {
                            Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, Globals.TacticalStr[(int)STR.NO_LOS_TO_TALK_TARGET], pSoldier.name, pTSoldier.name);
                        }
                        else
                        {
                            Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, gzLateLocalizedString[45], pSoldier.name);
                        }
                        return false;
                    }
                }

                if (pTSoldier.bCollapsed)
                {
                    Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, gzLateLocalizedString[21], pTSoldier.name);
                    return false;
                }

                // If Q on, turn off.....
                if (Globals.guiCurrentScreen == ScreenName.DEBUG_SCREEN)
                {
                    Globals.gfExitDebugScreen = true;
                }

                // ATE: if our own guy...
                if (pTSoldier.bTeam == Globals.gbPlayerNum && !AM_AN_EPC(pTSoldier))
                {
                    if (pTSoldier.ubProfile == NPCID.DIMITRI)
                    {
                        Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, gzLateLocalizedString[32], pTSoldier.name);
                        return false;
                    }

                    // Randomize quote to use....

                    // If buddy had a social trait...
                    if (Globals.gMercProfiles[pTSoldier.ubProfile].bAttitude != ATT.NORMAL)
                    {
                        ubDiceRoll = (byte)this.rnd.Next(3);
                    }
                    else
                    {
                        ubDiceRoll = (byte)this.rnd.Next(2);
                    }

                    // If we are a PC, only use 0
                    if (pTSoldier.ubWhatKindOfMercAmI == MERC_TYPE.PLAYER_CHARACTER)
                    {
                        ubDiceRoll = 0;
                    }

                    switch (ubDiceRoll)
                    {
                        case 0:

                            ubQuoteNum = QUOTE.NEGATIVE_COMPANY;
                            break;

                        case 1:

//                            if (QuoteExp_PassingDislike[pTSoldier.ubProfile])
//                            {
//                                ubQuoteNum = QUOTE.PASSING_DISLIKE;
//                            }
//                            else
//                            {
//                                ubQuoteNum = QUOTE.NEGATIVE_COMPANY;
//                            }
                            break;

                        case 2:

                            ubQuoteNum = QUOTE.SOCIAL_TRAIT;
                            break;

                        default:

                            ubQuoteNum = QUOTE.NEGATIVE_COMPANY;
                            break;
                    }

                    if (pTSoldier.ubProfile == NPCID.IRA)
                    {
                        ubQuoteNum = QUOTE.PASSING_DISLIKE;
                    }

                    DialogControl.TacticalCharacterDialogue(pTSoldier, ubQuoteNum);

                    return false;
                }

                // Check distance
                uiRange = IsometricUtils.GetRangeFromGridNoDiff(pSoldier.sGridNo, (int)usMapPos);

                // Double check path
//                if (GetCivType(pTSoldier) != CIV_TYPE_NA)
//                {
//                    // ATE: If one is already active, just remove it!
//                    if (ShutDownQuoteBoxIfActive())
//                    {
//                        return (false);
//                    }
//                }

//                if (uiRange > NPC_TALK_RADIUS)
                {
                    // First get an adjacent gridno....
                    sActionGridNo = Overhead.FindAdjacentGridEx(pSoldier, pTSoldier.sGridNo, ref ubDirection, out var _, false, true);

                    if (sActionGridNo == -1)
                    {
//                        Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, Globals.TacticalStr[NO_PATH]);
                        return false;
                    }

                    if (PathAI.UIPlotPath(pSoldier, sActionGridNo, PlotPathDefines.NO_COPYROUTE, false, PlotPathDefines.TEMPORARY, pSoldier.usUIMovementMode, PlotPathDefines.NOT_STEALTH, PlotPathDefines.FORWARD, pSoldier.bActionPoints) == 0)
                    {
//                        Messages.ScreenMsg(FontColor.FONT_MCOLOR_LTYELLOW, MSG.UI_FEEDBACK, Globals.TacticalStr[NO_PATH]);
                        return false;
                    }

                    // Walk up and talk to buddy....
                    Globals.gfNPCCircularDistLimit = true;
//                    sGoodGridNo = FindGridNoFromSweetSpotWithStructData(pSoldier, pSoldier.usUIMovementMode, pTSoldier.sGridNo, (NPC_TALK_RADIUS - 1), out ubNewDirection, true);
                    Globals.gfNPCCircularDistLimit = false;

                    // First calculate APs and validate...
                    sAPCost = AP.TALK;
                    //sAPCost += UIPlotPath( pSoldier, sGoodGridNo, PlotPath.NO_COPYROUTE, false, TEMPORARY, (uint)pSoldier.usUIMovementMode, NOT_STEALTH, FORWARD, pSoldier.bActionPoints );

                    // Check AP cost...
//                    if (!EnoughPoints(pSoldier, sAPCost, 0, true))
                    {
                        return false;
                    }

                    // Now walkup to talk....
                    pSoldier.ubPendingAction = MERC.TALK;
                    pSoldier.uiPendingActionData1 = pTSoldier.ubID;
                    pSoldier.ubPendingActionAnimCount = 0;

                    // WALK UP TO DEST FIRST
//                    SoldierControl.EVENT_InternalGetNewSoldierPath(pSoldier, sGoodGridNo, pSoldier.usUIMovementMode, 1, pSoldier.fNoAPToFinishMove);

                    return false;
                }
//                else
                {
                    sAPCost = AP.TALK;

                    // Check AP cost...
//                    if (!EnoughPoints(pSoldier, sAPCost, 0, true))
                    {
                        return false;
                    }

                    // OK, startup!
//                    PlayerSoldierStartTalking(pSoldier, pTSoldier.ubID, false);
                }

//                if (GetCivType(pTSoldier) != CIV_TYPE_NA)
                {
                    return false;
                }
//                else
                {
                    return true;
                }
            }
        }

        return false;
    }



    void SetUIBusy(int ubID)
    {
        if ((Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
            & (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED))
            && (Globals.gTacticalStatus.ubCurrentTeam == Globals.gbPlayerNum))
        {
            if (Globals.gusSelectedSoldier == ubID)
            {
                Globals.guiPendingOverrideEvent = UI_EVENT_DEFINES.LA_BEGINUIOURTURNLOCK;
                HandleTacticalUI();
            }
        }
    }

    public static void UnSetUIBusy(int ubID)
    {
        if (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)
            && Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)
            && (Globals.gTacticalStatus.ubCurrentTeam == Globals.gbPlayerNum))
        {
            if (!Globals.gTacticalStatus.fUnLockUIAfterHiddenInterrupt)
            {
                if (Globals.gusSelectedSoldier == ubID)
                {
                    Globals.guiPendingOverrideEvent = UI_EVENT_DEFINES.LA_ENDUIOUTURNLOCK;
                    HandleTacticalUI();

                    // Set grace period...
                    Globals.gTacticalStatus.uiTactialTurnLimitClock = Globals.GetJA2Clock();
                }
            }
            // player getting control back so reset all muzzle flashes
        }
    }

    public void BeginDisplayTimedCursor(UICursorDefines uiCursorID, uint uiDelay)
    {
        Globals.gfDisplayTimerCursor = true;
        Globals.guiTimerCursorID = uiCursorID;
        Globals.guiTimerLastUpdate = Globals.GetJA2Clock();
        Globals.guiTimerCursorDelay = uiDelay;
    }


    static bool fOverPool = false;
    static bool fOverEnemy = false;

    public static byte UIHandleInteractiveTilesAndItemsOnTerrain(SOLDIERTYPE pSoldier, int usMapPos, bool fUseOKCursor, bool fItemsOnlyIfOnIntTiles)
    {
        bool fSetCursor;
        LEVELNODE? pIntTile = null;
        int sActionGridNo;
        int sIntTileGridNo;
        bool fContinue = true;
        STRUCTURE? pStructure = null;
        bool fPoolContainsHiddenItems = false;
        SOLDIERTYPE pTSoldier;


        if (Globals.gfResetUIItemCursorOptimization)
        {
            Globals.gfResetUIItemCursorOptimization = false;
            fOverPool = false;
            fOverEnemy = false;
        }

        GetCursorMovementFlags(out MOUSE uiCursorFlags);

        // Default gridno to mouse pos
        sActionGridNo = usMapPos;

        // Look for being on a merc....
        // Steal.....
        UIHandleOnMerc(false);

        Globals.gfBeginVehicleCursor = false;

        if (Globals.gfUIFullTargetFound)
        {
            pTSoldier = Globals.MercPtrs[Globals.gusUIFullTargetID];

            if (OK_ENTERABLE_VEHICLE(pTSoldier) && pTSoldier.bVisible != -1)
            {
                // grab number of occupants in vehicles
                if (fItemsOnlyIfOnIntTiles)
                {
//                    if (!OKUseVehicle(pTSoldier.ubProfile))
//                    {
//                        // Set UI CURSOR....
//                        Globals.guiNewUICursor = UICursorDefines.CANNOT_MOVE_UICURSOR;
//
//                        Globals.gfBeginVehicleCursor = true;
//                        return (1);
//                    }
//                    else
//                    {
//                        if (GetNumberInVehicle(pTSoldier.bVehicleID) == 0)
//                        {
//                            // Set UI CURSOR....
//                            Globals.guiNewUICursor = UICursorDefines.ENTER_VEHICLE_UICURSOR;
//
//                            Globals.gfBeginVehicleCursor = true;
//                            return (1);
//                        }
//                    }
                }
                else
                {
                    // Set UI CURSOR....
                    Globals.guiNewUICursor = UICursorDefines.ENTER_VEHICLE_UICURSOR;
                    return 1;
                }
            }

            if (!fItemsOnlyIfOnIntTiles)
            {
                if (Globals.guiUIFullTargetFlags.HasFlag(FIND_SOLDIER_RESPONSES.ENEMY_MERC)
                    && !Globals.guiUIFullTargetFlags.HasFlag(FIND_SOLDIER_RESPONSES.UNCONSCIOUS_MERC))
                {
                    if (!fOverEnemy)
                    {
                        fOverEnemy = true;
                        Globals.gfPlotNewMovement = true;
                    }

                    //Set UI CURSOR
                    if (fUseOKCursor || (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)
                        && Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)))
                    {
                        Globals.guiNewUICursor = UICursorDefines.OKHANDCURSOR_UICURSOR;
                    }
                    else
                    {
                        Globals.guiNewUICursor = UICursorDefines.NORMALHANDCURSOR_UICURSOR;
                    }

                    fSetCursor = HandleUIMovementCursor(pSoldier, uiCursorFlags, sActionGridNo, MOVEUI_TARGET.STEAL);

                    // Display action points
                    Globals.gfUIDisplayActionPoints = true;

                    // Determine if we can afford!
//                    if (!EnoughPoints(pSoldier, Globals.gsCurrentActionPoints, 0, false))
//                    {
//                        Globals.gfUIDisplayActionPointsInvalid = true;
//                    }

                    return 0;
                }
            }
        }

        if (fOverEnemy)
        {
            PathAI.ErasePath(true);
            fOverEnemy = false;
            Globals.gfPlotNewMovement = true;
        }

        // If we are over an interactive struct, adjust gridno to this....
//        pIntTile = ConditionalGetCurInteractiveTileGridNoAndStructure(out sIntTileGridNo, out pStructure, false);
//        Globals.gpInvTileThatCausedMoveConfirm = pIntTile;

        if (pIntTile != null)
        {
//            sActionGridNo = sIntTileGridNo;
        }

        // Check if we are over an item pool
        if (HandleItems.GetItemPool(sActionGridNo, out ITEM_POOL? pItemPool, pSoldier.bLevel))
        {
            // If we want only on int tiles, and we have no int tiles.. ignore items!
            if (fItemsOnlyIfOnIntTiles && pIntTile == null)
            {

            }
            else if (fItemsOnlyIfOnIntTiles
                && pIntTile != null
                && pStructure.fFlags.HasFlag(STRUCTUREFLAGS.HASITEMONTOP))
            {
                // if in this mode, we don't want to automatically show hand cursor over items on strucutres
            }
            //else if ( pIntTile != null && ( pStructure.fFlags & ( STRUCTUREFLAGS.SWITCH | STRUCTUREFLAGS.ANYDOOR ) ) )
            else if (pIntTile != null
                && pStructure.fFlags.HasFlag(STRUCTUREFLAGS.SWITCH))
            {
                // We don't want switches messing around with items ever!
            }
            else if (pIntTile != null
                && pStructure.fFlags.HasFlag(STRUCTUREFLAGS.ANYDOOR)
                && (sActionGridNo != usMapPos || fItemsOnlyIfOnIntTiles))
            {
                // Next we look for if we are over a door and if the mouse position is != base door position, ignore items!
            }
            else
            {
//                fPoolContainsHiddenItems = DoesItemPoolContainAnyHiddenItems(pItemPool);

                // Adjust this if we have not visited this gridno yet...
                if (fPoolContainsHiddenItems)
                {
                    if (!Globals.gpWorldLevelData[sActionGridNo].uiFlags.HasFlag(MAPELEMENTFLAGS.REVEALED))
                    {
                        fPoolContainsHiddenItems = false;
                    }
                }

                if (ITEMPOOL_VISIBLE(pItemPool) || fPoolContainsHiddenItems)
                {

                    if (!fOverPool)
                    {
                        fOverPool = true;
                        Globals.gfPlotNewMovement = true;
                    }

                    //Set UI CURSOR
                    if (fUseOKCursor || (Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT)
                        && Globals.gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.TURNBASED)))
                    {
                        Globals.guiNewUICursor = UICursorDefines.OKHANDCURSOR_UICURSOR;
                    }
                    else
                    {
                        Globals.guiNewUICursor = UICursorDefines.NORMALHANDCURSOR_UICURSOR;
                    }

                    fSetCursor = HandleUIMovementCursor(pSoldier, uiCursorFlags, sActionGridNo, MOVEUI_TARGET.ITEMS);

                    // Display action points
                    Globals.gfUIDisplayActionPoints = true;

                    if (Globals.gsOverItemsGridNo == sActionGridNo)
                    {
                        Globals.gfPlotNewMovement = true;
                    }

                    // Determine if we can afford!
//                    if (!EnoughPoints(pSoldier, Globals.gsCurrentActionPoints, 0, false))
//                    {
//                        Globals.gfUIDisplayActionPointsInvalid = true;
//                    }

                    fContinue = false;
                }
            }
        }

        if (fContinue)
        {
            // Try interactive tiles now....
            if (pIntTile != null)
            {
                if (fOverPool)
                {
                    PathAI.ErasePath(true);
                    fOverPool = false;
                    Globals.gfPlotNewMovement = true;
                }

                HandleUIMovementCursor(pSoldier, uiCursorFlags, usMapPos, MOVEUI_TARGET.INTTILES);

                //Set UI CURSOR
                Globals.guiNewUICursor = InteractiveTiles.GetInteractiveTileCursor(guiNewUICursor, fUseOKCursor);
            }
            else
            {
                if (!fItemsOnlyIfOnIntTiles)
                {
                    // Let's at least show where the merc will walk to if they go here...
                    if (!fOverPool)
                    {
                        fOverPool = true;
                        Globals.gfPlotNewMovement = true;
                    }

                    fSetCursor = HandleUIMovementCursor(pSoldier, uiCursorFlags, sActionGridNo, MOVEUI_TARGET.ITEMS);

                    // Display action points
                    Globals.gfUIDisplayActionPoints = true;

                    // Determine if we can afford!
//                    if (!EnoughPoints(pSoldier, Globals.gsCurrentActionPoints, 0, false))
//                    {
//                        Globals.gfUIDisplayActionPointsInvalid = true;
//                    }
                }
            }

        }

        if (pIntTile == null)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }

    public static bool SelectedGuyInBusyAnimation()
    {
        SOLDIERTYPE? pSoldier;

        if (Globals.gusSelectedSoldier != Globals.NOBODY)
        {
            pSoldier = Globals.MercPtrs[Globals.gusSelectedSoldier];

            if (pSoldier.usAnimState == AnimationStates.LOB_ITEM
                || pSoldier.usAnimState == AnimationStates.THROW_ITEM
                || pSoldier.usAnimState == AnimationStates.PICKUP_ITEM
                || pSoldier.usAnimState == AnimationStates.DROP_ITEM
                || pSoldier.usAnimState == AnimationStates.OPEN_DOOR
                || pSoldier.usAnimState == AnimationStates.OPEN_STRUCT
                || pSoldier.usAnimState == AnimationStates.OPEN_STRUCT
                || pSoldier.usAnimState == AnimationStates.END_OPEN_DOOR
                || pSoldier.usAnimState == AnimationStates.END_OPEN_LOCKED_DOOR
                || pSoldier.usAnimState == AnimationStates.ADJACENT_GET_ITEM
                || pSoldier.usAnimState == AnimationStates.DROP_ADJACENT_OBJECT
                || pSoldier.usAnimState == AnimationStates.OPEN_DOOR_CROUCHED
                || pSoldier.usAnimState == AnimationStates.BEGIN_OPENSTRUCT_CROUCHED
                || pSoldier.usAnimState == AnimationStates.CLOSE_DOOR_CROUCHED
                || pSoldier.usAnimState == AnimationStates.OPEN_DOOR_CROUCHED
                || pSoldier.usAnimState == AnimationStates.OPEN_STRUCT_CROUCHED
                || pSoldier.usAnimState == AnimationStates.END_OPENSTRUCT_CROUCHED
                || pSoldier.usAnimState == AnimationStates.END_OPEN_DOOR_CROUCHED
                || pSoldier.usAnimState == AnimationStates.END_OPEN_LOCKED_DOOR_CROUCHED
                || pSoldier.usAnimState == AnimationStates.END_OPENSTRUCT_LOCKED_CROUCHED
                || pSoldier.usAnimState == AnimationStates.BEGIN_OPENSTRUCT)
            {
                return true;
            }
        }

        return false;
    }

    void GotoHeigherStance(SOLDIERTYPE pSoldier)
    {
        bool fNearHeigherLevel;

        switch (Globals.gAnimControl[pSoldier.usAnimState].ubEndHeight)
        {
            case AnimationHeights.ANIM_STAND:

                // Nowhere
                // Try to climb
                this.GetMercClimbDirection(pSoldier.ubID, out bool fNearLowerLevel, out fNearHeigherLevel);

                if (fNearHeigherLevel)
                {
                    //                    BeginSoldierClimbUpRoof(pSoldier);
                }
                break;

            case AnimationHeights.ANIM_CROUCH:

                TurnBasedInput.HandleStanceChangeFromUIKeys(AnimationHeights.ANIM_STAND);
                break;

            case AnimationHeights.ANIM_PRONE:

                TurnBasedInput.HandleStanceChangeFromUIKeys(AnimationHeights.ANIM_CROUCH);
                break;
        }
    }


    void GotoLowerStance(SOLDIERTYPE pSoldier)
    {
        bool fNearLowerLevel;


        switch (Globals.gAnimControl[pSoldier.usAnimState].ubEndHeight)
        {
            case AnimationHeights.ANIM_STAND:

                TurnBasedInput.HandleStanceChangeFromUIKeys(AnimationHeights.ANIM_CROUCH);
                break;

            case AnimationHeights.ANIM_CROUCH:

                TurnBasedInput.HandleStanceChangeFromUIKeys(AnimationHeights.ANIM_PRONE);
                break;

            case AnimationHeights.ANIM_PRONE:

                // Nowhere
                // Try to climb
                this.GetMercClimbDirection(pSoldier.ubID, out fNearLowerLevel, out bool fNearHeigherLevel);

                if (fNearLowerLevel)
                {
//                    BeginSoldierClimbDownRoof(pSoldier);
                }
                break;
        }
    }

    static int sOldHeight = 0;

    void SetInterfaceHeightLevel()
    {
        int sHeight;
        int sGridNo;

        if (Globals.gfBasement || Globals.gfCaves)
        {
            Globals.gsRenderHeight = 0;
            sOldHeight = 0;

            return;
        }


        // ATE: Use an entry point to determine what height to use....
        if (Globals.gMapInformation.sNorthGridNo != -1)
        {
            sGridNo = Globals.gMapInformation.sNorthGridNo;
        }
        else if (Globals.gMapInformation.sEastGridNo != -1)
        {
            sGridNo = Globals.gMapInformation.sEastGridNo;
        }
        else if (Globals.gMapInformation.sSouthGridNo != -1)
        {
            sGridNo = Globals.gMapInformation.sSouthGridNo;
        }
        else if (Globals.gMapInformation.sWestGridNo != -1)
        {
            sGridNo = Globals.gMapInformation.sWestGridNo;
        }
        else
        {
            //Assert(0);
            return;
        }


        sHeight = Globals.gpWorldLevelData[sGridNo].sHeight;

        if (sHeight != sOldHeight)
        {
            Globals.gsRenderHeight = sHeight;

            if (Globals.gsInterfaceLevel > 0)
            {
                Globals.gsRenderHeight += Globals.ROOF_LEVEL_HEIGHT;
            }

            RenderWorld.SetRenderFlags(RenderingFlags.FULL);
            PathAI.ErasePath(false);

            sOldHeight = sHeight;
        }
    }

    static bool fOldOnValidGuy = false;

    public static bool ValidQuickExchangePosition()
    {
        SOLDIERTYPE? pOverSoldier;
        int sDistVisible = 0;
        bool fOnValidGuy = false;

        // Check if we over a civ
        if (Globals.gfUIFullTargetFound)
        {
            pOverSoldier = Globals.MercPtrs[Globals.gusUIFullTargetID];

            //KM: Replaced this older if statement for the new one which allows exchanging with militia
            //if ( ( pOverSoldier.bSide != gbPlayerNum ) && pOverSoldier.bNeutral  )
            if ((pOverSoldier.bTeam != Globals.gbPlayerNum && pOverSoldier.bNeutral > 0) || (pOverSoldier.bTeam == TEAM.MILITIA_TEAM && pOverSoldier.bSide == 0))
            {
                // hehe - don't allow animals to exchange places
                if (!pOverSoldier.uiStatusFlags.HasFlag(SOLDIER.ANIMAL))
                {
                    // OK, we have a civ , now check if they are near selected guy.....
                    if (Overhead.GetSoldier(out SOLDIERTYPE? pSoldier, Globals.gusSelectedSoldier))
                    {
                        if (IsometricUtils.PythSpacesAway(pSoldier.sGridNo, pOverSoldier.sGridNo) == 1)
                        {
                            // Check if we have LOS to them....
                            sDistVisible = OppList.DistanceVisible(pSoldier, WorldDirections.DIRECTION_IRRELEVANT, WorldDirections.DIRECTION_IRRELEVANT, pOverSoldier.sGridNo, pOverSoldier.bLevel);

                            if (LOS.SoldierTo3DLocationLineOfSightTest(pSoldier, pOverSoldier.sGridNo, pOverSoldier.bLevel, (byte)3, (byte)sDistVisible, 1))
                            {
                                // ATE:
                                // Check that the path is good!
//                                if (FindBestPath(pSoldier, pOverSoldier.sGridNo, pSoldier.bLevel, pSoldier.usUIMovementMode, PlotPathDefines.NO_COPYROUTE, PlotPathDefines.PATH_IGNORE_PERSON_AT_DEST) == 1)
//                                {
//                                    fOnValidGuy = true;
//                                }
                            }
                        }
                    }
                }
            }
        }

        if (fOldOnValidGuy != fOnValidGuy)
        {
            // Update timer....
            // ATE: Adjust clock for automatic swapping so that the 'feel' is there....
            Globals.guiUIInterfaceSwapCursorsTime = Globals.GetJA2Clock();
            // Default it!
            Globals.gfOKForExchangeCursor = true;
        }

        // Update old value.....
        fOldOnValidGuy = fOnValidGuy;

        if (!Globals.gfOKForExchangeCursor)
        {
            fOnValidGuy = false;
        }

        return fOnValidGuy;
    }


    // This function contains the logic for allowing the player
    // to jump over people.
    bool IsValidJumpLocation(SOLDIERTYPE? pSoldier, int sGridNo, bool fCheckForPath)
    {
        int[] sFourGrids = new int[4];
        int sDistance = 0, sSpot, sIntSpot;
        WorldDirections[] sDirs = { WorldDirections.NORTH, WorldDirections.EAST, WorldDirections.SOUTH, WorldDirections.WEST };
        int cnt;
        int ubGuyThere;
        int ubMovementCost;
        int iDoorGridNo;

        // First check that action point cost is zero so far
        // ie: NO PATH!
        if (Globals.gsCurrentActionPoints != 0 && fCheckForPath)
        {
            return false;
        }

        // Loop through positions...
        for (cnt = 0; cnt < 4; cnt++)
        {
            // MOVE OUT TWO DIRECTIONS
            sIntSpot = IsometricUtils.NewGridNo(sGridNo, IsometricUtils.DirectionInc((int)sDirs[cnt]));

            // ATE: Check our movement costs for going through walls!
            ubMovementCost = Globals.gubWorldMovementCosts[sIntSpot, (int)sDirs[cnt], pSoldier.bLevel];
            if (TRAVELCOST.IS_TRAVELCOST_DOOR(ubMovementCost))
            {
//                ubMovementCost = PathAI.DoorTravelCost(pSoldier, sIntSpot, ubMovementCost, (bool)(pSoldier.bTeam == Globals.gbPlayerNum), out iDoorGridNo);
            }

            // If we have hit an obstacle, STOP HERE
            if (ubMovementCost >= TRAVELCOST.BLOCKED)
            {
                // no good, continue
                continue;
            }


            // TWICE AS FAR!
            sFourGrids[cnt] = sSpot = IsometricUtils.NewGridNo(sIntSpot, IsometricUtils.DirectionInc((int)sDirs[cnt]));

            // Is the soldier we're looking at here?
            ubGuyThere = WorldManager.WhoIsThere2(sSpot, pSoldier.bLevel);

            // Alright folks, here we are!
            if (ubGuyThere == pSoldier.ubID)
            {
                // Double check OK destination......
                if (Overhead.NewOKDestination(pSoldier, sGridNo, true, Globals.gsInterfaceLevel))
                {
                    // If the soldier in the middle of doing stuff?
                    if (!pSoldier.fTurningUntilDone)
                    {
                        // OK, NOW check if there is a guy in between us
                        // 
                        // 
                        ubGuyThere = WorldManager.WhoIsThere2(sIntSpot, pSoldier.bLevel);

                        // Is there a guy and is he prone?
                        if (ubGuyThere != Globals.NOBODY && ubGuyThere != pSoldier.ubID && Globals.gAnimControl[Globals.MercPtrs[ubGuyThere].usAnimState].ubHeight == AnimationHeights.ANIM_PRONE)
                        {
                            // It's a GO!
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
}

public enum MOUSE
{
    MOVING_IN_TILE = 0x00000001,
    MOVING = 0x00000002,
    MOVING_NEW_TILE = 0x00000004,
    STATIONARY = 0x00000008,
}

public enum UI_MODE
{
    DONT_CHANGEMODE,
    IDLE_MODE,
    MOVE_MODE,
    ACTION_MODE,
    MENU_MODE,
    POPUP_MODE,
    CONFIRM_MOVE_MODE,
    ADJUST_STANCE_MODE,
    CONFIRM_ACTION_MODE,
    HANDCURSOR_MODE,
    GETTINGITEM_MODE,
    ENEMYS_TURN_MODE,
    LOOKCURSOR_MODE,
    TALKINGMENU_MODE,
    TALKCURSOR_MODE,
    LOCKUI_MODE,
    OPENDOOR_MENU_MODE,
    LOCKOURTURN_UI_MODE,
    EXITSECTORMENU_MODE,
    RUBBERBAND_MODE,
    JUMPOVER_MODE,
}

public delegate ScreenName UI_HANDLEFNC(UI_EVENT ui_event);

public class UI_EVENT
{
    public UI_EVENT(
        UIEVENT uiFlags,
        UI_MODE ChangeToUIMode,
        Func<UI_EVENT, ScreenName> HandleEvent,
        bool fFirstTime,
        bool fDoneMenu,
        UI_MODE uiMenuPreviousMode,
        int[] uiParams)// [3];
    {
        this.uiFlags = uiFlags;
        this.ChangeToUIMode = ChangeToUIMode;
        this.HandleEvent = HandleEvent;
        this.fFirstTime = fFirstTime;
        this.fDoneMenu = fDoneMenu;
        this.uiMenuPreviousMode = uiMenuPreviousMode;
        this.uiParams = uiParams;
    }

    public UIEVENT uiFlags { get; set; }
    public UI_MODE ChangeToUIMode { get; set; }
    public Func<UI_EVENT, ScreenName> HandleEvent { get; set; }
    public bool fFirstTime { get; set; }
    public bool fDoneMenu { get; set; }
    public UI_MODE uiMenuPreviousMode { get; set; }
    public int[] uiParams { get; set; }
}

public enum UI_EVENT_DEFINES
{
    I_DO_NOTHING,
    I_EXIT,
    I_NEW_MERC,
    I_NEW_BADMERC,
    I_SELECT_MERC,
    I_ENTER_EDIT_MODE,
    I_ENTER_PALEDIT_MODE,
    I_ENDTURN,
    I_TESTHIT,
    I_CHANGELEVEL,
    I_ON_TERRAIN,
    I_CHANGE_TO_IDLE,
    I_LOADLEVEL,
    I_SOLDIERDEBUG,
    I_LOSDEBUG,
    I_LEVELNODEDEBUG,
    I_GOTODEMOMODE,
    I_LOADFIRSTLEVEL,
    I_LOADSECONDLEVEL,
    I_LOADTHIRDLEVEL,
    I_LOADFOURTHLEVEL,
    I_LOADFIFTHLEVEL,

    ET_ON_TERRAIN,
    ET_ENDENEMYS_TURN,

    M_ON_TERRAIN,
    M_CHANGE_TO_ACTION,
    M_CHANGE_TO_HANDMODE,
    M_CYCLE_MOVEMENT,
    M_CYCLE_MOVE_ALL,
    M_CHANGE_TO_ADJPOS_MODE,

    POPUP_DOMESSAGE,

    A_ON_TERRAIN,
    A_CHANGE_TO_MOVE,
    A_CHANGE_TO_CONFIM_ACTION,
    A_END_ACTION,
    U_MOVEMENT_MENU,
    U_POSITION_MENU,

    C_WAIT_FOR_CONFIRM,
    C_MOVE_MERC,
    C_ON_TERRAIN,

    PADJ_ADJUST_STANCE,

    CA_ON_TERRAIN,
    CA_MERC_SHOOT,
    CA_END_CONFIRM_ACTION,

    HC_ON_TERRAIN,

    G_GETTINGITEM,

    LC_ON_TERRAIN,
    LC_CHANGE_TO_LOOK,
    LC_LOOK,

    TA_TALKINGMENU,

    T_ON_TERRAIN,
    T_CHANGE_TO_TALKING,

    LU_ON_TERRAIN,
    LU_BEGINUILOCK,
    LU_ENDUILOCK,

    OP_OPENDOORMENU,

    LA_ON_TERRAIN,
    LA_BEGINUIOURTURNLOCK,
    LA_ENDUIOUTURNLOCK,

    EX_EXITSECTORMENU,

    RB_ON_TERRAIN,

    JP_ON_TERRAIN,
    JP_JUMP,

    // Should be == 63
    NUM_UI_EVENTS,
}

public enum MOVEUI_TARGET
{
    INTTILES = 1,
    ITEMS = 2,
    MERCS = 3,
    MERCSFORAID = 5,
    WIREFENCE,
    BOMB = 7,
    STEAL = 8,
    REPAIR = 9,
    JAR = 10,
    CAN = 11,
    REFUEL = 12,

    MOVEUI_RETURN_ON_TARGET_MERC = 1,
}
