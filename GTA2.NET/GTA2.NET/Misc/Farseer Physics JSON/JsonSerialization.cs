// The MIT License (MIT)

// Copyright (c) 2013 Hiale

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;

// This classes implement this JSON structure: https://www.iforce2d.net/rube/json-structure
// Only some properties are implemented.
namespace Hiale.FarseerPhysicsJSON
{
    public static class JsonWorldSerialization
    {
        public static void Serialize(World world, string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Create))
            {
                new WorldJsonSerializer().Serialize(world, fs);
            }
        }

        public static void Deserialize(World world, string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Open))
            {
                world = new WorldJsonDeserializer().Deserialize(fs);
            }
        }

        public static World Deserialize(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Open))
            {
                return new WorldJsonDeserializer().Deserialize(fs);
            }
        }
    }

    public class WorldJsonSerializer
    {
        public void Serialize(World world, Stream stream)
        {
            var jsonWorld = new JObject();
            jsonWorld.Add("gravity", (JToken)ToJsonObject(world.Gravity));
            jsonWorld.Add("autoClearForces", world.AutoClearForces);
            jsonWorld.Add("enableSubStepping", world.EnableSubStepping);
            var bodiesArray = new JArray();
            foreach ( var body in world.BodyList)
            {
                bodiesArray.Add(SerializeBody(body));
            }
            jsonWorld.Add(new JProperty("body", bodiesArray));

            using (var writer = new StreamWriter(stream))
            {
                writer.Write(jsonWorld.ToString());
            }
        }

        private static JObject SerializeBody(Body body)
        {
            var jsonBody = new JObject();
            //jsonBody.Add(new JProperty("name", "[unknown]"));
            jsonBody.Add(new JProperty("type", ToNumericType(body.BodyType)));
            jsonBody.Add(new JProperty("angle", FloatToHex(body.Rotation)));

            jsonBody.Add(new JProperty("angularDamping", FloatToHex(body.AngularDamping)));
            jsonBody.Add(new JProperty("angularVelocity", FloatToHex(body.AngularVelocity)));
            jsonBody.Add(new JProperty("awake", body.Awake));
            jsonBody.Add(new JProperty("bullet", body.IsBullet));
            jsonBody.Add(new JProperty("fixedRotation", body.FixedRotation));
            jsonBody.Add(new JProperty("linearDamping", FloatToHex(body.LinearDamping)));
            jsonBody.Add(new JProperty("linearVelocity", ToJsonObject(body.LinearVelocity)));

            jsonBody.Add(new JProperty("massData-mass", FloatToHex(body.Mass)));
            jsonBody.Add(new JProperty("massData-center", ToJsonObject(body.LocalCenter)));
            jsonBody.Add(new JProperty("massData-I", FloatToHex(body.Inertia)));
            jsonBody.Add(new JProperty("position", ToJsonObject(body.Position)));

            if (body.FixtureList.Count > 0)
            {
                var jsonFixtureArray = new JArray();
                foreach (var fixture in body.FixtureList)
                {
                    jsonFixtureArray.Add(SerializeFixture(fixture));
                }
                jsonBody.Add(new JProperty("fixture", jsonFixtureArray));
            }

            return jsonBody;
        }

        private static JObject SerializeFixture(Fixture fixture)
        {
            var jsonFixture = new JObject();

            //jsonFixture.Add(new JProperty("name", "[unknown"));
            jsonFixture.Add(new JProperty("density", FloatToHex(fixture.Shape.Density)));
            jsonFixture.Add(new JProperty("friction", FloatToHex(fixture.Friction)));
            jsonFixture.Add(new JProperty("restitution", FloatToHex(fixture.Restitution)));
            jsonFixture.Add(new JProperty("sensor", fixture.IsSensor));

            switch (fixture.ShapeType)
            {
                case ShapeType.Circle:
                    jsonFixture.Add(new JProperty("circle", SerializeCircleShape((CircleShape) fixture.Shape)));
                    break;
                case ShapeType.Polygon:
                    jsonFixture.Add(new JProperty("polygon", SerializePolygonShape((PolygonShape) fixture.Shape)));
                    break;
                case ShapeType.Loop:
                    jsonFixture.Add(new JProperty("chain", SerializeLoopShape((LoopShape) fixture.Shape)));
                    break;
                case ShapeType.Edge:
                    jsonFixture.Add(new JProperty("chain", SerializeEdgeShape((EdgeShape) fixture.Shape)));
                    break;
                default:
                    throw new NotSupportedException();
            }
            return jsonFixture;
        }

        private static JObject SerializeCircleShape(CircleShape shape)
        {
            return new JObject(new JProperty("center", ToJsonObject(shape.Position)), new JProperty("radius", FloatToHex(shape.Radius)));
        }

        private static JObject SerializePolygonShape(PolygonShape shape)
        {
            return new JObject(new JProperty("vertices", ToJsonObject(shape.Vertices)));
        }

        private static JObject SerializeLoopShape(LoopShape shape)
        {
            var jsonLoopShape = new JObject();
            jsonLoopShape.Add(new JProperty("vertices", ToJsonObject(shape.Vertices)));
            jsonLoopShape.Add(new JProperty("hasNextVertex", true));
            jsonLoopShape.Add(new JProperty("hasPrevVertex", true));
            jsonLoopShape.Add(new JProperty("nextVertex", ToJsonObject(shape.Vertices[1])));
            jsonLoopShape.Add(new JProperty("prevVertex", ToJsonObject(shape.Vertices[shape.Vertices.Count - 2])));
            return jsonLoopShape;
        }

        private static JObject SerializeEdgeShape(EdgeShape shape)
        {
            throw new NotImplementedException();
        }

        private void SerializeJoint(Joint joint)
        {
            
        }

        private static int ToNumericType(BodyType type)
        {
            switch (type)
            {
                case BodyType.Static:
                    return 0;
                case BodyType.Kinematic:
                    return 1;
                case BodyType.Dynamic:
                    return 2;
            }
            return 2;
        }

        private static object ToJsonObject(Vector2 value)
        {
            if (value == Vector2.Zero)
                return new JValue(0);
            return new JObject(new JProperty("x", FloatToHex(value.X)), new JProperty("y", FloatToHex(value.Y)));
        }

        private static JObject ToJsonObject(IEnumerable<Vector2> vertices)
        {
            var xArray = new JArray();
            var yArray = new JArray();
            foreach (var vector2 in vertices)
            {
                xArray.Add(FloatToHex(vector2.X));
                yArray.Add(FloatToHex(vector2.Y));
            }
            return new JObject(new JProperty("x", xArray), new JProperty("y", yArray));

        }

        private static object FloatToHex(float value)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (value == 0)
                return 0;
            // ReSharper restore CompareOfFloatsByEqualityOperator
            var bytes = BitConverter.GetBytes(value);
            return BitConverter.ToString(bytes).Replace("-", "");
        }
    }

    public class WorldJsonDeserializer
    {
        private readonly Dictionary<string, IList<Body>> _namedBodies;
        private readonly Dictionary<string, IList<Fixture>> _namedFixtures; 
        //private List<Body> _bodies = new List<Body>();
        //private List<Fixture> _fixtures = new List<Fixture>();
        //private List<Joint> _joints = new List<Joint>();
        //private List<Shape> _shapes = new List<Shape>();

        public WorldJsonDeserializer()
        {
            _namedBodies = new Dictionary<string, IList<Body>>();
            _namedFixtures = new Dictionary<string, IList<Fixture>>();
        }

        public World Deserialize(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Open))
            {
                return Deserialize(fs);
            }
        }

        public World Deserialize(Stream stream)
        {
            var world = new World(Vector2.Zero);
            Deserialize(world, stream);
            return world;
        }

        public IList<Body> GetBodiesByName(string name)
        {
            return _namedBodies.Any(namedBody => namedBody.Key == name) ? _namedBodies[name] : new List<Body>();
        }

        public IList<Fixture> GetFixturesByName(string name)
        {
            return _namedFixtures.Any(namedFixture => namedFixture.Key == name) ? _namedFixtures[name] : new List<Fixture>();
        }

        public void Deserialize(World world, Stream stream)
        {
            world.Clear();
            _namedBodies.Clear();
            _namedFixtures.Clear();

            string content;

            using (var reader = new StreamReader(stream))
                content = RemoveComments(reader.ReadToEnd());

            var jsonWorld = JObject.Parse(content);

            foreach (JProperty worldProperty in jsonWorld.Children())
            {
                //World properties
                switch (worldProperty.Name)
                {
                    case "gravity":
                        world.Gravity = ParseVector2(worldProperty);
                        break;
                    //case "allowSleep":
                    //    break;
                    case "autoClearForces":
                        world.AutoClearForces = (bool) worldProperty.Value;
                        break;
                    //case "positionIterations":
                    //    break;
                    //case "velocityIterations":
                    //    break;
                    //case "stepsPerSecond":
                    //    break;
                    //case "warmStarting":
                    //    break;
                    //case "continuousPhysics":
                    //    break;
                    case "subStepping":
                        world.EnableSubStepping = (bool)worldProperty.Value;
                        break;
                    case "body":
                        break; //ignore
                    default:
                        System.Diagnostics.Debug.WriteLine(worldProperty.Name + " not supported");
                        break;
                }
            }

            var bodies = (JArray)jsonWorld["body"];
            var images = (JArray)jsonWorld["image"];
            var joints = (JArray)jsonWorld["joint"];

            if (bodies != null)
            {
                foreach (JObject jsonBody in bodies)
                    ParseBody(jsonBody, world);
            }
            if (images != null)
            {
                throw new NotImplementedException();
            }
            if (joints != null)
            {
                throw new NotImplementedException();
            }
        }

        private void ParseBody(JObject jsonBody, World world)
        {
            var body = new Body(world);
            foreach (JProperty bodyProperty in jsonBody.Children())
            {
                //Body properties
                switch (bodyProperty.Name)
                {
                    case "name":
                        var value = bodyProperty.Value.ToString();
                        if (_namedBodies.ContainsKey(value))
                            _namedBodies[value].Add(body);
                        else
                        {
                            var list = new List<Body> {body};
                            _namedBodies.Add(value, list);
                        }
                        break;
                    case "type":
                        body.BodyType = ParseType(bodyProperty);
                        break;
                    case "angle":
                        body.Rotation = HexToFloat(bodyProperty.Value.ToString());
                        break;
                    case "angularDamping":
                        body.AngularDamping = HexToFloat(bodyProperty.Value.ToString());
                        break;
                    case "angularVelocity":
                        body.AngularVelocity = HexToFloat(bodyProperty.Value.ToString());
                        break;
                    case "awake":
                        body.Awake = (bool) bodyProperty.Value;
                        break;
                    case "bullet":
                        body.IsBullet = (bool) bodyProperty.Value;
                        break;
                    case "fixedRotation":
                        body.FixedRotation = (bool) bodyProperty.Value;
                        break;
                    case "linearDamping":
                        body.LinearDamping = HexToFloat(bodyProperty.Value.ToString());
                        break;
                    case "linearVelocity":
                        body.LinearVelocity = ParseVector2(bodyProperty);
                        break;
                    case "massData-mass":
                        body.Mass = HexToFloat(bodyProperty.Value.ToString());
                        break;
                    case "massData-center":
                        body.LocalCenter = ParseVector2(bodyProperty);
                        break;
                    case "massData-I":
                        body.Inertia = HexToFloat(bodyProperty.Value.ToString());
                        break;
                    case "position":
                        body.Position = ParseVector2(bodyProperty);
                        break;
                    case "fixture":
                        break; //ignore
                    default:
                        System.Diagnostics.Debug.WriteLine(bodyProperty.Name + " not supported.");
                        break;
                }
            }

            var fixtures = (JArray)jsonBody["fixture"];
            if (fixtures == null)
                return;
            foreach (JObject fixture in fixtures)
                ParseFixture(fixture, body);
        }

        private void ParseFixture(JObject jsonFixture, Body body)
        {
            Shape shape = null;

            var circles = (JObject)jsonFixture["circle"];
            var polygons = (JObject)jsonFixture["polygon"];
            var chains = (JObject)jsonFixture["chain"];

            if (circles != null)
            {
                var center = Vector2.Zero;
                float radius = 0;
                foreach (JProperty circleProperty in circles.Children())
                {
                    switch (circleProperty.Name)
                    {
                        case "center":
                            center = ParseVector2(circleProperty);
                            break;
                        case "radius":
                            radius = HexToFloat(circleProperty.Value.ToString());
                            break;
                    }
                }
                shape = new CircleShape(radius, 1);
                var circleShape = (CircleShape) shape;
                circleShape.Position = center;
            }
            else if (polygons != null)
            {
                Vertices vertices = null;
                foreach (var polygonProperty in polygons.Children().Cast<JProperty>().Where(polygonProperty => polygonProperty.Name == "vertices"))
                    vertices = new Vertices(ParseVector2Array(polygonProperty));
                if (vertices != null)
                    shape = new PolygonShape(vertices, 1);
            }
            else if (chains != null)
            {
                //shape = new 
                Vertices vertices = null;
                bool isLoopShape = false;
                bool hasNextVertex;
                bool hasPrevVertex;
                Vector2 nextVertex = Vector2.Zero;
                Vector2 prevVertex = Vector2.Zero;

                foreach (JProperty chainProperty in chains.Children())
                {
                    switch (chainProperty.Name)
                    {
                        case "vertices":
                            vertices = new Vertices(ParseVector2Array(chainProperty));
                            break;
                        case "hasNextVertex":
                            isLoopShape = true;
                            hasNextVertex = (bool) chainProperty.Value;
                            break;
                        case "hasPrevVertex":
                            hasPrevVertex = (bool) chainProperty.Value;
                            break;
                        case "nextVertex":
                            nextVertex = ParseVector2(chainProperty);
                            break;
                        case "prevVertex":
                            prevVertex = ParseVector2(chainProperty);
                            break;
                        default:
                            System.Diagnostics.Debug.WriteLine(chainProperty.Name + " not supported!");
                            break;
                    }
                }

                if (isLoopShape)
                {
                    var lastvertexIndex = vertices.Count - 1;
                    if (vertices[0] == vertices[lastvertexIndex])
                        vertices.RemoveAt(lastvertexIndex);
                    shape = new LoopShape(vertices);
                    //var loopShape = (LoopShape) shape;
                }
                else
                {
                    throw new NotImplementedException();
                    //shape = new EdgeShape(prevVertex, nextVertex);
                    //var edgeShape = (EdgeShape) shape;
                }
            }

            var fixture = body.CreateFixture(shape);

            foreach (JProperty fixtureProperty in jsonFixture.Children())
            {
                //Fixture properties
                switch (fixtureProperty.Name)
                {
                    case "name":
                        var value = fixtureProperty.Value.ToString();
                        if (_namedFixtures.ContainsKey(value))
                            _namedFixtures[value].Add(fixture);
                        else
                        {
                            var list = new List<Fixture> {fixture};
                            _namedFixtures.Add(value, list);
                        }
                        break;
                    case "density":
                        shape.Density = HexToFloat(fixtureProperty.Value.ToString());
                        break;
                    case "friction":
                        fixture.Friction = HexToFloat(fixtureProperty.Value.ToString());
                        break;
                    case "restitution":
                        fixture.Restitution = HexToFloat(fixtureProperty.Value.ToString());
                        break;
                    case "sensor":
                        fixture.IsSensor = (bool) fixtureProperty.Value;
                        break;
                    case "circle":
                    case "polygon":
                    case "chain":
                    //case "filter-categoryBits":
                    //case "filter-maskBits":
                    //case "filter-groupIndex":
                        break; //ignore
                    default:
                        System.Diagnostics.Debug.WriteLine(fixtureProperty.Name + " not supported!");
                        break;
                }
            }
        }

        public string RemoveComments(string input)
        {
            var regex = new Regex(" *//.*$", RegexOptions.Multiline);
            return regex.Replace(input, string.Empty);
        }

        private static BodyType ParseType(JProperty token)
        {
            var value = (int)token.Value;
            switch (value)
            {
                case 0:
                    return BodyType.Static;
                case 1:
                    return BodyType.Kinematic;
                case 2:
                    return BodyType.Dynamic;
            }
            return BodyType.Dynamic;
        }

        private static Vector2 ParseVector2(JProperty token)
        {
            float x = 0;
            float y = 0;
            var isX = true;
            foreach (JProperty vectorProperty in token.Value.Children())
            {
                if (isX)
                    x = HexToFloat(vectorProperty.Value.ToString());
                else
                    y = HexToFloat(vectorProperty.Value.ToString());
                isX = false;
            }
            return new Vector2(x, y);
        }

        private static IList<Vector2> ParseVector2Array(JProperty token)
        {
            var xValues = new List<string>();
            var yValues = new List<string>();
            var isX = true;
            foreach (JProperty vectorProperty in token.Value.Children())
            {
                var currentList = isX ? xValues : yValues;
                currentList.AddRange(from JValue variable in vectorProperty.Value.Children() select variable.Value.ToString());
                isX = false;
            }
            return xValues.Select((t, i) => new Vector2(HexToFloat(t), HexToFloat(yValues[i]))).ToList();
        }

        private static float HexToFloat(string hexValue)
        {
            var bytes = BitConverter.GetBytes(int.Parse(hexValue, NumberStyles.HexNumber));
            return BitConverter.ToSingle(bytes, 0);
        }
        
    }

}
