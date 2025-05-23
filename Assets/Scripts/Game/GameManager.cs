using UnityEngine;
using TacticalGame.Events;
using TacticalGame.ScriptableObjects;
using TacticalGame.Units;

namespace TacticalGame.Game
{
    /// <summary>
    /// Manages overall game state and win/lose conditions.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;

        [Header("Game Settings")]
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private Transform flagTransform;

        
        private GameEventManager eventManager;
        private int currentScore = 0;
        private int unitsDestroyed = 0;
        private bool gameActive = false;
        
        public static GameManager Instance { get; private set; }
        public Transform FlagTransform { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            
            if(flagTransform != null)
            {
                FlagTransform = flagTransform;
            }
        }
        
        private void Start()
        {
            eventManager = GameEventManager.Instance;
            
            if (eventManager != null)
            {
                eventManager.OnUnitDestroyed += HandleUnitDestroyed;
                eventManager.OnUnitReachedFlag += HandleUnitReachedFlag;
                eventManager.OnDifficultyChanged += OnDifficultyChanged;
            }
            
            gameActive = false;
        }
        
        private void OnDestroy()
        {
            if (eventManager != null)
            {
                eventManager.OnUnitDestroyed -= HandleUnitDestroyed;
                eventManager.OnUnitReachedFlag -= HandleUnitReachedFlag;
                eventManager.OnDifficultyChanged -= OnDifficultyChanged;
            }
        }
        
        private void HandleUnitDestroyed(GameObject unit)
        {
            if (!gameActive) 
                return;
            
            BaseUnit baseUnit = unit.GetComponent<BaseUnit>();
            if (baseUnit != null)
            {
                int pointValue = baseUnit.GetPointValue();
                currentScore += pointValue;
                
                if (eventManager != null)
                    eventManager.ScoreUpdated(currentScore);
                    
                unitsDestroyed++;
                
                CheckWinCondition();
            }
        }
        
        private void HandleUnitReachedFlag(GameObject unit)
        {
            if (!gameActive) 
                return;
            
            TriggerGameOver(false);
        }
        
        private void CheckWinCondition()
        {
            if (unitsDestroyed >= gameConfig.unitsToDestroyToWin)
            {
                TriggerGameOver(true);
            }
        }
        
        private void TriggerGameOver(bool isWin)
        {
            if (!gameActive)
                return;
                
            gameActive = false;
                
            if (eventManager != null)
                eventManager.GameOver(isWin);
                
            Debug.Log(isWin ? "Game Over: Player Won!" : "Game Over: Player Lost!");
        }
        
        private void OnDifficultyChanged(int newDifficulty)
        {
            // Adjust win conditions based on difficulty
            if (gameConfig != null)
            {
                // Example: Update UI to reflect new win conditions
                if (eventManager != null)
                {
                    eventManager.GameConfigUpdated();
                }
        
                Debug.Log($"GameManager: Game settings updated for difficulty {newDifficulty}");
            }
        }
        
        /// <summary>
        /// Start the game.
        /// </summary>
        public void StartGame()
        {
            if (gameActive)
                return;
                
            currentScore = 0;
            unitsDestroyed = 0;
            gameActive = true;
            
            if (eventManager != null)
                eventManager.GameStart();
                
            Debug.Log("Game Started!");
        }
        
        /// <summary>
        /// Pause the game.
        /// </summary>
        public void PauseGame()
        {
            if (gameActive)
                return;

            gameActive = false;
            if (eventManager != null)
                eventManager.GamePause();
                
            Debug.Log("Game Paused!");
        }
        
        /// <summary>
        /// Resume the game.
        /// </summary>
        public void ResumeGame()
        {
            if (!gameActive)
                return;

            gameActive = true;
            if (eventManager != null)
                eventManager.GameResume();
                
            Debug.Log("Game Resumed!");
        }
        
        public void QuitGame()
        {
            Application.Quit();
        }
        
        /// <summary>
        /// Get the current score.
        /// </summary>
        public int GetScore()
        {
            return currentScore;
        }
        
        /// <summary>
        /// Check if the game is active.
        /// </summary>
        public bool IsGameActive()
        {
            return gameActive;
        }
        
        public void SetMuteAudio(bool state)
        {
            audioSource.mute = state;
        }
        
        public GameConfig GetGameConfig()
        {
            return gameConfig;
        }

    }
}