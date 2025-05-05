using UnityEngine;
using TacticalGame.Events;
using TacticalGame.Units;
using TacticalGame.Units.Types;

namespace TacticalGame.Game
{
    public class UnitTargetingSystem : MonoBehaviour
    {
        [Header("Selection Settings")]
        [SerializeField] private LayerMask unitLayerMask;
        [SerializeField] private float maxSelectionDistance = 100f;
        [SerializeField] private Camera mainCamera;
        
        private GameEventManager eventManager;
        private BaseUnit selectedUnit;
        private AntPatroller antUnit;
        
        private void Start()
        {
            eventManager = GameEventManager.Instance;
            
            if (mainCamera == null)
                mainCamera = Camera.main;
                
            if (eventManager != null)
            {
                eventManager.OnGameStart += FindAntPatroller;
                eventManager.OnUnitDestroyed += HandleUnitDestroyed;
            }
            
            FindAntPatroller();
        }
        
        private void OnDestroy()
        {
            if (eventManager != null)
            {
                eventManager.OnGameStart -= FindAntPatroller;
                eventManager.OnUnitDestroyed -= HandleUnitDestroyed;
            }
        }
        
        private void Update()
        {
            // TODO: Idealy i want to use the new input system instead of the code below due to time constraints currently using this
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                HandleSelection();
            }
        }
        
        private void HandleSelection()
        {
            Ray ray = mainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, maxSelectionDistance, unitLayerMask))
            {
                // Try to get a BaseUnit component
                BaseUnit unit = hit.collider.GetComponent<BaseUnit>();
                
                if (unit != null)
                {
                    SelectUnit(unit);
                }
            }
        }
        
        private void SelectUnit(BaseUnit unit)
        {
            // Don't allow selecting the ant itself
            if (unit is AntPatroller)
                return;
                
            // Handle deselection of previous unit
            if (selectedUnit != null)
            {
                // Notify about deselection
                eventManager.UnitDeselected(selectedUnit.gameObject);
            }
            
            // Set new selection
            selectedUnit = unit;
            
            // Notify about selection
            eventManager.UnitSelected(unit.gameObject);
            
            // Command ant to target this unit
            if (antUnit != null)
            {
                antUnit.SetPlayerSelectedTarget(unit);
            }
        }
        
        private void FindAntPatroller()
        {
            if (antUnit == null)
            {
                // First try finding by name
                GameObject antObject = GameObject.Find("Ant");
                
                // If not found, try finding by type
                if (antObject == null)
                {
                    AntPatroller[] antPatrollers = FindObjectsOfType<AntPatroller>();
                    if (antPatrollers.Length > 0)
                    {
                        antUnit = antPatrollers[0];
                        return;
                    }
                }
                else
                {
                    antUnit = antObject.GetComponent<AntPatroller>();
                }
                
                // Additional logging for debugging
                if (antUnit == null)
                {
                    Debug.LogWarning("UnitTargetingSystem: Could not find AntPatroller in scene");
                }
            }
        }
        
        private void HandleUnitDestroyed(GameObject unit)
        {
            // If selected unit was destroyed, clear selection
            if (selectedUnit != null && selectedUnit.gameObject == unit)
            {
                selectedUnit = null;
            }
        }
        
        public BaseUnit GetSelectedUnit()
        {
            return selectedUnit;
        }
        
        public void ClearSelection()
        {
            if (selectedUnit != null)
            {
                eventManager.UnitDeselected(selectedUnit.gameObject);
                selectedUnit = null;
            }
        }
    }
}