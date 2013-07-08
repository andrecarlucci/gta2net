// GTA2.NET
// 
// File: ObstacleCollection.cs
// Created: 03.07.2013
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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Collision
{
    public class ObstacleCollection : List<IObstacle>
    {
        public List<IObstacle> GetObstacles(int layer)
        {
            return this.Where(obstacle => obstacle.Z == layer).ToList();
        }

        public List<IObstacle> GetObstacles(int layer, bool isSlope)
        {
            return this.Where(obstacle => obstacle.Z == layer && obstacle.IsSlope == isSlope).ToList();
        }

        public List<IObstacle> GetObstacles(ObstacleType type)
        {
            return this.Where(obstacle => (type & obstacle.Type) == obstacle.Type).ToList();
        }

        public List<IObstacle> GetObstacles(ObstacleType type, bool isSlope)
        {
            return this.Where(obstacle => (type & obstacle.Type) == obstacle.Type && obstacle.IsSlope == isSlope).ToList();
        }

        public List<IObstacle> GetObstacles(int layer, ObstacleType type)
        {
            return this.Where(obstacle => (type & obstacle.Type) == obstacle.Type && obstacle.Z == layer).ToList();
        }

        public List<IObstacle> GetObstacles(int layer, ObstacleType type, bool isSlope)
        {
            return this.Where(obstacle => (type & obstacle.Type) == obstacle.Type && obstacle.Z == layer && obstacle.IsSlope == isSlope).ToList();
        }

        /// <summary>
        /// Removes obstacles which are within other obstacles
        /// </summary>
        public void RemoveUnnecessary()
        {
            var itemsToRemove = new List<IObstacle>();
            foreach (var target in this)
            {
                foreach (var source in this)
                {
                    if (source == target)
                        continue;

                    if (source.Z != target.Z)
                        continue;

                    var targetPolygon = target as PolygonObstacle;
                    var targetRectangle = target as RectangleObstacle;
                    if (targetPolygon == null && targetRectangle == null)
                        break;

                    var sourcePoints = new List<Vector2>();

                    var sourceLine = source as LineObstacle;
                    if (sourceLine != null)
                    {
                        sourcePoints.Add(sourceLine.Start);
                        sourcePoints.Add(sourceLine.End);
                    }
                    var sourceRectangle = source as RectangleObstacle;
                    if (sourceRectangle != null)
                    {
                        sourcePoints.Add(new Vector2(sourceRectangle.X, sourceRectangle.Y));
                        sourcePoints.Add(new Vector2(sourceRectangle.X + sourceRectangle.Width, sourceRectangle.Y));
                        sourcePoints.Add(new Vector2(sourceRectangle.X, sourceRectangle.Y + sourceRectangle.Length));
                        sourcePoints.Add(new Vector2(sourceRectangle.X + sourceRectangle.Width, sourceRectangle.Y + sourceRectangle.Length));
                    }
                    var sourcePolygon = source as PolygonObstacle;
                    if (sourcePolygon != null)
                    {
                        sourcePoints.AddRange(sourcePolygon.Vertices);
                    }

                    var containsAll = true;
                    if (targetPolygon != null)
                    {
                        if (sourcePoints.Any(sourcePoint => !targetPolygon.Contains(sourcePoint)))
                            containsAll = false;
                    }
                    if (targetRectangle != null)
                    {
                        if (sourcePoints.Any(sourcePoint => !targetRectangle.Contains(sourcePoint)))
                            containsAll = false;
                    }
                    if (containsAll && !itemsToRemove.Contains(source))
                        itemsToRemove.Add(source);
                }
            }
            foreach (var item in itemsToRemove)
                Remove(item);
        }

        public void Save(Map.Map map)
        {
            using (var writer = new BinaryWriter(File.Open(map.Filename + ".col", FileMode.Create)))
            {
                writer.Write("GTA2.NETv0.1");
                var mapFile = Path.GetFileName(map.Filename);
                if (mapFile == null)
                    throw new NotSupportedException();
                writer.Write(mapFile);
                var mapChecksum = GetMapCheckSum(map);
                writer.Write(mapChecksum.Length);
                writer.Write(mapChecksum);
                writer.Write(Count);
                foreach (var obstacle in this)
                {
                    writer.Write((byte) obstacle.Type);
                    writer.Write(obstacle.IsSlope);
                    writer.Write(obstacle.Z);
                    if (obstacle is RectangleObstacle)
                    {
                        var rectangleObstacle = (RectangleObstacle) obstacle;
                        writer.Write(rectangleObstacle.X);
                        writer.Write(rectangleObstacle.Y);
                        writer.Write(rectangleObstacle.Width);
                        writer.Write(rectangleObstacle.Length);
                    }
                    else if (obstacle is LineObstacle)
                    {
                        var lineObstacle = (LineObstacle)obstacle;
                        writer.Write(lineObstacle.Start.X);
                        writer.Write(lineObstacle.Start.Y);
                        writer.Write(lineObstacle.End.X);
                        writer.Write(lineObstacle.End.Y);
                    }
                    else if (obstacle is PolygonObstacle)
                    {
                        var polygonObstacle = (PolygonObstacle) obstacle;
                        writer.Write(polygonObstacle.Vertices.Count);
                        foreach (var point in polygonObstacle.Vertices)
                        {
                            writer.Write(point.X);
                            writer.Write(point.Y);
                        }
                    }
                }
            }
        }

        public static ObstacleCollection Load(Map.Map map)
        {
            var obstacles = new ObstacleCollection();
            using (var reader = new BinaryReader(File.Open(map.Filename + ".col", FileMode.Open)))
            {
                reader.ReadString(); //GTA2.NET
                reader.ReadString(); //Map Filename
                var checksumLength = reader.ReadInt32(); //Checksum Length;
                reader.ReadBytes(checksumLength); //Checksum
                var obstacleCount = reader.ReadInt32(); //Obstacle count
                for (var i = 0; i < obstacleCount; i++)
                {
                    var type = reader.ReadByte();
                    var obstacle = CreateObstacle((ObstacleType) type);
                    obstacle.IsSlope = reader.ReadBoolean();
                    obstacle.Z = reader.ReadInt32();

                    if (obstacle is RectangleObstacle)
                    {
                        var rectangleObstacle = (RectangleObstacle) obstacle;
                        rectangleObstacle.X = reader.ReadSingle();
                        rectangleObstacle.Y = reader.ReadSingle();
                        rectangleObstacle.Width = reader.ReadSingle();
                        rectangleObstacle.Length = reader.ReadSingle();
                    }
                    else if (obstacle is LineObstacle)
                    {
                        var lineObstacle = (LineObstacle) obstacle;
                        lineObstacle.Start = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                        lineObstacle.End = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    }
                    else if (obstacle is PolygonObstacle)
                    {
                        var polygonObstacle = (PolygonObstacle) obstacle;
                        var verticesCount = reader.ReadInt32();
                        for (var j = 0; j < verticesCount; j++)
                            polygonObstacle.Vertices.Add(new Vector2(reader.ReadSingle(), reader.ReadSingle()));
                    }
                    obstacles.Add(obstacle);
                }
            }
            return obstacles;
        }

        private static IObstacle CreateObstacle(ObstacleType type)
        {
            switch (type)
            {
                case ObstacleType.Line:
                    return Activator.CreateInstance<LineObstacle>();
                case ObstacleType.Polygon:
                    return Activator.CreateInstance<PolygonObstacle>();
                case ObstacleType.Rectangle:
                    return Activator.CreateInstance<RectangleObstacle>();
            }
            throw new NotSupportedException();
        }

        private static byte[] GetMapCheckSum(Map.Map map)
        {
            var hashAlgorithm = MD5.Create();
            using (var stream = new FileStream(map.Filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return hashAlgorithm.ComputeHash(stream);
            }
        }
    }
}
