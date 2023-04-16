using System.Diagnostics;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.VideoSurfaces;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const int MAX_PROGRESSBARS = 4;
}

public class AnimatedProgressBar
{
    private static PROGRESSBAR[] pBar = new PROGRESSBAR[MAX_PROGRESSBARS];
    private static IVideoManager video;

    public AnimatedProgressBar(IVideoManager videoManager)
    {
        video = videoManager;
    }

    //An important setup function.  The best explanation is through example.  The example being the loading
    //of a file -- there are many stages of the map loading.  In JA2, the first step is to load the tileset.
    //Because it is a large chunk of the total loading of the map, we may gauge that it takes up 30% of the
    //total load.  Because it is also at the beginning, we would pass in the arguments ( 0, 30, "text" ).
    //As the process animates using UpdateProgressBar( 0 to 100 ), the total progress bar will only reach 30%
    //at the 100% mark within UpdateProgressBar.  At that time, you would go onto the next step, resetting the
    //relative start and end percentage from 30 to whatever, until your done.
    public static void SetRelativeStartAndEndPercentage(int ubID, float uiRelStartPerc, float uiRelEndPerc, string str)
    {
        PROGRESSBAR? pCurr;
        int usStartX, usStartY;

        Debug.Assert(ubID < MAX_PROGRESSBARS);
        pCurr = pBar[ubID];
        if (pCurr is null)
        {
            return;
        }

        pCurr.rStart = uiRelStartPerc * 0.01f;
        pCurr.rEnd = uiRelEndPerc * 0.01f;

        //Render the entire panel now, as it doesn't need update during the normal rendering
        if (pCurr.fPanel)
        {
            //Draw panel
            video.ColorFillVideoSurfaceArea(Surfaces.FRAME_BUFFER,
                pCurr.usPanelLeft, pCurr.usPanelTop, pCurr.usPanelRight, pCurr.usPanelBottom, pCurr.usLtColor);
            video.ColorFillVideoSurfaceArea(Surfaces.FRAME_BUFFER,
                pCurr.usPanelLeft + 1, pCurr.usPanelTop + 1, pCurr.usPanelRight, pCurr.usPanelBottom, pCurr.usDkColor);
            video.ColorFillVideoSurfaceArea(Surfaces.FRAME_BUFFER,
                pCurr.usPanelLeft + 1, pCurr.usPanelTop + 1, pCurr.usPanelRight - 1, pCurr.usPanelBottom - 1, pCurr.usColor);
            VeldridVideoManager.InvalidateRegion(pCurr.usPanelLeft, pCurr.usPanelTop, pCurr.usPanelRight, pCurr.usPanelBottom);
            //Draw title

            if (pCurr.swzTitle != string.Empty)
            {
                usStartX = pCurr.usPanelLeft +                                                                                 // left position
                                     (pCurr.usPanelRight - pCurr.usPanelLeft) / 2 -                               // + half width
                                     FontSubSystem.StringPixLength(pCurr.swzTitle, pCurr.usTitleFont) / 2;  // - half string width
                usStartY = pCurr.usPanelTop + 3;
                FontSubSystem.SetFont(pCurr.usTitleFont);
                FontSubSystem.SetFontForeground(pCurr.ubTitleFontForeColor);
                FontSubSystem.SetFontShadow(pCurr.ubTitleFontShadowColor);
                FontSubSystem.SetFontBackground(0);
                mprintf(usStartX, usStartY, pCurr.swzTitle);
            }
        }

        if (pCurr.fDisplayText)
        {
            //Draw message
            if (str == string.Empty)
            {
                if (pCurr.fUseSaveBuffer)
                {
                    int usFontHeight = FontSubSystem.GetFontHeight(pCurr.usMsgFont);

                    RenderDirty.RestoreExternBackgroundRect(pCurr.usBarLeft, pCurr.usBarBottom, (int)(pCurr.usBarRight - pCurr.usBarLeft), (int)(usFontHeight + 3));
                }

                FontSubSystem.SetFont(pCurr.usMsgFont);
                FontSubSystem.SetFontForeground(pCurr.ubMsgFontForeColor);
                FontSubSystem.SetFontShadow(pCurr.ubMsgFontShadowColor);
                FontSubSystem.SetFontBackground(0);
                mprintf(pCurr.usBarLeft, pCurr.usBarBottom + 3, str);
            }
        }
    }


