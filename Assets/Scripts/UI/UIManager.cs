using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TacticalGame.Events;
using TacticalGame.Game;

namespace TacticalGame.UI
{
    /// <summary>
    /// Manages the game UI.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject pausePanel;
        
        [Header("Text Elements")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI gameOverMessageText;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        
        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button mainMenuButton;
        
        private GameEventManager eventManager;
        private GameManager gameManager;

        private void Start()
        {
            // Find references
            eventManager = GameEventManager.Instance;
            gameManager = GameManager.Instance;
            
            // Subscribe to events
            if (eventManager != null)
            {
                eventManager.OnGameStart += HandleGameStart;
                eventManager.OnGamePause += HandleGamePause;
                eventManager.OnGameResume += HandleGameResume;
                eventManager.OnGameOver += HandleGameOver;
                eventManager.OnScoreUpdated += HandleScoreUpdated;
            }
            
            // Button listeners
            if (startButton != null)
                startButton.onClick.AddListener(OnStartButtonClicked);
                
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartButtonClicked);
                
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeButtonClicked);
                
            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseButtonClicked);
                
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
                
            // Initial UI state
            ShowMainMenu();
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (eventManager != null)
            {
                eventManager.OnGameStart -= HandleGameStart;
                eventManager.OnGamePause -= HandleGamePause;
                eventManager.OnGameResume -= HandleGameResume;
                eventManager.OnGameOver -= HandleGameOver;
                eventManager.OnScoreUpdated -= HandleScoreUpdated;
            }
        }
        
        private void HandleGameStart()
        {
            ShowGameplay();
        }
        
        private void HandleGamePause()
        {
            ShowPauseMenu();
        }
        
        private void HandleGameResume()
        {
            ShowGameplay();
        }
        
        private void HandleGameOver(bool isWin)
        {
            ShowGameOverScreen(isWin);
        }
        
        private void HandleScoreUpdated(int newScore)
        {
            UpdateScoreText(newScore);
        }
        
        private void UpdateScoreText(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }
        
        private void ShowMainMenu()
        {
            SetActivePanels(mainMenuPanel);
        }
        
        private void ShowGameplay()
        {
            SetActivePanels(gameplayPanel);
        }
        
        private void ShowPauseMenu()
        {
            SetActivePanels(pausePanel);
        }
        
        private void ShowGameOverScreen(bool isWin)
        {
            if (gameOverMessageText != null)
            {
                gameOverMessageText.text = isWin ? "Victory!" : "Defeat!";
            }
            
            if (finalScoreText != null && gameManager != null)
            {
                finalScoreText.text = $"Final Score: {gameManager.GetScore()}";
            }
            
            SetActivePanels(gameOverPanel);
        }
        
        private void SetActivePanels(GameObject activePanel)
        {
            // Deactivate all panels
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (gameplayPanel != null) gameplayPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            
            // Activate the specified panel
            if (activePanel != null)
            {
                activePanel.SetActive(true);
            }
        }
        
        // Button click handlers
        private void OnStartButtonClicked()
        {
            if (gameManager != null)
            {
                gameManager.StartGame();
            }
        }
        
        private void OnRestartButtonClicked()
        {
            if (gameManager != null)
            {
                gameManager.StartGame();
            }
        }
        
        private void OnResumeButtonClicked()
        {
            if (gameManager != null)
            {
                gameManager.ResumeGame();
            }
        }
        
        private void OnPauseButtonClicked()
        {
            if (gameManager != null)
            {
                gameManager.PauseGame();
            }
        }
        
        private void OnMainMenuButtonClicked()
        {
            ShowMainMenu();
        }
    }
}