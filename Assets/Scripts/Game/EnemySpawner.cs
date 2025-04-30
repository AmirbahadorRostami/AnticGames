using UnityEngine;
using TacticalGame.Events;

namespace TacticalGame.Game
{
    /// <summary>
    /// Spawns enemy units at runtime when the game starts.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Enemy Settings")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private float spawnDelay = 1f;
        [SerializeField] private float spawnDistanceFromFlag = 3f; // Distance to spawn away from flag
        
        [Header("References")]
        [SerializeField] private Transform flagTransform;
        
        [Header("Debug")]
        [SerializeField] private bool debugLog = true;
        
        private GameEventManager eventManager;
        private bool enemySpawned = false;
        
        private void Start()
        {
            // Find event manager
            eventManager = FindObjectOfType<GameEventManager>();
            if (eventManager != null)
            {
                eventManager.OnGameStart += SpawnEnemy;
                
                if (debugLog)
                    Debug.Log("[EnemySpawner] Registered with game start event");
            }
            else
            {
                if (debugLog)
                    Debug.Log("[EnemySpawner] No event manager found, will spawn after delay");
                    
                // Fallback: spawn after delay
                Invoke("SpawnEnemy", spawnDelay);
            }
            
            // Find flag if not set
            if (flagTransform == null)
            {
                GameObject flag = GameObject.FindGameObjectWithTag("Flag");
                if (flag != null)
                {
                    flagTransform = flag.transform;
                    
                    if (debugLog)
                        Debug.Log($"[EnemySpawner] Found flag at {flagTransform.position}");
                }
                else
                {
                    Debug.LogError("[EnemySpawner] No flag found in scene!");
                }
            }
            
            // Debug check for all flags in the scene
            GameObject[] flagObjects = GameObject.FindGameObjectsWithTag("Flag");
            Debug.Log($"[EnemySpawner] Found {flagObjects.Length} objects with tag 'Flag' in the scene:");
            foreach (GameObject flagObj in flagObjects)
            {
                Debug.Log($"[EnemySpawner] Flag object: {flagObj.name} at {flagObj.transform.position}");
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (eventManager != null)
            {
                eventManager.OnGameStart -= SpawnEnemy;
            }
        }
        
        /// <summary>
        /// Spawns the enemy unit at an offset from the flag position
        /// </summary>
        public void SpawnEnemy()
        {
            if (enemySpawned || enemyPrefab == null || flagTransform == null)
                return;
                
            if (debugLog)
                Debug.Log("[EnemySpawner] Spawning enemy near flag position");
                
            // Calculate spawn position with an offset from the flag
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized;
            
            Vector3 spawnPosition = flagTransform.position + (randomDirection * spawnDistanceFromFlag);
            spawnPosition.y += 0.5f; // Adjust y position to be above ground
            
            // Spawn enemy
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            enemy.name = "Enemy";
            
            if (debugLog)
                Debug.Log($"[EnemySpawner] Enemy spawned at {spawnPosition} (offset from flag)");
                
            enemySpawned = true;
        }
    }
}