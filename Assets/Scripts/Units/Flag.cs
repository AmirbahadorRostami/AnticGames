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
            Debug.Log($"[Flag] Start called for {name}");
            
            eventManager = GameEventManager.Instance;
            
            // Call base Start which handles grid registration
            base.Start();
            
            Debug.Log($"[Flag] After base.Start() call for {name}");
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if a unit has reached the flag
            BaseUnit unit = other.GetComponent<BaseUnit>();
            if (unit != null && unit.EntityType != EntityType.Enemy && unit.EntityType != EntityType.Flag)
            {
                // Notify the event system
                if (eventManager != null)
                {
                    Debug.Log($"[Flag] Unit reached flag: {unit.name}");
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