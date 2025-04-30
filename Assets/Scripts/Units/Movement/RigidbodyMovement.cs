using UnityEngine;

namespace TacticalGame.Units.Movement
{
    /// <summary>
    /// Movement strategy that uses Rigidbody physics for movement.
    /// Good for more realistic movement with physics interactions.
    /// </summary>
    public class RigidbodyMovement : IMovementStrategy
    {
        private Transform movingTransform;
        private Rigidbody rb;
        private Vector3 targetPosition;
        private float moveSpeed;
        private bool isActive = true;
        private float arrivalThreshold = 0.1f;
        private float rotationSpeed = 360f; // Degrees per second
        private float maxVelocity;
        private float acceleration = 10f;
        private float deceleration = 15f;

        public void Initialize(Transform movingObject, Vector3 target, float speed)
        {
            movingTransform = movingObject;
            rb = movingObject.GetComponent<Rigidbody>();
            
            if (rb == null)
            {
                Debug.LogError("RigidbodyMovement requires a Rigidbody component on the moving object.");
                return;
            }
            
            targetPosition = target;
            moveSpeed = speed;
            maxVelocity = speed;
            isActive = true;
            
            // Configure rigidbody
            rb.isKinematic = false;
            rb.useGravity = false; // For 2D movement on a plane
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.drag = 1.0f; // Add some drag for smoother movement
        }

        public bool UpdateMovement()
        {
            if (!isActive || movingTransform == null || rb == null)
                return false;

            // Check if we've reached destination
            float distanceToTarget = Vector3.Distance(movingTransform.position, targetPosition);
            if (distanceToTarget <= arrivalThreshold)
            {
                // Apply braking force to stop at the target
                rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, deceleration * Time.deltaTime);
                
                // If velocity is very low, consider it stopped
                if (rb.velocity.magnitude < 0.1f)
                {
                    rb.velocity = Vector3.zero;
                    return true;
                }
                
                return false;
            }

            // Calculate direction to target
            Vector3 directionToTarget = (targetPosition - movingTransform.position).normalized;
            
            // Rotate towards the direction of movement
            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                movingTransform.rotation = Quaternion.RotateTowards(
                    movingTransform.rotation, 
                    targetRotation, 
                    rotationSpeed * Time.deltaTime);
            }
            
            // Calculate force to apply
            Vector3 desiredVelocity = directionToTarget * moveSpeed;
            Vector3 velocityChange = desiredVelocity - rb.velocity;
            
            // Apply force based on acceleration
            rb.AddForce(velocityChange * acceleration, ForceMode.Acceleration);
            
            // Clamp velocity to max speed
            if (rb.velocity.magnitude > maxVelocity)
            {
                rb.velocity = rb.velocity.normalized * maxVelocity;
            }
            
            return false; // Not at destination yet
        }

        public void SetTarget(Vector3 target)
        {
            targetPosition = target;
        }

        public void SetSpeed(float speed)
        {
            moveSpeed = speed;
            maxVelocity = speed;
        }

        public void Stop()
        {
            isActive = false;
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
            }
        }

        public void Resume()
        {
            isActive = true;
        }
    }
}