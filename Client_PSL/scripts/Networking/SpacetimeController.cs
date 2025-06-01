using System;
using System.Threading;
using System.Threading.Tasks;
using Client_PSL.ViewModels;
using SpacetimeDB;
using SpacetimeDB.Types;
using Utils;

namespace Networking.SpacetimeController;

/// <summary>
/// Manages the lifecycle and interaction with a SpacetimeDB server instance,
/// including connection handling, session management, authentication, and account operations.
/// </summary>
/// <remarks>
/// <para>
/// The <c>SpacetimeController</c> class acts as the central control unit for the client,
/// encapsulating all processes related to communication with the server:
/// </para>
/// <list type="bullet">
///   <item><description>Establishing connections using either a temporary or persistent token</description></item>
///   <item><description>Starting and stopping background threads that handle database communication</description></item>
///   <item><description>Registering new users and handling login flows</description></item>
///   <item><description>Managing authentication tokens and persistent session data</description></item>
/// </list>
/// <para>
/// The class ensures that all SpacetimeDB operations are only executed when an active connection exists.
/// Background processing (e.g., <c>ProcessThread</c>) runs on a separate thread and uses a <see cref="CancellationToken"/>
/// to support cooperative cancellation and graceful shutdown.
/// </para>
/// </remarks>
public class SpacetimeController
{
    /// <summary>
    /// Gets the global singleton instance of the <see cref="SpacetimeController"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property ensures that only one central instance of the <see cref="SpacetimeController"/> exists,
    /// which can be accessed globally throughout the application.
    /// </para>
    /// <para>
    /// The instance is typically initialized once at application startup and reused for
    /// all connections and operations involving the SpacetimeDB server.
    /// </para>
    /// </remarks>
    public static SpacetimeController Instance { get; private set; }
    private Logger logger;
    private Logger serverLogger;

    private const string HOST = "https://yaene.dev";
    private const string DBNAME = "psl";

    private DbConnection? connection = null;
    private Identity? local_identity = null;
    private CancellationTokenSource cancellationTokenSource = new();
    private Thread thread;

    public bool IsConnected => connection == null ? false : connection.IsActive;

    private string tempToken = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpacetimeController"/> class
    /// and sets it as the global singleton <see cref="Instance"/>, if not already set.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If <see cref="Instance"/> has not been set yet, the current instance is registered
    /// and a dedicated logger is created with the highest log level.
    /// </para>
    /// <para>
    /// If an instance already exists, the constructor exits immediately to enforce the singleton guarantee.
    /// </para>
    /// </remarks>
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

