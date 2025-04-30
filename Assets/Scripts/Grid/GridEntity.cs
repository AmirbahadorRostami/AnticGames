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
        
        private Vector3 lastRegisteredPosition;
        private bool isRegistered = false;
        
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
            // Only update grid position if entity has moved
            if (isRegistered && Vector3.Distance(lastRegisteredPosition, transform.position) > 0.01f)
            {
                UpdateGridPosition();
            }
        }

        /// <summary>
        /// Registers this entity with the grid system.
        /// </summary>
        public virtual void RegisterWithGrid()
        {
            if (GridManager.Instance != null)
            {
                GridManager.Instance.RegisterEntity(this);
                lastRegisteredPosition = transform.position;
                isRegistered = true;
            }
            else
            {
                Debug.LogError($"GridManager not found! Cannot register entity {name}.");
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
    }
}