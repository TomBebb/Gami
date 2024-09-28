using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Gami.Core;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class PluginJson
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    private static string GetJsonPath(string addon) => Path.Join(Consts.BasePluginDir, addon + ".json");
    public static ValueTask<T?> LoadAsync<T>(string addon) where T : class
    {
        var path = GetJsonPath(addon);
        if (!File.Exists(path))
            return ValueTask.FromResult<T?>(null);
        var content = File.Open(path, FileMode.Open);
        return JsonSerializer.DeserializeAsync<T>(content, JsonSerializerOptions);
    }

    public static async ValueTask<T> LoadOrErrorAsync<T>(string addon) where T : class
    {
        var result = await LoadAsync<T>(addon);
        return result == null ? throw new ApplicationException($"Plugin {addon} needs JSON at {GetJsonPath(addon)}") : result;
    }

    public static T? Load<T>(string addon) where T : class
    {
        var path = GetJsonPath(addon);
        if (!File.Exists(path))
            return null;
        var content = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(content, JsonSerializerOptions);
    }

    public static async ValueTask Save<T>(T data, string addon) where T : class
    {
        var path = GetJsonPath(addon);
        var text = JsonSerializer.Serialize(data, JsonSerializerOptions);

        await File.WriteAllTextAsync(path, text);
    }
}