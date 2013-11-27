// GTA2.NET
// 
// File: Pedestrian.cs
// Created: 16.02.2010
// 
// 
// Copyright (C) 2010-2013 Hiale
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
// is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies
// or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR
// IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// Grand Theft Auto (GTA) is a registred trademark of Rockstar Games.

using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Logic
{
    public class Pedestrian : ControlableGameObject
    {
        private Color _color;

        private bool _isJumping;

        private int _healthPoint;

        private List<Weapon> _weapons;

        private Weapon _currentWeapon; //selected weapon

        public Pedestrian(Vector3 startUpPosition) : base(startUpPosition, 0, new Helper.CompactRectangle(0, 0, 1, 1))
        {
            //Velocity = 1f;
        }

        public override void Update(float elapsedTime)
        {
            throw new System.NotImplementedException();
        }

        public override void Update(ParticipantInput input, float elapsedTime)
        {
            //float x, y;
            //x = input.Forward + Position3.X;
            RotationAngle += MathHelper.PiOver4;
            //Position3 = new Vector3(x, y, Position3.Z);
        }
    }
}
