using System;
using System;
using System.Runtime.InteropServices;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.Desktop;
using OpenToolkit.Windowing.GraphicsLibraryFramework;
using SkiaSharp;

namespace SkaderToy
{
    public abstract class SampleBase : GameWindow
    {
        protected GRContext GrContext { get; private set; }
        private SKSurface _surface;
        protected SKCanvas Canvas { get; private set; }

        protected override void OnLoad()
        {
            var iface = GRGlInterface.AssembleGlInterface((_, proc) =>
            {
                if (proc.StartsWith("egl"))
                    return IntPtr.Zero;
                if (proc == "glShaderSource")
                {
                    return new ShaderProxy(GLFW.GetProcAddress(proc.ToString())).Pointer;
                }
                return GLFW.GetProcAddress(proc.ToString());
            });
            GrContext = GRContext.CreateGl(iface);
            base.OnLoad();
        }

        class ShaderProxy
        {
            private GlShaderSourceDelegate _original;

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            delegate void GlShaderSourceDelegate(int shader, int count, IntPtr strings, IntPtr lengths);
            public ShaderProxy(IntPtr original)
            {
                _original = Marshal.GetDelegateForFunctionPointer<GlShaderSourceDelegate>(original);
                GlShaderSourceDelegate del = Proxy;
                GCHandle.Alloc(del);
                Pointer = Marshal.GetFunctionPointerForDelegate(del);
            }

            public IntPtr Pointer { get; }

            unsafe void Proxy(int shader, int count, IntPtr strings, IntPtr lengths)
            {
                var pstrings = (IntPtr*)strings.ToPointer();
                var plens = (int*) lengths.ToPointer();
                
                
                for (int c = 0; c < count; c++)
                {
                    var sptr = pstrings[c];
                    var len = plens[c];
                    var s = Marshal.PtrToStringAnsi(sptr, len);
                    Console.WriteLine("Compiled shader dump");
                    Console.WriteLine(s);
                }
                _original(shader, count, strings, lengths);
            }
        }
        
        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            _surface?.Dispose();
            _surface = null;
            var fbo = GL.GetInteger(GetPName.FramebufferBinding);
            var stencilDepth = GL.GetInteger((GetPName) 0x0D57);
            var samples = GL.GetInteger((GetPName) 0x80A9);
            var target = new GRBackendRenderTarget(e.Width, e.Height, 
                samples, stencilDepth,  new GRGlFramebufferInfo((uint) fbo, GRPixelConfig.Rgba8888.ToGlSizedFormat()));
            _surface = SKSurface.Create(GrContext, target, GRSurfaceOrigin.TopLeft,
                GRPixelConfig.Rgba8888.ToColorType());
            Canvas = _surface.Canvas;
            base.OnResize(e);
        }

        protected override void OnUnload()
        {
            _surface?.Dispose();
            _surface = null;
            Canvas = null;
            GrContext?.Dispose();
            GrContext = null;
            base.OnUnload();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // Clear the color buffer.
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.ClearStencil(0);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GrContext.ResetContext();
            
            
            Canvas.Clear(SKColors.Transparent);
            SkiaRender();
            Canvas.Flush();
            this.SwapBuffers();

            base.OnRenderFrame(e);
        }

        protected abstract void SkiaRender();

        
        public SampleBase() : base(new GameWindowSettings(), new NativeWindowSettings())
        {
        }
    }
}