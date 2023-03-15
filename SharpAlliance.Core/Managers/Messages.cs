using System;
using System.IO;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;

using static SharpAlliance.Core.SubSystems.MessageSubSystem;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Managers;

public class Messages
{
    public static ScrollStringStPtr? pStringS = null;
    private static bool fScrollMessagesHidden;
    private static bool fOkToBeepNewMessage;
    private static int usLineWidthIfWordIsWiderThenWidth = 0;

    public static void DisableScrollMessages()
    {
        // will stop the scroll of messages in tactical and hide them during an NPC's dialogue
        // disble video overlay for tatcitcal scroll messages
        EnableDisableScrollStringVideoOverlay(false);
    }

    private static void EnableDisableScrollStringVideoOverlay(bool fEnable)
    {
    }

    public static void SetStringFont(ScrollStringStPtr pStringSt, FontStyle uiFont)
    {
        pStringSt.uiFont = uiFont;
    }

    public static FontStyle GetStringFont(ScrollStringStPtr pStringSt)
    {
        return pStringSt.uiFont;
    }

    private static ScrollStringStPtr AddString(string pString, FontColor usColor, FontStyle uiFont, bool fStartOfNewString, int ubPriority)
    {
        // add a new string to the list of strings
        ScrollStringStPtr pStringSt = new();

        SetString(pStringSt, pString);
        SetStringColor(pStringSt, usColor);
        pStringSt.uiFont = uiFont;
        pStringSt.fBeginningOfNewString = fStartOfNewString;
        pStringSt.uiFlags = ubPriority;

        SetStringNext(pStringSt, null);
        SetStringPrev(pStringSt, null);
        pStringSt.iVideoOverlay = -1;

        // now add string to map screen strings
        //AddStringToMapScreenMessageList(pString, usColor, uiFont, fStartOfNewString, ubPriority );

        return (pStringSt);
    }

    private static void SetString(ScrollStringStPtr pStringSt, string pString)
    {
        // ARM: Why x2 + 4 ???
        //pStringSt.pString16 = MemAlloc((wcslen(pString) * 2) + 4);
        pStringSt.pString16 = pString;
        //pStringSt.pString16[pString.Length] = 0;
    }


    void SetStringPosition(ScrollStringStPtr pStringSt, int usX, int usY)
    {
        SetStringVideoOverlayPosition(pStringSt, usX, usY);
    }



    private static void SetStringColor(ScrollStringStPtr pStringSt, FontColor usColor)
    {
        pStringSt.usColor = usColor;
    }

    private static ScrollStringStPtr? GetNextString(ScrollStringStPtr? pStringSt)
    {
        // returns pointer to next string line
        if (pStringSt == null)
        {
            return null;
        }
        else
        {
            return pStringSt.pNext;
        }
    }


    ScrollStringStPtr? GetPrevString(ScrollStringStPtr pStringSt)
    {
        // returns pointer to previous string line
        if (pStringSt == null)
        {
            return null;
        }
        else
        {
            return pStringSt.pPrev;
        }
    }


    private static ScrollStringStPtr SetStringNext(ScrollStringStPtr pStringSt, ScrollStringStPtr? pNext)
    {
        pStringSt.pNext = pNext;
        return pStringSt;
    }


    private static ScrollStringStPtr SetStringPrev(ScrollStringStPtr pStringSt, ScrollStringStPtr? pPrev)
    {
        pStringSt.pPrev = pPrev;
        return pStringSt;
    }


    bool CreateStringVideoOverlay(ScrollStringStPtr pStringSt, int usX, int usY)
    {
        VIDEO_OVERLAY_DESC VideoOverlayDesc = new();

        // SET VIDEO OVERLAY
        VideoOverlayDesc.sLeft = usX;
        VideoOverlayDesc.sTop = usY;
        VideoOverlayDesc.uiFontID = pStringSt.uiFont;
        VideoOverlayDesc.ubFontBack = FontColor.FONT_MCOLOR_BLACK;
        VideoOverlayDesc.ubFontFore = pStringSt.usColor;
        VideoOverlayDesc.sX = VideoOverlayDesc.sLeft;
        VideoOverlayDesc.sY = VideoOverlayDesc.sTop;
        VideoOverlayDesc.pzText = pStringSt.pString16!;
        VideoOverlayDesc.BltCallback = BlitString;
        pStringSt.iVideoOverlay = RenderDirty.RegisterVideoOverlay((VOVERLAY.DIRTYBYTEXT), VideoOverlayDesc);

        if (pStringSt.iVideoOverlay == -1)
        {
            return (false);
        }

        return (true);
    }


    void RemoveStringVideoOverlay(ScrollStringStPtr? pStringSt)
    {

        // error check, remove one not there
        if (pStringSt.iVideoOverlay == -1)
        {
            return;
        }


        RenderDirty.RemoveVideoOverlay(pStringSt.iVideoOverlay);
        pStringSt.iVideoOverlay = -1;
    }


