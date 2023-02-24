using System;
using SharpAlliance.Core.Managers;

namespace SharpAlliance.Core.SubSystems;

public class History
{
    // the page flipping buttons
    int[] giHistoryButton = new int[2];
    int[] giHistoryButtonImage = new int[2];
    bool fInHistoryMode = false;


    // current page displayed
    int iCurrentHistoryPage = 1;



    // the History record list
    history pHistoryListHead = null;

    // current History record (the one at the top of the current page)
    history pCurrentHistory = null;


    // last page in list
    int guiLastPageInHistoryRecordsList = 0;

    int SetHistoryFact(int ubCode, int ubSecondCode, int uiDate, int sSectorX, int sSectorY)
    {
        // adds History item to player's log(History List), returns unique id number of it
        // outside of the History system(the code in this .c file), this is the only function you'll ever need
        int uiId = 0;
        int ubColor = 0;
        history pHistory = pHistoryListHead;

        // clear the list
        ClearHistoryList();

        // process the actual data
        if (ubCode == HISTORY.QUEST_FINISHED)
        {
            ubColor = 0;
        }
        else
        {
            ubColor = 1;
        }
        uiId = ProcessAndEnterAHistoryRecord(ubCode, uiDate, ubSecondCode, sSectorX, sSectorY, 0, ubColor);
        ScreenMsg(FONT_MCOLOR_LTYELLOW, Globals.MSG_INTERFACE, pMessageStrings[MSG.HISTORY_UPDATED]);

        // history list head
        pHistory = pHistoryListHead;

        // append to end of file
        AppendHistoryToEndOfFile(pHistory);


        // if in history mode, reload current page
        if (fInHistoryMode)
        {
            iCurrentHistoryPage--;

            // load in first page
            LoadNextHistoryPage();
        }


        // return unique id of this transaction
        return uiId;
    }


    int AddHistoryToPlayersLog(int ubCode, int ubSecondCode, int uiDate, int sSectorX, int sSectorY)
    {
        // adds History item to player's log(History List), returns unique id number of it
        // outside of the History system(the code in this .c file), this is the only function you'll ever need
        int uiId = 0;
        history pHistory = pHistoryListHead;

        // clear the list
        ClearHistoryList();

        // process the actual data
        uiId = ProcessAndEnterAHistoryRecord(ubCode, uiDate, ubSecondCode, sSectorX, sSectorY, 0, 0);
        ScreenMsg(FONT_MCOLOR_LTYELLOW, Globals.MSG_INTERFACE, pMessageStrings[MSG.HISTORY_UPDATED]);

        // history list head
        pHistory = pHistoryListHead;

        // append to end of file
        AppendHistoryToEndOfFile(pHistory);


        // if in history mode, reload current page
        if (fInHistoryMode)
        {
            iCurrentHistoryPage--;

            // load in first page
            LoadNextHistoryPage();
        }


        // return unique id of this transaction
        return uiId;
    }


    void GameInitHistory()
    {
        if ((FileExists(HISTORY_DATA_FILE)))
        {
            // unlink history file
            FileClearAttributes(HISTORY_DATA_FILE);
            FileDelete(HISTORY_DATA_FILE);
        }

        AddHistoryToPlayersLog(HISTORY.ACCEPTED_ASSIGNMENT_FROM_ENRICO, 0, GetWorldTotalMin(), -1, -1);

    }

    void EnterHistory()
    {

        // load the graphics
        LoadHistory();

        // create History buttons
        CreateHistoryButtons();

        // reset current to first page
        if (LaptopSaveInfo.iCurrentHistoryPage > 0)
            iCurrentHistoryPage = LaptopSaveInfo.iCurrentHistoryPage - 1;
        else
            iCurrentHistoryPage = 0;

        // load in first page
        LoadNextHistoryPage();


        // render hbackground
        RenderHistory();


        // set the fact we are in the history viewer 
        fInHistoryMode = true;

        // build Historys list
        //OpenAndReadHistoryFile( );

        // force redraw of the entire screen
        //fReDrawScreenFlag=true;

        // set inital states
        SetHistoryButtonStates();

        return;
    }

    void ExitHistory()
    {
        LaptopSaveInfo.iCurrentHistoryPage = iCurrentHistoryPage;

        // not in History system anymore
        fInHistoryMode = false;


        // write out history list to file
        //OpenAndWriteHistoryFile( );

        // delete graphics
        RemoveHistory();

        // delete buttons
        DestroyHistoryButtons();

        ClearHistoryList();


        return;
    }

    void HandleHistory()
    {
        // DEF 2/5/99 Dont need to update EVERY FRAME!!!!
        // check and update status of buttons  
        //  SetHistoryButtonStates( );
    }

    void RenderHistory()
    {
        //render the background to the display
        RenderHistoryBackGround();

        // the title bar text
        DrawHistoryTitleText();

        // the actual lists background
        DisplayHistoryListBackground();

        // the headers to each column
        DisplayHistoryListHeaders();

        // render the currentpage of records
        DrawAPageofHistoryRecords();

        // stuff at top of page, the date range and page numbers 
        DisplayPageNumberAndDateRange();

        // title bar icon
        BlitTitleBarIcons();

        return;
    }


    bool LoadHistory()
    {
        VOBJECT_DESC VObjectDesc;
        // load History video objects into memory

        // title bar
        VObjectDesc.fCreateFlags = VOBJECT_CREATE_FROMFILE;
        FilenameForBPP("LAPTOP\\programtitlebar.sti", VObjectDesc.ImageFile);
        CHECKF(AddVideoObject(&VObjectDesc, &guiTITLE));

        // top portion of the screen background
        VObjectDesc.fCreateFlags = VOBJECT_CREATE_FROMFILE;
        FilenameForBPP("LAPTOP\\historywindow.sti", VObjectDesc.ImageFile);
        CHECKF(AddVideoObject(&VObjectDesc, &guiTOP));


        // shaded line
        VObjectDesc.fCreateFlags = VOBJECT_CREATE_FROMFILE;
        FilenameForBPP("LAPTOP\\historylines.sti", VObjectDesc.ImageFile);
        CHECKF(AddVideoObject(&VObjectDesc, &guiSHADELINE));

        /*
        Not being used???  DF commented out	
          // vert  line
          VObjectDesc.fCreateFlags=VOBJECT_CREATE_FROMFILE;
            FilenameForBPP("LAPTOP\\historyvertline.sti", VObjectDesc.ImageFile);
            CHECKF(AddVideoObject(&VObjectDesc, &guiVERTLINE));
        */
        // black divider line - long ( 480 length)
        VObjectDesc.fCreateFlags = VOBJECT_CREATE_FROMFILE;
        FilenameForBPP("LAPTOP\\divisionline480.sti", VObjectDesc.ImageFile);
        CHECKF(AddVideoObject(&VObjectDesc, &guiLONGLINE));

        return (true);
    }

