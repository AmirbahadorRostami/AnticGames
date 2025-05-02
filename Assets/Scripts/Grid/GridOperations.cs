using System;
using System.Collections.Generic;
using UnityEngine;
using TacticalGame.Utilities;

namespace TacticalGame.Grid
{
    public partial class Grid<TCell> where TCell : class
    {
        /// <summary>
        /// Delete a cell at specified coordinates.
        /// </summary>
        public void DeleteCell(int i, int j)
        {
            Vector2Int position = new Vector2Int(i, j);
            
            if (cells.TryGetValue(position, out TCell cell))
            {
                cellPositions.Remove(cell);
                cells.Remove(position);
                activeGridCells.Remove(position);
                
                // Handle any entities in this cell
                if (entitiesInCell.TryGetValue(position, out var entities) && entities.Count > 0)
                {
                    Debug.LogWarning($"Deleting cell at {position} with {entities.Count} entities still registered.");
                    
                    // Make a copy to avoid modifying during enumeration
                    var entitiesToUnregister = new List<IGridEntity>(entities);
                    foreach (var entity in entitiesToUnregister)
                    {
                        UnregisterEntity(entity);
                    }
                }
                
                // Only remove from entitiesInCell if there are no entities left
                if (!entitiesInCell.TryGetValue(position, out entities) || entities.Count == 0)
                {
                    entitiesInCell.Remove(position);
                }
            }
        }
        
        /// <summary>
        /// Delete a specific cell.
        /// </summary>
        public void DeleteCell(TCell cell)
        {
            if (cellPositions.TryGetValue(cell, out Vector2Int position))
            {
                cellPositions.Remove(cell);
                cells.Remove(position);
                
                if (!entitiesInCell.TryGetValue(position, out var entities) || entities.Count == 0)
                {
                    activeGridCells.Remove(position);
                }
                
                // Handle any entities in this cell
                if (entities != null && entities.Count > 0)
                {
                    Debug.LogWarning($"Deleting cell with {entities.Count} entities still registered.");
                    
                    // Make a copy to avoid modifying during enumeration
                    var entitiesToUnregister = new List<IGridEntity>(entities);
                    foreach (var entity in entitiesToUnregister)
                    {
                        UnregisterEntity(entity);
                    }
                }
                
                // Only remove from entitiesInCell if there are no entities left
                if (!entitiesInCell.TryGetValue(position, out entities) || entities.Count == 0)
                {
                    entitiesInCell.Remove(position);
                }
            }
        }
        
        // Fix: Removed readonly modifier and using local variables instead
        /// <summary>
        /// Convert a world position to grid coordinates. Optimized to minimize allocations.
        /// </summary>
        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            int x = Mathf.FloorToInt((worldPosition.x - WorldOffset.x) / CellSize);
            int y = Mathf.FloorToInt((worldPosition.z - WorldOffset.y) / CellSize);
            return new Vector2Int(x, y);
        }
        
        // Fix: Removed readonly modifier and using local variables instead
        /// <summary>
        /// Convert grid coordinates to a world position (center of the cell).
        /// Optimized to minimize allocations.
        /// </summary>
        public Vector3 GridToWorld(Vector2Int gridPosition)
        {
            float x = (gridPosition.x * CellSize) + (CellSize / 2) + WorldOffset.x;
            float y = 0;
            float z = (gridPosition.y * CellSize) + (CellSize / 2) + WorldOffset.y;
            return new Vector3(x, y, z);
        }
        
        /// <summary>
        /// Get all active cells in the grid. Returns a pooled list that must be returned.
        /// </summary>
        public List<Vector2Int> GetActiveCells()
        {
            List<Vector2Int> result = ListPool<Vector2Int>.Get();
            foreach (var cell in activeGridCells)
            {
                result.Add(cell);
            }
            return result;
        }
    }
}