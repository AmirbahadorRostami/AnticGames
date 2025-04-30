using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TacticalGame.Units.Movement
{
    /// <summary>
    /// Movement strategy that uses a tweening-like approach.
    /// Good for smooth, controlled movements with easing functions.
    /// </summary>
    public class TweenMovement : IMovementStrategy
    {
        private Transform movingTransform;
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private float moveSpeed;
        private bool isActive = true;
        private float arrivalThreshold = 0.1f;
        private float rotationSpeed = 360f; // Degrees per second
        
        private float travelTime;
        private float currentTime;
        private bool isMoving = false;
        
        // Movement curve for easing
        private AnimationCurve movementCurve = new AnimationCurve(
            new Keyframe(0, 0, 0, 1),
            new Keyframe(1, 1, 1, 0)
        );
        
        // Coroutine reference for movement
        private MonoBehaviour monoBehaviour;
        private Coroutine moveCoroutine;

        public void Initialize(Transform movingObject, Vector3 target, float speed)
        {
            movingTransform = movingObject;
            monoBehaviour = movingObject.GetComponent<MonoBehaviour>();
            
            if (monoBehaviour == null)
            {
                Debug.LogError("TweenMovement requires a MonoBehaviour component on the moving object for coroutines.");
                return;
            }
            
            targetPosition = target;
            moveSpeed = speed;
            isActive = true;
            
            // Begin movement immediately
            StartMovement();
        }

        private void StartMovement()
        {
            if (moveCoroutine != null)
            {
                monoBehaviour.StopCoroutine(moveCoroutine);
            }
            
            if (isActive && monoBehaviour != null)
            {
                moveCoroutine = monoBehaviour.StartCoroutine(MoveToTarget());
            }
        }

        private IEnumerator MoveToTarget()
        {
            startPosition = movingTransform.position;
            float distance = Vector3.Distance(startPosition, targetPosition);
            
            // Calculate travel time based on speed and distance
            travelTime = distance / moveSpeed;
            currentTime = 0;
            isMoving = true;
            
            while (currentTime < travelTime && isActive)
            {
                // Calculate normalized progress
                float t = currentTime / travelTime;
                float easedT = movementCurve.Evaluate(t);
                
                // Update position
                movingTransform.position = Vector3.Lerp(startPosition, targetPosition, easedT);
                
                // Update rotation
                Vector3 direction = (targetPosition - movingTransform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    movingTransform.rotation = Quaternion.RotateTowards(
                        movingTransform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime);
                }
                
                // Update time
                currentTime += Time.deltaTime;
                
                yield return null;
            }
            
            // Ensure we reach the exact target position
            if (isActive)
            {
                movingTransform.position = targetPosition;
            }
            
            isMoving = false;
        }

        public bool UpdateMovement()
        {
            // Check if we're already at the destination
            if (!isMoving && isActive)
            {
                return true;
            }
            
            // Movement is handled by the coroutine
            return false;
        }

        public void SetTarget(Vector3 target)
        {
            // Only start new movement if the target has changed significantly
            if (Vector3.Distance(targetPosition, target) > arrivalThreshold)
            {
                targetPosition = target;
                StartMovement();
            }
        }

        public void SetSpeed(float speed)
        {
            if (Mathf.Approximately(moveSpeed, speed))
                return;
            
            moveSpeed = speed;
            
            // If we're already moving, update the travel time and restart
            if (isMoving)
            {
                StartMovement();
            }
        }

        public void Stop()
        {
            isActive = false;
            if (moveCoroutine != null && monoBehaviour != null)
            {
                monoBehaviour.StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }
            isMoving = false;
        }

        public void Resume()
        {
            isActive = true;
            
            // If we were in the middle of a movement, restart it
            if (!isMoving)
            {
                StartMovement();
            }
        }
    }
}