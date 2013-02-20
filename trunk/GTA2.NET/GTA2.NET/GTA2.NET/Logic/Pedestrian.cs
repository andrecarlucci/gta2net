//Created: 16.02.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ANX.Framework;
using ANX.Framework.Graphics;

namespace Hiale.GTA2NET.Logic
{
    public class Pedestrian : GameplayObject
    {
        private Color _color;

        private bool _isJumping;

        private int _healthPoint;

        private List<Weapon> _weapons;

        private Weapon _currentWeapon; //selected weapon

        public Pedestrian(Vector3 startUpPosition) : base(startUpPosition, 1, 1)
        {
            //Velocity = 1f;
        }
        
    }
}
