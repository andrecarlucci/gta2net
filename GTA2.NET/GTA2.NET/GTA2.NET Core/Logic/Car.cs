// GTA2.NET
// 
// File: Car.cs
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
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Logic
{
    public class Car : Vehicle
    {
        public CarInfo CarInfo { get; private set; }

        private Body _body;

        private readonly Wheel[] _wheels;

        private RevoluteJoint _backLeftJoint;
        private RevoluteJoint _backRightJoint;
        private RevoluteJoint _frontLeftJoint;
        private RevoluteJoint _frontRightJoint;

        public Car(Vector3 startUpPosition, float startUpRotation, CarInfo carInfo) : base(startUpPosition, startUpRotation, carInfo.Width, carInfo.Height)
        {
            CarInfo = carInfo;
            _wheels = new Wheel[4];
        }

        public override void SetWorld(World world)
        {
            CreateCar(world);
        }

        private void CreateCar(World world)
        {
            _body = new Body(world) { BodyType = BodyType.Dynamic, AngularDamping = 5 };

            var halfWidth = Width/2;
            var halfHeight = Height/2;

            var frontWheelOffset = (float) CarInfo.FrontWheelOffset/64;
            var rearWheelOffset =  (float) CarInfo.RearWheelOffset/64;

            var vertices = new Vertices(4);
            vertices.Add(new Vector2(Position3.X - halfWidth, Position3.Y - halfHeight)); //Top-Left
            vertices.Add(new Vector2(Position3.X + halfWidth, Position3.Y - halfHeight)); //Top-Right
            vertices.Add(new Vector2(Position3.X + halfWidth, Position3.Y + halfHeight)); //Bottom-Right
            vertices.Add(new Vector2(Position3.X - halfWidth, Position3.Y + halfHeight)); //Bottom-Left

            var fixture = _body.CreateFixture(new PolygonShape(vertices.ToMeters(), 0.1f)); //shape, density

            float maxForwardSpeed = 300;
            float maxBackwardSpeed = -40;
            float backWheelMaxDriveForce = 950;
            float frontWheelMaxDriveForce = 400;
            float backWheelMaxLateralImpulse = 9;
            float fronWheelMaxLateralImpulse = 9;

            //back left wheel
            var wheel = new Wheel(world);
            wheel.SetCharacteristics(maxForwardSpeed, maxBackwardSpeed, backWheelMaxDriveForce, backWheelMaxLateralImpulse);
            _wheels[0] = wheel;
            _backLeftJoint = CreateJoint(_body, wheel.Body, new Vector2(Position3.X + halfWidth, Position3.Y + rearWheelOffset).ToMeters(), world);
            wheel.Body.Position = _backLeftJoint.LocalAnchorA;

            //back right wheel
            wheel = new Wheel(world);
            wheel.SetCharacteristics(maxForwardSpeed, maxBackwardSpeed, backWheelMaxDriveForce, backWheelMaxLateralImpulse);
            _wheels[1] = wheel;
            _backRightJoint = CreateJoint(_body, wheel.Body, new Vector2(Position3.X - halfWidth, Position3.Y + rearWheelOffset).ToMeters(), world);
            wheel.Body.Position = _backRightJoint.LocalAnchorA;

            //front left wheel
            wheel = new Wheel(world);
            wheel.SetCharacteristics(maxForwardSpeed, maxBackwardSpeed, frontWheelMaxDriveForce, fronWheelMaxLateralImpulse);
            _wheels[2] = wheel;
            _frontLeftJoint = CreateJoint(_body, wheel.Body, new Vector2(Position3.X + halfWidth, Position3.Y + frontWheelOffset).ToMeters(), world);
            wheel.Body.Position = _frontLeftJoint.LocalAnchorA;

            //front right wheel
            wheel = new Wheel(world);
            wheel.SetCharacteristics(maxForwardSpeed, maxBackwardSpeed, frontWheelMaxDriveForce, fronWheelMaxLateralImpulse);
            _wheels[3] = wheel;
            _frontRightJoint = CreateJoint(_body, wheel.Body, new Vector2(Position3.X - halfWidth, Position3.Y + frontWheelOffset).ToMeters(), world);
            wheel.Body.Position = _frontRightJoint.LocalAnchorA;
        }

        private static RevoluteJoint CreateJoint(Body carBody, Body wheelBody, Vector2 anchor, World world)
        {
            var joint = JointFactory.CreateRevoluteJoint(carBody, wheelBody, Vector2.Zero);
            joint.LocalAnchorA = anchor;
            joint.LimitEnabled = true;
            joint.LowerLimit = 0;
            joint.UpperLimit = 0;
            world.AddJoint(joint);
            return joint;
        }

        public override void Update(ParticipantInput input, float elapsedTime)
        {
            System.Diagnostics.Debug.WriteLine(_body.Position);
            foreach (var wheel in _wheels)
            {
                wheel.UpdateFriction();
                wheel.UpdateDrive(input.Forward, input.Rotation);
            }

            //control steering
            var lockAngle = MathHelper.ToRadians(35);
            var turnSpeedPerSec = MathHelper.ToRadians(320); //from lock to lock in 0.25s
            var turnPerTimeSpep = turnSpeedPerSec / 60f;
            var desiredAngle = 0f;
            if (input.Rotation < 0)
                desiredAngle = lockAngle;
            else if (input.Rotation > 0)
                desiredAngle = -lockAngle;
            var angleNow = _frontLeftJoint.JointAngle;
            var angleToTurn = desiredAngle - angleNow;
            angleToTurn = MathHelper.Clamp(angleToTurn, -turnPerTimeSpep, turnPerTimeSpep);
            var newAngle = angleNow + angleToTurn;
            _frontLeftJoint.LowerLimit = newAngle;
            _frontLeftJoint.UpperLimit = newAngle;
            _frontRightJoint.LowerLimit = newAngle;
            _frontRightJoint.UpperLimit = newAngle;
        }
    }
}
