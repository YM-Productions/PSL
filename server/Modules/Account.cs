using SpacetimeDB;
using StdbModule.Utils;
using System.Net.Mail;

namespace StdbModule.Modules;

public static partial class Module
{
    /// <summary>
    /// Represents a user account in the system. This table stores essential user information including
    /// login credentials, contact details, and user preferences.
    ///
    /// <para>
    /// Each account is uniquely identified by an <c>identity</c>, and both the <c>UserName</c>
    /// and <c>MailAddress</c> fields are enforced as unique to ensure no duplicates exist.
    /// </para>
    ///
    /// <para>
    /// Additional fields track authentication state, newsletter preferences, AGB acceptance,
    /// and how many name changes are still available.
    /// </para>
    /// </summary>
    [Table(Name = "Account", Public = false)]
    // [SpacetimeDB.Index.BTree(Name = "idx_Account_UserName_IsOnline", Columns = [nameof(UserName), nameof(IsOnline)])]
    public partial class Account
    {
        /// <summary>
        /// Unique identity associated with this account. Acts as the primary key.
        /// </summary>
        [PrimaryKey]
        public Identity identity;

        /// <summary>
        /// The user's chosen username. Must be unique.
        /// </summary>
        [Unique]
        public string UserName;

        /// <summary>
        /// The user's email address. Must be unique.
        /// </summary>
        [Unique]
        public string MailAddress;

        /// <summary>
        /// The securely hashed password used for login authentication.
        /// </summary>
        public string PasswordHash;

        /// <summary>
        /// Indicates whether the user's email address has been verified.
        /// </summary>
        public bool MailVerified;

        /// <summary>
        /// Indicates whether the user is currently online.
        /// </summary>
        public bool IsOnline;

        /// <summary>
        /// Indicates whether the user has agreed to receive newsletters.
        /// </summary>
        public bool SendNews;

        /// <summary>
        /// Indicates whether the user has accepted the terms and conditions (AGB).
        /// </summary>
        public bool AcceptedAGB;

        /// <summary>
        /// The number of remaining username changes the user is allowed to perform.
        /// </summary>
        public int NameTokens;

        /// <summary>
        /// Initializes a new instance of the <c>Account</c> class with default values for string fields.
        /// 
        /// <para>
        /// This constructor sets <c>UserName</c>, <c>MailAddress</c>, and <c>PasswordHash</c> to <c>string.Empty</c>
        /// to ensure the object is in a valid initial state. It can be used for serialization, testing,
        /// or scenarios where an empty account shell is needed before population.
        /// </para>
        /// </summary>
        public Account()
        {
            UserName = string.Empty;
            MailAddress = string.Empty;
            PasswordHash = string.Empty;
        }
    }

    /// <summary>
    /// Represents a persistent session linked to a user identity.
    /// This table is used to authenticate users across multiple requests without requiring re-login.
    ///
    /// <para>
    /// Each session is uniquely identified by the <c>identity</c> and contains a <c>Token</c> that serves as a
    /// long-lived authentication token. The <c>CreatedAt</c> timestamp records when the session was initialized.
    /// </para>
    ///
    /// <para>
    /// The <c>identity</c> field is a foreign key referencing the corresponding <c>Account</c>,
    /// and should be used with cascade delete to automatically remove sessions when the account is deleted.
    /// </para>
    /// </summary>
    [Table(Name = "PersistentSession", Public = false)]
    internal partial class PersistentSession
    {
        /// <summary>
        /// The identity of the user associated with this session. Acts as a foreign key and primary key.
        /// </summary>
        [PrimaryKey]
        public Identity identity;

        /// <summary>
        /// The persistent token used to identify and authenticate the session.
        /// </summary>
        public Guid Token;

        /// <summary>
        /// The UTC timestamp of when the session was created.
        /// </summary>
        public DateTime CreatedAt;
    }

