﻿using System;
using System.Collections.Generic;
using System.Linq;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.SubSystems;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Managers;

public delegate void SliderChangeCallback(int newValue);

public class SliderSubSystem
{
    private readonly IVideoManager video;
    private readonly IInputManager inputs;

    private string guiSliderBoxImageTag;
    public static bool gfSliderInited;

    public SliderSubSystem(
        IInputManager inputManager,
        IVideoManager videoManager)
    {
        video = videoManager;
        this.inputs = inputManager;
    }

    public List<Slider> Sliders { get; } = new();

    public Slider? gpCurrentSlider = null;

    public void RenderAllSliderBars()
    {
        // set the currently selectd slider bar
        if (this.inputs.gfLeftButtonState && this.gpCurrentSlider != null)
        {
            int usPosY = 0;

            if (this.inputs.gusMousePos.Y < this.gpCurrentSlider.usPos.Y)
            {
                usPosY = 0;
            }
            else
            {
                usPosY = this.inputs.gusMousePos.Y - this.gpCurrentSlider.usPos.Y;
            }

            //if the mouse 
            this.CalculateNewSliderIncrement(ref this.gpCurrentSlider, usPosY);
        }
        else
        {
            this.gpCurrentSlider = null;
        }

        foreach (var slider in this.Sliders)
        {
            this.RenderSelectedSliderBar(slider);
        }
    }

    private void RenderSelectedSliderBar(Slider pSlider)
    {
        if (pSlider.uiFlags.HasFlag(SliderDirection.SLIDER_VERTICAL))
        {
        }
        else
        {
            //display the background ( the bar ) 
            this.OptDisplayLine(new(pSlider.usPos.X + 1, pSlider.usPos.Y - 1), new(pSlider.usPos.X + pSlider.ubSliderWidth - 1, pSlider.usPos.Y - 1), pSlider.usBackGroundColor);
            this.OptDisplayLine(new(pSlider.usPos.X, pSlider.usPos.Y), new(pSlider.usPos.X + pSlider.ubSliderWidth, pSlider.usPos.Y), pSlider.usBackGroundColor);
            this.OptDisplayLine(new(pSlider.usPos.X + 1, pSlider.usPos.Y + 1), new(pSlider.usPos.X + pSlider.ubSliderWidth - 1, pSlider.usPos.Y + 1), pSlider.usBackGroundColor);

            //invalidate the area
            video.InvalidateRegion(new(
                pSlider.usPos.X,
                pSlider.usPos.Y - 2,
                pSlider.ubSliderWidth + 1,
                2));
        }

        this.RenderSliderBox(pSlider);
    }

