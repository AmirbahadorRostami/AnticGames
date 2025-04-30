using UnityEngine;

namespace TacticalGame.Units
{
    /// <summary>
    /// Provides public methods for controlling a unit.
    /// </summary>
    public partial class BaseUnit
    {
        /// <summary>
        /// Change the unit's target.
        /// </summary>
        public virtual void SetTarget(Transform newTarget)
        {
            targetTransform = newTarget;
            
            if (movementStrategy != null && targetTransform != null)
            {
                movementStrategy.SetTarget(targetTransform.position);
            }
        }
        
        /// <summary>
        /// Change the unit's target using a Vector3 position.
        /// </summary>
        public virtual void SetTarget(Vector3 targetPosition)
        {
            // Since the movement strategy can directly use positions, we can bypass the Transform
            if (movementStrategy != null)
            {
                movementStrategy.SetTarget(targetPosition);
            }
        }

        /// <summary>
        /// Change the unit's movement speed.
        /// </summary>
        public virtual void SetSpeed(float newSpeed)
        {
            if (movementStrategy != null)
            {
                movementStrategy.SetSpeed(newSpeed);
            }
        }

        /// <summary>
        /// Start or resume movement.
        /// </summary>
        public virtual void StartMoving()
        {
            isMoving = true;
            
            if (movementStrategy != null)
            {
                movementStrategy.Resume();
            }
        }

        /// <summary>
        /// Stop movement.
        /// </summary>
        public virtual void StopMoving()
        {
            isMoving = false;
            
            if (movementStrategy != null)
            {
                movementStrategy.Stop();
            }
        }

        /// <summary>
        /// Get the current health percentage.
        /// </summary>
        public float GetHealthPercentage()
        {
            return currentHealth / unitConfig.maxHealth;
        }

        /// <summary>
        /// Get the point value of this unit.
        /// </summary>
        public int GetPointValue()
        {
            return unitConfig.pointValue;
        }
    }
}