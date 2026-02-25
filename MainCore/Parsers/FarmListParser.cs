namespace MainCore.Parsers
{
    public static class FarmListParser
    {
        public static IEnumerable<HtmlNode> GetFarmNodes(HtmlDocument doc)
        {
            var farmListTable = doc.GetElementbyId("rallyPointFarmList");
            if (farmListTable is null) return [];

            var farmlistNodes = farmListTable
                .Descendants("div")
                .Where(x => x.HasClass("farmListHeader"));
            return farmlistNodes;
        }

        public static FarmId GetId(HtmlNode node)
        {
            var farmlistDiv = node
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("dragAndDrop"));

            if (farmlistDiv is null) return default;

            var id = farmlistDiv.GetAttributeValue("data-list", "0");
            return new FarmId(id.ParseInt());
        }

        public static string GetName(HtmlNode node)
        {
            var farmlistName = node
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("name"));
            if (farmlistName is null) return "";
            return farmlistName.InnerText.Trim();
        }

        public static HtmlNode? GetStartButton(HtmlDocument doc, FarmId raidId)
        {
            var nodes = GetFarmNodes(doc);
            foreach (var node in nodes)
            {
                var id = GetId(node);
                if (id != raidId) continue;

                var startNode = node
                    .Descendants("button")
                    .FirstOrDefault(x => x.HasClass("startFarmList"));
                if (startNode is null) continue;
                return startNode;
            }
            return null;
        }

        public static HtmlNode? GetStartAllButton(HtmlDocument doc)
        {
            var farmlistTable = doc.GetElementbyId("rallyPointFarmList");
            if (farmlistTable is null) return null;
            var startAllFarmListButton = farmlistTable
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("startAllFarmLists"));

            return startAllFarmListButton;
        }

        public static IEnumerable<HtmlNode> GetAllSlotNodes(HtmlDocument doc)
        {
            var farmListTable = doc.GetElementbyId("rallyPointFarmList");
            if (farmListTable is null) return [];

            return farmListTable
                .Descendants("tr")
                .Where(x => x.HasClass("slot"));
        }

        public static bool IsSlotEnabled(HtmlNode slotRow)
        {
            return !slotRow.HasClass("disabled");
        }

        public static int GetSlotId(HtmlNode slotRow)
        {
            var input = slotRow
                .Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "") == "selectOne");
            if (input is null) return 0;
            return input.GetAttributeValue("data-slot-id", "0").ParseInt();
        }

        public static int GetSlotFarmListId(HtmlNode slotRow)
        {
            var input = slotRow
                .Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "") == "selectOne");
            if (input is null) return 0;
            return input.GetAttributeValue("data-farm-list-id", "0").ParseInt();
        }

        public static RaidStateEnums GetLastRaidState(HtmlNode slotRow)
        {
            var icon = slotRow
                .Descendants("i")
                .FirstOrDefault(x => x.HasClass("lastRaidState"));
            if (icon is null) return RaidStateEnums.Unknown;

            if (icon.HasClass("attack_won_withoutLosses_small")) return RaidStateEnums.Green;
            if (icon.HasClass("attack_won_withLosses_small")) return RaidStateEnums.Orange;
            if (icon.HasClass("attack_lost_small")) return RaidStateEnums.Red;

            return RaidStateEnums.Unknown;
        }

        public static string? GetReportUrl(HtmlNode slotRow)
        {
            var link = slotRow
                .Descendants("a")
                .FirstOrDefault(x => x.HasClass("lastRaidReport"));
            return link?.GetAttributeValue("href", null);
        }

        public static HtmlNode? GetSlotCheckbox(HtmlNode slotRow)
        {
            return slotRow
                .Descendants("input")
                .FirstOrDefault(x => x.GetAttributeValue("name", "") == "selectOne");
        }

        public static string GetTargetName(HtmlNode slotRow)
        {
            var targetCell = slotRow
                .Descendants("td")
                .FirstOrDefault(x => x.HasClass("target"));
            if (targetCell is null) return "";

            var link = targetCell.Descendants("a").FirstOrDefault();
            if (link is null) return "";
            return link.InnerText.Trim();
        }
    }
}