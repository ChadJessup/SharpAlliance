using SharpAlliance.Platform.Interfaces;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX.Windows;
using d2 = SharpDX.Direct2D1;
using d3d = SharpDX.Direct3D11;
using dw = SharpDX.DirectWrite;
using dxgi = SharpDX.DXGI;
using wic = SharpDX.WIC;

namespace SharpAlliance
{
    public class DirectDrawVideoManager : IVideoManager
    {
//        private D2DFactory d2dFactory;
//        private DWriteFactory dwFactory;
        private RenderForm mainForm;
        private WindowRenderTarget renderTarget;

        public bool Initialize()
        {
            this.mainForm = new RenderForm("Sharp Alliance!")
            {
                Width = 640,
                Height = 480,
                 
            };

//            d2dFactory = new D2DFactory();
//            dwFactory = new DWriteFactory(SharpDX.DirectWrite.FactoryType.Shared);
            var defaultDevice = new d3d.Device(
                SharpDX.Direct3D.DriverType.Hardware,
                d3d.DeviceCreationFlags.VideoSupport
                | d3d.DeviceCreationFlags.BgraSupport
                | d3d.DeviceCreationFlags.None);

            var d3dDevice = defaultDevice.QueryInterface<d3d.Device1>(); // get a reference to the Direct3D 11.1 device
            var dxgiDevice = d3dDevice.QueryInterface<dxgi.Device>(); // get a reference to DXGI device

            var d2dDevice = new d2.Device(dxgiDevice); // initialize the D2D device

            var imagingFactory = new wic.ImagingFactory2(); // initialize the WIC factory

            // initialize the DeviceContext - it will be the D2D render target and will allow all rendering operations
            var d2dContext = new d2.DeviceContext(d2dDevice, d2.DeviceContextOptions.None);
            var sd = new SurfaceDescription()
            {
                   
            };

            var s1 = new SharpDX.DXGI.Surface1(d3dDevice.NativePointer);
            var s2 = new SharpDX.DXGI.Surface2(d3dDevice.NativePointer);
            
            var dwFactory = new dw.Factory();

            // specify a pixel format that is supported by both D2D and WIC
            var d2PixelFormat = new d2.PixelFormat(dxgi.Format.R8G8B8A8_UNorm, d2.AlphaMode.Premultiplied);

            // if in D2D was specified an R-G-B-A format - use the same for wic
            var wicPixelFormat = wic.PixelFormat.Format32bppPRGBA;

            HwndRenderTargetProperties wtp = new()
            {
                Hwnd = mainForm.Handle,
                PixelSize = new Size2(mainForm.ClientSize.Width, mainForm.ClientSize.Height),
                PresentOptions = PresentOptions.Immediately,
            };

            var rtp = new RenderTargetProperties(RenderTargetType.Hardware, d2PixelFormat, 0, 0, RenderTargetUsage.GdiCompatible, FeatureLevel.Level_DEFAULT);
            renderTarget = new WindowRenderTarget(d2dContext.Factory, new RenderTargetProperties(), wtp);

            RenderLoop.Run(mainForm, () =>
            {
                renderTarget.BeginDraw();

                try
                {
                    renderTarget.EndDraw();
                }
                catch
                {
                }
            });

            d2dContext.Dispose();
            dwFactory.Dispose();
            renderTarget.Dispose();

            return true;
        }

        public void Dispose()
        {
        }
    }
}
