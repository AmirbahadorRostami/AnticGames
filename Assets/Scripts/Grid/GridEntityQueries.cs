using System;
using System.Collections.Generic;
using UnityEngine;
using TacticalGame.Utilities;

namespace TacticalGame.Grid
{
    public partial class Grid<TCell> where TCell : class
    {
        // Cache for radius calculations to avoid recomputing the same values
        private Dictionary<int, Vector2Int[]> radiusOffsetCache = new Dictionary<int, Vector2Int[]>(8);
        
        /// <summary>
        /// Get all entities in a specific cell. Returns a pooled list that must be returned.
        /// </summary>
        public List<IGridEntity> GetEntitiesInCell(int i, int j)
        {
            Vector2Int position = new Vector2Int(i, j);
            List<IGridEntity> result = ListPool<IGridEntity>.Get();
            
            if (entitiesInCell.TryGetValue(position, out var entities))
            {
                foreach (var entity in entities)
                {
                    result.Add(entity);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Get all entities in a radius around a specified cell. Returns a pooled list that must be returned.
        /// </summary>
        public List<IGridEntity> GetEntitiesInRadius(int centerI, int centerJ, int radius)
        {
            List<IGridEntity> result = ListPool<IGridEntity>.Get();
            Vector2Int center = new Vector2Int(centerI, centerJ);
            
            // Get or compute radius offsets
            Vector2Int[] offsets = GetRadiusOffsets(radius);
            
            // Use the cached offsets to check cells
            foreach (var offset in offsets)
            {
                Vector2Int cellPos = center + offset;
                
                // Skip cells we know are inactive (optimization)
                if (!activeGridCells.Contains(cellPos))
                    continue;
                    
                if (entitiesInCell.TryGetValue(cellPos, out var entities))
                {
                    foreach (var entity in entities)
                    {
                        result.Add(entity);
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Cache and return offsets for a given radius.
        /// </summary>
        private Vector2Int[] GetRadiusOffsets(int radius)
        {
            if (radiusOffsetCache.TryGetValue(radius, out var offsets))
            {
                return offsets;
            }
            
            // Compute and cache the offsets for this radius
            List<Vector2Int> newOffsets = new List<Vector2Int>();
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    newOffsets.Add(new Vector2Int(dx, dy));
                }
            }
            
            Vector2Int[] offsetArray = newOffsets.ToArray();
            radiusOffsetCache[radius] = offsetArray;
            return offsetArray;
        }
        
        /// <summary>
        /// Get all entities in a radius around a world position. Returns a pooled list that must be returned.
        /// </summary>
        public List<IGridEntity> GetEntitiesInRadius(Vector3 worldPosition, float worldRadius)
        {
            Vector2Int centerPosition = WorldToGrid(worldPosition);
            int gridRadius = Mathf.CeilToInt(worldRadius / CellSize);
            
            return GetEntitiesInRadius(centerPosition.x, centerPosition.y, gridRadius);
        }
        
        /// <summary>
        /// Get all entities of a specific type within a radius. Returns a pooled list that must be returned.
        /// </summary>
        public List<IGridEntity> GetEntitiesOfTypeInRadius(Vector3 worldPosition, float worldRadius, EntityType entityType)
        {
            List<IGridEntity> result = ListPool<IGridEntity>.Get();
            var allEntities = GetEntitiesInRadius(worldPosition, worldRadius);
            
            try
            {
                foreach (var entity in allEntities)
                {
                    if (entity.EntityType == entityType)
                    {
                        result.Add(entity);
                    }
                }
            }
            finally
            {
                // Return the source list to the pool
                allEntities.ReturnToPool();
            }
            
            return result;
        }
        
        /// <summary>
        /// Fast check if any entity of a specific type exists within a radius.
        /// More efficient than getting all entities when you just need to check existence.
        /// </summary>
        public bool AnyEntityOfTypeInRadius(Vector3 worldPosition, float worldRadius, EntityType entityType)
        {
            Vector2Int centerPosition = WorldToGrid(worldPosition);
            int gridRadius = Mathf.CeilToInt(worldRadius / CellSize);
            
            // Get cached offsets
            Vector2Int[] offsets = GetRadiusOffsets(gridRadius);
            
            foreach (var offset in offsets)
            {
                Vector2Int cellPos = centerPosition + offset;
                
                if (!activeGridCells.Contains(cellPos))
                    continue;
                    
                if (entitiesInCell.TryGetValue(cellPos, out var entities))
                {
                    foreach (var entity in entities)
                    {
                        if (entity.EntityType == entityType)
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
    }
}