﻿namespace Gami.Desktop.Models;

public record struct Wrapped<T>(T Data);

public record struct WrappedText(string Data);