    void SetStringVideoOverlayPosition(ScrollStringStPtr pStringSt, int usX, int usY)
    {
        VIDEO_OVERLAY_DESC VideoOverlayDesc = new();

        //memset(out VideoOverlayDesc, 0, sizeof(VideoOverlayDesc));

        // Donot update if not allocated!
        if (pStringSt.iVideoOverlay != -1)
        {
            VideoOverlayDesc.uiFlags = VOVERLAY_DESC.POSITION;
            VideoOverlayDesc.sLeft = usX;
            VideoOverlayDesc.sTop = usY;
            VideoOverlayDesc.sX = VideoOverlayDesc.sLeft;
            VideoOverlayDesc.sY = VideoOverlayDesc.sTop;

            RenderDirty.UpdateVideoOverlay(VideoOverlayDesc, pStringSt.iVideoOverlay, false);
        }
    }


    void BlitString(VIDEO_OVERLAY pBlitter)
    {
        int? pDestBuf;
        int uiDestPitchBYTES;

        //gprintfdirty(pBlitter.sX,pBlitter.sY, pBlitter.zText);
        //RestoreExternBackgroundRect(pBlitter.sX,pBlitter.sY, pBlitter.sX+StringPixLength(pBlitter.zText,pBlitter.uiFontID ), pBlitter.sY+GetFontHeight(pBlitter.uiFontID ));

        if (fScrollMessagesHidden == true)
        {
            return;
        }


        pDestBuf = VeldridVideoManager.LockVideoSurface(pBlitter.uiDestBuff, out uiDestPitchBYTES);
        FontSubSystem.SetFont(pBlitter.uiFontID);

        FontSubSystem.SetFontBackground(pBlitter.ubFontBack);
        FontSubSystem.SetFontForeground(pBlitter.ubFontFore);
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
        //mprintf_buffer_coded(pDestBuf, uiDestPitchBYTES, pBlitter.uiFontID, pBlitter.sX, pBlitter.sY, pBlitter.zText);
        VeldridVideoManager.UnLockVideoSurface(pBlitter.uiDestBuff);

    }


    void EnableStringVideoOverlay(ScrollStringStPtr pStringSt, bool fEnable)
    {
        VIDEO_OVERLAY_DESC VideoOverlayDesc = new();

        if (pStringSt.iVideoOverlay != -1)
        {
            VideoOverlayDesc.fDisabled = !fEnable;
            VideoOverlayDesc.uiFlags = VOVERLAY_DESC.DISABLED;
            RenderDirty.UpdateVideoOverlay(VideoOverlayDesc, pStringSt.iVideoOverlay, false);
        }
    }


    void ClearDisplayedListOfTacticalStrings()
    {
        // this function will go through list of display strings and clear them all out
        int cnt;

        for (cnt = 0; cnt < Globals.MAX_LINE_COUNT; cnt++)
        {
            if (Globals.gpDisplayList[cnt] != null)
            {
                // CHECK IF WE HAVE AGED

                // Remove our sorry ass
                RemoveStringVideoOverlay(Globals.gpDisplayList[cnt]);
                Globals.gpDisplayList[cnt] = null;
            }
        }

        return;
    }



