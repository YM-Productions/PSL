using SpacetimeDB;

namespace StdbModule.Modules;

public static partial class Module
{
    [Reducer(SpacetimeDB.ReducerKind.Init)]
    public static void Init(ReducerContext ctx)
    {
        // Adding all Admins
        ctx.Db.Admin.Insert(new()
        {
            identity = Identity.FromHexString("c20075c6ca015d47de247f8d42d766884ae607b54f0a5b2298489d8ab9b7b39c"),
            Name = "Mungg",
            CreatedAt = ctx.Timestamp,
        });

        // HACK: Create a Plot with identity 0
        ctx.Db.Plot.Insert(new Plot
        {
            identity = "0",
        });
    }
}
