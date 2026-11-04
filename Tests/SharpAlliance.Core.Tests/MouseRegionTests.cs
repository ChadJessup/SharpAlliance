using SharpAlliance.Core;
using SharpAlliance.Core.SubSystems;
using SixLabors.ImageSharp;
using Xunit;

namespace SharpAlliance.Core.Tests;

public class MouseRegionTests
{
    [Fact]
    public void CornerOverloadMatchesTheOriginalCSignature()
    {
        // MSYS_DefineRegion( &region, 10, 20, 110, 70, ... ) in the C code: a 100 x 50 region at (10, 20)
        var region = new MOUSE_REGION("corners");

        MouseSubSystem.MSYS_DefineRegion(region, 10, 20, 110, 70, MSYS_PRIORITY.HIGH, CURSOR.WWW, null, null);

        Assert.Equal(new Rectangle(10, 20, 100, 50), region.Bounds);
        Assert.True(region.Bounds.Contains(10, 20));
        Assert.True(region.Bounds.Contains(109, 69));
        Assert.False(region.Bounds.Contains(110, 70));      // right/bottom are exclusive, as in C
        MouseSubSystem.MSYS_RemoveRegion(region);
    }

    [Fact]
    public void RectangleOverloadStillTakesPositionAndSize()
    {
        var region = new MOUSE_REGION("rect");

        MouseSubSystem.MSYS_DefineRegion(region, new Rectangle(10, 20, 100, 50), MSYS_PRIORITY.HIGH, CURSOR.WWW, null, null);

        Assert.Equal(new Rectangle(10, 20, 100, 50), region.Bounds);
        MouseSubSystem.MSYS_RemoveRegion(region);
    }
}
