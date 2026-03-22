using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ChatAppWpf.Services;

public sealed class SupabaseAuthClient
{
  private readonly AppConfig _config;
  private readonly HttpClient _http;

  public SupabaseAuthClient(AppConfig config)
  {
    _config = config;
    _http = new HttpClient();
  }

  public async Task<AuthSession> SignInWithPasswordAsync(string email, string password, CancellationToken ct)
  {
    if (string.IsNullOrWhiteSpace(_config.SupabaseUrl) || string.IsNullOrWhiteSpace(_config.SupabaseAnonKey))
    {
      throw new InvalidOperationException("缺少 SupabaseUrl 或 SupabaseAnonKey，请在 appsettings.json 中配置。");
    }

    var endpoint = $"{_config.SupabaseUrl.TrimEnd('/')}/auth/v1/token?grant_type=password";
    using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
    req.Headers.Add("apikey", _config.SupabaseAnonKey);
    req.Content = new StringContent(JsonSerializer.Serialize(new
    {
      email,
      password
    }), Encoding.UTF8, "application/json");

    using var resp = await _http.SendAsync(req, ct);
    var json = await resp.Content.ReadAsStringAsync(ct);

    if (!resp.IsSuccessStatusCode)
    {
      var msg = TryExtractAuthError(json) ?? $"登录失败（HTTP {(int)resp.StatusCode}）";
      throw new InvalidOperationException(msg);
    }

    var token = JsonSerializer.Deserialize<AuthTokenResponse>(json, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    });

    if (token?.AccessToken is null || token.User?.Id is null || token.User?.Email is null)
    {
      throw new InvalidOperationException("登录响应解析失败。");
    }

    return new AuthSession(
      token.AccessToken,
      token.RefreshToken ?? string.Empty,
      token.ExpiresIn,
      token.User.Id,
      token.User.Email
    );
  }

  private static string? TryExtractAuthError(string json)
  {
    try
    {
      var err = JsonSerializer.Deserialize<AuthErrorResponse>(json, new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true
      });
      return err?.Msg ?? err?.ErrorDescription ?? err?.Error;
    }
    catch
    {
      return null;
    }
  }

  private sealed class AuthErrorResponse
  {
    public string? Error { get; set; }

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }

    public string? Msg { get; set; }
  }

  private sealed class AuthTokenResponse
  {
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    public AuthUser? User { get; set; }
  }

  private sealed class AuthUser
  {
    public string? Id { get; set; }
    public string? Email { get; set; }
  }
}
