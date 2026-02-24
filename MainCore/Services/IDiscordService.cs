namespace MainCore.Services
{
    public interface IDiscordService
    {
        Task SendAttackAlert(string villageName, int x, int y, string accountName, DateTime arrivalAt);

        string? GetWebhookUrl();

        void SetWebhookUrl(string url);

        bool IsConfigured { get; }
    }
}
