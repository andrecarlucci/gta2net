using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Helper
{
    public struct FaceCoordinates
    {
        public Vector3 TopLeft;
        public Vector3 TopRight;
        public Vector3 BottomRight;
        public Vector3 BottomLeft;

        public FaceCoordinates(ref Vector3 topLeft, ref Vector3 topRight, ref Vector3 bottomRight, ref Vector3 bottomLeft)
        {
            this.TopLeft = topLeft;
            this.TopRight = topRight;
            this.BottomRight = bottomRight;
            this.BottomLeft = bottomLeft;
        }
    }
}
