using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private const int MAX_TILES_X_MAX = 10;
    private const int MAX_TILES_X_MIN = 6;
    private const int MAX_TILES_Y_MAX = 20;
    private const int MAX_TILES_Y_MIN = 10;

    private bool debounce = false;
    
    public GameObject TilePrefab = null;
    public int TileXOffset = 10;
    public int TileXSize = 20;
    public int TileYOffset = -10;
    public int TileYSize = 20;
    public GameObject TileArea = null;
    
    private static List<GameObject> _activeObjects;
    private static List<List<GameObject>> _gameMap;

    // Start is called before the first frame update
    void Start()
    {
        _activeObjects = new List<GameObject>();
        createTiles();
    }

    void createTiles()
    {
        _gameMap = new List<List<GameObject>>();

        int sizeX = Mathf.RoundToInt(Random.Range(MAX_TILES_X_MIN, MAX_TILES_X_MAX));
        int sizeY = Mathf.RoundToInt(Random.Range(MAX_TILES_Y_MIN, MAX_TILES_Y_MAX));

        // Ensure an even amount of tiles
        if ((sizeX * sizeY) % 2 == 1 )
        {
            sizeY++;
        }
        
        Debug.Log($"Creating set of tiles size {sizeX} x {sizeY}");

        // Spawn tiles
        for (int posY = 0; posY < sizeY; posY++)
        {
            Debug.Log($"Creating row at {posY}");
                
            // Create the X row and then add it to the main array, creating the mem address for it
            var newRow = new List<GameObject>();
            newRow.AddRange(new GameObject[sizeX]);
            _gameMap.Add(newRow);
            
            // Create each tile on this row
            for (int posX = 0; posX < sizeX; posX++)
            {
                Debug.Log($"Creating tile at position {posX} on row {posY}");
                
                var tile = Instantiate(
                    TilePrefab, 
                    new Vector3(TileXOffset + (posX * TileXSize), TileYOffset - (posY * TileYSize), 0), 
                    Quaternion.identity
                );
                
                _gameMap[posY][posX] = tile;
                _activeObjects.Add(tile);
                
                // Set random colour
                Image image = tile.GetComponent(typeof(Image)) as Image;
                image.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                
                // Set Parent
                tile.transform.SetParent(TileArea.transform, false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!debounce && Input.GetMouseButtonDown(0))
        {
            debounce = true;

            foreach (var obj in _activeObjects)
            {
                Destroy(obj);
            }

            createTiles();
        }


        if (!Input.GetMouseButtonDown(0))
        {
            debounce = false;
        }
    }

    private void Spawn()
    {
        
    }
}
