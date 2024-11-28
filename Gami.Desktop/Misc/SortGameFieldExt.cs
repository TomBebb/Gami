using System;

namespace Gami.Desktop.Misc;

public static class SortGameFieldExt
{
    public static string GetName(this SortGameField field)
    {
        return field switch
        {
            SortGameField.Name => "Name",
            SortGameField.LibraryType => "Library Type",
            SortGameField.ReleaseDate => "Release Date",
            SortGameField.InstallStatus => "Install",
            SortGameField.LastPlayed => "Last Played",
            SortGameField.PlayTime => "Total Play Time",
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, null)
        };
    }
}