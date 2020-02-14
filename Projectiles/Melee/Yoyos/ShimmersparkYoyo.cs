﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee.Yoyos
{
    public class ShimmersparkYoyo : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shimmerspark");
            ProjectileID.Sets.YoyosLifeTimeMultiplier[projectile.type] = 16f;
            ProjectileID.Sets.YoyosMaximumRange[projectile.type] = 275f;
            ProjectileID.Sets.YoyosTopSpeed[projectile.type] = 17f;
        }

        public override void SetDefaults()
        {
            projectile.aiStyle = 99;
            projectile.width = 16;
            projectile.height = 16;
            projectile.scale = 1f;
            projectile.friendly = true;
            projectile.melee = true;
            projectile.penetrate = -1;
        }

        public override void AI()
        {
			int[] array = new int[20];
			int num428 = 0;
			float distance = 300f;
			bool flag14 = false;
			for (int num430 = 0; num430 < 200; num430++)
			{
				if (Main.npc[num430].CanBeChasedBy(projectile, false))
				{
					float extraDistance = (float)(Main.npc[num430].width / 2) + (float)(Main.npc[num430].height / 2);

					bool useCollisionDetection = extraDistance < distance;
					bool canHit = true;
					if (useCollisionDetection)
						canHit = Collision.CanHit(projectile.Center, 1, 1, Main.npc[num430].Center, 1, 1);

					if (Vector2.Distance(Main.npc[num430].Center, projectile.Center) < (distance + extraDistance) && canHit)
					{
						if (num428 < 20)
						{
							array[num428] = num430;
							num428++;
						}
						flag14 = true;
					}
				}
			}

			if (flag14)
            {
                int num434 = Main.rand.Next(num428);
                num434 = array[num434];
                float num435 = Main.npc[num434].position.X + Main.npc[num434].width / 2;
                float num436 = Main.npc[num434].position.Y + Main.npc[num434].height / 2;
                projectile.localAI[0] += 1f;
                if (projectile.localAI[0] > 8f)
                {
                    projectile.localAI[0] = 0f;
                    float num437 = 6f;
                    Vector2 value10 = new Vector2(projectile.position.X + projectile.width * 0.5f, projectile.position.Y + projectile.height * 0.5f);
                    value10 += projectile.velocity * 4f;
                    float num438 = num435 - value10.X;
                    float num439 = num436 - value10.Y;
                    float num440 = (float)Math.Sqrt(num438 * num438 + num439 * num439);
                    num440 = num437 / num440;
                    num438 *= num440;
                    num439 *= num440;
                    if (Main.rand.NextBool(5))
                    {
                        if (projectile.owner == Main.myPlayer)
                        {
                            int proj = Projectile.NewProjectile(value10.X, value10.Y, num438, num439, 92, projectile.damage, projectile.knockBack, projectile.owner, 0f, 0f);
                            Main.projectile[proj].Calamity().forceMelee = true;
                        }
                    }
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = Main.projectileTexture[projectile.type];
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, projectile.GetAlpha(lightColor), projectile.rotation, tex.Size() / 2f, projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.Frostburn, 120);
        }
    }
}
