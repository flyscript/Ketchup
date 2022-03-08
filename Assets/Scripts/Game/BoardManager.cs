using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game
{

public class BoardManager : MonoBehaviour
{
    public float EffectTime = 0.4f;
        
    public GameObject TimerBar = null;
    public GameObject NewGameButton = null;
    public GameObject ScrambleButton = null;
        
    public float GameTime = 60.0f;
    private float _timeGameStarted;
    public float MatchBonusTime = 1.0f;
    
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
    
    private static List<GameObject> _activeTiles;
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
        
        _activeTiles = new List<GameObject>();
        CreateTiles();
    }

    /// <summary>
    /// Resets and enables the tile type tracker for ensuring all new tiles have matches
    /// </summary>
    private static void ResetTileTypeCounter()
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
    public void ScrambleBoard()
    {
        foreach (var tileA in _activeTiles)
        {
            var tileB = _activeTiles[Random.Range(0, _activeTiles.Count)];

            // Swap the type properties on the tiles
            var typeA = tileA.GetComponent<Tile>().Type;
            var typeB = tileB.GetComponent<Tile>().Type;
            tileA.GetComponent<Tile>().Type = typeB;
            tileB.GetComponent<Tile>().Type = typeA;
            
            // Swap the tiles visually
            Image imageA = tileA.transform.GetChild(0).GetComponent(typeof(Image)) as Image;
            imageA.color = TileTypeManager.GetColour(typeB);
            Image imageB = tileB.transform.GetChild(0).GetComponent(typeof(Image)) as Image;
            imageB.color = TileTypeManager.GetColour(typeA);
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
                        foreach (var tile in _activeTiles)
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
            PlaceItemInWorldAtGridPosition(line, directions[step+1]);
            lines.Add(line);
        }

        // Destroy the lines after the effect time
        foreach (var line in lines)
        {
            Destroy(line, EffectTime);
        }
    }

    public void NewBoardClick()
    {
        CreateTiles();
    }
    
    public void ScrambleBoardClick()
    {
        ScrambleBoard();
    }
    
    
    /// <summary>
    /// Tile creation function
    /// </summary>
    /// <param name="sizeX">The width of the grid to create</param>
    /// <param name="sizeY">The height of the grid to create</param>
    private void CreateTiles(int sizeX = 0, int sizeY = 0)
    {
        Debug.Log("CREATING NEW GAME!");
        
        // Clear the board
        GameManager.ClearSelection();
        foreach (var obj in _activeTiles)
        {
            Destroy(obj);
        }
        _activeTiles.Clear();
        
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
                _activeTiles.Add(tile);
                
                var type = TileTypeManager.GetRandomTileType();
                tile.GetComponent<Tile>().Type = type;
                _tileTypeCounter[type] += 1;
                
                Image image = tile.transform.GetChild(0).GetComponent(typeof(Image)) as Image;
                image.color = TileTypeManager.GetColour(type);
                
                PlaceItemInWorldAtGridPosition(tile, new Coord(posX, posY));
            }
        }
        
        // Once the rough tile map has been created, clean it up a bit
        EnsureColourMatching();
        //ScrambleBoard((sizeX * sizeY) / 2 );
            
        // Reset Timer Bar
        _timeGameStarted = Time.time;
        SetTimerBarScale(1.0f);
        
        GameManager.Playing = true;
        
        Debug.Log("NOW PLAYING!");
    }

    public GameObject Spawn(GameObject obj, Coord position)
    {
        var spawn = Instantiate(obj, Vector3.zero, Quaternion.identity);
        PlaceItemInWorldAtGridPosition(spawn, position);
        return spawn;
    }

    public GameObject CreateHighlight(Coord gridPosition)
    {
        var line = Instantiate(HighlightPrefab, Vector3.zero, Quaternion.identity);
        PlaceItemInWorldAtGridPosition(line, gridPosition);
        return line;
    }

    public void PlaceItemAtGridPosition(GameObject obj, Coord gridPosition)
    {
        var v3 = new Vector3(
            (TileXSize / 2) + (gridPosition.x * TileXSize),
            (TileYSize / -2) - (gridPosition.y * TileYSize),
            0);

        RectTransform transform = obj.transform.GetComponent(typeof(RectTransform)) as RectTransform;
        transform.position = v3;

        Debug.Log($"Setting {obj.name} position to coordinate {gridPosition.ToString()} : {v3.ToString()}");
    }

    public void PlaceItemInWorldAtGridPosition(GameObject obj, Coord gridPosition)
    {
        PlaceItemAtGridPosition(obj, gridPosition);
        
        // Set Parent
        obj.transform.SetParent(TileArea.transform, false);
    }

    /// <summary>
    /// Scales the timer bar
    /// </summary>
    /// <param name="fullness">How full the timer bar is, should be 0 - 1</param>
    private void SetTimerBarScale(float fullness)
    {
        // Clamp fullness between 0 and 1
        fullness = fullness > 1 ? 1 : fullness < 0 ? 0 : fullness;
        
        RectTransform timerTransform = TimerBar.transform.GetComponent(typeof(RectTransform)) as RectTransform;
        RectTransform timerParentTransform = TimerBar.transform.parent.GetComponent(typeof(RectTransform)) as RectTransform;
        
        timerTransform.sizeDelta = new Vector2(timerParentTransform.rect.width * fullness, timerTransform.rect.height);
        
        Image timerImage = TimerBar.transform.GetComponent(typeof(Image)) as Image;
        timerImage.color = new Color((fullness < 0.5f? 1.0f - fullness*2 : 0), fullness+0.25f, 0);

        // If time has run out, then no longer playing
        if (fullness < 0.001f)
        {
            Debug.LogWarning("RAN OUT OF TIME!");
            GameManager.Playing = false;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (_activeTiles != null && _activeTiles.Count != 0 && GameManager.Playing)
        {
            SetTimerBarScale(1.0f - ((Time.time - _timeGameStarted) / GameTime));
        }
    }

    public void NotifyMatch(GameObject tileA, GameObject TileB)
    {
        // Destroy the tiles
        DestroyTile(tileA);
        DestroyTile(TileB);
        
        // Add extra time
        _timeGameStarted += MatchBonusTime;
        
        // Has the player finished the game?
        if (_activeTiles.Count == 0)
        {
            Debug.Log("YOU COMPLETED THE GAME!");
            GameManager.Playing = false;
        }
        Debug.Log($"Tiles Left: {_activeTiles.Count}");
    }

    /// <summary>
    /// Command to remove a tile from the game board
    /// </summary>
    /// <param name="tile"></param>
    private void DestroyTile(GameObject tile)
    {
        Tile objTile = tile.GetComponent<Tile>();
        
        // Remove the tile from the grid data struct
        _gameMap[objTile.Position.y][objTile.Position.x] = null;
        
        // Remove the tile from the active tiles
        _activeTiles.Remove(tile);
        
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
    
}

}