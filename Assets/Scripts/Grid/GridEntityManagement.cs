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
            Vector2Int gridPosition = WorldToGrid(entity.WorldPosition);
            RegisterEntityAtGridPosition(entity, gridPosition);
        }

        /// <summary>
        /// Register an entity at a specific grid position.
        /// </summary>
        private void RegisterEntityAtGridPosition(IGridEntity entity, Vector2Int gridPosition)
        {
            // Ensure we have a list for this cell
            if (!entitiesInCell.ContainsKey(gridPosition))
            {
                entitiesInCell[gridPosition] = new List<IGridEntity>();
            }
            
            // Add entity to the cell
            entitiesInCell[gridPosition].Add(entity);
            entityPositions[entity] = gridPosition;
            
            // Notify subscribers
            OnEntityRegistered?.Invoke(gridPosition, entity);
        }

        /// <summary>
        /// Unregister an entity from the grid.
        /// </summary>
        public void UnregisterEntity(IGridEntity entity)
        {
            if (entityPositions.TryGetValue(entity, out Vector2Int gridPosition))
            {
                if (entitiesInCell.TryGetValue(gridPosition, out var entities))
                {
                    entities.Remove(entity);
                }
                
                entityPositions.Remove(entity);
                
                // Notify subscribers
                OnEntityUnregistered?.Invoke(gridPosition, entity);
            }
        }

        /// <summary>
        /// Update an entity's position in the grid.
        /// </summary>
        public void MoveEntity(IGridEntity entity)
        {
            Vector2Int newGridPosition = WorldToGrid(entity.WorldPosition);
            
            // If entity is already registered
            if (entityPositions.TryGetValue(entity, out Vector2Int oldGridPosition))
            {
                // If the entity hasn't changed cells, nothing to do
                if (oldGridPosition == newGridPosition)
                {
                    return;
                }
                
                // Remove from old cell
                if (entitiesInCell.TryGetValue(oldGridPosition, out var oldEntities))
                {
                    oldEntities.Remove(entity);
                }
                
                // Add to new cell
                if (!entitiesInCell.ContainsKey(newGridPosition))
                {
                    entitiesInCell[newGridPosition] = new List<IGridEntity>();
                }
                
                entitiesInCell[newGridPosition].Add(entity);
                entityPositions[entity] = newGridPosition;
                
                // Notify subscribers
                OnEntityMoved?.Invoke(oldGridPosition, newGridPosition, entity);
            }
            else
            {
                // Entity isn't registered yet, so register it
                RegisterEntity(entity);
            }
        }
    }
}