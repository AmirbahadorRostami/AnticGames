using UnityEngine;
using TacticalGame.Grid;
using TacticalGame.Events;

namespace TacticalGame.Units
{
    /// <summary>
    /// Represents the flag that units move toward.
    /// Handles registration with the grid and detecting units reaching it.
    /// </summary>
    public class Flag : GridEntity
    {
        [SerializeField] private float detectionRadius = 1.0f;
        
        private GameEventManager eventManager;

        protected override void Start()
        {
            base.Start();
            
            eventManager = GameEventManager.Instance;
            
            // The flag doesn't move, so we can register it once
            RegisterWithGrid();
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if a unit has reached the flag
            BaseUnit unit = other.GetComponent<BaseUnit>(); // change get component to use 
            if (unit != null && unit.EntityType != EntityType.Enemy && unit.EntityType != EntityType.Flag)
            {
                // Notify the event system
                if (eventManager != null)
                {
                    eventManager.UnitReachedFlag(unit.gameObject);
                }
            }
        }

        // Visualize the detection radius in the editor
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}