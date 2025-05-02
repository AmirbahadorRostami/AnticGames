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
        private float randomAngle = 0f;
        private float randomDistance = 0f;
        private float randomX = 0f;
        private float randomZ = 0f;
        private Vector3 flagPosition; 
        
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
            flagPosition = flagTransform.position;
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
            
            randomAngle = Random.Range(0f, 360f);
            randomDistance = Random.Range(gameConfig.minSpawnDistance, gameConfig.maxSpawnDistance);
            randomX = Mathf.Cos(randomAngle * Mathf.Deg2Rad) * randomDistance;
            randomZ = Mathf.Sin(randomAngle * Mathf.Deg2Rad) * randomDistance;
            return new Vector3(flagPosition.x + randomX, flagPosition.y + spawnHeightOffset, flagPosition.z + randomZ);;
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