    void ScrollString()
    {
        ScrollStringStPtr? pStringSt = pStringS;
        uint suiTimer = 0;
        int cnt;
        int iNumberOfNewStrings = 0; // the count of new strings, so we can update position by WIDTH_BETWEEN_NEW_STRINGS pixels in the y
        int iNumberOfMessagesOnQueue = 0;
        uint iMaxAge = 0;
        bool fDitchLastMessage = false;

        // UPDATE TIMER
        suiTimer = Globals.GetJA2Clock();

        // might have pop up text timer
        HandleLastQuotePopUpTimer();

        if (Globals.guiCurrentScreen == ScreenName.MAP_SCREEN)
        {
            return;
        }

        // DONOT UPDATE IF WE ARE SCROLLING!
        if (Globals.gfScrollPending || Globals.gfScrollInertia)
        {
            return;
        }

        // messages hidden
        if (fScrollMessagesHidden)
        {
            return;
        }

        iNumberOfMessagesOnQueue = GetMessageQueueSize();
        iMaxAge = Globals.MAX_AGE;

        if ((iNumberOfMessagesOnQueue > 0) && (Globals.gpDisplayList[Globals.MAX_LINE_COUNT - 1] != null))
        {
            fDitchLastMessage = true;
        }
        else
        {
            fDitchLastMessage = false;

        }


        if ((iNumberOfMessagesOnQueue * 1000) >= iMaxAge)
        {
            iNumberOfMessagesOnQueue = (int)(iMaxAge / 1000);
        }
        else if (iNumberOfMessagesOnQueue < 0)
        {
            iNumberOfMessagesOnQueue = 0;
        }

        //AGE
        for (cnt = 0; cnt < Globals.MAX_LINE_COUNT; cnt++)
        {
            if (Globals.gpDisplayList[cnt] != null)
            {
                if ((fDitchLastMessage) && (cnt == Globals.MAX_LINE_COUNT - 1))
                {
                    Globals.gpDisplayList[cnt].uiTimeOfLastUpdate = iMaxAge;
                }
                // CHECK IF WE HAVE AGED
                if ((suiTimer - Globals.gpDisplayList[cnt].uiTimeOfLastUpdate) > (int)(iMaxAge - (1000 * iNumberOfMessagesOnQueue)))
                {
                    // Remove our sorry ass
                    RemoveStringVideoOverlay(Globals.gpDisplayList[cnt]);

                    // Free slot
                    Globals.gpDisplayList[cnt] = null;
                }
            }
        }


        // CHECK FOR FREE SPOTS AND ADD ANY STRINGS IF WE HAVE SOME TO ADD!

        // FIRST CHECK IF WE HAVE ANY IN OUR QUEUE
        if (pStringS != null)
        {
            // CHECK IF WE HAVE A SLOT!
            // CHECK OUR LAST SLOT!
            if (Globals.gpDisplayList[Globals.MAX_LINE_COUNT - 1] == null)
            {
                // MOVE ALL UP!

                // cpy, then move	
                for (cnt = Globals.MAX_LINE_COUNT - 1; cnt > 0; cnt--)
                {
                    Globals.gpDisplayList[cnt] = Globals.gpDisplayList[cnt - 1];
                }

                // now add in the new string
                cnt = 0;
                Globals.gpDisplayList[cnt] = pStringS;
                CreateStringVideoOverlay(pStringS, Globals.X_START, Globals.Y_START);
                if (pStringS.fBeginningOfNewString == true)
                {
                    iNumberOfNewStrings++;
                }

                // set up age
                pStringS.uiTimeOfLastUpdate = Globals.GetJA2Clock();

                // now move
                for (cnt = 0; cnt <= Globals.MAX_LINE_COUNT - 1; cnt++)
                {

                    // Adjust position!
                    if (Globals.gpDisplayList[cnt] != null)
                    {

                        SetStringVideoOverlayPosition(Globals.gpDisplayList[cnt], Globals.X_START, ((Globals.Y_START - ((cnt) * FontSubSystem.GetFontHeight(FontStyle.SMALLFONT1))) - (Globals.WIDTH_BETWEEN_NEW_STRINGS * (iNumberOfNewStrings))));

                        // start of new string, increment count of new strings, for spacing purposes
                        if (Globals.gpDisplayList[cnt].fBeginningOfNewString == true)
                        {
                            iNumberOfNewStrings++;
                        }


                    }

                }


                // WE NOW HAVE A FREE SPACE, INSERT!

                // Adjust head!
                pStringS = pStringS.pNext;
                if (pStringS is not null)
                {
                    pStringS.pPrev = null;
                }

                //check if new meesage we have not seen since mapscreen..if so, beep
                if ((fOkToBeepNewMessage == true)
                    && (Globals.gpDisplayList[Globals.MAX_LINE_COUNT - 2] == null)
                    && ((Globals.guiCurrentScreen == ScreenName.GAME_SCREEN)
                    || (Globals.guiCurrentScreen == ScreenName.MAP_SCREEN))
                    && (Globals.gfFacePanelActive == false))
                {
                    PlayNewMessageSound();
                }
            }
        }
    }

    void EnableScrollMessages()
    {
        EnableDisableScrollStringVideoOverlay(true);
        return;
    }

    void HideMessagesDuringNPCDialogue()
    {
        // will stop the scroll of messages in tactical and hide them during an NPC's dialogue
        int cnt;

        VIDEO_OVERLAY_DESC VideoOverlayDesc = new();

        VideoOverlayDesc.fDisabled = true;
        VideoOverlayDesc.uiFlags = VOVERLAY_DESC.DISABLED;


        fScrollMessagesHidden = true;
        uiStartOfPauseTime = Globals.GetJA2Clock();

        for (cnt = 0; cnt < Globals.MAX_LINE_COUNT; cnt++)
        {
            if (Globals.gpDisplayList[cnt] != null)
            {
                RenderDirty.RestoreExternBackgroundRectGivenID(Globals.gVideoOverlays[Globals.gpDisplayList[cnt].iVideoOverlay].uiBackground);
                RenderDirty.UpdateVideoOverlay(VideoOverlayDesc, Globals.gpDisplayList[cnt].iVideoOverlay, false);
            }
        }

        return;
    }


    void UnHideMessagesDuringNPCDialogue()
    {
        VIDEO_OVERLAY_DESC VideoOverlayDesc = new();
        int cnt = 0;

        VideoOverlayDesc.fDisabled = false;
        VideoOverlayDesc.uiFlags = VOVERLAY_DESC.DISABLED;
        fScrollMessagesHidden = false;

        for (cnt = 0; cnt < Globals.MAX_LINE_COUNT; cnt++)
        {
            if (Globals.gpDisplayList[cnt] != null)
            {
                Globals.gpDisplayList[cnt].uiTimeOfLastUpdate += Globals.GetJA2Clock() - uiStartOfPauseTime;
                RenderDirty.UpdateVideoOverlay(VideoOverlayDesc, Globals.gpDisplayList[cnt].iVideoOverlay, false);
            }
        }


        return;
    }

