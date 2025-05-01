using UnityEngine;
using TacticalGame.Grid;
using TacticalGame.Events;

namespace TacticalGame.Units
{
    public class Flag : GridEntity
    {
        [SerializeField] private LayerMask unitLayerMask;
        [SerializeField] private bool debugLogging = false;
        private GameEventManager eventManager;

        protected override void Start()
        {
            eventManager = GameEventManager.Instance;
            base.Start();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & unitLayerMask) != 0)
            {
                if (eventManager != null)
                {
                    eventManager.UnitReachedFlag(other.gameObject);
                    if (debugLogging)
                    {
                        Debug.Log($"[Flag] Unit reached flag: {other.gameObject.name}");
                    }
                }
            }
        }
    }
}