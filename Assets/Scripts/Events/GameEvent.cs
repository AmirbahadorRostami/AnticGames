using System.Collections.Generic;
using UnityEngine;

namespace TacticalGame.Events
{
    /// <summary>
    /// ScriptableObject-based event system for decoupled communication between components.
    /// Implememnted for demonstration purposes, did not really use at this point
    /// great for extendebility
    /// </summary>
    [CreateAssetMenu(fileName = "NewGameEvent", menuName = "TacticalGame/Game Event")]
    public class GameEvent : ScriptableObject
    {
        /// <summary>
        /// The list of listeners that will be notified when the event is raised.
        /// </summary>
        private readonly List<GameEventListener> listeners = new List<GameEventListener>();

        /// <summary>
        /// Raises the event, notifying all listeners.
        /// </summary>
        public void Raise()
        {
            // Notify listeners from last to first in case listeners remove themselves during execution
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised();
            }
        }

        /// <summary>
        /// Registers a listener to this event.
        /// </summary>
        public void RegisterListener(GameEventListener listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        /// <summary>
        /// Unregisters a listener from this event.
        /// </summary>
        public void UnregisterListener(GameEventListener listener)
        {
            if (listeners.Contains(listener))
            {
                listeners.Remove(listener);
            }
        }
    }
}