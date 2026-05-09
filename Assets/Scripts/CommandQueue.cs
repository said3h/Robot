using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Maneja la cola de comandos y la ejecución paso a paso.
/// </summary>
public class CommandQueue : MonoBehaviour
{
    [Header("Referencias")]
    public RobotController robot;

    [Header("Configuración")]
    public float stepDelay = 0.4f;

    private List<RobotCommand> commands = new List<RobotCommand>();
    private int currentIndex = -1;
    private bool isExecuting = false;
    private float stepTimer = 0f;

    // Evento cuando se completa la ejecución
    public System.Action OnExecutionComplete;

    void Update()
    {
        if (isExecuting && robot.IsIdle())
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                ExecuteNextStep();
            }
        }
    }

    /// <summary>
    /// Agrega un comando a la cola.
    /// </summary>
    public void AddCommand(RobotCommand command)
    {
        // Si estamos ejecutando, no agregar hasta que termine
        if (isExecuting) return;

        commands.Add(command);
    }

    /// <summary>
    /// Comienza la ejecución desde el inicio.
    /// </summary>
    public void Execute()
    {
        if (commands.Count == 0) return;

        currentIndex = -1;
        isExecuting = true;
        stepTimer = 0f;
    }

    /// <summary>
    /// Detiene la ejecución y limpia.
    /// </summary>
    public void Stop()
    {
        isExecuting = false;
        stepTimer = 0f;
        commands.Clear();
        currentIndex = -1;
    }

    /// <summary>
    /// Solo limpia la cola sin detener.
    /// </summary>
    public void ClearCommands()
    {
        if (!isExecuting)
            commands.Clear();
    }

    void ExecuteNextStep()
    {
        currentIndex++;

        if (currentIndex >= commands.Count)
        {
            isExecuting = false;
            Debug.Log("Fin de comandos");
            OnExecutionComplete?.Invoke();
            return;
        }

        Debug.Log($"Paso {currentIndex + 1}: {commands[currentIndex]}");
        ExecuteCommand(commands[currentIndex]);
        stepTimer = stepDelay;
    }

    void ExecuteCommand(RobotCommand command)
    {
        switch (command)
        {
            case RobotCommand.MoveForward:
                robot.MoveForward();
                break;
            case RobotCommand.MoveBack:
                robot.MoveBack();
                break;
            case RobotCommand.RotateLeft:
                robot.RotateLeft();
                break;
            case RobotCommand.RotateRight:
                robot.RotateRight();
                break;
        }
    }

    public List<RobotCommand> GetCommands()
    {
        return commands;
    }

    public int GetCurrentIndex()
    {
        return currentIndex;
    }

    public bool IsExecuting()
    {
        return isExecuting;
    }

    public int GetCommandCount()
    {
        return commands.Count;
    }
}