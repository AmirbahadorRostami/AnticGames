using UnityEngine;

namespace TacticalGame.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject containing global game settings and configurations.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "TacticalGame/Game Configuration")]
    public class GameConfig : ScriptableObject
    {
        [Header("Game Settings")]
        [Tooltip("Units to destroy to win the game")]
        public int unitsToDestroyToWin = 20;
        
        [Tooltip("Game difficulty setting")]
        [Range(1, 5)]
        public int difficulty = 2;

        [Header("Spawn Settings")]
        [Tooltip("Base spawn rate in seconds")]
        public float baseSpawnRate = 3f;
        
        [Tooltip("Minimum distance from flag for unit spawning")]
        public float minSpawnDistance = 15f;
        
        [Tooltip("Maximum distance from flag for unit spawning")]
        public float maxSpawnDistance = 25f;
        
        [Tooltip("Spawn rate multiplier based on difficulty")]
        public AnimationCurve spawnRateByDifficulty = new AnimationCurve(
            new Keyframe(1, 1.5f),  // Easier: Slower spawns
            new Keyframe(3, 1.0f),  // Normal
            new Keyframe(5, 0.6f)   // Harder: Faster spawns
        );

        [Header("Unit Type Weights")]
        [Tooltip("Probability weight for Beetles units")]
        [Range(0, 100)]
        public float antSpawnWeight = 60f;
        
        [Tooltip("Probability weight for Aphid units")]
        [Range(0, 100)]
        public float aphidSpawnWeight = 30f;
        
        [Tooltip("Probability weight for Ladybug units")]
        [Range(0, 100)]
        public float beeSpawnWeight = 10f;

        [Header("Ant Settings")]
        [Tooltip("Ant movement speed")]
        public float enemySpeed = 5f;
        
        [Tooltip("Ant search radius")]
        public float enemySearchRadius = 8f;
        
        [Tooltip("Time enemy waits after destroying a unit")]
        public float enemyCooldown = 0.5f;
        
        [Tooltip("Ant speed multiplier based on difficulty")]
        public AnimationCurve enemySpeedByDifficulty = new AnimationCurve(
            new Keyframe(1, 0.8f),  // Easier: Slower enemy
            new Keyframe(3, 1.0f),  // Normal
            new Keyframe(5, 1.3f)   // Harder: Faster enemy
        );
        
        // Calculate actual spawn rate based on difficulty
        public float GetActualSpawnRate()
        {
            return baseSpawnRate * spawnRateByDifficulty.Evaluate(difficulty);
        }
        
        // Calculate actual enemy speed based on difficulty
        public float GetActualEnemySpeed()
        {
            return enemySpeed * enemySpeedByDifficulty.Evaluate(difficulty);
        }
        
        // Get total spawn weight
        public float GetTotalSpawnWeight()
        {
            return antSpawnWeight + aphidSpawnWeight + beeSpawnWeight;
        }
    }
}