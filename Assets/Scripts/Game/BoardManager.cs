using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game
{

public class BoardManager : MonoBehaviour, IPointerClickHandler
{
    private const int MAX_TILES_X_MAX = 8;
    private const int MAX_TILES_X_MIN = 6;
    private const int MAX_TILES_Y_MAX = 14;
    private const int MAX_TILES_Y_MIN = 10;
    
    public int TileXSize = 100;
    public int TileYSize = 100;
    
    public GameObject TilePrefab = null;
    public GameObject TileArea = null;
    
    private static List<GameObject> _activeObjects;
    private static List<List<GameObject>> _gameMap;
    private static Dictionary<TileType, int> _tileTypeCounter;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.SetBoardManager(this);
        
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

    /// <summary>
    /// Scrambles the board
    /// </summary>
    /// <param name="steps">The number of times to swap random tiles</param>
    public static void ScrambleBoard(int steps = 100)
    {
        for (int i = 0; i < steps; i++)
        {
            int x1 = Random.Range(1, _gameMap[0].Count - 1);
            int x2 = Random.Range(1, _gameMap[0].Count - 1);
            int y1 = Random.Range(1, _gameMap.Count - 1);
            int y2 = Random.Range(1, _gameMap.Count - 1);

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
                                Image image = tile.transform.GetChild(1).GetComponent(typeof(Image)) as Image;
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
        
        var firstBlankRow = new List<GameObject>();
        firstBlankRow.AddRange(new GameObject[sizeX+2]);
        _gameMap.Add(firstBlankRow);

        // Spawn tiles
        for (int posY = 0; posY < sizeY; posY++)
        {
            // Create the X row and then add it to the main array, creating the mem address for it
            var newRow = new List<GameObject>();
            newRow.AddRange(new GameObject[sizeX+2]);
            _gameMap.Add(newRow);
            
            // Create each tile on this row
            for (int posX = 0; posX < sizeX; posX++)
            {
                var tile = Instantiate(
                    TilePrefab, 
                    new Vector3((TileXSize / 2) + (posX * TileXSize), (TileYSize / -2) - (posY * TileYSize), 0), 
                    Quaternion.identity
                );

                tile.GetComponent<Tile>().XPosition = posX;
                tile.GetComponent<Tile>().YPosition = posY;
                
                _gameMap[posY+1][posX+1] = tile;
                _activeObjects.Add(tile);
                
                // Set tile type
                var type = TileTypeManager.GetRandomTileType();
                tile.GetComponent<Tile>().Type = type;
                
                _tileTypeCounter[type] += 1;
                
                Image image = tile.transform.GetChild(1).GetComponent(typeof(Image)) as Image;
                image.color = TileTypeManager.GetColour(type);
                
                // Set Parent
                tile.transform.SetParent(TileArea.transform, false);
            }
        }
        
        var lastBlankRow = new List<GameObject>();
        lastBlankRow.AddRange(new GameObject[sizeX+2]);
        _gameMap.Add(lastBlankRow);
        
        // Once the rough til map has been created, clean it up a bit
        EnsureColourMatching();
        ScrambleBoard((sizeX * sizeY) / 2 );
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
        _gameMap[objTile.YPosition + 1][objTile.XPosition + 1] = null;
        
        // Delete the game object itself
        Destroy(tile, 0.4f);
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