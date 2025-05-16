using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using SpacetimeDB;
using SpacetimeDB.Types;
using Utils;

namespace Networking.SpacetimeController;

// TODO:
// - Documentation
// - Base Spacetime Setup
// - TaskQueue for DbThread
// - Build ServerMessage to Log
public class SpacetimeController
{
    public static SpacetimeController Instance { get; private set; }
    private Logger logger;

    private const string HOST = "https://yaene.dev";
    private const string DBNAME = "psl";

    private DbConnection? connection = null;
    private Identity? local_identity = null;
    private CancellationTokenSource cancellationTokenSource = new();
    private Thread thread;

    private Guid tempToken = Guid.Empty;

    public SpacetimeController()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else return;

        logger = Logger.LoggerFactory.CreateLogger("SpacetimeController");
        logger.SetLevel(9);
        logger.Log("SpacetimeController initialized...");
    }

    public void CloseCon()
    {
        logger.Log("Closing Connection...");

        cancellationTokenSource.Cancel();
        thread.Join();
    }

    #region Session

    public void OpenSession(string token)
    {

    }

    #endregion

    #region TempSession

    public void OpenTemporarySession()
    {
        if (thread != null && thread.IsAlive)
        {
            Debug.LogError("You cant open a new Session - There already is a open one!");
            return;
        }

        cancellationTokenSource = new();
        logger.Log("Starting to connect temporary...");
        connection = Temp_DbConnection();
        Temp_RegisterCallbacks(connection);
        thread = new Thread(() => Temp_ProcessThread(connection, cancellationTokenSource.Token));
        thread.Start();
    }

    private void Temp_ProcessThread(DbConnection conn, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                conn.FrameTick();
                // if (conn.IsActive) conn.Reducers.Login("Yaene", "Banana");
                Thread.Sleep(100);
            }
        }
        finally
        {
            conn.Disconnect();

            if (conn.IsActive)
            {
                Debug.LogError("Unknown Error on manual disconnect!");
            }
            else
            {
                logger.Log("Disconnected successfully");
            }
        }
    }


    private DbConnection Temp_DbConnection()
    {
        tempToken = Guid.NewGuid();
        AuthToken.Init(tempToken.ToString());

        DbConnection conn = DbConnection.Builder()
            .WithUri(HOST)
            .WithModuleName(DBNAME)
            .WithToken(AuthToken.Token)
            .OnConnect(Temp_OnConnected)
            .OnConnectError(Temp_OnConnectError)
            .OnDisconnect(Temp_OnDisconnected) // NOTE: Only runs on Server disconnectEvent
            .Build();
        return conn;
    }

    private void Temp_OnConnected(DbConnection conn, Identity identity, string authToken)
    {
        logger.Log("Connected successfully");
        local_identity = identity;

        conn.SubscriptionBuilder()
            .OnApplied(Temp_OnBaseSubscriptionApplied)
            .Subscribe(new string[] {
                    $"SELECT * FROM {nameof(ClientDebugLog)}",
                    $"SELECT * FROM {nameof(PersistentSession)}",
                    });
    }

    private void Temp_OnBaseSubscriptionApplied(SubscriptionEventContext ctx)
    {
        logger.Log("Subscribed to base Subscriotions");
        // foreach (ClientDebugLog cdl in ctx.Db.ClientDebugLog.Iter().OrderBy(item => item.CreatedAt))
        // {
        //     logger.Log(cdl.Message);
        // }
    }

    private void Temp_OnConnectError(Exception e)
    {
        Debug.LogError($"Error while connecting: {e}");
    }

    private void Temp_OnDisconnected(DbConnection conn, Exception? e)
    {
        if (e != null)
        {
            Debug.LogError($"Disconnected abnormally: {e}");
        }
        else
        {
            logger.Log("Disconnected successfully");
        }
    }

    private void Temp_RegisterCallbacks(DbConnection conn)
    {
        conn.Db.PersistentSession.OnInsert += PersistentSession_OnInsert;

        conn.Db.ClientDebugLog.OnInsert += ClientDebugLog_OnInsert;
    }

    private void PersistentSession_OnInsert(EventContext ctx, PersistentSession value)
    {
        CloseCon();
        // TODO: Open new Final Game Connection
    }

    private void ClientDebugLog_OnInsert(EventContext ctx, ClientDebugLog value)
    {
        logger.Log($"New server Message: {value.Message}");
    }

    #endregion

    #region Loing/Register

    // TODO:
    // - Documentation
    // - Register Account
    // - Login

    public void Register(string userName, string mailAddress, string password, bool sendNews, bool agb)
    {
        if (connection == null ||
            !connection.IsActive)
        {
            Debug.LogError("Connect to a Server first!");
            return;
        }

        connection.Reducers.CreateAccount(userName, mailAddress, password, sendNews, agb, tempToken.ToString());
    }

    public void Login(string userName, string password)
    {
        if (connection == null ||
            !connection.IsActive)
        {
            Debug.LogError("Connect to a Server first!");
            return;
        }

        connection.Reducers.Login(userName, password);
    }

    #endregion
}
