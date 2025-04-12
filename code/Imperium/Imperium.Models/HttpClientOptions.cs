namespace Imperium.Models
{
    public class HttpClientOptions
    {

        public const string SectionName = "HttpClient";

        // <summary>
        /// How long the connection to is maintained until the connection is cycled
        /// </summary>
        public TimeSpan ConnectionLifeTime { get; set; } = TimeSpan.FromMinutes(60);
    }
}
