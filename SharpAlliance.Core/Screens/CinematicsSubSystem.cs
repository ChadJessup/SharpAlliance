using System;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Platform.Interfaces;
using Veldrid.Sdl2;

namespace SharpAlliance.Core.Screens
{
    public class CinematicsSubSystem
    {
        public void SmkInitialize(IVideoManager video, int width, int height)
        {
        }


    }

    public struct SMKFLIC
    {
        public string cFilename;
        //		HFILE										hFileHandle;
        // HWFILE hFileHandle;
        // Smack SmackHandle;
        // SmackBuf SmackBuffer;
        public int uiFlags;
        //LPDIRECTDRAWSURFACE2 lpDDS;
        public Sdl2Window hWindow;
        public int  uiFrame;
        public int  uiLeft, uiTop;
        //		LPDIRECTDRAW2						lpDD;
        //		UINT32									uiNumFrames;
        //		Ubyte										*pAudioData;
        //		Ubyte										*pCueData;
    }
}
