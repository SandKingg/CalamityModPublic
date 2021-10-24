using CalamityMod.Buffs.Summon;
using CalamityMod.NPCs.Other;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class SepulcherMinion : ModProjectile
    {
        public struct SepulcherSegment
        {
            public Vector2 CurrentPosition;
            public float Rotation;
            public SepulcherSegment(Vector2 position, float rotation)
            {
                CurrentPosition = position;
                Rotation = rotation;
            }
        }

        public class SepulcherArm
        {
            public class SepulcherArmLimb
            {
                public Vector2 Center;
                public float Rotation;

                public SepulcherArmLimb(Vector2 center, float rotation)
                {
                    Center = center;
                    Rotation = rotation;
                }

                public void SendData(BinaryWriter writer)
                {
                    writer.WritePackedVector2(Center);
                    writer.Write(Rotation);
                }

                public static SepulcherArmLimb ReceiveData(BinaryReader reader) => new SepulcherArmLimb(reader.ReadPackedVector2(), reader.ReadSingle());
            }

            public SepulcherArmLimb[] Limbs = new SepulcherArmLimb[2];
            public byte SegmentIndexToAttachTo;
            public Vector2 Center;
            public float Rotation;
            public bool ReelingBack = false;
            public bool Direction;
            public SepulcherArm(Vector2 center, byte segmentIndexToAttachTo, float rotation, bool reelingBack, bool direction)
            {
                Center = center;
                SegmentIndexToAttachTo = segmentIndexToAttachTo;
                Rotation = rotation;
                ReelingBack = reelingBack;
                Direction = direction;
                Limbs = new SepulcherArmLimb[2]
                {
                    new SepulcherArmLimb(center, rotation),
                    new SepulcherArmLimb(center, rotation)
                };
            }
        }

        public enum AIState
        {
            HoverNearOwner,
            AttackEnemy_Charge,
            AttackEnemy_ReleaseDartBurst,
            AttackOwner
        }

        public int IdleTimer;
        public int AttackTimer;
        public int PlayerAttackCountdown;
        public SepulcherSegment[] Segments = new SepulcherSegment[24];
        public List<SepulcherArm> Arms = new List<SepulcherArm>();
        public Player Owner => Main.player[projectile.owner];
        public AIState CurrentAIState
        {
            get => (AIState)(int)projectile.ai[0];
            set
            {
                // Don't do anything unless the caller is the owner of this projectile- changes will be synced to other clients in MP.
                if (Main.myPlayer != projectile.owner)
                    return;

                projectile.ai[0] = (int)value;
                projectile.netUpdate = true;
            }
        }
        public ref float JawRotation => ref projectile.localAI[0];
        public ref float JawSnapTimer => ref projectile.localAI[1];
        public const int HeartCreationRate = 420;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sepulcher");
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
            ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;
        }

        public override void SetDefaults()
        {
            projectile.width = 54;
            projectile.height = 54;
            projectile.friendly = true;
            projectile.ignoreWater = true;
            projectile.netImportant = true;
            projectile.penetrate = -1;
            projectile.minionSlots = 4f;
            projectile.timeLeft = 90000;
            projectile.Opacity = 0f;
            projectile.tileCollide = false;
            projectile.minion = true;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 4;
            projectile.hide = true;
        }

        #region Syncing
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(IdleTimer);
            writer.Write(PlayerAttackCountdown);
            writer.Write(Arms.Count);
            for (int i = 0; i < Arms.Count; i++)
            {
                writer.WritePackedVector2(Arms[i].Center);
                writer.Write(Arms[i].SegmentIndexToAttachTo);
                writer.Write(Arms[i].Rotation);
                writer.Write(Arms[i].ReelingBack);
                writer.Write(Arms[i].Direction);
                Arms[0].Limbs[0].SendData(writer);
                Arms[0].Limbs[1].SendData(writer);
            }
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            IdleTimer = reader.ReadInt32();
            PlayerAttackCountdown = reader.ReadInt32();
            Arms.Clear();
            for (int i = 0; i < reader.ReadInt32(); i++)
            {
                Arms.Add(new SepulcherArm(reader.ReadPackedVector2(), reader.ReadByte(), reader.ReadSingle(), reader.ReadBoolean(), reader.ReadBoolean()));
                Arms.Last().Limbs[0] = SepulcherArm.SepulcherArmLimb.ReceiveData(reader);
                Arms.Last().Limbs[1] = SepulcherArm.SepulcherArmLimb.ReceiveData(reader);
            }
        }
        #endregion

        #region AI
        public override void AI()
        {
            Initialize();
            HandleBuffs();
            HandleFlexibleDamage();
            UpdateSegments();

            // Fade in rapidly.
            projectile.Opacity = MathHelper.Clamp(projectile.Opacity + 0.2f, 0f, 1f);

            NPC potentialTarget = AttemptToFindTarget(1450f);

            // Enter the attack state if a valid enemy is found and go back to hovering otherwise.
            // This will be overriden if attacking the owner.
            if (CurrentAIState != AIState.AttackEnemy_ReleaseDartBurst && CurrentAIState != AIState.AttackEnemy_Charge)
            {
                if (potentialTarget != null)
                    CurrentAIState = AIState.AttackEnemy_ReleaseDartBurst;
            }
            else if (potentialTarget is null)
            {
                CurrentAIState = AIState.HoverNearOwner;
                AttackTimer = 0;
            }

            // Reset the AI to player attack mode if the countdown is going on.
            if (PlayerAttackCountdown > 0)
            {
                if (CurrentAIState != AIState.AttackOwner)
                    CurrentAIState = AIState.AttackOwner;

                PlayerAttackCountdown--;

                // If it's finished, go back to idle owner hover movement.
                if (PlayerAttackCountdown <= 0)
                    CurrentAIState = AIState.HoverNearOwner;
            }


            // Determine whether the projectile can attack its owner based on the attakc countdown.
            projectile.hostile = PlayerAttackCountdown > 0;

            switch (CurrentAIState)
            {
                case AIState.HoverNearOwner:
                    HoverNearOwner();
                    break;
                case AIState.AttackEnemy_ReleaseDartBurst:
                    AttackEnemyWithDarts(potentialTarget);
                    AttackTimer++;
                    break;
                case AIState.AttackEnemy_Charge:
                    AttackEnemyByCharging(potentialTarget);
                    AttackTimer++;
                    break;
                case AIState.AttackOwner:
                    AttackOwner();
                    break;
            }

            // Check if ready to enrage every so often.
            IdleTimer++;
            if (IdleTimer % HeartCreationRate == HeartCreationRate - 1f)
            {
                int heartsAttachedToOwner = 0;
                int heartType = ModContent.NPCType<ExhumedHeart>();
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].type != heartType || !Main.npc[i].active || Main.npc[i].target != projectile.owner)
                        continue;
                    heartsAttachedToOwner++;
                }

                float chanceToBecomeAngry = 0f;
                if (heartsAttachedToOwner >= 2)
                    chanceToBecomeAngry = Utils.InverseLerp(2f, 6f, heartsAttachedToOwner, true);
                if (Main.rand.NextFloat() < chanceToBecomeAngry)
                {
                    PlayerAttackCountdown = 300;
                    projectile.netUpdate = true;
                }
                else if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.NewNPC((int)Owner.Center.X, (int)Owner.Center.Y, heartType);
            }

            if (JawSnapTimer > 0f)
            {
                JawRotation = JawRotation.AngleTowards(-0.44f, 0.064f);
                JawSnapTimer--;
            }
            else
                JawRotation = JawRotation.AngleTowards(0f, 0.03f);
        }

        public void Initialize()
        {
            if (projectile.ai[1] != 0f)
                return;

            for (int i = 0; i < Segments.Length; i++)
                Segments[i].CurrentPosition = projectile.Center - projectile.velocity.SafeNormalize(Vector2.UnitY) * i * 0.2f;

            projectile.Calamity().spawnedPlayerMinionDamageValue = Owner.MinionDamage();
            projectile.Calamity().spawnedPlayerMinionProjectileDamageValue = projectile.damage;
            projectile.ai[1] = 1f;
            if (Main.myPlayer == projectile.owner)
            {
                float rotationalOffset = 0f;
                for (byte i = 3; i < Segments.Length - 2; i += 2)
                {
                    Arms.Add(new SepulcherArm(Segments[i].CurrentPosition, i, rotationalOffset, false, false));
                    rotationalOffset = MathHelper.WrapAngle(rotationalOffset + MathHelper.Pi / 6f);

                    Arms.Add(new SepulcherArm(Segments[i].CurrentPosition, i, rotationalOffset + MathHelper.Pi, false, true));
                    rotationalOffset = MathHelper.WrapAngle(rotationalOffset + MathHelper.Pi / 6f);
                }
                projectile.netUpdate = true;
            }
        }

        public void HandleBuffs()
        {
            // Maintain or remove the Sepulcher buff from the owner.
            Owner.AddBuff(ModContent.BuffType<SepulcherMinionBuff>(), 3600);
            if (Owner.dead)
                Owner.Calamity().sepulcher = false;
            if (Owner.Calamity().sepulcher)
                projectile.timeLeft = 2;
        }

        public void HandleFlexibleDamage()
        {
            // Adjust the damage of this projectile if the owner's current minion damage factor has changed for any reason.
            if (Owner.MinionDamage() != projectile.Calamity().spawnedPlayerMinionDamageValue)
            {
                int trueDamage = (int)(projectile.Calamity().spawnedPlayerMinionProjectileDamageValue / projectile.Calamity().spawnedPlayerMinionDamageValue * Owner.MinionDamage());
                projectile.damage = trueDamage;
            }
        }

        public void UpdateSegments()
        {
            float aheadRotation = projectile.rotation;
            Vector2 aheadPosition = projectile.Center;

            // Connect all segments to each-other and calculate their rotations.
            for (int i = 0; i < Segments.Length; i++)
            {
                Vector2 offsetToDestination = aheadPosition - Segments[i].CurrentPosition;
                // This variant of segment attachment incorporates rotation. 
                // Given the fact that all segments will execute this code is succession, the
                // result across the entire worm will exponentially decay over each segment, 
                // allowing for smooth rotations. This code is what the stardust dragon uses for its segmenting.
                if (aheadRotation != Segments[i].Rotation)
                {
                    float angleOffset = MathHelper.WrapAngle(aheadRotation - Segments[i].Rotation.AngleLerp(aheadRotation, 0.08f));
                    offsetToDestination = offsetToDestination.RotatedBy(angleOffset * 0.08f);
                }

                Segments[i].Rotation = (aheadPosition - Segments[i].CurrentPosition).ToRotation() + MathHelper.PiOver2;
                Segments[i].CurrentPosition = aheadPosition - offsetToDestination.SafeNormalize(Vector2.Zero) * 48f;

                // Adjust the ahead positions before interating further.
                aheadPosition = Segments[i].CurrentPosition;
                aheadRotation = Segments[i].Rotation;
            }

            // Adjust the arms.
            for (int i = 0; i < Arms.Count; i++)
            {
                SepulcherSegment segmentToAttachTo = Segments[Arms[i].SegmentIndexToAttachTo];
                Vector2 idealMovePosition = segmentToAttachTo.CurrentPosition;
                float sideFactor = MathHelper.Lerp(200f, 18f, Utils.InverseLerp(-0.51f, -0.06f, Arms[i].Rotation, true));
                float aheadFactor = MathHelper.Lerp(284f, 680f, Utils.InverseLerp(-0.51f, -0.06f, Arms[i].Rotation, true));
                int direction = Arms[i].Direction.ToDirectionInt();
                idealMovePosition += (segmentToAttachTo.Rotation + Arms[i].Rotation * direction - MathHelper.PiOver2).ToRotationVector2() * projectile.scale * aheadFactor;
                idealMovePosition += (segmentToAttachTo.Rotation + Arms[i].Rotation * direction - MathHelper.PiOver2 + MathHelper.PiOver2 * direction).ToRotationVector2() * projectile.scale * sideFactor;
                Arms[i].Center = idealMovePosition;

                Vector2 offsetFromSegment = Vector2.Zero;

                offsetFromSegment += new Vector2(direction * 35f, 28f).RotatedBy(segmentToAttachTo.Rotation - (MathHelper.PiOver2 - Arms[i].Rotation * 1.7f - 0.77f) * direction).SafeNormalize(Vector2.UnitY) * 62f;
                Arms[i].Limbs[0].Center = segmentToAttachTo.CurrentPosition + offsetFromSegment;
                Arms[i].Limbs[0].Rotation = offsetFromSegment.ToRotation();

                Arms[i].Limbs[1].Rotation = (Arms[i].Center - Arms[i].Limbs[0].Center).ToRotation();
                Arms[i].Limbs[1].Center = Arms[i].Limbs[0].Center + offsetFromSegment * 0.5f + (Arms[i].Center - Arms[i].Limbs[0].Center).SafeNormalize(Vector2.UnitY) * 64f;

                float rotationalVelocityFactor = Utils.InverseLerp(0f, 6f, projectile.velocity.Length(), true);
                if (CurrentAIState == AIState.AttackEnemy_Charge)
                    rotationalVelocityFactor *= 0.85f;
                if (CurrentAIState == AIState.AttackOwner)
                    rotationalVelocityFactor *= 1.2f;

                if (Arms[i].ReelingBack)
                {
                    Arms[i].Rotation -= rotationalVelocityFactor * 0.066f;
                    if (Arms[i].Rotation < -0.55f)
                        Arms[i].ReelingBack = false;
                }
                else
                {
                    Arms[i].Rotation += rotationalVelocityFactor * 0.029f;
                    if (Arms[i].Rotation > 0.77f)
                        Arms[i].ReelingBack = true;
                }
            }
        }

        public void HoverNearOwner()
        {
            // Teleport to the owner if it's very far away.
            if (!projectile.WithinRange(Owner.Center, 3200f))
            {
                projectile.Center = Owner.Center;
                projectile.velocity = Main.rand.NextVector2CircularEdge(4f, 4f);
                projectile.netUpdate = true;
            }

            // Attempt to move towards the owner.
            if (!projectile.WithinRange(Owner.Center, 540f))
            {
                Vector2 destination = Owner.Center + (IdleTimer / 31f).ToRotationVector2() * 240f;
                projectile.velocity = Vector2.Lerp(projectile.velocity, projectile.SafeDirectionTo(destination) * 18f, 0.03f);

                float updatedDirectionRotation = projectile.velocity.ToRotation().AngleTowards(projectile.AngleTo(destination), 0.04f);
                projectile.velocity = updatedDirectionRotation.ToRotationVector2() * projectile.velocity.Length();
            }
            else if (projectile.velocity.Length() < 16f)
                projectile.velocity *= 1.03f;

            // Determine the head rotation.
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public void AttackEnemyWithDarts(NPC target)
        {
            Vector2 destination = target.Center + ((AttackTimer / 20f).ToRotationVector2() * 320f);

            // Step towards the ideal velocity with an acceleration of 1 pixel/second^2.
            projectile.velocity = projectile.velocity.MoveTowards(projectile.SafeDirectionTo(destination) * 24f, 1f);
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Release a burst of darts from all segments at the target.
            if (Main.myPlayer == projectile.owner && AttackTimer % 60 > 60 - Segments.Length)
            {
                int segmentToFireFrom = 60 - AttackTimer % 60;
                Vector2 spawnPosition = Segments[segmentToFireFrom].CurrentPosition;
                Vector2 shootVelocity = (target.Center - spawnPosition).SafeNormalize(Vector2.UnitY) * 8f;
                Projectile.NewProjectile(spawnPosition, shootVelocity, ModContent.ProjectileType<BrimstoneDartMinion>(), projectile.damage / 2, projectile.knockBack, projectile.owner);
            }

            if (Main.myPlayer == projectile.owner && AttackTimer > 430)
            {
                CurrentAIState = AIState.AttackEnemy_Charge;
                AttackTimer = 0;
            }
        }

        public void AttackEnemyByCharging(NPC target)
        {
            if (!projectile.WithinRange(target.Center, 350f))
            {
                Vector2 destination = target.Center + (AttackTimer / 25f).ToRotationVector2() * MathHelper.Min(target.width * 0.35f, 150f);
                projectile.velocity = Vector2.Lerp(projectile.velocity, projectile.SafeDirectionTo(destination) * 34f, 0.08f);

                float updatedDirectionRotation = projectile.velocity.ToRotation().AngleTowards(projectile.AngleTo(destination), 0.12f);
                projectile.velocity = updatedDirectionRotation.ToRotationVector2() * projectile.velocity.Length();
            }
            else if (projectile.velocity.Length() < 29f)
            {
                projectile.velocity *= 1.035f;
                if (JawSnapTimer <= 0f)
                {
                    // Open the mouth of the jaw if close but not super close to the target.
                    if (projectile.WithinRange(target.Center, 270f))
                        JawRotation = JawRotation.AngleLerp(0.87f, 0.12f);

                    // If really close, snap.
                    if (projectile.WithinRange(target.Center, 165f))
                    {
                        Main.PlaySound(SoundID.DD2_SkeletonHurt, projectile.Center);
                        JawSnapTimer = 45f;
                    }
                }
            }

            // Determine the head rotation.
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Main.myPlayer == projectile.owner && AttackTimer > 480)
            {
                CurrentAIState = AIState.AttackEnemy_ReleaseDartBurst;
                AttackTimer = 0;
            }
        }

        public void AttackOwner()
        {
            // Attempt to angle towards the owner.
            float newSpeed = MathHelper.Lerp(projectile.velocity.Length(), 21f, 0.025f);

            if (!projectile.WithinRange(Owner.Center, 370f))
            {
                Vector2 idealVelocity = projectile.SafeDirectionTo(Owner.Center) * newSpeed;
                projectile.velocity = projectile.velocity.MoveTowards(idealVelocity, 2f);

                float idealAimDirection = idealVelocity.ToRotation();
                projectile.velocity = projectile.velocity.ToRotation().AngleLerp(idealAimDirection, 0.05f).ToRotationVector2() * projectile.velocity.Length();
            }
            else if (projectile.velocity.Length() < 36f)
            {
                projectile.velocity *= 1.03f;
                if (JawSnapTimer <= 0f)
                {
                    // Open the mouth of the jaw if close but not super close to the owner.
                    if (projectile.WithinRange(Owner.Center, 300f))
                        JawRotation = JawRotation.AngleLerp(0.87f, 0.12f);

                    // If really close, snap.
                    if (projectile.WithinRange(Owner.Center, 180f))
                    {
                        Main.PlaySound(SoundID.DD2_SkeletonHurt, projectile.Center);
                        JawSnapTimer = 45f;
                    }
                }
            }

            // Determine the head rotation.
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public NPC AttemptToFindTarget(float searchDistance)
        {
            NPC closestTarget = null;

            int heartType = ModContent.NPCType<ExhumedHeart>();
            if (Owner.HasMinionAttackTargetNPC)
            {
                NPC targetedNPC = Main.npc[Owner.MinionAttackTargetNPC];
                if (targetedNPC.WithinRange(projectile.Center, searchDistance) && targetedNPC.CanBeChasedBy() && targetedNPC.type != heartType)
                    return targetedNPC;
            }

            float distance = searchDistance * searchDistance;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].CanBeChasedBy() && Main.npc[i].type != heartType)
                {
                    float extraDistance = (Main.npc[i].width / 2) + (Main.npc[i].height / 2);

                    if (Main.npc[i].WithinRange(projectile.Center, distance + extraDistance))
                    {
                        distance = Main.npc[i].DistanceSQ(projectile.Center);
                        closestTarget = Main.npc[i];
                    }
                }
            }

            return closestTarget;
        }

        #endregion

        #region Drawing

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            float angerFactor = Utils.InverseLerp(300f, 280f, PlayerAttackCountdown, true) * Utils.InverseLerp(0f, 30f, PlayerAttackCountdown, true);
            float afterimageOutwardness = MathHelper.Lerp(6f, 8f, (float)Math.Cos(Main.GlobalTime * 2.3f) * 0.5f + 0.5f) * angerFactor;
            Color backAfterimageColor = Color.Red * angerFactor;
            backAfterimageColor.A = 0;

            Texture2D eyesTexture = ModContent.GetTexture("CalamityMod/Projectiles/Summon/SepulcherMinionEyes");
            Texture2D jawTexture = ModContent.GetTexture("CalamityMod/Projectiles/Summon/SepulcherMinionJaw");
            Texture2D headTexture = ModContent.GetTexture("CalamityMod/Projectiles/Summon/SepulcherMinionHead");
            Texture2D bodyTexture = ModContent.GetTexture("CalamityMod/Projectiles/Summon/SepulcherMinionBody");
            Texture2D bodyTexture2 = ModContent.GetTexture("CalamityMod/Projectiles/Summon/SepulcherMinionBodyAlt");
            Texture2D tailTexture = ModContent.GetTexture("CalamityMod/Projectiles/Summon/SepulcherMinionTail");
            Texture2D armTexture = ModContent.GetTexture("CalamityMod/Projectiles/Summon/SepulcherArm");
            Texture2D foreArmTexture = ModContent.GetTexture("CalamityMod/Projectiles/Summon/SepulcherForeArm");
            Texture2D handTexture = ModContent.GetTexture("CalamityMod/Projectiles/Summon/SepulcherHand");

            Vector2 drawPosition;

            // Draw the arms.
            for (int i = 0; i < Arms.Count; i++)
            {
                Vector2 forearmDrawPosition = Arms[i].Limbs[0].Center - Main.screenPosition;
                Color drawColor = Lighting.GetColor((int)(Arms[i].Limbs[0].Center.X / 16), (int)(Arms[i].Limbs[0].Center.Y / 16));

                if (afterimageOutwardness > 0f)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        Vector2 drawOffset = (MathHelper.TwoPi * j / 4f).ToRotationVector2() * afterimageOutwardness;
                        spriteBatch.Draw(foreArmTexture, forearmDrawPosition + drawOffset, null, backAfterimageColor, Arms[i].Limbs[0].Rotation + MathHelper.PiOver2, foreArmTexture.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0f);
                    }
                }
                spriteBatch.Draw(foreArmTexture, forearmDrawPosition, null, drawColor, Arms[i].Limbs[0].Rotation + MathHelper.PiOver2, foreArmTexture.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0f);

                Vector2 armDrawPosition = Arms[i].Limbs[1].Center - Main.screenPosition;
                drawColor = Lighting.GetColor((int)(Arms[i].Limbs[1].Center.X / 16), (int)(Arms[i].Limbs[1].Center.Y / 16));

                if (afterimageOutwardness > 0f)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        Vector2 drawOffset = (MathHelper.TwoPi * j / 4f).ToRotationVector2() * afterimageOutwardness;
                        spriteBatch.Draw(armTexture, armDrawPosition + drawOffset, null, backAfterimageColor, Arms[i].Limbs[1].Rotation + MathHelper.PiOver2, armTexture.Size() * new Vector2(0.5f, 0f), projectile.scale, SpriteEffects.FlipVertically, 0f);
                    }
                }
                spriteBatch.Draw(armTexture, armDrawPosition, null, drawColor, Arms[i].Limbs[1].Rotation + MathHelper.PiOver2, armTexture.Size() * new Vector2(0.5f, 0f), projectile.scale, SpriteEffects.FlipVertically, 0f);

                Vector2 handDrawPosition = armDrawPosition;
                SpriteEffects handDirection = Arms[i].Direction ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                if (afterimageOutwardness > 0f)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        Vector2 drawOffset = (MathHelper.TwoPi * j / 4f).ToRotationVector2() * afterimageOutwardness;
                        spriteBatch.Draw(handTexture, handDrawPosition + drawOffset, null, backAfterimageColor, Arms[i].Limbs[1].Rotation - MathHelper.PiOver2, handTexture.Size() * new Vector2(0.5f, 0f), projectile.scale, handDirection, 0f);
                    }
                }
                spriteBatch.Draw(handTexture, handDrawPosition, null, drawColor, Arms[i].Limbs[1].Rotation - MathHelper.PiOver2, handTexture.Size() * new Vector2(0.5f, 0f), projectile.scale, handDirection, 0f);
            }

            // Draw the segments.
            for (int i = 0; i < Segments.Length; i++)
            {
                Texture2D textureToUse = i % 2 == 1 ? bodyTexture2 : bodyTexture;

                // Use the tail texture for the last segment.
                if (i == Segments.Length - 1)
                    textureToUse = tailTexture;

                drawPosition = Segments[i].CurrentPosition - Main.screenPosition;
                lightColor = Lighting.GetColor((int)(drawPosition.X + Main.screenPosition.X) / 16, (int)(drawPosition.Y + Main.screenPosition.Y) / 16);

                if (afterimageOutwardness > 0f)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        Vector2 drawOffset = (MathHelper.TwoPi * j / 4f).ToRotationVector2() * afterimageOutwardness;
                        spriteBatch.Draw(textureToUse, drawPosition + drawOffset, null, backAfterimageColor, Segments[i].Rotation, textureToUse.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0f);
                    }
                }
                spriteBatch.Draw(textureToUse, drawPosition, null, projectile.GetAlpha(lightColor), Segments[i].Rotation, textureToUse.Size() * 0.5f, projectile.scale, SpriteEffects.None, 0f);
            }

            drawPosition = projectile.Center - Main.screenPosition;
            lightColor = Lighting.GetColor((int)(drawPosition.X + Main.screenPosition.X) / 16, (int)(drawPosition.Y + Main.screenPosition.Y) / 16);

            // Draw the jaws.
            for (int i = -1; i <= 1; i += 2)
            {
                float jawBaseOffset = 24f;
                SpriteEffects jawSpriteEffect = i == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                Vector2 jawPosition = projectile.Center - Main.screenPosition;
                jawPosition += Vector2.UnitX.RotatedBy(projectile.rotation + JawRotation * i) * i * (jawBaseOffset + (float)Math.Sin(JawRotation) * 14f);
                jawPosition -= Vector2.UnitY.RotatedBy(projectile.rotation) * (26f + (float)Math.Sin(JawRotation) * 8f);
                spriteBatch.Draw(jawTexture, jawPosition, null, projectile.GetAlpha(lightColor), projectile.rotation + JawRotation * i, jawTexture.Size() * 0.5f, projectile.scale * 1.25f, jawSpriteEffect, 0f);
            }

            // Draw the head.
            if (afterimageOutwardness > 0f)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 drawOffset = (MathHelper.TwoPi * i / 4f).ToRotationVector2() * afterimageOutwardness;
                    spriteBatch.Draw(headTexture, drawPosition + drawOffset, null, backAfterimageColor, projectile.rotation, headTexture.Size() * 0.5f, projectile.scale * 1.25f, SpriteEffects.None, 0f);
                }
            }
            spriteBatch.Draw(headTexture, drawPosition, null, projectile.GetAlpha(lightColor), projectile.rotation, headTexture.Size() * 0.5f, projectile.scale * 1.25f, SpriteEffects.None, 0f);

            // Draw demonic eyes if enraged.
            if (afterimageOutwardness > 0f)
            {
                for (int i = 0; i < (int)(projectile.oldPos.Length * angerFactor); i++)
                {
                    drawPosition = projectile.Center - Main.screenPosition - projectile.velocity.SafeNormalize(Vector2.Zero) * 3f * i;
                    Color fadeColor = Color.White * (1f - i / (float)projectile.oldPos.Length);
                    spriteBatch.Draw(eyesTexture, drawPosition, null, projectile.GetAlpha(fadeColor), projectile.oldRot[i], eyesTexture.Size() * 0.5f, projectile.scale * 1.25f, SpriteEffects.None, 0f);
                }
            }

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // If default collision is possible return true immediately.
            if (projHitbox.Intersects(targetHitbox))
                return true;

            // Check based on segments.
            for (int i = 0; i < Segments.Length; i++)
            {
                if (Utils.CenteredRectangle(Segments[i].CurrentPosition, Vector2.One * 52f).Intersects(targetHitbox))
                    return true;
            }

            return false;
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindProjectiles.Add(index);
        }

        #endregion

        #region Damage Stuff
        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            damage = Main.rand.Next(80, 90);
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (target.type == ModContent.NPCType<ExhumedHeart>())
                return false;
            return null;
        }
        #endregion
    }
}
