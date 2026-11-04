using System.Linq;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Core.SubSystems.LaptopSubSystem;
using Xunit;
using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Tests;

[Collection("World")]
public class EmailPageTests
{
    [Fact]
    public void MessagesFillPagesInOrderAndPagesGetIds()
    {
        Emails.ClearPages();

        // one more than fits on a page: used to recurse forever because the first (full) page was always checked
        for (var i = 0; i < MAX_MESSAGES_PAGE + 1; i++)
        {
            Emails.AddMessageToPages(i);
        }

        Assert.Equal(2, pPageList.Count);
        Assert.Equal(new[] { 0, 1 }, pPageList.Select(p => p.iPageId));
        Assert.Equal(MAX_MESSAGES_PAGE, pPageList[0].iIds.Count(id => id != -1));
        Assert.Equal(MAX_MESSAGES_PAGE, pPageList[1].iIds[0]);
        Assert.Equal(-1, pPageList[1].iIds[1]);
    }
}
