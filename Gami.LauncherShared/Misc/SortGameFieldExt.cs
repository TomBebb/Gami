using DynamicData.Binding;
using Gami.Core.Models;

namespace Gami.LauncherShared.Misc;

public static class SortGameFieldExt
{
    public static IQueryable<Game> Sort(this IQueryable<Game> games, SortGameField sort, SortDirection dir)
    {
        return sort switch
        {
            SortGameField.Name => games.Sort(v => v.Name, dir),
            SortGameField.LibraryType => games.Sort(v => v.LibraryType, dir),
            SortGameField.ReleaseDate => games.Sort(v => v.ReleaseDate, dir),
            SortGameField.InstallStatus => games.Sort(v => v.InstallStatus, dir),
            SortGameField.LastPlayed => games.Sort(v => v.LastPlayed, SortDirection.Descending),
            _ => games
        };
    }

    public static string GetName(this SortGameField field)
    {
        return field switch
        {
            SortGameField.Name => "Name",
            SortGameField.LibraryType => "Library Type",
            SortGameField.ReleaseDate => "Release Date",
            SortGameField.InstallStatus => "Install",
            SortGameField.LastPlayed => "Last Played",
            SortGameField.PlayTime => "Play time",
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, null)
        };
    }
}