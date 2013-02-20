//Created: 15.03.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiale.GTA2NET.Core.Map;
using Hiale.GTA2NET.Helper;
using ANX.Framework;

namespace Hiale.GTA2NET.Logic
{
    public class AI
    {
        public GameplayObject BaseObject { get; private set; }

        private bool moveForward;

        private int savedX;
        private int savedY;
        private int savedZ;
        private int rotation;

        public AI(GameplayObject baseObject)
        {
            BaseObject = baseObject;
        }

        public void DoSomething(ref float forwardDelta, ref float rotationDelta) //ToDo another name
        {
            //get current coords
            int x = (int)BaseObject.Position3.X;
            int y = (int)BaseObject.Position3.Y;
            int z = (int)BaseObject.Position3.Z;

            if (x != savedX || y != savedY || z != savedZ)
            {
                BlockInfo block = MainGame.Map.CityBlocks[x, y, z];

                savedX = x;
                savedY = y;
                savedZ = z;

                //get arrows
                bool up = ((RoadTrafficType.GreenUp & block.Arrows) == RoadTrafficType.GreenUp);
                bool down = ((RoadTrafficType.GreenDown & block.Arrows) == RoadTrafficType.GreenDown);
                bool left = ((RoadTrafficType.GreenLeft & block.Arrows) == RoadTrafficType.GreenLeft);
                bool right = ((RoadTrafficType.GreenRight & block.Arrows) == RoadTrafficType.GreenRight);

                byte count = 0;
                //bool[] arrows = new bool[4];

                byte upIndex = 0;
                byte downIndex = 0;
                byte leftIndex = 0;
                byte rightIndex = 0;

                if (up)
                {
                    upIndex = count;
                    count++;
                }
                if (down)
                {
                    downIndex = count;
                    count++;
                }
                if (left)
                {
                    leftIndex = count;
                    count++;
                }
                if (right)
                {
                    rightIndex = count;
                    count++;
                }
                if (count < 1)
                    return;
                int random = RandomHelper.GetRandomInt(true, count);

                //calculate rotation
                //int rotation = 0;
                if (random == upIndex)
                    rotation = 180;
                else if (random == downIndex)
                    rotation = 0;
                else if (random == leftIndex)
                    rotation = 90;
                else if (random == rightIndex)
                    rotation = 270;
            }

            //get rotation of object
            float rotationObject = Math.Abs(MathHelper.ToDegrees(BaseObject.RotationAngle));
            float rotationChange = Math.Abs(rotationObject - rotation);

            //System.Diagnostics.Debug.WriteLine(rotationObject + " " + rotationChange);

            if (moveForward)
            {
                forwardDelta = 1;
                if ((int)rotationChange != 0)
                {
                    rotationDelta = 1;
                }
            }
            else
            {
                forwardDelta = -1;
                if ((int)rotationChange != 0)
                {
                    rotationDelta = -1;
                }
            }

            //rotationDelta = 1;

            if ((int)rotationChange != 0)
            {
                moveForward = !moveForward;
            }

        }

    }
}
