using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.FurnitureAshen
{
    public class AshenChandelier : ModTile
    {
        int animationFrame = 0;

        public override void SetDefaults()
        {
            this.SetUpChandelier(true);
            AddMapEntry(new Color(191, 142, 111), Language.GetText("MapObject.Chandelier"));
            animationFrameHeight = 54;
            adjTiles = new int[] { TileID.Chandeliers };
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, 60, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, 1, 0f, 0f, 1, new Color(100, 100, 100), 1f);
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            frameCounter++;
            if (frameCounter >= 6)
            {
                frame = (frame + 1) % 6;
                animationFrame = frame;
                frameCounter = 0;
            }
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            if (Main.tile[i, j].frameX < 18)
            {
                r = 1f;
                g = 0.5f;
                b = 0.5f;
            }
            else
            {
                r = 0f;
                g = 0f;
                b = 0f;
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(i * 16, j * 16, 48, 48, ModContent.ItemType<Items.Placeables.FurnitureAshen.AshenChandelier>());
        }

        public override void HitWire(int i, int j)
        {
            CalamityUtils.LightHitWire(Type, i, j, 3, 3);
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            CalamityUtils.DrawStaticFlameEffect(ModContent.GetTexture("CalamityMod/Tiles/FurnitureAshen/AshenChandelierFlame"), i, j, offsetY: animationFrame * animationFrameHeight);
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            Tile tile = Main.tile[i, j];
            if (tile.frameY == 18 && tile.frameX < 54)
            {
                CalamityUtils.DrawFlameSparks(60, 5, i, j);
            }
        }
    }
}
