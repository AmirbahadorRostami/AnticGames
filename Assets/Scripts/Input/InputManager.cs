using UnityEngine;
using UnityEngine.InputSystem;
using TacticalGame.Game;

namespace TacticalGame.Input
{
    /// <summary>
    /// Handles player input using Unity's new Input System.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInput playerInput;
        
        private GameManager gameManager;
        
        private InputAction pauseAction;
        private InputAction confirmAction;
        private InputAction cancelAction;

        private void Awake()
        {
            if (playerInput == null)
            {
                playerInput = GetComponent<PlayerInput>();
                
                if (playerInput == null)
                {
                    Debug.LogError("PlayerInput component not found. Make sure you've set up the Input System package correctly.");
                    return;
                }
            }
            
            // Get references to actions
            pauseAction = playerInput.actions["Pause"];
            confirmAction = playerInput.actions["Confirm"];
            cancelAction = playerInput.actions["Cancel"];
        }
        
        private void OnEnable()
        {
            // Subscribe to input events
            if (pauseAction != null)
                pauseAction.performed += OnPauseActionPerformed;
                
            if (confirmAction != null)
                confirmAction.performed += OnConfirmActionPerformed;
                
            if (cancelAction != null)
                cancelAction.performed += OnCancelActionPerformed;
        }
        
        private void OnDisable()
        {
            // Unsubscribe from input events
            if (pauseAction != null)
                pauseAction.performed -= OnPauseActionPerformed;
                
            if (confirmAction != null)
                confirmAction.performed -= OnConfirmActionPerformed;
                
            if (cancelAction != null)
                cancelAction.performed -= OnCancelActionPerformed;
        }
        
        private void Start()
        {
            gameManager = GameManager.Instance;
        }
        
        private void OnPauseActionPerformed(InputAction.CallbackContext context)
        {
            if (gameManager == null)
                return;
                
            if (gameManager.IsGameActive())
            {
                gameManager.PauseGame();
            }
            else
            {
                gameManager.ResumeGame();
            }
        }
        
        private void OnConfirmActionPerformed(InputAction.CallbackContext context)
        {
            // Can be used for UI interactions
        }
        
        private void OnCancelActionPerformed(InputAction.CallbackContext context)
        {
            // Can be used for UI interactions
        }
    }
}