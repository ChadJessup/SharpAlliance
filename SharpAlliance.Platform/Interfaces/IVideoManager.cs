using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Platform.Interfaces
{
    public interface IVideoManager : ISharpAllianceManager
    {
        void Draw();
        void RefreshScreen(object p);
    }
}
