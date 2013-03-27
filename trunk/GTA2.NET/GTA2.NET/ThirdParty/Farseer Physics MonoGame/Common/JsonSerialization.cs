using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;

namespace FarseerPhysics.Common
{
    #region JsonWorldSerialization
    public static class JsonWorldSerialization
    {
        public static void Serialize(World world, string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create))
            {
                //new WorldXmlSerializer().Serialize(world, fs);
            }
        }

        public static void Deserialize(World world, string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                //new WorldXmlDeserializer().Deserialize(world, fs);
            }
        }

        public static World Deserialize(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                return new WorldJsonDeserializer().Deserialize(fs);
            }
        }
    }
    #endregion

    #region WorldJsonSerializer
    public class WorldJsonSerializer
    {
        private List<Body> _bodies = new List<Body>();
        private List<Fixture> _serializedFixtures = new List<Fixture>();
        private List<Shape> _serializedShapes = new List<Shape>();
    }
    #endregion

    public class WorldJsonDeserializer
    {
        private List<Body> _bodies = new List<Body>();
        private List<Fixture> _fixtures = new List<Fixture>();
        private List<Joint> _joints = new List<Joint>();
        private List<Shape> _shapes = new List<Shape>();

        public World Deserialize(Stream stream)
        {
            World world = new World(Vector2.Zero);
            Deserialize(world, stream);
            return world;
        }

        public void Deserialize(World world, Stream stream)
        {
            world.Clear();

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
                    case "allowSleep":
                        //world. = (bool)worldProperty.Value;
                        break;
                    case "autoClearForces":
                        world.AutoClearForces = (bool) worldProperty.Value;
                        break;
                    case "positionIterations":
                        break;
                    case "velocityIterations":
                        break;
                    case "stepsPerSecond":
                        break;
                    case "warmStarting":
                        break;
                    case "continuousPhysics":
                        break;
                    case "subStepping":
                        world.EnableSubStepping = (bool)worldProperty.Value;
                        break;
                }
            }

            JArray bodies = (JArray)jsonWorld["body"];
            JArray images = (JArray)jsonWorld["image"];
            JArray joints = (JArray)jsonWorld["joint"];

            if (bodies != null)
            {
                foreach (JObject jsonBody in bodies)
                    ParseBody(jsonBody, world);
            }
        }

        private static Body ParseBody(JObject jsonBody, World world)
        {
            var body = new Body(world);
            foreach (JProperty bodyProperty in jsonBody.Children())
            {
                //Body properties
                switch (bodyProperty.Name)
                {
                    case "name":
                        break;
                    case "type":
                        body.BodyType = ParseType(bodyProperty);
                        break;
                    case "angle":
                        body.Rotation = (float) bodyProperty.Value;
                        break;
                    case "angularDamping":
                        body.AngularDamping = (float) bodyProperty.Value;
                        break;
                    case "angularVelocity":
                        body.AngularVelocity = (float) bodyProperty.Value;
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
                        body.LinearDamping = (float) bodyProperty.Value;
                        break;
                    case "linearVelocity":
                        body.LinearVelocity = ParseVector2(bodyProperty);
                        break;
                    case "massData-mass":
                        body.Mass = (float) bodyProperty.Value;
                        break;
                    case "massData-center":
                        break;
                    case "massData-I":
                        break;
                    case "position":
                        body.Position = ParseVector2(bodyProperty);
                        break;
                }
            }
            var fixtures = (JArray)jsonBody["fixture"];
            if (fixtures == null)
                return body;
            foreach (JObject fixture in fixtures)
                ParseFixture(fixture, body);
            return body;
        }

        private static void ParseFixture(JObject jsonFixture, Body body)
        {
            Shape shape = null;
            var properties = new Dictionary<string, object>();

            var circles = (JObject)jsonFixture["circle"];
            var polygons = (JObject)jsonFixture["polygon"];
            var chains = (JObject)jsonFixture["chain"];

            if (circles != null)
            {
                shape = new CircleShape();
                var circleShape = (CircleShape) shape;
                foreach (JProperty circleProperty in circles.Children())
                {
                    switch (circleProperty.Name)
                    {
                        case "center":
                            circleShape.Position = ParseVector2(circleProperty);
                            break;
                        case "radius":
                            circleShape.Radius = (float) circleProperty.Value;
                            break;
                    }
                }
            }
            else if (polygons != null)
            {
                shape = new PolygonShape();
                var polygonShape = (PolygonShape) shape;
                foreach (JProperty polygonProperty in polygons.Children())
                {
                    if (polygonProperty.Name == "vertices")
                        polygonShape.Vertices = new Vertices(ParseVector2Array(polygonProperty));
                }
            }
            else if (chains != null)
            {
                //shape = new 
                Vertices vertices = null;
                bool isLoopShape = false;
                bool hasNextVertex;
                bool hasPrevVertex;
                Vector2 nextVertex;
                Vector2 prevVertex;

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
                            hasPrevVertex = (bool)chainProperty.Value;
                            break;
                        case "nextVertex":
                            nextVertex = ParseVector2(chainProperty);
                            break;
                        case "prevVertex":
                            prevVertex = ParseVector2(chainProperty);
                            break;
                    }
                }

                if (isLoopShape)
                {
                    shape = new LoopShape(vertices);
                    var loopShape = (LoopShape) shape;
                }
                else
                {
                    shape = new EdgeShape();
                    var edgeShape = (EdgeShape) shape;
                }
            }

            body.CreateFixture(shape);

            foreach (JProperty fixtureProperty in jsonFixture.Children())
            {
                //Fixture properties
                switch (fixtureProperty.Name)
                {
                    case "name":
                        break;
                    case "density":
                        shape.Density = (float) fixtureProperty.Value;
                        break;
                    case "filter-categoryBits":
                        break;
                    case "filter-maskBits":
                        break;
                    case "filter-groupIndex":
                        break;
                    case "friction":
                        body.Friction = HexToFloat(fixtureProperty.Value.ToString());
                        break;
                    case "restitution":
                        body.Restitution = HexToFloat(fixtureProperty.Value.ToString());
                        break;
                    case "sensor":
                        body.IsSensor = (bool) fixtureProperty.Value;
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
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes, 0);
        }
        
    }

}