    // new screen message
    public static void ScreenMsg(FontColor usColor, int ubPriority, params object[] pStringA)
    {
        if (Globals.fDisableJustForIan == true)
        {
            if (ubPriority == Globals.MSG_BETAVERSION)
            {
                return;
            }
            else if (ubPriority == Globals.MSG_TESTVERSION)
            {
                return;
            }
            else if (ubPriority == Globals.MSG_DEBUG)
            {
                return;
            }
        }

        if (ubPriority == Globals.MSG_DEBUG)
        {
            usColor = Globals.DEBUG_COLOR;
        }

        if (ubPriority == Globals.MSG_BETAVERSION)
        {
            usColor = Globals.BETAVERSION_COLOR;
        }

        if (ubPriority == Globals.MSG_TESTVERSION)
        {
            usColor = Globals.TESTVERSION_COLOR;
        }

        //va_start(argptr, pStringA);
        //vwprintf(DestString, pStringA, argptr);
        //va_end(argptr);

        // pass onto tactical message and mapscreen message
        TacticalScreenMsg(usColor, ubPriority, pStringA);

        //	if( ( ubPriority != MSG_DEBUG ) && ( ubPriority != MSG_TESTVERSION ) )
        {
            MapScreenMessage(usColor, ubPriority, pStringA);
        }

        if (Globals.guiCurrentScreen == ScreenName.MAP_SCREEN)
        {
            PlayNewMessageSound();
        }
        else
        {
            fOkToBeepNewMessage = true;
        }

        return;
    }

    private static void ClearWrappedStrings(WRAPPED_STRING? pStringWrapperHead)
    {
        WRAPPED_STRING? pNode = pStringWrapperHead;
        WRAPPED_STRING? pDeleteNode = null;
        // clear out a link list of wrapped string structures

        // error check, is there a node to delete?
        if (pNode == null)
        {
            // leave,
            return;
        }

        do
        {

            // set delete node as current node
            pDeleteNode = pNode;

            // set current node as next node
            pNode = pNode.pNextWrappedString;

            // clear out delete node
            pDeleteNode = null;

        } while (pNode is not null);


        //	MemFree( pNode );

        pStringWrapperHead = null;

    }


    // new tactical and mapscreen message system
    private static void TacticalScreenMsg(FontColor usColor, int ubPriority, params object[] pStringA)
    {
        // this function sets up the string into several single line structures

        ScrollStringStPtr? pStringSt;
        FontStyle uiFont = FontStyle.TINYFONT1;
        int usPosition = 0;
        int usCount = 0;
        int usStringLength = 0;
        int usCurrentSPosition = 0;
        int usCurrentLookup = 0;
        //wchar_t *pString;
        bool fLastLine = false;

        string DestString = string.Empty, DestStringA;
        //wchar_t *pStringBuffer;
        bool fMultiLine = false;
        ScrollStringStPtr? pTempStringSt = null;
        WRAPPED_STRING? pStringWrapper = null;
        WRAPPED_STRING? pStringWrapperHead = null;
        bool fNewString = false;

        if (Globals.giTimeCompressMode > TIME_COMPRESS.TIME_COMPRESS_X1)
        {
            return;
        }

        if (Globals.fDisableJustForIan == true && ubPriority != Globals.MSG_ERROR && ubPriority != Globals.MSG_INTERFACE)
        {
            return;
        }

        if (ubPriority == Globals.MSG_BETAVERSION)
        {
            usColor = Globals.BETAVERSION_COLOR;
            WriteMessageToFile(DestString);

        }

        if (ubPriority == Globals.MSG_TESTVERSION)
        {
            usColor = Globals.TESTVERSION_COLOR;

            WriteMessageToFile(DestString);

        }


        if (fFirstTimeInMessageSystem)
        {
            // Init display array!
            //Globals.gpDisplayList, 0, sizeof(gpDisplayList));
            fFirstTimeInMessageSystem = false;
            //if(!(InitializeMutex(SCROLL_MESSAGE_MUTEX,"ScrollMessageMutex" )))
            //	return;
        }


        pStringSt = pStringS;
        while (GetNextString(pStringSt) is not null)
        {
            pStringSt = GetNextString(pStringSt);
        }

        //va_start(argptr, pStringA);         // Set up variable argument pointer
        //vwprintf(DestString, pStringA, argptr); // process gprintf string (get output str)
        //va_end(argptr);

        if (ubPriority == Globals.MSG_DEBUG)
        {
            usColor = Globals.DEBUG_COLOR;
            DestStringA = DestString;
            //wprintf(DestString, "Debug: %s", DestStringA);
            WriteMessageToFile(DestStringA);
        }

        if (ubPriority == Globals.MSG_DIALOG)
        {
            usColor = Globals.DIALOGUE_COLOR;
        }

        if (ubPriority == Globals.MSG_INTERFACE)
        {
            usColor = Globals.INTERFACE_COLOR;
        }



        pStringWrapperHead = LineWrap(uiFont, LINE_WIDTH, usLineWidthIfWordIsWiderThenWidth, DestString);
        pStringWrapper = pStringWrapperHead;
        if (pStringWrapper is null)
        {
            return;
        }

        fNewString = true;
        while (pStringWrapper.pNextWrappedString != null)
        {
            if (pStringSt is null)
            {
                pStringSt = AddString(pStringWrapper.sString, usColor, uiFont, fNewString, ubPriority);
                fNewString = false;
                pStringSt.pNext = null;
                pStringSt.pPrev = null;
                pStringS = pStringSt;
            }
            else
            {
                pTempStringSt = AddString(pStringWrapper.sString, usColor, uiFont, fNewString, ubPriority);
                fNewString = false;
                pTempStringSt.pPrev = pStringSt;
                pStringSt.pNext = pTempStringSt;
                pStringSt = pTempStringSt;
                pTempStringSt.pNext = null;
            }
            pStringWrapper = pStringWrapper.pNextWrappedString;
        }
        pTempStringSt = AddString(pStringWrapper.sString, usColor, uiFont, fNewString, ubPriority);
        if (pStringSt is not null)
        {
            pStringSt.pNext = pTempStringSt;
            pTempStringSt.pPrev = pStringSt;
            pStringSt = pTempStringSt;
            pStringSt.pNext = null;
        }
        else
        {
            pStringSt = pTempStringSt;
            pStringSt.pNext = null;
            pStringSt.pPrev = null;
            pStringS = pStringSt;
        }

        // clear up list of wrapped strings
        ClearWrappedStrings(pStringWrapperHead);

        //LeaveMutex(SCROLL_MESSAGE_MUTEX, __LINE__, __FILE__);
        return;
    }

