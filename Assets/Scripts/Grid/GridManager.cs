using System.Collections.Generic;
using UnityEngine;

namespace TacticalGame.Grid
{
    /// <summary>
    /// Manages the game's grid and provides centralized access to grid operations.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector2Int gridSize = new Vector2Int(50, 50);
        [SerializeField] private Vector2 worldOffset = Vector2.zero;
        [SerializeField] private bool showDebugVisuals = false;
        [SerializeField] private Color debugLineColor = Color.white;

        public static GridManager Instance { get; private set; }
        public Grid<GridCell> Grid { get; private set; }

        private Dictionary<IGridEntity, GridEntity> trackedEntities = new Dictionary<IGridEntity, GridEntity>();

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            Grid = new Grid<GridCell>(cellSize, worldOffset);
            
            // Create cells for the entire grid
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    GridCell cell = new GridCell(new Vector2Int(x, y));
                    Grid.AddCell(x, y, cell);
                }
            }
            
            Debug.Log($"Grid initialized with dimensions {gridSize.x} x {gridSize.y} and cell size {cellSize}");
        }

        public void RegisterEntity(GridEntity entity)
        {
            if (entity == null)
            {
                Debug.LogError("Attempted to register null entity to grid");
                return;
            }
            
            trackedEntities[entity] = entity;
            Grid.RegisterEntity(entity);
            Debug.Log($"Entity registered: {entity.name} at position {entity.transform.position}");
        }

        public void UnregisterEntity(GridEntity entity)
        {
            if (entity == null)
            {
                Debug.LogError("Attempted to unregister null entity from grid");
                return;
            }
            
            if (trackedEntities.ContainsKey(entity))
            {
                trackedEntities.Remove(entity);
                Grid.UnregisterEntity(entity);
                Debug.Log($"Entity unregistered: {entity.name}");
            }
        }

        public void UpdateEntityPosition(GridEntity entity)
        {
            if (entity == null)
            {
                Debug.LogError("Attempted to update position of null entity");
                return;
            }
            
            if (trackedEntities.ContainsKey(entity))
            {
                Grid.MoveEntity(entity);
            }
            else
            {
                Debug.LogWarning($"Entity not registered: {entity.name}. Registering now.");
                RegisterEntity(entity);
            }
        }

        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            return Grid.GridToWorld(gridPos);
        }

        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            return Grid.WorldToGrid(worldPos);
        }

        private void OnDrawGizmos()
        {
            if (!showDebugVisuals || Grid == null)
                return;
            
            Gizmos.color = debugLineColor;
            
            // Draw grid lines
            for (int x = 0; x <= gridSize.x; x++)
            {
                Vector3 startPos = new Vector3(x * cellSize + worldOffset.x, 0, worldOffset.y);
                Vector3 endPos = new Vector3(x * cellSize + worldOffset.x, 0, gridSize.y * cellSize + worldOffset.y);
                Gizmos.DrawLine(startPos, endPos);
            }
            
            for (int y = 0; y <= gridSize.y; y++)
            {
                Vector3 startPos = new Vector3(worldOffset.x, 0, y * cellSize + worldOffset.y);
                Vector3 endPos = new Vector3(gridSize.x * cellSize + worldOffset.x, 0, y * cellSize + worldOffset.y);
                Gizmos.DrawLine(startPos, endPos);
            }
        }
    }

    /// <summary>
    /// Represents a cell in the grid.
    /// </summary>
    public class GridCell
    {
        public Vector2Int Coordinates { get; private set; }
        
        public GridCell(Vector2Int coordinates)
        {
            Coordinates = coordinates;
        }
    }
}