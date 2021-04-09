using CalamityMod.CalPlayer;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.UI
{
    public static class RipperUI
    {
        public const float DefaultRagePosX = 500f;
        public const float DefaultRagePosY = 30f;
        public const float DefaultAdrenPosX = 650f;
        public const float DefaultAdrenPosY = 30f;

        private const int RageAnimFrameDelay = 6;
        private const int RageAnimFrames = 9;
        private const int AdrenAnimFrameDelay = 5;
        private const int AdrenAnimFrames = 9;
        
        public static Vector2 rageDrawPos = new Vector2(DefaultRagePosX, DefaultRagePosY);
        public static Vector2 adrenDrawPos = new Vector2(DefaultAdrenPosX, DefaultAdrenPosY);
        private static Vector2? rageDragOffset = null;
        private static Vector2? adrenDragOffset = null;
        private static Vector2 pearlOffsetLeft = Vector2.Zero;
        private static Vector2 pearlOffsetCenter = Vector2.Zero;
        private static Vector2 pearlOffsetRight = Vector2.Zero;
        private static Vector2 PearlOffsetCenterLeft => 0.5f * (pearlOffsetLeft + pearlOffsetCenter);
        private static Vector2 PearlOffsetCenterRight => 0.5f * (pearlOffsetRight + pearlOffsetCenter);

        private static int rageAnimFrame = -1;
        private static int rageAnimTimer = 0;
        private static int adrenAnimFrame = -1;
        private static int adrenAnimTimer = 0;

        private static Texture2D rageBarTex, rageBorderTex, rageAnimTex;
        private static Texture2D mushroomPlasmaTex, infernalBloodTex, redLightningTex;
        private static Texture2D adrenBarTex, adrenBorderTex, adrenAnimTex;
        private static Texture2D electrolyteGelTex, starlightFuelTex, ectoheartTex;

        internal static void Load()
        {
            rageBarTex = ModContent.GetTexture("CalamityMod/ExtraTextures/UI/RageBar");
            rageBorderTex = ModContent.GetTexture("CalamityMod/ExtraTextures/UI/RageBarBorder");
            rageAnimTex = ModContent.GetTexture("CalamityMod/ExtraTextures/UI/RageFullAnimation");

            adrenBarTex = ModContent.GetTexture("CalamityMod/ExtraTextures/UI/AdrenalineBar");
            adrenBorderTex = ModContent.GetTexture("CalamityMod/ExtraTextures/UI/AdrenalineBarBorder");
            adrenAnimTex = ModContent.GetTexture("CalamityMod/ExtraTextures/UI/AdrenalineFullAnimation");

            mushroomPlasmaTex = ModContent.GetTexture("CalamityMod/ExtraTextures/UI/RageDisplay_MushroomPlasmaRoot");
            infernalBloodTex = ModContent.GetTexture("CalamityMod/ExtraTextures/UI/RageDisplay_InfernalBlood");
            redLightningTex = ModContent.GetTexture("CalamityMod/ExtraTextures/UI/RageDisplay_RedLightningContainer");

            electrolyteGelTex = ModContent.GetTexture("CalamityMod/ExtraTextures/UI/AdrenalineDisplay_ElectrolyteGelPack");
            starlightFuelTex = ModContent.GetTexture("CalamityMod/ExtraTextures/UI/AdrenalineDisplay_StarlightFuelCell");
            ectoheartTex = ModContent.GetTexture("CalamityMod/ExtraTextures/UI/AdrenalineDisplay_Ectoheart");

            pearlOffsetLeft = new Vector2(rageBorderTex.Width * 0.3333f - 6f, rageBorderTex.Height - 9f);
            pearlOffsetCenter = new Vector2(rageBorderTex.Width * 0.5f - 6f, rageBorderTex.Height - 9f);
            pearlOffsetRight = new Vector2(rageBorderTex.Width * 0.6667f - 6f, rageBorderTex.Height - 9f);

            Reset();
        }

        internal static void Unload()
        {
            Reset();

            rageBarTex = rageBorderTex = rageAnimTex = null;
            adrenBarTex = adrenBorderTex = adrenAnimTex = null;
            mushroomPlasmaTex = infernalBloodTex = redLightningTex = null;
            electrolyteGelTex = starlightFuelTex = ectoheartTex = null;
            pearlOffsetLeft = pearlOffsetCenter = pearlOffsetRight = Vector2.Zero;
        }

        internal static void Reset()
        {
            bool configExists = CalamityConfig.Instance != null;

            rageDrawPos = configExists ? new Vector2(CalamityConfig.Instance.RageMeterPosX, CalamityConfig.Instance.RageMeterPosY) : Vector2.Zero;
            adrenDrawPos = configExists ? new Vector2(CalamityConfig.Instance.AdrenalineMeterPosX, CalamityConfig.Instance.AdrenalineMeterPosY) : Vector2.Zero;
            rageDragOffset = null;
            adrenDragOffset = null;
            rageAnimFrame = -1;
            rageAnimTimer = 0;
            adrenAnimFrame = -1;
            adrenAnimTimer = 0;
        }

        public static void Draw(SpriteBatch spriteBatch, Player player)
        {
            // If Revengeance isn't on, or Rage and Adrenaline are turned off, don't draw anything.
            if (!CalamityWorld.revenge || !CalamityConfig.Instance.Rippers)
                return;

            // If for some reason either of the bars has been thrown into the corner (likely to 0,0 by default), put them at their default positions
            CheckGarbageCornerPos();

            // Prevent blurriness which results from decimals in the position variables.
            rageDrawPos.X = (int)rageDrawPos.X;
            rageDrawPos.Y = (int)rageDrawPos.Y;
            adrenDrawPos.X = (int)adrenDrawPos.X;
            adrenDrawPos.Y = (int)adrenDrawPos.Y;

            // Grab the ModPlayer object and then start drawing
            CalamityPlayer modPlayer = player.Calamity();
            DrawRageBar(spriteBatch, modPlayer);
            DrawAdrenalineBar(spriteBatch, modPlayer);

            HandleMouseInteraction(modPlayer, rageBorderTex.Size(), adrenBorderTex.Size());
        }

        private static void DrawRageBar(SpriteBatch spriteBatch, CalamityPlayer modPlayer)
        {
            Vector2 shakeOffset = modPlayer.rageModeActive ? GetShakeOffset() : Vector2.Zero;

            // If rage is full this frame and the animation hasn't started yet, start it.
            float rageRatio = modPlayer.rage / modPlayer.rageMax;
            if (rageRatio >= 1f && rageAnimFrame == -1)
                rageAnimFrame = 0;

            // If the animation has already finished and rage isn't full anymore, reset it.
            else if (rageRatio < 1f && rageAnimFrame == RageAnimFrames)
                rageAnimFrame = -1;

            // Otherwise, the animation runs to completion even if the user activates rage in the middle of it.
            bool animationActive = rageAnimFrame >= 0 && rageAnimFrame < RageAnimFrames;
            if (animationActive)
            {
                rageAnimTimer++;
                if(rageAnimTimer >= RageAnimFrameDelay)
                {
                    rageAnimTimer = 0;
                    rageAnimFrame++; // This will eventually increment it to RageAnimFrames, thus stopping the animation.
                }
            }

            float uiScale = Main.UIScale;

            // Draw the border of the Rage Bar first
            spriteBatch.Draw(rageBorderTex, rageDrawPos + shakeOffset, null, Color.White, 0f, rageBorderTex.Size() * 0.5f, uiScale, SpriteEffects.None, 0);

            // The amount of the bar to draw depends on the player's current Rage level
            // 7 pixels of dead space, 90 pixels of bar, 7 pixels of dead space. Bar is 24 pixels tall
            int deadSpace = 7;
            int barWidth = rageBarTex.Width - 2 * deadSpace;
            Rectangle cropRect = new Rectangle(0, 0, deadSpace + (int)(barWidth * rageRatio), rageBarTex.Height);
            spriteBatch.Draw(rageBarTex, rageDrawPos + shakeOffset, cropRect, Color.White, 0f, rageBorderTex.Size() * 0.5f, uiScale, SpriteEffects.None, 0);

            // Determine which pearls to draw (and their positions) based off of which Rage upgrades the player has.
            IList<Texture2D> pearls = new List<Texture2D>(3);
            if (modPlayer.rageBoostOne) // Mushroom Plasma Root
                pearls.Add(mushroomPlasmaTex);
            if (modPlayer.rageBoostTwo) // Infernal Blood
                pearls.Add(infernalBloodTex);
            if (modPlayer.rageBoostThree) // Red Lightning Container
                pearls.Add(redLightningTex);
            IList<Vector2> offsets = GetPearlOffsets(pearls.Count);
            
            // Draw pearls at appropriate positions.
            for (int i = 0; i < pearls.Count; ++i)
                spriteBatch.Draw(pearls[i], rageDrawPos + shakeOffset + offsets[i], null, Color.White, 0f, rageBorderTex.Size() * 0.5f, uiScale, SpriteEffects.None, 0);

            // If the animation is active, draw the animation on top of both the border and the bar.
            if (animationActive)
            {
                float xOffset = (rageBorderTex.Width - rageAnimTex.Width) / 2f;
                int frameHeight = (rageAnimTex.Height / RageAnimFrames) - 1;
                float yOffset = (rageBorderTex.Height - frameHeight) / 2f;
                Vector2 sizeDiffOffset = new Vector2(xOffset, yOffset);
                Rectangle animCropRect = new Rectangle(0, (frameHeight + 1) * rageAnimFrame, rageAnimTex.Width, frameHeight);
                spriteBatch.Draw(rageAnimTex, rageDrawPos + shakeOffset + sizeDiffOffset, animCropRect, Color.White, 0f, rageBorderTex.Size() * 0.5f, uiScale, SpriteEffects.None, 0);
            }
        }

        private static void DrawAdrenalineBar(SpriteBatch spriteBatch, CalamityPlayer modPlayer)
        {
            Vector2 shakeOffset = modPlayer.adrenalineModeActive ? GetShakeOffset() : Vector2.Zero;

            // If adrenaline is full this frame and the animation hasn't started yet, start it.
            float adrenRatio = modPlayer.adrenaline / modPlayer.adrenalineMax;
            if (adrenRatio >= 1f && adrenAnimFrame == -1)
                adrenAnimFrame = 0;

            // If the animation has already finished and adrenaline isn't full anymore, reset it.
            else if (adrenRatio < 1f && adrenAnimFrame == AdrenAnimFrames)
                adrenAnimFrame = -1;

            // Otherwise, the animation runs to completion even if the user activates adrenaline in the middle of it.
            bool animationActive = adrenAnimFrame >= 0 && adrenAnimFrame < AdrenAnimFrames;
            if (animationActive)
            {
                adrenAnimTimer++;
                if (adrenAnimTimer >= AdrenAnimFrameDelay)
                {
                    adrenAnimTimer = 0;
                    adrenAnimFrame++; // This will eventually increment it to AdrenAnimFrames, thus stopping the animation.
                }
            }

            float uiScale = Main.UIScale;

            // Draw the border of the Adrenaline Bar first
            spriteBatch.Draw(adrenBorderTex, adrenDrawPos + shakeOffset, null, Color.White, 0f, adrenBorderTex.Size() * 0.5f, uiScale, SpriteEffects.None, 0);

            // The amount of the bar to draw depends on the player's current Adrenaline level
            // 7 pixels of dead space, 90 pixels of bar, 7 pixels of dead space.
            int deadSpace = 7;
            int barWidth = adrenBarTex.Width - 2 * deadSpace;
            Rectangle cropRect = new Rectangle(0, 0, deadSpace + (int)(barWidth * adrenRatio), adrenBarTex.Height);
            spriteBatch.Draw(adrenBarTex, adrenDrawPos + shakeOffset, cropRect, Color.White, 0f, adrenBorderTex.Size() * 0.5f, uiScale, SpriteEffects.None, 0);

            // Determine which pearls to draw (and their positions) based off of which Adrenaline upgrades the player has.
            IList<Texture2D> pearls = new List<Texture2D>(3);
            if (modPlayer.adrenalineBoostOne) // Electrolyte Gel Pack
                pearls.Add(electrolyteGelTex);
            if (modPlayer.adrenalineBoostTwo) // Starlight Fuel Cell
                pearls.Add(starlightFuelTex);
            if (modPlayer.adrenalineBoostThree) // Ectoheart
                pearls.Add(ectoheartTex);
            IList<Vector2> offsets = GetPearlOffsets(pearls.Count);

            // Draw pearls at appropriate positions.
            for (int i = 0; i < pearls.Count; ++i)
                spriteBatch.Draw(pearls[i], adrenDrawPos + shakeOffset + offsets[i], null, Color.White, 0f, adrenBorderTex.Size() * 0.5f, uiScale, SpriteEffects.None, 0);

            // If the animation is active, draw the animation on top of both the border and the bar.
            if (animationActive)
            {
                float xOffset = (adrenBorderTex.Width - adrenAnimTex.Width) / 2f;
                int frameHeight = (adrenAnimTex.Height / AdrenAnimFrames) - 1;
                float yOffset = (adrenBorderTex.Height - frameHeight) / 2f;
                Vector2 sizeDiffOffset = new Vector2(xOffset, yOffset);
                Rectangle animCropRect = new Rectangle(0, (frameHeight + 1) * adrenAnimFrame, adrenAnimTex.Width, frameHeight);
                spriteBatch.Draw(adrenAnimTex, adrenDrawPos + shakeOffset + sizeDiffOffset, animCropRect, Color.White, 0f, adrenBorderTex.Size() * 0.5f, uiScale, SpriteEffects.None, 0);
            }
        }

        private static void HandleMouseInteraction(CalamityPlayer modPlayer, Vector2 rageBarSize, Vector2 adrenBarSize)
        {
            float uiScale = Main.UIScale;
            Rectangle mouse = new Rectangle((int)Main.MouseScreen.X, (int)Main.MouseScreen.Y, 8, 8);
            Rectangle rageBar = Utils.CenteredRectangle(rageDrawPos, rageBarSize * uiScale);
            Rectangle adrenBar = Utils.CenteredRectangle(adrenDrawPos, adrenBarSize * uiScale);

            bool rageHover = mouse.Intersects(rageBar);
            bool adrenHover = mouse.Intersects(adrenBar);

            MouseState mouseInput = Mouse.GetState();
            Vector2 mousePos = Main.MouseScreen;

            // Rage bar takes priority, and only one can be hovered at a time
            if (rageHover && !adrenHover)
            {
                // Add hover text if the mouse is over Rage bar
                Main.LocalPlayer.mouseInterface = true;
                string rageStr = MakeRipperPercentString(modPlayer.rage, modPlayer.rageMax);
                Main.instance.MouseText($"Rage: {rageStr}");

                // The bar is draggable if enabled in config.
                if (!CalamityConfig.Instance.MeterPosLock && mouseInput.LeftButton == ButtonState.Pressed)
                {
                    // On the first frame new mouse input comes in, set the offset. Otherwise do nothing.
                    if (rageDragOffset is null)
                        rageDragOffset = rageDrawPos - mousePos;
                }
            }
            else if (adrenHover)
            {
                // Add hover text if the mouse is over the bar
                Main.LocalPlayer.mouseInterface = true;
                string adrenStr = MakeRipperPercentString(modPlayer.adrenaline, modPlayer.adrenalineMax);
                Main.instance.MouseText($"Adrenaline: {adrenStr}");

                // The bar is draggable if enabled in config.
                if (!CalamityConfig.Instance.MeterPosLock && mouseInput.LeftButton == ButtonState.Pressed)
                {
                    // On the first frame new mouse input comes in, set the offset. Otherwise do nothing.
                    if (adrenDragOffset is null)
                        adrenDragOffset = adrenDrawPos - mousePos;
                }
            }

            // If the rage bar is currently being dragged, move it to follow the mouse.
            if (rageDragOffset.HasValue)
                rageDrawPos = mousePos + rageDragOffset.GetValueOrDefault();

            // If the adrenaline bar is currently being dragged, move it to follow the mouse.
            if (adrenDragOffset.HasValue)
                adrenDrawPos = mousePos + adrenDragOffset.GetValueOrDefault();

            // Regardless of what is hovered, when the mouse is released, destroy the offset, and save the config.
            if (mouseInput.LeftButton == ButtonState.Released)
            {
                bool updateCfg = false;

                if (rageDragOffset.HasValue)
                {
                    rageDragOffset = null;
                    if (CalamityConfig.Instance.RageMeterPosX != rageDrawPos.X)
                    {
                        CalamityConfig.Instance.RageMeterPosX = rageDrawPos.X;
                        updateCfg = true;
                    }
                    if (CalamityConfig.Instance.RageMeterPosY != rageDrawPos.Y)
                    {
                        CalamityConfig.Instance.RageMeterPosY = rageDrawPos.Y;
                        updateCfg = true;
                    }
                }
                if (adrenDragOffset.HasValue)
                {
                    adrenDragOffset = null;
                    if (CalamityConfig.Instance.AdrenalineMeterPosX != adrenDrawPos.X)
                    {
                        CalamityConfig.Instance.AdrenalineMeterPosX = adrenDrawPos.X;
                        updateCfg = true;
                    }
                    if (CalamityConfig.Instance.AdrenalineMeterPosY != adrenDrawPos.Y)
                    {
                        CalamityConfig.Instance.AdrenalineMeterPosY = adrenDrawPos.Y;
                        updateCfg = true;
                    }
                }
                if (updateCfg)
                    CalamityMod.SaveConfig(CalamityConfig.Instance);
            }
        }

        private static void CheckGarbageCornerPos()
        {
            // This should only happen if something happens to the config file.
            if (rageDrawPos == Vector2.Zero)
                rageDrawPos = new Vector2(DefaultRagePosX, DefaultRagePosY);
            rageDrawPos = Vector2.Clamp(rageDrawPos, Vector2.Zero, new Vector2(Main.screenWidth - 104f, Main.screenHeight - 24f));

            if (adrenDrawPos == Vector2.Zero)
                adrenDrawPos = new Vector2(DefaultAdrenPosX, DefaultAdrenPosY);
            adrenDrawPos = Vector2.Clamp(adrenDrawPos, Vector2.Zero, new Vector2(Main.screenWidth - 104f, Main.screenHeight - 24f));
        }

        private static Vector2 GetShakeOffset()
        {
            float shake = CalamityConfig.Instance.MeterShake;
            float shakeX = Main.rand.NextFloat(-shake, shake);
            float shakeY = Main.rand.NextFloat(-shake, shake);
            return new Vector2(shakeX, shakeY);
        }

        private static IList<Vector2> GetPearlOffsets(int count)
        {
            switch (count)
            {
                case 1:
                    return new List<Vector2>() { pearlOffsetCenter };
                case 2:
                    return new List<Vector2>() { PearlOffsetCenterLeft, PearlOffsetCenterRight };
                case 3:
                    return new List<Vector2>() { pearlOffsetLeft, pearlOffsetCenter, pearlOffsetRight };
                default:
                    return null;
            }
        }

        private static string MakeRipperPercentString(float value, float maxValue)
        {
            string topTwoDecimalPlaces = value.ToString("n2");
            string bottomTwoDecimalPlaces = maxValue.ToString("n2");
            string percent = (100f * value / maxValue).ToString("0.00");
            StringBuilder sb = new StringBuilder(32);
            sb.Append(percent);
            sb.Append("% (");
            sb.Append(topTwoDecimalPlaces);
            sb.Append(" / ");
            sb.Append(bottomTwoDecimalPlaces);
            sb.Append(')');
            return sb.ToString();
        }
    }
}
