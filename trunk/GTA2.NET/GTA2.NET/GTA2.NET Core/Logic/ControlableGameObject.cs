// GTA2.NET
// 
// File: GTA2Game.cs
// Created: 28.08.2013
// Created by: João Pires
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
using Microsoft.Xna.Framework;
using Hiale.GTA2NET.Core.Helper;

namespace Hiale.GTA2NET.Core.Logic
{
    /// <summary>
    /// Represent a GameObject that a player could control
    /// </summary>
    public abstract class ControlableGameObject : GameObject
    {
        /// <summary>
        /// True in case a player control this object, False otherwise
        /// </summary>
        public Boolean PlayerControlled { get; set; }

        /// <summary>
        /// Creates a instance of ControlableGameObject
        /// </summary>
        /// <param name="startUpPosition">The initial position for the object</param>
        /// <param name="startUpRotation">The initial rotation of the object</param>
        protected ControlableGameObject(Vector3 startUpPosition, float startUpRotation) : base(startUpPosition, startUpRotation, new Helper.CompactRectangle(0,0,1,1))
        {
        }

        /// <summary>
        /// Creates a instance of ControlableGameObject
        /// </summary>
        /// <param name="startUpPosition">The initial position for the object</param>
        /// <param name="startUpRotation">The initial rotation of the object</param>
        protected ControlableGameObject(Vector3 startUpPosition, float startUpRotation, CompactRectangle shape)
            : base(startUpPosition, startUpRotation, shape)
        {
        }

        /// <summary>
        /// Updates the state of the Object
        /// </summary>        
        /// <param name="input">The Input to apply to the Object</param>
        /// <param name="elapsedTime">The time occurred since the last Update</param>
        public abstract void Update(ParticipantInput input, float elapsedTime);
    }
}
