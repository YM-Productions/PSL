using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Client_PSL.ViewModels;
using Client_PSL.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utils;

/// <summary>
/// Provides utility methods for quick and convenient operations, such as clipboard access.
/// </summary>
public static class QuickUtils
{
    /// <summary>
    /// Asynchronously sets the specified text to the system clipboard using Avalonia's cross-platform clipboard API.
    /// </summary>
    /// <param name="text">
    /// The string value to be placed on the system clipboard. This value will overwrite any existing clipboard content.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous clipboard operation. The task completes when the text has been set to the clipboard.
    /// </returns>
    /// <remarks>
    /// This method uses Avalonia's <see cref="IClipboard"/> interface to provide a platform-independent way to set clipboard content.
    /// It works on all platforms supported by Avalonia, including Windows, Linux, and macOS.
    /// If <see cref="Application.Current"/> or its <see cref="Application.Clipboard"/> property is null, the method will not perform any operation and will complete silently.
    /// </remarks>
    public static void SetClipboard(UserControl view, string text)
    {
        if (TopLevel.GetTopLevel(view)?.Clipboard is IClipboard clipboard)
        {
            _ = Task.Run(() => clipboard.SetTextAsync(text));
        }
    }
}
