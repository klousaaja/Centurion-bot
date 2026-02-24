namespace MainCore.Parsers
{
    public static class IncomingAttackParser
    {
        public static List<IncomingAttackInfo> GetIncomingAttacks(HtmlDocument doc)
        {
            var attacks = new List<IncomingAttackInfo>();

            var movementsTable = doc.GetElementbyId("movements");
            if (movementsTable is null) return attacks;

            var rows = movementsTable
                .Descendants("tr")
                .ToList();

            foreach (var row in rows)
            {
                var attackSpan = row
                    .Descendants("span")
                    .FirstOrDefault(x => x.HasClass("a1"));

                if (attackSpan is null) continue;

                var timerSpan = row
                    .Descendants("span")
                    .FirstOrDefault(x => x.HasClass("timer"));

                var arrivalDuration = TimeSpan.Zero;
                if (timerSpan is not null)
                {
                    var timerText = timerSpan.InnerText.Trim();
                    if (!string.IsNullOrEmpty(timerText))
                    {
                        arrivalDuration = timerText.ToDuration();
                    }
                }

                attacks.Add(new IncomingAttackInfo
                {
                    ArrivalIn = arrivalDuration,
                    ArrivalAt = DateTime.Now.Add(arrivalDuration),
                });
            }

            return attacks;
        }

        public static bool HasIncomingAttack(HtmlDocument doc)
        {
            var movementsTable = doc.GetElementbyId("movements");
            if (movementsTable is null) return false;

            return movementsTable
                .Descendants("span")
                .Any(x => x.HasClass("a1"));
        }
    }

    public class IncomingAttackInfo
    {
        public TimeSpan ArrivalIn { get; set; }
        public DateTime ArrivalAt { get; set; }
    }
}
