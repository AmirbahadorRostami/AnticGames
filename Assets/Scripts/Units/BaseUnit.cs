using System.Collections;
using UnityEngine;
using TacticalGame.Grid;
using TacticalGame.Events;
using TacticalGame.ScriptableObjects;
using TacticalGame.Units.Movement;
using TacticalGame.Game;

namespace TacticalGame.Units
{
    /// <summary>
    /// Base class for all units in the game.
    /// Handles common unit functionality like movement, health, and grid registration.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public partial class BaseUnit : GridEntity
    {
        [Header("Unit Configuration")]
        [SerializeField] protected UnitConfig unitConfig;
        [SerializeField] protected Renderer meshRenderer;

        [Header("Target")]
        [SerializeField] protected Transform targetTransform;
        
        // State tracking
        [SerializeField] protected float currentHealth;
        [SerializeField] protected bool isAlive = true;
        
        // References
        protected GameEventManager eventManager;
        protected GameManager gameManager;
        
        // Movement
        protected IMovementStrategy movementStrategy;
        protected bool isMoving = true;
        protected float difficultyFactor;
        
        protected override void Start()
        {
            base.Start();
            
            // Find references
            eventManager = GameEventManager.Instance;
            gameManager = GameManager.Instance;
            if (targetTransform == null)
            {
                FindFlag();
            }
            
            // Initialize 
            currentHealth = unitConfig.maxHealth;
            meshRenderer.material.color = unitConfig.unitColor;
            InitializeMovement();
            
            // Notify about spawn
            if (eventManager != null)
            {
                eventManager.UnitSpawned(gameObject);
                eventManager.OnDifficultyChanged += OnDifficultyChanged;
            }
        }
        
        // Add to OnDestroy method
        protected virtual void OnDestroy()
        {
            if (eventManager != null)
            {
                eventManager.OnDifficultyChanged -= OnDifficultyChanged;
            }
        }

        protected virtual void FindFlag()
        {
            targetTransform = gameManager.FlagTransform;
            if (targetTransform == null)
            {
                targetTransform = GameObject.FindGameObjectWithTag("Flag").transform;
                if(targetTransform == null)
                {
                    Debug.LogError("Flag not found! Unit needs a target.");
                }
            }
        }

        protected virtual void InitializeMovement()
        {
            if (targetTransform == null)
            {
                Debug.LogError("Target not set! Cannot initialize movement.");
                return;
            }
            
            // Create appropriate movement strategy for this unit
            movementStrategy = MovementFactory.CreateMovementForEntityType(
                EntityType,
                transform,
                targetTransform.position,
                unitConfig.moveSpeed);
        }
        
        protected virtual void OnDifficultyChanged(int newDifficulty)
        {
            // Default implementation may vary by unit type
            difficultyFactor = newDifficulty / 3f; // Normalize to 0.33 - 1.67
    
            // Adjust movement speed
            if (movementStrategy != null)
            {
                float adjustedSpeed = unitConfig.moveSpeed * Mathf.Lerp(0.9f, 1.1f, difficultyFactor);
                movementStrategy.SetSpeed(adjustedSpeed);
            }
        }
    }
}