    private static void MapScreenMessage(FontColor usColor, int ubPriority, params object[] pStringA)
    {
        // this function sets up the string into several single line structures

        ScrollStringStPtr? pStringSt;
        FontStyle uiFont = Globals.MAP_SCREEN_MESSAGE_FONT;
        int usPosition = 0;
        int usCount = 0;
        int usStringLength = 0;
        int usCurrentSPosition = 0;
        int usCurrentLookup = 0;
        //wchar_t *pString;
        bool fLastLine = false;
        string DestString = string.Empty, DestStringA = string.Empty;
        //wchar_t *pStringBuffer;
        bool fMultiLine = false;
        WRAPPED_STRING? pStringWrapper = null;
        WRAPPED_STRING? pStringWrapperHead = null;
        bool fNewString = false;

        if (Globals.fDisableJustForIan == true)
        {
            if (ubPriority == Globals.MSG_BETAVERSION)
            {
                return;
            }
            else if (ubPriority == Globals.MSG_TESTVERSION)
            {
                return;
            }
            else if (ubPriority == Globals.MSG_DEBUG)
            {
                return;
            }
        }

        if (ubPriority == Globals.MSG_BETAVERSION)
        {
            usColor = Globals.BETAVERSION_COLOR;

            WriteMessageToFile(DestString);
        }

        if (ubPriority == Globals.MSG_TESTVERSION)
        {
            usColor = Globals.TESTVERSION_COLOR;

            WriteMessageToFile(DestString);
        }
        // OK, check if we are ani imeediate feedback message, if so, do something else!
        if (ubPriority == Globals.MSG_UI_FEEDBACK)
        {
            //va_start(argptr, pStringA);         // Set up variable argument pointer
            //vwprintf(DestString, pStringA, argptr); // process gprintf string (get output str)
            //va_end(argptr);

            BeginUIMessage(DestString);
            return;
        }

        if (ubPriority == Globals.MSG_SKULL_UI_FEEDBACK)
        {
            // va_start(argptr, pStringA);         // Set up variable argument pointer
            // vwprintf(DestString, pStringA, argptr); // process gprintf string (get output str)
            // va_end(argptr);

            InternalBeginUIMessage(true, DestString);
            return;
        }

        // check if error
        if (ubPriority == Globals.MSG_ERROR)
        {
            // va_start(argptr, pStringA);         // Set up variable argument pointer
            // vwprintf(DestString, pStringA, argptr); // process gprintf string (get output str)
            // va_end(argptr);

            // wprintf(DestStringA, "DEBUG: %s", DestString);

            BeginUIMessage(DestStringA);
            WriteMessageToFile(DestStringA);

            return;
        }


        // OK, check if we are an immediate MAP feedback message, if so, do something else!
        if ((ubPriority == Globals.MSG_MAP_UI_POSITION_UPPER) ||
                 (ubPriority == Globals.MSG_MAP_UI_POSITION_MIDDLE) ||
                 (ubPriority == Globals.MSG_MAP_UI_POSITION_LOWER))
        {
            // va_start(argptr, pStringA);         // Set up variable argument pointer
            // vwprintf(DestString, pStringA, argptr); // process gprintf string (get output str)
            // va_end(argptr);

            BeginMapUIMessage(ubPriority, DestString);
            return;
        }


        if (fFirstTimeInMessageSystem)
        {
            // Init display array!
            // Globals.gpDisplayList, 0, sizeof(gpDisplayList));
            fFirstTimeInMessageSystem = false;
            //if(!(InitializeMutex(SCROLL_MESSAGE_MUTEX,"ScrollMessageMutex" )))
            //	return;
        }


        pStringSt = pStringS;
        while (GetNextString(pStringSt) is not null)
        {
            pStringSt = GetNextString(pStringSt);
        }

        //va_start(argptr, pStringA);         // Set up variable argument pointer
        //vwprintf(DestString, pStringA, argptr); // process gprintf string (get output str)
        //va_end(argptr);

        if (ubPriority == Globals.MSG_DEBUG)
        {
            usColor = Globals.DEBUG_COLOR;
            // wcscpy(DestStringA, DestString);
            // wprintf(DestString, "Debug: %s", DestStringA);
        }

        if (ubPriority == Globals.MSG_DIALOG)
        {
            usColor = Globals.DIALOGUE_COLOR;
        }

        if (ubPriority == Globals.MSG_INTERFACE)
        {
            usColor = Globals.INTERFACE_COLOR;
        }

        pStringWrapperHead = LineWrap(uiFont, Globals.MAP_LINE_WIDTH, usLineWidthIfWordIsWiderThenWidth, DestString);
        pStringWrapper = pStringWrapperHead;
        if (pStringWrapper is null)
        {
            return;
        }

        fNewString = true;

        while (pStringWrapper.pNextWrappedString != null)
        {
            AddStringToMapScreenMessageList(pStringWrapper.sString, usColor, uiFont, fNewString, ubPriority);
            fNewString = false;

            pStringWrapper = pStringWrapper.pNextWrappedString;
        }

        AddStringToMapScreenMessageList(pStringWrapper.sString, usColor, uiFont, fNewString, ubPriority);


        // clear up list of wrapped strings
        ClearWrappedStrings(pStringWrapperHead);

        // play new message beep
        //PlayNewMessageSound( );

        MapScreenInterfaceBottom.MoveToEndOfMapScreenMessageList();

        //LeaveMutex(SCROLL_MESSAGE_MUTEX, __LINE__, __FILE__);
    }



