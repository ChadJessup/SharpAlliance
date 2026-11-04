using System;
using System.Collections.Generic;
using System.Linq;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SixLabors.ImageSharp;
using static SharpAlliance.Core.EnglishText;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

// Port of the user interface half of files.c: the file viewer (list of senders on the left, the selected file on the right).
public partial class Laptop
{
    private const int TOP_X = 0 + LAPTOP_SCREEN_UL_X;
    private const int TOP_Y = LAPTOP_SCREEN_UL_Y;
    private const int FILES_TITLE_X = 140;
    private const int FILES_TITLE_Y = 33;
    private const FontStyle FILES_TITLE_FONT = FontStyle.FONT14ARIAL;
    private const FontStyle FILES_TEXT_FONT = FontStyle.FONT10ARIAL;
    private const int BLOCK_HEIGHT = 10;
    private const int FILES_SENDER_TEXT_X = TOP_X + 15;
    private const int FILE_VIEWER_X = 236;
    private const int FILE_VIEWER_Y = 85;
    private const int FILE_VIEWER_WIDTH = 598 - 240;
    private const int FILE_GAP = 2;
    private const FontColor FILE_TEXT_COLOR = FontColor.FONT_BLACK;
    private const int FILE_STRING_SIZE = 400;
    private const int MAX_FILES_PAGE = 28;
    private const int FILES_LIST_X = FILES_SENDER_TEXT_X;
    private const int FILES_LIST_Y = 9 * BLOCK_HEIGHT;
    private const int FILES_LIST_WIDTH = 100;
    private const int LENGTH_OF_ENRICO_FILE = 68;
    private const int MAX_FILE_MESSAGE_PAGE_SIZE = 325;
    private const int PREVIOUS_FILE_PAGE_BUTTON_X = 553;
    private const int PREVIOUS_FILE_PAGE_BUTTON_Y = 53;
    private const int NEXT_FILE_PAGE_BUTTON_X = 577;
    private const int NEXT_FILE_PAGE_BUTTON_Y = PREVIOUS_FILE_PAGE_BUTTON_Y;
    private const int FILES_COUNTER_1_WIDTH = 7;
    private const int FILES_COUNTER_2_WIDTH = 43;
    private const int FILES_COUNTER_3_WIDTH = 45;

    private const int SLAY_LENGTH = 12;
    private const int ENRICO_LENGTH = 0;

    private static readonly int[] ubFileRecordsLength = { ENRICO_LENGTH, SLAY_LENGTH, SLAY_LENGTH, SLAY_LENGTH, SLAY_LENGTH, SLAY_LENGTH, SLAY_LENGTH };

    private static readonly int[] ubFileOffsets =
    {
        0,
        ENRICO_LENGTH,
        SLAY_LENGTH + ENRICO_LENGTH,
        2 * SLAY_LENGTH + ENRICO_LENGTH,
        3 * SLAY_LENGTH + ENRICO_LENGTH,
        4 * SLAY_LENGTH + ENRICO_LENGTH,
        5 * SLAY_LENGTH + ENRICO_LENGTH,
    };

    private static readonly int[] usProfileIdsForTerroristFiles =
    {
        0,   // no body
        112, // elgin
        64,  // slay
        82,  // mom
        83,  // imposter
        110, // tiff
        111, // t-rex
        112, // elgin
    };

    private static int iHighLightFileLine = -1;
    private static int giFilesPage = 0;
    private static bool fWaitAFrame = false;

    private static HVOBJECT guiTITLE = null!;
    private static HVOBJECT guiFileBack = null!;
    private static HVOBJECT guiTOP = null!;
    private static HVOBJECT guiHIGHLIGHT = null!;

    private static readonly GUI_BUTTON[] giFilesPageButtons = new GUI_BUTTON[2];
    private static readonly ButtonPic[] giFilesPageButtonsImage = new ButtonPic[2];
    private static readonly MOUSE_REGION[] pFilesRegions = CreateRegions(MAX_FILES_PAGE, nameof(pFilesRegions));

    // the lines of the file being displayed
    private static readonly List<string> pFileStringList = [];

    /// <summary>A line of a multi page file that is wrapped to a different width (to flow around a picture).</summary>
    private sealed record FileRecordWidth(int iRecordNumber, int iRecordWidth, int iRecordHeightAdjustment, int ubFlags);

