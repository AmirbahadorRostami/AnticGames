using UnityEngine;
using System.Collections.Generic;
using TacticalGame.Grid;
using TacticalGame.ScriptableObjects;
using TacticalGame.Utilities;

namespace TacticalGame.Units.Types
{
    /// <summary>
    /// Enemy unit that patrols and attacks using the grid system.
    /// Prioritizes attacking units that are closest to the flag.
    /// </summary>
    public class AntPatroller : BaseUnit
    {
        [Header("Search Settings")]
        [SerializeField] private float searchRadius = 10f;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackDamage = 50f;
        [SerializeField] private float searchInterval = 1.0f;
        [SerializeField] private float targetEvaluationCooldown = 0.5f;
        
        [Header("Optimization Settings")]
        [SerializeField] private int maxPotentialTargets = 16; // Limit target evaluation
        [SerializeField] private float distanceCheckOptimization = 0.2f; // Square this for distance checks
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        private BaseUnit currentTarget;
        private Vector3 patrolPoint;
        private bool isPatrolling = true;
        private bool isAttacking = false;
        private float nextSearchTime = 0f;
        private float nextTargetEvaluationTime = 0f;
        private float baseSearchRadius;
        private float baseAttackDamage;
        private GameConfig gameConfig;
        private Vector3 flagPosition;
        private GridManager gridManager;
        
        // Pre-allocate collections to prevent GC allocations
        private List<IGridEntity> potentialTargets;
        
        // Cache squared values for faster distance checks
        private float searchRadiusSqr;
        private float attackRangeSqr;
        
        // Cache transform to avoid GetComponent calls
        private Transform cachedTransform;
        
        // Entity type cache for faster queries
        private static readonly EntityType[] targetEntityTypes = { 
            EntityType.Beetles, 
            EntityType.Aphid, 
            EntityType.Ladybug 
        };
        
        // Optimization flags
        private bool needsPathRecalculation = false;

        protected override void Start()
        {
            base.Start();
            
            // Cache references
            gameConfig = gameManager?.GetGameConfig();
            gridManager = GridManager.Instance;
            cachedTransform = transform;
            
            // Store base values for difficulty scaling
            baseSearchRadius = searchRadius;
            baseAttackDamage = attackDamage;
            
            // Pre-compute squared distances for faster checks
            searchRadiusSqr = searchRadius * searchRadius;
            attackRangeSqr = attackRange * attackRange;
            
            // Initialize collections
            potentialTargets = new List<IGridEntity>(maxPotentialTargets);
            
            // Get flag position
            if (targetTransform != null)
            {
                flagPosition = targetTransform.position;
                SetRandomPatrolPoint();
            }
            
            StartMoving();
            
            // Subscribe to grid events
            if (gridManager?.Grid != null)
            {
                gridManager.Grid.OnEntityRegistered += OnEntityRegistered;
                gridManager.Grid.OnEntityMoved += OnEntityMoved;
                gridManager.Grid.OnEntityUnregistered += OnEntityUnregistered;
            }
        }

        private void OnEntityRegistered(Vector2Int pos, IGridEntity entity)
        {
            // Fast early rejection
            if (!IsValidTargetType(entity.EntityType))
                return;
                
            // If we don't have a target yet, evaluate this entity
            if (currentTarget == null)
            {
                EvaluateTarget(entity as BaseUnit);
            }
            // Otherwise check if this new entity is closer to the flag than current target
            else if (entity is BaseUnit unit)
            {
                // Compare distances - use sqrMagnitude for performance
                Vector3 currentTargetToFlag = currentTarget.transform.position - flagPosition;
                Vector3 newUnitToFlag = unit.transform.position - flagPosition;
                
                if (newUnitToFlag.sqrMagnitude < currentTargetToFlag.sqrMagnitude)
                {
                    SetTargetUnit(unit);
                }
            }
        }

        private void OnEntityMoved(Vector2Int oldPos, Vector2Int newPos, IGridEntity entity)
        {
            // Quick check if it's our current target
            if (currentTarget != null && entity == currentTarget)
            {
                // Mark for path recalculation on next frame
                needsPathRecalculation = true;
                
                // Don't waste CPU checking attack range - will be done in Update
            }
            // Only consider if we have no target and it's a valid type
            else if (currentTarget == null && IsValidTargetType(entity.EntityType))
            {
                EvaluateTarget(entity as BaseUnit);
            }
        }
        
