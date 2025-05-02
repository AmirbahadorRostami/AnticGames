using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TacticalGame.Events;
using TacticalGame.Game;
using UnityEngine.Serialization;

namespace TacticalGame.UI
{
    /// <summary>
    /// Manages the game UI.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject topPanel;
        
        [Header("Text Elements")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI gameOverMessageText;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        
        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button exitGameButton;
        [SerializeField] private Button musicButton;
        [SerializeField] private Button difficultyButton;
        
        [Header("Difficulty UI")]
        [SerializeField] private DifficultySelector difficultySelector;
        
        private GameEventManager eventManager;
        private GameManager gameManager;
        private bool isMusicOff = false;

        private void Start()
        {
            // Find references
            eventManager = GameEventManager.Instance;
            gameManager = GameManager.Instance;
            
            // Subscribe to events
            if (eventManager != null)
            {
                eventManager.OnGameOver += HandleGameOver;
                eventManager.OnScoreUpdated += HandleScoreUpdated;
            }
            
            // Button listeners
            if (startButton != null)
                startButton.onClick.AddListener(OnStartButtonClicked);
                
            if (restartButton != null)
                restartButton.onClick.AddListener(OnExitButtonClicked);
                
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeButtonClicked);
               
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);

            if (exitGameButton != null)
                exitGameButton.onClick.AddListener(OnExitButtonClicked);

            if (musicButton != null)
                musicButton.onClick.AddListener(OnMusicButtonClicked);
            
            if (difficultyButton != null)
                difficultyButton.onClick.AddListener(OnDifficultyButtonClicked);

            if (gameManager.IsGameActive())
            {
                startButton.gameObject.SetActive(false);
                resumeButton.gameObject.SetActive(true);
            }

            // Initial UI state
            ShowMainMenu();
        }
        
        private void OnDestroy()
        {

            // Button listeners
            if (startButton != null)
                startButton.onClick.RemoveListener(OnStartButtonClicked);

            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnExitButtonClicked);

            if (resumeButton != null)
                resumeButton.onClick.RemoveListener(OnResumeButtonClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClicked);

            if (exitGameButton != null)
                exitGameButton.onClick.RemoveListener(OnExitButtonClicked);

            if (musicButton != null)
                musicButton.onClick.RemoveListener(OnMusicButtonClicked);
            
            if (difficultyButton != null)
                difficultyButton.onClick.RemoveListener(OnDifficultyButtonClicked);

            // Unsubscribe from events
            if (eventManager != null)
            {
                eventManager.OnGameOver -= HandleGameOver;
                eventManager.OnScoreUpdated -= HandleScoreUpdated;
            }
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
        
        public void ShowMainMenu()
        {
            SetActivePanels(mainMenuPanel);
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
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if(difficultySelector.gameObject != null) difficultySelector.gameObject.SetActive(false);
            
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
            if (difficultySelector != null)
            {
                difficultySelector.gameObject.SetActive(false);
            }
            SetActivePanels(topPanel);
        }
        
        private void OnResumeButtonClicked()
        {
            if (gameManager != null)
            {
                gameManager.ResumeGame();
            }
        }

        private void OnExitButtonClicked()
        {
            if (gameManager != null)
            {
                gameManager.QuitGame();
            }
        }

        private void OnMainMenuButtonClicked()
        {
            if (gameManager != null)
            {
                gameManager.PauseGame();
            }

            ShowMainMenu();
        }

        private void OnMusicButtonClicked()
        {
            isMusicOff = !isMusicOff;
            gameManager.SetMuteAudio(isMusicOff);
        }

        private void OnDifficultyButtonClicked()
        {
            SetActivePanels(difficultySelector.gameObject);
        }
        
    }
}