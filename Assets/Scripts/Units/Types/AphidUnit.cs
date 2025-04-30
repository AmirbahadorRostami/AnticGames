using UnityEngine;
using System.Collections;

namespace TacticalGame.Units.Types
{
    /// <summary>
    /// Aphid unit implementation. Aphids move with physics and have stop/start behavior.
    /// </summary>
    public class AphidUnit : BaseUnit
    {
        [Header("Aphid-Specific Settings")]
        [SerializeField] private float stopProbability = 0.1f; // Chance to randomly stop
        [SerializeField] private float minStopDuration = 0.5f;
        [SerializeField] private float maxStopDuration = 2.0f;
        [SerializeField] private float burstSpeedMultiplier = 1.5f;
        
        private bool isStopped = false;
        private float originalSpeed;
        private Coroutine stopRoutine;

        protected override void Start()
        {
            base.Start();
            originalSpeed = unitConfig.moveSpeed;
            
            // Start checking for random stops
            StartCoroutine(RandomStopCheck());
        }

        private IEnumerator RandomStopCheck()
        {
            // Wait a bit before starting to randomly stop
            yield return new WaitForSeconds(2f);
            
            while (isAlive)
            {
                // Check if we should stop
                if (!isStopped && isMoving && Random.value < stopProbability)
                {
                    StartRandomStop();
                }
                
                // Check every second
                yield return new WaitForSeconds(1f);
            }
        }

        private void StartRandomStop()
        {
            if (stopRoutine != null)
            {
                StopCoroutine(stopRoutine);
            }
            
            stopRoutine = StartCoroutine(RandomStopRoutine());
        }

        private IEnumerator RandomStopRoutine()
        {
            // Stop moving
            isStopped = true;
            StopMoving();
            
            // Wait for random duration
            float stopDuration = Random.Range(minStopDuration, maxStopDuration);
            yield return new WaitForSeconds(stopDuration);
            
            // Resume movement with burst speed
            SetSpeed(originalSpeed * burstSpeedMultiplier);
            isStopped = false;
            StartMoving();
            
            // Return to normal speed after burst
            yield return new WaitForSeconds(1.0f);
            SetSpeed(originalSpeed);
        }
    }
}