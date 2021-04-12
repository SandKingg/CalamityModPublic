﻿using CalamityMod.CalPlayer;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor;
using CalamityMod.Items.Placeables.Furniture.Fountains;
using CalamityMod.Items.Tools;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items
{
	public partial class CalamityGlobalItem : GlobalItem
	{
		#region Main ModifyTooltips Function
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			// Apply rarity coloration to the item's name.
			TooltipLine nameLine = tooltips.FirstOrDefault(x => x.Name == "ItemName" && x.mod == "Terraria");
			if (nameLine != null)
				ApplyRarityColor(item, nameLine);

			// If the item is melee, add a true melee damage number adjacent to the standard damage number.
			if (item.melee && item.damage > 0 && Main.LocalPlayer.Calamity().trueMeleeDamage > 0D)
				TrueMeleeDamageTooltip(item, tooltips);

			// Modify all vanilla tooltips before appending mod mechanics (if any).
			ModifyVanillaTooltips(item, tooltips);

			// If the item has a stealth generation prefix, show that on the tooltip.
			// This is placed between vanilla tooltip edits and mod mechanics because it can apply to vanilla items.
			StealthGenAccessoryTooltip(item, tooltips);

			// Everything below this line can only apply to modded items. If the item is vanilla, stop here for efficiency.
			if (item.type < ItemID.Count)
				return;

			// Adds tooltips to Calamity fountains which match Fargo's fountain tooltips.
			FargoFountainTooltip(item, tooltips);

			// Adds a Current Charge tooltip to all items which use charge.
			CalamityGlobalItem modItem = item.Calamity();
			if (modItem?.UsesCharge ?? false)
			{
				// Convert current charge ratio into a percentage.
				float displayedPercent = ChargeRatio * 100f;
				TooltipLine line = new TooltipLine(mod, "CalamityCharge", $"Current Charge: {displayedPercent:N1}%");
				tooltips.Add(line);
			}

			// Adds "Donor Item" and "Developer Item" to donor items and developer items respectively.
			if (donorItem)
			{
				TooltipLine line = new TooltipLine(mod, "CalamityDonor", CalamityUtils.ColorMessage("- Donor Item -", CalamityUtils.DonatorItemColor));
				tooltips.Add(line);
			}
			if (devItem)
			{
				TooltipLine line = new TooltipLine(mod, "CalamityDev", CalamityUtils.ColorMessage("- Developer Item -", CalamityUtils.HotPinkRarityColor));
				tooltips.Add(line);
			}

			// Adds "Challenge Drop" or "Legendary Challenge Drop" to Malice Mode drops.
			// For Legendary Challenge Drops, this tooltip matches their unique rarity color.
			if (challengeDrop)
				ChallengeDropTooltip(item, tooltips);
		}
		#endregion

		#region Rarity Coloration
		private void ApplyRarityColor(Item item, TooltipLine nameLine)
		{
			// Apply standard post-ML rarities to the item's color first.
			Color? standardRarityColor = CalamityUtils.GetRarityColor(customRarity);
			if (standardRarityColor.HasValue)
				nameLine.overrideColor = standardRarityColor.Value;

			#region Uniquely Colored Developer Items
			if (item.type == ModContent.ItemType<Fabstaff>())
				nameLine.overrideColor = new Color(Main.DiscoR, 100, 255);
			if (item.type == ModContent.ItemType<BlushieStaff>())
				nameLine.overrideColor = new Color(0, 0, 255);
			if (item.type == ModContent.ItemType<Judgement>())
				nameLine.overrideColor = Judgement.GetSyncedLightColor();
			if (item.type == ModContent.ItemType<NanoblackReaperRogue>())
				nameLine.overrideColor = new Color(0.34f, 0.34f + 0.66f * Main.DiscoG / 255f, 0.34f + 0.5f * Main.DiscoG / 255f);
			if (item.type == ModContent.ItemType<ShatteredCommunity>())
				nameLine.overrideColor = ShatteredCommunity.GetRarityColor();
			if (item.type == ModContent.ItemType<ProfanedSoulCrystal>())
				nameLine.overrideColor = CalamityUtils.ColorSwap(new Color(255, 166, 0), new Color(25, 250, 25), 4f); //alternates between emerald green and amber (BanditHueh)
			if (item.type == ModContent.ItemType<BensUmbrella>())
				nameLine.overrideColor = CalamityUtils.ColorSwap(new Color(210, 0, 255), new Color(255, 248, 24), 4f);
			if (item.type == ModContent.ItemType<Endogenesis>())
				nameLine.overrideColor = CalamityUtils.ColorSwap(new Color(131, 239, 255), new Color(36, 55, 230), 4f);
			if (item.type == ModContent.ItemType<DraconicDestruction>())
				nameLine.overrideColor = CalamityUtils.ColorSwap(new Color(255, 69, 0), new Color(139, 0, 0), 4f);
			if (item.type == ModContent.ItemType<ScarletDevil>())
				nameLine.overrideColor = CalamityUtils.ColorSwap(new Color(191, 45, 71), new Color(185, 187, 253), 4f);
			if (item.type == ModContent.ItemType<RedSun>())
				nameLine.overrideColor = CalamityUtils.ColorSwap(new Color(204, 86, 80), new Color(237, 69, 141), 4f);
			if (item.type == ModContent.ItemType<GaelsGreatsword>())
				nameLine.overrideColor = new Color(146, 0, 0);
			if (item.type == ModContent.ItemType<CrystylCrusher>())
				nameLine.overrideColor = new Color(129, 29, 149);
			if (item.type == ModContent.ItemType<Svantechnical>())
				nameLine.overrideColor = new Color(220, 20, 60);
			if (item.type == ModContent.ItemType<SomaPrime>())
				nameLine.overrideColor = new Color(254, 253, 235);
			if (item.type == ModContent.ItemType<Contagion>())
				nameLine.overrideColor = new Color(207, 17, 117);
			if (item.type == ModContent.ItemType<TriactisTruePaladinianMageHammerofMightMelee>())
				nameLine.overrideColor = new Color(227, 226, 180);
			if (item.type == ModContent.ItemType<RoyalKnivesMelee>())
				nameLine.overrideColor = CalamityUtils.ColorSwap(new Color(154, 255, 151), new Color(228, 151, 255), 4f);
			if (item.type == ModContent.ItemType<DemonshadeHelm>() || item.type == ModContent.ItemType<DemonshadeBreastplate>() || item.type == ModContent.ItemType<DemonshadeGreaves>())
				nameLine.overrideColor = CalamityUtils.ColorSwap(new Color(255, 132, 22), new Color(221, 85, 7), 4f);

			// TODO -- for cleanliness, ALL color math should either be a one-line color swap or inside the item's own file
			// The items that currently violate this are all below:
			// Eternity, Flamsteed Ring, Earth
			if (item.type == ModContent.ItemType<Eternity>())
			{
				List<Color> colorSet = new List<Color>()
					{
						new Color(188, 192, 193), // white
						new Color(157, 100, 183), // purple
						new Color(249, 166, 77), // honey-ish orange
						new Color(255, 105, 234), // pink
						new Color(67, 204, 219), // sky blue
						new Color(249, 245, 99), // bright yellow
						new Color(236, 168, 247), // purplish pink
					};
				if (nameLine != null)
				{
					int colorIndex = (int)(Main.GlobalTime / 2 % colorSet.Count);
					Color currentColor = colorSet[colorIndex];
					Color nextColor = colorSet[(colorIndex + 1) % colorSet.Count];
					nameLine.overrideColor = Color.Lerp(currentColor, nextColor, Main.GlobalTime % 2f > 1f ? 1f : Main.GlobalTime % 1f);
				}
			}
			if (item.type == ModContent.ItemType<PrototypeAndromechaRing>())
			{
				if (Main.GlobalTime % 1f < 0.6f)
				{
					nameLine.overrideColor = new Color(89, 229, 255);
				}
				else if (Main.GlobalTime % 1f < 0.8f)
				{
					nameLine.overrideColor = Color.Lerp(new Color(89, 229, 255), Color.White, (Main.GlobalTime % 1f - 0.6f) / 0.2f);
				}
				else
				{
					nameLine.overrideColor = Color.Lerp(Color.White, new Color(89, 229, 255), (Main.GlobalTime % 1f - 0.8f) / 0.2f);
				}
			}
			if (item.type == ModContent.ItemType<Earth>())
			{
				List<Color> earthColors = new List<Color>()
							{
								new Color(255, 99, 146),
								new Color(255, 228, 94),
								new Color(127, 200, 248)
							};
				if (nameLine != null)
				{
					int colorIndex = (int)(Main.GlobalTime / 2 % earthColors.Count);
					Color currentColor = earthColors[colorIndex];
					Color nextColor = earthColors[(colorIndex + 1) % earthColors.Count];
					nameLine.overrideColor = Color.Lerp(currentColor, nextColor, Main.GlobalTime % 2f > 1f ? 1f : Main.GlobalTime % 1f);
				}
			}
			#endregion
		}
		#endregion

		#region Challenge Drop Tooltip
		private void ChallengeDropTooltip(Item item, IList<TooltipLine> tooltips)
		{
			Color? legendaryColor = null;
			if (item.type == ModContent.ItemType<AegisBlade>() || item.type == ModContent.ItemType<YharimsCrystal>())
				legendaryColor = new Color(255, Main.DiscoG, 53);
			if (item.type == ModContent.ItemType<BlossomFlux>() || item.type == ModContent.ItemType<Malachite>())
				legendaryColor = new Color(Main.DiscoR, 203, 103);
			if (item.type == ModContent.ItemType<BrinyBaron>() || item.type == ModContent.ItemType<ColdDivinity>())
				legendaryColor = new Color(53, Main.DiscoG, 255);
			if (item.type == ModContent.ItemType<CosmicDischarge>())
				legendaryColor = new Color(150, Main.DiscoG, 255);
			if (item.type == ModContent.ItemType<SeasSearing>())
				legendaryColor = new Color(60, Main.DiscoG, 190);
			if (item.type == ModContent.ItemType<SHPC>())
				legendaryColor = new Color(255, Main.DiscoG, 155);
			if (item.type == ModContent.ItemType<Vesuvius>() || item.type == ModContent.ItemType<GoldBurdenBreaker>())
				legendaryColor = new Color(255, Main.DiscoG, 0);
			if (item.type == ModContent.ItemType<PristineFury>())
				legendaryColor = CalamityUtils.ColorSwap(new Color(255, 168, 53), new Color(255, 249, 0), 2f);
			if (item.type == ModContent.ItemType<LeonidProgenitor>())
				legendaryColor = CalamityUtils.ColorSwap(LeonidProgenitor.blueColor, LeonidProgenitor.purpleColor, 3f);
			if (item.type == ModContent.ItemType<TheCommunity>())
				legendaryColor = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB);

			Color lineColor = legendaryColor.GetValueOrDefault(CalamityUtils.ChallengeDropColor);
			string text = legendaryColor.HasValue ? "- Legendary Challenge Drop -" : "- Challenge Drop";
			TooltipLine line = new TooltipLine(mod, "CalamityChallengeDrop", text)
			{
				overrideColor = lineColor
			};
			tooltips.Add(line);
		}
		#endregion

		#region Vanilla Item Tooltip Modification
		private void ModifyVanillaTooltips(Item item, IList<TooltipLine> tooltips)
		{
			#region Modular Tooltip Editing Code
			// This is a modular tooltip editor which loops over all tooltip lines of an item,
			// selects all those which match an arbitrary function you provide,
			// then edits them using another arbitrary function you provide.
			void ApplyTooltipEdits(IList<TooltipLine> lines, Func<Item, TooltipLine, bool> predicate, Action<TooltipLine> action)
			{
				foreach (TooltipLine line in lines)
					if (predicate.Invoke(item, line))
						action.Invoke(line);
			}

			// This function produces simple predicates to match a specific line of a tooltip, by number/index.
			Func<Item, TooltipLine, bool> LineNum(int n) => (Item i, TooltipLine l) => l.mod == "Terraria" && l.Name == $"Tooltip{n}";
			// This function produces simple predicates to match a specific line of a tooltip, by name.
			Func<Item, TooltipLine, bool> LineName(string s) => (Item i, TooltipLine l) => l.mod == "Terraria" && l.Name == s;

			// These functions are shorthand to invoke ApplyTooltipEdits using the above predicates.
			void EditTooltipByNum(int lineNum, Action<TooltipLine> action) => ApplyTooltipEdits(tooltips, LineNum(lineNum), action);
			void EditTooltipByName(string lineName, Action<TooltipLine> action) => ApplyTooltipEdits(tooltips, LineName(lineName), action);
			#endregion

			// Numerous random tooltip edits which don't fit into another category
			#region Various Tooltip Edits

			// Mirrors and Recall Potions cannot be used while a boss is alive.
			if (item.type == ItemID.MagicMirror || item.type == ItemID.IceMirror || item.type == ItemID.CellPhone || item.type == ItemID.RecallPotion)
				ApplyTooltipEdits(tooltips,
					(i, l) => l.mod == "Terraria" && l.Name == (i.type == ItemID.CellPhone ? "Tooltip1" : "Tooltip0"),
					(line) => line.text += "\nCannot be used while a boss is alive");

			// Rod of Discord cannot be used multiple times to hurt yourself
			if (item.type == ItemID.RodofDiscord)
				EditTooltipByNum(0, (line) => line.text += "\nTeleportation is disabled while Chaos State is active");

			// Water removing items cannot be used in the Abyss
			string noAbyssLine = "\nCannot be used in the Abyss";
			if (item.type == ItemID.SuperAbsorbantSponge)
				EditTooltipByNum(0, (line) => line.text += noAbyssLine);
			if (item.type == ItemID.EmptyBucket)
				EditTooltipByName("Defense", (line) => line.text += noAbyssLine);

			// If Early Hardmode Rework is enabled: Remind users that ores will NOT spawn when an altar is smashed.
			if (CalamityConfig.Instance.EarlyHardmodeProgressionRework && (item.type == ItemID.Pwnhammer || item.type == ItemID.Hammush))
				EditTooltipByNum(0, (line) => line.text += "\nDemon Altars no longer spawn ores when destroyed");

			// Bottled Honey gives the Honey buff
			if (item.type == ItemID.BottledHoney)
				EditTooltipByName("HealLife", (line) => line.text += "\nGrants the Honey buff for 2 minutes");

			// Warmth Potion provides debuff immunities and Death Mode cold protection
			if (item.type == ItemID.WarmthPotion)
			{
				string immunityLine = "\nMakes you immune to the Chilled, Frozen, and Glacial State debuffs";
				if (CalamityWorld.death)
					immunityLine += "\nProvides cold protection in Death Mode";
				EditTooltipByNum(0, (line) => line.text += immunityLine);
			}

			// Hand Warmer provides Death Mode cold protection and has a side bonus with Eskimo armor
			if (item.type == ItemID.HandWarmer)
			{
				string extraLine = "\nProvides a regeneration boost while wearing the Eskimo armor";
				if (CalamityWorld.death)
					extraLine += "\nProvides cold protection in Death Mode";
				EditTooltipByNum(0, (line) => line.text += extraLine);
			}

			// Invisibility Potion provides various rogue boosts
			if (item.type == ItemID.InvisibilityPotion)
				EditTooltipByNum(0, (line) => line.text += "\nBoosts various rogue stats depending on held weapon");

			// Golden Fishing Rod inherently contains High Test Fishing Line
			if (item.type == ItemID.GoldenFishingRod)
				EditTooltipByName("NeedsBait", (line) => line.text += "\nIts fishing line will never break");

			// Eternity Crystal notifies the player that they can accelerate the invasion
			if (item.type == ItemID.DD2ElderCrystal)
				EditTooltipByNum(0, (line) => line.text += "\nOnce placed you can right click the crystal to skip waves or increase the spawn rate of the invaders");

			// Fix a vanilla mistake in Magic Quiver's tooltip
			// TODO -- in 1.4 this mistake is already corrected
			if (item.type == ItemID.MagicQuiver)
				EditTooltipByNum(0, (line) => line.text = line.text.Replace(" damage", " arrow damage"));
			#endregion

			// Black Belt and Master Ninja Gear have guaranteed dodges on a 60 second cooldown.
			#region Dodging Belt Tooltips
			string beltDodgeLine = "Grants the ability to dodge attacks\n" +
				$"The dodge has a {CalamityPlayer.BeltDodgeCooldown} second cooldown which is shared with all other dodges and reflects";
			if (item.type == ItemID.BlackBelt)
				EditTooltipByNum(0, (line) => line.text = beltDodgeLine);
			if (item.type == ItemID.MasterNinjaGear)
				EditTooltipByNum(1, (line) => line.text = beltDodgeLine);
			#endregion

			// Early Hardmode ore melee weapons have new on-hit effects.
			#region Early Hardmode Melee Tooltips

			// Cobalt
			if (item.type == ItemID.CobaltSword || item.type == ItemID.CobaltNaginata)
				EditTooltipByName("Knockback", (line) => line.text += "\nDecreases enemy defense by 10% on hit");

			// Palladium
			if (item.type == ItemID.PalladiumSword || item.type == ItemID.PalladiumPike)
				EditTooltipByName("Knockback", (line) => line.text += "\nIncreases life regen on hit");

			// Mythril
			if (item.type == ItemID.MythrilSword || item.type == ItemID.MythrilHalberd)
				EditTooltipByName("Knockback", (line) => line.text += "\nDecreases enemy contact damage by 10% on hit");

			// Orichalcum
			if (item.type == ItemID.OrichalcumSword || item.type == ItemID.OrichalcumHalberd)
				EditTooltipByName("Knockback", (line) => line.text += "\nIncreases how frequently the Orichalcum set bonus triggers on hit");

			// Adamantite
			if (item.type == ItemID.AdamantiteSword || item.type == ItemID.AdamantiteGlaive)
				EditTooltipByName("Knockback", (line) => line.text += "\nSlows enemies on hit");

			// Titanium
			if (item.type == ItemID.TitaniumSword || item.type == ItemID.TitaniumTrident)
				EditTooltipByName("Knockback", (line) => line.text += "\nDeals increased damage to enemies with high knockback resistance");

			// Hallowed (and True Excalibur)
			if (item.type == ItemID.Excalibur || item.type == ItemID.Gungnir || item.type == ItemID.TrueExcalibur)
				EditTooltipByName("Knockback", (line) => line.text += "\nInflicts Holy Flames\nDeals double damage to enemies above 75% life");
			#endregion

			// Other melee weapon tooltips
			#region Other Melee Tooltips

			if (item.type == ItemID.CandyCaneSword || item.type == ItemID.FruitcakeChakram)
				EditTooltipByName("Knockback", (line) => line.text += "\nHeals you on hit");

			// Stylish Scissors, all Phaseblades, and all Phasesabers
			if (item.type == ItemID.StylistKilLaKillScissorsIWish || (item.type >= ItemID.BluePhaseblade && item.type <= ItemID.YellowPhaseblade) || (item.type >= ItemID.BluePhasesaber && item.type <= ItemID.YellowPhasesaber))
				EditTooltipByName("Knockback", (line) => line.text += "\nIgnores 100% of enemy defense");

			if (item.type == ItemID.AntlionClaw || item.type == ItemID.BoneSword || item.type == ItemID.BreakerBlade)
				EditTooltipByName("Knockback", (line) => line.text += "\nIgnores 50% of enemy defense");

			if (item.type == ItemID.LightsBane || item.type == ItemID.NightsEdge || item.type == ItemID.TrueNightsEdge)
				EditTooltipByName("Knockback", (line) => line.text += "\nInflicts Shadowflame on hit");

			if (item.type == ItemID.BloodButcherer || item.type == ItemID.TheRottedFork)
				EditTooltipByName("Knockback", (line) => line.text += "\nInflicts Burning Blood on hit");
			#endregion

			// Light pets, accessories, and other items which boost the player's Abyss light stat
			#region Abyss Light Tooltips

			// +1 to Abyss light level
			string abyssSmallLightLine = "\nProvides a small amount of light in the abyss";

			if (item.type == ItemID.CrimsonHeart || item.type == ItemID.ShadowOrb || item.type == ItemID.MagicLantern || item.type == ItemID.JellyfishNecklace || item.type == ItemID.MiningHelmet)
				EditTooltipByNum(0, (line) => line.text += abyssSmallLightLine);
			if (item.type == ItemID.JellyfishDivingGear)
				EditTooltipByNum(1, (line) => line.text += abyssSmallLightLine);

			// +2 to Abyss light level
			string abyssMediumLightLine = "\nProvides a moderate amount of light in the abyss";

			if (item.type == ItemID.ShinePotion)
				EditTooltipByName("BuffTime", (line) => line.text += abyssMediumLightLine);

			if (item.type == ItemID.FairyBell || item.type == ItemID.DD2PetGhost)
				EditTooltipByNum(0, (line) => line.text += abyssMediumLightLine);

			// +3 to Abyss light level
			string abyssLargeLightLine = "\nProvides a large amount of light in the abyss";

			if (item.type == ItemID.WispinaBottle)
				EditTooltipByNum(0, (line) => line.text += abyssLargeLightLine);
			if (item.type == ItemID.SuspiciousLookingTentacle)
				EditTooltipByNum(1, (line) => line.text += abyssLargeLightLine);
			#endregion

			// Accessories and other items which boost the player's ability to breathe in the Abyss
			#region Abyss Breath Tooltips

			// Moderate breath boost
			string abyssModerateBreathLine = "\nModerately reduces breath loss in the abyss";

			if (item.type == ItemID.DivingHelmet)
				EditTooltipByNum(0, (line) => line.text += abyssModerateBreathLine);
			if (item.type == ItemID.ArcticDivingGear)
				EditTooltipByNum(1, (line) => line.text += abyssSmallLightLine + abyssModerateBreathLine);

			// Great breath boost
			string abyssGreatBreathLine = "\nGreatly reduces breath loss in the abyss";

			if (item.type == ItemID.GillsPotion)
				EditTooltipByName("BuffTime", (line) => line.text += abyssGreatBreathLine);

			if (item.type == ItemID.NeptunesShell || item.type == ItemID.MoonShell)
				EditTooltipByNum(0, (line) => line.text += abyssGreatBreathLine);
			if (item.type == ItemID.CelestialShell)
				EditTooltipByNum(1, (line) => line.text += abyssModerateBreathLine);
			#endregion

			// Flasks apply to Rogue weapons
			#region Rogue Flask Tooltips
			if (item.type == ItemID.FlaskofCursedFlames)
				EditTooltipByNum(0, (line) => line.text += "\nRogue attacks inflict enemies with cursed flames");
			if (item.type == ItemID.FlaskofFire)
				EditTooltipByNum(0, (line) => line.text += "\nRogue attacks set enemies on fire");
			if (item.type == ItemID.FlaskofGold)
				EditTooltipByNum(0, (line) => line.text += "\nRogue attacks make enemies drop more gold");
			if (item.type == ItemID.FlaskofIchor)
				EditTooltipByNum(0, (line) => line.text += "\nRogue attacks decrease enemy defense");
			if (item.type == ItemID.FlaskofNanites)
				EditTooltipByNum(0, (line) => line.text += "\nRogue attacks confuse enemies");
			// party flask is unique because it affects ALL projectiles in Calamity, not just "also rogue ones"
			if (item.type == ItemID.FlaskofParty)
				EditTooltipByNum(0, (line) => line.text = "All attacks cause confetti to appear");
			if (item.type == ItemID.FlaskofPoison)
				EditTooltipByNum(0, (line) => line.text += "\nRogue attacks poison enemies");
			if (item.type == ItemID.FlaskofVenom)
				EditTooltipByNum(0, (line) => line.text += "\nRogue attacks inflict Venom on enemies");
			#endregion

			// Rebalances to vanilla item stats
			#region Vanilla Item Rebalance Tooltips

			// Worm Scarf only gives 10% DR instead of 17%
			if (item.type == ItemID.WormScarf)
				EditTooltipByNum(0, (line) => line.text = line.text = line.text.Replace("17%", "10%"));

			if (item.type == ItemID.TitanGlove)
				EditTooltipByNum(0, (line) => line.text += "\n10% increased true melee damage");
			if (item.type == ItemID.PowerGlove || item.type == ItemID.MechanicalGlove)
				EditTooltipByNum(1, (line) => line.text += "\n10% increased true melee damage");
			if (item.type == ItemID.FireGauntlet)
			{
				string extraLine = "\n10% increased true melee damage";
				if (CalamityWorld.death)
					extraLine += "\nProvides heat and cold protection in Death Mode";
				EditTooltipByNum(1, (line) => line.text = line.text.Replace("12%", "14%") + extraLine);
			}

			// Spectre Hood's lifesteal is heavily nerfed, so it only reduces magic damage by 20% instead of 40%
			if (item.type == ItemID.SpectreHood)
				EditTooltipByNum(0, (line) => line.text = line.text.Replace("40%", "20%"));
			#endregion

			// Items which provide immunity to either heat or cold in Death Mode, or interact with Lethal Lava
			#region Lava and Death Mode Environmental Immunity Tooltips
			string heatProtectionLine = "\nProvides heat protection in Death Mode";
			string bothProtectionLine = "\nProvides heat and cold protection in Death Mode";

			if (item.type == ItemID.ObsidianSkinPotion)
			{
				// If Lethal Lava is enabled, Obsidian Skin Potions work very differently.
				if (CalamityConfig.Instance.LethalLava)
				{
					string lethalLavaReplacement = "Provides immunity to direct damage from touching lava\n"
						+ "Provides temporary immunity to lava burn damage\n"
						+ "Greatly increases the speed at which lava immunity recharges\n"
						+ "Reduces lava burn damage";
					if (CalamityWorld.death)
						lethalLavaReplacement += heatProtectionLine;
					EditTooltipByNum(0, (line) => line.text = lethalLavaReplacement);
				}

				// Otherwise, changes are only made on Death Mode, where heat immunity is mentioned.
				else if (CalamityWorld.death)
					EditTooltipByNum(0, (line) => line.text += heatProtectionLine);
			}

			if (item.type == ItemID.ObsidianRose)
			{
				StringBuilder sb = new StringBuilder(128);
				// If Lethal Lava is enabled, Obsidian Rose reduces the damage from the debuff.
				if (CalamityConfig.Instance.LethalLava)
					sb.Append("\nGreatly reduces lava burn damage");
				if (CalamityWorld.death)
					sb.Append(heatProtectionLine);
				EditTooltipByNum(0, (line) => line.text += sb.ToString());
			}

			if (CalamityWorld.death)
			{
				if (item.type == ItemID.MagmaStone)
					EditTooltipByNum(0, (line) => line.text += bothProtectionLine);
				if (item.type == ItemID.LavaCharm)
					EditTooltipByNum(0, (line) => line.text += heatProtectionLine);
				if (item.type == ItemID.LavaWaders)
					EditTooltipByNum(1, (line) => line.text += heatProtectionLine);
				if (item.type == ItemID.FrostHelmet || item.type == ItemID.FrostBreastplate || item.type == ItemID.FrostLeggings)
					EditTooltipByName("SetBonus", (line) => line.text += bothProtectionLine);
			}
			#endregion

			// Add mentions of what Calamity ores vanilla pickaxes can mine
			#region Pickaxe New Ore Tooltips
			if (item.type == ItemID.Picksaw)
				EditTooltipByNum(0, (line) => line.text += "\nCan mine Scoria Ore");

			if (item.type == ItemID.SolarFlarePickaxe || item.type == ItemID.VortexPickaxe || item.type == ItemID.NebulaPickaxe || item.type == ItemID.StardustPickaxe)
				EditTooltipByName("Material", (line) => line.text += "\nCan mine Uelibloom Ore");
			#endregion

			// Rebalances and information about vanilla set bonuses
			#region Vanilla Set Bonus Tooltips

			string MiningSpeedString(int percent) => $"\n{percent}% increased mining speed";

			// Copper
			if (item.type == ItemID.CopperHelmet || item.type == ItemID.CopperChainmail || item.type == ItemID.CopperGreaves)
				EditTooltipByName("SetBonus", (line) => line.text += MiningSpeedString(15));

			// Tin
			if (item.type == ItemID.TinHelmet || item.type == ItemID.TinChainmail || item.type == ItemID.TinGreaves)
				EditTooltipByName("SetBonus", (line) => line.text += MiningSpeedString(10));

			// Iron
			if (item.type == ItemID.AncientIronHelmet || item.type == ItemID.IronHelmet || item.type == ItemID.IronChainmail || item.type == ItemID.IronGreaves)
				EditTooltipByName("SetBonus", (line) => line.text += MiningSpeedString(25));

			// Lead
			if (item.type == ItemID.LeadHelmet || item.type == ItemID.LeadChainmail || item.type == ItemID.LeadGreaves)
				EditTooltipByName("SetBonus", (line) => line.text += MiningSpeedString(20));

			// Silver
			if (item.type == ItemID.SilverHelmet || item.type == ItemID.SilverChainmail || item.type == ItemID.SilverGreaves)
				EditTooltipByName("SetBonus", (line) => line.text += MiningSpeedString(35));

			// Tungsten
			if (item.type == ItemID.TungstenHelmet || item.type == ItemID.TungstenChainmail || item.type == ItemID.TungstenGreaves)
				EditTooltipByName("SetBonus", (line) => line.text += MiningSpeedString(30));

			// Gold
			if (item.type == ItemID.AncientGoldHelmet || item.type == ItemID.GoldHelmet || item.type == ItemID.GoldChainmail || item.type == ItemID.GoldGreaves)
				EditTooltipByName("SetBonus", (line) => line.text += MiningSpeedString(45));

			// Platinum
			if (item.type == ItemID.PlatinumHelmet || item.type == ItemID.PlatinumChainmail || item.type == ItemID.PlatinumGreaves)
				EditTooltipByName("SetBonus", (line) => line.text += MiningSpeedString(40));

			// Gladiator
			if (item.type == ItemID.GladiatorHelmet)
				EditTooltipByName("Defense", (line) => line.text += "\n3% increased rogue damage");
			if (item.type == ItemID.GladiatorBreastplate)
				EditTooltipByName("Defense", (line) => line.text += "\n3% increased rogue critical strike chance");
			if (item.type == ItemID.GladiatorLeggings)
				EditTooltipByName("Defense", (line) => line.text += "\n3% increased rogue velocity");

			// Obsidian
			if (item.type == ItemID.ObsidianHelm)
				EditTooltipByName("Defense", (line) => line.text += "\n3% increased rogue damage");
			if (item.type == ItemID.ObsidianShirt)
				EditTooltipByName("Defense", (line) => line.text += "\n3% increased rogue critical strike chance");
			if (item.type == ItemID.ObsidianPants)
				EditTooltipByName("Defense", (line) => line.text += "\n3% increased rogue velocity");

			// Meteor Armor only makes space gun cost 50% instead of zero mana
			if (item.type == ItemID.MeteorHelmet || item.type == ItemID.MeteorSuit || item.type == ItemID.MeteorLeggings)
				EditTooltipByName("SetBonus", (line) => line.text = line.text.Replace("0", "50%"));

			// Molten
			if (item.type == ItemID.MoltenHelmet || item.type == ItemID.MoltenBreastplate || item.type == ItemID.MoltenGreaves)
			{
				string extraLine = "\n20% extra true melee damage\nGrants immunity to fire blocks and temporary immunity to lava";
				if (CalamityWorld.death)
					extraLine += bothProtectionLine;
				EditTooltipByName("SetBonus", (line) => line.text += extraLine);
			}

			// Forbidden (UNLESS you are wearing the Circlet, which is Summon/Rogue and does not get this line)
			if (item.type == ItemID.AncientBattleArmorHat || item.type == ItemID.AncientBattleArmorShirt || item.type == ItemID.AncientBattleArmorPants
				&& !Main.LocalPlayer.Calamity().forbiddenCirclet)
				EditTooltipByName("SetBonus", (line) => line.text += "\nThe minion damage nerf is reduced while wielding magic weapons");

			// Stardust
			// 1) Chest and Legs only give 1 minion slot each instead of 2 each.
			if (item.type == ItemID.StardustBreastplate || item.type == ItemID.StardustLeggings)
				EditTooltipByNum(0, (line) => line.text = line.text.Replace('2', '1'));
			// 2) Set bonus gives 2 minions instead of 0.
			if (item.type == ItemID.StardustHelmet || item.type == ItemID.StardustBreastplate || item.type == ItemID.StardustLeggings)
				EditTooltipByName("SetBonus", (line) => line.text += "\nIncreases your max number of minions by 2");
			#endregion

			// Provide the full, raw stats of every vanilla set of wings
			#region Wing Stat Tooltips

			// This function produces a "stat sheet" for a pair of wings from the raw stats.
			// For "vertical speed", 0 = Average, 1 = Good, 2 = Great.
			string[] vertSpeedStrings = new string[] { "Average vertical speed", "Good vertical speed", "Great vertical speed" };
			string WingStatsTooltip(float hSpeed, float accelMult, int vertSpeed, int flightTime, string extraTooltip = null)
			{
				StringBuilder sb = new StringBuilder(512);
				sb.Append('\n');
				sb.Append($"Horizontal speed: {hSpeed:N2}\n");
				sb.Append($"Acceleration multiplier: {accelMult:N1}\n");
				sb.Append(vertSpeedStrings[vertSpeed]);
				sb.Append('\n');
				sb.Append($"Flight time: {flightTime}");
				if (extraTooltip != null)
				{
					sb.Append('\n');
					sb.Append(extraTooltip);
				}
				return sb.ToString();
			}

			if (item.type == ItemID.AngelWings)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(6.25f, 1f, 0, 100,
					"+20 max life, +10 defense and +2 life regen"));

			if (item.type == ItemID.DemonWings)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(6.25f, 1f, 0, 100,
					"5% increased damage and critical strike chance"));

			if (item.type == ItemID.Jetpack)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(6.5f, 1f, 0, 115));

			if (item.type == ItemID.ButterflyWings)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(6.75f, 1f, 0, 130,
					"+20 max mana, 5% decreased mana usage,\n"
					+ "5% increased magic damage and magic critical strike chance"));

			if (item.type == ItemID.FairyWings)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(6.75f, 1f, 0, 130, "+60 max life"));

			if (item.type == ItemID.BeeWings)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(6.75f, 1f, 0, 130, "Permanently gives the Honey buff"));

			if (item.type == ItemID.HarpyWings)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(7f, 1f, 0, 140,
					"20% increased movement speed\n" +
					"Most attacks have a chance to fire a feather on swing if Harpy Ring or Angel Treads are equipped"));

			if (item.type == ItemID.BoneWings)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(7f, 1f, 0, 140,
					"10% increased movement speed, ranged damage and critical strike chance\n" +
					"and +30 defense while wearing the Necro Armor"));

			if (item.type == ItemID.FlameWings)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(7.5f, 1f, 0, 160,
					"5% increased melee damage and critical strike chance"));

			if (item.type == ItemID.FrozenWings)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(7.5f, 1f, 0, 160,
					"2% increased melee and ranged damage\n" +
					"and 1% increased melee and ranged critical strike chance\n" +
					"while wearing the Frost Armor"));

			if (item.type == ItemID.GhostWings)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(7.5f, 1f, 0, 160,
					"+10 defense and 5% increased damage reduction while wearing the Spectre Hood set\n" +
					"5% increased magic damage and critical strike chance while wearing the Spectre Mask set"));

			if (item.type == ItemID.BeetleWings)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(7.5f, 1f, 0, 160,
					"+10 defense and 5% increased damage reduction while wearing the Beetle Shell set\n" +
					"5% increased melee damage and critical strike chance while wearing the Beetle Scale Mail set"));

			if (item.type == ItemID.FinWings)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(0f, 0f, 0, 100,
					"Gills effect and you can move freely through liquids\n" +
					"You fall faster while submerged in liquid\n" +
					"15% increased movement speed and 18% increased jump speed"));

			if (item.type == ItemID.FishronWings)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(8f, 2f, 1, 180));

			if (item.type == ItemID.SteampunkWings)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(7.75f, 1f, 0, 180,
					"+8 defense, 10% increased movement speed,\n" +
					"4% increased damage, and 2% increased critical strike chance"));

			if (item.type == ItemID.LeafWings)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(6.75f, 1f, 0, 160,
					"+5 defense, 5% increased damage reduction,\n" +
					"and permanent Dryad's Blessing while wearing the Tiki Armor"));

			if (item.type == ItemID.BatWings)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(0f, 0f, 0, 140,
					"At night or during an eclipse, you will gain the following boosts:\n"
					+ "10% increased movement speed, 10% increased jump speed,\n"
					+ "7% increased damage and 3% increased critical strike chance"));

			// All developer wings have identical stats and no special effects
			if (item.type == ItemID.Yoraiz0rWings || item.type == ItemID.JimsWings || item.type == ItemID.SkiphsWings ||
				item.type == ItemID.LokisWings || item.type == ItemID.ArkhalisWings || item.type == ItemID.LeinforsWings ||
				item.type == ItemID.BejeweledValkyrieWing || item.type == ItemID.RedsWings || item.type == ItemID.DTownsWings ||
				item.type == ItemID.WillsWings || item.type == ItemID.CrownosWings || item.type == ItemID.CenxsWings)
			{
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(7f, 1f, 0, 150));
			}

			if (item.type == ItemID.TatteredFairyWings)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(7.5f, 1f, 0, 180,
					"5% increased damage and critical strike chance"));

			if (item.type == ItemID.SpookyWings)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(7.5f, 1f, 0, 180,
					"Increased minion knockback and 5% increased minion damage while wearing the Spooky Armor"));

			if (item.type == ItemID.Hoverboard)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(6.25f, 1f, 0, 170,
					"10% increased weapon-type damage while wearing the Shroomite Armor\n" +
					"The weapon type boosted matches which Shroomite helmet is worn"));

			if (item.type == ItemID.FestiveWings)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(7.5f, 1f, 0, 170,
					"+40 max life\nOrnaments rain down as you fly"));

			if (item.type == ItemID.MothronWings)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(0f, 0f, 0, 160,
					"+5 defense, 5% increased damage,\n" +
					"10% increased movement speed and 12% increased jump speed"));

			if (item.type == ItemID.WingsSolar)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(9f, 2.5f, 2, 180,
					"7% increased melee damage and 3% increased melee critical strike chance\n" +
					"while wearing the Solar Flare Armor"));

			if (item.type == ItemID.WingsStardust)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(9f, 2.5f, 2, 180,
					"+1 max minion and 5% increased minion damage while wearing the Stardust Armor"));

			if (item.type == ItemID.WingsVortex)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(6.5f, 1.5f, 1, 160,
					"3% increased ranged damage and 7% increased ranged critical strike chance\n" +
					"while wearing the Vortex Armor"));

			if (item.type == ItemID.WingsNebula)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(6.5f, 1.5f, 1, 160,
					"+20 max mana, 5% increased magic damage and critical strike chance,\n" +
					"and 5% decreased mana usage while wearing the Nebula Armor"));

			if (item.type == ItemID.BetsyWings)
				EditTooltipByNum(0, (line) => line.text += WingStatsTooltip(6f, 2.5f, 1, 150));
			#endregion

			#region Grappling Hook Stat Tooltips
			if (item.type == ItemID.GrapplingHook)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 18.75\n" +
							"Launch Velocity: 11.5\n" +
							"Pull Velocity: 11";
					}
				}
			}
			if (item.type == ItemID.AmethystHook)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 18.75\n" +
							"Launch Velocity: 10\n" +
							"Pull Velocity: 11";
					}
				}
			}
			if (item.type == ItemID.TopazHook)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 20.625\n" +
							"Launch Velocity: 10.5\n" +
							"Pull Velocity: 11.75";
					}
				}
			}
			if (item.type == ItemID.SapphireHook)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 22.5\n" +
							"Launch Velocity: 11\n" +
							"Pull Velocity: 12.5";
					}
				}
			}
			if (item.type == ItemID.EmeraldHook)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 24.375\n" +
							"Launch Velocity: 11.5\n" +
							"Pull Velocity: 13.25";
					}
				}
			}
			if (item.type == ItemID.RubyHook)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 26.25\n" +
							"Launch Velocity: 12\n" +
							"Pull Velocity: 14";
					}
				}
			}
			if (item.type == ItemID.DiamondHook)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 28.125\n" +
							"Launch Velocity: 12.5\n" +
							"Pull Velocity: 14.75";
					}
				}
			}
			if (item.type == ItemID.WebSlinger)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 15.625\n" +
							"Launch Velocity: 10\n" +
							"Pull Velocity: 11";
					}
				}
			}
			if (item.type == ItemID.SkeletronHand)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 21.875\n" +
							"Launch Velocity: 15\n" +
							"Pull Velocity: 11";
					}
				}
			}
			if (item.type == ItemID.SlimeHook)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 18.75\n" +
							"Launch Velocity: 13\n" +
							"Pull Velocity: 11";
					}
				}
			}
			if (item.type == ItemID.FishHook)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 25\n" +
							"Launch Velocity: 13\n" +
							"Pull Velocity: 11";
					}
				}
			}
			if (item.type == ItemID.IvyWhip)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 25\n" +
							"Launch Velocity: 13\n" +
							"Pull Velocity: 15";
					}
				}
			}
			if (item.type == ItemID.BatHook)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 31.25\n" +
							"Launch Velocity: 15.5\n" +
							"Pull Velocity: 20";
					}
				}
			}
			if (item.type == ItemID.CandyCaneHook)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 25\n" +
							"Launch Velocity: 11.5\n" +
							"Pull Velocity: 11";
					}
				}
			}
			if (item.type == ItemID.DualHook)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 27.5\n" +
							"Launch Velocity: 14\n" +
							"Pull Velocity: 17";
					}
				}
			}
			if (item.type == ItemID.ThornHook)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 30\n" +
							"Launch Velocity: 15\n" +
							"Pull Velocity: 18";
					}
				}
			}
			if (item.type == ItemID.WormHook)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 30\n" +
							"Launch Velocity: 15\n" +
							"Pull Velocity: 18";
					}
				}
			}
			if (item.type == ItemID.TendonHook)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 30\n" +
							"Launch Velocity: 15\n" +
							"Pull Velocity: 18";
					}
				}
			}
			if (item.type == ItemID.IlluminantHook)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 30\n" +
							"Launch Velocity: 15\n" +
							"Pull Velocity: 18";
					}
				}
			}
			if (item.type == ItemID.AntiGravityHook)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 31.25\n" +
							"Launch Velocity: 14\n" +
							"Pull Velocity: 20";
					}
				}
			}
			if (item.type == ItemID.SpookyHook)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 34.375\n" +
							"Launch Velocity: 15.5\n" +
							"Pull Velocity: 22";
					}
				}
			}
			if (item.type == ItemID.ChristmasHook)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 34.375\n" +
							"Launch Velocity: 15.5\n" +
							"Pull Velocity: 17";
					}
				}
			}
			if (item.type == ItemID.LunarHook)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 34.375\n" +
							"Launch Velocity: 16\n" +
							"Pull Velocity: 24";
					}
				}
			}
			if (item.type == ItemID.StaticHook)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 37.5\n" +
							"Launch Velocity: 16\n" +
							"Pull Velocity: 24";
					}
				}
			}
			if (item.type == ItemID.StaticHook)
			{
				foreach (TooltipLine line2 in tooltips)
				{
					if (line2.mod == "Terraria" && line2.Name == "Equipable")
					{
						line2.text = "Equipable\n" +
							"Reach: 37.5\n" +
							"Launch Velocity: 16\n" +
							"Pull Velocity: 24";
					}
				}
			}
			#endregion

			#region Accessory Prefix Rebalance Tooltips
			if (item.accessory)
			{
				if (item.prefix == PrefixID.Brisk)
				{
					foreach (TooltipLine line2 in tooltips)
					{
						if (line2.mod == "Terraria" && line2.Name == "PrefixAccMoveSpeed")
							line2.text = "+2% movement speed";
					}
				}
				if (item.prefix == PrefixID.Fleeting)
				{
					foreach (TooltipLine line2 in tooltips)
					{
						if (line2.mod == "Terraria" && line2.Name == "PrefixAccMoveSpeed")
							line2.text = "+4% movement speed";
					}
				}
				if (item.prefix == PrefixID.Hasty2)
				{
					foreach (TooltipLine line2 in tooltips)
					{
						if (line2.mod == "Terraria" && line2.Name == "PrefixAccMoveSpeed")
							line2.text = "+6% movement speed";
					}
				}
				if (item.prefix == PrefixID.Quick2)
				{
					foreach (TooltipLine line2 in tooltips)
					{
						if (line2.mod == "Terraria" && line2.Name == "PrefixAccMoveSpeed")
							line2.text = "+8% movement speed";
					}
				}
				if (item.prefix == PrefixID.Precise || item.prefix == PrefixID.Lucky)
				{
					foreach (TooltipLine line2 in tooltips)
					{
						if (line2.mod == "Terraria" && line2.Name == "PrefixAccCritChance")
							line2.text += "\n+1 armor penetration";
					}
				}
				if (item.prefix == PrefixID.Hard)
				{
					string defenseBoost = "+1 defense\n";
					if (CalamityWorld.downedDoG)
						defenseBoost = "+4 defense\n";
					else if (CalamityWorld.downedProvidence || CalamityWorld.downedPolterghast)
						defenseBoost = "+3 defense\n";
					else if (NPC.downedGolemBoss || NPC.downedMoonlord)
						defenseBoost = "+2 defense\n";

					foreach (TooltipLine line2 in tooltips)
					{
						if (line2.mod == "Terraria" && line2.Name == "PrefixAccDefense")
							line2.text = defenseBoost + "+0.25% damage reduction";
					}
				}
				if (item.prefix == PrefixID.Guarding)
				{
					string defenseBoost = "+2 defense\n";
					if (CalamityWorld.downedDoG)
						defenseBoost = "+8 defense\n";
					else if (CalamityWorld.downedPolterghast)
						defenseBoost = "+7 defense\n";
					else if (CalamityWorld.downedProvidence)
						defenseBoost = "+6 defense\n";
					else if (NPC.downedMoonlord)
						defenseBoost = "+5 defense\n";
					else if (NPC.downedGolemBoss)
						defenseBoost = "+4 defense\n";
					else if (Main.hardMode)
						defenseBoost = "+3 defense\n";

					foreach (TooltipLine line2 in tooltips)
					{
						if (line2.mod == "Terraria" && line2.Name == "PrefixAccDefense")
							line2.text = defenseBoost + "+0.5% damage reduction";
					}
				}
				if (item.prefix == PrefixID.Armored)
				{
					string defenseBoost = "+3 defense\n";
					if (CalamityWorld.downedDoG)
						defenseBoost = "+12 defense\n";
					else if (CalamityWorld.downedPolterghast)
						defenseBoost = "+10 defense\n";
					else if (CalamityWorld.downedProvidence)
						defenseBoost = "+9 defense\n";
					else if (NPC.downedMoonlord)
						defenseBoost = "+7 defense\n";
					else if (NPC.downedGolemBoss)
						defenseBoost = "+6 defense\n";
					else if (Main.hardMode)
						defenseBoost = "+4 defense\n";

					foreach (TooltipLine line2 in tooltips)
					{
						if (line2.mod == "Terraria" && line2.Name == "PrefixAccDefense")
							line2.text = defenseBoost + "+0.75% damage reduction";
					}
				}
				if (item.prefix == PrefixID.Warding)
				{
					string defenseBoost = "+4 defense\n";
					if (CalamityWorld.downedDoG)
						defenseBoost = "+16 defense\n";
					else if (CalamityWorld.downedPolterghast)
						defenseBoost = "+14 defense\n";
					else if (CalamityWorld.downedProvidence)
						defenseBoost = "+12 defense\n";
					else if (NPC.downedMoonlord)
						defenseBoost = "+10 defense\n";
					else if (NPC.downedGolemBoss)
						defenseBoost = "+8 defense\n";
					else if (Main.hardMode)
						defenseBoost = "+6 defense\n";

					foreach (TooltipLine line2 in tooltips)
					{
						if (line2.mod == "Terraria" && line2.Name == "PrefixAccDefense")
							line2.text = defenseBoost + "+1% damage reduction";
					}
				}
			}
			#endregion
		}
		#endregion

		#region True Melee Damage Tooltip
		private void TrueMeleeDamageTooltip(Item item, IList<TooltipLine> tooltips)
		{
			TooltipLine line = tooltips.FirstOrDefault((l) => l.mod == "Terraria" && l.Name == "Damage");

			// If there somehow isn't a damage tooltip line, do not try to perform any edits.
			if (line is null)
				return;

			// Start with the existing line of melee damage.
			StringBuilder sb = new StringBuilder(64);
			sb.Append(line.text).Append(" : ");

			Player p = Main.LocalPlayer;
			float itemCurrentDamage = item.damage * p.MeleeDamage();
			double trueMeleeBoost = 1D + p.Calamity().trueMeleeDamage;
			double imprecisionRoundingCorrection = 5E-06D;
			int damageToDisplay = (int)(itemCurrentDamage * trueMeleeBoost + imprecisionRoundingCorrection);
			sb.Append(damageToDisplay);

			// These two pieces are split apart for ease of translation
			sb.Append(' ');
			sb.Append("true melee damage");
			line.text = sb.ToString();
		}
		#endregion

		#region Stealth Generation Prefix Accessory Tooltip
		private void StealthGenAccessoryTooltip(Item item, IList<TooltipLine> tooltips)
		{
			if (!item.accessory || item.social || item.prefix <= 0)
				return;

			float stealthGenBoost = item.Calamity().StealthGenBonus - 1f;
			if (stealthGenBoost > 0)
			{
				TooltipLine StealthGen = new TooltipLine(mod, "PrefixStealthGenBoost", "+" + Math.Round(stealthGenBoost * 100f) + "% stealth generation")
				{
					isModifier = true
				};
				tooltips.Add(StealthGen);
			}
		}
		#endregion

		#region Fargo Biome Fountain Tooltip
		private void FargoFountainTooltip(Item item, IList<TooltipLine> tooltips)
		{
			if (CalamityMod.Instance.fargos is null)
				return;

			if (item.type == ModContent.ItemType<SunkenSeaFountain>())
			{
				TooltipLine line = new TooltipLine(mod, "FargoFountain", "Forces surrounding biome state to Sunken Sea upon activation");
				tooltips.Add(line);
			}
			if (item.type == ModContent.ItemType<SulphurousFountainItem>())
			{
				TooltipLine line = new TooltipLine(mod, "FargoFountain", "Forces surrounding biome state to Sulphurous Sea upon activation");
				tooltips.Add(line);
			}
			if (item.type == ModContent.ItemType<AstralFountainItem>())
			{
				TooltipLine line = new TooltipLine(mod, "FargoFountain", "Forces surrounding biome state to Astral upon activation");
				tooltips.Add(line);
			}
		}
		#endregion
	}
}
