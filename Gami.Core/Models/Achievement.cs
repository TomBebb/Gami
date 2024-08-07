﻿namespace Gami.Core.Models;

public sealed class Achievement : NamedIdItem
{
    public int GameId { get; set; }
    public Game Game { get; set; } = null!;
    public string LibraryId { get; set; } = null!;
    public byte[] LockedIcon { get; set; } = null!;
    public byte[] UnlockedIcon { get; set; } = null!;

    public bool Unlocked { get; set; } = false;
    public DateTime UnlockTime { get; set; }
}