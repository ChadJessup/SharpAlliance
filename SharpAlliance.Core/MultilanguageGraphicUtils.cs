using System;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public class MultilanguageGraphicUtils
{
    public static bool GetMLGFilename(out string filename, MLG usMLGGraphicID)
    {
        switch (usMLGGraphicID)
        {
            case MLG.AIMSYMBOL:
                filename = "LAPTOP\\AimSymbol.sti";
                return true;
            case MLG.BOBBYNAME:
                filename = "LAPTOP\\BobbyName.sti";
                return true;
            case MLG.BOBBYRAYAD21:
                filename = "LAPTOP\\BobbyRayAd_21.sti";
                return true;
            case MLG.BOBBYRAYLINK:
                filename = "LAPTOP\\BobbyRayLink.sti";
                return true;
            case MLG.CLOSED:
                filename = "LAPTOP\\Closed.sti";
                return true;
            case MLG.CONFIRMORDER:
                filename = "LAPTOP\\ConfirmOrder.sti";
                return true;
            case MLG.DESKTOP:
                filename = "LAPTOP\\desktop.pcx";
                return true;
            case MLG.FUNERALAD9:
                filename = "LAPTOP\\FuneralAd_9.sti";
                return true;
            case MLG.GOLDPIECEBUTTONS:
                filename = "INTERFACE\\goldpiecebuttons.sti";
                return true;
            case MLG.HISTORY:
                filename = "LAPTOP\\history.sti";
                return true;
            case MLG.INSURANCEAD10:
                filename = "LAPTOP\\insurancead_10.sti";
                return true;
            case MLG.INSURANCELINK:
                filename = "LAPTOP\\insurancelink.sti";
                return true;
            case MLG.INSURANCETITLE:
                filename = "LAPTOP\\largetitle.sti";
                return true;
            case MLG.LARGEFLORISTSYMBOL:
                filename = "LAPTOP\\LargeSymbol.sti";
                return true;
            case MLG.SMALLFLORISTSYMBOL:
                filename = "LAPTOP\\SmallSymbol.sti";
                return true;
            case MLG.MCGILLICUTTYS:
                filename = "LAPTOP\\McGillicuttys.sti";
                return true;
            case MLG.MORTUARY:
                filename = "LAPTOP\\Mortuary.sti";
                return true;
            case MLG.MORTUARYLINK:
                filename = "LAPTOP\\MortuaryLink.sti";
                return true;
            case MLG.ORDERGRID:
                filename = "LAPTOP\\OrderGrid.sti";
                return true;
            case MLG.PREBATTLEPANEL:
                filename = "INTERFACE\\PreBattlePanel.sti";
                return true;
            case MLG.SMALLTITLE:
                filename = "LAPTOP\\SmallTitle.sti";
                return true;
            case MLG.STATSBOX:
                filename = "LAPTOP\\StatsBox.sti";
                return true;
            case MLG.STOREPLAQUE:
                filename = "LAPTOP\\BobbyStorePlaque.sti";
                return true;
            case MLG.TITLETEXT:
                filename = "LOADSCREENS\\titletext.sti";
                return true;
            case MLG.TOALUMNI:
                filename = "LAPTOP\\ToAlumni.sti";
                return true;
            case MLG.TOMUGSHOTS:
                filename = "LAPTOP\\ToMugShots.sti";
                return true;
            case MLG.TOSTATS:
                filename = "LAPTOP\\ToStats.sti";
                return true;
            case MLG.WARNING:
                filename = "LAPTOP\\Warning.sti";
                return true;
            case MLG.YOURAD13:
                filename = "LAPTOP\\YourAd_13.sti";
                return true;
            case MLG.OPTIONHEADER:
                filename = "INTERFACE\\optionscreenaddons.sti";
                return true;
            case MLG.LOADSAVEHEADER:
                filename = "INTERFACE\\loadscreenaddons.sti";
                return true;
            case MLG.SPLASH:
                filename = "INTERFACE\\splash.sti";
                return true;
            case MLG.IMPSYMBOL:
                filename = "LAPTOP\\IMPSymbol.sti";
                return true;

            default:
                filename = string.Empty;
                return false;
        }
    }
}

public enum MLG
{
    AIMSYMBOL,
    BOBBYNAME,
    BOBBYRAYAD21,
    BOBBYRAYLINK,
    CLOSED,
    CONFIRMORDER,
    DESKTOP,
    FUNERALAD9,
    GOLDPIECEBUTTONS,
    HISTORY,
    IMPSYMBOL,
    INSURANCEAD10,
    INSURANCELINK,
    INSURANCETITLE, //LargeTitle
    LARGEFLORISTSYMBOL, //LargeSymbol
    LOADSAVEHEADER, //LoadScreenAddOns
    MCGILLICUTTYS,
    MORTUARY,
    MORTUARYLINK,
    OPTIONHEADER, //OptionScreenAddOns
    ORDERGRID,
    PREBATTLEPANEL,
    SECTORINVENTORY,
    SMALLFLORISTSYMBOL, //SmallSymbol
    SMALLTITLE,
    SPLASH,
    STATSBOX,
    STOREPLAQUE,
    TITLETEXT,
    TOALUMNI,
    TOMUGSHOTS,
    TOSTATS,
    WARNING,
    YOURAD13,
};

