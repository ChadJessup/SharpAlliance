using System;
using Xunit;

namespace SharpAlliance.Platform.Tests
{
    public class LibraryTests
    {
        [Fact]
        public void LoadsLibraries()
        {
            const string dataDir = @"G:\Projects\SharpAlliance\Data";

            var library = new Library(dataDir);
            Assert.True(library.InitializeFileDatabase());

            Assert.True(library.IsInitialized);
            Assert.All(library.Libraries.Values, l => Assert.True(l.fLibraryOpen));
        }
    }
}
