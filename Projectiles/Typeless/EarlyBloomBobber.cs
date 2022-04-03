using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using CalamityMod.Items.Fishing.FishingRods;
namespace CalamityMod.Projectiles.Typeless
{
    public class EarlyBloomBobber : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Early Bloom Bobber");
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = 61;
            Projectile.bobber = true;
            Projectile.penetrate = -1;
        }

        public override bool PreDrawExtras(SpriteBatch Main.spriteBatch)
        {
            Lighting.AddLight(Projectile.Center, 0.1f, 0.5f, 0.15f);
            return Projectile.DrawFishingLine(ModContent.ItemType<EarlyBloomRod>(), new Color(190, 140, 69, 100));
        }
    }
}
