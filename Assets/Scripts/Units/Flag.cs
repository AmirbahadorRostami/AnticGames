using UnityEngine;
using TacticalGame.Grid;
using TacticalGame.Events;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TacticalGame.Units
{
    /// <summary>
    /// Flag entity that detects units reaching it using the Grid system.
    /// Uses Task-based asynchronous pattern instead of coroutines for better performance.
    /// </summary>
    public class Flag : GridEntity
    {
        [Header("Flag Settings")]
        [SerializeField] private float checkRadius = 1.5f;
        [SerializeField] private int checkIntervalMs = 200; // Interval in milliseconds
        [SerializeField] private bool debugLogging = false;

        private GameEventManager eventManager;
        private HashSet<IGridEntity> processedEntities = new HashSet<IGridEntity>();
        private CancellationTokenSource cancellationTokenSource;
        private bool isCheckingActive = false;

        protected override void Start()
        {
            eventManager = GameEventManager.Instance;
            base.Start();
            
            // Start the grid check after a short delay
            StartGridCheck();
        }

        private async void StartGridCheck()
        {
            // Wait a short time to ensure grid system is initialized
            await Task.Delay(500);
            
            if (debugLogging)
            {
                Debug.Log($"[Flag] Starting grid-based entity detection at position {transform.position}");
            }
            
            // Create a new cancellation token source
            cancellationTokenSource = new CancellationTokenSource();
            isCheckingActive = true;
            
            // Start the periodic grid check
            try
            {
                await CheckForEntitiesAsync(cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Task was canceled, clean up if needed
                if (debugLogging)
                {
                    Debug.Log("[Flag] Entity detection task was canceled");
                }
            }
        }

        private async Task CheckForEntitiesAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && isCheckingActive)
            {
                // Make sure we're running on the main thread for Unity operations
                await Task.Delay(checkIntervalMs, cancellationToken);
                
                // Safety check in case the object was destroyed during the delay
                if (this == null || !isActiveAndEnabled)
                    return;
                
                CheckForEntitiesInRadius();
            }
        }

        private void CheckForEntitiesInRadius()
        {
            if (GridManager.Instance == null || GridManager.Instance.Grid == null)
            {
                if (debugLogging)
                {
                    Debug.LogWarning("[Flag] Grid system not available for entity detection");
                }
                return;
            }

            // Get all entities in radius
            List<IGridEntity> entitiesInRadius = GridManager.Instance.Grid.GetEntitiesInRadius(transform.position, checkRadius);
            
            if (debugLogging && entitiesInRadius.Count > 0)
            {
                Debug.Log($"[Flag] Found {entitiesInRadius.Count} entities in detection radius");
            }
            
            foreach (IGridEntity entity in entitiesInRadius)
            {
                // Skip if we've already processed this entity
                if (processedEntities.Contains(entity))
                {
                    continue;
                }
                
                // Skip if the entity is an enemy
                if (entity.EntityType == EntityType.Enemy)
                {
                    continue;
                }
                
                // Skip if the entity is the flag itself
                if (entity == this)
                {
                    continue;
                }
                
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
                            Debug.Log($"[Flag] Unit reached flag via grid detection: {entityObject.name}, type: {entity.EntityType}");
                        }
                    }
                }
            }
        }

        protected override void OnDestroy()
        {
            // Cancel the task when destroyed
            StopGridCheck();
            base.OnDestroy();
        }

        private void OnDisable()
        {
            // Cancel the task when disabled
            StopGridCheck();
        }

        private void StopGridCheck()
        {
            isCheckingActive = false;
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
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