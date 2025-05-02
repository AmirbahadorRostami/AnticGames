using System.Collections.Generic;
using UnityEngine;
using TacticalGame.Utilities;

namespace TacticalGame.Grid
{
    /// <summary>
    /// Manages the game's grid and provides centralized access to grid operations.
    /// Optimized for performance and memory usage.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector2Int gridSize = new Vector2Int(50, 50);
        [SerializeField] private Vector2 worldOffset = Vector2.zero;
        [SerializeField] private bool showDebugVisuals = false;
        [SerializeField] private Color debugLineColor = Color.white;
        [SerializeField] private bool showGridStats = false;
        [SerializeField] private float statsUpdateInterval = 2f;

        public static GridManager Instance { get; private set; }
        public Grid<GridCell> Grid { get; private set; }
        
        // Cache for entity lookups to reduce dictionary lookups
        private Dictionary<IGridEntity, GridEntity> trackedEntities = new Dictionary<IGridEntity, GridEntity>(128);
        
        // Performance monitoring
        private int entitiesRegistered = 0;
        private int entitiesUnregistered = 0;
        private int positionUpdates = 0;
        private float nextStatsTime = 0f;

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
            
            // Create cells for the entire grid - only if needed for your game
            // For large grids, consider lazy initialization
            CreateInitialCells();
            
            Debug.Log($"Grid initialized with dimensions {gridSize.x} x {gridSize.y} and cell size {cellSize}");
        }
        
        private void CreateInitialCells()
        {
            // For large grids, consider creating cells in chunks or on-demand
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    GridCell cell = new GridCell(new Vector2Int(x, y));
                    Grid.AddCell(x, y, cell);
                }
            }
        }
        
        private void Update()
        {
            // Performance monitoring
            if (showGridStats && Time.time >= nextStatsTime)
            {
                ShowGridStats();
                nextStatsTime = Time.time + statsUpdateInterval;
            }
        }
        
        private void ShowGridStats()
        {
            var (cellCount, entityCount, registrations, movements) = Grid.GetGridStats();
            
            Debug.Log($"Grid Stats: {cellCount} cells, {entityCount} entities, " +
                      $"{registrations} registrations, {movements} movements, " +
                      $"Tracked: +{entitiesRegistered} -{entitiesUnregistered} ~{positionUpdates}");
                      
            // Reset counters for next interval
            entitiesRegistered = 0;
            entitiesUnregistered = 0;
            positionUpdates = 0;
            Grid.ClearStats();
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
            entitiesRegistered++;
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
                entitiesUnregistered++;
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
                positionUpdates++;
            }
            else
            {
                // Automatically register
                RegisterEntity(entity);
            }
        }
        
        /// <summary>
        /// Get entities of a specific type within radius.
        /// Caller must return the list to the pool when done.
        /// </summary>
        public List<IGridEntity> GetEntitiesOfTypeInRadius(Vector3 position, float radius, EntityType type)
        {
            return Grid.GetEntitiesOfTypeInRadius(position, radius, type);
        }
        
        /// <summary>
        /// Fast check if any entity of a type exists in radius.
        /// More efficient than getting a full list for simple checks.
        /// </summary>
        public bool AnyEntityOfTypeInRadius(Vector3 position, float radius, EntityType type)
        {
            return Grid.AnyEntityOfTypeInRadius(position, radius, type);
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
            
            // Drawing every grid line can be expensive for large grids
            // For better performance, draw only the boundaries or a subset of lines
            DrawGridBoundaries();
            
            // Optionally draw active cells only
            if (Grid != null && showGridStats)
            {
                DrawActiveCells();
            }
        }
        
        private void DrawGridBoundaries()
        {
            // Draw grid boundaries
            Vector3 bottomLeft = new Vector3(worldOffset.x, 0, worldOffset.y);
            Vector3 bottomRight = new Vector3(gridSize.x * cellSize + worldOffset.x, 0, worldOffset.y);
            Vector3 topLeft = new Vector3(worldOffset.x, 0, gridSize.y * cellSize + worldOffset.y);
            Vector3 topRight = new Vector3(gridSize.x * cellSize + worldOffset.x, 0, gridSize.y * cellSize + worldOffset.y);
            
            // Draw perimeter
            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);
            
            // Draw some internal lines (e.g., every 5 cells)
            int step = Mathf.Max(1, gridSize.x / 10);
            
            for (int x = step; x < gridSize.x; x += step)
            {
                Vector3 bottom = new Vector3(x * cellSize + worldOffset.x, 0, worldOffset.y);
                Vector3 top = new Vector3(x * cellSize + worldOffset.x, 0, gridSize.y * cellSize + worldOffset.y);
                Gizmos.DrawLine(bottom, top);
            }
            
            for (int y = step; y < gridSize.y; y += step)
            {
                Vector3 left = new Vector3(worldOffset.x, 0, y * cellSize + worldOffset.y);
                Vector3 right = new Vector3(gridSize.x * cellSize + worldOffset.x, 0, y * cellSize + worldOffset.y);
                Gizmos.DrawLine(left, right);
            }
        }
        
        private void DrawActiveCells()
        {
            var activeCells = Grid.GetActiveCells();
            
            try
            {
                Gizmos.color = new Color(1, 0.5f, 0, 0.3f); // Orange semi-transparent
                
                foreach (var cell in activeCells)
                {
                    Vector3 cellCenter = Grid.GridToWorld(cell);
                    Gizmos.DrawCube(cellCenter, new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f));
                }
            }
            finally
            {
                // Return the list to the pool
                activeCells.ReturnToPool();
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