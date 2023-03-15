using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{
    public static bool HAS_SKILL_TRAIT(SOLDIERTYPE s, SkillTrait t) => (s.ubSkillTrait1 == t || s.ubSkillTrait2 == t);
    public static int NUM_SKILL_TRAITS(SOLDIERTYPE s, SkillTrait t) => ((s.ubSkillTrait1 == t)
        ? ((s.ubSkillTrait2 == t) ? 2 : 1)
        : ((s.ubSkillTrait2 == t) ? 1 : 0));

}
