using System;
using SpacetimeDB;

namespace StdbModule.Modules;

public static partial class Module
{
    /// <summary>
    /// Represents a client-side debug log entry stored in the SpacetimeDB.
    /// Each entry is associated with a specific client identity and contains
    /// information about the type and content of the log event.
    /// </summary>
    /// <remarks>
    /// This table is declared <c>Public = true</c>, allowing clients to query their
    /// own log entries based on identity, filtered via <see cref="CLIENTDEBUGLOG_FILTER"/>.
    /// It is primarily used for debugging and monitoring purposes.
    /// </remarks>
    [Table(Name = nameof(ClientDebugLog), Public = true)]
    public partial class ClientDebugLog
    {
        /// <summary>
        /// The identity of the client who generated the log entry.
        /// </summary>
        public Identity identity;

        /// <summary>
        /// The log severity layer:
        /// <list type="bullet">
        ///   <item><term>0</term><description>Error</description></item>
        ///   <item><term>1</term><description>Warning</description></item>
        ///   <item><term>5</term><description>Informational</description></item>
        /// </list>
        /// </summary>
        public int Layer;

        /// <summary>
        /// The log message describing the event.
        /// </summary>
        public string Message;

        /// <summary>
        /// The timestamp at which the log entry was created.
        /// </summary>
        public Timestamp CreatedAt;

        /// <summary>
        /// Default constructor required by SpacetimeDB. Initializes <see cref="Message"/> to an empty string.
        /// </summary>
        public ClientDebugLog()
        {
            Message = string.Empty;
        }

        /// <summary>
        /// Creates a new <see cref="ClientDebugLog"/> instance with the specified values.
        /// </summary>
        /// <param name="identity">The identity of the client who created the log.</param>
        /// <param name="layer">The severity level of the log (0 = error, 1 = warning, 5 = info).</param>
        /// <param name="message">The log message.</param>
        /// <param name="timestamp">The time the log entry was created.</param>
        public ClientDebugLog(Identity identity, int layer, string message, Timestamp timestamp)
        {
            this.identity = identity;
            Layer = layer;
            Message = message;
            CreatedAt = timestamp;
        }
    }

#pragma warning disable STDB_UNSTABLE

    /// <summary>
    /// A visibility filter ensuring that each client can only access their own <see cref="ClientDebugLog"/> entries.
    /// </summary>
    /// <remarks>
    /// This filter restricts data visibility based on the client's <c>Identity</c>,
    /// ensuring privacy and data isolation across all clients accessing the table.
    /// </remarks>
    [SpacetimeDB.ClientVisibilityFilter]
    public static readonly Filter CLIENTDEBUGLOG_FILTER = new Filter.Sql(
        $"SELECT * FROM {nameof(ClientDebugLog)} WHERE identity = :sender"
    );

    [SpacetimeDB.ClientVisibilityFilter]
    public static readonly Filter CLIENTDEBUGLOG_FILTER_ADMIN = new Filter.Sql(
        $"SELECT {nameof(ClientDebugLog)}.* FROM {nameof(ClientDebugLog)} JOIN {nameof(Admin)} WHERE {nameof(Admin)}.identity = :sender"
    );

#pragma warning restore STDB_UNSTABLE
}
