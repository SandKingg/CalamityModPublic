using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Ranged
{
    public class OrangeExoArrow : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Arrow");
        }

        public override void SetDefaults()
        {
            projectile.width = 5;
            projectile.height = 5;
            projectile.friendly = true;
            projectile.alpha = 255;
            projectile.penetrate = 1;
            projectile.extraUpdates = 2;
            projectile.timeLeft = 300;
            projectile.ranged = true;
            projectile.arrow = true;
        }

        public override void AI()
        {
            if (projectile.alpha > 0)
            {
                projectile.alpha -= 25;
            }
            if (projectile.alpha < 0)
            {
                projectile.alpha = 0;
            }
            float num55 = 40f;
            float num56 = 1.5f;
            if (projectile.ai[1] == 0f)
            {
                projectile.localAI[0] += num56;
                if (projectile.localAI[0] > num55)
                {
                    projectile.localAI[0] = num55;
                }
            }
            else
            {
                projectile.localAI[0] -= num56;
                if (projectile.localAI[0] <= 0f)
                {
                    projectile.Kill();
                }
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(250, 100, 0, projectile.alpha);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Color color25 = Lighting.GetColor((int)((double)projectile.position.X + (double)projectile.width * 0.5) / 16, (int)(((double)projectile.position.Y + (double)projectile.height * 0.5) / 16.0));
            int num147 = 0;
            int num148 = 0;
            float num149 = (float)(Main.projectileTexture[projectile.type].Width - projectile.width) * 0.5f + (float)projectile.width * 0.5f;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (projectile.spriteDirection == -1)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }
            Rectangle value6 = new Rectangle((int)Main.screenPosition.X - 500, (int)Main.screenPosition.Y - 500, Main.screenWidth + 1000, Main.screenHeight + 1000);
            if (projectile.getRect().Intersects(value6))
            {
                Vector2 value7 = new Vector2(projectile.position.X - Main.screenPosition.X + num149 + (float)num148, projectile.position.Y - Main.screenPosition.Y + (float)(projectile.height / 2) + projectile.gfxOffY);
                float num162 = 40f;
                float scaleFactor = 1.5f;
                if (projectile.ai[1] == 1f)
                {
                    num162 = (float)(int)projectile.localAI[0];
                }
                for (int num163 = 1; num163 <= (int)projectile.localAI[0]; num163++)
                {
                    Vector2 value8 = Vector2.Normalize(projectile.velocity) * (float)num163 * scaleFactor;
                    Color color29 = projectile.GetAlpha(color25);
                    color29 *= (num162 - (float)num163) / num162;
                    color29.A = 0;
                    Main.spriteBatch.Draw(Main.projectileTexture[projectile.type], value7 - value8, null, color29, projectile.rotation, new Vector2(num149, (float)(projectile.height / 2 + num147)), projectile.scale, spriteEffects, 0f);
                }
            }
            return false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
			target.ExoDebuffs();
        }

        public override void OnHitPvp(Player target, int damage, bool crit)
        {
			target.ExoDebuffs();
        }

        public override void Kill(int timeLeft)
        {
            projectile.position = projectile.Center;
            projectile.width = projectile.height = 188;
            projectile.position.X = projectile.position.X - (float)(projectile.width / 2);
            projectile.position.Y = projectile.position.Y - (float)(projectile.height / 2);
            projectile.maxPenetrate = -1;
            projectile.penetrate = -1;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 20;
            projectile.Damage();
            Main.PlaySound(SoundID.Item14, projectile.position);
            for (int num193 = 0; num193 < 4; num193++)
            {
                Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 127, 0f, 0f, 50, default, 1f);
            }
            for (int num194 = 0; num194 < 40; num194++)
            {
                int num195 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 127, 0f, 0f, 0, default, 1.5f);
                Main.dust[num195].noGravity = true;
                Main.dust[num195].noLight = true;
                Main.dust[num195].velocity *= 3f;
                num195 = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, 127, 0f, 0f, 50, default, 1f);
                Main.dust[num195].velocity *= 2f;
                Main.dust[num195].noGravity = true;
                Main.dust[num195].noLight = true;
            }
        }
    }
}
