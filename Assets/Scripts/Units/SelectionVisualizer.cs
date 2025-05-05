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
        private GameObject indicator;
        
        private void Start()
        {
            eventManager = GameEventManager.Instance;
            
            if (eventManager != null)
            {
                eventManager.OnUnitSelected += HandleUnitSelected;
                eventManager.OnUnitDeselected += HandleUnitDeselected;
                eventManager.OnUnitDestroyed += HandleUnitDestroyed;
            }

            if (indicator == null)
            {
                indicator = Instantiate(selectionIndicatorPrefab);
                indicator.SetActive(false);
            }
        }
        
        private void OnDestroy()
        {
            if (eventManager != null)
            {
                eventManager.OnUnitSelected -= HandleUnitSelected;
                eventManager.OnUnitDeselected -= HandleUnitDeselected;
                eventManager.OnUnitDestroyed -= HandleUnitDestroyed;
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
            // Position above the unit
            Vector3 position = unit.transform.position;
            position.y += hoverHeight;
            indicator.transform.position = position;
            
            // Parent to unit for automatic movement
            indicator.transform.SetParent(unit.transform, true);
            
            // Activate indicator
            indicator.SetActive(true);
        }
        
        private void HideSelectionIndicator(GameObject unit)
        {
            // Unparent and deactivate
            indicator.transform.SetParent(null);
            indicator.SetActive(false);
        }
    }
}