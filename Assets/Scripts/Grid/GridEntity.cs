using UnityEngine;

namespace TacticalGame.Grid
{
    /// <summary>
    /// Base component for any entity that exists in the grid system.
    /// Handles registration with the grid and position updates.
    /// </summary>
    public class GridEntity : MonoBehaviour, IGridEntity
    {
        [SerializeField] private EntityType entityType;
        [SerializeField] private float updateThreshold = 0.01f; // Distance threshold for position updates
        
        private Vector3 lastRegisteredPosition;
        private bool isRegistered = false;
        private int framesSinceLastCheck = 0;
        private const int CHECK_INTERVAL = 2; // Check position every N frames
        
        public Vector3 WorldPosition => transform.position;
        public EntityType EntityType => entityType;

        protected virtual void Start()
        {
            RegisterWithGrid();
        }

        protected virtual void OnDestroy()
        {
            UnregisterFromGrid();
        }

        protected virtual void Update()
        {
            // Only check position every N frames to reduce overhead
            if (++framesSinceLastCheck < CHECK_INTERVAL)
                return;
                
            framesSinceLastCheck = 0;
                
            // Only update grid position if entity has moved significantly
            if (isRegistered && Vector3.SqrMagnitude(lastRegisteredPosition - transform.position) > updateThreshold * updateThreshold)
            {
                UpdateGridPosition();
            }
        }

        /// <summary>
        /// Registers this entity with the grid system.
        /// </summary>
        public virtual void RegisterWithGrid()
        {
            // Prevent double registration
            if (isRegistered)
                return;
            
            if (GridManager.Instance != null)
            {
                GridManager.Instance.RegisterEntity(this);
                lastRegisteredPosition = transform.position;
                isRegistered = true;
            }
            else
            {
                Debug.LogError($"[GridEntity] GridManager not found! Cannot register entity {name}.");
            }
        }

        /// <summary>
        /// Unregisters this entity from the grid system.
        /// </summary>
        public virtual void UnregisterFromGrid()
        {
            if (GridManager.Instance != null && isRegistered)
            {
                GridManager.Instance.UnregisterEntity(this);
                isRegistered = false;
            }
        }

        /// <summary>
        /// Updates the entity's position in the grid system.
        /// </summary>
        public virtual void UpdateGridPosition()
        {
            if (GridManager.Instance != null && isRegistered)
            {
                GridManager.Instance.UpdateEntityPosition(this);
                lastRegisteredPosition = transform.position;
            }
        }

        /// <summary>
        /// Force an immediate grid position update.
        /// </summary>
        public virtual void ForceGridUpdate()
        {
            if (!isRegistered)
            {
                RegisterWithGrid();
            }
            else
            {
                UpdateGridPosition();
            }
        }
        
        /// <summary>
        /// Set the update threshold for position checks.
        /// </summary>
        public void SetUpdateThreshold(float threshold)
        {
            updateThreshold = Mathf.Max(0.001f, threshold);
        }
    }
}