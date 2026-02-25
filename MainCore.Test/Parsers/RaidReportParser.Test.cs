namespace MainCore.Test.Parsers
{
    public class RaidReportParser : BaseParser
    {
        private const string RaidReportPage = "Parsers/RaidReport/RaidReportPage.html";

        [Fact]
        public void GetAttackerSupplyLost_ReturnsCorrectValue()
        {
            _html.Load(RaidReportPage);
            var supplyLost = MainCore.Parsers.RaidReportParser.GetAttackerSupplyLost(_html);
            supplyLost.ShouldBe(5);
        }

        [Fact]
        public void GetAttackerSupplyLost_NoStatisticsDiv_ReturnsZero()
        {
            var emptyHtml = new HtmlAgilityPack.HtmlDocument();
            emptyHtml.LoadHtml("<html><body></body></html>");
            var supplyLost = MainCore.Parsers.RaidReportParser.GetAttackerSupplyLost(emptyHtml);
            supplyLost.ShouldBe(0);
        }

        [Fact]
        public void GetAttackerSupplyLost_NoSupplyLostRow_ReturnsZero()
        {
            var html = new HtmlAgilityPack.HtmlDocument();
            html.LoadHtml("<html><body><div class=\"combatStatistics\"><table class=\"combatStatistic\"><tbody><tr><th>Combat strength</th><td><span class=\"value\">100</span></td></tr></tbody></table></div></body></html>");
            var supplyLost = MainCore.Parsers.RaidReportParser.GetAttackerSupplyLost(html);
            supplyLost.ShouldBe(0);
        }
    }
}
