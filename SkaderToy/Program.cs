using System;

namespace SkaderToy
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            var program = new FileShader("Warping.sksl");
            program.Run();
        }
    }
}