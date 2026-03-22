using System.IO;
using System.Text.Json;
using System;

namespace ChatAppWpf.Services;

public sealed class AppConfig
{
  public string SupabaseUrl { get; init; } = string.Empty;
  public string SupabaseAnonKey { get; init; } = string.Empty;

  public static AppConfig? TryLoadFromFile(string path)
  {
    if (!File.Exists(path))
    {
      return null;
    }

    var json = File.ReadAllText(path);
    return JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    });
  }

  public static AppConfig? TryLoadNearest(string fileName, int maxDepth = 6)
  {
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    for (var i = 0; i < maxDepth && dir is not null; i++)
    {
      var candidate = Path.Combine(dir.FullName, fileName);
      var cfg = TryLoadFromFile(candidate);
      if (cfg is not null)
      {
        return cfg;
      }
      dir = dir.Parent;
    }

    return null;
  }
}
