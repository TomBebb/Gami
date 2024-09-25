using System;
using System.Collections.Frozen;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Gami.Core;
using Gami.Core.Models;
using Gami.Library.Gog;
using Gami.Scanner.Epic;
using Gami.Scanner.Steam;
using Serilog;

namespace Gami.Desktop.Plugins;

public static class GameExtensions
{
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
            if (typeof(TInterface).IsAssignableFrom(type))
            {
                Log.Debug("GetMatching: {Type} assignable from {Interface}", type.Name,
                    typeof(TInterface).Name);
                var result = Activator.CreateInstance(type)!;
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

        Log.Error(
            "Can't find any type which implements   {InterfaceName} in {Assembly} from {AssemblyLocation}.\nAvailable types: {AvailableTypes}",
            typeof(TInterface).Name, assembly.FullName, assembly.Location, availableTypes);
        return null;
    }

    private static readonly string[] Plugins =
    [
        @"C:\Users\topha\Code\Gami\Gami.Scanner.Steam\bin\Debug\net8.0\Gami.Scanner.Steam.dll",
    ];

    private static readonly JsonSerializerOptions PluginOpts = new(JsonSerializerDefaults.Web)
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

        Log.Debug("Parse test: {Parse}", JsonSerializer.Deserialize<PluginSettingType>("\"string\"", PluginOpts));
/*
        var configTextStream = new MemoryStream();
        configStream.CopyTo(configTextStream);

        var configText = Encoding.UTF8.GetString(configTextStream.GetBuffer());
        if (configText[0] == '?')
            configText = configText.Substring(1);

        Log.Debug("Config.json: {Content}; First: {First}", configText, configText[0]);
*/
        return JsonSerializer.Deserialize<PluginConfig>(configStream, PluginOpts)!;
    }

    private static FrozenDictionary<string, T> PluginsByType<T>() where T : class, IBasePlugin => Plugins.Select(p =>
        {
            var assembly = LoadPlugin(p);
            return GetMatching<T>(assembly);
        })
        .Where(v => v != null)
        .ToFrozenDictionary(v => v!.Type)!;


    public static readonly FrozenDictionary<string, IGameLibraryManagement>
        InstallersByName = PluginsByType<IGameLibraryManagement>();

    public static readonly FrozenDictionary<string, IGameLibraryLauncher>
        LaunchersByName = PluginsByType<IGameLibraryLauncher>();

    public static readonly FrozenDictionary<string, IGameLibraryScanner>
        ScannersByName = PluginsByType<IGameLibraryScanner>();

    public static readonly FrozenDictionary<string, IGameAchievementScanner>
        AchievementsByName = PluginsByType<IGameAchievementScanner>();

    public static readonly FrozenDictionary<string, IGameIconLookup>
        IconLookupByName = PluginsByType<IGameIconLookup>();

    public static readonly FrozenDictionary<string, IGameMetadataScanner>
        MetadataScannersByName = PluginsByType<IGameMetadataScanner>();

    public static readonly FrozenDictionary<string, PluginConfig>
        // ReSharper disable once UnusedMember.Global
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
        LaunchersByName.GetLauncher(game.LibraryType).Launch(game);
    }

    public static ValueTask Install(this Game game)
        =>
            InstallersByName.GetLauncher(game.LibraryType).Install(game);


    public static void Uninstall(this Game game)
    {
        InstallersByName.GetLauncher(game.LibraryType).Uninstall(game);
    }
}