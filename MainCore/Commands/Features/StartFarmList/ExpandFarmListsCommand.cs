namespace MainCore.Commands.Features.StartFarmList
{
    [Handler]
    public static partial class ExpandFarmListsCommand
    {
        public sealed record Command : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            IDelayService delayService,
            CancellationToken cancellationToken)
        {
            var js = @"
                document.querySelectorAll('.farmListWrapper.collapsed .expandCollapse')
                    .forEach(el => el.click());
            ";
            var result = await browser.ExecuteJsScript(js);
            if (result.IsFailed) return result;

            await delayService.DelayClick(cancellationToken);
            return Result.Ok();
        }
    }
}
