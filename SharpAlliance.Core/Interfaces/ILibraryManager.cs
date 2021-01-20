using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers.Library;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.Interfaces
{
    public interface ILibraryManager : ISharpAllianceManager
    {
        bool IsLibraryOpened(LibraryNames iNTRO);
    }
}
