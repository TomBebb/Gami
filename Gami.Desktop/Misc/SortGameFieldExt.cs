using System;

namespace Gami.Desktop.MIsc;

public static class SortGameFieldExt
{
    public static string GetName(this SortGameField field) => field switch
    {
        SortGameField.Name => "Name",
        SortGameField.LibraryType => "Library Type",
        SortGameField.ReleaseDate => "Release Date",
        SortGameField.InstallStatus => "Install",
        SortGameField.LastPlayed => "Last Played",
        _ => throw new ArgumentOutOfRangeException(nameof(field), field, null)
    };
}