using System.Diagnostics;
using Gami.Core.Models;
using Instances;

namespace Gami.Core.Ext;

public static class ProcessTools
{
    private static ProcessStartInfo GetStartInfo(this string exe, IGameLibraryRef gameRef)
    {
        if (OperatingSystem.IsWindows()) return new ProcessStartInfo(exe);
        if (OperatingSystem.IsLinux())
        {
            var prefix = gameRef.ResolveProtonPrefix();


            Directory.CreateDirectory(prefix);
            var protonPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".local/share/Steam/steamapps/common/Proton 3.7/proton");
            return new ProcessStartInfo()
            {
                FileName = protonPath,
                Arguments = exe,
                Environment = { { "WINE_PREFIX", prefix } }
            };
        }

        throw new ApplicationException("Unsupported OS.");
    }

    public static async ValueTask<IProcessResult> AutoRunExeAsync(this string exe, IGameLibraryRef gameRef)
    {
        var args = GetStartInfo(exe, gameRef);
        return await Instance.FinishAsync(args);
    }

    public static IProcessResult AutoRun(this string exe, IGameLibraryRef gameRef)
    {
        var args = GetStartInfo(exe, gameRef);
        return Instance.Finish(args);
    }
}