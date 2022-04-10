﻿using CalamityMod.Events;
using CalamityMod.Items.Materials;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.Signus;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.World;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent.Creative;

namespace CalamityMod.Items.SummonItems
{
    public class RuneofCos : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rune of Kos");
            Tooltip.SetDefault("A relic of the profaned flame\n" +
                "Contains the power hunted relentlessly by the sentinels of the cosmic devourer\n" +
                "When used in certain areas of the world, it will unleash them\n" +
                "Not consumable");
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.useAnimation = 45;
            Item.useTime = 45;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.UseSound = SoundID.Item44;
            Item.consumable = false;
            Item.rare = ItemRarityID.Purple;
            Item.Calamity().customRarity = CalamityRarity.Turquoise;
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override bool CanUseItem(Player player)
        {
            return (player.ZoneSkyHeight || player.ZoneUnderworldHeight || player.ZoneDungeon) &&
                !NPC.AnyNPCs(ModContent.NPCType<StormWeaverHead>()) && !NPC.AnyNPCs(ModContent.NPCType<CeaselessVoid>()) && !NPC.AnyNPCs(ModContent.NPCType<Signus>()) &&
                CalamityWorld.DoGSecondStageCountdown <= 0 && !BossRushEvent.BossRushActive;
        }

        public override bool? UseItem(Player player)
        {
            SoundEngine.PlaySound(SoundID.Roar, player.position, 0);
            if (player.ZoneDungeon)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<CeaselessVoid>());
                else
                    NetMessage.SendData(MessageID.SpawnBoss, -1, -1, null, player.whoAmI, ModContent.NPCType<CeaselessVoid>());
            }
            else if (player.ZoneUnderworldHeight)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<Signus>());
                else
                    NetMessage.SendData(MessageID.SpawnBoss, -1, -1, null, player.whoAmI, ModContent.NPCType<Signus>());
            }
            else if (player.ZoneSkyHeight)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.SpawnOnPlayer(player.whoAmI, ModContent.NPCType<StormWeaverHead>());
                else
                    NetMessage.SendData(MessageID.SpawnBoss, -1, -1, null, player.whoAmI, ModContent.NPCType<StormWeaverHead>());
            }

            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe(1).AddIngredient(ModContent.ItemType<UnholyEssence>(), 40).AddIngredient(ItemID.FragmentSolar, 5).AddIngredient(ItemID.LunarBar, 3).AddTile(TileID.LunarCraftingStation).Register();
        }
    }
}
