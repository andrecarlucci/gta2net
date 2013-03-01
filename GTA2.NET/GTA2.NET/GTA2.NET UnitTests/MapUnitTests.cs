using System.Drawing;
using System.Drawing.Imaging;
using Hiale.GTA2NET.Core.Collision;
using Hiale.GTA2NET.Core.Map;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace Hiale.GTA2NET.Test
{
    [TestClass]
    public class MapUnitTests
    {
        [TestMethod]
        public void LoadMapTinyTown()
        {
            Map map = new Map();
            System.Diagnostics.Debug.WriteLine(System.Environment.CurrentDirectory);
            map.ReadFromFile("C:\\Users\\Alexander\\Documents\\Visual Studio 2012\\Projects\\GTA2.NET\\GTA2.NET\\bin\\Debug\\data\\MP1-comp.gmp");
            var collision = new MapCollision(map);
            var blocks = collision.FloodFill(new Vector2(73,192));
            DisplayCollision(blocks);
            Assert.AreEqual(true, true);
        }

        [TestMethod]
        public void LoadMapBil()
        {
            Map map = new Map();
            System.Diagnostics.Debug.WriteLine(System.Environment.CurrentDirectory);
            map.ReadFromFile("C:\\Users\\Alexander\\Documents\\Visual Studio 2012\\Projects\\GTA2.NET\\GTA2.NET\\bin\\Debug\\data\\bil.gmp");
            var collision = new MapCollision(map);
            var blocks = collision.FloodFill(new Vector2(239, 192));
            DisplayCollision(blocks);
            Assert.AreEqual(true, true);
        }

        private void DisplayCollision(CollisionMapType[,,] blocks)
        {
            for (var z = 0; z < blocks.GetLength(2); z++)
            {
                using (var bmp = new Bitmap(2560, 2560))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.Clear(System.Drawing.Color.White);
                        for (var x = 0; x < blocks.GetLength(0); x++)
                        {
                            for (var y = 0; y < blocks.GetLength(1); y++)
                            {
                                if (DebugThis(x, y))
                                {
                                    g.FillRectangle(new SolidBrush(System.Drawing.Color.Turquoise), x * 10, y * 10, 10, 10);
                                    continue;
                                }
                                if (blocks[x, y, z] == CollisionMapType.Block)
                                    g.FillRectangle(new SolidBrush(System.Drawing.Color.Red), x*10, y*10, 10, 10);
                                if (blocks[x, y, z] == CollisionMapType.Free)
                                    g.FillRectangle(new SolidBrush(System.Drawing.Color.Green), x*10, y*10, 10, 10);
                                if (blocks[x, y, z] == CollisionMapType.Special)
                                    g.FillRectangle(new SolidBrush(System.Drawing.Color.Blue), x*10, y*10, 10, 10);
                                if (blocks[x, y, z] == CollisionMapType.None)
                                    g.FillRectangle(new SolidBrush(System.Drawing.Color.Yellow), x*10, y*10, 10, 10);
                                if (blocks[x, y, z] == CollisionMapType.Unknwon)
                                    g.FillRectangle(new SolidBrush(System.Drawing.Color.Violet), x*10, y*10, 10, 10);
                            }
                        }
                        bmp.Save("G:\\GTA2 Test\\" + z + ".png", ImageFormat.Png);
                    }
                }
            }
        }

        private bool DebugThis(int x, int y)
        {
            //if (x == 121 && y == 183)
            //{
            //    System.Diagnostics.Debug.WriteLine("OK");
            //    return true;
            //}
            return false;
        }
    }
}
