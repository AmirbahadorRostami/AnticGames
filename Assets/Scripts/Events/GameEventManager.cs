using System;
using UnityEngine;

namespace TacticalGame.Events
{
    /// <summary>
    /// Centralized event system for game-wide communication.
    /// Uses the observer pattern to decouple systems.
    /// </summary>
    public class GameEventManager : MonoBehaviour
    {
        public static GameEventManager Instance { get; private set; }

        // Game state events
        public event Action OnGameStart;
        public event Action OnGamePause;
        public event Action OnGameResume;
        public event Action<bool> OnGameOver;  // Parameter: true = win, false = lose
        public event Action<int> OnDifficultyChanged;
        public event Action OnGameConfigUpdated;
        
        // Unit events
        public event Action<GameObject> OnUnitSpawned;
        public event Action<GameObject> OnUnitDestroyed;
        public event Action<GameObject> OnUnitReachedFlag;

        // Ant events
        public event Action<GameObject, GameObject> OnEnemyTargetingUnit;  // Parameters: enemy, target
        public event Action<GameObject> OnEnemyIdle;

        // Score events
        public event Action<int> OnScoreUpdated;  // Parameter: new score

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

        // Game state methods
        public void GameStart()
        {
            OnGameStart?.Invoke();
        }

        public void GamePause()
        {
            OnGamePause?.Invoke();
        }

        public void GameResume()
        {
            OnGameResume?.Invoke();
        }

        public void GameOver(bool isWin)
        {
            OnGameOver?.Invoke(isWin);
        }

        // Unit methods
        public void UnitSpawned(GameObject unit)
        {
            OnUnitSpawned?.Invoke(unit);
        }

        public void UnitDestroyed(GameObject unit)
        {
            OnUnitDestroyed?.Invoke(unit);
        }

        public void UnitReachedFlag(GameObject unit)
        {
            OnUnitReachedFlag?.Invoke(unit);
        }

        // Ant methods
        public void EnemyTargetingUnit(GameObject enemy, GameObject target)
        {
            OnEnemyTargetingUnit?.Invoke(enemy, target);
        }

        public void EnemyIdle(GameObject enemy)
        {
            OnEnemyIdle?.Invoke(enemy);
        }

        // Score methods
        public void ScoreUpdated(int newScore)
        {
            OnScoreUpdated?.Invoke(newScore);
        }
        
        public void DifficultyChanged(int newDifficultyValue)
        {
            OnDifficultyChanged?.Invoke(newDifficultyValue);
            Debug.Log($"GameEventManager: Difficulty changed to {newDifficultyValue}");
        }
        
        public void GameConfigUpdated()
        {
            OnGameConfigUpdated?.Invoke();
        }
    }
}