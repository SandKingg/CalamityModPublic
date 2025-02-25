﻿using CalamityMod.Items.Placeables;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Rubblemaker.Abyss
{
    public class ShalePile1Echo : ModTile
    {
        public override string Texture => "CalamityMod/Tiles/Abyss/AbyssAmbient/ShalePile1";
        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.Origin = new Point16(1, 1);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.WaterDeath = false;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(47, 30, 63));
            DustType = 33;

            RegisterItemDrop(ModContent.ItemType<SulphurousShale>());
            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<SulphurousShale>(), Type, 0);

            base.SetStaticDefaults();
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 2;
        }
    }

    public class ShalePile2Echo : ShalePile1Echo
    {
        public override string Texture => "CalamityMod/Tiles/Abyss/AbyssAmbient/ShalePile2";
    }

    public class ShalePile3Echo : ShalePile1Echo
    {
        public override string Texture => "CalamityMod/Tiles/Abyss/AbyssAmbient/ShalePile3";
    }
}
