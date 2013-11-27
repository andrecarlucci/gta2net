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

        public FaceCoordinates(Vector3 topLeft, Vector3 topRight, Vector3 bottomRight, Vector3 bottomLeft)
        {
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
        }
    }
}
