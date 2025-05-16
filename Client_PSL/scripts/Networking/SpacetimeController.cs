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
public class SpacetimeController
{
    public static SpacetimeController Instance { get; private set; }
    private Logger logger;

    private const string HOST = "https://yaene.dev";
    private const string DBNAME = "psl0";

    private Identity? local_identity = null;
    private CancellationTokenSource cancellationTokenSource = new();
    Thread thread;


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

        logger.Log("Starting to connect temporary...");
        DbConnection conn = Temp_DbConnection();
        thread = new Thread(() => ProcessThread(conn, cancellationTokenSource.Token));
        thread.Start();
    }

    public void CloseCon()
    {
        logger.Log("Closing Connection...");

        cancellationTokenSource.Cancel();
        thread.Join();
    }

    private void ProcessThread(DbConnection conn, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                conn.FrameTick();
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

    #region TempSession

    // TODO:
    // - Temporary Connection without any Token
    // - OnConnect
    // - OnConnectError
    // - OnDisconnect

    private DbConnection Temp_DbConnection()
    {
        DbConnection conn = DbConnection.Builder()
            .WithUri(HOST)
            .WithModuleName(DBNAME)
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
            .Subscribe(new string[] { $"SELECT * FROM {nameof(ClientDebugLog)}" });
    }

    private void Temp_OnBaseSubscriptionApplied(SubscriptionEventContext ctx)
    {
        logger.Log("Subscribed to base Subscriotions");
        foreach (ClientDebugLog cdl in ctx.Db.ClientDebugLog.Iter().OrderBy(item => item.CreatedAt))
        {
            logger.Log(cdl.Message);
        }
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

    #endregion
}
