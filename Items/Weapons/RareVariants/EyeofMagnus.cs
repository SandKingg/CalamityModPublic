using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.RareVariants
{
    public class EyeofMagnus : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Eye of Magnus");
			Tooltip.SetDefault("Fires powerful beams\n" +
				"Heals mana and health on hit");
		}

		public override void SetDefaults()
		{
			item.width = 80;
			item.damage = 55;
			item.rare = 5;
			item.useAnimation = 20;
			item.useTime = 20;
			item.useStyle = 5;
			item.knockBack = 2f;
			item.UseSound = mod.GetLegacySoundSlot(SoundType.Item, "Sounds/Item/LaserCannon");
			item.magic = true;
			item.mana = 7;
			item.autoReuse = true;
			item.noMelee = true;
			item.height = 50;
			item.value = Item.buyPrice(0, 36, 0, 0);
			item.shoot = mod.ProjectileType("MagnusBeam");
			item.shootSpeed = 12f;
			item.GetGlobalItem<CalamityGlobalItem>(mod).postMoonLordRarity = 22;
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-15, 0);
		}
	}
}
