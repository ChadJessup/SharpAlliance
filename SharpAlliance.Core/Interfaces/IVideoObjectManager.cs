using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.Managers;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Core.Interfaces
{
    public interface IVideoObjectManager : ISharpAllianceManager
    {
        void AddVideoObject(ref VOBJECT_DESC vObjectDesc, object guiUpdatePanelTactical);
    }
}
