namespace MainCore.Commands.Features
{
    [Handler]
    public static partial class CheckIncomingAttackCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;

        private static async ValueTask HandleAsync(
            Command command,
            IChromeBrowser browser,
            AppDbContext context,
            IDiscordService discordService,
            ISettingService settingService
            )
        {
            var (accountId, villageId) = command;

            var isEnabled = settingService.BooleanByName(accountId, AccountSettingEnums.EnableAttackAlert);
            if (!isEnabled) return;

            if (!discordService.IsConfigured) return;

            if (!browser.CurrentUrl.Contains("dorf1")) return;

            var attacks = IncomingAttackParser.GetIncomingAttacks(browser.Html);
            if (attacks.Count == 0) return;

            var village = context.Villages.FirstOrDefault(v => v.Id == villageId.Value);
            var villageName = village?.Name ?? "Unknown";
            var x = village?.X ?? 0;
            var y = village?.Y ?? 0;

            var account = context.Accounts.FirstOrDefault(a => a.Id == accountId.Value);
            var accountName = account?.Username ?? "Unknown";

            foreach (var attack in attacks)
            {
                await discordService.SendAttackAlert(villageName, x, y, accountName, attack.ArrivalAt);
            }
        }
    }
}
