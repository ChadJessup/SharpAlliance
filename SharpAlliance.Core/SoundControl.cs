using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class SoundControl
{
    public static void SetPositionSndsVolumeAndPanning()
    {
        int cnt;
        POSITIONSND pPositionSnd;
        int bVolume, bPan;
        SOLDIERTYPE? pSoldier;

        for (cnt = 0; cnt < guiNumPositionSnds; cnt++)
        {
            pPositionSnd = gPositionSndData[cnt];

            if (pPositionSnd.fAllocated)
            {
                if (!pPositionSnd.fInActive)
                {
                    if (pPositionSnd.iSoundSampleID != NO_SAMPLE)
                    {
                        bVolume = PositionSoundVolume(15, pPositionSnd.sGridNo);

                        if ((pPositionSnd.uiFlags & POSITION_SOUND_FROM_SOLDIER) == 0)
                        {
                            pSoldier = (SOLDIERTYPE?)pPositionSnd.uiData;

                            if (pSoldier.bVisible == -1)
                            {
                                // Limit volume,,,
                                if (bVolume > 10)
                                {
                                    bVolume = 10;
                                }
                            }
                        }

                        SoundSetVolume(pPositionSnd.iSoundSampleID, bVolume);

                        bPan = PositionSoundDir(pPositionSnd.sGridNo);

                        SoundSetPan(pPositionSnd.iSoundSampleID, bPan);
                    }
                }
            }
        }
    }

    private static int PositionSoundVolume(int bInitialVolume, int sGridNo)
    {
        int sWorldX, sWorldY;
        int sScreenX, sScreenY;
        int sMiddleX, sMiddleY;
        int sDifX, sAbsDifX;
        int sDifY, sAbsDifY;
        int sMaxDistX, sMaxDistY;
        double sMaxSoundDist, sSoundDist;

        if (sGridNo == NOWHERE)
        {
            return (bInitialVolume);
        }

        // OK, get screen position of gridno.....
        IsometricUtils.ConvertGridNoToXY(sGridNo, out sWorldX, out sWorldY);

        // Get screen coordinates for current position of soldier
        IsometricUtils.GetWorldXYAbsoluteScreenXY((sWorldX), (sWorldY), out sScreenX, out sScreenY);

        // Get middle of where we are now....
        sMiddleX = gsTopLeftWorldX + (gsBottomRightWorldX - gsTopLeftWorldX) / 2;
        sMiddleY = gsTopLeftWorldY + (gsBottomRightWorldY - gsTopLeftWorldY) / 2;

        sDifX = sMiddleX - sScreenX;
        sDifY = sMiddleY - sScreenY;

        sAbsDifX = Math.Abs(sDifX);
        sAbsDifY = Math.Abs(sDifY);

        sMaxDistX = (int)((gsBottomRightWorldX - gsTopLeftWorldX) * 1.5);
        sMaxDistY = (int)((gsBottomRightWorldY - gsTopLeftWorldY) * 1.5);

        sMaxSoundDist = Math.Sqrt((double)(sMaxDistX * sMaxDistX) + (sMaxDistY * sMaxDistY));
        sSoundDist = Math.Sqrt((double)(sAbsDifX * sAbsDifX) + (sAbsDifY * sAbsDifY));

        if (sSoundDist == 0)
        {
            return (bInitialVolume);
        }

        if (sSoundDist > sMaxSoundDist)
        {
            sSoundDist = sMaxSoundDist;
        }

        // Scale
        return ((int)(bInitialVolume * ((sMaxSoundDist - sSoundDist) / sMaxSoundDist)));
    }
}

public class POSITIONSND
{
    public int uiFlags;
    public int sGridNo;
    public int iSoundSampleID;
    public int iSoundToPlay;
    public object? uiData;
    public bool fAllocated;
    public bool fInActive;
}
