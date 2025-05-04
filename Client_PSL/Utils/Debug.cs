using System;

namespace Utils;

public static class Debug
{
    /// <summary>
    /// This Method Logs always on Lvl 5 as INFO Logger
    /// <example>
    /// <code>
    /// // Prints Hello to Logs
    /// Debug.Log("Hello");
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="message">The Message to Log</param>
    public static void Log(string message) => Logger.LoggerFactory.GetLogger("INFO").Log(message);
    public static void LogWarning(string message) => Logger.LoggerFactory.GetLogger("WARNING").Log(message);
    public static void LogError(string message) => Logger.LoggerFactory.GetLogger("ERROR").Log(message);
}
