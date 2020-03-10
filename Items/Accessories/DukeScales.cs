using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class DukeScales : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Old Duke's Scales");
            Tooltip.SetDefault("While under the effects of a damaging debuff, you gain 10% increased damage and 5% crit\n" +
							"When below 75% life, you gain 3% increased damage and crit\n" +
							"When below 50% life, you gain an additional 5% increased damage and crit\n" +
							"Provides immunity to poisoned, venom, and sulphruic poisoning");
        }

        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 26;
            item.value = Item.buyPrice(0, 60, 0, 0);
            item.accessory = true;
            item.rare = 10;
            item.Calamity().customRarity = (CalamityRarity)12;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.dukeScales = true;
        }
    }
}
