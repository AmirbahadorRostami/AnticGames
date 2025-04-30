using UnityEngine;
using System.Collections;
using TacticalGame.ScriptableObjects;
using TacticalGame.Units;
using TacticalGame.Events;

namespace TacticalGame.Game
{
    /// <summary>
    /// Handles spawning units during gameplay.
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
        }

        private void StartSpawning()
        {
            if (!isSpawning)
            {
                isSpawning = true;
                StartCoroutine(SpawnRoutine());
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
                StartCoroutine(SpawnRoutine());
            }
        }

        private void StopSpawning(bool isWin)
        {
            isSpawning = false;
        }

        private IEnumerator SpawnRoutine()
        {
            while (isSpawning)
            {
                // Get actual spawn rate based on difficulty
                float spawnRate = gameConfig.GetActualSpawnRate();
                
                // Wait for the spawn interval
                yield return new WaitForSeconds(spawnRate);
                
                // Spawn a random unit
                SpawnRandomUnit();
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