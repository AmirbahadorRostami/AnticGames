using UnityEngine;

namespace TacticalGame.Units.Movement
{
    /// <summary>
    /// Movement strategy that uses Transform.position directly.
    /// Simple and efficient for basic movement.
    /// </summary>
    public class TransformMovement : IMovementStrategy
    {
        private Transform movingTransform;
        private Vector3 targetPosition;
        private float moveSpeed;
        private bool isActive = true;
        private float arrivalThreshold = 0.1f;
        private float rotationSpeed = 360f; // Degrees per second

        public void Initialize(Transform movingObject, Vector3 target, float speed)
        {
            movingTransform = movingObject;
            targetPosition = target;
            moveSpeed = speed;
            isActive = true;
        }

        public bool UpdateMovement()
        {
            if (!isActive || movingTransform == null)
                return false;

            // Check if we've already reached the destination
            float distanceToTarget = Vector3.Distance(movingTransform.position, targetPosition);
            if (distanceToTarget <= arrivalThreshold)
                return true;

            // Calculate direction to target
            Vector3 directionToTarget = (targetPosition - movingTransform.position).normalized;
            
            // Rotate towards the target
            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                movingTransform.rotation = Quaternion.RotateTowards(
                    movingTransform.rotation, 
                    targetRotation, 
                    rotationSpeed * Time.deltaTime);
            }
            
            // Move towards the target
            movingTransform.position = Vector3.MoveTowards(
                movingTransform.position,
                targetPosition,
                moveSpeed * Time.deltaTime);
            
            return false; // Not at destination yet
        }

        public void SetTarget(Vector3 target)
        {
            targetPosition = target;
        }

        public void SetSpeed(float speed)
        {
            moveSpeed = speed;
        }

        public void Stop()
        {
            isActive = false;
        }

        public void Resume()
        {
            isActive = true;
        }
    }
}