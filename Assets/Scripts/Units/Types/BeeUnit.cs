using UnityEngine;
using System.Collections;

namespace TacticalGame.Units.Types
{
    /// <summary>
    /// Bee unit implementation. Bees move with smooth, curved flight paths.
    /// </summary>
    public class BeeUnit : BaseUnit
    {
        [Header("Bee-Specific Settings")]
        [SerializeField] private float hoverHeight = 1.5f;
        [SerializeField] private float hoverVariation = 0.5f;
        [SerializeField] private float zigzagFrequency = 1.5f;
        [SerializeField] private float zigzagAmplitude = 2.0f;
        
        private float hoverOffset;
        private Vector3 basePosition;
        private float zigzagOffset;

        protected override void Start()
        {
            base.Start();
            
            // Initialize random offsets for unique flight patterns
            hoverOffset = Random.Range(0f, 2f * Mathf.PI);
            zigzagOffset = Random.Range(0f, 2f * Mathf.PI);
            
            // Start hover animation
            StartCoroutine(HoverAnimation());
        }

        protected override void InitializeMovement()
        {
            base.InitializeMovement();
            
            // Bees use tween-based movement by default (via the factory)
            // But we could override it here if needed
        }

        private IEnumerator HoverAnimation()
        {
            while (isAlive)
            {
                // Apply hover effect (vertical bobbing)
                float hoverDelta = Mathf.Sin(Time.time * 2f + hoverOffset) * hoverVariation;
                float currentHeight = hoverHeight + hoverDelta;
                
                // Update base position for zigzag calculation
                basePosition = transform.position;
                Vector3 newPosition = basePosition;
                newPosition.y = currentHeight;
                
                // Apply zigzag effect (horizontal sinusoidal movement)
                if (isMoving)
                {
                    // Get forward direction
                    Vector3 forward = transform.forward;
                    Vector3 right = transform.right;
                    
                    // Calculate zigzag offset perpendicular to movement direction
                    float zigzag = Mathf.Sin(Time.time * zigzagFrequency + zigzagOffset) * zigzagAmplitude;
                    newPosition += right * zigzag;
                }
                
                // Apply the position directly, bypassing movement strategy
                // This is just for visual effect - the actual pathfinding still happens in the movement strategy
                transform.position = newPosition;
                
                yield return null;
            }
        }

        // Override the OnReachedDestination method to implement bee-specific behavior
        protected override void OnReachedDestination()
        {
            base.OnReachedDestination();
            
            // Bees hover around the flag when they reach it rather than disappearing
            StartCoroutine(HoverAroundFlag());
        }

        private IEnumerator HoverAroundFlag()
        {
            float radius = 2f;
            float speed = 1f;
            float startAngle = Random.Range(0f, 360f);
            Vector3 flagPosition = targetTransform.position;
            
            while (isAlive)
            {
                // Calculate orbit position
                float angle = startAngle + (Time.time * speed * 60f);
                float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
                float z = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
                
                // Set target position
                Vector3 orbitPosition = flagPosition + new Vector3(x, hoverHeight, z);
                
                // Update position with smooth interpolation
                transform.position = Vector3.Lerp(transform.position, orbitPosition, Time.deltaTime * 2f);
                
                // Look toward center of orbit
                Vector3 lookDirection = flagPosition - transform.position;
                lookDirection.y = 0;
                if (lookDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        Quaternion.LookRotation(lookDirection),
                        Time.deltaTime * 5f
                    );
                }
                
                yield return null;
            }
        }
    }
}