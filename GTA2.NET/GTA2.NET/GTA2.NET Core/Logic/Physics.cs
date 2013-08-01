// GTA2.NET
// 
// File: Physics.cs
// Created: 27.07.2013
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Hiale.FarseerPhysicsJSON;
using Hiale.GTA2NET.Core.Collision;
using Hiale.GTA2NET.Core.Helper;
using Microsoft.Xna.Framework;

namespace Hiale.GTA2NET.Core.Logic
{
    public class Physics
    {
        private World _world;
        private readonly WorldJsonSerializer _json = new WorldJsonSerializer(); //debug

        public Physics()
        {
            //
        }

        public void Initialize(Map.Map map)
        {
            var collision = new CollisionMap(map);
            if (_world == null)
                _world = new World(new Vector2(0, 0));
            else
                _world.Clear();

            var obstacles = collision.GetObstacles();
            var layer2Obstacles = obstacles.GetObstacles(2);
            foreach (var obstacle in layer2Obstacles)
            {
                var body = new Body(_world) { BodyType = BodyType.Static };
                _json.SetName(body, "Building" + obstacle.Z);
                Shape shape;
                Fixture fixture;
                switch (obstacle.Type)
                {
                    case ObstacleType.Line:
                        var lineObstacle = (LineObstacle)obstacle;
                        shape = new EdgeShape(lineObstacle.Start.ToMeters(), lineObstacle.End.ToMeters());
                        fixture = body.CreateFixture(shape);
                        _json.SetName(fixture, "Building" + obstacle.Z);
                        break;
                    case ObstacleType.Polygon:
                        var polygonObstacle = (PolygonObstacle)obstacle;
                        var convexPolygons = BayazitDecomposer.ConvexPartition(polygonObstacle.Vertices);
                        foreach (var convexPolygon in convexPolygons)
                        {
                            shape = new PolygonShape(convexPolygon.ToMeters(), 1);
                            fixture = body.CreateFixture(shape);
                            _json.SetName(fixture, "Building" + obstacle.Z);
                        }
                        break;
                    case ObstacleType.Rectangle:
                        var rectangleObstacle = (RectangleObstacle)obstacle;
                        shape = new PolygonShape(rectangleObstacle.Vertices.ToMeters(), 1);
                        fixture = body.CreateFixture(shape);
                        _json.SetName(fixture, "Building" + obstacle.Z);
                        break;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            _world.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f, (1f / 60f)));
        }

        public void AddObject(IPhysicsBehaviour gameplayObject)
        {
            gameplayObject.CreateBody(_world, -1, -1); //ToDo
        }

        public void AddObject(Car car)
        {
            var widthInBlockUnits = ConvertUnits.ToBlockUnits(car.CarInfo.Width);
            var heightInBlockUnits = ConvertUnits.ToBlockUnits(car.CarInfo.Height);
            car.CreateBody(_world, widthInBlockUnits, heightInBlockUnits);
        }

        public void Debug(Car car)
        {
            var carBody = (Body)Extensions.GetPrivateField(car, "_body");
            _json.SetName(carBody, "Car");
            _json.SetName(carBody.FixtureList[0], "CarCollision");
            _json.SetName(carBody.FixtureList[1], "CarSprite");
            var wheels = (Wheel[])Extensions.GetPrivateField(car, "_wheels");
            _json.SetName(wheels[0].Body, "WheelBackLeft");
            _json.SetName(wheels[0].Body.FixtureList[0], "WheelBackLeft");
            _json.SetName(wheels[1].Body, "WheelBackRight");
            _json.SetName(wheels[1].Body.FixtureList[0], "WheelBackRight");
            _json.SetName(wheels[2].Body, "WheelFrontLeft");
            _json.SetName(wheels[2].Body.FixtureList[0], "WheelFrontLeft");
            _json.SetName(wheels[3].Body, "WheelFrontRight");
            _json.SetName(wheels[3].Body.FixtureList[0], "WheelFrontRight");

            _json.SetName((Joint)Extensions.GetPrivateField(car, "_backLeftJoint"), "BackLeftJoint");
            _json.SetName((Joint)Extensions.GetPrivateField(car, "_backRightJoint"), "BackRightJoint");
            _json.SetName((Joint)Extensions.GetPrivateField(car, "_frontLeftJoint"), "FrontLeftJoint");
            _json.SetName((Joint)Extensions.GetPrivateField(car, "_frontRightJoint"), "FrontRightJoint");

            SaveWorld("GTA2NET.json");
        }

        public void SaveWorld(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Create))
            {
                _json.Serialize(_world, fs);
            }
        }
    }
}
