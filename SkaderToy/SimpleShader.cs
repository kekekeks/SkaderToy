using SkiaSharp;

namespace SkaderToy
{
    public class SimpleShader : SampleBase
    {
        protected override void SkiaRender()
        {
            Canvas.Clear(SKColors.Chartreuse);
        }
    }
}