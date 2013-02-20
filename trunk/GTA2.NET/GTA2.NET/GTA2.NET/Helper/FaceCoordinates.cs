//Created: 11.02.2010

using ANX.Framework;

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
            TopLeft = topLeft;
            TopRight = topRight;
            BottomRight = bottomRight;
            BottomLeft = bottomLeft;
        }
    }
}
