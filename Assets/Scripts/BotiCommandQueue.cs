using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Simple command queue for Boti robot.
/// Stores movement commands and executes them one by one with visual delay.
/// </summary>
public class BotiCommandQueue : MonoBehaviour
{
    [Header("References")]
    public BotiRobotController robotController;

    [Header("Settings")]
    public float commandDelay = 0.4f;

    // Queue of commands to execute
    private List<System.Action> commandQueue = new List<System.Action>();

    // Command names for display
    private List<string> commandNames = new List<string>();

    // Is the queue currently executing
    private bool isExecuting = false;

    // Reference to running coroutine
    private Coroutine runningCoroutine;

    // Callback for UI update
    public System.Action OnCommandsChanged;

    void Awake()
    {
        Debug.Log("BotiCommandQueue: Awake called");
    }

    void Start()
    {
        Debug.Log("BotiCommandQueue: Start called");

        // Find robot controller if not assigned
        if (robotController == null)
        {
            Debug.Log("BotiCommandQueue: robotController is null, trying to find Robot");
            GameObject robotObj = GameObject.Find("Robot");
            if (robotObj != null)
            {
                robotController = robotObj.GetComponent<BotiRobotController>();
                Debug.Log("BotiCommandQueue: Found robotController: " + (robotController != null));
            }
            else
            {
                Debug.LogError("BotiCommandQueue: Robot GameObject not found!");
            }
        }
        else
        {
            Debug.Log("BotiCommandQueue: robotController already assigned: " + robotController.name);
        }
    }

    /// <summary>
    /// Add "move up" command to the queue.
    /// </summary>
    public void AddMoveUp()
    {
        if (!ValidateRobotController())
            return;

        // Use a method reference instead of lambda to avoid capturing null
        commandQueue.Add(MoveUpInternal);
        commandNames.Add("Up");
        OnCommandsChanged?.Invoke();
        Debug.Log("Command added: Move Up (queue now has " + commandQueue.Count + " commands)");
    }

    private void MoveUpInternal()
    {
        if (robotController != null)
            robotController.MoveNorth();
    }

    /// <summary>
    /// Add "move down" command to the queue.
    /// </summary>
    public void AddMoveDown()
    {
        if (!ValidateRobotController())
            return;

        commandQueue.Add(MoveDownInternal);
        commandNames.Add("Down");
        OnCommandsChanged?.Invoke();
        Debug.Log("Command added: Move Down (queue now has " + commandQueue.Count + " commands)");
    }

    private void MoveDownInternal()
    {
        if (robotController != null)
            robotController.MoveSouth();
    }

    /// <summary>
    /// Add "move left" command to the queue.
    /// </summary>
    public void AddMoveLeft()
    {
        if (!ValidateRobotController())
            return;

        commandQueue.Add(MoveLeftInternal);
        commandNames.Add("Left");
        OnCommandsChanged?.Invoke();
        Debug.Log("Command added: Move Left (queue now has " + commandQueue.Count + " commands)");
    }

    private void MoveLeftInternal()
    {
        if (robotController != null)
            robotController.MoveWest();
    }

    /// <summary>
    /// Add "move right" command to the queue.
    /// </summary>
    public void AddMoveRight()
    {
        if (!ValidateRobotController())
            return;

        commandQueue.Add(MoveRightInternal);
        commandNames.Add("Right");
        OnCommandsChanged?.Invoke();
        Debug.Log("Command added: Move Right (queue now has " + commandQueue.Count + " commands)");
    }

    private void MoveRightInternal()
    {
        if (robotController != null)
            robotController.MoveEast();
    }

    /// <summary>
    /// Validates that robotController is available. If null, attempts to find it.
    /// Returns true if valid, false otherwise.
    /// </summary>
    private bool ValidateRobotController()
    {
        if (robotController != null)
            return true;

        Debug.LogWarning("ValidateRobotController: robotController is null, attempting to find Robot...");

        // Try to find Robot in scene
        GameObject robotObj = GameObject.Find("Robot");
        if (robotObj != null)
        {
            robotController = robotObj.GetComponent<BotiRobotController>();
            if (robotController != null)
            {
                Debug.Log("ValidateRobotController: Found robotController: " + robotController.name);
                return true;
            }
        }

        Debug.LogError("ValidateRobotController: Failed to find BotiRobotController!");
        return false;
    }

    /// <summary>
    /// Clear all queued commands.
    /// </summary>
    public void ClearCommands()
    {
        commandQueue.Clear();
        commandNames.Clear();
        OnCommandsChanged?.Invoke();
        Debug.Log("Commands cleared");
    }

    /// <summary>
    /// Start executing queued commands one by one using a Coroutine.
    /// </summary>
    public void RunCommands()
    {
        Debug.Log("RunCommands called. Queue count: " + commandQueue.Count);

        if (commandQueue.Count == 0)
        {
            Debug.Log("No commands to execute!");
            return;
        }

        if (isExecuting)
        {
            Debug.Log("Already executing!");
            return;
        }

        if (robotController == null)
        {
            Debug.LogError("RunCommands: robotController is null!");
            return;
        }

        Debug.Log("Executing " + commandQueue.Count + " commands...");
        isExecuting = true;

        // Start the coroutine and store reference
        runningCoroutine = StartCoroutine(RunCommandsCoroutine());
    }

    /// <summary>
    /// Stop any currently running command execution.
    /// </summary>
    public void StopExecution()
    {
        if (runningCoroutine != null)
        {
            StopCoroutine(runningCoroutine);
            runningCoroutine = null;
        }
        isExecuting = false;
    }

    /// <summary>
    /// Reset the level: stop execution, clear commands, reset robot.
    /// </summary>
    public void ResetLevel()
    {
        // Stop any running execution
        StopExecution();

        // Clear the queues
        commandQueue.Clear();
        commandNames.Clear();

        // Reset robot position
        if (robotController != null)
        {
            robotController.ResetRobot();
        }

        // Update UI
        OnCommandsChanged?.Invoke();

        Debug.Log("Level reset!");
    }

    /// <summary>
    /// Coroutine that executes commands one by one with delay between each.
    /// </summary>
    private IEnumerator RunCommandsCoroutine()
    {
        Debug.Log("RunCommandsCoroutine started. Initial count: " + commandQueue.Count);

        int index = 0;

        while (index < commandQueue.Count)
        {
            // Execute the current command
            Debug.Log("Executing command at index " + index + ": " + commandNames[index]);

            System.Action command = commandQueue[index];
            try
            {
                command();
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error executing command: " + e.Message);
            }

            // Remove the executed command from the list (for display)
            commandQueue.RemoveAt(index);
            commandNames.RemoveAt(index);

            // Update UI to show remaining commands
            OnCommandsChanged?.Invoke();

            Debug.Log("Command executed. Remaining: " + commandQueue.Count);

            // Wait before executing next command
            yield return new WaitForSeconds(commandDelay);
        }

        // All done
        Debug.Log("All commands executed!");
        isExecuting = false;
        runningCoroutine = null;
    }

    /// <summary>
    /// Get the number of queued commands.
    /// </summary>
    public int GetQueueCount()
    {
        return commandQueue.Count;
    }

    /// <summary>
    /// Check if the queue is currently executing.
    /// </summary>
    public bool IsExecuting()
    {
        return isExecuting;
    }

    /// <summary>
    /// Get command queue as a display string.
    /// </summary>
    public string GetCommandsDisplay()
    {
        if (commandNames.Count == 0)
            return "(empty)";

        return string.Join(" | ", commandNames);
    }
}