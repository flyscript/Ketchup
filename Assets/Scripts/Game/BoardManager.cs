using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game
{

public class BoardManager : MonoBehaviour, IPointerClickHandler
{
    private const int MAX_TILES_X_MAX = 12;
    private const int MAX_TILES_X_MIN = 6;
    private const int MAX_TILES_Y_MAX = 23;
    private const int MAX_TILES_Y_MIN = 15;
    
    public int TileXSize = 100;
    public int TileYSize = 100;
    
    public GameObject TilePrefab = null;
    public GameObject TileArea = null;
    
    private static List<GameObject> _activeObjects;
    private static List<List<GameObject>> _gameMap;

    // Start is called before the first frame update
    void Start()
    {
        _activeObjects = new List<GameObject>();
        CreateTiles();
    }

    /// <summary>
    /// Tile creation function
    /// </summary>
    /// <param name="sizeX">The width of the grid to create</param>
    /// <param name="sizeY">The height of the grid to create</param>
    private void CreateTiles(int sizeX = 0, int sizeY = 0)
    {
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
                
                // Set random colour
                Image image = tile.GetComponent(typeof(Image)) as Image;
                image.color = Random.ColorHSV(0f, 1f, 0.75f, 1f, 0.5f, 1f);
                
                // Set Parent
                tile.transform.SetParent(TileArea.transform, false);
            }
        }
        
        var lastBlankRow = new List<GameObject>();
        lastBlankRow.AddRange(new GameObject[sizeX+2]);
        _gameMap.Add(lastBlankRow);
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
        _gameMap[objTile.XPosition + 1][objTile.YPosition + 1] = null;
        
        // Delete the game object itself
        Destroy(tile, 0.5f);
    }
    
    /// <summary>
    /// When the background is clicked, recreate the grid
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Remake!");
        
        foreach (var obj in _activeObjects)
        {
            Destroy(obj);
        }

        CreateTiles();
    }
}

}