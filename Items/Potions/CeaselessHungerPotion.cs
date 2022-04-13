﻿using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace CalamityMod.Items.Potions
{
    public class CeaselessHungerPotion : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ceaseless Hunger Potion");
            Tooltip.SetDefault("Causes you to suck up all items in the world");
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.useTurn = true;
            Item.maxStack = 30;
            Item.rare = ItemRarityID.Purple;
            Item.Calamity().customRarity = CalamityRarity.PureGreen;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.UseSound = SoundID.Item3;
            Item.consumable = true;
            Item.buffType = ModContent.BuffType<CeaselessHunger>();
            Item.buffTime = CalamityUtils.SecondsToFrames(10f);
            Item.value = Item.buyPrice(0, 2, 0, 0);
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 20;
        }

        public override void AddRecipes()
        {
            CreateRecipe(4).
                AddIngredient(ItemID.BottledWater, 4).
                AddIngredient<DarkPlasma>().
                AddIngredient<GalacticaSingularity>().
                AddTile(TileID.AlchemyTable).
                Register();

            CreateRecipe(4).
                AddIngredient(ItemID.BottledWater, 4).
                AddIngredient<BloodOrb>(20).
                AddIngredient<DarkPlasma>().
                AddTile(TileID.AlchemyTable).
                Register();
        }
    }
}