    public static void EnterFiles()
    {
        // load graphics for files system
        LoadFiles();

        // in files mode now, set the fact
        fInFilesMode = true;

        InitializeFilesMouseRegions();
        CreateButtonsForFilesPage();

        // now set start states
        HandleFileViewerButtonStates();

        // build files list
        OpenAndReadFilesFile();

        // render files system
        RenderFiles();

        // entered due to icon
        if (fEnteredFileViewerFromNewFileIcon)
        {
            OpenFirstUnreadFile();
            fEnteredFileViewerFromNewFileIcon = false;
        }
    }

    public static void ExitFiles()
    {
        // write files list out to disk
        OpenAndWriteFilesFile();

        RemoveFilesMouseRegions();
        DeleteButtonsForFilesPage();

        fInFilesMode = false;

        RemoveFiles();
    }

    public static void HandleFiles()
    {
        CheckForUnreadFiles();
    }

    public static void RenderFiles()
    {
        RenderFilesBackGround();

        // draw the title bars text
        DrawFilesTitleText();

        // display the list of senders
        DisplayFilesList();

        // draw the highlighted file
        DisplayFileMessage();

        // title bar icon
        BlitTitleBarIcons();

        // display border
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiLaptopBACKGROUND, 0, 108, 23, VO_BLT.SRCTRANSPARENCY);
    }

    private static void RenderFilesBackGround()
    {
        // blt title bar to screen
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiTITLE, 0, TOP_X, TOP_Y - 2, VO_BLT.SRCTRANSPARENCY);

        // the top part of the screen
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiTOP, 0, TOP_X, TOP_Y + 22, VO_BLT.SRCTRANSPARENCY);
    }

    private static void DrawFilesTitleText()
    {
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
        FontSubSystem.DrawTextToScreen(pFilesTitle[0], new(FILES_TITLE_X, FILES_TITLE_Y), 0, FILES_TITLE_FONT, FontColor.FONT_WHITE, FontColor.FONT_BLACK, TextJustifies.LEFT_JUSTIFIED);
    }

    private static bool LoadFiles()
    {
        // title bar
        guiTITLE = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\programtitlebar.sti"));

        // top portion of the screen background
        guiTOP = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\fileviewer.sti"));

        // the highlight
        guiHIGHLIGHT = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\highlight.sti"));

        // file background
        guiFileBack = video.GetVideoObject(Utils.FilenameForBPP("LAPTOP\\fileviewerwhite.sti"));

        return true;
    }

    private static void RemoveFiles()
    {
        video.DeleteVideoObject(guiTOP);
        video.DeleteVideoObject(guiTITLE);
        video.DeleteVideoObject(guiHIGHLIGHT);
        video.DeleteVideoObject(guiFileBack);
    }

    private static void DisplayFilesList()
    {
        // run through the list of files and display the 'sender'
        for (int iCounter = 0; iCounter < pFilesListHead.Count; iCounter++)
        {
            if (iCounter == iHighLightFileLine)
            {
                // render highlight
                video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiHIGHLIGHT, 0, FILES_SENDER_TEXT_X - 5, ((iCounter + 9) * BLOCK_HEIGHT) + (iCounter * 2) - 4, VO_BLT.SRCTRANSPARENCY);
            }

            FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);
            FontSubSystem.DrawTextToScreen(pFilesSenderList[(int)pFilesListHead[iCounter].ubCode], new(FILES_SENDER_TEXT_X, ((iCounter + 9) * BLOCK_HEIGHT) + (iCounter * 2) - 2), 0, FILES_TEXT_FONT, FontColor.FONT_BLACK, FontColor.FONT_BLACK, TextJustifies.LEFT_JUSTIFIED);
        }

        // reset shadow
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
    }

    private static void DisplayFileMessage()
    {
        // get the currently selected message
        if (iHighLightFileLine != -1)
        {
            DisplayFormattedText();
        }
        else
        {
            HandleFileViewerButtonStates();
        }

        // reset shadow
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);
    }

    private static void InitializeFilesMouseRegions()
    {
        for (int iCounter = 0; iCounter < MAX_FILES_PAGE; iCounter++)
        {
            MouseSubSystem.MSYS_DefineRegion(
                pFilesRegions[iCounter],
                FILES_LIST_X,
                FILES_LIST_Y + iCounter * (BLOCK_HEIGHT + 2),
                FILES_LIST_X + FILES_LIST_WIDTH,
                FILES_LIST_Y + (iCounter + 1) * (BLOCK_HEIGHT + 2),
                MSYS_PRIORITY.NORMAL + 2,
                MSYS_NO_CURSOR,
                MSYS_NO_CALLBACK,
                FilesBtnCallBack);
            MouseSubSystem.MSYS_AddRegion(ref pFilesRegions[iCounter]);
            MouseSubSystem.MSYS_SetRegionUserData(pFilesRegions[iCounter], 0, iCounter);
        }
    }

    private static void RemoveFilesMouseRegions()
    {
        foreach (MOUSE_REGION region in pFilesRegions)
        {
            MouseSubSystem.MSYS_RemoveRegion(region);
        }
    }

    private static void FilesBtnCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
        {
            return;
        }

        if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
            int iFileId = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 0);

            // reset iHighLightListLine
            iHighLightFileLine = -1;

            // make sure it is a valid file
            if (iFileId < pFilesListHead.Count)
            {
                giFilesPage = 0;
                iHighLightFileLine = iFileId;
            }

            fReDrawScreenFlag = true;
        }
    }

    /// <summary>Loads one of the text records of Files.edt.</summary>
    private static string LoadFilesRecord(string file, int record)
    {
        files.LoadEncryptedDataFromFile(file, out string sString, FILE_STRING_SIZE * record * 2, FILE_STRING_SIZE * 2);
        return sString;
    }

    private static int PictureWidth(HVOBJECT picture) => picture.pETRLEObject[0].usWidth;

    private static int PictureHeight(HVOBJECT picture) => picture.pETRLEObject[0].usHeight;

    private static int DisplayFileText(int iOffSet, int iLength, int iHeight)
    {
        for (int iCounter = 0; iCounter < iLength; iCounter++)
        {
            string sString = LoadFilesRecord("BINARYDATA\\Files.edt", iOffSet + iCounter);

            // display string and get height
            iHeight += WordWrap.IanDisplayWrappedString(new(FILE_VIEWER_X + 4, FILE_VIEWER_Y + iHeight), FILE_VIEWER_WIDTH, FILE_GAP, FILES_TEXT_FONT, FILE_TEXT_COLOR, sString, 0, false, IAN_TEXT_FLAGS.LEFT_JUSTIFIED);
        }

        return iHeight;
    }

    private static bool DisplayFormattedText()
    {
        fWaitAFrame = false;

        // get the file that was highlighted
        FilesUnit pFilesList = pFilesListHead[iHighLightFileLine];

        int iMessageCode = (int)pFilesList.ubCode;

        // set file as read
        pFilesList.fRead = true;

        // blt background to screen
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, guiFileBack, 0, FILE_VIEWER_X, FILE_VIEWER_Y - 4, VO_BLT.SRCTRANSPARENCY);

        // get the offset in the file
        int iOffSet = 0;
        for (int iCounter = 0; iCounter < iMessageCode; iCounter++)
        {
            iOffSet += ubFileRecordsLength[iCounter];
        }

        int iLength = ubFileRecordsLength[iMessageCode];

        // no shadow
        FontSubSystem.SetFontShadow(FontShadow.NO_SHADOW);

        switch (pFilesList.ubFormat)
        {
            case 0:
                // no format, all text
                DisplayFileText(iOffSet, iLength, 0);
                break;

            case 1:
            {
                // second format, one picture, all text below
                HVOBJECT picture = video.GetVideoObject(Utils.FilenameForBPP(pFilesList.pPicFileNameList[0]));

                video.BltVideoObject(SurfaceType.FRAME_BUFFER, picture, 0, FILE_VIEWER_X + 4 + (FILE_VIEWER_WIDTH - PictureWidth(picture)) / 2, FILE_VIEWER_Y + 10, VO_BLT.SRCTRANSPARENCY);

                DisplayFileText(iOffSet, iLength, PictureHeight(picture) + 20);

                video.DeleteVideoObject(picture);
                break;
            }

            case 2:
            {
                // third format, two pictures, side by side with all text below
                HVOBJECT first = video.GetVideoObject(Utils.FilenameForBPP(pFilesList.pPicFileNameList[0]));
                HVOBJECT second = video.GetVideoObject(Utils.FilenameForBPP(pFilesList.pPicFileNameList[1]));

                int usFreeSpace = (FILE_VIEWER_WIDTH - PictureWidth(first) - PictureWidth(second)) / 3;

                video.BltVideoObject(SurfaceType.FRAME_BUFFER, first, 0, FILE_VIEWER_X + usFreeSpace, FILE_VIEWER_Y + 10, VO_BLT.SRCTRANSPARENCY);

                // position of the second picture
                usFreeSpace = usFreeSpace * 2 + PictureWidth(first);
                video.BltVideoObject(SurfaceType.FRAME_BUFFER, second, 0, FILE_VIEWER_X + usFreeSpace, FILE_VIEWER_Y + 10, VO_BLT.SRCTRANSPARENCY);

                DisplayFileText(iOffSet, iLength, PictureHeight(first) + 20);

                video.DeleteVideoObject(first);
                video.DeleteVideoObject(second);
                break;
            }

            case 3:
                // picture on the left, with text on right and below
                HandleSpecialTerroristFile(iMessageCode, pFilesList.pPicFileNameList[0]);
                break;

            default:
                HandleSpecialFiles(pFilesList.ubFormat);
                break;
        }

        HandleFileViewerButtonStates();
        FontSubSystem.SetFontShadow(FontShadow.DEFAULT_SHADOW);

        return true;
    }

    private static void BltFilePicture(string file, int x, int y)
    {
        HVOBJECT picture = video.GetVideoObject(Utils.FilenameForBPP(file));
        video.BltVideoObject(SurfaceType.FRAME_BUFFER, picture, 0, x, y, VO_BLT.SRCTRANSPARENCY);
        video.DeleteVideoObject(picture);
    }

    /// <summary>The width a record of a multi page file is wrapped to, and how it moves the position on the page.</summary>
    private static int ApplyRecordWidth(List<FileRecordWidth> widths, int iRecord, int iPageSize, int usWidth, ref int iPosition)
    {
        foreach (FileRecordWidth width in widths)
        {
            if (iRecord == width.iRecordNumber)
            {
                usWidth = width.iRecordWidth;

                if (width.iRecordHeightAdjustment == iPageSize)
                {
                    if (iPosition != 0)
                    {
                        iPosition += iPageSize - iPosition;
                    }
                }
                else
                {
                    iPosition += width.iRecordHeightAdjustment;
                }
            }
        }

        return usWidth;
    }

    /// <summary>Port of GetFirstStringOnThisPage (WordWrap.c): the index of the first line of the given page.</summary>
    private static int GetFirstStringOnThisPage(List<string> records, FontStyle uiFont, int usWidth, int ubGap, int iPage, int iPageSize, List<FileRecordWidth> widths)
    {
        if (records.Count == 0)
        {
            return -1;
        }

        int current = 0;
        int iCurrentPositionOnThisPage = 0;
        int iCurrentPage = 0;

        while (iCurrentPage < iPage)
        {
            int usCurrentWidth = ApplyRecordWidth(widths, current, iPageSize, usWidth, ref iCurrentPositionOnThisPage);

            // build record list to this point
            while (current < records.Count
                && iCurrentPositionOnThisPage + WordWrap.IanWrappedStringHeight(0, 0, usCurrentWidth, ubGap, uiFont, 0, records[current], 0, false, 0) < iPageSize)
            {
                // still room on this page
                iCurrentPositionOnThisPage += WordWrap.IanWrappedStringHeight(0, 0, usCurrentWidth, ubGap, uiFont, 0, records[current], 0, false, 0);

                current++;
                usCurrentWidth = ApplyRecordWidth(widths, current, iPageSize, usWidth, ref iCurrentPositionOnThisPage);
            }

            if (current >= records.Count)
            {
                return -1;
            }

            iCurrentPositionOnThisPage = 0;
            iCurrentPage++;
        }

        return current;
    }

    private static void ClearFileStringList() => pFileStringList.Clear();

    private static void AddStringToFilesList(string pString) => pFileStringList.Add(pString);

    private static bool HandleSpecialFiles(int ubFormat)
    {
        ClearFileStringList();

        switch (ubFormat)
        {
            case 255:
            {
                // read the records of the Arulco intelligence file
                List<FileRecordWidth> widthList = CreateWidthRecordsForAruloIntelFile();
                for (int i = 0; i < LENGTH_OF_ENRICO_FILE; i++)
                {
                    AddStringToFilesList(LoadFilesRecord("BINARYDATA\\RIS.EDT", i));
                }

                int iYPositionOnPage = 0;
                int iCounter = Math.Max(0, GetFirstStringOnThisPage(pFileStringList, FILES_TEXT_FONT, 350, FILE_GAP, giFilesPage, MAX_FILE_MESSAGE_PAGE_SIZE, widthList));
                bool fPastTheEnd = GetFirstStringOnThisPage(pFileStringList, FILES_TEXT_FONT, 350, FILE_GAP, giFilesPage, MAX_FILE_MESSAGE_PAGE_SIZE, widthList) == -1;

                fOnLastFilesPageFlag = fPastTheEnd;

                // move through list and display
                while (!fPastTheEnd && iCounter < pFileStringList.Count)
                {
                    string sString = pFileStringList[iCounter];

                    if (sString.Length == 0)
                    {
                        // on last page
                        fOnLastFilesPageFlag = true;
                    }

                    // set up font
                    FontStyle uiFont = FILES_TEXT_FONT;
                    if (giFilesPage == 0 && iCounter == 0)
                    {
                        uiFont = FILES_TITLE_FONT;
                    }

                    // based on the record we are at, select the X start position and the width to wrap the line, to fit around pictures
                    int iFileLineWidth = 350;
                    int iFileStartX = FILE_VIEWER_X + 10;

                    if (iCounter == FILES_COUNTER_1_WIDTH)
                    {
                        if (giFilesPage == 0)
                        {
                            iYPositionOnPage += MAX_FILE_MESSAGE_PAGE_SIZE - iYPositionOnPage;
                        }
                    }
                    else if (iCounter == FILES_COUNTER_2_WIDTH || iCounter == FILES_COUNTER_3_WIDTH)
                    {
                        iFileLineWidth = 200;
                        iFileStartX = FILE_VIEWER_X + 150;
                    }

                    bool fGoingOffCurrentPage;
                    if (iYPositionOnPage + WordWrap.IanWrappedStringHeight(0, 0, iFileLineWidth, FILE_GAP, uiFont, 0, sString, 0, false, 0) < MAX_FILE_MESSAGE_PAGE_SIZE)
                    {
                        // now print it
                        iYPositionOnPage += WordWrap.IanDisplayWrappedString(new(iFileStartX, FILE_VIEWER_Y + iYPositionOnPage), iFileLineWidth, FILE_GAP, uiFont, FILE_TEXT_COLOR, sString, 0, false, IAN_TEXT_FLAGS.IAN_WRAP_NO_SHADOW);
                        fGoingOffCurrentPage = false;
                    }
                    else
                    {
                        // gonna get cut off...end now
                        fGoingOffCurrentPage = true;
                    }

                    iCounter++;
                    fOnLastFilesPageFlag = iCounter >= pFileStringList.Count;

                    // going over the edge, stop now
                    if (fGoingOffCurrentPage)
                    {
                        break;
                    }
                }

                ClearFileStringList();
                break;
            }
        }

        // place pictures
        if (giFilesPage == 0)
        {
            // page 1 picture of the country
            BltFilePicture("LAPTOP\\ArucoFilesMap.sti", 300, 270);
        }
        else if (giFilesPage == 4)
        {
            // kid pic
            BltFilePicture("LAPTOP\\Enrico_Y.sti", 260, 225);
        }
        else if (giFilesPage == 5)
        {
            // wedding pic
            BltFilePicture("LAPTOP\\Enrico_W.sti", 260, 85);
        }

        return true;
    }

    private static bool HandleSpecialTerroristFile(int iFileNumber, string sPictureName)
    {
        int iOffset = ubFileOffsets[iFileNumber];

        // grab width list
        List<FileRecordWidth> widthList = CreateWidthRecordsForTerroristFile();

        ClearFileStringList();
        for (int i = 0; i < ubFileRecordsLength[iFileNumber]; i++)
        {
            AddStringToFilesList(LoadFilesRecord("BINARYDATA\\files.EDT", iOffset + i));
        }

        int iYPositionOnPage = 0;
        int first = GetFirstStringOnThisPage(pFileStringList, FILES_TEXT_FONT, 350, FILE_GAP, giFilesPage, MAX_FILE_MESSAGE_PAGE_SIZE, widthList);
        int iCounter = Math.Max(0, first);

        fOnLastFilesPageFlag = first == -1;

        // move through list and display
        while (first != -1 && iCounter < pFileStringList.Count)
        {
            string sString = pFileStringList[iCounter];

            if (sString.Length == 0)
            {
                // on last page
                fOnLastFilesPageFlag = true;
            }

            // set up font
            FontStyle uiFont = FILES_TEXT_FONT;
            if (giFilesPage == 0 && iCounter == 0)
            {
                uiFont = FILES_TITLE_FONT;
            }

            int iFileLineWidth = 350;
            int iFileStartX = FILE_VIEWER_X + 10;
            if (iCounter > 3 && iCounter < 7)
            {
                iFileLineWidth = 170;
                iFileStartX = FILE_VIEWER_X + 180;
            }

            bool fGoingOffCurrentPage;
            if (iYPositionOnPage + WordWrap.IanWrappedStringHeight(0, 0, iFileLineWidth, FILE_GAP, uiFont, 0, sString, 0, false, 0) < MAX_FILE_MESSAGE_PAGE_SIZE)
            {
                // now print it
                iYPositionOnPage += WordWrap.IanDisplayWrappedString(new(iFileStartX, FILE_VIEWER_Y + iYPositionOnPage), iFileLineWidth, FILE_GAP, uiFont, FILE_TEXT_COLOR, sString, 0, false, IAN_TEXT_FLAGS.IAN_WRAP_NO_SHADOW);
                fGoingOffCurrentPage = false;
            }
            else
            {
                // gonna get cut off...end now
                fGoingOffCurrentPage = true;
            }

            fOnLastFilesPageFlag = iCounter + 1 >= pFileStringList.Count && !fGoingOffCurrentPage;

            // show picture
            if (giFilesPage == 0 && iCounter == 5)
            {
                int profile = usProfileIdsForTerroristFiles[iFileNumber + 1];
                string sTemp = profile < 100 ? $"FACES\\BIGFACES\\{profile:D2}.sti" : $"FACES\\BIGFACES\\{profile:D3}.sti";

                BltFilePicture(sTemp, FILE_VIEWER_X + 30, iYPositionOnPage + 21);
                BltFilePicture("LAPTOP\\InterceptBorder.sti", FILE_VIEWER_X + 25, iYPositionOnPage + 16);
            }

            // going over the edge, stop now
            if (fGoingOffCurrentPage)
            {
                break;
            }

            iCounter++;
        }

        ClearFileStringList();

        return true;
    }

    private static void CreateButtonsForFilesPage()
    {
        giFilesPageButtonsImage[0] = ButtonSubSystem.LoadButtonImage("LAPTOP\\arrows.sti", -1, 0, -1, 1, -1);
        giFilesPageButtons[0] = ButtonSubSystem.QuickCreateButton(giFilesPageButtonsImage[0], new Point(PREVIOUS_FILE_PAGE_BUTTON_X, PREVIOUS_FILE_PAGE_BUTTON_Y),
            ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1,
            (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)BtnPreviousFilePageCallback);

        giFilesPageButtonsImage[1] = ButtonSubSystem.LoadButtonImage("LAPTOP\\arrows.sti", -1, 6, -1, 7, -1);
        giFilesPageButtons[1] = ButtonSubSystem.QuickCreateButton(giFilesPageButtonsImage[1], new Point(NEXT_FILE_PAGE_BUTTON_X, NEXT_FILE_PAGE_BUTTON_Y),
            ButtonFlags.BUTTON_TOGGLE, MSYS_PRIORITY.HIGHEST - 1,
            (GUI_CALLBACK)MouseSubSystem.BtnGenericMouseMoveButtonCallback, (GUI_CALLBACK)BtnNextFilePageCallback);

        for (int i = 0; i < 2; i++)
        {
            ButtonSubSystem.SetButtonCursor(giFilesPageButtons[i], CURSOR.LAPTOP_SCREEN);
            buttonList.Add(giFilesPageButtons[i]);
        }
    }

    private static void DeleteButtonsForFilesPage()
    {
        for (int i = 0; i < 2; i++)
        {
            buttonList.Remove(giFilesPageButtons[i]);
            ButtonSubSystem.RemoveButton(giFilesPageButtons[i]);
            ButtonSubSystem.UnloadButtonImage(giFilesPageButtonsImage[i]);
        }
    }

    private static void BtnPreviousFilePageCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (!btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED) || fWaitAFrame)
        {
            return;
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) && btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
        {
            if (giFilesPage > 0)
            {
                giFilesPage--;
                fWaitAFrame = true;
            }

            fReDrawScreenFlag = true;
            btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
            ButtonSubSystem.MarkButtonsDirty(buttonList);
        }
    }

    private static void BtnNextFilePageCallback(ref GUI_BUTTON btn, MSYS_CALLBACK_REASON reason)
    {
        if (!btn.uiFlags.HasFlag(ButtonFlags.BUTTON_ENABLED) || fWaitAFrame)
        {
            return;
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            btn.uiFlags |= ButtonFlags.BUTTON_CLICKED_ON;
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP) && btn.uiFlags.HasFlag(ButtonFlags.BUTTON_CLICKED_ON))
        {
            if (!fOnLastFilesPageFlag)
            {
                fWaitAFrame = true;
                giFilesPage++;
            }

            fReDrawScreenFlag = true;
            btn.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
            ButtonSubSystem.MarkButtonsDirty(buttonList);
        }
    }

    private static void HandleFileViewerButtonStates()
    {
        GUI_BUTTON previous = giFilesPageButtons[0];
        GUI_BUTTON next = giFilesPageButtons[1];

        if (previous is null || next is null)
        {
            return;
        }

        if (iHighLightFileLine == -1)
        {
            // not displaying a message
            ButtonSubSystem.DisableButton(previous);
            ButtonSubSystem.DisableButton(next);
            previous.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
            next.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
            return;
        }

        // turn off previous page button
        if (giFilesPage == 0)
        {
            ButtonSubSystem.DisableButton(previous);
            previous.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
        }
        else
        {
            ButtonSubSystem.EnableButton(previous);
        }

        // turn off next page button
        if (fOnLastFilesPageFlag)
        {
            ButtonSubSystem.DisableButton(next);
            next.uiFlags &= ~ButtonFlags.BUTTON_CLICKED_ON;
        }
        else
        {
            ButtonSubSystem.EnableButton(next);
        }
    }

    private static List<FileRecordWidth> CreateWidthRecordsForAruloIntelFile() =>
    [
        new(FILES_COUNTER_1_WIDTH, 350, MAX_FILE_MESSAGE_PAGE_SIZE, 0),
        new(FILES_COUNTER_2_WIDTH, 200, 0, 0),
        new(FILES_COUNTER_3_WIDTH, 200, 0, 0),
    ];

    private static List<FileRecordWidth> CreateWidthRecordsForTerroristFile() =>
    [
        new(4, 170, 0, 0),
        new(5, 170, 0, 0),
        new(6, 170, 0, 0),
    ];

    private static void OpenFirstUnreadFile()
    {
        // open the first unread file in the list (the last one found wins, as in the original)
        for (int iCounter = 0; iCounter < pFilesListHead.Count; iCounter++)
        {
            if (!pFilesListHead[iCounter].fRead)
            {
                iHighLightFileLine = iCounter;
            }
        }
    }

    // add a file about this terrorist
    public static bool AddFileAboutTerrorist(int iProfileId)
    {
        for (int iCounter = 1; iCounter < 7; iCounter++)
        {
            if (usProfileIdsForTerroristFiles[iCounter] == iProfileId)
            {
                // checked, and this file is there
                AddFilesToPlayersLog((BACKGROUND)iCounter, 0, 3, null, null);
                return true;
            }
        }

        return false;
    }
}
