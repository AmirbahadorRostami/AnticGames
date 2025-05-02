using UnityEngine;

namespace TacticalGame.Units.Movement
{
    /// <summary>
    /// Factory for creating different movement strategies.
    /// Encapsulates the logic for creating the right movement type.
    /// </summary>
    public static class MovementFactory
    {
        /// <summary>
        /// Movement types available in the game.
        /// </summary>
        public enum MovementType
        {
            Transform,
            Rigidbody,
            Tween
        }

        /// <summary>
        /// Creates a movement strategy of the specified type.
        /// </summary>
        /// <param name="type">The type of movement strategy to create</param>
        /// <param name="movingObject">The Transform of the object that will move</param>
        /// <param name="targetPosition">The target position to move towards</param>
        /// <param name="moveSpeed">The movement speed</param>
        /// <returns>An initialized IMovementStrategy</returns>
        public static IMovementStrategy CreateMovementStrategy(
            MovementType type,
            Transform movingObject,
            Vector3 targetPosition,
            float moveSpeed)
        {
            IMovementStrategy strategy = null;
            
            switch (type)
            {
                case MovementType.Transform:
                    strategy = new TransformMovement();
                    break;
                    
                case MovementType.Rigidbody:
                    // Check if we have the required component
                    if (movingObject.GetComponent<Rigidbody>() == null)
                    {
                        Debug.LogWarning("Rigidbody movement requested but no Rigidbody found. Adding one.");
                        movingObject.gameObject.AddComponent<Rigidbody>();
                    }
                    strategy = new RigidbodyMovement();
                    break;
                    
                case MovementType.Tween:
                    strategy = new TweenMovement();
                    break;
                    
                default:
                    Debug.LogError($"Unknown movement type: {type}. Defaulting to Transform movement.");
                    strategy = new TransformMovement();
                    break;
            }
            
            // Initialize the strategy
            strategy.Initialize(movingObject, targetPosition, moveSpeed);
            
            return strategy;
        }

        /// <summary>
        /// Creates the most appropriate movement strategy for a given unit type.
        /// </summary>
        /// <param name="entityType">The type of entity</param>
        /// <param name="movingObject">The Transform of the object that will move</param>
        /// <param name="targetPosition">The target position to move towards</param>
        /// <param name="moveSpeed">The movement speed</param>
        /// <returns>An initialized IMovementStrategy</returns>
        public static IMovementStrategy CreateMovementForEntityType(
            Grid.EntityType entityType,
            Transform movingObject,
            Vector3 targetPosition,
            float moveSpeed)
        {
            // Choose the best movement type for each entity
            switch (entityType)
            {
                case Grid.EntityType.Beetles:
                    // Ants use transform-based movement - simple and direct
                    return CreateMovementStrategy(MovementType.Transform, movingObject, targetPosition, moveSpeed);
                    
                case Grid.EntityType.Aphid:
                    // Aphids use physics-based movement - more organic and can interact with environment
                    return CreateMovementStrategy(MovementType.Rigidbody, movingObject, targetPosition, moveSpeed);
                    
                case Grid.EntityType.Ladybug:
                    // Bees use tween movement - smooth and elegant flight paths
                    return CreateMovementStrategy(MovementType.Tween, movingObject, targetPosition, moveSpeed);
                    
                case Grid.EntityType.Ant:
                    // Ant uses transform-based movement for precise interception
                    return CreateMovementStrategy(MovementType.Transform, movingObject, targetPosition, moveSpeed);
                    
                default:
                    // Default to transform movement
                    return CreateMovementStrategy(MovementType.Transform, movingObject, targetPosition, moveSpeed);
            }
        }
    }
}