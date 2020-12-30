using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using SharpAlliance.Platform;
using Xunit;

namespace SharpAlliance.Core.Tests
{
    public class LibraryTests
    {
        [Fact]
        public void LoadsLibraries()
        {
            const string dataDir = @"C:\SharpAlliance\Data";
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "DataDirectory", dataDir },
                })
                .Build();

            var context = new GameContext(null, null, config);

            // TODO: Moqs for logger across all tests.
            var library = new LibraryFileManager(null, context);
            Assert.True(library.Initialize());

            Assert.True(library.IsInitialized);
            Assert.All(library.Libraries.Values, l => Assert.True(l.fLibraryOpen));
        }
    }
}
