//Created 18.09.2010
//23.02.2013 - Old version was crap

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Hiale.GTA2NET.Core.Map;
using Microsoft.Xna.Framework;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;

namespace Hiale.GTA2NET.Core.Collision
{
    /// <summary>
    /// Represents unpassable space
    /// </summary>
    public class MapCollision
    {
        private readonly Map.Map _map;

        public MapCollision(Map.Map map)
        {
            _map = map;
        }

        public CollisionMapType[,,] FloodFill(Vector2 start)
        {
            return FloodFill(start, false);
        }

        public CollisionMapType[,,] FloodFill(Vector2 start, bool invert)
        {
            var blocks = new CollisionMapType[_map.Width, _map.Length, _map.Height];
            for (var z = _map.Height - 1; z >= 0; z--)
            {
                var stack = new Stack<Vector2>();
                stack.Push(start);
                do
                {
                    var currentPos = stack.Pop();
                    var currentBlock = _map.CityBlocks[(int) currentPos.X, (int) currentPos.Y, z];
                    if (CheckBlockBounds(currentPos))
                    {
                        switch (currentBlock.SlopeType)
                        {
                            case SlopeType.DiagonalFacingUpLeft:
                            case SlopeType.DiagonalFacingUpRight:
                            case SlopeType.DiagonalFacingDownLeft:
                            case SlopeType.DiagonalFacingDownRight:
                            case SlopeType.PartialCentreBlock:
                                blocks[(int) currentPos.X, (int) currentPos.Y, z] = CollisionMapType.Special;
                                continue;
                        }
                        if (currentBlock.IsEmpty)
                            blocks[(int) currentPos.X, (int) currentPos.Y, z] = (invert ? CollisionMapType.Block : CollisionMapType.Free);
                        else
                            blocks[(int) currentPos.X, (int) currentPos.Y, z] = CollisionMapType.Unknwon;
                    }

                    var newPos = new Vector2(currentPos.X + 1, currentPos.Y); //right
                    if (CheckBlockBounds(newPos))
                    {
                        if (CheckNeighbor((int) newPos.X, (int) newPos.Y, z, blocks, BlockFaceDirection.Left, invert))
                            stack.Push(newPos);
                    }
                    newPos = new Vector2(currentPos.X, currentPos.Y + 1); //bottom
                    if (CheckBlockBounds(newPos))
                    {
                        if (CheckNeighbor((int) newPos.X, (int) newPos.Y, z, blocks, BlockFaceDirection.Top, invert))
                            stack.Push(newPos);
                    }
                    newPos = new Vector2(currentPos.X - 1, currentPos.Y); //left
                    if (CheckBlockBounds(newPos))
                    {
                        if (CheckNeighbor((int) newPos.X, (int) newPos.Y, z, blocks, BlockFaceDirection.Right, invert))
                            stack.Push(newPos);
                    }
                    newPos = new Vector2(currentPos.X, currentPos.Y - 1); //top
                    if (CheckBlockBounds(newPos))
                    {
                        if (CheckNeighbor((int) newPos.X, (int) newPos.Y, z, blocks, BlockFaceDirection.Bottom, invert))
                            stack.Push(newPos);
                    }
                } while (stack.Count > 0);
            }

            //Pass 2
            RemoveUnknownBlocks(blocks);

            //Pass 3
            FindLineObstacles(blocks);

            return blocks;
        }

        private void RemoveUnknownBlocks(CollisionMapType[,,] blocks)
        {
            for (var z = _map.Height - 1; z >= 0; z--)
            {
                for (var x = 0; x < _map.Width; x++)
                {
                    for (var y = 0; y < _map.Length; y++)
                    {
                        //remove Unknown blocks
                        if (blocks[x, y, z] == CollisionMapType.Unknwon)
                            blocks[x, y, z] = CollisionMapType.Block;
                        //ToDo, well, Unchecked (None) blocks could actually be possible, if you fall from a block above, but I don't think that happens in the original maps...
                        //So let's mark them 'Block'
                        if (blocks[x, y, z] == CollisionMapType.None)
                            blocks[x, y, z] = CollisionMapType.Block;
                    }
                }
            }
        }

