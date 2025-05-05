using UnityEngine;
using TacticalGame.Events;
using System.Collections.Generic;

namespace TacticalGame.Units
{
    public class SelectionVisualizer : MonoBehaviour
    {
        [SerializeField] private GameObject selectionIndicatorPrefab;
        [SerializeField] private float hoverHeight = 0.2f;
        [SerializeField] private int poolSize = 5; // Pre-allocate a small pool of indicators
        
        private GameEventManager eventManager;
        private Dictionary<GameObject, GameObject> activeIndicators = new Dictionary<GameObject, GameObject>();
        private Queue<GameObject> indicatorPool = new Queue<GameObject>();
        
        private void Start()
        {
            eventManager = GameEventManager.Instance;
            
            if (eventManager != null)
            {
                eventManager.OnUnitSelected += HandleUnitSelected;
                eventManager.OnUnitDeselected += HandleUnitDeselected;
                eventManager.OnUnitDestroyed += HandleUnitDestroyed;
            }
            
            // Initialize the pool
            InitializeIndicatorPool();
        }
        
        private void OnDestroy()
        {
            if (eventManager != null)
            {
                eventManager.OnUnitSelected -= HandleUnitSelected;
                eventManager.OnUnitDeselected -= HandleUnitDeselected;
                eventManager.OnUnitDestroyed -= HandleUnitDestroyed;
            }
            
            // Clean up pool
            foreach (var indicator in indicatorPool)
            {
                Destroy(indicator);
            }
            indicatorPool.Clear();
            
            // Clean up active indicators
            foreach (var indicator in activeIndicators.Values)
            {
                Destroy(indicator);
            }
            activeIndicators.Clear();
        }
        
        private void InitializeIndicatorPool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject indicator = Instantiate(selectionIndicatorPrefab);
                indicator.SetActive(false);
                indicatorPool.Enqueue(indicator);
            }
        }
        
        private void HandleUnitSelected(GameObject unit)
        {
            ShowSelectionIndicator(unit);
        }
        
        private void HandleUnitDeselected(GameObject unit)
        {
            HideSelectionIndicator(unit);
        }
        
        private void HandleUnitDestroyed(GameObject unit)
        {
            // If the unit is destroyed, release its indicator
            HideSelectionIndicator(unit);
        }
        
        private void ShowSelectionIndicator(GameObject unit)
        {
            // Check if unit already has an indicator
            if (activeIndicators.ContainsKey(unit))
                return;
                
            // Get indicator from pool or create new one if pool is empty
            GameObject indicator;
            if (indicatorPool.Count > 0)
            {
                indicator = indicatorPool.Dequeue();
            }
            else
            {
                // Create a new indicator if pool is empty
                indicator = Instantiate(selectionIndicatorPrefab);
            }
            
            // Position above the unit
            Vector3 position = unit.transform.position;
            position.y += hoverHeight;
            indicator.transform.position = position;
            
            // Parent to unit for automatic movement
            indicator.transform.SetParent(unit.transform, true);
            
            // Activate indicator
            indicator.SetActive(true);
            
            // Add to active indicators
            activeIndicators[unit] = indicator;
        }
        
        private void HideSelectionIndicator(GameObject unit)
        {
            if (activeIndicators.TryGetValue(unit, out GameObject indicator))
            {
                // Unparent and deactivate
                indicator.transform.SetParent(null);
                indicator.SetActive(false);
                
                // Return to pool
                indicatorPool.Enqueue(indicator);
                
                // Remove from active indicators
                activeIndicators.Remove(unit);
            }
        }
    }
}