    // add string to the map screen message list
    private static void AddStringToMapScreenMessageList(string pString, FontColor usColor, FontStyle uiFont, bool fStartOfNewString, int ubPriority)
    {
        int ubSlotIndex = 0;
        ScrollStringStPtr? pStringSt = null;


        pStringSt = new();

        SetString(pStringSt, pString);
        SetStringColor(pStringSt, usColor);
        pStringSt.uiFont = uiFont;
        pStringSt.fBeginningOfNewString = fStartOfNewString;
        pStringSt.uiFlags = ubPriority;
        pStringSt.iVideoOverlay = -1;

        // next/previous are not used, it's strictly a wraparound queue
        SetStringNext(pStringSt, null);
        SetStringPrev(pStringSt, null);


        // Figure out which queue slot index we're going to use to store this
        // If queue isn't full, this is easy, if is is full, we'll re-use the oldest slot
        // Must always keep the wraparound in mind, although this is easy enough with a static, fixed-size queue.


        // always store the new message at the END index

        // check if slot is being used, if so, clear it up
        if (Globals.gMapScreenMessageList[Globals.gubEndOfMapScreenMessageList] != null)
        {
            Globals.gMapScreenMessageList[Globals.gubEndOfMapScreenMessageList] = null;
        }

        // store the new message there
        Globals.gMapScreenMessageList[Globals.gubEndOfMapScreenMessageList] = pStringSt;

        // increment the end
        Globals.gubEndOfMapScreenMessageList = (Globals.gubEndOfMapScreenMessageList + 1) % 256;

        // if queue is full, end will now match the start
        if (Globals.gubEndOfMapScreenMessageList == Globals.gubStartOfMapScreenMessageList)
        {
            // if that's so, increment the start
            Globals.gubStartOfMapScreenMessageList = (Globals.gubStartOfMapScreenMessageList + 1) % 256;
        }
    }


    void DisplayStringsInMapScreenMessageList()
    {
        int ubCurrentStringIndex;
        int ubLinesPrinted;
        int sY;
        int usSpacing;


        FontSubSystem.SetFontDestBuffer(Surfaces.FRAME_BUFFER, 17, 360 + 6, 407, 360 + 101, false);

        FontSubSystem.SetFont(MAP_SCREEN_MESSAGE_FONT);       // no longer supports variable fonts
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        ubCurrentStringIndex = Globals.gubCurrentMapMessageString;

        sY = 377;
        usSpacing = FontSubSystem.GetFontHeight(Globals.MAP_SCREEN_MESSAGE_FONT);

        for (ubLinesPrinted = 0; ubLinesPrinted < Globals.MAX_MESSAGES_ON_MAP_BOTTOM; ubLinesPrinted++)
        {
            // reached the end of the list?
            if (ubCurrentStringIndex == Globals.gubEndOfMapScreenMessageList)
            {
                break;
            }

            // nothing stored there?
            if (Globals.gMapScreenMessageList[ubCurrentStringIndex] == null)
            {
                break;
            }

            // set font color
            FontSubSystem.SetFontForeground(Globals.gMapScreenMessageList[ubCurrentStringIndex].usColor);

            // print this line
            // mprintf_coded(20, sY, Globals.gMapScreenMessageList[ubCurrentStringIndex].pString16);

            sY += usSpacing;

            // next message index to print (may wrap around)
            ubCurrentStringIndex = (ubCurrentStringIndex + 1) % 256;
        }

        FontSubSystem.SetFontDestBuffer(Surfaces.FRAME_BUFFER, 0, 0, 640, 480, false);
    }


    private static uint uiSoundId = SoundManager.NO_SAMPLE;
    private static bool fFirstTimeInMessageSystem;

    private static void PlayNewMessageSound()
    {
        // play a new message sound, if there is one playing, do nothing

        if (uiSoundId != SoundManager.NO_SAMPLE)
        {
            // is sound playing?..don't play new one
            //if (SoundIsPlaying(uiSoundId) == true)
            //{
            //    return;
            //}
        }

        // otherwise no sound playing, play one
        //uiSoundId = PlayJA2SampleFromFile("Sounds\\newbeep.wav", RATE_11025, MIDVOLUME, 1, MIDDLEPAN);

        return;
    }


