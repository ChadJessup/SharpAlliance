using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems.LaptopSubSystem.IMPSubSystem;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem;

public partial class Laptop
{
    public static void GameInitCharProfile()
    {
        LaptopSaveInfo.iVoiceId = 0;
        IMP.iCurrentPortrait = 0;
        IMP.iCurrentVoices = 0;
        IMP.iPortraitNumber = 0;
    }
}
