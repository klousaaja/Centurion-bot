namespace MainCore.Parsers
{
    public static class RaidReportParser
    {
        public static int GetAttackerLosses(HtmlDocument doc)
        {
            var attackerTable = doc.GetElementbyId("attacker");
            if (attackerTable is null) return 0;

            var deadRow = attackerTable
                .Descendants("tr")
                .FirstOrDefault(x => x.HasClass("dead"));
            if (deadRow is null) return 0;

            var unitCells = deadRow
                .Descendants("td")
                .Where(x => x.HasClass("unit"));

            var totalLosses = 0;
            foreach (var cell in unitCells)
            {
                var text = cell.InnerText.Trim();
                var value = text.ParseInt();
                if (value > 0)
                {
                    totalLosses += value;
                }
            }

            return totalLosses;
        }
    }
}
