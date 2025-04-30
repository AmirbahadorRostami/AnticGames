using UnityEngine;
using UnityEngine.Events;

namespace TacticalGame.Events
{
    /// <summary>
    /// Component that listens for a GameEvent and responds with a UnityEvent.
    /// </summary>
    public class GameEventListener : MonoBehaviour
    {
        [Tooltip("Event to register with.")]
        public GameEvent Event;

        [Tooltip("Response to invoke when Event is raised.")]
        public UnityEvent Response;

        private void OnEnable()
        {
            if (Event != null)
            {
                Event.RegisterListener(this);
            }
        }

        private void OnDisable()
        {
            if (Event != null)
            {
                Event.UnregisterListener(this);
            }
        }

        /// <summary>
        /// Called when the event is raised.
        /// </summary>
        public void OnEventRaised()
        {
            Response?.Invoke();
        }
    }
}