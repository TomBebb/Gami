using System.Text.Json;

namespace Gami.Core;

public static class PluginJson
{
    public static ValueTask<T?> LoadAsync<T>(string addon) where T : class
    {
        var path = Path.Join(Consts.BasePluginDir, addon + ".json");
        if (!File.Exists(path))
            return ValueTask.FromResult<T?>(null);
        var content = File.Open(path, FileMode.Open);
        return JsonSerializer.DeserializeAsync<T>(content,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    public static T? Load<T>(string addon) where T : class
    {
        var path = Path.Join(Consts.BasePluginDir, addon + ".json");
        if (!File.Exists(path))
            return null;
        var content = File.Open(path, FileMode.Open);
        return JsonSerializer.Deserialize<T>(content,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    public static async ValueTask Save<T>(T data, string addon) where T : class
    {
        var path = Path.Join(Consts.BasePluginDir, addon + ".json");
        var text = JsonSerializer.Serialize<T>(data,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        await File.WriteAllTextAsync(path, text);
    }
}