        private void OnEntityUnregistered(Vector2Int pos, IGridEntity entity)
        {
            // If our target was unregistered, clear it
            if (currentTarget != null && entity == currentTarget)
            {
                currentTarget = null;
                isAttacking = false;
                SetRandomPatrolPoint();
                isPatrolling = true;
            }
        }

        protected override void Update()
        {
            
            base.Update();

            // Handle path recalculation if needed (deferred from event)
            if (needsPathRecalculation && currentTarget != null)
            {
                SetTarget(currentTarget.transform.position);
                CheckAttackRange();
                needsPathRecalculation = false;
            }

            // Periodically search for better targets
            if (Time.time >= nextTargetEvaluationTime)
            {
                EvaluateBetterTargets();
                nextTargetEvaluationTime = Time.time + targetEvaluationCooldown;
            }

            // Fallback search when we don't have a target
            if (currentTarget == null && Time.time >= nextSearchTime)
            {
                GridBasedSearch();
                nextSearchTime = Time.time + searchInterval;
            }

            // Attack logic - optimized to check only when needed
            if (currentTarget != null && isAttacking)
            {
                // Apply damage
                currentTarget.TakeDamage(attackDamage * Time.deltaTime);
            }
        }
        
        // Quick check if an entity type is one we care about
        private bool IsValidTargetType(EntityType type)
        {
            return type == EntityType.Beetles || 
                   type == EntityType.Aphid || 
                   type == EntityType.Ladybug;
        }
        
        private void GridBasedSearch()
        {
            if (gridManager == null || !isAlive)
                return;
                
            BaseUnit closestToFlag = null;
            float closestDistanceToFlagSqr = float.MaxValue;
            
            // Process target types in priority order
            foreach (var entityType in targetEntityTypes)
            {
                var entities = gridManager.GetEntitiesOfTypeInRadius(cachedTransform.position, searchRadius, entityType);
                
                try
                {
                    foreach (var entity in entities)
                    {
                        if (entity is BaseUnit unit)
                        {
                            Vector3 toFlag = unit.transform.position - flagPosition;
                            float distanceToFlagSqr = toFlag.sqrMagnitude;
                            
                            if (distanceToFlagSqr < closestDistanceToFlagSqr)
                            {
                                closestToFlag = unit;
                                closestDistanceToFlagSqr = distanceToFlagSqr;
                            }
                        }
                    }
                }
                finally
                {
                    entities.ReturnToPool();
                }
                
                // Early stopping if we found a target
                if (closestToFlag != null)
                    break;
            }
            
            // Set the target to the unit closest to the flag
            if (closestToFlag != null)
            {
                SetTargetUnit(closestToFlag);
            }
            else if (!isPatrolling)
            {
                // If no target found and not patrolling, return to patrolling
                SetRandomPatrolPoint();
                isPatrolling = true;
                StartMoving();
            }
        }
        
