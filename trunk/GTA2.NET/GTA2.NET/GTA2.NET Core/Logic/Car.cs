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

        internal Body Body;

        private readonly Wheel[] _wheels;

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
            Body = new Body(world) { BodyType = BodyType.Dynamic, AngularDamping = 5 };

            float halfWidth = Width/2;
            float halfHeight = Height/2;

            var vertices = new Vertices(4);
            vertices.Add(new Vector2(Position3.X - halfWidth, Position3.Y - halfHeight)); //Top-Left
            vertices.Add(new Vector2(Position3.X + halfWidth, Position3.Y - halfHeight)); //Top-Right
            vertices.Add(new Vector2(Position3.X + halfWidth, Position3.Y + halfHeight)); //Bottom-Right
            vertices.Add(new Vector2(Position3.X - halfWidth, Position3.Y + halfHeight)); //Bottom-Left

            var fixture = Body.CreateFixture(new PolygonShape(vertices, 0.1f)); //shape, density

            float maxForwardSpeed = 300;
            float maxBackwardSpeed = -40;
            float backTireMaxDriveForce = 950;
            float frontTireMaxDriveForce = 400;
            float backTireMaxLateralImpulse = 9;
            float frontTireMaxLateralImpulse = 9;

            //back left tire
            var tire = new Wheel(world);
            tire.SetCharacteristics(maxForwardSpeed, maxBackwardSpeed, backTireMaxDriveForce, backTireMaxLateralImpulse);
            _wheels[0] = tire;
            CreateJoint(Body, tire.Body, new Vector2(-halfWidth, 0.75f), world);

            //back right tire
            tire = new Wheel(world);
            tire.SetCharacteristics(maxForwardSpeed, maxBackwardSpeed, backTireMaxDriveForce, backTireMaxLateralImpulse);
            _wheels[1] = tire;
            CreateJoint(Body, tire.Body, new Vector2(halfWidth, 0.75f), world);

            //front left tire
            tire = new Wheel(world);
            tire.SetCharacteristics(maxForwardSpeed, maxBackwardSpeed, frontTireMaxDriveForce, frontTireMaxLateralImpulse);
            _wheels[2] = tire;
            _frontLeftJoint = CreateJoint(Body, tire.Body, new Vector2(-halfWidth, 8.5f), world);

            //front right tire
            tire = new Wheel(world);
            tire.SetCharacteristics(maxForwardSpeed, maxBackwardSpeed, frontTireMaxDriveForce, frontTireMaxLateralImpulse);
            _wheels[3] = tire;
            _frontRightJoint = CreateJoint(Body, tire.Body, new Vector2(halfWidth, 8.5f), world);
        }

        private static RevoluteJoint CreateJoint(Body carBody, Body tireBody, Vector2 anchor, World world)
        {
            var joint = JointFactory.CreateRevoluteJoint(carBody, tireBody, Vector2.Zero);
            joint.LocalAnchorA = anchor;
            joint.LimitEnabled = true;
            joint.LowerLimit = 0;
            joint.UpperLimit = 0;
            world.AddJoint(joint);
            return joint;
        }

        public override void Update(ParticipantInput input, float elapsedTime)
        {
            System.Diagnostics.Debug.WriteLine(Body.Position);
            foreach (var tire in _wheels)
            {
                tire.UpdateFriction();
                tire.UpdateDrive(input.Forward, input.Rotation);
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
