namespace ChatAppWpf.Services;

public sealed record AuthSession(
  string AccessToken,
  string RefreshToken,
  int ExpiresInSeconds,
  string UserId,
  string Email
);
