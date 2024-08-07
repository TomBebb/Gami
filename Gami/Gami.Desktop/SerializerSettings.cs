using System.Text.Json;

namespace Gami.Desktop;

public static class SerializerSettings
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };
}