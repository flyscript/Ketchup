using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public struct Coord
    {
        // X*3 + Y
        public enum Direction
        {
            TopLeft     = -4,    // -1  -1
            Left        = -3,    // -1   0
            BottomLeft  = -2,    // -1   1
            Down        = -1,    //  0  -1
            Nowhere     =  0,    //  0   0
            Up          =  1,    //  0   1
            TopRight    =  2,    //  1  -1
            Right       =  3,    //  1   0
            BottomRight =  4,    //  1   1
        }
        
        
        public int x;
        public int y;

        public Coord(Coord init)
        {
            this.x = init.x;
            this.y = init.y;
        }
        
        public Coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Coord Up()
        {
            return new Coord(0, -1);
        }
        public static Coord Down()
        {
            return new Coord(0, 1);
        }
        public static Coord Left()
        {
            return new Coord(-1, 0);
        }
        public static Coord Right()
        {
            return new Coord(1, 0);
        }

        public Coord Clone()
        {
            return new Coord(this);
        }
        
        public static Coord Clone(Coord clone)
        {
            return new Coord(clone);
        }

        public Coord TurnCounterclockwise()
        {
            (x, y) = (y, -x);
            return this;
        }

        public Coord TurnClockwise()
        {
            (x, y) = (-y, x);
            return this;
        }

        public static Coord TurnClockwise(Coord turn)
        {
            return new Coord(-turn.y, turn.x);
        }

        public static Coord TurnCounterclockwise(Coord turn)
        {
            return new Coord(turn.y, -turn.x);
        }
        

        //Operator overloads
        public static Coord operator +(Coord a) => a;
        
        public static Coord operator +(Coord a, Coord b)
        {
            return new Coord(a.x + b.x, a.y + b.y);
        }

        public static Coord operator -(Coord a) => new Coord(-a.x, -a.y);
        
        public static Coord operator -(Coord a, Coord b)
        {
            return new Coord(a.x - b.x, a.y - b.y);
        }
        
        public static bool operator ==(Coord a, Coord b)
        {
            return a.x == b.x && a.y == b.y;
        }
        
        public static bool operator !=(Coord a, Coord b)
        {
            return a.x != b.x || a.y != b.y;
        }
        
        public override bool Equals(object obj)
        {
            Coord? item = obj as Coord?;

            if (item == null || !item.HasValue)
            {
                return false;
            }

            return this.x == item.Value.x && this.y == item.Value.y;
        }

        public override string ToString()
        {
            return $"({this.x}, {this.y})";
        }


        // Vector methods
        
        public Direction GetDirection()
        {
            var direction = (x * 3) + y;
            
            return (Direction)direction;
        }

        public Coord GetCoordDirectionTo(Coord position)
        {
            Coord direction = position - this;
            direction.x = direction.x > 0 ? 1 : direction.x < 0 ? -1 : 0;
            direction.y = direction.y > 0 ? 1 : direction.y < 0 ? -1 : 0;
            
            return direction;
        }
        
        public static Coord GetCoordDirectionTo(Coord positionA, Coord positionB)
        {
            Coord direction = positionB - positionA;
            direction.x = direction.x > 0 ? 1 : direction.x < 0 ? -1 : 0;
            direction.y = direction.y > 0 ? 1 : direction.y < 0 ? -1 : 0;
            
            return direction;
        }
        
        public Direction GetDirectionTo(Coord position)
        {
            Coord coordDirection = GetCoordDirectionTo(position);
            return (Direction) (coordDirection.x * 3 + coordDirection.y);
        }
        
        public static Direction GetDirectionTo(Coord positionA, Coord positionB)
        {
            Coord coordDirection = GetCoordDirectionTo(positionA, positionB);
            return (Direction) (coordDirection.x * 3 + coordDirection.y);
        }
        
    }
    
    public struct Path
    {
        private int turns;
        private List<Coord> path;

        public Path(Coord sourcePosition)
        {
            turns = 0;
            path = new List<Coord>();
            path.Add(sourcePosition);
        }

        public Path Clone()
        {
            var copy = new Path();
            copy.turns = this.turns;
            
            var pathCopy = new List<Coord>();
            foreach (var step in path)
            {
                pathCopy.Add(step);
            }

            copy.path = pathCopy;
            
            return copy;
        }

        public static Path Clone(Path original)
        {
            return original.Clone();
        }
        
        public int AddTurn()
        {
            turns++;
            return turns;
        }

        public int GetTurns()
        {
            return turns;
        }

        public List<Coord> GetDirections()
        {
            return path;
        }
        
        public int GetNumberOfSteps()
        {
            return path.Count;
        }
        
        public Coord GetCurrentPosition()
        {
            return path.Last();
        }

        public void AddStop(Coord location)
        {
            path.Add(location);
        }
    }
    
    public static class PathFinder
    {
        public static BoardManager Board;
        public static GameObject CheckSpot;
        public static GameObject TerminateSpot;
        public static GameObject FoundSpot;
        public static bool PostSpots = false;

        private struct PathStep
        {
            public Coord source;
            public Coord target;
            public Path path;
            public Coord direction;
            public List<List<GameObject>> tileMap;
            
            public bool ArrivedAtTarget()
            {
                return path.GetCurrentPosition() == target;
            }

            public PathStep Clone()
            {
                var copy = new PathStep
                {
                    source = this.source.Clone(),
                    target = this.target.Clone(),
                    path = this.path.Clone(),
                    direction = this.direction.Clone(),
                    tileMap = this.tileMap
                };

                return copy;
            }
            
            public static PathStep Clone(PathStep original)
            {
                return original.Clone();
            }
            
        }

        private static Path? pathSearchResult = null;
        
        /// <summary>
        /// Public entry point for pathfinding.
        /// Given a source, target, and copy of the map;
        ///     it will look for the shortest way of reaching the target from the source
        /// </summary>
        /// <param name="source">The start coordinate</param>
        /// <param name="target">The target coordinate of the search</param>
        /// <param name="tileMap">The map of objects to search within, so the pathfinder knows where obstacles are</param>
        /// <returns></returns>
        public static Path? FindPath(Coord source, Coord target, List<List<GameObject>> tileMap)
        {
            // Create stepping struct
            PathStep searchStep = new PathStep
            {
                source = source,
                target = target,
                path = new Path(source),
                direction = new Coord(0,0),
                tileMap = tileMap
            };
            
            // Search every direction
            pathSearchResult = null;
            
            SearchPath(searchStep);
            
            return pathSearchResult;

        }

        /// <summary>
        /// Private method for pathfinding that performs most of the search logic
        /// </summary>
        /// <param name="step">The running path step struct</param>
        private static void SearchPath(PathStep step)
        {
            // Did we arrive at the destination?
            if (step.ArrivedAtTarget())
            {
                Debug.Log($"Found target {step.path.GetCurrentPosition().ToString()} by going {step.direction.ToString()} from {step.path.GetDirections()[step.path.GetNumberOfSteps()-2].ToString()}");

                pathSearchResult = step.path;
                
                if (PostSpots)
                {
                    var foundSpot = Board.Spawn(FoundSpot, step.path.GetCurrentPosition());
                    Board.DestroyObjectWithEffect(foundSpot);
                }
                return;
            }

            // Wasn't the target, so will have to advance. Before doing that or performing any other checks,
            //  check that advancing wouldn't be longer than the current valid path anyway
            if (pathSearchResult != null && step.path.GetNumberOfSteps() + 1 >= pathSearchResult.Value.GetNumberOfSteps())
            {
                Debug.Log($"Exhausted search length at {step.path.GetCurrentPosition().ToString()} by going {step.direction.ToString()} from {step.path.GetDirections()[step.path.GetNumberOfSteps()-2].ToString()}");

                if (PostSpots)
                {
                    var terminateSpot = Board.Spawn(TerminateSpot, step.path.GetCurrentPosition());
                    Board.DestroyObjectWithEffect(terminateSpot);
                }
                return;
            }
            
            // If advancing is worth it, check that the current position isn't out of bounds
            if (step.path.GetCurrentPosition().x < 0 || step.path.GetCurrentPosition().x >= step.tileMap[0].Count ||
                step.path.GetCurrentPosition().y < 0 || step.path.GetCurrentPosition().y >= step.tileMap.Count )
            {
                Debug.Log($"Out of bounds at {step.path.GetCurrentPosition().ToString()} by going {step.direction.ToString()} from {step.path.GetDirections()[step.path.GetNumberOfSteps()-2].ToString()}");

                if (PostSpots)
                {
                    var terminateSpot = Board.Spawn(TerminateSpot, step.path.GetCurrentPosition());
                    Board.DestroyObjectWithEffect(terminateSpot);
                }
                return;
            }
            
            Debug.Log($"Item at {step.path.GetCurrentPosition().ToString()} = {step.tileMap[step.path.GetCurrentPosition().y][step.path.GetCurrentPosition().x]}");

            // If not out of bounds, check that the current position isn't an object
            if
            (
                step.path.GetCurrentPosition() != step.source &&
                step.tileMap[step.path.GetCurrentPosition().y][step.path.GetCurrentPosition().x] != null
            )
            {
                
                Debug.Log($"Ran into object at {step.path.GetCurrentPosition().ToString()} by going {step.direction.ToString()} from {step.path.GetDirections()[step.path.GetNumberOfSteps()-2].ToString()}");

                if (PostSpots)
                {
                    var terminateSpot = Board.Spawn(TerminateSpot, step.path.GetCurrentPosition());
                    Board.DestroyObjectWithEffect(terminateSpot);
                }
                return;
            }

            // Successfully checked this position as a blank space at this point
            if (PostSpots)
            {
                var checkSpot = Board.Spawn(CheckSpot, step.path.GetCurrentPosition());
                Board.DestroyObjectWithEffect(checkSpot);
            }
            
            // Check each direction from this position, favouring the direction it was already going
            Coord[] directions = new[] {Coord.Up(), Coord.Down(), Coord.Left(), Coord.Right()};
            switch (step.direction.GetDirection())
            {
                case Coord.Direction.Down:
                    (directions[0], directions[1]) = (directions[1], directions[0]);
                    break;
                case Coord.Direction.Left:
                    (directions[0], directions[2]) = (directions[2], directions[0]);
                    break;
                case Coord.Direction.Right:
                    (directions[0], directions[3]) = (directions[3], directions[0]);
                    break;
            }

            foreach (var direction in directions)
            {
                StepAlongSearchPath(step.Clone(), direction);
            }
        }
        
        /// <summary>
        /// Private method for handling logic of advancing to the next position of the search
        /// </summary>
        /// <param name="searchStep"></param>
        /// <param name="intendedDirection"></param>
        private static void StepAlongSearchPath(PathStep searchStep, Coord intendedDirection)
        {
            // Prevent going backwards over itself
            if (searchStep.direction == -intendedDirection)
            {
                return;
            }
            
            // Manage turning
            if (searchStep.direction != intendedDirection)
            {
                searchStep.path.AddTurn();
                if (searchStep.path.GetTurns() > 3)
                {
                    return;
                }
            }
            
            // Advance
            searchStep.path.AddStop(searchStep.path.GetCurrentPosition() + intendedDirection);
            searchStep.direction = intendedDirection;
            SearchPath(searchStep);
        }
        
    }
}