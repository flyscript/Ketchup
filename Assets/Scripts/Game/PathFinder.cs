using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using UnityEngine;

namespace Game
{
    public struct Coord
    {
        public int x;
        public int y;

        public Coord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        
        public Coord ToWholeGridCoord(Coord coord)
        {
            return new Coord(x + 1, y + 1);
        }
        
        public Coord ToMainGridCoord(Coord coord)
        {
            return new Coord(x - 1, y - 1);
        }

        public Coord TurnClockwise()
        {
            (x, y) = (y, -x);
            return this;
        }

        public Coord TurnCounterclockwise()
        {
            (x, y) = (-y, x);
            return this;
        }

        public static Coord TurnClockwise(Coord turn)
        {
            return new Coord(turn.y, -turn.x);
        }

        public static Coord TurnCounterclockwise(Coord turn)
        {
            return new Coord(-turn.y, turn.x);
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
        
        public Coord GetLastPosition()
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

        private struct PathStep
        {
            public Coord source;
            public Coord target;
            public Path path;
            public Coord direction;
            public List<List<GameObject>> tileMap;
            
            public bool ArrivedAtTarget()
            {
                return path.GetLastPosition() == target;
            }
        }
        
        public static Path? FindPath(Coord source, Coord target, List<List<GameObject>> tileMap)
        {

            // Create stepping struct
            PathStep step = new PathStep
            {
                source = source,
                target = target,
                path = new Path(source),
                tileMap = tileMap
            };

            List<Path> validPaths = new List<Path>();
            
            // Try every direction
            for (int dirX = 0; dirX < 2; dirX++)
            {
                for (int dirY = 0; dirY < 2; dirY++)
                {
                    step.direction = new Coord(dirX, dirY);

                    var path = StepAlongPath(step);

                    if (path.ArrivedAtTarget())
                    {
                        validPaths.Add(path.path);
                    }
                }
            }

            // If no valid paths were found then return null
            if (validPaths.Count == 0)
            {
                return null;
            }
            
            // Find the shortest of the valid paths
            Path shortestPath = validPaths[0];
            foreach (var path in validPaths)
            {
                if (path.GetDirections().Count < shortestPath.GetDirections().Count)
                {
                    shortestPath = path;
                }
            }

            return shortestPath;

        }

        private static PathStep StepAlongPath(PathStep step)
        {
            // Did we already arrive at the destination?
            if (step.ArrivedAtTarget())
            {
                return step;
            }
            
            // Get next position in the direction of travel
            Coord newPosition = step.path.GetLastPosition() + step.direction;
            
            // Check direction of travel is valid (e.g: out of bounds or hit another tile)
            if (newPosition.x < 0 || newPosition.y < 0 || newPosition.x > step.tileMap[0].Count || newPosition.y > step.tileMap.Count || step.tileMap[newPosition.y][newPosition.x] != null)
            {
                // If we don't have turns left then just return
                if (step.path.GetTurns() > 2)
                {
                    return step;
                }

                step.path.AddTurn();

                var originalDirection = step.direction;
                var left = Coord.TurnCounterclockwise(originalDirection);
                //TODO: needs to search both directions and see if they pull up anything
                var leftSearch = 
                
                var right = Coord.TurnClockwise(originalDirection);
                
                

            }
            
            // Go the next pos
            step.path.AddStop(newPosition);
            
            // Did we arrive at the destination?
            if (newPosition == step.target)
            {
                return step;
            }
            
            // Continue in that direction
            return StepAlongPath(step);
            
        }
        
    }
}