using System;

namespace Utils;

public static class Debug
{
    #region Logging
    private static string dateTime => DateTime.Now.ToString("yyyy-MM-dd|HH:mm:ss");

    public static void Log(string message)
    {
        string msg = $"{dateTime}    LOG: {message}";
    }
    #endregion
}
