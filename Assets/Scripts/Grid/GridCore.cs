using System;
using System.Collections.Generic;
using UnityEngine;

namespace TacticalGame.Grid
{
    /// <summary>
    /// A generic grid data structure for spatial partitioning and efficient lookups.
    /// </summary>
    public partial class Grid<TCell> where TCell : class
    {
        private Dictionary<Vector2Int, TCell> cells;
        private Dictionary<TCell, Vector2Int> cellPositions;
        private Dictionary<IGridEntity, Vector2Int> entityPositions;
        private Dictionary<Vector2Int, List<IGridEntity>> entitiesInCell;
        
        public float CellSize { get; private set; }
        public Vector2 WorldOffset { get; private set; }
        
        public event Action<Vector2Int, IGridEntity> OnEntityRegistered;
        public event Action<Vector2Int, IGridEntity> OnEntityUnregistered;
        public event Action<Vector2Int, Vector2Int, IGridEntity> OnEntityMoved;

        public Grid(float cellSize, Vector2 worldOffset)
        {
            CellSize = cellSize;
            WorldOffset = worldOffset;
            cells = new Dictionary<Vector2Int, TCell>();
            cellPositions = new Dictionary<TCell, Vector2Int>();
            entityPositions = new Dictionary<IGridEntity, Vector2Int>();
            entitiesInCell = new Dictionary<Vector2Int, List<IGridEntity>>();
        }

        /// <summary>
        /// Add a cell to the grid at specified coordinates.
        /// </summary>
        public void AddCell(int i, int j, TCell cell)
        {
            Vector2Int position = new Vector2Int(i, j);
            
            if (cells.ContainsKey(position))
            {
                Debug.LogWarning($"Cell at position {position} already exists. Replacing.");
                cells[position] = cell;
            }
            else
            {
                cells.Add(position, cell);
            }
            
            cellPositions[cell] = position;
            
            // Initialize list for entities at this position
            if (!entitiesInCell.ContainsKey(position))
            {
                entitiesInCell.Add(position, new List<IGridEntity>());
            }
        }

        /// <summary>
        /// Get a cell at specified coordinates.
        /// </summary>
        public TCell GetCell(int i, int j)
        {
            Vector2Int position = new Vector2Int(i, j);
            
            if (cells.TryGetValue(position, out TCell cell))
            {
                return cell;
            }
            
            return null;
        }
    }
}