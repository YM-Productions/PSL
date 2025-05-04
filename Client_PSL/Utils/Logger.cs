using Client_PSL.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils;

public class Test
{
    public void test()
    {
        Logger logger = Logger.LoggerFactory.GetLogger("");
    }
}

public class Logger
{
    private static readonly int ERROR_LEVEL = 0;
    private static readonly int WARNING_LEVEL = 1;
    private static readonly int INFO_LEVEL = 5;

    public static int LEVEL { get; private set; } = INFO_LEVEL;
    public static Dictionary<int, (string name, Avalonia.Media.Color color)> LevelNames = new()
    {
        { ERROR_LEVEL, ("ERROR", Avalonia.Media.Colors.Red) },
        { WARNING_LEVEL, ("WARNING", Avalonia.Media.Colors.Yellow) },
        { INFO_LEVEL, ("INFO", Avalonia.Media.Colors.Cyan) },
    };

    public string Name { get; private set; }
    public int Level { get; private set; }

    #region Constructor
    internal Logger(string name)
    {
        Name = name;
        Level = INFO_LEVEL;
    }

    internal Logger(string name, int level)
    { 
        Name = name; 
        Level = level; 
    }
    #endregion

    #region Fuctions
    /// <summary>
    /// This Method Loggs any given Message as the current Logger.
    /// <example>
    /// <code>
    /// // Prints Hello to Logs
    /// logger.Log("Hello");
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="message">The message to Log</param>
    public void Log(string message)
    {
        if (Level > LEVEL) return;
        DebugViewModel.Instance.AddMessage(LevelNames[Level].name, LevelNames[Level].color, message);
    }

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
    public static class LoggerFactory
    {
        private static Dictionary<string, Logger> _loggerDic = new()
        {
            { "ERROR", new("ERROR", ERROR_LEVEL) },
            { "WARNING", new("WARNING", WARNING_LEVEL) },
            { "INFO", new("INFO", INFO_LEVEL) },
        };

        public static Logger CreateLogger(string name)
        {
            if (_loggerDic.ContainsKey(name)) throw new Exception($"Error: The Logger {name} already exists");
            return new Logger(name);
        }

        public static Logger GetLogger(string name)
        {
            if (_loggerDic.ContainsKey(name)) return _loggerDic[name];

            throw new Exception($"Error: There is no logger {name}");
        }
    }
    #endregion
}

