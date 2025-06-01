using System;
using System.Diagnostics;
using System.Text;
using SpacetimeDB;
using StdbModule.Utils;
using StdbModule.Utils.Cryptography;

namespace StdbModule.Modules;

public static partial class Module
{
    /// <summary>
    /// Represents a user account in the system. This table is used to store authentication,
    /// contact, and status information for each registered user.
    /// </summary>
    /// <remarks>
    /// This table is publicly accessible (<c>Public = true</c>) and may be queried by clients.
    /// It is uniquely identified by the <see cref="identity"/> field. The table enforces uniqueness
    /// on both <see cref="UserName"/> and <see cref="MailAddress"/> to prevent duplicates.
    ///
    /// The account also contains fields related to user verification, preferences,
    /// and game-specific metadata like <see cref="NameTokens"/>.
    /// </remarks>
    [Table(Name = nameof(Account), Public = true)]
    // [SpacetimeDB.Index.BTree(Name = "idx_Account_UserName_IsOnline", Columns = [nameof(UserName), nameof(IsOnline)])]
    public partial class Account
    {
        /// <summary>
        /// The unique identity associated with the account, used as the primary key.
        /// </summary>
        [PrimaryKey]
        public Identity identity;

        /// <summary>
        /// The unique username chosen by the user. This field must be globally unique.
        /// </summary>
        [Unique]
        public string UserName;

        /// <summary>
        /// The unique email address associated with the account. Used for account recovery and verification.
        /// </summary>
        [Unique]
        public string MailAddress;

        /// <summary>
        /// The hashed password for secure authentication.
        /// This should be stored using a secure hash function like SHA-256.
        /// </summary>
        public string PasswordHash;

        /// <summary>
        /// Indicates whether the user has successfully verified their email address.
        /// </summary>
        public bool MailVerified;

        /// <summary>
        /// Indicates whether the user is currently online or logged in.
        /// </summary>
        public bool IsOnline;

        /// <summary>
        /// Determines whether the user has opted in to receive newsletters or product updates.
        /// </summary>
        public bool SendNews;

        /// <summary>
        /// Indicates whether the user has accepted the Terms of Service (AGB).
        /// Required for account creation and full access to features.
        /// </summary>
        public bool AcceptedAGB;

        /// <summary>
        /// Represents how many name change tokens the user currently holds.
        /// These tokens may be spent to change the <see cref="UserName"/>.
        /// </summary>
        public int NameTokens;

        /// <summary>
        /// The timestamp when the account was created.
        /// </summary>
        public Timestamp CreatedAt;

        /// <summary>
        /// Default constructor required for SpacetimeDB table instantiation.
        /// Initializes string fields to empty values.
        /// </summary>
        public Account()
        {
            UserName = string.Empty;
            MailAddress = string.Empty;
            PasswordHash = string.Empty;
        }
    }

    /// <summary>
    /// Represents a long-lived session token associated with a user identity.
    /// Used to persist authenticated sessions across multiple connections.
    /// </summary>
    /// <remarks>
    /// Each session is uniquely identified by <see cref="identity"/> and contains a secure token
    /// that can be reused to restore the session state. This table is declared as <c>Public = true</c>
    /// to allow clients to access or validate their own session data via filters.
    /// </remarks>
    [Table(Name = nameof(PersistentSession), Public = true)]
    public partial class PersistentSession
    {
        /// <summary>
        /// The identity associated with the session. Acts as the primary key.
        /// Only one persistent session can exist per identity.
        /// </summary>
        [PrimaryKey]
        public Identity identity;

        /// <summary>
        /// The persistent session token. This token is typically generated server-side
        /// and securely returned to the client for re-authentication purposes.
        /// </summary>
        public string Tkn;

        /// <summary>
        /// The timestamp at which the session was created. Can be used for expiration logic.
        /// </summary>
        public Timestamp CreatedAt;

        /// <summary>
        /// Default constructor required by SpacetimeDB. Initializes <see cref="Tkn"/> to an empty string.
        /// </summary>
        public PersistentSession()
        {
            Tkn = string.Empty;
        }

        /// <summary>
        /// Constructs a new <see cref="PersistentSession"/> instance with the specified identity, token, and creation time.
        /// </summary>
        /// <param name="identity">The identity of the user associated with the session.</param>
        /// <param name="token">The persistent token string for session recovery.</param>
        /// <param name="createdAt">The timestamp when the session was created.</param>
        public PersistentSession(Identity identity, string token, Timestamp createdAt)
        {
            this.identity = identity;
            Tkn = token;

            CreatedAt = createdAt;
        }
    }

