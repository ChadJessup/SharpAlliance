using System;

using static SharpAlliance.Core.Globals;
using static SharpAlliance.Core.EnglishText;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using System.Diagnostics;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Platform.Interfaces;
using System.Collections.Generic;

namespace SharpAlliance.Core.SubSystems;

public class Emails
{
    bool fSortDateUpwards = false;
    bool fSortSenderUpwards = false;
    bool fSortSubjectUpwards = false;

    int iViewerPositionY = 0;
    int iDeleteId = 0;
    bool fUnReadMailFlag = false;
    bool fOldUnreadFlag = true;
    public static bool fNewMailFlag = false;
    bool fDisplayMessageFlag = false;
    bool fOldDisplayMessageFlag = false;
    bool fReDraw = false;
    private bool fPausedReDrawScreenFlag;
    bool fDeleteMailFlag = false;
    bool fReDrawMessageFlag = false;
    bool fOnLastPageFlag = false;
    bool fJustStartedEmail = false;
    bool fDeleteInternal = false;
    bool fOpenMostRecentUnReadFlag = false;

    // mouse regions
    MOUSE_REGION[] pEmailRegions = new MOUSE_REGION[MAX_MESSAGES_PAGE];
    MOUSE_REGION pScreenMask;
    MOUSE_REGION pDeleteScreenMask;

    // the email info struct to speed up email
    EmailPageInfoStruct[] pEmailPageInfo = new EmailPageInfoStruct[MAX_NUMBER_EMAIL_PAGES];

    int iTotalHeight = 0;

    public Emails(IVideoManager videoManager, IFileManager fileManager, FontSubSystem fontSubSystem, ButtonSubSystem buttonSubSystem)
    {
        this.fonts = fontSubSystem;
        this.buttons = buttonSubSystem;
        this.files = fileManager;
        this.video = videoManager;
    }

    void InitializeMouseRegions()
    {
        int iCounter = 0;

        // init mouseregions
        for (iCounter = 0; iCounter < MAX_MESSAGES_PAGE; iCounter++)
        {
            var region = this.pEmailRegions[iCounter];
            MouseSubSystem.MSYS_DefineRegion(
                region,
                new(MIDDLE_X,
                    (int)(MIDDLE_Y + iCounter * MIDDLE_WIDTH),
                    MIDDLE_X + LINE_WIDTH,
                    (int)(MIDDLE_Y + iCounter * MIDDLE_WIDTH + MIDDLE_WIDTH)),
                MSYS_PRIORITY.NORMAL + 2,
                CURSOR.MSYS_NO_CURSOR,
                this.EmailMvtCallBack,
                this.EmailBtnCallBack);

            MouseSubSystem.MSYS_AddRegion(ref this.pEmailRegions[iCounter]);
            MouseSubSystem.MSYS_SetRegionUserData(this.pEmailRegions[iCounter], 0, iCounter);
        }

        //SetUpSortRegions();

        this.CreateDestroyNextPreviousRegions();
    }

    void DeleteEmailMouseRegions()
    {

        // this function will remove the mouse regions added
        int iCounter = 0;


        for (iCounter = 0; iCounter < MAX_MESSAGES_PAGE; iCounter++)
        {
            MouseSubSystem.MSYS_RemoveRegion(this.pEmailRegions[iCounter]);
        }
        //DeleteSortRegions();
        this.CreateDestroyNextPreviousRegions();

    }
    void GameInitEmail()
    {
        pEmailList = null;
        pPageList = null;

        iLastPage = -1;

        iCurrentPage = 0;
        this.iDeleteId = 0;

        // reset display message flag
        this.fDisplayMessageFlag = false;

        // reset page being displayed
        giMessagePage = 0;
    }

    bool EnterEmail()
    {
        // load graphics
        iCurrentPage = LaptopSaveInfo.iCurrentEmailPage;

        // title bar
        this.video.GetVideoObject("LAPTOP\\programtitlebar.sti", out guiEmailTitle);

        // the list background
        this.video.GetVideoObject("LAPTOP\\Mailwindow.sti", out guiEmailBackground);

        // the indication/notification box
        this.video.GetVideoObject("LAPTOP\\MailIndicator.sti", out guiEmailIndicator);

        // the message background
        this.video.GetVideoObject("LAPTOP\\emailviewer.sti", out guiEmailMessage);

        // the message background
        this.video.GetVideoObject("LAPTOP\\maillistdivider.sti", out guiMAILDIVIDER);

        //AddEmail(IMP_EMAIL_PROFILE_RESULTS, IMP_EMAIL_PROFILE_RESULTS_LENGTH, IMP_PROFILE_RESULTS, GetWorldTotalMin( ) );
        // initialize mouse regions
        this.InitializeMouseRegions();

        // just started email
        this.fJustStartedEmail = true;

        // create buttons 
        this.CreateMailScreenButtons();

        // marks these buttons dirty
        ButtonSubSystem.MarkButtonsDirty(buttonList);

        // no longer fitrst time in email
        this.fFirstTime = false;

        // reset current page of the message being displayed
        giMessagePage = 0;

        // render email background and text
        this.RenderEmail();


        //AddEmail( MERC_REPLY_GRIZZLY, MERC_REPLY_LENGTH_GRIZZLY, GRIZZLY_MAIL, GetWorldTotalMin() );
        //RenderButtons( );


        return true;
    }

    void ExitEmail()
    {
        LaptopSaveInfo.iCurrentEmailPage = iCurrentPage;

        // clear out message record list
        this.ClearOutEmailMessageRecordsList();

        // displayed message?...get rid of it
        if (this.fDisplayMessageFlag)
        {
            this.fDisplayMessageFlag = false;
            this.AddDeleteRegionsToMessageRegion(0);
            this.fDisplayMessageFlag = true;
            this.fReDrawMessageFlag = true;
        }
        else
        {
            giMessageId = -1;
        }

        // delete mail notice?...get rid of it
        if (this.fDeleteMailFlag)
        {
            this.fDeleteMailFlag = false;
            this.CreateDestroyDeleteNoticeMailButton();
        }

        // remove all mouse regions in use in email
        this.DeleteEmailMouseRegions();

        // reset flags of new messages
        this.SetUnNewMessages();

        // remove video objects being used by email screen
        this.video.DeleteVideoObjectFromIndex(guiEmailTitle);
        this.video.DeleteVideoObjectFromIndex(guiEmailBackground);
        this.video.DeleteVideoObjectFromIndex(guiMAILDIVIDER);
        this.video.DeleteVideoObjectFromIndex(guiEmailIndicator);
        this.video.DeleteVideoObjectFromIndex(guiEmailMessage);


        // remove buttons
        this.DestroyMailScreenButtons();


    }

    static bool fEmailListBeenDrawAlready = false;
    void HandleEmail()
    {

        int iViewerY = 0;
        //RenderButtonsFastHelp( );


        // check if email message record list needs to be updated
        this.UpDateMessageRecordList();

        // does email list need to be draw, or can be drawn 
        if ((!this.fDisplayMessageFlag) && (!fNewMailFlag) && (!this.fDeleteMailFlag) && (fEmailListBeenDrawAlready == false))
        {
            this.DisplayEmailList();
            fEmailListBeenDrawAlready = true;
        }
        // if the message flag, show message
        else if (this.fDisplayMessageFlag && this.fReDrawMessageFlag)
        {
            // redisplay list
            this.DisplayEmailList();

            // this simply redraws message without button manipulation
            iViewerY = this.DisplayEmailMessage(this.GetEmailMessage(giMessageId));
            fEmailListBeenDrawAlready = false;

        }
        else if (this.fDisplayMessageFlag && (!this.fOldDisplayMessageFlag))
        {

            // redisplay list
            this.DisplayEmailList();

            // this simply redraws message with button manipulation
            iViewerY = this.DisplayEmailMessage(this.GetEmailMessage(giMessageId));
            this.AddDeleteRegionsToMessageRegion(iViewerY);
            fEmailListBeenDrawAlready = false;

        }

        // not displaying anymore?
        if ((this.fDisplayMessageFlag == false) && this.fOldDisplayMessageFlag)
        {
            // then clear it out
            this.ClearOutEmailMessageRecordsList();
        }


        // if new message is being displayed...check to see if it's buttons need to be created or destroyed
        this.AddDeleteRegionsToMessageRegion(0);

        // same with delete notice
        this.CreateDestroyDeleteNoticeMailButton();

        // if delete notice needs to be displayed?...display it
        if (this.fDeleteMailFlag)
        {
            this.DisplayDeleteNotice(this.GetEmailMessage(this.iDeleteId));
        }


        // update buttons
        this.HandleEmailViewerButtonStates();

        // set up icons for buttons
        this.SetUpIconForButton();

        // redraw screen
        //ReDraw();

        //redraw headers to sort buttons
        this.DisplayEmailHeaders();


        // handle buttons states
        this.UpdateStatusOfNextPreviousButtons();

        if (this.fOpenMostRecentUnReadFlag == true)
        {
            // enter email due to email icon on program panel
            this.OpenMostRecentUnreadEmail();
            this.fOpenMostRecentUnReadFlag = false;

        }

        return;
    }

