using System.Collections;
using UnityEngine;
using TacticalGame.Events;

namespace TacticalGame.Units
{
    /// <summary>
    /// Handles core unit behavior like movement, life cycle, damage, etc.
    /// </summary>
    public partial class BaseUnit
    {
        protected override void Update()
        {
            base.Update();
            
            if (!isAlive)
                return;
                
            if (isMoving && movementStrategy != null)
            {
                bool reachedDestination = movementStrategy.UpdateMovement();
                
                if (reachedDestination)
                {
                    OnReachedDestination();
                }
            }
        }

        /// <summary>
        /// Called when the unit reaches its destination.
        /// </summary>
        protected virtual void OnReachedDestination()
        {
            // Unit reached the flag
            if (eventManager != null)
            {
                eventManager.UnitReachedFlag(gameObject);
            }
            
            // By default, stop moving once destination is reached
            isMoving = false;
        }

        /// <summary>
        /// Apply damage to this unit.
        /// </summary>
        public virtual void TakeDamage(float damageAmount)
        {
            if (!isAlive)
                return;
                
            currentHealth -= damageAmount;
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Kill this unit.
        /// </summary>
        public virtual void Die()
        {
            if (!isAlive)
                return;
                
            isAlive = false;
            
            // Stop movement
            if (movementStrategy != null)
            {
                movementStrategy.Stop();
            }
            
            // Notify about destruction
            if (eventManager != null)
            {
                eventManager.UnitDestroyed(gameObject);
            }
            
            // Unregister from grid
            UnregisterFromGrid();
            
            // Visual feedback
            StartCoroutine(DeathAnimation());
        }

        /// <summary>
        /// Play death animation/effect and then destroy the unit.
        /// </summary>
        protected virtual IEnumerator DeathAnimation()
        {
            // Simple scale down animation
            float duration = 0.5f;
            float elapsed = 0f;
            Vector3 originalScale = transform.localScale;
            
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Destroy the GameObject
            Destroy(gameObject);
        }
    }
}