﻿using Terraria.DataStructures;
using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class ConsecratedWater : ModItem
    {
        public const int BaseDamage = 48;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Consecrated Water");
            Tooltip.SetDefault("The bottle is surprisingly dusty\n" +
                               "Throws a holy flask of water that explodes into a sacred flame pillar on death\n" +
                               "The pillar is destroyed if there's no tiles below it\n" +
                               "Stealth strikes create three flame pillars instead of one on impact");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = BaseDamage;
            Item.width = 22;
            Item.height = 24;
            Item.useAnimation = 29;
            Item.useTime = 29;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4.5f;
            Item.value = CalamityGlobalItem.Rarity5BuyPrice;
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item106;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<ConsecratedWaterProjectile>();
            Item.shootSpeed = 15f;
            Item.Calamity().rogue = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            float strikeValue = player.Calamity().StealthStrikeAvailable().ToInt(); //0 if false, 1 if true
            int p = Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<ConsecratedWaterProjectile>(), damage, knockback, player.whoAmI, ai1: strikeValue);
            if (player.Calamity().StealthStrikeAvailable() && p.WithinBounds(Main.maxProjectiles))
                Main.projectile[p].Calamity().stealthStrike = true;
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.HolyWater, 100).
                AddRecipeGroup("AnyAdamantiteBar", 5).
                AddIngredient(ItemID.CrystalShard, 10).
                AddIngredient(ItemID.SoulofLight, 7).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
