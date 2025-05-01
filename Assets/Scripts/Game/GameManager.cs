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
        [Header("Game Settings")]
        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private Transform flagTransform;
        
        private GameEventManager eventManager;
        private int currentScore = 0;
        private int unitsDestroyed = 0;
        private bool gameActive = false;
        
        public static GameManager Instance { get; private set; }

        public static Transform FlagTransform { get; private set; }

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
            }
            
            gameActive = false;
        }
        
        private void OnDestroy()
        {
            if (eventManager != null)
            {
                eventManager.OnUnitDestroyed -= HandleUnitDestroyed;
                eventManager.OnUnitReachedFlag -= HandleUnitReachedFlag;
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
            if (!gameActive)
                return;
                
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
                
            if (eventManager != null)
                eventManager.GameResume();
                
            Debug.Log("Game Resumed!");
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
    }
}