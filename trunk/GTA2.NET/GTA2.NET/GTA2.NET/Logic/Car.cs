//Created: 16.02.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Hiale.GTA2NET.Core;

namespace Hiale.GTA2NET.Logic
{
    public class Car : MovableObject
    {
        public CarInfo CarInfo { get; private set; }

        private List<CarWeapon> _weapons;

        private CarWeapon _currentWeapon; //selected weapon

        public Car(Vector3 startUpPosition, CarInfo carInfo) : base(startUpPosition, carInfo.Width, carInfo.Height)
        {
            CarInfo = carInfo;
            CreateSprite();
        }
    }
}
