using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public static class GameManager
    {
        private static BoardManager boardManager = null;

        private static GameObject currentSelection = null;

        public static void SetBoardManager(BoardManager manager)
        {
            boardManager = manager;
        }

        /// <summary>
        /// Deselects the currently selected tile
        /// </summary>
        /// <returns>True if a tile was cleared</returns>
        public static bool ClearSelection()
        {
            if (currentSelection != null)
            {
                currentSelection.transform.GetChild(0).GetComponent<Image>().color = new Color(0.0f,0.0f,0.0f,0.0f);
                currentSelection = null;
                return true;
            }

            return false;
        }

        private static bool ValidateTileMatch(GameObject tileA, GameObject tileB)
        {
            //TODO: check path

            if (tileA.GetComponent<Tile>().Type != tileB.GetComponent<Tile>().Type)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// The main entry point for actions when a tile is clicked
        /// </summary>
        /// <param name="tile">The tile that was selected</param>
        public static void TileClicked(GameObject tile)
        {
            // Highlight selection
            tile.transform.GetChild(0).GetComponent<Image>().color = Color.white;
            
            // If no selection existed, then the incoming tile is the start of a new selection
            if (currentSelection == null)
            {
                currentSelection = tile;
                return;
            }
            
            // If the new tile is the same as the selected tile, or valdiation failed, then deselect it
            if (currentSelection == tile || !ValidateTileMatch(currentSelection, tile))
            {
                tile.transform.GetChild(0).GetComponent<Image>().color = new Color(0.0f,0.0f,0.0f,0.0f);
                ClearSelection();
                return;
            }
            
            // Destroy tiles
            currentSelection.transform.GetChild(0).GetComponent<Image>().color = Color.green;
            boardManager.DestroyTile(currentSelection);
                
            tile.transform.GetChild(0).GetComponent<Image>().color = Color.green;
            boardManager.DestroyTile(tile);
            
            currentSelection = null;
        }
        
    }
}