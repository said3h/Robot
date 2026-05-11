using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BotiCommandQueue : MonoBehaviour
{
    public BotiRobotController robot;
    public Text queueText;
    public Text debugText;

    private List<string> commands = new List<string>();
    private Coroutine runningCoroutine;

    public void AddMoveUp()
    {
        debugText.text = "UP clicked";
        Debug.Log("UP BUTTON CLICKED");
        commands.Add("Up");
        UpdateQueueText();
    }

    public void AddMoveDown()
    {
        debugText.text = "DOWN clicked";
        Debug.Log("DOWN BUTTON CLICKED");
        commands.Add("Down");
        UpdateQueueText();
    }

    public void AddMoveLeft()
    {
        debugText.text = "LEFT clicked";
        Debug.Log("LEFT BUTTON CLICKED");
        commands.Add("Left");
        UpdateQueueText();
    }

    public void AddMoveRight()
    {
        debugText.text = "RIGHT clicked";
        Debug.Log("RIGHT BUTTON CLICKED");
        commands.Add("Right");
        UpdateQueueText();
    }

    public void RunCommands()
    {
        debugText.text = "RUN clicked";
        Debug.Log("RUN BUTTON CLICKED");

        if (commands.Count == 0)
        {
            debugText.text = "ERROR: command list empty";
            Debug.Log("No commands!");
            return;
        }

        if (runningCoroutine != null)
            StopCoroutine(runningCoroutine);

        runningCoroutine = StartCoroutine(RunCommandsRoutine());
    }

    private IEnumerator RunCommandsRoutine()
    {
        debugText.text = "Running commands...";
        Debug.Log("COROUTINE STARTED");

        while (commands.Count > 0)
        {
            string cmd = commands[0];

            if (robot == null)
            {
                debugText.text = "ERROR: robot is null";
                Debug.LogError("Robot is null!");
                yield break;
            }

            debugText.text = "Executing: " + cmd;
            Debug.Log("Executing: " + cmd);

            commands.RemoveAt(0);

            if (cmd == "Up")
                robot.MoveNorth();
            else if (cmd == "Down")
                robot.MoveSouth();
            else if (cmd == "Left")
                robot.MoveWest();
            else if (cmd == "Right")
                robot.MoveEast();

            UpdateQueueText();
            yield return new WaitForSeconds(0.4f);
        }

        runningCoroutine = null;
    }

    public void ClearCommands()
    {
        commands.Clear();
        UpdateQueueText();
    }

    public void ResetLevel()
    {
        if (runningCoroutine != null)
        {
            StopCoroutine(runningCoroutine);
            runningCoroutine = null;
        }

        commands.Clear();

        if (robot != null)
            robot.ResetRobot();

        UpdateQueueText();
    }

    private void UpdateQueueText()
    {
        if (queueText == null) return;

        if (commands.Count == 0)
            queueText.text = "Commands: (empty)";
        else
            queueText.text = "Commands: " + string.Join(" | ", commands);
    }
}