﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Managers.VideoSurfaces;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class Faces
{
    private readonly FontSubSystem fonts;
    private static IVideoManager video;
    public Faces(IVideoManager videoManager, FontSubSystem fontSubSystem)
    {
        video = videoManager;
        this.fonts = fontSubSystem;
    }

    private static int GetFreeFace()
    {
        int uiCount;

        for (uiCount = 0; uiCount < guiNumFaces; uiCount++)
        {
            if (gFacesData[uiCount].fAllocated == false)
            {
                return (int)uiCount;
            }
        }

        if (guiNumFaces < NUM_FACE_SLOTS)
        {
            return (int)guiNumFaces++;
        }

        return -1;
    }

    private static void RecountFaces()
    {
        int uiCount;

        for (uiCount = guiNumFaces - 1; uiCount >= 0; uiCount--)
        {
            if (gFacesData[uiCount].fAllocated)
            {
                guiNumFaces = (int)(uiCount + 1);
                break;
            }
        }
    }


    public static int InitSoldierFace(SOLDIERTYPE? pSoldier)
    {
        int iFaceIndex;

        // Check if we have a face init already
        iFaceIndex = pSoldier.iFaceIndex;

        if (iFaceIndex != -1)
        {
            return iFaceIndex;
        }

        return InitFace(pSoldier.ubProfile, pSoldier.ubID, 0);
    }


    public static int InitFace(NPCID usMercProfileID, int ubSoldierID, FACE uiInitFlags)
    {
        int uiBlinkFrequency = 0;
        int uiExpressionFrequency = 0;

        if (usMercProfileID == NO_PROFILE)
        {
            return -1;
        }

        uiBlinkFrequency = gMercProfiles[usMercProfileID].uiBlinkFrequency;
        uiExpressionFrequency = gMercProfiles[usMercProfileID].uiExpressionFrequency;

        if (Globals.Random.Next(2) > 0)
        {
            uiBlinkFrequency += Globals.Random.Next(2000);
        }
        else
        {
            uiBlinkFrequency -= Globals.Random.Next(2000);
        }

        return InternalInitFace(usMercProfileID, ubSoldierID, uiInitFlags, gMercProfiles[usMercProfileID].ubFaceIndex, uiBlinkFrequency, uiExpressionFrequency);

    }


    private static int InternalInitFace(NPCID usMercProfileID, int ubSoldierID, FACE uiInitFlags, int iFaceFileID, int uiBlinkFrequency, int uiExpressionFrequency)
    {
        FACETYPE? pFace = null;
        VOBJECT_DESC VObjectDesc = new();
        string uiVideoObject = string.Empty;
        int iFaceIndex = 0;
        ETRLEObject ETRLEObject = new();
        HVOBJECT? hVObject = null;
        List<Rgba32> Pal = new(256);

        if ((iFaceIndex = GetFreeFace()) == (-1))
        {
            return -1;
        }

        // Load face file
        // 

        // ATE: If we are merc profile ID #151-154, all use 151's protrait....
        if (usMercProfileID >= (NPCID)151 && usMercProfileID <= (NPCID)154)
        {
            iFaceFileID = 151;
        }


        // Check if we are a big-face....
        if (uiInitFlags.HasFlag(FACE.BIGFACE))
        {
            // The filename is the profile ID!
            if (iFaceFileID < 100)
            {
                VObjectDesc.ImageFile = $"FACES\\b{iFaceFileID:D2}.sti";
            }
            else
            {
                VObjectDesc.ImageFile = $"FACES\\b{iFaceFileID:D3}.sti";
            }

            // ATE: Check for profile - if elliot , use special face :)
            if (usMercProfileID == NPCID.ELLIOT)
            {
                if (gMercProfiles[NPCID.ELLIOT].bNPCData > 3 && gMercProfiles[NPCID.ELLIOT].bNPCData < 7)
                {
                    VObjectDesc.ImageFile = $"FACES\\b{iFaceFileID:D2}a.sti";
                }
                else if (gMercProfiles[NPCID.ELLIOT].bNPCData > 6 && gMercProfiles[NPCID.ELLIOT].bNPCData < 10)
                {
                    VObjectDesc.ImageFile = $"FACES\\b{iFaceFileID:D2}b.sti";
                }
                else if (gMercProfiles[NPCID.ELLIOT].bNPCData > 9 && gMercProfiles[NPCID.ELLIOT].bNPCData < 13)
                {
                    VObjectDesc.ImageFile = $"FACES\\b{iFaceFileID:D2}c.sti";
                }
                else if (gMercProfiles[NPCID.ELLIOT].bNPCData > 12 && gMercProfiles[NPCID.ELLIOT].bNPCData < 16)
                {
                    VObjectDesc.ImageFile = $"FACES\\b{iFaceFileID:D2}d.sti";
                }
                else if (gMercProfiles[NPCID.ELLIOT].bNPCData == 17)
                {
                    VObjectDesc.ImageFile = $"FACES\\b{iFaceFileID:D2}e.sti";
                }
            }
        }
        else
        {

            if (iFaceFileID < 100)
            {
                // The filename is the profile ID!
                VObjectDesc.ImageFile = $"FACES\\{iFaceFileID:D2}.sti";
            }
            else
            {
                VObjectDesc.ImageFile = $"FACES\\{iFaceFileID:D3}.sti";
            }
        }

        // Load
        if (video.GetVideoObject(VObjectDesc.ImageFile, out uiVideoObject) is null)
        {
            // If we are a big face, use placeholder...
            if (uiInitFlags.HasFlag(FACE.BIGFACE))
            {
                sprintf(VObjectDesc.ImageFile, "FACES\\placeholder.sti");
        
                if (video.GetVideoObject(VObjectDesc.ImageFile, out uiVideoObject) is null)
                {
                    return -1;
                }
            }
            else
            {
                return -1;
            }
        }

        gFacesData[iFaceIndex] = new();

        pFace = gFacesData[iFaceIndex];

        // Get profile data and set into face data
        pFace.ubSoldierID = ubSoldierID;

        pFace.iID = iFaceIndex;
        pFace.fAllocated = true;

        //Default to off!
        pFace.fDisabled = true;
        pFace.iVideoOverlay = -1;
        //pFace.uiEyeDelay			=	gMercProfiles[ usMercProfileID ].uiEyeDelay;
        //pFace.uiMouthDelay		= gMercProfiles[ usMercProfileID ].uiMouthDelay;
        pFace.uiEyeDelay = 50 + Globals.Random.Next(30);
        pFace.uiMouthDelay = 120;
        pFace.ubCharacterNum = usMercProfileID;


        pFace.uiBlinkFrequency = uiBlinkFrequency;
        pFace.uiExpressionFrequency = uiExpressionFrequency;

        pFace.sEyeFrame = 0;
        pFace.sMouthFrame = 0;
        pFace.uiFlags = uiInitFlags;


        // Set palette
        hVObject = null; //video.GetVideoObject(uiVideoObject);
        if (hVObject is not null)
        {
            // Build a grayscale palette! ( for testing different looks )
            for (int uiCount = 0; uiCount < 256; uiCount++)
            {
                Pal.Add(new()
                {
                    R = 255,
                    G = 255,
                    B = 255,
                });
            }

            hVObject.pShades[(ushort)FLASH_PORTRAIT.NOSHADE] = video.Create16BPPPaletteShaded(hVObject.Palette, 255, 255, 255, false);
            hVObject.pShades[(ushort)FLASH_PORTRAIT.STARTSHADE] = video.Create16BPPPaletteShaded(Pal.ToArray(), 255, 255, 255, false);
            hVObject.pShades[(ushort)FLASH_PORTRAIT.ENDSHADE] = video.Create16BPPPaletteShaded(hVObject.Palette, 250, 25, 25, true);
            hVObject.pShades[(ushort)FLASH_PORTRAIT.DARKSHADE] = video.Create16BPPPaletteShaded(hVObject.Palette, 100, 100, 100, true);
            hVObject.pShades[(ushort)FLASH_PORTRAIT.LITESHADE] = video.Create16BPPPaletteShaded(hVObject.Palette, 100, 100, 100, false);

            for (int uiCount = 0; uiCount < 256; uiCount++)
            {
                var pal = Pal[uiCount];
                pal.R = (byte)((byte)(uiCount % 128) + 128);
                pal.G = (byte)((byte)(uiCount % 128) + 128);
                pal.B = (byte)((byte)(uiCount % 128) + 128);
            }
            hVObject.pShades[(ushort)FLASH_PORTRAIT.GRAYSHADE] = video.Create16BPPPaletteShaded(Pal.ToArray(), 255, 255, 255, false);

        }


        // Get FACE height, width
        if (video.GetVideoObjectETRLEPropertiesFromIndex(uiVideoObject, out ETRLEObject, 0) == false)
        {
            return -1;
        }

        pFace.usFaceSize.Width = ETRLEObject.usWidth;
        pFace.usFaceSize.Height = ETRLEObject.usHeight;


        // OK, check # of items
        if (hVObject.usNumberOfObjects == 8)
        {
            pFace.fInvalidAnim = false;

            // Get EYE height, width
            if (video.GetVideoObjectETRLEPropertiesFromIndex(uiVideoObject, out ETRLEObject, 1) == false)
            {
                return -1;
            }

            pFace.usEyesWidth = ETRLEObject.usWidth;
            pFace.usEyesHeight = ETRLEObject.usHeight;


            // Get Mouth height, width
            if (video.GetVideoObjectETRLEPropertiesFromIndex(uiVideoObject, out ETRLEObject, 5) == false)
            {
                return -1;
            }

            pFace.usMouthWidth = ETRLEObject.usWidth;
            pFace.usMouthHeight = ETRLEObject.usHeight;
        }
        else
        {
            pFace.fInvalidAnim = true;
        }

        // Set id
        pFace.uiVideoObject = uiVideoObject;

        return iFaceIndex;
    }


    public static void DeleteSoldierFace(SOLDIERTYPE pSoldier)
    {
        DeleteFace(pSoldier.iFaceIndex);

        pSoldier.iFaceIndex = -1;
    }

    public static void DeleteFace(int iFaceIndex)
    {
        FACETYPE? pFace;

        // Check face index
        //        CHECKV(iFaceIndex != -1);

        pFace = gFacesData[iFaceIndex];

        // Check for a valid slot!
        //        CHECKV(pFace.fAllocated != false);

        pFace.fCanHandleInactiveNow = true;

        if (!pFace.fDisabled)
        {
            SetAutoFaceInActive(iFaceIndex);
        }

        // If we are still talking, stop!
        if (pFace.fTalking)
        {
            // Call dialogue handler function
            pFace.fTalking = false;

            //            HandleDialogueEnd(pFace);
        }

        // Delete vo	
        //        DeleteVideoObjectFromIndex(pFace.uiVideoObject);

        // Set uncallocated
        pFace.fAllocated = false;

        RecountFaces();

    }

    void SetAutoFaceActiveFromSoldier(SurfaceType uiDisplayBuffer, SurfaceType uiRestoreBuffer, int ubSoldierID, int usFaceX, int usFaceY)
    {
        if (ubSoldierID == NOBODY)
        {
            return;
        }

        this.SetAutoFaceActive(uiDisplayBuffer, uiRestoreBuffer, MercPtrs[ubSoldierID].iFaceIndex, usFaceX, usFaceY);

    }

    public static void GetFaceRelativeCoordinates(FACETYPE? pFace, out int pusEyesX, out int pusEyesY, out int pusMouthX, out int pusMouthY)
    {
        NPCID usMercProfileID;
        int usEyesX;
        int usEyesY;
        int usMouthX;
        int usMouthY;
        int cnt;

        usMercProfileID = pFace.ubCharacterNum;

        //Take eyes x,y from profile unless we are an RPC and we are small faced.....
        usEyesX = gMercProfiles[usMercProfileID].usEyesX;
        usEyesY = gMercProfiles[usMercProfileID].usEyesY;
        usMouthY = gMercProfiles[usMercProfileID].usMouthY;
        usMouthX = gMercProfiles[usMercProfileID].usMouthX;

        // Use some other values for x,y, base on if we are a RPC!
        if (!pFace.uiFlags.HasFlag(FACE.BIGFACE) || pFace.uiFlags.HasFlag(FACE.FORCE_SMALL))
        {
            // Are we a recruited merc? .. or small?
            if (gMercProfiles[usMercProfileID].ubMiscFlags.HasFlag(PROFILE_MISC_FLAG.RECRUITED | PROFILE_MISC_FLAG.EPCACTIVE)
                || pFace.uiFlags.HasFlag(FACE.FORCE_SMALL))
            {
                // Loop through all values of availible merc IDs to find ours!
                for (cnt = 0; cnt < ubRPCNumSmallFaceValues; cnt++)
                {
                    // We've found one!
                    if (gubRPCSmallFaceProfileNum[cnt] == usMercProfileID)
                    {
                        usEyesX = gRPCSmallFaceValues[cnt].bEyesX;
                        usEyesY = gRPCSmallFaceValues[cnt].bEyesY;
                        usMouthY = gRPCSmallFaceValues[cnt].bMouthY;
                        usMouthX = gRPCSmallFaceValues[cnt].bMouthX;
                    }
                }

            }
        }

        pusEyesX = usEyesX;
        pusEyesY = usEyesY;
        pusMouthX = usMouthX;
        pusMouthY = usMouthY;
    }


    void SetAutoFaceActive(SurfaceType uiDisplayBuffer, SurfaceType uiRestoreBuffer, int iFaceIndex, int usFaceX, int usFaceY)
    {
        FACETYPE? pFace;

        // Check face index
        //        CHECKV(iFaceIndex != -1);

        pFace = gFacesData[iFaceIndex];

        GetFaceRelativeCoordinates(pFace, out int usEyesX, out int usEyesY, out int usMouthX, out int usMouthY);

        this.InternalSetAutoFaceActive(uiDisplayBuffer, uiRestoreBuffer, iFaceIndex, usFaceX, usFaceY, usEyesX, usEyesY, usMouthX, usMouthY);

    }


    void InternalSetAutoFaceActive(SurfaceType uiDisplayBuffer, SurfaceType uiRestoreBuffer, int iFaceIndex, int usFaceX, int usFaceY, int usEyesX, int usEyesY, int usMouthX, int usMouthY)
    {
        NPCID usMercProfileID = 0;
        FACETYPE? pFace = null;
        VSURFACE_DESC vs_desc = new();
        int usWidth = 0;
        int usHeight = 0;
        int ubBitDepth = 0;

        // Check face index
        //        CHECKV(iFaceIndex != -1);

        pFace = gFacesData[iFaceIndex];

        // IF we are already being contained elsewhere, return without doing anything!

        // ATE: Don't allow another activity from setting active....
        if (pFace.uiFlags.HasFlag(FACE.INACTIVE_HANDLED_ELSEWHERE))
        {
            return;
        }

        // Check if we are active already, remove if so!
        if (pFace.fDisabled)
        {
            SetAutoFaceInActive(iFaceIndex);
        }

        if (uiRestoreBuffer == FACE_AUTO_RESTORE_BUFFER)
        {
            // BUILD A BUFFER
            //            GetCurrentVideoSettings(usWidth, usHeight, ubBitDepth);
            // OK, ignore screen widths, height, only use BPP 
            //            vs_desc.fCreateFlags = VSURFACE_CREATE_DEFAULT | VSURFACE_SYSTEM_MEM_USAGE;
            vs_desc.usWidth = pFace.usFaceSize.Width;
            vs_desc.usHeight = pFace.usFaceSize.Height;
            vs_desc.ubBitDepth = ubBitDepth;

            pFace.fAutoRestoreBuffer = true;

            //            CHECKV(video.AddVideoObject(out vs_desc, out pFace.uiAutoRestoreBuffer) > 0);
        }
        else
        {
            pFace.fAutoRestoreBuffer = false;
            pFace.uiAutoRestoreBuffer = uiRestoreBuffer;
        }

        if (uiDisplayBuffer == FACE_AUTO_DISPLAY_BUFFER)
        {
            // BUILD A BUFFER
            //            GetCurrentVideoSettings(usWidth, usHeight, ubBitDepth);
            // OK, ignore screen widths, height, only use BPP 
            //            vs_desc.fCreateFlags = VSURFACE_CREATE_DEFAULT | VSURFACE_SYSTEM_MEM_USAGE;
            vs_desc.usWidth = pFace.usFaceSize.Width;
            vs_desc.usHeight = pFace.usFaceSize.Height;
            vs_desc.ubBitDepth = ubBitDepth;

            pFace.fAutoDisplayBuffer = true;

            //            CHECKV(AddVideoObject(vs_desc, (pFace.uiAutoDisplayBuffer)));
        }
        else
        {
            pFace.fAutoDisplayBuffer = false;
            pFace.uiAutoDisplayBuffer = uiDisplayBuffer;
        }


        usMercProfileID = pFace.ubCharacterNum;

        pFace.usFaceLocation.X = usFaceX;
        pFace.usFaceLocation.Y = usFaceY;
        pFace.fCanHandleInactiveNow = false;


        //Take eyes x,y from profile unless we are an RPC and we are small faced.....
        pFace.usEyesX = usEyesX + usFaceX;
        pFace.usEyesY = usEyesY + usFaceY;
        pFace.usMouthY = usMouthY + usFaceY;
        pFace.usMouthX = usMouthX + usFaceX;

        // Save offset values
        pFace.usEyesOffsetX = usEyesX;
        pFace.usEyesOffsetY = usEyesY;
        pFace.usMouthOffsetY = usMouthY;
        pFace.usMouthOffsetX = usMouthX;


        if (pFace.usEyesY == usFaceY || pFace.usMouthY == usFaceY)
        {
            pFace.fInvalidAnim = true;
        }

        pFace.fDisabled = false;
        pFace.uiLastBlink = GetJA2Clock();
        pFace.uiLastExpression = GetJA2Clock();
        pFace.uiEyelast = GetJA2Clock();
        pFace.fStartFrame = true;

        // Are we a soldier?
        if (pFace.ubSoldierID != NOBODY)
        {
            pFace.bOldSoldierLife = (int)MercPtrs[pFace.ubSoldierID].bLife;
        }
    }


    public static void SetAutoFaceInActiveFromSoldier(int ubSoldierID)
    {
        // Check for valid soldier
        //        CHECKV(ubSoldierID != NOBODY);

        SetAutoFaceInActive(MercPtrs[ubSoldierID].iFaceIndex);
    }


    public static void SetAutoFaceInActive(int iFaceIndex)
    {
        FACETYPE? pFace;
        SOLDIERTYPE? pSoldier;

        // Check face index
        //        CHECKV(iFaceIndex != -1);

        pFace = gFacesData[iFaceIndex];

        // Check for a valid slot!
        //        CHECKV(pFace.fAllocated != false);


        // Turn off some flags
        if (pFace.uiFlags.HasFlag(FACE.INACTIVE_HANDLED_ELSEWHERE))
        {
            if (!pFace.fCanHandleInactiveNow)
            {
                return;
            }
        }

        if (pFace.uiFlags.HasFlag(FACE.MAKEACTIVE_ONCE_DONE))
        {
            // 
            if (pFace.ubSoldierID != NOBODY)
            {
                pSoldier = MercPtrs[pFace.ubSoldierID];

                // IF we are in tactical
                //                if (pSoldier.bAssignment == iCurrentTacticalSquad && guiCurrentScreen == ScreenName.GAME_SCREEN)
                {
                    // Make the interfac panel dirty..
                    // This will dirty the panel next frame...
                    gfRerenderInterfaceFromHelpText = true;
                }

            }

        }

        if (pFace.fAutoRestoreBuffer)
        {
            //            DeleteVideoSurfaceFromIndex(pFace.uiAutoRestoreBuffer);
        }

        if (pFace.fAutoDisplayBuffer)
        {
            //            DeleteVideoSurfaceFromIndex(pFace.uiAutoDisplayBuffer);
        }

        if (pFace.iVideoOverlay != -1)
        {
            //            RemoveVideoOverlay(pFace.iVideoOverlay);
            pFace.iVideoOverlay = -1;
        }

        // Turn off some flags
        pFace.uiFlags &= ~FACE.INACTIVE_HANDLED_ELSEWHERE;

        // Disable!
        pFace.fDisabled = true;

    }


    public static void SetAllAutoFacesInactive()
    {
        int uiCount;
        FACETYPE? pFace;

        for (uiCount = 0; uiCount < guiNumFaces; uiCount++)
        {
            if (gFacesData[uiCount].fAllocated)
            {
                pFace = gFacesData[uiCount];

                SetAutoFaceInActive(uiCount);
            }
        }
    }



    public void BlinkAutoFace(int iFaceIndex)
    {
        FACETYPE? pFace;
        int sFrame;
        bool fDoBlink = false;

        if (gFacesData[iFaceIndex].fAllocated && !gFacesData[iFaceIndex].fDisabled && !gFacesData[iFaceIndex].fInvalidAnim)
        {
            pFace = gFacesData[iFaceIndex];

            // CHECK IF BUDDY IS DEAD, UNCONSCIOUS, ASLEEP, OR POW!
            if (pFace.ubSoldierID != NOBODY)
            {
                if ((MercPtrs[pFace.ubSoldierID].bLife < OKLIFE) ||
                         (MercPtrs[pFace.ubSoldierID].fMercAsleep == true) ||
                         (MercPtrs[pFace.ubSoldierID].bAssignment == Assignment.ASSIGNMENT_POW))
                {
                    return;
                }
            }

            if (pFace.ubExpression == Expression.NO_EXPRESSION)
            {
                // Get Delay time, if the first frame, use a different delay
                if ((GetJA2Clock() - pFace.uiLastBlink) > pFace.uiBlinkFrequency)
                {
                    pFace.uiLastBlink = GetJA2Clock();
                    pFace.ubExpression = Expression.BLINKING;
                    pFace.uiEyelast = GetJA2Clock();
                }

                if (pFace.fAnimatingTalking)
                {
                    if ((GetJA2Clock() - pFace.uiLastExpression) > pFace.uiExpressionFrequency)
                    {
                        pFace.uiLastExpression = GetJA2Clock();

                        if (Globals.Random.Next(2) == 0)
                        {
                            pFace.ubExpression = Expression.ANGRY;
                        }
                        else
                        {
                            pFace.ubExpression = Expression.SURPRISED;
                        }
                    }

                }

            }

            if (pFace.ubExpression != Expression.NO_EXPRESSION)
            {
                if (pFace.fStartFrame)
                {
                    if ((GetJA2Clock() - pFace.uiEyelast) > pFace.uiEyeDelay) //> Globals.Random.Next( 10000 ) )
                    {
                        fDoBlink = true;
                        pFace.fStartFrame = false;
                    }
                }
                else
                {
                    if ((GetJA2Clock() - pFace.uiEyelast) > pFace.uiEyeDelay)
                    {
                        fDoBlink = true;
                    }
                }

                // Are we going to blink?
                if (fDoBlink)
                {
                    pFace.uiEyelast = GetJA2Clock();

                    // Adjust
                    NewEye(pFace);

                    sFrame = pFace.sEyeFrame;

                    if (sFrame >= 5)
                    {
                        sFrame = 4;
                    }

                    if (sFrame > 0)
                    {
                        // Blit Accordingly!
                        //                        BltVideoObjectFromIndex(pFace.uiAutoDisplayBuffer, pFace.uiVideoObject, (int)(sFrame), pFace.usEyesX, pFace.usEyesY, VO_BLT.SRCTRANSPARENCY, null);

                        if (pFace.uiAutoDisplayBuffer == SurfaceType.FRAME_BUFFER)
                        {
                            //                            InvalidateRegion(pFace.usEyesX, pFace.usEyesY, pFace.usEyesX + pFace.usEyesWidth, pFace.usEyesY + pFace.usEyesHeight);
                        }
                    }
                    else
                    {
                        //RenderFace( uiDestBuffer , uiCount );
                        pFace.ubExpression = Expression.NO_EXPRESSION;
                        // Update rects just for eyes

                        if (pFace.uiAutoRestoreBuffer == SurfaceType.SAVE_BUFFER)
                        {
                            FaceRestoreSavedBackgroundRect(iFaceIndex, pFace.usEyesX, pFace.usEyesY, pFace.usEyesX, pFace.usEyesY, pFace.usEyesWidth, pFace.usEyesHeight);
                        }
                        else
                        {
                            FaceRestoreSavedBackgroundRect(iFaceIndex, pFace.usEyesX, pFace.usEyesY, pFace.usEyesOffsetX, pFace.usEyesOffsetY, pFace.usEyesWidth, pFace.usEyesHeight);
                        }

                    }

                    this.HandleRenderFaceAdjustments(pFace, true, false, 0, pFace.usFaceLocation.X, pFace.usFaceLocation.Y, pFace.usEyesX, pFace.usEyesY);

                }
            }

        }

    }

    private static void HandleFaceHilights(FACETYPE? pFace, SurfaceType uiBuffer, int sFaceX, int sFaceY)
    {
        int uiDestPitchBYTES;
        int pDestBuf;
        int usLineColor;
        int iFaceIndex;

        iFaceIndex = pFace.iID;

        if (!gFacesData[iFaceIndex].fDisabled)
        {
            if (pFace.uiAutoDisplayBuffer == SurfaceType.FRAME_BUFFER && guiCurrentScreen == ScreenName.GAME_SCREEN)
            {
                // If we are highlighted, do this now!
                if (pFace.uiFlags.HasFlag(FACE.SHOW_WHITE_HILIGHT))
                {
                    // Lock buffer
                    //                    pDestBuf = LockVideoSurface(uiBuffer, out uiDestPitchBYTES);
                    //                    SetClippingRegionAndImageWidth(uiDestPitchBYTES, sFaceX - 2, sFaceY - 1, sFaceX + pFace.usFaceWidth + 4, sFaceY + pFace.usFaceHeight + 4);
                    //
                    //                    usLineColor = Get16BPPColor(FROMRGB(255, 255, 255));
                    //                    RectangleDraw(true, (sFaceX - 2), (sFaceY - 1), sFaceX + pFace.usFaceWidth + 1, sFaceY + pFace.usFaceHeight, usLineColor, pDestBuf);
                    //
                    //                    SetClippingRegionAndImageWidth(uiDestPitchBYTES, 0, 0, 640, 480);
                    //
                    //                    UnLockVideoSurface(uiBuffer);
                }
                else if (pFace.uiFlags.HasFlag(FACE.SHOW_MOVING_HILIGHT))
                {
                    if (pFace.ubSoldierID != NOBODY)
                    {
                        if (MercPtrs[pFace.ubSoldierID].bLife >= OKLIFE)
                        {
                            // Lock buffer
                            //                            pDestBuf = LockVideoSurface(uiBuffer, out uiDestPitchBYTES);
                            //                           SetClippingRegionAndImageWidth(uiDestPitchBYTES, sFaceX - 2, sFaceY - 1, sFaceX + pFace.usFaceWidth + 4, sFaceY + pFace.usFaceHeight + 4);
                            //
                            //                           if (MercPtrs[pFace.ubSoldierID].bStealthMode)
                            //                           {
                            //                               usLineColor = Get16BPPColor(FROMRGB(158, 158, 12));
                            //                           }
                            //                           else
                            //                           {
                            //                               usLineColor = Get16BPPColor(FROMRGB(8, 12, 118));
                            //                           }
                            //                           RectangleDraw(true, (sFaceX - 2), (sFaceY - 1), sFaceX + pFace.usFaceWidth + 1, sFaceY + pFace.usFaceHeight, usLineColor, pDestBuf);
                            //
                            //                           SetClippingRegionAndImageWidth(uiDestPitchBYTES, 0, 0, 640, 480);
                            //
                            //                           UnLockVideoSurface(uiBuffer);
                        }
                    }
                }
                else
                {
                    // ATE: Zero out any highlight boxzes....
                    // Lock buffer
                    //                    pDestBuf = LockVideoSurface(pFace.uiAutoDisplayBuffer, uiDestPitchBYTES);
                    //                    SetClippingRegionAndImageWidth(uiDestPitchBYTES, pFace.usFaceX - 2, pFace.usFaceY - 1, pFace.usFaceX + pFace.usFaceWidth + 4, pFace.usFaceY + pFace.usFaceHeight + 4);
                    //
                    //                    usLineColor = Get16BPPColor(FROMRGB(0, 0, 0));
                    //                    RectangleDraw(true, (pFace.usFaceX - 2), (pFace.usFaceY - 1), pFace.usFaceX + pFace.usFaceWidth + 1, pFace.usFaceY + pFace.usFaceHeight, usLineColor, pDestBuf);
                    //
                    //                    SetClippingRegionAndImageWidth(uiDestPitchBYTES, 0, 0, 640, 480);
                    //
                    //                    UnLockVideoSurface(pFace.uiAutoDisplayBuffer);
                }
            }
        }


        if (pFace.fCompatibleItems && !gFacesData[iFaceIndex].fDisabled)
        {
            // Lock buffer
            //            pDestBuf = LockVideoSurface(uiBuffer, uiDestPitchBYTES);
            //            SetClippingRegionAndImageWidth(uiDestPitchBYTES, sFaceX - 2, sFaceY - 1, sFaceX + pFace.usFaceWidth + 4, sFaceY + pFace.usFaceHeight + 4);
            //
            //            usLineColor = Get16BPPColor(FROMRGB(255, 0, 0));
            //            RectangleDraw(true, (sFaceX - 2), (sFaceY - 1), sFaceX + pFace.usFaceWidth + 1, sFaceY + pFace.usFaceHeight, usLineColor, pDestBuf);
            //
            //            SetClippingRegionAndImageWidth(uiDestPitchBYTES, 0, 0, 640, 480);
            //
            //            UnLockVideoSurface(uiBuffer);
        }

    }


    void MouthAutoFace(int iFaceIndex)
    {
        FACETYPE? pFace;
        int sFrame;

        if (gFacesData[iFaceIndex].fAllocated)
        {
            pFace = gFacesData[iFaceIndex];

            // Remove video overlay is present....
            if (pFace.uiFlags.HasFlag(FACE.DESTROY_OVERLAY))
            {
                //if ( pFace.iVideoOverlay != -1 )
                //{
                //	if ( pFace.uiStopOverlayTimer != 0 )
                //	{
                //		if ( ( GetJA2Clock( ) - pFace.uiStopOverlayTimer ) > END_FACE_OVERLAY_DELAY )
                //		{
                //	RemoveVideoOverlay( pFace.iVideoOverlay );
                //			pFace.iVideoOverlay = -1;
                //		}
                //	}
                //}
            }

            if (pFace.fTalking)
            {
                if (!gFacesData[iFaceIndex].fDisabled && !gFacesData[iFaceIndex].fInvalidAnim)
                {
                    if (pFace.fAnimatingTalking)
                    {
                        //                        PollAudioGap(pFace.uiSoundID, (pFace.GapList));

                        // Check if we have an audio gap
                        //                        if (pFace.GapList.audio_gap_active)
                        //                        {
                        //                            pFace.sMouthFrame = 0;
                        //
                        //                            if (pFace.uiAutoRestoreBuffer == Surfaces.SAVE_BUFFER)
                        //                            {
                        //                                FaceRestoreSavedBackgroundRect(iFaceIndex, pFace.usMouthX, pFace.usMouthY, pFace.usMouthX, pFace.usMouthY, pFace.usMouthWidth, pFace.usMouthHeight);
                        //                            }
                        //                            else
                        //                            {
                        //                                FaceRestoreSavedBackgroundRect(iFaceIndex, pFace.usMouthX, pFace.usMouthY, pFace.usMouthOffsetX, pFace.usMouthOffsetY, pFace.usMouthWidth, pFace.usMouthHeight);
                        //                            }
                        //
                        //                        }
                        //                        else
                        //                        {
                        //                            // Get Delay time
                        //                            if ((GetJA2Clock() - pFace.uiMouthlast) > pFace.uiMouthDelay)
                        //                            {
                        //                                pFace.uiMouthlast = (int)GetJA2Clock();
                        //
                        //                                // Adjust
                        //                                NewMouth(pFace);
                        //
                        //                                sFrame = pFace.sMouthFrame;
                        //
                        //                                if (sFrame > 0)
                        //                                {
                        //                                    // Blit Accordingly!
                        //                                    BltVideoObjectFromIndex(pFace.uiAutoDisplayBuffer, pFace.uiVideoObject, (int)(sFrame + 4), pFace.usMouthX, pFace.usMouthY, VO_BLT.SRCTRANSPARENCY, null);
                        //
                        //                                    // Update rects
                        //                                    if (pFace.uiAutoDisplayBuffer == Surfaces.FRAME_BUFFER)
                        //                                    {
                        //                                        InvalidateRegion(pFace.usMouthX, pFace.usMouthY, pFace.usMouthX + pFace.usMouthWidth, pFace.usMouthY + pFace.usMouthHeight);
                        //                                    }
                        //                                }
                        //                                else
                        //                                {
                        //                                    //RenderFace( uiDestBuffer , uiCount );
                        //                                    //pFace.fTaking = false;
                        //                                    // Update rects just for Mouth
                        //                                    if (pFace.uiAutoRestoreBuffer == Surfaces.SAVE_BUFFER)
                        //                                    {
                        //                                        FaceRestoreSavedBackgroundRect(iFaceIndex, pFace.usMouthX, pFace.usMouthY, pFace.usMouthX, pFace.usMouthY, pFace.usMouthWidth, pFace.usMouthHeight);
                        //                                    }
                        //                                    else
                        //                                    {
                        //                                        FaceRestoreSavedBackgroundRect(iFaceIndex, pFace.usMouthX, pFace.usMouthY, pFace.usMouthOffsetX, pFace.usMouthOffsetY, pFace.usMouthWidth, pFace.usMouthHeight);
                        //                                    }
                        //
                        //                                }
                        //
                        //                                HandleRenderFaceAdjustments(pFace, true, false, 0, pFace.usFaceX, pFace.usFaceY, pFace.usEyesX, pFace.usEyesY);
                        //
                        //                            }
                        //                        }
                    }
                }
            }

            if (!pFace.uiFlags.HasFlag(FACE.INACTIVE_HANDLED_ELSEWHERE))
            {
                HandleFaceHilights(pFace, pFace.uiAutoDisplayBuffer, pFace.usFaceLocation.X, pFace.usFaceLocation.Y);
            }
        }
    }


    public static void HandleTalkingAutoFace(int iFaceIndex)
    {
        FACETYPE? pFace;

        if (gFacesData[iFaceIndex].fAllocated)
        {
            pFace = gFacesData[iFaceIndex];

            if (pFace.fTalking)
            {
                // Check if we are done!	( Check this first! )
                if (pFace.fValidSpeech)
                {
                    // Check if we have finished, set some flags for the final delay down if so!
                    //                    if (!SoundIsPlaying(pFace.uiSoundID) && !pFace.fFinishTalking)
                    //                    {
                    //                        SetupFinalTalkingDelay(pFace);
                    //                    }
                }
                else
                {
                    // Check if our delay is over
                    if (!pFace.fFinishTalking)
                    {
                        if ((GetJA2Clock() - pFace.uiTalkingTimer) > pFace.uiTalkingDuration)
                        {
                            // If here, setup for last delay!
                            SetupFinalTalkingDelay(pFace);

                        }
                    }
                }

                // Now check for end of talking
                if (pFace.fFinishTalking)
                {
                    if ((GetJA2Clock() - pFace.uiTalkingTimer) > pFace.uiTalkingDuration)
                    {
                        pFace.fTalking = false;
                        pFace.fAnimatingTalking = false;

                        // Remove gap info
                        //                        AudioGapListDone((pFace.GapList));

                        // Remove video overlay is present....
                        if (pFace.iVideoOverlay != -1)
                        {
                            //if ( pFace.uiStopOverlayTimer == 0 )
                            //{
                            //	pFace.uiStopOverlayTimer = GetJA2Clock();
                            //}
                        }

                        // Call dialogue handler function
                        //                        HandleDialogueEnd(pFace);
                    }
                }
            }
        }
    }


    // Local function - uses these variables because they have already been validated
    private static void SetFaceShade(SOLDIERTYPE? pSoldier, FACETYPE? pFace, bool fExternBlit)
    {
        // Set to default
        //        SetObjectHandleShade(pFace.uiVideoObject, FLASH_PORTRAIT.NOSHADE);

        if (pFace.iVideoOverlay == -1 && !fExternBlit)
        {
            if ((pSoldier.bActionPoints == 0) && !gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.REALTIME) && gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.INCOMBAT))
            {
                //                SetObjectHandleShade(pFace.uiVideoObject, FLASH_PORTRAIT.LITESHADE);
            }
        }

        if (pSoldier.bLife < OKLIFE)
        {
            //            SetObjectHandleShade(pFace.uiVideoObject, FLASH_PORTRAIT.DARKSHADE);
        }

        // ATE: Don't shade for damage if blitting extern face...
        if (!fExternBlit)
        {
            if (pSoldier.fFlashPortrait == FLASH_PORTRAIT.START)
            {
                //                SetObjectHandleShade(pFace.uiVideoObject, pSoldier.bFlashPortraitFrame);
            }
        }
    }

    bool RenderAutoFaceFromSoldier(int ubSoldierID)
    {
        // Check for valid soldier
        CHECKF(ubSoldierID != NOBODY);

        return this.RenderAutoFace(MercPtrs[ubSoldierID].iFaceIndex);
    }

    public static void GetXYForIconPlacement(FACETYPE? pFace, int ubIndex, int sFaceX, int sFaceY, out int psX, out int psY)
    {
        int sX = 0, sY = 0;
        int usWidth = 0, usHeight = 0;
        ETRLEObject? pTrav = null;
        HVOBJECT hVObject = new();


        // Get height, width of icon...
        //        GetVideoObject(hVObject, guiPORTRAITICONS);
        //        pTrav = (hVObject.pETRLEObject[ubIndex]);
        //        usHeight = pTrav.usHeight;
        //        usWidth = pTrav.usWidth;

        sX = sFaceX + pFace.usFaceSize.Width - usWidth - 1;
        sY = sFaceY + pFace.usFaceSize.Height - usHeight - 1;

        psX = sX;
        psY = sY;
    }

    private static void GetXYForRightIconPlacement(FACETYPE? pFace, int ubIndex, int sFaceX, int sFaceY, out int psX, out int psY, int bNumIcons)
    {
        int sX = 0, sY = 0;
        int usWidth = 0, usHeight = 0;
        ETRLEObject? pTrav = null;
        HVOBJECT hVObject = new();


        // Get height, width of icon...
        //        GetVideoObject(hVObject, guiPORTRAITICONS);
        //        pTrav = (hVObject.pETRLEObject[ubIndex]);
        //        usHeight = pTrav.usHeight;
        //        usWidth = pTrav.usWidth;

        sX = sFaceX + (usWidth * bNumIcons) + 1;
        sY = sFaceY + pFace.usFaceSize.Height - usHeight - 1;

        psX = sX;
        psY = sY;
    }



    private static void DoRightIcon(SurfaceType uiRenderBuffer, FACETYPE? pFace, int sFaceX, int sFaceY, int bNumIcons, int sIconIndex)
    {

        // Find X, y for placement
        GetXYForRightIconPlacement(pFace, sIconIndex, sFaceX, sFaceY, out int sIconX, out int sIconY, bNumIcons);
        //        BltVideoObjectFromIndex(uiRenderBuffer, guiPORTRAITICONS, sIconIndex, sIconX, sIconY, VO_BLT.SRCTRANSPARENCY, null);
    }


    public void HandleRenderFaceAdjustments(FACETYPE? pFace, bool fDisplayBuffer, bool fUseExternBuffer, SurfaceType uiBuffer, int sFaceX, int sFaceY, int usEyesX, int usEyesY)
    {
        int sIconIndex = -1;
        bool fDoIcon = false;
        SurfaceType uiRenderBuffer;
        int sPtsAvailable = 0;
        int usMaximumPts = 0;
        string sString = string.Empty;
        int usTextWidth;
        bool fAtGunRange = false;
        bool fShowNumber = false;
        bool fShowMaximum = false;
        SOLDIERTYPE? pSoldier;
        int sFontX, sFontY;
        int sX1, sY1, sY2, sX2;
        int uiDestPitchBYTES;
        int pDestBuf;
        int usLineColor;
        int bNumRightIcons = 0;

        // If we are using an extern buffer...
        if (fUseExternBuffer)
        {
            uiRenderBuffer = uiBuffer;
        }
        else
        {
            if (fDisplayBuffer)
            {
                uiRenderBuffer = pFace.uiAutoDisplayBuffer;
            }
            else
            {
                uiRenderBuffer = pFace.uiAutoRestoreBuffer;

                if (pFace.uiAutoRestoreBuffer == FACE_NO_RESTORE_BUFFER)
                {
                    return;
                }
            }
        }

        // BLIT HATCH
        if (pFace.ubSoldierID != NOBODY)
        {
            pSoldier = MercPtrs[pFace.ubSoldierID];

            if (MercPtrs[pFace.ubSoldierID].bLife < CONSCIOUSNESS || MercPtrs[pFace.ubSoldierID].fDeadPanel)
            {
                // Blit Closed eyes here!
                //                BltVideoObjectFromIndex(uiRenderBuffer, pFace.uiVideoObject, 1, usEyesX, usEyesY, VO_BLT.SRCTRANSPARENCY, null);

                // Blit hatch!
                //                BltVideoObjectFromIndex(uiRenderBuffer, guiHATCH, 0, sFaceX, sFaceY, VO_BLT.SRCTRANSPARENCY, null);
            }

            if (MercPtrs[pFace.ubSoldierID].fMercAsleep == true)
            {
                // blit eyes closed
                //                BltVideoObjectFromIndex(uiRenderBuffer, pFace.uiVideoObject, 1, usEyesX, usEyesY, VO_BLT.SRCTRANSPARENCY, null);
            }

            if (pSoldier.uiStatusFlags.HasFlag(SOLDIER.DEAD))
            {
                // IF we are in the process of doing any deal/close animations, show face, not skill...
                if (!pSoldier.fClosePanel && !pSoldier.fDeadPanel && !pSoldier.fUIdeadMerc && !pSoldier.fUICloseMerc)
                {
                    // Put close panel there
                    //                    BltVideoObjectFromIndex(uiRenderBuffer, guiDEAD, 5, sFaceX, sFaceY, VO_BLT.SRCTRANSPARENCY, null);

                    // Blit hatch!
                    //                    BltVideoObjectFromIndex(uiRenderBuffer, guiHATCH, 0, sFaceX, sFaceY, VO_BLT.SRCTRANSPARENCY, null);
                }
            }

            // ATE: If talking in popup, don't do the other things.....
            if (pFace.fTalking && gTacticalStatus.uiFlags.HasFlag(TacticalEngineStatus.IN_ENDGAME_SEQUENCE))
            {
                return;
            }

            // ATE: Only do this, because we can be talking during an interrupt....
            if (pFace.uiFlags.HasFlag(FACE.INACTIVE_HANDLED_ELSEWHERE) && !fUseExternBuffer)
            {
                // Don't do this if we are being handled elsewhere and it's not an extern buffer...
            }
            else
            {
                HandleFaceHilights(pFace, uiRenderBuffer, sFaceX, sFaceY);

                if (pSoldier.bOppCnt > 0)
                {
                    FontSubSystem.SetFontDestBuffer(uiRenderBuffer, 0, 0, 640, 480, false);

                    sString = wprintf("%d", pSoldier.bOppCnt);

                    FontSubSystem.SetFont(FontStyle.TINYFONT1);
                    FontSubSystem.SetFontForeground(FontColor.FONT_DKRED);
                    FontSubSystem.SetFontBackground(FontColor.FONT_NEARBLACK);

                    sX1 = (int)sFaceX;
                    sY1 = (int)sFaceY;

                    sX2 = sX1 + FontSubSystem.StringPixLength(sString, FontStyle.TINYFONT1) + 1;
                    sY2 = sY1 + FontSubSystem.GetFontHeight(FontStyle.TINYFONT1) - 1;

                    mprintf((int)(sX1 + 1), (int)(sY1 - 1), sString);
                    FontSubSystem.SetFontDestBuffer(SurfaceType.FRAME_BUFFER, 0, 0, 640, 480, false);

                    // Draw box
                    //                    pDestBuf = LockVideoSurface(uiRenderBuffer, out uiDestPitchBYTES);
                    //                    SetClippingRegionAndImageWidth(uiDestPitchBYTES, 0, 0, 640, 480);
                    //
                    //                    usLineColor = Get16BPPColor(FROMRGB(105, 8, 9));
                    //                    RectangleDraw(true, sX1, sY1, sX2, sY2, usLineColor, pDestBuf);
                    //
                    //                    UnLockVideoSurface(uiRenderBuffer);

                }

                if (MercPtrs[pFace.ubSoldierID].bInSector && ((gTacticalStatus.ubCurrentTeam != OUR_TEAM) || !OK_INTERRUPT_MERC(MercPtrs[pFace.ubSoldierID])) && !gfHiddenInterrupt || (gfSMDisableForItems && !gfInItemPickupMenu && gusSMCurrentMerc == pFace.ubSoldierID
                    && gsCurInterfacePanel == InterfacePanelDefines.SM_PANEL))
                {
                    // Blit hatch!
                    //                    BltVideoObjectFromIndex(uiRenderBuffer, guiHATCH, 0, sFaceX, sFaceY, VO_BLT.SRCTRANSPARENCY, null);
                }

                if (!pFace.fDisabled && !pFace.fInvalidAnim)
                {
                    // Render text above here if that's what was asked for
                    //                    if (pFace.fDisplayTextOver != FACE_NO_TEXT_OVER)
                    //                    {
                    //                        FontSubSystem.SetFont(FontStyle.TINYFONT1);
                    //                        FontSubSystem.SetFontBackground(FontColor.FONT_MCOLOR_BLACK);
                    //                        FontSubSystem.SetFontForeground(FontColor.FONT_MCOLOR_WHITE);
                    //
                    //                        FontSubSystem.SetFontDestBuffer(uiRenderBuffer, 0, 0, 640, 480, false);
                    //
                    ////                        VarFindFontCenterCoordinates(sFaceX, sFaceY, pFace.usFaceWidth, pFace.usFaceHeight, FontStyle.TINYFONT1, &sFontX, &sFontY, pFace.zDisplayText);
                    //
                    //                        if (pFace.fDisplayTextOver == FACE_DRAW_TEXT_OVER)
                    //                        {
                    //                            gprintfinvalidate(sFontX, sFontY, pFace.zDisplayText);
                    //                            mprintf(sFontX, sFontY, pFace.zDisplayText);
                    //                        }
                    //                        else if (pFace.fDisplayTextOver == FACE_ERASE_TEXT_OVER)
                    //                        {
                    //                            gprintfRestore(sFontX, sFontY, pFace.zDisplayText);
                    //                            pFace.fDisplayTextOver = FACE_NO_TEXT_OVER;
                    //                        }
                    //
                    //                        FontSubSystem.SetFontDestBuffer(Surfaces.FRAME_BUFFER, 0, 0, 640, 480, false);
                    //
                    //                    }
                }
            }

            // Check if a robot and is not controlled....
            if (MercPtrs[pFace.ubSoldierID].uiStatusFlags.HasFlag(SOLDIER.ROBOT))
            {
                if (!SoldierControl.CanRobotBeControlled(MercPtrs[pFace.ubSoldierID]))
                {
                    // Not controlled robot
                    sIconIndex = 5;
                    fDoIcon = true;
                }
            }

            if (SoldierControl.ControllingRobot(MercPtrs[pFace.ubSoldierID]))
            {
                // controlling robot
                sIconIndex = 4;
                fDoIcon = true;
            }

            // If blind...
            if (MercPtrs[pFace.ubSoldierID].bBlindedCounter > 0)
            {
                DoRightIcon(uiRenderBuffer, pFace, sFaceX, sFaceY, bNumRightIcons, 6);
                bNumRightIcons++;
            }

            if (MercPtrs[pFace.ubSoldierID].bDrugEffect[DRUG_TYPE_ADRENALINE] > 0)
            {
                DoRightIcon(uiRenderBuffer, pFace, sFaceX, sFaceY, bNumRightIcons, 7);
                bNumRightIcons++;
            }

            if (DrugsAndAlcohol.GetDrunkLevel(MercPtrs[pFace.ubSoldierID]) != DrunkLevel.SOBER)
            {
                DoRightIcon(uiRenderBuffer, pFace, sFaceX, sFaceY, bNumRightIcons, 8);
                bNumRightIcons++;
            }


            switch (pSoldier.bAssignment)
            {
                case Assignment.DOCTOR:

                    sIconIndex = 1;
                    fDoIcon = true;
                    //                    sPtsAvailable = CalculateHealingPointsForDoctor(MercPtrs[pFace.ubSoldierID], usMaximumPts, false);
                    fShowNumber = true;
                    fShowMaximum = true;

                    // divide both amounts by 10 to make the displayed numbers a little more user-palatable (smaller)
                    sPtsAvailable = (sPtsAvailable + 5) / 10;
                    usMaximumPts = (usMaximumPts + 5) / 10;
                    break;

                case Assignment.PATIENT:

                    sIconIndex = 2;
                    fDoIcon = true;
                    // show current health / maximum health
                    sPtsAvailable = (int)MercPtrs[pFace.ubSoldierID].bLife;
                    usMaximumPts = (int)MercPtrs[pFace.ubSoldierID].bLifeMax;
                    fShowNumber = true;
                    fShowMaximum = true;
                    break;

                case Assignment.TRAIN_SELF:
                case Assignment.TRAIN_TOWN:
                case Assignment.TRAIN_TEAMMATE:
                case Assignment.TRAIN_BY_OTHER:
                    sIconIndex = 3;
                    fDoIcon = true;
                    fShowNumber = true;
                    fShowMaximum = true;
                    // there could be bonus pts for training at gun range
                    if ((MercPtrs[pFace.ubSoldierID].sSectorX == 13) && (MercPtrs[pFace.ubSoldierID].sSectorY == MAP_ROW.H) && (MercPtrs[pFace.ubSoldierID].bSectorZ == 0))
                    {
                        fAtGunRange = true;
                    }

                    switch (MercPtrs[pFace.ubSoldierID].bAssignment)
                    {
                        case Assignment.TRAIN_SELF:
                            //                            sPtsAvailable = GetSoldierTrainingPts(MercPtrs[pFace.ubSoldierID], MercPtrs[pFace.ubSoldierID].bTrainStat, fAtGunRange, usMaximumPts);
                            break;
                        case Assignment.TRAIN_BY_OTHER:
                            //                            sPtsAvailable = GetSoldierStudentPts(MercPtrs[pFace.ubSoldierID], MercPtrs[pFace.ubSoldierID].bTrainStat, fAtGunRange, usMaximumPts);
                            break;
                        case Assignment.TRAIN_TOWN:
                            //                            sPtsAvailable = GetTownTrainPtsForCharacter(MercPtrs[pFace.ubSoldierID], &usMaximumPts);
                            // divide both amounts by 10 to make the displayed numbers a little more user-palatable (smaller)
                            sPtsAvailable = (sPtsAvailable + 5) / 10;
                            usMaximumPts = (usMaximumPts + 5) / 10;
                            break;
                        case Assignment.TRAIN_TEAMMATE:
                            //                            sPtsAvailable = GetBonusTrainingPtsDueToInstructor(MercPtrs[pFace.ubSoldierID], null, MercPtrs[pFace.ubSoldierID].bTrainStat, fAtGunRange, &usMaximumPts);
                            break;
                    }
                    break;

                case Assignment.REPAIR:

                    sIconIndex = 0;
                    fDoIcon = true;
                    //                    sPtsAvailable = CalculateRepairPointsForRepairman(MercPtrs[pFace.ubSoldierID], &usMaximumPts, false);
                    fShowNumber = true;
                    fShowMaximum = true;

                    // check if we are repairing a vehicle
                    if (Menptr[pFace.ubSoldierID].bVehicleUnderRepairID != -1)
                    {
                        // reduce to a multiple of VEHICLE_REPAIR_POINTS_DIVISOR.  This way skill too low will show up as 0 repair pts.
                        //                        sPtsAvailable -= (sPtsAvailable % VEHICLE_REPAIR_POINTS_DIVISOR);
                        //                        usMaximumPts -= (usMaximumPts % VEHICLE_REPAIR_POINTS_DIVISOR);
                    }

                    break;
            }

            // Check for being serviced...
            if (MercPtrs[pFace.ubSoldierID].ubServicePartner != NOBODY)
            {
                // Doctor...
                sIconIndex = 1;
                fDoIcon = true;
            }

            if (MercPtrs[pFace.ubSoldierID].ubServiceCount != 0)
            {
                // Patient
                sIconIndex = 2;
                fDoIcon = true;
            }


            if (fDoIcon)
            {
                // Find X, y for placement
                GetXYForIconPlacement(pFace, sIconIndex, sFaceX, sFaceY, out int sIconX, out int sIconY);
                //                BltVideoObjectFromIndex(uiRenderBuffer, guiPORTRAITICONS, sIconIndex, sIconX, sIconY, VO_BLT.SRCTRANSPARENCY, null);

                // ATE: Show numbers only in mapscreen
                if (fShowNumber)
                {
                    FontSubSystem.SetFontDestBuffer(uiRenderBuffer, 0, 0, 640, 480, false);

                    if (fShowMaximum)
                    {
                        wprintf(sString, "%d/%d", sPtsAvailable, usMaximumPts);
                    }
                    else
                    {
                        wprintf(sString, "%d", sPtsAvailable);
                    }

                    usTextWidth = FontSubSystem.StringPixLength(sString, FontStyle.FONT10ARIAL);
                    usTextWidth += 1;

                    FontSubSystem.SetFont(FontStyle.FONT10ARIAL);
                    FontSubSystem.SetFontForeground(FontColor.FONT_YELLOW);
                    FontSubSystem.SetFontBackground(FontColor.FONT_BLACK);

                    mprintf(sFaceX + pFace.usFaceSize.Width - usTextWidth, (int)(sFaceY + 3), sString);
                    FontSubSystem.SetFontDestBuffer(SurfaceType.FRAME_BUFFER, 0, 0, 640, 480, false);
                }
            }
        }
        else
        {
            if (pFace.ubCharacterNum == NPCID.FATHER || pFace.ubCharacterNum == NPCID.MICKY)
            {
                if (gMercProfiles[pFace.ubCharacterNum].bNPCData >= 5)
                {
                    DoRightIcon(uiRenderBuffer, pFace, sFaceX, sFaceY, 0, 8);
                }
            }
        }
    }


    bool RenderAutoFace(int iFaceIndex)
    {
        FACETYPE? pFace;

        // Check face index
        CHECKF(iFaceIndex != -1);

        pFace = gFacesData[iFaceIndex];

        // Check for a valid slot!
        CHECKF(pFace.fAllocated != false);

        // Check for disabled guy!
        CHECKF(pFace.fDisabled != true);

        // Set shade
        if (pFace.ubSoldierID != NOBODY)
        {
            SetFaceShade(MercPtrs[pFace.ubSoldierID], pFace, false);
        }

        // Blit face to save buffer!
        if (pFace.uiAutoRestoreBuffer != FACE_NO_RESTORE_BUFFER)
        {
            if (pFace.uiAutoRestoreBuffer == SurfaceType.SAVE_BUFFER)
            {
                //                BltVideoObjectFromIndex(pFace.uiAutoRestoreBuffer, pFace.uiVideoObject, 0, pFace.usFaceX, pFace.usFaceY, VO_BLT.SRCTRANSPARENCY, null);
            }
            else
            {
                //                BltVideoObjectFromIndex(pFace.uiAutoRestoreBuffer, pFace.uiVideoObject, 0, 0, 0, VO_BLT.SRCTRANSPARENCY, null);
            }
        }

        this.HandleRenderFaceAdjustments(pFace, false, false, 0, pFace.usFaceLocation.X, pFace.usFaceLocation.Y, pFace.usEyesX, pFace.usEyesY);

        // Restore extern rect
        if (pFace.uiAutoRestoreBuffer == SurfaceType.SAVE_BUFFER)
        {
            FaceRestoreSavedBackgroundRect(iFaceIndex, (int)pFace.usFaceLocation.X, (int)pFace.usFaceLocation.Y, (int)pFace.usFaceLocation.X, (int)pFace.usFaceLocation.Y, (int)pFace.usFaceSize.Width, (int)pFace.usFaceSize.Height);
        }
        else
        {
            FaceRestoreSavedBackgroundRect(iFaceIndex, pFace.usFaceLocation.X, pFace.usFaceLocation.Y, 0, 0, pFace.usFaceSize.Width, pFace.usFaceSize.Height);
        }

        return true;
    }


    public bool ExternRenderFaceFromSoldier(SurfaceType uiBuffer, int ubSoldierID, int sX, int sY)
    {
        // Check for valid soldier
        CHECKF(ubSoldierID != NOBODY);

        return this.ExternRenderFace(uiBuffer, MercPtrs[ubSoldierID].iFaceIndex, sX, sY);
    }


    public bool ExternRenderFace(SurfaceType uiBuffer, int iFaceIndex, int sX, int sY)
    {
        FACETYPE? pFace;

        // Check face index
        CHECKF(iFaceIndex != -1);

        pFace = gFacesData[iFaceIndex];

        // Check for a valid slot!
        CHECKF(pFace.fAllocated != false);

        // Here, any face can be rendered, even if disabled

        // Set shade
        if (pFace.ubSoldierID != NOBODY)
        {
            SetFaceShade(MercPtrs[pFace.ubSoldierID], pFace, true);
        }

        // Blit face to save buffer!
        //        BltVideoObjectFromIndex(uiBuffer, pFace.uiVideoObject, 0, sX, sY, VO_BLT.SRCTRANSPARENCY, null);

        GetFaceRelativeCoordinates(pFace, out int usEyesX, out int usEyesY, out int usMouthX, out int usMouthY);

        this.HandleRenderFaceAdjustments(pFace, false, true, uiBuffer, sX, sY, (int)(sX + usEyesX), (int)(sY + usEyesY));

        // Restore extern rect
        if (uiBuffer == SurfaceType.SAVE_BUFFER)
        {
            //            RestoreExternBackgroundRect(sX, sY, pFace.usFaceWidth, pFace.usFaceWidth);
        }

        return true;
    }



    public static void NewEye(FACETYPE? pFace)
    {

        switch (pFace.sEyeFrame)
        {
            case 0: //pFace.sEyeFrame = (int)Globals.Random.Next(2);	// normal - can blink or frown
                if (pFace.ubExpression == Expression.ANGRY)
                {
                    pFace.ubEyeWait = 0;
                    pFace.sEyeFrame = 3;
                }
                else if (pFace.ubExpression == Expression.SURPRISED)
                {
                    pFace.ubEyeWait = 0;
                    pFace.sEyeFrame = 4;
                }
                else
                {
                    //if (pFace.sEyeFrame && Talk.talking && Talk.expression != DYING)
                    //    pFace.sEyeFrame = 3;
                    //else
                    pFace.sEyeFrame = 1;
                }

                break;
            case 1: // starting to blink  - has to finish unless dying
                    //if (Talk.expression == DYING)
                    //    pFace.sEyeFrame = 1;
                    //else
                pFace.sEyeFrame = 2;
                break;
            case 2: //pFace.sEyeFrame = (int)Globals.Random.Next(2);	// finishing blink - can go normal or frown
                    //if (pFace.sEyeFrame && Talk.talking)
                    //    pFace.sEyeFrame = 3;
                    //else
                    //   if (Talk.expression == ANGRY)
                    // pFace.sEyeFrame = 3;
                    //   else
                pFace.sEyeFrame = 0;
                break;

            case 3: //pFace.sEyeFrame = 4; break;	// frown

                pFace.ubEyeWait++;

                if (pFace.ubEyeWait > 6)
                {
                    pFace.sEyeFrame = 0;
                }
                break;

            case 4:

                pFace.ubEyeWait++;

                if (pFace.ubEyeWait > 6)
                {
                    pFace.sEyeFrame = 0;
                }
                break;

            case 5:
                pFace.sEyeFrame = 6;

                pFace.sEyeFrame = 0;
                break;

            case 6:
                pFace.sEyeFrame = 7;
                break;
            case 7:
                pFace.sEyeFrame = (int)Globals.Random.Next(2);    // can stop frowning or continue
                                                                  //if (pFace.sEyeFrame && Talk.expression != DYING)
                                                                  //   pFace.sEyeFrame = 8;
                                                                  //else
                                                                  //    pFace.sEyeFrame = 0;
                break;
            case 8:
                pFace.sEyeFrame = 9;
                break;
            case 9:
                pFace.sEyeFrame = 10;
                break;
            case 10:
                pFace.sEyeFrame = 11;
                break;
            case 11:
                pFace.sEyeFrame = 12;
                break;
            case 12:
                pFace.sEyeFrame = 0;
                break;
        }
    }


    void NewMouth(FACETYPE pFace)
    {
        bool OK = false;
        int sOld = pFace.sMouthFrame;

        // if (audio_gap_active == 1)
        //  {
        //   Talk.mouth = 0;
        //   return;
        //  }

        do
        {
            //Talk.mouth = random(4);

            pFace.sMouthFrame = (int)Globals.Random.Next(6);

            if (pFace.sMouthFrame > 3)
            {
                pFace.sMouthFrame = 0;
            }

            switch (sOld)
            {
                case 0:
                    if (pFace.sMouthFrame != 0)
                    {
                        OK = true;
                    }

                    break;
                case 1:
                    if (pFace.sMouthFrame != 1)
                    {
                        OK = true;
                    }

                    break;
                case 2:
                    if (pFace.sMouthFrame != 2)
                    {
                        OK = true;
                    }

                    break;
                case 3:
                    if (pFace.sMouthFrame != 3)
                    {
                        OK = true;
                    }

                    break;
            }

        } while (!OK);

    }


    void HandleAutoFaces()
    {
        int uiCount;
        FACETYPE? pFace;
        int bLife;
        bool bInSector;
        int bAPs;
        bool fRerender = false;
        bool fHandleFace;
        bool fHandleUIHatch;
        SOLDIERTYPE? pSoldier;


        for (uiCount = 0; uiCount < guiNumFaces; uiCount++)
        {
            fRerender = false;
            fHandleFace = true;
            fHandleUIHatch = false;

            // OK, NOW, check if our bLife status has changed, re-render if so!
            if (gFacesData[uiCount].fAllocated)
            {
                pFace = gFacesData[uiCount];

                // Are we a soldier?
                if (pFace.ubSoldierID != NOBODY)
                {
                    // Get Life now
                    pSoldier = MercPtrs[pFace.ubSoldierID];
                    bLife = (int)pSoldier.bLife;
                    bInSector = pSoldier.bInSector;
                    bAPs = pSoldier.bActionPoints;

                    if (pSoldier.ubID == gsSelectedGuy && gfUIHandleSelectionAboveGuy)
                    {
                        pFace.uiFlags |= FACE.SHOW_WHITE_HILIGHT;
                    }
                    else
                    {
                        pFace.uiFlags &= ~FACE.SHOW_WHITE_HILIGHT;
                    }

                    if (pSoldier.sGridNo != pSoldier.sFinalDestination && pSoldier.sGridNo != NOWHERE)
                    {
                        pFace.uiFlags |= FACE.SHOW_MOVING_HILIGHT;
                    }
                    else
                    {
                        pFace.uiFlags &= ~FACE.SHOW_MOVING_HILIGHT;
                    }

                    if (pSoldier.bStealthMode != pFace.bOldStealthMode)
                    {
                        fRerender = true;
                    }

                    // Check if we have fallen below OKLIFE...
                    if (bLife < OKLIFE && pFace.bOldSoldierLife >= OKLIFE)
                    {
                        fRerender = true;
                    }

                    if (bLife >= OKLIFE && pFace.bOldSoldierLife < OKLIFE)
                    {
                        fRerender = true;
                    }

                    // Check if we have fallen below CONSCIOUSNESS
                    if (bLife < CONSCIOUSNESS && pFace.bOldSoldierLife >= CONSCIOUSNESS)
                    {
                        fRerender = true;
                    }

                    if (bLife >= CONSCIOUSNESS && pFace.bOldSoldierLife < CONSCIOUSNESS)
                    {
                        fRerender = true;
                    }

                    if (pSoldier.bOppCnt != pFace.bOldOppCnt)
                    {
                        fRerender = true;
                    }

                    // Check if assignment is idfferent....
                    if (pSoldier.bAssignment != pFace.bOldAssignment)
                    {
                        pFace.bOldAssignment = pSoldier.bAssignment;
                        fRerender = true;
                    }

                    // Check if we have fallen below CONSCIOUSNESS
                    if (bAPs == 0 && pFace.bOldActionPoints > 0)
                    {
                        fRerender = true;
                    }

                    if (bAPs > 0 && pFace.bOldActionPoints == 0)
                    {
                        fRerender = true;
                    }

                    if (!pFace.uiFlags.HasFlag(FACE.SHOW_WHITE_HILIGHT) && pFace.fOldShowHighlight)
                    {
                        fRerender = true;
                    }

                    if (pFace.uiFlags.HasFlag(FACE.SHOW_WHITE_HILIGHT) && !pFace.fOldShowHighlight)
                    {
                        fRerender = true;
                    }

                    if (!pFace.uiFlags.HasFlag(FACE.SHOW_MOVING_HILIGHT) && pFace.fOldShowMoveHilight)
                    {
                        fRerender = true;
                    }

                    if (pFace.uiFlags.HasFlag(FACE.SHOW_MOVING_HILIGHT) && !pFace.fOldShowMoveHilight)
                    {
                        fRerender = true;
                    }

                    if (pFace.ubOldServiceCount != pSoldier.ubServiceCount)
                    {
                        fRerender = true;
                        pFace.ubOldServiceCount = pSoldier.ubServiceCount;
                    }

                    if (pFace.fOldCompatibleItems != pFace.fCompatibleItems || gfInItemPickupMenu || gpItemPointer != null)
                    {
                        fRerender = true;
                        pFace.fOldCompatibleItems = pFace.fCompatibleItems;
                    }


                    if (pFace.ubOldServicePartner != pSoldier.ubServicePartner)
                    {
                        fRerender = true;
                        pFace.ubOldServicePartner = pSoldier.ubServicePartner;
                    }

                    pFace.fOldHandleUIHatch = fHandleUIHatch;
                    pFace.bOldSoldierLife = bLife;
                    pFace.bOldActionPoints = bAPs;
                    pFace.bOldStealthMode = pSoldier.bStealthMode;
                    pFace.bOldOppCnt = pSoldier.bOppCnt;

                    if (pFace.uiFlags.HasFlag(FACE.SHOW_WHITE_HILIGHT))
                    {
                        pFace.fOldShowHighlight = true;
                    }
                    else
                    {
                        pFace.fOldShowHighlight = false;
                    }

                    if (pFace.uiFlags.HasFlag(FACE.SHOW_MOVING_HILIGHT))
                    {
                        pFace.fOldShowMoveHilight = true;
                    }
                    else
                    {
                        pFace.fOldShowMoveHilight = false;
                    }


                    if (pSoldier.fGettingHit > 0 && pSoldier.fFlashPortrait == FLASH_PORTRAIT.STOP)
                    {
                        pSoldier.fFlashPortrait = FLASH_PORTRAIT.START;
                        pSoldier.bFlashPortraitFrame = FLASH_PORTRAIT.STARTSHADE;
                        RESETTIMECOUNTER(ref pSoldier.PortraitFlashCounter, (uint)FLASH_PORTRAIT.DELAY);
                        fRerender = true;
                    }
                    if (pSoldier.fFlashPortrait == FLASH_PORTRAIT.START)
                    {
                        // Loop through flash values
                        if (TIMECOUNTERDONE(pSoldier.PortraitFlashCounter, (uint)FLASH_PORTRAIT.DELAY))
                        {
                            RESETTIMECOUNTER(ref pSoldier.PortraitFlashCounter, (uint)FLASH_PORTRAIT.DELAY);
                            pSoldier.bFlashPortraitFrame++;

                            if (pSoldier.bFlashPortraitFrame > FLASH_PORTRAIT.ENDSHADE)
                            {
                                pSoldier.bFlashPortraitFrame = FLASH_PORTRAIT.ENDSHADE;

                                if (pSoldier.fGettingHit > 0)
                                {
                                    pSoldier.fFlashPortrait = FLASH_PORTRAIT.WAITING;
                                }
                                else
                                {
                                    // Render face again!
                                    pSoldier.fFlashPortrait = FLASH_PORTRAIT.STOP;
                                }

                                fRerender = true;
                            }
                        }
                    }
                    // CHECK IF WE WERE WAITING FOR GETTING HIT TO FINISH!
                    if (pSoldier.fGettingHit == 0 && pSoldier.fFlashPortrait == FLASH_PORTRAIT.WAITING)
                    {
                        pSoldier.fFlashPortrait = FLASH_PORTRAIT.STOP;
                        fRerender = true;
                    }

                    if (pSoldier.fFlashPortrait == FLASH_PORTRAIT.START)
                    {
                        fRerender = true;
                    }

                    if (pFace.uiFlags.HasFlag(FACE.REDRAW_WHOLE_FACE_NEXT_FRAME))
                    {
                        pFace.uiFlags &= ~FACE.REDRAW_WHOLE_FACE_NEXT_FRAME;

                        fRerender = true;
                    }

                    if (fInterfacePanelDirty == DIRTYLEVEL2 && guiCurrentScreen == ScreenName.GAME_SCREEN)
                    {
                        fRerender = true;
                    }

                    if (fRerender)
                    {
                        this.RenderAutoFace(uiCount);
                    }

                    if (bLife < CONSCIOUSNESS)
                    {
                        fHandleFace = false;
                    }
                }

                if (fHandleFace)
                {
                    this.BlinkAutoFace(uiCount);
                }

                this.MouthAutoFace(uiCount);

            }

        }

    }

    private static void HandleTalkingAutoFaces()
    {
        int uiCount;
        FACETYPE? pFace;

        for (uiCount = 0; uiCount < guiNumFaces; uiCount++)
        {
            // OK, NOW, check if our bLife status has changed, re-render if so!
            if (gFacesData[uiCount].fAllocated)
            {
                pFace = gFacesData[uiCount];

                HandleTalkingAutoFace(uiCount);

            }
        }
    }

    private static bool FaceRestoreSavedBackgroundRect(int iFaceIndex, int sDestLeft, int sDestTop, int sSrcLeft, int sSrcTop, int sWidth, int sHeight)
    {
        FACETYPE? pFace;
        int uiDestPitchBYTES, uiSrcPitchBYTES;
        Image<Rgba32> pDestBuf, pSrcBuf;

        // Check face index
        CHECKF(iFaceIndex != -1);

        pFace = gFacesData[iFaceIndex];

        // DOn't continue if we do not want the resotre to happen ( ei blitting entrie thing every frame...
        if (pFace.uiAutoRestoreBuffer == FACE_NO_RESTORE_BUFFER)
        {
            return false;
        }

//        pDestBuf = video.LockVideoSurface(pFace.uiAutoDisplayBuffer, out uiDestPitchBYTES);
//        pSrcBuf = video.LockVideoSurface(pFace.uiAutoRestoreBuffer, out uiSrcPitchBYTES);

//        video.Blt16BPPTo16BPP(pDestBuf, uiDestPitchBYTES,
//                    pSrcBuf, uiSrcPitchBYTES,
//                    sDestLeft, sDestTop,
//                    sSrcLeft, sSrcTop,
//                    sWidth, sHeight);
//
//        video.UnLockVideoSurface(pFace.uiAutoDisplayBuffer);
//        video.UnLockVideoSurface(pFace.uiAutoRestoreBuffer);

        // Add rect to frame buffer queue
        if (pFace.uiAutoDisplayBuffer == SurfaceType.FRAME_BUFFER)
        {
            video.InvalidateRegionEx(sDestLeft - 2, sDestTop - 2, sDestLeft + sWidth + 3, sDestTop + sHeight + 2, 0);
        }

        return true;
    }


    bool SetFaceTalking(int iFaceIndex, string zSoundFile, string zTextString, int usRate, int ubVolume, int ubLoops, int uiPan)
    {
        FACETYPE? pFace;

        pFace = gFacesData[iFaceIndex];

        // Set face to talking
        pFace.fTalking = true;
        pFace.fAnimatingTalking = true;
        pFace.fFinishTalking = false;

        if (!Meanwhile.AreInMeanwhile())
        {
            //            TurnOnSectorLocator(pFace.ubCharacterNum);
        }

        // Play sample
        if (GameSettings.fOptions[TOPTION.SPEECH])
        {
            // pFace.uiSoundID = PlayJA2GapSample(zSoundFile, RATE_11025, HIGHVOLUME, 1, MIDDLEPAN, &(pFace.GapList));
        }
        else
        {
            pFace.uiSoundID = SOUND_ERROR;
        }

        if (pFace.uiSoundID != SOUND_ERROR)
        {
            pFace.fValidSpeech = true;

            pFace.uiTalkingFromVeryBeginningTimer = GetJA2Clock();
        }
        else
        {
            pFace.fValidSpeech = false;

            // Set delay based on sound...
            pFace.uiTalkingTimer = pFace.uiTalkingFromVeryBeginningTimer = GetJA2Clock();

            //            pFace.uiTalkingDuration = FindDelayForString(zTextString);
        }

        return true;
    }


    bool ExternSetFaceTalking(int iFaceIndex, int uiSoundID)
    {
        FACETYPE? pFace;

        pFace = gFacesData[iFaceIndex];

        // Set face to talki	ng
        pFace.fTalking = true;
        pFace.fAnimatingTalking = true;
        pFace.fFinishTalking = false;
        pFace.fValidSpeech = true;

        pFace.uiSoundID = uiSoundID;

        return true;
    }

    public static void InternalShutupaYoFace(int iFaceIndex, bool fForce)
    {
        FACETYPE? pFace;

        // Check face index
        //        CHECKV(iFaceIndex != -1);

        pFace = gFacesData[iFaceIndex];

        if (pFace.fTalking)
        {
            // OK, only do this if we have been talking for a min. amount fo time...
            if ((GetJA2Clock() - pFace.uiTalkingFromVeryBeginningTimer) < 500 && !fForce)
            {
                return;
            }

            if (pFace.uiSoundID != SOUND_ERROR)
            {
                //                SoundStop(pFace.uiSoundID);
            }

            // Remove gap info
            //            AudioGapListDone((pFace.GapList));

            // Shutup mouth!
            pFace.sMouthFrame = 0;

            // ATE: Only change if active!
            if (!pFace.fDisabled)
            {
                if (pFace.uiAutoRestoreBuffer == SurfaceType.SAVE_BUFFER)
                {
                    FaceRestoreSavedBackgroundRect(iFaceIndex, pFace.usMouthX, pFace.usMouthY, pFace.usMouthX, pFace.usMouthY, pFace.usMouthWidth, pFace.usMouthHeight);
                }
                else
                {
                    FaceRestoreSavedBackgroundRect(iFaceIndex, pFace.usMouthX, pFace.usMouthY, pFace.usMouthOffsetX, pFace.usMouthOffsetY, pFace.usMouthWidth, pFace.usMouthHeight);
                }
            }
            // OK, smart guy, make sure this guy has finished talking,
            // before attempting to end dialogue UI.
            pFace.fTalking = false;

            // Call dialogue handler function
            //            HandleDialogueEnd(pFace);

            pFace.fTalking = false;
            pFace.fAnimatingTalking = false;

            gfUIWaitingForUserSpeechAdvance = false;

        }

    }

    public static void ShutupaYoFace(int iFaceIndex)
    {
        InternalShutupaYoFace(iFaceIndex, true);
    }

    public static void SetupFinalTalkingDelay(FACETYPE? pFace)
    {
        pFace.fFinishTalking = true;

        pFace.fAnimatingTalking = false;

        pFace.uiTalkingTimer = GetJA2Clock();

        if (GameSettings.fOptions[TOPTION.SUBTITLES])
        {
            //pFace.uiTalkingDuration = FINAL_TALKING_DURATION;
            pFace.uiTalkingDuration = 300;
        }
        else
        {
            pFace.uiTalkingDuration = 300;
        }

        pFace.sMouthFrame = 0;

        // Close mouth!
        if (!pFace.fDisabled)
        {
            if (pFace.uiAutoRestoreBuffer == SurfaceType.SAVE_BUFFER)
            {
                FaceRestoreSavedBackgroundRect(pFace.iID, pFace.usMouthX, pFace.usMouthY, pFace.usMouthX, pFace.usMouthY, pFace.usMouthWidth, pFace.usMouthHeight);
            }
            else
            {
                FaceRestoreSavedBackgroundRect(pFace.iID, pFace.usMouthX, pFace.usMouthY, pFace.usMouthOffsetX, pFace.usMouthOffsetY, pFace.usMouthWidth, pFace.usMouthHeight);
            }
        }

        // Setup flag to wait for advance ( because we have no text! )
        if (GameSettings.fOptions[TOPTION.KEY_ADVANCE_SPEECH] && pFace.uiFlags.HasFlag(FACE.POTENTIAL_KEYWAIT))
        {

            // Check if we have had valid speech!
            if (!pFace.fValidSpeech || GameSettings.fOptions[TOPTION.SUBTITLES])
            {
                // Set false!
                pFace.fFinishTalking = false;
                // Set waiting for advance to true!
                gfUIWaitingForUserSpeechAdvance = true;
            }
        }

        // Set final delay!
        pFace.fValidSpeech = false;
    }
}

public record RPC_SMALL_FACE_VALUES(
    int bEyesX,
    int bEyesY,
    int bMouthX,
    int bMouthY);


// FLASH PORTRAIT CODES
public enum FLASH_PORTRAIT
{
    STOP = 0,
    START = 1,
    WAITING = 2,
    DELAY = 150,

    // FLASH PORTRAIT PALETTE IDS
    NOSHADE = 0,
    STARTSHADE = 1,
    ENDSHADE = 2,
    DARKSHADE = 3,
    GRAYSHADE = 4,
    LITESHADE = 5,
}
