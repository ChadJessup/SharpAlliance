using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers.VideoSurfaces;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const string FINANCES_DATA_FILE = "TEMP\\finances.dat";
}

public class Finances
{
    // graphical positions
    private const int TOP_X = 0 + LAPTOP_SCREEN_UL_X;
    private const int TOP_Y = LAPTOP_SCREEN_UL_Y;
    private const int BLOCK_HEIGHT = 10;
    private const int TOP_DIVLINE_Y = 102;
    private const int DIVLINE_X = 130;
    private const int MID_DIVLINE_Y = 205;
    private const int BOT_DIVLINE_Y = 180;
    private const int MID_DIVLINE_Y2 = 263 + 20;
    private const int BOT_DIVLINE_Y2 = MID_DIVLINE_Y2 + MID_DIVLINE_Y - BOT_DIVLINE_Y;
    private const int TITLE_X = 140;
    private const int TITLE_Y = 33;
    private const int TEXT_X = 140;
    private const int PAGE_SIZE = 17;

    // yesterdyas/todays income and balance text positions
    private const int YESTERDAYS_INCOME = 114;
    private const int YESTERDAYS_OTHER = 138;
    private const int YESTERDAYS_DEBITS = 162;
    private const int YESTERDAYS_BALANCE = 188;
    private const int TODAYS_INCOME = 215;
    private const int TODAYS_OTHER = 239;
    private const int TODAYS_DEBITS = 263;
    private const int TODAYS_CURRENT_BALANCE = 263 + 28;
    private const int TODAYS_CURRENT_FORCAST_INCOME = 330;
    private const int TODAYS_CURRENT_FORCAST_BALANCE = 354;
    private const int SUMMARY_NUMBERS_X = 0;
    private const FontStyle FINANCE_HEADER_FONT = FontStyle.FONT14ARIAL;
    private const FontStyle FINANCE_TEXT_FONT = FontStyle.FONT12ARIAL;
    private const int NUM_RECORDS_PER_PAGE = PAGE_SIZE;

    // records text positions
    private const int RECORD_CREDIT_WIDTH = 106 - 47;
    private const int RECORD_DEBIT_WIDTH = RECORD_CREDIT_WIDTH;
    private const int RECORD_DATE_X = TOP_X + 10;
    private const int RECORD_TRANSACTION_X = RECORD_DATE_X + RECORD_DATE_WIDTH;
    private const int RECORD_TRANSACTION_WIDTH = 500 - 280;
    private const int RECORD_DEBIT_X = RECORD_TRANSACTION_X + RECORD_TRANSACTION_WIDTH;
    private const int RECORD_CREDIT_X = RECORD_DEBIT_X + RECORD_DEBIT_WIDTH;
    private const int RECORD_Y = 107 - 10;
    private const int RECORD_DATE_WIDTH = 47;
    private const int RECORD_BALANCE_X = RECORD_DATE_X + 385;
    private const int RECORD_BALANCE_WIDTH = 479 - 385;
    private const int RECORD_HEADER_Y = 90;

    private const int PAGE_NUMBER_X = TOP_X + 297; //345
    private const int PAGE_NUMBER_Y = TOP_Y + 33;

    // button positions

    private const int FIRST_PAGE_X = 505;
    private const int NEXT_BTN_X = 553;//577
    private const int PREV_BTN_X = 529;//553
    private const int LAST_PAGE_X = 577;
    private const int BTN_Y = 53;
    private readonly IFileManager files;
    private readonly ButtonSubSystem buttons;
    private readonly ILogger<Finances> logger;
    private readonly FontSubSystem fonts;
    private readonly IVideoManager video;

    // sizeof one record
    public int RECORD_SIZE { get; } = (sizeof(uint) + sizeof(int) + sizeof(int) + sizeof(byte) + sizeof(byte));

    private bool fPausedReDrawScreenFlag;

    // the financial record list
    FinanceUnitPtr pFinanceListHead = null;

    // current players balance
    //int iCurrentBalance=0;

    // current page displayed
    int iCurrentPage = 0;

    // current financial record (the one at the top of the current page)
    FinanceUnitPtr pCurrentFinance = null;

    // video object id's
    string guiTITLE;
    string guiGREYFRAME;
    string guiTOP;
    string guiMIDDLE;
    string guiBOTTOM;
    string guiLINE;
    string guiLONGLINE;
    string guiLISTCOLUMNS;

    // are in the financial system right now?
    bool fInFinancialMode = false;

    // the last page loaded
    uint guiLastPageLoaded = 0;

    // the last page altogether
    int guiLastPageInRecordsList = 0;

    // finance screen buttons
    Dictionary<FinanceButton, GUI_BUTTON> giFinanceButton = new();
    Dictionary<FinanceButton, ButtonPic> giFinanceButtonImage = new();

    public Finances(
        ILogger<Finances> logger,
        FontSubSystem fontSubSystem,
        IFileManager fileManager,
        IVideoManager videoManager,
        ButtonSubSystem buttonSubSystem)
    {
        this.files = fileManager;
        this.buttons = buttonSubSystem;
        this.logger = logger;
        this.fonts = fontSubSystem;
        this.video = videoManager;
    }

    public int AddTransactionToPlayersBook(FinanceEvent ubCode, NPCID ubSecondCode, uint uiDate, int iAmount)
    {
        // adds transaction to player's book(Financial List), returns unique id number of it
        // outside of the financial system(the code in this .c file), this is the only function you'll ever need

        int iCurPage = iCurrentPage;
        int uiId = 0;
        FinanceUnitPtr pFinance = pFinanceListHead;

        // read in balance from file

        GetBalanceFromDisk();
        // process the actual data


        //
        // If this transaction is for the hiring/extending of a mercs contract
        //
        if (ubCode == FinanceEvent.HIRED_MERC ||
                ubCode == FinanceEvent.IMP_PROFILE ||
                ubCode == FinanceEvent.PAYMENT_TO_NPC ||
                ubCode == FinanceEvent.EXTENDED_CONTRACT_BY_1_DAY ||
                ubCode == FinanceEvent.EXTENDED_CONTRACT_BY_1_WEEK ||
                ubCode == FinanceEvent.EXTENDED_CONTRACT_BY_2_WEEKS
            )
        {
            gMercProfiles[ubSecondCode].uiTotalCostToDate += -iAmount;
        }

        // clear list
        ClearFinanceList();

        pFinance = pFinanceListHead;

        // update balance
        LaptopSaveInfo.iCurrentBalance += iAmount;

        uiId = ProcessAndEnterAFinacialRecord(ubCode, uiDate, iAmount, ubSecondCode, LaptopSaveInfo.iCurrentBalance);

        // write balance to disk
        WriteBalanceToDisk();

        // append to end of file
        AppendFinanceToEndOfFile(pFinance);

        // set number of pages
        SetLastPageInRecords();

        if (!fInFinancialMode)
        {
            ClearFinanceList();
        }
        else
        {
            SetFinanceButtonStates();

            // force update
            fPausedReDrawScreenFlag = true;
        }

        fMapScreenBottomDirty = true;

        // return unique id of this transaction
        return uiId;
    }

    FinanceUnitPtr GetFinance(uint uiId)
    {
        FinanceUnitPtr pFinance = pFinanceListHead;

        // get a finance object and return a pointer to it, the obtaining of the 
        // finance object is via a unique ID the programmer must store
        // , it is returned on addition of a financial transaction

        // error check
        if (pFinance is null)
            return (null);

        // look for finance object with Id
        while (pFinance is not null)
        {
            if (pFinance.uiIdNumber == uiId)
                break;

            // next finance record
            pFinance = pFinance.Next;
        }

        return (pFinance);
    }

    uint GetTotalDebits()
    {
        // returns the total of the debits
        uint uiDebits = 0;
        FinanceUnitPtr pFinance = pFinanceListHead;

        // run to end of list
        while (pFinance is not null)
        {
            // if a debit, add to debit total
            if (pFinance.iAmount > 0)
                uiDebits += ((uint)(pFinance.iAmount));

            // next finance record
            pFinance = pFinance.Next;
        }

        return uiDebits;
    }

    uint GetTotalCredits()
    {
        // returns the total of the credits
        uint uiCredits = 0;
        FinanceUnitPtr pFinance = pFinanceListHead;

        // run to end of list
        while (pFinance is not null)
        {
            // if a credit, add to credit total
            if (pFinance.iAmount < 0)
                uiCredits += ((uint)(pFinance.iAmount));

            // next finance record
            pFinance = pFinance.Next;
        }

        return uiCredits;
    }

    uint GetDayCredits(uint usDayNumber)
    {
        // returns the total of the credits for day( note resolution of usDayNumber is days)
        uint uiCredits = 0;
        FinanceUnitPtr pFinance = pFinanceListHead;

        while (pFinance is not null)
        {
            // if a credit and it occurs on day passed
            if ((pFinance.iAmount < 0) && ((pFinance.uiDate / (60 * 24)) == usDayNumber))
                uiCredits += ((uint)(pFinance.iAmount));

            // next finance record
            pFinance = pFinance.Next;
        }

        return uiCredits;
    }

    uint GetDayDebits(uint usDayNumber)
    {
        // returns the total of the debits
        uint uiDebits = 0;
        FinanceUnitPtr pFinance = pFinanceListHead;

        while (pFinance is not null)
        {
            if ((pFinance.iAmount > 0) && ((pFinance.uiDate / (60 * 24)) == usDayNumber))
                uiDebits += ((uint)(pFinance.iAmount));

            // next finance record
            pFinance = pFinance.Next;
        }

        return uiDebits;
    }

    int GetTotalToDay(uint sTimeInMins)
    {
        // gets the total amount to this day
        int uiTotal = 0;
        FinanceUnitPtr pFinance = pFinanceListHead;

        while (pFinance is not null)
        {
            if (((int)(pFinance.uiDate / (60 * 24)) <= sTimeInMins / (24 * 60)))
                uiTotal += ((pFinance.iAmount));

            // next finance record
            pFinance = pFinance.Next;
        }

        return uiTotal;
    }
    uint GetYesterdaysIncome()
    {
        // get income for yesterday
        return (GetDayDebits(((GameClock.GetWorldTotalMin() - (24 * 60)) / (24 * 60))) + GetDayCredits(((uint)(GameClock.GetWorldTotalMin() - (24 * 60)) / (24 * 60))));
    }

    int GetCurrentBalance()
    {
        // get balance to this minute
        return (LaptopSaveInfo.iCurrentBalance);

        // return(GetTotalDebits((GetWorldTotalMin()))+GetTotalCredits((GetWorldTotalMin())));
    }

    int GetTodaysIncome()
    {
        // get income 
        return (GetCurrentBalance() - GetTotalToDay(GameClock.GetWorldTotalMin() - (24 * 60)));
    }


