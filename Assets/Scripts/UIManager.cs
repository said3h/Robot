using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Maneja la UI del juego.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Referencias de UI")]
    public GameObject freeModePanel;
    public GameObject programModePanel;
    public GameObject commandQueuePanel;
    public GameObject victoryPanel;

    [Header("Botones de comandos")]
    public GameObject[] commandButtons;

    private CommandQueue commandQueue;
    private RobotController robot;
    private bool isProgramMode = false;

    void Start()
    {
        commandQueue = FindObjectOfType<CommandQueue>();
        robot = FindObjectOfType<RobotController>();

        UpdateUI();
    }

    /// <summary>
    /// Cambia entre modo libre y modo programa.
    /// </summary>
    public void ToggleMode()
    {
        isProgramMode = !isProgramMode;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (freeModePanel)
            freeModePanel.SetActive(!isProgramMode);

        if (programModePanel)
            programModePanel.SetActive(isProgramMode);

        if (commandQueuePanel)
            commandQueuePanel.SetActive(isProgramMode);
    }

    /// <summary>
    /// Agrega comando desde los botones.
    /// </summary>
    public void AddMoveForward()
    {
        if (commandQueue && !commandQueue.IsExecuting())
            commandQueue.AddCommand(RobotCommand.MoveForward);
    }

    public void AddMoveBack()
    {
        if (commandQueue && !commandQueue.IsExecuting())
            commandQueue.AddCommand(RobotCommand.MoveBack);
    }

    public void AddRotateLeft()
    {
        if (commandQueue && !commandQueue.IsExecuting())
            commandQueue.AddCommand(RobotCommand.RotateLeft);
    }

    public void AddRotateRight()
    {
        if (commandQueue && !commandQueue.IsExecuting())
            commandQueue.AddCommand(RobotCommand.RotateRight);
    }

    /// <summary>
    /// Ejecuta los comandos programados.
    /// </summary>
    public void ExecuteCommands()
    {
        if (commandQueue)
            commandQueue.Execute();
    }

    /// <summary>
    /// Limpia los comandos.
    /// </summary>
    public void ClearCommands()
    {
        if (commandQueue)
            commandQueue.ClearCommands();
    }

    /// <summary>
    /// Muestra la pantalla de victoria.
    /// </summary>
    public void ShowVictory()
    {
        if (victoryPanel)
            victoryPanel.SetActive(true);
    }

    /// <summary>
    /// Oculta la pantalla de victoria.
    /// </summary>
    public void HideVictory()
    {
        if (victoryPanel)
            victoryPanel.SetActive(false);
    }

    /// <summary>
    /// Reinicia el nivel.
    /// </summary>
    public void RestartLevel()
    {
        FindObjectOfType<LevelManager>()?.RestartLevel();
    }

    /// <summary>
    /// Carga el siguiente nivel.
    /// </summary>
    public void LoadNextLevel()
    {
        FindObjectOfType<LevelManager>()?.LoadNextLevel();
    }
}