    public static partial class AccountReducers
    {
        /// <summary>
        /// The <c>CreateAccount</c> reducer handles the creation of a new user account in the database,
        /// performing validation on identity, username, email, and AGB acceptance before inserting the account.
        ///
        /// <para>
        /// This reducer is designed to be called from the client and will fail gracefully if:
        /// - An account with the same identity already exists.
        /// - The username is already taken.
        /// - The email is invalid or already in use.
        /// - The user has not accepted the terms and conditions (AGB).
        /// </para>
        ///
        /// <para>
        /// On success, a new <c>Account</c> instance is created with the provided values, and a corresponding
        /// <c>PersistentSession</c> is generated and inserted. This session allows the user to authenticate in future requests.
        /// The initial number of available name change tokens is defined by <c>SETTINGS.initial_name_tokens</c>.
        /// </para>
        ///
        /// <para>
        /// Example usage:
        /// <code>
        /// var status = Reducers.CreateAccount(ctx, "NewUser", "mail@example.com", hash, true, true);
        /// if (status == (int)Status.Success)
        /// {
        ///     Console.WriteLine("Account created successfully.");
        /// }
        /// </code>
        /// </para>
        /// </summary>
        /// <param name="ctx">The reducer context, including identity and database access.</param>
        /// <param name="userName">The desired username. Must be unique.</param>
        /// <param name="mailAddress">The user's email address. Must be valid and not already registered.</param>
        /// <param name="passwordHash">A securely hashed password string.</param>
        /// <param name="sendNews">Whether the user agrees to receive newsletters.</param>
        /// <param name="acceptedAGB">Whether the user accepted the terms and conditions (must be <c>true</c>).</param>
        /// <returns>
        /// A status code represented as <c>int</c>:
        /// <list type="bullet">
        /// <item><term><see cref="Status.Success"/></term><description>Account creation succeeded and session initialized.</description></item>
        /// <item><term><see cref="Status.UnknownError"/></term><description>Identity is already linked to an account.</description></item>
        /// <item><term><see cref="Status.UserNameTaken"/></term><description>Username is already in use.</description></item>
        /// <item><term><see cref="Status.InvalidMail"/></term><description>Email is invalid or already registered.</description></item>
        /// <item><term><see cref="Status.MustAcceptAGB"/></term><description>AGB (terms and conditions) not accepted.</description></item>
        /// </list>
        /// </returns>
        [Reducer]
        public static int CreateAccount(ReducerContext ctx, string userName, string mailAddress, string passwordHash, bool sendNews, bool acceptedAGB)
        {
            // Verify Identity
            if (ctx.Db.Account.identity.Find(ctx.Identity) != null) return (int)Status.UnknownError;
            // Verify UserName
            if (ctx.Db.Account.UserName.Find(userName) != null) return (int)Status.UserNameTaken;
            // Verify MailAddress
            if (ctx.Db.Account.MailAddress.Find(mailAddress) != null &&
                new MailAddress(mailAddress) is MailAddress _) return (int)Status.InvalidMail;
            // Verify AGB
            if (acceptedAGB) return (int)Status.MustAcceptAGB;

            Account account = new()
            {
                identity = ctx.Identity,
                UserName = userName,
                MailAddress = mailAddress,
                PasswordHash = passwordHash,
                MailVerified = false,
                IsOnline = false,
                SendNews = sendNews,
                AcceptedAGB = acceptedAGB,
                NameTokens = SETTINGS.initial_name_tokens,
            };

            PersistentSession persistentSession = new()
            {
                identity = account.identity,
                Token = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
            };

            ctx.Db.Account.Insert(account);
            ctx.Db.PersistentSession.Insert(persistentSession);

            // Creation succeded
            return (int)Status.Success;
        }

        /// <summary>
        /// The <c>Login</c> reducer handles user authentication based on username and password.
        /// On successful login, the associated persistent session token is returned.
        ///
        /// <para>
        /// This reducer performs the following checks:
        /// - Validates that an account with the provided username exists.
        /// - Verifies that the provided password hash matches the stored hash.
        /// - Retrieves the corresponding <c>PersistentSession</c> and returns its token.
        /// </para>
        ///
        /// <para>
        /// If any of the validation steps fail, an appropriate status code is returned and the output token remains <c>null</c>.
        /// </para>
        ///
        /// <para>
        /// Example usage:
        /// <code>
        /// var status = Reducers.Login(ctx, "User123", hash, out Guid? token);
        /// if (status == (int)Status.Success)
        /// {
        ///     Console.WriteLine($"Login succeeded. Session token: {token}");
        /// }
        /// </code>
        /// </para>
        /// </summary>
        /// <param name="ctx">The reducer context, including database access.</param>
        /// <param name="userName">The username used for login.</param>
        /// <param name="passwordHash">The hashed password to verify.</param>
        /// <param name="Token">The session token returned on success; otherwise <c>null</c>.</param>
        /// <returns>
        /// A status code represented as <c>int</c>:
        /// <list type="bullet">
        /// <item><term><see cref="Status.Success"/></term><description>Login succeeded and token returned.</description></item>
        /// <item><term><see cref="Status.InvalidUserName"/></term><description>No account with the given username was found.</description></item>
        /// <item><term><see cref="Status.InvalidPassword"/></term><description>Password hash does not match the stored value.</description></item>
        /// <item><term><see cref="Status.UnknownError"/></term><description>No persistent session found for the account.</description></item>
        /// </list>
        /// </returns>
        [Reducer]
        public static int Login(ReducerContext ctx, string userName, string passwordHash, out Guid? Token)
        {
            Token = null;

            // Verify Account By UserName
            if (ctx.Db.Account.UserName.Find(userName) is Account account)
            {
                // Verify PasswordHash
                if (account.PasswordHash != passwordHash) return (int)Status.InvalidPassword;

                // Check if Token exists
                if (ctx.Db.PersistentSession.identity.Find(account.identity) is PersistentSession ps)
                {
                    Token = ps.Token;
                    return (int)Status.Success;
                }

                return (int)Status.UnknownError;
            }
            else return (int)Status.InvalidUserName;
        }
    }
}
