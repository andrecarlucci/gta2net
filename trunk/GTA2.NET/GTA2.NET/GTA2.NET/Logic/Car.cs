//Created: 16.02.2010

using System.Collections.Generic;
using ANX.Framework;
using Hiale.GTA2NET.Core;

namespace Hiale.GTA2NET.Logic
{
    public class Car : GameplayObject
    {
        public CarInfo CarInfo { get; private set; }

        private List<CarWeapon> _weapons;

        private CarWeapon _currentWeapon; //selected weapon

        public Car(Vector3 startUpPosition, CarInfo carInfo) : base(startUpPosition, carInfo.Width, carInfo.Height)
        {
            CarInfo = carInfo;
            CreateSprite();
        }

        public override void Update(float elapsedTime)
        {
            //forwardDelta *= CarInfo.Physics.Thrust;
            //rotationDelta *= CarInfo.Physics.TurnRatio * 2;
            base.Update(elapsedTime);
        }
    }
}
