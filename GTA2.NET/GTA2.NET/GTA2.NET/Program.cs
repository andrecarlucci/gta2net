using System;

namespace Hiale.GTA2NET
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            //string[] files = System.IO.Directory.GetFiles("textures\\tiles");
           // Hiale.GTA2NET.NET.Helper.ImageCreator.CreateImageDictionary(files, 64, 64);

            //Hiale.GTA2NET.Core.Style.Style style = new Hiale.GTA2NET.Core.Style.Style();
            //Hiale.GTA2NET.Core.Map.Map map = new Hiale.GTA2NET.Core.Map.Map();

            using (MainGame game = new MainGame())
            {
                game.Run();
            }            
        }
    }
}