    void DisplayEmailHeaders()
    {
        // draw the text at the top of the screen

        // font stuff
        FontSubSystem.SetFont(EMAIL_WARNING_FONT);
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);
        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);

        // draw headers to the email list the player sees

        // sender text
        //mprintf(FROM_X, FROM_Y, pEmailHeaders[FROM_HEADER]);

        // subject text
        //mprintf(SUBJECTHEAD_X, FROM_Y, pEmailHeaders[SUBJECT_HEADER]);

        // date re'vd
        //mprintf(RECD_X, FROM_Y, pEmailHeaders[RECD_HEADER]);

        // reset shadow
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);


        return;
    }

    void RenderEmail()
    {
        HVOBJECT hHandle;
        int iCounter = 0;

        // get and blt the email list background
        hHandle = this.video.GetVideoObject(guiEmailBackground);
//        video.BltVideoObject(Surfaces.FRAME_BUFFER, hHandle, 0, LAPTOP_SCREEN_UL_X, EMAIL_LIST_WINDOW_Y + LAPTOP_SCREEN_UL_Y, VO_BLT.SRCTRANSPARENCY, null);


        // get and blt the email title bar
        hHandle = this.video.GetVideoObject(guiEmailTitle);
        //        video.BltVideoObject(Surfaces.FRAME_BUFFER, hHandle, 0, LAPTOP_SCREEN_UL_X, LAPTOP_SCREEN_UL_Y - 2, VO_BLT.SRCTRANSPARENCY, null);

        // show text on titlebar
        this.DisplayTextOnTitleBar();

        // redraw list if no graphics are being displayed on top of it
        //if((!fDisplayMessageFlag)&&(!fNewMailFlag)) 
        //{
        this.DisplayEmailList();
        //}

        // redraw line dividers
        this.DrawLineDividers();


        // show next/prev page buttons depending if there are next/prev page
        //DetermineNextPrevPageDisplay( );

        // draw headers for buttons
        this.DisplayEmailHeaders();

        // display border
        hHandle = this.video.GetVideoObject(guiLaptopBACKGROUND);
        //        video.BltVideoObject(Surfaces.FRAME_BUFFER, hHandle, 0, 108, 23, VO_BLT.SRCTRANSPARENCY, null);


        this.ReDisplayBoxes();

        //        BlitTitleBarIcons();



        // show which page we are on
        this.DisplayWhichPageOfEmailProgramIsDisplayed();


        this.video.InvalidateRegion(0, 0, 640, 480);
        // invalidate region to force update
        return;
    }

    public static void AddEmailWithSpecialData(int iMessageOffset, int iMessageLength, EmailAddresses ubSender, uint iDate, int iFirstData, object uiSecondData)
    {
        string pSubject = string.Empty;
        //MessagePtr pMessageList;
        //MessagePtr pMessage;
        //string pMessageString[320];
        int iPosition = 0;
        int iCounter = 1;
        email FakeEmail = new();


        // starts at iSubjectOffset amd goes iSubjectLength, reading in string
//        files.LoadEncryptedDataFromFile("BINARYDATA\\Email.edt", out pSubject, 640 * (iMessageOffset), 640);

        //Make a fake email that will contain the codes ( ie the merc ID )
        FakeEmail.iFirstData = iFirstData;
        FakeEmail.uiSecondData = uiSecondData;

        //Replace the $mercname$ with the actual mercname
        ReplaceMercNameAndAmountWithProperData(pSubject, FakeEmail);

        // add message to list
        AddEmailMessage(iMessageOffset, iMessageLength, pSubject, iDate, ubSender, false, iFirstData, uiSecondData);

        // if we are in fact int he laptop, redraw icons, might be change in mail status

        if (fCurrentlyInLaptop == true)
        {
            // redraw icons, might be new mail
            Laptop.DrawLapTopIcons();
        }

        return;
    }

    public static void AddEmail(int iMessageOffset, int iMessageLength, EmailAddresses ubSender, uint iDate)
    {
        string pSubject = string.Empty;
        //MessagePtr pMessageList;
        //MessagePtr pMessage;
        //string pMessageString[320];
        int iPosition = 0;
        int iCounter = 1;


        // starts at iSubjectOffset amd goes iSubjectLength, reading in string
//        files.LoadEncryptedDataFromFile("BINARYDATA\\Email.edt", out pSubject, 640 * (iMessageOffset), 640);

        // add message to list
        AddEmailMessage(iMessageOffset, iMessageLength, pSubject, iDate, ubSender, false, 0, 0);

        // if we are in fact int he laptop, redraw icons, might be change in mail status

        if (fCurrentlyInLaptop == true)
        {
            // redraw icons, might be new mail
            Laptop.DrawLapTopIcons();
        }

        return;
    }

    void AddPreReadEmail(int iMessageOffset, int iMessageLength, EmailAddresses ubSender, uint iDate)
    {
        //MessagePtr pMessageList;
        //MessagePtr pMessage;
        //string pMessageString[320];
        int iPosition = 0;
        int iCounter = 1;


        // starts at iSubjectOffset amd goes iSubjectLength, reading in string
        this.files.LoadEncryptedDataFromFile("BINARYDATA\\Email.edt", out string pSubject, (uint)(640 * iMessageOffset), 640);

        // add message to list
        AddEmailMessage(iMessageOffset, iMessageLength, pSubject, iDate, ubSender, true, 0, 0);

        // if we are in fact int he laptop, redraw icons, might be change in mail status

        if (fCurrentlyInLaptop == true)
        {
            // redraw icons, might be new mail
            Laptop.DrawLapTopIcons();
        }

        return;
    }

    private static void AddEmailMessage(int iMessageOffset, int iMessageLength, string pSubject, uint iDate, EmailAddresses ubSender, bool fAlreadyRead, int iFirstData, object uiSecondData)
    {
        // will add a message to the list of messages
        email? pEmail = pEmailList;
        email? pTempEmail = null;
        int iCounter = 0;
        int iId = 0;

        // run through list of messages, get id of oldest message
        if (pEmail is not null)
        {
            while (pEmail is not null)
            {
                if (pEmail.iId > iId)
                {
                    iId = pEmail.iId;
                }

                pEmail = pEmail.Next;
            }
        }

        // reset pEmail
        pEmail = pEmailList;

        // move to end of list
        if (pEmail is not null)
        {
            while (pEmail.Next is not null)
            {
                pEmail = pEmail.Next;
            }
        }

        // add new element onto list
        pTempEmail = new();
        // add in strings
        //while(pMessage !=null)
        //{
        //pTempEmail.pText[iCounter]=MemAlloc((wcslen(pMessage.pString)+2)*2);
        //wcscpy(pTempEmail.pText[iCounter],pMessage.pString);
        //pMessage=pMessage.Next;
        //iCounter++;
        //}	
        //pTempEmail.pText[iCounter]=null;

        // copy subject
        pTempEmail.pSubject = pSubject;

        // copy offset and length of the actual message in email.edt
        pTempEmail.usOffset = (int)iMessageOffset;
        pTempEmail.usLength = (int)iMessageLength;

        // null out last byte of subject
        //pTempEmail.pSubject[wcslen(pSubject) + 1] = 0;


        // set date and sender, Id
        if (pEmail is not null)
        {
            pTempEmail.iId = iId + 1;
        }
        else
        {
            pTempEmail.iId = 0;
        }

        // copy date and sender id's
        pTempEmail.iDate = iDate;
        pTempEmail.ubSender = ubSender;

        // the special data
        pTempEmail.iFirstData = iFirstData;
        pTempEmail.uiSecondData = uiSecondData;

        // place into list
        if (pEmail is not null)
        {
            // list exists, place at end
            pEmail.Next = pTempEmail;
            pTempEmail.Prev = pEmail;
        }
        else
        {
            // no list, becomes head of a new list
            pEmail = pTempEmail;
            pTempEmail.Prev = null;
            pEmailList = pEmail;
        }

        // reset Next ptr
        pTempEmail.Next = null;

        // set flag that new mail has arrived
        fNewMailFlag = true;

        // add this message to the pages of email
        AddMessageToPages(pTempEmail.iId);

        // reset read flag of this particular message
        pTempEmail.fRead = fAlreadyRead;

        // set fact this message is new
        pTempEmail.fNew = true;
        return;
    }


    void RemoveEmailMessage(int iId)
    {
        // run through list and remove message, update everyone afterwards
        email? pEmail = pEmailList;
        email? pTempEmail = null;
        int iCounter = 0;


        // error check
        if (pEmail is null)
        {
            return;
        }

        // look for message
        pEmail = this.GetEmailMessage(iId);
        //while((pEmail.iId !=iId)&&(pEmail . Next))
        //	pEmail=pEmail.Next;

        // end of list, no mail found, leave
        if (pEmail is null)
        {
            return;
        }
        // found

        // set tempt o current
        pTempEmail = pEmail;

        // check position of message in list
        if ((pEmail.Prev is not null) && (pTempEmail.Next is not null))
        {
            // in the middle of the list
            pEmail = pEmail.Prev;
            pTempEmail = pTempEmail.Next;
            pEmail.Next.pSubject = string.Empty;
            //while(pEmail.Next.pText[iCounter])
            //{
            //MemFree(pEmail.Next.pText[iCounter]);
            //iCounter++;
            //}
            pEmail.Next = pTempEmail;
            pTempEmail.Prev = pEmail;
        }
        else if (pEmail.Prev is not null)
        {
            // end of the list
            pEmail = pEmail.Prev;
            MemFree(pEmail.Next.pSubject);
            //while(pEmail.Next.pText[iCounter])
            //{
            //MemFree(pEmail.Next.pText[iCounter]);
            //iCounter++;
            //}
            pEmail.Next = null;
        }
        else if (pTempEmail.Next is not null)
        {
            // beginning of the list
            pEmail = pTempEmail;
            pTempEmail = pTempEmail.Next;
            MemFree(pEmail.pSubject);
            //while(pEmail.pText[iCounter])
            //{
            //MemFree(pEmail.pText[iCounter]);
            //iCounter++;
            //}
            MemFree(pEmail);
            pTempEmail.Prev = null;
            pEmailList = pTempEmail;
        }
        else
        {
            // all alone
            MemFree(pEmail.pSubject);
            //	while(pEmail.pText[iCounter])
            //{
            //MemFree(pEmail.pText[iCounter]);
            //iCounter++;
            //}
            MemFree(pEmail);
            pEmailList = null;
        }
    }

    email? GetEmailMessage(int iId)
    {
        email? pEmail = pEmailList;
        // return pointer to message with iId

        // invalid id
        if (iId == -1)
        {
            return null;
        }

        // invalid list
        if (pEmail == null)
        {
            return null;
        }

        // look for message 
        while ((pEmail.iId != iId) && (pEmail.Next is not null))
        {
            pEmail = pEmail.Next;
        }

        if ((pEmail.iId != iId) && (pEmail.Next == null))
        {
            pEmail = null;
        }

        // no message, or is there?
        if (pEmail is null)
        {
            return null;
        }
        else
        {
            return pEmail;
        }
    }


    public static void AddEmailPage()
    {
        // simple adds a page to the list
        PagePtr pPage = pPageList;
        if (pPage is not null)
        {
            while (pPage.Next is not null)
            {
                pPage = pPage.Next;
            }
        }


        if (pPage is not null)
        {

            // there is a page, add current page after it
            pPage.Next = new();//MemAlloc(sizeof(Page));
            pPage.Next.Prev = pPage;
            pPage = pPage.Next;
            pPage.Next = null;
            pPage.iPageId = pPage.Prev.iPageId + 1;
            //memset(pPage.iIds, -1, sizeof(int) * MAX_MESSAGES_PAGE);
        }
        else
        {

            // page becomes head of page list
            pPageList = new();//MemAlloc(sizeof(Page));
            pPage = pPageList;
            pPage.Prev = null;
            pPage.Next = null;
            pPage.iPageId = 0;
            //memset(pPage.iIds, -1, sizeof(int) * MAX_MESSAGES_PAGE);
            pPageList = pPage;
        }
        iLastPage++;
        return;

    }


    void RemoveEmailPage(int iPageId)
    {
        PagePtr? pPage = pPageList;
        PagePtr? pTempPage = null;

        // run through list until page is matched, or out of pages
        while ((pPage.iPageId != iPageId) && (pPage is not null))
        {
            pPage = pPage.Next;
        }

        // error check
        if (pPage is null)
        {
            return;
        }


        // found
        pTempPage = pPage;
        if ((pPage.Prev is not null) && (pTempPage.Next is not null))
        {
            // in the middle of the list
            pPage = pPage.Prev;
            pTempPage = pTempPage.Next;
            MemFree(pPage.Next);
            pPage.Next = pTempPage;
            pTempPage.Prev = pPage;
        }
        else if (pPage.Prev is not null)
        {
            // end of the list
            pPage = pPage.Prev;
            MemFree(pPage.Next);
            pPage.Next = null;
        }
        else if (pTempPage.Next is not null)
        {
            // beginning of the list
            pPage = pTempPage;
            pTempPage = pTempPage.Next;
            MemFree(pPage);
            pTempPage.Prev = null;
        }
        else
        {
            // all alone

            MemFree(pPage);
            pPageList = null;
        }
        if (iLastPage != 0)
        {
            iLastPage--;
        }
    }

    public static void AddMessageToPages(int iMessageId)
    {
        // go to end of page list
        PagePtr? pPage = pPageList;
        int iCounter = 0;
        if (pPage is null)
        {
            AddEmailPage();
        }

        pPage = pPageList;
        while ((pPage.Next is not null) && (pPage.iIds[MAX_MESSAGES_PAGE - 1] != -1))
        {
            pPage = pPage.Next;
        }
        // if list is full, add new page
        while (iCounter < MAX_MESSAGES_PAGE)
        {
            if (pPage.iIds[iCounter] == -1)
            {
                break;
            }

            iCounter++;
        }
        if (iCounter == MAX_MESSAGES_PAGE)
        {
            AddEmailPage();
            AddMessageToPages(iMessageId);
            return;
        }
        else
        {
            pPage.iIds[iCounter] = iMessageId;
        }
        return;
    }

    void SortMessages(EmailFields iCriteria)
    {
        email? pA = pEmailList;
        email? pB = pEmailList;
        string pSubjectA = string.Empty;
        string pSubjectB = string.Empty;
        int iId = 0;

        // no messages to sort?
        if ((pA == null) || (pB == null))
        {
            return;
        }

        // nothing here either?
        if (pA.Next is null)
        {
            return;
        }

        pB = pA.Next;
        switch (iCriteria)
        {
            case EmailFields.RECEIVED:
                while (pA is not null)
                {

                    // set B to next in A
                    pB = pA.Next;
                    while (pB is not null)
                    {

                        if (this.fSortDateUpwards)
                        {
                            // if date is lesser, swap
                            if (pA.iDate > pB.iDate)
                            {
                                this.SwapMessages(pA.iId, pB.iId);
                            }
                        }
                        else
                        {
                            // if date is lesser, swap
                            if (pA.iDate < pB.iDate)
                            {
                                this.SwapMessages(pA.iId, pB.iId);
                            }
                        }


                        // next in B's list
                        pB = pB.Next;
                    }

                    // next in A's List
                    pA = pA.Next;
                }
                break;
            case EmailFields.SENDER:
                while (pA is not null)
                {

                    pB = pA.Next;
                    while (pB is not null)
                    {
                        // lesser string?...need sorting 
                        if (this.fSortSenderUpwards)
                        {
                            //                            if ((wcscmp(pSenderNameList[pA.ubSender], pSenderNameList[pB.ubSender])) < 0)
                            //                            {
                            //                                SwapMessages(pA.iId, pB.iId);
                            //                            }
                        }
                        else
                        {
                            //                            if ((wcscmp(pSenderNameList[pA.ubSender], pSenderNameList[pB.ubSender])) > 0)
                            //                            {
                            //                                SwapMessages(pA.iId, pB.iId);
                            //                            }
                        }
                        // next in B's list
                        pB = pB.Next;
                    }
                    // next in A's List
                    pA = pA.Next;
                }
                break;
            case EmailFields.SUBJECT:
                while (pA is not null)
                {

                    pB = pA.Next;
                    while (pB is not null)
                    {
                        // clear out control codes
                        //                        CleanOutControlCodesFromString(pA.pSubject, pSubjectA);
                        //                        CleanOutControlCodesFromString(pB.pSubject, pSubjectB);

                        // lesser string?...need sorting  
                        if (this.fSortSubjectUpwards)
                        {
                            if (wcscmp(pA.pSubject, pB.pSubject) < 0)
                            {
                                this.SwapMessages(pA.iId, pB.iId);
                            }
                        }
                        else
                        {
                            if (wcscmp(pA.pSubject, pB.pSubject) > 0)
                            {
                                this.SwapMessages(pA.iId, pB.iId);
                            }
                        }
                        // next in B's list
                        pB = pB.Next;
                    }
                    // next in A's List
                    pA = pA.Next;
                }
                break;

            case EmailFields.READ:
                while (pA is not null)
                {

                    pB = pA.Next;
                    while (pB is not null)
                    {
                        // one read and another not?...need sorting  
                        if (pA.fRead && (!pB.fRead))
                        {
                            this.SwapMessages(pA.iId, pB.iId);
                        }

                        // next in B's list
                        pB = pB.Next;
                    }
                    // next in A's List
                    pA = pA.Next;
                }
                break;
        }


        // place new list into pages of email
        //PlaceMessagesinPages();

        // redraw the screen
        fReDrawScreenFlag = true;
    }

    void SwapMessages(int iIdA, int iIdB)
    {
        // swaps locations of messages in the linked list
        email? pA = pEmailList;
        email? pB = pEmailList;
        email? pTemp = new();//MemAlloc(sizeof(Email));

        pTemp.pSubject = string.Empty;

        if (pA.Next is null)
        {
            return;
        }
        //find pA
        while (pA.iId != iIdA)
        {
            pA = pA.Next;
        }
        // find pB
        while (pB.iId != iIdB)
        {
            pB = pB.Next;
        }

        // swap

        // pTemp becomes pA
        pTemp.iId = pA.iId;
        pTemp.fRead = pA.fRead;
        pTemp.fNew = pA.fNew;
        pTemp.usOffset = pA.usOffset;
        pTemp.usLength = pA.usLength;
        pTemp.iDate = pA.iDate;
        pTemp.ubSender = pA.ubSender;
        pTemp.pSubject = wcscpy(pA.pSubject);

        // pA becomes pB
        pA.iId = pB.iId;
        pA.fRead = pB.fRead;
        pA.fNew = pB.fNew;
        pA.usOffset = pB.usOffset;
        pA.usLength = pB.usLength;
        pA.iDate = pB.iDate;
        pA.ubSender = pB.ubSender;
        pA.pSubject = wcscpy(pB.pSubject);

        // pB becomes pTemp
        pB.iId = pTemp.iId;
        pB.fRead = pTemp.fRead;
        pB.fNew = pTemp.fNew;
        pB.usOffset = pTemp.usOffset;
        pB.usLength = pTemp.usLength;
        pB.iDate = pTemp.iDate;
        pB.ubSender = pTemp.ubSender;
        pB.pSubject = wcscpy(pTemp.pSubject);

        // free up memory
        MemFree(pTemp.pSubject);
        MemFree(pTemp);
        return;
    }

    public static void ClearPages()
    {
        // run through list of message pages and set to -1
        PagePtr? pPage = pPageList;

        // error check
        if (pPageList == null)
        {
            return;
        }

        while (pPage.Next is not null)
        {
            pPage = pPage.Next;
            MemFree(pPage.Prev);
        }
        if (pPage is not null)
        {
            MemFree(pPage);
        }

        pPageList = null;
        iLastPage = -1;

        return;
    }

    void PlaceMessagesinPages()
    {
        email pEmail = pEmailList;
        // run through the list of messages and add to pages
        ClearPages();
        while (pEmail is not null)
        {
            AddMessageToPages(pEmail.iId);
            pEmail = pEmail.Next;

        }
        if (iCurrentPage > iLastPage)
        {
            iCurrentPage = iLastPage;
        }

        return;
    }

    void DisplayMessageList(int iPageNum)
    {
        // will display page with idNumber iPageNum
        PagePtr pPage = pPageList;
        while (pPage.iPageId != iPageNum)
        {
            pPage = pPage.Next;
            if (pPage is null)
            {
                return;
            }
        }
        // found page show it
        return;
    }

    void DrawLetterIcon(int iCounter, bool fRead)
    {
        HVOBJECT hHandle;
        // will draw the icon for letter in mail list depending if the mail has been read or not

        // grab video object
        hHandle = this.video.GetVideoObject(guiEmailIndicator);

        // is it read or not?
        if (fRead)
        {
//            video.BltVideoObject(Surfaces.FRAME_BUFFER, hHandle, 0, INDIC_X, (MIDDLE_Y + iCounter * MIDDLE_WIDTH + 2), VO_BLT.SRCTRANSPARENCY, null);
        }
        else
        {
//            video.BltVideoObject(Surfaces.FRAME_BUFFER, hHandle, 1, INDIC_X, (MIDDLE_Y + iCounter * MIDDLE_WIDTH + 2), VO_BLT.SRCTRANSPARENCY, null);
        }

        return;
    }

    void DrawSubject(int iCounter, string pSubject, bool fRead)
    {
        string pTempSubject;


        // draw subject line of mail being viewed in viewer

        // lock buffer to prevent overwrite
        FontSubSystem.SetFontDestBuffer(SurfaceType.FRAME_BUFFER, SUBJECT_X, (int)(MIDDLE_Y + iCounter * MIDDLE_WIDTH), SUBJECT_X + SUBJECT_WIDTH, ((int)(MIDDLE_Y + iCounter * MIDDLE_WIDTH)) + MIDDLE_WIDTH, false);
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);
        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);

        pTempSubject = pSubject;

        if (fRead)
        {
            //if the subject will be too long, cap it, and add the '...'
            if (FontSubSystem.StringPixLength(pTempSubject, MESSAGE_FONT) >= SUBJECT_WIDTH - 10)
            {
                WordWrap.ReduceStringLength(pTempSubject, SUBJECT_WIDTH - 10, MESSAGE_FONT);
            }

            // display string subject
            //            IanDisplayWrappedString(SUBJECT_X, ((int)(4 + MIDDLE_Y + iCounter * MIDDLE_WIDTH)), SUBJECT_WIDTH, MESSAGE_GAP, MESSAGE_FONT, MESSAGE_COLOR, pTempSubject, 0, false, TextJustifies.LEFT_JUSTIFIED);
        }
        else
        {
            //if the subject will be too long, cap it, and add the '...'
            if (FontSubSystem.StringPixLength(pTempSubject, FontStyle.FONT10ARIALBOLD) >= SUBJECT_WIDTH - 10)
            {
                WordWrap.ReduceStringLength(pTempSubject, SUBJECT_WIDTH - 10, FontStyle.FONT10ARIALBOLD);
            }

            // display string subject
            //            IanDisplayWrappedString(SUBJECT_X, ((int)(4 + MIDDLE_Y + iCounter * MIDDLE_WIDTH)), SUBJECT_WIDTH, MESSAGE_GAP,
            //                FontStyle.FONT10ARIALBOLD, MESSAGE_COLOR, pTempSubject, 0, false,
            //                TextJustifies.LEFT_JUSTIFIED);

        }

        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
        // reset font dest buffer
        FontSubSystem.SetFontDestBuffer(SurfaceType.FRAME_BUFFER, 0, 0, 640, 480, false);

        return;
    }

    void DrawSender(int iCounter, EmailAddresses ubSender, bool fRead)
    {
        // draw name of sender in mail viewer
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);
        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);

        if (fRead)
        {
            FontSubSystem.SetFont(MESSAGE_FONT);
        }
        else
        {
            FontSubSystem.SetFont(FontStyle.FONT10ARIALBOLD);
        }

        //        mprintf(SENDER_X, ((int)(4 + MIDDLE_Y + iCounter * MIDDLE_WIDTH)), pSenderNameList[ubSender]);

        FontSubSystem.SetFont(MESSAGE_FONT);
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
        return;
    }

    void DrawDate(int iCounter, uint iDate, bool fRead)
    {
        string sString = string.Empty;

        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);
        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);

        if (fRead)
        {
            FontSubSystem.SetFont(MESSAGE_FONT);
        }
        else
        {
            FontSubSystem.SetFont(FontStyle.FONT10ARIALBOLD);
        }
        // draw date of message being displayed in mail viewer
        //        wprintf(sString, "%s %d", pDayStrings[0], iDate / (24 * 60));
        mprintf(DATE_X, (int)(4 + MIDDLE_Y + iCounter * MIDDLE_WIDTH), sString);

        FontSubSystem.SetFont(MESSAGE_FONT);
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
        return;
    }

    void DisplayEmailList()
    {
        int iCounter = 0;
        // look at current page, and display
        PagePtr? pPage = pPageList;
        email? pEmail = null;


        // error check, if no page, return
        if (pPage is null)
        {
            return;
        }

        // if current page ever ends up negative, reset to 0
        if (iCurrentPage == -1)
        {
            iCurrentPage = 0;
        }

        // loop until we get to the current page
        while ((pPage.iPageId != iCurrentPage) && (iCurrentPage <= iLastPage))
        {
            pPage = pPage.Next;
        }

        // now we have current page, display it
        pEmail = this.GetEmailMessage(pPage.iIds[iCounter]);
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);
        FontSubSystem.SetFont(EMAIL_TEXT_FONT);


        // draw each line of the list for this page
        while (pEmail is not null)
        {

            // highlighted message, set text of message in list to blue
            //            if (iCounter == iHighLightLine)
            //            {
            //                FontSubSystem.SetFontForeground(FontColor.FONT_BLUE);
            //            }
            //            else if (pEmail.fRead)
            //            {
            //                // message has been read, reset color to black
            //                FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
            //                //SetFontBackground(FONT_BLACK);
            //
            //            }
            //            else
            //            {
            //                // defualt, message is not read, set font red
            //                FontSubSystem.SetFontForeground(FontColor.FONT_RED);
            //                //SetFontBackground(FONT_BLACK);
            //            }

            FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);

            //draw the icon, sender, date, subject
            this.DrawLetterIcon(iCounter, pEmail.fRead);
            this.DrawSubject(iCounter, pEmail.pSubject, pEmail.fRead);
            this.DrawSender(iCounter, pEmail.ubSender, pEmail.fRead);
            this.DrawDate(iCounter, pEmail.iDate, pEmail.fRead);

            iCounter++;

            // too many messages onthis page, reset pEmail, so no more are drawn
            if (iCounter >= MAX_MESSAGES_PAGE)
            {
                pEmail = null;
            }
            else
            {
                pEmail = this.GetEmailMessage(pPage.iIds[iCounter]);
            }
        }

        this.video.InvalidateRegion(LAPTOP_SCREEN_UL_X, LAPTOP_SCREEN_UL_Y, LAPTOP_SCREEN_LR_X, LAPTOP_SCREEN_LR_Y);

        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
        return;
    }


    void LookForUnread()
    {
        bool fStatusOfNewEmailFlag = this.fUnReadMailFlag;

        // simply runrs through list of messages, if any unread, set unread flag

        email? pA = pEmailList;

        // reset unread flag
        this.fUnReadMailFlag = false;

        // look for unread mail
        while (pA is not null)
        {
            // unread mail found, set flag
            if (!pA.fRead)
            {
                this.fUnReadMailFlag = true;
            }

            pA = pA.Next;
        }

        if (fStatusOfNewEmailFlag != this.fUnReadMailFlag)
        {
            //Since there is no new email, get rid of the hepl text
            //            CreateFileAndNewEmailIconFastHelpText(LAPTOP_BN_HLP_TXT_YOU_HAVE_NEW_MAIL, (bool)!fUnReadMailFlag);
        }

        return;
    }

    void EmailBtnCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        int iCount;
        PagePtr? pPage = pPageList;
        int iId = 0;
        email? pEmail = null;
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
        {
            return;
        }
        if (this.fDisplayMessageFlag)
        {
            return;
        }

        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {

            // error check
            iCount = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);
            // check for valid email
            // find surrent page
            if (pPage is null)
            {
                return;
            }

            while ((pPage.Next is not null) && (pPage.iPageId != iCurrentPage))
            {
                pPage = pPage.Next;
            }

            if (pPage is null)
            {
                return;
            }
            // found page

            // get id for element iCount
            iId = pPage.iIds[iCount];

            // invalid message
            if (iId == -1)
            {
                this.fDisplayMessageFlag = false;
                return;
            }
            // Get email and display
            this.fDisplayMessageFlag = true;
            giMessagePage = 0;
            giPrevMessageId = giMessageId;
            giMessageId = iId;


        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            iCount = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);

            // error check
            if (pPage is null)
            {
                //                HandleRightButtonUpEvent();
                return;
            }

            giMessagePage = 0;

            while ((pPage.Next is not null) && (pPage.iPageId != iCurrentPage))
            {
                pPage = pPage.Next;
            }

            if (pPage is null)
            {
                //                HandleRightButtonUpEvent();
                return;
            }
            // found page
            // get id for element iCount
            iId = pPage.iIds[iCount];
            if (this.GetEmailMessage(iId) is null)
            {
                // no mail here, handle right button up event
                //                HandleRightButtonUpEvent();
                return;
            }
            else
            {
                this.fDeleteMailFlag = true;
                this.iDeleteId = iId;
                //DisplayDeleteNotice(GetEmailMessage(iDeleteId));
                //DeleteEmail();
            }
        }
    }
    void EmailMvtCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
        {
            return;
        }
        if (this.fDisplayMessageFlag)
        {
            return;
        }

        if (iReason == MSYS_CALLBACK_REASON.MOVE)
        {

            // set highlight to current regions data, this is the message to display
            //            iHighLightLine = MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);
        }
        if (iReason == MSYS_CALLBACK_REASON.LOST_MOUSE)
        {

            // reset highlight line to invalid message
            //            iHighLightLine = -1;
        }
    }

    void BtnMessageXCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (!btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
        {
            return;
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN) || reason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_DWN))
        {

            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;

        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) || reason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {

            if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                // X button has been pressed and let up, this means to stop displaying the currently displayed message

                // reset display message flag
                this.fDisplayMessageFlag = false;

                // reset button flag
                btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;

                // reset page being displayed
                giMessagePage = 0;

                // redraw icons
                Laptop.DrawLapTopIcons();

                // force update of entire screen
                this.fPausedReDrawScreenFlag = true;

                // rerender email
                //RenderEmail();
            }
        }

    }
    void
    SetUnNewMessages()
    {
        // on exit from the mailer, set all new messages as 'un'new
        email? pEmail = pEmailList;
        // run through the list of messages and add to pages

        while (pEmail is not null)
        {
            pEmail.fNew = false;
            pEmail = pEmail.Next;
        }
        return;
    }

    int DisplayEmailMessage(email pMail)
    {
        HVOBJECT hHandle;
        int iCnt = 0;
        int iHeight = 0;
        int iCounter = 1;
        //	string pString[MAIL_STRING_SIZE/2 + 1];
        string pString;
        int iOffSet = 0;
        int iHeightTemp = 0;
        int iHeightSoFar = 0;
        RecordPtr? pTempRecord;
        int iPageSize = 0;
        int iPastHeight = 0;
        int iYPositionOnPage = 0;
        int iTotalYPosition = 0;
        bool fGoingOffCurrentPage = false;
        bool fDonePrintingMessage = false;



        if (pMail is null)
        {
            return 0;
        }

        iOffSet = (int)pMail.usOffset;

        // reset redraw email message flag
        this.fReDrawMessageFlag = false;

        // we KNOW the player is going to "read" this, so mark it as so
        pMail.fRead = true;

        // draw text for title bar
        //wprintf(pString, "%s / %s", pSenderNameList[pMail.ubSender],pMail.pSubject);
        //DisplayWrappedString(VIEWER_X+VIEWER_HEAD_X+4, VIEWER_Y+VIEWER_HEAD_Y+4, VIEWER_HEAD_WIDTH, MESSAGE_GAP, MESSAGE_FONT, MESSAGE_COLOR, pString, 0,false,0);

        // increment height for size of one line
        iHeight += FontSubSystem.GetFontHeight(MESSAGE_FONT);

        // is there any special event meant for this mail?..if so, handle it
        this.HandleAnySpecialEmailMessageEvents(iOffSet);

        this.HandleMailSpecialMessages((int)iOffSet, this.iViewerPositionY, pMail);

        this.PreProcessEmail(pMail);


        //        pTempRecord = pMessageRecordList;



        // blt in top line of message as a blank graphic
        // get a handle to the bitmap of EMAIL VIEWER Background
        hHandle = this.video.GetVideoObject(guiEmailMessage);

        // place the graphic on the frame buffer
//        video.BltVideoObject(Surfaces.FRAME_BUFFER, hHandle, 1, VIEWER_X, VIEWER_MESSAGE_BODY_START_Y + iViewerPositionY, VO_BLT.SRCTRANSPARENCY, null);
//        video.BltVideoObject(Surfaces.FRAME_BUFFER, hHandle, 1, VIEWER_X, VIEWER_MESSAGE_BODY_START_Y + FontSubSystem.GetFontHeight(MESSAGE_FONT) + iViewerPositionY, VO_BLT.SRCTRANSPARENCY, null);

        // set shadow
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);

        // get a handle to the bitmap of EMAIL VIEWER
        hHandle = this.video.GetVideoObject(guiEmailMessage);

        // place the graphic on the frame buffer
