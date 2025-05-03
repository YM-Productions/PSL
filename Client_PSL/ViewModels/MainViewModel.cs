using CommunityToolkit.Mvvm.ComponentModel;

using System;
using System.Linq;
using SpacetimeDB;
using SpacetimeDB.Types;
using System.Collections.Concurrent;
using System.Threading;

namespace Client_PSL.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";

    Identity? local_identity = null;
    ConcurrentQueue<(string Command, string Args)> input_queue = new();

    public MainViewModel()
    {
        AuthToken.Init(".spacetime_csharp_psl");
        DbConnection? conn = null;
        RegisterCallbacks(conn);
        CancellationTokenSource cancellationTokenSource = new();
        Thread thread = new Thread(() => ProcessThread(conn, cancellationTokenSource.Token));
        thread.Start();
        InputLoop();
        cancellationTokenSource.Cancel();
        thread.Join();
    }

    const string HOST = "https://yaene.dev";
    const string DBNAME = "psl";

    DbConnection ConnectToDB()
    {
        DbConnection? conn = null;
        conn = DbConnection.Builder()
            .WithUri(HOST)
            .WithModuleName(DBNAME)
            .WithToken(AuthToken.Token)
            .OnConnect(OnConnected)
            .OnConnectError(OnConnectError)
            .OnDisconnect(OnDisconnected)
            .Build();
        return conn;
    }

    private void OnConnected(DbConnection conn, Identity identity, string authToken)
    {
        local_identity = identity;
        AuthToken.SaveToken(authToken);

        conn.SubscriptionBuilder()
            .OnApplied(OnSubscriptionApplied)
            .SubscribeToAllTables();
    }

    private void OnSubscriptionApplied(SubscriptionEventContext ctx)
    {
        Console.WriteLine("Connected");
        PrintMessagesInOrder(ctx.Db);
    }

    private void PrintMessagesInOrder(RemoteTables tables)
    {
        foreach (Message message in tables.Message.Iter().OrderBy(item => item.Sent))
        {
            PrintMessage(tables, message);
        }
    }

    private void OnConnectError(Exception e)
    {
        // Log Manager
        Console.Write($"Error while connectiong: {e}");
    }

    private void OnDisconnected(DbConnection conn, Exception e)
    {
        if (e != null)
        {
            Console.Write($"Disconnected abnormally: {e}");
        }
        else
        {
            Console.Write($"Disconnected succesfully.");
        }
    }

    private void ProcessThread(DbConnection conn, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                conn.FrameTick();
                ProcessCommands(conn.Reducers);
                Thread.Sleep(100);
            }
        }
        finally
        {
            conn.Disconnect();
        }
    }

    private void InputLoop()
    {
        while (true)
        {
            string input = "Console.Readline()";
            if (input == null)
            {
                break;
            }

            if (input.StartsWith("/name "))
            {
                input_queue.Enqueue(("name", input[6..]));
                continue;
            }
            else
            {
                input_queue.Enqueue(("message", input));
            }
        }
    }

    private void ProcessCommands(RemoteReducers reducers)
    {
        while (input_queue.TryDequeue(out (string Command, string Args) command))
        {
            switch (command.Command)
            {
                case "message":
                    reducers.SendMessage(command.Args);
                    break;
                case "name":
                    reducers.SetName(command.Args);
                    break;
            }
        }
    }

    private void RegisterCallbacks(DbConnection conn)
    {
        conn.Db.User.OnInsert += User_OnInsert;
        conn.Db.User.OnUpdate += User_OnUpdate;

        conn.Db.Message.OnInsert += Message_OnInsert;

        conn.Reducers.OnSetName += Reducer_OnSetNameEvent;
        conn.Reducers.OnSendMessage += Reducer_OnSendMessageEvent;
    }

    private string UserNameOrIdentity(User user) => user.Name ?? user.Identity.ToString()[..8];

    private void User_OnInsert(EventContext ctx, User insertedValue)
    {
        if (insertedValue.Online)
        {
            Console.WriteLine($"{UserNameOrIdentity(insertedValue)} is online");
        }
    }

    private void User_OnUpdate(EventContext ctx, User oldValue, User newValue)
    {
        if (oldValue.Name != newValue.Name)
        {
            Console.WriteLine($"{UserNameOrIdentity(oldValue)} renamed to {newValue.Name}");
        }
        else
        {
            Console.WriteLine($"{UserNameOrIdentity(newValue)} Disconnected");
        }
    }

    private void Message_OnInsert(EventContext ctx, Message insertedValue)
    {
        if (ctx.Event is not Event<Reducer>.SubscribeApplied)
        {
            PrintMessage(ctx.Db, insertedValue);
        }
    }

    private void PrintMessage(RemoteTables tables, Message message)
    {
        User sender = tables.User.Identity.Find(message.Sender);
        string senderName = "unknown";
        if (sender != null)
        {
            senderName = UserNameOrIdentity(sender);
        }

        Console.WriteLine($"{senderName}: {message.Text}");
    }

    private void Reducer_OnSetNameEvent(ReducerEventContext ctx, string name)
    {
        ReducerEvent<Reducer> e = ctx.Event;
        if (e.CallerIdentity == local_identity && e.Status is Status.Failed(string error))
        {
            Console.Write($"Failed to change name to {name}: {error}");
        }
    }

    void Reducer_OnSendMessageEvent(ReducerEventContext ctx, string text)
    {
        ReducerEvent<Reducer> e = ctx.Event;
        if (e.CallerIdentity == local_identity && e.Status is Status.Failed(string error))
        {
            Console.Write($"Failed to send message {text}: {error}");
        }
    }
}
