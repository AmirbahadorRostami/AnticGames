using UnityEngine;
using TacticalGame.Events;
using TacticalGame.ScriptableObjects;

namespace TacticalGame.Game
{
    /// <summary>
    /// Manages game difficulty settings, allowing changes during gameplay.
    /// </summary>
    public class DifficultyManager : MonoBehaviour
    {
        public enum DifficultyLevel
        {
            Easy = 5,
            Medium = 3,
            Hard = 1
        }

        [Header("Configuration")]
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private DifficultyLevel currentDifficulty = DifficultyLevel.Medium;

        private GameEventManager eventManager;
        private int previousDifficulty;

        // Singleton instance
        public static DifficultyManager Instance { get; private set; }

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            eventManager = GameEventManager.Instance;
            
            // Initialize with current difficulty
            SetDifficulty(currentDifficulty);
        }

        /// <summary>
        /// Sets the game difficulty and updates all related systems.
        /// </summary>
        public void SetDifficulty(DifficultyLevel level)
        {
            previousDifficulty = gameConfig.difficulty;
            currentDifficulty = level;
            
            // Update the GameConfig difficulty value
            gameConfig.difficulty = (int)level;
            
            // Notify systems about difficulty change
            if (eventManager != null)
            {
                eventManager.DifficultyChanged((int)level);
            }
            
            Debug.Log($"Game difficulty changed to {level} (value: {(int)level})");
        }

        /// <summary>
        /// Returns the current difficulty level.
        /// </summary>
        public DifficultyLevel GetCurrentDifficulty()
        {
            return currentDifficulty;
        }

        /// <summary>
        /// Cycles to the next difficulty level.
        /// </summary>
        public void CycleDifficulty()
        {
            DifficultyLevel nextLevel = currentDifficulty switch
            {
                DifficultyLevel.Easy => DifficultyLevel.Medium,
                DifficultyLevel.Medium => DifficultyLevel.Hard,
                DifficultyLevel.Hard => DifficultyLevel.Easy,
                _ => DifficultyLevel.Medium
            };
            
            SetDifficulty(nextLevel);
        }
    }
}