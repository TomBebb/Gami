using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Gami;

public static class ShellHelper
{
    public static async ValueTask<string> RunAsync(this ProcessStartInfo info)
    {
        var process = new Process
        {
            StartInfo = info
        };
        process.Start();
        var result = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        Console.WriteLine(result);
        return result;
    }
}