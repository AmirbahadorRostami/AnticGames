using System;
using System.Collections.Generic;
using UnityEngine;
using TacticalGame.Utilities;

namespace TacticalGame.Grid
{
    /// <summary>
    /// A generic grid data structure for spatial partitioning and efficient lookups.
    /// Optimized for performance with object pooling and spatial partitioning.
    /// </summary>
    public partial class Grid<TCell> where TCell : class
    {
        // Core data structures
        private Dictionary<Vector2Int, TCell> cells = new Dictionary<Vector2Int, TCell>(256);
        private Dictionary<TCell, Vector2Int> cellPositions = new Dictionary<TCell, Vector2Int>(256);
        
        // Entity tracking - optimized for spatial lookups
        private Dictionary<IGridEntity, Vector2Int> entityPositions = new Dictionary<IGridEntity, Vector2Int>(128);
        private Dictionary<Vector2Int, HashSet<IGridEntity>> entitiesInCell = new Dictionary<Vector2Int, HashSet<IGridEntity>>(256);
        
        // Optional spatial acceleration structure
        private HashSet<Vector2Int> activeGridCells = new HashSet<Vector2Int>();
        
        // Cell size and world offset for coordinate transformations
        public float CellSize { get; private set; }
        public Vector2 WorldOffset { get; private set; }
        
        // Events
        public event Action<Vector2Int, IGridEntity> OnEntityRegistered;
        public event Action<Vector2Int, IGridEntity> OnEntityUnregistered;
        public event Action<Vector2Int, Vector2Int, IGridEntity> OnEntityMoved;
        
        // Statistics for debugging
        private int entityRegistrationCount = 0;
        private int entityMovementCount = 0;
        
        /// <summary>
        /// Initialize a new grid with specified cell size and world offset.
        /// </summary>
        public Grid(float cellSize, Vector2 worldOffset)
        {
            CellSize = cellSize;
            WorldOffset = worldOffset;
        }
        
        /// <summary>
        /// Add a cell to the grid at specified coordinates.
        /// </summary>
        public void AddCell(int i, int j, TCell cell)
        {
            Vector2Int position = new Vector2Int(i, j);
            
            if (cells.ContainsKey(position))
            {
                cells[position] = cell;
            }
            else
            {
                cells.Add(position, cell);
                activeGridCells.Add(position);
            }
            
            cellPositions[cell] = position;
            
            // Initialize entity collection for this position if needed
            if (!entitiesInCell.ContainsKey(position))
            {
                entitiesInCell[position] = new HashSet<IGridEntity>();
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
        
        /// <summary>
        /// Get statistics for debugging.
        /// </summary>
        public (int cellCount, int entityCount, int registrations, int movements) GetGridStats()
        {
            return (cells.Count, entityPositions.Count, entityRegistrationCount, entityMovementCount);
        }
        
        /// <summary>
        /// Clear registration stats for profiling.
        /// </summary>
        public void ClearStats()
        {
            entityRegistrationCount = 0;
            entityMovementCount = 0;
        }
    }
}