    /// <summary>
    /// Represents a temporary or internal-use token associated with a specific client identity.
    /// Used for short-lived authentication or handshake mechanisms on the server side.
    /// </summary>
    /// <remarks>
    /// This table is declared with <c>Public = false</c>, meaning it is only accessible on the server side.
    /// It is typically used for intermediate steps in authentication flows, such as exchanging credentials
    /// for a persistent session token or validating a login attempt.
    /// </remarks>
    [Table(Name = nameof(ClientToken), Public = false)]
    public partial class ClientToken
    {
        /// <summary>
        /// The identity associated with this token. Serves as the primary key.
        /// </summary>
        [PrimaryKey]
        public Identity identity;

        /// <summary>
        /// The token string assigned to this identity. Typically generated server-side.
        /// </summary>
        public string Tkn;

        /// <summary>
        /// Default constructor required by SpacetimeDB. Initializes <see cref="Tkn"/> to an empty string.
        /// </summary>
        public ClientToken()
        {
            Tkn = string.Empty;
        }

        /// <summary>
        /// Constructs a new <see cref="ClientToken"/> with the given identity and token value.
        /// </summary>
        /// <param name="identity">The identity associated with the token.</param>
        /// <param name="tkn">The token string to store.</param>
        public ClientToken(Identity identity, string tkn)
        {
            this.identity = identity;
            Tkn = tkn;
        }
    }

#pragma warning disable STDB_UNSTABLE

    /// <summary>
    /// A visibility filter that restricts access to <see cref="Account"/> records,
    /// allowing each client to see only their own account entry.
    /// </summary>
    /// <remarks>
    /// This filter enforces per-client data isolation by selecting rows where
    /// <c>identity = :sender</c>. It prevents clients from querying or viewing
    /// other users' account data while still allowing them to access their own information.
    /// </remarks>
    [SpacetimeDB.ClientVisibilityFilter]
    public static readonly Filter ACCOUNT_FILTER = new Filter.Sql(
        $"SELECT * FROM {nameof(Account)} WHERE identity = :sender"
    );

    [SpacetimeDB.ClientVisibilityFilter]
    public static readonly Filter ACCOUNT_FILTER_ADMIN = new Filter.Sql(
        $"SELECT {nameof(Account)}.* FROM {nameof(Account)} JOIN {nameof(Admin)} WHERE {nameof(Admin)}.identity = :sender"
    );

    /// <summary>
    /// A visibility filter that restricts access to <see cref="PersistentSession"/> entries
    /// so that each client can view only their own persistent session data.
    /// </summary>
    /// <remarks>
    /// This filter is useful for session validation and continuity, while preserving privacy
    /// by ensuring clients do not have access to other users' session tokens.
    /// </remarks>
    [SpacetimeDB.ClientVisibilityFilter]
    public static readonly Filter PERSISTENTSESSION_FILTER = new Filter.Sql(
        $"SELECT * FROM {nameof(PersistentSession)} WHERE identity = :sender"
    );

    [SpacetimeDB.ClientVisibilityFilter]
    public static readonly Filter PERSISTENTSESSION_FILTER_ADMIN = new Filter.Sql(
        $"SELECT {nameof(PersistentSession)}.* FROM {nameof(PersistentSession)} JOIN {nameof(Admin)} WHERE {nameof(Admin)}.identity = :sender"
    );

#pragma warning restore STDB_UNSTABLE

