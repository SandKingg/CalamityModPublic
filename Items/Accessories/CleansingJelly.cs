﻿using System;
using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    [LegacyName("ManaJelly")]
    public class CleansingJelly : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 40;
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.rare = ItemRarityID.Blue;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.cleansingjelly = true;
        }
    }
}
