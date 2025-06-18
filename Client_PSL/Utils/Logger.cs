using Client_PSL.ViewModels;
using Client_PSL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Utils;

/// <summary>
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
    public static int LEVEL { get; private set; } = 24;

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
        Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (Level > LEVEL) return;
                string name = LevelNames.ContainsKey(Level) ? LevelNames[Level].name : Name;
                Avalonia.Media.Color color = LevelNames.ContainsKey(Level) ? LevelNames[Level].color : Avalonia.Media.Colors.Lime;
                Globals.debugViewModel.AddMessage(name, color, message);
            }
        );
    }

    /// <summary>
    /// Sets the logging level for this logger instance.
    /// </summary>
    /// <param name="level">
    /// The desired logging level. Must not be equal to <c>ERROR_LEVEL</c>, <c>WARNING_LEVEL</c>, or <c>INFO_LEVEL</c>,
    /// and must not be less than zero. If an invalid level is provided, an <see cref="Exception"/> is thrown.
    /// </param>
    /// <exception cref="Exception">
    /// Thrown when the specified <paramref name="level"/> is invalid (i.e., equals <c>ERROR_LEVEL</c>, <c>WARNING_LEVEL</c>, <c>INFO_LEVEL</c>, or is less than zero).
    /// </exception>
    public void SetLevel(int level)
    {
        if (level == ERROR_LEVEL ||
            level == WARNING_LEVEL ||
            level == INFO_LEVEL ||
            level < 0) throw new Exception($"Cant set level to {level}");
        Level = level;
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
            Logger logger = new(name);
            _loggerDic.Add(name, logger);
            return logger;
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

            CreateLogger(name);
            return _loggerDic[name];
        }
    }
    #endregion
}

