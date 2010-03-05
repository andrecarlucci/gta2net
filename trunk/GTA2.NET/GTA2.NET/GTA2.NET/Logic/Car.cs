//Created: 16.02.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Logic
{
    public class Car : MovableObject
    {
        private List<CarWeapon> _weapons;

        private CarWeapon _currentWeapon; //selected weapon

        public Car(Vector3 position) : base(position)
        {
            
        }

    }
}
