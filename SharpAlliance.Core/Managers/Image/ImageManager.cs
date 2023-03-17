using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpAlliance.Core.Interfaces;
using SharpAlliance.Platform;

using static SharpAlliance.Core.Globals;

namespace SharpAlliance.Core.Managers;

public class ImageManager : IImageManager
{
    private readonly ILogger<ImageManager> logger;

    public ImageManager(ILogger<ImageManager> logger)
    {
        this.logger = logger;


        this.IsInitialized = true;
    }

    public bool IsInitialized { get; }

    public ValueTask<bool> Initialize()
    {
        return ValueTask.FromResult(true);
    }

    public void Dispose()
    {
    }
}
