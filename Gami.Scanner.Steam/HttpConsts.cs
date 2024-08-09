using CacheCow.Client;

namespace Gami.Scanner.Steam;

public static class HttpConsts
{
    public static readonly HttpClient HttpClient = ClientExtensions.CreateClient();
}