//Created 24.02.2013

using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Collision
{
    public interface IObstacle
    {
        
    }

    public class RectangleObstacle : IObstacle
    {
        
    }

    public class PolygonObstacle : IObstacle
    {
        
    }

    //public class LineObstacle : IObstacle
    //{
        
    //}

    public class TriangleObstacle : IObstacle
    {
        
    }

    public enum LineObstacleType
    {
        Horizontal,
        Vertical,
        Other
    }

    public struct LineObstacle : IObstacle
    {
        public Vector2 Start;
        public Vector2 End;
        public LineObstacleType Type;

        public LineObstacle(Vector2 start, Vector2 end, LineObstacleType type)
        {
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
