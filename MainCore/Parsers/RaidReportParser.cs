namespace MainCore.Parsers
{
    public static class RaidReportParser
    {
        public static int GetAttackerSupplyLost(HtmlDocument doc)
        {
            var statsDiv = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("combatStatistics"));
            if (statsDiv is null) return 0;

            var rows = statsDiv.Descendants("tr");
            foreach (var row in rows)
            {
                var th = row.Descendants("th").FirstOrDefault();
                if (th is null) continue;
                if (!th.InnerText.Trim().Equals("Supply lost", StringComparison.OrdinalIgnoreCase)) continue;

                var attackerTd = row.Descendants("td").FirstOrDefault();
                if (attackerTd is null) return 0;

                var valueSpan = attackerTd
                    .Descendants("span")
                    .FirstOrDefault(x => x.HasClass("value"));
                if (valueSpan is null) return 0;

                var parsed = valueSpan.InnerText.Trim().ParseInt();
                return parsed < 0 ? 0 : parsed;
            }

            return 0;
        }
    }
}
