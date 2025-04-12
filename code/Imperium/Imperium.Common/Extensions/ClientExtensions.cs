using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Imperium.Common.Extensions;

public static class ClientExtensions
{
    /// <summary>
    /// IMPORTANT: this method exists because by default .NET PostAsJson will set 
    /// TransferEncodingChunked to true and the content length to zero. This breaks servers
    /// (e.g. simple device HTTP servers) that do not support chunked messages.
    /// </summary>
    public static async Task<HttpResponseMessage> PostAsJsonUnchunked<TModel>(
        this HttpClient client, 
        [StringSyntax(StringSyntaxAttribute.Uri)] string? requestUri, 
        TModel model, 
        JsonSerializerOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        // Serialise model
        var json = JsonSerializer.Serialize(model, options);

        // Convert to body content (this will correctly set content-length header)
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Construct request
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = content
        };

        // Return task that will post request when awaited
        return await client.SendAsync(request, cancellationToken);
    }
}