    private void RenderSliderBox(Slider pSlider)
    {
        HVOBJECT hPixHandle;
        Rectangle SrcRect = new();
        Rectangle DestRect = new();


        if (pSlider.uiFlags.HasFlag(SliderDirection.SLIDER_VERTICAL))
        {
            //fill out the settings for the current dest and source rects
            SrcRect.X = 0;
            SrcRect.Y = 0;
            SrcRect.Width = pSlider.ubSliderWidth;
            SrcRect.Height = pSlider.ubSliderHeight;

            DestRect.X = pSlider.usPos.X - pSlider.ubSliderWidth / 2;
            DestRect.Y = pSlider.usCurrentSliderBoxPosition - pSlider.ubSliderHeight / 2;
            DestRect.Width = pSlider.ubSliderWidth;
            DestRect.Height = pSlider.ubSliderHeight;


            //If it is not the first time to render the slider
            if (!(pSlider.LastRect.Left == 0 && pSlider.LastRect.Right == 0))
            {
                //Restore the old rect
                video.BlitBufferToBuffer(SurfaceType.SAVE_BUFFER, SurfaceType.RENDER_BUFFER, new(pSlider.LastRect.Left, pSlider.LastRect.Top, pSlider.ubSliderWidth, pSlider.ubSliderHeight));

                //invalidate the old area
                video.InvalidateRegion(new(pSlider.LastRect.Left, pSlider.LastRect.Top, pSlider.LastRect.Right, pSlider.LastRect.Bottom));
            }

            //Blit the new rect
            video.BlitBufferToBuffer(SurfaceType.RENDER_BUFFER, SurfaceType.SAVE_BUFFER, new(DestRect.Left, DestRect.Top, pSlider.ubSliderWidth, pSlider.ubSliderHeight));
        }
        else
        {
            //fill out the settings for the current dest and source rects
            SrcRect.X = 0;
            SrcRect.Y = 0;
            SrcRect.Width = pSlider.ubSliderWidth;
            SrcRect.Height = pSlider.ubSliderHeight;

            DestRect.X = pSlider.usCurrentSliderBoxPosition;
            DestRect.Y = pSlider.usPos.Y - Slider.DEFAULT_SLIDER_SIZE;
            DestRect.Width = DestRect.Left + pSlider.ubSliderWidth;
            DestRect.Height = DestRect.Top + pSlider.ubSliderHeight;

            //If it is not the first time to render the slider
            if (!(pSlider.LastRect.Left == 0 && pSlider.LastRect.Right == 0))
            {
                //Restore the old rect
                video.BlitBufferToBuffer(SurfaceType.SAVE_BUFFER, SurfaceType.RENDER_BUFFER,new(pSlider.LastRect.Left, pSlider.LastRect.Top, 8, 15));
            }

            //save the new rect
            video.BlitBufferToBuffer(SurfaceType.RENDER_BUFFER, SurfaceType.SAVE_BUFFER, new(DestRect.Left, DestRect.Top, 8, 15));
        }

        //Save the new rect location
        pSlider.LastRect = DestRect;

        if (pSlider.uiFlags.HasFlag(SliderDirection.SLIDER_VERTICAL))
        {
            //display the slider box
            hPixHandle = video.GetVideoObject(this.guiSliderBoxImageTag);
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, hPixHandle, 0, pSlider.LastRect.Left, pSlider.LastRect.Top);

            //invalidate the area
            video.InvalidateRegion(new(pSlider.LastRect.Left, pSlider.LastRect.Top, pSlider.LastRect.Right, pSlider.LastRect.Bottom));
        }
        else
        {
            //display the slider box
            hPixHandle = video.GetVideoObject(this.guiSliderBoxImageTag);
            video.BltVideoObject(SurfaceType.FRAME_BUFFER, hPixHandle, 0, pSlider.usCurrentSliderBoxPosition, pSlider.usPos.Y - Slider.DEFAULT_SLIDER_SIZE);

            //invalidate the area
            video.InvalidateRegion(new(pSlider.usCurrentSliderBoxPosition, pSlider.usPos.Y - Slider.DEFAULT_SLIDER_SIZE, pSlider.usCurrentSliderBoxPosition + 9, pSlider.usPos.Y + Slider.DEFAULT_SLIDER_SIZE));
        }
    }

    private void OptDisplayLine(PointF usStart, PointF usEnd, Color iColor)
    {
        var frame = video.Surfaces[SurfaceType.FRAME_BUFFER];

        video.SetClippingRegionAndImageWidth(frame.Width, new Rectangle(0, 0, 640, 480));

        // draw the line 
        video.LineDraw(false, usStart, usEnd, iColor, frame);
    }

    private void CalculateNewSliderIncrement(ref Slider pSlider, int usPos)
    {
        float dNewIncrement = 0.0f;
        int usOldIncrement;
        bool fLastSpot = false;
        bool fFirstSpot = false;

        usOldIncrement = pSlider.usCurrentIncrement;

        if (pSlider.uiFlags == SliderDirection.SLIDER_VERTICAL)
        {
            if (usPos >= (int)(pSlider.Bounds.Height * (float).99))
            {
                fLastSpot = true;
            }

            if (usPos <= (int)(pSlider.Bounds.Height * (float).01))
            {
                fFirstSpot = true;
            }

            //pSlider.usNumberOfIncrements
            if (fFirstSpot)
            {
                dNewIncrement = 0;
            }
            else if (fLastSpot)
            {
                dNewIncrement = pSlider.usNumberOfIncrements;
            }
            else
            {
                dNewIncrement = usPos / (float)pSlider.Bounds.Height * pSlider.usNumberOfIncrements;
            }
        }
        else
        {
            dNewIncrement = usPos / (float)pSlider.Bounds.Width * pSlider.usNumberOfIncrements;
        }

        pSlider.usCurrentIncrement = (int)(dNewIncrement + .5);

        this.CalculateNewSliderBoxPosition(ref pSlider);


        //if the the new value is different
        if (usOldIncrement != pSlider.usCurrentIncrement)
        {
            if (pSlider.uiFlags.HasFlag(SliderDirection.SLIDER_VERTICAL))
            {
                //Call the call back for the slider
                pSlider.SliderChangeCallback(pSlider.usNumberOfIncrements - pSlider.usCurrentIncrement);
            }
            else
            {
                //Call the call back for the slider
                pSlider.SliderChangeCallback(pSlider.usCurrentIncrement);
            }
        }
    }

    private void CalculateNewSliderBoxPosition(ref Slider pSlider)
    {
        int usMaxPos;

        if (pSlider.uiFlags == SliderDirection.SLIDER_VERTICAL)
        {
            //if the box is in the last position
            if (pSlider.usCurrentIncrement >= pSlider.usNumberOfIncrements)
            {
                pSlider.usCurrentSliderBoxPosition = pSlider.usPos.Y + pSlider.Bounds.Height;// - pSlider.ubSliderHeight / 2;	// - minus box width
            }
            else if (pSlider.usCurrentIncrement == 0)
            {
                //else if the box is in the first position
                pSlider.usCurrentSliderBoxPosition = pSlider.usPos.Y;// - pSlider.ubSliderHeight / 2;
            }
            else
            {
                pSlider.usCurrentSliderBoxPosition = pSlider.usPos.Y + (pSlider.Bounds.Height / pSlider.usNumberOfIncrements * pSlider.usCurrentIncrement);
            }

            usMaxPos = pSlider.usPos.Y + pSlider.Bounds.Height;// - pSlider.ubSliderHeight//2 + 1;

            //if the box is past the edge, move it back
            if (pSlider.usCurrentSliderBoxPosition > usMaxPos)
            {
                pSlider.usCurrentSliderBoxPosition = usMaxPos;
            }
        }
        else
        {
            //if the box is in the last position
            if (pSlider.usCurrentIncrement == pSlider.usNumberOfIncrements)
            {
                pSlider.usCurrentSliderBoxPosition = pSlider.usPos.X + pSlider.Bounds.Width - 8 + 1;   // - minus box width
            }
            else
            {
                pSlider.usCurrentSliderBoxPosition = pSlider.usPos.X + (pSlider.Bounds.Width / pSlider.usNumberOfIncrements * pSlider.usCurrentIncrement);
            }

            usMaxPos = pSlider.usPos.X + pSlider.Bounds.Width - 8 + 1;

            //if the box is past the edge, move it back
            if (pSlider.usCurrentSliderBoxPosition > usMaxPos)
            {
                pSlider.usCurrentSliderBoxPosition = usMaxPos;
            }
        }
    }

    public void InitSliderSystem()
    {
        // load Slider Box Graphic graphic and add it
        video.GetVideoObject("INTERFACE\\SliderBox.sti", out this.guiSliderBoxImageTag);

        gfSliderInited = true;
    }

    public Slider AddSlider(
        SliderStyle style,
        CURSOR usCursor,
        Point loc,
        int usWidth,
        int usNumberOfIncrements,
        MSYS_PRIORITY sPriority,
        SliderChangeCallback SliderChangeCallback,
        SliderDirection uiFlags)
    {
        Slider? pTemp = null;
        Slider? pNewSlider = null;
        int iNewID = 0;
        uint cnt = 0;
        ushort usIncrementWidth = 0;

        pNewSlider = new();

        //Assign the settings to the current slider
        pNewSlider.ubStyle = style;
        pNewSlider.usPos = loc;

        //	pNewSlider.usWidth = usWidth;
        pNewSlider.usNumberOfIncrements = usNumberOfIncrements;
        pNewSlider.SliderChangeCallback = SliderChangeCallback;
        pNewSlider.usCurrentIncrement = 0;
        pNewSlider.usBackGroundColor = Color.FromRgb(255, 255, 255);
        pNewSlider.uiFlags = uiFlags;

        //Get a new Identifier for the slider
        //Temp just increment for now
        pNewSlider.uiSliderID = this.Sliders.Count;

        //
        // Create the mouse regions for each increment in the slider
        //

        //add the region
        loc = pNewSlider.usPos;
        // usPos.X = pNewSlider.usPos.X;
        // usPos.Y = pNewSlider.usPos.Y;

        //Add the last one, the width will be whatever is left over
        switch (style)
        {
            case SliderStyle.SLIDER_VERTICAL_STEEL:

                pNewSlider.uiFlags |= SliderDirection.SLIDER_VERTICAL;
                //                    pNewSlider.usWidth = Slider.STEEL_SLIDER_WIDTH;
                //                    pNewSlider.usHeight = usWidth;
                pNewSlider.ubSliderWidth = Slider.STEEL_SLIDER_WIDTH;
                pNewSlider.ubSliderHeight = Slider.STEEL_SLIDER_HEIGHT;


                MouseSubSystem.MSYS_DefineRegion(
                    pNewSlider.ScrollAreaMouseRegion,
                    new Rectangle(
                        loc.X - pNewSlider.ubSliderWidth / 2,
                        loc.Y,
                        loc.X + pNewSlider.ubSliderWidth / 2,
                        pNewSlider.usPos.Y + pNewSlider.ubSliderHeight),
                    sPriority,
                    usCursor,
                    this.SelectedSliderMovementCallBack,
                    this.SelectedSliderButtonCallBack);

                MouseSubSystem.MSYS_SetRegionUserData(pNewSlider.ScrollAreaMouseRegion, 1, pNewSlider.uiSliderID);
                break;

            case SliderStyle.SLIDER_DEFAULT_STYLE:
            default:

                pNewSlider.uiFlags |= SliderDirection.SLIDER_HORIZONTAL;
                pNewSlider.ubSliderWidth = usWidth;
                pNewSlider.ubSliderHeight = Slider.DEFAULT_SLIDER_SIZE;

                MouseSubSystem.MSYS_DefineRegion(
                    pNewSlider.ScrollAreaMouseRegion,
                    new Rectangle(loc.X, loc.Y - Slider.DEFAULT_SLIDER_SIZE, pNewSlider.usPos.X + pNewSlider.ubSliderWidth, loc.Y + Slider.DEFAULT_SLIDER_SIZE),
                    sPriority,
                    usCursor,
                    this.SelectedSliderMovementCallBack,
                    this.SelectedSliderButtonCallBack);

                MouseSubSystem.MSYS_SetRegionUserData(pNewSlider.ScrollAreaMouseRegion, 1, pNewSlider.uiSliderID);
                break;
        }

        this.Sliders.Add(pNewSlider);

        this.CalculateNewSliderBoxPosition(ref pNewSlider);

        return pNewSlider;
    }

    private void SelectedSliderMovementCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON reason)
    {
        int uiSelectedSlider;
        Slider? pSlider = null;

        //if we already have an anchored slider bar
        if (this.gpCurrentSlider != null)
        {
            return;
        }

        if (reason.HasFlag(MSYS_CALLBACK_REASON.LOST_MOUSE))
        {
            MouseRegionFlags flag = (MouseRegionFlags)ButtonFlags.BUTTON_CLICKED_ON;
            pRegion.uiFlags &= ~flag;

            if (this.inputs.gfLeftButtonState)
            {
                uiSelectedSlider = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 1);
                pSlider = this.GetSliderFromID(uiSelectedSlider);
                if (pSlider == null)
                {
                    return;
                }

                // set the currently selectd slider bar
                if (this.inputs.gfLeftButtonState)
                {
                    this.gpCurrentSlider = pSlider;
                }


                if (pSlider.uiFlags.HasFlag(SliderDirection.SLIDER_VERTICAL))
                {
                    this.CalculateNewSliderIncrement(ref pSlider, pRegion.RelativeMousePos.Y);
                }
                else
                {
                    this.CalculateNewSliderIncrement(ref pSlider, pRegion.RelativeMousePos.X);
                }
            }
        }
        else if (reason.HasFlag(MSYS_CALLBACK_REASON.GAIN_MOUSE))
        {
            MouseRegionFlags flag = (MouseRegionFlags)ButtonFlags.BUTTON_CLICKED_ON;
            pRegion.uiFlags |= flag;

            if (this.inputs.gfLeftButtonState)
            {
                uiSelectedSlider = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 1);
                pSlider = this.GetSliderFromID(uiSelectedSlider);
                if (pSlider == null)
                {
                    return;
                }

                // set the currently selectd slider bar
                //			gpCurrentSlider = pSlider;


                if (pSlider.uiFlags.HasFlag(SliderDirection.SLIDER_VERTICAL))
                {
                    this.CalculateNewSliderIncrement(ref pSlider, pRegion.RelativeMousePos.Y);
                }
                else
                {
                    this.CalculateNewSliderIncrement(ref pSlider, pRegion.RelativeMousePos.X);
                }

            }
        }

        else if (reason.HasFlag(MSYS_CALLBACK_REASON.MOVE))
        {
            MouseRegionFlags flag = (MouseRegionFlags)ButtonFlags.BUTTON_CLICKED_ON;
            pRegion.uiFlags |= flag;

            if (this.inputs.gfLeftButtonState)
            {
                uiSelectedSlider = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 1);
                pSlider = this.GetSliderFromID(uiSelectedSlider);
                if (pSlider == null)
                {
                    return;
                }

                // set the currently selectd slider bar
                //			gpCurrentSlider = pSlider;

                if (pSlider.uiFlags.HasFlag(SliderDirection.SLIDER_VERTICAL))
                {
                    this.CalculateNewSliderIncrement(ref pSlider, pRegion.RelativeMousePos.Y);
                }
                else
                {
                    this.CalculateNewSliderIncrement(ref pSlider, pRegion.RelativeMousePos.X);
                }
            }
        }
    }



    void SelectedSliderButtonCallBack(ref MOUSE_REGION pRegion, MSYS_CALLBACK_REASON iReason)
    {
        int uiSelectedSlider;
        Slider? pSlider = null;

        //if we already have an anchored slider bar
        if (this.gpCurrentSlider != null)
        {
            return;
        }

        if (iReason.HasFlag(MSYS_CALLBACK_REASON.INIT))
        {
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_DWN))
        {
            uiSelectedSlider = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 1);

            pSlider = this.GetSliderFromID(uiSelectedSlider);
            if (pSlider == null)
            {
                return;
            }


            /*		// set the currently selectd slider bar
                    if( gfLeftButtonState )
                    {
                        gpCurrentSlider = pSlider;
                    }
            */

            if (pSlider.uiFlags.HasFlag(SliderDirection.SLIDER_VERTICAL))
            {
                this.CalculateNewSliderIncrement(ref pSlider, pRegion.RelativeMousePos.Y);
            }
            else
            {
                this.CalculateNewSliderIncrement(ref pSlider, pRegion.RelativeMousePos.X);
            }
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_REPEAT))
        {
            uiSelectedSlider = (int)MouseSubSystem.MSYS_GetRegionUserData(ref pRegion, 1);

            pSlider = this.GetSliderFromID(uiSelectedSlider);
            if (pSlider == null)
            {
                return;
            }

            // set the currently selectd slider bar
            /*		if( gfLeftButtonState )
                    {
                        gpCurrentSlider = pSlider;
                    }
            */

            if (pSlider.uiFlags.HasFlag(SliderDirection.SLIDER_VERTICAL))
            {
                this.CalculateNewSliderIncrement(ref pSlider, pRegion.RelativeMousePos.Y);
            }
            else
            {
                this.CalculateNewSliderIncrement(ref pSlider, pRegion.RelativeMousePos.X);
            }
        }
        else if (iReason.HasFlag(MSYS_CALLBACK_REASON.LBUTTON_UP))
        {
        }
    }

    private Slider? GetSliderFromID(int sliderIdx)
    {
        return this.Sliders.FirstOrDefault(s => s.uiSliderID == sliderIdx);
    }

    public void SetSliderValue(ref Slider slider, int newValue)
    {
    }

    internal static void RemoveSliderBar(Slider slider)
    {
        MouseSubSystem.MSYS_RemoveRegion(slider.ScrollAreaMouseRegion);
    }

    public static void ShutDownSlider()
    {
        gfSliderInited = false;
    }
}

public class Slider
{
    public const int DEFAULT_SLIDER_SIZE = 7;
    public const int STEEL_SLIDER_WIDTH = 42;
    public const int STEEL_SLIDER_HEIGHT = 25;

    public int uiSliderID;

    public SliderStyle ubStyle;
    public Rectangle Bounds;
    public Point usPos;

    public int usNumberOfIncrements;
    public SliderChangeCallback SliderChangeCallback;

    public int usCurrentIncrement;

    public Color usBackGroundColor;

    public MOUSE_REGION ScrollAreaMouseRegion = new(nameof(Slider));

    public uint uiSliderBoxImage;
    public int usCurrentSliderBoxPosition;

    public Rectangle LastRect;

    public SliderDirection uiFlags;

    public int ubSliderWidth;
    public int ubSliderHeight;
}

public enum SliderStyle
{
    SLIDER_DEFAULT_STYLE,
    SLIDER_VERTICAL_STEEL,
    NUM_SLIDER_STYLES,
};

public enum SliderDirection
{
    SLIDER_VERTICAL = 0x00000001,
    SLIDER_HORIZONTAL = 0x00000002,
}
