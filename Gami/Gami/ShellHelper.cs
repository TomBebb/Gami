using System.Diagnostics;
using System.Threading.Tasks;
using Instances;

namespace Gami;

public static class ShellHelper
{
    public static async ValueTask RunAsync(this ProcessStartInfo info)
    {
        await Instance.FinishAsync(info);
    }
}