    void RemoveHistory()
    {

        // delete history video objects from memory
        DeleteVideoObjectFromIndex(Globals.guiLONGLINE);
        DeleteVideoObjectFromIndex(Globals.guiTOP);
        DeleteVideoObjectFromIndex(Globals.guiTITLE);
        DeleteVideoObjectFromIndex(Globals.guiSHADELINE);

        return;
    }


    void RenderHistoryBackGround()
    {
        // render generic background for history system
        HVOBJECT hHandle;
        int iCounter = 0;

        // get title bar object
        GetVideoObject(out hHandle, Globals.guiTITLE);

        // blt title bar to screen
        BltVideoObject(FRAME_BUFFER, hHandle, 0, Globals.TOP_X, Globals.TOP_Y - 2, VO_BLT_SRCTRANSPARENCY, null);


        // get and blt the top part of the screen, video object and blt to screen
        GetVideoObject(out hHandle, Globals.guiTOP);
        BltVideoObject(FRAME_BUFFER, hHandle, 0, Globals.TOP_X, Globals.TOP_Y + 22, VO_BLT_SRCTRANSPARENCY, null);

        // display background for history list
        DisplayHistoryListBackground();
        return;
    }

    void DrawHistoryTitleText()
    {
        // setup the font stuff
        SetFont(HISTORY.HEADER_FONT);
        SetFontForeground(FontColor.FONT_WHITE);
        SetFontBackground(FontColor.FONT_BLACK);
        SetFontShadow(FontShadow.DEFAULT_SHADOW);

        // draw the pages title
        //mprintf(Globals.TITLE_X, Globals.TITLE_Y, pHistoryTitle[0]);

        return;
    }

    void CreateHistoryButtons()
    {

        // the prev page button
        Globals.giHistoryButtonImage[(int)PAGE_BUTTON.PREV_PAGE_BUTTON] = LoadButtonImage("LAPTOP\\arrows.sti", -1, 0, -1, 1, -1);
        Globals.giHistoryButton[(int)PAGE_BUTTON.PREV_PAGE_BUTTON] = QuickCreateButton(Globals.giHistoryButtonImage[(int)PAGE_BUTTON.PREV_PAGE_BUTTON], Globals.PREV_BTN_X, Globals.BTN_Y,
                                            BUTTON_TOGGLE, MSYS_PRIORITY_HIGHEST - 1,
                                            (GUI_CALLBACK)BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)BtnHistoryDisplayPrevPageCallBack);

