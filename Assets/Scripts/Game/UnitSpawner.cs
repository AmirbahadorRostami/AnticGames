using UnityEngine;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using TacticalGame.ScriptableObjects;
using TacticalGame.Units;
using TacticalGame.Events;

namespace TacticalGame.Game
{
    /// <summary>
    /// Handles spawning units during gameplay using async/await instead of coroutines.
    /// </summary>
    public class UnitSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private Transform flagTransform;
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private float spawnHeightOffset = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool showSpawnPoints = true;

        private bool isSpawning = false;
        private GameEventManager eventManager;
        private CancellationTokenSource cancellationTokenSource;

        private void Start()
        {
            eventManager = GameEventManager.Instance;

            if (eventManager != null)
            {
                eventManager.OnGameStart += StartSpawning;
                eventManager.OnGamePause += PauseSpawning;
                eventManager.OnGameResume += ResumeSpawning;
                eventManager.OnGameOver += StopSpawning;
            }
        }

        private void OnDestroy()
        {
            if (eventManager != null)
            {
                eventManager.OnGameStart -= StartSpawning;
                eventManager.OnGamePause -= PauseSpawning;
                eventManager.OnGameResume -= ResumeSpawning;
                eventManager.OnGameOver -= StopSpawning;
            }

            // Make sure to cancel any running task when the object is destroyed
            CancelSpawning();
        }

        private void StartSpawning()
        {
            if (!isSpawning)
            {
                isSpawning = true;

                // Create a new cancellation token source
                cancellationTokenSource = new CancellationTokenSource();

                // Start the spawn task
                _ = SpawnRoutineAsync(cancellationTokenSource.Token);
            }
        }

        private void PauseSpawning()
        {
            isSpawning = false;
        }

        private void ResumeSpawning()
        {
            if (!isSpawning)
            {
                isSpawning = true;

                // Create a new cancellation token source
                cancellationTokenSource = new CancellationTokenSource();

                // Start the spawn task
                _ = SpawnRoutineAsync(cancellationTokenSource.Token);
            }
        }

        private void StopSpawning(bool isWin)
        {
            CancelSpawning();
            isSpawning = false;
        }

        private void CancelSpawning()
        {
            // Cancel the current task if it's running
            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Creates a task that completes after the specified delay, while remaining on the main thread.
        /// </summary>
        private Task DelayOnMainThread(float seconds, CancellationToken cancellationToken)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            // Use Unity's own timing system instead of Task.Delay
            StartCoroutine(DelayCoroutine(seconds, tcs, cancellationToken));

            return tcs.Task;
        }

        private IEnumerator DelayCoroutine(float seconds, TaskCompletionSource<bool> tcs, CancellationToken cancellationToken)
        {
            yield return new WaitForSeconds(seconds);

            if (cancellationToken.IsCancellationRequested)
            {
                tcs.SetCanceled();
            }
            else
            {
                tcs.SetResult(true);
            }
        }

        private async Task SpawnRoutineAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (isSpawning && !cancellationToken.IsCancellationRequested)
                {
                    // Get actual spawn rate based on difficulty
                    float spawnRate = gameConfig.GetActualSpawnRate();

                    // Wait for the spawn interval using our custom delay that stays on the main thread
                    await DelayOnMainThread(spawnRate, cancellationToken);

                    // After the delay, we need to check if we're still spawning and not cancelled
                    if (isSpawning && !cancellationToken.IsCancellationRequested)
                    {
                        SpawnRandomUnit();
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Task was canceled, nothing to do here
                Debug.Log("Spawning task was canceled");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error in spawning task: {ex.Message}");
            }
        }

        private void SpawnRandomUnit()
        {
            if (UnitFactory.Instance == null || flagTransform == null)
                return;

            // Calculate spawn position
            Vector3 spawnPosition = GetRandomSpawnPosition();

            // Create the unit
            GameObject unit = UnitFactory.Instance.CreateRandomUnit(spawnPosition);

            if (unit != null)
            {
                Debug.Log($"Spawned unit at {spawnPosition}");
            }
        }

        private Vector3 GetRandomSpawnPosition()
        {
            if (flagTransform == null)
                return Vector3.zero;

            // Get random angle
            float angle = Random.Range(0f, 360f);

            // Get random distance
            float distance = Random.Range(gameConfig.minSpawnDistance, gameConfig.maxSpawnDistance);

            // Calculate position
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * distance;
            float z = Mathf.Sin(angle * Mathf.Deg2Rad) * distance;

            // Create spawn position relative to flag
            Vector3 flagPos = flagTransform.position;
            Vector3 spawnPos = new Vector3(flagPos.x + x, flagPos.y + spawnHeightOffset, flagPos.z + z);

            return spawnPos;
        }

        private void OnDrawGizmos()
        {
            if (!showSpawnPoints || flagTransform == null || gameConfig == null)
                return;

            // Draw inner spawn radius
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(flagTransform.position, gameConfig.minSpawnDistance);

            // Draw outer spawn radius
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(flagTransform.position, gameConfig.maxSpawnDistance);
        }
    }
}