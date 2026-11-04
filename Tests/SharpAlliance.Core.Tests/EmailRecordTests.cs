using System.Collections.Generic;
using SharpAlliance.Core.SubSystems;
using SharpAlliance.Core.SubSystems.LaptopSubSystem;
using Xunit;

namespace SharpAlliance.Core.Tests;

[Collection("World")]
public class EmailRecordTests
{
    private static List<string?> Records()
    {
        var list = new List<string?>();
        for (var r = Emails.pMessageRecordList; r is not null; r = r.Next)
        {
            list.Add(r.pRecord);
        }

        return list;
    }

    [Fact]
    public void RecordsAreAppendedInOrder()
    {
        Emails.ClearOutEmailMessageRecordsList();
        Assert.Null(Emails.pMessageRecordList);

        Emails.AddEmailRecordToList("first");
        Emails.AddEmailRecordToList("second");
        Emails.AddEmailRecordToList("third");

        Assert.Equal(new List<string?> { "first", "second", "third" }, Records());
        Assert.Equal("first", Emails.pMessageRecordList!.pRecord);
        Assert.Null(Emails.pMessageRecordList.Next!.Next!.Next);       // list is terminated

        Emails.ClearOutEmailMessageRecordsList();
    }

    [Fact]
    public void ClearingTheListEmptiesItAndAllowsStartingAgain()
    {
        Emails.AddEmailRecordToList("old");
        Emails.ClearOutEmailMessageRecordsList();
        Assert.Empty(Records());

        Emails.AddEmailRecordToList("new");
        Assert.Equal(new List<string?> { "new" }, Records());

        Emails.ClearOutEmailMessageRecordsList();
    }
}