        private void FindLineObstacles(CollisionMapType[,,] blocks)
        {
            var lines = new List<LineObstacle>();

            //we check all 'Blocked blocks' which are 1 block wide, maybe they are not all blocked, but only a line is blocked for example a fence.
            for (var z = _map.Height - 1; z >= 0; z--)
            {
                for (var x = 0; x < _map.Width; x++)
                {
                    for (var y = 0; y < _map.Length; y++)
                    {
                        if (blocks[x, y, z] == CollisionMapType.Block)
                        {
                            if (_map.CityBlocks[x, y, z].Left && !_map.CityBlocks[x, y, z].Right) //left
                            {
                                if ((x + 1) < _map.Width && blocks[x + 1, y, z] == CollisionMapType.Free)
                                {
                                    blocks[x, y, z] = CollisionMapType.Free;
                                    lines.Add(new LineObstacle(new Vector2(x, y), new Vector2(x, y + 1), LineObstacleType.Vertical));
                                }
                            }

                            if (_map.CityBlocks[x, y, z].Right && !_map.CityBlocks[x, y, z].Left) //right
                            {
                                if ((x - 1) < _map.Width && blocks[x - 1, y, z] == CollisionMapType.Free)
                                {
                                    blocks[x, y, z] = CollisionMapType.Free;
                                    lines.Add(new LineObstacle(new Vector2(x + 1, y), new Vector2(x + 1, y + 1), LineObstacleType.Vertical));
                                }
                            }

                            if (_map.CityBlocks[x, y, z].Top && !_map.CityBlocks[x, y, z].Bottom) //top
                            {
                                if ((y + 1) < _map.Length && blocks[x, y + 1, z] == CollisionMapType.Free)
                                {
                                    blocks[x, y, z] = CollisionMapType.Free;
                                    lines.Add(new LineObstacle(new Vector2(x, y), new Vector2(x + 1, y), LineObstacleType.Horizontal));
                                }
                            }

                            if (_map.CityBlocks[x, y, z].Bottom && !_map.CityBlocks[x, y, z].Top) //bottom
                            {
                                if ((y - 1) < _map.Length && blocks[x, y - 1, z] == CollisionMapType.Free)
                                {
                                    blocks[x, y, z] = CollisionMapType.Free;
                                    lines.Add(new LineObstacle(new Vector2(x, y + 1), new Vector2(x + 1, y + 1), LineObstacleType.Horizontal));
                                }
                            }
                        }

                    }
                }
            }
        }

        private bool CheckNeighbor(int x, int y, int z, CollisionMapType[,,] blocks, BlockFaceDirection direction, bool invert)
        {
            if (blocks[x, y,z] == CollisionMapType.None)
            {
                var newBlock = _map.CityBlocks[x, y, z];
                if (newBlock.IsEmpty)
                    blocks[x, y, z] = (invert ? CollisionMapType.Block : CollisionMapType.Free);
                if (newBlock.SlopeType != SlopeType.None && newBlock.SlopeType != SlopeType.SlopeAbove)
                {
                    blocks[x, y,z] = CollisionMapType.Special;
                    return false;
                }
                switch (direction)
                {
                    case BlockFaceDirection.Left:
                        if (!newBlock.Left.Wall)
                            return true;
                        break;
                    case BlockFaceDirection.Right:
                        if (!newBlock.Right.Wall)
                            return true;
                        break;
                    case BlockFaceDirection.Top:
                        if (!newBlock.Top.Wall)
                            return true;
                        break;
                    case BlockFaceDirection.Bottom:
                        if (!newBlock.Bottom.Wall)
                            return true;
                        break;
                }
                blocks[x, y, z] = (invert ? CollisionMapType.Free : CollisionMapType.Block);
            }
            else if (blocks[x, y,z] == CollisionMapType.Unknwon)
            {
                blocks[x, y,z] = UnknwonBlocks(x, y, z, invert);
            }
            return false;
        }

