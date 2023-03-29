using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.SubSystems;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class AnimationCache
{
    void DetermineOptimumAnimationCacheSize()
    {
        // If we have lots-a memory, adjust accordingly!
        guiCacheSize = MIN_CACHE_SIZE;
    }

    bool InitAnimationCache(int usSoldierID, AnimationSurfaceCacheType pAnimCache)
    {
        int cnt;

        // Allocate entries
        Messages.AnimDebugMsg(string.Format("*** Initializing anim cache surface for soldier %d", usSoldierID));
        pAnimCache.usCachedSurfaces = new();

        Messages.AnimDebugMsg(string.Format("*** Initializing anim cache hit counter for soldier %d", usSoldierID));
        // pAnimCache.sCacheHits = MemAlloc(sizeof(int) * guiCacheSize);
        pAnimCache.sCacheHits = new();

        // Zero entries
        for (cnt = 0; cnt < guiCacheSize; cnt++)
        {
            pAnimCache.usCachedSurfaces[cnt] = EMPTY_CACHE_ENTRY;
            pAnimCache.sCacheHits[cnt] = 0;
        }
        pAnimCache.ubCacheSize = 0;

        // Zero surface databse history for this soldeir
        ClearAnimationSurfacesUsageHistory(usSoldierID);

        return (true);
    }

    void DeleteAnimationCache(int usSoldierID, AnimationSurfaceCacheType pAnimCache)
    {
        // Allocate entries
        if (pAnimCache.usCachedSurfaces != null)
        {
            Messages.AnimDebugMsg(string.Format("*** Removing Anim Cache surface for soldier %d", usSoldierID));
            MemFree(pAnimCache.usCachedSurfaces);
        }

        if (pAnimCache.sCacheHits != null)
        {
            Messages.AnimDebugMsg(string.Format("*** Removing Anim Cache hit counter for soldier %d", usSoldierID));
            MemFree(pAnimCache.sCacheHits);
        }
    }


    bool GetCachedAnimationSurface(int usSoldierID, AnimationSurfaceCacheType pAnimCache, AnimationSurfaceTypes usSurfaceIndex, AnimationStates usCurrentAnimation)
    {
        int cnt;
        int ubLowestIndex = 0;
        int sMostHits = (int)32000;
        AnimationSurfaceTypes usCurrentAnimSurface;

        // Check to see if surface exists already
        for (cnt = 0; cnt < pAnimCache.ubCacheSize; cnt++)
        {
            if (pAnimCache.usCachedSurfaces[cnt] == usSurfaceIndex)
            {
                // Found surface, return
                Messages.AnimDebugMsg(string.Format("Anim Cache: Hit %d ( Soldier %d )", usSurfaceIndex, usSoldierID));
                pAnimCache.sCacheHits[cnt]++;
                return (true);
            }
        }

        // Check if max size has been reached
        if (pAnimCache.ubCacheSize == guiCacheSize)
        {
            Messages.AnimDebugMsg(string.Format("Anim Cache: Determining Bump Candidate ( Soldier %d )", usSoldierID));

            // Determine exisiting surface used by merc
            usCurrentAnimSurface = AnimationControl.DetermineSoldierAnimationSurface(MercPtrs[usSoldierID], usCurrentAnimation);
            // If the surface we are going to bump is our existing animation, reject it as a candidate

            // If we get here, we need to remove an animation, pick the best one
            // Loop through and pick one with lowest cache hits
            for (cnt = 0; cnt < pAnimCache.ubCacheSize; cnt++)
            {
                Messages.AnimDebugMsg(string.Format("Anim Cache: Slot %d Hits %d ( Soldier %d )", cnt, pAnimCache.sCacheHits[cnt], usSoldierID));

                if (pAnimCache.usCachedSurfaces[cnt] == usCurrentAnimSurface)
                {
                    Messages.AnimDebugMsg(string.Format("Anim Cache: REJECTING Slot %d EXISTING ANIM SURFACE ( Soldier %d )", cnt, usSoldierID));
                }
                else
                {
                    if (pAnimCache.sCacheHits[cnt] < sMostHits)
                    {
                        sMostHits = pAnimCache.sCacheHits[cnt];
                        ubLowestIndex = cnt;
                    }
                }
            }

            // Bump off lowest index
            Messages.AnimDebugMsg(string.Format("Anim Cache: Bumping %d ( Soldier %d )", ubLowestIndex, usSoldierID));
            UnLoadAnimationSurface(usSoldierID, pAnimCache.usCachedSurfaces[ubLowestIndex]);

            // Decrement
            pAnimCache.sCacheHits[ubLowestIndex] = 0;
            pAnimCache.usCachedSurfaces[ubLowestIndex] = EMPTY_CACHE_ENTRY;
            pAnimCache.ubCacheSize--;

        }

        // If here, Insert at an empty slot
        // Find an empty slot
        for (cnt = 0; cnt < guiCacheSize; cnt++)
        {
            if (pAnimCache.usCachedSurfaces[cnt] == EMPTY_CACHE_ENTRY)
            {
                Messages.AnimDebugMsg(string.Format("Anim Cache: Loading Surface %d ( Soldier %d )", usSurfaceIndex, usSoldierID));

                // Insert here
                CHECKF(LoadAnimationSurface(usSoldierID, usSurfaceIndex, usCurrentAnimation) != false);
                pAnimCache.sCacheHits[cnt] = 0;
                pAnimCache.usCachedSurfaces[cnt] = usSurfaceIndex;
                pAnimCache.ubCacheSize++;

                break;
            }
        }

        return (true);
    }



    void UnLoadCachedAnimationSurfaces(int usSoldierID, AnimationSurfaceCacheType* pAnimCache)
    {
        int cnt;

        // Check to see if surface exists already
        for (cnt = 0; cnt < pAnimCache.ubCacheSize; cnt++)
        {
            if (pAnimCache.usCachedSurfaces[cnt] != EMPTY_CACHE_ENTRY)
            {
                UnLoadAnimationSurface(usSoldierID, pAnimCache.usCachedSurfaces[cnt]);
            }
        }

    }
}

public struct AnimationSurfaceCacheType
{
    public List<AnimationSurfaceTypes> usCachedSurfaces;
    public List<int> sCacheHits;
    public int ubCacheSize;
}

