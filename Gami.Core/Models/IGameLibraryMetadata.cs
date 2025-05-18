using System.Diagnostics.CodeAnalysis;

namespace Gami.Core.Models;

[SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
public interface IGameLibraryMetadata : IGameLibraryRef, IGameImages
{
    public DateTime? LastPlayed { get; set; }
    public GameInstallStatus InstallStatus { get; set; }
    public TimeSpan Playtime { get; set; }
}