    int GetProjectedTotalDailyIncome()
    {
        // return total  projected income, including what is earned today already

        // CJC: I DON'T THINK SO!
        // The point is:  PredictIncomeFromPlayerMines isn't dependant on the time of day 
        // (anymore) and this would report income of 0 at midnight!
        /*
      if (GetWorldMinutesInDay() <= 0)
        {	
            return ( 0 );
        }
        */
        // look at we earned today

        // then there is how many deposits have been made, now look at how many mines we have, thier rate, amount of ore left and predict if we still
        // had these mines how much more would we get?

        return (StrategicMines.PredictIncomeFromPlayerMines());
    }

    int GetProjectedBalance()
    {
        // return the projected balance for tommorow - total for today plus the total income, projected.
        return (GetProjectedTotalDailyIncome() + GetCurrentBalance());
    }

    uint GetConfidenceValue()
    {
        // return confidence that the projected income is infact correct
        return (((GameClock.GetWorldMinutesInDay() * 100) / (60 * 24)));
    }

    void GameInitFinances()
    {
        // initialize finances on game start up
        // unlink Finances data file
        if ((this.files.FileExists(FINANCES_DATA_FILE)))
        {
            this.files.FileClearAttributes(FINANCES_DATA_FILE);
            this.files.FileDelete(FINANCES_DATA_FILE);
        }
        GetBalanceFromDisk();
    }

    void EnterFinances()
    {
        //entry into finanacial system, load graphics, set variables..draw screen once
        // set the fact we are in the financial display system

        fInFinancialMode = true;
        // build finances list
        //OpenAndReadFinancesFile( );

        // reset page we are on
        iCurrentPage = LaptopSaveInfo.iCurrentFinancesPage;


        // get the balance
        GetBalanceFromDisk();

        // clear the list
        ClearFinanceList();

        // force redraw of the entire screen
        fReDrawScreenFlag = true;

        // set number of pages
        SetLastPageInRecords();

        // load graphics into memory
        LoadFinances();

        // create buttons
        CreateFinanceButtons();

        // set button state
        SetFinanceButtonStates();

        // draw finance 
        RenderFinances();

        //  DrawSummary( );

        // draw page number
        DisplayFinancePageNumberAndDateRange();



        //InvalidateRegion(0,0,640,480);
        return;
    }

    void ExitFinances()
    {
        LaptopSaveInfo.iCurrentFinancesPage = iCurrentPage;


        // not in finance system anymore
        fInFinancialMode = false;

        // destroy buttons
        DestroyFinanceButtons();

        // clear out list
        ClearFinanceList();


        // remove graphics
        RemoveFinances();
        return;

    }

    void HandleFinances()
    {

    }

    void RenderFinances()
    {
        HVOBJECT hHandle;

        // draw background
        RenderBackGround();

        // if we are on the first page, draw the summary
        if (iCurrentPage == 0)
            DrawSummary();
        else
            DrawAPageOfRecords();



        //title
        DrawFinanceTitleText();

        // draw pages and dates
        DisplayFinancePageNumberAndDateRange();


        // display border
        hHandle = this.video.GetVideoObject(guiLaptopBACKGROUND);
        this.video.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 0, 108, 23, VO_BLT.SRCTRANSPARENCY, null);


        // title bar icon
        Laptop.BlitTitleBarIcons();