//        video.BltVideoObject(Surfaces.FRAME_BUFFER, hHandle, 0, VIEWER_X, VIEWER_Y + iViewerPositionY, VO_BLT.SRCTRANSPARENCY, null);


        // the icon for the title of this box
        hHandle = this.video.GetVideoObject(guiTITLEBARICONS);
        //        video.BltVideoObject(Surfaces.FRAME_BUFFER, hHandle, 0, VIEWER_X + 5, VIEWER_Y + iViewerPositionY + 2, VO_BLT.SRCTRANSPARENCY, null);

        // display header text
        this.DisplayEmailMessageSubjectDateFromLines(pMail, this.iViewerPositionY);

        // display title text
        this.DrawEmailMessageDisplayTitleText(this.iViewerPositionY);



        iCounter = 0;
        // now blit the text background based on height
        for (iCounter = 2; iCounter < (this.iTotalHeight / FontSubSystem.GetFontHeight(MESSAGE_FONT)); iCounter++)
        {
            // get a handle to the bitmap of EMAIL VIEWER Background
            hHandle = this.video.GetVideoObject(guiEmailMessage);

            // place the graphic on the frame buffer
//            video.BltVideoObject(Surfaces.FRAME_BUFFER, hHandle, 1, VIEWER_X, iViewerPositionY + VIEWER_MESSAGE_BODY_START_Y + ((FontSubSystem.GetFontHeight(MESSAGE_FONT)) * (iCounter)), VO_BLT.SRCTRANSPARENCY, null);

        }


        // now the bottom piece to the message viewer
        hHandle = this.video.GetVideoObject(guiEmailMessage);

        if (giNumberOfPagesToCurrentEmail <= 2)
        {
            // place the graphic on the frame buffer
//            video.BltVideoObject(Surfaces.FRAME_BUFFER, hHandle, 2, VIEWER_X, iViewerPositionY + VIEWER_MESSAGE_BODY_START_Y + ((FontSubSystem.GetFontHeight(MESSAGE_FONT)) * (iCounter)), VO_BLT.SRCTRANSPARENCY, null);
        }
        else
        {
            // place the graphic on the frame buffer
//            video.BltVideoObject(Surfaces.FRAME_BUFFER, hHandle, 3, VIEWER_X, iViewerPositionY + VIEWER_MESSAGE_BODY_START_Y + ((FontSubSystem.GetFontHeight(MESSAGE_FONT)) * (iCounter)), VO_BLT.SRCTRANSPARENCY, null);
        }

        // reset iCounter and iHeight
        iCounter = 1;
        iHeight = FontSubSystem.GetFontHeight(MESSAGE_FONT);

        // draw body of text. Any particular email can encompass more than one "record" in the
        // email file. Draw each record (length is number of records)

        // now place the text

        // reset ptemprecord to head of list
        //        pTempRecord = pMessageRecordList;
        // reset shadow
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);

        pTempRecord = this.pEmailPageInfo[giMessagePage].pFirstRecord;

        if (pTempRecord is not null)
        {
            while (fDonePrintingMessage == false)
            {


                // copy over string 
                pString = wcscpy(pTempRecord.pRecord);

                // get the height of the string, ONLY!...must redisplay ON TOP OF background graphic
                //                iHeight += IanDisplayWrappedString(VIEWER_X + MESSAGE_X + 4, (int)(VIEWER_MESSAGE_BODY_START_Y + iHeight + iViewerPositionY), MESSAGE_WIDTH, MESSAGE_GAP, MESSAGE_FONT, MESSAGE_COLOR, pString, 0, false, IAN_WRAP_NO_SHADOW);

                // increment email record ptr
                pTempRecord = pTempRecord.Next;



                if (pTempRecord == null)
                {
                    fDonePrintingMessage = true;
                }
                else if ((pTempRecord == this.pEmailPageInfo[giMessagePage].pLastRecord) && (this.pEmailPageInfo[giMessagePage + 1].pFirstRecord != null))
                {
                    fDonePrintingMessage = true;
                }
            }
        }

        /*
        if(iTotalHeight < MAX_EMAIL_MESSAGE_PAGE_SIZE)
        {
            fOnLastPageFlag = true;
          while( pTempRecord )
            {
          // copy over string 
              wcscpy( pString, pTempRecord . pRecord );

            // get the height of the string, ONLY!...must redisplay ON TOP OF background graphic
            iHeight += IanDisplayWrappedString(VIEWER_X + MESSAGE_X + 4, ( int )( VIEWER_MESSAGE_BODY_START_Y + iHeight + iViewerPositionY), MESSAGE_WIDTH, MESSAGE_GAP, MESSAGE_FONT, MESSAGE_COLOR,pString,0,false, IAN_WRAP_NO_SHADOW);	  

                // increment email record ptr
              pTempRecord = pTempRecord . Next;
            }


        }
        else 
        {

            iYPositionOnPage = 0;
            // go to the right record
            pTempRecord = GetFirstRecordOnThisPage( pMessageRecordList, MESSAGE_FONT, MESSAGE_WIDTH, MESSAGE_GAP, giMessagePage, MAX_EMAIL_MESSAGE_PAGE_SIZE );
        while( pTempRecord )
            {
                // copy over string 
              wcscpy( pString, pTempRecord . pRecord );

                if( pString[ 0 ] == 0 )
                {
                    // on last page
                    fOnLastPageFlag = true;
                }


                if( ( iYPositionOnPage + IanWrappedStringHeight(0, 0, MESSAGE_WIDTH, MESSAGE_GAP, 
                                                                  MESSAGE_FONT, 0, pTempRecord.pRecord, 
                                                                 0, 0, 0 ) )  <= MAX_EMAIL_MESSAGE_PAGE_SIZE  )
                {
              // now print it
                iYPositionOnPage += IanDisplayWrappedString(VIEWER_X + MESSAGE_X + 4, ( int )( VIEWER_MESSAGE_BODY_START_Y + 10 +iYPositionOnPage + iViewerPositionY), MESSAGE_WIDTH, MESSAGE_GAP, MESSAGE_FONT, MESSAGE_COLOR,pString,0,false, IAN_WRAP_NO_SHADOW);	  
                    fGoingOffCurrentPage = false;
                }
                else
                {
                    // gonna get cut off...end now
                    fGoingOffCurrentPage = true;
                }


                pTempRecord = pTempRecord .Next;


                if( ( pTempRecord == null ) && ( fGoingOffCurrentPage == false ) )
                {
                    // on last page
                    fOnLastPageFlag = true;
                }
                else
                {
                    fOnLastPageFlag = false;
                }

                // record get cut off?...end now

                if( fGoingOffCurrentPage == true )
                {
                    pTempRecord = null;
                }
            }

        }

        */
        // show number of pages to this email
        this.DisplayNumberOfPagesToThisEmail(this.iViewerPositionY);

        // mark this area dirty
        this.video.InvalidateRegion(LAPTOP_SCREEN_UL_X, LAPTOP_SCREEN_UL_Y, LAPTOP_SCREEN_LR_X, LAPTOP_SCREEN_LR_Y);


        // reset shadow
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);


        return this.iViewerPositionY;
    }



    void BtnNewOkback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (!btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
        {
            return;
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            if (!btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
            }
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
                fNewMailFlag = false;

            }
        }
    }

    void AddDeleteRegionsToMessageRegion(int iViewerY)
    {
        // will create/destroy mouse region for message display

        if (this.fDisplayMessageFlag && (!this.fOldDisplayMessageFlag))
        {

            // set old flag
            this.fOldDisplayMessageFlag = true;


            // add X button
            giMessageButtonImage[0] = this.buttons.LoadButtonImage("LAPTOP\\X.sti", -1, 0, -1, 1, -1);
            giMessageButton[0] = ButtonSubSystem.QuickCreateButton(giMessageButtonImage[0], new(BUTTON_X + 2, (int)(BUTTON_Y + (int)iViewerY + 1)),
                ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1,
                (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)this.BtnMessageXCallback);
            ButtonSubSystem.SetButtonCursor(giMessageButton[0], CURSOR.LAPTOP_SCREEN);

            if (giNumberOfPagesToCurrentEmail > 2)
            {
                // add next and previous mail page buttons
                giMailMessageButtonsImage[0] = this.buttons.LoadButtonImage("LAPTOP\\NewMailButtons.sti", -1, 0, -1, 3, -1);
                giMailMessageButtons[0] = ButtonSubSystem.QuickCreateButton(giMailMessageButtonsImage[0], new(PREVIOUS_PAGE_BUTTON_X, (int)(LOWER_BUTTON_Y + (int)iViewerY + 2)),
                    ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1,
                    (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)this.BtnPreviousEmailPageCallback);

                giMailMessageButtonsImage[1] = this.buttons.LoadButtonImage("LAPTOP\\NewMailButtons.sti", -1, 1, -1, 4, -1);
                giMailMessageButtons[1] = ButtonSubSystem.QuickCreateButton(giMailMessageButtonsImage[1], new(NEXT_PAGE_BUTTON_X, (int)(LOWER_BUTTON_Y + (int)iViewerY + 2)),
                    ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1,
                    (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)this.BtnNextEmailPageCallback);

                gfPageButtonsWereCreated = true;
            }

            giMailMessageButtonsImage[2] = this.buttons.LoadButtonImage("LAPTOP\\NewMailButtons.sti", -1, 2, -1, 5, -1);
            giMailMessageButtons[2] = ButtonSubSystem.QuickCreateButton(giMailMessageButtonsImage[2], new(DELETE_BUTTON_X, (int)(BUTTON_LOWER_Y + (int)iViewerY + 2)),
                ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1,
                (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)this.BtnDeleteCallback);
            /*
                    // set up disable methods
                    SpecifyDisabledButtonStyle( giMailMessageButtons[1], DISABLED_STYLE_SHADED );
                SpecifyDisabledButtonStyle( giMailMessageButtons[0], DISABLED_STYLE_SHADED );
            */
            // set cursors
            ButtonSubSystem.SetButtonCursor(giMailMessageButtons[0], CURSOR.LAPTOP_SCREEN);
            ButtonSubSystem.SetButtonCursor(giMailMessageButtons[1], CURSOR.LAPTOP_SCREEN);
            ButtonSubSystem.SetButtonCursor(giMailMessageButtons[2], CURSOR.LAPTOP_SCREEN);
            ButtonSubSystem.SetButtonCursor(giMessageButton[0], CURSOR.LAPTOP_SCREEN);

            // force update of screen
            fReDrawScreenFlag = true;
        }
        else if ((!this.fDisplayMessageFlag) && this.fOldDisplayMessageFlag)
        {
            // delete region
            this.fOldDisplayMessageFlag = false;
            ButtonSubSystem.RemoveButton(giMessageButton[0]);
            ButtonSubSystem.UnloadButtonImage(giMessageButtonImage[0]);

            // net/previous email page buttons
            if (gfPageButtonsWereCreated)
            {
                ButtonSubSystem.RemoveButton(giMailMessageButtons[0]);
                ButtonSubSystem.UnloadButtonImage(giMailMessageButtonsImage[0]);
                ButtonSubSystem.RemoveButton(giMailMessageButtons[1]);
                ButtonSubSystem.UnloadButtonImage(giMailMessageButtonsImage[1]);
                gfPageButtonsWereCreated = false;
            }

            ButtonSubSystem.RemoveButton(giMailMessageButtons[2]);
            ButtonSubSystem.UnloadButtonImage(giMailMessageButtonsImage[2]);
            // force update of screen
            fReDrawScreenFlag = true;
        }

    }

    static bool fOldNewMailFlag = false;
    void CreateDestroyNewMailButton()
    {

        // check if we are video conferencing, if so, do nothing
        if (gubVideoConferencingMode != 0)
        {
            return;
        }


        if (fNewMailFlag && (!fOldNewMailFlag))
        {
            // create new mail dialog box button 

            // set old flag (stating button has been created)
            fOldNewMailFlag = true;

            // load image and setup button
            giNewMailButtonImage[0] = this.buttons.LoadButtonImage("LAPTOP\\YesNoButtons.sti", -1, 0, -1, 1, -1);
            giNewMailButton[0] = ButtonSubSystem.QuickCreateButton(giNewMailButtonImage[0], new(NEW_BTN_X + 10, NEW_BTN_Y),
                                                  ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 2,
                                                  (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)this.BtnNewOkback);

            // set cursor
            ButtonSubSystem.SetButtonCursor(giNewMailButton[0], CURSOR.LAPTOP_SCREEN);

            // set up screen mask region
            MouseSubSystem.MSYS_DefineRegion(
                this.pScreenMask,
                new(0, 0, 640, 480),
                MSYS_PRIORITY.HIGHEST - 3,
                CURSOR.LAPTOP_SCREEN,
                MSYS_NO_CALLBACK,
                Laptop.LapTopScreenCallBack);

            MouseSubSystem.MSYS_AddRegion(ref this.pScreenMask);
            ButtonSubSystem.MarkAButtonDirty(giNewMailButton[0]);
            fReDrawScreenFlag = true;
        }
        else if ((!fNewMailFlag) && fOldNewMailFlag)
        {


            // reset old flag
            fOldNewMailFlag = false;

            // remove the button
            ButtonSubSystem.RemoveButton(giNewMailButton[0]);
            ButtonSubSystem.UnloadButtonImage(giNewMailButtonImage[0]);

            // remove screen mask
            MouseSubSystem.MSYS_RemoveRegion(this.pScreenMask);


            //re draw screen
            this.fReDraw = true;

            // redraw screen
            this.fPausedReDrawScreenFlag = true;
        }
    }

    bool DisplayNewMailBox()
    {
        HVOBJECT hHandle;
        // will display a new mail box whenever new mail has arrived

        // check if we are video conferencing, if so, do nothing
        if (gubVideoConferencingMode != 0)
        {
            return false;
        }

        // just stopped displaying box, reset old flag
        if ((!fNewMailFlag) && fOldNewMailFlag)
        {
            fOldNewMailFlag = false;
            return false;
        }

        // not even set, leave NOW!
        if (!fNewMailFlag)
        {
            return false;
        }



        // is set but already drawn, LEAVE NOW!
        //if( ( fNewMailFlag ) && ( fOldNewMailFlag ) )
        //	return ( false );

        hHandle = this.video.GetVideoObject(guiEmailWarning);
//        video.BltVideoObject(Surfaces.FRAME_BUFFER, hHandle, 0, EMAIL_WARNING_X, EMAIL_WARNING_Y, VO_BLT.SRCTRANSPARENCY, null);


        // the icon for the title of this box
        hHandle = this.video.GetVideoObject(guiTITLEBARICONS);
//        video.BltVideoObject(Surfaces.FRAME_BUFFER, hHandle, 0, EMAIL_WARNING_X + 5, EMAIL_WARNING_Y + 2, VO_BLT.SRCTRANSPARENCY, null);

        // font stuff 
        FontSubSystem.SetFont(EMAIL_HEADER_FONT);
        FontSubSystem.SetFontForeground(FontColor.FONT_WHITE);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        // print warning
        //        mprintf(EMAIL_WARNING_X + 30, EMAIL_WARNING_Y + 8, pEmailTitleText[0]);

        // font stuff
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);
        FontSubSystem.SetFont(EMAIL_WARNING_FONT);
        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);

        // printf warning string
        mprintf(EMAIL_WARNING_X + 60, EMAIL_WARNING_Y + 63, pNewMailStrings[0]);
        Laptop.DrawLapTopIcons();

        // invalidate region
        this.video.InvalidateRegion(EMAIL_WARNING_X, EMAIL_WARNING_Y, EMAIL_WARNING_X + 270, EMAIL_WARNING_Y + 200);

        // mark button
        ButtonSubSystem.MarkAButtonDirty(giNewMailButton[0]);

        // reset shadow
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        // redraw icons

        // set box as displayed
        fOldNewMailFlag = true;



        // return
        return true;
    }

    void ReDrawNewMailBox()
    {

        // this function will check to see if the new mail region needs to be redrawn
        if (this.fReDrawNewMailFlag == true)
        {
            if (fNewMailFlag)
            {
                // set display flag back to orginal
                fNewMailFlag = false;

                // display new mail box
                this.DisplayNewMailBox();

                // dirty buttons
                ButtonSubSystem.MarkAButtonDirty(giNewMailButton[0]);

                // set display flag back to orginal
                fNewMailFlag = true;

                // time to redraw
                this.DisplayNewMailBox();
            }

            // return;

            // reset flag for redraw 
            this.fReDrawNewMailFlag = false;

            return;
        }
    }

    void SwitchEmailPages()
    {
        // this function will switch current page

        // gone too far, reset page to start
        if (iCurrentPage > iLastPage)
        {
            iCurrentPage = 0;
        }

        // set to last page
        if (iCurrentPage < 0)
        {
            iCurrentPage = iLastPage;
        }

        return;

    }


    void DetermineNextPrevPageDisplay()
    {
        // will determine which of previous and next page graphics to display



        if (iCurrentPage > 0)
        {
            // display Previous graphic

            // font stuff
            FontSubSystem.SetFont(TRAVERSE_EMAIL_FONT);
            FontSubSystem.SetFontForeground(FontColor.FONT_RED);
            FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);

            // print previous string
            mprintf(PREVIOUS_PAGE_X, PREVIOUS_PAGE_Y, pTraverseStrings[PREVIOUS_PAGE]);
        }

        // less than last page, so there is a next page
        if (iCurrentPage < iLastPage)
        {
            // display Next graphic

            // font stuff
            FontSubSystem.SetFont(TRAVERSE_EMAIL_FONT);
            FontSubSystem.SetFontForeground(FontColor.FONT_RED);
            FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);

            // next string
            mprintf(NEXT_PAGE_X, NEXT_PAGE_Y, pTraverseStrings[NEXT_PAGE]);
        }
    }

    void NextRegionButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {

        if (!btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
        {
            return;
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            if (!btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
            }
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;

                // not on last page, move ahead one
                if (iCurrentPage < iLastPage)
                {
                    iCurrentPage++;
                    this.fReDraw = true;
                    this.RenderEmail();
                    ButtonSubSystem.MarkButtonsDirty(buttonList);
                }
            }
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            // nothing yet
        }

    }

    void BtnPreviousEmailPageCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (!btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
        {
            return;
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            if (!btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
            }
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                if (giMessagePage > 0)
                {
                    giMessagePage--;
                }

                btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;

                this.fReDraw = true;
                this.RenderEmail();
                ButtonSubSystem.MarkButtonsDirty(buttonList);
            }
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            // nothing yet
        }

    }

    void BtnNextEmailPageCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (!btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
        {
            return;
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            if (!btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
            }
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            // not on last page, move ahead one
            btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;

            if ((giNumberOfPagesToCurrentEmail - 1) <= giMessagePage)
            {
                return;
            }

            if (!this.fOnLastPageFlag)
            {
                if ((giNumberOfPagesToCurrentEmail - 1) > (giMessagePage + 1))
                {
                    giMessagePage++;
                }
            }

            ButtonSubSystem.MarkButtonsDirty(buttonList);
            fReDrawScreenFlag = true;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            // nothing yet
        }

    }

    void PreviousRegionButtonCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (!btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
        {
            return;
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            if (!btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
            }
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
                // if we are not on forst page, more back one
                if (iCurrentPage > 0)
                {
                    iCurrentPage--;
                    this.fReDraw = true;
                    this.RenderEmail();
                    ButtonSubSystem.MarkButtonsDirty(buttonList);
                }
            }
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            // nothing yet
        }

    }


    void BtnDeleteNoback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (!btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
        {
            return;
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            if (!btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
            }
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
                this.fDeleteMailFlag = false;
                fReDrawScreenFlag = true;
            }
        }
    }


    void BtnDeleteYesback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (!btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED))
        {
            return;
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            if (!btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
            }
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            if (btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
            {
                btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
                fReDrawScreenFlag = true;
                this.DeleteEmail();

            }
        }
    }


    static bool fCreated = false;
    void CreateDestroyNextPreviousRegions()
    {
        if (fCreated)
        {
            // destroy already create next, previous mouse regions
            fCreated = false;

            ButtonSubSystem.RemoveButton(giMailPageButtons[1]);
            ButtonSubSystem.UnloadButtonImage(giMailPageButtonsImage[1]);
            ButtonSubSystem.RemoveButton(giMailPageButtons[0]);
            ButtonSubSystem.UnloadButtonImage(giMailPageButtonsImage[0]);
        }
        else
        {
            // create uncreated mouse regions
            fCreated = true;

            this.CreateNextPreviousEmailPageButtons();

            /*
            // ' next' region
        MSYS_DefineRegion(&pEmailMoveRegions[NEXT_BUTTON],NEXT_PAGE_X, NEXT_PAGE_Y,(int) (NEXT_PAGE_X+NEXT_WIDTH), (int)(NEXT_PAGE_Y+NEXT_HEIGHT),
                MSYS_PRIORITY.NORMAL+2,MSYS_NO_CURSOR, MSYS_NO_CALLBACK, NextRegionButtonCallback);

            // ' previous ' region
          MSYS_DefineRegion(&pEmailMoveRegions[PREVIOUS_BUTTON],PREVIOUS_PAGE_X,PREVIOUS_PAGE_Y, (int)(PREVIOUS_PAGE_X+PREVIOUS_WIDTH),(int)(PREVIOUS_PAGE_Y+PREVIOUS_HEIGHT),
                MSYS_PRIORITY.NORMAL+2,MSYS_NO_CURSOR, MSYS_NO_CALLBACK, PreviousRegionButtonCallback );

            // add regions
            MSYS_AddRegion(&pEmailMoveRegions[PREVIOUS_BUTTON]);
          MSYS_AddRegion(&pEmailMoveRegions[NEXT_BUTTON]);
            */
        }
    }

    void ReDraw()
    {
        // forces update of entire laptop screen
        if (this.fReDraw)
        {
            Laptop.RenderLaptop();
            //EnterNewLaptopMode();
            //            DrawLapTopText();
            //            ReDrawHighLight();
            ButtonSubSystem.MarkButtonsDirty(buttonList);
            this.fReDraw = false;
        }

    }

    static bool fOldDeleteMailFlag = false;
    private bool fReDrawNewMailFlag;
    private bool fFirstTime;
    private readonly IVideoManager video;
    private readonly FontSubSystem fonts;
    private readonly ButtonSubSystem buttons;
    private readonly IFileManager files;
    private readonly IEnumerable<GUI_BUTTON> buttonList;

    void CreateDestroyDeleteNoticeMailButton()
    {
        if (this.fDeleteMailFlag && (!fOldDeleteMailFlag))
        {
            // confirm delete email buttons

            // YES button
            fOldDeleteMailFlag = true;
            giDeleteMailButtonImage[0] = this.buttons.LoadButtonImage("LAPTOP\\YesNoButtons.sti", -1, 0, -1, 1, -1);
            giDeleteMailButton[0] = ButtonSubSystem.QuickCreateButton(giDeleteMailButtonImage[0], new(NEW_BTN_X + 1, NEW_BTN_Y),
                ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 2,
                (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)this.BtnDeleteYesback);

            // NO button
            giDeleteMailButtonImage[1] = this.buttons.LoadButtonImage("LAPTOP\\YesNoButtons.sti", -1, 2, -1, 3, -1);
            giDeleteMailButton[1] = ButtonSubSystem.QuickCreateButton(giDeleteMailButtonImage[1], new(NEW_BTN_X + 40, NEW_BTN_Y),
                ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 2,
                (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)this.BtnDeleteNoback);

            // set up cursors
            ButtonSubSystem.SetButtonCursor(giDeleteMailButton[0], CURSOR.LAPTOP_SCREEN);
            ButtonSubSystem.SetButtonCursor(giDeleteMailButton[1], CURSOR.LAPTOP_SCREEN);

            // set up screen mask to prevent other actions while delete mail box is destroyed
            //            MouseSubSystem.MSYS_DefineRegion(pDeleteScreenMask, 0, 0, 640, 480,
            //                MSYS_PRIORITY.HIGHEST - 3, CURSOR.LAPTOP_SCREEN, MSYS_NO_CALLBACK, LapTopScreenCallBack);
            MouseSubSystem.MSYS_AddRegion(ref this.pDeleteScreenMask);

            // force update
            fReDrawScreenFlag = true;

        }
        else if ((!this.fDeleteMailFlag) && fOldDeleteMailFlag)
        {

            // clear out the buttons and screen mask
            fOldDeleteMailFlag = false;
            ButtonSubSystem.RemoveButton(giDeleteMailButton[0]);
            ButtonSubSystem.UnloadButtonImage(giDeleteMailButtonImage[0]);
            ButtonSubSystem.RemoveButton(giDeleteMailButton[1]);
            ButtonSubSystem.UnloadButtonImage(giDeleteMailButtonImage[1]);

            // the region
            MouseSubSystem.MSYS_RemoveRegion(this.pDeleteScreenMask);

            // force refresh
            fReDrawScreenFlag = true;

        }
        return;
    }
    bool DisplayDeleteNotice(email pMail)
    {


        HVOBJECT hHandle;
        // will display a delete mail box whenever delete mail has arrived
        if (!this.fDeleteMailFlag)
        {
            return false;
        }

        if (!fReDrawScreenFlag)
        {
            // no redraw flag, leave
            return false;
        }

        // error check.. no valid message passed
        if (pMail == null)
        {
            return false;
        }


        // load graphics

        hHandle = this.video.GetVideoObject(guiEmailWarning);
//        video.BltVideoObject(Surfaces.FRAME_BUFFER, hHandle, 0, EMAIL_WARNING_X, EMAIL_WARNING_Y, VO_BLT.SRCTRANSPARENCY, null);


        // font stuff 
        FontSubSystem.SetFont(EMAIL_HEADER_FONT);
        FontSubSystem.SetFontForeground(FontColor.FONT_WHITE);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        // the icon for the title of this box
        hHandle = this.video.GetVideoObject(guiTITLEBARICONS);
//        video.BltVideoObject(Surfaces.FRAME_BUFFER, hHandle, 0, EMAIL_WARNING_X + 5, EMAIL_WARNING_Y + 2, VO_BLT.SRCTRANSPARENCY, null);

        // title 
        //        mprintf(EMAIL_WARNING_X + 30, EMAIL_WARNING_Y + 8, pEmailTitleText[0]);

        // shadow, font, and foreground
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);
        FontSubSystem.SetFont(EMAIL_WARNING_FONT);
        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);

        // draw text based on mail being read or not
        if (pMail.fRead)
        {
            //            mprintf(EMAIL_WARNING_X + 95, EMAIL_WARNING_Y + 65, pDeleteMailStrings[0]);
        }
        else
        {
            //            mprintf(EMAIL_WARNING_X + 70, EMAIL_WARNING_Y + 65, pDeleteMailStrings[1]);
        }


        // invalidate screen area, for refresh

        if (!fNewMailFlag)
        {
            // draw buttons
            ButtonSubSystem.MarkButtonsDirty(buttonList);
            this.video.InvalidateRegion(EMAIL_WARNING_X, EMAIL_WARNING_Y, EMAIL_WARNING_X + EMAIL_WARNING_WIDTH, EMAIL_WARNING_Y + EMAIL_WARNING_HEIGHT);
        }

        // reset font shadow
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        return true;
    }

    void DeleteEmail()
    {

        // error check, invalid mail, or not time to delete mail
        if (this.fDeleteInternal != true)
        {
            if ((this.iDeleteId == -1) || (!this.fDeleteMailFlag))
            {
                return;
            }
        }
        // remove the message
        this.RemoveEmailMessage(this.iDeleteId);

        // stop displaying message, if so
        this.fDisplayMessageFlag = false;

        // upadte list
        this.PlaceMessagesinPages();

        // redraw icons (if deleted message was last unread, remove checkmark)
        Laptop.DrawLapTopIcons();

        // if all of a sudden we are beyond last page, move back one
        if (iCurrentPage > iLastPage)
        {
            iCurrentPage = iLastPage;
        }

        // rerender mail list
        this.RenderEmail();

        // nolong time to delete mail
        this.fDeleteMailFlag = false;
        fReDrawScreenFlag = true;
        // refresh screen (get rid of dialog box image)
        //ReDraw();

        // invalidate
        this.video.InvalidateRegion(0, 0, 640, 480);
    }

    void FromCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
        {
            return;
        }
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {

            // sort messages based on sender name, then replace into pages of email
            this.fSortSenderUpwards = !this.fSortSenderUpwards;

            this.SortMessages(EmailFields.SENDER);

            //SpecifyButtonIcon( giSortButton[1] , giArrowsForEmail, int usVideoObjectIndex,  int bXOffset, int bYOffset, true );

            this.fJustStartedEmail = false;

            this.PlaceMessagesinPages();
            btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
        }

        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            // nothing yet
        }

    }

    void SubjectCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
        {
            return;
        }
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            // sort message on subject and reorder list
            this.fSortSubjectUpwards = !this.fSortSubjectUpwards;

            this.SortMessages(EmailFields.SUBJECT);
            this.fJustStartedEmail = false;
            this.PlaceMessagesinPages();



            btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            // nothing yet
        }

    }


    void BtnDeleteCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
        {
            return;
        }
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {

            btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
            this.iDeleteId = giMessageId;
            this.fDeleteMailFlag = true;

        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            // nothing yet
        }

    }

    void DateCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
        {
            return;
        }
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            // sort messages based on date recieved and reorder lsit
            this.fSortDateUpwards = !this.fSortDateUpwards;
            this.SortMessages(EmailFields.RECEIVED);
            this.PlaceMessagesinPages();

            this.fJustStartedEmail = false;

            btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            // nothing yet
        }

    }


    void ReadCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
        {
            return;
        }
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            // sort messages based on date recieved and reorder lsit
            this.SortMessages(EmailFields.READ);
            this.PlaceMessagesinPages();

            this.fJustStartedEmail = false;

            btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.RBUTTON_UP))
        {
            // nothing yet
        }

    }


    void SetUpSortRegions()
    {

        // have been replaced by buttons
        return;

        // will set up sort mail regions

        // from region
        /*
          MSYS_DefineRegion(&pSortMailRegions[0],FROM_BOX_X ,FROM_BOX_Y, FROM_BOX_X+FROM_BOX_WIDTH ,FROM_BOX_Y+TOP_HEIGHT,
                  MSYS_PRIORITY.NORMAL+2,MSYS_NO_CURSOR,MSYS_NO_CALLBACK, FromCallback );

          // subject region
          MSYS_DefineRegion(&pSortMailRegions[1],SUBJECT_X ,FROM_BOX_Y, SUBJECT_BOX_X+SUBJECT_WIDTH ,FROM_BOX_Y+TOP_HEIGHT,
                  MSYS_PRIORITY.NORMAL+2,MSYS_NO_CURSOR,MSYS_NO_CALLBACK, SubjectCallback );

          // date region
          MSYS_DefineRegion(&pSortMailRegions[2],DATE_X ,FROM_BOX_Y, DATE_BOX_X+DATE_WIDTH ,FROM_BOX_Y+TOP_HEIGHT,
                  MSYS_PRIORITY.NORMAL+2,MSYS_NO_CURSOR,MSYS_NO_CALLBACK, DateCallback );

          //add regions
          MSYS_AddRegion(&pSortMailRegions[0]);
        MSYS_AddRegion(&pSortMailRegions[1]);
        MSYS_AddRegion(&pSortMailRegions[2]);

          return;
          */
    }

    void DeleteSortRegions()
    {

        // have been replaced by buttons
        return;
        /*
        MSYS_RemoveRegion(&pSortMailRegions[0]);
        MSYS_RemoveRegion(&pSortMailRegions[1]);
        MSYS_RemoveRegion(&pSortMailRegions[2]);
        */
    }



    void DisplayTextOnTitleBar()
    {
        // draw email screen title text

        // font stuff
        FontSubSystem.SetFont(EMAIL_TITLE_FONT);
        FontSubSystem.SetFontForeground(FontColor.FONT_WHITE);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);

        // printf the title
        //        mprintf(EMAIL_TITLE_X, EMAIL_TITLE_Y, pEmailTitleText[0]);

        // reset the shadow

    }

    void DestroyMailScreenButtons()
    {
        // this function will destory the buttons used in the email screen

        // the sort email buttons
        ButtonSubSystem.RemoveButton(giSortButton[0]);
        ButtonSubSystem.UnloadButtonImage(giSortButtonImage[0]);
        ButtonSubSystem.RemoveButton(giSortButton[1]);
        ButtonSubSystem.UnloadButtonImage(giSortButtonImage[1]);
        ButtonSubSystem.RemoveButton(giSortButton[2]);
        ButtonSubSystem.UnloadButtonImage(giSortButtonImage[2]);
        ButtonSubSystem.RemoveButton(giSortButton[3]);
        ButtonSubSystem.UnloadButtonImage(giSortButtonImage[3]);

        return;
    }

    void CreateMailScreenButtons()
    {

        // create sort buttons, right now - not finished

        // read sort
        giSortButtonImage[0] = this.buttons.LoadButtonImage("LAPTOP\\mailbuttons.sti", -1, 0, -1, 4, -1);
        giSortButton[0] = ButtonSubSystem.QuickCreateButton(giSortButtonImage[0], new(ENVELOPE_BOX_X, FROM_BOX_Y),
                                            ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1,
                                            (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)this.ReadCallback);
        ButtonSubSystem.SetButtonCursor(giSortButton[0], CURSOR.LAPTOP_SCREEN);


        // subject sort
        giSortButtonImage[1] = this.buttons.LoadButtonImage("LAPTOP\\mailbuttons.sti", -1, 1, -1, 5, -1);
        giSortButton[1] = ButtonSubSystem.QuickCreateButton(giSortButtonImage[1], new(FROM_BOX_X, FROM_BOX_Y),
                                            ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1,
                                            (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)this.FromCallback);
        ButtonSubSystem.SetButtonCursor(giSortButton[1], CURSOR.LAPTOP_SCREEN);
        //        SpecifyFullButtonTextAttributes(giSortButton[1], pEmailHeaders[FROM_HEADER], EMAIL_WARNING_FONT,
        //                                                                               FontColor.FONT_BLACK, FontColor.FONT_BLACK,
        //                                                                                 FontColor.FONT_BLACK, FontColor.FONT_BLACK, TEXT_CJUSTIFIED);


        // sender sort
        giSortButtonImage[2] = this.buttons.LoadButtonImage("LAPTOP\\mailbuttons.sti", -1, 2, -1, 6, -1);
        giSortButton[2] = ButtonSubSystem.QuickCreateButton(giSortButtonImage[2], new(SUBJECT_BOX_X, FROM_BOX_Y),
                                            ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1,
                                            (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)this.SubjectCallback);
        ButtonSubSystem.SetButtonCursor(giSortButton[2], CURSOR.LAPTOP_SCREEN);
        //        SpecifyFullButtonTextAttributes(giSortButton[2], pEmailHeaders[SUBJECT_HEADER], EMAIL_WARNING_FONT,
        //                                                                              FontColor.FONT_BLACK, FontColor.FONT_BLACK,
        //                                                                                FontColor.FONT_BLACK, FontColor.FONT_BLACK, TEXT_CJUSTIFIED);



        // date sort
        giSortButtonImage[3] = this.buttons.LoadButtonImage("LAPTOP\\mailbuttons.sti", -1, 3, -1, 7, -1);
        giSortButton[3] = ButtonSubSystem.QuickCreateButton(giSortButtonImage[3], new(DATE_BOX_X, FROM_BOX_Y),
                                            ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1,
                                            (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)this.DateCallback);
        ButtonSubSystem.SetButtonCursor(giSortButton[3], CURSOR.LAPTOP_SCREEN);
        //        SpecifyFullButtonTextAttributes(giSortButton[3], pEmailHeaders[RECD_HEADER], EMAIL_WARNING_FONT,
        //                                                                              FontColor.FONT_BLACK, FontColor.FONT_BLACK,
        //                                                                                FontColor.FONT_BLACK, FontColor.FONT_BLACK, TEXT_CJUSTIFIED);
        return;
    }


    void DisplayEmailMessageSubjectDateFromLines(email pMail, int iViewerY)
    {
        // this procedure will draw the title/headers to From, Subject, Date fields in the display
        // message box
        int usX, usY;
        string sString;

        // font stuff	
        FontSubSystem.SetFont(MESSAGE_FONT);
        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);

        // all headers, but not info are right justified 

        // print from
        //        FontSubSystem.FindFontRightCoordinates(MESSAGE_HEADER_X - 20, (int)(MESSAGE_FROM_Y + (int)iViewerY), MESSAGE_HEADER_WIDTH, (int)(MESSAGE_FROM_Y + FontSubSystem.GetFontHeight(MESSAGE_FONT)), pEmailHeaders[0], MESSAGE_FONT, out usX, out usY);
        //        mprintf(usX, MESSAGE_FROM_Y + (int)iViewerY, pEmailHeaders[0]);

        // the actual from info
        //        mprintf(MESSAGE_HEADER_X + MESSAGE_HEADER_WIDTH - 13, MESSAGE_FROM_Y + iViewerY, pSenderNameList[pMail.ubSender]);


        // print date
        //        FontSubSystem.FindFontRightCoordinates(MESSAGE_HEADER_X + 168, (int)(MESSAGE_DATE_Y + (int)iViewerY), MESSAGE_HEADER_WIDTH, (int)(MESSAGE_DATE_Y + FontSubSystem.GetFontHeight(MESSAGE_FONT)), pEmailHeaders[2], MESSAGE_FONT, out usX, out usY);
        //        mprintf(usX, MESSAGE_DATE_Y + (int)iViewerY, pEmailHeaders[2]);

        // the actual date info
        sString = wprintf("%d", pMail.iDate / (24 * 60));
        mprintf(MESSAGE_HEADER_X + 235, MESSAGE_DATE_Y + (int)iViewerY, sString);



        // print subject
        //        FontSubSystem.FindFontRightCoordinates(MESSAGE_HEADER_X - 20, MESSAGE_SUBJECT_Y, MESSAGE_HEADER_WIDTH, (int)(MESSAGE_SUBJECT_Y + FontSubSystem.GetFontHeight(MESSAGE_FONT)), pEmailHeaders[1], MESSAGE_FONT, out usX, out usY);
        //        mprintf(usX, MESSAGE_SUBJECT_Y + (int)iViewerY, pEmailHeaders[1]);

        // the actual subject info
        //mprintf( , MESSAGE_SUBJECT_Y, pMail.pSubject);
        //        IanDisplayWrappedString(SUBJECT_LINE_X + 2, (int)(SUBJECT_LINE_Y + 2 + (int)iViewerY), SUBJECT_LINE_WIDTH, MESSAGE_GAP, MESSAGE_FONT, MESSAGE_COLOR, pMail.pSubject, 0, false, 0);


        // reset shadow
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
        return;
    }


    void DrawEmailMessageDisplayTitleText(int iViewerY)
    {
        // this procedure will display the title of the email message display box

        // font stuff
        FontSubSystem.SetFont(EMAIL_HEADER_FONT);
        FontSubSystem.SetFontForeground(FontColor.FONT_WHITE);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);

        // dsiplay mail viewer title on message viewer
        //        mprintf(VIEWER_X + 30, VIEWER_Y + 8 + (int)iViewerY, pEmailTitleText[0]);

        return;
    }

    void DrawLineDividers()
    {
        // this function draws divider lines between lines of text
        int iCounter = 0;
        HVOBJECT hHandle;

        for (iCounter = 1; iCounter < 19; iCounter++)
        {
            hHandle = this.video.GetVideoObject(guiMAILDIVIDER);
//            video.BltVideoObject(Surfaces.FRAME_BUFFER, hHandle, 0, INDIC_X - 10, (MIDDLE_Y + iCounter * MIDDLE_WIDTH - 1), VO_BLT.SRCTRANSPARENCY, null);
        }


        return;
    }


    void ClearOutEmailMessageRecordsList()
    {
        RecordPtr pTempRecord;
        int iCounter = 0;

        // runt hrough list freeing records up
        //        while (pMessageRecordList)
        //        {
        //            // set temp to current
        //            pTempRecord = pMessageRecordList;
        //
        //            // next element
        //            pMessageRecordList = pMessageRecordList.Next;
        //
        //            MemFree(pTempRecord);
        //        }

        for (iCounter = 0; iCounter < MAX_NUMBER_EMAIL_PAGES; iCounter++)
        {
            this.pEmailPageInfo[iCounter].pFirstRecord = null;
            this.pEmailPageInfo[iCounter].pLastRecord = null;
            this.pEmailPageInfo[iCounter].iPageNumber = iCounter;
        }

        // null out list
        //        pMessageRecordList = null;

        return;
    }

    void AddEmailRecordToList(string pString)
    {
        RecordPtr? pTempRecord = null;

        // set to head of list
        //        pTempRecord = pMessageRecordList;

        if (pTempRecord is null)
        {

            // list empty, set this node to head
            //            pTempRecord = MemAlloc(sizeof(Record));
            //            pMessageRecordList = pTempRecord;
        }
        else
        {
            // run to end of list
            //            while (pTempRecord.Next)
            //            {
            //                pTempRecord = pTempRecord.Next;
            //            }

            // found, alloc
            //            pTempRecord.Next = MemAlloc(sizeof(Record));

            // move to node
            pTempRecord = pTempRecord.Next;
        }

        // set next to null
        pTempRecord.Next = null;

        // copy in string
        pTempRecord.pRecord = wcscpy(pString);

        // done return

        return;

    }


    void UpDateMessageRecordList()
    {

        // simply checks to see if old and new message ids are the same, if so, do nothing
        // otherwise clear list

        if (giMessageId != giPrevMessageId)
        {
            // if chenged, clear list
            this.ClearOutEmailMessageRecordsList();

            // set prev to current
            giPrevMessageId = giMessageId;
        }

    }

    void HandleAnySpecialEmailMessageEvents(int iMessageId)
    {

        // handles any special message events

        switch (iMessageId)
        {

            case IMP_EMAIL_AGAIN:
                Laptop.SetBookMark(BOOKMARK.IMP_BOOKMARK);
                break;
            case IMP_EMAIL_INTRO:
                Laptop.SetBookMark(BOOKMARK.IMP_BOOKMARK);
                break;
        }
    }

    void ReDisplayBoxes()
    {




        // the email message itself
        if (this.fDisplayMessageFlag)
        {
            // this simply redraws message with button manipulation
            this.DisplayEmailMessage(this.GetEmailMessage(giMessageId));
        }

        if (this.fDeleteMailFlag)
        {
            // delete message, redisplay
            this.DisplayDeleteNotice(this.GetEmailMessage(this.iDeleteId));
        }

        if (fNewMailFlag)
        {
            // if new mail, redisplay box
            this.DisplayNewMailBox();
        }
    }


    bool HandleMailSpecialMessages(int usMessageId, int? iResults, email pMail)
    {
        bool fSpecialCase = false;

        // this procedure will handle special cases of email messages that are not stored in email.edt, or need special processing
        switch (usMessageId)
        {
            case IMP_EMAIL_PROFILE_RESULTS:

                this.HandleIMPCharProfileResultsMessage();
                fSpecialCase = true;

                break;
            case MERC_INTRO:
                Laptop.SetBookMark(BOOKMARK.MERC_BOOKMARK);
                fReDrawScreenFlag = true;
                break;


            case INSUR_PAYMENT:
            case INSUR_SUSPIC:
            case INSUR_SUSPIC_2:
            case INSUR_INVEST_OVER:
                this.ModifyInsuranceEmails(usMessageId, iResults, pMail, INSUR_PAYMENT_LENGTH);
                break;

            case INSUR_1HOUR_FRAUD:
                this.ModifyInsuranceEmails(usMessageId, iResults, pMail, INSUR_1HOUR_FRAUD_LENGTH);
                break;

            case MERC_NEW_SITE_ADDRESS:
                //Set the book mark so the player can access the site
                Laptop.SetBookMark(BOOKMARK.MERC_BOOKMARK);
                break;

            case MERC_DIED_ON_OTHER_ASSIGNMENT:
                this.ModifyInsuranceEmails(usMessageId, iResults, pMail, MERC_DIED_ON_OTHER_ASSIGNMENT_LENGTH);
                break;

            case AIM_MEDICAL_DEPOSIT_REFUND:
            case AIM_MEDICAL_DEPOSIT_NO_REFUND:
            case AIM_MEDICAL_DEPOSIT_PARTIAL_REFUND:
                this.ModifyInsuranceEmails(usMessageId, iResults, pMail, AIM_MEDICAL_DEPOSIT_REFUND_LENGTH);
                break;
        }

        return fSpecialCase;
    }





    void HandleIMPCharProfileResultsMessage()
    {
        // special case, IMP profile return
        int iTotalHeight = 0;
        int iCnt = 0;
        int iHeight = 0;
        int iCounter = 0;
        //	string pString[MAIL_STRING_SIZE/2 + 1];
        string pString;
        int iOffSet = 0;
        int iViewerY = 0;
        int iHeightTemp = 0;
        int iHeightSoFar = 0;
        RecordPtr? pTempRecord = null;
        int iEndOfSection = 0;
        int iRand = 0;
        bool fSufficientMechSkill = false, fSufficientMarkSkill = false, fSufficientMedSkill = false, fSufficientExplSkill = false;
        bool fSufficientHlth = false, fSufficientStr = false, fSufficientWis = false, fSufficientAgi = false, fSufficientDex = false, fSufficientLdr = false;

        iRand = Globals.Random.Next(32767);

        // set record ptr to head of list
        //        pTempRecord = pMessageRecordList;

        // increment height for size of one line
        iHeight += FontSubSystem.GetFontHeight(MESSAGE_FONT);

        // load intro
        iEndOfSection = IMP_RESULTS_INTRO_LENGTH;

        // list doesn't exist, reload
        if (pTempRecord is null)
        {

            while (iEndOfSection > iCounter)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, (uint)(MAIL_STRING_SIZE * (iOffSet + iCounter)), MAIL_STRING_SIZE);

                // have to place players name into string for first record
                if (iCounter == 0)
                {
                    string zTemp = string.Empty;

                    wprintf(zTemp, " %s", gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].zName);
                    wcscat(pString, zTemp);
                }

                // add to list
                this.AddEmailRecordToList(pString);

                // increment email record counter
                iCounter++;
            }

            // now the personality intro
            iOffSet = IMP_RESULTS_PERSONALITY_INTRO;
            iEndOfSection = IMP_RESULTS_PERSONALITY_INTRO_LENGTH + 1;
            iCounter = 0;

            while (iEndOfSection > iCounter)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, (uint)(MAIL_STRING_SIZE * (iOffSet + iCounter)), MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);

                // increment email record counter
                iCounter++;
            }

            // personality itself
            switch (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bPersonalityTrait)
            {
                // normal as can be
                case PersonalityTrait.NO_PERSONALITYTRAIT:
                    iOffSet = IMP_PERSONALITY_NORMAL;
                    break;
                case PersonalityTrait.HEAT_INTOLERANT:
                    iOffSet = IMP_PERSONALITY_HEAT;
                    break;
                case PersonalityTrait.NERVOUS:
                    iOffSet = IMP_PERSONALITY_NERVOUS;
                    break;
                case PersonalityTrait.CLAUSTROPHOBIC:
                    iOffSet = IMP_PERSONALITY_CLAUSTROPHOBIC;
                    break;
                case PersonalityTrait.NONSWIMMER:
                    iOffSet = IMP_PERSONALITY_NONSWIMMER;
                    break;
                case PersonalityTrait.FEAR_OF_INSECTS:
                    iOffSet = IMP_PERSONALITY_FEAR_OF_INSECTS;
                    break;
                case PersonalityTrait.FORGETFUL:
                    iOffSet = IMP_PERSONALITY_FORGETFUL;
                    break;
                case PersonalityTrait.PSYCHO:
                    iOffSet = IMP_PERSONALITY_PSYCHO;
                    break;
            }

            // personality tick
            //  DEF: removed 1/12/99, cause it was changing the length of email that were already calculated
            //		LoadEncryptedDataFromFile( "BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * ( iOffSet + Globals.Random.Next( IMP_PERSONALITY_LENGTH - 1 ) + 1 ), MAIL_STRING_SIZE );
            this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, (uint)(MAIL_STRING_SIZE * (iOffSet + 1)), MAIL_STRING_SIZE);
            // add to list
            this.AddEmailRecordToList(pString);

            // persoanlity paragraph
            this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, (uint)(MAIL_STRING_SIZE * (iOffSet + IMP_PERSONALITY_LENGTH)), MAIL_STRING_SIZE);
            // add to list
            this.AddEmailRecordToList(pString);

            // extra paragraph for bugs
            if (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bPersonalityTrait == PersonalityTrait.FEAR_OF_INSECTS)
            {
                // persoanlity paragraph
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, (uint)(MAIL_STRING_SIZE * (iOffSet + IMP_PERSONALITY_LENGTH + 1)), MAIL_STRING_SIZE);
                // add to list
                this.AddEmailRecordToList(pString);
            }

            // attitude intro
            // now the personality intro
            iOffSet = IMP_RESULTS_ATTITUDE_INTRO;
            iEndOfSection = IMP_RESULTS_ATTITUDE_LENGTH;
            iCounter = 0;

            while (iEndOfSection > iCounter)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, (uint)(MAIL_STRING_SIZE * (iOffSet + iCounter)), MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);

                // increment email record counter
                iCounter++;
            }

            // personality itself
            switch (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bAttitude)
            {
                // normal as can be
                case ATT.NORMAL:
                    iOffSet = IMP_ATTITUDE_NORMAL;
                    break;
                case ATT.FRIENDLY:
                    iOffSet = IMP_ATTITUDE_FRIENDLY;
                    break;
                case ATT.LONER:
                    iOffSet = IMP_ATTITUDE_LONER;
                    break;
                case ATT.OPTIMIST:
                    iOffSet = IMP_ATTITUDE_OPTIMIST;
                    break;
                case ATT.PESSIMIST:
                    iOffSet = IMP_ATTITUDE_PESSIMIST;
                    break;
                case ATT.AGGRESSIVE:
                    iOffSet = IMP_ATTITUDE_AGGRESSIVE;
                    break;
                case ATT.ARROGANT:
                    iOffSet = IMP_ATTITUDE_ARROGANT;
                    break;
                case ATT.ASSHOLE:
                    iOffSet = IMP_ATTITUDE_ASSHOLE;
                    break;
                case ATT.COWARD:
                    iOffSet = IMP_ATTITUDE_COWARD;
                    break;

            }

            // attitude title
            this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, (uint)(MAIL_STRING_SIZE * iOffSet), MAIL_STRING_SIZE);
            // add to list
            this.AddEmailRecordToList(pString);


            // attitude tick
            //  DEF: removed 1/12/99, cause it was changing the length of email that were already calculated
            //		LoadEncryptedDataFromFile( "BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * ( iOffSet + Globals.Random.Next( IMP_ATTITUDE_LENGTH - 2 ) + 1 ), MAIL_STRING_SIZE );
            this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, (uint)(MAIL_STRING_SIZE * (iOffSet + 1)), MAIL_STRING_SIZE);
            // add to list
            this.AddEmailRecordToList(pString);

            // attitude paragraph
            this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, (uint)(MAIL_STRING_SIZE * (iOffSet + IMP_ATTITUDE_LENGTH - 1)), MAIL_STRING_SIZE);
            // add to list
            this.AddEmailRecordToList(pString);

            //check for second paragraph
            if (iOffSet != IMP_ATTITUDE_NORMAL)
            {
                // attitude paragraph
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, (uint)(MAIL_STRING_SIZE * (iOffSet + IMP_ATTITUDE_LENGTH)), MAIL_STRING_SIZE);
                // add to list
                this.AddEmailRecordToList(pString);
            }


            // skills
            // now the skills intro
            iOffSet = IMP_RESULTS_SKILLS;
            iEndOfSection = IMP_RESULTS_SKILLS_LENGTH;
            iCounter = 0;

            while (iEndOfSection > iCounter)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, (uint)(MAIL_STRING_SIZE * (iOffSet + iCounter)), MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);

                // increment email record counter
                iCounter++;
            }

            // imperial skills
            iOffSet = IMP_SKILLS_IMPERIAL_SKILLS;
            iEndOfSection = 0;
            iCounter = 0;

            // marksmanship
            if (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bMarksmanship >= SUPER_SKILL_VALUE)
            {
                fSufficientMarkSkill = true;
                iEndOfSection = 1;
            }

            // medical
            if (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bMedical >= SUPER_SKILL_VALUE)
            {
                fSufficientMedSkill = true;
                iEndOfSection = 1;
            }

            // mechanical
            if (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bMechanical >= SUPER_SKILL_VALUE)
            {
                fSufficientMechSkill = true;
                iEndOfSection = 1;
            }

            if (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bExplosive >= SUPER_SKILL_VALUE)
            {
                fSufficientExplSkill = true;
                iEndOfSection = 1;
            }

            while (iEndOfSection > iCounter)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, (uint)(MAIL_STRING_SIZE * (iOffSet + iCounter)), MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);

                // increment email record counter
                iCounter++;
            }

            // now handle skills
            if (fSufficientMarkSkill)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_IMPERIAL_MARK, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }


            if (fSufficientMedSkill)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_IMPERIAL_MED, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if (fSufficientMechSkill)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_IMPERIAL_MECH, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            // explosives	
            if (fSufficientExplSkill)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_IMPERIAL_EXPL, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            fSufficientMechSkill = false;
            fSufficientMarkSkill = false;
            fSufficientExplSkill = false;
            fSufficientMedSkill = false;

            // imperial skills
            iOffSet = IMP_SKILLS_NEED_TRAIN_SKILLS;
            iEndOfSection = 0;
            iCounter = 0;



            // now the needs training values
            //            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bMarksmanship > NO_CHANCE_IN_HELL_SKILL_VALUE) && (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bMarksmanship <= NEEDS_TRAINING_SKILL_VALUE))
            //            {
            //                fSufficientMarkSkill = true;
            //                iEndOfSection = 1;
            //            }
            //
            //            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bMedical > NO_CHANCE_IN_HELL_SKILL_VALUE) && (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bMedical <= NEEDS_TRAINING_SKILL_VALUE))
            //            {
            //                fSufficientMedSkill = true;
            //                iEndOfSection = 1;
            //            }
            //
            //            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bMechanical > NO_CHANCE_IN_HELL_SKILL_VALUE) && (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bMechanical <= NEEDS_TRAINING_SKILL_VALUE))
            //            {
            //                fSufficientMechSkill = true;
            //                iEndOfSection = 1;
            //            }
            //
            //            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bExplosive > NO_CHANCE_IN_HELL_SKILL_VALUE) && (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bExplosive <= NEEDS_TRAINING_SKILL_VALUE))
            //            {
            //                fSufficientExplSkill = true;
            //                iEndOfSection = 1;
            //            }

            while (iEndOfSection > iCounter)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, (uint)(MAIL_STRING_SIZE * (iOffSet + iCounter)), MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);

                // increment email record counter
                iCounter++;
            }

            if (fSufficientMarkSkill)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_NEED_TRAIN_MARK, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if (fSufficientMedSkill)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_NEED_TRAIN_MED, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if (fSufficientMechSkill)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_NEED_TRAIN_MECH, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if (fSufficientExplSkill)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_NEED_TRAIN_EXPL, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            fSufficientMechSkill = false;
            fSufficientMarkSkill = false;
            fSufficientExplSkill = false;
            fSufficientMedSkill = false;

            // and the no chance in hell of doing anything useful values

            // no skill
            iOffSet = IMP_SKILLS_NO_SKILL;
            iEndOfSection = 0;
            iCounter = 0;

            if (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bMarksmanship <= NO_CHANCE_IN_HELL_SKILL_VALUE)
            {
                fSufficientMarkSkill = true;
                iEndOfSection = 1;
            }

            if (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bMedical <= NO_CHANCE_IN_HELL_SKILL_VALUE)
            {
                fSufficientMedSkill = true;
                iEndOfSection = 1;
            }

            if (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bMechanical <= NO_CHANCE_IN_HELL_SKILL_VALUE)
            {
                fSufficientMechSkill = true;
                iEndOfSection = 1;
            }

            if (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bExplosive <= NO_CHANCE_IN_HELL_SKILL_VALUE)
            {
                fSufficientExplSkill = true;
                iEndOfSection = 1;
            }

            while (iEndOfSection > iCounter)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, (uint)(MAIL_STRING_SIZE * (iOffSet + iCounter)), MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);

                // increment email record counter
                iCounter++;
            }

            if (fSufficientMechSkill)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_NO_SKILL_MECH, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if (fSufficientMarkSkill)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_NO_SKILL_MARK, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if (fSufficientMedSkill)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_NO_SKILL_MED, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }
            if (fSufficientExplSkill)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_NO_SKILL_EXPL, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            // now the specialized skills
            // imperial skills
            iOffSet = IMP_SKILLS_SPECIAL_INTRO;
            iEndOfSection = IMP_SKILLS_SPECIAL_INTRO_LENGTH;
            iCounter = 0;

            while (iEndOfSection > iCounter)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, (uint)(MAIL_STRING_SIZE * (iOffSet + iCounter)), MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);

                // increment email record counter
                iCounter++;
            }

            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait == SkillTrait.KNIFING) || (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait2 == SkillTrait.KNIFING))
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_SPECIAL_KNIFE, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            // lockpick     
            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait == SkillTrait.LOCKPICKING) || (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait2 == SkillTrait.LOCKPICKING))
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_SPECIAL_LOCK, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            // hand to hand
            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait == SkillTrait.HANDTOHAND) || (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait2 == SkillTrait.HANDTOHAND))
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_SPECIAL_HAND, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            // electronics
            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait == SkillTrait.ELECTRONICS) || (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait2 == SkillTrait.ELECTRONICS))
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_SPECIAL_ELEC, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait == SkillTrait.NIGHTOPS) || (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait2 == SkillTrait.NIGHTOPS))
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_SPECIAL_NIGHT, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait == SkillTrait.THROWING) || (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait2 == SkillTrait.THROWING))
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_SPECIAL_THROW, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait == SkillTrait.TEACHING) || (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait2 == SkillTrait.TEACHING))
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_SPECIAL_TEACH, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait == SkillTrait.HEAVY_WEAPS) || (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait2 == SkillTrait.HEAVY_WEAPS))
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_SPECIAL_HEAVY, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait == SkillTrait.AUTO_WEAPS) || (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait2 == SkillTrait.AUTO_WEAPS))
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_SPECIAL_AUTO, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait == SkillTrait.STEALTHY) || (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait2 == SkillTrait.STEALTHY))
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_SPECIAL_STEALTH, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait == SkillTrait.AMBIDEXT) || (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait2 == SkillTrait.AMBIDEXT))
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_SPECIAL_AMBI, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait == SkillTrait.THIEF) || (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait2 == SkillTrait.THIEF))
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_SPECIAL_THIEF, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait == SkillTrait.MARTIALARTS) || (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bSkillTrait2 == SkillTrait.MARTIALARTS))
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_SKILLS_SPECIAL_MARTIAL, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }


            // now the physical
            // imperial physical
            iOffSet = IMP_RESULTS_PHYSICAL;
            iEndOfSection = IMP_RESULTS_PHYSICAL_LENGTH;
            iCounter = 0;

            while (iEndOfSection > iCounter)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, (uint)(MAIL_STRING_SIZE * (iOffSet + iCounter)), MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);

                // increment email record counter
                iCounter++;
            }

            // super physical
            iOffSet = IMP_PHYSICAL_SUPER;
            iEndOfSection = 0;
            iCounter = 0;


            // health
            if (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bLife >= SUPER_STAT_VALUE)
            {
                fSufficientHlth = true;
                iEndOfSection = 1;
            }

            // dex
            if (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bDexterity >= SUPER_STAT_VALUE)
            {
                fSufficientDex = true;
                iEndOfSection = 1;
            }

            // agility
            if (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bAgility >= SUPER_STAT_VALUE)
            {
                fSufficientAgi = true;
                iEndOfSection = 1;
            }

            // strength
            if (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bStrength >= SUPER_STAT_VALUE)
            {
                fSufficientStr = true;
                iEndOfSection = 1;
            }

            // wisdom
            if (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bWisdom >= SUPER_STAT_VALUE)
            {
                fSufficientWis = true;
                iEndOfSection = 1;
            }

            // leadership
            if (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bLeadership >= SUPER_STAT_VALUE)
            {
                fSufficientLdr = true;
                iEndOfSection = 1;
            }

            while (iEndOfSection > iCounter)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, (uint)(MAIL_STRING_SIZE * (iOffSet + iCounter)), MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);

                // increment email record counter
                iCounter++;
            }

            if (fSufficientHlth)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_PHYSICAL_SUPER_HEALTH, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }


            if (fSufficientDex)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_PHYSICAL_SUPER_DEXTERITY, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if (fSufficientStr)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_PHYSICAL_SUPER_STRENGTH, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if (fSufficientAgi)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_PHYSICAL_SUPER_AGILITY, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if (fSufficientWis)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_PHYSICAL_SUPER_WISDOM, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if (fSufficientLdr)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_PHYSICAL_SUPER_LEADERSHIP, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            fSufficientHlth = false;
            fSufficientStr = false;
            fSufficientWis = false;
            fSufficientAgi = false;
            fSufficientDex = false;
            fSufficientLdr = false;

            // now the low attributes
            // super physical
            iOffSet = IMP_PHYSICAL_LOW;
            iEndOfSection = 0;
            iCounter = 0;

            // health
            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bLife < NEEDS_TRAINING_STAT_VALUE) && (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bLife > NO_CHANCE_IN_HELL_STAT_VALUE))
            {
                fSufficientHlth = true;
                iEndOfSection = 1;
            }

            // strength
            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bStrength < NEEDS_TRAINING_STAT_VALUE) && (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bStrength > NO_CHANCE_IN_HELL_STAT_VALUE))
            {
                fSufficientStr = true;
                iEndOfSection = 1;
            }

            // agility
            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bAgility < NEEDS_TRAINING_STAT_VALUE) && (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bAgility <= NO_CHANCE_IN_HELL_STAT_VALUE))
            {
                fSufficientAgi = true;
                iEndOfSection = 1;
            }

            // wisdom
            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bWisdom < NEEDS_TRAINING_STAT_VALUE) && (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bWisdom > NO_CHANCE_IN_HELL_STAT_VALUE))
            {
                fSufficientWis = true;
                iEndOfSection = 1;
            }

            // leadership
            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bLeadership < NEEDS_TRAINING_STAT_VALUE) && (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bLeadership > NO_CHANCE_IN_HELL_STAT_VALUE))
            {
                fSufficientLdr = true;
                iEndOfSection = 1;
            }

            // dex
            if ((gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bDexterity < NEEDS_TRAINING_STAT_VALUE) && (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bDexterity > NO_CHANCE_IN_HELL_STAT_VALUE))
            {
                fSufficientDex = true;
                iEndOfSection = 1;
            }

            while (iEndOfSection > iCounter)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, (uint)(MAIL_STRING_SIZE * (iOffSet + iCounter)), MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);

                // increment email record counter
                iCounter++;
            }

            if (fSufficientHlth)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_PHYSICAL_LOW_HEALTH, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }


            if (fSufficientDex)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_PHYSICAL_LOW_DEXTERITY, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if (fSufficientStr)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_PHYSICAL_LOW_STRENGTH, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }



            if (fSufficientAgi)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_PHYSICAL_LOW_AGILITY, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if (fSufficientWis)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_PHYSICAL_LOW_WISDOM, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if (fSufficientLdr)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_PHYSICAL_LOW_LEADERSHIP, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }









            // very low physical
            iOffSet = IMP_PHYSICAL_VERY_LOW;
            iEndOfSection = 0;
            iCounter = 0;

            fSufficientHlth = false;
            fSufficientStr = false;
            fSufficientWis = false;
            fSufficientAgi = false;
            fSufficientDex = false;
            fSufficientLdr = false;

            // health
            if (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bLife <= NO_CHANCE_IN_HELL_STAT_VALUE)
            {
                fSufficientHlth = true;
                iEndOfSection = 1;
            }

            // dex
            if (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bDexterity <= NO_CHANCE_IN_HELL_STAT_VALUE)
            {
                fSufficientDex = true;
                iEndOfSection = 1;
            }

            // strength
            if (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bStrength <= NO_CHANCE_IN_HELL_STAT_VALUE)
            {
                fSufficientStr = true;
                iEndOfSection = 1;
            }

            // agility
            if (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bAgility <= NO_CHANCE_IN_HELL_STAT_VALUE)
            {
                fSufficientAgi = true;
                iEndOfSection = 1;
            }

            // wisdom
            if (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bWisdom <= NO_CHANCE_IN_HELL_STAT_VALUE)
            {
                fSufficientWis = true;
                iEndOfSection = 1;
            }

            while (iEndOfSection > iCounter)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, (uint)(MAIL_STRING_SIZE * (iOffSet + iCounter)), MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);

                // increment email record counter
                iCounter++;
            }

            if (fSufficientHlth)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_PHYSICAL_VERY_LOW_HEALTH, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }



            if (fSufficientDex)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_PHYSICAL_VERY_LOW_DEXTERITY, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            if (fSufficientStr)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_PHYSICAL_VERY_LOW_STRENGTH, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }



            if (fSufficientAgi)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_PHYSICAL_VERY_LOW_AGILITY, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }



            if (fSufficientWis)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_PHYSICAL_VERY_LOW_WISDOM, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }


            // leadership
            if (gMercProfiles[PLAYER_GENERATED_CHARACTER_ID + LaptopSaveInfo.iVoiceId].bLeadership <= NO_CHANCE_IN_HELL_STAT_VALUE)
            {
                fSufficientLdr = true;
            }

            if (fSufficientLdr)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, MAIL_STRING_SIZE * IMP_PHYSICAL_VERY_LOW_LEADERSHIP, MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);
            }

            // very low physical
            iOffSet = IMP_RESULTS_PORTRAIT;
            iEndOfSection = IMP_RESULTS_PORTRAIT_LENGTH;
            iCounter = 0;

            while (iEndOfSection > iCounter)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, (uint)(MAIL_STRING_SIZE * (iOffSet + iCounter)), MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);

                // increment email record counter
                iCounter++;
            }


            // portraits

            //            switch (iPortraitNumber)
            //            {
            //                case (0):
            //                    iOffSet = IMP_PORTRAIT_MALE_1;
            //                    break;
            //                case (1):
            //                    iOffSet = IMP_PORTRAIT_MALE_2;
            //                    break;
            //                case (2):
            //                    iOffSet = IMP_PORTRAIT_MALE_3;
            //                    break;
            //                case (3):
            //                    iOffSet = IMP_PORTRAIT_MALE_4;
            //                    break;
            //                case (4):
            //                case (5):
            //                    iOffSet = IMP_PORTRAIT_MALE_5;
            //                    break;
            //                case (6):
            //                case (7):
            //                    iOffSet = IMP_PORTRAIT_MALE_6;
            //                    break;
            //                case (8):
            //                    iOffSet = IMP_PORTRAIT_FEMALE_1;
            //                    break;
            //                case (9):
            //                    iOffSet = IMP_PORTRAIT_FEMALE_2;
            //                    break;
            //                case (10):
            //                    iOffSet = IMP_PORTRAIT_FEMALE_3;
            //                    break;
            //                case (11):
            //                case (12):
            //                    iOffSet = IMP_PORTRAIT_FEMALE_4;
            //                    break;
            //                case (13):
            //                case (14):
            //                    iOffSet = IMP_PORTRAIT_FEMALE_5;
            //                    break;
            //            }

            if ((iRand % 2) == 0)
            {
                iOffSet += 2;
            }

            iEndOfSection = 2;
            iCounter = 0;

            while (iEndOfSection > iCounter)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, (uint)(MAIL_STRING_SIZE * (iOffSet + iCounter)), MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);

                // increment email record counter
                iCounter++;
            }

            iOffSet = IMP_RESULTS_END;
            iEndOfSection = IMP_RESULTS_END_LENGTH;
            iCounter = 0;

            while (iEndOfSection > iCounter)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Impass.edt", out pString, (uint)(MAIL_STRING_SIZE * (iOffSet + iCounter)), MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);

                // increment email record counter
                iCounter++;
            }

            giPrevMessageId = giMessageId;

        }

        //        pTempRecord = pMessageRecordList;


    }

    void HandleEmailViewerButtonStates()
    {
        // handle state of email viewer buttons

        if (this.fDisplayMessageFlag == false)
        {
            // not displaying message, leave
            return;
        }



        if (giNumberOfPagesToCurrentEmail <= 2)
        {
            return;
        }

        // turn off previous page button
        if (giMessagePage == 0)
        {
            ButtonSubSystem.DisableButton(giMailMessageButtons[0]);
        }
        else
        {
            ButtonSubSystem.EnableButton(giMailMessageButtons[0]);
        }


        // turn off next page button
        if (this.pEmailPageInfo[giMessagePage + 1].pFirstRecord == null)
        {
            ButtonSubSystem.DisableButton(giMailMessageButtons[1]);
        }
        else
        {
            ButtonSubSystem.EnableButton(giMailMessageButtons[1]);
        }

        return;

    }


    void SetUpIconForButton()
    {
        // if we just got in, return, don't set any

        if (this.fJustStartedEmail == true)
        {
            return;
        }




        return;
    }


    void DeleteCurrentMessage()
    {
        // will delete the currently displayed message

        // set current message to be deleted
        this.iDeleteId = giMessageId;

        // set the currently displayed message to none
        giMessageId = -1;

        // reset display message flag
        this.fDisplayMessageFlag = false;

        // reset page being displayed
        giMessagePage = 0;

        this.fDeleteInternal = true;

        // delete message
        this.DeleteEmail();

        this.fDeleteInternal = false;

        // force update of entire screen
        fReDrawScreenFlag = true;

        // rerender email
        this.RenderEmail();

        return;
    }


    void CreateNextPreviousEmailPageButtons()
    {

        // this function will create the buttons to advance and go back email pages

        // next button
        giMailPageButtonsImage[0] = this.buttons.LoadButtonImage("LAPTOP\\NewMailButtons.sti", -1, 1, -1, 4, -1);
        giMailPageButtons[0] = ButtonSubSystem.QuickCreateButton(giMailPageButtonsImage[0], new(NEXT_PAGE_X, NEXT_PAGE_Y),
                                            ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1,
                                            (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)this.NextRegionButtonCallback);
        ButtonSubSystem.SetButtonCursor(giMailPageButtons[0], CURSOR.LAPTOP_SCREEN);

        // previous button
        giMailPageButtonsImage[1] = this.buttons.LoadButtonImage("LAPTOP\\NewMailButtons.sti", -1, 0, -1, 3, -1);
        giMailPageButtons[1] = ButtonSubSystem.QuickCreateButton(giMailPageButtonsImage[1], new(PREVIOUS_PAGE_X, NEXT_PAGE_Y),
                                        ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1,
                                        (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)this.PreviousRegionButtonCallback);
        ButtonSubSystem.SetButtonCursor(giMailPageButtons[1], CURSOR.LAPTOP_SCREEN);

        /*
        // set up disable methods
      SpecifyDisabledButtonStyle( giMailPageButtons[1], DISABLED_STYLE_SHADED );
      SpecifyDisabledButtonStyle( giMailPageButtons[0], DISABLED_STYLE_SHADED );
    */

        return;
    }


    void UpdateStatusOfNextPreviousButtons()
    {

        // set the states of the page advance buttons

        ButtonSubSystem.DisableButton(giMailPageButtons[0]);
        ButtonSubSystem.DisableButton(giMailPageButtons[1]);

        if (iCurrentPage > 0)
        {
            ButtonSubSystem.EnableButton(giMailPageButtons[1]);
        }

        if (iCurrentPage < iLastPage)
        {
            ButtonSubSystem.EnableButton(giMailPageButtons[0]);
        }
    }


    void DisplayWhichPageOfEmailProgramIsDisplayed()
    {
        // will draw the number of the email program we are viewing right now
        string sString = string.Empty;

        // font stuff	
        FontSubSystem.SetFont(MESSAGE_FONT);
        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);

        // page number
        if (iLastPage < 0)
        {
            wprintf(sString, "%d / %d", 1, 1);
        }
        else
        {
            wprintf(sString, "%d / %d", iCurrentPage + 1, iLastPage + 1);
        }

        // print it
        mprintf(PAGE_NUMBER_X, PAGE_NUMBER_Y, sString);

        // restore shadow
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        return;
    }

    void OpenMostRecentUnreadEmail()
    {
        // will open the most recent email the player has recieved and not read
        int iMostRecentMailId = -1;
        email? pB = pEmailList;
        int iLowestDate = 9999999;

        while (pB is not null)
        {
            // if date is lesser and unread , swap
            if ((pB.iDate < iLowestDate) && (pB.fRead == false))
            {
                iMostRecentMailId = pB.iId;
                iLowestDate = (int)pB.iDate;
            }

            // next in B's list
            pB = pB.Next;
        }

        // set up id
        giMessageId = iMostRecentMailId;

        // valid message, show it
        if (giMessageId != -1)
        {
            this.fDisplayMessageFlag = true;
        }

        return;
    }


    bool DisplayNumberOfPagesToThisEmail(int iViewerY)
    {
        // display the indent for the display of pages to this email..along with the current page/number of pages


        int iCounter = 0;
        string sString = string.Empty;


        // get and blt the email list background
        // load, blt and delete graphics
        //VObjectDesc.fCreateFlags=VOBJECT_CREATE_FROMFILE;
        //	FilenameForBPP( "LAPTOP\\mailindent.sti", VObjectDesc.ImageFile );
        //CHECKF( AddVideoObject( &VObjectDesc, &uiMailIndent ) );
        // GetVideoObject( &hHandle, uiMailIndent );
        // BltVideoObject( FRAME_BUFFER, hHandle, 0,VIEWER_X + INDENT_X_OFFSET, VIEWER_Y + iViewerY + INDENT_Y_OFFSET - 10, VO_BLT.SRCTRANSPARENCY,null );
        // DeleteVideoObjectFromIndex( uiMailIndent );

        giNumberOfPagesToCurrentEmail = giNumberOfPagesToCurrentEmail;

        // parse current page and max number of pages to email
        wprintf(sString, "%d / %d", giMessagePage + 1, giNumberOfPagesToCurrentEmail - 1);

        FontSubSystem.SetFont(FontStyle.FONT12ARIAL);
        FontSubSystem.SetFontForeground(FontColor.FONT_BLACK);
        FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);

        // turn off the shadows
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);

        FontSubSystem.SetFontDestBuffer(SurfaceType.FRAME_BUFFER, 0, 0, 640, 480, false);

        this.fonts.FindFontCenterCoordinates(VIEWER_X + INDENT_X_OFFSET, 0, INDENT_X_WIDTH, 0, sString, FontStyle.FONT12ARIAL, out int sX, out int sY);
        mprintf(sX, VIEWER_Y + iViewerY + INDENT_Y_OFFSET - 2, sString);


        // restore shadows
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        return true;
    }


    int GetNumberOfPagesToEmail()
    {
        RecordPtr? pTempRecord = null;
        int iNumberOfPagesToEmail = 0;


        // set temp record to head of list
        //        pTempRecord = pMessageRecordList;

        // run through messages, and find out how many 
        while (pTempRecord is not null)
        {
            //            pTempRecord = GetFirstRecordOnThisPage(pMessageRecordList, MESSAGE_FONT, MESSAGE_WIDTH, MESSAGE_GAP, iNumberOfPagesToEmail, MAX_EMAIL_MESSAGE_PAGE_SIZE);
            iNumberOfPagesToEmail++;
        }


        return iNumberOfPagesToEmail;
    }


    public static void ShutDownEmailList()
    {
        email? pEmail = pEmailList;
        email? pTempEmail = null;

        //loop through all the emails to delete them
        while (pEmail is not null)
        {
            pTempEmail = pEmail;

            pEmail = pEmail.Next;

            MemFree(pTempEmail.pSubject);
            pTempEmail.pSubject = null;

            MemFree(pTempEmail);
            pTempEmail = null;
        }
        pEmailList = null;

        ClearPages();
    }

    void PreProcessEmail(email pMail)
    {
        RecordPtr? pTempRecord = null, pCurrentRecord = null, pLastRecord, pTempList;
        string pString;
        int iCounter = 0, iHeight = 0, iOffSet = 0;
        bool fGoingOffCurrentPage = false;
        int iYPositionOnPage = 0;

        iOffSet = (int)pMail.usOffset;

        // set record ptr to head of list
        //        pTempRecord = pMessageRecordList;

        if (this.pEmailPageInfo[0].pFirstRecord != null)
        {
            // already processed
            return;
        }

        // list doesn't exist, reload
        if (pTempRecord is null)
        {
            while (pMail.usLength > iCounter)
            {
                // read one record from email file
                this.files.LoadEncryptedDataFromFile("BINARYDATA\\Email.edt", out pString, (uint)(MAIL_STRING_SIZE * (iOffSet + iCounter)), MAIL_STRING_SIZE);

                // add to list
                this.AddEmailRecordToList(pString);

                // increment email record counter
                iCounter++;
            }
            giPrevMessageId = giMessageId;

        }

        // set record ptr to head of list
        //        pTempRecord = pMessageRecordList;
        //def removed
        // pass the subject line
        if (pTempRecord is not null && pMail.usOffset != IMP_EMAIL_PROFILE_RESULTS)
        {
            pTempRecord = pTempRecord.Next;
        }

        // get number of pages to this email
        giNumberOfPagesToCurrentEmail = this.GetNumberOfPagesToEmail();


        while (pTempRecord is not null)
        {

            // copy over string
            pString = wcscpy(pTempRecord.pRecord);

            // get the height of the string, ONLY!...must redisplay ON TOP OF background graphic
            //            iHeight += IanWrappedStringHeight(VIEWER_X + MESSAGE_X + 4, (int)(VIEWER_MESSAGE_BODY_START_Y + iHeight + FontSubSystem.GetFontHeight(MESSAGE_FONT)), MESSAGE_WIDTH, MESSAGE_GAP, MESSAGE_FONT, MESSAGE_COLOR, pString, 0, false, 0);

            // next message record string
            pTempRecord = pTempRecord.Next;

        }

        // set iViewerY so to center the viewer
        this.iViewerPositionY = (LAPTOP_SCREEN_LR_Y - 2 * VIEWER_Y - 2 * VIEWER_MESSAGE_BODY_START_Y - iHeight) / 2;

        if (this.iViewerPositionY < 0)
        {
            this.iViewerPositionY = 0;
        }

        // set total height to height of records displayed
        //        iTotalHeight = iHeight;

        // if the message background is less than MIN_MESSAGE_HEIGHT_IN_LINES, set to that number
        //        if ((iTotalHeight / FontSubSystem.GetFontHeight(MESSAGE_FONT)) < MIN_MESSAGE_HEIGHT_IN_LINES)
        //        {
        //            iTotalHeight = FontSubSystem.GetFontHeight(MESSAGE_FONT) * MIN_MESSAGE_HEIGHT_IN_LINES;
        //        }

        //        if (iTotalHeight > MAX_EMAIL_MESSAGE_PAGE_SIZE)
        //        {
        //            // if message to big to fit on page
        //            iTotalHeight = MAX_EMAIL_MESSAGE_PAGE_SIZE + 10;
        //        }
        //        else
        //        {
        //            iTotalHeight += 10;
        //        }
        //
        //        pTempRecord = pMessageRecordList;
        //
        //        if (iTotalHeight < MAX_EMAIL_MESSAGE_PAGE_SIZE)
        //        {
        //            fOnLastPageFlag = true;
        //
        //            if (pTempRecord is not null && pMail.usOffset != IMP_EMAIL_PROFILE_RESULTS)
        //            {
        //                pTempRecord = pTempRecord.Next;
        //            }
        //
        //            /*
        //            //Def removed
        //                    if( pTempRecord )
        //                    {
        //                        pTempRecord = pTempRecord.Next;
        //                    }
        //            */
        //
        //            pEmailPageInfo[0].pFirstRecord = pTempRecord;
        //            pEmailPageInfo[0].iPageNumber = 0;
        //
        //
        //            Debug.Assert(pTempRecord is not null);        // required, otherwise we're testing pCurrentRecord when undefined later
        //
        //            while (pTempRecord is not null)
        //            {
        //                pCurrentRecord = pTempRecord;
        //
        //                // increment email record ptr
        //                pTempRecord = pTempRecord.Next;
        //
        //            }
        //
        //            // only one record to this email?..then set next to null
        //            if (pCurrentRecord == pEmailPageInfo[0].pFirstRecord)
        //            {
        //                pCurrentRecord = null;
        //            }
        //
        //            // set up the last record for the page
        //            pEmailPageInfo[0].pLastRecord = pCurrentRecord;
        //
        //            // now set up the next page
        //            pEmailPageInfo[1].pFirstRecord = null;
        //            pEmailPageInfo[1].pLastRecord = null;
        //            pEmailPageInfo[1].iPageNumber = 1;
        //        }
        //        else
        //        {
        //            fOnLastPageFlag = false;
        //            pTempList = pMessageRecordList;
        //
        //            if (pTempList is not null && pMail.usOffset != IMP_EMAIL_PROFILE_RESULTS)
        //            {
        //                pTempList = pTempList.Next;
        //            }
        //
        //            /*
        //            //def removed
        //                    // skip the subject
        //                    if( pTempList )
        //                    {
        //                        pTempList = pTempList.Next;
        //                    }
        //
        //            */
        //            iCounter = 0;
        //
        //            // more than one page
        //            //for( iCounter = 0; iCounter < giNumberOfPagesToCurrentEmail; iCounter++ )
        //            while (pTempRecord = GetFirstRecordOnThisPage(pTempList, MESSAGE_FONT, MESSAGE_WIDTH, MESSAGE_GAP, iCounter, MAX_EMAIL_MESSAGE_PAGE_SIZE))
        //            {
        //                iYPositionOnPage = 0;
        //
        //                pEmailPageInfo[iCounter].pFirstRecord = pTempRecord;
        //                pEmailPageInfo[iCounter].iPageNumber = iCounter;
        //                pLastRecord = null;
        //
        //                // go to the right record
        //                while (pTempRecord is not null)
        //                {
        //                    // copy over string 
        //                    pString = wcscpy(pTempRecord.pRecord);
        //
        //                    if (pString[0] == 0)
        //                    {
        //                        // on last page
        //                        fOnLastPageFlag = true;
        //                    }
        //
        //
        ////                    if ((iYPositionOnPage + IanWrappedStringHeight(0, 0, MESSAGE_WIDTH, MESSAGE_GAP,
        ////                                                                        MESSAGE_FONT, 0, pTempRecord.pRecord,
        ////                                                                     0, 0, 0)) <= MAX_EMAIL_MESSAGE_PAGE_SIZE)
        ////                    {
        ////                        // now print it
        ////                        iYPositionOnPage += IanWrappedStringHeight(VIEWER_X + MESSAGE_X + 4, (int)(VIEWER_MESSAGE_BODY_START_Y + 10 + iYPositionOnPage + iViewerPositionY), MESSAGE_WIDTH, MESSAGE_GAP, MESSAGE_FONT, MESSAGE_COLOR, pString, 0, false, IAN_WRAP_NO_SHADOW);
        ////                        fGoingOffCurrentPage = false;
        ////                    }
        ////                    else
        ////                    {
        ////                        // gonna get cut off...end now
        ////                        fGoingOffCurrentPage = true;
        ////                    }
        //
        //
        //
        //                    pCurrentRecord = pTempRecord;
        //                    pTempRecord = pTempRecord.Next;
        //
        //                    if (fGoingOffCurrentPage == false)
        //                    {
        //                        pLastRecord = pTempRecord;
        //                    }
        //                    // record get cut off?...end now
        //
        //                    if (fGoingOffCurrentPage == true)
        //                    {
        //                        pTempRecord = null;
        //                    }
        //                }
        //
        //                if (pLastRecord == pEmailPageInfo[iCounter].pFirstRecord)
        //                {
        //                    pLastRecord = null;
        //                }
        //
        //                pEmailPageInfo[iCounter].pLastRecord = pLastRecord;
        //                iCounter++;
        //            }
        //
        //            pEmailPageInfo[iCounter].pFirstRecord = null;
        //            pEmailPageInfo[iCounter].pLastRecord = null;
        //            pEmailPageInfo[iCounter].iPageNumber = iCounter;
        //        }
    }

    void ModifyInsuranceEmails(int usMessageId, int? iResults, email pMail, int ubNumberOfRecords)
    {
        int iHeight = 0;
        RecordPtr pTempRecord;
        //	string pString[MAIL_STRING_SIZE/2 + 1];
        int ubCnt;


        // Replace the name in the subject line
        //	wprintf( pMail.pSubject, gMercProfiles[ pMail.ubFirstData ].zNickname );

        // set record ptr to head of list
        //        pTempRecord = pMessageRecordList;

        // increment height for size of one line
        iHeight += FontSubSystem.GetFontHeight(MESSAGE_FONT);

        for (ubCnt = 0; ubCnt < ubNumberOfRecords; ubCnt++)
        {
            // read one record from email file
            this.files.LoadEncryptedDataFromFile("BINARYDATA\\Email.edt", out string pString, (uint)(MAIL_STRING_SIZE * usMessageId), MAIL_STRING_SIZE);

            //Replace the $MERCNAME$ and $AMOUNT$ with the mercs name and the amountm if the string contains the keywords.
            ReplaceMercNameAndAmountWithProperData(pString, pMail);

            // add to list
            this.AddEmailRecordToList(pString);

            usMessageId++;
        }


        //
        giPrevMessageId = giMessageId;
    }

    private static bool ReplaceMercNameAndAmountWithProperData(string? pFinishedString, email pMail)
    {
        //	string		pTempString[MAIL_STRING_SIZE/2 + 1];
        string pTempString = string.Empty;
        int iLength = 0;
        int iCurLocInSourceString = 0;
        int iLengthOfSourceString = wcslen(pFinishedString);      //Get the length of the source string
        string? pMercNameString = null;
        string? pAmountString = null;
        string? pSubString = null;
        bool fReplacingMercName = true;

        string sMercName = "$MERCNAME$";   //Doesnt need to be translated, inside Email.txt and will be replaced by the mercs name
        string sAmount = "$AMOUN$";        //Doesnt need to be translated, inside Email.txt and will be replaced by a dollar amount
        string sSearchString = string.Empty;

        //Copy the original string over to the temp string
        pTempString = wcscpy(pFinishedString);

        //Null out the string
        //pFinishedString[0] = '\0';


        //Keep looping through to replace all references to the keyword
        while (iCurLocInSourceString < iLengthOfSourceString)
        {
            iLength = 0;
            pSubString = null;

            //Find out if the $MERCNAME$ is in the string
            pMercNameString = Globals.wcsstr(pTempString[iCurLocInSourceString], sMercName);

            pAmountString = wcsstr(pTempString[iCurLocInSourceString], sAmount);

            if (pMercNameString != null && pAmountString != null)
            {
                //                if (pMercNameString < pAmountString)
                //                {
                //                    fReplacingMercName = true;
                //                    pSubString = pMercNameString;
                //                    wcscpy(sSearchString, sMercName);
                //                }
                //                else
                //                {
                //                    fReplacingMercName = false;
                //                    pSubString = pAmountString;
                //                    wcscpy(sSearchString, sAmount);
                //                }
            }
            else if (pMercNameString != null)
            {
                fReplacingMercName = true;
                pSubString = pMercNameString;
                sSearchString = wcscpy(sMercName);
            }
            else if (pAmountString != null)
            {
                fReplacingMercName = false;
                pSubString = pAmountString;
                sSearchString = wcscpy(sAmount);
            }
            else
            {
                pSubString = null;
            }


            // if there is a substring
            if (pSubString != null)
            {
                //                iLength = pSubString - pTempString[iCurLocInSourceString];

                //Copy the part of the source string upto the keyword
                Globals.wcsncat(pFinishedString, pTempString[iCurLocInSourceString], iLength);

                //increment the source string counter by how far in the keyword is and by the length of the keyword
                iCurLocInSourceString += iLength + wcslen(sSearchString);

                if (fReplacingMercName)
                {
                    //add the mercs name to the string
                    wcscat(pFinishedString, gMercProfiles[(NPCID)pMail.uiSecondData].zName);
                }
                else
                {
                    string sDollarAmount;

                    sDollarAmount = wprintf("%d", pMail.iFirstData);

                    //                    InsertCommasForDollarFigure(sDollarAmount);
                    //                    InsertDollarSignInToString(sDollarAmount);

                    //add the mercs name to the string
                    wcscat(pFinishedString, sDollarAmount);
                }
            }
            else
            {
                //add the rest of the string
                wcscat(pFinishedString, pTempString[iCurLocInSourceString..]);

                iCurLocInSourceString += wcslen(pTempString[iCurLocInSourceString..]);
            }
        }

        return true;
    }
}


