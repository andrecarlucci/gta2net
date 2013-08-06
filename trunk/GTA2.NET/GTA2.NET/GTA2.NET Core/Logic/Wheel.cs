// GTA2.NET
// 
// File: Wheel.cs
// Created: 17.07.2013
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
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using Hiale.GTA2NET.Core.Helper;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Logic
{
    public class Wheel
    {
        private float _mMaxForwardSpeed;
        float _mMaxBackwardSpeed;
        float _mMaxDriveForce;
        float _mMaxLateralImpulse;
        //std::set<GroundAreaFUDR*> m_groundAreas;
        //private readonly List<WaterArea> _mGroundAreas; 
        float _mCurrentTraction;
        float _mCurrentDrag;

        float _mLastDriveImpulse;
        float _mLastLateralFrictionImpulse;

        public Body Body;

        public Wheel(World world)
        {
            //_mGroundAreas = new List<WaterArea>();

            Body = new Body(world) {BodyType = BodyType.Dynamic, IsSensor = true, UserData = this};

            var shape = new PolygonShape(1);
            shape.SetAsBox(ConvertUnits.ToMeters(0.03125f), ConvertUnits.ToMeters(0.125f));
            var fixture = Body.CreateFixture(shape);
            fixture.UserData = this;
            Body.UserData = this;

            _mCurrentTraction = 1;
            _mCurrentDrag = 1;

        }

        public void SetCharacteristics(float maxForwardSpeed, float maxBackwardSpeed, float maxDriveForce, float maxLateralImpulse)
        {
            _mMaxForwardSpeed = maxForwardSpeed;
            _mMaxBackwardSpeed = maxBackwardSpeed;
            _mMaxDriveForce = maxDriveForce;
            _mMaxLateralImpulse = maxLateralImpulse;
        }

        public void UpdateTractionAndDrag()
        {
            //if (_mGroundAreas.Count == 0)
            //{
            //    _mCurrentTraction = 1;
            //    _mCurrentDrag = 1;
            //}
            //else
            //{
            //    //find area with highest traction, same for drag
            //    _mCurrentTraction = 0;
            //    _mCurrentDrag = 1; //not zero!
            //    foreach (var mGroundArea in _mGroundAreas)
            //    {
            //        if (mGroundArea.FrictionModifier > _mCurrentTraction)
            //            _mCurrentTraction = mGroundArea.FrictionModifier;
            //        if (mGroundArea.DragModifier > _mCurrentDrag)
            //            _mCurrentDrag = mGroundArea.DragModifier;
            //    }
            //}

            _mCurrentTraction = 0;
            _mCurrentDrag = 1;

        }

        public Vector2 GetLateralVelocity()
        {
            var currentRightNormal = Body.GetWorldVector(Vector2.UnitX);
            return Vector2.Dot(currentRightNormal, Body.LinearVelocity)*currentRightNormal;
        }

        public Vector2 GetForwardVelocity()
        {
            var currentForwardNormal = Body.GetWorldVector(Vector2.UnitY);
            return Vector2.Dot(currentForwardNormal, Body.LinearVelocity)*currentForwardNormal;
        }

        public void Update(float acceleration, float elapsedTime)
        {
            //angulat velocity
            Body.ApplyAngularImpulse(_mCurrentTraction * 0.1f * Body.Inertia * -Body.AngularVelocity);

            //forward linear velocity
            var currentForwardNormal = GetForwardVelocity();
            float currentForwardSpeed = Geometry.SafeNormalize(ref currentForwardNormal);
            float dragForceMagnitude = -0.25f * currentForwardSpeed;
            dragForceMagnitude *= _mCurrentDrag;
            Body.ApplyForce(_mCurrentTraction * dragForceMagnitude * currentForwardNormal, Body.WorldCenter);

            //find desired speed
            float desiredSpeed = 0;
            if (acceleration > 0)
                desiredSpeed = _mMaxForwardSpeed;
            else if (acceleration < 0)
                desiredSpeed = _mMaxBackwardSpeed;

            //find current speed in forward direction
            var worldVector = Body.GetWorldVector(Vector2.UnitY);
            float currentSpeed = Vector2.Dot(GetForwardVelocity(), worldVector);

            //apply necessary force
            float force = 0;
            if (acceleration != 0)
            {
                if (desiredSpeed > currentSpeed)
                    force = _mMaxDriveForce;
                else if (desiredSpeed < currentSpeed)
                    force = -_mMaxDriveForce*0.5f;
            }

            float speedFactor = currentSpeed/120;

            var driveImpulse = (force*elapsedTime)*worldVector;
            if (driveImpulse.Length() > _mMaxLateralImpulse)
                driveImpulse *= _mMaxLateralImpulse/driveImpulse.Length();

            var lateralFrictionImpulse = Body.Mass*-GetLateralVelocity();
            var lateralImpulseAvailable = _mMaxLateralImpulse;
            lateralImpulseAvailable *= 2.0f*speedFactor;
            if (lateralImpulseAvailable < 0.5f*_mMaxLateralImpulse)
                lateralImpulseAvailable = 0.5f*_mMaxLateralImpulse;
            if (lateralFrictionImpulse.Length() > lateralImpulseAvailable)
                lateralFrictionImpulse *= lateralImpulseAvailable/lateralFrictionImpulse.Length();

            _mLastDriveImpulse = driveImpulse.Length();
            _mLastLateralFrictionImpulse = lateralFrictionImpulse.Length();

            var impulse = driveImpulse + lateralFrictionImpulse;
            if (impulse.Length() > _mMaxLateralImpulse)
                impulse *= _mMaxLateralImpulse/impulse.Length();
            Body.ApplyLinearImpulse(_mCurrentTraction * impulse, Body.WorldCenter);
        }

        //public void AddGroundArea(WaterArea ga)
        //{
        //    _mGroundAreas.Add(ga);
        //    UpdateTractionAndDrag();
            
        //}

        //public void RemoveGroundArea(WaterArea ga)
        //{
        //    _mGroundAreas.Remove(ga);
        //    UpdateTractionAndDrag();
        //}
    }
}
