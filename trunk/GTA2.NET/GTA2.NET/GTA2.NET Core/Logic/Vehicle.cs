// GTA2.NET
// 
// File: Vehicle.cs
// Created: 16.07.2013
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

using System;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Logic
{
    public abstract class Vehicle : GameplayObject, IPhysicsBehaviour, ISprite
    {
        protected Vehicle(Vector3 startUpPosition, float startUpRotation) : base(startUpPosition, startUpRotation)
        {

        }

        public new virtual Vector3 Position3
        {
            get { return base.Position3; }
            set { base.Position3 = value; }
        }

        public new virtual Vector2 Position2
        {
            get { return base.Position2; }
            set { throw new NotSupportedException("Set Position3 instead."); }
        }

        public abstract Vector2 CollisionTopLeft { get; }
        public abstract Vector2 CollisionTopRight { get; }
        public abstract Vector2 CollisionBottomRight { get; }
        public abstract Vector2 CollisionBottomLeft { get; }
        public abstract float CollisionWidth { get; }
        public abstract float CollisionHeight { get; }
        public abstract void CreateBody(World world, float width, float height);

        public abstract Vector2 SpriteTopLeft { get; }
        public abstract Vector2 SpriteTopRight { get; }
        public abstract Vector2 SpriteBottomRight { get; }
        public abstract Vector2 SpriteBottomLeft { get; }
        public abstract float SpriteWidth { get; }
        public abstract float SpriteHeight { get; }
        public abstract void SetDimensions(float width, float height);
    }
}