    bool SaveMapScreenMessagesToSaveGameFile(Stream hFile)
    {
        int uiNumBytesWritten = sizeof(int);
        int uiCount;
        int uiSizeOfString;
        StringSaveStruct StringSave;


        //	write to the begining of the message list
        //FileWrite(hFile, Globals.gubEndOfMapScreenMessageList, sizeof(int), out uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int))
        {
            return (false);
        }

        //FileWrite(hFile, &gubStartOfMapScreenMessageList, sizeof(int), &uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int))
        {
            return (false);
        }

        //	write the current message string
        //FileWrite(hFile, &gubCurrentMapMessageString, sizeof(int), &uiNumBytesWritten);
        if (uiNumBytesWritten != sizeof(int))
        {
            return (false);
        }


        //Loopthrough all the messages
        for (uiCount = 0; uiCount < 256; uiCount++)
        {
            if (Globals.gMapScreenMessageList[uiCount] is not null)
            {
                uiSizeOfString = (Globals.gMapScreenMessageList[uiCount].pString16.Length) + 1 * 2;
            }
            else
            {
                uiSizeOfString = 0;
            }

            //	write to the file the size of the message
            //FileWrite(hFile, &uiSizeOfString, sizeof(int), &uiNumBytesWritten);
            if (uiNumBytesWritten != sizeof(int))
            {
                return (false);
            }

            //if there is a message
            if (uiSizeOfString > 0)
            {
                //	write the message to the file
                //FileWrite(hFile, gMapScreenMessageList[uiCount].pString16, uiSizeOfString, &uiNumBytesWritten);
                if (uiNumBytesWritten != uiSizeOfString)
                {
                    return (false);
                }

                // Create  the saved string struct
                StringSave.uiFont = Globals.gMapScreenMessageList[uiCount].uiFont;
                StringSave.usColor = Globals.gMapScreenMessageList[uiCount].usColor;
                StringSave.fBeginningOfNewString = Globals.gMapScreenMessageList[uiCount].fBeginningOfNewString;
                StringSave.uiTimeOfLastUpdate = Globals.gMapScreenMessageList[uiCount].uiTimeOfLastUpdate;
                StringSave.uiFlags = Globals.gMapScreenMessageList[uiCount].uiFlags;


                //Write the rest of the message information to the saved game file
                //FileWrite(hFile, &StringSave, sizeof(StringSaveStruct), &uiNumBytesWritten);
                // if (uiNumBytesWritten != sizeof(StringSaveStruct))
                {
                    return (false);
                }
            }

        }

        return (true);
    }


    public bool LoadMapScreenMessagesFromSaveGameFile(Stream hFile)
    {
        int uiNumBytesRead = sizeof(int);
        int uiCount;
        int uiSizeOfString = 0;
        StringSaveStruct StringSave = new();
        string SavedString = string.Empty;// [512];

        // clear tactical message queue
        ClearTacticalMessageQueue();

        Globals.gubEndOfMapScreenMessageList = 0;
        Globals.gubStartOfMapScreenMessageList = 0;
        Globals.gubCurrentMapMessageString = 0;

        //	Read to the begining of the message list
        //FileRead(hFile, &gubEndOfMapScreenMessageList, sizeof(int), &uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int))
        {
            return (false);
        }

        //	Read the current message string
        //FileRead(hFile, &gubStartOfMapScreenMessageList, sizeof(int), &uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int))
        {
            return (false);
        }

        //	Read the current message string
        //FileRead(hFile, &gubCurrentMapMessageString, sizeof(int), &uiNumBytesRead);
        if (uiNumBytesRead != sizeof(int))
        {
            return (false);
        }

        //Loopthrough all the messages
        for (uiCount = 0; uiCount < 256; uiCount++)
        {
            //	Read to the file the size of the message
            //FileRead(hFile, &uiSizeOfString, sizeof(int), &uiNumBytesRead);
            if (uiNumBytesRead != sizeof(int))
            {
                return (false);
            }

            //if there is a message
            if (uiSizeOfString > 0)
            {
                //	Read the message from the file
                //FileRead(hFile, SavedString, uiSizeOfString, &uiNumBytesRead);
                if (uiNumBytesRead != uiSizeOfString)
                {
                    return (false);
                }

                //if there is an existing string,delete it
                if (Globals.gMapScreenMessageList[uiCount] is not null)
                {
                    if (Globals.gMapScreenMessageList[uiCount].pString16 is not null)
                    {
                        Globals.gMapScreenMessageList[uiCount] = null;
                    }
                }
                else
                {
                    // There is now message here, add one
                    ScrollStringStPtr sScroll;


                    sScroll = new();

                    if (sScroll == null)
                    {
                        return (false);
                    }

                    Globals.gMapScreenMessageList[uiCount] = sScroll;
                }

                //allocate space for the new string
                Globals.gMapScreenMessageList[uiCount].pString16 = string.Empty;
                if (Globals.gMapScreenMessageList[uiCount].pString16 == null)
                {
                    return (false);
                }

                //copy the string over
                Globals.gMapScreenMessageList[uiCount].pString16 = SavedString;


                //Read the rest of the message information to the saved game file
                //FileRead(hFile, StringSave, sizeof(StringSaveStruct), out uiNumBytesRead);
                if (uiNumBytesRead != 0)// sizeof(StringSaveStruct))
                {
                    return (false);
                }

                // Create  the saved string struct
                Globals.gMapScreenMessageList[uiCount].uiFont = StringSave.uiFont;
                Globals.gMapScreenMessageList[uiCount].usColor = StringSave.usColor;
                Globals.gMapScreenMessageList[uiCount].uiFlags = StringSave.uiFlags;
                Globals.gMapScreenMessageList[uiCount].fBeginningOfNewString = StringSave.fBeginningOfNewString;
                Globals.gMapScreenMessageList[uiCount].uiTimeOfLastUpdate = StringSave.uiTimeOfLastUpdate;
            }
            else
            {
                Globals.gMapScreenMessageList[uiCount] = null;
            }
        }


        // this will set a valid value for gubFirstMapscreenMessageIndex, which isn't being saved/restored
        MapScreenInterfaceBottom.MoveToEndOfMapScreenMessageList();

        return (true);
    }


