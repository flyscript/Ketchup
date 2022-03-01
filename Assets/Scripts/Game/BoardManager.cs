using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game
{

public class BoardManager : MonoBehaviour, IPointerClickHandler
{
    public float EffectTime = 0.4f;
    
    public int MAX_TILES_X_MAX = 8;
    public int MAX_TILES_X_MIN = 6;
    public int MAX_TILES_Y_MAX = 14;
    public int MAX_TILES_Y_MIN = 10;
    
    public int TileXSize = 100;
    public int TileYSize = 100;
    
    public GameObject CheckSpotPrefab = null;
    public GameObject TerminateSpotPrefab = null;
    public GameObject FoundSpotPrefab = null;
    public GameObject TilePrefab = null;
    public GameObject HighlightPrefab = null;
    public GameObject HorizontalLinePrefab = null;
    public GameObject VerticalLinePrefab = null;
    public GameObject TopLeftLinePrefab = null;
    public GameObject TopRightLinePrefab = null;
    public GameObject BottomLeftLinePrefab = null;
    public GameObject BottomRightLinePrefab = null;
    public GameObject TileArea = null;
    
    private static List<GameObject> _activeObjects;
    private static List<List<GameObject>> _gameMap;
    private static Dictionary<TileType, int> _tileTypeCounter;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.SetBoardManager(this);
        PathFinder.Board = this;
        PathFinder.CheckSpot = CheckSpotPrefab;
        PathFinder.TerminateSpot = TerminateSpotPrefab;
        PathFinder.FoundSpot = FoundSpotPrefab;
        
        _activeObjects = new List<GameObject>();
        CreateTiles();
    }

    /// <summary>
    /// Resets and enables the tile type tracker for ensuring all new tiles have matches
    /// </summary>
    public static void ResetTileTypeCounter()
    {
        _tileTypeCounter = new Dictionary<TileType, int>();

        for (int e = 0; e < (int) TileType.Maximum; e++)
        {
            _tileTypeCounter[(TileType) e] = 0;
        }
    }

    public List<List<GameObject>> GetMap()
    {
        return _gameMap;
    }
    

    /// <summary>
    /// Scrambles the board
    /// </summary>
    /// <param name="steps">The number of times to swap random tiles</param>
    public void ScrambleBoard(int steps = 100)
    {
        for (int i = 0; i < steps; i++)
        {
            int x1 = Random.Range(1, _gameMap[0].Count - 1);
            int x2 = Random.Range(1, _gameMap[0].Count - 1);
            int y1 = Random.Range(1, _gameMap.Count - 1);
            int y2 = Random.Range(1, _gameMap.Count - 1);

            var tileA = _gameMap[y1][x1];
            var coordA = tileA.GetComponent<Tile>().Position.Clone();
            var tileB = _gameMap[y2][x2];
            var coordB = tileB.GetComponent<Tile>().Position.Clone();
            
            // Swap them visually
            tileA.GetComponent<Tile>().Position = coordB;
            tileB.GetComponent<Tile>().Position = coordA;
            
            PlaceItemAtGridPosition(tileA, coordB);
            PlaceItemAtGridPosition(tileA, coordA);
            
            // Swap them in the data struct
            (_gameMap[y1][x1], _gameMap[y2][x2]) = (_gameMap[y2][x2], _gameMap[y1][x1]);
        }
        
        //TODO: Check matches are possible, and if not then re-scramble
    }

    /// <summary>
    /// Ensure that there is a match for each tile
    /// TODO: This could probably be improved by just creating the tiles in pairs in the first place
    /// </summary>
    private static void EnsureColourMatching()
    {
        for (int tileTypeA = 0; tileTypeA < _tileTypeCounter.Count; tileTypeA++)
        {
            // We ensure that the total number of tiles is even, so
            //  if any of the types are uneven, then another must be too 
            if (_tileTypeCounter[(TileType)tileTypeA] % 2 == 1)
            {
                // Check all subsequent types for the next one used an un-even number of times
                for (int tileTypeB = tileTypeA+1; tileTypeB < _tileTypeCounter.Count; tileTypeB++)
                {
                    if (_tileTypeCounter[(TileType)tileTypeB] % 2 == 1)
                    {
                        // Get the first tile of the second type
                        // (if there's an uneven amount there definitely is one)
                        foreach (var tile in _activeObjects)
                        {
                            if (tile.GetComponent<Tile>().Type == (TileType) tileTypeB)
                            {
                                // Make a tile of type B becme type A
                                tile.GetComponent<Tile>().Type = (TileType) tileTypeA;
                                Image image = tile.transform.GetChild(0).GetComponent(typeof(Image)) as Image;
                                image.color = TileTypeManager.GetColour((TileType) tileTypeA);
                                
                                // Adjust figures on type counter
                                _tileTypeCounter[(TileType) tileTypeB] -= 1;
                                _tileTypeCounter[(TileType) tileTypeA] += 1;
                                break;
                            }
                            
                        }
                    }
                }
            }
            
        }
    }

    public void DrawPath(Path path)
    {
        List<GameObject> lines = new List<GameObject>();
        var directions = path.GetDirections();

        for (int step = 0; step < directions.Count-2; step++)
        {
            var direction1 = directions[step].GetDirectionTo(directions[step+1]);
            var direction2 = directions[step+1].GetDirectionTo(directions[step+2]);

            GameObject lineDirection = null;
            
            // Based on where the path is headed, a different kind of line is required
            switch ((direction1, direction2))
            {
                case (Coord.Direction.Down, Coord.Direction.Left):
                    lineDirection = BottomLeftLinePrefab;
                    break;
                case (Coord.Direction.Down, Coord.Direction.Right):
                    lineDirection = BottomRightLinePrefab;
                    break;
                case (Coord.Direction.Down, Coord.Direction.Down):
                    lineDirection = VerticalLinePrefab;
                    break;
                case (Coord.Direction.Up, Coord.Direction.Left):
                    lineDirection = TopLeftLinePrefab;
                    break;
                case (Coord.Direction.Up, Coord.Direction.Right):
                    lineDirection = TopRightLinePrefab;
                    break;
                case (Coord.Direction.Up, Coord.Direction.Up):
                    lineDirection = VerticalLinePrefab;
                    break;
                case (Coord.Direction.Left, Coord.Direction.Up):
                    lineDirection = BottomRightLinePrefab;
                    break;
                case (Coord.Direction.Left, Coord.Direction.Down):
                    lineDirection = TopRightLinePrefab;
                    break;
                case (Coord.Direction.Left, Coord.Direction.Left):
                    lineDirection = HorizontalLinePrefab;
                    break;
                case (Coord.Direction.Right, Coord.Direction.Up):
                    lineDirection = BottomLeftLinePrefab;
                    break;
                case (Coord.Direction.Right, Coord.Direction.Down):
                    lineDirection = TopLeftLinePrefab;
                    break;
                case (Coord.Direction.Right, Coord.Direction.Right):
                    lineDirection = HorizontalLinePrefab;
                    break;
            }

            // If somehow the direction wasn't found, then something was wrong with the directions
            if (lineDirection == null)
            {
                Debug.LogError($"Directions were unable to produce line path! Directions as follows: {direction1.ToString()}, {direction2.ToString()} \n {string.Join("\n", directions)}");
                return;
            }
            
            // Place line along path
            var line = Instantiate(lineDirection, Vector3.zero, Quaternion.identity);
            PlaceItemAtGridPosition(line, directions[step+1]);
            lines.Add(line);
        }

        // Destroy the lines after the effect time
        foreach (var line in lines)
        {
            Destroy(line, EffectTime);
        }
    }
    
    
    
    /// <summary>
    /// Tile creation function
    /// </summary>
    /// <param name="sizeX">The width of the grid to create</param>
    /// <param name="sizeY">The height of the grid to create</param>
    private void CreateTiles(int sizeX = 0, int sizeY = 0)
    {
        // Clear the board
        _activeObjects.Clear();
        
        // Create random tile size
        //TODO: remove random size when game is at desired stage
        sizeX = Mathf.RoundToInt(Random.Range(MAX_TILES_X_MIN, MAX_TILES_X_MAX));
        sizeY = Mathf.RoundToInt(Random.Range(MAX_TILES_Y_MIN, MAX_TILES_Y_MAX));
        
        // Ensure an even amount of tiles
        if ((sizeX * sizeY) % 2 == 1 )
        {
            sizeY++;
        }
        
        // Create tile datastruct
        _gameMap = new List<List<GameObject>>();
        ResetTileTypeCounter();
        

        // Spawn tiles
        for (int posY = 0; posY <= sizeY+1; posY++)
        {
            // Create the X row and then add it to the main array, creating the entry for it
            _gameMap.Add(new List<GameObject>());
            
            // Create each tile on this row
            for (int posX = 0; posX <= sizeX+1; posX++)
            {
                
                // If the first or last row or column, then create the blank space
                if (posX == 0 || posX == sizeX + 1 || posY == 0 || posY == sizeY + 1)
                {
                    _gameMap[posY].Add(null);
                    continue;
                }
                
                // Otherwise, create a random tile
                var tile = Instantiate(TilePrefab, Vector3.zero, Quaternion.identity);
                tile.name = $"Tile ({posX}, {posY})";
                tile.GetComponent<Tile>().Position = new Coord(posX, posY);
                
                _gameMap[posY].Add(tile);
                _activeObjects.Add(tile);
                
                var type = TileTypeManager.GetRandomTileType();
                tile.GetComponent<Tile>().Type = type;
                _tileTypeCounter[type] += 1;
                
                Image image = tile.transform.GetChild(0).GetComponent(typeof(Image)) as Image;
                image.color = TileTypeManager.GetColour(type);
                
                PlaceItemAtGridPosition(tile, new Coord(posX, posY));
            }
        }
        
        // Once the rough tile map has been created, clean it up a bit
        EnsureColourMatching();
        //ScrambleBoard((sizeX * sizeY) / 2 );
    }

    public GameObject Spawn(GameObject obj, Coord position)
    {
        var spawn = Instantiate(obj, Vector3.zero, Quaternion.identity);
        PlaceItemAtGridPosition(spawn, position);
        return spawn;
    }

    public GameObject CreateHighlight(Coord gridPosition)
    {
        var line = Instantiate(HighlightPrefab, Vector3.zero, Quaternion.identity);
        PlaceItemAtGridPosition(line, gridPosition);
        return line;
    }

    public void PlaceItemAtGridPosition(GameObject obj, Coord gridPosition)
    {
        var v3 = new Vector3(
            (TileXSize / 2) + (gridPosition.x * TileXSize),
            (TileYSize / -2) - (gridPosition.y * TileYSize),
            0);
        
        obj.transform.position = v3;
        
        //Debug.Log($"Setting object position to coordinate {gridPosition.ToString()} : {v3.ToString()}");
        
        // Set Parent
        obj.transform.SetParent(TileArea.transform, false);
    }

    // Update is called once per frame
    void Update()
    {
    }

    /// <summary>
    /// Command to remove a tile from the game board
    /// </summary>
    /// <param name="tile"></param>
    public void DestroyTile(GameObject tile)
    {
        Tile objTile = tile.GetComponent<Tile>();
        
        // Remove the tile from the grid data struct
        _gameMap[objTile.Position.y][objTile.Position.x] = null;
        
        // Delete the game object itself
        Destroy(tile, EffectTime);
    }

    /// <summary>
    /// Command to remove a non-tile object with a timing effect
    /// </summary>
    /// <param name="obj"></param>
    public void DestroyObjectWithEffect(GameObject obj)
    {
        // Delete the game object itself
        Destroy(obj, EffectTime);
    }
    
    /// <summary>
    /// Command to remove a non-tile object without any wait
    /// </summary>
    /// <param name="obj"></param>
    public void DestroyObject(GameObject obj)
    {
        // Delete the game object itself
        Destroy(obj);
    }
    
    /// <summary>
    /// When the background is clicked, recreate the grid
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!GameManager.ClearSelection())
        {
            foreach (var obj in _activeObjects)
            {
                Destroy(obj);
            }
            
            _activeObjects.Clear();
            
            CreateTiles();
        }
    }
}

}