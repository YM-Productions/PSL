using SpacetimeDB;

namespace StdbModule.Utils;

public static class ClientLog
{
    public static void Error(ReducerContext ctx, string message)
    {
        ctx.Db.ClientDebugLog.Insert(new(ctx.Identity, 0, message));
    }

    public static void Warning(ReducerContext ctx, string message)
    {
        ctx.Db.ClientDebugLog.Insert(new(ctx.Identity, 1, message));
    }

    public static void Info(ReducerContext ctx, string message)
    {
        ctx.Db.ClientDebugLog.Insert(new(ctx.Identity, 5, message));
    }
}
