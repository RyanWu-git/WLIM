using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ChatAppWpf.Services;

public sealed class SupabaseRestClient
{
  private readonly AppConfig _config;
  private readonly HttpClient _http;
  private readonly AuthSessionStore _sessionStore;

  public SupabaseRestClient(AppConfig config, AuthSessionStore sessionStore)
  {
    _config = config;
    _sessionStore = sessionStore;
    _http = new HttpClient();
  }

  public async Task<IReadOnlyList<T>> GetAsync<T>(string relativePathWithQuery, CancellationToken ct)
  {
    EnsureReady();
    var url = $"{_config.SupabaseUrl.TrimEnd('/')}/rest/v1/{relativePathWithQuery.TrimStart('/')}";
    using var req = new HttpRequestMessage(HttpMethod.Get, url);
    AttachHeaders(req);
    using var resp = await _http.SendAsync(req, ct);
    var json = await resp.Content.ReadAsStringAsync(ct);
    if (!resp.IsSuccessStatusCode)
    {
      throw new InvalidOperationException($"请求失败（HTTP {(int)resp.StatusCode}）");
    }

    return JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    }) ?? [];
  }

  public async Task<T> PostAsync<T>(string relativePathWithQuery, object body, CancellationToken ct)
  {
    EnsureReady();
    var url = $"{_config.SupabaseUrl.TrimEnd('/')}/rest/v1/{relativePathWithQuery.TrimStart('/')}";
    using var req = new HttpRequestMessage(HttpMethod.Post, url);
    AttachHeaders(req);
    req.Headers.TryAddWithoutValidation("Prefer", "return=representation");
    req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

    using var resp = await _http.SendAsync(req, ct);
    var json = await resp.Content.ReadAsStringAsync(ct);
    if (!resp.IsSuccessStatusCode)
    {
      throw new InvalidOperationException($"请求失败（HTTP {(int)resp.StatusCode}）");
    }

    var list = JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    });

    if (list is null || list.Count == 0)
    {
      throw new InvalidOperationException("响应为空。");
    }

    return list[0];
  }

  private void EnsureReady()
  {
    if (string.IsNullOrWhiteSpace(_config.SupabaseUrl) || string.IsNullOrWhiteSpace(_config.SupabaseAnonKey))
    {
      throw new InvalidOperationException("缺少 SupabaseUrl 或 SupabaseAnonKey，请在 appsettings.json 中配置。");
    }

    if (!_sessionStore.IsAuthenticated)
    {
      throw new InvalidOperationException("尚未登录。");
    }
  }

  private void AttachHeaders(HttpRequestMessage req)
  {
    req.Headers.Add("apikey", _config.SupabaseAnonKey);
    req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _sessionStore.Session!.AccessToken);
  }
}
