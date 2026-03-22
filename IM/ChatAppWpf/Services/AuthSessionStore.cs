namespace ChatAppWpf.Services;

public sealed class AuthSessionStore
{
  public AuthSession? Session { get; private set; }

  public bool IsAuthenticated => Session is not null && !string.IsNullOrWhiteSpace(Session.AccessToken);

  public void Set(AuthSession session)
  {
    Session = session;
  }

  public void Clear()
  {
    Session = null;
  }
}
