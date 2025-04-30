using UnityEngine;

namespace TacticalGame.Grid
{
    /// <summary>
    /// Interface for entities that can be registered in the grid.
    /// </summary>
    public interface IGridEntity
    {
        Vector3 WorldPosition { get; }
        EntityType EntityType { get; }
    }
}