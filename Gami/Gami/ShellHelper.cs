using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Gami;

public static class ShellHelper
{
    public static ProcessStartInfo WrapShell(this string cmd)
    {
        var escapedArgs = cmd.Replace("\"", "\\\"");

        return new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{escapedArgs}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
    }

    public static async ValueTask RunShellAsync(this string cmd)
    {
        var process = new Process
        {
            StartInfo = WrapShell(cmd)
        };
        process.Start();
        var result = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        Console.WriteLine(result);
    }
}