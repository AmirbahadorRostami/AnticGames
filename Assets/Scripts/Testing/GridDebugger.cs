using UnityEngine;
using TacticalGame.Grid;
using System.Collections;
using System.Collections.Generic;

namespace TacticalGame.Testing
{
    /// <summary>
    /// Helper component for debugging grid-related issues.
    /// </summary>
    public class GridDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool monitorFlagObjects = true;
        [SerializeField] private float checkInterval = 1f;
        
        private void Start()
        {
            // Start monitoring entities
            StartCoroutine(MonitorEntities());
        }
        
        private IEnumerator MonitorEntities()
        {
            yield return new WaitForSeconds(1.0f); // Initial delay
            
            Debug.Log("[GridDebugger] Starting entity monitoring");
            
            while (true)
            {
                if (monitorFlagObjects)
                {
                    CheckFlagObjects();
                }
                
                yield return new WaitForSeconds(checkInterval);
            }
        }
        
        private void CheckFlagObjects()
        {
            // Find all GameObjects with Flag component
            var flags = FindObjectsOfType<TacticalGame.Units.Flag>();
            Debug.Log($"[GridDebugger] Found {flags.Length} Flag components in scene:");
            
            foreach (var flag in flags)
            {
                Debug.Log($"[GridDebugger] Flag: {flag.name} at {flag.transform.position}");
            }
            
            // Find all objects with the "Flag" tag
            GameObject[] taggedFlags = GameObject.FindGameObjectsWithTag("Flag");
            Debug.Log($"[GridDebugger] Found {taggedFlags.Length} objects with 'Flag' tag:");
            
            foreach (var flagObj in taggedFlags)
            {
                Debug.Log($"[GridDebugger] Tagged Flag: {flagObj.name} at {flagObj.transform.position}");
            }
            
            // Check for GridEntities with Flag type
            GridEntity[] allEntities = FindObjectsOfType<GridEntity>();
            int flagTypeCount = 0;
            
            foreach (var entity in allEntities)
            {
                if (entity.EntityType == EntityType.Flag)
                {
                    flagTypeCount++;
                    Debug.Log($"[GridDebugger] GridEntity with Flag type: {entity.name} at {entity.transform.position}");
                }
            }
            
            Debug.Log($"[GridDebugger] Total GridEntities with Flag type: {flagTypeCount}");
        }
        
        // Method to be called from other components or the Unity Editor
        [ContextMenu("Check Grid Entities")]
        public void ManualCheckEntities()
        {
            CheckFlagObjects();
        }
    }
}