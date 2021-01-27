using System;
using SharpAlliance.Core.Managers.Image;

namespace SharpAlliance.Core.SubSystems
{
    public class StructureFile
    {
        public STRUCTURE_FILE_REF? LoadStructureFile(string sFilename)
        {
            return null;
        }
    }

    public class STRUCTURE_FILE_REF
    {
        STRUCTURE_FILE_REF pPrev;
        STRUCTURE_FILE_REF pNext;
        AuxObjectData pAuxData;
        RelTileLoc pTileLocData;
        byte pubStructureData;
        //List<DB_STRUCTURE_REF> pDBStructureRef; // dynamic array
        ushort usNumberOfStructures;
        ushort usNumberOfStructuresStored;
    }; // 24 bytes
}
