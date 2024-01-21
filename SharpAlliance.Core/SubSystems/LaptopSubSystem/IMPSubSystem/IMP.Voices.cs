using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core.SubSystems.LaptopSubSystem.IMPSubSystem;

public partial class IMP
{
    //current and last pages
    public static int iCurrentVoices = 0;
    public static int iLastVoice = 2;
}

public enum IMP_PAGE
{
    IMP_HOME_PAGE,
    IMP_BEGIN,
    IMP_FINISH,
    IMP_MAIN_PAGE,
    IMP_PERSONALITY,
    IMP_PERSONALITY_QUIZ,
    IMP_PERSONALITY_FINISH,
    IMP_ATTRIBUTE_ENTRANCE,
    IMP_ATTRIBUTE_PAGE,
    IMP_ATTRIBUTE_FINISH,
    IMP_PORTRAIT,
    IMP_VOICE,
    IMP_ABOUT_US,
    IMP_CONFIRM,

    IMP_NUM_PAGES,
};
