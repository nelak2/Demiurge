using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Windows;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var form = new RenderForm("Test");

            RenderLoop.Run(form, () =>
                {
                    

                });
        }
    }
}
