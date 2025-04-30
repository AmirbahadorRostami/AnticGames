using System;
using System.Collections.Generic;
using UnityEngine;

namespace TacticalGame.Grid
{
    public partial class Grid<TCell> where TCell : class
    {
        /// <summary>
        /// Get all entities in a specific cell.
        /// </summary>
        public List<IGridEntity> GetEntitiesInCell(int i, int j)
        {
            Vector2Int position = new Vector2Int(i, j);
            
            if (entitiesInCell.TryGetValue(position, out var entities))
            {
                return new List<IGridEntity>(entities);  // Return a copy to prevent modification issues
            }
            
            return new List<IGridEntity>();
        }

        /// <summary>
        /// Get all entities in a radius around a specified cell.
        /// </summary>
        public List<IGridEntity> GetEntitiesInRadius(int centerI, int centerJ, int radius)
        {
            List<IGridEntity> result = new List<IGridEntity>();
            
            for (int i = centerI - radius; i <= centerI + radius; i++)
            {
                for (int j = centerJ - radius; j <= centerJ + radius; j++)
                {
                    result.AddRange(GetEntitiesInCell(i, j));
                }
            }
            
            return result;
        }

        /// <summary>
        /// Get all entities in a radius around a world position.
        /// </summary>
        public List<IGridEntity> GetEntitiesInRadius(Vector3 worldPosition, float worldRadius)
        {
            Vector2Int centerPosition = WorldToGrid(worldPosition);
            int gridRadius = Mathf.CeilToInt(worldRadius / CellSize);
            
            return GetEntitiesInRadius(centerPosition.x, centerPosition.y, gridRadius);
        }

        /// <summary>
        /// Get the nearest entity to a position that matches a predicate.
        /// </summary>
        public IGridEntity GetNearestEntity(Vector3 worldPosition, Predicate<IGridEntity> filter = null)
        {
            // Start with small radius and expand
            float searchRadius = CellSize;
            float maxSearchRadius = 50f;  // Adjust based on game scale
            
            while (searchRadius <= maxSearchRadius)
            {
                var entities = GetEntitiesInRadius(worldPosition, searchRadius);
                
                IGridEntity nearest = null;
                float nearestDistance = float.MaxValue;
                
                foreach (var entity in entities)
                {
                    if (filter != null && !filter(entity))
                    {
                        continue;
                    }
                    
                    float distance = Vector3.Distance(worldPosition, entity.WorldPosition);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearest = entity;
                    }
                }
                
                if (nearest != null)
                {
                    return nearest;
                }
                
                // Expand search radius and try again
                searchRadius += CellSize * 2;
            }
            
            return null;  // No entity found within max search radius
        }

        /// <summary>
        /// Get all entities of a specific type within a radius
        /// </summary>
        public List<IGridEntity> GetEntitiesOfTypeInRadius(Vector3 worldPosition, float worldRadius, EntityType entityType)
        {
            List<IGridEntity> allEntities = GetEntitiesInRadius(worldPosition, worldRadius);
            List<IGridEntity> filteredEntities = new List<IGridEntity>();
            
            foreach (var entity in allEntities)
            {
                if (entity.EntityType == entityType)
                {
                    filteredEntities.Add(entity);
                }
            }
            
            return filteredEntities;
        }
    }
}