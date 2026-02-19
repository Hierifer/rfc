using UnityEngine;
using System.Collections.Generic;

namespace MazeGame
{
    /// <summary>
    /// Manages the 19×13 maze grid (from sample/utils/maze-state.js)
    /// </summary>
    public class GridManager
    {
        private CellType[,] grid;

        public GridManager()
        {
            grid = new CellType[GridConstants.Height, GridConstants.Width];
            InitializeGrid();
        }

        /// <summary>
        /// Initialize grid with empty cells and boundary walls
        /// </summary>
        private void InitializeGrid()
        {
            for (int row = 0; row < GridConstants.Height; row++)
            {
                for (int col = 0; col < GridConstants.Width; col++)
                {
                    // Set boundary walls
                    if (row == 0 || row == GridConstants.Height - 1 ||
                        col == 0 || col == GridConstants.Width - 1)
                    {
                        grid[row, col] = CellType.Wall;
                    }
                    else
                    {
                        grid[row, col] = CellType.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// Get cell type at position (safe with bounds checking)
        /// Returns Wall for out-of-bounds positions
        /// </summary>
        public CellType GetCell(Vector2Int pos)
        {
            return GetCell(pos.y, pos.x); // Note: Vector2Int.y is row, x is col
        }

        /// <summary>
        /// Get cell type at row, col (safe with bounds checking)
        /// </summary>
        public CellType GetCell(int row, int col)
        {
            if (!IsInside(row, col))
                return CellType.Wall;

            return grid[row, col];
        }

        /// <summary>
        /// Set cell type at position (safe with bounds checking)
        /// </summary>
        public void SetCell(Vector2Int pos, CellType type)
        {
            SetCell(pos.y, pos.x, type);
        }

        /// <summary>
        /// Set cell type at row, col (safe with bounds checking)
        /// </summary>
        public void SetCell(int row, int col, CellType type)
        {
            if (!IsInside(row, col))
                return;

            grid[row, col] = type;
        }

        /// <summary>
        /// Check if position is inside grid bounds
        /// </summary>
        public bool IsInside(Vector2Int pos)
        {
            return IsInside(pos.y, pos.x);
        }

        /// <summary>
        /// Check if row, col is inside grid bounds
        /// </summary>
        public bool IsInside(int row, int col)
        {
            return row >= 0 && row < GridConstants.Height &&
                   col >= 0 && col < GridConstants.Width;
        }

        /// <summary>
        /// Clear all entities from grid (reset to empty/floor, preserve walls)
        /// </summary>
        public void ClearEntities()
        {
            for (int row = 0; row < GridConstants.Height; row++)
            {
                for (int col = 0; col < GridConstants.Width; col++)
                {
                    CellType cell = grid[row, col];

                    // Preserve walls and exits, clear everything else to floor
                    if (cell != CellType.Wall && cell != CellType.Exit)
                    {
                        grid[row, col] = CellType.Floor;
                    }
                }
            }
        }

        /// <summary>
        /// Get a copy of the grid (for debugging/rendering)
        /// </summary>
        public CellType[,] GetGridCopy()
        {
            return (CellType[,])grid.Clone();
        }

        /// <summary>
        /// Reset grid to initial state
        /// </summary>
        public void Reset()
        {
            InitializeGrid();
        }
    }
}