        // the next page button
        Globals.giHistoryButtonImage[NEXT_PAGE_BUTTON] = LoadButtonImage("LAPTOP\\arrows.sti", -1, 6, -1, 7, -1);
        Globals.giHistoryButton[NEXT_PAGE_BUTTON] = QuickCreateButton(giHistoryButtonImage[NEXT_PAGE_BUTTON], NEXT_BTN_X, BTN_Y,
                                            BUTTON_TOGGLE, MSYS_PRIORITY_HIGHEST - 1,
                                                (GUI_CALLBACK)BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)BtnHistoryDisplayNextPageCallBack);


        // set buttons
        SetButtonCursor(giHistoryButton[0], Globals.CURSOR_LAPTOP_SCREEN);
        SetButtonCursor(giHistoryButton[1], Globals.CURSOR_LAPTOP_SCREEN);

        return;
    }


    void DestroyHistoryButtons()
    {

        // remove History buttons and images from memory

        // next page button
        RemoveButton(giHistoryButton[1]);
        UnloadButtonImage(giHistoryButtonImage[1]);

        // prev page button
        RemoveButton(giHistoryButton[0]);
        UnloadButtonImage(giHistoryButtonImage[0]);

        return;
    }

    void BtnHistoryDisplayPrevPageCallBack(GUI_BUTTON? btn, int reason)
    {
        // force redraw
        if (reason & MouseCallbackReasons.LBUTTON_DWN)
        {
            fReDrawScreenFlag = true;
        }


        // force redraw
        if (reason & MouseCallbackReasons.LBUTTON_UP)
        {
            fReDrawScreenFlag = true;
            btn.uiFlags &= ~(BUTTON_CLICKED_ON);
            // this page is > 0, there are pages before it, decrement

            if (iCurrentHistoryPage > 0)
            {
                LoadPreviousHistoryPage();
                //iCurrentHistoryPage--;
                DrawAPageofHistoryRecords();
            }

            // set new state
            SetHistoryButtonStates();
        }


    }

    void BtnHistoryDisplayNextPageCallBack(GUI_BUTTON? btn, MouseCallbackReasons reason)
    {

        if (reason & MouseCallbackReasons.LBUTTON_DWN)
        {
            fReDrawScreenFlag = true;
        }


        // force redraw
        if (reason & MouseCallbackReasons.LBUTTON_UP)
        {
            // increment currentPage
            btn.uiFlags &= ~(BUTTON_CLICKED_ON);
            LoadNextHistoryPage();
            // set new state
            SetHistoryButtonStates();
            fReDrawScreenFlag = true;
        }



    }

    bool IncrementCurrentPageHistoryDisplay()
    {
        // run through list, from pCurrentHistory, to NUM_RECORDS_PER_PAGE +1 HistoryUnits
        history pTempHistory = pCurrentHistory;
        bool fOkToIncrementPage = false;
        int iCounter = 0;
        HWFILE hFileHandle;
        int uiFileSize = 0;
        int uiSizeOfRecordsOnEachPage = 0;

        if (!(FileExists(HISTORY_DATA_FILE)))
            return (false);

        // open file
        hFileHandle = FileOpen(HISTORY_DATA_FILE, (FILE_OPEN_EXISTING | FILE_ACCESS_READ), false);

        // failed to get file, return
        if (!hFileHandle)
        {
            return (false);
        }

        // make sure file is more than 0 length
        if (FileGetSize(hFileHandle) == 0)
        {
            FileClose(hFileHandle);
            return (false);
        }

        uiFileSize = FileGetSize(hFileHandle) - 1;
        uiSizeOfRecordsOnEachPage = (NUM_RECORDS_PER_PAGE * (sizeof(int) + sizeof(int) + 3 * sizeof(int) + sizeof(int) + sizeof(int)));

        // is the file long enough?
        //  if( ( FileGetSize( hFileHandle ) - 1 ) / ( NUM_RECORDS_PER_PAGE * ( sizeof( int ) + sizeof( int ) + 3*sizeof( int )+ sizeof(int) + sizeof( int ) ) ) + 1 < ( int )( iCurrentHistoryPage + 1 ) )
        if (uiFileSize / uiSizeOfRecordsOnEachPage + 1 < (int)(iCurrentHistoryPage + 1))
        {
            // nope
            FileClose(hFileHandle);
            return (false);
        }
        else
        {
            iCurrentHistoryPage++;
            FileClose(hFileHandle);
        }


        /*
        // haven't reached end of list and not yet at beginning of next page
        while( ( pTempHistory )&&( ! fOkToIncrementPage ) )
        {
        // found the next page,  first record thereof
            if(iCounter==NUM_RECORDS_PER_PAGE+1)
            {
                fOkToIncrementPage=true;
              pCurrentHistory=pTempHistory.Next;
            }

            //next record
            pTempHistory=pTempHistory.Next;
        iCounter++;
        }
    */
        // if ok to increment, increment


        return (true);
    }


    int ProcessAndEnterAHistoryRecord(int ubCode, int uiDate, int ubSecondCode, int sSectorX, int sSectorY, int bSectorZ, int ubColor)
    {
        int uiId = 0;
        history pHistory = pHistoryListHead;

        // add to History list
        if (pHistory)
        {
            // go to end of list
            while (pHistory.Next)
                pHistory = pHistory.Next;

            // alloc space
            pHistory.Next = MemAlloc(sizeof(HistoryUnit));

            // increment id number
            uiId = pHistory.uiIdNumber + 1;

            // set up information passed
            pHistory = pHistory.Next;
            pHistory.Next = null;
            pHistory.ubCode = ubCode;
            pHistory.ubSecondCode = ubSecondCode;
            pHistory.uiDate = uiDate;
            pHistory.uiIdNumber = uiId;
            pHistory.sSectorX = sSectorX;
            pHistory.sSectorY = sSectorY;
            pHistory.bSectorZ = bSectorZ;
            pHistory.ubColor = ubColor;

        }
        else
        {
            // alloc space
            pHistory = MemAlloc(sizeof(HistoryUnit));

            // setup info passed
            pHistory.Next = null;
            pHistory.ubCode = ubCode;
            pHistory.ubSecondCode = ubSecondCode;
            pHistory.uiDate = uiDate;
            pHistory.uiIdNumber = uiId;
            pHistoryListHead = pHistory;
            pHistory.sSectorX = sSectorX;
            pHistory.sSectorY = sSectorY;
            pHistory.bSectorZ = bSectorZ;
            pHistory.ubColor = ubColor;

        }

        return uiId;
    }


    void OpenAndReadHistoryFile()
    {
        // this procedure will open and read in data to the History list

        //HWFILE hFileHandle;
        int ubCode, ubSecondCode;
        int uiDate;
        int sSectorX, sSectorY;
        int bSectorZ = 0;
        int ubColor;
        int iBytesRead = 0;
        int uiByteCount = 0;

        // clear out the old list
        ClearHistoryList();

        // no file, return
        if (!(FileExists(HISTORY_DATA_FILE)))
            return;

        // open file
        hFileHandle = FileOpen(HISTORY_DATA_FILE, (FILE_OPEN_EXISTING | FILE_ACCESS_READ), false);

        // failed to get file, return
        if (!hFileHandle)
        {
            return;
        }

        // make sure file is more than 0 length
        if (FileGetSize(hFileHandle) == 0)
        {
            FileClose(hFileHandle);
            return;
        }

        // file exists, read in data, continue until file end
        while (FileGetSize(hFileHandle) > uiByteCount)
        {
            // read in other data
            // FileRead(hFileHandle, &ubCode, sizeof(int), &iBytesRead);
            // FileRead(hFileHandle, &ubSecondCode, sizeof(int), &iBytesRead);
            // FileRead(hFileHandle, &uiDate, sizeof(int), &iBytesRead);
            // FileRead(hFileHandle, &sSectorX, sizeof(int), &iBytesRead);
            // FileRead(hFileHandle, &sSectorY, sizeof(int), &iBytesRead);
            // FileRead(hFileHandle, &bSectorZ, sizeof(int), &iBytesRead);
            // FileRead(hFileHandle, &ubColor, sizeof(int), &iBytesRead);

            // add transaction
            ProcessAndEnterAHistoryRecord(ubCode, uiDate, ubSecondCode, sSectorX, sSectorY, bSectorZ, ubColor);

            // increment byte counter
            uiByteCount += SIZE_OF_HISTORY_FILE_RECORD;
        }

        // close file 
        FileClose(hFileHandle);

        return;
    }

    bool OpenAndWriteHistoryFile()
    {
        // this procedure will open and write out data from the History list

        HWFILE hFileHandle;
        int iBytesWritten = 0;
        history pHistoryList = pHistoryListHead;


        // open file
        hFileHandle = FileOpen(HISTORY_DATA_FILE, FILE_ACCESS_WRITE | FILE_CREATE_ALWAYS, false);

        // if no file exits, do nothing
        if (!hFileHandle)
        {
            return (false);
        }
        // write info, while there are elements left in the list
        while (pHistoryList)
        {
            // now write date and amount, and code
            FileWrite(hFileHandle, &(pHistoryList.ubCode), sizeof(int), null);
            FileWrite(hFileHandle, &(pHistoryList.ubSecondCode), sizeof(int), null);
            FileWrite(hFileHandle, &(pHistoryList.uiDate), sizeof(int), null);
            FileWrite(hFileHandle, &(pHistoryList.sSectorX), sizeof(int), null);
            FileWrite(hFileHandle, &(pHistoryList.sSectorY), sizeof(int), null);
            FileWrite(hFileHandle, &(pHistoryList.bSectorZ), sizeof(int), null);
            FileWrite(hFileHandle, &(pHistoryList.ubColor), sizeof(int), null);

            // next element in list
            pHistoryList = pHistoryList.Next;

        }

        // close file
        FileClose(hFileHandle);
        // clear out the old list
        ClearHistoryList();

        return (true);
    }


    void ClearHistoryList()
    {
        // remove each element from list of transactions

        history pHistoryList = pHistoryListHead;
        history pHistoryNode = pHistoryList;

        // while there are elements in the list left, delete them
        while (pHistoryList)
        {
            // set node to list head
            pHistoryNode = pHistoryList;

            // set list head to next node
            pHistoryList = pHistoryList.Next;

            // delete current node
            MemFree(pHistoryNode);
        }
        pHistoryListHead = null;

        return;
    }

    void DisplayHistoryListHeaders()
    {
        // this procedure will display the headers to each column in History
        int usX, usY;

        // font stuff
        SetFont(HISTORY.TEXT_FONT);
        SetFontForeground(FONT_BLACK);
        SetFontBackground(FONT_BLACK);
        SetFontShadow(NO_SHADOW);

        // the date header
        FindFontCenterCoordinates(RECORD_DATE_X + 5, 0, RECORD_DATE_WIDTH, 0, pHistoryHeaders[0], HISTORY.TEXT_FONT, &usX, &usY);
        mprintf(usX, RECORD_HEADER_Y, pHistoryHeaders[0]);

        // the date header
        FindFontCenterCoordinates(RECORD_DATE_X + RECORD_DATE_WIDTH + 5, 0, RECORD_LOCATION_WIDTH, 0, pHistoryHeaders[3], HISTORY.TEXT_FONT, &usX, &usY);
        mprintf(usX, RECORD_HEADER_Y, pHistoryHeaders[3]);

        // event header
        FindFontCenterCoordinates(RECORD_DATE_X + RECORD_DATE_WIDTH + RECORD_LOCATION_WIDTH + 5, 0, RECORD_LOCATION_WIDTH, 0, pHistoryHeaders[3], HISTORY.TEXT_FONT, &usX, &usY);
        mprintf(usX, RECORD_HEADER_Y, pHistoryHeaders[4]);
        // reset shadow
        SetFontShadow(DEFAULT_SHADOW);
        return;
    }


    void DisplayHistoryListBackground()
    {
        // this function will display the History list display background
        HVOBJECT hHandle;
        int iCounter = 0;



        // get shaded line object
        GetVideoObject(hHandle, Globals.guiSHADELINE);
        for (iCounter = 0; iCounter < 11; iCounter++)
        {
            // blt title bar to screen
            BltVideoObject(FRAME_BUFFER, hHandle, 0, Globals.TOP_X + 15, (Globals.TOP_DIVLINE_Y + Globals.BOX_HEIGHT * 2 * iCounter), VO_BLT_SRCTRANSPARENCY, null);
        }

        // the long hortizontal line int he records list display region
        GetVideoObject(out hHandle, Globals.guiLONGLINE);
        BltVideoObject(FRAME_BUFFER, hHandle, 0, Globals.TOP_X + 9, (Globals.TOP_DIVLINE_Y), VO_BLT_SRCTRANSPARENCY, null);
        BltVideoObject(FRAME_BUFFER, hHandle, 0, Globals.TOP_X + 9, (Globals.TOP_DIVLINE_Y + Globals.BOX_HEIGHT * 2 * 11), VO_BLT_SRCTRANSPARENCY, null);


        return;
    }

    void DrawHistoryRecordsText()
    {
        // draws the text of the records
        history pCurHistory = pHistoryListHead;
        history pTempHistory = pHistoryListHead;
        string sString;
        int iCounter = 0;
        int usX, usY;
        int iBalance = 0;
        int sX = 0, sY = 0;

        // setup the font stuff
        SetFont(HISTORY.TEXT_FONT);
        SetFontForeground(FONT_BLACK);
        SetFontBackground(FONT_BLACK);
        SetFontShadow(NO_SHADOW);

        // error check
        if (!pCurHistory)
            return;


        // loop through record list
        for (iCounter; iCounter < NUM_RECORDS_PER_PAGE; iCounter++)
        {
            if (pCurHistory.ubColor == 0)
            {
                SetFontForeground(FONT_BLACK);
            }
            else
            {
                SetFontForeground(FONT_RED);
            }
            // get and write the date
            wprintf(sString, "%d", (pCurHistory.uiDate / (24 * 60)));
            FindFontCenterCoordinates(Globals.RECORD_DATE_X + 5, 0, Globals.RECORD_DATE_WIDTH, 0, sString, Globals.HISTORY.TEXT_FONT, out usX, out usY);
            mprintf(usX, Globals.RECORD_Y + (iCounter * (Globals.BOX_HEIGHT)) + 3, sString);

            // now the actual history text
            //FindFontCenterCoordinates(RECORD_DATE_X + RECORD_DATE_WIDTH,0,RECORD_HISTORY.WIDTH,0,  pHistoryStrings[pCurHistory.ubCode], HISTORY.TEXT_FONT,&usX, &usY);
            ProcessHistoryTransactionString(sString, pCurHistory);
            //	mprintf(RECORD_DATE_X + RECORD_DATE_WIDTH + 25, RECORD_Y + ( iCounter * ( BOX_HEIGHT ) ) + 3, pHistoryStrings[pCurHistory.ubCode] );
            mprintf(Globals.RECORD_DATE_X + Globals.RECORD_LOCATION_WIDTH + Globals.RECORD_DATE_WIDTH + 15, Globals.RECORD_Y + (iCounter * (Globals.BOX_HEIGHT)) + 3, sString);


            // no location
            if ((pCurHistory.sSectorX == -1) || (pCurHistory.sSectorY == -1))
            {
                FindFontCenterCoordinates(Globals.RECORD_DATE_X + Globals.RECORD_DATE_WIDTH, 0, Globals.RECORD_LOCATION_WIDTH + 10, 0, pHistoryLocations[0], Globals.HISTORY.TEXT_FONT, out sX, out sY);
                mprintf(sX, RECORD_Y + (iCounter * (BOX_HEIGHT)) + 3, pHistoryLocations[0]);
            }
            else
            {
                GetSectorIDString(pCurHistory.sSectorX, pCurHistory.sSectorY, pCurHistory.bSectorZ, sString, true);
                FindFontCenterCoordinates(RECORD_DATE_X + RECORD_DATE_WIDTH, 0, RECORD_LOCATION_WIDTH + 10, 0, sString, HISTORY.TEXT_FONT, &sX, &sY);

                ReduceStringLength(sString, RECORD_LOCATION_WIDTH + 10, HISTORY.TEXT_FONT);

                mprintf(sX, RECORD_Y + (iCounter * (BOX_HEIGHT)) + 3, sString);
            }

            // restore font color
            SetFontForeground(FONT_BLACK);

            // next History
            pCurHistory = pCurHistory.Next;

            // last page, no Historys left, return
            if (!pCurHistory)
            {

                // restore shadow
                SetFontShadow(DEFAULT_SHADOW);
                return;
            }

        }

        // restore shadow
        SetFontShadow(DEFAULT_SHADOW);
        return;
    }


    void DrawAPageofHistoryRecords()
    {
        // this procedure will draw a series of history records to the screen
        int iCurPage = 1;
        int iCount = 0;
        pCurrentHistory = pHistoryListHead;

        // (re-)render background

        // the title bar text
        DrawHistoryTitleText();

        // the actual lists background
        DisplayHistoryListBackground();

        // the headers to each column
        DisplayHistoryListHeaders();


        // error check
        if (iCurrentHistoryPage == -1)
        {
            iCurrentHistoryPage = 0;
        }


        // current page is found, render  from here
        DrawHistoryRecordsText();

        // update page numbers, and date ranges
        DisplayPageNumberAndDateRange();

        return;
    }

    void DisplayPageNumberAndDateRange()
    {
        // this function will go through the list of 'histories' starting at current until end or 
        // MAX_PER_PAGE...it will get the date range and the page number
        int iLastPage = 0;
        int iCounter = 0;
        int uiLastDate;
        history pTempHistory = pHistoryListHead;
        string sString;



        // setup the font stuff
        SetFont(HISTORY_TEXT_FONT);
        SetFontForeground(FONT_BLACK);
        SetFontBackground(FONT_BLACK);
        SetFontShadow(NO_SHADOW);

        if (!pCurrentHistory)
        {
            wprintf(sString, "%s  %d / %d", pHistoryHeaders[1], 1, 1);
            mprintf(Globals.PAGE_NUMBER_X, Globals.PAGE_NUMBER_Y, sString);

            wprintf(sString, "%s %d - %d", pHistoryHeaders[2], 1, 1);
            mprintf(Globals.HISTORY_DATE_X, Globals.HISTORY_DATE_Y, sString);

            // reset shadow
            SetFontShadow(DEFAULT_SHADOW);

            return;
        }

        uiLastDate = pCurrentHistory.uiDate;

        /*
            // find last page
            while(pTempHistory)
            {
                iCounter++;
                pTempHistory=pTempHistory.Next;
            }

          // set last page
            iLastPage=iCounter/NUM_RECORDS_PER_PAGE;
        */

        iLastPage = GetNumberOfHistoryPages();

        // set temp to current, to get last date
        pTempHistory = pCurrentHistory;

        // reset counter
        iCounter = 0;

        // run through list until end or num_records, which ever first
        while ((pTempHistory is not null) && (iCounter < Globals.NUM_RECORDS_PER_PAGE))
        {
            uiLastDate = pTempHistory.uiDate;
            iCounter++;

            pTempHistory = pTempHistory.Next;
        }



        // get the last page

        wprintf(sString, "%s  %d / %d", pHistoryHeaders[1], iCurrentHistoryPage, iLastPage + 1);
        mprintf(Globals.PAGE_NUMBER_X, Globals.PAGE_NUMBER_Y, sString);

        wprintf(sString, "%s %d - %d", pHistoryHeaders[2], pCurrentHistory.uiDate / (24 * 60), uiLastDate / (24 * 60));
        mprintf(Globals.HISTORY.DATE_X, Globals.HISTORY.DATE_Y, sString);


        // reset shadow
        SetFontShadow(DEFAULT_SHADOW);

        return;
    }


    void ProcessHistoryTransactionString(string pString, history pHistory)
    {
        string sString;

        switch (pHistory.ubCode)
        {
            case HISTORY.ENTERED_HISTORY.MODE:
                wprintf(pString, pHistoryStrings[HISTORY.ENTERED_HISTORY.MODE]);
                break;

            case HISTORY.HIRED_MERC_FROM_AIM:
                wprintf(pString, pHistoryStrings[HISTORY.HIRED_MERC_FROM_AIM], Globals.gMercProfiles[pHistory.ubSecondCode].zName);
                break;

            case HISTORY.MERC_KILLED:
                if (pHistory.ubSecondCode != NO_PROFILE)
                {
                    wprintf(pString, pHistoryStrings[HISTORY.MERC_KILLED], Globals.gMercProfiles[pHistory.ubSecondCode].zName);
                }
                break;

            case HISTORY.HIRED_MERC_FROM_MERC:
                wprintf(pString, pHistoryStrings[HISTORY.HIRED_MERC_FROM_MERC], Globals.gMercProfiles[pHistory.ubSecondCode].zName);
                break;

            case HISTORY.SETTLED_ACCOUNTS_AT_MERC:
                wprintf(pString, pHistoryStrings[HISTORY.SETTLED_ACCOUNTS_AT_MERC]);
                break;
            case HISTORY.ACCEPTED_ASSIGNMENT_FROM_ENRICO:
                wprintf(pString, pHistoryStrings[HISTORY.ACCEPTED_ASSIGNMENT_FROM_ENRICO]);
                break;
            case (HISTORY.CHARACTER_GENERATED):
                wprintf(pString, pHistoryStrings[HISTORY.CHARACTER_GENERATED]);
                break;
            case (HISTORY.PURCHASED_INSURANCE):
                wprintf(pString, pHistoryStrings[HISTORY.PURCHASED_INSURANCE], Globals.gMercProfiles[pHistory.ubSecondCode].zNickname);
                break;
            case (HISTORY.CANCELLED_INSURANCE):
                wprintf(pString, pHistoryStrings[HISTORY.CANCELLED_INSURANCE], Globals.gMercProfiles[pHistory.ubSecondCode].zNickname);
                break;
            case (HISTORY.INSURANCE_CLAIM_PAYOUT):
                wprintf(pString, pHistoryStrings[HISTORY.INSURANCE_CLAIM_PAYOUT], Globals.gMercProfiles[pHistory.ubSecondCode].zNickname);
                break;

            case HISTORY.EXTENDED_CONTRACT_1_DAY:
                wprintf(pString, pHistoryStrings[HISTORY.EXTENDED_CONTRACT_1_DAY], Globals.gMercProfiles[pHistory.ubSecondCode].zNickname);
                break;

            case HISTORY.EXTENDED_CONTRACT_1_WEEK:
                wprintf(pString, pHistoryStrings[HISTORY.EXTENDED_CONTRACT_1_WEEK], Globals.gMercProfiles[pHistory.ubSecondCode].zNickname);
                break;

            case HISTORY.EXTENDED_CONTRACT_2_WEEK:
                wprintf(pString, pHistoryStrings[HISTORY.EXTENDED_CONTRACT_2_WEEK], Globals.gMercProfiles[pHistory.ubSecondCode].zNickname);
                break;

            case (HISTORY.MERC_FIRED):
                wprintf(pString, pHistoryStrings[HISTORY.MERC_FIRED], Globals.gMercProfiles[pHistory.ubSecondCode].zNickname);
                break;

            case (HISTORY.MERC_QUIT):
                wprintf(pString, pHistoryStrings[HISTORY.MERC_QUIT], Globals.gMercProfiles[pHistory.ubSecondCode].zNickname);
                break;

            case (HISTORY.QUEST_STARTED):
                GetQuestStartedString(pHistory.ubSecondCode, sString);
                wprintf(pString, sString);

                break;
            case (HISTORY.QUEST_FINISHED):
                GetQuestEndedString(pHistory.ubSecondCode, sString);
                wprintf(pString, sString);

                break;
            case (HISTORY.TALKED_TO_MINER):
                wprintf(pString, pHistoryStrings[HISTORY.TALKED_TO_MINER], pTownNames[pHistory.ubSecondCode]);
                break;
            case (HISTORY.LIBERATED_TOWN):
                wprintf(pString, pHistoryStrings[HISTORY.LIBERATED_TOWN], pTownNames[pHistory.ubSecondCode]);
                break;
            case (HISTORY.CHEAT_ENABLED):
                wprintf(pString, pHistoryStrings[HISTORY.CHEAT_ENABLED]);
                break;
            case HISTORY.TALKED_TO_FATHER_WALKER:
                wprintf(pString, pHistoryStrings[HISTORY.TALKED_TO_FATHER_WALKER]);
                break;
            case HISTORY.MERC_MARRIED_OFF:
                wprintf(pString, pHistoryStrings[HISTORY.MERC_MARRIED_OFF], gMercProfiles[pHistory.ubSecondCode].zNickname);
                break;
            case HISTORY.MERC_CONTRACT_EXPIRED:
                wprintf(pString, pHistoryStrings[HISTORY.MERC_CONTRACT_EXPIRED], gMercProfiles[pHistory.ubSecondCode].zName);
                break;
            case HISTORY.RPC_JOINED_TEAM:
                wprintf(pString, pHistoryStrings[HISTORY.RPC_JOINED_TEAM], gMercProfiles[pHistory.ubSecondCode].zName);
                break;
            case HISTORY.ENRICO_COMPLAINED:
                wprintf(pString, pHistoryStrings[HISTORY.ENRICO_COMPLAINED]);
                break;
            case HISTORY.MINE_RUNNING_OUT:
            case HISTORY.MINE_RAN_OUT:
            case HISTORY.MINE_SHUTDOWN:
            case HISTORY.MINE_REOPENED:
                // all the same format
                wprintf(pString, pHistoryStrings[pHistory.ubCode], pTownNames[pHistory.ubSecondCode]);
                break;
            case HISTORY.LOST_BOXING:
            case HISTORY.WON_BOXING:
            case HISTORY.DISQUALIFIED_BOXING:
            case HISTORY.NPC_KILLED:
            case HISTORY.MERC_KILLED_CHARACTER:
                wprintf(pString, pHistoryStrings[pHistory.ubCode], gMercProfiles[pHistory.ubSecondCode].zNickname);
                break;

            // ALL SIMPLE HISTORY LOG MSGS, NO PARAMS
            case HISTORY.FOUND_MONEY:
            case HISTORY.ASSASSIN:
            case HISTORY.DISCOVERED_TIXA:
            case HISTORY.DISCOVERED_ORTA:
            case HISTORY.GOT_ROCKET_RIFLES:
            case HISTORY.DEIDRANNA_DEAD_BODIES:
            case HISTORY.BOXING_MATCHES:
            case HISTORY.SOMETHING_IN_MINES:
            case HISTORY.DEVIN:
            case HISTORY.MIKE:
            case HISTORY.TONY:
            case HISTORY.KROTT:
            case HISTORY.KYLE:
            case HISTORY.MADLAB:
            case HISTORY.GABBY:
            case HISTORY.KEITH_OUT_OF_BUSINESS:
            case HISTORY.HOWARD_CYANIDE:
            case HISTORY.KEITH:
            case HISTORY.HOWARD:
            case HISTORY.PERKO:
            case HISTORY.SAM:
            case HISTORY.FRANZ:
            case HISTORY.ARNOLD:
            case HISTORY.FREDO:
            case HISTORY.RICHGUY_BALIME:
            case HISTORY.JAKE:
            case HISTORY.BUM_KEYCARD:
            case HISTORY.WALTER:
            case HISTORY.DAVE:
            case HISTORY.PABLO:
            case HISTORY.KINGPIN_MONEY:
            //VARIOUS BATTLE CONDITIONS
            case HISTORY.LOSTTOWNSECTOR:
            case HISTORY.DEFENDEDTOWNSECTOR:
            case HISTORY.LOSTBATTLE:
            case HISTORY.WONBATTLE:
            case HISTORY.FATALAMBUSH:
            case HISTORY.WIPEDOUTENEMYAMBUSH:
            case HISTORY.UNSUCCESSFULATTACK:
            case HISTORY.SUCCESSFULATTACK:
            case HISTORY.CREATURESATTACKED:
            case HISTORY.KILLEDBYBLOODCATS:
            case HISTORY.SLAUGHTEREDBLOODCATS:
            case HISTORY.GAVE_CARMEN_HEAD:
            case HISTORY.SLAY_MYSTERIOUSLY_LEFT:
                wprintf(pString, pHistoryStrings[pHistory.ubCode]);
                break;
        }
    }


    void DrawHistoryLocation(int sSectorX, int sSectorY)
    {
        // will draw the location of the history event 


        return;
    }


    void SetHistoryButtonStates()
    {
        // this function will look at what page we are viewing, enable and disable buttons as needed

        if (iCurrentHistoryPage == 1)
        {
            // first page, disable left buttons
            DisableButton(giHistoryButton[PREV_PAGE_BUTTON]);

        }
        else
        {
            // enable buttons
            EnableButton(giHistoryButton[PREV_PAGE_BUTTON]);

        }

        if (IncrementCurrentPageHistoryDisplay())
        {
            // decrement page
            iCurrentHistoryPage--;
            DrawAPageofHistoryRecords();

            // enable buttons
            EnableButton(giHistoryButton[NEXT_PAGE_BUTTON]);

        }
        else
        {
            DisableButton(giHistoryButton[NEXT_PAGE_BUTTON]);
        }
    }


    bool LoadInHistoryRecords(int uiPage)
    {
        // loads in records belogning, to page uiPage
        // no file, return
        bool fOkToContinue = true;
        int iCount = 0;
        HWFILE hFileHandle;
        int ubCode, ubSecondCode;
        int sSectorX, sSectorY;
        int bSectorZ;
        int uiDate;
        int ubColor;
        int iBytesRead = 0;
        int uiByteCount = 0;

        // check if bad page
        if (uiPage == 0)
        {
            return (false);
        }


        if (!(FileExists(HISTORY_DATA_FILE)))
            return (false);

        // open file
        hFileHandle = FileOpen(HISTORY_DATA_FILE, (FILE_OPEN_EXISTING | FILE_ACCESS_READ), false);

        // failed to get file, return
        if (!hFileHandle)
        {
            return (false);
        }

        // make sure file is more than 0 length
        if (FileGetSize(hFileHandle) == 0)
        {
            FileClose(hFileHandle);
            return (false);
        }

        // is the file long enough?
        if ((FileGetSize(hFileHandle) - 1) / (NUM_RECORDS_PER_PAGE * SIZE_OF_HISTORY_FILE_RECORD) + 1 < uiPage)
        {
            // nope
            FileClose(hFileHandle);
            return (false);
        }

        FileSeek(hFileHandle, (uiPage - 1) * NUM_RECORDS_PER_PAGE * (SIZE_OF_HISTORY_FILE_RECORD), FILE_SEEK_FROM_START);

        uiByteCount = (uiPage - 1) * NUM_RECORDS_PER_PAGE * (SIZE_OF_HISTORY_FILE_RECORD);
        // file exists, read in data, continue until end of page
        while ((iCount < NUM_RECORDS_PER_PAGE) && (fOkToContinue))
        {

            // read in other data
            FileRead(hFileHandle, out ubCode, sizeof(int), &iBytesRead);
            FileRead(hFileHandle, out ubSecondCode, sizeof(int), &iBytesRead);
            FileRead(hFileHandle, out uiDate, sizeof(int), &iBytesRead);
            FileRead(hFileHandle, out sSectorX, sizeof(int), &iBytesRead);
            FileRead(hFileHandle, out sSectorY, sizeof(int), &iBytesRead);
            FileRead(hFileHandle, out bSectorZ, sizeof(int), &iBytesRead);
            FileRead(hFileHandle, out ubColor, sizeof(int), &iBytesRead);

            // add transaction
            ProcessAndEnterAHistoryRecord(ubCode, uiDate, ubSecondCode, sSectorX, sSectorY, bSectorZ, ubColor);

            // increment byte counter
            uiByteCount += SIZE_OF_HISTORY_FILE_RECORD;

            // we've overextended our welcome, and bypassed end of file, get out
            if (uiByteCount >= FileGetSize(hFileHandle))
            {
                // not ok to continue
                fOkToContinue = false;
            }

            iCount++;
        }

        // close file 
        FileClose(hFileHandle);

        // check to see if we in fact have a list to display
        if (pHistoryListHead == null)
        {
            // got no records, return false
            return (false);
        }

        // set up current finance
        pCurrentHistory = pHistoryListHead;

        return (true);
    }


    bool WriteOutHistoryRecords(int uiPage)
    {
        // loads in records belogning, to page uiPage
        // no file, return
        bool fOkToContinue = true;
        int iCount = 0;
        HWFILE hFileHandle;
        history pList;
        int iBytesRead = 0;
        int uiByteCount = 0;

        // check if bad page
        if (uiPage == 0)
        {
            return (false);
        }


        if (!(FileExists(HISTORY_DATA_FILE)))
            return (false);

        // open file
        hFileHandle = FileOpen(HISTORY_DATA_FILE, (FILE_OPEN_EXISTING | FILE_ACCESS_WRITE), false);

        // failed to get file, return
        if (!hFileHandle)
        {
            return (false);
        }

        // make sure file is more than 0 length
        if (FileGetSize(hFileHandle) == 0)
        {
            FileClose(hFileHandle);
            return (false);
        }

        // is the file long enough?
        if ((FileGetSize(hFileHandle) - 1) / (NUM_RECORDS_PER_PAGE * SIZE_OF_HISTORY_FILE_RECORD) + 1 < uiPage)
        {
            // nope
            FileClose(hFileHandle);
            return (false);
        }

        pList = pHistoryListHead;

        if (pList == null)
        {
            return (false);
        }

        FileSeek(hFileHandle, sizeof(int) + (uiPage - 1) * NUM_RECORDS_PER_PAGE * SIZE_OF_HISTORY_FILE_RECORD, FILE_SEEK_FROM_START);

        uiByteCount = /*sizeof( int )+ */(uiPage - 1) * NUM_RECORDS_PER_PAGE * SIZE_OF_HISTORY_FILE_RECORD;
        // file exists, read in data, continue until end of page
        while ((iCount < NUM_RECORDS_PER_PAGE) && (fOkToContinue))
        {

            FileWrite(hFileHandle, (pList.ubCode), sizeof(int), null);
            FileWrite(hFileHandle, (pList.ubSecondCode), sizeof(int), null);
            FileWrite(hFileHandle, (pList.uiDate), sizeof(int), null);
            FileWrite(hFileHandle, (pList.sSectorX), sizeof(int), null);
            FileWrite(hFileHandle, (pList.sSectorY), sizeof(int), null);
            FileWrite(hFileHandle, (pList.bSectorZ), sizeof(int), null);
            FileWrite(hFileHandle, (pList.ubColor), sizeof(int), null);

            pList = pList.Next;

            // we've overextended our welcome, and bypassed end of file, get out
            if (pList == null)
            {
                // not ok to continue
                fOkToContinue = false;
            }

            iCount++;
        }

        // close file 
        FileClose(hFileHandle);

        ClearHistoryList();

        return (true);
    }

    bool LoadNextHistoryPage()
    {

        // clear out old list of records, and load in previous page worth of records
        ClearHistoryList();



        // now load in previous page's records, if we can
        if (LoadInHistoryRecords(iCurrentHistoryPage + 1))
        {
            iCurrentHistoryPage++;
            return (true);
        }
        else
        {
            LoadInHistoryRecords(iCurrentHistoryPage);
            return (false);
        }

    }


    bool LoadPreviousHistoryPage()
    {

        // clear out old list of records, and load in previous page worth of records
        ClearHistoryList();

        // load previous page
        if ((iCurrentHistoryPage == 1))
        {
            return (false);
        }

        // now load in previous page's records, if we can
        if (LoadInHistoryRecords(iCurrentHistoryPage - 1))
        {
            iCurrentHistoryPage--;
            return (true);
        }
        else
        {
            LoadInHistoryRecords(iCurrentHistoryPage);
            return (false);
        }
    }


    void SetLastPageInHistoryRecords()
    {
        // grabs the size of the file and interprets number of pages it will take up
        HWFILE hFileHandle;
        int iBytesRead = 0;

        // no file, return
        if (!(FileExists(HISTORY_DATA_FILE)))
            return;

        // open file
        hFileHandle = FileOpen(HISTORY_DATA_FILE, (FILE_OPEN_EXISTING | FILE_ACCESS_READ), false);

        // failed to get file, return
        if (!hFileHandle)
        {
            guiLastPageInHistoryRecordsList = 1;
            return;
        }

        // make sure file is more than 0 length
        if (FileGetSize(hFileHandle) == 0)
        {
            FileClose(hFileHandle);
            guiLastPageInHistoryRecordsList = 1;
            return;
        }


        // done with file, close it
        FileClose(hFileHandle);

        guiLastPageInHistoryRecordsList = ReadInLastElementOfHistoryListAndReturnIdNumber() / NUM_RECORDS_PER_PAGE;

        return;
    }

    int ReadInLastElementOfHistoryListAndReturnIdNumber()
    {
        // this function will read in the last unit in the history list, to grab it's id number


        HWFILE hFileHandle;
        int iBytesRead = 0;
        int iFileSize = 0;

        // no file, return
        if (!(FileExists(HISTORY_DATA_FILE)))
            return 0;

        // open file
        hFileHandle = FileOpen(HISTORY_DATA_FILE, (FILE_OPEN_EXISTING | FILE_ACCESS_READ), false);

        // failed to get file, return
        if (!hFileHandle)
        {
            return 0;
        }

        // make sure file is more than balance size + length of 1 record - 1 byte
        if (FileGetSize(hFileHandle) < SIZE_OF_HISTORY_FILE_RECORD)
        {
            FileClose(hFileHandle);
            return 0;
        }

        // size is?
        iFileSize = FileGetSize(hFileHandle);

        // done with file, close it
        FileClose(hFileHandle);

        // file size  / sizeof record in bytes is id
        return ((iFileSize) / (SIZE_OF_HISTORY_FILE_RECORD));

    }


    bool AppendHistoryToEndOfFile(history pHistory)
    {
        // will write the current finance to disk
        HWFILE hFileHandle;
        int iBytesWritten = 0;
        history pHistoryList = pHistoryListHead;


        // open file
        hFileHandle = FileOpen(HISTORY_DATA_FILE, FILE_ACCESS_WRITE | FILE_OPEN_ALWAYS, false);

        // if no file exits, do nothing
        if (!hFileHandle)
        {
            return (false);
        }

        // go to the end
        if (FileSeek(hFileHandle, 0, FILE_SEEK_FROM_END) == false)
        {
            // error
            FileClose(hFileHandle);
            return (false);
        }

        // now write date and amount, and code
        FileWrite(hFileHandle, (pHistoryList.ubCode), sizeof(int), null);
        FileWrite(hFileHandle, (pHistoryList.ubSecondCode), sizeof(int), null);
        FileWrite(hFileHandle, (pHistoryList.uiDate), sizeof(int), null);
        FileWrite(hFileHandle, (pHistoryList.sSectorX), sizeof(int), null);
        FileWrite(hFileHandle, (pHistoryList.sSectorY), sizeof(int), null);
        FileWrite(hFileHandle, (pHistoryList.bSectorZ), sizeof(int), null);
        FileWrite(hFileHandle, (pHistoryList.ubColor), sizeof(int), null);


        // close file
        FileClose(hFileHandle);

        return (true);
    }

    void ResetHistoryFact(int ubCode, int sSectorX, int sSectorY)
    {
        // run through history list
        int iOldHistoryPage = iCurrentHistoryPage;
        history? pList = pHistoryListHead;
        bool fFound = false;

        // set current page to before list	
        iCurrentHistoryPage = 0;

        SetLastPageInHistoryRecords();

        OpenAndReadHistoryFile();

        pList = pHistoryListHead;

        while (pList is not null)
        {
            if ((pList.ubSecondCode == ubCode) && (pList.ubCode == HISTORY.QUEST_STARTED))
            {
                // reset color
                pList.ubColor = 0;
                fFound = true;

                // save
                OpenAndWriteHistoryFile();
                pList = null;
            }

            if (fFound != true)
            {
                pList = pList.Next;
            }
        }

        if (fInHistoryMode)
        {
            iCurrentHistoryPage--;

            // load in first page
            LoadNextHistoryPage();
        }

        SetHistoryFact(HISTORY.QUEST_FINISHED, ubCode, GetWorldTotalMin(), sSectorX, sSectorY);
        return;
    }


    int GetTimeQuestWasStarted(int ubCode)
    {
        // run through history list
        int iOldHistoryPage = iCurrentHistoryPage;
        history? pList = pHistoryListHead;
        bool fFound = false;
        int uiTime = 0;

        // set current page to before list	
        iCurrentHistoryPage = 0;

        SetLastPageInHistoryRecords();

        OpenAndReadHistoryFile();

        pList = pHistoryListHead;

        while (pList is not null)
        {
            if ((pList.ubSecondCode == ubCode) && (pList.ubCode == HISTORY.QUEST_STARTED))
            {
                uiTime = pList.uiDate;
                fFound = true;

                pList = null;
            }

            if (fFound != true)
            {
                pList = pList.Next;
            }
        }

        if (fInHistoryMode)
        {
            iCurrentHistoryPage--;

            // load in first page
            LoadNextHistoryPage();
        }

        return (uiTime);
    }

    void GetQuestStartedString(int ubQuestValue, string sQuestString)
    {
        // open the file and copy the string
        LoadEncryptedDataFromFile("BINARYDATA\\quests.edt", sQuestString, 160 * (ubQuestValue * 2), 160);
    }


    void GetQuestEndedString(int ubQuestValue, string sQuestString)
    {
        // open the file and copy the string
        LoadEncryptedDataFromFile("BINARYDATA\\quests.edt", sQuestString, 160 * ((ubQuestValue * 2) + 1), 160);
    }

    int GetNumberOfHistoryPages()
    {
        HWFILE hFileHandle;
        int uiFileSize = 0;
        int uiSizeOfRecordsOnEachPage = 0;
        int iNumberOfHistoryPages = 0;

        if (!(FileExists(Globals.HISTORY_DATA_FILE)))
            return (0);

        // open file
        hFileHandle = FileOpen(Globals.HISTORY_DATA_FILE, (FILE_OPEN_EXISTING | FILE_ACCESS_READ), false);

        // failed to get file, return
        if (!hFileHandle)
        {
            return (0);
        }

        // make sure file is more than 0 length
        if (FileGetSize(hFileHandle) == 0)
        {
            FileClose(hFileHandle);
            return (0);
        }

        uiFileSize = FileGetSize(hFileHandle) - 1;
        uiSizeOfRecordsOnEachPage = (Globals.NUM_RECORDS_PER_PAGE * (sizeof(int) + sizeof(int) + 3 * sizeof(int) + sizeof(int) + sizeof(int)));

        iNumberOfHistoryPages = (int)(uiFileSize / uiSizeOfRecordsOnEachPage);

        FileClose(hFileHandle);

        return (iNumberOfHistoryPages);
    }
}

