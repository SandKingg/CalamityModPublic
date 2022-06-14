﻿using System.Collections.Generic;
using System.Linq;
using CalamityMod.Balancing;
using CalamityMod.CalPlayer;
using CalamityMod.World;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class DraedonsHeart : ModItem
    {
        private const double ContactDamageReduction = 0.15D;

        // Duration of Nanomachines in frames.
        internal static readonly int NanomachinesDuration = 120;

        // Duration of time where Nanomachines won't accumulate after taking damage, in frames.
        internal static readonly int NanomachinePauseAfterDamage = 240;

        internal static readonly int NanomachinesHeal = 240;

        public override void SetStaticDefaults()
        {
            SacrificeTotal = 1;
            DisplayName.SetDefault("Draedon's Heart");
            Tooltip.SetDefault("15% reduced contact damage from enemies\n" +
                "Reduces defense damage taken by 50%\n" + "Replaces Adrenaline with the Nanomachines meter\n" +
                $"Unlike Adrenaline, you lose no Nanomachines when you take damage, but they stop accumulating for {NanomachinePauseAfterDamage / 60} seconds\n" +
                $"With full Nanomachines, press & to heal {NanomachinesHeal} health over {NanomachinesDuration / 60} seconds\n" +
                "While healing, you take &% less damage\n" +
                "'Nanomachines, son.'");
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 11));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 68;
            Item.accessory = true;
            Item.defense = 48;
            Item.rare = ItemRarityID.Red;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.Calamity().customRarity = CalamityRarity.Violet;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();

            // On the first frame of equipping Draedon's Heart, lose all adrenaline.
            // This occurs because you didn't have nanomachines LAST frame.
            if (!modPlayer.hadNanomachinesLastFrame)
                modPlayer.adrenaline = 0f;

            modPlayer.draedonsHeart = true;
            modPlayer.hadNanomachinesLastFrame = true;
            modPlayer.AdrenalineDuration = NanomachinesDuration;
            modPlayer.contactDamageReduction += ContactDamageReduction;
        }

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            // The 3rd tooltip line "Replaces Adrenaline..." is replaced on Normal or Expert
            TooltipLine nanomachineMeterLine = list.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "Tooltip2");
            if (nanomachineMeterLine != null && !CalamityWorld.revenge)
                nanomachineMeterLine.Text = "Adds the Nanomachines meter";

            // The 4th tooltip line "Unlike Adrenaline..." is replaced on Normal or Expert
            TooltipLine doesntStopOnDamageLine = list.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "Tooltip3");
            if (doesntStopOnDamageLine != null && !CalamityWorld.revenge)
                doesntStopOnDamageLine.Text = "Nanomachines accumulate over time while fighting bosses\n" +
                    $"Taking damage stops the accumulation for {NanomachinePauseAfterDamage / 60} seconds";

            // The 5th tooltip line "With full Nanomachines" has the & replaced with the hotkey.
            string adrenKey = CalamityKeybinds.AdrenalineHotKey.TooltipHotkeyString();
            TooltipLine hotkeyLine = list.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "Tooltip4");
            if (hotkeyLine != null)
            {
                string tooltipWithHotkey = hotkeyLine.Text.Replace("&", adrenKey);
                hotkeyLine.Text = tooltipWithHotkey;
            }

            // The 6th tooltip line "While healing..." has the & replaced with full adrenaline DR.
            string fullAdrenDRString = (100f * BalancingConstants.FullAdrenalineDR).ToString("n0");
            TooltipLine healingDRLine = list.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "Tooltip5");
            if (healingDRLine != null)
            {
                string tooltipWithDR = fullAdrenDRString.Replace("&", fullAdrenDRString);
                healingDRLine.Text = tooltipWithDR;
            }
        }
    }
}
