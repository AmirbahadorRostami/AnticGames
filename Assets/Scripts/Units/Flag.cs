using UnityEngine;
using TacticalGame.Grid;
using TacticalGame.Events;
using System.Collections.Generic;

namespace TacticalGame.Units
{
    /// <summary>
    /// Flag entity that detects units reaching it using the Grid system's events.
    /// Uses event-based architecture instead of polling with delays.
    /// </summary>
    public class Flag : GridEntity
    {
        [Header("Flag Settings")]
        [SerializeField] private float checkRadius = 1.5f;
        [SerializeField] private bool debugLogging = false;

        private GameEventManager eventManager;
        private HashSet<IGridEntity> processedEntities = new HashSet<IGridEntity>();

        protected override void Start()
        {
            eventManager = GameEventManager.Instance;
            base.Start();
            
            // Subscribe to grid events instead of periodic checking
            if (GridManager.Instance?.Grid != null)
            {
                GridManager.Instance.Grid.OnEntityRegistered += OnEntityRegistered;
                GridManager.Instance.Grid.OnEntityMoved += OnEntityMoved;
                
                if (debugLogging)
                {
                    Debug.Log($"[Flag] Initialized with grid events at position {transform.position}");
                }
            }
            else
            {
                Debug.LogWarning("[Flag] Grid system not available - flag detection may not work properly");
            }
        }

        private void OnEntityRegistered(Vector2Int pos, IGridEntity entity)
        {
            CheckEntityInRadius(entity);
        }

        private void OnEntityMoved(Vector2Int oldPos, Vector2Int newPos, IGridEntity entity)
        {
            CheckEntityInRadius(entity);
        }

        private void CheckEntityInRadius(IGridEntity entity)
        {
            // Skip if we've already processed this entity
            if (processedEntities.Contains(entity))
                return;
                
            // Skip if the entity is an enemy
            if (entity.EntityType == EntityType.Ant)
                return;
                
            // Skip if the entity is the flag itself
            if (entity == this)
                return;
                
            // Check if within radius
            float distance = Vector3.Distance(transform.position, entity.WorldPosition);
            if (distance <= checkRadius)
            {
                // Get the GameObject from the entity
                GameObject entityObject = null;
                if (entity is GridEntity gridEntity)
                {
                    entityObject = gridEntity.gameObject;
                }
                
                if (entityObject != null)
                {
                    // Add to processed set to avoid multiple triggers
                    processedEntities.Add(entity);
                    
                    if (eventManager != null)
                    {
                        eventManager.UnitReachedFlag(entityObject);
                        
                        if (debugLogging)
                        {
                            Debug.Log($"[Flag] Unit reached flag via grid event: {entityObject.name}, type: {entity.EntityType}");
                        }
                    }
                }
            }
        }

        protected override void OnDestroy()
        {
            // Unsubscribe from grid events
            if (GridManager.Instance?.Grid != null)
            {
                GridManager.Instance.Grid.OnEntityRegistered -= OnEntityRegistered;
                GridManager.Instance.Grid.OnEntityMoved -= OnEntityMoved;
            }
            
            base.OnDestroy();
        }
        
        #if UNITY_EDITOR
        // Visual debugging
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, checkRadius);
        }
        #endif
    }
}