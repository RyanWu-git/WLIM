using System;

namespace ChatAppWpf.Models;

public sealed record ChatSession(
  string Id,
  string Type,
  string? Name,
  DateTimeOffset? LastMessageAt
);