public enum EmailFields
{
    SENDER = 0,
    RECEIVED,
    SUBJECT,
    READ,
};

public enum EmailAddresses
{
    MAIL_ENRICO = 0,
    CHAR_PROFILE_SITE,
    GAME_HELP,
    IMP_PROFILE_RESULTS,
    SPECK_FROM_MERC,
    RIS_EMAIL,
    BARRY_MAIL,
    MELTDOWN_MAIL = BARRY_MAIL + 39,
    INSURANCE_COMPANY,
    BOBBY_R,
    KING_PIN,
    JOHN_KULBA,
    AIM_SITE,
}

// the enumeration of headers
public enum EmailHeaders
{
    FROM_HEADER = 0,
    SUBJECT_HEADER,
    RECD_HEADER,
};

public enum EMAILTRAVERSALBUTTON
{
    PREVIOUS_BUTTON = 0,
    NEXT_BUTTON,
};

public struct EmailPageInfoStruct
{
    public RecordPtr pFirstRecord;
    public RecordPtr pLastRecord;
    public int iPageNumber;
}

public class RecordPtr
{
    public string? pRecord;
    public RecordPtr? Next;
}

public class email
{
    public string pSubject;
    public int usOffset;
    public int usLength;
    public EmailAddresses ubSender;
    public uint iDate;
    public int iId;
    public int iFirstData;
    public object uiSecondData;
    public bool fRead;
    public bool fNew;

    public int iThirdData;
    public int iFourthData;
    public int uiFifthData;
    public int uiSixData;

    public email? Next;
    public email? Prev;
};

public record PagePtr
{
    public int[] iIds = new int[MAX_MESSAGES_PAGE];
    public int iPageId;
    public PagePtr? Next;
    public PagePtr? Prev;
};

// This used when saving the emails to disk.
public record SavedEmailStruct
{
    public int usOffset;
    public int usLength;
    public EmailAddresses ubSender;
    public uint iDate;
    public int iId;
    public int iFirstData;
    public object uiSecondData;
    public int iThirdData;
    public int iFourthData;
    public int uiFifthData;
    public int uiSixData;
    public bool fRead;
    public bool fNew;
}
