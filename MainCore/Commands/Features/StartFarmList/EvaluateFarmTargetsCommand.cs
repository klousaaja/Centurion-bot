namespace MainCore.Commands.Features.StartFarmList
{
    [Handler]
    public static partial class EvaluateFarmTargetsCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            ISettingService settingService,
            IDelayService delayService,
            ExpandFarmListsCommand.Handler expandFarmListsCommand,
            CancellationToken cancellationToken)
        {
            var accountId = command.AccountId;

            var isEnabled = settingService.BooleanByName(accountId, AccountSettingEnums.EnableFarmListProtection);
            if (!isEnabled) return Result.Ok();

            var threshold = settingService.ByName(accountId, AccountSettingEnums.FarmTroopLossThreshold);

            var farmListUrl = browser.CurrentUrl;

            var slotsToUncheck = new List<int>();

            var html = browser.Html;
            var allSlots = FarmListParser.GetAllSlotNodes(html).ToList();

            foreach (var slot in allSlots)
            {
                if (!FarmListParser.IsSlotEnabled(slot)) continue;

                var raidState = FarmListParser.GetLastRaidState(slot);
                var slotId = FarmListParser.GetSlotId(slot);
                var targetName = FarmListParser.GetTargetName(slot);

                if (raidState == RaidStateEnums.Red)
                {
                    browser.Logger.Information("[FarmProtection] Red sword on {Target} (slot {SlotId}) - will uncheck", targetName, slotId);
                    slotsToUncheck.Add(slotId);
                    continue;
                }

                if (raidState == RaidStateEnums.Orange)
                {
                    var reportUrl = FarmListParser.GetReportUrl(slot);
                    if (string.IsNullOrEmpty(reportUrl))
                    {
                        browser.Logger.Information("[FarmProtection] Orange sword on {Target} (slot {SlotId}) but no report link", targetName, slotId);
                        continue;
                    }

                    var navResult = await browser.Navigate(reportUrl, cancellationToken);
                    if (navResult.IsFailed)
                    {
                        browser.Logger.Information("[FarmProtection] Failed to navigate to report for {Target}", targetName);
                        continue;
                    }

                    await delayService.DelayClick(cancellationToken);

                    var reportHtml = browser.Html;
                    var losses = RaidReportParser.GetAttackerLosses(reportHtml);

                    browser.Logger.Information("[FarmProtection] Orange sword on {Target} (slot {SlotId}) - losses: {Losses}, threshold: {Threshold}", targetName, slotId, losses, threshold);

                    if (losses > threshold)
                    {
                        slotsToUncheck.Add(slotId);
                    }
                }
            }

            if (slotsToUncheck.Count == 0) return Result.Ok();

            var backResult = await browser.Navigate(farmListUrl, cancellationToken);
            if (backResult.IsFailed) return backResult;

            await delayService.DelayClick(cancellationToken);

            var expandResult = await expandFarmListsCommand.HandleAsync(new(), cancellationToken);
            if (expandResult.IsFailed) return expandResult;

            foreach (var slotId in slotsToUncheck)
            {
                var (_, isFailed, element, errors) = await browser.GetElement(
                    doc => FindCheckboxBySlotId(doc, slotId),
                    cancellationToken);

                if (isFailed)
                {
                    browser.Logger.Information("[FarmProtection] Could not find checkbox for slot {SlotId}", slotId);
                    continue;
                }

                var clickResult = await browser.Click(element, cancellationToken);
                if (clickResult.IsFailed)
                {
                    browser.Logger.Information("[FarmProtection] Failed to uncheck slot {SlotId}", slotId);
                    continue;
                }

                await delayService.DelayClick(cancellationToken);
            }

            browser.Logger.Information("[FarmProtection] Unchecked {Count} dangerous target(s)", slotsToUncheck.Count);
            return Result.Ok();
        }

        private static HtmlNode? FindCheckboxBySlotId(HtmlDocument doc, int slotId)
        {
            var allSlots = FarmListParser.GetAllSlotNodes(doc);
            foreach (var slot in allSlots)
            {
                var id = FarmListParser.GetSlotId(slot);
                if (id == slotId)
                {
                    return FarmListParser.GetSlotCheckbox(slot);
                }
            }
            return null;
        }
    }
}