        private CollisionMapType UnknwonBlocks(int x, int y, int z, bool invert)
        {
            var newBlock = _map.CityBlocks[x, y, z];
            if (newBlock.Left)
            {
                if (CheckBlockBounds(new Vector2(x - 1, y)))
                {
                    if (_map.CityBlocks[x - 1, y, z].Right)
                        return (invert ? CollisionMapType.Block : CollisionMapType.Free);
                }
            }
            if (newBlock.Top)
            {
                if (CheckBlockBounds(new Vector2(x, y - 1)))
                {
                    if (_map.CityBlocks[x, y - 1, z].Bottom)
                        return (invert ? CollisionMapType.Block : CollisionMapType.Free);
                }
            }
            if (newBlock.Right)
            {
                if (CheckBlockBounds(new Vector2(x + 1, y)))
                {
                    if (_map.CityBlocks[x + 1, y, z].Left)
                        return (invert ? CollisionMapType.Block : CollisionMapType.Free);
                }
            }
            if (newBlock.Bottom)
            {
                if (CheckBlockBounds(new Vector2(x, y + 1)))
                {
                    if (_map.CityBlocks[x, y + 1, z].Top)
                        return (invert ? CollisionMapType.Block : CollisionMapType.Free);
                }
            }
            return (invert ? CollisionMapType.Free : CollisionMapType.Block);;
        }
        
        private bool CheckBlockBounds(Vector2 newPos)
        {
            return (newPos.X > -1) && (newPos.Y > -1) && (newPos.X < _map.Width) && (newPos.Y < _map.Length);
        }

        //public List<Obstacle>[] CreateMapVertices()
        //{
        //    //we create a list of unpassable obsticles of each layer (z coord)
        //    var obstacles = new List<Obstacle>[_map.CityBlocks.GetLength(2)];
        //    var straightObstacles = new List<Obstacle>[_map.CityBlocks.GetLength(2)];
        //    for (var i = 0; i < obstacles.Length; i++)
        //    {
        //        obstacles[i] = new List<Obstacle>();
        //        straightObstacles[i] = new List<Obstacle>();
        //    }

