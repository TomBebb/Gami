namespace Gami.Core;

public static class Consts
{
    public static readonly string AppDir =
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "gami");
    public static readonly string ConfDir =
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "gami");

    public static readonly string ImagesDir =
        Path.Join(AppDir,
            "images");

    public static readonly string ProtonDir =
        Path.Join(AppDir,
            "proton");

    public static readonly string ProtonPrefixDir =
        Path.Join(AppDir,
            "prefix");

    public static readonly string ProtonDlDir =
        Path.Join(ProtonDir,
            "dl");

    public static readonly string BasePluginDir =
        Path.Join(AppDir,
            "plugins");
}