    void HandleLastQuotePopUpTimer()
    {
        if ((Globals.fTextBoxMouseRegionCreated == false)
            || (Globals.fDialogueBoxDueToLastMessage == false))
        {
            return;
        }

        // check if timed out
        if (Globals.GetJA2Clock() - Globals.guiDialogueLastQuoteTime > Globals.guiDialogueLastQuoteDelay)
        {
            // done clear up
            DialogControl.ShutDownLastQuoteTacticalTextBox();
            Globals.guiDialogueLastQuoteTime = 0;
            Globals.guiDialogueLastQuoteDelay = 0;

        }
    }


    ScrollStringStPtr? MoveToBeginningOfMessageQueue()
    {
        ScrollStringStPtr pStringSt = pStringS;

        if (pStringSt == null)
        {
            return (null);
        }

        while (pStringSt.pPrev is not null)
        {
            pStringSt = pStringSt.pPrev;
        }

        return (pStringSt);
    }



    int GetMessageQueueSize()
    {
        ScrollStringStPtr pStringSt = pStringS;
        int iCounter = 0;

        pStringSt = MoveToBeginningOfMessageQueue();

        while (pStringSt is not null)
        {
            iCounter++;
            pStringSt = pStringSt.pNext;
        }

        return (iCounter);
    }



    void ClearTacticalMessageQueue()
    {
        ScrollStringStPtr? pStringSt = pStringS;
        ScrollStringStPtr? pOtherStringSt = pStringS;

        ClearDisplayedListOfTacticalStrings();

        // now run through all the tactical messages
        while (pStringSt is not null)
        {
            pOtherStringSt = pStringSt;
            pStringSt = pStringSt.pNext;
            pOtherStringSt = null;
        }

        pStringS = null;

        return;
    }

    private static void WriteMessageToFile(string pString)
    {
        //# if JA2BETAVERSION
        //
        //        FILE* fp;
        //
        //        fp = fopen("DebugMessage.txt", "a");
        //
        //        if (fp == null)
        //        {
        //            return;
        //        }
        //
        //        fprintf(fp, "%S\n", pString);
        //        fclose(fp);
        //
        //#endif
    }



    void InitGlobalMessageList()
    {
        int iCounter = 0;

        for (iCounter = 0; iCounter < 256; iCounter++)
        {
            Globals.gMapScreenMessageList[iCounter] = null;
        }

        Globals.gubEndOfMapScreenMessageList = 0;
        Globals.gubStartOfMapScreenMessageList = 0;
        Globals.gubCurrentMapMessageString = 0;
        //	ubTempPosition = 0;

        return;
    }


    void FreeGlobalMessageList()
    {
        int iCounter = 0;

        for (iCounter = 0; iCounter < 256; iCounter++)
        {
            // check if next unit is empty, if not...clear it up
            if (Globals.gMapScreenMessageList[iCounter] != null)
            {
                //gMapScreenMessageList[iCounter].pString16);
                Globals.gMapScreenMessageList[iCounter] = null;
            }
        }

        InitGlobalMessageList();

        return;
    }


    public static int GetRangeOfMapScreenMessages()
    {
        int ubRange = 0;

        // NOTE: End is non-inclusive, so start/end 0/0 means no messages, 0/1 means 1 message, etc.
        if (Globals.gubStartOfMapScreenMessageList <= Globals.gubEndOfMapScreenMessageList)
        {
            ubRange = Globals.gubEndOfMapScreenMessageList - Globals.gubStartOfMapScreenMessageList;
        }
        else
        {
            // this should always be 255 now, since this only happens when queue fills up, and we never remove any messages
            ubRange = (Globals.gubEndOfMapScreenMessageList + 256) - Globals.gubStartOfMapScreenMessageList;
        }

        return (ubRange);
    }
}

public class ScrollStringStPtr
{
    public string? pString16;
    public int iVideoOverlay;
    public FontStyle uiFont;
    public FontColor usColor;
    public int uiFlags;
    public bool fBeginningOfNewString;
    public uint uiTimeOfLastUpdate;
    public int[] uiPadding = new int[5];
    public ScrollStringStPtr? pNext;
    public ScrollStringStPtr? pPrev;
};

public struct StringSaveStruct
{
    public FontStyle uiFont;
    public uint uiTimeOfLastUpdate;
    public int uiFlags;
    public int[] uiPadding;// [3];
    public FontColor usColor;
    public bool fBeginningOfNewString;
}
