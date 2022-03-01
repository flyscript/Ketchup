using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
    public enum TileType
    {
        A = 0,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        Maximum // Used for counting number of tile types
    }
    
    public static class TileTypeManager
    {
        
        /// <summary>
        /// Simple method to return a random TileType enum
        /// </summary>
        /// <returns>A random tile type</returns>
        public static TileType GetRandomTileType()
        {
            return (TileType) Random.Range(0, (int)TileType.Maximum);
        }

        /// <summary>
        /// Get colour for a given tile type
        /// </summary>
        /// <param name="tileType">The type to get a colour for</param>
        /// <returns>A Unity RGBA colour specific to the given type</returns>
        /// <exception cref="Exception">Errors if the given type was unsupported (e.g TileType.Maximum)</exception>
        public static Color GetColour(TileType tileType)
        {
            switch (tileType)
            {
                case TileType.A:
                    return Color.blue;
                case TileType.B:
                    return Color.cyan;
                case TileType.C:
                    return Color.green;
                case TileType.D:
                    return Color.magenta;
                case TileType.E:
                    return Color.red;
                case TileType.F:
                    return Color.yellow;
                case TileType.G:
                    return Color.gray;
                case TileType.H:
                    return Color.white;
            }

            throw new Exception("Tile Type unrecognised");
        }
        
        
    }
}