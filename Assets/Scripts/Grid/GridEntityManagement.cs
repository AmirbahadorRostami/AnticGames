using System;
using System.Collections.Generic;
using UnityEngine;

namespace TacticalGame.Grid
{
    public partial class Grid<TCell> where TCell : class
    {
        /// <summary>
        /// Register an entity to the grid based on its world position.
        /// </summary>
        public void RegisterEntity(IGridEntity entity)
        {
            if (entity == null)
                return;
                
            Vector2Int gridPosition = WorldToGrid(entity.WorldPosition);
            RegisterEntityAtGridPosition(entity, gridPosition);
            entityRegistrationCount++;
        }
        
        /// <summary>
        /// Register an entity at a specific grid position.
        /// </summary>
        private void RegisterEntityAtGridPosition(IGridEntity entity, Vector2Int gridPosition)
        {
            // Get or create the entity collection for this cell
            HashSet<IGridEntity> entitiesAtPosition;
            if (!entitiesInCell.TryGetValue(gridPosition, out entitiesAtPosition))
            {
                entitiesAtPosition = new HashSet<IGridEntity>();
                entitiesInCell[gridPosition] = entitiesAtPosition;
                activeGridCells.Add(gridPosition);
            }
            
            // Add entity to the cell
            entitiesAtPosition.Add(entity);
            entityPositions[entity] = gridPosition;
            
            // Notify subscribers
            OnEntityRegistered?.Invoke(gridPosition, entity);
        }
        
        /// <summary>
        /// Unregister an entity from the grid.
        /// </summary>
        public void UnregisterEntity(IGridEntity entity)
        {
            if (entity == null)
                return;
                
            if (entityPositions.TryGetValue(entity, out Vector2Int gridPosition))
            {
                if (entitiesInCell.TryGetValue(gridPosition, out var entities))
                {
                    entities.Remove(entity);
                    
                    // If the cell is now empty, consider cleanup
                    if (entities.Count == 0 && !cells.ContainsKey(gridPosition))
                    {
                        entitiesInCell.Remove(gridPosition);
                        activeGridCells.Remove(gridPosition);
                    }
                }
                
                entityPositions.Remove(entity);
                
                // Notify subscribers
                OnEntityUnregistered?.Invoke(gridPosition, entity);
            }
        }
        
        /// <summary>
        /// Update an entity's position in the grid. Optimized to avoid unnecessary operations.
        /// </summary>
        public void MoveEntity(IGridEntity entity)
        {
            if (entity == null)
                return;
                
            Vector2Int newGridPosition = WorldToGrid(entity.WorldPosition);
            
            // If entity is already registered
            if (entityPositions.TryGetValue(entity, out Vector2Int oldGridPosition))
            {
                // If the entity hasn't changed cells, nothing to do
                if (oldGridPosition == newGridPosition)
                    return;
                
                // Remove from old cell
                if (entitiesInCell.TryGetValue(oldGridPosition, out var oldEntities))
                {
                    oldEntities.Remove(entity);
                    
                    // If the cell is now empty, consider cleanup
                    if (oldEntities.Count == 0 && !cells.ContainsKey(oldGridPosition))
                    {
                        entitiesInCell.Remove(oldGridPosition);
                        activeGridCells.Remove(oldGridPosition);
                    }
                }
                
                // Add to new cell
                HashSet<IGridEntity> newEntities;
                if (!entitiesInCell.TryGetValue(newGridPosition, out newEntities))
                {
                    newEntities = new HashSet<IGridEntity>();
                    entitiesInCell[newGridPosition] = newEntities;
                    activeGridCells.Add(newGridPosition);
                }
                
                newEntities.Add(entity);
                entityPositions[entity] = newGridPosition;
                
                // Notify subscribers
                OnEntityMoved?.Invoke(oldGridPosition, newGridPosition, entity);
                entityMovementCount++;
            }
            else
            {
                // Entity isn't registered yet, so register it
                RegisterEntity(entity);
            }
        }
        
        /// <summary>
        /// Bulk register multiple entities at once for efficiency.
        /// </summary>
        public void BulkRegisterEntities(IEnumerable<IGridEntity> entities)
        {
            if (entities == null)
                return;
                
            foreach (var entity in entities)
            {
                RegisterEntity(entity);
            }
        }
    }
}