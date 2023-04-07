using System.Runtime.InteropServices;
using SharpAlliance.Core.SubSystems;

namespace SharpAlliance.Core;

public partial class Globals
{
    public const uint FAST_TURN_ANIM_SPEED = 30;

    public const int TOTALBODYTYPES = (int)SoldierBodyTypes.TOTALBODYTYPES;
    public const int NUMANIMATIONSTATES = (int)AnimationStates.NUMANIMATIONSTATES;

    public int[,] gubAnimSurfaceIndex = new int[TOTALBODYTYPES, NUMANIMATIONSTATES];
    public int[,] gubAnimSurfaceMidWaterSubIndex = new int[TOTALBODYTYPES, NUMANIMATIONSTATES];
    public int[,] gubAnimSurfaceItemSubIndex = new int[TOTALBODYTYPES, NUMANIMATIONSTATES];
    public int[,] gubAnimSurfaceCorpseID = new int[TOTALBODYTYPES, NUMANIMATIONSTATES];

    public static ANIMSUBTYPE[] gRifleInjuredSub =
    {
        new()
        {
            usAnimState = AnimationStates.WALKING,
            usAnimationSurfaces = new []
            {
                AnimationSurfaceTypes.RGMHURTWALKINGR,
                AnimationSurfaceTypes.BGMHURTWALKINGR,
                AnimationSurfaceTypes.RGMHURTWALKINGR,
                AnimationSurfaceTypes.RGFHURTWALKINGR,
            }
        }
    };

    public static ANIMSUBTYPE[] gNothingInjuredSub =
    {
        new()
        {
            usAnimState = AnimationStates.WALKING,
            usAnimationSurfaces = new []
            {
                AnimationSurfaceTypes.RGMHURTWALKINGN,
                AnimationSurfaceTypes.BGMHURTWALKINGN,
                AnimationSurfaceTypes.RGMHURTWALKINGN,
                AnimationSurfaceTypes.RGFHURTWALKINGN,
            }
        }
    };

    public static ANIMSUBTYPE[] gDoubleHandledSub =
    {
        new()
        {
            usAnimState = AnimationStates.STANDING,
            usAnimationSurfaces = new[]
            {
                AnimationSurfaceTypes.RGMDBLBREATH,
                AnimationSurfaceTypes.BGMDBLBREATH,
                AnimationSurfaceTypes.RGMDBLBREATH,
                AnimationSurfaceTypes.RGFDBLBREATH,
            }
        }
    };
}

[StructLayout(LayoutKind.Explicit, Size = 4)]
public struct ANIMSUBTYPE
{
    [FieldOffset(00)] public AnimationStates usAnimState;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    [FieldOffset(02)] public AnimationSurfaceTypes[] usAnimationSurfaces;// [4];
}
