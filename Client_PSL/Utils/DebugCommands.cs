using SpacetimeDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using Networking.SpacetimeController;

namespace Utils.DebugCommands;

// TODO:
// - Documentation
// - Login
// - Register
// - LoggerControll
//      example:
//      /logger --layer 5 -> Only layer >= 5 will be shown
//      /logger --disable <name> -> This specific logger wont Log
//      /logger --focus <name> -> Only logs this specific logger

public static class DebugCommand
{
    public static Dictionary<string, Action<Dictionary<string, string>>> Commands = new()
    {
        { nameof(Help).ToLower(), Help },
        { nameof(Login).ToLower(), Login },
        { nameof(Register).ToLower(), Register },

        // NOTE: This is only temporary
        { "d" , _ => SpacetimeController.Instance.CloseCon() },
        { "tempsession" , _ => SpacetimeController.Instance.OpenTemporarySession() },
        { "time" , _ => Debug.Log(((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()).ToString()) },
    };

    public static void Help(Dictionary<string, string> attributes)
    {
        if (attributes.Count == 0)
        {
            Debug.Log("--------------------------\nCommand Format:\n/<command> --<attributeName> <attribute>\nExample:\n/Login --name NAME --Password PWD");
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

    public static void Login(Dictionary<string, string> attributes)
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

    public static void Register(Dictionary<string, string> attributes)
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
}
