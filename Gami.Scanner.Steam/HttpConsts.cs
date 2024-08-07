namespace Gami.Scanner.Steam;

public class HttpConsts
{
    public static readonly HttpClient HttpClient = new(new HttpClientHandler()
    {
        MaxConnectionsPerServer = 8
    });
}