//Created 24.02.2013

using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Collision
{
    public interface IObstacle
    {
        int Z { get; set; }

    }

    public class RectangleObstacle : IObstacle
    {
        public int Z { get; set; }

        public Vector2 Position;

        public int Width;

        public int Length;

        public RectangleObstacle(int z, Vector2 position, int width, int length)
        {
            Z = z;
            Position = position;
            Width = width;
            Length = length;
        }
    }

    public class PolygonObstacle : IObstacle
    {
        public int Z { get; set; }
    }

    public class TriangleObstacle : IObstacle
    {
        public int Z { get; set; }
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

        public override string ToString()
        {
            return Start + " - " + End;
        }

        
    }
}
