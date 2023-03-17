using System;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Platform.Interfaces;
using Veldrid.Sdl2;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Screens;

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
    // Stream hFileHandle;
    // Smack SmackHandle;
    // SmackBuf SmackBuffer;
    public int uiFlags;
    //LPDIRECTDRAWSURFACE2 lpDDS;
    public Sdl2Window hWindow;
    public int  uiFrame;
    public int  uiLeft, uiTop;
    //		LPDIRECTDRAW2						lpDD;
    //		int									uiNumFrames;
    //		Ubyte										*pAudioData;
    //		Ubyte										*pCueData;
}
