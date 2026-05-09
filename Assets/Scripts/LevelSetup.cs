using UnityEngine;

/// <summary>
/// Crea un nivel simple programáticamente.
/// </summary>
public class LevelSetup : MonoBehaviour
{
    [Header("Referencias")]
    public GridManager gridManager;
    public RobotController robot;

    [Header("Configuración del nivel")]
    public Vector2 gridSize = new Vector2(7, 7);
    public Vector3 robotStartPosition = new Vector3(-2, 0, -2);
    public Vector3 goalPosition = new Vector3(2, 0, 2);
    public Vector3[] obstaclePositions;

    void Start()
    {
        SetupLevel();
    }

    public void SetupLevel()
    {
        // Crear grid
        if (gridManager)
        {
            gridManager.gridSize = gridSize;
            gridManager.CreateGrid();

            // Crear obstáculos
            if (obstaclePositions != null)
            {
                foreach (Vector3 pos in obstaclePositions)
                {
                    Vector3 worldPos = new Vector3(
                        robotStartPosition.x + pos.x,
                        0,
                        robotStartPosition.z + pos.z
                    );
                    gridManager.AddObstacle(worldPos);
                }
            }

            // Crear meta
            Vector3 goalWorldPos = new Vector3(
                robotStartPosition.x + goalPosition.x,
                0,
                robotStartPosition.z + goalPosition.z
            );
            gridManager.SetGoal(goalWorldPos);

            // Spawn robot
            robot = gridManager.SpawnRobot(robotStartPosition);
        }
    }

    /// <summary>
    /// Crea un nivel predeterminado simple.
    /// </summary>
    public void CreateSimpleLevel()
    {
        gridSize = new Vector2(7, 7);
        robotStartPosition = new Vector3(-3, 0, -3);
        goalPosition = new Vector3(3, 0, 3);
        obstaclePositions = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 1),
            new Vector3(2, 0, -1)
        };

        SetupLevel();
    }
}