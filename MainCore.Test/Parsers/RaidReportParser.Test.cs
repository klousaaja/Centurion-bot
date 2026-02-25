namespace MainCore.Test.Parsers
{
    public class RaidReportParser : BaseParser
    {
        private const string RaidReportPage = "Parsers/RaidReport/RaidReportPage.html";

        [Fact]
        public void GetAttackerLosses_ReturnsTotalLosses()
        {
            _html.Load(RaidReportPage);
            var losses = MainCore.Parsers.RaidReportParser.GetAttackerLosses(_html);
            losses.ShouldBe(5);
        }

        [Fact]
        public void GetAttackerLosses_NoAttackerTable_ReturnsZero()
        {
            var emptyHtml = new HtmlAgilityPack.HtmlDocument();
            emptyHtml.LoadHtml("<html><body></body></html>");
            var losses = MainCore.Parsers.RaidReportParser.GetAttackerLosses(emptyHtml);
            losses.ShouldBe(0);
        }

        [Fact]
        public void GetAttackerLosses_NoDeadRow_ReturnsZero()
        {
            var html = new HtmlAgilityPack.HtmlDocument();
            html.LoadHtml("<html><body><table id=\"attacker\"><tbody><tr><td class=\"unit\">5</td></tr></tbody></table></body></html>");
            var losses = MainCore.Parsers.RaidReportParser.GetAttackerLosses(html);
            losses.ShouldBe(0);
        }
    }
}
