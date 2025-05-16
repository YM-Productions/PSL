using System;
using SpacetimeDB;

namespace StdbModule.ClientTools;

public static partial class Module
{
    // TODO:
    // - Documentation

    [Table(Name = nameof(ClientDebugLog), Public = true)]
    public partial class ClientDebugLog
    {
        public Identity identity;
        public int Layer;
        public string Message;
        public Timestamp CreatedAt;

        public ClientDebugLog()
        {
            Message = string.Empty;
        }

        public ClientDebugLog(Identity identity, int layer, string message, Timestamp timestamp)
        {
            this.identity = identity;
            Layer = layer;
            Message = message;
            CreatedAt = timestamp;
        }
    }

#pragma warning disable STDB_UNSTABLE

    [SpacetimeDB.ClientVisibilityFilter]
    public static readonly Filter CLIENTDEBUGLOG_FILTER = new Filter.Sql(
        $"SELECT * FROM {nameof(ClientDebugLog)} WHERE identity = :sender"
    );

#pragma warning restore STDB_UNSTABLE
}
