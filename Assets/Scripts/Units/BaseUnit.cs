using System.Collections;
using UnityEngine;
using TacticalGame.Grid;
using TacticalGame.Events;
using TacticalGame.ScriptableObjects;
using TacticalGame.Units.Movement;

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
        
        [Header("Target")]
        [SerializeField] protected Transform targetTransform;
        
        // Movement
        protected IMovementStrategy movementStrategy;
        protected bool isMoving = true;
        
        // State tracking
        protected float currentHealth;
        protected bool isAlive = true;
        
        // References
        protected GameEventManager eventManager;
        protected Transform flagTransform;

        protected override void Start()
        {
            base.Start();
            
            // Find references
            eventManager = GameEventManager.Instance;
            
            if (targetTransform == null)
            {
                FindFlag();
            }
            
            // Initialize health
            currentHealth = unitConfig.maxHealth;
            
            // Initialize movement
            InitializeMovement();
            
            // Notify about spawn
            if (eventManager != null)
            {
                eventManager.UnitSpawned(gameObject);
            }
        }

        protected virtual void FindFlag()
        {
            GameObject flagObj = GameObject.FindGameObjectWithTag("Flag");
            if (flagObj != null)
            {
                targetTransform = flagObj.transform;
            }
            else
            {
                Debug.LogError("Flag not found! Unit needs a target.");
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
    }
}