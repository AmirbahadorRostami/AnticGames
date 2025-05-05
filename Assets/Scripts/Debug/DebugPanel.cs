using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TacticalGame.Events;
using TacticalGame.Units;
using TacticalGame.Units.Types;
using TacticalGame.Game;
using System.Text;

namespace TacticalGame.UI
{
    /// <summary>
    /// Debug panel to display information about the AntPatroller's current target.
    /// </summary>
    public class DebugPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject debugPanel;
        [SerializeField] private Button toggleButton;
        [SerializeField] private TextMeshProUGUI debugInfoText;
        
        [Header("Flag Risk Thresholds")]
        [SerializeField] private float highRiskDistance = 5f;
        [SerializeField] private float mediumRiskDistance = 10f;
        
        [Header("Update Settings")]
        [SerializeField] private float updateInterval = 0.2f;
        
        private GameEventManager eventManager;
        private GameManager gameManager;
        private GameObject currentTarget;
        private Transform flagTransform;
        private AntPatroller antPatroller;
        private float nextUpdateTime = 0f;
        private StringBuilder infoBuilder = new StringBuilder(256);
        
        private void Start()
        {
            eventManager = GameEventManager.Instance;
            
            if (eventManager == null)
            {
                Debug.LogError("EventManager not found in scene!");
                return;
            }
            
            // Subscribe to events
            eventManager.OnEnemyTargetingUnit += HandleEnemyTargetingUnit;
            eventManager.OnAntPatrollerSpawned += HandleAntPatrollerSpawned;
            eventManager.OnUnitDestroyed += HandleUnitDestroyed;
            
            if (toggleButton != null)
                toggleButton.onClick.AddListener(ToggleDebugPanel);
            
            // Initially hide the panel
            if (debugPanel != null)
                debugPanel.SetActive(false);
            
            gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                flagTransform = gameManager.FlagTransform;
            }
            
            if (flagTransform == null)
            {
                Debug.LogWarning("Flag transform not found for DebugPanel!");
            }
            UpdateDebugInfoText();
        }
        
        private void OnDestroy()
        {
            if (eventManager != null)
            {
                eventManager.OnEnemyTargetingUnit -= HandleEnemyTargetingUnit;
                eventManager.OnAntPatrollerSpawned -= HandleAntPatrollerSpawned;
                eventManager.OnUnitDestroyed -= HandleUnitDestroyed;
            }
            
            if (toggleButton != null)
                toggleButton.onClick.RemoveListener(ToggleDebugPanel);
        }
        
        private void HandleAntPatrollerSpawned(AntPatroller ant)
        {
            antPatroller = ant;
            Debug.Log("Debug Panel: Ant Patroller registered");
            UpdateDebugInfoText();
        }
        
        private void HandleEnemyTargetingUnit(GameObject enemy, GameObject target)
        {
            // Check if this is our ant patroller
            if (antPatroller != null && enemy == antPatroller.gameObject)
            {
                currentTarget = target;
                UpdateDebugInfoText();
                Debug.Log($"Debug Panel: Target updated to {target.name}");
            }
        }
        
        private void HandleUnitDestroyed(GameObject unit)
        {
            // Clear target if it was destroyed
            if (currentTarget == unit)
            {
                currentTarget = null;
                UpdateDebugInfoText();
                Debug.Log("Debug Panel: Target was destroyed");
            }
        }
        
        private void Update()
        {
            // Throttle updates to avoid too many text updates per frame
            if (debugPanel != null && debugPanel.activeSelf && Time.time >= nextUpdateTime)
            {
                UpdateDebugInfoText();
                nextUpdateTime = Time.time + updateInterval;
            }
        }
        
        private void UpdateDebugInfoText()
        {
            if (debugInfoText == null)
                return;

            // Clear the string builder
            infoBuilder.Clear();
                
            // Ant Patroller Status
            infoBuilder.AppendLine("<b>ANT PATROL DEBUG</b>");
            infoBuilder.AppendLine("=================");
            
            if (antPatroller == null)
            {
                infoBuilder.AppendLine("No Ant Patroller found");
                debugInfoText.text = infoBuilder.ToString();
                return;
            }
            
            // Current Target
            infoBuilder.AppendLine(currentTarget != null 
                ? $"<b>TARGET:</b> {currentTarget.name}" 
                : "<b>TARGET:</b> None");
            
            // Only show additional info if there's a target
            if (currentTarget != null)
            {
                // Get distance
                float distance = Vector3.Distance(antPatroller.transform.position, currentTarget.transform.position);
                infoBuilder.AppendLine($"<b>DISTANCE:</b> {distance:F2}m");
                
                // Get point value
                BaseUnit targetUnit = currentTarget.GetComponent<BaseUnit>();
                if (targetUnit != null)
                {
                    int pointValue = targetUnit.GetPointValue();
                    infoBuilder.AppendLine($"<b>POINT VALUE:</b> {pointValue}");
                }
                
                if (flagTransform != null)
                {
                    float distanceToFlag = Vector3.Distance(currentTarget.transform.position, flagTransform.position);
                    string riskLevel = "Low";
                    string colorTag = "<color=green>";
                    
                    if (distanceToFlag <= highRiskDistance)
                    {
                        riskLevel = "High";
                        colorTag = "<color=red>";
                    }
                    else if (distanceToFlag <= mediumRiskDistance)
                    {
                        riskLevel = "Medium";
                        colorTag = "<color=yellow>";
                    }
                    
                    infoBuilder.AppendLine($"<b>FLAG DISTANCE:</b> {distanceToFlag:F2}m");
                    infoBuilder.AppendLine($"<b>FLAG RISK:</b> {colorTag}{riskLevel}</color>");
                }
            }
            debugInfoText.text = infoBuilder.ToString();
        }
        
        private void ToggleDebugPanel()
        {
            if (debugPanel != null)
            {
                debugPanel.SetActive(!debugPanel.activeSelf);
                if (debugPanel.activeSelf)
                {
                    UpdateDebugInfoText();
                }
            }
        }
    }
}