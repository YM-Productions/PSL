using SpacetimeDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using Networking.SpacetimeController;
using Client_PSL.ViewModels;

namespace Utils.DebugCommands;

// TODO:
// - Login
// - Register
// - LoggerControll
//      example:
//      /logger --layer 5 -> Only layer >= 5 will be shown
//      /logger --disable <name> -> This specific logger wont Log
//      /logger --focus <name> -> Only logs this specific logger

/// <summary>
/// Provides a central interface for handling textual debug and developer commands,
/// including parsing input, routing to command handlers, and showing help information.
/// </summary>
/// <remarks>
/// <para>
/// The <c>DebugCommand</c> class contains a registry of supported commands in the <see cref="Commands"/> dictionary,
/// where each command maps to a corresponding action with named parameters.
/// </para>
/// <para>
/// Commands are expected to be invoked in the format:
/// <c>/command --key value</c>. For example:
/// <c>/login --name Alice --pwd secret</c>.
/// </para>
/// <para>
/// This class is typically used in developer tools, in-game consoles, or debug UIs
/// to test server connectivity, authentication flows, and user management functionality.
/// </para>
/// </remarks>
public static class DebugCommand
{
    /// <summary>
    /// A dictionary that maps command names (as lowercase strings) to their corresponding actions,
    /// each accepting a <see cref="Dictionary{TKey, TValue}"/> of string arguments.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This collection serves as a command dispatcher, allowing various commands (e.g., <c>login</c>, <c>register</c>, <c>help</c>)
    /// to be executed dynamically based on string input.
    /// </para>
    /// <para>
    /// The keys are case-insensitive command names, typically derived from method names using <c>nameof(...).ToLower()</c>.
    /// </para>
    /// <para>
    /// Example usage:
    /// <code>
    /// Commands["login"](new Dictionary&lt;string, string&gt; { ["username"] = "yaene", ["password"] = "123" });
    /// </code>
    /// </para>
    /// <para>
    /// Temporary or utility commands like <c>"d"</c> (disconnect), <c>"tempsession"</c> (open a temporary session),
    /// or <c>"time"</c> (log current UNIX time) are also included for development or debugging purposes.
    /// </para>
    /// </remarks>
    public static Dictionary<string, Action<Dictionary<string, string>>> Commands = new()
    {
        { nameof(Help).ToLower(), Help },
        { nameof(Login).ToLower(), Login },
        { nameof(Register).ToLower(), Register },
        { nameof(AutoScroll).ToLower(), AutoScroll },
        { nameof(MessageLimit).ToLower(), MessageLimit },

        // NOTE: This is only temporary
        { "d" , _ => SpacetimeController.Instance.CloseCon() },
        { "tempsession" , _ => SpacetimeController.Instance.OpenTemporarySession() },
        { "time" , _ => Debug.Log(((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()).ToString()) },
    };

    private static void Help(Dictionary<string, string> attributes)
    {
        if (attributes.Count == 0)
        {
            Debug.Log("--------------------------\nCommand Format:\n/<command> --<attributeName> <attribute>\nExample:\n/login --name NAME --Password PWD");
            return;
        }

        if (attributes.TryGetValue("c", out string? value) && !string.IsNullOrEmpty(value))
        {
            StringBuilder sb = new();

            switch (value)
            {
                case "login":
                    sb.AppendLine("/login");
                    sb.AppendLine("The login command is used to Login.");
                    sb.AppendLine("/login --name <NAME> --pwd <PWD>");
                    sb.AppendLine("Attributes:");
                    sb.AppendLine("--name   -> UserName");
                    sb.AppendLine("--pwd    -> Password");
                    break;
                case "register":
                    sb.AppendLine("/register");
                    sb.AppendLine("The register command is used to create a new Account.");
                    sb.AppendLine("/register --name <NAME> --mail <MAIL> --pwd <PWD> --news <true:false> --agb <true:false>");
                    sb.AppendLine("--name   -> UserName");
                    sb.AppendLine("--mail   -> EMailAddress");
                    sb.AppendLine("--pwd    -> Password");
                    sb.AppendLine("--news   -> Subscribe to Newsletter");
                    sb.AppendLine("--AGB    -> Accept AGB");
                    break;
                default:
                    Debug.LogError($"<{value}> is no valid Value in this Context!");
                    return;
            }
            Debug.Log(sb.ToString());
            return;
        }

        Debug.LogError("Invalid Command structure!\nUse </help> for more info");
    }

    private static void Login(Dictionary<string, string> attributes)
    {
        if (attributes.Values.Any(string.IsNullOrEmpty))
        {
            Debug.LogError("Attributes must not be empty!");
            return;
        }

        if (!attributes.TryGetValue("name", out string? userName) ||
            !attributes.TryGetValue("pwd", out string? password))
        {
            Debug.LogError("Invalid Arguments!\nUse </help -c login> for more info");
            return;
        }

        SpacetimeController.Instance.Login(userName, password);
    }

    private static void Register(Dictionary<string, string> attributes)
    {
        if (attributes.Values.Any(string.IsNullOrEmpty))
        {
            Debug.LogError("Attributes must not be empty!");
            return;
        }

        if (!attributes.TryGetValue("name", out string? userName) ||
            !attributes.TryGetValue("mail", out string? mail) ||
            !attributes.TryGetValue("pwd", out string? pwd) ||
            !attributes.TryGetValue("news", out string? news) ||
            !attributes.TryGetValue("agb", out string? agb))
        {
            Debug.LogError("Invalid Arguments!\nUse</help -c register> for more info");
            return;
        }

        if (news != "true" && news != "false")
        {
            Debug.LogError("news must be <true> or <false>");
            return;
        }
        if (agb != "true")
        {
            Debug.LogError("agb must be <true>");
            return;
        }

        SpacetimeController.Instance.Register(userName, mail, pwd, news == "true", true);
    }

    private static void AutoScroll(Dictionary<string, string> attributes)
    {
        if (attributes.Values.Any(string.IsNullOrEmpty))
        {
            Debug.LogError("Attributes must not be empty!");
            return;
        }

        switch (attributes.Count)
        {
            case 0:
                bool newVal = DebugViewModel.Instance.ToggleAutoScroll();
                Debug.Log($"AutoScroll was set to <{newVal}>");
                break;
            case 1:
                if (!attributes.TryGetValue("set", out string? scrollStr))
                {
                    Debug.LogError("Invalid Arguments!\nUse </help -c autoscroll> for more info");
                    return;
                }

                if (scrollStr != "true" && scrollStr != "false")
                {
                    Debug.LogError("set must be <true> or <false>");
                }

                DebugViewModel.Instance.SetAutoScroll(scrollStr == "true");
                Debug.Log($"AutoScroll was set to <{scrollStr}>");
                break;
            default:
                Debug.LogError("Invalid Arguments!\nUse </help -c autoscroll> for more info");
                return;
        }
    }

    private static void MessageLimit(Dictionary<string, string> attributes)
    {
        if (attributes.Values.Any(string.IsNullOrEmpty))
        {
            Debug.LogError("Attributes must not be empty!");
            return;
        }

        if (attributes.Count == 0)
        {
            Debug.Log($"MaxMessages is set to <{DebugViewModel.Instance.MaxMessages}>");
            return;
        }

        if (!attributes.TryGetValue("set", out string? maxValStr))
        {
            Debug.LogError("Invalid Arguments!\nUse </help -c messagelimit> for more info");
            return;
        }

        if (!int.TryParse(maxValStr, out int maxVal))
        {
            Debug.LogError("set must be an integer Value!");
            return;
        }

        DebugViewModel.Instance.MaxMessages = maxVal;
        Debug.Log($"MaxMessages was set to <{maxVal}>");
    }
}
