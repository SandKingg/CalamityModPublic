using Terraria.DataStructures;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class AlphaVirus : RogueWeapon
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Alpha Virus");
            Tooltip.SetDefault("Throws a giant plague cell with a lethal aura that splits into 6 plague seekers on death\n" +
                               "Stealth strikes cause the plague cell to move slower, accumulating an aura of swirling plague seekers as it flies");
        }

        public override void SafeSetDefaults()
        {
            Item.damage = 333;
            Item.width = 44;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useAnimation = 31;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 31;
            Item.knockBack = 1f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.height = 44;
            Item.maxStack = 1;
            Item.value = CalamityGlobalItem.Rarity12BuyPrice;
            Item.shoot = ModContent.ProjectileType<AlphaVirusProjectile>();
            Item.shootSpeed = 4f;
            Item.rare = ItemRarityID.Purple;
            Item.Calamity().customRarity = CalamityRarity.Turquoise;
            Item.Calamity().rogue = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.Calamity().StealthStrikeAvailable())
            {
                int p = Projectile.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, type, damage, knockback, player.whoAmI, Item.shootSpeed, 0f);
                if (p.WithinBounds(Main.maxProjectiles))
                    Main.projectile[p].Calamity().stealthStrike = true;
                return false;
            }
            return true;
        }


        public override void AddRecipes()
        {
            CreateRecipe(1).AddIngredient(ModContent.ItemType<EpidemicShredder>()).AddIngredient(ModContent.ItemType<UeliaceBar>(), 5).AddIngredient(ModContent.ItemType<BloodstoneCore>(), 5).AddTile(TileID.LunarCraftingStation).Register();
        }

    }
}
