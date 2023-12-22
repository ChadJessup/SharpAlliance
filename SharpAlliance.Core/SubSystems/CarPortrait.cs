using System;
using SharpAlliance.Core.Interfaces;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core;

public partial class Globals
{
    // the ids for the car portraits
    public static string[] giCarPortraits = { string.Empty, string.Empty, string.Empty, string.Empty };

    // the car portrait file names
    public static string[] pbCarPortraitFileNames =
    {
        "INTERFACE\\eldorado.sti",
        "INTERFACE\\Hummer.sti",
        "INTERFACE\\ice Cream Truck.sti",
        "INTERFACE\\Jeep.sti",
    };
}

public class Cars
{
    private readonly IVideoManager video;

    public Cars(IVideoManager videoManager)
    {
        this.video = videoManager;
    }

    public bool LoadCarPortraitValues()
    {
        if (giCarPortraits[0] != string.Empty)
        {
            return false;
        }

        for (CarPortrait iCounter = 0; iCounter < CarPortrait.NUMBER_CAR_PORTRAITS; iCounter++)
        {
            this.video.GetVideoObject(pbCarPortraitFileNames[(int)iCounter], out var key);
            giCarPortraits[(int)iCounter] = key;
        }

        return true;
    }
}

public enum CarPortrait
{
    ELDORADO_PORTRAIT = 0,
    HUMMER_PORTRAIT,
    ICE_CREAM_TRUCT_PORTRAIT,
    JEEP_PORTRAIT,
    NUMBER_CAR_PORTRAITS,
};