    /// <summary>
    /// Contains server-side reducer methods for handling operations related to user accounts,
    /// such as authentication, registration, profile updates, and state management.
    /// </summary>
    /// <remarks>
    /// All methods defined in this static partial class are intended to be invoked by clients
    /// through SpacetimeDB reducers. These methods directly manipulate the <see cref="Account"/> table
    /// and enforce validation, permission checks, and business logic.
    ///
    /// This class is marked as <c>partial</c> to allow modular separation of different reducer types,
    /// such as login, registration, or settings updates, into separate files or logical groups.
    /// </remarks>
    public static partial class AccountReducers
    {
        /// <summary>
        /// Creates a new user account and stores it in the <see cref="Account"/> table,
        /// along with an associated <see cref="ClientToken"/> for temporary session handling.
        /// </summary>
        /// <param name="ctx">
        /// The current <see cref="ReducerContext"/> containing metadata such as the sender identity and current timestamp.
        /// </param>
        /// <param name="userName">The desired unique username for the account.</param>
        /// <param name="mailAddress">The unique email address to associate with the account.</param>
        /// <param name="password">The plaintext password that will be hashed and stored securely.</param>
        /// <param name="sendNews">Whether the user wants to receive news or updates.</param>
        /// <param name="acceptedAGB">Whether the user accepted the Terms and Conditions (AGB). Must be <c>true</c> to proceed.</param>
        /// <param name="usedToken">A temporary session token to associate with the account via the <see cref="ClientToken"/> table.</param>
        /// <remarks>
        /// This reducer performs the following validations before creating the account:
        /// <list type="bullet">
        /// <item><description>Ensures no account already exists for the current identity.</description></item>
        /// <item><description>Checks that the username is not already taken.</description></item>
        /// <item><description>Checks that the email address is not already registered.</description></item>
        /// <item><description>Verifies that the AGB (terms) have been accepted.</description></item>
        /// </list>
        /// On success, the method:
        /// <list type="bullet">
        /// <item><description>Hashes the password securely using SHA-256.</description></item>
        /// <item><description>Inserts a new <see cref="Account"/> entry tied to the sender’s identity.</description></item>
        /// <item><description>Stores the provided token in the <see cref="ClientToken"/> table.</description></item>
        /// <item><description>Writes log messages to both the server log and client debug log.</description></item>
        /// </list>
        /// </remarks>
        [Reducer]
        public static void CreateAccount(ReducerContext ctx, string userName, string mailAddress, string password, bool sendNews, bool acceptedAGB, string usedToken)
        {
            // Verify Identity
            if (ctx.Db.Account.identity.Find(ctx.Sender) != null)
            {
                Log.Warn($"{ctx.Sender.ToString()[0..8]} tried to create an Account, there was already an account with this Identity");
                ClientLog.Error(ctx, "There is already a existing Account with this Identity");
                return;
            }
            // Verify UserName
            if (ctx.Db.Account.UserName.Find(userName) != null)
            {
                ClientLog.Error(ctx, $"The Name {userName} is already taken");
                return;
            }
            // Verify MailAddress
            if (ctx.Db.Account.MailAddress.Find(mailAddress) != null)
            {
                ClientLog.Error(ctx, "There already is a Account with this EmailAddress");
                return;
            }
            // Verify AGB
            if (!acceptedAGB)
            {
                ClientLog.Error(ctx, "You must accept the AGB");
                return;
            }

            Account account = new()
            {
                identity = ctx.Sender,
                UserName = userName,
                MailAddress = mailAddress,
                PasswordHash = Sha256.ComputeHash(password),
                MailVerified = false,
                IsOnline = false,
                SendNews = sendNews,
                AcceptedAGB = acceptedAGB,
                NameTokens = SETTINGS.initial_name_tokens,
                CreatedAt = ctx.Timestamp,
            };

            ClientToken token = new(account.identity, usedToken);

            ctx.Db.Account.Insert(account);
            ctx.Db.ClientToken.Insert(token);

            Log.Info($"New Account {account.identity.ToString()[0..8]} was created");
            ClientLog.Info(ctx, "Your Account was successfully created");
        }

        /// <summary>
        /// Attempts to log a user in using their username and password.  
        /// On success, a new or updated <see cref="PersistentSession"/> is created using a stored <see cref="ClientToken"/>.
        /// </summary>
        /// <param name="ctx">
        /// The current <see cref="ReducerContext"/>, which provides the sender's identity and current timestamp.
        /// </param>
        /// <param name="userName">The username provided by the user attempting to log in.</param>
        /// <param name="password">The plaintext password, which will be hashed and compared against the stored hash.</param>
        /// <remarks>
        /// This reducer performs the following steps:
        /// <list type="bullet">
        /// <item><description>Looks up the <see cref="Account"/> by <paramref name="userName"/>.</description></item>
        /// <item><description>Ensures a <see cref="ClientToken"/> exists for the account's identity.</description></item>
        /// <item><description>Validates the password against the stored hash.</description></item>
        /// <item><description>Creates or updates a <see cref="PersistentSession"/> entry for session continuity.</description></item>
        /// <item><description>Logs the result (success or failure) to the client debug log.</description></item>
        /// </list>
        /// If the username does not exist or the token is missing, an error message "Invalid UserName" is returned.
        /// If the password is incorrect, "Invalid Password" is returned.
        /// </remarks>
        [Reducer]
        public static void Login(ReducerContext ctx, string userName, string password)
        {
            if (ctx.Db.Account.UserName.Find(userName) is Account account &&
                ctx.Db.ClientToken.identity.Find(account.identity) is ClientToken token)
            {
                if (account.PasswordHash != Sha256.ComputeHash(password))
                {
                    ClientLog.Error(ctx, "Invalid Password");
                    return;
                }

                PersistentSession ps = new(ctx.Sender, token.Tkn, ctx.Timestamp);
                if (ctx.Db.PersistentSession.identity.Find(ps.identity) != null) ctx.Db.PersistentSession.identity.Update(ps);
                else ctx.Db.PersistentSession.Insert(ps);

                account.IsOnline = true;
                ctx.Db.Account.identity.Update(account);

                ClientLog.Info(ctx, "Login successful");
            }
            else
            {
                ClientLog.Error(ctx, "Invalid UserName");
                return;
            }
        }

