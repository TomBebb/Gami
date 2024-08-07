namespace Gami.Core;

public static class Consts
{
    public static readonly string AppDir =
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "gami");

    public static readonly string BasePluginDir =
        Path.Join(AppDir,
            "plugins");
}