        //    for (var z = 0; z < _map.CityBlocks.GetLength(2); z++)
        //    {
        //        for (var x = 0; x < _map.CityBlocks.GetLength(0); x++)
        //        {
        //            for (var y = 0; y < _map.CityBlocks.GetLength(1); y++)
        //            {
        //                var block = _map.CityBlocks[x, y, z];
        //                if (block.IsEmpty)
        //                    continue;
        //                if (block.Left && block.Left.Wall)
        //                {
        //                    switch (block.SlopeType)
        //                    {
        //                        case SlopeType.DiagonalFacingUpLeft:
        //                        case SlopeType.DiagonalSlopeFacingUpLeft:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x, y + 1), new Vector2(x + 1, y), ObstacleType.Other));
        //                            break;
        //                        case SlopeType.DiagonalFacingDownLeft:
        //                        case SlopeType.DiagonalSlopeFacingDownLeft:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x, y), new Vector2(x + 1, y + 1), ObstacleType.Other));
        //                            break;
        //                        case SlopeType.PartialBlockRight:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x + 1 - BlockInfo.PartialBlockScalar, y), new Vector2(x + 1, y), ObstacleType.Vertical));
        //                            break;
        //                        case SlopeType.PartialBlockTop:
        //                        case SlopeType.PartialBlockTopLeft:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x, y), new Vector2(x, y + BlockInfo.PartialBlockScalar), ObstacleType.Vertical));
        //                            break;
        //                        case SlopeType.PartialBlockBottom:
        //                        case SlopeType.PartialBlockBottomLeft:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x, y + 1 - BlockInfo.PartialBlockScalar), new Vector2(x, y + 1), ObstacleType.Vertical));
        //                            break;
        //                        case SlopeType.PartialBlockTopRight:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x + 1 - BlockInfo.PartialBlockScalar, y), new Vector2(x + 1 - BlockInfo.PartialBlockScalar, y + BlockInfo.PartialBlockScalar), ObstacleType.Vertical));
        //                            break;
        //                        case SlopeType.PartialBlockBottomRight:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x + 1 - BlockInfo.PartialBlockScalar, y + 1 - BlockInfo.PartialBlockScalar), new Vector2(x + 1 - BlockInfo.PartialBlockScalar, y + 1), ObstacleType.Vertical));
        //                            break;
        //                        default:
        //                            straightObstacles[z - 1].Add(new Obstacle(new Vector2(x, y), new Vector2(x, y + 1), ObstacleType.Vertical));
        //                            break;
        //                    }
        //                }
        //                if (block.Top && block.Top.Wall)
        //                {
        //                    switch (block.SlopeType)
        //                    {
        //                        case SlopeType.PartialBlockLeft:
        //                        case SlopeType.PartialBlockTopLeft:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x, y), new Vector2(x + BlockInfo.PartialBlockScalar, y), ObstacleType.Horizontal));
        //                            break;
        //                        case SlopeType.PartialBlockRight:
        //                        case SlopeType.PartialBlockTopRight:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x + 1 - BlockInfo.PartialBlockScalar, y), new Vector2(x + 1, y), ObstacleType.Horizontal));
        //                            break;
        //                        case SlopeType.PartialBlockBottom:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x, y + 1 - BlockInfo.PartialBlockScalar), new Vector2(x + 1, y + 1 - BlockInfo.PartialBlockScalar), ObstacleType.Horizontal));
        //                            break;
        //                        case SlopeType.PartialBlockBottomRight:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x + 1 - BlockInfo.PartialBlockScalar, y + 1 - BlockInfo.PartialBlockScalar), new Vector2(x + 1, y + 1 - BlockInfo.PartialBlockScalar), ObstacleType.Horizontal));
        //                            break;
        //                        case SlopeType.PartialBlockBottomLeft:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x, y + 1 - BlockInfo.PartialBlockScalar), new Vector2(x + BlockInfo.PartialBlockScalar, y + 1 - BlockInfo.PartialBlockScalar), ObstacleType.Horizontal));
        //                            break;
        //                        default:
        //                            straightObstacles[z - 1].Add(new Obstacle(new Vector2(x, y), new Vector2(x + 1, y), ObstacleType.Horizontal));
        //                            break;
        //                    }
        //                }
        //                if (block.Right && block.Right.Wall)
        //                {
        //                    switch (block.SlopeType)
        //                    {
        //                        case SlopeType.DiagonalFacingUpRight:
        //                        case SlopeType.DiagonalSlopeFacingUpRight:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x, y), new Vector2(x + 1, y + 1), ObstacleType.Other));
        //                            break;
        //                        case SlopeType.DiagonalFacingDownRight:
        //                        case SlopeType.DiagonalSlopeFacingDownRight:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x, y + 1), new Vector2(x + 1, y), ObstacleType.Other));
        //                            break;
        //                        case SlopeType.PartialBlockLeft:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x + BlockInfo.PartialBlockScalar, y), new Vector2(x + BlockInfo.PartialBlockScalar, y + 1), ObstacleType.Vertical));
        //                            break;
        //                        case SlopeType.PartialBlockTop:
        //                        case SlopeType.PartialBlockTopRight:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x + 1, y), new Vector2(x + 1, y + BlockInfo.PartialBlockScalar), ObstacleType.Vertical));
        //                            break;
        //                        case SlopeType.PartialBlockBottom:
        //                        case SlopeType.PartialBlockBottomRight:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x + 1, y + 1 - BlockInfo.PartialBlockScalar), new Vector2(x + 1, y + 1), ObstacleType.Vertical));
        //                            break;
        //                        case SlopeType.PartialBlockTopLeft:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x + BlockInfo.PartialBlockScalar, y), new Vector2(x + BlockInfo.PartialBlockScalar, y + BlockInfo.PartialBlockScalar), ObstacleType.Vertical));
        //                            break;
        //                        case SlopeType.PartialBlockBottomLeft:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x + BlockInfo.PartialBlockScalar, y + 1 - BlockInfo.PartialBlockScalar), new Vector2(x + BlockInfo.PartialBlockScalar, y + 1), ObstacleType.Vertical));
        //                            break;
        //                        default:
        //                            straightObstacles[z - 1].Add(new Obstacle(new Vector2(x + 1, y), new Vector2(x + 1, y + 1), ObstacleType.Vertical));
        //                            break;
        //                    }
        //                }
        //                if (block.Bottom && block.Bottom.Wall)
        //                {
        //                    switch (block.SlopeType)
        //                    {
        //                        case SlopeType.PartialBlockLeft:
        //                        case SlopeType.PartialBlockBottomLeft:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x, y + 1), new Vector2(x + BlockInfo.PartialBlockScalar, y + 1), ObstacleType.Horizontal));
        //                            break;
        //                        case SlopeType.PartialBlockRight:
        //                        case SlopeType.PartialBlockBottomRight:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x + 1 - BlockInfo.PartialBlockScalar, y + 1), new Vector2(x + 1, y + 1), ObstacleType.Horizontal));
        //                            break;
        //                        case SlopeType.PartialBlockTop:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x, y + BlockInfo.PartialBlockScalar), new Vector2(x + 1, y + BlockInfo.PartialBlockScalar), ObstacleType.Horizontal));
        //                            break;
        //                        case SlopeType.PartialBlockTopLeft:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x, y + BlockInfo.PartialBlockScalar), new Vector2(x + BlockInfo.PartialBlockScalar, y + BlockInfo.PartialBlockScalar), ObstacleType.Horizontal));
        //                            break;
        //                        case SlopeType.PartialBlockTopRight:
        //                            obstacles[z - 1].Add(new Obstacle(new Vector2(x + 1 - BlockInfo.PartialBlockScalar, y + BlockInfo.PartialBlockScalar), new Vector2(x + 1, y + BlockInfo.PartialBlockScalar), ObstacleType.Horizontal));
        //                            break;
        //                        default:
        //                            straightObstacles[z - 1].Add(new Obstacle(new Vector2(x, y + 1), new Vector2(x + 1, y + 1), ObstacleType.Horizontal));
        //                            break;
        //                    }
        //                }
        //            }

