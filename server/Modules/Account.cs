using SpacetimeDB;

namespace StdbModule.Modules
{
    public static partial class Module
    {
        [Table(Name = "Account", Public = false)]
        // [SpacetimeDB.Index.BTree(Name = "idx_Account_UserName_IsOnline", Columns = [nameof(UserName), nameof(IsOnline)])]
        public partial class Account
        {
            [PrimaryKey]
            Identity Identity;
            [Unique]
            public string UserName;
            [Unique]
            public string MailAddress;
            public string PasswordHash;
            public bool IsOnline;
            public bool SendNews;
            public bool AcceptedAGB;
            public int NameTokens;
        }

        public static partial class AccountReducers
        {
            [Reducer]
            public static 
        }
    }
}
