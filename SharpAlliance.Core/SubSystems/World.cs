﻿using System;
using System.Threading.Tasks;

namespace SharpAlliance.Core.SubSystems
{
    public class World
    {
        public void TrashWorld()
        {
        }

        public ValueTask<bool> InitializeWorld()
        {
            return ValueTask.FromResult(true);
        }
    }
}