        //        }
        //    }
        //    OptimizeStraightVertices(straightObstacles, obstacles);

        //    //for Debug
        //    for (var z = 0; z < obstacles.Length; z++)
        //    {
        //        using (var bmp = new Bitmap(2560, 2560))
        //        {
        //            using (var g = Graphics.FromImage(bmp))
        //            {
        //                g.Clear(Color.White);
        //                foreach (var obstacle in obstacles[z])
        //                {
        //                    g.DrawLine(new Pen(Color.Red), new Point((int)obstacle.Start.X * 10, (int)obstacle.Start.Y * 10), new Point((int)obstacle.End.X * 10, (int)obstacle.End.Y * 10));
        //                }
        //                bmp.Save(z + ".png", ImageFormat.Png);
        //            }
        //        }
        //    }
        //    return obstacles;
        //}

        ///// <summary> 
        ///// Combines straight obstacles to optimize collision detection.
        ///// </summary>
        ///// <param name="straightObstacles"></param>
        ///// <param name="obstacles"></param>
        //private void OptimizeStraightVertices(List<Obstacle>[] straightObstacles, List<Obstacle>[] obstacles)
        //{
        //    //var optimizedObstacles = new List<Obstacle>[straightObstacles.Length];
        //    for (var z = 0; z < straightObstacles.Length; z++)
        //    {
        //        var obstaclesHorizontal = new bool[256, 256 + 1];
        //        var obstaclesVertical = new bool[256 + 1, 256];
        //        foreach (var straightObstacle in straightObstacles[z])
        //        {
        //            if (straightObstacle.Type == ObstacleType.Horizontal)
        //                obstaclesHorizontal[(int) straightObstacle.Start.X, (int) straightObstacle.Start.Y] = true;
        //            else if (straightObstacle.Type == ObstacleType.Vertical)
        //                obstaclesVertical[(int)straightObstacle.Start.X, (int)straightObstacle.Start.Y] = true;
        //        }

        //        //Horizontal
        //        for (var y = 0; y < obstaclesHorizontal.GetLength(1); y++)
        //        {
        //            var start = new Vector2();
        //            var open = false;
        //            for (var x = 0; x < obstaclesHorizontal.GetLength(0); x++)
        //            {
        //                if (!obstaclesHorizontal[x, y])
        //                {
        //                    if (open)
        //                    {
        //                        var end = new Vector2(x, y);
        //                        obstacles[z] .Add(new Obstacle(start, end, ObstacleType.Horizontal));
        //                        open = false;
        //                    }
        //                    continue;
        //                }
        //                if (open)
        //                    continue;
        //                open = true;
        //                start = new Vector2(x, y);
        //            }
        //        }

        //        //Vertical
        //        for (var x = 0; x < obstaclesVertical.GetLength(0); x++)
        //        {
        //            var start = new Vector2();
        //            var open = false;
        //            for (var y = 0; y < obstaclesVertical.GetLength(1); y++)
        //            {
        //                if (!obstaclesVertical[x, y])
        //                {
        //                    if (open)
        //                    {
        //                        var end = new Vector2(x, y);
        //                        obstacles[z].Add(new Obstacle(start, end, ObstacleType.Vertical));
        //                        open = false;
        //                    }
        //                    continue;
        //                }
        //                if (open)
        //                    continue;
        //                open = true;
        //                start = new Vector2(x, y);
        //            }
        //        }

        //    }
        //}
    }
}
