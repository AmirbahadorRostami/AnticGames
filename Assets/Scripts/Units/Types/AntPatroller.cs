using UnityEngine;
using System.Collections.Generic;
using TacticalGame.Grid;
using TacticalGame.ScriptableObjects;

namespace TacticalGame.Units.Types
{
    /// <summary>
    /// Enemy unit that patrols and attacks using event-driven grid system
    /// rather than periodic task-based searching.
    /// </summary>
    public class AntPatroller : BaseUnit
    {
        [SerializeField] private float searchRadius = 10f;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackDamage = 50f;
        [SerializeField] private float searchInterval = 0.5f;  // Still use for fallback searches

        private BaseUnit currentTarget;
        private Vector3 patrolPoint;
        private bool isPatrolling = true;
        private float nextSearchTime = 0f;
        private float baseSearchRadius;
        private float baseAttackDamage;
        private GameConfig gameConfig;

        protected override void Start()
        {
            base.Start();
            
            gameConfig = gameManager.GetGameConfig(); // Add a getter method to GameManager
            baseSearchRadius = searchRadius;
            baseAttackDamage = attackDamage;
            if (targetTransform != null)
            {
                SetRandomPatrolPoint();
            }
            StartMoving();
            if (GridManager.Instance?.Grid != null)
            {
                GridManager.Instance.Grid.OnEntityRegistered += OnEntityRegistered;
                GridManager.Instance.Grid.OnEntityMoved += OnEntityMoved;
                Debug.Log("Ant: Subscribed to grid events");
            }
        }

        private void OnEntityRegistered(Vector2Int pos, IGridEntity entity)
        {
            CheckPotentialTarget(entity);
        }

        private void OnEntityMoved(Vector2Int oldPos, Vector2Int newPos, IGridEntity entity)
        {
            CheckPotentialTarget(entity);
        }

        private void CheckPotentialTarget(IGridEntity entity)
        {
            // Skip self and other enemies
            if (entity is AntPatroller || entity == this)
                return;
                
            // Skip flag
            if (entity.EntityType == EntityType.Flag)
                return;
                
            // Check if entity is a valid target unit
            if (entity is BaseUnit unit)
            {
                // Check if within search radius
                float distance = Vector3.Distance(transform.position, entity.WorldPosition);
                if (distance <= searchRadius)
                {
                    // Only switch targets if we don't have one or if this one is closer
                    if (currentTarget == null)
                    {
                        SetTargetUnit(unit);
                    }
                    else
                    {
                        float currentDistance = Vector3.Distance(transform.position, currentTarget.transform.position);
                        if (distance < currentDistance)
                        {
                            SetTargetUnit(unit);
                        }
                    }
                }
            }
        }

        protected override void Update()
        {
            base.Update();

            // Fallback search when using the grid events isn't sufficient
            if (currentTarget == null && Time.time >= nextSearchTime)
            {
                FallbackSearch();
                nextSearchTime = Time.time + searchInterval;
            }

            // Attack logic
            if (currentTarget != null)
            {
                float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

                if (distance <= attackRange)
                {
                    StopMoving();

                    // Apply damage immediately for testing
                    currentTarget.TakeDamage(attackDamage * Time.deltaTime);

                    Debug.Log($"Ant: Attacking {currentTarget.name}");
                }
                else
                {
                    StartMoving();
                }

                // If target is destroyed or gone
                if (!currentTarget.gameObject.activeInHierarchy)
                {
                    currentTarget = null;
                    SetRandomPatrolPoint();
                    isPatrolling = true;
                }
            }
        }
        
        protected override void OnDifficultyChanged(int newDifficulty)
        {
            // Update search and attack parameters based on difficulty
            // Higher difficulty = larger search radius, more damage
            if (gameConfig != null)
            {
                float difficultyFactor = newDifficulty / 3f; // Normalize to 0.33 - 1.67
        
                // Adjust search radius
                searchRadius = baseSearchRadius * Mathf.Lerp(0.8f, 1.2f, difficultyFactor);
        
                // Adjust attack damage
                attackDamage = baseAttackDamage * Mathf.Lerp(0.8f, 1.3f, difficultyFactor);
        
                // Adjust movement speed through the movement strategy
                float speedAdjustment = Mathf.Lerp(0.8f, 1.3f, difficultyFactor);
                if (movementStrategy != null)
                {
                    movementStrategy.SetSpeed(unitConfig.moveSpeed * speedAdjustment);
                }
        
                Debug.Log($"AntPatroller: Adjusted for difficulty {newDifficulty}");
            }
        }

        private void FallbackSearch()
        {
            Debug.Log("Ant: Using fallback search");

            // Direct find
            BaseUnit[] allUnits = FindObjectsOfType<BaseUnit>();

            foreach (BaseUnit unit in allUnits)
            {
                // Skip self and other enemies
                if (unit == this || unit is AntPatroller)
                    continue;

                float distance = Vector3.Distance(transform.position, unit.transform.position);
                if (distance <= searchRadius)
                {
                    SetTargetUnit(unit);
                    return;
                }
            }

            // No target found, continue patrol
            if (currentTarget == null && !isPatrolling)
            {
                SetRandomPatrolPoint();
                isPatrolling = true;
                StartMoving();
            }
        }

        private void SetTargetUnit(BaseUnit target)
        {
            currentTarget = target;
            base.SetTarget(target.transform.position);
            isPatrolling = false;
            StartMoving();

            Debug.Log($"Ant: Found target {target.name}");
        }

        private void SetRandomPatrolPoint()
        {
            if (targetTransform == null)
                return;

            // Get random point around flag
            Vector2 randomCircle = Random.insideUnitCircle * 5f;
            Vector3 randomPoint = targetTransform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            // Use the Vector3 overload of SetTarget
            base.SetTarget(randomPoint);

            // Store patrol point for reference
            patrolPoint = randomPoint;

            Debug.Log($"Ant: New patrol point at {randomPoint}");
        }

        private void OnDestroy()
        {
            // Unsubscribe from grid events
            if (GridManager.Instance?.Grid != null)
            {
                GridManager.Instance.Grid.OnEntityRegistered -= OnEntityRegistered;
                GridManager.Instance.Grid.OnEntityMoved -= OnEntityMoved;
            }
        }
    }
}