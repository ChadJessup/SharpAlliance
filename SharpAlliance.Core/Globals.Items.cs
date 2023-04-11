using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpAlliance.Core;

public partial class Globals
{
    public static Dictionary<Items, List<Items>> Launchable = new()
    {
        { Items.GL_HE_GRENADE,      new() { Items.GLAUNCHER, Items.UNDER_GLAUNCHER } },
        { Items.GL_TEARGAS_GRENADE, new() { Items.GLAUNCHER, Items.UNDER_GLAUNCHER } },
        { Items.GL_STUN_GRENADE,    new() { Items.GLAUNCHER, Items.UNDER_GLAUNCHER } },
        { Items.GL_SMOKE_GRENADE,   new() { Items.GLAUNCHER, Items.UNDER_GLAUNCHER } },
        { Items.MORTAR_SHELL,       new() { Items.MORTAR } },
        { Items.TANK_SHELL,         new() { Items.TANK_CANNON } },
        { (Items)0,                 new() { (Items)0 } }
    };

}
