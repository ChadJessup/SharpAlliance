﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Screens;
using Veldrid.OpenGLBinding;

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

public class HandleUI
{
    private const int MAX_ON_DUTY_SOLDIERS = 6;
    // LOCAL DEFINES
    private const int UIEVENT_SINGLEEVENT = 0x00000002;
    private const int UIEVENT_SNAPMOUSE = 0x00000008;

    private const int GO_MOVE_ONE = 40;
    private const int GO_MOVE_TWO = 80;
    private const int GO_MOVE_THREE = 100;

    int gsTreeRevealXPos, gsTreeRevealYPos;

    //extern bool gfExitDebugScreen;
    //extern byte gCurDebugPage;
    //extern bool gfGetNewPathThroughPeople;
    //extern bool gfIgnoreOnSelectedGuy;
    //extern bool gfInOpenDoorMenu;

    public HandleUI()
    {
        gEvents = new()
        {
            new(0, UI_MODE.IDLE_MODE, UIHandleIDoNothing, false, false, 0, new int[] { 0, 0, 0 }),
            new(0, UI_MODE.IDLE_MODE, UIHandleExit, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, UIHandleNewMerc, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, UIHandleNewBadMerc, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.MOVE_MODE, UIHandleSelectMerc, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.MOVE_MODE, UIHandleEnterEditMode, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.MOVE_MODE, UIHandleEnterPalEditMode, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, UIHandleEndTurn, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, UIHandleTestHit, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.MOVE_MODE, UIHandleChangeLevel, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.IDLE_MODE, UIHandleIOnTerrain, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.IDLE_MODE, UIHandleIChangeToIdle, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.IDLE_MODE, UIHandleILoadLevel, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, UIHandleISoldierDebug, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, UIHandleILOSDebug, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, UIHandleILevelNodeDebug, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, UIHandleIGotoDemoMode, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, UIHandleILoadFirstLevel, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, UIHandleILoadSecondLevel, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, UIHandleILoadThirdLevel, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, UIHandleILoadFourthLevel, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.DONT_CHANGEMODE, UIHandleILoadFifthLevel, false, false, 0, new int[] { 0, 0, 0 }),
            new(0,UI_MODE.ENEMYS_TURN_MODE, UIHandleIETOnTerrain, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT,UI_MODE.MOVE_MODE, UIHandleIETEndTurn, false, false, 0, new int[] { 0, 0, 0 }),
            new(0,UI_MODE.MOVE_MODE, UIHandleMOnTerrain, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.ACTION_MODE,                    UIHandleMChangeToAction, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.HANDCURSOR_MODE,            UIHandleMChangeToHandMode, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.MOVE_MODE,                      UIHandleMCycleMovement, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.CONFIRM_MOVE_MODE,      UIHandleMCycleMoveAll, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SNAPMOUSE, UI_MODE.ADJUST_STANCE_MODE,     UIHandleMAdjustStanceMode, false, false, 0, new int[] { 0, 0, 0 }),
            new(0, UI_MODE.POPUP_MODE, UIHandlePOPUPMSG, false, false, 0, new int[] { 0, 0, 0 }),
            new(0, UI_MODE.ACTION_MODE, UIHandleAOnTerrain, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.MOVE_MODE,                      UIHandleAChangeToMove, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.CONFIRM_ACTION_MODE,    UIHandleAChangeToConfirmAction, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.MOVE_MODE,                      UIHandleAEndAction, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SNAPMOUSE, UI_MODE.MENU_MODE,                      UIHandleMovementMenu, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SNAPMOUSE, UI_MODE.MENU_MODE,                      UIHandlePositionMenu, false, false, 0, new int[] { 0, 0, 0 }),
            new(0, UI_MODE.CONFIRM_MOVE_MODE,    UIHandleCWait, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.MOVE_MODE,                      UIHandleCMoveMerc, false, false, 0, new int[] { 0, 0, 0 }),
            new(0, UI_MODE.CONFIRM_MOVE_MODE,      UIHandleCOnTerrain, false, false, 0, new int[] { 0, 0, 0 }),
            new(0, UI_MODE.MOVE_MODE,                      UIHandlePADJAdjustStance, false, false, 0, new int[] { 0, 0, 0 }),
            new(0, UI_MODE.CONFIRM_ACTION_MODE,    UIHandleCAOnTerrain, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.ACTION_MODE,                    UIHandleCAMercShoot, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.ACTION_MODE,                    UIHandleCAEndConfirmAction, false, false, 0, new int[] { 0, 0, 0 }),
            new(0, UI_MODE.HANDCURSOR_MODE,            UIHandleHCOnTerrain, false, false, 0, new int[] { 0, 0, 0 }),
            new(0, UI_MODE.GETTINGITEM_MODE,           UIHandleHCGettingItem, false, false, 0, new int[] { 0, 0, 0 }),
            new(0, UI_MODE.LOOKCURSOR_MODE,            UIHandleLCOnTerrain, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.LOOKCURSOR_MODE,            UIHandleLCChangeToLook, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.MOVE_MODE,                      UIHandleLCLook, false, false, 0, new int[] { 0, 0, 0 }),
            new(0,UI_MODE.TALKINGMENU_MODE,           UIHandleTATalkingMenu, false, false, 0, new int[] { 0, 0, 0 }),
            new(0,UI_MODE.TALKCURSOR_MODE,            UIHandleTOnTerrain, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT,UI_MODE.TALKCURSOR_MODE,            UIHandleTChangeToTalking, false, false, 0, new int[] { 0, 0, 0 }),
            new(0,UI_MODE.LOCKUI_MODE,                    UIHandleLUIOnTerrain, false, false, 0, new int[] { 0, 0, 0 }),
            new(0,UI_MODE.LOCKUI_MODE,                    UIHandleLUIBeginLock, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT,UI_MODE.MOVE_MODE,                      UIHandleLUIEndLock, false, false, 0, new int[] { 0, 0, 0 }),
            new(0, UI_MODE.OPENDOOR_MENU_MODE,     UIHandleOpenDoorMenu, false, false, 0, new int[] { 0, 0, 0 }),
            new(0, UI_MODE.LOCKOURTURN_UI_MODE,    UIHandleLAOnTerrain, false, false, 0, new int[] { 0, 0, 0 }),
            new(0, UI_MODE.LOCKOURTURN_UI_MODE,    UIHandleLABeginLockOurTurn, false, false, 0, new int[] { 0, 0, 0 }),
            new(UIEVENT_SINGLEEVENT, UI_MODE.MOVE_MODE,                      UIHandleLAEndLockOurTurn, false, false, 0, new int[] { 0, 0, 0 }),
            new(0, UI_MODE.EXITSECTORMENU_MODE,    UIHandleEXExitSectorMenu, false, false, 0, new int[] { 0, 0, 0 }),
            new(0, UI_MODE.RUBBERBAND_MODE,            UIHandleRubberBandOnTerrain, false, false, 0, new int[] { 0, 0, 0 }),
            new(0, UI_MODE.JUMPOVER_MODE,              UIHandleJumpOverOnTerrain, false, false, 0, new int[] { 0, 0, 0 }),
            new(0, UI_MODE.MOVE_MODE, UIHandleJumpOver, false, false, 0, new int[] { 0, 0, 0 }),
        };
    }

    SOLDIERTYPE? gpRequesterMerc = null;
    SOLDIERTYPE? gpRequesterTargetMerc = null;
    int gsRequesterGridNo;
    int gsOverItemsGridNo = (int)IsometricDefines.NOWHERE;
    int gsOverItemsLevel = 0;
    bool gfUIInterfaceSetBusy = false;
    uint guiUIInterfaceBusyTime = 0;

    bool gfTacticalForceNoCursor = false;
    LEVELNODE? gpInvTileThatCausedMoveConfirm = null;
    bool gfResetUIMovementOptimization = false;
    bool gfResetUIItemCursorOptimization = false;
    bool gfBeginVehicleCursor = false;
    uint gsOutOfRangeGridNo = (int)IsometricDefines.NOWHERE;
    byte gubOutOfRangeMerc = OverheadTypes.NOBODY;
    bool gfOKForExchangeCursor = false;
    uint guiUIInterfaceSwapCursorsTime = 0;
    int gsJumpOverGridNo = 0;

    List<UI_EVENT> gEvents = new();

    UI_MODE gCurrentUIMode = IDLE_MODE;
    UI_MODE gOldUIMode = IDLE_MODE;
    uint guiCurrentEvent = I_DO_NOTHING;
    uint guiOldEvent = I_DO_NOTHING;
    uint guiCurrentUICursor = NO_UICURSOR;
    uint guiNewUICursor = NORMAL_SNAPUICURSOR;
    uint guiPendingOverrideEvent = I_DO_NOTHING;
    uint gusSavedMouseX;
    uint gusSavedMouseY;
    UIKEYBOARD_HOOK gUIKeyboardHook = null;
    bool gUIActionModeChangeDueToMouseOver = false;

    bool gfDisplayTimerCursor = false;
    uint guiTimerCursorID = 0;
    uint guiTimerLastUpdate = 0;
    uint guiTimerCursorDelay = 0;


    int gzLocation[20];
    bool gfLocation = false;

    int gzIntTileLocation[20];
    bool gfUIIntTileLocation;

    int gzIntTileLocation2[20];
    bool gfUIIntTileLocation2;

    MOUSE_REGION gDisableRegion;
    bool gfDisableRegionActive = false;

    MOUSE_REGION gUserTurnRegion;
    bool gfUserTurnRegionActive = false;


    // For use with mouse button query routines
    bool fRightButtonDown = false;
    bool fLeftButtonDown = false;
    bool fIgnoreLeftUp = false;

    bool gUITargetReady = false;
    bool gUITargetShotWaiting = false;
    uint gsUITargetShotGridNo = (int)IsometricDefines.NOWHERE;
    bool gUIUseReverse = false;

    SGPRect gRubberBandRect = { 0, 0, 0, 0 };
    bool gRubberBandActive = false;
    bool gfIgnoreOnSelectedGuy = false;
    bool gfViewPortAdjustedForSouth = false;

    int guiCreateGuyIndex = 0;
    // Temp values for placing bad guys
    int guiCreateBadGuyIndex = 8;

    // FLAGS
    // These flags are set for a single frame execution and then are reset for the next iteration. 
    bool gfUIDisplayActionPoints = false;
    bool gfUIDisplayActionPointsInvalid = false;
    bool gfUIDisplayActionPointsBlack = false;
    bool gfUIDisplayActionPointsCenter = false;

    int gUIDisplayActionPointsOffY = 0;
    int gUIDisplayActionPointsOffX = 0;
    bool gfUIDoNotHighlightSelMerc = false;
    bool gfUIHandleSelection = false;
    bool gfUIHandleSelectionAboveGuy = false;
    bool gfUIInDeadlock = false;
    byte gUIDeadlockedSoldier = OverheadTypes.NOBODY;
    bool gfUIHandleShowMoveGrid = false;
    uint gsUIHandleShowMoveGridLocation = (int)IsometricDefines.NOWHERE;
    bool gfUIOverItemPool = false;
    int gfUIOverItemPoolGridNo = 0;
    int gsCurrentActionPoints = 1;
    bool gfUIHandlePhysicsTrajectory = false;
    bool gfUIMouseOnValidCatcher = false;
    byte gubUIValidCatcherID = 0;



    bool gfUIConfirmExitArrows = false;

    bool gfUIShowCurIntTile = false;

    bool gfUIWaitingForUserSpeechAdvance = false;        // Waiting for key input/mouse click to advance speech
    bool gfUIKeyCheatModeOn = false;     // Sets cool cheat keys on
    bool gfUIAllMoveOn = false;      // Sets to all move
    bool gfUICanBeginAllMoveCycle = false;       // GEts set so we know that the next right-click is a move-call inc\stead of a movement cycle through

    int gsSelectedGridNo = 0;
    int gsSelectedLevel = I_GROUND_LEVEL;
    int gsSelectedGuy = NO_SOLDIER;

    bool gfUIDisplayDamage = false;
    byte gbDamage = 0;
    uint gsDamageGridNo = 0;

    bool gfUIRefreshArrows = false;


    // Thse flags are not re-set after each frame
    bool gfPlotNewMovement = false;
    bool gfPlotNewMovementNOCOST = false;
    uint guiShowUPDownArrows = ARROWS_HIDE_UP | ARROWS_HIDE_DOWN;
    byte gbAdjustStanceDiff = 0;
    byte gbClimbID = 0;

    bool gfUIShowExitEast = false;
    bool gfUIShowExitWest = false;
    bool gfUIShowExitNorth = false;
    bool gfUIShowExitSouth = false;
    bool gfUIShowExitExitGrid = false;

    bool gfUINewStateForIntTile = false;

    bool gfUIForceReExamineCursorData = false;

    // MAIN TACTICAL UI HANDLER
    static LEVELNODE? pOldIntTile = null;

    ScreenName HandleTacticalUI()
    {
        ScreenName ReturnVal = ScreenName.GAME_SCREEN;
        uint uiNewEvent;
        uint usMapPos;
        LEVELNODE* pIntTile;


        // RESET FLAGS
        gfUIDisplayActionPoints = false;
        gfUIDisplayActionPointsInvalid = false;
        gfUIDisplayActionPointsBlack = false;
        gfUIDisplayActionPointsCenter = false;
        gfUIDoNotHighlightSelMerc = false;
        gfUIHandleSelection = NO_GUY_SELECTION;
        gfUIHandleSelectionAboveGuy = false;
        gfUIDisplayDamage = false;
        guiShowUPDownArrows = ARROWS_HIDE_UP | ARROWS_HIDE_DOWN;
        gfUIBodyHitLocation = false;
        gfUIIntTileLocation = false;
        gfUIIntTileLocation2 = false;
        //gfUIForceReExamineCursorData		= false;
        gfUINewStateForIntTile = false;
        gfUIShowExitExitGrid = false;
        gfUIOverItemPool = false;
        gfUIHandlePhysicsTrajectory = false;
        gfUIMouseOnValidCatcher = false;
        gfIgnoreOnSelectedGuy = false;

        // Set old event value
        guiOldEvent = uiNewEvent = guiCurrentEvent;

        if (gfUIInterfaceSetBusy)
        {
            if ((GetJA2Clock() - guiUIInterfaceBusyTime) > 25000)
            {
                gfUIInterfaceSetBusy = false;

                //UNLOCK UI
                UnSetUIBusy((byte)gusSelectedSoldier);

                // Decrease global busy  counter...
                gTacticalStatus.ubAttackBusyCount = 0;
                DebugMsg(TOPIC_JA2, DBG_LEVEL_3, String("Setting attack busy count to 0 due to ending AI lock"));

                guiPendingOverrideEvent = LU_ENDUILOCK;
                UIHandleLUIEndLock(null);
            }
        }

        if ((GetJA2Clock() - guiUIInterfaceSwapCursorsTime) > 1000)
        {
            gfOKForExchangeCursor = !gfOKForExchangeCursor;
            guiUIInterfaceSwapCursorsTime = GetJA2Clock();
        }

        // OK, do a check for on an int tile...
        pIntTile = GetCurInteractiveTile();

        if (pIntTile != pOldIntTile)
        {
            gfUINewStateForIntTile = true;

            pOldIntTile = pIntTile;
        }

        if (guiPendingOverrideEvent == I_DO_NOTHING)
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
            if ((gTacticalStatus.uiFlags & REALTIME) || !(gTacticalStatus.uiFlags & INCOMBAT))
            {
                // FROM MOUSE POSITION
                GetRTMousePositionInput(&uiNewEvent);
                // FROM KEYBOARD POLLING
                GetPolledKeyboardInput(&uiNewEvent);
                // FROM MOUSE CLICKS
                GetRTMouseButtonInput(&uiNewEvent);
                // FROM KEYBOARD
                GetKeyboardInput(&uiNewEvent);

            }
            else
            {
                // FROM MOUSE POSITION
                GetTBMousePositionInput(&uiNewEvent);
                // FROM KEYBOARD POLLING
                GetPolledKeyboardInput(&uiNewEvent);
                // FROM MOUSE CLICKS
                GetTBMouseButtonInput(&uiNewEvent);
                // FROM KEYBOARD
                GetKeyboardInput(&uiNewEvent);
            }


        }
        else
        {
            uiNewEvent = guiPendingOverrideEvent;
            guiPendingOverrideEvent = I_DO_NOTHING;
        }

        if (HandleItemPickupMenu())
        {
            uiNewEvent = A_CHANGE_TO_MOVE;
        }

        // Set Current event to new one!
        guiCurrentEvent = uiNewEvent;

        //ATE: New! Get flags for over soldier or not...
        gfUIFullTargetFound = false;
        gfUISelectiveTargetFound = false;

        if (GetMouseMapPos(&usMapPos))
        {
            // Look for soldier full
            if (FindSoldier(usMapPos, &gusUIFullTargetID, &guiUIFullTargetFlags, (FINDSOLDIERSAMELEVEL(gsInterfaceLevel))))
            {
                gfUIFullTargetFound = true;
            }

            // Look for soldier selective
            if (FindSoldier(usMapPos, &gusUISelectiveTargetID, &guiUISelectiveTargetFlags, FINDSOLDIERSELECTIVESAMELEVEL(gsInterfaceLevel)))
            {
                gfUISelectiveTargetFound = true;
            }

        }


        // Check if current event has changed and clear event if so, to prepare it for execution
        // Clearing it does things like set first time flag, param variavles, etc
        if (uiNewEvent != guiOldEvent)
        {
            // Snap mouse back if it's that type
            if (gEvents[guiOldEvent].uiFlags & UIEVENT_SNAPMOUSE)
            {
                SimulateMouseMovement((uint)gusSavedMouseX, (uint)gusSavedMouseY);
            }

            ClearEvent(&gEvents[uiNewEvent]);
        }

        // Restore not scrolling from stance adjust....
        if (gOldUIMode == ADJUST_STANCE_MODE)
        {
            gfIgnoreScrolling = false;
        }

        // IF this event is of type snap mouse, save position
        if (gEvents[uiNewEvent].uiFlags & UIEVENT_SNAPMOUSE && gEvents[uiNewEvent].fFirstTime)
        {
            // Save mouse position
            gusSavedMouseX = gusMouseXPos;
            gusSavedMouseY = gusMouseYPos;
        }

        // HANDLE UI EVENT
        ReturnVal = gEvents[uiNewEvent].HandleEvent(&(gEvents[uiNewEvent]));

        if (gfInOpenDoorMenu)
        {
            return (ReturnVal);
        }

        // Set first time flag to false, now that it has been executed
        gEvents[uiNewEvent].fFirstTime = false;

        // Check if UI mode has changed from previous event
        if (gEvents[uiNewEvent].ChangeToUIMode != gCurrentUIMode && (gEvents[uiNewEvent].ChangeToUIMode != DONT_CHANGEMODE))
        {
            gEvents[uiNewEvent].uiMenuPreviousMode = gCurrentUIMode;

            gOldUIMode = gCurrentUIMode;

            gCurrentUIMode = gEvents[uiNewEvent].ChangeToUIMode;

            // CHANGE MODE - DO SPECIAL THINGS IF WE ENTER THIS MODE
            switch (gCurrentUIMode)
            {
                case ACTION_MODE:
                    ErasePath(true);
                    break;
            }

        }

        // Check if menu event is done and if so set to privious mode
        // This is needed to hook into the interface stuff which sets the fDoneMenu flag
        if (gEvents[uiNewEvent].fDoneMenu == true)
        {
            if (gCurrentUIMode == MENU_MODE || gCurrentUIMode == POPUP_MODE || gCurrentUIMode == LOOKCURSOR_MODE)
            {
                gCurrentUIMode = gEvents[uiNewEvent].uiMenuPreviousMode;
            }
        }
        // Check to return to privious mode
        // If the event is a single event, return to previous
        if (gEvents[uiNewEvent].uiFlags & UIEVENT_SINGLEEVENT)
        {
            // ATE: OK - don't revert to single event if our mouse is not
            // in viewport - rather use m_on_t event
            if ((gViewportRegion.uiFlags & MSYS_MOUSE_IN_AREA))
            {
                guiCurrentEvent = guiOldEvent;
            }
            else
            {
                // ATE: Check first that some modes are met....
                if (gCurrentUIMode != HANDCURSOR_MODE && gCurrentUIMode != LOOKCURSOR_MODE && gCurrentUIMode != TALKCURSOR_MODE)
                {
                    guiCurrentEvent = M_ON_TERRAIN;
                }
            }
        }

        // Donot display APs if not in combat
        if (!(gTacticalStatus.uiFlags & INCOMBAT) || (gTacticalStatus.uiFlags & REALTIME))
        {
            gfUIDisplayActionPoints = false;
        }


        // Will set the cursor but only if different
        SetUIMouseCursor();

        // ATE: Check to reset selected guys....
        if (gTacticalStatus.fAtLeastOneGuyOnMultiSelect)
        {
            // If not in MOVE_MODE, CONFIRM_MOVE_MODE, RUBBERBAND_MODE, stop....
            if (gCurrentUIMode != MOVE_MODE && gCurrentUIMode != CONFIRM_MOVE_MODE && gCurrentUIMode != RUBBERBAND_MODE && gCurrentUIMode != ADJUST_STANCE_MODE && gCurrentUIMode != TALKCURSOR_MODE && gCurrentUIMode != LOOKCURSOR_MODE)
            {
                ResetMultiSelection();
            }
        }


