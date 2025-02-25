﻿using System;
using CalamityMod.Items.Weapons.Melee;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class GaelExplosion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 45;
            Projectile.height = 45;
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 15;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = GaelsGreatsword.ImmunityFrames;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, (255 - Projectile.alpha) * 0f / 255f, (255 - Projectile.alpha) * 0.75f / 255f, (255 - Projectile.alpha) * 0.75f / 255f);
            if (Projectile.localAI[0] == 0f)
            {
                SoundEngine.PlaySound(SoundID.Item74, Projectile.Center);
                Projectile.localAI[0] += 1f;
            }
            bool xflag = false;
            bool yflag = false;
            if (Projectile.velocity.X < 0f && Projectile.position.X < Projectile.ai[0])
            {
                xflag = true;
            }
            if (Projectile.velocity.X > 0f && Projectile.position.X > Projectile.ai[0])
            {
                xflag = true;
            }
            if (Projectile.velocity.Y < 0f && Projectile.position.Y < Projectile.ai[1])
            {
                yflag = true;
            }
            if (Projectile.velocity.Y > 0f && Projectile.position.Y > Projectile.ai[1])
            {
                yflag = true;
            }
            if (xflag && yflag)
            {
                Projectile.Kill();
            }
            float projTimer = 25f;
            if (Projectile.ai[0] > 180f)
            {
                projTimer -= (Projectile.ai[0] - 180f) / 2f;
            }
            if (projTimer <= 0f)
            {
                projTimer = 0f;
                Projectile.Kill();
            }
            projTimer *= 0.7f;
            Projectile.ai[0] += 4f;
            int timerCounter = 0;
            while ((float)timerCounter < projTimer)
            {
                float rando1 = (float)Main.rand.Next(-30, 31);
                float rando2 = (float)Main.rand.Next(-30, 31);
                float rando3 = (float)Main.rand.Next(9, 27);
                float randoAdjuster = (float)Math.Sqrt((double)(rando1 * rando1 + rando2 * rando2));
                randoAdjuster = rando3 / randoAdjuster;
                rando1 *= randoAdjuster;
                rando2 *= randoAdjuster;
                int gaelDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Rain_BloodMoon, 0f, 0f, 100, new Color(0, 255, 255), 1.5f);
                Main.dust[gaelDust].noGravity = true;
                Main.dust[gaelDust].position.X = Projectile.Center.X;
                Main.dust[gaelDust].position.Y = Projectile.Center.Y;
                Dust dust = Main.dust[gaelDust];
                dust.position.X += (float)Main.rand.Next(-10, 11);
                dust = Main.dust[gaelDust];
                dust.position.Y += (float)Main.rand.Next(-10, 11);
                Main.dust[gaelDust].velocity.X = rando1;
                Main.dust[gaelDust].velocity.Y = rando2;
                timerCounter++;
            }
        }
    }
}
