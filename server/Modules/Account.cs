using System;
using SpacetimeDB;
using StdbModule.Utils;
using System.Net.Mail;

namespace StdbModule.Modules;

public static partial class Module
{
    [Table(Name = nameof(Account), Public = false)]
    // [SpacetimeDB.Index.BTree(Name = "idx_Account_UserName_IsOnline", Columns = [nameof(UserName), nameof(IsOnline)])]
    public partial class Account
    {
        [PrimaryKey]
        public Identity identity;
        [Unique]
        public string UserName;
        [Unique]
        public string MailAddress;
        public string PasswordHash;
        public bool MailVerified;
        public bool IsOnline;
        public bool SendNews;
        public bool AcceptedAGB;
        public int NameTokens;
        public int CreatedAt;

        public Account()
        {
            UserName = string.Empty;
            MailAddress = string.Empty;
            PasswordHash = string.Empty;

            CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }

    [Table(Name = nameof(PersistentSession), Public = true)]
    public partial class PersistentSession
    {
        [PrimaryKey]
        public Identity identity;
        public string Tkn;
        public int CreatedAt;

        public PersistentSession()
        {
            Tkn = string.Empty;
        }

        public PersistentSession(Identity identity, Guid token)
        {
            this.identity = identity;
            Tkn = token.ToString();

            CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }

    [Table(Name = nameof(ClientToken), Public = false)]
    public partial class ClientToken
    {
        [PrimaryKey]
        public Identity identity;
        public string Tkn;

        public ClientToken()
        {
            Tkn = string.Empty;
        }

        public ClientToken(Identity identity, string tkn)
        {
            this.identity = identity;
            Tkn = tkn;
        }
    }

#pragma warning disable STDB_UNSTABLE

    [SpacetimeDB.ClientVisibilityFilter]
    public static readonly Filter ACCOUNT_FILTER = new Filter.Sql(
        $"SELECT * FROM {nameof(Account)} WHERE identity = :sender"
    );

    [SpacetimeDB.ClientVisibilityFilter]
    public static readonly Filter PERSISTENTSESSION_FILTER = new Filter.Sql(
        $"SELECT * FROM {nameof(PersistentSession)} WHERE identity = :sender"
    );

#pragma warning restore STDB_UNSTABLE

    public static partial class AccountReducers
    {
        [Reducer]
        public static void CreateAccount(ReducerContext ctx, string userName, string mailAddress, string passwordHash, bool sendNews, bool acceptedAGB)
        {
            // Verify Identity
            if (ctx.Db.Account.identity.Find(ctx.Identity) != null)
            {
                Log.Warn($"{ctx.Identity.ToString()[0..8]} tried to create an Account, there was already an account with this Identity");
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
            if (new MailAddress(mailAddress) is MailAddress _)
            {
                ClientLog.Error(ctx, "Please use a valid EmailAddress");
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

            ClientToken token = new(account.identity, Guid.NewGuid().ToString());

            ctx.Db.Account.Insert(account);
            ctx.Db.ClientToken.Insert(token);

            Log.Info($"New Account {account.identity.ToString()[0..8]} was created");
            ClientLog.Info(ctx, "Your Account was successfully created");
        }

        [Reducer]
        public static void Login(ReducerContext ctx, string userName, string passwordHash)
        {
            // Verify Username Password
            // If correct -> Write Token to PersistentSession

            if (ctx.Db.Account.UserName.Find(userName) is Account account &&
                ctx.Db.ClientToken.identity.Find(account.identity) is ClientToken token)
            {
                if (account.PasswordHash != passwordHash)
                {
                    ClientLog.Error(ctx, "Invalid Password");
                }

                PersistentSession ps = new(ctx.Identity, Guid.Parse(token.Tkn));
                if (ctx.Db.PersistentSession.identity.Find(ps.identity) != null) ctx.Db.PersistentSession.identity.Update(ps);
                else ctx.Db.PersistentSession.Insert(ps);

                ClientLog.Info(ctx, "Login successful");
            }
            else
            {
                ClientLog.Error(ctx, "Invalid UserName");
                return;
            }
        }
    }
}
