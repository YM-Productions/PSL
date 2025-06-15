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
    /// Asynchronously sets the specified text to the system clipboard using the clipboard service
    /// associated with the top-level control of the provided <paramref name="view"/>.
    /// </summary>
    /// <param name="view">
    /// The <see cref="UserControl"/> whose top-level parent is used to access the clipboard service.
    /// </param>
    /// <param name="text">
    /// The text to be placed on the clipboard.
    /// </param>
    /// <remarks>
    /// This method runs the clipboard operation on a background task to avoid blocking the UI thread.
    /// If the <paramref name="view"/> is not attached to a top-level control or the clipboard service is unavailable,
    /// the method will silently do nothing.
    /// </remarks>
    public static void SetClipboard(UserControl view, string text)
    {
        if (TopLevel.GetTopLevel(view)?.Clipboard is IClipboard clipboard)
        {
            _ = Task.Run(() => clipboard.SetTextAsync(text));
        }
    }
}
