using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Maneja el estado del nivel y la victoria.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Referencias")]
    public Transform goalPosition;
    public GameObject victoryPanel;
    public GameObject nextLevelButton;

    private RobotController robot;
    private bool levelCompleted = false;

    void Start()
    {
        robot = FindObjectOfType<RobotController>();

        if (victoryPanel)
            victoryPanel.SetActive(false);
    }

    void Update()
    {
        if (levelCompleted) return;

        CheckVictory();
    }

    void CheckVictory()
    {
        if (!robot || !goalPosition) return;

        Vector3 robotPos = robot.GetGridPosition();
        Vector3 goalPos = goalPosition.position;

        if (Mathf.Approximately(robotPos.x, goalPos.x) &&
            Mathf.Approximately(robotPos.z, goalPos.z))
        {
            LevelComplete();
        }
    }

    void LevelComplete()
    {
        levelCompleted = true;

        if (victoryPanel)
            victoryPanel.SetActive(true);
    }

    /// <summary>
    /// Reinicia el nivel actual.
    /// </summary>
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Carga el siguiente nivel.
    /// </summary>
    public void LoadNextLevel()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentIndex + 1);
    }

    /// <summary>
    /// Verifica si el nivel actual tiene más niveles después.
    /// </summary>
    public bool HasNextLevel()
    {
        return SceneManager.sceneCountInBuildSettings > SceneManager.GetActiveScene().buildIndex + 1;
    }
}