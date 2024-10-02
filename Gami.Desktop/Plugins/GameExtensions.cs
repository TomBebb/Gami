using System;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Gami.Core;
using Gami.Core.Models;
using Serilog;
using Tomlyn;
using Tomlyn.Model;

namespace Gami.Desktop.Plugins;

public static class GameExtensions
{
    private static readonly string[] Plugins = [];

    public static readonly JsonSerializerOptions PluginOpts = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };


    public static readonly FrozenDictionary<string, IGameLibraryManagement>
        InstallersByName = PluginsByType<IGameLibraryManagement>();

    public static readonly FrozenDictionary<string, IGameLibraryLauncher>
        LaunchersByName = PluginsByType<IGameLibraryLauncher>();

    public static readonly FrozenDictionary<string, IGameLibraryScanner>
        ScannersByName = PluginsByType<IGameLibraryScanner>();

    public static readonly FrozenDictionary<string, IGameAchievementScanner>
        AchievementsByName = PluginsByType<IGameAchievementScanner>();

    public static readonly FrozenDictionary<string, IGameMetadataScanner>
        MetadataScannersByName = PluginsByType<IGameMetadataScanner>();

    public static readonly FrozenDictionary<string, IGameLibraryAuth>
        LibraryAuthByName = PluginsByType<IGameLibraryAuth>();

    public static readonly FrozenDictionary<string, PluginConfig>
        // ReSharper disable once UnusedMember.Global
        PluginConfigs = Plugins.Select(p =>
            {
                var assembly = LoadPlugin(p);
                return (p, LoadPluginConfig(assembly));
            })
            .ToFrozenDictionary(v => v.p, v => v.Item2);


    private static Assembly LoadPlugin(string pluginLocation)
    {
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


    private static TInterface? GetMatching<TInterface>(Assembly assembly) where
        TInterface : class
    {
        foreach (var type in assembly.GetTypes())
        {
            Log.Debug("Check GetMatching: {Type} assignable from {Interface}; implemented: {Interfaces}", type.Name,
                typeof(TInterface).Name, type.GetInterfaces());

            if (type.GetInterfaces().Select(i => i.FullName).Contains(typeof(TInterface).FullName))
            {
                Log.Information("Found {Interface}: {Cl}", typeof(TInterface).Name, type.FullName);
                return (TInterface)Activator.CreateInstance(type);
            }

            if (typeof(TInterface).IsAssignableFrom(type))
            {
                Log.Debug("GetMatching: {Type} assignable from {Interface}", type.Name,
                    typeof(TInterface).Name);
                var result = Activator.CreateInstance(type)!;
                return (TInterface)result;
            }

            Log.Debug(
                "GetMatching: {Type} not assignable from {Interface}; {Interfaces}",
                type.Name,
                typeof(TInterface).Name, string.Join(", ", type.GetInterfaces()
                    .Select(i => i.Name)));
        }

        var availableTypes =
            string.Join(",", assembly.GetTypes().Select(t => t.FullName));

        Log.Error(
            "Can't find any type which implements   {InterfaceName} in {Assembly} from {AssemblyLocation}.\nAvailable types: {AvailableTypes}",
            typeof(TInterface).Name, assembly.FullName, assembly.Location, availableTypes);
        return null;
    }

    private static PluginConfig LoadPluginConfig(Assembly assembly)
    {
        var manifest = assembly.GetManifestResourceNames();
        var configPath = $"{assembly.FullName!.Split(",")[0]}.config.toml";
        Log.Debug("Manifest for {Assembly}; {Games}; Config={Conf}", assembly.FullName, manifest, configPath);
        var configStream = assembly.GetManifestResourceStream(configPath);

        if (configStream == null)
            throw new ApplicationException($"Missing '{configPath}' in {assembly.FullName!}");


        var text = new StreamReader(configStream).ReadToEnd();
        var model = Toml.ToModel(text);
        var settings = model.TryGetValue("settings", out var data) ? (TomlArray)data : new TomlArray();
        return new PluginConfig
        {
            Key = (string)model["key"],
            Name = (string)model["name"],
            Settings = settings.Cast<TomlTable>().Select(v => new PluginConfigSetting
            {
                Key = (string)v["key"],
                Name = (string)v["key"],
                Hint = (string?)v["hint"],
                Type = v.TryGetValue("type", out var type) && type is PluginConfigSettingType ty
                    ? ty
                    : PluginConfigSettingType.String
            }).ToImmutableArray()
            //            Settings = 
        };
        return JsonSerializer.Deserialize<PluginConfig>(configStream, PluginOpts)!;
    }

    private static FrozenDictionary<string, T> PluginsByType<T>() where T : class, IBasePlugin
    {
        return Plugins.Select(p =>
            {
                var assembly = LoadPlugin(p);
                var matching = GetMatching<T>(assembly);
                Log.Debug("Matching for {Type}: {Cl}", typeof(T).Name, matching.GetType().Name);
                return matching;
            })
            .Where(v => v != null)
            .ToFrozenDictionary(v => v!.Type)!;
    }

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
        LaunchersByName.GetLauncher(game.LibraryType).Launch(game);
    }

    public static ValueTask Install(this Game game)
    {
        return InstallersByName.GetLauncher(game.LibraryType).Install(game);
    }


    public static void Uninstall(this Game game)
    {
        InstallersByName.GetLauncher(game.LibraryType).Uninstall(game);
    }
}