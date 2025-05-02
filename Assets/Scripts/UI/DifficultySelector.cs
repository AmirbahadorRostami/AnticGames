using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TacticalGame.Game;

namespace TacticalGame.UI
{
    /// <summary>
    /// UI component for selecting game difficulty.
    /// </summary>
    public class DifficultySelector : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button easyButton;
        [SerializeField] private Button mediumButton;
        [SerializeField] private Button hardButton;
        [SerializeField] private Button cycleDifficultyButton;
        [SerializeField] private TextMeshProUGUI currentDifficultyText;
        
        [Header("Button Colors")]
        [SerializeField] private Color selectedColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color unselectedColor = Color.white;

        private DifficultyManager difficultyManager;
        
        private void Start()
        {
            difficultyManager = DifficultyManager.Instance;
            
            // Set up button listeners
            if (easyButton != null)
                easyButton.onClick.AddListener(() => SetDifficulty(DifficultyManager.DifficultyLevel.Easy));
                
            if (mediumButton != null)
                mediumButton.onClick.AddListener(() => SetDifficulty(DifficultyManager.DifficultyLevel.Medium));
                
            if (hardButton != null)
                hardButton.onClick.AddListener(() => SetDifficulty(DifficultyManager.DifficultyLevel.Hard));
                
            if (cycleDifficultyButton != null)
                cycleDifficultyButton.onClick.AddListener(CycleDifficulty);
            
            // Update UI to match initial difficulty
            UpdateDifficultyUI();
        }
        
        private void OnDestroy()
        {
            // Remove button listeners
            if (easyButton != null)
                easyButton.onClick.RemoveAllListeners();
                
            if (mediumButton != null)
                mediumButton.onClick.RemoveAllListeners();
                
            if (hardButton != null)
                hardButton.onClick.RemoveAllListeners();
                
            if (cycleDifficultyButton != null)
                cycleDifficultyButton.onClick.RemoveAllListeners();
        }
        
        /// <summary>
        /// Set the difficulty level through the DifficultyManager.
        /// </summary>
        public void SetDifficulty(DifficultyManager.DifficultyLevel level)
        {
            if (difficultyManager != null)
            {
                difficultyManager.SetDifficulty(level);
                UpdateDifficultyUI();
            }
        }
        
        /// <summary>
        /// Cycle to the next difficulty level.
        /// </summary>
        public void CycleDifficulty()
        {
            if (difficultyManager != null)
            {
                difficultyManager.CycleDifficulty();
                UpdateDifficultyUI();
            }
        }
        
        /// <summary>
        /// Update UI elements to reflect current difficulty.
        /// </summary>
        private void UpdateDifficultyUI()
        {
            if (difficultyManager == null)
                return;
                
            DifficultyManager.DifficultyLevel currentLevel = difficultyManager.GetCurrentDifficulty();
            
            // Update text
            if (currentDifficultyText != null)
            {
                currentDifficultyText.text = $"Difficulty: {currentLevel}";
            }
            
            // Update button colors
            UpdateButtonState(easyButton, currentLevel == DifficultyManager.DifficultyLevel.Easy);
            UpdateButtonState(mediumButton, currentLevel == DifficultyManager.DifficultyLevel.Medium);
            UpdateButtonState(hardButton, currentLevel == DifficultyManager.DifficultyLevel.Hard);
        }
        
        private void UpdateButtonState(Button button, bool isSelected)
        {
            if (button == null)
                return;
                
            ColorBlock colors = button.colors;
            colors.normalColor = isSelected ? selectedColor : unselectedColor;
            button.colors = colors;
        }
    }
}