using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core;
using SharpAlliance.Core.Managers;
using SharpAlliance.Core.Managers.Image;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public class TileSurface
{
    public static TILE_IMAGERY? LoadTileSurface(string cFilename)
    {
        // Add tile surface
        TILE_IMAGERY pTileSurf = null;
        VOBJECT_DESC VObjectDesc;
        HVOBJECT hVObject;
        HIMAGE hImage;
        SGPFILENAME cStructureFilename;
        STR cEndOfName;
        STRUCTURE_FILE_REF? pStructureFileRef;
        bool fOk;


        hImage = CreateImage(cFilename, IMAGE_ALLDATA);
        if (hImage == null)
        {
            // Report error
            SET_ERROR("Could not load tile file: %s", cFilename);
            return (null);
        }

        VObjectDesc.fCreateFlags = VOBJECT_CREATE_FROMHIMAGE;
        VObjectDesc.hImage = hImage;

        hVObject = CreateVideoObject(&VObjectDesc);

        if (hVObject == null)
        {
            // Report error
            SET_ERROR("Could not load tile file: %s", cFilename);
            // Video Object will set error conition.]
            DestroyImage(hImage);
            return (null);
        }

        // Load structure data, if any.
        // Start by hacking the image filename into that for the structure data
        strcpy(cStructureFilename, cFilename);
        cEndOfName = strchr(cStructureFilename, '.');
        if (cEndOfName != null)
        {
            cEndOfName++;
        }
        else
        {
            strcat(cStructureFilename, ".");
        }

        strcat(cStructureFilename, STRUCTURE_FILE_EXTENSION);
        if (FileExists(cStructureFilename))
        {
            pStructureFileRef = LoadStructureFile(cStructureFilename);
            if (pStructureFileRef == null || hVObject.usNumberOfObjects != pStructureFileRef.usNumberOfStructures)
            {
                DestroyImage(hImage);
                DeleteVideoObject(hVObject);
                SET_ERROR("Structure file error: %s", cStructureFilename);
                return (null);
            }

            // DebugMsg(TOPIC_JA2, DBG_LEVEL_3, cStructureFilename);

            fOk = AddZStripInfoToVObject(hVObject, pStructureFileRef, false, 0);
            if (fOk == false)
            {
                DestroyImage(hImage);
                DeleteVideoObject(hVObject);
                SET_ERROR("ZStrip creation error: %s", cStructureFilename);
                return (null);
            }

        }
        else
        {
            pStructureFileRef = null;
        }

        pTileSurf = MemAlloc(sizeof(TILE_IMAGERY));

        // Set all values to zero
        memset(pTileSurf, 0, sizeof(TILE_IMAGERY));

        pTileSurf.vo = hVObject;
        pTileSurf.pStructureFileRef = pStructureFileRef;

        if (pStructureFileRef is not null && pStructureFileRef.pAuxData != null)
        {
            pTileSurf.pAuxData = pStructureFileRef.pAuxData;
            pTileSurf.pTileLocData = pStructureFileRef.pTileLocData;
        }
        else if (hImage.uiAppDataSize == hVObject.usNumberOfObjects * sizeof(AuxObjectData))
        {
            // Valid auxiliary data, so make a copy of it for TileSurf
            pTileSurf.pAuxData = MemAlloc(hImage.uiAppDataSize);
            if (pTileSurf.pAuxData == null)
            {
                DestroyImage(hImage);
                DeleteVideoObject(hVObject);
                return (null);
            }
            memcpy(pTileSurf.pAuxData, hImage.pAppData, hImage.uiAppDataSize);
        }
        else
        {
            pTileSurf.pAuxData = null;
        }
        // the hImage is no longer needed
        DestroyImage(hImage);

        return (pTileSurf);
    }


    public static void DeleteTileSurface(TILE_IMAGERY pTileSurf)
    {
        if (pTileSurf.pStructureFileRef != null)
        {
            FreeStructureFile(pTileSurf.pStructureFileRef);
        }
        else
        {
            // If a structure file exists, it will free the auxdata.
            // Since there is no structure file in this instance, we
            // free it ourselves.
            if (pTileSurf.pAuxData != null)
            {
                pTileSurf.pAuxData = null;
            }
        }

        VeldridVideoManager.DeleteVideoObject(pTileSurf.vo);
    }


    void SetRaisedObjectFlag(string cFilename, TILE_IMAGERY? pTileSurf)
    {
        int cnt = 0;
        string cRootFile;
        string[] ubRaisedObjectFiles =
        {
            "bones",
            "bones2",
            "grass2",
            "grass3",
            "l_weed3",
            "litter",
            "miniweed",
            "sblast",
            "sweeds",
            "twigs",
            "wing",
            "1",
        };

        // Loop through array of RAISED objecttype imagery and
        // set global value...
        if ((pTileSurf.fType >= TileTypeDefines.DEBRISWOOD && pTileSurf.fType <= TileTypeDefines.DEBRISWEEDS)
            || pTileSurf.fType == TileTypeDefines.DEBRIS2MISC
            || pTileSurf.fType == TileTypeDefines.ANOTHERDEBRIS)
        {
            GetRootName(cRootFile, cFilename);
            while (ubRaisedObjectFiles[cnt][0] != '1')
            {
                if (stricmp(ubRaisedObjectFiles[cnt], cRootFile) == 0)
                {
                    pTileSurf.bRaisedObjectType = 1;
                }

                cnt++;
            }
        }
    }
}
