// GTA2.NET
// 
// File: Car.cs
// Created: 21.02.2013
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
            //CreateSprite();
        }

        public override void Update(float elapsedTime)
        {
            //forwardDelta *= CarInfo.Physics.Thrust;
            //rotationDelta *= CarInfo.Physics.TurnRatio * 2;
            base.Update(elapsedTime);
        }
    }
}
