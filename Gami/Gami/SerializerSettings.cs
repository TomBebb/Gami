using System.Text.Json;

namespace Gami;

public static class SerializerSettings
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
}