        private void EvaluateBetterTargets()
        {
            if (currentTarget == null || gridManager == null || !isAlive)
                return;
                
            Vector3 currentToFlag = currentTarget.transform.position - flagPosition;
            float currentDistanceToFlagSqr = currentToFlag.sqrMagnitude;
            
            BaseUnit betterTarget = null;
            float betterTargetDistanceSqr = currentDistanceToFlagSqr;
            
            // Clear and reuse the shared list
            potentialTargets.Clear();
            
            // Optimization: Only check if something is in range before doing expensive queries
            foreach (var entityType in targetEntityTypes)
            {
                // Fast check without creating lists
                if (gridManager.AnyEntityOfTypeInRadius(cachedTransform.position, searchRadius, entityType))
                {
                    var entities = gridManager.GetEntitiesOfTypeInRadius(cachedTransform.position, searchRadius, entityType);
                    try
                    {
                        // Only collect up to max capacity to avoid allocations
                        foreach (var entity in entities)
                        {
                            if (potentialTargets.Count < maxPotentialTargets)
                            {
                                potentialTargets.Add(entity);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    finally
                    {
                        entities.ReturnToPool();
                    }
                }
            }
            
            // Find the best target
            foreach (var entity in potentialTargets)
            {
                if (entity == this || entity is AntPatroller)
                    continue;
                    
                if (entity is BaseUnit unit)
                {
                    Vector3 unitToFlag = unit.transform.position - flagPosition;
                    float distanceToFlagSqr = unitToFlag.sqrMagnitude;
                    
                    if (distanceToFlagSqr < betterTargetDistanceSqr)
                    {
                        betterTarget = unit;
                        betterTargetDistanceSqr = distanceToFlagSqr;
                    }
                }
            }
            
            // Switch to better target if found
            if (betterTarget != null && betterTarget != currentTarget)
            {
                SetTargetUnit(betterTarget);
            }
        }

        /// <summary>
        /// Simplified target evaluation that uses squared distance.
        /// </summary>
        private void EvaluateTarget(BaseUnit unit)
        {
            if (unit == null)
                return;
                
            Vector3 toUnit = unit.transform.position - cachedTransform.position;
            float distanceToUnitSqr = toUnit.sqrMagnitude;
            
            if (distanceToUnitSqr <= searchRadiusSqr)
            {
                if (currentTarget == null)
                {
                    SetTargetUnit(unit);
                }
                else
                {
                    // Compare square distances
                    Vector3 currentToFlag = currentTarget.transform.position - flagPosition;
                    Vector3 newToFlag = unit.transform.position - flagPosition;
                    
                    if (newToFlag.sqrMagnitude < currentToFlag.sqrMagnitude)
                    {
                        SetTargetUnit(unit);
                    }
                }
            }
        }

        /// <summary>
        /// Set target and start pursuit with minimal allocation.
        /// </summary>
        private void SetTargetUnit(BaseUnit target)
        {
            if (target == null || target == currentTarget)
                return;
                
            currentTarget = target;
            isPatrolling = false;
            isAttacking = false;
            
            // Set movement target
            SetTarget(target.transform.position);
            StartMoving();
            
            // Check if already in attack range
            CheckAttackRange();
        }

        /// <summary>
        /// Check attack range using squared distance.
        /// </summary>
        private void CheckAttackRange()
        {
            if (currentTarget == null)
                return;
                
            Vector3 toTarget = currentTarget.transform.position - cachedTransform.position;
            float distanceSqr = toTarget.sqrMagnitude;
            
            if (distanceSqr <= attackRangeSqr)
            {
                // Stop and attack
                StopMoving();
                isAttacking = true;
            }
            else if (isAttacking)
            {
                // Resume chasing
                isAttacking = false;
                StartMoving();
            }
        }

        /// <summary>
        /// Set random patrol point with minimal allocation.
        /// </summary>
        private void SetRandomPatrolPoint()
        {
            if (targetTransform == null)
                return;

            // Reuse patrolPoint to avoid allocation
            patrolPoint.x = flagPosition.x + Random.Range(-5f, 5f);
            patrolPoint.z = flagPosition.z + Random.Range(-5f, 5f);
            patrolPoint.y = flagPosition.y;

            // Set target to the random point
            SetTarget(patrolPoint);
        }
        
        /// <summary>
        /// Update parameter caches when difficulty changes.
        /// </summary>
        protected override void OnDifficultyChanged(int newDifficulty)
        {
            if (gameConfig != null)
            {
                float difficultyFactor = newDifficulty / 3f;
        
                // Update parameters
                searchRadius = baseSearchRadius * Mathf.Lerp(0.8f, 1.2f, difficultyFactor);
                attackDamage = baseAttackDamage * Mathf.Lerp(0.8f, 1.3f, difficultyFactor);
                
                // Recalculate squared values
                searchRadiusSqr = searchRadius * searchRadius;
                attackRangeSqr = attackRange * attackRange;
                
                // Update movement speed
                if (movementStrategy != null)
                {
                    movementStrategy.SetSpeed(unitConfig.moveSpeed * Mathf.Lerp(0.8f, 1.3f, difficultyFactor));
                }
            }
        }

        protected override void OnDestroy()
        {
            // Unsubscribe from grid events
            if (gridManager?.Grid != null)
            {
                gridManager.Grid.OnEntityRegistered -= OnEntityRegistered;
                gridManager.Grid.OnEntityMoved -= OnEntityMoved;
                gridManager.Grid.OnEntityUnregistered -= OnEntityUnregistered;
            }
            
            // Clear references to avoid memory leaks
            currentTarget = null;
            potentialTargets.Clear();
            potentialTargets = null;
            
            base.OnDestroy();
        }
    }
}