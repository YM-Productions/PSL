using SpacetimeDB;

namespace StdbModule.Utils;

public static class ClientLog
{
    // TODO:
    // - Documentation

    public static void Error(ReducerContext ctx, string message)
    {
        ctx.Db.ClientDebugLog.Insert(new(ctx.Sender, 0, message, ctx.Timestamp));
    }

    public static void Warning(ReducerContext ctx, string message)
    {
        ctx.Db.ClientDebugLog.Insert(new(ctx.Sender, 1, message, ctx.Timestamp));
    }

    public static void Info(ReducerContext ctx, string message)
    {
        ctx.Db.ClientDebugLog.Insert(new(ctx.Sender, 5, message, ctx.Timestamp));
    }
}
