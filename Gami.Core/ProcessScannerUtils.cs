using System.Diagnostics;
using GlobExpressions;

namespace Gami.Core;

public static class ProcessScannerUtils
{
    public static Process? ResolveMatchingProcess(this string appDir)
    {
        var exes = Glob.Files(pattern: "**/*.exe", workingDirectory: appDir);
        return exes.SelectMany(exe => Process.GetProcessesByName(exe.Replace(".exe", "")))
            .FirstOrDefault();
    }
}