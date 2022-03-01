using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public static class GameManager
    {
        private static BoardManager _boardManager = null;

        private static GameObject _currentSelection = null;
        private static GameObject _currentSelectionHighlight = null;

        public static void SetBoardManager(BoardManager manager)
        {
            _boardManager = manager;
        }

        /// <summary>
        /// Deselects the currently selected tile
        /// </summary>
        /// <returns>True if a tile was cleared</returns>
        public static bool ClearSelection()
        {
            if (_currentSelection != null)
            {
                _boardManager.DestroyObject(_currentSelectionHighlight);
                _currentSelectionHighlight = null;
                _currentSelection = null;
                return true;
            }

            return false;
        }

        private static Path? ValidateTileMatch(GameObject tileA, GameObject tileB)
        {
            if (tileA.GetComponent<Tile>().Type != tileB.GetComponent<Tile>().Type)
            {
                Debug.Log($"Tile Type Mismatch");
                return null;
            }

            return PathFinder.FindPath(tileA.GetComponent<Tile>().Position, tileB.GetComponent<Tile>().Position, _boardManager.GetMap());
        }

        /// <summary>
        /// The main entry point for actions when a tile is clicked
        /// </summary>
        /// <param name="tile">The tile that was selected</param>
        public static void TileClicked(GameObject tile)
        {
            // If no selection existed, then the incoming tile is the start of a new selection
            if (_currentSelection == null)
            {
                _currentSelection = tile;
                _currentSelectionHighlight = _boardManager.CreateHighlight(tile.GetComponent<Tile>().Position);
                Debug.Log($"New Selection {tile.GetComponent<Tile>().Type.ToString()} @ {tile.GetComponent<Tile>().Position.ToString()}");
                return;
            }

            // Or, if the new tile is the same as the selected tile then deselect it
            if (_currentSelection == tile)
            {
                Debug.Log($"Deselect {tile.GetComponent<Tile>().Type.ToString()} @ {tile.GetComponent<Tile>().Position.ToString()}");
                ClearSelection();
                return;
            }
            
            // Draw the new highlight
            Debug.Log($"Second Selection {tile.GetComponent<Tile>().Type.ToString()} @ {tile.GetComponent<Tile>().Position.ToString()}");
            var newSelectionHighlight = _boardManager.CreateHighlight(tile.GetComponent<Tile>().Position);

            // Find a path
            var validPath = ValidateTileMatch(_currentSelection, tile);
            
            // If the match was invalid, then 
            if (validPath == null)
            {
                Debug.Log($"Drawing Red Highlights");
                
                foreach (var image in _currentSelectionHighlight.GetComponentsInChildren<Image>())
                {
                    image.color = Color.red;
                }
                
                foreach (var image in newSelectionHighlight.GetComponentsInChildren<Image>())
                {
                    image.color = Color.red;
                }
                
                _boardManager.DestroyObjectWithEffect(_currentSelectionHighlight);
                _boardManager.DestroyObjectWithEffect(newSelectionHighlight);
                _currentSelectionHighlight = null;
                _currentSelection = null;
                return;
            }
            
            // Finally, if the match is a go, then handle that
            Debug.Log($"Drawing Green Highlights and Line Path");
            foreach (var image in _currentSelectionHighlight.GetComponentsInChildren<Image>())
            {
                image.color = Color.green;
            }
                
            foreach (var image in newSelectionHighlight.GetComponentsInChildren<Image>())
            {
                image.color = Color.green;
            }
            
            _boardManager.DrawPath(validPath.Value);
                
            _boardManager.DestroyObjectWithEffect(_currentSelectionHighlight);
            _boardManager.DestroyObjectWithEffect(newSelectionHighlight);
            
            _boardManager.DestroyTile(_currentSelection);
            _boardManager.DestroyTile(tile);
            
            _currentSelectionHighlight = null;
            _currentSelection = null;
        }
        
    }
}