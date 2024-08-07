namespace Gami.Scanner.Steam;

public record SteamConfig(
    /**
     * @
     * From https://steamcommunity.com/dev/apikey
     */
    string ApiKey = "",
    string SteamId = "");