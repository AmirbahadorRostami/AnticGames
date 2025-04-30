using System;
using System.Collections.Generic;
using UnityEngine;

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
                
                // Handle any entities in this cell
                if (entitiesInCell.TryGetValue(position, out var entities) && entities.Count > 0)
                {
                    Debug.LogWarning($"Deleting cell at {position} with {entities.Count} entities still registered.");
                    
                    // Make a copy to avoid modifying during enumeration
                    List<IGridEntity> entitiesToUnregister = new List<IGridEntity>(entities);
                    foreach (var entity in entitiesToUnregister)
                    {
                        UnregisterEntity(entity);
                    }
                }
                
                entitiesInCell.Remove(position);
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
                
                // Handle any entities in this cell
                if (entitiesInCell.TryGetValue(position, out var entities) && entities.Count > 0)
                {
                    Debug.LogWarning($"Deleting cell with {entities.Count} entities still registered.");
                    
                    // Make a copy to avoid modifying during enumeration
                    List<IGridEntity> entitiesToUnregister = new List<IGridEntity>(entities);
                    foreach (var entity in entitiesToUnregister)
                    {
                        UnregisterEntity(entity);
                    }
                }
                
                entitiesInCell.Remove(position);
            }
        }

        /// <summary>
        /// Convert a world position to grid coordinates.
        /// </summary>
        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            int x = Mathf.FloorToInt((worldPosition.x - WorldOffset.x) / CellSize);
            int y = Mathf.FloorToInt((worldPosition.z - WorldOffset.y) / CellSize);  // For 2D, using XZ plane
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// Convert grid coordinates to a world position (center of the cell).
        /// </summary>
        public Vector3 GridToWorld(Vector2Int gridPosition)
        {
            float x = (gridPosition.x * CellSize) + (CellSize / 2) + WorldOffset.x;
            float z = (gridPosition.y * CellSize) + (CellSize / 2) + WorldOffset.y;  // For 2D, using XZ plane
            return new Vector3(x, 0, z);
        }
    }
}