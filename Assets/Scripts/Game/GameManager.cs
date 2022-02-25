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
            boardManager = new BoardManager();
        }

        /// <summary>
        /// The main entry point for actions when a tile is clicked
        /// </summary>
        /// <param name="tile">The tile that was selected</param>
        public static void TileClicked(GameObject tile)
        {
            // Highlight selection
            tile.GetComponent<Image>().color = Color.red;
            
            // If no selection existed, then the incoming tile is the start of a new selection
            if (currentSelection == null)
            {
                currentSelection = tile;
                return;
            }
            
            // If a selection existed, then check the path
            //TODO: check path
            
            // Destroy tiles
            boardManager.DestroyTile(currentSelection);
            boardManager.DestroyTile(tile);
        }
        
    }
}