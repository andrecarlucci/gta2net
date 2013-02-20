//Created: 23.02.2010
//Source: http://www.codeproject.com/KB/GDI-plus/PolygonCollision.aspx --> ported to XNA

using System;
using System.Collections.Generic;
using Hiale.GTA2NET.Core;
using ANX.Framework;

namespace Hiale.GTA2NET.Helper
{
    public class Polygon
    {
        public List<Vector2> Edges { get; private set; }

        public List<Vector2> Points { get; private set; }

        public Polygon()
        {
            Points = new List<Vector2>();
            Edges = new List<Vector2>();
        }

        public void BuildEdges()
        {
            Vector2 p1;
            Vector2 p2;
            Edges.Clear();
            for (int i = 0; i < Points.Count; i++)
            {
                p1 = Points[i];
                if (i + 1 >= Points.Count)
                {
                    p2 = Points[0];
                }
                else
                {
                    p2 = Points[i + 1];
                }
                Edges.Add(p2 - p1);
            }
        }

        public Vector2 Center
        {
            get
            {
                float totalX = 0;
                float totalY = 0;
                for (int i = 0; i < Points.Count; i++)
                {
                    totalX += Points[i].X;
                    totalY += Points[i].Y;
                }

                return new Vector2(totalX / (float)Points.Count, totalY / (float)Points.Count);
            }
        }

        public void Offset(Vector2 v)
        {
            Offset(v.X, v.Y);
        }

        public void Offset(float x, float y)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                Vector2 p = Points[i];
                Points[i] = new Vector2(p.X + x, p.Y + y);
            }
        }

        public override string ToString()
        {
            string result = "";

            for (int i = 0; i < Points.Count; i++)
            {
                if (result != "") result += " ";
                result += "{" + Points[i].ToString() + "}";
            }

            return result;
        }
    }

    public class PolygonLine : Polygon
    {
        public Direction BlockFaceType { get; set; }

        public Vector2 Start
        {
            get { return Points[0]; }
            set { Points[0] = value; }
        }

        public Vector2 End
        {
            get { return Points[1]; }
            set { Points[1] = value; }
        }


        public PolygonLine()
        {

        }

        public PolygonLine(Vector2 pointA, Vector2 pointB) : this(pointA, pointB, Direction.None)
        {

        }

        public PolygonLine(Vector2 pointA, Vector2 pointB, Direction blockFaceType)
        {
            Points.Add(pointA);
            Points.Add(pointB);
            BlockFaceType = blockFaceType;
        }

        public void CropStart()
        {
            float x = End.X - Start.X;
            float y = End.Y - Start.Y;
            if (x > 0)
                x -= BaseGame.Epsilon * 20;
            else if (x < 0)
                x += BaseGame.Epsilon * 20;
            if (y > 0)
                y -= BaseGame.Epsilon * 20;
            else if (y < 0)
                y += BaseGame.Epsilon * 20;
            Start = new Vector2(End.X - x, End.Y - y);
        }

        /// <summary> 
        /// Returns the intersection point between this segment and the  
        /// provided segment, if one exists. If the two segments are parallel 
        /// or coincident, then no intersection exists. 
        /// </summary> 
        /// <remarks> 
        /// http://local.wasp.uwa.edu.au/~pbourke/geometry/lineline2d/ 
        /// </remarks> 
        public Vector2? IntersectionWith(PolygonLine other)
        {
            float x1 = Points[0].X, x2 = Points[1].X, x3 = other.Points[0].X, x4 = other.Points[1].X;
            float y1 = Points[0].Y, y2 = Points[1].Y, y3 = other.Points[0].Y, y4 = other.Points[1].Y;

            float denom = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);

            if (denom == 0)
                // Lines are parallel (or coincident) 
                return null;

            float ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / denom;
            float ub = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / denom;

            if (ua < 0 || ua > 1 || ub < 0 || ub > 1)
                // Intersection point lies outside one or both segments. 
                return null;
            else
                return new Vector2(x1 + ua * (x2 - x1), y1 + ua * (y2 - y1));
        }

    }
}