// the financial structure
public class history
{
    public HISTORY ubCode; // the code index in the finance code table
    public int uiIdNumber; // unique id number
    public int ubSecondCode; // secondary code 
    public int uiDate; // time in the world in global time
    public int sSectorX; // sector X this took place in
    public int sSectorY; // sector Y this took place in
    public int bSectorZ;
    public int ubColor;
    public history? Next; // next unit in the list
};


public enum HISTORY
{
    ENTERED_HISTORY_MODE = 0,
    HIRED_MERC_FROM_AIM,
    HIRED_MERC_FROM_MERC,
    MERC_KILLED,
    SETTLED_ACCOUNTS_AT_MERC,
    ACCEPTED_ASSIGNMENT_FROM_ENRICO,
    CHARACTER_GENERATED,
    PURCHASED_INSURANCE,
    CANCELLED_INSURANCE,
    INSURANCE_CLAIM_PAYOUT,
    EXTENDED_CONTRACT_1_DAY,
    EXTENDED_CONTRACT_1_WEEK,
    EXTENDED_CONTRACT_2_WEEK,
    MERC_FIRED,
    MERC_QUIT,
    QUEST_STARTED,
    QUEST_FINISHED,
    TALKED_TO_MINER,
    LIBERATED_TOWN,
    CHEAT_ENABLED,
    TALKED_TO_FATHER_WALKER,
    MERC_MARRIED_OFF,
    MERC_CONTRACT_EXPIRED,
    RPC_JOINED_TEAM,
    ENRICO_COMPLAINED,
    WONBATTLE,
    MINE_RUNNING_OUT,
    MINE_RAN_OUT,
    MINE_SHUTDOWN,
    MINE_REOPENED,
    DISCOVERED_TIXA,
    DISCOVERED_ORTA,
    GOT_ROCKET_RIFLES,
    DEIDRANNA_DEAD_BODIES,
    BOXING_MATCHES,
    SOMETHING_IN_MINES,
    DEVIN,
    MIKE,
    TONY,
    KROTT,
    KYLE,
    MADLAB,
    GABBY,
    KEITH_OUT_OF_BUSINESS,
    HOWARD_CYANIDE,
    KEITH,
    HOWARD,
    PERKO,
    SAM,
    FRANZ,
    ARNOLD,
    FREDO,
    RICHGUY_BALIME,
    JAKE,
    BUM_KEYCARD,
    WALTER,
    DAVE,
    PABLO,
    KINGPIN_MONEY,
    WON_BOXING,
    LOST_BOXING,
    DISQUALIFIED_BOXING,
    FOUND_MONEY,
    ASSASSIN,
    LOSTTOWNSECTOR,
    DEFENDEDTOWNSECTOR,
    LOSTBATTLE,
    FATALAMBUSH,
    WIPEDOUTENEMYAMBUSH,
    UNSUCCESSFULATTACK,
    SUCCESSFULATTACK,
    CREATURESATTACKED,
    KILLEDBYBLOODCATS,
    SLAUGHTEREDBLOODCATS,
    NPC_KILLED,
    GAVE_CARMEN_HEAD,
    SLAY_MYSTERIOUSLY_LEFT,
    MERC_KILLED_CHARACTER,
};
