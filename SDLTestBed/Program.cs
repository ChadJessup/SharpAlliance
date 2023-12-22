using SDL2;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;

// Initilizes SDL.

var cur = Environment.CurrentDirectory;

if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
{
    Console.WriteLine($"There was an issue initilizing SDL. {SDL.SDL_GetError()}");
}

// Create a new window given a title, size, and passes it a flag indicating it should be shown.
var window = SDL.SDL_CreateWindow(
    "SDL & ImageSharp",
    SDL.SDL_WINDOWPOS_UNDEFINED,
    SDL.SDL_WINDOWPOS_UNDEFINED,
    640,
    480,
    SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);

if (window == IntPtr.Zero)
{
    Console.WriteLine($"There was an issue creating the window. {SDL.SDL_GetError()}");
}

// Creates a new SDL hardware renderer using the default graphics device with VSYNC enabled.
var renderer = SDL.SDL_CreateRenderer(
    window,
    -1,
    SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
    SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

if (renderer == IntPtr.Zero)
{
    Console.WriteLine($"There was an issue creating the renderer. {SDL.SDL_GetError()}");
}

// Initilizes SDL_image for use with png files.
//if (SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG) == 0)
//{
//    Console.WriteLine($"There was an issue initilizing SDL2_Image {SDL_image.IMG_GetError()}");
//}

var il = new ImageLoader();

var running = true;
nint texturePtr = il.Process(renderer);

// Main loop for the program
while (running)
{
    // Check to see if there are any events and continue to do so until the queue is empty.
    while (SDL.SDL_PollEvent(out SDL.SDL_Event e) == 1)
    {
        switch (e.type)
        {
            case SDL.SDL_EventType.SDL_QUIT:
                running = false;
                break;
        }
    }

    // Sets the color that the screen will be cleared with.
    if (SDL.SDL_SetRenderDrawColor(renderer, 135, 206, 235, 255) < 0)
    {
        Console.WriteLine($"There was an issue with setting the render draw color. {SDL.SDL_GetError()}");
    }

    // Clears the current render surface.
    if (SDL.SDL_RenderClear(renderer) < 0)
    {
        Console.WriteLine($"There was an issue with clearing the render surface. {SDL.SDL_GetError()}");
    }

    var image = il.UpdateTexture();
    //    var surf = ImageLoader.AsSdlSurface(il.surfacePtr);
    unsafe
    {
            if (SDL.SDL_UpdateTexture(texturePtr, 0, (nint)il.pinHandle.Pointer, 4 * image.Width) < 0)
            {
                var error = SDL.SDL_GetError();
            }
    }
    SDL.SDL_RenderCopy(renderer, texturePtr, 0, 0);
    SDL.SDL_RenderPresent(renderer);
}

// Clean up the resources that were created.
SDL.SDL_DestroyRenderer(renderer);
SDL.SDL_DestroyWindow(window);
SDL.SDL_Quit();


internal class ImageLoader
{
    public MemoryHandle pinHandle { get; set; }
    public nint surfacePtr { get; set; }
    private Image<Rgba32> image;
    private nint texturePtr;

    public static unsafe ref SDL.SDL_Surface AsSdlSurface(IntPtr ptr) => ref Unsafe.AsRef<SDL.SDL_Surface>((void*)ptr);

    public unsafe nint Process(nint renderer)
    {
        if (this.texturePtr != IntPtr.Zero)
        {
            return this.texturePtr;
        }

        Configuration customConfig = Configuration.Default.Clone();
        customConfig.PreferContiguousImageBuffers = true;

        var doptions = new DecoderOptions
        {
            Configuration = customConfig,
        };

        this.image = (Image<Rgba32>)Image.Load(doptions, @"C:\Users\chadj\OneDrive\Pictures\100votes-comeon!.PNG");

        if (!this.image.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> memory))
        {
            throw new Exception(
                "This can only happen with multi-GB images or when PreferContiguousImageBuffers is not set to true.");
        }

        this.pinHandle = memory.Pin();
        this.texturePtr = SDL.SDL_CreateTexture(
            renderer,
            SDL.SDL_PIXELFORMAT_ABGR8888,
            (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET,
            640,
            480);



        //surfacePtr = SDL.SDL_CreateRGBSurfaceFrom(
        //    (nint)pinHandle.Pointer,
        //    image.Width,
        //    image.Height,
        //    depth: 32,
        //    pitch: 4 * image.Width, // unsure of this, I'd expect at least gibberish if wrong.
        //    Rmask: 0x000000FF,
        //    Gmask: 0x0000FF00,
        //    Bmask: 0x00FF0000,
        //    Amask: 0xFF000000);
        //
        //if (surfacePtr == IntPtr.Zero)
        //{
        //    string error = SDL.SDL_GetError();
        //    Console.WriteLine(error);
        //}

       // texturePtr = SDL.SDL_CreateTextureFromSurface(renderer, surfacePtr);

        if (this.texturePtr == IntPtr.Zero)
        {
            string error = SDL.SDL_GetError();
            Console.WriteLine(error);
        }

        return this.texturePtr;
    }

    private int degrees = 0;
    public Image<Rgba32> UpdateTexture()
    {
        this.degrees += 5;
//        image.Mutate(ctx =>
//        {
//            ctx.Rotate(degrees);
//        });

  //      if(degrees % 5 == 0)
  //      {
  //          image.SaveAsPng(@"C:\temp\test.png");
  //      }

        return this.image;
    }
}
