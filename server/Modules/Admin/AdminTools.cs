using SpacetimeDB;

namespace StdbModule.Modules;

public static partial class Module
{
    [Table(Name = nameof(Admin), Public = true)]
    public partial class Admin
    {
        [PrimaryKey]
        public Identity identity;
        public string? Name;
        public Timestamp CreatedAt;
    }

#pragma warning disable STDB_UNSTABLE

    [SpacetimeDB.ClientVisibilityFilter]
    public static readonly Filter ADMIN_FILTER = new Filter.Sql(
        $"SELECT * FROM {nameof(Admin)} WHERE identity = :sender"
    );

#pragma warning restore STDB_UNSTABLE

    // [Reducer]
    // public static void GetAdmin(ReducerContext ctx, string name, string password)
    // {
    //     if (password == ADMIN_PWD)
    //     {
    //         Admin admin = new()
    //         {
    //             identity = ctx.Sender,
    //             Name = name,
    //             CreatedAt = ctx.Timestamp,
    //         };
    //
    //         ctx.Db.Admin.Insert(admin);
    //     }
    // }
}
