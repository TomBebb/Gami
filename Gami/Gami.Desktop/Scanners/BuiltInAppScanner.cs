using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Gami.Desktop.Db.Models;
using IniParser.Parser;

namespace Gami.Desktop.Scanners;

public static class BuiltInAppScanner
{
    public static ImmutableList<IGameLibraryRef> ScanApps()
    {
        IGameLibraryRef ParseDesktop(string iniDataString)
        {
            iniDataString = string.Join("\n", iniDataString.Split("\n").Where(line => !line.StartsWith("#")));
            var parser = new IniDataParser();
            var data = parser.Parse(iniDataString)["Desktop Entry"];
            return new GameLibraryRef
            {
                Name = data["Name"],
                LibraryId = Path.Join(data["Path"], data["Exec"]),
                LibraryType = "exec",
                
            };
        }

        if (!OperatingSystem.IsWindows())
        {
            var desktopPaths = new[]
            {
                "/usr/share/applications", Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".local/share/applications")
            }.Where(Path.Exists);
            return desktopPaths.SelectMany(Directory.EnumerateFiles)
                .Select(desktopFullPath => ParseDesktop(File.ReadAllText(desktopFullPath)))
                .ToImmutableList();
        }

        var path = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
        return Directory.EnumerateFiles(path, "*.lnk", SearchOption.AllDirectories)
            .Select(filePath =>
            {
                var parsed = Lnk.Lnk.LoadFile(filePath);
                return new GameLibraryRef
                {
                    Name = Path.GetFileNameWithoutExtension(filePath),
                    LibraryId = parsed.LocalPath
                };
            })
            .Aggregate(ImmutableList<IGameLibraryRef>.Empty, (current, filePath) => current.Add(filePath));
    }
}