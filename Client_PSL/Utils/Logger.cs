using Client_PSL.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils;

/// <summary>
/// Andrin was here!
/// 
/// Represents a structured and centralized logging utility that supports logging messages with
/// different severity levels such as INFO, WARNING, and ERROR. Each <see cref="Logger"/> instance
/// is associated with a name and a fixed level and can be retrieved via the <see cref="LoggerFactory"/>.
///
/// <para>
/// The logger writes messages to the <see cref="DebugViewModel"/>, enabling live debug output within
/// the UI. It supports color-coded log messages based on severity for better visibility.
/// </para>
/// </summary>
public class Logger
{
    private static readonly int ERROR_LEVEL = 0;
    private static readonly int WARNING_LEVEL = 1;
    private static readonly int INFO_LEVEL = 5;

    /// <summary>
    /// Gets or sets the current global logging level. Only messages with a logger level
    /// equal to or below this threshold will be logged.
    /// </summary>
    public static int LEVEL { get; private set; } = INFO_LEVEL;

    /// <summary>
    /// Contains predefined logger levels with their display name and associated Avalonia color.
    /// Used internally to format log output.
    /// </summary>
    public static Dictionary<int, (string name, Avalonia.Media.Color color)> LevelNames = new()
    {
        { ERROR_LEVEL, ("ERROR", Avalonia.Media.Colors.Red) },
        { WARNING_LEVEL, ("WARNING", Avalonia.Media.Colors.Yellow) },
        { INFO_LEVEL, ("INFO", Avalonia.Media.Colors.Cyan) },
    };

    /// <summary>
    /// Gets the name of the logger, which is used to identify it within the system.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the fixed level of this logger instance. Determines the severity category.
    /// </summary>
    public int Level { get; private set; }

    #region Constructor

    /// <summary>
    /// Initializes a new logger with the specified name and default INFO level.
    /// </summary>
    /// <param name="name">The name of the logger (e.g., "SYSTEM", "DATABASE").</param>
    internal Logger(string name)
    {
        Name = name;
        Level = INFO_LEVEL;
    }

    /// <summary>
    /// Initializes a new logger with a specified name and level.
    /// </summary>
    /// <param name="name">The name of the logger.</param>
    /// <param name="level">The severity level (0=ERROR, 1=WARNING, 5=INFO).</param>
    internal Logger(string name, int level)
    { 
        Name = name; 
        Level = level; 
    }
    #endregion

    #region Fuctions

    /// <summary>
    /// Logs the specified message to the <see cref="DebugViewModel"/>, if the logger's level
    /// is within the global log level threshold.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <example>
    /// <code>
    /// var logger = Logger.LoggerFactory.GetLogger("INFO");
    /// logger.Log("Initialization complete.");
    /// </code>
    /// </example>
    public void Log(string message)
    {
        if (Level > LEVEL) return;
        DebugViewModel.Instance.AddMessage(LevelNames[Level].name, LevelNames[Level].color, message);
    }

    /// <summary>
    /// Sets the global logging threshold. Only loggers with a level less than or equal to this value will log messages.
    /// </summary>
    /// <param name="level">The new global log level (0=ERROR, 1=WARNING, 5=INFO).</param>
    /// <exception cref="Exception">Thrown if the provided level is not one of the allowed constants.</exception>
    public void SetLevel(int level)
    {
        if (level == ERROR_LEVEL ||
            level == WARNING_LEVEL ||
            level == INFO_LEVEL ||
            level < 0) throw new Exception($"Cant set level to {level}");
        LEVEL = level;
    }
    #endregion

    #region Factory

    /// <summary>
    /// Provides access to predefined loggers and the ability to create custom ones.
    /// Manages logger instances internally using a dictionary by name.
    /// </summary>
    public static class LoggerFactory
    {
        private static Dictionary<string, Logger> _loggerDic = new()
        {
            { "ERROR", new("ERROR", ERROR_LEVEL) },
            { "WARNING", new("WARNING", WARNING_LEVEL) },
            { "INFO", new("INFO", INFO_LEVEL) },
        };

        /// <summary>
        /// Creates a new logger with the specified name and the default INFO level.
        /// </summary>
        /// <param name="name">The name of the new logger.</param>
        /// <returns>A new logger instance.</returns>
        /// <exception cref="Exception">Thrown if a logger with the given name already exists.</exception>
        public static Logger CreateLogger(string name)
        {
            if (_loggerDic.ContainsKey(name)) throw new Exception($"Error: The Logger {name} already exists");
            return new Logger(name);
        }

        /// <summary>
        /// Retrieves an existing logger by name.
        /// </summary>
        /// <param name="name">The name of the logger to retrieve.</param>
        /// <returns>The logger instance if found.</returns>
        /// <exception cref="Exception">Thrown if no logger with the specified name exists.</exception>
        public static Logger GetLogger(string name)
        {
            if (_loggerDic.ContainsKey(name)) return _loggerDic[name];

            throw new Exception($"Error: There is no logger {name}");
        }
    }
    #endregion
}

