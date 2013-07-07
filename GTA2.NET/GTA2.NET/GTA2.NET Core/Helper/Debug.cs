// GTA2.NET
// 
// File: Debug.cs
// Created: 01.06.2013
// 
// 
// Copyright (C) 2010-2013 Hiale
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
// is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies
// or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR
// IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Grand Theft Auto (GTA) is a registred trademark of Rockstar Games.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Hiale.GTA2NET.Core.Collision;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Helper
{
    public static class Debug
    {
        public static void Run()
        {
            DebugObstacles();
        }

        private static void DebugObstacles()
        {
            try
            {
                var map = new Map.Map(Globals.MapsSubDir + "\\MP1-comp.gmp");
                //var map = new Map.Map(Globals.MapsSubDir + "\\bil.gmp");
                var collision = new MapCollision(map);
                var obstacles = collision.GetObstacles();
                DisplayCollision(obstacles);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
            
        }

        private static void DisplayCollision(IEnumerable<IObstacle> obstacles)
        {
            var layers = new Dictionary<int, Bitmap>();
            foreach (var obstacle in obstacles)
            {
                Bitmap bmp;
                if (layers.ContainsKey(obstacle.Z))
                {
                    bmp = layers[obstacle.Z];
                }
                else
                {
                    bmp = new Bitmap(2560, 2560);
                    layers.Add(obstacle.Z, bmp);
                }

                using (var g = Graphics.FromImage(bmp))
                {
                    if (obstacle is SlopeRectangleObstacle)
                    {
                        DrawRectangle(obstacle, g, System.Drawing.Color.Blue);
                    }
                    else if (obstacle is RectangleObstacle)
                    {
                        DrawRectangle(obstacle, g, System.Drawing.Color.Red);
                    }
                    else if (obstacle is LineObstacle)
                    {
                        var lineObstacle = (LineObstacle)obstacle;
                        g.DrawLine(new Pen(System.Drawing.Color.FromArgb(192,System.Drawing.Color.Magenta)), new System.Drawing.Point((int)lineObstacle.Start.X * 10, (int)lineObstacle.Start.Y * 10), new System.Drawing.Point((int)lineObstacle.End.X * 10, (int)lineObstacle.End.Y * 10));
                        g.DrawEllipse(new Pen(System.Drawing.Color.Magenta), lineObstacle.Start.X * 10 - 2, lineObstacle.Start.Y * 10 - 2, 4, 4);
                        g.DrawEllipse(new Pen(System.Drawing.Color.Magenta), lineObstacle.End.X * 10 - 2, lineObstacle.End.Y * 10 - 2, 4, 4);
                    }
                    else if (obstacle is SlopePolygonObstacle)
                    {
                        DrawPolygon(obstacle, g, System.Drawing.Color.Cyan);
                    }
                    else if (obstacle is PolygonObstacle)
                    {
                        DrawPolygon(obstacle, g, System.Drawing.Color.OrangeRed);
                    }

                }
            }

            foreach (var pair in layers)
            {
                pair.Value.Save(pair.Key + ".png", ImageFormat.Png);
                pair.Value.Dispose();
            }
        }

        private static void DrawPolygon(IObstacle obstacle, Graphics g, System.Drawing.Color color)
        {
            var polygonObstacle = (PolygonObstacle) obstacle;
            var points = new System.Drawing.Point[polygonObstacle.Vertices.Count];
            for (var i = 0; i < polygonObstacle.Vertices.Count; i++)
            {
                points[i] = new System.Drawing.Point((int) polygonObstacle.Vertices[i].X*10, (int) polygonObstacle.Vertices[i].Y*10);
                g.DrawEllipse(new Pen(color), points[i].X - 2, points[i].Y - 2, 4, 4);
            }
            g.FillPolygon(new SolidBrush(System.Drawing.Color.FromArgb(128, color)), points);
            g.DrawPolygon(new Pen(System.Drawing.Color.FromArgb(192, color)), points);
        }

        private static void DrawRectangle(IObstacle obstacle, Graphics g, System.Drawing.Color color)
        {
            var rectObstacle = (RectangleObstacle) obstacle;
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(128, color)), rectObstacle.X * 10, rectObstacle.Y * 10, rectObstacle.Width * 10, rectObstacle.Length * 10);
            g.DrawRectangle(new Pen(System.Drawing.Color.FromArgb(192, color)), rectObstacle.X * 10, rectObstacle.Y * 10, rectObstacle.Width * 10, rectObstacle.Length * 10);
            g.DrawEllipse(new Pen(color), rectObstacle.X * 10 - 2, rectObstacle.Y * 10 - 2, 4, 4);
            g.DrawEllipse(new Pen(color), (rectObstacle.X + rectObstacle.Width) * 10 - 2, rectObstacle.Y * 10 - 2, 4, 4);
            g.DrawEllipse(new Pen(color), (rectObstacle.X + rectObstacle.Width) * 10 - 2, (rectObstacle.Y + rectObstacle.Length) * 10 - 2, 4, 4);
            g.DrawEllipse(new Pen(color), rectObstacle.X * 10 - 2, (rectObstacle.Y + rectObstacle.Length) * 10 - 2, 4, 4);
        }


        public static void SaveSegmentsPicture(List<LineSegment> segments, Stream outputStream, string name)
        {
            var fileName = name + ".png";
            Bitmap bmp;
            if (File.Exists(fileName))
            {
                var image = Image.FromFile(fileName);
                bmp = new Bitmap(image);
                image.Dispose();
            }
            else
                bmp = new Bitmap(2560, 2560);
            using (var g = Graphics.FromImage(bmp))
            {
                foreach (var segment in segments)
                {
                    g.DrawLine(new Pen(new SolidBrush(System.Drawing.Color.Red), 1), segment.Start.X * 10, segment.Start.Y * 10, segment.End.X * 10, segment.End.Y * 10);
                }
            }
            if (outputStream == null)
                bmp.Save("debug\\" + fileName, ImageFormat.Png);
            else
                bmp.Save(outputStream, ImageFormat.Png);
            bmp.Dispose();

        }

        public static void SaveSegmentsPicture(List<LineSegment> segments, string name)
        {
            SaveSegmentsPicture(segments, null, name);
        }

        public static void SavePolygonPicture(List<Vector2> vertices)
        {
            var points = new PointF[vertices.Count];
            for (var i = 0; i < vertices.Count; i++)
                points[i] = new PointF(vertices[i].X * 10, vertices[i].Y * 10);
            SavePolygonPicture(points);
        }

        public static void SavePolygonPicture(PointF[] points)
        {
            try
            {
                using (var bmp = new Bitmap(2560, 2560))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.DrawPolygon(new Pen(System.Drawing.Color.OrangeRed, 1), points);
                    }
                    bmp.Save("debug\\polygon.png", ImageFormat.Png);
                }
            }
            catch (Exception)
            {
                //ignore
            }
        }
    }
}
