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
    private string _output = string.Empty;

    Identity? local_identity = null;
    ConcurrentQueue<(string Command, string Args)> input_queue = new();
    CancellationTokenSource cancellationTokenSource = new();
    Thread thread;

    public MainViewModel()
    {
        AuthToken.Init(".spacetime_csharp_psl");
        DbConnection? conn = null;
        conn = ConnectToDB();
        RegisterCallbacks(conn);
        thread = new Thread(() => ProcessThread(conn, cancellationTokenSource.Token));
        thread.Start();
    }

    public void CloseCon()
    {
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
        Output = "Connected"; 
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
        Output = $"Error while connectiong: {e}";
    }

    private void OnDisconnected(DbConnection conn, Exception e)
    {
        if (e != null)
        {
            Output = $"Disconnected abnormally: {e}";
        }
        else
        {
            Output = $"Disconnected succesfully.";
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

    public void PricessInput(string input)
    {
        if (input == null)
        {
            throw new Exception("Error: input was null!");
        }

        if (input.StartsWith("/name "))
        {
            input_queue.Enqueue(("name", input[6..]));
        }
        else
        {
            input_queue.Enqueue(("message", input));
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
            Output = $"{UserNameOrIdentity(insertedValue)} is online";
        }
    }

    private void User_OnUpdate(EventContext ctx, User oldValue, User newValue)
    {
        if (oldValue.Name != newValue.Name)
        {
            Output = $"{UserNameOrIdentity(oldValue)} renamed to {newValue.Name}";
        }
        else
        {
            Output = $"{UserNameOrIdentity(newValue)} Disconnected";
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

        Output = $"{senderName}: {message.Text}";
    }

    private void Reducer_OnSetNameEvent(ReducerEventContext ctx, string name)
    {
        ReducerEvent<Reducer> e = ctx.Event;
        if (e.CallerIdentity == local_identity && e.Status is Status.Failed(string error))
        {
            Output = $"Failed to change name to {name}: {error}";
        }
    }

    void Reducer_OnSendMessageEvent(ReducerEventContext ctx, string text)
    {
        ReducerEvent<Reducer> e = ctx.Event;
        if (e.CallerIdentity == local_identity && e.Status is Status.Failed(string error))
        {
            Output = $"Failed to send message {text}: {error}";
        }
    }
}