        serverLogger = Logger.LoggerFactory.CreateLogger("Server");
        serverLogger.SetLevel(6);
    }

    /// <summary>
    /// Gracefully closes the active connection by signaling cancellation and waiting for the background thread to finish.
    /// </summary>
    /// <remarks>
    /// This method performs two key actions:
    /// <list type="bullet">
    ///   <item><description>Logs a shutdown message via <c>logger.Log</c>.</description></item>
    ///   <item><description>Invokes <see cref="CancellationTokenSource.Cancel"/> to signal cooperative cancellation.</description></item>
    ///   <item><description>Waits for the background <see cref="Thread"/> (likely running a loop with the token) to finish using <see cref="Thread.Join"/>.</description></item>
    /// </list>
    /// This ensures that any background work is cleanly completed or aborted before disposing of resources.
    /// </remarks>
    public void CloseCon()
    {
        logger.Log("Closing Connection...");

        cancellationTokenSource.Cancel();
        thread.Join();
    }

    /// <summary>
    /// Retrieves the current database connection if the controller is connected.
    /// </summary>
    /// <remarks>
    /// This method returns the active <see cref="DbConnection"/> instance if the controller is currently connected to the database.
    /// If the controller is not connected, the method returns <c>null</c>.
    /// Use this method to access the underlying database connection for executing queries or commands.
    /// </remarks>
    /// <returns>
    /// The active <see cref="DbConnection"/> if connected; otherwise, <c>null</c>.
    /// </returns>
    public DbConnection? GetConnection()
    {
        if (!IsConnected)
            return null;
        return connection;
    }

    #region Session

    /// <summary>
    /// Initializes and starts a new database session using the provided authentication token.
    /// </summary>
    /// <param name="token">
    /// The session token used to authenticate the connection with the SpacetimeDB server.
    /// </param>
    /// <remarks>
    /// This method performs the following steps:
    /// <list type="bullet">
    ///   <item><description>Checks whether a session is already active by inspecting the current thread. If so, logs an error and returns early.</description></item>
    ///   <item><description>Initializes a new <see cref="CancellationTokenSource"/> for cooperative shutdown.</description></item>
    ///   <item><description>Logs the attempt to connect using <c>logger.Log</c>.</description></item>
    ///   <item><description>Creates a new database connection using <see cref="CreateDbConnection(string)"/> and stores it in <c>connection</c>.</description></item>
    ///   <item><description>Registers default callbacks via <see cref="RegisterBaseCallbacks"/>.</description></item>
    ///   <item><description>Starts a background thread that processes the connection using <see cref="ProcessThread(DbConnection, CancellationToken)"/>.</description></item>
    /// </list>
    /// If a session is already running, it prevents double initialization to avoid conflicts or data corruption.
    /// </remarks>
    private void OpenSession(string token)
    {
        if (thread != null && thread.IsAlive)
        {
            Debug.LogError("Can't open new Session - There already is a open one!");
            return;
        }

        cancellationTokenSource = new();
        logger.Log("Starting to connect...");
        connection = CreateDbConnection(token);
        RegisterBaseCallbacks();
        thread = new Thread(() => ProcessThread(connection, cancellationTokenSource.Token));
        thread.Start();
    }

    private DbConnection CreateDbConnection(string token)
    {
        DbConnection conn = DbConnection.Builder()
            .WithUri(HOST)
            .WithModuleName(DBNAME)
            .WithToken(token)
            .OnConnect(OnConnected)
            .OnConnectError(OnConnectError)
            .OnDisconnect(OnDisconnected)
            .Build();
        return conn;
    }

    private void OnConnected(DbConnection conn, Identity identity, string authToken)
    {
        logger.Log("Connected successfully");
        local_identity = identity;

        // conn.SubscriptionBuilder()
        //     .OnApplied(OnBaseSubscriptionApplied)
        //     .Subscribe(new string[] {
        //             $"SELECT * FROM {nameof(ClientDebugLog)}",
        //             });

        // HACK: Just temporaryly subscribe to all tables
        conn.SubscriptionBuilder()
            .OnApplied(OnBaseSubscriptionApplied)
            .SubscribeToAllTables();

        MainViewModel.Instance.SetLandingPage();
    }

    private void OnBaseSubscriptionApplied(SubscriptionEventContext ctx)
    {
        logger.Log("Subscribe to base Subscriptions");
    }

    private void OnConnectError(Exception e)
    {
        Debug.LogError($"Error while connecting: {e}");
    }

    private void OnDisconnected(DbConnection conn, Exception? e)
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

    private void RegisterBaseCallbacks()
    {
        connection.Db.ClientDebugLog.OnInsert += ClientDebugLog_OnInsert;
    }

    private void ClientDebugLog_OnInsert(EventContext ctx, ClientDebugLog value)
    {
        serverLogger.Log(value.Message);
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

            conn.Reducers.SetSenderOffline();
            for (int i = 0; i < 5; i++)
            {
                conn.FrameTick();
                Thread.Sleep(100);
            }
        }
        finally
        {
            // conn.Reducers.SetSenderOffline();
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

    #endregion

    #region TempSession

    /// <summary>
    /// Initializes and starts a temporary database session without requiring a persistent authentication token.
    /// </summary>
    /// <remarks>
    /// This method is intended for short-lived or unauthenticated interactions, such as login or account creation flows.
    /// It performs the following operations:
    /// <list type="bullet">
    ///   <item><description>Checks if another session is already active by validating <see cref="thread"/>. If so, logs an error and aborts.</description></item>
    ///   <item><description>Initializes a new <see cref="CancellationTokenSource"/> for cooperative cancellation.</description></item>
    ///   <item><description>Logs the attempt to start a temporary connection using <c>logger.Log</c>.</description></item>
    ///   <item><description>Creates a temporary <see cref="DbConnection"/> via <see cref="Temp_DbConnection"/> and stores it in <c>connection</c>.</description></item>
    ///   <item><description>Registers event callbacks for this temporary connection using <see cref="Temp_RegisterCallbacks(DbConnection)"/>.</description></item>
    ///   <item><description>Starts a background thread to process connection logic using <see cref="Temp_ProcessThread(DbConnection, CancellationToken)"/>.</description></item>
    /// </list>
    /// This session is fully isolated from persistent sessions and is typically used for guest or initial client flows.
    /// </remarks>
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
        tempToken = authToken;

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
        conn.Db.PersistentSession.OnInsert += Temp_PersistentSession_OnInsert;

        conn.Db.ClientDebugLog.OnInsert += Temp_ClientDebugLog_OnInsert;
    }

    private void Temp_PersistentSession_OnInsert(EventContext ctx, PersistentSession value)
    {
        _ = Task.Run(CloseCon);

        _ = Task.Run(async () =>
        {
            while (connection.IsActive)
            {
                await Task.Delay(100);
            }
            OpenSession(value.Tkn);
        });
        // while (connection.IsActive) ;
        // OpenSession(value.Tkn);
    }

    private void Temp_ClientDebugLog_OnInsert(EventContext ctx, ClientDebugLog value)
    {
        serverLogger.Log(value.Message);
    }

    #endregion

    #region Loing/Register

    /// <summary>
    /// Sends a request to the server to create a new user account using the provided registration data.
    /// </summary>
    /// <param name="userName">The desired unique username for the new account.</param>
    /// <param name="mailAddress">The email address to associate with the account.</param>
    /// <param name="password">The plaintext password that will be hashed server-side before storage.</param>
    /// <param name="sendNews">Whether the user wants to subscribe to news or updates.</param>
    /// <param name="agb">Whether the user accepts the AGB (Terms and Conditions). Must be <c>true</c> to proceed.</param>
    /// <remarks>
    /// This method performs a client-side check to ensure an active connection exists before attempting registration.
    /// If the connection is valid, it invokes the <c>CreateAccount</c> reducer on the server with the provided data.
    /// The <c>tempToken</c> is used to bind the account creation to the current temporary session.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no active connection to the server exists.
    /// </exception>
    public void Register(string userName, string mailAddress, string password, bool sendNews, bool agb)
    {
        if (connection == null ||
            !connection.IsActive)
        {
            Debug.LogError("Connect to a Server first!");
            return;
        }

        connection.Reducers.CreateAccount(userName, mailAddress, password, sendNews, agb, tempToken);
    }

    /// <summary>
    /// Sends a login request to the server using the provided credentials.
    /// </summary>
    /// <param name="userName">The username of the account to log into.</param>
    /// <param name="password">The plaintext password, which will be hashed server-side for verification.</param>
    /// <remarks>
    /// This method first checks whether a connection to the server is active. If not, it logs an error
    /// and aborts the login attempt. If connected, it invokes the <c>Login</c> reducer on the server with
    /// the provided credentials.
    ///
    /// The server handles identity resolution, password hashing and comparison, and creation or update of
    /// a <see cref="PersistentSession"/> upon successful authentication. Errors such as invalid username or
    /// password are handled server-side and communicated back via client logs or events.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no active connection to the server exists.
    /// </exception>
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
