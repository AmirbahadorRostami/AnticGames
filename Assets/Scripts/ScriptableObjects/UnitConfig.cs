using UnityEngine;

namespace TacticalGame.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject containing configuration data for a specific unit type.
    /// </summary>
    [CreateAssetMenu(fileName = "UnitConfig", menuName = "TacticalGame/Unit Configuration")]
    public class UnitConfig : ScriptableObject
    {
        [Header("General Properties")]
        [Tooltip("Prefab for this unit type")]
        public GameObject unitPrefab;
        
        [Tooltip("Display name for this unit type")]
        public string unitName;
        
        [Tooltip("Description of this unit's abilities")]
        [TextArea(3, 5)]
        public string description;

        [Header("Movement Properties")]
        [Tooltip("Base movement speed")]
        public float moveSpeed = 3f;
        
        [Tooltip("Movement acceleration")]
        public float acceleration = 8f;
        
        [Tooltip("Movement deceleration")]
        public float deceleration = 12f;
        
        [Tooltip("Rotation speed in degrees per second")]
        public float rotationSpeed = 360f;

        [Header("Gameplay Properties")]
        [Tooltip("Health points")]
        public float maxHealth = 100f;
        
        [Tooltip("Points awarded for destroying this unit")]
        public int pointValue = 10;
        
        [Tooltip("Detection range for this unit")]
        public float detectionRange = 5f;
        
        [Tooltip("Attack range for this unit (if applicable)")]
        public float attackRange = 1.5f;
        
        [Tooltip("Damage inflicted by attacks (if applicable)")]
        public float attackDamage = 25f;
        
        [Tooltip("Cooldown between attacks in seconds (if applicable)")]
        public float attackCooldown = 1f;

        [Header("Visual Properties")]
        [Tooltip("Color for unit visualization")]
        public Color unitColor = Color.white;
    }
}