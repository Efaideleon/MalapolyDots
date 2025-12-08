using UnityEngine;
using System.IO;
using System;

public class CustomFileLogger : MonoBehaviour
{
    private string logFilePath;

    void Awake()
    {
        // Define the log file path in a persistent location
        logFilePath = Path.Combine(Application.persistentDataPath, "game_log.txt");
        
        Debug.Log("Writing Logs to Persistent Data Path: " + Application.persistentDataPath);
        // Clear the log file on startup (optional)
        if (File.Exists(logFilePath))
        {
            File.Delete(logFilePath);
        }

        // Subscribe to Unity's log message event
        Application.logMessageReceived += HandleLog;
        
        Debug.Log($"Custom log file created at: {logFilePath}");
    }

    void OnDestroy()
    {
        // Unsubscribe when the object is destroyed
        Application.logMessageReceived -= HandleLog;
    }

    // This method is called every time Debug.Log, Debug.LogError, etc., is called.
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Format the log message
        string formattedLog = $"[{DateTime.Now:HH:mm:ss.fff}] [{type}] {logString}";

        try
        {
            // Append the log message to the file
            File.AppendAllText(logFilePath, formattedLog + Environment.NewLine);
        }
        catch (Exception e)
        {
            // Handle file access errors
            Debug.LogError($"Failed to write to log file: {e.Message}");
        }
    }
}
