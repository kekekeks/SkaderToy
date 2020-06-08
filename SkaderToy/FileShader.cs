using System;
using System.Diagnostics;
using System.IO;
using OpenToolkit.Windowing.Common;
using SkiaSharp;

namespace SkaderToy
{
    public class FileShader : SampleBase
    {
        private string _shaderText;
        private SKRuntimeEffect _runtimeEffect;

        public FileShader(string path)
        {
            _shaderText = File.ReadAllText(path);
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            _runtimeEffect = SKRuntimeEffect.Create(_shaderText, out var errors);
            if (_runtimeEffect == null)
            {
                Console.Error.WriteLine(errors);
                Environment.Exit(1);
            }
        }
        
        Stopwatch _st = Stopwatch.StartNew();
        protected override void SkiaRender()
        {
            var inputs = new SKRuntimeEffectInputs(_runtimeEffect);
            if(inputs.Contains("iTime"))
                inputs.Set("iTime", (float)_st.Elapsed.TotalSeconds*5);
            if(inputs.Contains("iResolution"))
                inputs.Set("iResolution", new float[]
                {
                    Size.X,
                    Size.Y,
                    0
                });
            
            using(var shader = _runtimeEffect.ToShader(inputs, false))
            using (var paint = new SKPaint
            {
                Shader = shader
            })
            {
                Canvas.Save();
                
                //Canvas.Translate(MousePosition.X, Size.Y - MousePosition.Y);
                //Canvas.DrawCircle(0, 0, 50, paint);
                //Canvas.DrawCircle(MousePosition.X,Size.Y - MousePosition.Y, 50, paint);
                //Canvas.Restore();
                Canvas.DrawRect(0, 0, Size.X, Size.Y, paint);
                /*
                Canvas.DrawRect(0, 0, Size.X/3, Size.Y, paint);
                Canvas.DrawRect(Size.X/3*2, 0, Size.X/3, Size.Y, paint);*/
            }
        }
    }
}