using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.Screens;
using SharpAlliance.Core.SubSystems;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class Faces
{
    int GetFreeFace()
    {
        int uiCount;

        for (uiCount = 0; uiCount < guiNumFaces; uiCount++)
        {
            if ((gFacesData[uiCount].fAllocated == false))
            {
                return ((int)uiCount);
            }
        }

        if (guiNumFaces < NUM_FACE_SLOTS)
        {
            return ((int)guiNumFaces++);
        }

        return (-1);
    }

    private static void RecountFaces()
    {
        int uiCount;

        for (uiCount = guiNumFaces - 1; (uiCount >= 0); uiCount--)
        {
            if ((gFacesData[uiCount].fAllocated))
            {
                guiNumFaces = (int)(uiCount + 1);
                break;
            }
        }
    }


    int InitSoldierFace(SOLDIERTYPE? pSoldier)
    {
        int iFaceIndex;

        // Check if we have a face init already
        iFaceIndex = pSoldier.iFaceIndex;

        if (iFaceIndex != -1)
        {
            return (iFaceIndex);
        }

        return (InitFace(pSoldier.ubProfile, pSoldier.ubID, 0));
    }


    int InitFace(NPCID usMercProfileID, int ubSoldierID, FACE uiInitFlags)
    {
        int uiBlinkFrequency;
        int uiExpressionFrequency;

        if (usMercProfileID == NO_PROFILE)
        {
            return (-1);
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

        return (InternalInitFace(usMercProfileID, ubSoldierID, uiInitFlags, gMercProfiles[usMercProfileID].ubFaceIndex, uiBlinkFrequency, uiExpressionFrequency));

    }


    int InternalInitFace(NPCID usMercProfileID, int ubSoldierID, FACE uiInitFlags, int iFaceFileID, int uiBlinkFrequency, int uiExpressionFrequency)
    {
        FACETYPE? pFace;
        // VOBJECT_DESC VObjectDesc;
        int uiVideoObject;
        int iFaceIndex;
        ETRLEObject ETRLEObject;
        HVOBJECT hVObject;
        int uiCount;
        SGPPaletteEntry[] Pal = new SGPPaletteEntry[256];

        if ((iFaceIndex = GetFreeFace()) == (-1))
        {
            return (-1);
        }

        // Load face file
        // VObjectDesc.fCreateFlags = VOBJECT_CREATE_FROMFILE;

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
                sprintf(VObjectDesc.ImageFile, "FACES\\b%02d.sti", iFaceFileID);
            }
            else
            {
                sprintf(VObjectDesc.ImageFile, "FACES\\b%03d.sti", iFaceFileID);
            }

            // ATE: Check for profile - if elliot , use special face :)
            if (usMercProfileID == NPCID.ELLIOT)
            {
                if (gMercProfiles[NPCID.ELLIOT].bNPCData > 3 && gMercProfiles[NPCID.ELLIOT].bNPCData < 7)
                {
                    sprintf(VObjectDesc.ImageFile, "FACES\\b%02da.sti", iFaceFileID);
                }
                else if (gMercProfiles[NPCID.ELLIOT].bNPCData > 6 && gMercProfiles[NPCID.ELLIOT].bNPCData < 10)
                {
                    sprintf(VObjectDesc.ImageFile, "FACES\\b%02db.sti", iFaceFileID);
                }
                else if (gMercProfiles[NPCID.ELLIOT].bNPCData > 9 && gMercProfiles[NPCID.ELLIOT].bNPCData < 13)
                {
                    sprintf(VObjectDesc.ImageFile, "FACES\\b%02dc.sti", iFaceFileID);
                }
                else if (gMercProfiles[NPCID.ELLIOT].bNPCData > 12 && gMercProfiles[NPCID.ELLIOT].bNPCData < 16)
                {
                    sprintf(VObjectDesc.ImageFile, "FACES\\b%02dd.sti", iFaceFileID);
                }
                else if (gMercProfiles[NPCID.ELLIOT].bNPCData == 17)
                {
                    sprintf(VObjectDesc.ImageFile, "FACES\\b%02de.sti", iFaceFileID);
                }
            }
        }
        else
        {

            if (iFaceFileID < 100)
            {
                // The filename is the profile ID!
                sprintf(VObjectDesc.ImageFile, "FACES\\%02d.sti", iFaceFileID);
            }
            else
            {
                sprintf(VObjectDesc.ImageFile, "FACES\\%03d.sti", iFaceFileID);
            }
        }

        // Load
        if (AddVideoObject(&VObjectDesc, uiVideoObject) == false)
        {
            // If we are a big face, use placeholder...
            if (uiInitFlags.HasFlag(FACE.BIGFACE))
            {
                sprintf(VObjectDesc.ImageFile, "FACES\\placeholder.sti");

                if (AddVideoObject(&VObjectDesc, uiVideoObject) == false)
                {
                    return (-1);
                }
            }
            else
            {
                return (-1);
            }
        }

        // memset(gFacesData[iFaceIndex], 0, sizeof(FACETYPE));

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
        if (GetVideoObject(hVObject, uiVideoObject))
        {
            // Build a grayscale palette! ( for testing different looks )
            for (uiCount = 0; uiCount < 256; uiCount++)
            {
                Pal[uiCount].peRed = 255;
                Pal[uiCount].peGreen = 255;
                Pal[uiCount].peBlue = 255;
            }

            hVObject.pShades[(ushort)FLASH_PORTRAIT.NOSHADE] = Create16BPPPaletteShaded(hVObject.pPaletteEntry, 255, 255, 255, false);
            hVObject.pShades[(ushort)FLASH_PORTRAIT.STARTSHADE] = Create16BPPPaletteShaded(Pal, 255, 255, 255, false);
            hVObject.pShades[(ushort)FLASH_PORTRAIT.ENDSHADE] = Create16BPPPaletteShaded(hVObject.pPaletteEntry, 250, 25, 25, true);
            hVObject.pShades[(ushort)FLASH_PORTRAIT.DARKSHADE] = Create16BPPPaletteShaded(hVObject.pPaletteEntry, 100, 100, 100, true);
            hVObject.pShades[(ushort)FLASH_PORTRAIT.LITESHADE] = Create16BPPPaletteShaded(hVObject.pPaletteEntry, 100, 100, 100, false);

            for (uiCount = 0; uiCount < 256; uiCount++)
            {
                Pal[uiCount].peRed = (uiCount % 128) + 128;
                Pal[uiCount].peGreen = (uiCount % 128) + 128;
                Pal[uiCount].peBlue = (uiCount % 128) + 128;
            }
            hVObject.pShades[(ushort)FLASH_PORTRAIT.GRAYSHADE] = Create16BPPPaletteShaded(Pal, 255, 255, 255, false);

        }


        // Get FACE height, width
        if (GetVideoObjectETRLEPropertiesFromIndex(uiVideoObject, ETRLEObject, 0) == false)
        {
            return (-1);
        }
        pFace.usFaceWidth = ETRLEObject.usWidth;
        pFace.usFaceHeight = ETRLEObject.usHeight;


        // OK, check # of items
        if (hVObject.usNumberOfObjects == 8)
        {
            pFace.fInvalidAnim = false;

            // Get EYE height, width
            if (GetVideoObjectETRLEPropertiesFromIndex(uiVideoObject, ETRLEObject, 1) == false)
            {
                return (-1);
            }
            pFace.usEyesWidth = ETRLEObject.usWidth;
            pFace.usEyesHeight = ETRLEObject.usHeight;


            // Get Mouth height, width
            if (GetVideoObjectETRLEPropertiesFromIndex(uiVideoObject, ETRLEObject, 5) == false)
            {
                return (-1);
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

        return (iFaceIndex);

    }


    public static void DeleteSoldierFace(SOLDIERTYPE? pSoldier)
    {
        DeleteFace(pSoldier.iFaceIndex);

        pSoldier.iFaceIndex = -1;
    }

    public static void DeleteFace(int iFaceIndex)
    {
        FACETYPE? pFace;

        // Check face index
        CHECKV(iFaceIndex != -1);

        pFace = gFacesData[iFaceIndex];

        // Check for a valid slot!
        CHECKV(pFace.fAllocated != false);

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

            HandleDialogueEnd(pFace);
        }

        // Delete vo	
        DeleteVideoObjectFromIndex(pFace.uiVideoObject);

        // Set uncallocated
        pFace.fAllocated = false;

        RecountFaces();

    }

    void SetAutoFaceActiveFromSoldier(int uiDisplayBuffer, int uiRestoreBuffer, int ubSoldierID, int usFaceX, int usFaceY)
    {
        if (ubSoldierID == NOBODY)
        {
            return;
        }

        SetAutoFaceActive(uiDisplayBuffer, uiRestoreBuffer, MercPtrs[ubSoldierID].iFaceIndex, usFaceX, usFaceY);

    }

    void GetFaceRelativeCoordinates(FACETYPE? pFace, out int pusEyesX, out int pusEyesY, out int pusMouthX, out int pusMouthY)
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
        if (!(pFace.uiFlags.HasFlag(FACE.BIGFACE)) || ((pFace.uiFlags.HasFlag(FACE.FORCE_SMALL))))
        {
            // Are we a recruited merc? .. or small?
            if ((gMercProfiles[usMercProfileID].ubMiscFlags & (PROFILE_MISC_FLAG_RECRUITED | PROFILE_MISC_FLAG_EPCACTIVE)) || (pFace.uiFlags & FACE_FORCE_SMALL))
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

        (pusEyesX) = usEyesX;
        (pusEyesY) = usEyesY;
        (pusMouthX) = usMouthX;
        (pusMouthY) = usMouthY;
    }


    void SetAutoFaceActive(int uiDisplayBuffer, int uiRestoreBuffer, int iFaceIndex, int usFaceX, int usFaceY)
    {
        int usEyesX;
        int usEyesY;
        int usMouthX;
        int usMouthY;
        FACETYPE? pFace;

        // Check face index
        CHECKV(iFaceIndex != -1);

        pFace = gFacesData[iFaceIndex];

        GetFaceRelativeCoordinates(pFace, out usEyesX, out usEyesY, out usMouthX, out usMouthY);

        InternalSetAutoFaceActive(uiDisplayBuffer, uiRestoreBuffer, iFaceIndex, usFaceX, usFaceY, usEyesX, usEyesY, usMouthX, usMouthY);

    }


    void InternalSetAutoFaceActive(int uiDisplayBuffer, int uiRestoreBuffer, int iFaceIndex, int usFaceX, int usFaceY, int usEyesX, int usEyesY, int usMouthX, int usMouthY)
    {
        NPCID usMercProfileID;
        FACETYPE? pFace;
        VSURFACE_DESC vs_desc;
        int usWidth;
        int usHeight;
        int ubBitDepth;

        // Check face index
        CHECKV(iFaceIndex != -1);

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
            GetCurrentVideoSettings(usWidth, usHeight, ubBitDepth);
            // OK, ignore screen widths, height, only use BPP 
            vs_desc.fCreateFlags = VSURFACE_CREATE_DEFAULT | VSURFACE_SYSTEM_MEM_USAGE;
            vs_desc.usWidth = pFace.usFaceWidth;
            vs_desc.usHeight = pFace.usFaceHeight;
            vs_desc.ubBitDepth = ubBitDepth;

            pFace.fAutoRestoreBuffer = true;

            CHECKV(AddVideoSurface(vs_desc, (pFace.uiAutoRestoreBuffer)));
        }
        else
        {
            pFace.fAutoRestoreBuffer = false;
            pFace.uiAutoRestoreBuffer = uiRestoreBuffer;
        }

        if (uiDisplayBuffer == FACE_AUTO_DISPLAY_BUFFER)
        {
            // BUILD A BUFFER
            GetCurrentVideoSettings(usWidth, usHeight, ubBitDepth);
            // OK, ignore screen widths, height, only use BPP 
            vs_desc.fCreateFlags = VSURFACE_CREATE_DEFAULT | VSURFACE_SYSTEM_MEM_USAGE;
            vs_desc.usWidth = pFace.usFaceWidth;
            vs_desc.usHeight = pFace.usFaceHeight;
            vs_desc.ubBitDepth = ubBitDepth;

            pFace.fAutoDisplayBuffer = true;

            CHECKV(AddVideoSurface(vs_desc, (pFace.uiAutoDisplayBuffer)));
        }
        else
        {
            pFace.fAutoDisplayBuffer = false;
            pFace.uiAutoDisplayBuffer = uiDisplayBuffer;
        }


        usMercProfileID = pFace.ubCharacterNum;

        pFace.usFaceX = usFaceX;
        pFace.usFaceY = usFaceY;
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
            pFace.bOldSoldierLife = MercPtrs[pFace.ubSoldierID].bLife;
        }
    }


    public static void SetAutoFaceInActiveFromSoldier(int ubSoldierID)
    {
        // Check for valid soldier
        CHECKV(ubSoldierID != NOBODY);

        SetAutoFaceInActive(MercPtrs[ubSoldierID].iFaceIndex);
    }


    public static void SetAutoFaceInActive(int iFaceIndex)
    {
        FACETYPE? pFace;
        SOLDIERTYPE? pSoldier;

        // Check face index
        CHECKV(iFaceIndex != -1);

        pFace = gFacesData[iFaceIndex];

        // Check for a valid slot!
        CHECKV(pFace.fAllocated != false);


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
                if (pSoldier.bAssignment == iCurrentTacticalSquad && guiCurrentScreen == ScreenName.GAME_SCREEN)
                {
                    // Make the interfac panel dirty..
                    // This will dirty the panel next frame...
                    gfRerenderInterfaceFromHelpText = true;
                }

            }

        }

        if (pFace.fAutoRestoreBuffer)
        {
            DeleteVideoSurfaceFromIndex(pFace.uiAutoRestoreBuffer);
        }

        if (pFace.fAutoDisplayBuffer)
        {
            DeleteVideoSurfaceFromIndex(pFace.uiAutoDisplayBuffer);
        }

        if (pFace.iVideoOverlay != -1)
        {
            RemoveVideoOverlay(pFace.iVideoOverlay);
            pFace.iVideoOverlay = -1;
        }

        // Turn off some flags
        pFace.uiFlags &= (~FACE.INACTIVE_HANDLED_ELSEWHERE);

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



    public static void BlinkAutoFace(int iFaceIndex)
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
                         (MercPtrs[pFace.ubSoldierID].bAssignment == Assignments.ASSIGNMENT_POW))
                {
                    return;
                }
            }

            if (pFace.ubExpression == NO_EXPRESSION)
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
                        BltVideoObjectFromIndex(pFace.uiAutoDisplayBuffer, pFace.uiVideoObject, (int)(sFrame), pFace.usEyesX, pFace.usEyesY, VO_BLT_SRCTRANSPARENCY, null);

                        if (pFace.uiAutoDisplayBuffer == FRAME_BUFFER)
                        {
                            InvalidateRegion(pFace.usEyesX, pFace.usEyesY, pFace.usEyesX + pFace.usEyesWidth, pFace.usEyesY + pFace.usEyesHeight);
                        }
                    }
                    else
                    {
                        //RenderFace( uiDestBuffer , uiCount );
                        pFace.ubExpression = NO_EXPRESSION;
                        // Update rects just for eyes

                        if (pFace.uiAutoRestoreBuffer == guiSAVEBUFFER)
                        {
                            FaceRestoreSavedBackgroundRect(iFaceIndex, pFace.usEyesX, pFace.usEyesY, pFace.usEyesX, pFace.usEyesY, pFace.usEyesWidth, pFace.usEyesHeight);
                        }
                        else
                        {
                            FaceRestoreSavedBackgroundRect(iFaceIndex, pFace.usEyesX, pFace.usEyesY, pFace.usEyesOffsetX, pFace.usEyesOffsetY, pFace.usEyesWidth, pFace.usEyesHeight);
                        }

                    }

                    HandleRenderFaceAdjustments(pFace, true, false, 0, pFace.usFaceX, pFace.usFaceY, pFace.usEyesX, pFace.usEyesY);

                }
            }

        }

    }


    void HandleFaceHilights(FACETYPE? pFace, int uiBuffer, int sFaceX, int sFaceY)
    {
        int uiDestPitchBYTES;
        int pDestBuf;
        int usLineColor;
        int iFaceIndex;

        iFaceIndex = pFace.iID;

        if (!gFacesData[iFaceIndex].fDisabled)
        {
            if (pFace.uiAutoDisplayBuffer == FRAME_BUFFER && guiCurrentScreen == GAME_SCREEN)
            {
                // If we are highlighted, do this now!
                if ((pFace.uiFlags & FACE_SHOW_WHITE_HILIGHT))
                {
                    // Lock buffer
                    pDestBuf = LockVideoSurface(uiBuffer, uiDestPitchBYTES);
                    SetClippingRegionAndImageWidth(uiDestPitchBYTES, sFaceX - 2, sFaceY - 1, sFaceX + pFace.usFaceWidth + 4, sFaceY + pFace.usFaceHeight + 4);

                    usLineColor = Get16BPPColor(FROMRGB(255, 255, 255));
                    RectangleDraw(true, (sFaceX - 2), (sFaceY - 1), sFaceX + pFace.usFaceWidth + 1, sFaceY + pFace.usFaceHeight, usLineColor, pDestBuf);

                    SetClippingRegionAndImageWidth(uiDestPitchBYTES, 0, 0, 640, 480);

                    UnLockVideoSurface(uiBuffer);
                }
                else if ((pFace.uiFlags & FACE_SHOW_MOVING_HILIGHT))
                {
                    if (pFace.ubSoldierID != NOBODY)
                    {
                        if (MercPtrs[pFace.ubSoldierID].bLife >= OKLIFE)
                        {
                            // Lock buffer
                            pDestBuf = LockVideoSurface(uiBuffer, uiDestPitchBYTES);
                            SetClippingRegionAndImageWidth(uiDestPitchBYTES, sFaceX - 2, sFaceY - 1, sFaceX + pFace.usFaceWidth + 4, sFaceY + pFace.usFaceHeight + 4);

                            if (MercPtrs[pFace.ubSoldierID].bStealthMode)
                            {
                                usLineColor = Get16BPPColor(FROMRGB(158, 158, 12));
                            }
                            else
                            {
                                usLineColor = Get16BPPColor(FROMRGB(8, 12, 118));
                            }
                            RectangleDraw(true, (sFaceX - 2), (sFaceY - 1), sFaceX + pFace.usFaceWidth + 1, sFaceY + pFace.usFaceHeight, usLineColor, pDestBuf);

                            SetClippingRegionAndImageWidth(uiDestPitchBYTES, 0, 0, 640, 480);

                            UnLockVideoSurface(uiBuffer);
                        }
                    }
                }
                else
                {
                    // ATE: Zero out any highlight boxzes....
                    // Lock buffer
                    pDestBuf = LockVideoSurface(pFace.uiAutoDisplayBuffer, uiDestPitchBYTES);
                    SetClippingRegionAndImageWidth(uiDestPitchBYTES, pFace.usFaceX - 2, pFace.usFaceY - 1, pFace.usFaceX + pFace.usFaceWidth + 4, pFace.usFaceY + pFace.usFaceHeight + 4);

                    usLineColor = Get16BPPColor(FROMRGB(0, 0, 0));
                    RectangleDraw(true, (pFace.usFaceX - 2), (pFace.usFaceY - 1), pFace.usFaceX + pFace.usFaceWidth + 1, pFace.usFaceY + pFace.usFaceHeight, usLineColor, pDestBuf);

                    SetClippingRegionAndImageWidth(uiDestPitchBYTES, 0, 0, 640, 480);

                    UnLockVideoSurface(pFace.uiAutoDisplayBuffer);
                }
            }
        }


        if ((pFace.fCompatibleItems && !gFacesData[iFaceIndex].fDisabled))
        {
            // Lock buffer
            pDestBuf = LockVideoSurface(uiBuffer, uiDestPitchBYTES);
            SetClippingRegionAndImageWidth(uiDestPitchBYTES, sFaceX - 2, sFaceY - 1, sFaceX + pFace.usFaceWidth + 4, sFaceY + pFace.usFaceHeight + 4);

            usLineColor = Get16BPPColor(FROMRGB(255, 0, 0));
            RectangleDraw(true, (sFaceX - 2), (sFaceY - 1), sFaceX + pFace.usFaceWidth + 1, sFaceY + pFace.usFaceHeight, usLineColor, pDestBuf);

            SetClippingRegionAndImageWidth(uiDestPitchBYTES, 0, 0, 640, 480);

            UnLockVideoSurface(uiBuffer);
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
            if (pFace.uiFlags & FACE_DESTROY_OVERLAY)
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
                        PollAudioGap(pFace.uiSoundID, &(pFace.GapList));

                        // Check if we have an audio gap
                        if (pFace.GapList.audio_gap_active)
                        {
                            pFace.sMouthFrame = 0;

                            if (pFace.uiAutoRestoreBuffer == guiSAVEBUFFER)
                            {
                                FaceRestoreSavedBackgroundRect(iFaceIndex, pFace.usMouthX, pFace.usMouthY, pFace.usMouthX, pFace.usMouthY, pFace.usMouthWidth, pFace.usMouthHeight);
                            }
                            else
                            {
                                FaceRestoreSavedBackgroundRect(iFaceIndex, pFace.usMouthX, pFace.usMouthY, pFace.usMouthOffsetX, pFace.usMouthOffsetY, pFace.usMouthWidth, pFace.usMouthHeight);
                            }

                        }
                        else
                        {
                            // Get Delay time
                            if ((GetJA2Clock() - pFace.uiMouthlast) > pFace.uiMouthDelay)
                            {
                                pFace.uiMouthlast = GetJA2Clock();

                                // Adjust
                                NewMouth(pFace);

                                sFrame = pFace.sMouthFrame;

                                if (sFrame > 0)
                                {
                                    // Blit Accordingly!
                                    BltVideoObjectFromIndex(pFace.uiAutoDisplayBuffer, pFace.uiVideoObject, (int)(sFrame + 4), pFace.usMouthX, pFace.usMouthY, VO_BLT_SRCTRANSPARENCY, null);

                                    // Update rects
                                    if (pFace.uiAutoDisplayBuffer == FRAME_BUFFER)
                                    {
                                        InvalidateRegion(pFace.usMouthX, pFace.usMouthY, pFace.usMouthX + pFace.usMouthWidth, pFace.usMouthY + pFace.usMouthHeight);
                                    }
                                }
                                else
                                {
                                    //RenderFace( uiDestBuffer , uiCount );
                                    //pFace.fTaking = false;
                                    // Update rects just for Mouth
                                    if (pFace.uiAutoRestoreBuffer == guiSAVEBUFFER)
                                    {
                                        FaceRestoreSavedBackgroundRect(iFaceIndex, pFace.usMouthX, pFace.usMouthY, pFace.usMouthX, pFace.usMouthY, pFace.usMouthWidth, pFace.usMouthHeight);
                                    }
                                    else
                                    {
                                        FaceRestoreSavedBackgroundRect(iFaceIndex, pFace.usMouthX, pFace.usMouthY, pFace.usMouthOffsetX, pFace.usMouthOffsetY, pFace.usMouthWidth, pFace.usMouthHeight);
                                    }

                                }

                                HandleRenderFaceAdjustments(pFace, true, false, 0, pFace.usFaceX, pFace.usFaceY, pFace.usEyesX, pFace.usEyesY);

                            }
                        }
                    }
                }
            }

            if (!(pFace.uiFlags & FACE_INACTIVE_HANDLED_ELSEWHERE))
            {
                HandleFaceHilights(pFace, pFace.uiAutoDisplayBuffer, pFace.usFaceX, pFace.usFaceY);
            }
        }
    }


    void HandleTalkingAutoFace(int iFaceIndex)
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
                    if (!SoundIsPlaying(pFace.uiSoundID) && !pFace.fFinishTalking)
                    {
                        SetupFinalTalkingDelay(pFace);
                    }
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
                        AudioGapListDone(&(pFace.GapList));

                        // Remove video overlay is present....
                        if (pFace.iVideoOverlay != -1)
                        {
                            //if ( pFace.uiStopOverlayTimer == 0 )
                            //{
                            //	pFace.uiStopOverlayTimer = GetJA2Clock();
                            //}
                        }

                        // Call dialogue handler function
                        HandleDialogueEnd(pFace);
                    }
                }
            }
        }
    }


    // Local function - uses these variables because they have already been validated
    private static void SetFaceShade(SOLDIERTYPE? pSoldier, FACETYPE? pFace, bool fExternBlit)
    {
        // Set to default
        SetObjectHandleShade(pFace.uiVideoObject, FLASH_PORTRAIT_NOSHADE);

        if (pFace.iVideoOverlay == -1 && !fExternBlit)
        {
            if ((pSoldier.bActionPoints == 0) && !(gTacticalStatus.uiFlags & REALTIME) && (gTacticalStatus.uiFlags & INCOMBAT))
            {
                SetObjectHandleShade(pFace.uiVideoObject, FLASH_PORTRAIT_LITESHADE);
            }
        }

        if (pSoldier.bLife < OKLIFE)
        {
            SetObjectHandleShade(pFace.uiVideoObject, FLASH_PORTRAIT_DARKSHADE);
        }

        // ATE: Don't shade for damage if blitting extern face...
        if (!fExternBlit)
        {
            if (pSoldier.fFlashPortrait == FLASH_PORTRAIT_START)
            {
                SetObjectHandleShade(pFace.uiVideoObject, pSoldier.bFlashPortraitFrame);
            }
        }
    }

    bool RenderAutoFaceFromSoldier(int ubSoldierID)
    {
        // Check for valid soldier
        CHECKF(ubSoldierID != NOBODY);

        return (RenderAutoFace(MercPtrs[ubSoldierID].iFaceIndex));
    }

    void GetXYForIconPlacement(FACETYPE? pFace, int ubIndex, int sFaceX, int sFaceY, out int psX, out int psY)
    {
        int sX, sY;
        int usWidth, usHeight;
        ETRLEObject? pTrav;
        HVOBJECT hVObject;


        // Get height, width of icon...
        GetVideoObject(&hVObject, guiPORTRAITICONS);
        pTrav = &(hVObject.pETRLEObject[ubIndex]);
        usHeight = pTrav.usHeight;
        usWidth = pTrav.usWidth;

        sX = sFaceX + pFace.usFaceWidth - usWidth - 1;
        sY = sFaceY + pFace.usFaceHeight - usHeight - 1;

        psX = sX;
        psY = sY;
    }

    void GetXYForRightIconPlacement(FACETYPE? pFace, int ubIndex, int sFaceX, int sFaceY, out int psX, out int psY, int bNumIcons)
    {
        int sX, sY;
        int usWidth, usHeight;
        ETRLEObject? pTrav;
        HVOBJECT hVObject;


        // Get height, width of icon...
        GetVideoObject(hVObject, guiPORTRAITICONS);
        pTrav = (hVObject.pETRLEObject[ubIndex]);
        usHeight = pTrav.usHeight;
        usWidth = pTrav.usWidth;

        sX = sFaceX + (usWidth * bNumIcons) + 1;
        sY = sFaceY + pFace.usFaceHeight - usHeight - 1;

        psX = sX;
        psY = sY;
    }



    void DoRightIcon(int uiRenderBuffer, FACETYPE? pFace, int sFaceX, int sFaceY, int bNumIcons, int sIconIndex)
    {
        int sIconX, sIconY;

        // Find X, y for placement
        GetXYForRightIconPlacement(pFace, sIconIndex, sFaceX, sFaceY, out sIconX, out sIconY, bNumIcons);
        BltVideoObjectFromIndex(uiRenderBuffer, guiPORTRAITICONS, sIconIndex, sIconX, sIconY, VO_BLT_SRCTRANSPARENCY, null);
    }


    void HandleRenderFaceAdjustments(FACETYPE? pFace, bool fDisplayBuffer, bool fUseExternBuffer, int uiBuffer, int sFaceX, int sFaceY, int usEyesX, int usEyesY)
    {
        int sIconX, sIconY;
        int sIconIndex = -1;
        bool fDoIcon = false;
        int uiRenderBuffer;
        int sPtsAvailable = 0;
        int usMaximumPts = 0;
        string sString;
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

            if ((MercPtrs[pFace.ubSoldierID].bLife < CONSCIOUSNESS || MercPtrs[pFace.ubSoldierID].fDeadPanel))
            {
                // Blit Closed eyes here!
                BltVideoObjectFromIndex(uiRenderBuffer, pFace.uiVideoObject, 1, usEyesX, usEyesY, VO_BLT_SRCTRANSPARENCY, null);

                // Blit hatch!
                BltVideoObjectFromIndex(uiRenderBuffer, guiHATCH, 0, sFaceX, sFaceY, VO_BLT_SRCTRANSPARENCY, null);
            }

            if (MercPtrs[pFace.ubSoldierID].fMercAsleep == true)
            {
                // blit eyes closed
                BltVideoObjectFromIndex(uiRenderBuffer, pFace.uiVideoObject, 1, usEyesX, usEyesY, VO_BLT_SRCTRANSPARENCY, null);
            }

            if ((pSoldier.uiStatusFlags & SOLDIER_DEAD))
            {
                // IF we are in the process of doing any deal/close animations, show face, not skill...
                if (!pSoldier.fClosePanel && !pSoldier.fDeadPanel && !pSoldier.fUIdeadMerc && !pSoldier.fUICloseMerc)
                {
                    // Put close panel there
                    BltVideoObjectFromIndex(uiRenderBuffer, guiDEAD, 5, sFaceX, sFaceY, VO_BLT_SRCTRANSPARENCY, null);

                    // Blit hatch!
                    BltVideoObjectFromIndex(uiRenderBuffer, guiHATCH, 0, sFaceX, sFaceY, VO_BLT_SRCTRANSPARENCY, null);
                }
            }

            // ATE: If talking in popup, don't do the other things.....
            if (pFace.fTalking && gTacticalStatus.uiFlags & IN_ENDGAME_SEQUENCE)
            {
                return;
            }

            // ATE: Only do this, because we can be talking during an interrupt....
            if ((pFace.uiFlags & FACE_INACTIVE_HANDLED_ELSEWHERE) && !fUseExternBuffer)
            {
                // Don't do this if we are being handled elsewhere and it's not an extern buffer...
            }
            else
            {
                HandleFaceHilights(pFace, uiRenderBuffer, sFaceX, sFaceY);

                if (pSoldier.bOppCnt > 0)
                {
                    SetFontDestBuffer(uiRenderBuffer, 0, 0, 640, 480, false);

                    wprintf(sString, "%d", pSoldier.bOppCnt);

                    SetFont(TINYFONT1);
                    SetFontForeground(FONT_DKRED);
                    SetFontBackground(FONT_NEARBLACK);

                    sX1 = (int)(sFaceX);
                    sY1 = (int)(sFaceY);

                    sX2 = sX1 + StringPixLength(sString, TINYFONT1) + 1;
                    sY2 = sY1 + GetFontHeight(TINYFONT1) - 1;

                    mprintf((int)(sX1 + 1), (int)(sY1 - 1), sString);
                    SetFontDestBuffer(FRAME_BUFFER, 0, 0, 640, 480, false);

                    // Draw box
                    pDestBuf = LockVideoSurface(uiRenderBuffer, &uiDestPitchBYTES);
                    SetClippingRegionAndImageWidth(uiDestPitchBYTES, 0, 0, 640, 480);

                    usLineColor = Get16BPPColor(FROMRGB(105, 8, 9));
                    RectangleDraw(true, sX1, sY1, sX2, sY2, usLineColor, pDestBuf);

                    UnLockVideoSurface(uiRenderBuffer);

                }

                if (MercPtrs[pFace.ubSoldierID].bInSector && (((gTacticalStatus.ubCurrentTeam != OUR_TEAM) || !OK_INTERRUPT_MERC(MercPtrs[pFace.ubSoldierID])) && !gfHiddenInterrupt) || ((gfSMDisableForItems && !gfInItemPickupMenu) && gusSMCurrentMerc == pFace.ubSoldierID && gsCurInterfacePanel == SM_PANEL))
                {
                    // Blit hatch!
                    BltVideoObjectFromIndex(uiRenderBuffer, guiHATCH, 0, sFaceX, sFaceY, VO_BLT_SRCTRANSPARENCY, null);
                }

                if (!pFace.fDisabled && !pFace.fInvalidAnim)
                {
                    // Render text above here if that's what was asked for
                    if (pFace.fDisplayTextOver != FACE_NO_TEXT_OVER)
                    {
                        SetFont(TINYFONT1);
                        SetFontBackground(FONT_MCOLOR_BLACK);
                        SetFontForeground(FONT_MCOLOR_WHITE);

                        SetFontDestBuffer(uiRenderBuffer, 0, 0, 640, 480, false);

                        VarFindFontCenterCoordinates(sFaceX, sFaceY, pFace.usFaceWidth, pFace.usFaceHeight, TINYFONT1, &sFontX, &sFontY, pFace.zDisplayText);

                        if (pFace.fDisplayTextOver == FACE_DRAW_TEXT_OVER)
                        {
                            gprintfinvalidate(sFontX, sFontY, pFace.zDisplayText);
                            mprintf(sFontX, sFontY, pFace.zDisplayText);
                        }
                        else if (pFace.fDisplayTextOver == FACE_ERASE_TEXT_OVER)
                        {
                            gprintfRestore(sFontX, sFontY, pFace.zDisplayText);
                            pFace.fDisplayTextOver = FACE_NO_TEXT_OVER;
                        }

                        SetFontDestBuffer(FRAME_BUFFER, 0, 0, 640, 480, false);

                    }
                }

            }

            // Check if a robot and is not controlled....
            if (MercPtrs[pFace.ubSoldierID].uiStatusFlags & SOLDIER.ROBOT)
            {
                if (!CanRobotBeControlled(MercPtrs[pFace.ubSoldierID]))
                {
                    // Not controlled robot
                    sIconIndex = 5;
                    fDoIcon = true;
                }
            }

            if (ControllingRobot(MercPtrs[pFace.ubSoldierID]))
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

            if (MercPtrs[pFace.ubSoldierID].bDrugEffect[DRUG_TYPE_ADRENALINE])
            {
                DoRightIcon(uiRenderBuffer, pFace, sFaceX, sFaceY, bNumRightIcons, 7);
                bNumRightIcons++;
            }

            if (GetDrunkLevel(MercPtrs[pFace.ubSoldierID]) != SOBER)
            {
                DoRightIcon(uiRenderBuffer, pFace, sFaceX, sFaceY, bNumRightIcons, 8);
                bNumRightIcons++;
            }


            switch (pSoldier.bAssignment)
            {
                case Assignments.DOCTOR:

                    sIconIndex = 1;
                    fDoIcon = true;
                    sPtsAvailable = CalculateHealingPointsForDoctor(MercPtrs[pFace.ubSoldierID], usMaximumPts, false);
                    fShowNumber = true;
                    fShowMaximum = true;

                    // divide both amounts by 10 to make the displayed numbers a little more user-palatable (smaller)
                    sPtsAvailable = (sPtsAvailable + 5) / 10;
                    usMaximumPts = (usMaximumPts + 5) / 10;
                    break;

                case Assignments.PATIENT:

                    sIconIndex = 2;
                    fDoIcon = true;
                    // show current health / maximum health
                    sPtsAvailable = MercPtrs[pFace.ubSoldierID].bLife;
                    usMaximumPts = MercPtrs[pFace.ubSoldierID].bLifeMax;
                    fShowNumber = true;
                    fShowMaximum = true;
                    break;

                case Assignments.TRAIN_SELF:
                case Assignments.TRAIN_TOWN:
                case Assignments.TRAIN_TEAMMATE:
                case Assignments.TRAIN_BY_OTHER:
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
                        case (Assignments.TRAIN_SELF):
                            sPtsAvailable = GetSoldierTrainingPts(MercPtrs[pFace.ubSoldierID], MercPtrs[pFace.ubSoldierID].bTrainStat, fAtGunRange, usMaximumPts);
                            break;
                        case (Assignments.TRAIN_BY_OTHER):
                            sPtsAvailable = GetSoldierStudentPts(MercPtrs[pFace.ubSoldierID], MercPtrs[pFace.ubSoldierID].bTrainStat, fAtGunRange, usMaximumPts);
                            break;
                        case (Assignments.TRAIN_TOWN):
                            sPtsAvailable = GetTownTrainPtsForCharacter(MercPtrs[pFace.ubSoldierID], &usMaximumPts);
                            // divide both amounts by 10 to make the displayed numbers a little more user-palatable (smaller)
                            sPtsAvailable = (sPtsAvailable + 5) / 10;
                            usMaximumPts = (usMaximumPts + 5) / 10;
                            break;
                        case (Assignments.TRAIN_TEAMMATE):
                            sPtsAvailable = GetBonusTrainingPtsDueToInstructor(MercPtrs[pFace.ubSoldierID], null, MercPtrs[pFace.ubSoldierID].bTrainStat, fAtGunRange, &usMaximumPts);
                            break;
                    }
                    break;

                case Assignments.REPAIR:

                    sIconIndex = 0;
                    fDoIcon = true;
                    sPtsAvailable = CalculateRepairPointsForRepairman(MercPtrs[pFace.ubSoldierID], &usMaximumPts, false);
                    fShowNumber = true;
                    fShowMaximum = true;

                    // check if we are repairing a vehicle
                    if (Menptr[pFace.ubSoldierID].bVehicleUnderRepairID != -1)
                    {
                        // reduce to a multiple of VEHICLE_REPAIR_POINTS_DIVISOR.  This way skill too low will show up as 0 repair pts.
                        sPtsAvailable -= (sPtsAvailable % VEHICLE_REPAIR_POINTS_DIVISOR);
                        usMaximumPts -= (usMaximumPts % VEHICLE_REPAIR_POINTS_DIVISOR);
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
                GetXYForIconPlacement(pFace, sIconIndex, sFaceX, sFaceY, &sIconX, &sIconY);
                BltVideoObjectFromIndex(uiRenderBuffer, guiPORTRAITICONS, sIconIndex, sIconX, sIconY, VO_BLT_SRCTRANSPARENCY, null);

                // ATE: Show numbers only in mapscreen
                if (fShowNumber)
                {
                    SetFontDestBuffer(uiRenderBuffer, 0, 0, 640, 480, false);

                    if (fShowMaximum)
                    {
                        wprintf(sString, "%d/%d", sPtsAvailable, usMaximumPts);
                    }
                    else
                    {
                        wprintf(sString, "%d", sPtsAvailable);
                    }

                    usTextWidth = StringPixLength(sString, FONT10ARIAL);
                    usTextWidth += 1;

                    SetFont(FONT10ARIAL);
                    SetFontForeground(FONT_YELLOW);
                    SetFontBackground(FONT_BLACK);

                    mprintf(sFaceX + pFace.usFaceWidth - usTextWidth, (int)(sFaceY + 3), sString);
                    SetFontDestBuffer(FRAME_BUFFER, 0, 0, 640, 480, false);
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
            if (pFace.uiAutoRestoreBuffer == guiSAVEBUFFER)
            {
                BltVideoObjectFromIndex(pFace.uiAutoRestoreBuffer, pFace.uiVideoObject, 0, pFace.usFaceX, pFace.usFaceY, VO_BLT_SRCTRANSPARENCY, null);
            }
            else
            {
                BltVideoObjectFromIndex(pFace.uiAutoRestoreBuffer, pFace.uiVideoObject, 0, 0, 0, VO_BLT_SRCTRANSPARENCY, null);
            }
        }

        HandleRenderFaceAdjustments(pFace, false, false, 0, pFace.usFaceX, pFace.usFaceY, pFace.usEyesX, pFace.usEyesY);

        // Restore extern rect
        if (pFace.uiAutoRestoreBuffer == guiSAVEBUFFER)
        {
            FaceRestoreSavedBackgroundRect(iFaceIndex, (int)(pFace.usFaceX), (int)(pFace.usFaceY), (int)(pFace.usFaceX), (int)(pFace.usFaceY), (int)(pFace.usFaceWidth), (int)(pFace.usFaceHeight));
        }
        else
        {
            FaceRestoreSavedBackgroundRect(iFaceIndex, pFace.usFaceX, pFace.usFaceY, 0, 0, pFace.usFaceWidth, pFace.usFaceHeight);
        }

        return (true);
    }


    public static bool ExternRenderFaceFromSoldier(int uiBuffer, int ubSoldierID, int sX, int sY)
    {
        // Check for valid soldier
        CHECKF(ubSoldierID != NOBODY);

        return (ExternRenderFace(uiBuffer, MercPtrs[ubSoldierID].iFaceIndex, sX, sY));
    }


    public static bool ExternRenderFace(int uiBuffer, int iFaceIndex, int sX, int sY)
    {
        int usEyesX;
        int usEyesY;
        int usMouthX;
        int usMouthY;
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
        BltVideoObjectFromIndex(uiBuffer, pFace.uiVideoObject, 0, sX, sY, VO_BLT_SRCTRANSPARENCY, null);

        GetFaceRelativeCoordinates(pFace, &usEyesX, &usEyesY, &usMouthX, &usMouthY);

        HandleRenderFaceAdjustments(pFace, false, true, uiBuffer, sX, sY, (int)(sX + usEyesX), (int)(sY + usEyesY));

        // Restore extern rect
        if (uiBuffer == guiSAVEBUFFER)
        {
            RestoreExternBackgroundRect(sX, sY, pFace.usFaceWidth, pFace.usFaceWidth);
        }

        return (true);
    }



    public static void NewEye(FACETYPE? pFace)
    {

        switch (pFace.sEyeFrame)
        {
            case 0: //pFace.sEyeFrame = (int)Globals.Random.Next(2);	// normal - can blink or frown
                if (pFace.ubExpression == ANGRY)
                {
                    pFace.ubEyeWait = 0;
                    pFace.sEyeFrame = 3;
                }
                else if (pFace.ubExpression == SURPRISED)
                {
                    pFace.ubEyeWait = 0;
                    pFace.sEyeFrame = 4;
                }
                else
                {
                    //if (pFace.sEyeFrame && Talk.talking && Talk.expression != DYING)
                    ///    pFace.sEyeFrame = 3;
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
                                                                  //break;
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


    void NewMouth(FACETYPE? pFace)
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
        int bInSector;
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
                    bLife = pSoldier.bLife;
                    bInSector = pSoldier.bInSector;
                    bAPs = pSoldier.bActionPoints;

                    if (pSoldier.ubID == gsSelectedGuy && gfUIHandleSelectionAboveGuy)
                    {
                        pFace.uiFlags |= FACE_SHOW_WHITE_HILIGHT;
                    }
                    else
                    {
                        pFace.uiFlags &= (~FACE_SHOW_WHITE_HILIGHT);
                    }

                    if (pSoldier.sGridNo != pSoldier.sFinalDestination && pSoldier.sGridNo != NOWHERE)
                    {
                        pFace.uiFlags |= FACE_SHOW_MOVING_HILIGHT;
                    }
                    else
                    {
                        pFace.uiFlags &= (~FACE_SHOW_MOVING_HILIGHT);
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

                    if (!(pFace.uiFlags & FACE_SHOW_WHITE_HILIGHT) && pFace.fOldShowHighlight)
                    {
                        fRerender = true;
                    }

                    if ((pFace.uiFlags & FACE_SHOW_WHITE_HILIGHT) && !(pFace.fOldShowHighlight))
                    {
                        fRerender = true;
                    }

                    if (!(pFace.uiFlags & FACE_SHOW_MOVING_HILIGHT) && pFace.fOldShowMoveHilight)
                    {
                        fRerender = true;
                    }

                    if ((pFace.uiFlags & FACE_SHOW_MOVING_HILIGHT) && !(pFace.fOldShowMoveHilight))
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

                    if (pFace.uiFlags & FACE_SHOW_WHITE_HILIGHT)
                    {
                        pFace.fOldShowHighlight = true;
                    }
                    else
                    {
                        pFace.fOldShowHighlight = false;
                    }

                    if (pFace.uiFlags & FACE_SHOW_MOVING_HILIGHT)
                    {
                        pFace.fOldShowMoveHilight = true;
                    }
                    else
                    {
                        pFace.fOldShowMoveHilight = false;
                    }


                    if (pSoldier.fGettingHit && pSoldier.fFlashPortrait == FLASH_PORTRAIT_STOP)
                    {
                        pSoldier.fFlashPortrait = true;
                        pSoldier.bFlashPortraitFrame = FLASH_PORTRAIT_STARTSHADE;
                        RESETTIMECOUNTER(pSoldier.PortraitFlashCounter, FLASH_PORTRAIT_DELAY);
                        fRerender = true;
                    }
                    if (pSoldier.fFlashPortrait == FLASH_PORTRAIT_START)
                    {
                        // Loop through flash values
                        if (TIMECOUNTERDONE(pSoldier.PortraitFlashCounter, FLASH_PORTRAIT_DELAY))
                        {
                            RESETTIMECOUNTER(pSoldier.PortraitFlashCounter, FLASH_PORTRAIT_DELAY);
                            pSoldier.bFlashPortraitFrame++;

                            if (pSoldier.bFlashPortraitFrame > FLASH_PORTRAIT_ENDSHADE)
                            {
                                pSoldier.bFlashPortraitFrame = FLASH_PORTRAIT_ENDSHADE;

                                if (pSoldier.fGettingHit)
                                {
                                    pSoldier.fFlashPortrait = FLASH_PORTRAIT_WAITING;
                                }
                                else
                                {
                                    // Render face again!
                                    pSoldier.fFlashPortrait = FLASH_PORTRAIT_STOP;
                                }

                                fRerender = true;
                            }
                        }
                    }
                    // CHECK IF WE WERE WAITING FOR GETTING HIT TO FINISH!
                    if (!pSoldier.fGettingHit && pSoldier.fFlashPortrait == FLASH_PORTRAIT_WAITING)
                    {
                        pSoldier.fFlashPortrait = false;
                        fRerender = true;
                    }

                    if (pSoldier.fFlashPortrait == FLASH_PORTRAIT_START)
                    {
                        fRerender = true;
                    }

                    if (pFace.uiFlags & FACE_REDRAW_WHOLE_FACE_NEXT_FRAME)
                    {
                        pFace.uiFlags &= ~FACE_REDRAW_WHOLE_FACE_NEXT_FRAME;

                        fRerender = true;
                    }

                    if (fInterfacePanelDirty == DIRTYLEVEL2 && guiCurrentScreen == GAME_SCREEN)
                    {
                        fRerender = true;
                    }

                    if (fRerender)
                    {
                        RenderAutoFace(uiCount);
                    }

                    if (bLife < CONSCIOUSNESS)
                    {
                        fHandleFace = false;
                    }
                }

                if (fHandleFace)
                {
                    BlinkAutoFace(uiCount);
                }

                MouthAutoFace(uiCount);

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
        int pDestBuf, pSrcBuf;

        // Check face index
        CHECKF(iFaceIndex != -1);

        pFace = gFacesData[iFaceIndex];

        // DOn't continue if we do not want the resotre to happen ( ei blitting entrie thing every frame...
        if (pFace.uiAutoRestoreBuffer == FACE_NO_RESTORE_BUFFER)
        {
            return (false);
        }

        pDestBuf = LockVideoSurface(pFace.uiAutoDisplayBuffer, &uiDestPitchBYTES);
        pSrcBuf = LockVideoSurface(pFace.uiAutoRestoreBuffer, &uiSrcPitchBYTES);

        Blt16BPPTo16BPP(pDestBuf, uiDestPitchBYTES,
                    pSrcBuf, uiSrcPitchBYTES,
                    sDestLeft, sDestTop,
                    sSrcLeft, sSrcTop,
                    sWidth, sHeight);

        UnLockVideoSurface(pFace.uiAutoDisplayBuffer);
        UnLockVideoSurface(pFace.uiAutoRestoreBuffer);

        // Add rect to frame buffer queue
        if (pFace.uiAutoDisplayBuffer == FRAME_BUFFER)
        {
            InvalidateRegionEx(sDestLeft - 2, sDestTop - 2, (sDestLeft + sWidth + 3), (sDestTop + sHeight + 2), 0);
        }
        return (true);
    }


    bool SetFaceTalking(int iFaceIndex, string zSoundFile, string zTextString, int usRate, int ubVolume, int ubLoops, int uiPan)
    {
        FACETYPE? pFace;

        pFace = gFacesData[iFaceIndex];

        // Set face to talking
        pFace.fTalking = true;
        pFace.fAnimatingTalking = true;
        pFace.fFinishTalking = false;

        if (!AreInMeanwhile())
        {
            TurnOnSectorLocator(pFace.ubCharacterNum);
        }

        // Play sample
        if (gGameSettings.fOptions[TOPTION_SPEECH])
        {
            pFace.uiSoundID = PlayJA2GapSample(zSoundFile, RATE_11025, HIGHVOLUME, 1, MIDDLEPAN, &(pFace.GapList));
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

            pFace.uiTalkingDuration = FindDelayForString(zTextString);
        }

        return (true);
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

        return (true);
    }



    void InternalShutupaYoFace(int iFaceIndex, bool fForce)
    {
        FACETYPE? pFace;

        // Check face index
        CHECKV(iFaceIndex != -1);

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
                SoundStop(pFace.uiSoundID);
            }

            // Remove gap info
            AudioGapListDone(&(pFace.GapList));

            // Shutup mouth!
            pFace.sMouthFrame = 0;

            // ATE: Only change if active!
            if (!pFace.fDisabled)
            {
                if (pFace.uiAutoRestoreBuffer == guiSAVEBUFFER)
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
            HandleDialogueEnd(pFace);

            pFace.fTalking = false;
            pFace.fAnimatingTalking = false;

            gfUIWaitingForUserSpeechAdvance = false;

        }

    }

    void ShutupaYoFace(int iFaceIndex)
    {
        InternalShutupaYoFace(iFaceIndex, true);
    }

    void SetupFinalTalkingDelay(FACETYPE? pFace)
    {
        pFace.fFinishTalking = true;

        pFace.fAnimatingTalking = false;

        pFace.uiTalkingTimer = GetJA2Clock();

        if (gGameSettings.fOptions[TOPTION_SUBTITLES])
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
            if (pFace.uiAutoRestoreBuffer == guiSAVEBUFFER)
            {
                FaceRestoreSavedBackgroundRect(pFace.iID, pFace.usMouthX, pFace.usMouthY, pFace.usMouthX, pFace.usMouthY, pFace.usMouthWidth, pFace.usMouthHeight);
            }
            else
            {
                FaceRestoreSavedBackgroundRect(pFace.iID, pFace.usMouthX, pFace.usMouthY, pFace.usMouthOffsetX, pFace.usMouthOffsetY, pFace.usMouthWidth, pFace.usMouthHeight);
            }
        }

        // Setup flag to wait for advance ( because we have no text! )
        if (gGameSettings.fOptions[TOPTION_KEY_ADVANCE_SPEECH] && (pFace.uiFlags & FACE_POTENTIAL_KEYWAIT))
        {

            // Check if we have had valid speech!
            if (!pFace.fValidSpeech || gGameSettings.fOptions[TOPTION_SUBTITLES])
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

[Flags]
public enum FaceFlags
{
    // FLAGS....
    FACE_DESTROY_OVERLAY = 0x00000000,                      // A face may contain a video overlay
    FACE_BIGFACE = 0x00000001,                      // A BIGFACE instead of small face
    FACE_POTENTIAL_KEYWAIT = 0x00000002,                        // If the option is set, will not stop face until key pressed
    FACE_PCTRIGGER_NPC = 0x00000004,                        // This face has to trigger an NPC after being done
    FACE_INACTIVE_HANDLED_ELSEWHERE = 0x00000008,   // This face has been setup and any disable should be done
                                                    // Externally                   
    FACE_TRIGGER_PREBATTLE_INT = 0x00000010,
    FACE_SHOW_WHITE_HILIGHT = 0x00000020,           // Show highlight around face
    FACE_FORCE_SMALL = 0x00000040,                      // force to small face	
    FACE_MODAL = 0x00000080,                        // make game modal
    FACE_MAKEACTIVE_ONCE_DONE = 0x00000100,
    FACE_SHOW_MOVING_HILIGHT = 0x00000200,
    FACE_REDRAW_WHOLE_FACE_NEXT_FRAM = 0x00000400,						// Redraw the complete face next frame
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
