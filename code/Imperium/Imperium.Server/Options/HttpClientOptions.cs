namespace Imperium.Server.Options;

public class HttpClientOptions
{
    public const string SectionName = "HttpClient";

    // <summary>
    /// How long the connection to is maintained until the connection is cycled
    /// </summary>
    public TimeSpan ConnectionLifeTime { get; set; } = TimeSpan.FromMinutes(60);

    /// <summary>
    /// Overall timeout for the entire request (connect + send + receive).
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Timeout for the initial TCP connection.
    /// </summary>
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Timeout to drain the response stream.
    /// </summary>
    public TimeSpan ResponseDrainTimeout { get; set; } = TimeSpan.FromSeconds(5);
}
