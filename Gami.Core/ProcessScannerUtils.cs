using System.Diagnostics;
using GlobExpressions;

namespace Gami.Core;

public static class ProcessScannerUtils
{
    public static Process? ResolveMatchingProcess(this string appDir)
    {
        
        var fullExePaths = Glob.Files(pattern: "**/*.exe", workingDirectory: appDir);
        var exeFileNames = fullExePaths.Select(Path.GetFileName);

        return exeFileNames.SelectMany(Process.GetProcessesByName)
            .FirstOrDefault();
    }
}