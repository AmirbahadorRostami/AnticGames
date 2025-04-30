using UnityEngine;

namespace TacticalGame.ScriptableObjects
{
    /// <summary>
    /// Base class for ScriptableObjects that need to store runtime variable values.
    /// </summary>
    public abstract class RuntimeVariable<T> : ScriptableObject
    {
        [SerializeField] private T initialValue;
        [TextArea(1, 5)]
        [SerializeField] private string description;

        private T runtimeValue;

        /// <summary>
        /// The current value of the variable during runtime.
        /// </summary>
        public T Value
        {
            get => runtimeValue;
            set
            {
                runtimeValue = value;
                OnValueChanged?.Invoke(runtimeValue);
            }
        }

        /// <summary>
        /// Event triggered when the value changes.
        /// </summary>
        public System.Action<T> OnValueChanged;

        protected virtual void OnEnable()
        {
            runtimeValue = initialValue;
        }

        /// <summary>
        /// Reset the runtime value to the initial value.
        /// </summary>
        public virtual void ResetValue()
        {
            Value = initialValue;
        }

        /// <summary>
        /// Set the initial value programmatically.
        /// </summary>
        public void SetInitialValue(T value)
        {
            initialValue = value;
            ResetValue();
        }
    }

    // Integer variable implementation
    [CreateAssetMenu(fileName = "IntVariable", menuName = "TacticalGame/Variables/Int Variable")]
    public class IntVariable : RuntimeVariable<int> { }

    // Float variable implementation 
    [CreateAssetMenu(fileName = "FloatVariable", menuName = "TacticalGame/Variables/Float Variable")]
    public class FloatVariable : RuntimeVariable<float> { }

    // Bool variable implementation
    [CreateAssetMenu(fileName = "BoolVariable", menuName = "TacticalGame/Variables/Bool Variable")]
    public class BoolVariable : RuntimeVariable<bool> { }

    // String variable implementation
    [CreateAssetMenu(fileName = "StringVariable", menuName = "TacticalGame/Variables/String Variable")]
    public class StringVariable : RuntimeVariable<string> { }

    // Vector3 variable implementation
    [CreateAssetMenu(fileName = "Vector3Variable", menuName = "TacticalGame/Variables/Vector3 Variable")]
    public class Vector3Variable : RuntimeVariable<Vector3> { }
}