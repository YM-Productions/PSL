using System;

namespace Utils;

public static class Debug
{
    public static void Log(string message) => Logger.LoggerFactory.GetLogger("INFO").Log(message);
    public static void LogWarning(string message) => Logger.LoggerFactory.GetLogger("WARNING").Log(message);
    public static void LogError(string message) => Logger.LoggerFactory.GetLogger("ERROR").Log(message);
}
