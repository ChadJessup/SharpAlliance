using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Platform.NullManagers
{
    public class NullSoundManager : ISoundManager, ISound2dManager, ISound3dManager
    {
        public bool Initialize()
        {
            return true;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
