using UnityEngine;

namespace TacticalGame.Units.Movement
{
    /// <summary>
    /// Interface for movement strategies - different ways units can move.
    /// </summary>
    public interface IMovementStrategy
    {
        /// <summary>
        /// Initialize the movement strategy with a target.
        /// </summary>
        void Initialize(Transform movingObject, Vector3 targetPosition, float moveSpeed);
        
        /// <summary>
        /// Update the movement, return true if destination reached.
        /// </summary>
        bool UpdateMovement();
        
        /// <summary>
        /// Change the target position.
        /// </summary>
        void SetTarget(Vector3 targetPosition);
        
        /// <summary>
        /// Change the movement speed.
        /// </summary>
        void SetSpeed(float moveSpeed);
        
        /// <summary>
        /// Stop the movement.
        /// </summary>
        void Stop();
        
        /// <summary>
        /// Resume the movement.
        /// </summary>
        void Resume();
    }
}