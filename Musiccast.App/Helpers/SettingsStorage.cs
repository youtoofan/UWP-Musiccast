using System;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace Musiccast.App.Helpers
{
    /// <summary>
    /// Lightweight replacement for the old Windows Community Toolkit settings
    /// storage extensions, backed by ApplicationData.LocalSettings and System.Text.Json.
    /// </summary>
    public static class SettingsStorage
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        public static Task SaveAsync<T>(string key, T value)
        {
            var json = JsonSerializer.Serialize(value, Options);
            ApplicationData.Current.LocalSettings.Values[key] = json;
            return Task.CompletedTask;
        }

        public static Task<T?> ReadAsync<T>(string key)
        {
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var obj) && obj is string json)
            {
                try
                {
                    return Task.FromResult(JsonSerializer.Deserialize<T>(json, Options));
                }
                catch
                {
                    return Task.FromResult<T?>(default);
                }
            }

            return Task.FromResult<T?>(default);
        }
    }
}
