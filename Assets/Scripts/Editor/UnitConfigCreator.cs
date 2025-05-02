#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TacticalGame.ScriptableObjects;

namespace TacticalGame.Editor
{
    /// <summary>
    /// Editor utility for creating unit configuration ScriptableObjects.
    /// </summary>
    public static class UnitConfigCreator
    {
        [MenuItem("TacticalGame/Create Default Unit Configs")]
        public static void CreateDefaultUnitConfigs()
        {
            // Create Beetles Config
            UnitConfig antConfig = ScriptableObject.CreateInstance<UnitConfig>();
            antConfig.unitName = "Beetles";
            antConfig.description = "Basic unit with simple movement patterns.";
            antConfig.moveSpeed = 3.0f;
            antConfig.maxHealth = 50.0f;
            antConfig.pointValue = 10;
            
            // Create Aphid Config
            UnitConfig aphidConfig = ScriptableObject.CreateInstance<UnitConfig>();
            aphidConfig.unitName = "Aphid";
            aphidConfig.description = "Physics-based unit with stop-and-go movement.";
            aphidConfig.moveSpeed = 2.5f;
            aphidConfig.maxHealth = 75.0f;
            aphidConfig.pointValue = 15;
            
            // Create Ladybug Config
            UnitConfig beeConfig = ScriptableObject.CreateInstance<UnitConfig>();
            beeConfig.unitName = "Ladybug";
            beeConfig.description = "Fast unit with smooth, curved flight paths.";
            beeConfig.moveSpeed = 4.0f;
            beeConfig.maxHealth = 40.0f;
            beeConfig.pointValue = 25;
            
            // Create Ant Config
            UnitConfig enemyConfig = ScriptableObject.CreateInstance<UnitConfig>();
            enemyConfig.unitName = "Ant";
            enemyConfig.description = "Ant unit that hunts and intercepts other units.";
            enemyConfig.moveSpeed = 3.5f;
            enemyConfig.maxHealth = 150.0f;
            enemyConfig.pointValue = 0;
            
            // Create folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            {
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            }
            
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Units"))
            {
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Units");
            }
            
            // Save the configs as assets
            AssetDatabase.CreateAsset(antConfig, "Assets/ScriptableObjects/Units/AntConfig.asset");
            AssetDatabase.CreateAsset(aphidConfig, "Assets/ScriptableObjects/Units/AphidConfig.asset");
            AssetDatabase.CreateAsset(beeConfig, "Assets/ScriptableObjects/Units/BeeConfig.asset");
            AssetDatabase.CreateAsset(enemyConfig, "Assets/ScriptableObjects/Units/EnemyConfig.asset");
            
            AssetDatabase.SaveAssets();
            
            // Create Game Config
            CreateGameConfig();
            
            Debug.Log("Created default unit configurations!");
        }
        
        private static void CreateGameConfig()
        {
            GameConfig gameConfig = ScriptableObject.CreateInstance<GameConfig>();
            gameConfig.unitsToDestroyToWin = 20;
            gameConfig.difficulty = 2;
            gameConfig.baseSpawnRate = 3.0f;
            
            // Create folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/GameSettings"))
            {
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "GameSettings");
            }
            
            // Save the config as an asset
            AssetDatabase.CreateAsset(gameConfig, "Assets/ScriptableObjects/GameSettings/DefaultGameConfig.asset");
            AssetDatabase.SaveAssets();
        }
    }
}
#endif