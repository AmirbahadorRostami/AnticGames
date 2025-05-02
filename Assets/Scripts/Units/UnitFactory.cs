using UnityEngine;
using TacticalGame.Grid;
using TacticalGame.ScriptableObjects;

namespace TacticalGame.Units
{
    /// <summary>
    /// Factory for creating different unit types.
    /// Implements the factory pattern for unit creation.
    /// </summary>
    public class UnitFactory : MonoBehaviour
    {
        [Header("Unit Configurations")]
        [SerializeField] private UnitConfig antConfig;
        [SerializeField] private UnitConfig aphidConfig;
        [SerializeField] private UnitConfig beeConfig;
        
        [Header("Spawn Settings")]
        [SerializeField] private Transform flagTransform;
        [SerializeField] private GameConfig gameConfig;
        
        public static UnitFactory Instance { get; private set; }

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }

        /// <summary>
        /// Create a unit of the specified type at the specified position.
        /// </summary>
        public GameObject CreateUnit(EntityType unitType, Vector3 position)
        {
            UnitConfig config = GetConfigForType(unitType);
            
            if (config == null || config.unitPrefab == null)
            {
                Debug.LogError($"Missing configuration for unit type: {unitType}");
                return null;
            }
            
            // Instantiate the unit
            GameObject unitObject = Instantiate(config.unitPrefab, position, Quaternion.identity);
            
            // Ensure the unit looks toward the flag
            if (flagTransform != null)
            {
                Vector3 directionToFlag = (flagTransform.position - position).normalized;
                if (directionToFlag != Vector3.zero)
                {
                    unitObject.transform.rotation = Quaternion.LookRotation(directionToFlag);
                }
            }
            
            // Configure the unit
            BaseUnit unit = unitObject.GetComponent<BaseUnit>();
            if (unit != null)
            {
                // Additional unit-specific configuration could go here
            }
            else
            {
                Debug.LogError($"Unit prefab does not have a BaseUnit component: {config.unitPrefab.name}");
            }
            
            return unitObject;
        }

        /// <summary>
        /// Create a unit of a random type (weighted by spawn probabilities).
        /// </summary>
        public GameObject CreateRandomUnit(Vector3 position)
        {
            if (gameConfig == null)
            {
                Debug.LogError("Missing game configuration in UnitFactory");
                return null;
            }
            
            // Get total spawn weight
            float totalWeight = gameConfig.GetTotalSpawnWeight();
            float randomValue = Random.Range(0f, totalWeight);
            
            // Determine which unit type to spawn based on weights
            EntityType unitType;
            
            if (randomValue < gameConfig.antSpawnWeight)
            {
                unitType = EntityType.Beetles;
            }
            else if (randomValue < gameConfig.antSpawnWeight + gameConfig.aphidSpawnWeight)
            {
                unitType = EntityType.Aphid;
            }
            else
            {
                unitType = EntityType.Ladybug;
            }
            
            return CreateUnit(unitType, position);
        }

        /// <summary>
        /// Get the configuration for a specific unit type.
        /// </summary>
        private UnitConfig GetConfigForType(EntityType unitType)
        {
            switch (unitType)
            {
                case EntityType.Beetles:
                    return antConfig;
                case EntityType.Aphid:
                    return aphidConfig;
                case EntityType.Ladybug:
                    return beeConfig;
                default:
                    return null;
            }
        }
    }
}