using UnityEngine;
using TacticalGame.ScriptableObjects;
using TacticalGame.Units;
using TacticalGame.Events;

namespace TacticalGame.Game
{
    /// <summary>
    /// Handles spawning units during gameplay using frame-based timing instead of async/await.
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
        private float nextSpawnTime = 0f;

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

        private void Update()
        {
            // Frame-based spawn timing instead of coroutine or task delay
            if (isSpawning && Time.time >= nextSpawnTime)
            {
                SpawnRandomUnit();
                
                // Calculate next spawn time based on game config and difficulty
                float spawnRate = gameConfig.GetActualSpawnRate();
                nextSpawnTime = Time.time + spawnRate;
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
        }

        private void StartSpawning()
        {
            if (!isSpawning)
            {
                isSpawning = true;
                nextSpawnTime = Time.time; // Spawn the first unit immediately
                Debug.Log("[UnitSpawner] Started spawning");
            }
        }

        private void PauseSpawning()
        {
            isSpawning = false;
            Debug.Log("[UnitSpawner] Paused spawning");
        }

        private void ResumeSpawning()
        {
            if (!isSpawning)
            {
                isSpawning = true;
                nextSpawnTime = Time.time; // Spawn the next unit immediately on resume
                Debug.Log("[UnitSpawner] Resumed spawning");
            }
        }

        private void StopSpawning(bool isWin)
        {
            isSpawning = false;
            Debug.Log("[UnitSpawner] Stopped spawning");
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