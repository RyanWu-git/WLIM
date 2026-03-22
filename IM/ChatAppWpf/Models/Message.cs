using System;

namespace ChatAppWpf.Models;

public sealed record Message(
  string Id,
  string SessionId,
  string SenderId,
  string Content,
  DateTimeOffset CreatedAt
);
