using System;

namespace Utils;

/// <summary>
/// The <c>Debug</c> class provides simplified access to logging functionality across different severity levels:
/// Info, Warning, and Error. It serves as a static helper to consistently write log entries in a structured format.
///
/// <para>
/// All log entries are internally forwarded to the <see cref="Logger.LoggerFactory"/> and categorized by predefined
/// logging channels: "INFO" (level 5), "WARNING" (level 1), and "ERROR" (level 0).
/// </para>
///
/// <para>
/// This class is particularly useful for developers who want to quickly insert logs into their application
/// without having to manage logger instances manually.
/// </para>
///
/// <para>
/// Typical usage:
/// <code>
/// Debug.Log("Application started");
/// Debug.LogWarning("Low memory warning");
/// Debug.LogError("Unhandled exception occurred");
/// </code>
/// </para>
/// </summary>
public static class Debug
{
    /// <summary>
    /// Logs a message as informational output.
    /// This method always uses the "INFO" logger (log level 5).
    ///
    /// <para>
    /// Use this method for general-purpose messages that describe normal program execution,
    /// such as state changes, configuration values, or important checkpoints.
    /// </para>
    /// </summary>
    /// <param name="message">The message to log. This should be a meaningful and human-readable string.</param>
    /// <example>
    /// <code>
    /// Debug.Log("Server started on port 8080");
    /// Debug.Log($"User {user.Id} authenticated successfully");
    /// </code>
    /// </example>
    public static void Log(string message) => Logger.LoggerFactory.GetLogger("INFO").Log(message);

    /// <summary>
    /// Logs a message as a warning.
    /// This method always uses the "WARNING" logger (log level 1).
    ///
    /// <para>
    /// Use this method to indicate potential issues that are not necessarily errors but could require attention,
    /// such as deprecated API usage, retryable failures, or suspicious states.
    /// </para>
    /// </summary>
    /// <param name="message">The warning message to log.</param>
    /// <example>
    /// <code>
    /// Debug.LogWarning("Configuration file is missing; using defaults");
    /// Debug.LogWarning($"Attempt #{retry} failed, will retry...");
    /// </code>
    /// </example>
    public static void LogWarning(string message) => Logger.LoggerFactory.GetLogger("WARNING").Log(message);

    /// <summary>
    /// Logs a message as an error.
    /// This method always uses the "ERROR" logger (log level 0).
    ///
    /// <para>
    /// Use this method to report serious issues or exceptions that may interrupt application flow.
    /// These messages should be easily identifiable in logs and may be used for alerting or diagnostics.
    /// </para>
    /// </summary>
    /// <param name="message">The error message to log.</param>
    /// <example>
    /// <code>
    /// Debug.LogError("Failed to connect to database");
    /// Debug.LogError($"Unexpected exception: {ex.Message}");
    /// </code>
    /// </example>
    public static void LogError(string message) => Logger.LoggerFactory.GetLogger("ERROR").Log(message);
}
