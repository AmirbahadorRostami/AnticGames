using UnityEngine;
using TacticalGame.Grid;
using TacticalGame.Events;
using TacticalGame.Game;
using TacticalGame.Units;
using TacticalGame.Units.Types;

namespace TacticalGame.Utilities
{
    /// <summary>
    /// Helper script for setting up the game scene.
    /// </summary>
    public class SceneSetup : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector2Int gridSize = new Vector2Int(50, 50);
        [SerializeField] private Vector3 gridOffset = Vector3.zero;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject gridManagerPrefab;
        [SerializeField] private GameObject gameEventManagerPrefab;
        [SerializeField] private GameObject gameManagerPrefab;
        [SerializeField] private GameObject unitFactoryPrefab;
        [SerializeField] private GameObject unitSpawnerPrefab;
        [SerializeField] private GameObject flagPrefab;
        [SerializeField] private GameObject enemyPrefab;
        
        [Header("Flag Settings")]
        [SerializeField] private Vector3 flagPosition = Vector3.zero;
        
        [Header("Enemy Settings")]
        [SerializeField] private bool spawnEnemy = true;

        /// <summary>
        /// Set up the necessary game components.
        /// Call this from the Unity Editor to initialize the game.
        /// </summary>
        [ContextMenu("Setup Game Scene")]
        public void SetupGameScene()
        {
            // Create managers
            CreateManagers();
            
            // Create flag
            GameObject flag = CreateFlag();
            
            // Create enemy
            if (spawnEnemy)
            {
                CreateEnemy(flag.transform.position);
            }
            
            Debug.Log("Game scene setup complete!");
        }
        
        private void CreateManagers()
        {
            // Create Grid Manager
            if (gridManagerPrefab != null && FindObjectOfType<GridManager>() == null)
            {
                GameObject gridManagerObj = Instantiate(gridManagerPrefab);
                gridManagerObj.name = "GridManager";
                
                GridManager gridManager = gridManagerObj.GetComponent<GridManager>();
                if (gridManager != null)
                {
                    // TODO: Configure grid manager properties
                    // Would typically be done through Inspector
                }
            }
            
            // Create Event Manager
            if (gameEventManagerPrefab != null && FindObjectOfType<GameEventManager>() == null)
            {
                GameObject eventManagerObj = Instantiate(gameEventManagerPrefab);
                eventManagerObj.name = "GameEventManager";
            }
            
            // Create Game Manager
            if (gameManagerPrefab != null && FindObjectOfType<GameManager>() == null)
            {
                GameObject gameManagerObj = Instantiate(gameManagerPrefab);
                gameManagerObj.name = "GameManager";
            }
            
            // Create Unit Factory
            if (unitFactoryPrefab != null && FindObjectOfType<UnitFactory>() == null)
            {
                GameObject unitFactoryObj = Instantiate(unitFactoryPrefab);
                unitFactoryObj.name = "UnitFactory";
            }
            
            // Create Unit Spawner
            if (unitSpawnerPrefab != null && FindObjectOfType<UnitSpawner>() == null)
            {
                GameObject unitSpawnerObj = Instantiate(unitSpawnerPrefab);
                unitSpawnerObj.name = "UnitSpawner";
            }
        }
        
        private GameObject CreateFlag()
        {
            if (flagPrefab == null)
            {
                Debug.LogError("Flag prefab not assigned!");
                return null;
            }
            
            // Check if flag already exists
            Flag existingFlag = FindObjectOfType<Flag>();
            if (existingFlag != null)
            {
                return existingFlag.gameObject;
            }
            
            // Create flag
            GameObject flagObj = Instantiate(flagPrefab, flagPosition, Quaternion.identity);
            flagObj.name = "Flag";
            
            return flagObj;
        }
        
        private GameObject CreateEnemy(Vector3 flagPosition)
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("Enemy prefab not assigned!");
                return null;
            }
            
            // Check if enemy already exists
            EnemyUnit existingEnemy = FindObjectOfType<EnemyUnit>();
            if (existingEnemy != null)
            {
                return existingEnemy.gameObject;
            }
            
            // Create enemy at flag position
            GameObject enemyObj = Instantiate(enemyPrefab, flagPosition, Quaternion.identity);
            enemyObj.name = "Enemy";
            
            return enemyObj;
        }
    }
}