﻿using System.Threading.Tasks;
using SharpAlliance.Platform.Interfaces;

namespace SharpAlliance.Platform.NullManagers
{
    public class NullVideoManager : IVideoManager
    {
        public bool IsInitialized { get; } = true;

        public void Dispose()
        {
        }

        public ValueTask<bool> Initialize()
        {
            return ValueTask.FromResult(true);
        }
    }
}