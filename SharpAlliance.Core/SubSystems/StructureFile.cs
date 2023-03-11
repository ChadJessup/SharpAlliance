using System;
using System.Collections.Generic;
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
        public STRUCTURE_FILE_REF pPrev;
        public STRUCTURE_FILE_REF pNext;
        public AuxObjectData pAuxData;
        public RelTileLoc pTileLocData;
        public byte pubStructureData;
        public List<DB_STRUCTURE_REF> pDBStructureRef; // dynamic array
        public ushort usNumberOfStructures;
        public ushort usNumberOfStructuresStored;
    }; // 24 bytes
}