    //This part renders the progress bar at the percentage level that you specify.  If you have set relative
    //percentage values in the above function, then the uiPercentage will be reflected based off of the relative
    //percentages.  
    static uint uiLastTime = 0;
    public static void RenderProgressBar(int ubID, int uiPercentage)
    {
        uint uiCurTime = GetJA2Clock();
        float rActual;
        PROGRESSBAR? pCurr = null;
        //int r, g;
        int end;

        Debug.Assert(ubID < MAX_PROGRESSBARS);
        pCurr = pBar[ubID];

        if (pCurr == null)
        {
            return;
        }

        if (pCurr is not null)
        {
            rActual = pCurr.rStart + (float)(pCurr.rEnd - pCurr.rStart) * (float)uiPercentage * 0.01f;

            if (rActual - pCurr.rLastActual < 0.01)
            {
                return;
            }

            pCurr.rLastActual = (float)((int)(rActual * 100) * 0.01);

            end = (int)(pCurr.usBarLeft + 2.0 + rActual * (pCurr.usBarRight - pCurr.usBarLeft - 4));
            if (end < pCurr.usBarLeft + 2 || end > pCurr.usBarRight - 2)
            {
                return;
            }

            if (gfUseLoadScreenProgressBar)
            {
                ColorFillVideoSurfaceArea(
                    Surfaces.FRAME_BUFFER,
                    pCurr.usBarLeft,
                    pCurr.usBarTop,
                    end,
                    pCurr.usBarBottom,
                    Get16BPPColor(
                        FROMRGB(
                            pCurr.ubColorFillRed,
                            pCurr.ubColorFillGreen,
                            pCurr.ubColorFillBlue)));
                //if( pCurr.usBarRight > gusLeftmostShaded )
                //{
                //	ShadowVideoSurfaceRect( Surfaces.FRAME_BUFFER, gusLeftmostShaded+1, pCurr.usBarTop, end, pCurr.usBarBottom );	
                //	gusLeftmostShaded = (int)end;
                //}
            }
            else
            {
                //Border edge of the progress bar itself in gray
                video.ColorFillVideoSurfaceArea(Surfaces.FRAME_BUFFER,
                    pCurr.usBarLeft, pCurr.usBarTop, pCurr.usBarRight, pCurr.usBarBottom,
                    Get16BPPColor(FROMRGB(160, 160, 160)));
                //Interior of progress bar in black
                video.ColorFillVideoSurfaceArea(Surfaces.FRAME_BUFFER,
                    pCurr.usBarLeft + 2, pCurr.usBarTop + 2, pCurr.usBarRight - 2, pCurr.usBarBottom - 2,
                    Get16BPPColor(FROMRGB(0, 0, 0)));
                video.ColorFillVideoSurfaceArea(Surfaces.FRAME_BUFFER, pCurr.usBarLeft + 2, pCurr.usBarTop + 2, end, pCurr.usBarBottom - 2, Get16BPPColor(FROMRGB(72, 155, 24)));
            }

            VeldridVideoManager.InvalidateRegion(pCurr.usBarLeft, pCurr.usBarTop, pCurr.usBarRight, pCurr.usBarBottom);
            video.ExecuteBaseDirtyRectQueue();
            video.EndFrameBufferRender();
            video.RefreshScreen();
        }

        // update music here
        if (uiCurTime > (uiLastTime + 200))
        {
//            MusicPoll(true);
            uiLastTime = GetJA2Clock();
        }
    }

    void SetProgressBarColor(int ubID, byte ubColorFillRed, byte ubColorFillGreen, byte ubColorFillBlue)
    {
        PROGRESSBAR? pCurr = null;

        Debug.Assert(ubID < MAX_PROGRESSBARS);

        pCurr = pBar[ubID];
        if (pCurr == null)
            return;

        pCurr.ubColorFillRed = ubColorFillRed;
        pCurr.ubColorFillGreen = ubColorFillGreen;
        pCurr.ubColorFillBlue = ubColorFillBlue;
    }
    void SetProgressBarTextDisplayFlag(int ubID, bool fDisplayText, bool fUseSaveBuffer, bool fSaveScreenToFrameBuffer)
    {
        PROGRESSBAR? pCurr = null;
        Debug.Assert(ubID < MAX_PROGRESSBARS);
        pCurr = pBar[ubID];
        if (pCurr == null)
        {
            return;
        }

        pCurr.fDisplayText = fDisplayText;

        pCurr.fUseSaveBuffer = fUseSaveBuffer;

        //if we are to use the save buffer, blit the portion of the screen to the save buffer
        if (fSaveScreenToFrameBuffer)
        {
            int usFontHeight = FontSubSystem.GetFontHeight(pCurr.usMsgFont) + 3;

            //blit everything to the save buffer ( cause the save buffer can bleed through )
            RenderDirty.BlitBufferToBuffer(guiRENDERBUFFER, guiSAVEBUFFER, pCurr.usBarLeft, pCurr.usBarBottom, (int)(pCurr.usBarRight - pCurr.usBarLeft), usFontHeight);
        }
    }
}

public class PROGRESSBAR
{
    public int ubProgressBarID;
    public int usBarLeft, usBarTop, usBarRight, usBarBottom;
    public bool fPanel;
    public int usPanelLeft, usPanelTop, usPanelRight, usPanelBottom;
    public int usColor, usLtColor, usDkColor;
    public string swzTitle;
    public FontStyle usTitleFont;
    public FontColor ubTitleFontForeColor;
    public FontShadow ubTitleFontShadowColor;
    public FontStyle usMsgFont;
    public FontColor ubMsgFontForeColor;
    public FontShadow ubMsgFontShadowColor;
    public int ubRelativeStartPercentage, ubRelativeEndPercentage;
    public byte ubColorFillRed;
    public byte ubColorFillGreen;
    public byte ubColorFillBlue;
    public float rStart, rEnd;
    public bool fDisplayText;
    public bool fUseSaveBuffer; //use the save buffer when display the text
    public float rLastActual;
}
