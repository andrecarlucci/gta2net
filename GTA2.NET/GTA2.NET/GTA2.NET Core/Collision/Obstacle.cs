//Created 24.02.2013

using System.Collections.Generic;
using Hiale.GTA2NET.Core.Map;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Collision
{
    public interface IObstacle
    {
        int Z { get; set; }
    }

    public struct SlopeObstacle : IObstacle
    {
        public int Z { get; set; }

        public Vector2 Position;

        public SlopeType SlopeType;

        public SlopeObstacle(int z, Vector2 position, SlopeType slopeType) : this()
        {
            Z = z;
            Position = position;
            SlopeType = slopeType;
        }
    }

    public struct RectangleObstacle : IObstacle
    {
        public int Z { get; set; }

        public Vector2 Position;

        public int Width;

        public int Length;

        public RectangleObstacle(int z, Vector2 position, int width, int length) : this()
        {
            Z = z;
            Position = position;
            Width = width;
            Length = length;
        }
    }

    public struct PolygonObstacle : IObstacle
    {
        public int Z { get; set; }

        public List<Vector2> Vertices { get; set; }
 
        public PolygonObstacle(int z) : this()
        {
            Z = z;
            Vertices = new List<Vector2>();
        }

    }

    public enum LineObstacleType
    {
        Horizontal,
        Vertical,
        Other
    }

    public struct LineObstacle : IObstacle
    {
        public int Z { get; set; }
        public Vector2 Start;
        public Vector2 End;
        public LineObstacleType Type;

        public LineObstacle(int z, Vector2 start, Vector2 end, LineObstacleType type) : this()
        {
            Z = z;
            Start = start;
            End = end;
            Type = type;
        }

        public static LineObstacle DefaultLeft(int x, int y, int z)
        {
            return new LineObstacle(z, new Vector2(x, y), new Vector2(x, y + 1), LineObstacleType.Vertical);
        }

        public static LineObstacle DefaultTop(int x, int y, int z)
        {
            return new LineObstacle(z, new Vector2(x, y), new Vector2(x + 1, y), LineObstacleType.Horizontal);
        }

        public static LineObstacle DefaultRight(int x, int y, int z)
        {
            return new LineObstacle(z, new Vector2(x + 1, y), new Vector2(x + 1, y + 1), LineObstacleType.Vertical);
        }

        public static LineObstacle DefaultBottom(int x, int y, int z)
        {
            return new LineObstacle(z, new Vector2(x, y + 1), new Vector2(x + 1, y + 1), LineObstacleType.Horizontal);
        }

        public override string ToString()
        {
            return Start + " - " + End;
        }

        
    }
}