        return;
    }

    bool LoadFinances()
    {
        HVOBJECT VObjectDesc;
        // load Finance video objects into memory

        // title bar
        VObjectDesc = this.video.GetVideoObject("LAPTOP\\programtitlebar.sti", out guiTITLE);

        // top portion of the screen background
        VObjectDesc = this.video.GetVideoObject("LAPTOP\\Financeswindow.sti", out guiTOP);

        // black divider line - long ( 480 length)
        VObjectDesc = this.video.GetVideoObject("LAPTOP\\divisionline480.sti", out guiLONGLINE);

        // the records columns
        VObjectDesc = this.video.GetVideoObject("LAPTOP\\recordcolumns.sti", out guiLISTCOLUMNS);

        // black divider line - long ( 480 length)
        VObjectDesc = this.video.GetVideoObject("LAPTOP\\divisionline.sti", out guiLINE);

        return (true);
    }

    void RemoveFinances()
    {

        // delete Finance video objects from memory
        this.video.DeleteVideoObjectFromIndex(guiLONGLINE);
        this.video.DeleteVideoObjectFromIndex(guiLINE);
        this.video.DeleteVideoObjectFromIndex(guiLISTCOLUMNS);
        this.video.DeleteVideoObjectFromIndex(guiTOP);
        this.video.DeleteVideoObjectFromIndex(guiTITLE);


        return;
    }

    void RenderBackGround()
    {
        // render generic background for Finance system
        HVOBJECT hHandle;
        int iCounter = 0;

        // get title bar object
        hHandle = this.video.GetVideoObject(guiTITLE);
        this.video.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 0, TOP_X, TOP_Y - 2, VO_BLT.SRCTRANSPARENCY, null);

        // get and blt the top part of the screen, video object and blt to screen
        hHandle = this.video.GetVideoObject(guiTOP);
        this.video.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 0, TOP_X, TOP_Y + 22, VO_BLT.SRCTRANSPARENCY, null);
        DrawFinanceTitleText();
        return;
    }




    void DrawSummary()
    {
        // draw day's summary to screen
        DrawSummaryLines();
        DrawSummaryText();
        DrawFinanceTitleText();
        return;
    }

    void DrawSummaryLines()
    {
        // draw divider lines on screen
        HVOBJECT hHandle;

        // the summary LINE object handle
       hHandle = this.video.GetVideoObject(guiLINE);

        // blit summary LINE object to screen
        this.video.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 0, DIVLINE_X, TOP_DIVLINE_Y, VO_BLT.SRCTRANSPARENCY, null);
        this.video.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 0, DIVLINE_X, TOP_DIVLINE_Y + 2, VO_BLT.SRCTRANSPARENCY, null);
        //BltVideoObject(FRAME_BUFFER, hHandle, 0,DIVLINE_X, MID_DIVLINE_Y, VO_BLT.SRCTRANSPARENCY,null);
        this.video.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 0, DIVLINE_X, BOT_DIVLINE_Y, VO_BLT.SRCTRANSPARENCY, null);
        this.video.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 0, DIVLINE_X, MID_DIVLINE_Y2, VO_BLT.SRCTRANSPARENCY, null);
        //BltVideoObject(FRAME_BUFFER, hHandle, 0,DIVLINE_X, BOT_DIVLINE_Y2, VO_BLT.SRCTRANSPARENCY,null);


        return;
    }

    void DrawAPageOfRecords()
    {
        // this procedure will draw a series of financial records to the screen
        int iCurPage = 1;
        int iCount = 0;
        pCurrentFinance = pFinanceListHead;

        // (re-)render background
        DrawRecordsBackGround();

        // error check
        if (iCurrentPage == -1)
            return;


        // current page is found, render  from here
        DrawRecordsText();
        DisplayFinancePageNumberAndDateRange();
        return;
    }

    void DrawRecordsBackGround()
    {
        // proceudre will draw the background for the list of financial records
        int iCounter = 6;
        HVOBJECT hHandle;

        // render the generic background
        RenderBackGround();


        // now the columns
        for (; iCounter < 35; iCounter++)
        {
            // get and blt middle background to screen
            hHandle = this.video.GetVideoObject(guiLISTCOLUMNS);
            this.video.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 0, TOP_X + 10, TOP_Y + 18 + (iCounter * BLOCK_HEIGHT) + 1, VO_BLT.SRCTRANSPARENCY, null);
        }

        // the divisorLines
        hHandle = this.video.GetVideoObject(guiLONGLINE);
        this.video.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 0, TOP_X + 10, TOP_Y + 17 + (6 * (BLOCK_HEIGHT)), VO_BLT.SRCTRANSPARENCY, null);
        hHandle = this.video.GetVideoObject(guiLONGLINE);
        this.video.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 0, TOP_X + 10, TOP_Y + 19 + (6 * (BLOCK_HEIGHT)), VO_BLT.SRCTRANSPARENCY, null);
        hHandle = this.video.GetVideoObject(guiLONGLINE);
        this.video.BltVideoObject(SurfaceType.FRAME_BUFFER, hHandle, 0, TOP_X + 10, TOP_Y + 19 + ((iCounter) * (BLOCK_HEIGHT)), VO_BLT.SRCTRANSPARENCY, null);


        // the header text
        DrawRecordsColumnHeadersText();

        return;

    }

    void DrawRecordsColumnHeadersText()
    {
        // write the headers text for each column
        int usX, usY;

        // font stuff
        FontSubSystem.SetFont(FINANCE_TEXT_FONT);
        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);

        // the date header
        FontSubSystem.FindFontCenterCoordinates(RECORD_DATE_X, 0, RECORD_DATE_WIDTH, 0, pFinanceHeaders[0], FINANCE_TEXT_FONT, out usX, out usY);
        mprintf(usX, RECORD_HEADER_Y, pFinanceHeaders[0]);

        // debit header
        FontSubSystem.FindFontCenterCoordinates(RECORD_DEBIT_X, 0, RECORD_DEBIT_WIDTH, 0, pFinanceHeaders[1], FINANCE_TEXT_FONT, out usX, out usY);
        mprintf(usX, RECORD_HEADER_Y, pFinanceHeaders[1]);

        // credit header
        FontSubSystem.FindFontCenterCoordinates(RECORD_CREDIT_X, 0, RECORD_CREDIT_WIDTH, 0, pFinanceHeaders[2], FINANCE_TEXT_FONT, out usX, out usY);
        mprintf(usX, RECORD_HEADER_Y, pFinanceHeaders[2]);

        // balance header
        FontSubSystem.FindFontCenterCoordinates(RECORD_BALANCE_X, 0, RECORD_BALANCE_WIDTH, 0, pFinanceHeaders[4], FINANCE_TEXT_FONT, out usX, out usY);
        mprintf(usX, RECORD_HEADER_Y, pFinanceHeaders[4]);

        // transaction header
        FontSubSystem.FindFontCenterCoordinates(RECORD_TRANSACTION_X, 0, RECORD_TRANSACTION_WIDTH, 0, pFinanceHeaders[3], FINANCE_TEXT_FONT, out usX, out usY);
        mprintf(usX, RECORD_HEADER_Y, pFinanceHeaders[3]);

        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
        return;
    }

    void DrawRecordsText()
    {
        // draws the text of the records
        FinanceUnitPtr pCurFinance = pCurrentFinance;
        FinanceUnitPtr pTempFinance = pFinanceListHead;
        string sString;
        int iCounter = 0;
        int usX, usY;
        int iBalance = 0;

        // setup the font stuff
        FontSubSystem.SetFont(FINANCE_TEXT_FONT);
        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);


        // anything to print
        if (pCurrentFinance == null)
        {
            // nothing to print 
            return;
        }

        // get balance to this point
        while (pTempFinance != pCurFinance)
        {
            // increment balance by amount of transaction
            iBalance += pTempFinance.iAmount;

            // next element
            pTempFinance = pTempFinance.Next;
        }

        // loop through record list
        for (; iCounter < NUM_RECORDS_PER_PAGE; iCounter++)
        {
            // get and write the date
            sString = wprintf("%d", pCurFinance.uiDate / (24 * 60));



            FontSubSystem.FindFontCenterCoordinates(RECORD_DATE_X, 0, RECORD_DATE_WIDTH, 0, sString, FINANCE_TEXT_FONT, out usX, out usY);
            mprintf(usX, 12 + RECORD_Y + (iCounter * (FontSubSystem.GetFontHeight(FINANCE_TEXT_FONT) + 6)), sString);

            // get and write debit/ credit
            if (pCurFinance.iAmount >= 0)
            {
                // increase in asset - debit
                sString = wprintf("%d", pCurFinance.iAmount);
                // insert commas
                InsertCommasForDollarFigure(sString);
                // insert dollar sight for first record in the list
                //DEF: 3/19/99: removed cause we want to see the dollar sign on ALL entries
                //		 if( iCounter == 0 )
                {
                    InsertDollarSignInToString(sString);
                }

                FontSubSystem.FindFontCenterCoordinates(RECORD_DEBIT_X, 0, RECORD_DEBIT_WIDTH, 0, sString, FINANCE_TEXT_FONT, out usX, out usY);
                mprintf(usX, 12 + RECORD_Y + (iCounter * (FontSubSystem.GetFontHeight(FINANCE_TEXT_FONT) + 6)), sString);
            }
            else
            {
                // decrease in asset - credit
                wprintf(sString, "%d", pCurFinance.iAmount * (-1));
                FontSubSystem.SetFontForeground(FontColor.FONT_RED);
                InsertCommasForDollarFigure(sString);
                // insert dollar sight for first record in the list
                //DEF: 3/19/99: removed cause we want to see the dollar sign on ALL entries
                //		 if( iCounter == 0 )
                {
                    InsertDollarSignInToString(sString);
                }

                FontSubSystem.FindFontCenterCoordinates(RECORD_CREDIT_X, 0, RECORD_CREDIT_WIDTH, 0, sString, FINANCE_TEXT_FONT, out usX, out usY);
                mprintf(usX, 12 + RECORD_Y + (iCounter * (FontSubSystem.GetFontHeight(FINANCE_TEXT_FONT) + 6)), sString);
                FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
            }

            // the balance to this point
            iBalance = pCurFinance.iBalanceToDate;

            // set font based on balance
            if (iBalance >= 0)
            {
                FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
            }
            else
            {
                FontSubSystem.SetFontForeground(FontColor.FONT_RED);
                iBalance = (iBalance) * (-1);
            }

            // transaction string
            ProcessTransactionString(sString, pCurFinance);
            FontSubSystem.FindFontCenterCoordinates(RECORD_TRANSACTION_X, 0, RECORD_TRANSACTION_WIDTH, 0, sString, FINANCE_TEXT_FONT, out usX, out usY);
            mprintf(usX, 12 + RECORD_Y + (iCounter * (FontSubSystem.GetFontHeight(FINANCE_TEXT_FONT) + 6)), sString);


            // print the balance string
            wprintf(sString, "%d", iBalance);
            InsertCommasForDollarFigure(sString);
            // insert dollar sight for first record in the list
            //DEF: 3/19/99: removed cause we want to see the dollar sign on ALL entries
            //		if( iCounter == 0 )
            {
                InsertDollarSignInToString(sString);
            }

            FontSubSystem.FindFontCenterCoordinates(RECORD_BALANCE_X, 0, RECORD_BALANCE_WIDTH, 0, sString, FINANCE_TEXT_FONT, out usX, out usY);
            mprintf(usX, 12 + RECORD_Y + (iCounter * (FontSubSystem.GetFontHeight(FINANCE_TEXT_FONT) + 6)), sString);

            // restore font color
            FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);

            // next finance
            pCurFinance = pCurFinance.Next;

            // last page, no finances left, return
            if (pCurFinance is null)
            {

                // restore shadow
                FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
                return;
            }

        }

        // restore shadow
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
        return;
    }
    void DrawFinanceTitleText()
    {
        // setup the font stuff
        FontSubSystem.SetFont(FINANCE_HEADER_FONT);
        FontSubSystem.SetFontForeground(FontColor.FONT_WHITE);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
        // reset shadow
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        // draw the pages title
        mprintf(TITLE_X, TITLE_Y, pFinanceTitle[0]);


        return;
    }

    void InvalidateLapTopScreen()
    {
        // invalidates blit region to force refresh of screen

        this.video.InvalidateRegion(LAPTOP_SCREEN_UL_X, LAPTOP_SCREEN_UL_Y, LAPTOP_SCREEN_LR_X, LAPTOP_SCREEN_LR_Y);

        return;
    }

    void DrawSummaryText()
    {
        int usX, usY;
        string pString;
        int iBalance = 0;


        // setup the font stuff
        FontSubSystem.SetFont(FINANCE_TEXT_FONT);
        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);

        // draw summary text to the screen
        mprintf(TEXT_X, YESTERDAYS_INCOME, pFinanceSummary[2]);
        mprintf(TEXT_X, YESTERDAYS_OTHER, pFinanceSummary[3]);
        mprintf(TEXT_X, YESTERDAYS_DEBITS, pFinanceSummary[4]);
        mprintf(TEXT_X, YESTERDAYS_BALANCE, pFinanceSummary[5]);
        mprintf(TEXT_X, TODAYS_INCOME, pFinanceSummary[6]);
        mprintf(TEXT_X, TODAYS_OTHER, pFinanceSummary[7]);
        mprintf(TEXT_X, TODAYS_DEBITS, pFinanceSummary[8]);
        mprintf(TEXT_X, TODAYS_CURRENT_BALANCE, pFinanceSummary[9]);
        mprintf(TEXT_X, TODAYS_CURRENT_FORCAST_INCOME, pFinanceSummary[10]);
        mprintf(TEXT_X, TODAYS_CURRENT_FORCAST_BALANCE, pFinanceSummary[11]);

        // draw the actual numbers



        // yesterdays income
        iBalance = GetPreviousDaysIncome();
        pString = wprintf("%d", iBalance);

        InsertCommasForDollarFigure(pString);

        if (iBalance != 0)
            InsertDollarSignInToString(pString);

        FontSubSystem.FindFontRightCoordinates(0, 0, 580, 0, pString, FINANCE_TEXT_FONT, out usX, out usY);

        mprintf(usX, YESTERDAYS_INCOME, pString);

        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);

        // yesterdays other
        iBalance = GetYesterdaysOtherDeposits();
        wprintf(pString, "%d", iBalance);

        InsertCommasForDollarFigure(pString);
        if (iBalance != 0)
            InsertDollarSignInToString(pString);
        FontSubSystem.FindFontRightCoordinates(0, 0, 580, 0, pString, FINANCE_TEXT_FONT, out usX, out usY);

        mprintf(usX, YESTERDAYS_OTHER, pString);

        FontSubSystem.SetFontForeground(FontColor.FONT_RED);

        // yesterdays debits
        iBalance = GetYesterdaysDebits();
        if (iBalance < 0)
        {
            FontSubSystem.SetFontForeground(FontColor.FONT_RED);
            iBalance *= -1;
        }

        wprintf(pString, "%d", iBalance);

        InsertCommasForDollarFigure(pString);
        if (iBalance != 0)
            InsertDollarSignInToString(pString);
        FontSubSystem.FindFontRightCoordinates(0, 0, 580, 0, pString, FINANCE_TEXT_FONT, out usX, out usY);

        mprintf(usX, YESTERDAYS_DEBITS, pString);

        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);

        // yesterdays balance..ending balance..so todays balance then
        iBalance = GetTodaysBalance();

        if (iBalance < 0)
        {
            FontSubSystem.SetFontForeground(FontColor.FONT_RED);
            iBalance *= -1;
        }

        wprintf(pString, "%d", iBalance);
        InsertCommasForDollarFigure(pString);
        if (iBalance != 0)
            InsertDollarSignInToString(pString);
        FontSubSystem.FindFontRightCoordinates(0, 0, 580, 0, pString, FINANCE_TEXT_FONT, out usX, out usY);

        mprintf(usX, YESTERDAYS_BALANCE, pString);

        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);

        // todays income
        iBalance = GetTodaysDaysIncome();
        wprintf(pString, "%d", iBalance);

        InsertCommasForDollarFigure(pString);
        if (iBalance != 0)
            InsertDollarSignInToString(pString);
        FontSubSystem.FindFontRightCoordinates(0, 0, 580, 0, pString, FINANCE_TEXT_FONT, out usX, out usY);

        mprintf(usX, TODAYS_INCOME, pString);

        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);

        // todays other
        iBalance = GetTodaysOtherDeposits();
        wprintf(pString, "%d", iBalance);

        InsertCommasForDollarFigure(pString);
        if (iBalance != 0)
            InsertDollarSignInToString(pString);
        FontSubSystem.FindFontRightCoordinates(0, 0, 580, 0, pString, FINANCE_TEXT_FONT, out usX, out usY);

        mprintf(usX, TODAYS_OTHER, pString);

        FontSubSystem.SetFontForeground(FontColor.FONT_RED);

        // todays debits
        iBalance = GetTodaysDebits();

        // absolute value
        if (iBalance < 0)
        {
            iBalance *= (-1);
        }

        wprintf(pString, "%d", iBalance);

        InsertCommasForDollarFigure(pString);
        if (iBalance != 0)
            InsertDollarSignInToString(pString);
        FontSubSystem.FindFontRightCoordinates(0, 0, 580, 0, pString, FINANCE_TEXT_FONT, out usX, out usY);

        mprintf(usX, TODAYS_DEBITS, pString);

        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);

        // todays current balance
        iBalance = GetCurrentBalance();
        if (iBalance < 0)
        {
            iBalance *= -1;
            FontSubSystem.SetFontForeground(FontColor.FONT_RED);
            wprintf(pString, "%d", iBalance);
            iBalance *= -1;
        }
        else
        {
            wprintf(pString, "%d", iBalance);
        }

        InsertCommasForDollarFigure(pString);
        if (iBalance != 0)
            InsertDollarSignInToString(pString);
        FontSubSystem.FindFontRightCoordinates(0, 0, 580, 0, pString, FINANCE_TEXT_FONT, out usX, out usY);
        mprintf(usX, TODAYS_CURRENT_BALANCE, pString);
        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);


        // todays forcast income
        iBalance = GetProjectedTotalDailyIncome();
        wprintf(pString, "%d", iBalance);

        InsertCommasForDollarFigure(pString);
        if (iBalance != 0)
            InsertDollarSignInToString(pString);

        FontSubSystem.FindFontRightCoordinates(0, 0, 580, 0, pString, FINANCE_TEXT_FONT, out usX, out usY);

        mprintf(usX, TODAYS_CURRENT_FORCAST_INCOME, pString);

        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);


        // todays forcast balance
        iBalance = GetCurrentBalance() + GetProjectedTotalDailyIncome();
        if (iBalance < 0)
        {
            iBalance *= -1;
            FontSubSystem.SetFontForeground(FontColor.FONT_RED);
            wprintf(pString, "%d", iBalance);
            iBalance *= -1;
        }
        else
        {
            wprintf(pString, "%d", iBalance);
        }

        InsertCommasForDollarFigure(pString);
        if (iBalance != 0)
            InsertDollarSignInToString(pString);
        FontSubSystem.FindFontRightCoordinates(0, 0, 580, 0, pString, FINANCE_TEXT_FONT, out usX, out usY);
        mprintf(usX, TODAYS_CURRENT_FORCAST_BALANCE, pString);
        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);



        // reset the shadow
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        return;
    }


    void OpenAndReadFinancesFile()
    {
        // this procedure will open and read in data to the finance list
        Stream hFileHandle;
        FinanceEvent ubCode = FinanceEvent.UNKNOWN;
        NPCID ubSecondCode = NPCID.Unknown;

        uint uiDate = 0;
        int iAmount = 0;
        int iBalanceToDate = 0;
        int iBytesRead = 0;
        uint uiByteCount = 0;

        // clear out the old list
        ClearFinanceList();

        // no file, return
        if (!(this.files.FileExists(FINANCES_DATA_FILE)))
            return;

        // open file
        hFileHandle = this.files.FileOpen(FINANCES_DATA_FILE, (FILE_OPEN_EXISTING | FILE_ACCESS_READ), false);

        // make sure file is more than 0 length
        if (this.files.FileGetSize(hFileHandle) == 0)
        {
            this.files.FileClose(hFileHandle);
            return;
        }

        // read in balance
        // write balance to disk first
        this.files.FileRead(hFileHandle, ref LaptopSaveInfo.iCurrentBalance, sizeof(int), out iBytesRead);
        uiByteCount += sizeof(int);

        //AssertMsg(iBytesRead, "Failed To Read Data Entry");

        // file exists, read in data, continue until file end
        while (this.files.FileGetSize(hFileHandle) > uiByteCount)
        {

            // read in other data
            this.files.FileRead(hFileHandle, ref ubCode, sizeof(byte), out iBytesRead);
            this.files.FileRead(hFileHandle, ref ubSecondCode, sizeof(byte), out iBytesRead);
            this.files.FileRead(hFileHandle, ref uiDate, sizeof(uint), out iBytesRead);
            this.files.FileRead(hFileHandle, ref iAmount, sizeof(int), out iBytesRead);
            this.files.FileRead(hFileHandle, ref iBalanceToDate, sizeof(int), out iBytesRead);

           // AssertMsg(iBytesRead, "Failed To Read Data Entry");

            // add transaction
            ProcessAndEnterAFinacialRecord(ubCode, uiDate, iAmount, ubSecondCode, iBalanceToDate);

            // increment byte counter
            uiByteCount += sizeof(int) + sizeof(uint) + sizeof(byte) + sizeof(byte) + sizeof(int);
        }

        // close file 
        this.files.FileClose(hFileHandle);

        return;
    }


    void ClearFinanceList()
    {
        // remove each element from list of transactions
        FinanceUnitPtr pFinanceList = pFinanceListHead;
        FinanceUnitPtr pFinanceNode = pFinanceList;

        // while there are elements in the list left, delete them
        while (pFinanceList is not null)
        {
            // set node to list head
            pFinanceNode = pFinanceList;

            // set list head to next node
            pFinanceList = pFinanceList.Next;

            // delete current node
            MemFree(pFinanceNode);
        }
        pCurrentFinance = null;
        pFinanceListHead = null;
        return;
    }


    int ProcessAndEnterAFinacialRecord(FinanceEvent ubCode, uint uiDate, int iAmount, NPCID ubSecondCode, int iBalanceToDate)
    {
        int uiId = 0;
        FinanceUnitPtr pFinance = pFinanceListHead;

        // add to finance list
        if (pFinance is not null)
        {
            // go to end of list
            while (pFinance.Next is not null)
                pFinance = pFinance.Next;

            // alloc space
            pFinance.Next = new();

            // increment id number
            uiId = pFinance.uiIdNumber + 1;

            // set up information passed
            pFinance = pFinance.Next;
            pFinance.Next = null;
            pFinance.ubCode = ubCode;
            pFinance.ubSecondCode = ubSecondCode;
            pFinance.uiDate = uiDate;
            pFinance.iAmount = iAmount;
            pFinance.uiIdNumber = uiId;
            pFinance.iBalanceToDate = iBalanceToDate;


        }
        else
        {
            // alloc space
            uiId = ReadInLastElementOfFinanceListAndReturnIdNumber();
            pFinance = new();

            // setup info passed
            pFinance.Next = null;
            pFinance.ubCode = ubCode;
            pFinance.ubSecondCode = ubSecondCode;
            pFinance.uiDate = uiDate;
            pFinance.iAmount = iAmount;
            pFinance.uiIdNumber = uiId;
            pFinance.iBalanceToDate = iBalanceToDate;
            pFinanceListHead = pFinance;
        }

        pCurrentFinance = pFinanceListHead;

        return uiId;
    }

    void CreateFinanceButtons()
    {
        giFinanceButtonImage[PREV_PAGE_BUTTON] = this.buttons.LoadButtonImage("LAPTOP\\arrows.sti", -1, 0, -1, 1, -1);
        giFinanceButton[PREV_PAGE_BUTTON] = ButtonSubSystem.QuickCreateButton(giFinanceButtonImage[PREV_PAGE_BUTTON], new(PREV_BTN_X, BTN_Y),
                                            ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1,
                                            MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)BtnFinanceDisplayPrevPageCallBack);


        giFinanceButtonImage[FinanceButton.NEXT_PAGE_BUTTON] = ButtonSubSystem.UseLoadedButtonImage(giFinanceButtonImage[PREV_PAGE_BUTTON], -1, 6, -1, 7, -1);
        giFinanceButton[FinanceButton.NEXT_PAGE_BUTTON] = ButtonSubSystem.QuickCreateButton(giFinanceButtonImage[FinanceButton.NEXT_PAGE_BUTTON], new(NEXT_BTN_X, BTN_Y),
                                            ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1,
                                            MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)BtnFinanceDisplayNextPageCallBack);


        //button to go to the first page
        giFinanceButtonImage[FinanceButton.FIRST_PAGE_BUTTON] = ButtonSubSystem.UseLoadedButtonImage(giFinanceButtonImage[PREV_PAGE_BUTTON], -1, 3, -1, 4, -1);
        giFinanceButton[FinanceButton.FIRST_PAGE_BUTTON] = ButtonSubSystem.QuickCreateButton(giFinanceButtonImage[FinanceButton.FIRST_PAGE_BUTTON], new(FIRST_PAGE_X, BTN_Y),
                                            ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1,
                                            MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)BtnFinanceFirstLastPageCallBack);


        ButtonSubSystem.MSYS_SetBtnUserData(giFinanceButton[FinanceButton.FIRST_PAGE_BUTTON], 0, 0);

        //button to go to the last page
        giFinanceButtonImage[FinanceButton.LAST_PAGE_BUTTON] = ButtonSubSystem.UseLoadedButtonImage(giFinanceButtonImage[PREV_PAGE_BUTTON], -1, 9, -1, 10, -1);
        giFinanceButton[FinanceButton.LAST_PAGE_BUTTON] = ButtonSubSystem.QuickCreateButton(giFinanceButtonImage[FinanceButton.LAST_PAGE_BUTTON], new(LAST_PAGE_X, BTN_Y),
                                            ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1,
                                            MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)BtnFinanceFirstLastPageCallBack);
        ButtonSubSystem.MSYS_SetBtnUserData(giFinanceButton[FinanceButton.LAST_PAGE_BUTTON], 0, 1);

        ButtonSubSystem.SetButtonCursor(giFinanceButton[(FinanceButton)0], CURSOR.LAPTOP_SCREEN);
        ButtonSubSystem.SetButtonCursor(giFinanceButton[(FinanceButton)1], CURSOR.LAPTOP_SCREEN);
        ButtonSubSystem.SetButtonCursor(giFinanceButton[(FinanceButton)2], CURSOR.LAPTOP_SCREEN);
        ButtonSubSystem.SetButtonCursor(giFinanceButton[(FinanceButton)3], CURSOR.LAPTOP_SCREEN);
        return;
    }


    void DestroyFinanceButtons()
    {
        uint uiCnt;

        for (uiCnt = 0; uiCnt < 4; uiCnt++)
        {
            ButtonSubSystem.RemoveButton(giFinanceButton[(FinanceButton)uiCnt]);
            ButtonSubSystem.UnloadButtonImage(giFinanceButtonImage[(FinanceButton)uiCnt]);
        }
    }
    void BtnFinanceDisplayPrevPageCallBack(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {

            btn.uiFlags &= ~(BUTTON_CLICKED_ON);

            // if greater than page zero, we can move back, decrement iCurrentPage counter
            LoadPreviousPage();
            pCurrentFinance = pFinanceListHead;

            // set button state
            SetFinanceButtonStates();
            fReDrawScreenFlag = true;
        }

    }

    void BtnFinanceDisplayNextPageCallBack(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            btn.uiFlags &= ~(BUTTON_CLICKED_ON);
            // increment currentPage
            //IncrementCurrentPageFinancialDisplay( );
            LoadNextPage();

            // set button state
            SetFinanceButtonStates();

            pCurrentFinance = pFinanceListHead;
            // redraw screen
            fReDrawScreenFlag = true;
        }
    }

    void BtnFinanceFirstLastPageCallBack(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            uint uiButton = (uint)ButtonSubSystem.MSYS_GetBtnUserData(btn, 0);

            btn.uiFlags &= ~(BUTTON_CLICKED_ON);

            //if its the first page button
            if (uiButton == 0)
            {
                iCurrentPage = 0;
                LoadInRecords(iCurrentPage);
            }

            //else its the last page button
            else
            {
                LoadInRecords(guiLastPageInRecordsList + 1);

                iCurrentPage = guiLastPageInRecordsList + 1;
            }

            // set button state
            SetFinanceButtonStates();

            pCurrentFinance = pFinanceListHead;
            // redraw screen
            fReDrawScreenFlag = true;
        }
    }


    void IncrementCurrentPageFinancialDisplay()
    {
        // run through list, from pCurrentFinance, to NUM_RECORDS_PER_PAGE +1 FinancialUnits
        FinanceUnitPtr pTempFinance = pCurrentFinance;
        bool fOkToIncrementPage = false;
        int iCounter = 0;

        // on the overview page, simply set iCurrent to head of list, and page to 1
        if (iCurrentPage == 0)
        {

            pCurrentFinance = pFinanceListHead;
            iCurrentPage = 1;

            return;
        }

        // no list, we are on page 2
        if (pTempFinance == null)
        {
            iCurrentPage = 2;
            return;
        }

        // haven't reached end of list and not yet at beginning of next page
        while ((pTempFinance is not null) && (!fOkToIncrementPage))
        {
            // found the next page,  first record thereof
            if (iCounter == NUM_RECORDS_PER_PAGE + 1)
            {
                fOkToIncrementPage = true;
                pCurrentFinance = pTempFinance.Next;
            }

            //next record
            pTempFinance = pTempFinance.Next;
            iCounter++;
        }

        // if ok to increment, increment
        if (fOkToIncrementPage)
        {
            iCurrentPage++;

        }

        return;
    }

    void ProcessTransactionString(string pString, FinanceUnitPtr pFinance)
    {

        switch (pFinance.ubCode)
        {
            case FinanceEvent.ACCRUED_INTEREST:
                wprintf(pString, "%s", pTransactionText[FinanceEvent.ACCRUED_INTEREST]);
                break;

            case FinanceEvent.ANONYMOUS_DEPOSIT:
                wprintf(pString, "%s", pTransactionText[FinanceEvent.ANONYMOUS_DEPOSIT]);
                break;

            case FinanceEvent.TRANSACTION_FEE:
                wprintf(pString, "%s", pTransactionText[FinanceEvent.TRANSACTION_FEE]);
                break;

            case FinanceEvent.HIRED_MERC:
                wprintf(pString, pMessageStrings[MSG.HIRED_MERC], gMercProfiles[pFinance.ubSecondCode].zNickname);
                break;

            case FinanceEvent.BOBBYR_PURCHASE:
                wprintf(pString, "%s", pTransactionText[FinanceEvent.BOBBYR_PURCHASE]);
                break;

            case FinanceEvent.PAY_SPECK_FOR_MERC:
                wprintf(pString, "%s", pTransactionText[FinanceEvent.PAY_SPECK_FOR_MERC], gMercProfiles[pFinance.ubSecondCode].zName);
                break;

            case FinanceEvent.MEDICAL_DEPOSIT:
                wprintf(pString, pTransactionText[FinanceEvent.MEDICAL_DEPOSIT], gMercProfiles[pFinance.ubSecondCode].zNickname);
                break;

            case FinanceEvent.IMP_PROFILE:
                wprintf(pString, "%s", pTransactionText[FinanceEvent.IMP_PROFILE]);
                break;

            case FinanceEvent.PURCHASED_INSURANCE:
                wprintf(pString, pTransactionText[FinanceEvent.PURCHASED_INSURANCE], gMercProfiles[pFinance.ubSecondCode].zNickname);
                break;

            case FinanceEvent.REDUCED_INSURANCE:
                wprintf(pString, pTransactionText[FinanceEvent.REDUCED_INSURANCE], gMercProfiles[pFinance.ubSecondCode].zNickname);
                break;

            case FinanceEvent.EXTENDED_INSURANCE:
                wprintf(pString, pTransactionText[FinanceEvent.EXTENDED_INSURANCE], gMercProfiles[pFinance.ubSecondCode].zNickname);
                break;

            case FinanceEvent.CANCELLED_INSURANCE:
                wprintf(pString, pTransactionText[FinanceEvent.CANCELLED_INSURANCE], gMercProfiles[pFinance.ubSecondCode].zNickname);
                break;

            case FinanceEvent.INSURANCE_PAYOUT:
                wprintf(pString, pTransactionText[FinanceEvent.INSURANCE_PAYOUT], gMercProfiles[pFinance.ubSecondCode].zNickname);
                break;

            case FinanceEvent.EXTENDED_CONTRACT_BY_1_DAY:
                wprintf(pString, pTransactionAlternateText[1], gMercProfiles[pFinance.ubSecondCode].zNickname);
                break;

            case FinanceEvent.EXTENDED_CONTRACT_BY_1_WEEK:
                wprintf(pString, pTransactionAlternateText[2], gMercProfiles[pFinance.ubSecondCode].zNickname);
                break;

            case FinanceEvent.EXTENDED_CONTRACT_BY_2_WEEKS:
                wprintf(pString, pTransactionAlternateText[3], gMercProfiles[pFinance.ubSecondCode].zNickname);
                break;

            case FinanceEvent.DEPOSIT_FROM_GOLD_MINE:
            case FinanceEvent.DEPOSIT_FROM_SILVER_MINE:
                wprintf(pString, pTransactionText[(FinanceEvent)16]);
                break;

            case FinanceEvent.PURCHASED_FLOWERS:
                wprintf(pString, "%s", pTransactionText[FinanceEvent.PURCHASED_FLOWERS]);
                break;

            case FinanceEvent.FULL_MEDICAL_REFUND:
                wprintf(pString, pTransactionText[FinanceEvent.FULL_MEDICAL_REFUND], gMercProfiles[pFinance.ubSecondCode].zNickname);
                break;

            case FinanceEvent.PARTIAL_MEDICAL_REFUND:
                wprintf(pString, pTransactionText[FinanceEvent.PARTIAL_MEDICAL_REFUND], gMercProfiles[pFinance.ubSecondCode].zNickname);
                break;

            case FinanceEvent.NO_MEDICAL_REFUND:
                wprintf(pString, pTransactionText[FinanceEvent.NO_MEDICAL_REFUND], gMercProfiles[pFinance.ubSecondCode].zNickname);
                break;

            case FinanceEvent.TRANSFER_FUNDS_TO_MERC:
                wprintf(pString, pTransactionText[FinanceEvent.TRANSFER_FUNDS_TO_MERC], gMercProfiles[pFinance.ubSecondCode].zNickname);
                break;
            case FinanceEvent.TRANSFER_FUNDS_FROM_MERC:
                wprintf(pString, pTransactionText[FinanceEvent.TRANSFER_FUNDS_FROM_MERC], gMercProfiles[pFinance.ubSecondCode].zNickname);
                break;
            case FinanceEvent.PAYMENT_TO_NPC:
                wprintf(pString, pTransactionText[FinanceEvent.PAYMENT_TO_NPC], gMercProfiles[pFinance.ubSecondCode].zNickname);
                break;
            case (FinanceEvent.TRAIN_TOWN_MILITIA):
                {
                    string str;
                    int ubSectorX;
                    MAP_ROW ubSectorY;
                    ubSectorX = SECTORINFO.SECTORX((SEC)pFinance.ubSecondCode);
                    ubSectorY = SECTORINFO.SECTORY((SEC)pFinance.ubSecondCode);
                    StrategicMap.GetSectorIDString(ubSectorX, ubSectorY, 0, out str, true);
                    wprintf(pString, pTransactionText[FinanceEvent.TRAIN_TOWN_MILITIA], str);
                }
                break;

            case (FinanceEvent.PURCHASED_ITEM_FROM_DEALER):
                wprintf(pString, pTransactionText[FinanceEvent.PURCHASED_ITEM_FROM_DEALER], gMercProfiles[pFinance.ubSecondCode].zNickname);
                break;

            case (FinanceEvent.MERC_DEPOSITED_MONEY_TO_PLAYER_ACCOUNT):
                wprintf(pString, pTransactionText[FinanceEvent.MERC_DEPOSITED_MONEY_TO_PLAYER_ACCOUNT], gMercProfiles[pFinance.ubSecondCode].zNickname);
                break;
        }
    }


    void DisplayFinancePageNumberAndDateRange()
    {
        // this function will go through the list of 'histories' starting at current until end or 
        // MAX_PER_PAGE...it will get the date range and the page number
        int iLastPage = 0;
        int iCounter = 0;
        uint uiLastDate;
        FinanceUnitPtr pTempFinance = pFinanceListHead;
        string sString;


        // setup the font stuff
        FontSubSystem.SetFont(FINANCE_TEXT_FONT);
        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);

        if (pCurrentFinance is null)
        {
            pCurrentFinance = pFinanceListHead;
            if (pCurrentFinance is null)
            {
                sString = wprintf("%s %d / %d", pFinanceHeaders[5], iCurrentPage + 1, guiLastPageInRecordsList + 2);
                mprintf(PAGE_NUMBER_X, PAGE_NUMBER_Y, sString);
                return;
            }
        }

        uiLastDate = pCurrentFinance.uiDate;
        // find last page
        while (pTempFinance is not null)
        {
            iCounter++;
            pTempFinance = pTempFinance.Next;
        }

        // get the last page

        sString = wprintf("%s %d / %d", pFinanceHeaders[5], iCurrentPage + 1, guiLastPageInRecordsList + 2);
        mprintf(PAGE_NUMBER_X, PAGE_NUMBER_Y, sString);

        // reset shadow
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
    }


    bool WriteBalanceToDisk()
    {
        // will write the current balance to disk
        Stream hFileHandle;
        int iBytesWritten = 0;
        FinanceUnitPtr pFinanceList = pFinanceListHead;


        // open file
        hFileHandle = this.files.FileOpen(FINANCES_DATA_FILE, FILE_ACCESS_WRITE | FILE_CREATE_ALWAYS, false);

        // write balance to disk
        this.files.FileWrite(hFileHandle, (LaptopSaveInfo.iCurrentBalance), sizeof(int), out var _);

        // close file
        this.files.FileClose(hFileHandle);


        return (true);
    }

    void GetBalanceFromDisk()
    {
        // will grab the current blanace from disk
        // assuming file already openned
        // this procedure will open and read in data to the finance list
        Stream hFileHandle;
        int iBytesRead = 0;

        // open file
        hFileHandle = this.files.FileOpen(FINANCES_DATA_FILE, (FILE_OPEN_EXISTING | FILE_ACCESS_READ), false);

        // start at beginning 
        this.files.FileSeek(hFileHandle, 0, FILE_SEEK_FROM_START);

        // get balance from disk first
        this.files.FileRead(hFileHandle, ref LaptopSaveInfo.iCurrentBalance, sizeof(int), out iBytesRead);

        Debug.Assert(iBytesRead != 0);//, "Failed To Read Data Entry");

        // close file
        this.files.FileClose(hFileHandle);

        return;
    }


    bool AppendFinanceToEndOfFile(FinanceUnitPtr pFinance)
    {
        // will write the current finance to disk
        Stream hFileHandle;
        int iBytesWritten = 0;
        FinanceUnitPtr pFinanceList = pFinanceListHead;


        // open file
        hFileHandle = this.files.FileOpen(FINANCES_DATA_FILE, FILE_ACCESS_WRITE | FILE_OPEN_ALWAYS, false);

        // go to the end
        if (this.files.FileSeek(hFileHandle, 0, FILE_SEEK_FROM_END) == false)
        {
            // error
            this.files.FileClose(hFileHandle);
            return (false);
        }


        // write finance to disk
        // now write date and amount, and code
        this.files.FileWrite(hFileHandle, (pFinanceList.ubCode), sizeof(byte), out var _);
        this.files.FileWrite(hFileHandle, (pFinanceList.ubSecondCode), sizeof(byte), out var _);
        this.files.FileWrite(hFileHandle, (pFinanceList.uiDate), sizeof(uint), out var _);
        this.files.FileWrite(hFileHandle, (pFinanceList.iAmount), sizeof(int), out var _);
        this.files.FileWrite(hFileHandle, (pFinanceList.iBalanceToDate), sizeof(int), out var _);

        // close file
        this.files.FileClose(hFileHandle);

        return (true);
    }

    private int ReadInLastElementOfFinanceListAndReturnIdNumber()
    {
        // this function will read in the last unit in the finance list, to grab it's id number

        Stream hFileHandle;
        int iBytesRead = 0;
        int iFileSize = 0;

        // no file, return
        if (!(this.files.FileExists(FINANCES_DATA_FILE)))
            return 0;

        // open file
        hFileHandle = this.files.FileOpen(FINANCES_DATA_FILE, (FILE_OPEN_EXISTING | FILE_ACCESS_READ), false);

        // make sure file is more than balance size + length of 1 record - 1 byte
        if (this.files.FileGetSize(hFileHandle) < sizeof(int) + sizeof(uint) + sizeof(byte) + sizeof(byte) + sizeof(int))
        {
            this.files.FileClose(hFileHandle);
            return 0;
        }

        // size is?
        iFileSize = this.files.FileGetSize(hFileHandle);

        // done with file, close it
        this.files.FileClose(hFileHandle);

        // file size -1 / sizeof record in bytes is id
        return ((iFileSize - 1) / (sizeof(int) + sizeof(uint) + sizeof(byte) + sizeof(byte) + sizeof(int)));

    }

    void SetLastPageInRecords()
    {
        // grabs the size of the file and interprets number of pages it will take up
        Stream hFileHandle;
        int iBytesRead = 0;

        // no file, return
        if (!(this.files.FileExists(FINANCES_DATA_FILE)))
            return;

        // open file
        hFileHandle = this.files.FileOpen(FINANCES_DATA_FILE, (FILE_OPEN_EXISTING | FILE_ACCESS_READ), false);

        // make sure file is more than 0 length
        if (this.files.FileGetSize(hFileHandle) == 0)
        {
            this.files.FileClose(hFileHandle);
            guiLastPageInRecordsList = 1;
            return;
        }


        // done with file, close it
        this.files.FileClose(hFileHandle);

        guiLastPageInRecordsList = (ReadInLastElementOfFinanceListAndReturnIdNumber() - 1) / NUM_RECORDS_PER_PAGE;

        return;
    }


    bool LoadPreviousPage()
    {

        // clear out old list of records, and load in previous page worth of records
        ClearFinanceList();

        // load previous page
        if ((iCurrentPage == 1) || (iCurrentPage == 0))
        {
            iCurrentPage = 0;
            return (false);
        }

        // now load in previous page's records, if we can
        if (LoadInRecords(iCurrentPage - 1))
        {
            iCurrentPage--;
            return (true);
        }
        else
        {
            LoadInRecords(iCurrentPage);
            return (false);
        }
    }

    bool LoadNextPage()
    {

        // clear out old list of records, and load in previous page worth of records
        ClearFinanceList();



        // now load in previous page's records, if we can
        if (LoadInRecords(iCurrentPage + 1))
        {
            iCurrentPage++;
            return (true);
        }
        else
        {
            LoadInRecords(iCurrentPage);
            return (false);
        }

    }

    bool LoadInRecords(int uiPage)
    {
        // loads in records belogning, to page uiPage
        // no file, return
        bool fOkToContinue = true;
        int iCount = 0;
        Stream hFileHandle;
        FinanceEvent ubCode = FinanceEvent.UNKNOWN;
        NPCID ubSecondCode = 0;
        int iBalanceToDate = 0;
        uint uiDate = 0;
        int iAmount = 0;
        int iBytesRead = 0;
        int uiByteCount = 0;

        // check if bad page
        if (uiPage == 0)
        {
            return (false);
        }


        if (!(this.files.FileExists(FINANCES_DATA_FILE)))
            return (false);

        // open file
        hFileHandle = this.files.FileOpen(FINANCES_DATA_FILE, (FILE_OPEN_EXISTING | FILE_ACCESS_READ), false);

        // make sure file is more than 0 length
        if (this.files.FileGetSize(hFileHandle) == 0)
        {
            this.files.FileClose(hFileHandle);
            return (false);
        }

        // is the file long enough?
        if ((this.files.FileGetSize(hFileHandle) - sizeof(int) - 1) / (NUM_RECORDS_PER_PAGE * (sizeof(int) + sizeof(uint) + sizeof(byte) + sizeof(byte) + sizeof(int))) + 1 < uiPage)
        {
            // nope
            this.files.FileClose(hFileHandle);
            return (false);
        }

        this.files.FileSeek(hFileHandle, sizeof(int) + (uiPage - 1) * NUM_RECORDS_PER_PAGE * (sizeof(int) + sizeof(uint) + sizeof(byte) + sizeof(byte) + sizeof(int)), FILE_SEEK_FROM_START);

        uiByteCount = sizeof(int) + (uiPage - 1) * NUM_RECORDS_PER_PAGE * (sizeof(int) + sizeof(uint) + sizeof(byte) + sizeof(byte) + sizeof(int));
        // file exists, read in data, continue until end of page
        while ((iCount < NUM_RECORDS_PER_PAGE) && (fOkToContinue) && (uiByteCount < this.files.FileGetSize(hFileHandle)))
        {

            // read in data		
            this.files.FileRead(hFileHandle, ref ubCode, sizeof(byte), out iBytesRead);
            this.files.FileRead(hFileHandle, ref ubSecondCode, sizeof(byte), out iBytesRead);
            this.files.FileRead(hFileHandle, ref uiDate, sizeof(uint), out iBytesRead);
            this.files.FileRead(hFileHandle, ref iAmount, sizeof(int), out iBytesRead);
            this.files.FileRead(hFileHandle, ref iBalanceToDate, sizeof(int), out iBytesRead);

            //AssertMsg(iBytesRead, "Failed To Read Data Entry");

            // add transaction
            ProcessAndEnterAFinacialRecord(ubCode, uiDate, iAmount, ubSecondCode, iBalanceToDate);

            // increment byte counter
            uiByteCount += sizeof(int) + sizeof(uint) + sizeof(byte) + sizeof(byte) + sizeof(int);

            // we've overextended our welcome, and bypassed end of file, get out
            if (uiByteCount >= this.files.FileGetSize(hFileHandle))
            {
                // not ok to continue
                fOkToContinue = false;
            }

            iCount++;
        }

        // close file 
        this.files.FileClose(hFileHandle);

        // check to see if we in fact have a list to display
        if (pFinanceListHead == null)
        {
            // got no records, return false
            return (false);
        }

        // set up current finance
        pCurrentFinance = pFinanceListHead;

        return (true);
    }


    string InsertCommasForDollarFigure(string figure)
    {
        char[] pString = figure.ToCharArray();
        short sCounter = 0;
        short sZeroCount = 0;
        short sTempCounter = 0;
        short sEndPosition = 0;

        // go to end of dollar figure
        while (pString[sCounter] != 0)
        {
            sCounter++;
        }

        // negative?
        if (pString[0] == '-')
        {
            // stop one slot in advance of normal
            sEndPosition = 1;
        }

        // is there under $1,000?
        if (sCounter < 4)
        {
            // can't do anything, return
            return figure;
        }

        // at end, start backing up until beginning
        while (sCounter > sEndPosition)
        {


            // enough for a comma?
            if (sZeroCount == 3)
            {
                // reset count
                sZeroCount = 0;
                // set tempcounter to current counter
                sTempCounter = sCounter;

                // run until end 
                while (pString[sTempCounter] != 0)
                {
                    sTempCounter++;
                }
                // now shift everything over ot the right one place until sTempCounter = sCounter
                while (sTempCounter >= sCounter)
                {
                    pString[sTempCounter + 1] = pString[sTempCounter];
                    sTempCounter--;
                }
                // now insert comma
                pString[sCounter] = ',';
            }

            // increment count of digits
            sZeroCount++;

            // decrement counter
            sCounter--;
        }

        return new(pString);

    }


    string InsertDollarSignInToString(string figure)
    {
        // run to end of string, copy everything in string 2 places right, insert a space at pString[ 1 ] and a '$' at pString[ 0 ]

        char[] pString = figure.ToCharArray();
        int iCounter = 0;

        // run to end of string
        while (pString[iCounter] != 0)
        {
            iCounter++;
        }

        // now copy over
        while (iCounter >= 0)
        {
            pString[iCounter + 1] = pString[iCounter];
            iCounter--;
        }

        pString[0] = '$';

        return new(pString);
    }

    int GetPreviousBalanceToDate()
    {

        // will grab balance to date of previous record
        // grabs the size of the file and interprets number of pages it will take up
        Stream hFileHandle;
        int iBytesRead = 0;
        int iBalanceToDate = 0;

        // no file, return
        if (!(this.files.FileExists(FINANCES_DATA_FILE)))
            return 0;

        // open file
        hFileHandle = this.files.FileOpen(FINANCES_DATA_FILE, (FILE_OPEN_EXISTING | FILE_ACCESS_READ), false);

        if (this.files.FileGetSize(hFileHandle) < sizeof(int) + sizeof(uint) + sizeof(byte) + sizeof(byte) + sizeof(int))
        {
            this.files.FileClose(hFileHandle);
            return 0;
        }

        this.files.FileSeek(hFileHandle, (sizeof(int)), FILE_SEEK_FROM_END);

        // get balnce to date
        this.files.FileRead(hFileHandle, ref iBalanceToDate, sizeof(int), out iBytesRead);

        this.files.FileClose(hFileHandle);

        return iBalanceToDate;
    }


    int GetPreviousDaysBalance()
    {
        // find out what today is, then go back 2 days, get balance for that day
        int iPreviousDaysBalance = 0;
        Stream hFileHandle;
        int iBytesRead = 0;
        uint iDateInMinutes = 0;
        bool fOkToContinue = false;
        int iByteCount = 0;
        int iCounter = 1;
        byte ubCode = 0;
        byte ubSecondCode = 0;
        uint uiDate = 0;
        int iAmount = 0;
        int iBalanceToDate = 0;
        bool fGoneTooFar = false;
        int iFileSize = 0;

        // what day is it?
        iDateInMinutes = GameClock.GetWorldTotalMin() - (60 * 24);

        // error checking
        // no file, return
        if (!(this.files.FileExists(FINANCES_DATA_FILE)))
            return 0;

        // open file
        hFileHandle = this.files.FileOpen(FINANCES_DATA_FILE, (FILE_OPEN_EXISTING | FILE_ACCESS_READ), false);

        // start at the end, move back until Date / 24 * 60 on the record is =  ( iDateInMinutes /  ( 24 * 60 ) ) - 2
        iByteCount += sizeof(int);
        // loop, make sure we don't pass beginning of file, if so, we have an error, and check for condifition above
        while ((iByteCount < this.files.FileGetSize(hFileHandle)) && (!fOkToContinue) && (!fGoneTooFar))
        {
            this.files.FileSeek(hFileHandle, RECORD_SIZE * iCounter, FILE_SEEK_FROM_END);

            // incrment byte count
            iByteCount += RECORD_SIZE;

            this.files.FileRead(hFileHandle, ref ubCode, sizeof(byte), out iBytesRead);
            this.files.FileRead(hFileHandle, ref ubSecondCode, sizeof(byte), out iBytesRead);
            this.files.FileRead(hFileHandle, ref uiDate, sizeof(uint), out iBytesRead);
            this.files.FileRead(hFileHandle, ref iAmount, sizeof(int), out iBytesRead);
            this.files.FileRead(hFileHandle, ref iBalanceToDate, sizeof(int), out iBytesRead);

            // check to see if we are far enough
            if ((uiDate / (24 * 60)) == (iDateInMinutes / (24 * 60)) - 2)
            {
                fOkToContinue = true;
            }

            if (iDateInMinutes / (24 * 60) >= 2)
            {
                // there are no entries for the previous day
                if ((uiDate / (24 * 60)) < (iDateInMinutes / (24 * 60)) - 2)
                {
                    fGoneTooFar = true;

                }
            }
            else
            {
                fGoneTooFar = true;
            }
            iCounter++;
        }

        if (fOkToContinue == false)
        {
            // reached beginning of file, nothing found, return 0
            // close file 
            this.files.FileClose(hFileHandle);
            return 0;
        }

        this.files.FileClose(hFileHandle);

        // reached 3 days ago, or beginning of file
        return iBalanceToDate;

    }



    int GetTodaysBalance()
    {
        // find out what today is, then go back 2 days, get balance for that day
        int iPreviousDaysBalance = 0;
        Stream hFileHandle;
        int iBytesRead = 0;
        uint iDateInMinutes = 0;
        bool fOkToContinue = false;
        int iByteCount = 0;
        int iCounter = 1;
        byte ubCode = 0;
        byte ubSecondCode = 0;
        uint uiDate = 0;
        int iAmount = 0;
        int iBalanceToDate = 0;
        bool fGoneTooFar = false;



        // what day is it?
        iDateInMinutes = GameClock.GetWorldTotalMin();

        // error checking
        // no file, return
        if (!(this.files.FileExists(FINANCES_DATA_FILE)))
            return 0;

        // open file
        hFileHandle = this.files.FileOpen(FINANCES_DATA_FILE, (FILE_OPEN_EXISTING | FILE_ACCESS_READ), false);

        // start at the end, move back until Date / 24 * 60 on the record is =  ( iDateInMinutes /  ( 24 * 60 ) ) - 2
        iByteCount += sizeof(int);

        // loop, make sure we don't pass beginning of file, if so, we have an error, and check for condifition above
        while ((iByteCount < this.files.FileGetSize(hFileHandle)) && (!fOkToContinue) && (!fGoneTooFar))
        {
            this.files.FileSeek(hFileHandle, RECORD_SIZE * iCounter, FILE_SEEK_FROM_END);

            // incrment byte count
            iByteCount += RECORD_SIZE;

            this.files.FileRead(hFileHandle, ref ubCode, sizeof(byte), out iBytesRead);
            this.files.FileRead(hFileHandle, ref ubSecondCode, sizeof(byte), out iBytesRead);
            this.files.FileRead(hFileHandle, ref uiDate, sizeof(uint), out iBytesRead);
            this.files.FileRead(hFileHandle, ref iAmount, sizeof(int), out iBytesRead);
            this.files.FileRead(hFileHandle, ref iBalanceToDate, sizeof(int), out iBytesRead);

            //AssertMsg(iBytesRead, "Failed To Read Data Entry");
            // check to see if we are far enough
            if ((uiDate / (24 * 60)) == (iDateInMinutes / (24 * 60)) - 1)
            {
                fOkToContinue = true;
            }

            iCounter++;
        }


        this.files.FileClose(hFileHandle);

        // not found ?
        if (fOkToContinue == false)
        {
            iBalanceToDate = 0;
        }

        // reached 3 days ago, or beginning of file
        return iBalanceToDate;
    }



    int GetPreviousDaysIncome()
    {
        // will return the income from the previous day
        // which is todays starting balance - yesterdays starting balance
        int iPreviousDaysBalance = 0;
        Stream hFileHandle;
        int iBytesRead = 0;
        uint iDateInMinutes = 0;
        bool fOkToContinue = false;
        bool fOkToIncrement = false;
        int iByteCount = 0;
        int iCounter = 1;
        FinanceEvent ubCode = 0;
        byte ubSecondCode = 0;
        uint uiDate = 0;
        int iAmount = 0;
        int iBalanceToDate = 0;
        bool fGoneTooFar = false;
        int iTotalPreviousIncome = 0;

        // what day is it?
        iDateInMinutes = GameClock.GetWorldTotalMin();

        // error checking
        // no file, return
        if (!(this.files.FileExists(FINANCES_DATA_FILE)))
            return 0;

        // open file
        hFileHandle = this.files.FileOpen(FINANCES_DATA_FILE, (FILE_OPEN_EXISTING | FILE_ACCESS_READ), false);

        // start at the end, move back until Date / 24 * 60 on the record is =  ( iDateInMinutes /  ( 24 * 60 ) ) - 2
        iByteCount += sizeof(int);

        // loop, make sure we don't pass beginning of file, if so, we have an error, and check for condifition above
        while ((iByteCount < this.files.FileGetSize(hFileHandle)) && (!fOkToContinue) && (!fGoneTooFar))
        {
            this.files.FileGetPos(hFileHandle);

            this.files.FileSeek(hFileHandle, RECORD_SIZE * iCounter, FILE_SEEK_FROM_END);

            // incrment byte count
            iByteCount += RECORD_SIZE;

            this.files.FileGetPos(hFileHandle);

            this.files.FileRead(hFileHandle, ref ubCode, sizeof(byte), out iBytesRead);
            this.files.FileRead(hFileHandle, ref ubSecondCode, sizeof(byte), out iBytesRead);
            this.files.FileRead(hFileHandle, ref uiDate, sizeof(uint), out iBytesRead);
            this.files.FileRead(hFileHandle, ref iAmount, sizeof(int), out iBytesRead);
            this.files.FileRead(hFileHandle, ref iBalanceToDate, sizeof(int), out iBytesRead);

            //AssertMsg(iBytesRead, "Failed To Read Data Entry");
            // check to see if we are far enough
            if ((uiDate / (24 * 60)) == (iDateInMinutes / (24 * 60)) - 2)
            {
                fOkToContinue = true;
            }

            // there are no entries for the previous day
            if ((uiDate / (24 * 60)) < (iDateInMinutes / (24 * 60)) - 2)
            {
                fGoneTooFar = true;

            }

            if ((uiDate / (24 * 60)) == (iDateInMinutes / (24 * 60)) - 1)
            {
                // now ok to increment amount
                fOkToIncrement = true;
            }

            if ((fOkToIncrement) && ((ubCode == FinanceEvent.DEPOSIT_FROM_GOLD_MINE) || (ubCode == FinanceEvent.DEPOSIT_FROM_SILVER_MINE)))
            {
                // increment total
                iTotalPreviousIncome += iAmount;
            }

            iCounter++;
        }


        // now run back one more day and add up the total of deposits 

        // close file 
        this.files.FileClose(hFileHandle);

        return (iTotalPreviousIncome);

    }


    int GetTodaysDaysIncome()
    {
        // will return the income from the previous day
        // which is todays starting balance - yesterdays starting balance
        int iPreviousDaysBalance = 0;
        Stream hFileHandle;
        int iBytesRead = 0;
        uint iDateInMinutes = 0;
        bool fOkToContinue = false;
        bool fOkToIncrement = false;
        int iByteCount = 0;
        int iCounter = 1;
        FinanceEvent ubCode = 0;
        byte ubSecondCode = 0;
        uint uiDate = 0;
        int iAmount = 0;
        int iBalanceToDate = 0;
        bool fGoneTooFar = false;
        int iTotalIncome = 0;

        // what day is it?
        iDateInMinutes = GameClock.GetWorldTotalMin();

        // error checking
        // no file, return
        if (!(this.files.FileExists(FINANCES_DATA_FILE)))
            return 0;

        // open file
        hFileHandle = this.files.FileOpen(FINANCES_DATA_FILE, (FILE_OPEN_EXISTING | FILE_ACCESS_READ), false);

        // start at the end, move back until Date / 24 * 60 on the record is =  ( iDateInMinutes /  ( 24 * 60 ) ) - 2
        iByteCount += sizeof(int);

        // loop, make sure we don't pass beginning of file, if so, we have an error, and check for condifition above
        while ((iByteCount < this.files.FileGetSize(hFileHandle)) && (!fOkToContinue) && (!fGoneTooFar))
        {
            this.files.FileSeek(hFileHandle, RECORD_SIZE * iCounter, FILE_SEEK_FROM_END);

            // incrment byte count
            iByteCount += RECORD_SIZE;

            this.files.FileRead(hFileHandle, ref ubCode, sizeof(byte), out iBytesRead);
            this.files.FileRead(hFileHandle, ref ubSecondCode, sizeof(byte), out iBytesRead);
            this.files.FileRead(hFileHandle, ref uiDate, sizeof(uint), out iBytesRead);
            this.files.FileRead(hFileHandle, ref iAmount, sizeof(int), out iBytesRead);
            this.files.FileRead(hFileHandle, ref iBalanceToDate, sizeof(int), out iBytesRead);

            //AssertMsg(iBytesRead, "Failed To Read Data Entry");
            // check to see if we are far enough
            if ((uiDate / (24 * 60)) == (iDateInMinutes / (24 * 60)) - 1)
            {
                fOkToContinue = true;
            }

            if ((uiDate / (24 * 60)) > (iDateInMinutes / (24 * 60)) - 1)
            {
                // now ok to increment amount
                fOkToIncrement = true;
            }

            if ((fOkToIncrement) && ((ubCode == FinanceEvent.DEPOSIT_FROM_GOLD_MINE) || (ubCode == FinanceEvent.DEPOSIT_FROM_SILVER_MINE)))
            {
                // increment total
                iTotalIncome += iAmount;
                fOkToIncrement = false;
            }

            iCounter++;
        }

        // no entries, return nothing - no income for the day
        if (fGoneTooFar == true)
        {
            this.files.FileClose(hFileHandle);
            return 0;
        }

        // now run back one more day and add up the total of deposits 

        // close file 
        this.files.FileClose(hFileHandle);

        return (iTotalIncome);

    }

    void SetFinanceButtonStates()
    {
        // this function will look at what page we are viewing, enable and disable buttons as needed

        if (iCurrentPage == 0)
        {
            // first page, disable left buttons
            ButtonSubSystem.DisableButton(giFinanceButton[PREV_PAGE_BUTTON]);
            ButtonSubSystem.DisableButton(giFinanceButton[FinanceButton.FIRST_PAGE_BUTTON]);
        }
        else
        {
            // enable buttons
            ButtonSubSystem.EnableButton(giFinanceButton[PREV_PAGE_BUTTON]);
            ButtonSubSystem.EnableButton(giFinanceButton[FinanceButton.FIRST_PAGE_BUTTON]);
        }

        if (LoadNextPage())
        {
            // decrement page
            LoadPreviousPage();


            // enable buttons
            ButtonSubSystem.EnableButton(giFinanceButton[FinanceButton.NEXT_PAGE_BUTTON]);
            ButtonSubSystem.EnableButton(giFinanceButton[FinanceButton.LAST_PAGE_BUTTON]);

        }
        else
        {
            ButtonSubSystem.DisableButton(giFinanceButton[FinanceButton.NEXT_PAGE_BUTTON]);
            ButtonSubSystem.DisableButton(giFinanceButton[FinanceButton.LAST_PAGE_BUTTON]);
        }
    }


    int GetTodaysOtherDeposits()
    {
        // grab todays other deposits

        int iPreviousDaysBalance = 0;
        Stream hFileHandle;
        int iBytesRead = 0;
        uint iDateInMinutes = 0;
        bool fOkToContinue = false;
        bool fOkToIncrement = false;
        int iByteCount = 0;
        int iCounter = 1;
        FinanceEvent ubCode = 0;
        byte ubSecondCode = 0;
        uint uiDate = 0;
        int iAmount = 0;
        int iBalanceToDate = 0;
        bool fGoneTooFar = false;
        int iTotalIncome = 0;

        // what day is it?
        iDateInMinutes = GameClock.GetWorldTotalMin();

        // error checking
        // no file, return
        if (!(this.files.FileExists(FINANCES_DATA_FILE)))
            return 0;

        // open file
        hFileHandle = this.files.FileOpen(FINANCES_DATA_FILE, (FILE_OPEN_EXISTING | FILE_ACCESS_READ), false);

        // start at the end, move back until Date / 24 * 60 on the record is =  ( iDateInMinutes /  ( 24 * 60 ) ) - 2
        iByteCount += sizeof(int);

        // loop, make sure we don't pass beginning of file, if so, we have an error, and check for condifition above
        while ((iByteCount < this.files.FileGetSize(hFileHandle)) && (!fOkToContinue) && (!fGoneTooFar))
        {
            this.files.FileSeek(hFileHandle, RECORD_SIZE * iCounter, FILE_SEEK_FROM_END);

            // incrment byte count
            iByteCount += RECORD_SIZE;

            this.files.FileRead(hFileHandle, ref ubCode, sizeof(byte), out iBytesRead);
            this.files.FileRead(hFileHandle, ref ubSecondCode, sizeof(byte), out iBytesRead);
            this.files.FileRead(hFileHandle, ref uiDate, sizeof(uint), out iBytesRead);
            this.files.FileRead(hFileHandle, ref iAmount, sizeof(int), out iBytesRead);
            this.files.FileRead(hFileHandle, ref iBalanceToDate, sizeof(int), out iBytesRead);

            //AssertMsg(iBytesRead, "Failed To Read Data Entry");
            // check to see if we are far enough
            if ((uiDate / (24 * 60)) == (iDateInMinutes / (24 * 60)) - 1)
            {
                fOkToContinue = true;
            }

            if ((uiDate / (24 * 60)) > (iDateInMinutes / (24 * 60)) - 1)
            {
                // now ok to increment amount
                fOkToIncrement = true;
            }

            if ((fOkToIncrement) && ((ubCode != FinanceEvent.DEPOSIT_FROM_GOLD_MINE) && (ubCode != FinanceEvent.DEPOSIT_FROM_SILVER_MINE)))
            {
                if (iAmount > 0)
                {
                    // increment total
                    iTotalIncome += iAmount;
                    fOkToIncrement = false;
                }
            }

            iCounter++;
        }

        // no entries, return nothing - no income for the day
        if (fGoneTooFar == true)
        {
            this.files.FileClose(hFileHandle);
            return 0;
        }

        // now run back one more day and add up the total of deposits 

        // close file 
        this.files.FileClose(hFileHandle);

        return (iTotalIncome);
    }


    int GetYesterdaysOtherDeposits()
    {

        int iPreviousDaysBalance = 0;
        Stream hFileHandle;
        int iBytesRead = 0;
        uint iDateInMinutes = 0;
        bool fOkToContinue = false;
        bool fOkToIncrement = false;
        int iByteCount = 0;
        int iCounter = 1;
        FinanceEvent ubCode = 0;
        byte ubSecondCode = 0;
        uint uiDate = 0;
        int iAmount = 0;
        int iBalanceToDate = 0;
        bool fGoneTooFar = false;
        int iTotalPreviousIncome = 0;

        // what day is it?
        iDateInMinutes = GameClock.GetWorldTotalMin();

        // error checking
        // no file, return
        if (!(this.files.FileExists(FINANCES_DATA_FILE)))
            return 0;

        // open file
        hFileHandle = this.files.FileOpen(FINANCES_DATA_FILE, (FILE_OPEN_EXISTING | FILE_ACCESS_READ), false);

        // start at the end, move back until Date / 24 * 60 on the record is =  ( iDateInMinutes /  ( 24 * 60 ) ) - 2
        iByteCount += sizeof(int);

        // loop, make sure we don't pass beginning of file, if so, we have an error, and check for condifition above
        while ((iByteCount < this.files.FileGetSize(hFileHandle)) && (!fOkToContinue) && (!fGoneTooFar))
        {
            this.files.FileSeek(hFileHandle, RECORD_SIZE * iCounter, FILE_SEEK_FROM_END);

            // incrment byte count
            iByteCount += RECORD_SIZE;

            this.files.FileRead(hFileHandle, ref ubCode, sizeof(byte), out iBytesRead);
            this.files.FileRead(hFileHandle, ref ubSecondCode, sizeof(byte), out iBytesRead);
            this.files.FileRead(hFileHandle, ref uiDate, sizeof(uint), out iBytesRead);
            this.files.FileRead(hFileHandle, ref iAmount, sizeof(int), out iBytesRead);
            this.files.FileRead(hFileHandle, ref iBalanceToDate, sizeof(int), out iBytesRead);

            //AssertMsg(iBytesRead, "Failed To Read Data Entry");
            // check to see if we are far enough
            if ((uiDate / (24 * 60)) == (iDateInMinutes / (24 * 60)) - 2)
            {
                fOkToContinue = true;
            }

            // there are no entries for the previous day
            if ((uiDate / (24 * 60)) < (iDateInMinutes / (24 * 60)) - 2)
            {
                fGoneTooFar = true;

            }

            if ((uiDate / (24 * 60)) == (iDateInMinutes / (24 * 60)) - 1)
            {
                // now ok to increment amount
                fOkToIncrement = true;
            }

            if ((fOkToIncrement) && ((ubCode != FinanceEvent.DEPOSIT_FROM_GOLD_MINE) && (ubCode != FinanceEvent.DEPOSIT_FROM_SILVER_MINE)))
            {
                if (iAmount > 0)
                {
                    // increment total
                    iTotalPreviousIncome += iAmount;
                }
            }

            iCounter++;
        }

        // close file 
        this.files.FileClose(hFileHandle);

        return (iTotalPreviousIncome);
    }


    int GetTodaysDebits()
    {
        // return the expenses for today

        // currentbalance - todays balance - Todays income - other deposits

        return (GetCurrentBalance() - GetTodaysBalance() - GetTodaysDaysIncome() - GetTodaysOtherDeposits());
    }

    int GetYesterdaysDebits()
    {
        // return the expenses for yesterday

        return (GetTodaysBalance() - GetPreviousDaysBalance() - GetPreviousDaysIncome() - GetYesterdaysOtherDeposits());
    }


    void LoadCurrentBalance()
    {
        // will load the current balance from finances.dat file
        Stream hFileHandle;
        int iBytesRead = 0;

        // is the first record in the file
        // error checking
        // no file, return
        if (!(this.files.FileExists(FINANCES_DATA_FILE)))
        {
            LaptopSaveInfo.iCurrentBalance = 0;
            return;
        }

        // open file
        hFileHandle = this.files.FileOpen(FINANCES_DATA_FILE, (FILE_OPEN_EXISTING | FILE_ACCESS_READ), false);

        this.files.FileSeek(hFileHandle, 0, FILE_SEEK_FROM_START);
        this.files.FileRead(hFileHandle, ref LaptopSaveInfo.iCurrentBalance, sizeof(int), out iBytesRead);

        //AssertMsg(iBytesRead, "Failed To Read Data Entry");
        // close file 
        this.files.FileClose(hFileHandle);


        return;
    }
}
// BUTTON defines
public enum FinanceButton
{
    PREV_PAGE_BUTTON = 0,
    NEXT_PAGE_BUTTON,
    FIRST_PAGE_BUTTON,
    LAST_PAGE_BUTTON,
};

