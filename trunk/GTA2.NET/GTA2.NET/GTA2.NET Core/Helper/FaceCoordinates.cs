using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiale.GTA2NET.Core.Helper
{
    public class FaceCoordinates
    {
        public Vector3 BottomLeft;
        public Vector3 BottomRight;
        public Vector3 TopLeft;
        public Vector3 TopRight;

        private float width;
        private float height;
        private Vector3 centerPos;
        public Vector3 CenterPos 
        { 
            get { return centerPos; }
            set 
            { 
                centerPos = value;
                TopLeft = new Vector3(centerPos.X - (width / 2), -(centerPos.Y - (height / 2)), centerPos.Z);
                TopRight = new Vector3(centerPos.X + (width / 2), -(centerPos.Y - (height / 2)), centerPos.Z);
                BottomRight = new Vector3(centerPos.X + (width / 2), -(centerPos.Y + (height / 2)), centerPos.Z);
                BottomLeft = new Vector3(centerPos.X - (width / 2), -(centerPos.Y + (height / 2)), centerPos.Z);
            } 
        }

        public FaceCoordinates(Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
        }

        public FaceCoordinates(Vector3 centerPos, float width, float height)
        {
            CenterPos = centerPos;
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Rotates all the vertices around CenterPos by angle radians. 
        /// </summary>
        /// <param name="angle">The angle to rotate.</param>
        public FaceCoordinates Rotate(float angle)
        {
            Vector2 rotationPoint = new Vector2(CenterPos.X, -CenterPos.Y);
            Vector3 tLeft = new Vector3(Geometry.RotatePoint(new Vector2(TopLeft.X, TopLeft.Y), rotationPoint, angle), TopLeft.Z);
            Vector3 tRight = new Vector3(Geometry.RotatePoint(new Vector2(TopRight.X, TopRight.Y), rotationPoint, angle), TopRight.Z);
            Vector3 bRight = new Vector3(Geometry.RotatePoint(new Vector2(BottomRight.X, BottomRight.Y), rotationPoint, angle), BottomRight.Z);
            Vector3 bLeft = new Vector3(Geometry.RotatePoint(new Vector2(BottomLeft.X, BottomLeft.Y), rotationPoint, angle), BottomLeft.Z);

            return new FaceCoordinates(tLeft, tRight, bRight, bLeft);
        }
    }
}
