using System;
using System.Collections.Generic;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.SubSystems;
using SixLabors.ImageSharp;

namespace SharpAlliance.Core.Managers
{
    public delegate void SliderChangeCallback(int newValue);

    public class SliderSubSystem
    {
        private readonly IInputManager inputs;
        private readonly IVideoManager video;

        private string guiSliderBoxImageTag;
        private bool gfSliderInited;

        public SliderSubSystem(
            IInputManager inputManager,
            IVideoManager videoManager)
        {
            this.inputs = inputManager;
            this.video = videoManager;
        }

        public List<Slider> Sliders { get; } = new();

        public Slider? gpCurrentSlider = null;

        public void RenderSliderBars()
        {
            Slider pTemp = null;

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

            foreach(var slider in this.Sliders)
            {
                this.RenderSelectedSliderBar(slider);
            }
        }

        private void RenderSelectedSliderBar(Slider slider)
        {
        }

        private void CalculateNewSliderIncrement(ref Slider pSlider, int usPos)
        {
            float dNewIncrement = 0.0f;
            int usOldIncrement;
            bool fLastSpot  = false;
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

        public void InitSlider()
        {
            VOBJECT_DESC VObjectDesc = new();

            // load Slider Box Graphic graphic and add it
            VObjectDesc.fCreateFlags = VideoObjectCreateFlags.VOBJECT_CREATE_FROMFILE;
            VObjectDesc.ImageFile = Utils.FilenameForBPP("INTERFACE\\SliderBox.sti");
            this.video.AddVideoObject(ref VObjectDesc, out this.guiSliderBoxImageTag);

            this.gfSliderInited = true;
        }

        public Slider AddSlider(
            SliderStyle style,
            Cursor cursor,
            int oPT_SOUND_EFFECTS_SLIDER_X, 
            int oPT_SOUND_EFFECTS_SLIDER_Y, 
            int oPT_SLIDER_BAR_SIZE, 
            int v1, 
            MSYS_PRIORITY priority, 
            SliderChangeCallback soundFXSliderChangeCallBack, 
            int v2)
        {
            return new Slider();
        }

        public void SetSliderValue(ref Slider slider, int newValue)
        {
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

        Color usBackGroundColor;

        public MouseRegion ScrollAreaMouseRegion;

        public uint uiSliderBoxImage;
        public int usCurrentSliderBoxPosition;

        Rectangle LastRect;

        public SliderDirection uiFlags;

        public int ubSliderWidth;
        public int ubSliderHeight;

        //struct TAG_SLIDER       *pNext;
        //struct TAG_SLIDER       *pPrev;
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
}