        /// <summary>
        /// Attempts to change the username of the account associated with the current sender identity.
        /// </summary>
        /// <param name="ctx">
        /// The current <see cref="ReducerContext"/> containing the sender's identity and database access.
        /// </param>
        /// <param name="userName">
        /// The new username the sender wishes to set. Must be non-empty and unique across all accounts.
        /// </param>
        /// <remarks>
        /// <para>This reducer performs the following checks before applying the change:</para>
        /// <list type="bullet">
        ///   <item><description>Verifies that the sender has an existing <see cref="Account"/>.</description></item>
        ///   <item><description>Checks that the user has at least one <c>NameToken</c> available.</description></item>
        ///   <item><description>Ensures that the new username is not empty or already in use.</description></item>
        ///   <item><description>Ensures the new username is actually different from the current one.</description></item>
        /// </list>
        /// <para>
        /// If all checks pass, the username is updated, one <c>NameToken</c> is consumed, and the change is saved.
        /// Success and error feedback is sent via <see cref="ClientLog.Info"/> or <see cref="ClientLog.Error"/>.
        /// </para>
        /// </remarks>
        [Reducer]
        public static void ChangeSenderUserName(ReducerContext ctx, string userName)
        {
            if (ctx.Db.Account.identity.Find(ctx.Sender) is Account account)
            {
                if (account.NameTokens <= 0)
                {
                    ClientLog.Info(ctx, "You need at leadst 1 Token to change your Name!");
                    return;
                }

                if (string.IsNullOrEmpty(userName))
                {
                    ClientLog.Error(ctx, "UserName must not be empty!");
                    return;
                }

                if (account.UserName == userName)
                {
                    ClientLog.Info(ctx, $"{userName} already is set as your UserName");
                    return;
                }

                if (ctx.Db.Account.UserName.Find(userName) != null)
                {
                    ClientLog.Info(ctx, $"The Name <{userName}> is already taken!");
                    return;
                }

                account.UserName = userName;
                account.NameTokens -= 1;
                ctx.Db.Account.identity.Update(account);
                ClientLog.Info(ctx, $"Successfully changed UserName to <{account.UserName}>");
            }
            else
            {
                ClientLog.Error(ctx, "No Account found with Identity");
                return;
            }
        }

