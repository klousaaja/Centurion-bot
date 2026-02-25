using MainCore.Enums;

namespace MainCore.Test.Parsers
{
    public class FarmListSlotParser : BaseParser
    {
        private const string FarmListPage = "Parsers/FarmList/FarmListPage.html";

        [Fact]
        public void GetAllSlotNodes_ReturnsSlots()
        {
            _html.Load(FarmListPage);
            var slots = MainCore.Parsers.FarmListParser.GetAllSlotNodes(_html).ToList();
            slots.ShouldNotBeEmpty();
            slots.Count.ShouldBe(9);
        }

        [Fact]
        public void GetSlotId_ReturnsValidId()
        {
            _html.Load(FarmListPage);
            var slots = MainCore.Parsers.FarmListParser.GetAllSlotNodes(_html).ToList();
            var slotId = MainCore.Parsers.FarmListParser.GetSlotId(slots.First());
            slotId.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void GetSlotFarmListId_ReturnsValidId()
        {
            _html.Load(FarmListPage);
            var slots = MainCore.Parsers.FarmListParser.GetAllSlotNodes(_html).ToList();
            var farmListId = MainCore.Parsers.FarmListParser.GetSlotFarmListId(slots.First());
            farmListId.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void IsSlotEnabled_DetectsDisabledSlots()
        {
            _html.Load(FarmListPage);
            var slots = MainCore.Parsers.FarmListParser.GetAllSlotNodes(_html).ToList();

            var enabledCount = slots.Count(s => MainCore.Parsers.FarmListParser.IsSlotEnabled(s));
            var disabledCount = slots.Count(s => !MainCore.Parsers.FarmListParser.IsSlotEnabled(s));

            enabledCount.ShouldBeGreaterThan(0);
            disabledCount.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void GetLastRaidState_DetectsGreenSword()
        {
            _html.Load(FarmListPage);
            var slots = MainCore.Parsers.FarmListParser.GetAllSlotNodes(_html).ToList();

            var greenSlots = slots.Where(s =>
                MainCore.Parsers.FarmListParser.GetLastRaidState(s) == RaidStateEnums.Green);

            greenSlots.Count().ShouldBe(8);
        }

        [Fact]
        public void GetLastRaidState_DetectsRedSword()
        {
            _html.Load(FarmListPage);
            var slots = MainCore.Parsers.FarmListParser.GetAllSlotNodes(_html).ToList();

            var redSlots = slots.Where(s =>
                MainCore.Parsers.FarmListParser.GetLastRaidState(s) == RaidStateEnums.Red);

            redSlots.Count().ShouldBe(1);
        }

        [Fact]
        public void GetReportUrl_ReturnsUrl()
        {
            _html.Load(FarmListPage);
            var slots = MainCore.Parsers.FarmListParser.GetAllSlotNodes(_html).ToList();
            var url = MainCore.Parsers.FarmListParser.GetReportUrl(slots.First());
            url.ShouldNotBeNullOrEmpty();
            url.ShouldContain("report?id=");
        }

        [Fact]
        public void GetTargetName_ReturnsName()
        {
            _html.Load(FarmListPage);
            var slots = MainCore.Parsers.FarmListParser.GetAllSlotNodes(_html).ToList();
            var name = MainCore.Parsers.FarmListParser.GetTargetName(slots.First());
            name.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public void GetSlotCheckbox_ReturnsNode()
        {
            _html.Load(FarmListPage);
            var slots = MainCore.Parsers.FarmListParser.GetAllSlotNodes(_html).ToList();
            var checkbox = MainCore.Parsers.FarmListParser.GetSlotCheckbox(slots.First());
            checkbox.ShouldNotBeNull();
        }
    }
}