// the financial structure
public class FinanceUnitPtr
{
    public FinanceEvent ubCode; // the code index in the finance code table
    public int uiIdNumber; // unique id number
    public NPCID ubSecondCode; // secondary code 
    public uint uiDate; // time in the world in global time
    public int iAmount; // the amount of the transaction
    public int iBalanceToDate;
    public FinanceUnitPtr? Next; // next unit in the list
};

public enum FinanceEvent
{
    UNKNOWN = 0,
    ACCRUED_INTEREST,
    ANONYMOUS_DEPOSIT,
    TRANSACTION_FEE,
    HIRED_MERC,
    BOBBYR_PURCHASE,
    PAY_SPECK_FOR_MERC,
    MEDICAL_DEPOSIT,
    IMP_PROFILE,
    PURCHASED_INSURANCE,
    REDUCED_INSURANCE,
    EXTENDED_INSURANCE,
    CANCELLED_INSURANCE,
    INSURANCE_PAYOUT,
    EXTENDED_CONTRACT_BY_1_DAY,
    EXTENDED_CONTRACT_BY_1_WEEK,
    EXTENDED_CONTRACT_BY_2_WEEKS,
    DEPOSIT_FROM_GOLD_MINE,
    DEPOSIT_FROM_SILVER_MINE,
    PURCHASED_FLOWERS,
    FULL_MEDICAL_REFUND,
    PARTIAL_MEDICAL_REFUND,
    NO_MEDICAL_REFUND,
    PAYMENT_TO_NPC,
    TRANSFER_FUNDS_TO_MERC,
    TRANSFER_FUNDS_FROM_MERC,
    TRAIN_TOWN_MILITIA,
    PURCHASED_ITEM_FROM_DEALER,
    MERC_DEPOSITED_MONEY_TO_PLAYER_ACCOUNT,
};
