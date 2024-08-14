namespace Gami.Core;

public static class Consts
{
    public static readonly string AppDir =
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "gami");

    public static readonly string ProtonDir =
        Path.Join(AppDir,
            "proton");

    public static readonly string ProtonDlDir =
        Path.Join(ProtonDir,
            "dl");

    public static readonly string BasePluginDir =
        Path.Join(AppDir,
            "plugins");
}