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
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Hiale.GTA2NET.Core.Helper;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Logic
{
    /// <summary>
    /// Represents a car.
    /// See Car physics.png:
    /// Steering mechanism joints - The front wheels rotate on these points. Angle is locked from -35 degrees to +35 degrees (subject to change).
    /// Front wheels - Are needed to determine in which angle the car should drive; not displayed to the player, only exists as physics engine object
    /// Sprite area (whole gray area) - Any sprite data is drawn on this layer. Usually bigger than the collsions box, because it can also contain door information for example.
    /// Collision box - determines the area for collision detection. Usually smaller than the sprite area.
    /// Rear wheels - Fixed angle wheels. Invisible. Force is applied to these wheels which moves the whole car as a consequence,
    /// </summary>
    public class Car : Vehicle
    {
        public CarInfo CarInfo { get; private set; }

        private const float angleLock = 35f;
        private const float lockToLockTime = 500; //from lock to lock in 0.5 seconds

        private Body _body;
        private PolygonShape _shape;
        private PolygonShape _spriteShape;

        private float _collisionWidth;
        private float _collisionHeight;
        private float _spriteWidth;
        private float _spriteHeight;

        private readonly Wheel[] _wheels;

        private RevoluteJoint _backLeftJoint;
        private RevoluteJoint _backRightJoint;
        private RevoluteJoint _frontLeftJoint;
        private RevoluteJoint _frontRightJoint;

        public override float CollisionWidth
        {
            get { return _collisionWidth; }
        }

        public override float CollisionHeight
        {
            get { return _collisionHeight; }
        }

        public override event EventHandler Collided;

        public override Vector2 CollisionTopLeft
        {
            get { return _shape.Vertices[0].ToBlockUnits(); }
        }

        public override Vector2 CollisionTopRight
        {
            get { return _shape.Vertices[1].ToBlockUnits(); }
        }

        public override Vector2 CollisionBottomLeft
        {
            get { return _shape.Vertices[3].ToBlockUnits(); }
        }

        public override Vector2 CollisionBottomRight
        {
            get { return _shape.Vertices[2].ToBlockUnits(); }
        }


        private Vector2 GetSpriteVector(int index)
        {
            Transform transform;
            _body.GetTransform(out transform);
            var vector = MathUtils.Multiply(ref transform, _spriteShape.Vertices[index]);
            return vector.ToBlockUnits();
        }

        public override Vector2 SpriteTopLeft
        {
            get { return GetSpriteVector(0); }
        }

        public override Vector2 SpriteTopRight
        {
            get { return GetSpriteVector(1); }
        }

        public override Vector2 SpriteBottomLeft
        {
            get { return GetSpriteVector(3); }
        }

        public override Vector2 SpriteBottomRight
        {
            get { return GetSpriteVector(2); }
        }

        public override float SpriteWidth
        {
            get { return _spriteWidth; }
        }

        public override float SpriteHeight
        {
            get { return _spriteHeight; }
        }

        public new float RotationAngle
        {
            get { return _body.Rotation; }
        }

        public override void SetDimensions(float width, float height)
        {
            _spriteWidth = width;
            _spriteHeight = height;
        }

        public Car(Vector3 startUpPosition, float startUpRotation, CarInfo carInfo) : base(startUpPosition, startUpRotation)
        {
            CarInfo = carInfo;
            _wheels = new Wheel[4];
        }

        public override void CreateBody(World world, float width, float height)
        {
            _collisionWidth = width;
            _collisionHeight = height;
            var carPosition = Position2.ToMeters();
            _body = new Body(world) {BodyType = BodyType.Dynamic, Position = carPosition, AngularDamping = 5};

            var halfWidth = width/2;
            var halfHeight = height/2;

            var frontWheelOffset = (float) CarInfo.FrontWheelOffset/64;
            var rearWheelOffset =  (float) CarInfo.RearWheelOffset/64;

            //collision detection Fixture
            var vertices = new Vertices(4);
            vertices.Add(new Vector2(-halfWidth, -halfHeight)); //Top-Left
            vertices.Add(new Vector2(halfWidth, -halfHeight)); //Top-Right
            vertices.Add(new Vector2(halfWidth, halfHeight)); //Bottom-Right
            vertices.Add(new Vector2(-halfWidth, halfHeight)); //Bottom-Left
            _shape = new PolygonShape(vertices.ToMeters(), 0.1f);
            var fixture = _body.CreateFixture(_shape); //shape, density
            fixture.OnCollision += OnCollision;

            //SpriteId Fixture
            //var spriteHalfWidth = (float) CarInfo.Sprite.Rectangle.CollisionWidth/2; //ToDo
            //var spriteHalfHeight = (float) CarInfo.Sprite.Rectangle.CollisionHeight/2;
            var spriteHalfWidth = (float) 52/64/2;
            var spriteHalfHeight = (float) 128/64/2;
            var spriteVertices = new Vertices(4);
            spriteVertices.Add(new Vector2(-spriteHalfWidth, -spriteHalfHeight)); //Top-Left
            spriteVertices.Add(new Vector2(spriteHalfWidth, -spriteHalfHeight)); //Top-Right
            spriteVertices.Add(new Vector2(spriteHalfWidth, spriteHalfHeight)); //Bottom-Right
            spriteVertices.Add(new Vector2(-spriteHalfWidth, spriteHalfHeight)); //Bottom-Left
            _spriteShape = new PolygonShape(spriteVertices.ToMeters(), 0.1f);
            var spriteFixture = _body.CreateFixture(_spriteShape);
            spriteFixture.IsSensor = true;

            float maxForwardSpeed = 300;
            float maxBackwardSpeed = -40;
            float backWheelMaxDriveForce = 950;
            float frontWheelMaxDriveForce = 400;
            float backWheelMaxLateralImpulse = 9;
            float fronWheelMaxLateralImpulse = 9;

            //back left wheel
            var wheelOffsetPosition = new Vector2(halfWidth, rearWheelOffset).ToMeters();
            var wheel = new Wheel(world);
            wheel.SetCharacteristics(maxForwardSpeed, maxBackwardSpeed, backWheelMaxDriveForce, backWheelMaxLateralImpulse);
            _wheels[0] = wheel;
            _backLeftJoint = CreateJoint(_body, wheel.Body, wheelOffsetPosition, world);

            //back right wheel
            wheelOffsetPosition = new Vector2(-halfWidth, rearWheelOffset).ToMeters();
            wheel = new Wheel(world);
            wheel.SetCharacteristics(maxForwardSpeed, maxBackwardSpeed, backWheelMaxDriveForce, backWheelMaxLateralImpulse);
            _wheels[1] = wheel;
            _backRightJoint = CreateJoint(_body, wheel.Body, wheelOffsetPosition, world);

            //front left wheel
            wheelOffsetPosition = new Vector2(halfWidth, frontWheelOffset).ToMeters();
            wheel = new Wheel(world);
            wheel.SetCharacteristics(maxForwardSpeed, maxBackwardSpeed, frontWheelMaxDriveForce, fronWheelMaxLateralImpulse);
            _wheels[2] = wheel;
            _frontLeftJoint = CreateJoint(_body, wheel.Body, wheelOffsetPosition, world);

            //front right wheel
            wheelOffsetPosition = new Vector2(-halfWidth, frontWheelOffset).ToMeters();
            wheel = new Wheel(world);
            wheel.SetCharacteristics(maxForwardSpeed, maxBackwardSpeed, frontWheelMaxDriveForce, fronWheelMaxLateralImpulse);
            _wheels[3] = wheel;
            _frontRightJoint = CreateJoint(_body, wheel.Body, wheelOffsetPosition, world);
        }

        private bool OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            if (Collided != null)
                Collided(this, EventArgs.Empty);
            return true;
        }

        private static RevoluteJoint CreateJoint(Body carBody, Body wheelBody, Vector2 anchor, World world)
        {
            var joint = JointFactory.CreateRevoluteJoint(carBody, wheelBody, Vector2.Zero);
            joint.LocalAnchorA = anchor;
            joint.LimitEnabled = true;
            joint.LowerLimit = 0;
            joint.UpperLimit = 0;
            world.AddJoint(joint);
            wheelBody.Position = carBody.Position + anchor;
            return joint;
        }

        public override void Update(ParticipantInput input, float elapsedTime)
        {
            //System.Diagnostics.Debug.WriteLine(_body.Position);
            var position = _body.Position.ToBlockUnits();
            Position3 = new Vector3(position.X, position.Y, Position3.Z);
            foreach (var wheel in _wheels)
            {
                wheel.Update(input.Forward, elapsedTime);
            }

            //control steering
            var lockAngle = MathHelper.ToRadians(angleLock);
            var turnSpeedPerSec = MathHelper.ToRadians((angleLock*2)/(lockToLockTime/1000));
            var turnPerTimeSpep = turnSpeedPerSec*elapsedTime;
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

        public override Frame Draw()
        {
            throw new NotImplementedException();
        }

        public override void Update(float elapsedTime)
        {
            throw new NotImplementedException();
        }
    }
}
