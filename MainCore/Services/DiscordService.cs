using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Serilog;

namespace MainCore.Services
{
    [RegisterSingleton<IDiscordService, DiscordService>]
    public sealed class DiscordService : IDiscordService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, DateTime> _recentAlerts = new();
        private static readonly TimeSpan AlertCooldown = TimeSpan.FromMinutes(5);
        private string? _webhookUrl;

        private static readonly string ConfigFilePath = Path.Combine(AppContext.BaseDirectory, "discord_config.json");

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_webhookUrl);

        public DiscordService(ILogger logger)
        {
            _logger = logger.ForContext<DiscordService>();
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Centurion-Bot");
            LoadConfig();
        }

        public string? GetWebhookUrl() => _webhookUrl;

        public void SetWebhookUrl(string url)
        {
            _webhookUrl = url;
            SaveConfig();
        }

        public async Task SendAttackAlert(string villageName, int x, int y, string accountName, DateTime arrivalAt)
        {
            if (!IsConfigured) return;

            var alertKey = $"{villageName}_{x}_{y}_{arrivalAt:HH:mm}";
            if (_recentAlerts.TryGetValue(alertKey, out var lastAlerted))
            {
                if (DateTime.Now - lastAlerted < AlertCooldown) return;
            }
            _recentAlerts[alertKey] = DateTime.Now;

            CleanupOldAlerts();

            var arrivalTimeStr = arrivalAt.ToString("HH:mm:ss");
            var arrivalIn = arrivalAt - DateTime.Now;
            var arrivalInStr = arrivalIn > TimeSpan.Zero
                ? $"{(int)arrivalIn.TotalHours:D2}:{arrivalIn.Minutes:D2}:{arrivalIn.Seconds:D2}"
                : "NOW";

            var payload = new
            {
                content = "@everyone",
                embeds = new[]
                {
                    new
                    {
                        title = "INCOMING ATTACK",
                        color = 15158332, // Red
                        fields = new object[]
                        {
                            new { name = "Account", value = accountName, inline = true },
                            new { name = "Village", value = $"{villageName} ({x}|{y})", inline = true },
                            new { name = "Arrives at", value = arrivalTimeStr, inline = true },
                            new { name = "Time remaining", value = arrivalInStr, inline = true },
                        },
                        footer = new { text = "Centurion Attack Alert" },
                        timestamp = DateTime.UtcNow.ToString("o"),
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);

            try
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_webhookUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.Warning("Discord webhook returned {StatusCode}", response.StatusCode);
                }
                else
                {
                    _logger.Information("Attack alert sent for {Village} ({X}|{Y})", villageName, x, y);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send Discord attack alert");
            }
        }

        private void CleanupOldAlerts()
        {
            var expiredKeys = _recentAlerts
                .Where(kvp => DateTime.Now - kvp.Value > TimeSpan.FromMinutes(30))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _recentAlerts.TryRemove(key, out _);
            }
        }

        private void LoadConfig()
        {
            try
            {
                if (!File.Exists(ConfigFilePath)) return;
                var json = File.ReadAllText(ConfigFilePath);
                var config = JsonSerializer.Deserialize<DiscordConfig>(json);
                _webhookUrl = config?.WebhookUrl;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load Discord config");
            }
        }

        private void SaveConfig()
        {
            try
            {
                var config = new DiscordConfig { WebhookUrl = _webhookUrl ?? "" };
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to save Discord config");
            }
        }

        private sealed class DiscordConfig
        {
            public string WebhookUrl { get; set; } = "";
        }
    }
}