        return (ReturnVal);
    }

    static int sOldExitGridNo = (int)IsometricDefines.NOWHERE;
    static bool fOkForExit = false;

    void SetUIMouseCursor()
    {
        uint uiCursorFlags;
        uint uiTraverseTimeInMinutes;
        bool fForceUpdateNewCursor = false;
        bool fUpdateNewCursor = true;

        // Check if we moved from confirm mode on exit arrows
        // If not in move mode, return!
        if (gCurrentUIMode == MOVE_MODE)
        {
            if (gfUIConfirmExitArrows)
            {
                GetCursorMovementFlags(&uiCursorFlags);

                if (uiCursorFlags & MOUSE_MOVING)
                {
                    gfUIConfirmExitArrows = false;
                }
            }

            if (gfUIShowExitEast)
            {
                gfUIDisplayActionPoints = false;
                ErasePath(true);

                if (OKForSectorExit(EAST_STRATEGIC_MOVE, 0, &uiTraverseTimeInMinutes))
                {
                    if (gfUIConfirmExitArrows)
                    {
                        guiNewUICursor = CONFIRM_EXIT_EAST_UICURSOR;
                    }
                    else
                    {
                        guiNewUICursor = EXIT_EAST_UICURSOR;
                    }
                }
                else
                {
                    guiNewUICursor = NOEXIT_EAST_UICURSOR;
                }

                if (gusMouseXPos < 635)
                {
                    gfUIShowExitEast = false;
                }
            }

            if (gfUIShowExitWest)
            {
                gfUIDisplayActionPoints = false;
                ErasePath(true);

                if (OKForSectorExit(WEST_STRATEGIC_MOVE, 0, &uiTraverseTimeInMinutes))
                {
                    if (gfUIConfirmExitArrows)
                    {
                        guiNewUICursor = CONFIRM_EXIT_WEST_UICURSOR;
                    }
                    else
                    {
                        guiNewUICursor = EXIT_WEST_UICURSOR;
                    }
                }
                else
                {
                    guiNewUICursor = NOEXIT_WEST_UICURSOR;
                }

                if (gusMouseXPos > 5)
                {
                    gfUIShowExitWest = false;
                }
            }

            if (gfUIShowExitNorth)
            {
                gfUIDisplayActionPoints = false;
                ErasePath(true);

                if (OKForSectorExit(NORTH_STRATEGIC_MOVE, 0, &uiTraverseTimeInMinutes))
                {
                    if (gfUIConfirmExitArrows)
                    {
                        guiNewUICursor = CONFIRM_EXIT_NORTH_UICURSOR;
                    }
                    else
                    {
                        guiNewUICursor = EXIT_NORTH_UICURSOR;
                    }
                }
                else
                {
                    guiNewUICursor = NOEXIT_NORTH_UICURSOR;
                }

                if (gusMouseYPos > 5)
                {
                    gfUIShowExitNorth = false;
                }
            }


            if (gfUIShowExitSouth)
            {
                gfUIDisplayActionPoints = false;
                ErasePath(true);

                if (OKForSectorExit(SOUTH_STRATEGIC_MOVE, 0, &uiTraverseTimeInMinutes))
                {
                    if (gfUIConfirmExitArrows)
                    {
                        guiNewUICursor = CONFIRM_EXIT_SOUTH_UICURSOR;
                    }
                    else
                    {
                        guiNewUICursor = EXIT_SOUTH_UICURSOR;
                    }
                }
                else
                {
                    guiNewUICursor = NOEXIT_SOUTH_UICURSOR;
                }

                if (gusMouseYPos < 478)
                {
                    gfUIShowExitSouth = false;

                    // Define region for viewport
                    MSYS_RemoveRegion(&gViewportRegion);

                    MSYS_DefineRegion(&gViewportRegion, 0, 0, gsVIEWPORT_END_X, gsVIEWPORT_WINDOW_END_Y, MSYS_PRIORITY_NORMAL,
                                         VIDEO_NO_CURSOR, MSYS_NO_CALLBACK, MSYS_NO_CALLBACK);


                    // Adjust where we blit our cursor!
                    gsGlobalCursorYOffset = 0;
                    SetCurrentCursorFromDatabase(CURSOR_NORMAL);
                }
                else
                {
                    if (gfScrollPending || gfScrollInertia)
                    {

                    }
                    else
                    {
                        // Adjust viewport to edge of screen!
                        // Define region for viewport
                        MSYS_RemoveRegion(&gViewportRegion);
                        MSYS_DefineRegion(&gViewportRegion, 0, 0, gsVIEWPORT_END_X, 480, MSYS_PRIORITY_NORMAL,
                                             VIDEO_NO_CURSOR, MSYS_NO_CALLBACK, MSYS_NO_CALLBACK);

                        gsGlobalCursorYOffset = (480 - gsVIEWPORT_WINDOW_END_Y);
                        SetCurrentCursorFromDatabase(gUICursors[guiNewUICursor].usFreeCursorName);

                        gfViewPortAdjustedForSouth = true;

                    }
                }
            }
            else
            {
                if (gfViewPortAdjustedForSouth)
                {
                    // Define region for viewport
                    MSYS_RemoveRegion(&gViewportRegion);

                    MSYS_DefineRegion(&gViewportRegion, 0, 0, gsVIEWPORT_END_X, gsVIEWPORT_WINDOW_END_Y, MSYS_PRIORITY_NORMAL,
                                         VIDEO_NO_CURSOR, MSYS_NO_CALLBACK, MSYS_NO_CALLBACK);


                    // Adjust where we blit our cursor!
                    gsGlobalCursorYOffset = 0;
                    SetCurrentCursorFromDatabase(CURSOR_NORMAL);

                    gfViewPortAdjustedForSouth = false;
                }
            }

            if (gfUIShowExitExitGrid)
            {
                uint usMapPos;
                byte ubRoomNum;

                gfUIDisplayActionPoints = false;
                ErasePath(true);

                if (GetMouseMapPos(&usMapPos))
                {
                    if (gusSelectedSoldier != OverheadTypes.NOBODY && MercPtrs[gusSelectedSoldier].bLevel == 0)
                    {
                        // ATE: Is this place revealed?
                        if (!InARoom(usMapPos, &ubRoomNum) || (InARoom(usMapPos, &ubRoomNum) && gpWorldLevelData[usMapPos].uiFlags & MAPELEMENT_REVEALED))
                        {
                            if (sOldExitGridNo != usMapPos)
                            {
                                fOkForExit = OKForSectorExit((byte)-1, usMapPos, &uiTraverseTimeInMinutes);
                                sOldExitGridNo = usMapPos;
                            }

                            if (fOkForExit)
                            {
                                if (gfUIConfirmExitArrows)
                                {
                                    guiNewUICursor = CONFIRM_EXIT_GRID_UICURSOR;
                                }
                                else
                                {
                                    guiNewUICursor = EXIT_GRID_UICURSOR;
                                }
                            }
                            else
                            {
                                guiNewUICursor = NOEXIT_GRID_UICURSOR;
                            }
                        }
                    }
                }
            }
            else
            {
                sOldExitGridNo = (int)IsometricDefines.NOWHERE;
            }

        }
        else
        {
            gsGlobalCursorYOffset = 0;
        }

        if (gfDisplayTimerCursor)
        {
            SetUICursor(guiTimerCursorID);

            fUpdateNewCursor = false;

            if ((GetJA2Clock() - guiTimerLastUpdate) > guiTimerCursorDelay)
            {
                gfDisplayTimerCursor = false;

                // OK, timer may be different, update...
                fForceUpdateNewCursor = true;
                fUpdateNewCursor = true;
            }
        }

        if (fUpdateNewCursor)
        {
            if (!gfTacticalForceNoCursor)
            {
                if (guiNewUICursor != guiCurrentUICursor || fForceUpdateNewCursor)
                {
                    SetUICursor(guiNewUICursor);

                    guiCurrentUICursor = guiNewUICursor;
                }
            }
        }
    }

    void SetUIKeyboardHook(UIKEYBOARD_HOOK KeyboardHookFnc)
    {
        gUIKeyboardHook = KeyboardHookFnc;
    }


    void ClearEvent(UI_EVENT pUIEvent)
    {
        //memset(pUIEvent.uiParams, 0, sizeof(pUIEvent.uiParams) );
        pUIEvent.fDoneMenu = false;
        pUIEvent.fFirstTime = true;
        pUIEvent.uiMenuPreviousMode = 0;

    }

    void EndMenuEvent(uint uiEvent)
    {
        gEvents[uiEvent].fDoneMenu = true;
    }

    // HANDLER FUCNTIONS

    ScreenName UIHandleIDoNothing(UI_EVENT pUIEvent)
    {
        guiNewUICursor = NORMAL_SNAPUICURSOR;

        return (ScreenName.GAME_SCREEN);
    }

    ScreenName UIHandleExit(UI_EVENT pUIEvent)
    {
        gfProgramIsRunning = false;
        return (ScreenName.GAME_SCREEN);
    }

    static byte ubTemp = 3;
    static int iSoldierCount = 0;

    ScreenName UIHandleNewMerc(UI_EVENT pUIEvent)
    {
        int usMapPos;
        MERC_HIRE_STRUCT HireMercStruct;
        byte bReturnCode;
        SOLDIERTYPE? pSoldier;


        // Get Grid Corrdinates of mouse
        if (GetMouseMapPos(&usMapPos))
        {
            ubTemp += 2;

            memset(&HireMercStruct, 0, sizeof(MERC_HIRE_STRUCT));

            HireMercStruct.ubProfileID = ubTemp;

            //DEF: temp
            HireMercStruct.sSectorX = gWorldSectorX;
            HireMercStruct.sSectorY = gWorldSectorY;
            HireMercStruct.bSectorZ = gbWorldSectorZ;
            HireMercStruct.ubInsertionCode = INSERTION_CODE_GRIDNO;
            HireMercStruct.usInsertionData = usMapPos;
            HireMercStruct.fCopyProfileItemsOver = true;
            HireMercStruct.iTotalContractLength = 7;

            //specify when the merc should arrive
            HireMercStruct.uiTimeTillMercArrives = 0;

            //if we succesfully hired the merc
            bReturnCode = HireMerc(&HireMercStruct);

            if (bReturnCode == MERC_HIRE_FAILED)
            {
                ScreenMsg(FONT_ORANGE, MSG_BETAVERSION, "Merc hire failed:  Either already hired or dislikes you.");
            }
            else if (bReturnCode == MERC_HIRE_OVER_20_MERCS_HIRED)
            {
                ScreenMsg(FONT_ORANGE, MSG_BETAVERSION, "Can't hire more than 20 mercs.");
            }
            else
            {
                // Get soldier from profile
                pSoldier = FindSoldierByProfileID(ubTemp, false);

                MercArrivesCallback(pSoldier.ubID);
                SelectSoldier(pSoldier.ubID, false, true);
            }

        }
        return (ScreenName.GAME_SCREEN);
    }

    ScreenName UIHandleNewBadMerc(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE? pSoldier;
        uint usMapPos;
        uint usRandom;

        //Get map postion and place the enemy there.
        if (GetMouseMapPos(&usMapPos))
        {
            // Are we an OK dest?
            if (!IsLocationSittable(usMapPos, 0))
            {
                return (ScreenName.GAME_SCREEN);
            }

            usRandom = (uint)Random(10);
            if (usRandom < 4)
                pSoldier = TacticalCreateAdministrator();
            else if (usRandom < 8)
                pSoldier = TacticalCreateArmyTroop();
            else
                pSoldier = TacticalCreateEliteEnemy();

            //Add soldier strategic info, so it doesn't break the counters!
            if (pSoldier)
            {
                if (!gbWorldSectorZ)
                {
                    SECTORINFO* pSector = &SectorInfo[SECTOR(gWorldSectorX, gWorldSectorY)];
                    switch (pSoldier.ubSoldierClass)
                    {
                        case SOLDIER_CLASS_ADMINISTRATOR:
                            pSector.ubNumAdmins++;
                            pSector.ubAdminsInBattle++;
                            break;
                        case SOLDIER_CLASS_ARMY:
                            pSector.ubNumTroops++;
                            pSector.ubTroopsInBattle++;
                            break;
                        case SOLDIER_CLASS_ELITE:
                            pSector.ubNumElites++;
                            pSector.ubElitesInBattle++;
                            break;
                    }
                }
                else
                {
                    UNDERGROUND_SECTORINFO* pSector = FindUnderGroundSector(gWorldSectorX, gWorldSectorY, gbWorldSectorZ);
                    if (pSector)
                    {
                        switch (pSoldier.ubSoldierClass)
                        {
                            case SOLDIER_CLASS_ADMINISTRATOR:
                                pSector.ubNumAdmins++;
                                pSector.ubAdminsInBattle++;
                                break;
                            case SOLDIER_CLASS_ARMY:
                                pSector.ubNumTroops++;
                                pSector.ubTroopsInBattle++;
                                break;
                            case SOLDIER_CLASS_ELITE:
                                pSector.ubNumElites++;
                                pSector.ubElitesInBattle++;
                                break;
                        }
                    }
                }

                pSoldier.ubStrategicInsertionCode = INSERTION_CODE_GRIDNO;
                pSoldier.usStrategicInsertionData = usMapPos;
                UpdateMercInSector(pSoldier, gWorldSectorX, gWorldSectorY, gbWorldSectorZ);
                AllTeamsLookForAll(NO_INTERRUPTS);
            }
        }
        return (ScreenName.GAME_SCREEN);
    }


    ScreenName UIHandleEnterEditMode(UI_EVENT pUIEvent)
    {
        return (ScreenName.EDIT_SCREEN);
    }

    ScreenName UIHandleEnterPalEditMode(UI_EVENT pUIEvent)
    {
        return (ScreenName.PALEDIT_SCREEN);
    }

    ScreenName UIHandleEndTurn(UI_EVENT pUIEvent)
    {
        // CANCEL FROM PLANNING MODE!
        if (InUIPlanMode())
        {
            EndUIPlan();
        }

        // ATE: If we have an item pointer end it!
        CancelItemPointer();

        //ScreenMsg( FONT_MCOLOR_LTYELLOW, MSG_INTERFACE, TacticalStr[ ENDING_TURN ] );

        if (CheckForEndOfCombatMode(false))
        {
            // do nothing...
        }
        else
        {
            if (FileExists("..\\AutoSave.pls") && CanGameBeSaved())
            {
                //Save the game
                guiPreviousOptionScreen = guiCurrentScreen;
                SaveGame(SAVE__END_TURN_NUM, L"End Turn Auto Save");
            }

            // End our turn!
            EndTurn(gbPlayerNum + 1);
        }

        return (ScreenName.GAME_SCREEN);
    }

    ScreenName UIHandleTestHit(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE? pSoldier;
        byte bDamage;

        // CHECK IF WE'RE ON A GUY ( EITHER SELECTED, OURS, OR THEIRS
        if (gfUIFullTargetFound)
        {
            // Get Soldier
            GetSoldier(&pSoldier, gusUIFullTargetID);

            if (_KeyDown(SHIFT))
            {
                pSoldier.bBreath -= 30;

                if (pSoldier.bBreath < 0)
                    pSoldier.bBreath = 0;

                bDamage = 1;
            }
            else
            {
                if (Random(2))
                {
                    bDamage = 20;
                }
                else
                {
                    bDamage = 25;
                }
            }

            gTacticalStatus.ubAttackBusyCount++;

            EVENT_SoldierGotHit(pSoldier, 1, bDamage, 10, pSoldier.bDirection, 320, OverheadTypes.NOBODY, FIRE_WEAPON_NO_SPECIAL, pSoldier.bAimShotLocation, 0, (int)IsometricDefines.NOWHERE);

        }
        return ScreenName.GAME_SCREEN;
    }

    void ChangeInterfaceLevel(int sLevel)
    {
        // Only if different!
        if (sLevel == gsInterfaceLevel)
        {
            return;
        }

        gsInterfaceLevel = sLevel;

        if (gsInterfaceLevel == 1)
        {
            gsRenderHeight += ROOF_LEVEL_HEIGHT;
            gTacticalStatus.uiFlags |= SHOW_ALL_ROOFS;
            InvalidateWorldRedundency();
        }
        else if (gsInterfaceLevel == 0)
        {
            gsRenderHeight -= ROOF_LEVEL_HEIGHT;
            gTacticalStatus.uiFlags &= (~SHOW_ALL_ROOFS);
            InvalidateWorldRedundency();
        }

        SetRenderFlags(RENDER_FLAG_FULL);
        // Remove any interactive tiles we could be over!
        BeginCurInteractiveTileCheck(INTILE_CHECK_SELECTIVE);
        gfPlotNewMovement = true;
        ErasePath(false);
    }


    ScreenName UIHandleChangeLevel(UI_EVENT pUIEvent)
    {
        if (gsInterfaceLevel == 0)
        {
            ChangeInterfaceLevel(1);
        }
        else if (gsInterfaceLevel == 1)
        {
            ChangeInterfaceLevel(0);
        }

        return ScreenName.GAME_SCREEN;
    }


    extern void InternalSelectSoldier(uint usSoldierID, bool fAcknowledge, bool fForceReselect, bool fFromUI);

    ScreenName UIHandleSelectMerc(UI_EVENT pUIEvent)
    {
        int iCurrentSquad;

        // Get merc index at mouse and set current selection
        if (gfUIFullTargetFound)
        {
            iCurrentSquad = CurrentSquad();

            InternalSelectSoldier(gusUIFullTargetID, true, false, true);

            // If different, display message
            if (CurrentSquad() != iCurrentSquad)
            {
                ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_INTERFACE, pMessageStrings[MSG_SQUAD_ACTIVE], (CurrentSquad() + 1));
            }
        }

        return (ScreenName.GAME_SCREEN);
    }

    static int sGridNoForItemsOver;
    static byte bLevelForItemsOver;
    static uint uiItemsOverTimer;
    static bool fOverItems;

    ScreenName UIHandleMOnTerrain(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE? pSoldier;
        uint usMapPos;
        bool fSetCursor = false;
        uint uiCursorFlags;
        LEVELNODE* pIntNode;
        EXITGRID ExitGrid;
        int sIntTileGridNo;
        bool fContinue = true;
        ITEM_POOL* pItemPool;

        if (!GetMouseMapPos(&usMapPos))
        {
            return (ScreenName.GAME_SCREEN);
        }

        gUIActionModeChangeDueToMouseOver = false;

        // If we are a vehicle..... just show an X
        if (GetSoldier(&pSoldier, gusSelectedSoldier))
        {
            if ((OK_ENTERABLE_VEHICLE(pSoldier)))
            {
                if (!UIHandleOnMerc(true))
                {
                    guiNewUICursor = FLOATING_X_UICURSOR;
                    return (ScreenName.GAME_SCREEN);
                }
            }
        }

        // CHECK IF WE'RE ON A GUY ( EITHER SELECTED, OURS, OR THEIRS
        if (!UIHandleOnMerc(true))
        {
            // Are we over items...
            if (GetItemPool(usMapPos, &pItemPool, (byte)gsInterfaceLevel) && ITEMPOOL_VISIBLE(pItemPool))
            {
                // Are we already in...
                if (fOverItems)
                {
                    // Is this the same level & gridno...
                    if (gsInterfaceLevel == (int)bLevelForItemsOver && usMapPos == sGridNoForItemsOver)
                    {
                        // Check timer...
                        if ((GetJA2Clock() - uiItemsOverTimer) > 1500)
                        {
                            // Change to hand curso mode
                            guiPendingOverrideEvent = M_CHANGE_TO_HANDMODE;
                            gsOverItemsGridNo = usMapPos;
                            gsOverItemsLevel = gsInterfaceLevel;
                            fOverItems = false;
                        }
                    }
                    else
                    {
                        uiItemsOverTimer = GetJA2Clock();
                        bLevelForItemsOver = (byte)gsInterfaceLevel;
                        sGridNoForItemsOver = usMapPos;
                    }
                }
                else
                {
                    fOverItems = true;

                    uiItemsOverTimer = GetJA2Clock();
                    bLevelForItemsOver = (byte)gsInterfaceLevel;
                    sGridNoForItemsOver = usMapPos;
                }
            }
            else
            {
                fOverItems = false;
            }


            if (GetSoldier(&pSoldier, gusSelectedSoldier))
            {

                if (pSoldier.sGridNo == (int)IsometricDefines.NOWHERE)
                {
                    int i = 0;
                }

                if (GetExitGrid(usMapPos, &ExitGrid) && pSoldier.bLevel == 0)
                {
                    gfUIShowExitExitGrid = true;
                }

                // ATE: Draw invalidc cursor if heights different
                if (gpWorldLevelData[usMapPos].sHeight != gpWorldLevelData[pSoldier.sGridNo].sHeight)
                {
                    // ERASE PATH
                    ErasePath(true);

                    guiNewUICursor = FLOATING_X_UICURSOR;

                    return (ScreenName.GAME_SCREEN);
                }
            }

            // DO SOME CURSOR POSITION FLAGS SETTING
            GetCursorMovementFlags(&uiCursorFlags);

            if (gusSelectedSoldier != NO_SOLDIER)
            {
                // Get Soldier Pointer
                GetSoldier(&pSoldier, gusSelectedSoldier);

                // Get interactvie tile node
                pIntNode = GetCurInteractiveTileGridNo(&sIntTileGridNo);

                // Check were we are
                // CHECK IF WE CAN MOVE HERE
                // THIS IS JUST A CRUDE TEST FOR NOW
                if (pSoldier.bLife < OKLIFE)
                {
                    byte ubID;
                    // Show reg. cursor
                    // GO INTO IDLE MODE
                    // guiPendingOverrideEvent = I_CHANGE_TO_IDLE;
                    // gusSelectedSoldier = NO_SOLDIER;	
                    ubID = FindNextActiveAndAliveMerc(pSoldier, false, false);

                    if (ubID != OverheadTypes.NOBODY)
                    {
                        SelectSoldier((int)ubID, false, false);
                    }
                    else
                    {
                        gusSelectedSoldier = NO_SOLDIER;
                        // Change UI mode to reflact that we are selected
                        guiPendingOverrideEvent = I_ON_TERRAIN;
                    }
                }
                else if ((UIOKMoveDestination(pSoldier, usMapPos) != 1) && pIntNode == null)
                {
                    // ERASE PATH
                    ErasePath(true);

                    guiNewUICursor = CANNOT_MOVE_UICURSOR;

                }
                else
                {
                    if (!UIHandleInteractiveTilesAndItemsOnTerrain(pSoldier, usMapPos, false, true))
                    {
                        // Are we in combat?
                        if ((gTacticalStatus.uiFlags & INCOMBAT) && (gTacticalStatus.uiFlags & TURNBASED))
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
                guiNewUICursor = NORMAL_SNAPUICURSOR;
            }
        }
        else
        {
            if (ValidQuickExchangePosition())
            {
                // Do new cursor!
                guiNewUICursor = EXCHANGE_PLACES_UICURSOR;
            }
        }


        {

            //if ( fSetCursor && guiNewUICursor != ENTER_VEHICLE_UICURSOR )
            if (fSetCursor && !gfBeginVehicleCursor)
            {
                SetMovementModeCursor(pSoldier);
            }
        }

        return ScreenName.GAME_SCREEN;
    }

    ScreenName UIHandleMovementMenu(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE? pSoldier;


        // Get soldier
        if (!GetSoldier(&pSoldier, gusSelectedSoldier))
        {
            return (ScreenName.GAME_SCREEN);
        }

        // Popup Menu
        if (pUIEvent.fFirstTime)
        {
            //Pop-up menu
            PopupMovementMenu(pUIEvent);

            // Change cusror to normal
            guiNewUICursor = NORMAL_FREEUICURSOR;

        }

        // Check for done flag
        if (pUIEvent.fDoneMenu)
        {
            PopDownMovementMenu();

            // Excecute command, if user hit a button
            if (pUIEvent.uiParams[1] == true)
            {
                if (pUIEvent.uiParams[2] == MOVEMENT_MENU_LOOK)
                {
                    guiPendingOverrideEvent = LC_CHANGE_TO_LOOK;
                }
                else if (pUIEvent.uiParams[2] == MOVEMENT_MENU_HAND)
                {
                    guiPendingOverrideEvent = HC_ON_TERRAIN;
                }
                else if (pUIEvent.uiParams[2] == MOVEMENT_MENU_ACTIONC)
                {
                    guiPendingOverrideEvent = M_CHANGE_TO_ACTION;
                }
                else if (pUIEvent.uiParams[2] == MOVEMENT_MENU_TALK)
                {
                    guiPendingOverrideEvent = T_CHANGE_TO_TALKING;
                }
                else
                {
                    // Change stance based on params!
                    switch (pUIEvent.uiParams[0])
                    {
                        case MOVEMENT_MENU_RUN:

                            if (pSoldier.usUIMovementMode != WALKING && pSoldier.usUIMovementMode != RUNNING)
                            {
                                UIHandleSoldierStanceChange(pSoldier.ubID, ANIM_STAND);
                                pSoldier.fUIMovementFast = 1;
                            }
                            else
                            {
                                pSoldier.fUIMovementFast = 1;
                                pSoldier.usUIMovementMode = RUNNING;
                                gfPlotNewMovement = true;
                            }
                            break;

                        case MOVEMENT_MENU_WALK:

                            UIHandleSoldierStanceChange(pSoldier.ubID, ANIM_STAND);
                            break;

                        case MOVEMENT_MENU_SWAT:

                            UIHandleSoldierStanceChange(pSoldier.ubID, ANIM_CROUCH);
                            break;

                        case MOVEMENT_MENU_PRONE:

                            UIHandleSoldierStanceChange(pSoldier.ubID, ANIM_PRONE);
                            break;

                    }

                    guiPendingOverrideEvent = A_CHANGE_TO_MOVE;

                    //pSoldier.usUIMovementMode = (byte)pUIEvent.uiParams[ 0 ];
                }

            }
        }

        return (ScreenName.GAME_SCREEN);
    }


    ScreenName UIHandlePositionMenu(UI_EVENT pUIEvent)
    {

        return (ScreenName.GAME_SCREEN);
    }


    ScreenName UIHandleAOnTerrain(UI_EVENT pUIEvent)
    {
        uint usMapPos;
        SOLDIERTYPE? pSoldier;
        //	int							sTargetXPos, sTargetYPos;

        if (!GetMouseMapPos(&usMapPos))
        {
            return (ScreenName.GAME_SCREEN);
        }

        if (gpItemPointer != null)
        {
            return (ScreenName.GAME_SCREEN);
        }

        // Get soldier to determine range
        if (GetSoldier(&pSoldier, gusSelectedSoldier))
        {
            // ATE: Add stuff here to display a system message if we are targeting smeothing and
            //  are out of range.
            // Are we using a gun?
            if (GetActionModeCursor(pSoldier) == TARGETCURS)
            {
                SetActionModeDoorCursorText();

                // Yep, she's a gun.
                // Are we in range?
                if (!InRange(pSoldier, usMapPos))
                {
                    // Are we over a guy?
                    if (gfUIFullTargetFound)
                    {
                        // No, ok display message IF this is the first time at this gridno
                        if (gsOutOfRangeGridNo != MercPtrs[gusUIFullTargetID].sGridNo || gubOutOfRangeMerc != gusSelectedSoldier)
                        {
                            // Display
                            ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_INTERFACE, TacticalStr[OUT_OF_RANGE_STRING]);

                            //PlayJA2Sample( TARGET_OUT_OF_RANGE, RATE_11025, MIDVOLUME, 1, MIDDLEPAN );			              

                            // Set
                            gsOutOfRangeGridNo = MercPtrs[gusUIFullTargetID].sGridNo;
                            gubOutOfRangeMerc = (byte)gusSelectedSoldier;
                        }
                    }
                }

            }

            guiNewUICursor = GetProperItemCursor((byte)gusSelectedSoldier, pSoldier.inv[HANDPOS].usItem, usMapPos, false);

            // Show UI ON GUY
            UIHandleOnMerc(false);

            // If we are in realtime, and in a stationary animation, follow!
            if ((gTacticalStatus.uiFlags & REALTIME) || !(gTacticalStatus.uiFlags & INCOMBAT))
            {
                if (gAnimControl[pSoldier.usAnimState].uiFlags & ANIM_STATIONARY && pSoldier.ubPendingAction == NO_PENDING_ACTION)
                {
                    // Check if we have a shot waiting!
                    if (gUITargetShotWaiting)
                    {
                        guiPendingOverrideEvent = CA_MERC_SHOOT;
                    }

                    if (!gUITargetReady)
                    {
                        // Move to proper stance + direction!
                        // Convert our grid-not into an XY
                        //	ConvertGridNoToXY( usMapPos, &sTargetXPos, &sTargetYPos );

                        // Ready weapon
                        //		SoldierReadyWeapon( pSoldier, sTargetXPos, sTargetYPos, false );

                        gUITargetReady = true;
                    }



                }
                else
                {
                    gUITargetReady = false;
                }
            }


        }


        return (ScreenName.GAME_SCREEN);
    }

    ScreenName UIHandleMChangeToAction(UI_EVENT pUIEvent)
    {
        gUITargetShotWaiting = false;

        EndPhysicsTrajectoryUI();

        //guiNewUICursor = CONFIRM_MOVE_UICURSOR;

        return (ScreenName.GAME_SCREEN);
    }

    ScreenName UIHandleMChangeToHandMode(UI_EVENT pUIEvent)
    {
        ErasePath(false);

        return (ScreenName.GAME_SCREEN);
    }

    ScreenName UIHandleAChangeToMove(UI_EVENT pUIEvent)
    {
        // Set merc glow back to normal
        // ( could have been set when in target cursor )
        SetMercGlowNormal();

        // gsOutOfRangeGridNo = NOWHERE;

        gfPlotNewMovement = true;

        return (ScreenName.GAME_SCREEN);
    }

    ScreenName UIHandleCWait(UI_EVENT pUIEvent)
    {
        uint usMapPos;
        SOLDIERTYPE? pSoldier;
        bool fSetCursor;
        uint uiCursorFlags;
        LEVELNODE* pInvTile;

        if (!GetMouseMapPos(&usMapPos))
        {
            return (ScreenName.GAME_SCREEN);
        }

        if (GetSoldier(&pSoldier, gusSelectedSoldier))
        {
            pInvTile = GetCurInteractiveTile();

            if (pInvTile && gpInvTileThatCausedMoveConfirm != pInvTile)
            {
                // Get out og this mode...
                guiPendingOverrideEvent = A_CHANGE_TO_MOVE;
                return (ScreenName.GAME_SCREEN);
            }

            GetCursorMovementFlags(&uiCursorFlags);

            if (pInvTile != null)
            {

                fSetCursor = HandleUIMovementCursor(pSoldier, uiCursorFlags, usMapPos, MOVEUI_TARGET.INTTILES);

                //Set UI CURSOR
                guiNewUICursor = GetInteractiveTileCursor(guiNewUICursor, true);

                // Make red tile under spot... if we've previously found one...
                if (gfUIHandleShowMoveGrid)
                {
                    gfUIHandleShowMoveGrid = 2;
                }


                return (ScreenName.GAME_SCREEN);
            }

            // Display action points
            gfUIDisplayActionPoints = true;

            // Determine if we can afford!
            if (!EnoughPoints(pSoldier, gsCurrentActionPoints, 0, false))
            {
                gfUIDisplayActionPointsInvalid = true;
            }

            SetConfirmMovementModeCursor(pSoldier, false);

            // If we are not in combat, draw path here!
            if ((gTacticalStatus.uiFlags & REALTIME) || !(gTacticalStatus.uiFlags & INCOMBAT))
            {
                //DrawUIMovementPath( pSoldier, usMapPos,  0 );
                fSetCursor = HandleUIMovementCursor(pSoldier, uiCursorFlags, usMapPos, 0);
            }

        }

        return (ScreenName.GAME_SCREEN);
    }


    // NOTE, ONCE AT THIS FUNCTION, WE HAVE ASSUMED TO HAVE CHECKED FOR ENOUGH APS THROUGH
    // SelectedMercCanAffordMove
    ScreenName UIHandleCMoveMerc(UI_EVENT pUIEvent)
    {
        uint usMapPos;
        SOLDIERTYPE? pSoldier;
        int sDestGridNo;
        int sActionGridNo;
        STRUCTURE* pStructure;
        byte ubDirection;
        bool fAllMove;
        byte bLoop;
        LEVELNODE* pIntTile;
        int sIntTileGridNo;
        bool fOldFastMove;

        if (gusSelectedSoldier != NO_SOLDIER)
        {
            fAllMove = gfUIAllMoveOn;
            gfUIAllMoveOn = false;

            if (!GetMouseMapPos(&usMapPos))
            {
                return (ScreenName.GAME_SCREEN);
            }

            // ERASE PATH
            ErasePath(true);

            if (fAllMove)
            {
                gfGetNewPathThroughPeople = true;

                // Loop through all mercs and make go!
                // TODO: Only our squad!
                for (bLoop = gTacticalStatus.Team[gbPlayerNum].bFirstID, pSoldier = MercPtrs[bLoop]; bLoop <= gTacticalStatus.Team[gbPlayerNum].bLastID; bLoop++, pSoldier++)
                {
                    if (OK_CONTROLLABLE_MERC(pSoldier) && pSoldier.bAssignment == CurrentSquad() && !pSoldier.fMercAsleep)
                    {
                        // If we can't be controlled, returninvalid...
                        if (pSoldier.uiStatusFlags & SOLDIER_ROBOT)
                        {
                            if (!CanRobotBeControlled(pSoldier))
                            {
                                continue;
                            }
                        }

                        AdjustNoAPToFinishMove(pSoldier, false);

                        fOldFastMove = pSoldier.fUIMovementFast;

                        if (fAllMove == 2)
                        {
                            pSoldier.fUIMovementFast = true;
                            pSoldier.usUIMovementMode = RUNNING;
                        }
                        else
                        {
                            pSoldier.fUIMovementFast = false;
                            pSoldier.usUIMovementMode = GetMoveStateBasedOnStance(pSoldier, gAnimControl[pSoldier.usAnimState].ubEndHeight);
                        }

                        // Remove any previous actions
                        pSoldier.ubPendingAction = NO_PENDING_ACTION;


                        //if ( !( gTacticalStatus.uiFlags & INCOMBAT ) && ( gAnimControl[ pSoldier.usAnimState ].uiFlags & ANIM_MOVING ) )
                        //{
                        //	pSoldier.sRTPendingMovementGridNo = usMapPos;
                        //	pSoldier.usRTPendingMovementAnim  = pSoldier.usUIMovementMode;
                        //}
                        //else					
                        if (EVENT_InternalGetNewSoldierPath(pSoldier, usMapPos, pSoldier.usUIMovementMode, true, false))
                        {
                            InternalDoMercBattleSound(pSoldier, BATTLE_SOUND_OK1, BATTLE_SND_LOWER_VOLUME);
                        }
                        else
                        {
                            ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_INTERFACE, TacticalStr[NO_PATH_FOR_MERC], pSoldier.name);
                        }

                        pSoldier.fUIMovementFast = fOldFastMove;

                    }
                }
                gfGetNewPathThroughPeople = false;

                // RESET MOVE FAST FLAG
                SetConfirmMovementModeCursor(pSoldier, true);

                gfUIAllMoveOn = 0;

            }
            else
            {
                // Get soldier
                if (GetSoldier(&pSoldier, gusSelectedSoldier))
                {
                    // FOR REALTIME - DO MOVEMENT BASED ON STANCE!
                    if ((gTacticalStatus.uiFlags & REALTIME) || !(gTacticalStatus.uiFlags & INCOMBAT))
                    {
                        pSoldier.usUIMovementMode = GetMoveStateBasedOnStance(pSoldier, gAnimControl[pSoldier.usAnimState].ubEndHeight);
                    }


                    sDestGridNo = usMapPos;


                    // Get structure info for in tile!
                    pIntTile = GetCurInteractiveTileGridNoAndStructure(&sIntTileGridNo, &pStructure);

                    // We should not have null here if we are given this flag...
                    if (pIntTile != null)
                    {
                        sActionGridNo = FindAdjacentGridEx(pSoldier, sIntTileGridNo, &ubDirection, null, false, true);
                        if (sActionGridNo != -1)
                        {
                            SetUIBusy(pSoldier.ubID);

                            // Set dest gridno
                            sDestGridNo = sActionGridNo;

                            // check if we are at this location
                            if (pSoldier.sGridNo == sDestGridNo)
                            {
                                StartInteractiveObject(sIntTileGridNo, pStructure.usStructureID, pSoldier, ubDirection);
                                InteractWithInteractiveObject(pSoldier, pStructure, ubDirection);
                                return (ScreenName.GAME_SCREEN);
                            }
                        }
                        else
                        {
                            ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_UI_FEEDBACK, TacticalStr[NO_PATH]);
                            return (ScreenName.GAME_SCREEN);
                        }
                    }

                    SetUIBusy(pSoldier.ubID);

                    if ((gTacticalStatus.uiFlags & REALTIME) || !(gTacticalStatus.uiFlags & INCOMBAT))
                    {
                        // RESET MOVE FAST FLAG
                        SetConfirmMovementModeCursor(pSoldier, true);

                        if (!gTacticalStatus.fAtLeastOneGuyOnMultiSelect)
                        {
                            pSoldier.fUIMovementFast = false;
                        }

                        //StartLooseCursor( usMapPos, 0 );
                    }

                    if (gTacticalStatus.fAtLeastOneGuyOnMultiSelect && pIntTile == null)
                    {
                        HandleMultiSelectionMove(sDestGridNo);
                    }
                    else
                    {

                        if (gUIUseReverse)
                        {
                            pSoldier.bReverse = true;
                        }
                        else
                        {
                            pSoldier.bReverse = false;
                        }

                        // Remove any previous actions
                        pSoldier.ubPendingAction = NO_PENDING_ACTION;

                        {
                            EVENT_InternalGetNewSoldierPath(pSoldier, sDestGridNo, pSoldier.usUIMovementMode, true, pSoldier.fNoAPToFinishMove);
                        }

                        if (pSoldier.usPathDataSize > 5)
                        {
                            DoMercBattleSound(pSoldier, BATTLE_SOUND_OK1);
                        }

                        // HANDLE ANY INTERACTIVE OBJECTS HERE!
                        if (pIntTile != null)
                        {
                            StartInteractiveObject(sIntTileGridNo, pStructure.usStructureID, pSoldier, ubDirection);
                        }
                    }
                }
            }
        }
        return (ScreenName.GAME_SCREEN);
    }

    ScreenName UIHandleMCycleMoveAll(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE? pSoldier;

        if (!GetSoldier(&pSoldier, gusSelectedSoldier))
        {
            return (ScreenName.GAME_SCREEN);
        }

        if (gfUICanBeginAllMoveCycle)
        {
            gfUIAllMoveOn = true;
            gfUICanBeginAllMoveCycle = false;
        }
        return (ScreenName.GAME_SCREEN);
    }


    ScreenName UIHandleMCycleMovement(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE* pSoldier;
        bool fGoodMode = false;

        if (!GetSoldier(&pSoldier, gusSelectedSoldier))
        {
            return (ScreenName.GAME_SCREEN);
        }

        gfUIAllMoveOn = false;

        if (pSoldier.ubBodyType == ROBOTNOWEAPON)
        {
            pSoldier.usUIMovementMode = WALKING;
            gfPlotNewMovement = true;
            return (ScreenName.GAME_SCREEN);
        }

        do
        {
            // Cycle gmovement state
            if (pSoldier.usUIMovementMode == RUNNING)
            {
                pSoldier.usUIMovementMode = WALKING;
                if (IsValidMovementMode(pSoldier, WALKING))
                {
                    fGoodMode = true;
                }
            }
            else if (pSoldier.usUIMovementMode == WALKING)
            {
                pSoldier.usUIMovementMode = SWATTING;
                if (IsValidMovementMode(pSoldier, SWATTING))
                {
                    fGoodMode = true;
                }
            }
            else if (pSoldier.usUIMovementMode == SWATTING)
            {
                pSoldier.usUIMovementMode = CRAWLING;
                if (IsValidMovementMode(pSoldier, CRAWLING))
                {
                    fGoodMode = true;
                }
            }
            else if (pSoldier.usUIMovementMode == CRAWLING)
            {
                pSoldier.fUIMovementFast = 1;
                pSoldier.usUIMovementMode = RUNNING;
                if (IsValidMovementMode(pSoldier, RUNNING))
                {
                    fGoodMode = true;
                }
            }

        } while (fGoodMode != true);

        gfPlotNewMovement = true;


        return (ScreenName.GAME_SCREEN);
    }

    ScreenName UIHandleCOnTerrain(UI_EVENT pUIEvent)
    {

        return (ScreenName.GAME_SCREEN);
    }


    static uint gusAnchorMouseY;
    static uint usOldMouseY;
    static bool ubNearHeigherLevel;
    static bool ubNearLowerLevel;
    static byte ubUpHeight, ubDownDepth;
    static uint uiOldShowUPDownArrows;

    ScreenName UIHandleMAdjustStanceMode(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE* pSoldier;
        bool fCheck = false;
        int iPosDiff;
        byte bNewDirection;

        // Change cusror to normal
        guiNewUICursor = NO_UICURSOR;


        if (pUIEvent.fFirstTime)
        {
            gusAnchorMouseY = gusMouseYPos;
            usOldMouseY = gusMouseYPos;
            ubNearHeigherLevel = false;
            ubNearLowerLevel = false;

            guiShowUPDownArrows = ARROWS_SHOW_DOWN_BESIDE | ARROWS_SHOW_UP_BESIDE;
            uiOldShowUPDownArrows = guiShowUPDownArrows;

            gbAdjustStanceDiff = 0;
            gbClimbID = 0;

            gfIgnoreScrolling = true;

            // Get soldier current height of animation
            if (GetSoldier(&pSoldier, gusSelectedSoldier))
            {
                // IF we are on a basic level...(temp)
                if (pSoldier.bLevel == 0)
                {
                    if (FindHeigherLevel(pSoldier, pSoldier.sGridNo, pSoldier.bDirection, &bNewDirection))
                    {
                        ubNearHeigherLevel = true;
                    }
                }

                // IF we are higher...
                if (pSoldier.bLevel > 0)
                {
                    if (FindLowerLevel(pSoldier, pSoldier.sGridNo, pSoldier.bDirection, &bNewDirection))
                    {
                        ubNearLowerLevel = true;
                    }
                }

                switch (gAnimControl[pSoldier.usAnimState].ubEndHeight)
                {
                    case ANIM_STAND:
                        if (ubNearHeigherLevel)
                        {
                            ubUpHeight = 1;
                            ubDownDepth = 2;
                        }
                        else if (ubNearLowerLevel)
                        {
                            ubUpHeight = 0;
                            ubDownDepth = 3;
                        }
                        else
                        {
                            ubUpHeight = 0;
                            ubDownDepth = 2;
                        }
                        break;

                    case ANIM_CROUCH:
                        if (ubNearHeigherLevel)
                        {
                            ubUpHeight = 2;
                            ubDownDepth = 1;
                        }
                        else if (ubNearLowerLevel)
                        {
                            ubUpHeight = 1;
                            ubDownDepth = 2;
                        }
                        else
                        {
                            ubUpHeight = 1;
                            ubDownDepth = 1;
                        }
                        break;

                    case ANIM_PRONE:
                        if (ubNearHeigherLevel)
                        {
                            ubUpHeight = 3;
                            ubDownDepth = 0;
                        }
                        else if (ubNearLowerLevel)
                        {
                            ubUpHeight = 2;
                            ubDownDepth = 1;
                        }
                        else
                        {
                            ubUpHeight = 2;
                            ubDownDepth = 0;
                        }
                        break;
                }


            }

        }

        // Check if delta X has changed alot since last time
        iPosDiff = abs((int)(usOldMouseY - gusMouseYPos));

        //guiShowUPDownArrows = ARROWS_SHOW_DOWN_BESIDE | ARROWS_SHOW_UP_BESIDE;
        guiShowUPDownArrows = uiOldShowUPDownArrows;

        {
            if (gusAnchorMouseY > gusMouseYPos)
            {
                // Get soldier
                if (GetSoldier(&pSoldier, gusSelectedSoldier))
                {
                    if (iPosDiff < GO_MOVE_ONE && ubUpHeight >= 1)
                    {
                        // Change arrows to move down arrow + show
                        //guiShowUPDownArrows = ARROWS_SHOW_UP_ABOVE_Y;
                        guiShowUPDownArrows = ARROWS_SHOW_DOWN_BESIDE | ARROWS_SHOW_UP_BESIDE;
                        gbAdjustStanceDiff = 0;
                        gbClimbID = 0;
                    }
                    else if (iPosDiff > GO_MOVE_ONE && iPosDiff < GO_MOVE_TWO && ubUpHeight >= 1)
                    {
                        //guiShowUPDownArrows = ARROWS_SHOW_UP_ABOVE_G;
                        if (ubUpHeight == 1 && ubNearHeigherLevel)
                        {
                            guiShowUPDownArrows = ARROWS_SHOW_UP_ABOVE_CLIMB;
                            gbClimbID = 1;
                        }
                        else
                        {
                            guiShowUPDownArrows = ARROWS_SHOW_UP_ABOVE_Y;
                            gbClimbID = 0;
                        }
                        gbAdjustStanceDiff = 1;
                    }
                    else if (iPosDiff >= GO_MOVE_TWO && iPosDiff < GO_MOVE_THREE && ubUpHeight >= 2)
                    {
                        if (ubUpHeight == 2 && ubNearHeigherLevel)
                        {
                            guiShowUPDownArrows = ARROWS_SHOW_UP_ABOVE_CLIMB;
                            gbClimbID = 1;
                        }
                        else
                        {
                            guiShowUPDownArrows = ARROWS_SHOW_UP_ABOVE_YY;
                            gbClimbID = 0;
                        }
                        gbAdjustStanceDiff = 2;
                    }
                    else if (iPosDiff >= GO_MOVE_THREE && ubUpHeight >= 3)
                    {
                        if (ubUpHeight == 3 && ubNearHeigherLevel)
                        {
                            guiShowUPDownArrows = ARROWS_SHOW_UP_ABOVE_CLIMB;
                            gbClimbID = 1;
                        }
                    }
                }

            }

            if (gusAnchorMouseY < gusMouseYPos)
            {

                // Get soldier
                if (GetSoldier(&pSoldier, gusSelectedSoldier))
                {
                    if (iPosDiff < GO_MOVE_ONE && ubDownDepth >= 1)
                    {
                        // Change arrows to move down arrow + show
                        //guiShowUPDownArrows = ARROWS_SHOW_DOWN_BELOW_Y;
                        guiShowUPDownArrows = ARROWS_SHOW_DOWN_BESIDE | ARROWS_SHOW_UP_BESIDE;
                        gbAdjustStanceDiff = 0;
                        gbClimbID = 0;

                    }
                    else if (iPosDiff >= GO_MOVE_ONE && iPosDiff < GO_MOVE_TWO && ubDownDepth >= 1)
                    {
                        //						guiShowUPDownArrows = ARROWS_SHOW_DOWN_BELOW_G;
                        if (ubDownDepth == 1 && ubNearLowerLevel)
                        {
                            guiShowUPDownArrows = ARROWS_SHOW_DOWN_CLIMB;
                            gbClimbID = -1;
                        }
                        else
                        {
                            guiShowUPDownArrows = ARROWS_SHOW_DOWN_BELOW_Y;
                            gbClimbID = 0;
                        }
                        gbAdjustStanceDiff = -1;
                    }
                    else if (iPosDiff > GO_MOVE_TWO && iPosDiff < GO_MOVE_THREE && ubDownDepth >= 2)
                    {
                        //guiShowUPDownArrows = ARROWS_SHOW_DOWN_BELOW_GG;
                        if (ubDownDepth == 2 && ubNearLowerLevel)
                        {
                            guiShowUPDownArrows = ARROWS_SHOW_DOWN_CLIMB;
                            gbClimbID = -1;
                        }
                        else
                        {
                            guiShowUPDownArrows = ARROWS_SHOW_DOWN_BELOW_YY;
                            gbClimbID = 0;
                        }
                        gbAdjustStanceDiff = -2;
                    }
                    else if (iPosDiff > GO_MOVE_THREE && ubDownDepth >= 3)
                    {
                        //guiShowUPDownArrows = ARROWS_SHOW_DOWN_BELOW_GG;
                        if (ubDownDepth == 3 && ubNearLowerLevel)
                        {
                            guiShowUPDownArrows = ARROWS_SHOW_DOWN_CLIMB;
                            gbClimbID = -1;
                        }
                    }

                }

            }

        }

        uiOldShowUPDownArrows = guiShowUPDownArrows;

        return (ScreenName.GAME_SCREEN);
    }

    ScreenName UIHandleAChangeToConfirmAction(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE* pSoldier;

        if (GetSoldier(&pSoldier, gusSelectedSoldier))
        {
            HandleLeftClickCursor(pSoldier);

        }

        ResetBurstLocations();

        return (ScreenName.GAME_SCREEN);
    }


    ScreenName UIHandleCAOnTerrain(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE* pSoldier;
        uint usMapPos;

        if (!GetMouseMapPos(&usMapPos))
        {
            return (ScreenName.GAME_SCREEN);
        }

        if (GetSoldier(&pSoldier, gusSelectedSoldier))
        {
            guiNewUICursor = GetProperItemCursor((byte)gusSelectedSoldier, pSoldier.inv[HANDPOS].usItem, usMapPos, true);

            UIHandleOnMerc(false);

        }

        return (ScreenName.GAME_SCREEN);
    }



    void UIHandleMercAttack(SOLDIERTYPE* pSoldier, SOLDIERTYPE* pTargetSoldier, uint usMapPos)
    {
        int iHandleReturn;
        int sTargetGridNo;
        byte bTargetLevel;
        uint usItem;
        LEVELNODE* pIntNode;
        STRUCTURE* pStructure;
        int sGridNo, sNewGridNo;
        byte ubItemCursor;

        // get cursor
        ubItemCursor = GetActionModeCursor(pSoldier);

        if (!(gTacticalStatus.uiFlags & INCOMBAT) && pTargetSoldier && Item[pSoldier.inv[HANDPOS].usItem].usItemClass & IC_WEAPON)
        {
            if (NPCFirstDraw(pSoldier, pTargetSoldier))
            {
                // go into turnbased for that person
                CancelAIAction(pTargetSoldier, true);
                AddToShouldBecomeHostileOrSayQuoteList(pTargetSoldier.ubID);
                //MakeCivHostile( pTargetSoldier, 2 );
                //TriggerNPCWithIHateYouQuote( pTargetSoldier.ubProfile );
                return;
            }
        }

        // Set aim time to one in UI
        pSoldier.bAimTime = (pSoldier.bShownAimTime / 2);
        usItem = pSoldier.inv[HANDPOS].usItem;

        // ATE: Check if we are targeting an interactive tile, and adjust gridno accordingly...
        pIntNode = GetCurInteractiveTileGridNoAndStructure(&sGridNo, &pStructure);

        if (pTargetSoldier != null)
        {
            sTargetGridNo = pTargetSoldier.sGridNo;
            bTargetLevel = pTargetSoldier.bLevel;
        }
        else
        {
            sTargetGridNo = usMapPos;
            bTargetLevel = (byte)gsInterfaceLevel;

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
            if (sGridNo == pSoldier.sGridNo && ubItemCursor != AIDCURS)
            {
                // Get orientation....
                switch (pStructure.ubWallOrientation)
                {
                    case OUTSIDE_TOP_LEFT:
                    case INSIDE_TOP_LEFT:

                        sNewGridNo = NewGridNo(sGridNo, DirectionInc(SOUTH));
                        break;

                    case OUTSIDE_TOP_RIGHT:
                    case INSIDE_TOP_RIGHT:

                        sNewGridNo = NewGridNo(sGridNo, DirectionInc(EAST));
                        break;

                    default:
                        sNewGridNo = sGridNo;
                }

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
        if (!(gTacticalStatus.uiFlags & INCOMBAT))
        {
            if (gAnimControl[pSoldier.usAnimState].uiFlags & ANIM_FIRE)
            {
                return;
            }
        }

        // If in turn-based mode - return to movement
        if ((gTacticalStatus.uiFlags & INCOMBAT))
        {
            // Reset some flags for cont move...
            pSoldier.sFinalDestination = pSoldier.sGridNo;
            pSoldier.bGoodContPath = false;
            //  guiPendingOverrideEvent = A_CHANGE_TO_MOVE;
        }


        if (pSoldier.bWeaponMode == WM_ATTACHED)
        {
            iHandleReturn = HandleItem(pSoldier, sTargetGridNo, bTargetLevel, UNDER_GLAUNCHER, true);
        }
        else
        {
            iHandleReturn = HandleItem(pSoldier, sTargetGridNo, bTargetLevel, pSoldier.inv[HANDPOS].usItem, true);
        }

        if (iHandleReturn < 0)
        {
            if (iHandleReturn == ITEM_HANDLE_RELOADING)
            {
                guiNewUICursor = ACTION_TARGET_RELOADING;
                return;
            }

            if (iHandleReturn == ITEM_HANDLE_NOROOM)
            {
                ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_UI_FEEDBACK, pMessageStrings[MSG_CANT_FIRE_HERE]);
                return;
            }
        }


        if (gTacticalStatus.uiFlags & TURNBASED && !(gTacticalStatus.uiFlags & INCOMBAT))
        {
            HandleUICursorRTFeedback(pSoldier);
        }

        gfUIForceReExamineCursorData = true;
    }

    void AttackRequesterCallback(byte bExitValue)
    {
        if (bExitValue == MSG_BOX_RETURN_YES)
        {
            gTacticalStatus.ubLastRequesterTargetID = gpRequesterTargetMerc.ubProfile;

            UIHandleMercAttack(gpRequesterMerc, gpRequesterTargetMerc, gsRequesterGridNo);
        }
    }


    ScreenName UIHandleCAMercShoot(UI_EVENT pUIEvent)
    {
        uint usMapPos;
        SOLDIERTYPE? pSoldier, pTSoldier = null;
        bool fDidRequester = false;

        if (gusSelectedSoldier != NO_SOLDIER)
        {

            if (!GetMouseMapPos(&usMapPos))
            {
                return (ScreenName.GAME_SCREEN);
            }

            // Get soldier
            if (GetSoldier(&pSoldier, gusSelectedSoldier))
            {
                // Get target guy...
                if (gfUIFullTargetFound)
                {
                    // Get target soldier, if one exists
                    pTSoldier = MercPtrs[gusUIFullTargetID];
                }


                if (pTSoldier != null)
                {
                    // If this is one of our own guys.....pop up requiester...
                    if ((pTSoldier.bTeam == gbPlayerNum || pTSoldier.bTeam == MILITIA_TEAM) && Item[pSoldier.inv[HANDPOS].usItem].usItemClass != IC_MEDKIT && pSoldier.inv[HANDPOS].usItem != GAS_CAN && gTacticalStatus.ubLastRequesterTargetID != pTSoldier.ubProfile && (pTSoldier.ubID != pSoldier.ubID))
                    {
                        int zStr[200];

                        gpRequesterMerc = pSoldier;
                        gpRequesterTargetMerc = pTSoldier;
                        gsRequesterGridNo = usMapPos;

                        fDidRequester = true;

                        wprintf(zStr, TacticalStr[ATTACK_OWN_GUY_PROMPT], pTSoldier.name);

                        DoMessageBox(MSG_BOX_BASIC_STYLE, zStr, GAME_SCREEN, (byte)MSG_BOX_FLAG_YESNO, AttackRequesterCallback, null);

                    }
                }

                if (!fDidRequester)
                {
                    UIHandleMercAttack(pSoldier, pTSoldier, usMapPos);
                }
            }
        }

        return (ScreenName.GAME_SCREEN);
    }


    ScreenName UIHandleAEndAction(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE* pSoldier;
        int sTargetXPos, sTargetYPos;
        uint usMapPos;

        // Get gridno at this location
        if (!GetMouseMapPos(&usMapPos))
        {
            return (ScreenName.GAME_SCREEN);
        }


        if (GetSoldier(&pSoldier, gusSelectedSoldier))
        {
            if ((gTacticalStatus.uiFlags & REALTIME) || !(gTacticalStatus.uiFlags & INCOMBAT))
            {
                if (gUITargetReady)
                {
                    // Move to proper stance + direction!
                    // Convert our grid-not into an XY
                    ConvertGridNoToXY(usMapPos, &sTargetXPos, &sTargetYPos);

                    // UNReady weapon
                    SoldierReadyWeapon(pSoldier, sTargetXPos, sTargetYPos, true);

                    gUITargetReady = false;
                }

            }
        }
        return (ScreenName.GAME_SCREEN);
    }

    ScreenName UIHandleCAEndConfirmAction(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE* pSoldier;

        if (GetSoldier(&pSoldier, gusSelectedSoldier))
        {
            HandleEndConfirmCursor(pSoldier);
        }

        return (ScreenName.GAME_SCREEN);
    }


    ScreenName UIHandleIOnTerrain(UI_EVENT pUIEvent)
    {
        uint usMapPos;

        // Get gridno at this location
        if (!GetMouseMapPos(&usMapPos))
        {
            return (ScreenName.GAME_SCREEN);
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
                guiNewUICursor = NORMAL_SNAPUICURSOR;
            }
        }

        return (ScreenName.GAME_SCREEN);
    }

    ScreenName UIHandleIChangeToIdle(UI_EVENT pUIEvent)
    {
        return (ScreenName.GAME_SCREEN);
    }

    ScreenName UIHandlePADJAdjustStance(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE* pSoldier;
        byte ubNewStance;
        bool fChangeStance = false;

        guiShowUPDownArrows = ARROWS_HIDE_UP | ARROWS_HIDE_DOWN;


        gfIgnoreScrolling = false;

        if (gusSelectedSoldier != NO_SOLDIER && gbAdjustStanceDiff != 0)
        {
            // Get soldier
            if (GetSoldier(&pSoldier, gusSelectedSoldier))
            {
                ubNewStance = GetAdjustedAnimHeight(gAnimControl[pSoldier.usAnimState].ubEndHeight, gbAdjustStanceDiff);

                if (gbClimbID == 1)
                {
                    BeginSoldierClimbUpRoof(pSoldier);
                }
                else if (gbClimbID == -1)
                {
                    BeginSoldierClimbDownRoof(pSoldier);
                }
                else
                {
                    // Set state to result
                    UIHandleSoldierStanceChange(pSoldier.ubID, ubNewStance);
                }

                // Once we have APs, we can safely reset nomove flag!
                // AdjustNoAPToFinishMove( pSoldier, false );

            }

        }
        return (ScreenName.GAME_SCREEN);
    }


    byte GetAdjustedAnimHeight(byte ubAnimHeight, byte bChange)
    {
        byte ubNewAnimHeight = ubAnimHeight;

        if (ubAnimHeight == ANIM_STAND)
        {
            if (bChange == -1)
            {
                ubNewAnimHeight = ANIM_CROUCH;
            }
            if (bChange == -2)
            {
                ubNewAnimHeight = ANIM_PRONE;
            }
            if (bChange == 1)
            {
                ubNewAnimHeight = 50;
            }
        }
        else if (ubAnimHeight == ANIM_CROUCH)
        {
            if (bChange == 1)
            {
                ubNewAnimHeight = ANIM_STAND;
            }
            if (bChange == -1)
            {
                ubNewAnimHeight = ANIM_PRONE;
            }
            if (bChange == -2)
            {
                ubNewAnimHeight = 55;
            }
        }
        else if (ubAnimHeight == ANIM_PRONE)
        {
            if (bChange == -1)
            {
                ubNewAnimHeight = 55;
            }
            if (bChange == 1)
            {
                ubNewAnimHeight = ANIM_CROUCH;
            }
            if (bChange == 2)
            {
                ubNewAnimHeight = ANIM_STAND;
            }
        }

        return (ubNewAnimHeight);
    }

    void HandleObjectHighlighting()
    {
        SOLDIERTYPE* pSoldier;
        uint usMapPos;

        if (!GetMouseMapPos(&usMapPos))
        {
            return;
        }

        // CHECK IF WE'RE ON A GUY ( EITHER SELECTED, OURS, OR THEIRS
        if (gfUIFullTargetFound)
        {
            // Get Soldier
            GetSoldier(&pSoldier, gusUIFullTargetID);

            // If an enemy, and in a given mode, highlight
            if (guiUIFullTargetFlags & ENEMY_MERC)
            {
                switch (gCurrentUIMode)
                {

                    case CONFIRM_MOVE_MODE:
                    case MENU_MODE:

                        break;

                    case MOVE_MODE:
                    case CONFIRM_ACTION_MODE:
                    //case ACTION_MODE:
                    case IDLE_MODE:

                        // Set as selected
                        //pSoldier.pCurrentShade = pSoldier.pShades[ 1 ];
                        break;
                }
            }
            else if (guiUIFullTargetFlags & OWNED_MERC)
            {
                // Check for selected
                pSoldier.pCurrentShade = pSoldier.pShades[0];
                gfUIDoNotHighlightSelMerc = true;
            }
        }

    }

    void AdjustSoldierCreationStartValues()
    {
        int cnt;
        SOLDIERTYPE* pSoldier;


        cnt = gTacticalStatus.Team[gbPlayerNum].bFirstID;
        guiCreateGuyIndex = (int)cnt;

        for (pSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[gbPlayerNum].bLastID; pSoldier++, cnt++)
        {
            if (!pSoldier.bActive)
            {
                guiCreateGuyIndex = (int)cnt;
                break;
            }
        }

        cnt = gTacticalStatus.Team[gbPlayerNum].bLastID + 1;
        guiCreateBadGuyIndex = (int)cnt;

        for (pSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[LAST_TEAM].bLastID; pSoldier++, cnt++)
        {
            if (!pSoldier.bActive && cnt > gTacticalStatus.Team[gbPlayerNum].bLastID)
            {
                guiCreateBadGuyIndex = (int)cnt;
                break;
            }
        }

    }

    bool SelectedMercCanAffordAttack()
    {
        SOLDIERTYPE* pSoldier;
        SOLDIERTYPE* pTargetSoldier;
        uint usMapPos;
        int sTargetGridNo;
        bool fEnoughPoints = true;
        int sAPCost;
        byte ubItemCursor;
        uint usInHand;

        if (gusSelectedSoldier != NO_SOLDIER)
        {

            if (!GetMouseMapPos(&usMapPos))
            {
                return true; // (ScreenName.GAME_SCREEN);
            }

            // Get soldier
            if (GetSoldier(&pSoldier, gusSelectedSoldier))
            {
                // LOOK IN GUY'S HAND TO CHECK LOCATION
                usInHand = pSoldier.inv[HANDPOS].usItem;

                // Get cursor value
                ubItemCursor = GetActionModeCursor(pSoldier);

                if (ubItemCursor == INVALIDCURS)
                {
                    return (false);
                }

                if (ubItemCursor == BOMBCURS)
                {
                    // Check as...
                    if (EnoughPoints(pSoldier, GetTotalAPsToDropBomb(pSoldier, usMapPos), 0, true))
                    {
                        return (true);
                    }
                }
                else if (ubItemCursor == REMOTECURS)
                {
                    // Check as...
                    if (EnoughPoints(pSoldier, GetAPsToUseRemote(pSoldier), 0, true))
                    {
                        return (true);
                    }
                }
                else
                {
                    // Look for a soldier at this position
                    if (gfUIFullTargetFound)
                    {
                        // GetSoldier
                        GetSoldier(&pTargetSoldier, gusUIFullTargetID);
                        sTargetGridNo = pTargetSoldier.sGridNo;
                    }
                    else
                    {
                        sTargetGridNo = usMapPos;
                    }

                    sAPCost = CalcTotalAPsToAttack(pSoldier, sTargetGridNo, true, (byte)(pSoldier.bShownAimTime / 2));

                    if (EnoughPoints(pSoldier, sAPCost, 0, true))
                    {
                        return (true);
                    }
                    else
                    {
                        // Play curse....
                        DoMercBattleSound(pSoldier, BATTLE_SOUND_CURSE1);
                    }
                }
            }

        }

        return (false);
    }


    bool SelectedMercCanAffordMove()
    {
        SOLDIERTYPE* pSoldier;
        uint sAPCost = 0;
        int sBPCost = 0;
        uint usMapPos;
        LEVELNODE* pIntTile;

        // Get soldier
        if (GetSoldier(&pSoldier, gusSelectedSoldier))
        {
            if (!GetMouseMapPos(&usMapPos))
            {
                return true;//(ScreenName.GAME_SCREEN);
            }


            // IF WE ARE OVER AN INTERACTIVE TILE, GIVE GRIDNO OF POSITION
            pIntTile = GetCurInteractiveTile();

            if (pIntTile != null)
            {
                // CHECK APS
                if (EnoughPoints(pSoldier, gsCurrentActionPoints, 0, true))
                {
                    return (true);
                }
                else
                {
                    return (false);
                }
            }

            // Take the first direction!
            sAPCost = PtsToMoveDirection(pSoldier, (byte)guiPathingData[0]);

            sAPCost += GetAPsToChangeStance(pSoldier, gAnimControl[pSoldier.usUIMovementMode].ubHeight);

            if (EnoughPoints(pSoldier, sAPCost, 0, true))
            {
                return (true);
            }
            else
            {
                // OK, remember where we were trying to get to.....
                pSoldier.sContPathLocation = usMapPos;
                pSoldier.bGoodContPath = true;
            }
        }

        return (false);
    }

    void GetMercClimbDirection(byte ubSoldierID, bool* pfGoDown, bool* pfGoUp)
    {
        byte bNewDirection;
        SOLDIERTYPE* pSoldier;

        *pfGoDown = false;
        *pfGoUp = false;

        if (!GetSoldier(&pSoldier, ubSoldierID))
        {
            return;
        }

        // Check if we are close / can climb
        if (pSoldier.bLevel == 0)
        {
            // See if we are not in a building!
            if (FindHeigherLevel(pSoldier, pSoldier.sGridNo, pSoldier.bDirection, &bNewDirection))
            {
                *pfGoUp = true;
            }
        }

        // IF we are higher...
        if (pSoldier.bLevel > 0)
        {
            if (FindLowerLevel(pSoldier, pSoldier.sGridNo, pSoldier.bDirection, &bNewDirection))
            {
                *pfGoDown = true;
            }
        }



    }

    void RemoveTacticalCursor()
    {
        guiNewUICursor = NO_UICURSOR;
        ErasePath(true);
    }

    ScreenName UIHandlePOPUPMSG(UI_EVENT pUIEvent)
    {
        return (ScreenName.GAME_SCREEN);
    }


    ScreenName UIHandleHCOnTerrain(UI_EVENT pUIEvent)
    {
        uint usMapPos;
        SOLDIERTYPE* pSoldier;

        if (!GetMouseMapPos(&usMapPos))
        {
            return (ScreenName.GAME_SCREEN);
        }

        if (!GetSoldier(&pSoldier, gusSelectedSoldier))
        {
            return (ScreenName.GAME_SCREEN);
        }

        // If we are out of breath, no cursor...
        if (pSoldier.bBreath < OKBREATH && pSoldier.bCollapsed)
        {
            guiNewUICursor = INVALID_ACTION_UICURSOR;
        }
        else
        {
            if (gsOverItemsGridNo != (int)IsometricDefines.NOWHERE && (usMapPos != gsOverItemsGridNo || gsInterfaceLevel != gsOverItemsLevel))
            {
                gsOverItemsGridNo = (int)IsometricDefines.NOWHERE;
                guiPendingOverrideEvent = A_CHANGE_TO_MOVE;
            }
            else
            {
                guiNewUICursor = NORMALHANDCURSOR_UICURSOR;

                UIHandleInteractiveTilesAndItemsOnTerrain(pSoldier, usMapPos, true, false);
            }
        }
        return (ScreenName.GAME_SCREEN);
    }


    ScreenName UIHandleHCGettingItem(UI_EVENT pUIEvent)
    {
        guiNewUICursor = NORMAL_FREEUICURSOR;

        return (ScreenName.GAME_SCREEN);
    }


    ScreenName UIHandleTATalkingMenu(UI_EVENT pUIEvent)
    {
        guiNewUICursor = NORMAL_FREEUICURSOR;

        return (ScreenName.GAME_SCREEN);
    }


    ScreenName UIHandleEXExitSectorMenu(UI_EVENT pUIEvent)
    {
        guiNewUICursor = NORMAL_FREEUICURSOR;

        return (ScreenName.GAME_SCREEN);
    }


    ScreenName UIHandleOpenDoorMenu(UI_EVENT pUIEvent)
    {
        guiNewUICursor = NORMAL_FREEUICURSOR;

        return (ScreenName.GAME_SCREEN);
    }

    void ToggleHandCursorMode(uint* puiNewEvent)
    {
        // Toggle modes			
        if (gCurrentUIMode == HANDCURSOR_MODE)
        {
            *puiNewEvent = A_CHANGE_TO_MOVE;
        }
        else
        {

            *puiNewEvent = M_CHANGE_TO_HANDMODE;
        }
    }

    void ToggleTalkCursorMode(uint* puiNewEvent)
    {
        // Toggle modes			
        if (gCurrentUIMode == TALKCURSOR_MODE)
        {
            *puiNewEvent = A_CHANGE_TO_MOVE;
        }
        else
        {
            *puiNewEvent = T_CHANGE_TO_TALKING;
        }
    }

    void ToggleLookCursorMode(uint* puiNewEvent)
    {
        // Toggle modes			
        if (gCurrentUIMode == LOOKCURSOR_MODE)
        {
            guiPendingOverrideEvent = A_CHANGE_TO_MOVE;
            HandleTacticalUI();
        }
        else
        {
            guiPendingOverrideEvent = LC_CHANGE_TO_LOOK;
            HandleTacticalUI();
        }
    }


    bool UIHandleOnMerc(bool fMovementMode)
    {
        SOLDIERTYPE* pSoldier;
        uint usSoldierIndex;
        uint uiMercFlags;
        uint usMapPos;
        bool fFoundMerc = false;

        if (!GetMouseMapPos(&usMapPos))
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
            fFoundMerc = gfUIFullTargetFound;
            usSoldierIndex = gusUIFullTargetID;
            uiMercFlags = guiUIFullTargetFlags;
        }

        // CHECK IF WE'RE ON A GUY ( EITHER SELECTED, OURS, OR THEIRS
        if (fFoundMerc)
        {
            // Get Soldier
            GetSoldier(&pSoldier, usSoldierIndex);

            if (uiMercFlags & OWNED_MERC)
            {
                // ATE: Check if this is an empty vehicle.....
                //if ( OK_ENTERABLE_VEHICLE( pSoldier ) && GetNumberInVehicle( pSoldier.bVehicleID ) == 0 )
                //{
                //	return( false );
                //}

                // If not unconscious, select
                if (!(uiMercFlags & UNCONSCIOUS_MERC))
                {
                    if (fMovementMode)
                    {
                        // ERASE PATH
                        ErasePath(true);

                        // Show cursor with highlight on selected merc
                        guiNewUICursor = NO_UICURSOR;

                        // IF selected, do selection one
                        if ((uiMercFlags & SELECTED_MERC))
                        {
                            // Add highlight to guy in interface.c
                            gfUIHandleSelection = SELECTED_GUY_SELECTION;

                            if (gpItemPointer == null)
                            {
                                // Don't do this unless we want to

                                // Check if buddy is stationary!
                                if (gAnimControl[pSoldier.usAnimState].uiFlags & ANIM_STATIONARY || pSoldier.fNoAPToFinishMove)
                                {
                                    guiShowUPDownArrows = ARROWS_SHOW_DOWN_BESIDE | ARROWS_SHOW_UP_BESIDE;
                                }
                            }

                        }
                        else
                        {
                            //if ( ( uiMercFlags & ONDUTY_MERC ) && !( uiMercFlags & NOINTERRUPT_MERC ) )
                            if (!(uiMercFlags & NOINTERRUPT_MERC))
                            {
                                // Add highlight to guy in interface.c
                                gfUIHandleSelection = NONSELECTED_GUY_SELECTION;
                            }
                            else
                            {
                                gfUIHandleSelection = ENEMY_GUY_SELECTION;
                                gfUIHandleSelectionAboveGuy = true;
                            }
                        }
                    }
                }

                // If not dead, show above guy!
                if (!(uiMercFlags & DEAD_MERC))
                {
                    if (fMovementMode)
                    {
                        // ERASE PATH
                        ErasePath(true);

                        // Show cursor with highlight on selected merc
                        guiNewUICursor = NO_UICURSOR;

                        gsSelectedGridNo = pSoldier.sGridNo;
                        gsSelectedLevel = pSoldier.bLevel;
                    }

                    gsSelectedGuy = usSoldierIndex;
                    gfUIHandleSelectionAboveGuy = true;
                }

            }
            else if (((uiMercFlags & ENEMY_MERC) || (uiMercFlags & NEUTRAL_MERC)) && (uiMercFlags & VISIBLE_MERC))
            {
                // ATE: If we are a vehicle, let the mouse cursor be a wheel...
                if ((OK_ENTERABLE_VEHICLE(pSoldier)))
                {
                    return (false);
                }
                else
                {
                    if (fMovementMode)
                    {

                        // Check if this guy is on the enemy team....
                        if (!pSoldier.bNeutral && (pSoldier.bSide != gbPlayerNum))
                        {
                            gUIActionModeChangeDueToMouseOver = true;

                            guiPendingOverrideEvent = M_CHANGE_TO_ACTION;
                            // Return false
                            return (false);
                        }
                        else
                        {
                            // ERASE PATH
                            ErasePath(true);

                            // Show cursor with highlight on selected merc
                            guiNewUICursor = NO_UICURSOR;
                            // Show cursor with highlight
                            gfUIHandleSelection = ENEMY_GUY_SELECTION;
                            gsSelectedGridNo = pSoldier.sGridNo;
                            gsSelectedLevel = pSoldier.bLevel;
                        }
                    }

                    gfUIHandleSelectionAboveGuy = true;
                    gsSelectedGuy = usSoldierIndex;
                }
            }
            else
            {
                if (pSoldier.uiStatusFlags & SOLDIER_VEHICLE)
                {
                    return (false);
                }
            }
        }
        else
        {
            gfIgnoreOnSelectedGuy = false;

            return (false);
        }

        return (true);
    }

    ScreenName UIHandleILoadLevel(UI_EVENT pUIEvent)
    {
        return (ScreenName.InitScreen);
    }

    ScreenName UIHandleISoldierDebug(UI_EVENT pUIEvent)
    {
        // Use soldier display pages
        SetDebugRenderHook((RENDER_HOOK)DebugSoldierPage1, 0);
        SetDebugRenderHook((RENDER_HOOK)DebugSoldierPage2, 1);
        SetDebugRenderHook((RENDER_HOOK)DebugSoldierPage3, 2);
        SetDebugRenderHook((RENDER_HOOK)DebugSoldierPage4, 3);
        gCurDebugPage = 1;

        return (ScreenName.DEBUG_SCREEN);
    }

    ScreenName UIHandleILOSDebug(UI_EVENT pUIEvent)
    {
        SetDebugRenderHook((RENDER_HOOK)DebugStructurePage1, 0);
        return (ScreenName.DEBUG_SCREEN);
    }

    ScreenName UIHandleILevelNodeDebug(UI_EVENT pUIEvent)
    {
        SetDebugRenderHook((RENDER_HOOK)DebugLevelNodePage, 0);
        return (ScreenName.DEBUG_SCREEN);
    }

    ScreenName UIHandleIETOnTerrain(UI_EVENT pUIEvent)
    {
        //guiNewUICursor = CANNOT_MOVE_UICURSOR;
        guiNewUICursor = NO_UICURSOR;

        SetCurrentCursorFromDatabase(VIDEO_NO_CURSOR);

        return (ScreenName.GAME_SCREEN);
    }


    void UIHandleSoldierStanceChange(byte ubSoldierID, byte bNewStance)
    {
        SOLDIERTYPE? pSoldier;

        pSoldier = MercPtrs[ubSoldierID];

        // Is this a valid stance for our position?
        if (!IsValidStance(pSoldier, bNewStance))
        {
            if (pSoldier.bCollapsed && pSoldier.bBreath < OKBREATH)
            {
                ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_UI_FEEDBACK, gzLateLocalizedString[4], pSoldier.name);
            }
            else
            {
                if (pSoldier.uiStatusFlags & SOLDIER_VEHICLE)
                {
                    ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_UI_FEEDBACK, TacticalStr[VEHICLES_NO_STANCE_CHANGE_STR]);
                }
                else if (pSoldier.uiStatusFlags & SOLDIER_ROBOT)
                {
                    ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_UI_FEEDBACK, TacticalStr[ROBOT_NO_STANCE_CHANGE_STR]);
                }
                else
                {
                    if (pSoldier.bCollapsed)
                    {
                        ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_UI_FEEDBACK, pMessageStrings[MSG_CANT_CHANGE_STANCE], pSoldier.name);
                    }
                    else
                    {
                        ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_UI_FEEDBACK, TacticalStr[CANNOT_STANCE_CHANGE_STR], pSoldier.name);
                    }
                }
            }
            return;
        }

        // IF turn-based - adjust stance now!
        if (gTacticalStatus.uiFlags & TURNBASED && (gTacticalStatus.uiFlags & INCOMBAT))
        {
            pSoldier.fTurningFromPronePosition = false;

            // Check if we have enough APS
            if (SoldierCanAffordNewStance(pSoldier, bNewStance))
            {
                // Adjust stance
                //ChangeSoldierStance( pSoldier, bNewStance );
                SendChangeSoldierStanceEvent(pSoldier, bNewStance);

                pSoldier.sFinalDestination = pSoldier.sGridNo;
                pSoldier.bGoodContPath = false;

            }
            else
                return;
        }

        // If realtime- change walking animation!
        if ((gTacticalStatus.uiFlags & REALTIME) || !(gTacticalStatus.uiFlags & INCOMBAT))
        {

            // If we are stationary, do something else!
            if (gAnimControl[pSoldier.usAnimState].uiFlags & ANIM_STATIONARY)
            {
                // Change stance normally
                SendChangeSoldierStanceEvent(pSoldier, bNewStance);

            }
            else
            {
                // Pick moving animation based on stance

                // LOCK VARIBLE FOR NO UPDATE INDEX...
                pSoldier.usUIMovementMode = GetMoveStateBasedOnStance(pSoldier, bNewStance);

                if (pSoldier.usUIMovementMode == CRAWLING && gAnimControl[pSoldier.usAnimState].ubEndHeight != ANIM_PRONE)
                {
                    pSoldier.usDontUpdateNewGridNoOnMoveAnimChange = LOCKED_NO_NEWGRIDNO;
                    pSoldier.bPathStored = false;
                }
                else
                {
                    pSoldier.usDontUpdateNewGridNoOnMoveAnimChange = 1;
                }


                ChangeSoldierState(pSoldier, pSoldier.usUIMovementMode, 0, false);

            }
        }

        // Set UI value for soldier
        SetUIbasedOnStance(pSoldier, bNewStance);

        gfUIStanceDifferent = true;

        // ATE: If we are being serviced...stop...
        // InternalReceivingSoldierCancelServices( pSoldier, false );
        InternalGivingSoldierCancelServices(pSoldier, false);
        //gfPlotNewMovement   = true;

    }

    ScreenName UIHandleIETEndTurn(UI_EVENT pUIEvent)
    {
        return (ScreenName.GAME_SCREEN);
    }


    ScreenName UIHandleIGotoDemoMode(UI_EVENT pUIEvent)
    {
        return (EnterTacticalDemoMode());
    }


    ScreenName UIHandleILoadFirstLevel(UI_EVENT pUIEvent)
    {
        gubCurrentScene = 0;
        return (ScreenName.InitScreen);
    }

    ScreenName UIHandleILoadSecondLevel(UI_EVENT pUIEvent)
    {
        gubCurrentScene = 1;
        return (ScreenName.InitScreen);
    }

    ScreenName UIHandleILoadThirdLevel(UI_EVENT pUIEvent)
    {
        gubCurrentScene = 2;
        return (ScreenName.InitScreen);
    }

    ScreenName UIHandleILoadFourthLevel(UI_EVENT pUIEvent)
    {
        gubCurrentScene = 3;
        return (ScreenName.InitScreen);
    }

    ScreenName UIHandleILoadFifthLevel(UI_EVENT pUIEvent)
    {
        gubCurrentScene = 4;
        return (ScreenName.InitScreen);
    }

    static bool fStationary = false;
    static uint usOldMouseXPos = 32000;
    static uint usOldMouseYPos = 32000;
    static uint usOldMapPos = 32000;

    static uint uiSameFrameCursorFlags;
    static uint uiOldFrameNumber = 99999;

    void GetCursorMovementFlags(uint puiCursorFlags)
    {
        uint usMapPos;
        int sXPos, sYPos;


        // Check if this is the same frame as before, return already calculated value if so!
        if (uiOldFrameNumber == guiGameCycleCounter)
        {
            (*puiCursorFlags) = uiSameFrameCursorFlags;
            return;
        }

        GetMouseMapPos(&usMapPos);
        ConvertGridNoToXY(usMapPos, &sXPos, &sYPos);

        *puiCursorFlags = 0;

        if (gusMouseXPos != usOldMouseXPos || gusMouseYPos != usOldMouseYPos)
        {
            (*puiCursorFlags) |= MOUSE_MOVING;

            // IF CURSOR WAS PREVIOUSLY STATIONARY, MAKE THE ADDITIONAL CHECK OF GRID POS CHANGE
            if (fStationary && usOldMapPos == usMapPos)
            {
                (*puiCursorFlags) |= MOUSE_MOVING_IN_TILE;
            }
            else
            {
                fStationary = false;
                (*puiCursorFlags) |= MOUSE_MOVING_NEW_TILE;
            }

        }
        else
        {
            (*puiCursorFlags) |= MOUSE_STATIONARY;
            fStationary = true;
        }

        usOldMapPos = usMapPos;
        usOldMouseXPos = gusMouseXPos;
        usOldMouseYPos = gusMouseYPos;

        uiOldFrameNumber = guiGameCycleCounter;
        uiSameFrameCursorFlags = (*puiCursorFlags);
    }

    static uint usTargetID = OverheadTypes.NOBODY;
    static bool fTargetFound = false;

    bool HandleUIMovementCursor(SOLDIERTYPE? pSoldier, uint uiCursorFlags, uint usMapPos, uint uiFlags)
    {
        bool fSetCursor = false;
        bool fCalculated = false;
        bool fTargetFoundAndLookingForOne = false;
        bool fIntTileFoundAndLookingForOne = false;

        // Determine if we can afford!
        if (!EnoughPoints(pSoldier, gsCurrentActionPoints, 0, false))
        {
            gfUIDisplayActionPointsInvalid = true;
        }

        // Check if we're stationary
        if (((gTacticalStatus.uiFlags & REALTIME) || !(gTacticalStatus.uiFlags & INCOMBAT)) || ((gAnimControl[pSoldier.usAnimState].uiFlags & ANIM_STATIONARY) || pSoldier.fNoAPToFinishMove) || pSoldier.ubID >= MAX_NUM_SOLDIERS)
        {
            // If we are targeting a merc for some reason, don't go thorugh normal channels if we are on someone now
            if (uiFlags == MOVEUI_TARGET.MERCS || uiFlags == MOVEUI_TARGET.MERCSFORAID)
            {
                if (gfUIFullTargetFound != fTargetFound || usTargetID != gusUIFullTargetID || gfResetUIMovementOptimization)
                {
                    gfResetUIMovementOptimization = false;

                    // ERASE PATH
                    ErasePath(true);

                    // Try and get a path right away
                    DrawUIMovementPath(pSoldier, usMapPos, uiFlags);
                }

                // Save for next time...
                fTargetFound = gfUIFullTargetFound;
                usTargetID = gusUIFullTargetID;

                if (fTargetFound)
                {
                    fTargetFoundAndLookingForOne = true;
                }
            }

            if (uiFlags == MOVEUI_TARGET.ITEMS)
            {
                gfUIOverItemPool = true;
                gfUIOverItemPoolGridNo = usMapPos;
            }
            else if (uiFlags == MOVEUI_TARGET.MERCSFORAID)
            {
                // Set values for AP display...
                gfUIDisplayActionPointsCenter = true;
            }

            // IF CURSOR IS MOVING
            if ((uiCursorFlags & MOUSE_MOVING) || gfUINewStateForIntTile)
            {
                // SHOW CURSOR
                fSetCursor = true;

                // IF CURSOR WAS PREVIOUSLY STATIONARY, MAKE THE ADDITIONAL CHECK OF GRID POS CHANGE
                if (((uiCursorFlags & MOUSE_MOVING_NEW_TILE) && !fTargetFoundAndLookingForOne) || gfUINewStateForIntTile)
                {
                    // ERASE PATH
                    ErasePath(true);

                    // Reset counter
                    RESETCOUNTER(PATHFINDCOUNTER);

                    gfPlotNewMovement = true;

                }

                if (uiCursorFlags & MOUSE_MOVING_IN_TILE)
                {
                    gfUIDisplayActionPoints = true;
                }

            }

            if (uiCursorFlags & MOUSE_STATIONARY)
            {
                // CURSOR IS STATIONARY
                if (_KeyDown(SHIFT) && !gfPlotNewMovementNOCOST)
                {
                    gfPlotNewMovementNOCOST = true;
                    gfPlotNewMovement = true;
                }
                if (!(_KeyDown(SHIFT)) && gfPlotNewMovementNOCOST)
                {
                    gfPlotNewMovementNOCOST = false;
                    gfPlotNewMovement = true;
                }


                // ONLY DIPSLAY PATH AFTER A DELAY
                if (COUNTERDONE(PATHFINDCOUNTER))
                {
                    // Reset counter
                    RESETCOUNTER(PATHFINDCOUNTER);

                    if (gfPlotNewMovement)
                    {
                        DrawUIMovementPath(pSoldier, usMapPos, uiFlags);

                        gfPlotNewMovement = false;
                    }
                }

                fSetCursor = true;

                // DISPLAY POINTS EVEN WITHOUT DELAY
                // ONLY IF GFPLOT NEW MOVEMENT IS false!
                if (!gfPlotNewMovement)
                {
                    if (gsCurrentActionPoints < 0 || ((gTacticalStatus.uiFlags & REALTIME) || !(gTacticalStatus.uiFlags & INCOMBAT)))
                    {
                        gfUIDisplayActionPoints = false;
                    }
                    else
                    {
                        gfUIDisplayActionPoints = true;

                        if (uiFlags == MOVEUI_TARGET.INTTILES)
                        {
                            // Set values for AP display...
                            gUIDisplayActionPointsOffX = 22;
                            gUIDisplayActionPointsOffY = 15;
                        }
                        if (uiFlags == MOVEUI_TARGET.BOMB)
                        {
                            // Set values for AP display...
                            gUIDisplayActionPointsOffX = 22;
                            gUIDisplayActionPointsOffY = 15;
                        }
                        else if (uiFlags == MOVEUI_TARGET.ITEMS)
                        {
                            // Set values for AP display...
                            gUIDisplayActionPointsOffX = 22;
                            gUIDisplayActionPointsOffY = 15;
                        }
                        else
                        {
                            switch (pSoldier.usUIMovementMode)
                            {
                                case WALKING:

                                    gUIDisplayActionPointsOffY = 10;
                                    gUIDisplayActionPointsOffX = 10;
                                    break;

                                case RUNNING:
                                    gUIDisplayActionPointsOffY = 15;
                                    gUIDisplayActionPointsOffX = 21;
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
            ErasePath(true);

            fSetCursor = true;

        }

        return (fSetCursor);
    }



    byte DrawUIMovementPath(SOLDIERTYPE? pSoldier, uint usMapPos, uint uiFlags)
    {
        int sAPCost, sBPCost;
        int sActionGridNo;
        STRUCTURE* pStructure;
        bool fOnInterTile = false;
        byte ubDirection;
        //	ITEM_POOL					*pItemPool;
        int sAdjustedGridNo;
        int sIntTileGridNo;
        LEVELNODE* pIntTile;
        byte bReturnCode = 0;
        bool fPlot;
        byte ubMercID;

        if ((gTacticalStatus.uiFlags & INCOMBAT) && (gTacticalStatus.uiFlags & TURNBASED) || _KeyDown(SHIFT))
        {
            fPlot = PLOT;
        }
        else
        {
            fPlot = NO_PLOT;
        }

        sActionGridNo = usMapPos;
        sAPCost = 0;

        ErasePath(false);

        // IF WE ARE OVER AN INTERACTIVE TILE, GIVE GRIDNO OF POSITION
        if (uiFlags == MOVEUI_TARGET.INTTILES)
        {
            // Get structure info for in tile!
            pIntTile = GetCurInteractiveTileGridNoAndStructure(&sIntTileGridNo, &pStructure);

            // We should not have null here if we are given this flag...
            if (pIntTile != null)
            {
                sActionGridNo = FindAdjacentGridEx(pSoldier, sIntTileGridNo, &ubDirection, null, false, true);
                if (sActionGridNo == -1)
                {
                    sActionGridNo = sIntTileGridNo;
                }
                CalcInteractiveObjectAPs(sIntTileGridNo, pStructure, &sAPCost, &sBPCost);
                //sAPCost += UIPlotPath( pSoldier, sActionGridNo, NO_COPYROUTE, PLOT, TEMPORARY, (uint)pSoldier.usUIMovementMode, NOT_STEALTH, FORWARD, pSoldier.bActionPoints);
                sAPCost += UIPlotPath(pSoldier, sActionGridNo, NO_COPYROUTE, fPlot, TEMPORARY, (uint)pSoldier.usUIMovementMode, NOT_STEALTH, FORWARD, pSoldier.bActionPoints);

                if (sActionGridNo != pSoldier.sGridNo)
                {
                    gfUIHandleShowMoveGrid = true;
                    gsUIHandleShowMoveGridLocation = sActionGridNo;
                }

                // Add cost for stance change....
                sAPCost += GetAPsToChangeStance(pSoldier, ANIM_STAND);
            }
            else
            {
                sAPCost += UIPlotPath(pSoldier, sActionGridNo, NO_COPYROUTE, fPlot, TEMPORARY, (uint)pSoldier.usUIMovementMode, NOT_STEALTH, FORWARD, pSoldier.bActionPoints);
            }
        }
        else if (uiFlags == MOVEUI_TARGET.WIREFENCE)
        {
            sActionGridNo = FindAdjacentGridEx(pSoldier, usMapPos, &ubDirection, null, false, true);
            if (sActionGridNo == -1)
            {
                sAPCost = 0;
            }
            else
            {
                sAPCost = GetAPsToCutFence(pSoldier);

                sAPCost += UIPlotPath(pSoldier, sActionGridNo, NO_COPYROUTE, fPlot, TEMPORARY, (uint)pSoldier.usUIMovementMode, NOT_STEALTH, FORWARD, pSoldier.bActionPoints);

                if (sActionGridNo != pSoldier.sGridNo)
                {
                    gfUIHandleShowMoveGrid = true;
                    gsUIHandleShowMoveGridLocation = sActionGridNo;
                }
            }
        }
        else if (uiFlags == MOVEUI_TARGET.JAR)
        {
            sActionGridNo = FindAdjacentGridEx(pSoldier, usMapPos, &ubDirection, null, false, true);
            if (sActionGridNo == -1)
            {
                sActionGridNo = usMapPos;
            }

            sAPCost = GetAPsToUseJar(pSoldier, sActionGridNo);

            sAPCost += UIPlotPath(pSoldier, sActionGridNo, NO_COPYROUTE, fPlot, TEMPORARY, (uint)pSoldier.usUIMovementMode, NOT_STEALTH, FORWARD, pSoldier.bActionPoints);

            if (sActionGridNo != pSoldier.sGridNo)
            {
                gfUIHandleShowMoveGrid = true;
                gsUIHandleShowMoveGridLocation = sActionGridNo;
            }
        }
        else if (uiFlags == MOVEUI_TARGET.CAN)
        {
            // Get structure info for in tile!
            pIntTile = GetCurInteractiveTileGridNoAndStructure(&sIntTileGridNo, &pStructure);

            // We should not have null here if we are given this flag...
            if (pIntTile != null)
            {
                sActionGridNo = FindAdjacentGridEx(pSoldier, sIntTileGridNo, &ubDirection, null, false, true);
                if (sActionGridNo != -1)
                {
                    sAPCost = AP_ATTACH_CAN;
                    sAPCost += UIPlotPath(pSoldier, sActionGridNo, NO_COPYROUTE, fPlot, TEMPORARY, (uint)pSoldier.usUIMovementMode, NOT_STEALTH, FORWARD, pSoldier.bActionPoints);

                    if (sActionGridNo != pSoldier.sGridNo)
                    {
                        gfUIHandleShowMoveGrid = true;
                        gsUIHandleShowMoveGridLocation = sActionGridNo;
                    }
                }
            }
            else
            {
                sAPCost += UIPlotPath(pSoldier, sActionGridNo, NO_COPYROUTE, fPlot, TEMPORARY, (uint)pSoldier.usUIMovementMode, NOT_STEALTH, FORWARD, pSoldier.bActionPoints);
            }

        }
        else if (uiFlags == MOVEUI_TARGET.REPAIR)
        {
            // For repair, check if we are over a vehicle, then get gridnot to edge of that vehicle!
            if (IsRepairableStructAtGridNo(usMapPos, &ubMercID) == 2)
            {
                int sNewGridNo;
                byte ubDirection;

                sNewGridNo = FindGridNoFromSweetSpotWithStructDataFromSoldier(pSoldier, pSoldier.usUIMovementMode, 5, &ubDirection, 0, MercPtrs[ubMercID]);

                if (sNewGridNo != (int)IsometricDefines.NOWHERE)
                {
                    usMapPos = sNewGridNo;
                }
            }

            sActionGridNo = FindAdjacentGridEx(pSoldier, usMapPos, &ubDirection, null, false, true);
            if (sActionGridNo == -1)
            {
                sActionGridNo = usMapPos;
            }

            sAPCost = GetAPsToBeginRepair(pSoldier);

            sAPCost += UIPlotPath(pSoldier, sActionGridNo, NO_COPYROUTE, fPlot, TEMPORARY, (uint)pSoldier.usUIMovementMode, NOT_STEALTH, FORWARD, pSoldier.bActionPoints);

            if (sActionGridNo != pSoldier.sGridNo)
            {
                gfUIHandleShowMoveGrid = true;
                gsUIHandleShowMoveGridLocation = sActionGridNo;
            }
        }
        else if (uiFlags == MOVEUI_TARGET.REFUEL)
        {
            // For repair, check if we are over a vehicle, then get gridnot to edge of that vehicle!
            if (IsRefuelableStructAtGridNo(usMapPos, &ubMercID) == 2)
            {
                int sNewGridNo;
                byte ubDirection;

                sNewGridNo = FindGridNoFromSweetSpotWithStructDataFromSoldier(pSoldier, pSoldier.usUIMovementMode, 5, &ubDirection, 0, MercPtrs[ubMercID]);

                if (sNewGridNo != (int)IsometricDefines.NOWHERE)
                {
                    usMapPos = sNewGridNo;
                }
            }

            sActionGridNo = FindAdjacentGridEx(pSoldier, usMapPos, &ubDirection, null, false, true);
            if (sActionGridNo == -1)
            {
                sActionGridNo = usMapPos;
            }

            sAPCost = GetAPsToRefuelVehicle(pSoldier);

            sAPCost += UIPlotPath(pSoldier, sActionGridNo, NO_COPYROUTE, fPlot, TEMPORARY, (uint)pSoldier.usUIMovementMode, NOT_STEALTH, FORWARD, pSoldier.bActionPoints);

            if (sActionGridNo != pSoldier.sGridNo)
            {
                gfUIHandleShowMoveGrid = true;
                gsUIHandleShowMoveGridLocation = sActionGridNo;
            }
        }
        else if (uiFlags == MOVEUI_TARGET.MERCS)
        {
            int sGotLocation = (int)IsometricDefines.NOWHERE;
            bool fGotAdjacent = false;

            // Check if we are on a target
            if (gfUIFullTargetFound)
            {
                int cnt;
                int sSpot;
                byte ubGuyThere;

                for (cnt = 0; cnt < NUM_WORLD_DIRECTIONS; cnt++)
                {
                    sSpot = (int)NewGridNo(pSoldier.sGridNo, DirectionInc((byte)cnt));

                    // Make sure movement costs are OK....
                    if (gubWorldMovementCosts[sSpot][cnt][gsInterfaceLevel] >= TRAVELCOST_BLOCKED)
                    {
                        continue;
                    }


                    // Check for who is there...
                    ubGuyThere = WhoIsThere2(sSpot, pSoldier.bLevel);

                    if (ubGuyThere == MercPtrs[gusUIFullTargetID].ubID)
                    {
                        // We've got a guy here....
                        // Who is the one we want......
                        sGotLocation = sSpot;
                        sAdjustedGridNo = MercPtrs[gusUIFullTargetID].sGridNo;
                        ubDirection = (byte)cnt;
                        break;
                    }
                }

                if (sGotLocation == (int)IsometricDefines.NOWHERE)
                {
                    sActionGridNo = FindAdjacentGridEx(pSoldier, MercPtrs[gusUIFullTargetID].sGridNo, &ubDirection, &sAdjustedGridNo, true, false);

                    if (sActionGridNo == -1)
                    {
                        sGotLocation = (int)IsometricDefines.NOWHERE;
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

            if (sGotLocation != (int)IsometricDefines.NOWHERE)
            {
                sAPCost += MinAPsToAttack(pSoldier, sAdjustedGridNo, true);
                sAPCost += UIPlotPath(pSoldier, sGotLocation, NO_COPYROUTE, fPlot, TEMPORARY, (uint)pSoldier.usUIMovementMode, NOT_STEALTH, FORWARD, pSoldier.bActionPoints);

                if (sGotLocation != pSoldier.sGridNo && fGotAdjacent)
                {
                    gfUIHandleShowMoveGrid = true;
                    gsUIHandleShowMoveGridLocation = sGotLocation;
                }
            }
        }
        else if (uiFlags == MOVEUI_TARGET.STEAL)
        {
            // Check if we are on a target
            if (gfUIFullTargetFound)
            {
                sActionGridNo = FindAdjacentGridEx(pSoldier, MercPtrs[gusUIFullTargetID].sGridNo, &ubDirection, &sAdjustedGridNo, true, false);
                if (sActionGridNo == -1)
                {
                    sActionGridNo = sAdjustedGridNo;
                }
                sAPCost += AP_STEAL_ITEM;
                // CJC August 13 2002: take into account stance in AP prediction
                if (!(PTR_STANDING))
                {
                    sAPCost += GetAPsToChangeStance(pSoldier, ANIM_STAND);
                }
                sAPCost += UIPlotPath(pSoldier, sActionGridNo, NO_COPYROUTE, fPlot, TEMPORARY, (uint)pSoldier.usUIMovementMode, NOT_STEALTH, FORWARD, pSoldier.bActionPoints);

                if (sActionGridNo != pSoldier.sGridNo)
                {
                    gfUIHandleShowMoveGrid = true;
                    gsUIHandleShowMoveGridLocation = sActionGridNo;
                }
            }
        }
        else if (uiFlags == MOVEUI_TARGET.BOMB)
        {
            sAPCost += GetAPsToDropBomb(pSoldier);
            sAPCost += UIPlotPath(pSoldier, usMapPos, NO_COPYROUTE, fPlot, TEMPORARY, (uint)pSoldier.usUIMovementMode, NOT_STEALTH, FORWARD, pSoldier.bActionPoints);

            gfUIHandleShowMoveGrid = true;
            gsUIHandleShowMoveGridLocation = usMapPos;
        }
        else if (uiFlags == MOVEUI_TARGET.MERCSFORAID)
        {
            if (gfUIFullTargetFound)
            {
                sActionGridNo = FindAdjacentGridEx(pSoldier, MercPtrs[gusUIFullTargetID].sGridNo, &ubDirection, &sAdjustedGridNo, true, false);

                // Try again at another gridno...
                if (sActionGridNo == -1)
                {
                    sActionGridNo = FindAdjacentGridEx(pSoldier, usMapPos, &ubDirection, &sAdjustedGridNo, true, false);

                    if (sActionGridNo == -1)
                    {
                        sActionGridNo = sAdjustedGridNo;
                    }
                }
                sAPCost += GetAPsToBeginFirstAid(pSoldier);
                sAPCost += UIPlotPath(pSoldier, sActionGridNo, NO_COPYROUTE, fPlot, TEMPORARY, (uint)pSoldier.usUIMovementMode, NOT_STEALTH, FORWARD, pSoldier.bActionPoints);
                if (sActionGridNo != pSoldier.sGridNo)
                {
                    gfUIHandleShowMoveGrid = true;
                    gsUIHandleShowMoveGridLocation = sActionGridNo;
                }
            }
        }
        else if (uiFlags == MOVEUI_TARGET.ITEMS)
        {
            //if ( GetItemPool( usMapPos, &pItemPool, pSoldier.bLevel ) )
            {
                //if ( ITEMPOOL_VISIBLE( pItemPool ) )
                {
                    sActionGridNo = AdjustGridNoForItemPlacement(pSoldier, sActionGridNo);

                    if (pSoldier.sGridNo != sActionGridNo)
                    {
                        sAPCost += UIPlotPath(pSoldier, sActionGridNo, NO_COPYROUTE, fPlot, TEMPORARY, (uint)pSoldier.usUIMovementMode, NOT_STEALTH, FORWARD, pSoldier.bActionPoints);
                        if (sAPCost != 0)
                        {
                            sAPCost += AP_PICKUP_ITEM;
                        }
                    }
                    else
                    {
                        sAPCost += AP_PICKUP_ITEM;
                    }

                    if (sActionGridNo != pSoldier.sGridNo)
                    {
                        gfUIHandleShowMoveGrid = true;
                        gsUIHandleShowMoveGridLocation = sActionGridNo;
                    }

                }
            }
        }
        else
        {
            sAPCost += UIPlotPath(pSoldier, sActionGridNo, NO_COPYROUTE, fPlot, TEMPORARY, (uint)pSoldier.usUIMovementMode, NOT_STEALTH, FORWARD, pSoldier.bActionPoints);
        }

        if (gTacticalStatus.uiFlags & SHOW_AP_LEFT)
        {
            gsCurrentActionPoints = pSoldier.bActionPoints - sAPCost;
        }
        else
        {
            gsCurrentActionPoints = sAPCost;
        }

        return (bReturnCode);
    }


    bool UIMouseOnValidAttackLocation(SOLDIERTYPE* pSoldier)
    {
        uint usInHand;
        bool fGuyHere = false;
        SOLDIERTYPE* pTSoldier;
        byte ubItemCursor;
        uint usMapPos;

        if (!GetMouseMapPos(&usMapPos))
        {
            return (false);
        }

        // LOOK IN GUY'S HAND TO CHECK LOCATION
        usInHand = pSoldier.inv[HANDPOS].usItem;

        // Get cursor value
        ubItemCursor = GetActionModeCursor(pSoldier);

        if (ubItemCursor == INVALIDCURS)
        {
            return (false);
        }


        if (ubItemCursor == WIRECUTCURS)
        {
            if (IsCuttableWireFenceAtGridNo(usMapPos) && pSoldier.bLevel == 0)
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }

        if (ubItemCursor == REPAIRCURS)
        {
            if (gfUIFullTargetFound)
            {
                usMapPos = MercPtrs[gusUIFullTargetID].sGridNo;
            }

            if (IsRepairableStructAtGridNo(usMapPos, null) && pSoldier.bLevel == 0)
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }

        if (ubItemCursor == REFUELCURS)
        {
            if (gfUIFullTargetFound)
            {
                usMapPos = MercPtrs[gusUIFullTargetID].sGridNo;
            }

            if (IsRefuelableStructAtGridNo(usMapPos, null) && pSoldier.bLevel == 0)
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }

        if (ubItemCursor == BOMBCURS)
        {
            if (usMapPos == pSoldier.sGridNo)
            {
                return (true);
            }

            if (!NewOKDestination(pSoldier, usMapPos, true, pSoldier.bLevel))
            {
                return (false);
            }
        }

        // SEE IF THERE IS SOMEBODY HERE
        if (gfUIFullTargetFound && ubItemCursor != KNIFECURS)
        {
            fGuyHere = true;

            if (guiUIFullTargetFlags & SELECTED_MERC && Item[usInHand].usItemClass != IC_MEDKIT)
            {
                return (false);
            }
        }

        if (!CanPlayerUseRocketRifle(pSoldier, true))
        {
            return (false);
        }

        //if ( Item[ usInHand ].usItemClass == IC_BLADE && usInHand != THROWING_KNIFE )	
        //{
        //	if ( !fGuyHere )
        //	{
        //	return( false );
        //	}
        //}

        if (Item[usInHand].usItemClass == IC_PUNCH)
        {
            if (!fGuyHere)
            {
                return (false);
            }
        }

        //if ( Item[ usInHand ].usItemClass == IC_BLADE )	
        //{
        //	if ( !fGuyHere )
        //	{
        //		return( false );
        //	}
        //}

        if (Item[usInHand].usItemClass == IC_MEDKIT)
        {
            if (!fGuyHere)
            {
                return (false);
            }

            // IF a guy's here, chack if they need medical help!
            pTSoldier = MercPtrs[gusUIFullTargetID];

            // If we are a vehicle...
            if ((pTSoldier.uiStatusFlags & (SOLDIER_VEHICLE | SOLDIER_ROBOT)))
            {
                ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_UI_FEEDBACK, TacticalStr[CANNOT_DO_FIRST_AID_STR], pTSoldier.name);
                return (false);
            }

            if (pSoldier.bMedical == 0)
            {
                ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_UI_FEEDBACK, pMessageStrings[MSG_MERC_HAS_NO_MEDSKILL], pSoldier.name);
                return (false);
            }

            if (pTSoldier.bBleeding == 0 && pTSoldier.bLife != pTSoldier.bLifeMax)
            {
                ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_UI_FEEDBACK, gzLateLocalizedString[19], pTSoldier.name);
                return (false);
            }

            if (pTSoldier.bBleeding == 0 && pTSoldier.bLife >= OKLIFE)
            {
                ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_UI_FEEDBACK, TacticalStr[CANNOT_NO_NEED_FIRST_AID_STR], pTSoldier.name);
                return (false);
            }

        }

        return (true);
    }


    bool UIOkForItemPickup(SOLDIERTYPE* pSoldier, int sGridNo)
    {
        int sAPCost;
        ITEM_POOL* pItemPool;

        sAPCost = GetAPsToPickupItem(pSoldier, sGridNo);

        if (sAPCost == 0)
        {
            ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_UI_FEEDBACK, TacticalStr[NO_PATH]);
        }
        else
        {
            if (GetItemPool(sGridNo, &pItemPool, pSoldier.bLevel))
            {
                //if ( !ITEMPOOL_VISIBLE( pItemPool ) )
                {
                    //		return( false );
                }
            }

            if (EnoughPoints(pSoldier, sAPCost, 0, true))
            {
                return (true);
            }
        }

        return (false);
    }


    bool SoldierCanAffordNewStance(SOLDIERTYPE* pSoldier, byte ubDesiredStance)
    {
        byte bCurrentHeight;
        byte bAP = 0, bBP = 0;

        bCurrentHeight = (ubDesiredStance - gAnimControl[pSoldier.usAnimState].ubEndHeight);

        // Now change to appropriate animation

        switch (bCurrentHeight)
        {
            case ANIM_STAND - ANIM_CROUCH:
            case ANIM_CROUCH - ANIM_STAND:

                bAP = AP_CROUCH;
                bBP = BP_CROUCH;
                break;

            case ANIM_STAND - ANIM_PRONE:
            case ANIM_PRONE - ANIM_STAND:

                bAP = AP_CROUCH + AP_PRONE;
                bBP = BP_CROUCH + BP_PRONE;
                break;

            case ANIM_CROUCH - ANIM_PRONE:
            case ANIM_PRONE - ANIM_CROUCH:

                bAP = AP_PRONE;
                bBP = BP_PRONE;
                break;

        }

        return (EnoughPoints(pSoldier, bAP, bBP, true));
    }

    void SetUIbasedOnStance(SOLDIERTYPE* pSoldier, byte bNewStance)
    {
        // Set UI based on our stance!
        switch (bNewStance)
        {
            case ANIM_STAND:
                pSoldier.usUIMovementMode = WALKING;
                break;

            case ANIM_CROUCH:
                pSoldier.usUIMovementMode = SWATTING;
                break;

            case ANIM_PRONE:
                pSoldier.usUIMovementMode = CRAWLING;
                break;
        }

        // Set UI cursor!

    }


    void SetMovementModeCursor(SOLDIERTYPE* pSoldier)
    {
        if (gTacticalStatus.uiFlags & TURNBASED && (gTacticalStatus.uiFlags & INCOMBAT))
        {
            if ((OK_ENTERABLE_VEHICLE(pSoldier)))
            {
                guiNewUICursor = MOVE_VEHICLE_UICURSOR;
            }
            else
            {
                // Change mouse cursor based on type of movement we want to do
                switch (pSoldier.usUIMovementMode)
                {
                    case WALKING:
                        guiNewUICursor = MOVE_WALK_UICURSOR;
                        break;

                    case RUNNING:
                        guiNewUICursor = MOVE_RUN_UICURSOR;
                        break;

                    case SWATTING:
                        guiNewUICursor = MOVE_SWAT_UICURSOR;
                        break;

                    case CRAWLING:
                        guiNewUICursor = MOVE_PRONE_UICURSOR;
                        break;
                }
            }
        }

        if ((gTacticalStatus.uiFlags & REALTIME) || !(gTacticalStatus.uiFlags & INCOMBAT))
        {
            if (gfUIAllMoveOn)
            {
                guiNewUICursor = ALL_MOVE_REALTIME_UICURSOR;
            }
            else
            {
                //if ( pSoldier.fUIMovementFast )
                //{
                //	BeginDisplayTimedCursor( MOVE_RUN_REALTIME_UICURSOR, 300 );
                //}

                guiNewUICursor = MOVE_REALTIME_UICURSOR;
            }
        }

        guiNewUICursor = GetInteractiveTileCursor(guiNewUICursor, false);

    }



    void SetConfirmMovementModeCursor(SOLDIERTYPE pSoldier, bool fFromMove)
    {
        if (gTacticalStatus.uiFlags & TURNBASED && (gTacticalStatus.uiFlags & INCOMBAT))
        {
            if (gfUIAllMoveOn)
            {
                if ((OK_ENTERABLE_VEHICLE(pSoldier)))
                {
                    guiNewUICursor = ALL_MOVE_VEHICLE_UICURSOR;
                }
                else
                {
                    // Change mouse cursor based on type of movement we want to do
                    switch (pSoldier.usUIMovementMode)
                    {
                        case WALKING:
                            guiNewUICursor = ALL_MOVE_WALK_UICURSOR;
                            break;

                        case RUNNING:
                            guiNewUICursor = ALL_MOVE_RUN_UICURSOR;
                            break;

                        case SWATTING:
                            guiNewUICursor = ALL_MOVE_SWAT_UICURSOR;
                            break;

                        case CRAWLING:
                            guiNewUICursor = ALL_MOVE_PRONE_UICURSOR;
                            break;
                    }
                }
            }
            else
            {
                if (pSoldier.uiStatusFlags & SOLDIER_VEHICLE)
                {
                    guiNewUICursor = CONFIRM_MOVE_VEHICLE_UICURSOR;
                }
                else
                {
                    // Change mouse cursor based on type of movement we want to do
                    switch (pSoldier.usUIMovementMode)
                    {
                        case WALKING:
                            guiNewUICursor = CONFIRM_MOVE_WALK_UICURSOR;
                            break;

                        case RUNNING:
                            guiNewUICursor = CONFIRM_MOVE_RUN_UICURSOR;
                            break;

                        case SWATTING:
                            guiNewUICursor = CONFIRM_MOVE_SWAT_UICURSOR;
                            break;

                        case CRAWLING:
                            guiNewUICursor = CONFIRM_MOVE_PRONE_UICURSOR;
                            break;
                    }
                }
            }
        }

        if ((gTacticalStatus.uiFlags & REALTIME) || !(gTacticalStatus.uiFlags & INCOMBAT))
        {
            if (gfUIAllMoveOn)
            {
                if (gfUIAllMoveOn == 2)
                {
                    BeginDisplayTimedCursor(MOVE_RUN_REALTIME_UICURSOR, 300);
                }
                else
                {
                    guiNewUICursor = ALL_MOVE_REALTIME_UICURSOR;
                }
            }
            else
            {
                if (pSoldier.fUIMovementFast && pSoldier.usAnimState == RUNNING && fFromMove)
                {
                    BeginDisplayTimedCursor(MOVE_RUN_REALTIME_UICURSOR, 300);
                }

                guiNewUICursor = CONFIRM_MOVE_REALTIME_UICURSOR;
            }
        }

        guiNewUICursor = GetInteractiveTileCursor(guiNewUICursor, true);

    }


    ScreenName UIHandleLCOnTerrain(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE pSoldier;
        int sFacingDir, sXPos, sYPos;

        guiNewUICursor = LOOK_UICURSOR;

        // Get soldier
        if (!GetSoldier(&pSoldier, gusSelectedSoldier))
        {
            return ScreenName.GAME_SCREEN;
        }

        gfUIDisplayActionPoints = true;

        gUIDisplayActionPointsOffX = 14;
        gUIDisplayActionPointsOffY = 7;


        // Get soldier
        if (!GetSoldier(&pSoldier, gusSelectedSoldier))
        {
            return (ScreenName.GAME_SCREEN);
        }

        GetMouseXY(&sXPos, &sYPos);

        // Get direction from mouse pos
        sFacingDir = GetDirectionFromXY(sXPos, sYPos, pSoldier);

        // Set # of APs
        if (sFacingDir != pSoldier.bDirection)
        {
            gsCurrentActionPoints = GetAPsToLook(pSoldier);
        }
        else
        {
            gsCurrentActionPoints = 0;
        }

        // Determine if we can afford!
        if (!EnoughPoints(pSoldier, gsCurrentActionPoints, 0, false))
        {
            gfUIDisplayActionPointsInvalid = true;
        }

        return (ScreenName.GAME_SCREEN);

    }


    ScreenName UIHandleLCChangeToLook(UI_EVENT pUIEvent)
    {
        ErasePath(true);

        return (ScreenName.GAME_SCREEN);
    }


    bool MakeSoldierTurn(SOLDIERTYPE* pSoldier, int sXPos, int sYPos)
    {
        int sFacingDir, sAPCost;

        // Get direction from mouse pos
        sFacingDir = GetDirectionFromXY(sXPos, sYPos, pSoldier);

        if (sFacingDir != pSoldier.bDirection)
        {
            sAPCost = GetAPsToLook(pSoldier);

            // Check AP cost...
            if (!EnoughPoints(pSoldier, sAPCost, 0, true))
            {
                return (false);
            }

            // ATE: make stationary if...
            if (pSoldier.fNoAPToFinishMove)
            {
                SoldierGotoStationaryStance(pSoldier);
            }

            //DEF:  made it an event
            SendSoldierSetDesiredDirectionEvent(pSoldier, sFacingDir);

            pSoldier.bTurningFromUI = true;

            // ATE: Hard-code here previous event to ui busy event...
            guiOldEvent = LA_BEGINUIOURTURNLOCK;

            return (true);
        }

        return (false);
    }


    ScreenName UIHandleLCLook(UI_EVENT pUIEvent)
    {
        int sXPos, sYPos;
        SOLDIERTYPE* pSoldier;
        int cnt;
        SOLDIERTYPE* pFirstSoldier = null;


        if (!GetMouseXY(&sXPos, &sYPos))
        {
            return (ScreenName.GAME_SCREEN);
        }

        if (gTacticalStatus.fAtLeastOneGuyOnMultiSelect)
        {
            // OK, loop through all guys who are 'multi-selected' and
            cnt = gTacticalStatus.Team[gbPlayerNum].bFirstID;
            for (pSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[gbPlayerNum].bLastID; cnt++, pSoldier++)
            {
                if (pSoldier.bActive && pSoldier.bInSector)
                {
                    if (pSoldier.uiStatusFlags & SOLDIER_MULTI_SELECTED)
                    {
                        MakeSoldierTurn(pSoldier, sXPos, sYPos);
                    }
                }
            }
        }
        else
        {
            // Get soldier
            if (!GetSoldier(&pSoldier, gusSelectedSoldier))
            {
                return (ScreenName.GAME_SCREEN);
            }

            if (MakeSoldierTurn(pSoldier, sXPos, sYPos))
            {
                SetUIBusy(pSoldier.ubID);
            }
        }
        return (ScreenName.GAME_SCREEN);
    }



    ScreenName UIHandleTOnTerrain(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE* pSoldier;
        byte ubTargID;
        uint uiRange;
        uint usMapPos;
        bool fValidTalkableGuy = false;
        int sTargetGridNo;
        int sDistVisible;

        // Get soldier
        if (!GetSoldier(&pSoldier, gusSelectedSoldier))
        {
            return (ScreenName.GAME_SCREEN);
        }

        if (!GetMouseMapPos(&usMapPos))
        {
            return (ScreenName.GAME_SCREEN);
        }

        if (ValidQuickExchangePosition())
        {
            // Do new cursor!
            guiPendingOverrideEvent = M_ON_TERRAIN;
            return (UIHandleMOnTerrain(pUIEvent));
        }

        sTargetGridNo = usMapPos;


        UIHandleOnMerc(false);


        //CHECK FOR VALID TALKABLE GUY HERE
        fValidTalkableGuy = IsValidTalkableNPCFromMouse(&ubTargID, false, true, false);

        // USe cursor based on distance
        // Get distance away
        if (fValidTalkableGuy)
        {
            sTargetGridNo = MercPtrs[ubTargID].sGridNo;
        }

        uiRange = GetRangeFromGridNoDiff(pSoldier.sGridNo, sTargetGridNo);


        //ATE: Check if we have good LOS
        // is he close enough to see that gridno if he turns his head?
        sDistVisible = DistanceVisible(pSoldier, DIRECTION_IRRELEVANT, DIRECTION_IRRELEVANT, sTargetGridNo, pSoldier.bLevel);


        if (uiRange <= NPC_TALK_RADIUS)
        {
            if (fValidTalkableGuy)
            {
                guiNewUICursor = TALK_A_UICURSOR;
            }
            else
            {
                guiNewUICursor = TALK_NA_UICURSOR;
            }
        }
        else
        {
            if (fValidTalkableGuy)
            {
                //guiNewUICursor = TALK_OUT_RANGE_A_UICURSOR;		
                guiNewUICursor = TALK_A_UICURSOR;
            }
            else
            {
                guiNewUICursor = TALK_OUT_RANGE_NA_UICURSOR;
            }
        }

        if (fValidTalkableGuy)
        {
            if (!SoldierTo3DLocationLineOfSightTest(pSoldier, sTargetGridNo, pSoldier.bLevel, 3, (byte)sDistVisible, true))
            {
                //. ATE: Make range far, so we alternate cursors...
                guiNewUICursor = TALK_OUT_RANGE_A_UICURSOR;
            }
        }

        gfUIDisplayActionPoints = true;

        gUIDisplayActionPointsOffX = 8;
        gUIDisplayActionPointsOffY = 3;

        // Set # of APs
        gsCurrentActionPoints = 6;

        // Determine if we can afford!
        if (!EnoughPoints(pSoldier, gsCurrentActionPoints, 0, false))
        {
            gfUIDisplayActionPointsInvalid = true;
        }

        if (!(gTacticalStatus.uiFlags & INCOMBAT))
        {
            if (gfUIFullTargetFound)
            {
                PauseRT(true);
            }
            else
            {
                PauseRT(false);
            }
        }

        return (ScreenName.GAME_SCREEN);

    }


    ScreenName UIHandleTChangeToTalking(UI_EVENT pUIEvent)
    {
        ErasePath(true);

        return (ScreenName.GAME_SCREEN);
    }


    ScreenName UIHandleLUIOnTerrain(UI_EVENT pUIEvent)
    {
        //guiNewUICursor = NO_UICURSOR;
        //	SetCurrentCursorFromDatabase( VIDEO_NO_CURSOR );

        return (ScreenName.GAME_SCREEN);
    }


    ScreenName UIHandleLUIBeginLock(UI_EVENT pUIEvent)
    {
        // Don't let both versions of the locks to happen at the same time!
        // ( They are mutually exclusive )!
        UIHandleLAEndLockOurTurn(null);

        if (!gfDisableRegionActive)
        {
            gfDisableRegionActive = true;

            RemoveTacticalCursor();
            //SetCurrentCursorFromDatabase( VIDEO_NO_CURSOR );

            MSYS_DefineRegion(&gDisableRegion, 0, 0, 640, 480, MSYS_PRIORITY_HIGHEST,
                                 CURSOR_WAIT, MSYS_NO_CALLBACK, MSYS_NO_CALLBACK);
            // Add region
            MSYS_AddRegion(&gDisableRegion);

            //guiPendingOverrideEvent = LOCKUI_MODE;

            // UnPause time!
            PauseGame();
            LockPauseState(16);

        }

        return (ScreenName.GAME_SCREEN);
    }

    ScreenName UIHandleLUIEndLock(UI_EVENT pUIEvent)
    {
        if (gfDisableRegionActive)
        {
            gfDisableRegionActive = false;

            // Add region
            MSYS_RemoveRegion(&gDisableRegion);
            RefreshMouseRegions();

            //SetCurrentCursorFromDatabase( guiCurrentUICursor );

            guiForceRefreshMousePositionCalculation = true;
            UIHandleMOnTerrain(null);

            if (gViewportRegion.uiFlags & MSYS_MOUSE_IN_AREA)
            {
                SetCurrentCursorFromDatabase(gUICursors[guiNewUICursor].usFreeCursorName);
            }

            guiPendingOverrideEvent = M_ON_TERRAIN;
            HandleTacticalUI();

            // ATE: Only if NOT in conversation!
            if (!(gTacticalStatus.uiFlags & ENGAGED_IN_CONV))
            {
                // UnPause time!
                UnLockPauseState();
                UnPauseGame();
            }
        }

        return (ScreenName.GAME_SCREEN);
    }


    void CheckForDisabledRegionRemove()
    {
        if (gfDisableRegionActive)
        {
            gfDisableRegionActive = false;

            // Remove region
            MSYS_RemoveRegion(&gDisableRegion);

            UnLockPauseState();
            UnPauseGame();

        }

        if (gfUserTurnRegionActive)
        {
            gfUserTurnRegionActive = false;

            gfUIInterfaceSetBusy = false;

            // Remove region
            MSYS_RemoveRegion(&gUserTurnRegion);

            UnLockPauseState();
            UnPauseGame();
        }
    }

    ScreenName UIHandleLAOnTerrain(UI_EVENT pUIEvent)
    {
        //guiNewUICursor = NO_UICURSOR;
        //SetCurrentCursorFromDatabase( VIDEO_NO_CURSOR );

        return (ScreenName.GAME_SCREEN);
    }


    void GetGridNoScreenXY(int sGridNo, int* pScreenX, int* pScreenY)
    {
        int sScreenX, sScreenY;
        int sOffsetX, sOffsetY;
        int sTempX_S, sTempY_S;
        int sXPos, sYPos;

        ConvertGridNoToCellXY(sGridNo, &sXPos, &sYPos);

        // Get 'true' merc position
        sOffsetX = sXPos - gsRenderCenterX;
        sOffsetY = sYPos - gsRenderCenterY;

        FromCellToScreenCoordinates(sOffsetX, sOffsetY, &sTempX_S, &sTempY_S);

        sScreenX = ((gsVIEWPORT_END_X - gsVIEWPORT_START_X) / 2) + (int)sTempX_S;
        sScreenY = ((gsVIEWPORT_END_Y - gsVIEWPORT_START_Y) / 2) + (int)sTempY_S;

        // Adjust for offset position on screen
        sScreenX -= gsRenderWorldOffsetX;
        sScreenY -= gsRenderWorldOffsetY;
        sScreenY -= gpWorldLevelData[sGridNo].sHeight;

        // Adjust based on interface level

        // Adjust for render height
        sScreenY += gsRenderHeight;

        // Adjust y offset!
        sScreenY += (WORLD_TILE_Y / 2);

        (*pScreenX) = sScreenX;
        (*pScreenY) = sScreenY;
    }


    void EndMultiSoldierSelection(bool fAcknowledge)
    {
        SOLDIERTYPE* pSoldier;
        int cnt;
        SOLDIERTYPE* pFirstSoldier = null;
        bool fSelectedSoldierInBatch = false;


        gTacticalStatus.fAtLeastOneGuyOnMultiSelect = false;


        // OK, loop through all guys who are 'multi-selected' and
        // check if our currently selected guy is amoung the
        // lucky few.. if not, change to a guy who is...
        cnt = gTacticalStatus.Team[gbPlayerNum].bFirstID;
        for (pSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[gbPlayerNum].bLastID; cnt++, pSoldier++)
        {
            if (pSoldier.bActive && pSoldier.bInSector)
            {
                if (pSoldier.uiStatusFlags & SOLDIER_MULTI_SELECTED)
                {
                    gTacticalStatus.fAtLeastOneGuyOnMultiSelect = true;

                    if (pSoldier.ubID != gusSelectedSoldier && pFirstSoldier == null)
                    {
                        pFirstSoldier = pSoldier;
                    }

                    if (pSoldier.ubID == gusSelectedSoldier)
                    {
                        fSelectedSoldierInBatch = true;
                    }

                    if (!gGameSettings.fOptions[TOPTION_MUTE_CONFIRMATIONS] && fAcknowledge)
                        InternalDoMercBattleSound(pSoldier, BATTLE_SOUND_ATTN1, BATTLE_SND_LOWER_VOLUME);

                    if (pSoldier.fMercAsleep)
                    {
                        PutMercInAwakeState(pSoldier);
                    }
                }
            }
        }

        // If here, select the first guy...
        if (pFirstSoldier != null && !fSelectedSoldierInBatch)
        {
            SelectSoldier(pFirstSoldier.ubID, true, true);
        }

    }


    void StopRubberBandedMercFromMoving()
    {
        SOLDIERTYPE* pSoldier;
        int cnt;
        SOLDIERTYPE* pFirstSoldier = null;


        if (!gTacticalStatus.fAtLeastOneGuyOnMultiSelect)
        {
            return;
        }

        // OK, loop through all guys who are 'multi-selected' and
        // check if our currently selected guy is amoung the
        // lucky few.. if not, change to a guy who is...
        cnt = gTacticalStatus.Team[gbPlayerNum].bFirstID;
        for (pSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[gbPlayerNum].bLastID; cnt++, pSoldier++)
        {
            if (pSoldier.bActive && pSoldier.bInSector)
            {
                if (pSoldier.uiStatusFlags & SOLDIER_MULTI_SELECTED)
                {
                    pSoldier.fDelayedMovement = false;
                    pSoldier.sFinalDestination = pSoldier.sGridNo;
                    StopSoldier(pSoldier);
                }
            }
        }

    }


    void EndRubberBanding()
    {
        if (gRubberBandActive)
        {
            FreeMouseCursor();
            gfIgnoreScrolling = false;

            EndMultiSoldierSelection(true);

            gRubberBandActive = false;
        }
    }


    bool HandleMultiSelectionMove(int sDestGridNo)
    {
        SOLDIERTYPE* pSoldier;
        int cnt;
        bool fAtLeastOneMultiSelect = false;
        bool fMoveFast = false;

        // OK, loop through all guys who are 'multi-selected' and
        // Make them move....

        // Do a loop first to see if the selected guy is told to go fast...
        gfGetNewPathThroughPeople = true;

        cnt = gTacticalStatus.Team[gbPlayerNum].bFirstID;
        for (pSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[gbPlayerNum].bLastID; cnt++, pSoldier++)
        {
            if (pSoldier.bActive && pSoldier.bInSector)
            {
                if (pSoldier.uiStatusFlags & SOLDIER_MULTI_SELECTED)
                {
                    if (pSoldier.ubID == gusSelectedSoldier)
                    {
                        fMoveFast = pSoldier.fUIMovementFast;
                        break;
                    }
                }
            }
        }

        cnt = gTacticalStatus.Team[gbPlayerNum].bFirstID;
        for (pSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[gbPlayerNum].bLastID; cnt++, pSoldier++)
        {
            if (pSoldier.bActive && pSoldier.bInSector)
            {
                if (pSoldier.uiStatusFlags & SOLDIER_MULTI_SELECTED)
                {
                    // If we can't be controlled, returninvalid...
                    if (pSoldier.uiStatusFlags & SOLDIER_ROBOT)
                    {
                        if (!CanRobotBeControlled(pSoldier))
                        {
                            continue;
                        }
                    }

                    pSoldier.fUIMovementFast = fMoveFast;
                    pSoldier.usUIMovementMode = GetMoveStateBasedOnStance(pSoldier, gAnimControl[pSoldier.usAnimState].ubEndHeight);

                    pSoldier.fUIMovementFast = false;

                    if (gUIUseReverse)
                    {
                        pSoldier.bReverse = true;
                    }
                    else
                    {
                        pSoldier.bReverse = false;
                    }

                    // Remove any previous actions
                    pSoldier.ubPendingAction = NO_PENDING_ACTION;


                    if (EVENT_InternalGetNewSoldierPath(pSoldier, sDestGridNo, pSoldier.usUIMovementMode, true, pSoldier.fNoAPToFinishMove))
                    {
                        InternalDoMercBattleSound(pSoldier, BATTLE_SOUND_OK1, BATTLE_SND_LOWER_VOLUME);
                    }
                    else
                    {
                        ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_INTERFACE, TacticalStr[NO_PATH_FOR_MERC], pSoldier.name);
                    }

                    fAtLeastOneMultiSelect = true;
                }
            }
        }
        gfGetNewPathThroughPeople = false;

        return (fAtLeastOneMultiSelect);
    }


    void ResetMultiSelection()
    {
        SOLDIERTYPE* pSoldier;
        int cnt;

        // OK, loop through all guys who are 'multi-selected' and
        // Make them move....

        cnt = gTacticalStatus.Team[gbPlayerNum].bFirstID;
        for (pSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[gbPlayerNum].bLastID; cnt++, pSoldier++)
        {
            if (pSoldier.bActive && pSoldier.bInSector)
            {
                if (pSoldier.uiStatusFlags & SOLDIER_MULTI_SELECTED)
                {
                    pSoldier.uiStatusFlags &= (~SOLDIER_MULTI_SELECTED);
                }
            }
        }

        gTacticalStatus.fAtLeastOneGuyOnMultiSelect = false;
    }



    ScreenName UIHandleRubberBandOnTerrain(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE* pSoldier;
        int cnt;
        int sScreenX, sScreenY;
        int iTemp;
        SGPRect aRect;
        bool fAtLeastOne = false;

        guiNewUICursor = NO_UICURSOR;
        //SetCurrentCursorFromDatabase( VIDEO_NO_CURSOR );

        gRubberBandRect.iRight = gusMouseXPos;
        gRubberBandRect.iBottom = gusMouseYPos;

        // Copy into temp rect
        memcpy(&aRect, &gRubberBandRect, sizeof(gRubberBandRect));

        if (aRect.iRight < aRect.iLeft)
        {
            iTemp = aRect.iLeft;
            aRect.iLeft = aRect.iRight;
            aRect.iRight = iTemp;
        }


        if (aRect.iBottom < aRect.iTop)
        {
            iTemp = aRect.iTop;
            aRect.iTop = aRect.iBottom;
            aRect.iBottom = iTemp;
        }

        // ATE:Check at least for one guy that's in point!
        cnt = gTacticalStatus.Team[gbPlayerNum].bFirstID;
        for (pSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[gbPlayerNum].bLastID; cnt++, pSoldier++)
        {
            // Check if this guy is OK to control....
            if (OK_CONTROLLABLE_MERC(pSoldier) && !(pSoldier.uiStatusFlags & (SOLDIER_VEHICLE | SOLDIER_PASSENGER | SOLDIER_DRIVER)))
            {
                // Get screen pos of gridno......
                GetGridNoScreenXY(pSoldier.sGridNo, &sScreenX, &sScreenY);

                // ATE: If we are in a hiehger interface level, subttrasct....
                if (gsInterfaceLevel == 1)
                {
                    sScreenY -= 50;
                }

                if (IsPointInScreenRect(sScreenX, sScreenY, &aRect))
                {
                    fAtLeastOne = true;
                }
            }
        }

        if (!fAtLeastOne)
        {
            return (ScreenName.GAME_SCREEN);
        }

        // ATE: Now loop through our guys and see if any fit!
        cnt = gTacticalStatus.Team[gbPlayerNum].bFirstID;
        for (pSoldier = MercPtrs[cnt]; cnt <= gTacticalStatus.Team[gbPlayerNum].bLastID; cnt++, pSoldier++)
        {

            // Check if this guy is OK to control....
            if (OK_CONTROLLABLE_MERC(pSoldier) && !(pSoldier.uiStatusFlags & (SOLDIER_VEHICLE | SOLDIER_PASSENGER | SOLDIER_DRIVER)))
            {
                if (!_KeyDown(ALT))
                {
                    pSoldier.uiStatusFlags &= (~SOLDIER_MULTI_SELECTED);
                }

                // Get screen pos of gridno......
                GetGridNoScreenXY(pSoldier.sGridNo, &sScreenX, &sScreenY);

                // ATE: If we are in a hiehger interface level, subttrasct....
                if (gsInterfaceLevel == 1)
                {
                    sScreenY -= 50;
                }

                if (IsPointInScreenRect(sScreenX, sScreenY, &aRect))
                {
                    // Adjust this guy's flag...
                    pSoldier.uiStatusFlags |= SOLDIER_MULTI_SELECTED;
                }
            }
        }


        return (ScreenName.GAME_SCREEN);
    }


    ScreenName UIHandleJumpOverOnTerrain(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE* pSoldier;
        uint usMapPos;

        // Here, first get map screen
        if (!GetSoldier(&pSoldier, gusSelectedSoldier))
        {
            return (ScreenName.GAME_SCREEN);
        }

        if (!GetMouseMapPos(&usMapPos))
        {
            return (ScreenName.GAME_SCREEN);
        }

        if (!IsValidJumpLocation(pSoldier, usMapPos, false))
        {
            guiPendingOverrideEvent = M_ON_TERRAIN;
            return (ScreenName.GAME_SCREEN);
        }

        // Display APs....
        gsCurrentActionPoints = GetAPsToJumpOver(pSoldier);

        gfUIDisplayActionPoints = true;
        gfUIDisplayActionPointsCenter = true;

        guiNewUICursor = JUMP_OVER_UICURSOR;

        return (ScreenName.GAME_SCREEN);
    }

    ScreenName UIHandleJumpOver(UI_EVENT pUIEvent)
    {
        SOLDIERTYPE* pSoldier;
        uint usMapPos;
        byte bDirection;

        // Here, first get map screen
        if (!GetSoldier(&pSoldier, gusSelectedSoldier))
        {
            return (ScreenName.GAME_SCREEN);
        }

        if (!GetMouseMapPos(&usMapPos))
        {
            return (ScreenName.GAME_SCREEN);
        }

        if (!IsValidJumpLocation(pSoldier, usMapPos, false))
        {
            return (ScreenName.GAME_SCREEN);
        }

        SetUIBusy(pSoldier.ubID);

        // OK, Start jumping!
        // Remove any previous actions
        pSoldier.ubPendingAction = NO_PENDING_ACTION;

        // Get direction to goto....
        bDirection = (byte)GetDirectionFromGridNo(usMapPos, pSoldier);


        pSoldier.fDontChargeTurningAPs = true;
        EVENT_InternalSetSoldierDesiredDirection(pSoldier, bDirection, false, pSoldier.usAnimState);
        pSoldier.fTurningUntilDone = true;
        // ATE: Reset flag to go back to prone...
        //pSoldier.fTurningFromPronePosition = TURNING_FROM_PRONE_OFF;
        pSoldier.usPendingAnimation = JUMP_OVER_BLOCKING_PERSON;


        return (ScreenName.GAME_SCREEN);
    }

    ScreenName UIHandleLABeginLockOurTurn(UI_EVENT pUIEvent)
    {
        // Don't let both versions of the locks to happen at the same time!
        // ( They are mutually exclusive )!
        UIHandleLUIEndLock(null);

        if (!gfUserTurnRegionActive)
        {
            gfUserTurnRegionActive = true;

            gfUIInterfaceSetBusy = true;
            guiUIInterfaceBusyTime = GetJA2Clock();

            //guiNewUICursor = NO_UICURSOR;
            //SetCurrentCursorFromDatabase( VIDEO_NO_CURSOR );

            MSYS_DefineRegion(&gUserTurnRegion, 0, 0, 640, 480, MSYS_PRIORITY_HIGHEST,
                                 CURSOR_WAIT, MSYS_NO_CALLBACK, MSYS_NO_CALLBACK);
            // Add region
            MSYS_AddRegion(&gUserTurnRegion);

            //guiPendingOverrideEvent = LOCKOURTURN_UI_MODE;

            ErasePath(true);

            // Pause time!
            PauseGame();
            LockPauseState(17);
        }

        return (ScreenName.GAME_SCREEN);
    }

    ScreenName UIHandleLAEndLockOurTurn(UI_EVENT pUIEvent)
    {
        if (gfUserTurnRegionActive)
        {
            gfUserTurnRegionActive = false;

            gfUIInterfaceSetBusy = false;

            // Add region
            MSYS_RemoveRegion(&gUserTurnRegion);
            RefreshMouseRegions();
            //SetCurrentCursorFromDatabase( guiCurrentUICursor );

            gfPlotNewMovement = true;

            guiForceRefreshMousePositionCalculation = true;
            UIHandleMOnTerrain(null);

            if (gViewportRegion.uiFlags & MSYS_MOUSE_IN_AREA)
            {
                SetCurrentCursorFromDatabase(gUICursors[guiNewUICursor].usFreeCursorName);
            }
            guiPendingOverrideEvent = M_ON_TERRAIN;
            HandleTacticalUI();

            TurnOffTeamsMuzzleFlashes(gbPlayerNum);

            // UnPause time!
            UnLockPauseState();
            UnPauseGame();
        }

        return (ScreenName.GAME_SCREEN);
    }



    bool IsValidTalkableNPCFromMouse(byte* pubSoldierID, bool fGive, bool fAllowMercs, bool fCheckCollapsed)
    {
        // Check if there is a guy here to talk to!
        if (gfUIFullTargetFound)
        {
            *pubSoldierID = (byte)gusUIFullTargetID;
            return (IsValidTalkableNPC((byte)gusUIFullTargetID, fGive, fAllowMercs, fCheckCollapsed));
        }

        return (false);
    }


    bool IsValidTalkableNPC(byte ubSoldierID, bool fGive, bool fAllowMercs, bool fCheckCollapsed)
    {
        SOLDIERTYPE* pSoldier = MercPtrs[ubSoldierID];
        bool fValidGuy = false;


        if (gusSelectedSoldier != OverheadTypes.NOBODY)
        {
            if (AM_A_ROBOT(MercPtrs[gusSelectedSoldier]))
            {
                return (false);
            }
        }

        // CHECK IF ACTIVE!
        if (!pSoldier.bActive)
        {
            return (false);
        }

        // CHECK IF DEAD
        if (pSoldier.bLife == 0)
        {
            return (false);
        }

        if (pSoldier.bCollapsed && fCheckCollapsed)
        {
            return (false);
        }

        if (pSoldier.uiStatusFlags & SOLDIER_VEHICLE)
        {
            return (false);
        }


        // IF BAD GUY - CHECK VISIVILITY
        if (pSoldier.bTeam != gbPlayerNum)
        {
            if (pSoldier.bVisible == -1 && !(gTacticalStatus.uiFlags & SHOW_ALL_MERCS))
            {
                return (false);
            }
        }

        if (pSoldier.ubProfile != NO_PROFILE && pSoldier.ubProfile >= FIRST_RPC && !RPC_RECRUITED(pSoldier) && !AM_AN_EPC(pSoldier))
        {
            fValidGuy = true;
        }

        // Check for EPC...
        if (pSoldier.ubProfile != NO_PROFILE && (gCurrentUIMode == TALKCURSOR_MODE || fGive) && AM_AN_EPC(pSoldier))
        {
            fValidGuy = true;
        }

        // ATE: We can talk to our own teammates....
        if (pSoldier.bTeam == gbPlayerNum && fAllowMercs)
        {
            fValidGuy = true;
        }

        if (GetCivType(pSoldier) != CIV_TYPE_NA && !fGive)
        {
            fValidGuy = true;
        }

        // Alright, let's do something special here for robot...
        if (pSoldier.uiStatusFlags & SOLDIER_ROBOT)
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
            if ((gAnimControl[pSoldier.usAnimState].uiFlags & ANIM_MOVING) && !(gTacticalStatus.uiFlags & INCOMBAT))
            {
                return (false);
            }

            return (true);
        }

        return (false);
    }


    bool HandleTalkInit()
    {
        int sAPCost;
        SOLDIERTYPE? pSoldier;
        SOLDIERTYPE? pTSoldier;
        uint uiRange;
        uint usMapPos;
        int sGoodGridNo;
        byte ubNewDirection;
        byte ubQuoteNum;
        byte ubDiceRoll;
        int sDistVisible;
        int sActionGridNo;
        byte ubDirection;

        // Get soldier
        if (!GetSoldier(&pSoldier, gusSelectedSoldier))
        {
            return (false);
        }

        if (!GetMouseMapPos(&usMapPos))
        {
            return (false);
        }

        // Check if there is a guy here to talk to!
        if (gfUIFullTargetFound)
        {
            // Is he a valid NPC?
            if (IsValidTalkableNPC((byte)gusUIFullTargetID, false, true, false))
            {
                GetSoldier(&pTSoldier, gusUIFullTargetID);

                if (pTSoldier.ubID != pSoldier.ubID)
                {
                    //ATE: Check if we have good LOS
                    // is he close enough to see that gridno if he turns his head?
                    sDistVisible = DistanceVisible(pSoldier, DIRECTION_IRRELEVANT, DIRECTION_IRRELEVANT, pTSoldier.sGridNo, pTSoldier.bLevel);

                    // Check LOS!
                    if (!SoldierTo3DLocationLineOfSightTest(pSoldier, pTSoldier.sGridNo, pTSoldier.bLevel, 3, (byte)sDistVisible, true))
                    {
                        if (pTSoldier.ubProfile != NO_PROFILE)
                        {
                            ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_UI_FEEDBACK, TacticalStr[NO_LOS_TO_TALK_TARGET], pSoldier.name, pTSoldier.name);
                        }
                        else
                        {
                            ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_UI_FEEDBACK, gzLateLocalizedString[45], pSoldier.name);
                        }
                        return (false);
                    }
                }

                if (pTSoldier.bCollapsed)
                {
                    ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_UI_FEEDBACK, gzLateLocalizedString[21], pTSoldier.name);
                    return (false);
                }

                // If Q on, turn off.....
                if (guiCurrentScreen == DEBUG_SCREEN)
                {
                    gfExitDebugScreen = true;
                }

                // ATE: if our own guy...
                if (pTSoldier.bTeam == gbPlayerNum && !AM_AN_EPC(pTSoldier))
                {
                    if (pTSoldier.ubProfile == DIMITRI)
                    {
                        ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_UI_FEEDBACK, gzLateLocalizedString[32], pTSoldier.name);
                        return (false);
                    }

                    // Randomize quote to use....

                    // If buddy had a social trait...
                    if (gMercProfiles[pTSoldier.ubProfile].bAttitude != ATT_NORMAL)
                    {
                        ubDiceRoll = (byte)Random(3);
                    }
                    else
                    {
                        ubDiceRoll = (byte)Random(2);
                    }

                    // If we are a PC, only use 0
                    if (pTSoldier.ubWhatKindOfMercAmI == MERC_TYPE__PLAYER_CHARACTER)
                    {
                        ubDiceRoll = 0;
                    }

                    switch (ubDiceRoll)
                    {
                        case 0:

                            ubQuoteNum = QUOTE_NEGATIVE_COMPANY;
                            break;

                        case 1:

                            if (QuoteExp_PassingDislike[pTSoldier.ubProfile])
                            {
                                ubQuoteNum = QUOTE_PASSING_DISLIKE;
                            }
                            else
                            {
                                ubQuoteNum = QUOTE_NEGATIVE_COMPANY;
                            }
                            break;

                        case 2:

                            ubQuoteNum = QUOTE_SOCIAL_TRAIT;
                            break;

                        default:

                            ubQuoteNum = QUOTE_NEGATIVE_COMPANY;
                            break;
                    }

                    if (pTSoldier.ubProfile == IRA)
                    {
                        ubQuoteNum = QUOTE_PASSING_DISLIKE;
                    }

                    TacticalCharacterDialogue(pTSoldier, ubQuoteNum);

                    return (false);
                }

                // Check distance
                uiRange = GetRangeFromGridNoDiff(pSoldier.sGridNo, usMapPos);

                // Double check path
                if (GetCivType(pTSoldier) != CIV_TYPE_NA)
                {
                    // ATE: If one is already active, just remove it!
                    if (ShutDownQuoteBoxIfActive())
                    {
                        return (false);
                    }
                }

                if (uiRange > NPC_TALK_RADIUS)
                {
                    // First get an adjacent gridno....
                    sActionGridNo = FindAdjacentGridEx(pSoldier, pTSoldier.sGridNo, &ubDirection, null, false, true);

                    if (sActionGridNo == -1)
                    {
                        ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_UI_FEEDBACK, TacticalStr[NO_PATH]);
                        return (false);
                    }

                    if (UIPlotPath(pSoldier, sActionGridNo, NO_COPYROUTE, false, TEMPORARY, (uint)pSoldier.usUIMovementMode, NOT_STEALTH, FORWARD, pSoldier.bActionPoints) == 0)
                    {
                        ScreenMsg(FONT_MCOLOR_LTYELLOW, MSG_UI_FEEDBACK, TacticalStr[NO_PATH]);
                        return (false);
                    }

                    // Walk up and talk to buddy....
                    gfNPCCircularDistLimit = true;
                    sGoodGridNo = FindGridNoFromSweetSpotWithStructData(pSoldier, pSoldier.usUIMovementMode, pTSoldier.sGridNo, (NPC_TALK_RADIUS - 1), &ubNewDirection, true);
                    gfNPCCircularDistLimit = false;

                    // First calculate APs and validate...
                    sAPCost = AP_TALK;
                    //sAPCost += UIPlotPath( pSoldier, sGoodGridNo, NO_COPYROUTE, false, TEMPORARY, (uint)pSoldier.usUIMovementMode, NOT_STEALTH, FORWARD, pSoldier.bActionPoints );

                    // Check AP cost...
                    if (!EnoughPoints(pSoldier, sAPCost, 0, true))
                    {
                        return (false);
                    }

                    // Now walkup to talk....
                    pSoldier.ubPendingAction = MERC_TALK;
                    pSoldier.uiPendingActionData1 = pTSoldier.ubID;
                    pSoldier.ubPendingActionAnimCount = 0;

                    // WALK UP TO DEST FIRST
                    EVENT_InternalGetNewSoldierPath(pSoldier, sGoodGridNo, pSoldier.usUIMovementMode, true, pSoldier.fNoAPToFinishMove);

                    return (false);
                }
                else
                {
                    sAPCost = AP_TALK;

                    // Check AP cost...
                    if (!EnoughPoints(pSoldier, sAPCost, 0, true))
                    {
                        return (false);
                    }

                    // OK, startup!
                    PlayerSoldierStartTalking(pSoldier, pTSoldier.ubID, false);
                }

                if (GetCivType(pTSoldier) != CIV_TYPE_NA)
                {
                    return (false);
                }
                else
                {
                    return (true);
                }
            }
        }

        return (false);
    }



    void SetUIBusy(byte ubID)
    {
        if ((gTacticalStatus.uiFlags & INCOMBAT) && (gTacticalStatus.uiFlags & TURNBASED) && (gTacticalStatus.ubCurrentTeam == gbPlayerNum))
        {
            if (gusSelectedSoldier == ubID)
            {
                guiPendingOverrideEvent = LA_BEGINUIOURTURNLOCK;
                HandleTacticalUI();
            }
        }
    }

    void UnSetUIBusy(byte ubID)
    {
        if ((gTacticalStatus.uiFlags & INCOMBAT) && (gTacticalStatus.uiFlags & TURNBASED) && (gTacticalStatus.ubCurrentTeam == gbPlayerNum))
        {
            if (!gTacticalStatus.fUnLockUIAfterHiddenInterrupt)
            {
                if (gusSelectedSoldier == ubID)
                {
                    guiPendingOverrideEvent = LA_ENDUIOUTURNLOCK;
                    HandleTacticalUI();

                    // Set grace period...
                    gTacticalStatus.uiTactialTurnLimitClock = GetJA2Clock();
                }
            }
            // player getting control back so reset all muzzle flashes
        }
    }


    void BeginDisplayTimedCursor(uint uiCursorID, uint uiDelay)
    {
        gfDisplayTimerCursor = true;
        guiTimerCursorID = uiCursorID;
        guiTimerLastUpdate = GetJA2Clock();
        guiTimerCursorDelay = uiDelay;
    }


    static bool fOverPool = false;
    static bool fOverEnemy = false;

    byte UIHandleInteractiveTilesAndItemsOnTerrain(SOLDIERTYPE* pSoldier, int usMapPos, bool fUseOKCursor, bool fItemsOnlyIfOnIntTiles)
    {
        ITEM_POOL* pItemPool;
        bool fSetCursor;
        uint uiCursorFlags;
        LEVELNODE* pIntTile;
        int sActionGridNo;
        int sIntTileGridNo;
        bool fContinue = true;
        STRUCTURE* pStructure = null;
        bool fPoolContainsHiddenItems = false;
        SOLDIERTYPE* pTSoldier;


        if (gfResetUIItemCursorOptimization)
        {
            gfResetUIItemCursorOptimization = false;
            fOverPool = false;
            fOverEnemy = false;
        }

        GetCursorMovementFlags(&uiCursorFlags);

        // Default gridno to mouse pos
        sActionGridNo = usMapPos;

        // Look for being on a merc....
        // Steal.....
        UIHandleOnMerc(false);

        gfBeginVehicleCursor = false;

        if (gfUIFullTargetFound)
        {
            pTSoldier = MercPtrs[gusUIFullTargetID];

            if (OK_ENTERABLE_VEHICLE(pTSoldier) && pTSoldier.bVisible != -1)
            {
                // grab number of occupants in vehicles
                if (fItemsOnlyIfOnIntTiles)
                {
                    if (!OKUseVehicle(pTSoldier.ubProfile))
                    {
                        // Set UI CURSOR....
                        guiNewUICursor = CANNOT_MOVE_UICURSOR;

                        gfBeginVehicleCursor = true;
                        return (1);
                    }
                    else
                    {
                        if (GetNumberInVehicle(pTSoldier.bVehicleID) == 0)
                        {
                            // Set UI CURSOR....
                            guiNewUICursor = ENTER_VEHICLE_UICURSOR;

                            gfBeginVehicleCursor = true;
                            return (1);
                        }
                    }
                }
                else
                {
                    // Set UI CURSOR....
                    guiNewUICursor = ENTER_VEHICLE_UICURSOR;
                    return (1);
                }
            }

            if (!fItemsOnlyIfOnIntTiles)
            {
                if ((guiUIFullTargetFlags & ENEMY_MERC) && !(guiUIFullTargetFlags & UNCONSCIOUS_MERC))
                {
                    if (!fOverEnemy)
                    {
                        fOverEnemy = true;
                        gfPlotNewMovement = true;
                    }

                    //Set UI CURSOR
                    if (fUseOKCursor || ((gTacticalStatus.uiFlags & INCOMBAT) && (gTacticalStatus.uiFlags & TURNBASED)))
                    {
                        guiNewUICursor = OKHANDCURSOR_UICURSOR;
                    }
                    else
                    {
                        guiNewUICursor = NORMALHANDCURSOR_UICURSOR;
                    }

                    fSetCursor = HandleUIMovementCursor(pSoldier, uiCursorFlags, sActionGridNo, MOVEUI_TARGET.STEAL);

                    // Display action points
                    gfUIDisplayActionPoints = true;

                    // Determine if we can afford!
                    if (!EnoughPoints(pSoldier, gsCurrentActionPoints, 0, false))
                    {
                        gfUIDisplayActionPointsInvalid = true;
                    }

                    return (0);
                }
            }
        }

        if (fOverEnemy)
        {
            ErasePath(true);
            fOverEnemy = false;
            gfPlotNewMovement = true;
        }

        // If we are over an interactive struct, adjust gridno to this....
        pIntTile = ConditionalGetCurInteractiveTileGridNoAndStructure(&sIntTileGridNo, &pStructure, false);
        gpInvTileThatCausedMoveConfirm = pIntTile;

        if (pIntTile != null)
        {
            sActionGridNo = sIntTileGridNo;
        }

        // Check if we are over an item pool
        if (GetItemPool(sActionGridNo, &pItemPool, pSoldier.bLevel))
        {
            // If we want only on int tiles, and we have no int tiles.. ignore items!
            if (fItemsOnlyIfOnIntTiles && pIntTile == null)
            {

            }
            else if (fItemsOnlyIfOnIntTiles && pIntTile != null && (pStructure.fFlags & STRUCTURE_HASITEMONTOP))
            {
                // if in this mode, we don't want to automatically show hand cursor over items on strucutres
            }
            //else if ( pIntTile != null && ( pStructure.fFlags & ( STRUCTURE_SWITCH | STRUCTURE_ANYDOOR ) ) )
            else if (pIntTile != null && (pStructure.fFlags & (STRUCTURE_SWITCH)))
            {
                // We don't want switches messing around with items ever!
            }
            else if ((pIntTile != null && (pStructure.fFlags & (STRUCTURE_ANYDOOR))) && (sActionGridNo != usMapPos || fItemsOnlyIfOnIntTiles))
            {
                // Next we look for if we are over a door and if the mouse position is != base door position, ignore items!
            }
            else
            {
                fPoolContainsHiddenItems = DoesItemPoolContainAnyHiddenItems(pItemPool);

                // Adjust this if we have not visited this gridno yet...
                if (fPoolContainsHiddenItems)
                {
                    if (!(gpWorldLevelData[sActionGridNo].uiFlags & MAPELEMENT_REVEALED))
                    {
                        fPoolContainsHiddenItems = false;
                    }
                }

                if (ITEMPOOL_VISIBLE(pItemPool) || fPoolContainsHiddenItems)
                {

                    if (!fOverPool)
                    {
                        fOverPool = true;
                        gfPlotNewMovement = true;
                    }

                    //Set UI CURSOR
                    if (fUseOKCursor || ((gTacticalStatus.uiFlags & INCOMBAT) && (gTacticalStatus.uiFlags & TURNBASED)))
                    {
                        guiNewUICursor = OKHANDCURSOR_UICURSOR;
                    }
                    else
                    {
                        guiNewUICursor = NORMALHANDCURSOR_UICURSOR;
                    }

                    fSetCursor = HandleUIMovementCursor(pSoldier, uiCursorFlags, sActionGridNo, MOVEUI_TARGET.ITEMS);

                    // Display action points
                    gfUIDisplayActionPoints = true;

                    if (gsOverItemsGridNo == sActionGridNo)
                    {
                        gfPlotNewMovement = true;
                    }

                    // Determine if we can afford!
                    if (!EnoughPoints(pSoldier, gsCurrentActionPoints, 0, false))
                    {
                        gfUIDisplayActionPointsInvalid = true;
                    }

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
                    ErasePath(true);
                    fOverPool = false;
                    gfPlotNewMovement = true;
                }

                HandleUIMovementCursor(pSoldier, uiCursorFlags, usMapPos, MOVEUI_TARGET.INTTILES);

                //Set UI CURSOR
                guiNewUICursor = GetInteractiveTileCursor(guiNewUICursor, fUseOKCursor);
            }
            else
            {
                if (!fItemsOnlyIfOnIntTiles)
                {
                    // Let's at least show where the merc will walk to if they go here...
                    if (!fOverPool)
                    {
                        fOverPool = true;
                        gfPlotNewMovement = true;
                    }

                    fSetCursor = HandleUIMovementCursor(pSoldier, uiCursorFlags, sActionGridNo, MOVEUI_TARGET.ITEMS);

                    // Display action points
                    gfUIDisplayActionPoints = true;

                    // Determine if we can afford!
                    if (!EnoughPoints(pSoldier, gsCurrentActionPoints, 0, false))
                    {
                        gfUIDisplayActionPointsInvalid = true;
                    }
                }
            }

        }

        if (pIntTile == null)
        {
            return (0);
        }
        else
        {
            return (1);
        }
    }


    void HandleTacticalUILoseCursorFromOtherScreen()
    {
        SetUICursor(0);

        gfTacticalForceNoCursor = true;

        ErasePath(true);

        (*(GameScreens[GAME_SCREEN].HandleScreen))();

        gfTacticalForceNoCursor = false;

        SetUICursor(guiCurrentUICursor);
    }


    bool SelectedGuyInBusyAnimation()
    {
        SOLDIERTYPE? pSoldier;

        if (gusSelectedSoldier != OverheadTypes.NOBODY)
        {
            pSoldier = MercPtrs[gusSelectedSoldier];

            if (pSoldier.usAnimState == LOB_ITEM ||
                     pSoldier.usAnimState == THROW_ITEM ||
                     pSoldier.usAnimState == PICKUP_ITEM ||
                     pSoldier.usAnimState == DROP_ITEM ||
                     pSoldier.usAnimState == OPEN_DOOR ||
                     pSoldier.usAnimState == OPEN_STRUCT ||
                     pSoldier.usAnimState == OPEN_STRUCT ||
                     pSoldier.usAnimState == END_OPEN_DOOR ||
                     pSoldier.usAnimState == END_OPEN_LOCKED_DOOR ||
                     pSoldier.usAnimState == ADJACENT_GET_ITEM ||
             pSoldier.usAnimState == DROP_ADJACENT_OBJECT ||

                     pSoldier.usAnimState == OPEN_DOOR_CROUCHED ||
                     pSoldier.usAnimState == BEGIN_OPENSTRUCT_CROUCHED ||
                     pSoldier.usAnimState == CLOSE_DOOR_CROUCHED ||
                     pSoldier.usAnimState == OPEN_DOOR_CROUCHED ||
                     pSoldier.usAnimState == OPEN_STRUCT_CROUCHED ||
                     pSoldier.usAnimState == END_OPENSTRUCT_CROUCHED ||
                     pSoldier.usAnimState == END_OPEN_DOOR_CROUCHED ||
                     pSoldier.usAnimState == END_OPEN_LOCKED_DOOR_CROUCHED ||
                     pSoldier.usAnimState == END_OPENSTRUCT_LOCKED_CROUCHED ||
                     pSoldier.usAnimState == BEGIN_OPENSTRUCT)
            {
                return (true);
            }
        }

        return (false);
    }

    void GotoHeigherStance(SOLDIERTYPE* pSoldier)
    {
        bool fNearHeigherLevel;
        bool fNearLowerLevel;

        switch (gAnimControl[pSoldier.usAnimState].ubEndHeight)
        {
            case ANIM_STAND:

                // Nowhere
                // Try to climb
                GetMercClimbDirection(pSoldier.ubID, &fNearLowerLevel, &fNearHeigherLevel);

                if (fNearHeigherLevel)
                {
                    BeginSoldierClimbUpRoof(pSoldier);
                }
                break;

            case ANIM_CROUCH:

                HandleStanceChangeFromUIKeys(ANIM_STAND);
                break;

            case ANIM_PRONE:

                HandleStanceChangeFromUIKeys(ANIM_CROUCH);
                break;
        }
    }


    void GotoLowerStance(SOLDIERTYPE* pSoldier)
    {
        bool fNearHeigherLevel;
        bool fNearLowerLevel;


        switch (gAnimControl[pSoldier.usAnimState].ubEndHeight)
        {
            case ANIM_STAND:

                HandleStanceChangeFromUIKeys(ANIM_CROUCH);
                break;

            case ANIM_CROUCH:

                HandleStanceChangeFromUIKeys(ANIM_PRONE);
                break;

            case ANIM_PRONE:

                // Nowhere
                // Try to climb
                GetMercClimbDirection(pSoldier.ubID, &fNearLowerLevel, &fNearHeigherLevel);

                if (fNearLowerLevel)
                {
                    BeginSoldierClimbDownRoof(pSoldier);
                }
                break;
        }
    }

    static int sOldHeight = 0;

    void SetInterfaceHeightLevel()
    {
        int sHeight;
        int sGridNo;

        if (gfBasement || gfCaves)
        {
            gsRenderHeight = 0;
            sOldHeight = 0;

            return;
        }


        // ATE: Use an entry point to determine what height to use....
        if (gMapInformation.sNorthGridNo != -1)
            sGridNo = gMapInformation.sNorthGridNo;
        else if (gMapInformation.sEastGridNo != -1)
            sGridNo = gMapInformation.sEastGridNo;
        else if (gMapInformation.sSouthGridNo != -1)
            sGridNo = gMapInformation.sSouthGridNo;
        else if (gMapInformation.sWestGridNo != -1)
            sGridNo = gMapInformation.sWestGridNo;
        else
        {
            Assert(0);
            return;
        }


        sHeight = gpWorldLevelData[sGridNo].sHeight;

        if (sHeight != sOldHeight)
        {
            gsRenderHeight = sHeight;

            if (gsInterfaceLevel > 0)
            {
                gsRenderHeight += ROOF_LEVEL_HEIGHT;
            }

            SetRenderFlags(RENDER_FLAG_FULL);
            ErasePath(false);

            sOldHeight = sHeight;
        }
    }

    static bool fOldOnValidGuy = false;

    bool ValidQuickExchangePosition()
    {
        SOLDIERTYPE? pSoldier, pOverSoldier;
        int sDistVisible = 0;
        bool fOnValidGuy = false;

        // Check if we over a civ
        if (gfUIFullTargetFound)
        {
            pOverSoldier = MercPtrs[gusUIFullTargetID];

            //KM: Replaced this older if statement for the new one which allows exchanging with militia
            //if ( ( pOverSoldier.bSide != gbPlayerNum ) && pOverSoldier.bNeutral  )
            if ((pOverSoldier.bTeam != gbPlayerNum && pOverSoldier.bNeutral) || (pOverSoldier.bTeam == MILITIA_TEAM && pOverSoldier.bSide == 0))
            {
                // hehe - don't allow animals to exchange places
                if (!(pOverSoldier.uiStatusFlags & (SOLDIER_ANIMAL)))
                {
                    // OK, we have a civ , now check if they are near selected guy.....
                    if (GetSoldier(&pSoldier, gusSelectedSoldier))
                    {
                        if (PythSpacesAway(pSoldier.sGridNo, pOverSoldier.sGridNo) == 1)
                        {
                            // Check if we have LOS to them....
                            sDistVisible = DistanceVisible(pSoldier, DIRECTION_IRRELEVANT, DIRECTION_IRRELEVANT, pOverSoldier.sGridNo, pOverSoldier.bLevel);

                            if (SoldierTo3DLocationLineOfSightTest(pSoldier, pOverSoldier.sGridNo, pOverSoldier.bLevel, (byte)3, (byte)sDistVisible, true))
                            {
                                // ATE:
                                // Check that the path is good!
                                if (FindBestPath(pSoldier, pOverSoldier.sGridNo, pSoldier.bLevel, pSoldier.usUIMovementMode, NO_COPYROUTE, PATH_IGNORE_PERSON_AT_DEST) == 1)
                                {
                                    fOnValidGuy = true;
                                }
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
            guiUIInterfaceSwapCursorsTime = GetJA2Clock();
            // Default it!
            gfOKForExchangeCursor = true;
        }

        // Update old value.....
        fOldOnValidGuy = fOnValidGuy;

        if (!gfOKForExchangeCursor)
        {
            fOnValidGuy = false;
        }

        return (fOnValidGuy);
    }


    // This function contains the logic for allowing the player
    // to jump over people.
    bool IsValidJumpLocation(SOLDIERTYPE* pSoldier, int sGridNo, bool fCheckForPath)
    {
        int sFourGrids[4], sDistance = 0, sSpot, sIntSpot;
        int sDirs[4] = { NORTH, EAST, SOUTH, WEST };
        int cnt;
        byte ubGuyThere;
        byte ubMovementCost;
        int iDoorGridNo;

        // First check that action point cost is zero so far
        // ie: NO PATH!
        if (gsCurrentActionPoints != 0 && fCheckForPath)
        {
            return (false);
        }

        // Loop through positions...
        for (cnt = 0; cnt < 4; cnt++)
        {
            // MOVE OUT TWO DIRECTIONS
            sIntSpot = NewGridNo(sGridNo, DirectionInc(sDirs[cnt]));

            // ATE: Check our movement costs for going through walls!
            ubMovementCost = gubWorldMovementCosts[sIntSpot][sDirs[cnt]][pSoldier.bLevel];
            if (IS_TRAVELCOST_DOOR(ubMovementCost))
            {
                ubMovementCost = DoorTravelCost(pSoldier, sIntSpot, ubMovementCost, (bool)(pSoldier.bTeam == gbPlayerNum), &iDoorGridNo);
            }

            // If we have hit an obstacle, STOP HERE
            if (ubMovementCost >= TRAVELCOST_BLOCKED)
            {
                // no good, continue
                continue;
            }


            // TWICE AS FAR!
            sFourGrids[cnt] = sSpot = NewGridNo(sIntSpot, DirectionInc(sDirs[cnt]));

            // Is the soldier we're looking at here?
            ubGuyThere = WhoIsThere2(sSpot, pSoldier.bLevel);

            // Alright folks, here we are!
            if (ubGuyThere == pSoldier.ubID)
            {
                // Double check OK destination......
                if (NewOKDestination(pSoldier, sGridNo, true, (byte)gsInterfaceLevel))
                {
                    // If the soldier in the middle of doing stuff?
                    if (!pSoldier.fTurningUntilDone)
                    {
                        // OK, NOW check if there is a guy in between us
                        // 
                        // 
                        ubGuyThere = WhoIsThere2(sIntSpot, pSoldier.bLevel);

                        // Is there a guy and is he prone?
                        if (ubGuyThere != OverheadTypes.NOBODY && ubGuyThere != pSoldier.ubID && gAnimControl[MercPtrs[ubGuyThere].usAnimState].ubHeight == ANIM_PRONE)
                        {
                            // It's a GO!
                            return (true);
                        }
                    }
                }
            }
        }

        return (false);
    }
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

public record UI_EVENT(
    int uiFlags,
    UI_MODE ChangeToUIMode,
    Func<UI_EVENT, ScreenName> HandleEvent,
    bool fFirstTime,
    bool fDoneMenu,
    int uiMenuPreviousMode,
    int[] uiParams);// [3];

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
    MERCSFORA = 5,
    WIREFENCE,
    BOMB = 7,
    STEAL = 8,
    REPAIR = 9,
    JAR = 10,
    CAN = 11,
    REFUEL = 12,

    MOVEUI_RETURN_ON_TARGET_MERC = 1,
}
