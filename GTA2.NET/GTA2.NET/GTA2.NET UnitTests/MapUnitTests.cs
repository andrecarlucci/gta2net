using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Hiale.GTA2NET.Core.Collision;
using Hiale.GTA2NET.Core.Helper;
using Hiale.GTA2NET.Core.Map;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace Hiale.GTA2NET.Test
{
    [TestClass]
    public class MapUnitTests
    {
        [TestInitialize]
        public void InitializeTests()
        {
            System.Environment.CurrentDirectory = "..\\..\\..\\GTA2.NET\\bin\\Debug\\";
        }

        [TestMethod]
        public void LoadMapTinyTown()
        {
            var map = new Map("data\\MP1-comp.gmp");
            var collision = new MapCollision(map);
            var obstacles = collision.CollisionMap(new Vector2(73,192));
            DisplayCollision(obstacles);
            Assert.AreEqual(true, true);
        }

        [TestMethod]
        public void LoadMapBil()
        {
            var map = new Map("data\\bil.gmp");
            var collision = new MapCollision(map);
            var obstacles = collision.CollisionMap(new Vector2(239, 192));
            DisplayCollision(obstacles);
            Assert.AreEqual(true, true);
        }

        [TestMethod]
        public void SaveMapTinyTown()
        {
            var map = new Map("data\\MP1-comp.gmp");
            map.Save("data\\TinyTown.gta2map");
            Assert.AreEqual(true, true);
        }

        [TestMethod]
        public void LoadMapTinyTownInternalFormat()
        {
            SaveMapTinyTown();
            var map = new Map();
            map.Load("data\\TinyTown.gta2map");
            Assert.AreEqual(true, true);
        }

        private void DisplayCollision(IEnumerable<IObstacle> obstacles)
        {
            var layers = new Dictionary<int, Bitmap>();
            foreach (var obstacle in obstacles)
            {
                //int currentLayer = 0;
                Bitmap bmp;
                if (layers.ContainsKey(obstacle.Z))
                {
                    //currentLayer = obstacle.Z;
                    bmp = layers[obstacle.Z];
                }
                else
                {
                    bmp = new Bitmap(2560, 2560);
                    layers.Add(obstacle.Z, bmp);
                    //currentLayer = layers.Count - 1;
                }

                 using (var g = Graphics.FromImage(bmp))
                 {
                     if (obstacle is RectangleObstacle)
                     {
                         var rectObstacle = (RectangleObstacle)obstacle;
                         g.FillRectangle(new SolidBrush(System.Drawing.Color.Red), rectObstacle.Position.X * 10, rectObstacle.Position.Y * 10, 10, 10);
                     }
                     else if (obstacle is LineObstacle)
                     {
                         var lineObstacle = (LineObstacle)obstacle;
                         g.DrawLine(new Pen(System.Drawing.Color.Magenta), new System.Drawing.Point((int)lineObstacle.Start.X * 10, (int)lineObstacle.Start.Y * 10), new System.Drawing.Point((int)lineObstacle.End.X * 10, (int)lineObstacle.End.Y * 10));
                     }
                     else if (obstacle is SlopeObstacle)
                     {
                         var slopeObstacle = (SlopeObstacle)obstacle;
                         g.FillRectangle(new SolidBrush(System.Drawing.Color.Blue), slopeObstacle.Position.X * 10, slopeObstacle.Position.Y * 10, 10, 10);
                     }
                     else if (obstacle is PolygonObstacle)
                     {
                         var polygonObstacle = (PolygonObstacle)obstacle;
                         var points = new System.Drawing.Point[polygonObstacle.Vertices.Count];
                         for (var i = 0; i < polygonObstacle.Vertices.Count; i++)
                             points[i] = new System.Drawing.Point((int) polygonObstacle.Vertices[i].X * 10, (int) polygonObstacle.Vertices[i].Y * 10);
                         g.FillPolygon(new SolidBrush(System.Drawing.Color.OrangeRed), points);
                     }

                 }
            }

            foreach (var pair in layers)
            {
                pair.Value.Save(pair.Key + ".png", ImageFormat.Png);
                pair.Value.Dispose();
            }

            //for (var z = 0; z < blocks.GetLength(2); z++)
            //{
            //    using (var bmp = new Bitmap(2560, 2560))
            //    {
            //        using (var g = Graphics.FromImage(bmp))
            //        {
            //            g.Clear(System.Drawing.Color.White);
            //            for (var x = 0; x < blocks.GetLength(0); x++)
            //            {
            //                for (var y = 0; y < blocks.GetLength(1); y++)
            //                {
            //                    if (DebugThis(x, y))
            //                    {
            //                        g.FillRectangle(new SolidBrush(System.Drawing.Color.Turquoise), x * 10, y * 10, 10, 10);
            //                        continue;
            //                    }
            //                    if (blocks[x, y, z] == CollisionMapType.Block)
            //                        g.FillRectangle(new SolidBrush(System.Drawing.Color.Red), x * 10, y * 10, 10, 10);
            //                    if (blocks[x, y, z] == CollisionMapType.Free)
            //                        g.FillRectangle(new SolidBrush(System.Drawing.Color.Green), x * 10, y * 10, 10, 10);
            //                    if (blocks[x, y, z] == CollisionMapType.Special)
            //                        g.FillRectangle(new SolidBrush(System.Drawing.Color.Blue), x * 10, y * 10, 10, 10);
            //                    if (blocks[x, y, z] == CollisionMapType.None)
            //                        g.FillRectangle(new SolidBrush(System.Drawing.Color.Yellow), x * 10, y * 10, 10, 10);
            //                    if (blocks[x, y, z] == CollisionMapType.Unknwon)
            //                        g.FillRectangle(new SolidBrush(System.Drawing.Color.Violet), x * 10, y * 10, 10, 10);
            //                    g.DrawRectangle(new Pen(System.Drawing.Color.LightGray), x * 10, y * 10, 10, 10);
            //                }
            //            }
            //            bmp.Save("G:\\GTA2 Test\\" + z + ".png", ImageFormat.Png);
            //        }
            //    }
            //}
        }

        private void DisplayCollision2(CollisionMapType[,,] blocks)
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
                                if (blocks[x, y, z] == CollisionMapType.Slope)
                                    g.FillRectangle(new SolidBrush(System.Drawing.Color.Blue), x*10, y*10, 10, 10);
                                if (blocks[x, y, z] == CollisionMapType.None)
                                    g.FillRectangle(new SolidBrush(System.Drawing.Color.Yellow), x*10, y*10, 10, 10);
                                if (blocks[x, y, z] == CollisionMapType.Unknwon)
                                    g.FillRectangle(new SolidBrush(System.Drawing.Color.Violet), x*10, y*10, 10, 10);
                                g.DrawRectangle(new Pen(System.Drawing.Color.LightGray), x * 10, y * 10, 10, 10);
                            }
                        }
                        bmp.Save("G:\\GTA2 Test\\" + z + ".png", ImageFormat.Png);
                    }
                }
            }
        }

        private bool DebugThis(int x, int y)
        {
            if (x == 108 && y == 193)
            {
                System.Diagnostics.Debug.WriteLine("OK");
                return true;
            }
            return false;
        }
    }
}
