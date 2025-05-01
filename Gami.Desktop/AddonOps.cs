using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Serilog;

namespace Gami.Desktop;
public static class AddonOps
{
    private class JsConsole
    {
        // ReSharper disable once UnusedMember.Local
        public void Log(params JsValue[] args)
        {
            Console.WriteLine(string.Join(' ', args.Select(a => a.ToString())));
        }
        // ReSharper disable once UnusedMember.Local
        public void Debug(params JsValue[] args)
        {
            Serilog.Log.Debug(string.Join(' ', args.Select(a => a.ToString())));
        }
        // ReSharper disable once UnusedMember.Local
        public void Error(params JsValue[] args)
        {
            Serilog.Log.Error(string.Join(' ', args.Select(a => a.ToString())));
        }
    }
    public static Engine SetupEngine()
    {
        var engine = new Engine();
        engine.SetValue("console", new JsConsole());
        var client = new HttpClient();
        Log.Debug("IP {:?}", client.GetAsync("https://api.ipify.org?format=json").GetAwaiter().GetResult());
        engine.SetValue("fetch", async (JsValue  urlOrRequest) =>
        {
            Log.Information("Fetchign");
            Log.Information("Fetching {urlOrRequest}", urlOrRequest);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            var res = await client.GetAsync("https://store.steampowered.com/api/appdetails?appids=1121560").ConfigureAwait(false);
            Log.Information("OK");
            Log.Debug("Res: {:?}", res);
            return res.StatusCode;
        } );
        return engine;
    }
    public static async ValueTask RunScriptDemo()
    {
        var engine = SetupEngine();
        engine.Execute("console.error('error')");
        engine.Execute("console.debug('debug')");
        engine.Evaluate("fetch('https://api.ipify.org?format=json').then(console.log).catch(console.error)");
    }
    
}