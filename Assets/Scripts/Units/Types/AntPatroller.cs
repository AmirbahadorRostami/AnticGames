using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TacticalGame.Grid;

namespace TacticalGame.Units.Types
{
    /// <summary>
    /// Simple enemy unit that patrols and attacks without
    /// total dependency on the grid system. Uses Task-based async pattern.
    /// </summary>
    public class AntPatroller : BaseUnit
    {
        [SerializeField] private float searchRadius = 10f;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackDamage = 50f;

        private BaseUnit currentTarget;
        private Vector3 patrolPoint;
        private bool isPatrolling = true;
        private CancellationTokenSource cts;

        protected override void Start()
        {
            base.Start();

            // Start with a delay to ensure systems are initialized
            cts = new CancellationTokenSource();
            _ = DelayedStartAsync(cts.Token);
        }

        private async Task DelayedStartAsync(CancellationToken token)
        {
            try
            {
                await Task.Delay(500, token); // Initial delay of 0.5 seconds

                Debug.Log("Enemy: Starting patrol and search");

                // Set initial patrol point at flag
                if (targetTransform != null)
                {
                    SetRandomPatrolPoint();
                }

                // Start moving
                StartMoving();

                // Start periodic searching
                _ = PeriodicSearchAsync(token);
            }
            catch (TaskCanceledException)
            {
                // Task was canceled, nothing to do
                Debug.Log("Enemy: Delayed start was canceled");
            }
        }

        private async Task PeriodicSearchAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    SearchForTargets();
                    await Task.Delay(500, token); // Search every 0.5 seconds
                }
            }
            catch (TaskCanceledException)
            {
                // Task was canceled, nothing to do
                Debug.Log("Enemy: Periodic search was canceled");
            }
        }

        private void SearchForTargets()
        {
            // Try to use grid if available
            if (GridManager.Instance != null && GridManager.Instance.Grid != null)
            {
                List<IGridEntity> nearbyEntities = GridManager.Instance.Grid.GetEntitiesInRadius(transform.position, searchRadius);
                Debug.Log($"Enemy: Grid search found {nearbyEntities.Count} entities");

                foreach (IGridEntity entity in nearbyEntities)
                {
                    if (entity is BaseUnit && !(entity is AntPatroller) && entity != this)
                    {
                        BaseUnit unit = entity as BaseUnit;
                        SetTargetUnit(unit);
                        return;
                    }
                }
            }

            // Fallback: direct search
            FallbackSearch();
        }

        private void FallbackSearch()
        {
            Debug.Log("Enemy: Using fallback search");

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

            Debug.Log($"Enemy: Found target {target.name}");
        }

        protected override void Update()
        {
            base.Update();

            // Attack logic
            if (currentTarget != null)
            {
                float distance = Vector3.Distance(transform.position, currentTarget.transform.position);

                if (distance <= attackRange)
                {
                    StopMoving();

                    // Apply damage immediately for testing
                    currentTarget.TakeDamage(attackDamage * Time.deltaTime);

                    Debug.Log($"Enemy: Attacking {currentTarget.name}");
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

            Debug.Log($"Enemy: New patrol point at {randomPoint}");
        }

        private void OnDisable()
        {
            // Cancel all running tasks when disabled
            cts?.Cancel();
        }

        private void OnDestroy()
        {
            // Clean up resources
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }
    }
}