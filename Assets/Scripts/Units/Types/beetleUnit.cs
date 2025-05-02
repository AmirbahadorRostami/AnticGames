using UnityEngine;

namespace TacticalGame.Units.Types
{
    /// <summary>
    /// Beetles unit implementation. Ants are basic units with straightforward movement.
    /// </summary>
    public class BeetleUnit : BaseUnit
    {
        [Header("Beetles-Specific Settings")]
        [SerializeField] private float pathDirectness = 0.9f; // How directly the ant moves toward the target (0-1)
        
        private Vector3 lastMoveDirection;
        private float randomDirectionTimer;
        private float randomDirectionInterval = 2f; // How often to recalculate random direction

        protected override void Start()
        {
            base.Start();
            lastMoveDirection = (targetTransform.position - transform.position).normalized;
        }

        protected override void InitializeMovement()
        {
            base.InitializeMovement();
            
            // Ants use transform-based movement by default (via the factory)
            // But we could override it here if needed
        }

        protected override void Update()
        {
            // Update random movement direction periodically
            randomDirectionTimer -= Time.deltaTime;
            if (randomDirectionTimer <= 0f)
            {
                UpdateMoveDirection();
                randomDirectionTimer = randomDirectionInterval;
            }
            
            // Let the base class handle the actual movement
            base.Update();
        }

        private void UpdateMoveDirection()
        {
            if (targetTransform == null || !isMoving)
                return;
                
            // Get direct direction to target
            Vector3 directDirection = (targetTransform.position - transform.position).normalized;
            
            // Get random direction
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;
            
            // Blend between direct path and random direction based on pathDirectness
            Vector3 blendedDirection = Vector3.Lerp(randomDirection, directDirection, pathDirectness);
            
            // Update target position with slight deviation from direct path
            Vector3 currentPos = transform.position;
            Vector3 newTargetPos = targetTransform.position;
            
            // Only update if the movement strategy supports it
            if (movementStrategy != null)
            {
                // Project a point ahead in the blended direction
                Vector3 projectedPoint = currentPos + blendedDirection * 5f;
                
                // But keep the general direction toward the flag
                Vector3 adjustedTarget = Vector3.Lerp(projectedPoint, newTargetPos, 0.7f);
                
                // Update the movement strategy target
                movementStrategy.SetTarget(adjustedTarget);
                
                // Save last direction
                lastMoveDirection = blendedDirection;
            }
        }

        // Ants have a simple death animation
        protected override System.Collections.IEnumerator DeathAnimation()
        {
            // Flatten the ant
            float duration = 0.3f;
            float elapsed = 0f;
            Vector3 originalScale = transform.localScale;
            Vector3 squashedScale = new Vector3(originalScale.x * 1.5f, originalScale.y * 0.2f, originalScale.z * 1.5f);
            
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(originalScale, squashedScale, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Hold for a moment
            yield return new WaitForSeconds(0.2f);
            
            // Fade out
            duration = 0.5f;
            elapsed = 0f;
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                foreach (Renderer renderer in renderers)
                {
                    Color color = renderer.material.color;
                    color.a = 1 - t;
                    renderer.material.color = color;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Destroy the GameObject
            Destroy(gameObject);
        }
    }
}