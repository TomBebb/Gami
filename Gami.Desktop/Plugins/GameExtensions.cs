using System;
using System.Collections.Frozen;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Controls.Documents;
using Gami.Core;
using Gami.Core.Models;
using Serilog;

namespace Gami.Desktop.Plugins;

using Scanner.Steam;

public static class GameExtensions
{
    private static Assembly LoadPlugin(string pluginLocation)
    {
        return Assembly.GetAssembly(typeof(SteamCommon));
        foreach (var assemblyName in Assembly.GetExecutingAssembly()
                     .GetReferencedAssemblies())
            if (assemblyName.FullName.Contains("Gami") ||
                assemblyName.FullName.Contains("Steam"))
                Log.Information("Assembly {Name}", assemblyName.Name);

        Log.Debug("Loading plugin from: {Path}", pluginLocation);
        var loadContext = new PluginLoadContext(pluginLocation);
        return loadContext.LoadFromAssemblyName(
            new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
    }


    private static TInterface GetMatching<TInterface>(Assembly assembly) where
        TInterface : class
    {
        foreach (var type in assembly.GetTypes())
            if (typeof(TInterface).IsAssignableFrom(type))
            {
                Log.Debug("GetMatching: {Type} assignable from {Interface}", type.Name,
                    typeof(TInterface).Name);
                var result = Activator.CreateInstance(type);
                return (TInterface)result;
            }
            else
            {
                Log.Debug(
                    "GetMatching: {Type} not assignable from {Interface}; {Interfaces}",
                    type.Name,
                    typeof(TInterface).Name, string.Join(", ", type.GetInterfaces()
                        .Select(i => i.Name)));
            }


        var availableTypes =
            string.Join(",", assembly.GetTypes().Select(t => t.FullName));
        throw new ApplicationException(
            $"Can't find any type which implements   {typeof(TInterface).Name} in {assembly} from {assembly.Location}.\n" +
            $"Available types: {availableTypes}");
    }

    private static readonly string[] Plugins = new[]
    {
        "C:\\Users\\topha\\Code\\Gami\\Gami.Scanner.Steam\\bin\\Debug\\net8.0\\Gami.Scanner.Steam.dll"
    };

    private static JsonSerializerOptions _pluginOpts = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static PluginConfig LoadPluginConfig(Assembly assembly)
    {
        var manifest = assembly.GetManifestResourceNames();
        var configPath = $"{assembly.FullName!.Split(",")[0]}.config.json";
        Log.Debug("Manifest for {Assembly}; {Games}; Config={Conf}", assembly.FullName, manifest, configPath);
        var configStream = assembly.GetManifestResourceStream(configPath);

        if (configStream == null)
            throw new ApplicationException($"Missing '{configPath}' in {assembly.FullName!}");

        Log.Debug("Parse test: {Parse}", JsonSerializer.Deserialize<PluginSettingType>("\"string\"", _pluginOpts));
/*
        var configTextStream = new MemoryStream();
        configStream.CopyTo(configTextStream);

        var configText = Encoding.UTF8.GetString(configTextStream.GetBuffer());
        if (configText[0] == '?')
            configText = configText.Substring(1);

        Log.Debug("Config.json: {Content}; First: {First}", configText, configText[0]);
*/
        return JsonSerializer.Deserialize<PluginConfig>(configStream, _pluginOpts);
    }

    private static readonly FrozenDictionary<string, IGameLibraryManagement>
        InstallersByName = Plugins.Select(p =>
            {
                var assembly = LoadPlugin(p);
                return GetMatching<IGameLibraryManagement>(assembly);
            })
            .ToFrozenDictionary(v => v.Type);


    private static readonly FrozenDictionary<string, IGameLibraryLauncher>
        LaunchersByName = Plugins.Select(p =>
            {
                var assembly = LoadPlugin(p);
                return GetMatching<IGameLibraryLauncher>(assembly);
            })
            .ToFrozenDictionary(v => v.Type);

    public static readonly FrozenDictionary<string, IGameLibraryScanner>
        ScannersByName = Plugins.Select(p =>
            {
                var assembly = LoadPlugin(p);
                return GetMatching<IGameLibraryScanner>(assembly);
            })
            .ToFrozenDictionary(v => v.Type);

    public static readonly FrozenDictionary<string, IGameAchievementScanner>
        AchievementsByName = Plugins.Select(p =>
            {
                var assembly = LoadPlugin(p);
                return GetMatching<IGameAchievementScanner>(assembly);
            })
            .ToFrozenDictionary(v => v.Type);

    public static readonly FrozenDictionary<string, IGameIconLookup>
        IconLookupByName = Plugins.Select(p =>
            {
                var assembly = LoadPlugin(p);
                return GetMatching<IGameIconLookup>(assembly);
            })
            .ToFrozenDictionary(v => v.Type);

    public static readonly FrozenDictionary<string, PluginConfig>
        PluginConfigs = Plugins.Select(p =>
            {
                var assembly = LoadPlugin(p);
                return (p, LoadPluginConfig(assembly));
            })
            .ToFrozenDictionary(v => v.p, v => v.Item2);

    private static T GetLauncher<T>(this FrozenDictionary<string, T> dictionary,
        string libType)
    {
        if (!dictionary.TryGetValue(libType, out var launcher))
            throw new ApplicationException(
                $"No launcher found with library type '{libType}\", valid keys: {string.Join(", ", dictionary.Keys)}");
        return launcher;
    }

    public static void Launch(this Game game)
    {
        LaunchersByName.GetLauncher(game.LibraryType).Launch(game.LibraryId);
    }

    public static void Install(this Game game)
    {
        InstallersByName.GetLauncher(game.LibraryType).Install(game.LibraryId);
    }

    public static void Uninstall(this Game game)
    {
        InstallersByName.GetLauncher(game.LibraryType).Uninstall(game.LibraryId);
    }
}