﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CalamityMod.Items.Materials
{
    public class Phantoplasm : ModItem
    {
        public override void SetStaticDefaults()
        {
            SacrificeTotal = 100;
            DisplayName.SetDefault("Phantoplasm");
            Tooltip.SetDefault("It churns and seethes with ghastly malice");
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 5));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.maxStack = 999;
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Purple;
        }

        public override Color? GetAlpha(Color lightColor) => new Color(200, 200, 200, 0);
    }
}