        /// <summary>
        /// Attempts to update the email address of the account associated with the current sender identity,
        /// after verifying the user's password.
        /// </summary>
        /// <param name="ctx">
        /// The current <see cref="ReducerContext"/>, which includes the sender's identity and access to the database.
        /// </param>
        /// <param name="password">
        /// The current plaintext password of the account, used to authorize the email change.
        /// </param>
        /// <param name="newMail">
        /// The new email address to associate with the account. Must be unique and non-empty.
        /// </param>
        /// <remarks>
        /// <para>This reducer performs the following validations before applying the change:</para>
        /// <list type="bullet">
        ///   <item><description>Checks that the sender has a valid <see cref="Account"/> entry.</description></item>
        ///   <item><description>Verifies the provided <paramref name="password"/> by hashing it and comparing it to the stored hash.</description></item>
        ///   <item><description>Ensures <paramref name="newMail"/> is not null or empty.</description></item>
        ///   <item><description>Ensures the new email is different from the current one.</description></item>
        ///   <item><description>Checks that the new email is not already associated with another account.</description></item>
        /// </list>
        /// <para>
        /// If all checks succeed, the email is updated, the <c>MailVerified</c> flag is reset to <c>false</c>,
        /// and the change is saved in the database. Status messages are sent back to the client via <see cref="ClientLog"/>.
        /// </para>
        /// </remarks>
        [Reducer]
        public static void ChangeSenderMailAddress(ReducerContext ctx, string password, string newMail)
        {
            if (ctx.Db.Account.identity.Find(ctx.Sender) is Account account)
            {
                if (account.PasswordHash != Sha256.ComputeHash(password))
                {
                    ClientLog.Info(ctx, "Invalid Password");
                    return;
                }

                if (string.IsNullOrEmpty(newMail))
                {
                    ClientLog.Info(ctx, "EMailAddress must not be empty!");
                    return;
                }

                if (account.MailAddress == newMail)
                {
                    ClientLog.Info(ctx, $"EMail is already set to {newMail}");
                    return;
                }

                if (ctx.Db.Account.MailAddress.Find(newMail) != null)
                {
                    ClientLog.Info(ctx, "This EMail is already in use!");
                    return;
                }

                account.MailAddress = newMail;
                account.MailVerified = false;
                ctx.Db.Account.identity.Update(account);
                ClientLog.Info(ctx, $"Successfully changed EMail to <{account.MailAddress}>");
            }
            else
            {
                ClientLog.Error(ctx, "No Account found with Identity");
                return;
            }
        }

        /// <summary>
        /// Marks the sender's account as online by setting the <c>IsOnline</c> flag to <c>true</c>.
        /// </summary>
        /// <param name="ctx">
        /// The current <see cref="ReducerContext"/> containing the sender's identity and database access.
        /// </param>
        /// <remarks>
        /// <para>
        /// This reducer looks up the <see cref="Account"/> associated with the current sender.
        /// If found, it sets <c>IsOnline</c> to <c>true</c> and updates the record in the database.
        /// </para>
        /// <para>
        /// If no account is found for the sender identity, an error is logged to the client's debug log.
        /// </para>
        /// </remarks>
        [Reducer]
        public static void SetSenderOnline(ReducerContext ctx)
        {
            if (ctx.Db.Account.identity.Find(ctx.Sender) is Account account)
            {
                account.IsOnline = true;
                ctx.Db.Account.identity.Update(account);
            }
            else
            {
                ClientLog.Error(ctx, "Account does not exist");
            }
        }

        /// <summary>
        /// Marks the sender's account as offline by setting the <c>IsOnline</c> flag to <c>false</c>.
        /// </summary>
        /// <param name="ctx">
        /// The current <see cref="ReducerContext"/> containing the sender's identity and database access.
        /// </param>
        /// <remarks>
        /// <para>
        /// This reducer looks up the <see cref="Account"/> associated with the sender.
        /// If found, it sets <c>IsOnline</c> to <c>false</c> and updates the record in the database.
        /// </para>
        /// <para>
        /// If no matching account is found, an error is logged to the client's debug log.
        /// </para>
        /// </remarks>
        [Reducer]
        public static void SetSenderOffline(ReducerContext ctx)
        {
            if (ctx.Db.Account.identity.Find(ctx.Sender) is Account account)
            {
                account.IsOnline = false;
                ctx.Db.Account.identity.Update(account);
            }
            else
            {
                ClientLog.Error(ctx, "Account does not exist");
            }
        }

        /// <summary>
        /// Updates the sender's account preference for receiving newsletters and updates.
        /// </summary>
        /// <param name="ctx">
        /// The current <see cref="ReducerContext"/> containing the sender's identity and database access.
        /// </param>
        /// <param name="sendNews">
        /// A boolean value indicating whether the user wants to receive newsletters and promotional content.
        /// </param>
        /// <remarks>
        /// <para>
        /// This reducer looks up the <see cref="Account"/> associated with the sender identity.
        /// If the account exists, the <c>SendNews</c> flag is updated accordingly and saved to the database.
        /// </para>
        /// <para>
        /// If the account is not found, an error is logged to the client's debug log.
        /// </para>
        /// </remarks>
        [Reducer]
        public static void SetSenderNews(ReducerContext ctx, bool sendNews)
        {
            if (ctx.Db.Account.identity.Find(ctx.Sender) is Account account)
            {
                account.SendNews = sendNews;
                ctx.Db.Account.identity.Update(account);
            }
            else
            {
                ClientLog.Error(ctx, "Account does not exist");
            }
        }
    }
}
