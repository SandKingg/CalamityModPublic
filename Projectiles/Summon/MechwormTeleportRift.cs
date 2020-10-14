using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class MechwormTeleportRift : ModProjectile
    {
		public ref float GeneralRotationalOffset => ref projectile.ai[0];
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rift");
			ProjectileID.Sets.NeedsUUID[projectile.type] = true;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 170;
			projectile.scale = 0.25f;
            projectile.friendly = true;
            projectile.ignoreWater = true;
            projectile.tileCollide = false;
			projectile.Opacity = 0f;
            projectile.penetrate = -1;
            projectile.timeLeft = 45;
			projectile.netImportant = true;
        }

        public override void AI()
        {
			if (projectile.ai[0] == 0f)
				projectile.ai[0] = 45f;

			GeneralRotationalOffset += MathHelper.Pi / 8f;
			projectile.Opacity = MathHelper.Clamp(GeneralRotationalOffset, 0f, 1f);

			if (projectile.Opacity == 1f && Main.rand.NextBool(15))
			{
				Dust dust = Dust.NewDustDirect(projectile.Center, 0, 0, 267, 0f, 0f, 100, new Color(150, 100, 255, 255), 1f);
				dust.velocity.X = 0f;
				dust.noGravity = true;
				dust.position = projectile.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(4f, 20f);
				dust.fadeIn = 1f;
				dust.scale = 0.5f;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			float currentFade = Utils.InverseLerp(0f, 5f, projectile.timeLeft, true) * Utils.InverseLerp(projectile.ai[0], projectile.ai[0] - 5f, projectile.timeLeft, true);
			currentFade *= (1f + 0.2f * (float)Math.Cos(Main.GlobalTime % 30f * MathHelper.Pi * 3f)) * 0.8f;

			Texture2D texture = Main.projectileTexture[projectile.type];
			Vector2 drawPos = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
			Color baseColor = new Color(150, 100, 255, 255) * projectile.Opacity;
			baseColor *= 0.5f;
			baseColor.A = 0;
			Color colorA = baseColor;
			Color colorB = baseColor * 0.5f;
			colorA *= currentFade;
			colorB *= currentFade;
			Vector2 origin = texture.Size() / 2f;
			Vector2 scale = new Vector2(3f, 7f) * projectile.Opacity * projectile.scale * currentFade;

			SpriteEffects spriteEffects = SpriteEffects.None;
			if (projectile.spriteDirection == -1)
				spriteEffects = SpriteEffects.FlipHorizontally;

			spriteBatch.Draw(texture, drawPos, null, colorA, MathHelper.PiOver2, origin, scale, spriteEffects, 0);
			spriteBatch.Draw(texture, drawPos, null, colorA, 0f, origin, scale, spriteEffects, 0);
			spriteBatch.Draw(texture, drawPos, null, colorB, MathHelper.PiOver2, origin, scale * 0.8f, spriteEffects, 0);
			spriteBatch.Draw(texture, drawPos, null, colorB, 0f, origin, scale * 0.8f, spriteEffects, 0);

			spriteBatch.Draw(texture, drawPos, null, colorA, MathHelper.PiOver2 + GeneralRotationalOffset * 0.25f, origin, scale, spriteEffects, 0);
			spriteBatch.Draw(texture, drawPos, null, colorA, GeneralRotationalOffset * 0.25f, origin, scale, spriteEffects, 0);
			spriteBatch.Draw(texture, drawPos, null, colorB, MathHelper.PiOver2 + GeneralRotationalOffset * 0.5f, origin, scale * 0.8f, spriteEffects, 0);
			spriteBatch.Draw(texture, drawPos, null, colorB, GeneralRotationalOffset * 0.5f, origin, scale * 0.8f, spriteEffects, 0);

			spriteBatch.Draw(texture, drawPos, null, colorA, MathHelper.PiOver4, origin, scale * 0.6f, spriteEffects, 0);
			spriteBatch.Draw(texture, drawPos, null, colorA, MathHelper.PiOver4 * 3f, origin, scale * 0.6f, spriteEffects, 0);
			spriteBatch.Draw(texture, drawPos, null, colorB, MathHelper.PiOver4, origin, scale * 0.4f, spriteEffects, 0);
			spriteBatch.Draw(texture, drawPos, null, colorB, MathHelper.PiOver4 * 3f, origin, scale * 0.4f, spriteEffects, 0);

			spriteBatch.Draw(texture, drawPos, null, colorA, MathHelper.PiOver4 + GeneralRotationalOffset * 0.75f, origin, scale * 0.6f, spriteEffects, 0);
			spriteBatch.Draw(texture, drawPos, null, colorA, MathHelper.PiOver4 * 3f + GeneralRotationalOffset * 0.75f, origin, scale * 0.6f, spriteEffects, 0);
			spriteBatch.Draw(texture, drawPos, null, colorB, MathHelper.PiOver4 + GeneralRotationalOffset, origin, scale * 0.4f, spriteEffects, 0);
			spriteBatch.Draw(texture, drawPos, null, colorB, MathHelper.PiOver4 * 3f + GeneralRotationalOffset, origin, scale * 0.4f, spriteEffects, 0);

			return false;
		}

		public override bool CanDamage() => false;
	}
}
