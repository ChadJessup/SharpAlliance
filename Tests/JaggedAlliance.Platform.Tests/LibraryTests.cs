using System;
using Xunit;

namespace JaggedAlliance.Platform.Tests
{
    public class LibraryTests
    {
        [Fact]
        public void LoadsLibraries()
        {
            const string dataDir = @"C:\JaggedAllianceDotnet\Data";

            var library = new Library(dataDir);
            Assert.True(library.InitializeFileDatabase());

            Assert.True(library.IsInitialized);
            Assert.All(library.Libraries.Values, l => Assert.True(l.fLibraryOpen));
        }
    }
}
