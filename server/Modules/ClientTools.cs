using System;
using SpacetimeDB;

namespace StdbModule.ClientTools;

public static partial class Module
{
    [Table(Name = nameof(ClientDebugLog), Public = true)]
    public partial class ClientDebugLog
    {
        [PrimaryKey]
        public Identity identity;
        public int Layer;
        public string Message;
        public int CreatedAt;

        public ClientDebugLog()
        {
            Message = string.Empty;
        }

        public ClientDebugLog(Identity identity, int layer, string message)
        {
            this.identity = identity;
            Layer = layer;
            Message = message;
            CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }

#pragma warning disable STDB_UNSTABLE

    [SpacetimeDB.ClientVisibilityFilter]
    public static readonly Filter CLIENTDEBUGLOG_FILTER = new Filter.Sql(
        $"SELECT * FROM {nameof(ClientDebugLog)} WHERE identity = :sender"
    );

#pragma warning restore STDB_UNSTABLE
}
