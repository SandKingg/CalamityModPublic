using CalamityMod.Buffs;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.CalPlayer;
using CalamityMod.Dusts;
using CalamityMod.Events;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Tools;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.NPCs.Abyss;
using CalamityMod.NPCs.AcidRain;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.Astral;
using CalamityMod.NPCs.AstrumAureus;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.BrimstoneElemental;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.Calamitas;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.Crabulon;
using CalamityMod.NPCs.Crags;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.DraedonLabThings;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.HiveMind;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.PlaguebringerGoliath;
using CalamityMod.NPCs.PlagueEnemies;
using CalamityMod.NPCs.Polterghast;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.Signus;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.NPCs.SulphurousSea;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.Yharon;
using CalamityMod.Particles;
using CalamityMod.Projectiles.DraedonsArsenal;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.UI;
using CalamityMod.Walls.DraedonStructures;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using static Terraria.ModLoader.ModContent;
using CalamityMod.NPCs.ExoMechs;

namespace CalamityMod.NPCs
{
	public class CalamityGlobalNPC : GlobalNPC
    {
		#region Variables

		public float TimedDRScaleFactor { get; set; } = 0f;
		private const float timedDR_Disabled = 0f;
		private const float timedDR_Malice = 2f;
		private const float timedDR_Worm = 3f;
		private const float timedDR_NightProvi = 10f;
		private const float timedDRMult_Normal = 0.67f;
		private const float timedDRMult_Expert = 0.85f;

		public float DR { get; set; } = 0f;

		public int KillTime { get; set; } = 0;

		public const int DoGPhase1KillTime = 5400;

		// Debuff Vulnerabilities
		// null = neutral, true = vulnerable, false = resistant
		// neutral = 100% effective, vulnerable = 400% effective DoT and 200% effective non-DoT effects (within reason, some aren't exactly 200% more effective), resistant = 50% effective
		public bool? VulnerableToHeat = null;
		public bool? VulnerableToCold = null;
		public bool? VulnerableToSickness = null;
		public bool? VulnerableToElectricity = null;
		public bool? VulnerableToWater = null;

		// Biome enrage timer max
		public const int biomeEnrageTimerMax = 300;

		public bool ShouldFallThroughPlatforms;

		/// <summary>
		/// If this is set to true, the NPC's DR cannot be reduced via any means. This applies regardless of whether customDR is true or false.
		/// </summary>
		public bool unbreakableDR = false;

        /// <summary>
        /// Overrides the normal DR math and uses custom DR reductions for each debuff, registered separately.<br></br>
        /// Used primarily by post-Moon Lord bosses.
        /// </summary>
        public bool customDR = false;
        public Dictionary<int, float> flatDRReductions = new Dictionary<int, float>();
        public Dictionary<int, float> multDRReductions = new Dictionary<int, float>();

		/// <summary>
		/// Allows hostile NPCs to deal damage to the player's defense stat, used mostly for hard-hitting bosses.
		/// </summary>
		public bool canBreakPlayerDefense = false;

		// Total defense loss from some true melee hits and other things that reduce defense
		public int miscDefenseLoss = 0;

		// Distance values for when bosses increase velocity to catch up to their target
		public const float CatchUpDistance200Tiles = 3200f;
		public const float CatchUpDistance350Tiles = 5600f;

		// Boss Zen distance
		private const float BossZenDistance = 6400f;

		// Boss contact and projectile damage multiplier in Malice Mode
		public const double MaliceModeDamageMultiplier = 1.35;

		// Variable used to nerf desert prehardmode enemies pre-Desert Scourge
		private const double DesertEnemyStatMultiplier = 0.75;

		// Max velocity used in contact damage scaling
		public float maxVelocity = 0f;

		// Dash damage immunity timer
		public const int maxPlayerImmunities = Main.maxPlayers + 1;
		public int[] dashImmunityTime = new int[maxPlayerImmunities];

		// Town NPC shop alert animation variables
		public int shopAlertAnimTimer = 0;
		public int shopAlertAnimFrame = 0;

		// Rage interactions
		public bool DoesNotGenerateRage = false;

        // NewAI
        internal const int maxAIMod = 4;
        public float[] newAI = new float[maxAIMod];
		public int AITimer = 0;
		public int AIIncreasedAggressionTimer = 0;
		public float killTimeRatio_IncreasedAggression = 0f;

        // Town NPC Patreon
        public bool setNewName = true;

        // Draedons Remote
        public static bool DraedonMayhem = false;

		// Stuff used by the Boss Health UI
		public bool SplittingWorm = false;
		public bool CanHaveBossHealthBar = false;
		public bool ShouldCloseHPBar = false;

		// Timer for how long an NPC is immune to certain debuffs
		public const int slowingDebuffResistanceMin = 1800;
		public int debuffResistanceTimer = 0;

		// Debuffs
		public int vaporfied = 0;
        public int timeSlow = 0;
        public int gState = 0;
        public int tesla = 0;
        public int tSad = 0;
        public int eFreeze = 0;
        public int eutrophication = 0;
        public int webbed = 0;
        public int slowed = 0;
        public int electrified = 0;
        public int yellowCandle = 0;
        public int pearlAura = 0;
        public int wCleave = 0;
        public int bBlood = 0;
        public int dFlames = 0;
		public int marked = 0;
        public int irradiated = 0;
        public int bFlames = 0;
        public int hFlames = 0;
        public int pFlames = 0;
        public int aCrunch = 0;
        public int pShred = 0;
        public int cDepth = 0;
        public int gsInferno = 0;
        public int astralInfection = 0;
        public int aFlames = 0;
        public int wDeath = 0;
        public int nightwither = 0;
        public int enraged = 0;
        public int shellfishVore = 0;
        public int clamDebuff = 0;
        public int sulphurPoison = 0;
        public int ladHearts = 0;
        public int kamiFlu = 0;
        public int relicOfResilienceCooldown = 0;
        public int relicOfResilienceWeakness = 0;
        public int GaussFluxTimer = 0;
        public int sagePoisonTime = 0;
        public int sagePoisonDamage = 0;
        public int vulnerabilityHex = 0;
        public int banishingFire = 0;
		public int wither = 0;

        // whoAmI Variables
        public static int[] bobbitWormBottom = new int[5];
        public static int DD2CrystalIndex = -1;
        public static int hiveMind = -1;
        public static int perfHive = -1;
        public static int slimeGodPurple = -1;
        public static int slimeGodRed = -1;
        public static int slimeGod = -1;
        public static int laserEye = -1;
        public static int fireEye = -1;
        public static int primeLaser = -1;
        public static int primeCannon = -1;
        public static int primeVice = -1;
        public static int primeSaw = -1;
        public static int brimstoneElemental = -1;
        public static int cataclysm = -1;
        public static int catastrophe = -1;
        public static int calamitas = -1;
        public static int leviathan = -1;
        public static int siren = -1;
        public static int scavenger = -1;
        public static int energyFlame = -1;
        public static int doughnutBoss = -1;
        public static int holyBossAttacker = -1;
        public static int holyBossDefender = -1;
        public static int holyBossHealer = -1;
        public static int holyBoss = -1;
        public static int voidBoss = -1;
		public static int signus = -1;
        public static int ghostBossClone = -1;
        public static int ghostBoss = -1;
        public static int DoGHead = -1;
        public static int SCalCataclysm = -1;
        public static int SCalCatastrophe = -1;
        public static int SCal = -1;
        public static int SCalWorm = -1;
		public static int draedon = -1;
		public static int draedonExoMechWorm = -1;
		public static int draedonExoMechTwinRed = -1;
		public static int draedonExoMechTwinGreen = -1;
		public static int draedonExoMechPrime = -1;
		public static int draedonExoMechPrimePlasmaCannon = -1;
		public static int adultEidolonWyrmHead = -1;

		// Drawing variables.
		public FireParticleSet VulnerabilityHexFireDrawer = null;

		// Boss Enrage variable for use with the boss health UI.
		// The logic behind this is as follows:
		// 1 - For special cases with super-enrages (specifically Yharon/SCal with their arenas), go solely based on whether that enrage is active. That information is most important to the player.
		// 2 - Otherwise, check if the demonshade enrage is active. If it is, register this as true. If not, go to step 3.
		// 3 - Check if a specific enrage condition (such as Duke Fishron's Ocean check) is met. If it is, and Boss Rush is not active, set this to true. If not, go to step 4.
		// 4 - Check if malice is active and Boss Rush isn't. If so, set this to true.
		public bool CurrentlyEnraged;

		// Other Boss Rush stuff
		public bool DoesNotDisappearInBossRush;

		// Collections
		// NOTE - Be sure to reference the NeedsFourLifeBytes list in the IL Editing code if changes are made here.
		public static SortedDictionary<int, int> BossRushHPChanges = new SortedDictionary<int, int>
        {
            // Tier 1
            { NPCID.QueenBee, 315000 }, // 30 seconds

            { NPCID.BrainofCthulhu, 100000 }, // 30 seconds with creepers
            { NPCID.Creeper, 10000 },

            { NPCID.KingSlime, 300000 }, // 30 seconds
            { NPCID.BlueSlime, 3600 },
            { NPCID.SlimeSpiked, 7200 },
            { NPCID.GreenSlime, 2700 },
            { NPCID.RedSlime, 5400 },
            { NPCID.PurpleSlime, 7200 },
            { NPCID.YellowSlime, 6300 },
            { NPCID.IceSlime, 4500 },
            { NPCID.UmbrellaSlime, 5400 },
            { NPCID.RainbowSlime, 30000 },
            { NPCID.Pinky, 15000 },

            { NPCID.EyeofCthulhu, 450000 }, // 30 seconds
            { NPCID.ServantofCthulhu, 6000 },

            { NPCID.SkeletronPrime, 110000 }, // 30 seconds
            { NPCID.PrimeVice, 54000 },
            { NPCID.PrimeCannon, 45000 },
            { NPCID.PrimeSaw, 45000 },
            { NPCID.PrimeLaser, 38000 },

            { NPCID.Golem, 50000 }, // 30 seconds
            { NPCID.GolemHead, 30000 },
            { NPCID.GolemHeadFree, 30000 },
            { NPCID.GolemFistLeft, 25000 },
            { NPCID.GolemFistRight, 25000 },

            { NPCID.EaterofWorldsHead, 10000 }, // 30 seconds + immunity timer at start
            { NPCID.EaterofWorldsBody, 10000 },
            { NPCID.EaterofWorldsTail, 10000 },

            // Tier 2
            { NPCID.TheDestroyer, 250000 }, // 30 seconds + immunity timer at start
            { NPCID.TheDestroyerBody, 250000 },
            { NPCID.TheDestroyerTail, 250000 },
            { NPCID.Probe, 10000 },

            { NPCID.Spazmatism, 150000 }, // 30 seconds
            { NPCID.Retinazer, 125000 },

            { NPCID.WallofFlesh, 450000 }, // 30 seconds
            { NPCID.WallofFleshEye, 450000 },

            { NPCID.SkeletronHead, 160000 }, // 30 seconds
            { NPCID.SkeletronHand, 60000 },

            // Tier 3
            { NPCID.CultistBoss, 220000 }, // 30 seconds
            { NPCID.CultistDragonHead, 60000 },
            { NPCID.CultistDragonBody1, 60000 },
            { NPCID.CultistDragonBody2, 60000 },
            { NPCID.CultistDragonBody3, 60000 },
            { NPCID.CultistDragonBody4, 60000 },
            { NPCID.CultistDragonTail, 60000 },
            { NPCID.AncientCultistSquidhead, 50000 },

            { NPCID.Plantera, 160000 }, // 30 seconds
            { NPCID.PlanterasTentacle, 40000 },

            // Tier 4
            { NPCID.DukeFishron, 290000 }, // 30 seconds

            { NPCID.MoonLordCore, 160000 }, // 1 minute
            { NPCID.MoonLordHand, 45000 },
            { NPCID.MoonLordHead, 60000 },
            { NPCID.MoonLordLeechBlob, 800 }

			// 8 minutes in total for vanilla Boss Rush bosses
        };

        public static SortedDictionary<int, int> BossValues = new SortedDictionary<int, int>
        {
            { NPCID.QueenBee, Item.buyPrice(0, 5)},
            { NPCID.SkeletronHead, Item.buyPrice(0, 7) },
            { NPCID.DukeFishron, Item.buyPrice(0, 25) },
            { NPCID.CultistBoss, Item.buyPrice(0, 25) },
            { NPCID.MoonLordCore, Item.buyPrice(0, 30) }
        };

		// Lists of enemies that resist piercing to some extent (mostly worms).
		// Could prove useful for other things as well.

		public static List<int> AstrumDeusIDs = new List<int>
		{
			NPCType<AstrumDeusHeadSpectral>(),
			NPCType<AstrumDeusBodySpectral>(),
			NPCType<AstrumDeusTailSpectral>()
		};

        public static List<int> DevourerOfGodsIDs = new List<int>
        {
            NPCType<DevourerofGodsHead>(),
            NPCType<DevourerofGodsBody>(),
            NPCType<DevourerofGodsTail>()
        };

		public static List<int> CosmicGuardianIDs = new List<int>
		{
			NPCType<DevourerofGodsHead2>(),
			NPCType<DevourerofGodsBody2>(),
			NPCType<DevourerofGodsTail2>()
		};

		public static List<int> AquaticScourgeIDs = new List<int>
		{
			NPCType<AquaticScourgeHead>(),
			NPCType<AquaticScourgeBody>(),
			NPCType<AquaticScourgeBodyAlt>(),
			NPCType<AquaticScourgeTail>()
		};

		public static List<int> PerforatorIDs = new List<int>
		{
			NPCType<PerforatorHeadLarge>(),
			NPCType<PerforatorBodyLarge>(),
			NPCType<PerforatorTailLarge>(),
			NPCType<PerforatorHeadMedium>(),
			NPCType<PerforatorBodyMedium>(),
			NPCType<PerforatorTailMedium>(),
			NPCType<PerforatorHeadSmall>(),
			NPCType<PerforatorBodySmall>(),
			NPCType<PerforatorTailSmall>()
		};

		public static List<int> DesertScourgeIDs = new List<int>
		{
			NPCType<DesertScourgeHead>(),
			NPCType<DesertScourgeBody>(),
			NPCType<DesertScourgeTail>()
		};

		public static List<int> EaterofWorldsIDs = new List<int>
		{
			NPCID.EaterofWorldsHead,
			NPCID.EaterofWorldsBody,
			NPCID.EaterofWorldsTail
		};

		public static List<int> DeathModeSplittingWormIDs = new List<int>
		{
			NPCID.DuneSplicerHead,
			NPCID.DuneSplicerBody,
			NPCID.DuneSplicerTail,
			NPCID.DiggerHead,
			NPCID.DiggerBody,
			NPCID.DiggerTail,
			NPCID.SeekerHead,
			NPCID.SeekerBody,
			NPCID.SeekerTail
		};

		public static List<int> DestroyerIDs = new List<int>
		{
			NPCID.TheDestroyer,
			NPCID.TheDestroyerBody,
			NPCID.TheDestroyerTail
		};

		public static List<int> ThanatosIDs = new List<int>
		{
			NPCType<ThanatosHead>(),
			NPCType<ThanatosBody1>(),
			NPCType<ThanatosBody2>(),
			NPCType<ThanatosTail>()
		};

		public static List<int> AresIDs = new List<int>
		{
			NPCType<AresBody>(),
			NPCType<AresGaussNuke>(),
			NPCType<AresLaserCannon>(),
			NPCType<AresPlasmaFlamethrower>(),
			NPCType<AresTeslaCannon>()
		};

		public static List<int> SkeletronPrimeIDs = new List<int>
		{
			NPCID.SkeletronPrime,
			NPCID.PrimeCannon,
			NPCID.PrimeLaser,
			NPCID.PrimeSaw,
			NPCID.PrimeVice
		};

		public static List<int> StormWeaverIDs = new List<int>
		{
			NPCType<StormWeaverHead>(),
			NPCType<StormWeaverBody>(),
			NPCType<StormWeaverTail>()
		};

		public static List<int> GrenadeResistIDs = new List<int>
		{
			ProjectileID.Grenade,
			ProjectileID.StickyGrenade,
			ProjectileID.BouncyGrenade,
			ProjectileID.Bomb,
			ProjectileID.StickyBomb,
			ProjectileID.BouncyBomb,
			ProjectileID.Dynamite,
			ProjectileID.StickyDynamite,
			ProjectileID.BouncyDynamite,
			ProjectileID.Explosives,
			ProjectileID.ExplosiveBunny,
			ProjectileID.PartyGirlGrenade,
			ProjectileID.BombFish,
			ProjectileID.Beenade,
			ProjectileID.Bee,
			ProjectileID.GiantBee,
			ProjectileType<AeroExplosive>()
			//ProjectileID.ScarabBomb
		};

		public static List<int> ZeroContactDamageNPCList = new List<int>
		{
			NPCID.DarkCaster,
			NPCID.FireImp,
			NPCID.Tim,
			NPCID.CultistArcherBlue,
			NPCID.DesertDjinn,
			NPCID.DiabolistRed,
			NPCID.DiabolistWhite,
			NPCID.Gastropod,
			NPCID.IceElemental,
			NPCID.IchorSticker,
			NPCID.Necromancer,
			NPCID.NecromancerArmored,
			NPCID.RaggedCaster,
			NPCID.RaggedCasterOpenCoat,
			NPCID.RuneWizard,
			NPCID.SkeletonArcher,
			NPCID.SkeletonCommando,
			NPCID.SkeletonSniper,
			NPCID.TacticalSkeleton,
			NPCID.Clown,
			NPCID.GoblinArcher,
			NPCID.GoblinSorcerer,
			NPCID.GoblinSummoner,
			NPCID.PirateCrossbower,
			NPCID.PirateDeadeye,
			NPCID.PirateCaptain,
			NPCID.SnowmanGangsta,
			NPCID.SnowBalla,
			NPCID.DrManFly,
			NPCID.Eyezor,
			NPCID.Nailhead,
			NPCID.MartianWalker,
			NPCID.MartianTurret,
			NPCID.ElfCopter,
			NPCID.ElfArcher,
			NPCID.NebulaBrain,
			NPCID.StardustJellyfishBig,
			NPCID.PirateShipCannon,
			NPCID.MartianSaucer,
			NPCID.MartianSaucerCannon,
			NPCID.MartianSaucerCore,
			NPCID.MartianSaucerTurret,
			NPCID.Probe,
			NPCID.CultistBoss,
			NPCID.GolemHeadFree,
			NPCID.MoonLordFreeEye,
			//NPCID.BloodSquid,
			NPCID.PlanterasHook
		};

		// Reduce contact damage by 25%
		public static List<int> HardmodeNPCNerfList = new List<int>
		{
			NPCID.AnglerFish,
			NPCID.AngryTrapper,
			NPCID.Arapaima,
			NPCID.BlackRecluse,
			NPCID.BlackRecluseWall,
			NPCID.BloodJelly,
			NPCID.FungoFish,
			NPCID.GreenJellyfish,
			NPCID.Clinger,
			NPCID.ArmoredSkeleton,
			NPCID.ArmoredViking,
			NPCID.Mummy,
			NPCID.DarkMummy,
			NPCID.LightMummy,
			NPCID.BloodFeeder,
			NPCID.DesertBeast,
			NPCID.ChaosElemental,
			//NPCID.BloodMummy,
			NPCID.CorruptSlime,
			NPCID.Slimeling,
			NPCID.Corruptor,
			NPCID.Crimslime,
			NPCID.BigCrimslime,
			NPCID.LittleCrimslime,
			NPCID.CrimsonAxe,
			NPCID.CursedHammer,
			NPCID.Derpling,
			NPCID.Herpling,
			NPCID.DiggerHead,
			NPCID.DiggerBody,
			NPCID.DiggerTail,
			NPCID.DesertGhoul,
			NPCID.DesertGhoulCorruption,
			NPCID.DesertGhoulCrimson,
			NPCID.DesertGhoulHallow,
			NPCID.DuneSplicerHead,
			NPCID.DuneSplicerBody,
			NPCID.DuneSplicerTail,
			NPCID.EnchantedSword,
			NPCID.FloatyGross,
			NPCID.GiantBat,
			NPCID.GiantFlyingFox,
			NPCID.GiantFungiBulb,
			NPCID.FungiSpore,
			NPCID.GiantTortoise,
			NPCID.IceTortoise,
			NPCID.HoppinJack,
			NPCID.Mimic,
			NPCID.IchorSticker,
			NPCID.IcyMerman,
			NPCID.IlluminantBat,
			NPCID.IlluminantSlime,
			NPCID.JungleCreeper,
			NPCID.JungleCreeperWall,
			NPCID.DesertLamiaDark,
			NPCID.DesertLamiaLight,
			NPCID.BigMossHornet,
			NPCID.GiantMossHornet,
			NPCID.LittleMossHornet,
			NPCID.MossHornet,
			NPCID.TinyMossHornet,
			NPCID.Moth,
			NPCID.PigronCorruption,
			NPCID.PigronCrimson,
			NPCID.PigronHallow,
			NPCID.Pixie,
			NPCID.PossessedArmor,
			//NPCID.RockGolem,
			NPCID.DesertScorpionWalk,
			NPCID.DesertScorpionWall,
			NPCID.Slimer,
			NPCID.Slimer2,
			NPCID.ToxicSludge,
			NPCID.Unicorn,
			NPCID.WanderingEye,
			NPCID.Werewolf,
			NPCID.Wolf,
			NPCID.SeekerHead,
			NPCID.SeekerBody,
			NPCID.SeekerTail,
			NPCID.Wraith,
			NPCID.ChatteringTeethBomb,
			NPCID.Clown,
			NPCID.AngryNimbus,
			NPCID.IceGolem,
			NPCID.RainbowSlime,
			NPCID.SandShark,
			NPCID.SandsharkCorrupt,
			NPCID.SandsharkCrimson,
			NPCID.SandsharkHallow,
			NPCID.ShadowFlameApparition,
			NPCID.Parrot,
			NPCID.PirateCorsair,
			NPCID.PirateDeckhand,
			//NPCID.PiratesCurse,
			NPCID.BlueArmoredBonesMace,
			NPCID.BlueArmoredBonesSword,
			NPCID.BoneLee,
			NPCID.DungeonSpirit,
			NPCID.FlyingSnake,
			NPCID.HellArmoredBones,
			NPCID.HellArmoredBonesSpikeShield,
			NPCID.HellArmoredBonesSword,
			NPCID.MisterStabby,
			NPCID.SnowBalla,
			NPCID.SnowmanGangsta,
			NPCID.Butcher,
			NPCID.CreatureFromTheDeep,
			NPCID.DeadlySphere,
			NPCID.Frankenstein,
			NPCID.Fritz,
			NPCID.Psycho,
			NPCID.Reaper,
			NPCID.SwampThing,
			NPCID.ThePossessed,
			NPCID.Vampire,
			NPCID.VampireBat,
			NPCID.HeadlessHorseman,
			NPCID.Hellhound,
			NPCID.Poltergeist,
			NPCID.Scarecrow1,
			NPCID.Scarecrow2,
			NPCID.Scarecrow3,
			NPCID.Scarecrow4,
			NPCID.Scarecrow5,
			NPCID.Scarecrow6,
			NPCID.Scarecrow7,
			NPCID.Scarecrow8,
			NPCID.Scarecrow9,
			NPCID.Scarecrow10,
			NPCID.Splinterling,
			NPCID.Flocko,
			NPCID.GingerbreadMan,
			NPCID.Krampus,
			NPCID.Nutcracker,
			NPCID.NutcrackerSpinning,
			NPCID.PresentMimic,
			NPCID.Yeti,
			NPCID.ZombieElf,
			NPCID.ZombieElfBeard,
			NPCID.ZombieElfGirl
			//NPCID.BloodEelHead,
			//NPCID.BloodEelBody,
			//NPCID.BloodEelTail,
			//NPCID.HemogoblinShark,
			//NPCID.WanderingEyeFish,
			//NPCID.ZombieMerman,
		};

		public static List<int> BoundNPCIDs = new List<int>
		{
			NPCID.BoundGoblin,
			NPCID.BoundWizard,
			NPCID.BoundMechanic,
			NPCID.SleepingAngler,
			NPCID.BartenderUnconscious,
			NPCID.WebbedStylist,
			//NPCID.GolferRescue
		};
		#endregion

		#region Instance Per Entity
		public override bool InstancePerEntity => true;
        #endregion

        #region Reset Effects
        public override void ResetEffects(NPC npc)
        {
            void ResetSavedIndex(ref int type, int type1, int type2 = -1)
            {
                if (type >= 0)
                {
					if (!Main.npc[type].active)
					{
						type = -1;
					}
					else if (type2 == -1)
					{
						if (Main.npc[type].type != type1)
							type = -1;
					}
					else
					{
						if (Main.npc[type].type != type1 && Main.npc[type].type != type2)
							type = -1;
					}
                }
            }

			for (int i = 0; i < bobbitWormBottom.Length; i++)
				ResetSavedIndex(ref bobbitWormBottom[i], NPCType<BobbitWormSegment>());

            ResetSavedIndex(ref DD2CrystalIndex, NPCID.DD2EterniaCrystal);
            ResetSavedIndex(ref hiveMind, NPCType<HiveMind.HiveMind>());
            ResetSavedIndex(ref perfHive, NPCType<PerforatorHive>());
            ResetSavedIndex(ref slimeGodPurple, NPCType<SlimeGod.SlimeGod>(), NPCType<SlimeGodSplit>());
            ResetSavedIndex(ref slimeGodRed, NPCType<SlimeGodRun>(), NPCType<SlimeGodRunSplit>());
            ResetSavedIndex(ref slimeGod, NPCType<SlimeGodCore>());
            ResetSavedIndex(ref laserEye, NPCID.Retinazer);
            ResetSavedIndex(ref fireEye, NPCID.Spazmatism);
            ResetSavedIndex(ref primeLaser, NPCID.PrimeLaser);
            ResetSavedIndex(ref primeCannon, NPCID.PrimeCannon);
            ResetSavedIndex(ref primeVice, NPCID.PrimeVice);
            ResetSavedIndex(ref primeSaw, NPCID.PrimeSaw);
            ResetSavedIndex(ref brimstoneElemental, NPCType<BrimstoneElemental.BrimstoneElemental>());
            ResetSavedIndex(ref cataclysm, NPCType<CalamitasRun>());
            ResetSavedIndex(ref catastrophe, NPCType<CalamitasRun2>());
            ResetSavedIndex(ref calamitas, NPCType<CalamitasRun3>());
            ResetSavedIndex(ref leviathan, NPCType<Leviathan.Leviathan>());
            ResetSavedIndex(ref siren, NPCType<Siren>());
            ResetSavedIndex(ref scavenger, NPCType<RavagerBody>());
            ResetSavedIndex(ref energyFlame, NPCType<ProfanedEnergyBody>());
            ResetSavedIndex(ref doughnutBoss, NPCType<ProfanedGuardianBoss>());
            ResetSavedIndex(ref holyBossAttacker, NPCType<ProvSpawnOffense>());
            ResetSavedIndex(ref holyBossDefender, NPCType<ProvSpawnDefense>());
            ResetSavedIndex(ref holyBossHealer, NPCType<ProvSpawnHealer>());
            ResetSavedIndex(ref holyBoss, NPCType<Providence.Providence>());
            ResetSavedIndex(ref voidBoss, NPCType<CeaselessVoid.CeaselessVoid>());
			ResetSavedIndex(ref signus, NPCType<Signus.Signus>());
            ResetSavedIndex(ref ghostBossClone, NPCType<PolterPhantom>());
            ResetSavedIndex(ref ghostBoss, NPCType<Polterghast.Polterghast>());
            ResetSavedIndex(ref DoGHead, NPCType<DevourerofGodsHead>());
            ResetSavedIndex(ref SCalCataclysm, NPCType<SupremeCataclysm>());
            ResetSavedIndex(ref SCalCatastrophe, NPCType<SupremeCatastrophe>());
            ResetSavedIndex(ref SCal, NPCType<SupremeCalamitas.SupremeCalamitas>());
            ResetSavedIndex(ref SCalWorm, NPCType<SCalWormHead>());

			ResetSavedIndex(ref draedon, NPCType<Draedon>());
			ResetSavedIndex(ref draedonExoMechWorm, NPCType<ThanatosHead>());
			ResetSavedIndex(ref draedonExoMechTwinRed, NPCType<Artemis>());
			ResetSavedIndex(ref draedonExoMechTwinGreen, NPCType<Apollo>());
			ResetSavedIndex(ref draedonExoMechPrime, NPCType<AresBody>());
			ResetSavedIndex(ref draedonExoMechPrimePlasmaCannon, NPCType<AresPlasmaFlamethrower>());

			ResetSavedIndex(ref adultEidolonWyrmHead, NPCType<EidolonWyrmHeadHuge>());

			// Reset the enraged state every frame. The expectation is that bosses will continuously set it back to true if necessary.
			CurrentlyEnraged = false;
			CanHaveBossHealthBar = false;
			ShouldCloseHPBar = false;
		}
        #endregion

        #region Life Regen
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            int genLimit = Main.maxTilesX / 2;
            int abyssChasmX = CalamityWorld.abyssSide ? genLimit - (genLimit - 135) : genLimit + (genLimit - 135);
            bool abyssPosX = false;

            if (CalamityWorld.abyssSide)
            {
                if ((double)(npc.position.X / 16f) < abyssChasmX + 80)
                    abyssPosX = true;
            }
            else
            {
                if ((double)(npc.position.X / 16f) > abyssChasmX - 80)
                    abyssPosX = true;
            }

            bool inAbyss = (npc.position.Y / 16f > (Main.rockLayer - Main.maxTilesY * 0.05)) && ((double)(npc.position.Y / 16f) <= Main.maxTilesY - 250) && abyssPosX;
            bool hurtByAbyss = npc.wet && npc.damage > 0 && !npc.boss && !npc.friendly && !npc.dontTakeDamage && inAbyss && !npc.buffImmune[BuffType<CrushDepth>()] && !CalamityLists.enemyImmunityList.Contains(npc.type);
            if (hurtByAbyss)
            {
                npc.AddBuff(BuffType<CrushDepth>(), 2);
                npc.DeathSound = null;
                npc.HitSound = null;
            }

            if (npc.damage > 0 && !npc.boss && !npc.friendly && !npc.dontTakeDamage && CalamityWorld.sulphurTiles > 30 &&
                !npc.buffImmune[BuffID.Poisoned] && !npc.buffImmune[BuffType<CrushDepth>()])
            {
                if (npc.wet)
                    npc.AddBuff(BuffID.Poisoned, 2);

                if (Main.raining)
                    npc.AddBuff(BuffType<Irradiated>(), 2);
            }

			// Special venom debuff case for the Lionfish and certain other projectiles.
			if (npc.venom)
            {
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;

                int projectileCount = 0;
                for (int j = 0; j < Main.maxProjectiles; j++)
                {
                    if (Main.projectile[j].active &&
                        (Main.projectile[j].type == ProjectileType<LionfishProj>() || Main.projectile[j].type == ProjectileType<SulphuricAcidBubble2>() || Main.projectile[j].type == ProjectileType<LeviathanTooth>() || Main.projectile[j].type == ProjectileType<JawsProjectile>()) &&
                        Main.projectile[j].ai[0] == 1f && Main.projectile[j].ai[1] == npc.whoAmI)
                    {
                        projectileCount++;
                    }
                }

                if (projectileCount > 0)
                {
                    npc.lifeRegen -= projectileCount * 30;

                    if (damage < projectileCount * 6)
                        damage = projectileCount * 6;
                }
            }

            if (npc.javelined)
            {
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;

                int projectileCount = 0;
                for (int j = 0; j < Main.maxProjectiles; j++)
                {
                    if (Main.projectile[j].active && Main.projectile[j].type == ProjectileType<BonebreakerProjectile>() &&
                        Main.projectile[j].ai[0] == 1f && Main.projectile[j].ai[1] == npc.whoAmI)
                    {
                        projectileCount++;
                    }
                }

                if (projectileCount > 0)
                {
                    npc.lifeRegen -= projectileCount * 20;

                    if (damage < projectileCount * 4)
                        damage = projectileCount * 4;
                }
            }

            if (shellfishVore > 0)
            {
                int projectileCount = 0;
				int owner = 255;
                for (int j = 0; j < Main.maxProjectiles; j++)
                {
                    if (Main.projectile[j].active && Main.projectile[j].type == ProjectileType<Shellfish>() &&
                        Main.projectile[j].ai[0] == 1f && Main.projectile[j].ai[1] == npc.whoAmI)
                    {
						owner = Main.projectile[j].owner;
                        projectileCount++;
						if (projectileCount >= 5)
						{
							projectileCount = 5;
							break;
						}
                    }
                }

				Item heldItem = Main.player[owner].ActiveItem();
				int totalDamage = (int)(150 * Main.player[owner].minionDamage);
				bool forbidden = Main.player[owner].head == ArmorIDs.Head.AncientBattleArmor && Main.player[owner].body == ArmorIDs.Body.AncientBattleArmor && Main.player[owner].legs == ArmorIDs.Legs.AncientBattleArmor;
				bool reducedNerf = Main.player[owner].Calamity().fearmongerSet || (forbidden && heldItem.magic);

				double summonNerfMult = reducedNerf ? 0.75 : 0.5;
				if (!Main.player[owner].Calamity().profanedCrystalBuffs)
				{
					if (heldItem.type > ItemID.None)
					{
						if (!heldItem.summon &&
							(heldItem.melee || heldItem.ranged || heldItem.magic || heldItem.Calamity().rogue) &&
							heldItem.hammer == 0 && heldItem.pick == 0 && heldItem.axe == 0 && heldItem.useStyle != 0 &&
							!heldItem.accessory && heldItem.ammo == AmmoID.None)
						{
							totalDamage = (int)(totalDamage * summonNerfMult);
						}
					}
				}

				int totalDisplayedDamage = totalDamage / 5;
				ApplyDPSDebuff(projectileCount * totalDamage, projectileCount * totalDisplayedDamage, ref npc.lifeRegen, ref damage);
            }

            if (clamDebuff > 0)
            {
                int projectileCount = 0;
                for (int j = 0; j < Main.maxProjectiles; j++)
                {
                    if (Main.projectile[j].active && Main.projectile[j].ai[0] == 1f && Main.projectile[j].ai[1] == npc.whoAmI)
                    {
						if (Main.projectile[j].type == ProjectileType<SnapClamProj>())
							projectileCount += 2;
						if (Main.projectile[j].type == ProjectileType<SnapClamStealth>())
							projectileCount++;
                    }
                }

				ApplyDPSDebuff(projectileCount * 15, projectileCount * 3, ref npc.lifeRegen, ref damage);
            }

            if (irradiated > 0)
            {
                int projectileCount = 0;
                for (int j = 0; j < Main.maxProjectiles; j++)
                {
                    if (Main.projectile[j].active && Main.projectile[j].type == ProjectileType<WaterLeechProj>() &&
                        Main.projectile[j].ai[0] == 1f && Main.projectile[j].ai[1] == npc.whoAmI)
                    {
                        projectileCount++;
                    }
                }

                if (projectileCount > 0)
					ApplyDPSDebuff(projectileCount * 20, projectileCount * 4, ref npc.lifeRegen, ref damage);
				else
					ApplyDPSDebuff(20, 4, ref npc.lifeRegen, ref damage);
            }

			// Exo Freeze, Glacial State and Temporal Sadness don't work on normal/expert Queen Bee.
			if (debuffResistanceTimer <= 0 || (debuffResistanceTimer > slowingDebuffResistanceMin))
			{
				if (npc.type != NPCID.QueenBee || CalamityWorld.revenge || CalamityWorld.malice || BossRushEvent.BossRushActive)
				{
					float baseXVelocityMult = 0.5f;
					float baseYVelocityIncrease = 0.1f;
					if (VulnerableToCold.HasValue)
					{
						if (VulnerableToCold.Value)
						{
							baseXVelocityMult = 0.25f;
							baseYVelocityIncrease = 0.2f;
						}
						else
						{
							baseXVelocityMult = 0.75f;
							baseYVelocityIncrease = 0.05f;
						}
					}

					if (eFreeze > 0)
					{
						if (!CalamityPlayer.areThereAnyDamnBosses)
						{
							npc.velocity.X *= baseXVelocityMult;
							npc.velocity.Y += baseYVelocityIncrease;
							if (npc.velocity.Y > 15f)
								npc.velocity.Y = 15f;
						}
						else
							npc.velocity *= baseXVelocityMult;
					}
					else if (gState > 0)
					{
						if (!CalamityPlayer.areThereAnyDamnBosses)
						{
							npc.velocity.X *= baseXVelocityMult;
							npc.velocity.Y += baseYVelocityIncrease * 0.5f;
							if (npc.velocity.Y > 15f)
								npc.velocity.Y = 15f;
						}
						else
						{
							npc.velocity *= baseXVelocityMult;
						}
					}
					else if (tSad > 0)
						npc.velocity *= 0.5f;
				}
			}

			// Debuff vulnerabilities and resistances.
			// Damage multiplier calcs.
			// Worms that are vulnerable to debuffs take reduced damage from vulnerabilities.
			bool wormBoss = DesertScourgeIDs.Contains(npc.type) || EaterofWorldsIDs.Contains(npc.type) || PerforatorIDs.Contains(npc.type) ||
				AquaticScourgeIDs.Contains(npc.type) || AstrumDeusIDs.Contains(npc.type) || StormWeaverIDs.Contains(npc.type);
			double heatDamageMult = npc.drippingSlime ? (wormBoss ? 3D : 5D) : 1D;
			if (VulnerableToHeat.HasValue)
			{
				if (VulnerableToHeat.Value)
					heatDamageMult *= npc.drippingSlime ? (wormBoss ? 1.5 : 2D) : (wormBoss ? 3D : 5D);
				else
					heatDamageMult *= npc.drippingSlime ? 0.2 : 0.5;
			}

			double coldDamageMult = 1D;
			if (VulnerableToCold.HasValue)
			{
				if (VulnerableToCold.Value)
					coldDamageMult *= wormBoss ? 3D : 5D;
				else
					coldDamageMult *= 0.5;
			}

			double sicknessDamageMult = irradiated > 0 ? (wormBoss ? 3D : 5D) : 1D;
			if (VulnerableToSickness.HasValue)
			{
				if (VulnerableToSickness.Value)
					sicknessDamageMult *= irradiated > 0 ? (wormBoss ? 1.5 : 2D) : (wormBoss ? 3D : 5D);
				else
					sicknessDamageMult *= irradiated > 0 ? 0.2 : 0.5;
			}

			bool increasedElectricityDamage = npc.wet || npc.honeyWet || npc.lavaWet || npc.dripping;
			double electricityDamageMult = increasedElectricityDamage ? (wormBoss ? 3D : 5D) : 1D;
			if (VulnerableToElectricity.HasValue)
			{
				if (VulnerableToElectricity.Value)
					electricityDamageMult *= increasedElectricityDamage ? (wormBoss ? 1.5 : 2D) : (wormBoss ? 3D : 5D);
				else
					electricityDamageMult *= increasedElectricityDamage ? 0.2 : 0.5;
			}

			double waterDamageMult = 1D;
			if (VulnerableToWater.HasValue)
			{
				if (VulnerableToWater.Value)
					waterDamageMult *= wormBoss ? 3D : 5D;
				else
					waterDamageMult *= 0.5;
			}

			// Subtract 1 for the vanilla damage multiplier because it's already dealing DoT in the vanilla regen code.
			double vanillaHeatDamageMult = heatDamageMult - 1D;
			double vanillaColdDamageMult = coldDamageMult - 1D;
			double vanillaSicknessDamageMult = sicknessDamageMult - 1D;

			// On Fire
			if (npc.onFire)
			{
				int baseOnFireDoTValue = (int)((npc.oiled ? 28 : 8) * vanillaHeatDamageMult);
				npc.lifeRegen -= baseOnFireDoTValue;
				if (damage < baseOnFireDoTValue / 4)
					damage = baseOnFireDoTValue / 4;
			}

			// Cursed Inferno
			if (npc.onFire2)
			{
				int baseCursedInfernoDoTValue = (int)((npc.oiled ? 36 : 12) * vanillaHeatDamageMult);
				npc.lifeRegen -= baseCursedInfernoDoTValue;
				if (damage < baseCursedInfernoDoTValue / 4)
					damage = baseCursedInfernoDoTValue / 4;
			}

			// Shadowflame
			if (npc.shadowFlame)
			{
				int baseShadowFlameDoTValue = (int)((npc.oiled ? 58 : 30) * vanillaHeatDamageMult);
				npc.lifeRegen -= baseShadowFlameDoTValue;
				if (damage < baseShadowFlameDoTValue / 4)
					damage = baseShadowFlameDoTValue / 4;
			}

			// Brimstone Flames
			if (bFlames > 0)
			{
				int baseBrimstoneFlamesDoTValue = (int)((npc.oiled ? 92 : 40) * heatDamageMult);
				ApplyDPSDebuff(baseBrimstoneFlamesDoTValue, baseBrimstoneFlamesDoTValue / 5, ref npc.lifeRegen, ref damage);
			}

			// Holy Flames
			if (hFlames > 0)
			{
				int baseHolyFlamesDoTValue = (int)((npc.oiled ? 114 : 50) * heatDamageMult);
				ApplyDPSDebuff(baseHolyFlamesDoTValue, baseHolyFlamesDoTValue / 5, ref npc.lifeRegen, ref damage);
			}

			// God Slayer Inferno
			if (gsInferno > 0)
			{
				int baseGodSlayerInfernoDoTValue = (int)((npc.oiled ? 514 : 250) * heatDamageMult);
				ApplyDPSDebuff(baseGodSlayerInfernoDoTValue, baseGodSlayerInfernoDoTValue / 5, ref npc.lifeRegen, ref damage);
			}

			// Abyssal Flames
			if (aFlames > 0)
			{
				int baseAbyssalFlamesDoTValue = (int)((npc.oiled ? 265 : 125) * heatDamageMult);
				ApplyDPSDebuff(baseAbyssalFlamesDoTValue, baseAbyssalFlamesDoTValue / 5, ref npc.lifeRegen, ref damage);
			}

			// Demon Flames
			if (dFlames > 0)
			{
				int baseDemonFlamesDoTValue = (int)((npc.oiled ? 5012 : 2500) * heatDamageMult);
				ApplyDPSDebuff(baseDemonFlamesDoTValue, baseDemonFlamesDoTValue / 5, ref npc.lifeRegen, ref damage);
			}

			// Banishing Fire
			if (banishingFire > 0)
			{
				int baseBanishingFireDoTValue = (int)((npc.oiled ? (npc.lifeMax >= 1000000 ? (npc.lifeMax / 500) + 1512 : 5512) : (npc.lifeMax >= 1000000 ? npc.lifeMax / 500 : 4000)) * heatDamageMult);
				ApplyDPSDebuff(baseBanishingFireDoTValue, baseBanishingFireDoTValue / 5, ref npc.lifeRegen, ref damage);
			}

			// Vulnerability Hex
			if (vulnerabilityHex > 0)
			{
				int baseVulnerabilityHexDoTValue = (int)((npc.oiled ? VulnerabilityHex.DPS * 2 : VulnerabilityHex.DPS) * heatDamageMult);
				ApplyDPSDebuff(baseVulnerabilityHexDoTValue, VulnerabilityHex.TickNumber, ref npc.lifeRegen, ref damage);
			}

			// Frostburn
			if (npc.onFrostBurn)
			{
				int baseFrostBurnDoTValue = (int)((npc.oiled ? 44 : 16) * vanillaColdDamageMult);
				npc.lifeRegen -= baseFrostBurnDoTValue;
				if (damage < baseFrostBurnDoTValue / 4)
					damage = baseFrostBurnDoTValue / 4;
			}

			// Nightwither
			if (nightwither > 0)
			{
				int baseNightwitherDoTValue = (int)(200 * coldDamageMult);
				ApplyDPSDebuff(baseNightwitherDoTValue, baseNightwitherDoTValue / 5, ref npc.lifeRegen, ref damage);
			}

			// Plague
			if (pFlames > 0)
			{
				int basePlagueDoTValue = (int)(100 * sicknessDamageMult);
				ApplyDPSDebuff(basePlagueDoTValue, basePlagueDoTValue / 5, ref npc.lifeRegen, ref damage);
			}

			// Astral Infection
			if (astralInfection > 0)
			{
				int baseAstralInfectionDoTValue = (int)(75 * sicknessDamageMult);
				ApplyDPSDebuff(baseAstralInfectionDoTValue, baseAstralInfectionDoTValue / 5, ref npc.lifeRegen, ref damage);
			}

			// Sulphuric Poisoning
			if (sulphurPoison > 0)
			{
				int baseSulphurPoisonDoTValue = (int)(180 * sicknessDamageMult);
				ApplyDPSDebuff(baseSulphurPoisonDoTValue, baseSulphurPoisonDoTValue / 5, ref npc.lifeRegen, ref damage);
			}

			// Sage Poison
			if (sagePoisonTime > 0)
			{
				int baseSagePoisonDoTValue = (int)(90 * sicknessDamageMult);
				ApplyDPSDebuff(baseSagePoisonDoTValue, npc.Calamity().sagePoisonDamage, ref npc.lifeRegen, ref damage);
			}

			// Kami Debuff from Yanmei's Knife
			if (kamiFlu > 0)
			{
				int baseKamiFluDoTValue = (int)(250 * sicknessDamageMult);
				ApplyDPSDebuff(baseKamiFluDoTValue, baseKamiFluDoTValue / 10, ref npc.lifeRegen, ref damage);
			}

			// Poisoned
			if (npc.poisoned)
			{
				int basePoisonedDoTValue = (int)(4 * vanillaSicknessDamageMult);
				npc.lifeRegen -= basePoisonedDoTValue;
				if (damage < basePoisonedDoTValue / 4)
					damage = basePoisonedDoTValue / 4;
			}

			// Venom
			if (npc.venom)
			{
				int baseVenomDoTValue = (int)(12 * vanillaSicknessDamageMult);
				npc.lifeRegen -= baseVenomDoTValue;
				if (damage < baseVenomDoTValue / 4)
					damage = baseVenomDoTValue / 4;
			}

			// Electrified
			if (electrified > 0)
			{
				int baseElectrifiedDoTValue = (int)((CalamityPlayer.areThereAnyDamnBosses ? 5 : 10) * (npc.velocity.X == 0 ? 1 : 4) * electricityDamageMult);
				ApplyDPSDebuff(baseElectrifiedDoTValue, baseElectrifiedDoTValue / 5, ref npc.lifeRegen, ref damage);
			}

			// Crush Depth
			if (cDepth > 0)
			{
				int baseCrushDepthDoTValue = (int)(((hurtByAbyss ? 300 : Main.hardMode ? 36 : 12) - npc.defense) * 5 * waterDamageMult);
				if (baseCrushDepthDoTValue > 0)
					ApplyDPSDebuff(baseCrushDepthDoTValue, baseCrushDepthDoTValue / 5, ref npc.lifeRegen, ref damage);
			}

			// Debuffs that aren't affected by weaknesses or resistances.
			if (vaporfied > 0)
				ApplyDPSDebuff(30, 6, ref npc.lifeRegen, ref damage);
			if (pShred > 0)
				ApplyDPSDebuff(1500, 300, ref npc.lifeRegen, ref damage);
			if (bBlood > 0)
				ApplyDPSDebuff(50, 10, ref npc.lifeRegen, ref damage);
		}

		public void ApplyDPSDebuff(int lifeRegenValue, int damageValue, ref int lifeRegen, ref int damage)
		{
			if (lifeRegen > 0)
				lifeRegen = 0;

			lifeRegen -= lifeRegenValue;

			if (damage < damageValue)
				damage = damageValue;
		}
        #endregion

        #region Set Defaults
        public override void SetDefaults(NPC npc)
        {
			ShouldFallThroughPlatforms = false;

			for (int i = 0; i < maxPlayerImmunities; i++)
				dashImmunityTime[i] = 0;

			for (int m = 0; m < maxAIMod; m++)
                newAI[m] = 0f;

			// Apply DR to vanilla NPCs.
			// This also applies DR to other mods' NPCs who have set up their NPCs to have DR.
			if (CalamityMod.DRValues.ContainsKey(npc.type))
			{
				CalamityMod.DRValues.TryGetValue(npc.type, out float newDR);
				DR = newDR;
			}

			if (CalamityMod.bossKillTimes.ContainsKey(npc.type))
			{
				CalamityMod.bossKillTimes.TryGetValue(npc.type, out int revKillTime);
				KillTime = revKillTime;
			}

			// Fixing more red mistakes
			if (npc.type == NPCID.WallofFleshEye)
				npc.netAlways = true;

            sagePoisonDamage = 0;
			if (npc.type == NPCID.Golem && (CalamityWorld.revenge || CalamityWorld.malice))
				npc.noGravity = true;

			DeclareBossHealthUIVariables(npc);
			DebuffImmunities(npc);

			BaseVanillaBossHPAdjustments(npc);

			if (BossRushEvent.BossRushActive)
                BossRushStatChanges(npc, mod);

            BossValueChanges(npc);

            if (DraedonMayhem)
                DraedonMechaMayhemStatChanges(npc);

			if (CalamityWorld.revenge)
                RevDeathStatChanges(npc, mod);

            OtherStatChanges(npc);

			VulnerabilitiesAndResistances(npc);

			TimedDRScaleFactors(npc);

			CalamityGlobalTownNPC.BoundNPCSafety(mod, npc);
        }
		#endregion

		#region Boss Health UI Variable Setting
		public void DeclareBossHealthUIVariables(NPC npc)
		{
			if (npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.EaterofWorldsBody || npc.type == NPCID.EaterofWorldsTail)
				SplittingWorm = true;
		}
		#endregion

		#region Debuff Immunities
		private void DebuffImmunities(NPC npc)
        {
			// Check out NPCDebuffs.cs as this function sets the debuff immunities for all enemies in Cal bar the ones described below.
			npc.SetNPCDebuffImmunities();

			// All bosses and several enemies are automatically immune to Pearl Aura.
			if (CalamityLists.enemyImmunityList.Contains(npc.type) || npc.boss)
				npc.buffImmune[BuffType<PearlAura>()] = true;

			// All enemies are automatically immune to the Confused debuff, so we must specifically set them not to be.
			// Extra note: Clams are not in this list as they initially immune to Confused, but are no longer immune once aggro'd. This is set in their AI().
			if (CalamityLists.confusionEnemyList.Contains(npc.type))
				npc.buffImmune[BuffID.Confused] = false;

			// Any enemy not immune to Venom shouldn't be immune to Sulphuric Poisoning as it is an upgrade.
			if (!npc.buffImmune[BuffID.Venom])
				npc.buffImmune[BuffType<SulphuricPoisoning>()] = false;

			// Sets certain vanilla NPCs and all town NPCs to be immune to most debuffs.
			if (((DesertScourgeIDs.Contains(npc.type) || npc.type == NPCID.Creeper || PerforatorIDs.Contains(npc.type)) && CalamityWorld.malice) ||
				DestroyerIDs.Contains(npc.type) || npc.type == NPCID.SkeletronHead || npc.type == NPCID.SpikeBall || npc.type == NPCID.BlazingWheel ||
				(EaterofWorldsIDs.Contains(npc.type) && (BossRushEvent.BossRushActive || CalamityWorld.malice)) ||
				npc.type == NPCID.DD2EterniaCrystal || npc.townNPC)
			{
				for (int k = 0; k < npc.buffImmune.Length; k++)
				{
					npc.buffImmune[k] = true;
				}

				if (npc.townNPC)
				{
					npc.buffImmune[BuffID.Wet] = false;
					npc.buffImmune[BuffID.Slimed] = false;
					npc.buffImmune[BuffID.Lovestruck] = false;
					npc.buffImmune[BuffID.Stinky] = false;
				}
			}

			// Most bosses and boss servants are not immune to Kami Flu.
            if (YanmeisKnifeSlash.CanRecieveCoolEffectsFrom(npc))
                npc.buffImmune[BuffType<KamiDebuff>()] = false;

			// Nothing should be immune to Enraged.
            npc.buffImmune[BuffType<Enraged>()] = false;

            // Extra Notes:
            // Shellfish minions set debuff immunity to Shellfish Claps on enemy hits, so most things are technically not immune.
            // The Spiteful Candle sets the debuff immunity of Spite to all nearby enemies in the tile file for an enemy with less than 99% DR.
        }
		#endregion

		#region Base Vanilla Boss HP Adjustments
		private void BaseVanillaBossHPAdjustments(NPC npc)
		{
			switch (npc.type)
			{
				case NPCID.MoonLordCore:
					npc.lifeMax = 92000;
					break;

				case NPCID.CultistBoss:
					npc.lifeMax = 53500;
					break;

				case NPCID.CultistDragonBody1:
				case NPCID.CultistDragonBody2:
				case NPCID.CultistDragonBody3:
				case NPCID.CultistDragonBody4:
				case NPCID.CultistDragonHead:
				case NPCID.CultistDragonTail:
					npc.lifeMax = 41600;
					break;

				case NPCID.DukeFishron:
					npc.lifeMax = 81250;
					break;

				case NPCID.Golem:
					npc.lifeMax = 30000;
					break;

				case NPCID.GolemHead:
					npc.lifeMax = 20000;
					break;

				case NPCID.Plantera:
					npc.lifeMax = 75000;
					break;

				case NPCID.Retinazer:
					npc.lifeMax = 22000;
					break;

				case NPCID.Spazmatism:
					npc.lifeMax = 26000;
					break;

				case NPCID.WallofFlesh:
				case NPCID.WallofFleshEye:
					npc.lifeMax = 12800;
					break;

				case NPCID.BrainofCthulhu:
					npc.lifeMax = 1400;
					break;

				case NPCID.EaterofWorldsBody:
				case NPCID.EaterofWorldsHead:
				case NPCID.EaterofWorldsTail:
					npc.lifeMax = 175;
					break;

				case NPCID.EyeofCthulhu:
					npc.lifeMax = 3050;
					break;
			}
		}
		#endregion

		#region Boss Rush Stat Changes
		private void BossRushStatChanges(NPC npc, Mod mod)
        {
            if (!npc.friendly)
            {
				npc.buffImmune[BuffType<Enraged>()] = false;
				npc.buffImmune[BuffType<PearlAura>()] = true;
            }

            foreach (KeyValuePair<int, int> BossRushHPChange in BossRushHPChanges)
            {
                if (npc.type == BossRushHPChange.Key)
                {
                    npc.lifeMax = BossRushHPChange.Value;
                    break;
                }
            }
        }
        #endregion

        #region Boss Value Changes
        private void BossValueChanges(NPC npc)
        {
            foreach (KeyValuePair<int, int> BossValue in BossValues)
            {
                if (npc.type == BossValue.Key)
                {
                    npc.value = BossValue.Value;
                    break;
                }
            }
        }
        #endregion

        #region Draedon Mecha Mayhem Stat Changes
        private void DraedonMechaMayhemStatChanges(NPC npc)
        {
            switch (npc.type)
            {
                case NPCID.TheDestroyer:
                case NPCID.TheDestroyerBody:
                case NPCID.TheDestroyerTail:
                    npc.lifeMax = (int)(npc.lifeMax * 1.8);
                    npc.scale = 1.5f;
                    npc.npcSlots = 10f;
                    break;

                case NPCID.Probe:
                    npc.lifeMax = (int)(npc.lifeMax * 1.6);
                    npc.scale *= 1.2f;
                    break;

                case NPCID.SkeletronPrime:
                    npc.lifeMax = (int)(npc.lifeMax * 1.45);
                    npc.npcSlots = 12f;
                    break;

                case NPCID.PrimeVice:
                case NPCID.PrimeCannon:
                case NPCID.PrimeSaw:
                case NPCID.PrimeLaser:
                    npc.lifeMax = (int)(npc.lifeMax * 0.8);
                    break;

                case NPCID.Retinazer:
                case NPCID.Spazmatism:
                    npc.lifeMax = (int)(npc.lifeMax * 1.8);
                    npc.npcSlots = 10f;
                    break;
            }
        }
		#endregion

		#region Revengeance and Death Mode Stat Changes
		private void RevDeathStatChanges(NPC npc, Mod mod)
        {
            npc.value = (int)(npc.value * 1.5);

			if (DeathModeSplittingWormIDs.Contains(npc.type))
			{
				if (CalamityWorld.death)
					npc.lifeMax = (int)(npc.lifeMax * 0.25);
			}

            if (npc.type == NPCID.Mothron)
            {
                npc.scale = 1.25f;
            }
            else if (npc.type == NPCID.MoonLordCore || npc.type == NPCID.MoonLordHand || npc.type == NPCID.MoonLordHead || npc.type == NPCID.MoonLordLeechBlob)
            {
                npc.lifeMax = (int)(npc.lifeMax * 1.2);

				if (npc.type == NPCID.MoonLordCore)
					npc.npcSlots = 36f;
            }
            else if (npc.type == NPCID.CultistBoss || (npc.type >= NPCID.CultistDragonHead && npc.type <= NPCID.CultistDragonTail))
            {
                npc.lifeMax = (int)(npc.lifeMax * 1.2);

				if (npc.type == NPCID.CultistBoss)
					npc.npcSlots = 20f;
			}
            else if (npc.type == NPCID.DukeFishron)
            {
                npc.lifeMax = (int)(npc.lifeMax * 1.2);
                npc.npcSlots = 20f;
            }
            else if (npc.type == NPCID.Sharkron || npc.type == NPCID.Sharkron2)
            {
                npc.lifeMax = (int)(npc.lifeMax * 5.0);
            }
            else if (npc.type == NPCID.Golem || npc.type == NPCID.GolemHead)
            {
                npc.lifeMax = (int)(npc.lifeMax * 1.2);

				if (npc.type == NPCID.Golem)
					npc.npcSlots = 64f;
            }
            else if (npc.type == NPCID.GolemHeadFree)
            {
                npc.lifeMax = (int)(npc.lifeMax * 1.7);
				npc.width = 88;
				npc.height = 90;
                npc.dontTakeDamage = false;
            }
            else if (npc.type == NPCID.Plantera)
            {
                npc.lifeMax = (int)(npc.lifeMax * 1.2);
                npc.npcSlots = 32f;
            }
            else if (npc.type == NPCID.WallofFlesh || npc.type == NPCID.WallofFleshEye)
            {
                npc.lifeMax = (int)(npc.lifeMax * 1.2);

                if (npc.type == NPCID.WallofFlesh)
                    npc.npcSlots = 20f;
            }
            else if (npc.type == NPCID.SkeletronHead)
            {
				if (CalamityWorld.death)
					npc.lifeMax = (int)(npc.lifeMax * 0.5);
				else
					npc.lifeMax = (int)(npc.lifeMax * 0.75);

				npc.npcSlots = 12f;
            }
            else if (npc.type == NPCID.SkeletronHand)
            {
				if (CalamityWorld.death)
					npc.lifeMax = (int)(npc.lifeMax * 0.65);
				else
					npc.lifeMax = (int)(npc.lifeMax * 0.9);
			}
            else if (npc.type == NPCID.QueenBee)
            {
                npc.lifeMax = (int)(npc.lifeMax * 1.2);
                npc.npcSlots = 14f;
            }
			else if ((npc.type == NPCID.Bee || npc.type == NPCID.BeeSmall) && CalamityPlayer.areThereAnyDamnBosses)
			{
				npc.lifeMax = (int)(npc.lifeMax * 1.4);
				npc.scale = 1.25f;
			}
            else if (npc.type == NPCID.BrainofCthulhu)
            {
                npc.lifeMax = (int)(npc.lifeMax * 1.2);
                npc.npcSlots = 12f;
            }
            else if (npc.type == NPCID.Creeper)
            {
                npc.lifeMax = (int)(npc.lifeMax * 1.1);
            }
            else if (EaterofWorldsIDs.Contains(npc.type))
            {
                npc.lifeMax = (int)(npc.lifeMax * 1.2);

                if (npc.type == NPCID.EaterofWorldsHead)
                    npc.npcSlots = 10f;

				if (CalamityWorld.death)
					npc.scale = 1.1f;
			}
            else if (npc.type == NPCID.EyeofCthulhu)
            {
                npc.lifeMax = (int)(npc.lifeMax * 1.2);
                npc.npcSlots = 10f;
            }
			else if (npc.type == NPCID.KingSlime)
			{
				if (CalamityWorld.death)
					npc.scale = 3f;
			}
            else if (npc.type == NPCID.Wraith || npc.type == NPCID.Mimic || npc.type == NPCID.Reaper || npc.type == NPCID.PresentMimic || npc.type == NPCID.SandElemental)
            {
                npc.knockBackResist = 0f;
            }

            if (!DraedonMayhem)
            {
                if (DestroyerIDs.Contains(npc.type))
                {
                    npc.lifeMax = (int)(npc.lifeMax * 1.25);
                    npc.scale = 1.5f;
                    npc.npcSlots = 10f;
                }
                else if (npc.type == NPCID.SkeletronPrime)
                {
					npc.lifeMax = (int)(npc.lifeMax * 1.2);
					npc.npcSlots = 12f;
                }
				else if (npc.type <= NPCID.PrimeLaser && npc.type >= NPCID.PrimeCannon)
				{
					npc.lifeMax = (int)(npc.lifeMax * 0.65);
				}
                else if (npc.type == NPCID.Retinazer || npc.type == NPCID.Spazmatism)
                {
                    npc.lifeMax = (int)(npc.lifeMax * 1.2);
                    npc.npcSlots = 10f;
                }
            }

            if (CalamityLists.revengeanceLifeStealExceptionList.Contains(npc.type))
            {
                npc.canGhostHeal = false;
            }
        }
		#endregion

		#region Timed DR Stats
		private void TimedDRScaleFactors(NPC npc)
		{
			bool malice = CalamityWorld.malice;
			bool bossHasBeenDowned = GetDownedBossVariable(npc.type);

			// Every boss post-DoG has normal timed DR until it is downed
			bool removePostDoGTimedDR = bossHasBeenDowned;

			// Post-Provi Pre-DoG timed DR is removed when anything DoG and onward is defeated
			bool removePostProviPreDoGTimedDR = removePostDoGTimedDR || CalamityWorld.downedAdultEidolonWyrm ||
				CalamityWorld.downedSCal || CalamityWorld.downedExoMechs || CalamityWorld.downedYharon || CalamityWorld.downedDoG;

			// Post-ML Pre-Provi timed DR is removed when anything Providence and onward is defeated
			bool removePostMLPreProviTimedDR = removePostProviPreDoGTimedDR || CalamityWorld.downedBoomerDuke ||
				CalamityWorld.downedPolterghast || CalamityWorld.downedSentinel1 || CalamityWorld.downedSentinel2 ||
				CalamityWorld.downedSentinel3 || CalamityWorld.downedProvidence;

			// Late Hardmode timed DR is removed when anything Moon Lord and onward is defeated
			bool removeLateHardmodeTimedDR = removePostMLPreProviTimedDR || CalamityWorld.downedBumble ||
				CalamityWorld.downedGuardians || NPC.downedMoonlord;

			// Early Hardmode timed DR is removed when anything Plantera and onward is defeated
			bool removeEarlyHardmodeTimedDR = removeLateHardmodeTimedDR || CalamityWorld.downedStarGod ||
				NPC.downedAncientCultist || CalamityWorld.downedScavenger || NPC.downedFishron ||
				CalamityWorld.downedPlaguebringer || NPC.downedGolemBoss || CalamityWorld.downedAstrageldon ||
				CalamityWorld.downedLeviathan || NPC.downedPlantBoss;

			// Pre Hardmode timed DR is removed when anything Wall of Flesh and onward is defeated
			bool removePreHardmodeTimedDR = removeEarlyHardmodeTimedDR || CalamityWorld.downedCalamitas ||
				NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3 || CalamityWorld.downedBrimstoneElemental ||
				CalamityWorld.downedAquaticScourge || CalamityWorld.downedCryogen || Main.hardMode;

			// Vanilla bosses
			switch (npc.type)
			{
				// Pre Hardmode bosses
				case NPCID.KingSlime:
				case NPCID.EyeofCthulhu:
				case NPCID.EaterofWorldsHead:
				case NPCID.EaterofWorldsBody:
				case NPCID.EaterofWorldsTail:
				case NPCID.BrainofCthulhu:
				case NPCID.Creeper:
				case NPCID.QueenBee:
				case NPCID.SkeletronHead:
				case NPCID.WallofFlesh:
				case NPCID.WallofFleshEye:

					TimedDRScaleFactor = removePreHardmodeTimedDR ? timedDR_Disabled : malice ? timedDR_Malice : timedDR_Disabled;

					break;

				// The Destroyer
				case NPCID.TheDestroyer:
				case NPCID.TheDestroyerBody:
				case NPCID.TheDestroyerTail:

					TimedDRScaleFactor = removeEarlyHardmodeTimedDR ? timedDR_Disabled : malice ? timedDR_Worm : timedDR_Disabled;

					break;

				// Early Hardmode bosses
				case NPCID.Spazmatism:
				case NPCID.Retinazer:
				case NPCID.SkeletronPrime:
				case NPCID.Plantera:

					TimedDRScaleFactor = removeEarlyHardmodeTimedDR ? timedDR_Disabled : malice ? timedDR_Malice : timedDR_Disabled;

					break;

				// Late Hardmode bosses
				case NPCID.Golem:
				case NPCID.GolemHead:
				case NPCID.DukeFishron:
				case NPCID.CultistBoss:
				case NPCID.MoonLordCore:
				case NPCID.MoonLordHead:
				case NPCID.MoonLordHand:

					TimedDRScaleFactor = removeLateHardmodeTimedDR ? timedDR_Disabled : malice ? timedDR_Malice : timedDR_Disabled;

					break;
			}

			// Calamity bosses
			bool preHardmodeCalamityBosses = DesertScourgeIDs.Contains(npc.type) || npc.type == NPCType<CrabulonIdle>() ||
				npc.type == NPCType<HiveMind.HiveMind>() || npc.type == NPCType<PerforatorHive>() || npc.type == NPCType<SlimeGod.SlimeGod>() ||
				npc.type == NPCType<SlimeGodRun>() || npc.type == NPCType<SlimeGodSplit>() || npc.type == NPCType<SlimeGodRunSplit>() ||
				npc.type == NPCType<SlimeGodCore>();

			bool earlyHardmodeCalamityBosses = npc.type == NPCType<Cryogen.Cryogen>() || AquaticScourgeIDs.Contains(npc.type) ||
				npc.type == NPCType<BrimstoneElemental.BrimstoneElemental>() || npc.type == NPCType<CalamitasRun3>();

			bool lateHardmodeCalamityBosses = npc.type == NPCType<Siren>() || npc.type == NPCType<Leviathan.Leviathan>() ||
				npc.type == NPCType<AstrumAureus.AstrumAureus>() || npc.type == NPCType<PlaguebringerGoliath.PlaguebringerGoliath>() ||
				npc.type == NPCType<RavagerBody>() || AstrumDeusIDs.Contains(npc.type);

			bool postMLPreProviCalamityBosses = npc.type == NPCType<ProfanedGuardianBoss>() || npc.type == NPCType<Bumblefuck>() ||
				npc.type == NPCType<Providence.Providence>();

			bool postProviPreDoGCalamityBosses = npc.type == NPCType<CeaselessVoid.CeaselessVoid>() || npc.type == NPCType<DarkEnergy>() ||
				StormWeaverIDs.Contains(npc.type) || npc.type == NPCType<Signus.Signus>() || npc.type == NPCType<Polterghast.Polterghast>() ||
				npc.type == NPCType<OldDuke.OldDuke>() || DevourerOfGodsIDs.Contains(npc.type);

			bool postDoGCalamityBosses = npc.type == NPCType<Yharon.Yharon>() || npc.type == NPCType<SupremeCalamitas.SupremeCalamitas>() ||
				ThanatosIDs.Contains(npc.type) || npc.type == NPCType<Apollo>() || npc.type == NPCType<Artemis>() ||
				AresIDs.Contains(npc.type) || npc.type == NPCType<EidolonWyrmHeadHuge>();

			if (preHardmodeCalamityBosses)
			{
				TimedDRScaleFactor = removePreHardmodeTimedDR ? timedDR_Disabled : malice ? (DesertScourgeIDs.Contains(npc.type) ? timedDR_Worm : timedDR_Malice) : timedDR_Disabled;
			}
			else if (earlyHardmodeCalamityBosses)
			{
				TimedDRScaleFactor = removeEarlyHardmodeTimedDR ? timedDR_Disabled : malice ? (AquaticScourgeIDs.Contains(npc.type) ? timedDR_Worm : timedDR_Malice) : timedDR_Disabled;
			}
			else if (lateHardmodeCalamityBosses)
			{
				TimedDRScaleFactor = removeLateHardmodeTimedDR ? timedDR_Disabled : malice ? (AstrumDeusIDs.Contains(npc.type) ? timedDR_Worm : timedDR_Malice) : timedDR_Disabled;
			}
			else if (postMLPreProviCalamityBosses)
			{
				TimedDRScaleFactor = removePostMLPreProviTimedDR ? timedDR_Disabled : malice ? timedDR_Malice : timedDR_Disabled;
			}
			else if (postProviPreDoGCalamityBosses)
			{
				TimedDRScaleFactor = removePostProviPreDoGTimedDR ? timedDR_Disabled : malice ? (StormWeaverIDs.Contains(npc.type) ? timedDR_Worm : timedDR_Malice) : timedDR_Disabled;
			}
			else if (postDoGCalamityBosses)
			{
				TimedDRScaleFactor = removePostDoGTimedDR ? timedDR_Disabled : malice ? timedDR_Malice : timedDR_Disabled;
			}

			// Reduce timed DR in lower difficulties
			if (!Main.expertMode)
				TimedDRScaleFactor *= timedDRMult_Normal;
			else if (!CalamityWorld.revenge)
				TimedDRScaleFactor *= timedDRMult_Expert;
		}
		#endregion

		#region Vulnerabilities and Resistances
		private void VulnerabilitiesAndResistances(NPC npc)
		{
			// These enemies are categorized in such a way to make them easy to understand.
			// Do not mess with these categories unless you ask me for permission - Fab.
			switch (npc.type)
			{
				// Regular organic desert enemies.
				case NPCID.Antlion:
				case NPCID.FlyingAntlion:
				case NPCID.WalkingAntlion:
				case NPCID.TombCrawlerHead:
				case NPCID.TombCrawlerBody:
				case NPCID.TombCrawlerTail:
				case NPCID.DesertBeast:
				case NPCID.DuneSplicerHead:
				case NPCID.DuneSplicerBody:
				case NPCID.DuneSplicerTail:
				case NPCID.DesertLamiaDark:
				case NPCID.DesertLamiaLight:
				case NPCID.DesertGhoul:
				case NPCID.DesertGhoulCorruption:
				case NPCID.DesertGhoulCrimson:
				case NPCID.DesertGhoulHallow:
				case NPCID.Mummy:
				case NPCID.DarkMummy:
				case NPCID.LightMummy:
				case NPCID.Tumbleweed:
				case NPCID.SandShark:
				case NPCID.SandsharkCorrupt:
				case NPCID.SandsharkCrimson:
				case NPCID.SandsharkHallow:
				case NPCID.SandElemental:
					VulnerableToCold = true;
					VulnerableToSickness = true;
					VulnerableToWater = true;
					break;

				// Scorpions.
				case NPCID.DesertScorpionWalk:
				case NPCID.DesertScorpionWall:
					VulnerableToCold = true;
					VulnerableToSickness = false;
					VulnerableToWater = true;
					break;

				// Desert slimes.
				case NPCID.SandSlime:
					VulnerableToCold = true;
					VulnerableToSickness = false;
					VulnerableToWater = true;
					VulnerableToHeat = true;
					break;

				// Organic undead or other enemies that are covered in slime.
				case NPCID.ArmedZombieSlimed:
				case NPCID.BigSlimedZombie:
				case NPCID.BunnySlimed:
				case NPCID.SlimedZombie:
				case NPCID.SmallSlimedZombie:
					VulnerableToCold = true;
					VulnerableToHeat = true;
					break;

				// Slimes that use heat-related attacks.
				case NPCID.LavaSlime:
					VulnerableToCold = true;
					VulnerableToSickness = false;
					VulnerableToHeat = false;
					VulnerableToWater = true;
					break;

				// Regular slimes.
				case NPCID.DungeonSlime:
				case NPCID.BabySlime:
				case NPCID.BlackSlime:
				case NPCID.BlueSlime:
				case NPCID.CorruptSlime:
				case NPCID.GreenSlime:
				case NPCID.IlluminantSlime:
				case NPCID.JungleSlime:
				case NPCID.KingSlime:
				case NPCID.MotherSlime:
				case NPCID.PurpleSlime:
				case NPCID.RainbowSlime:
				case NPCID.RedSlime:
				case NPCID.Slimeling:
				case NPCID.SlimeMasked:
				case NPCID.Slimer:
				case NPCID.Slimer2:
				case NPCID.SlimeRibbonGreen:
				case NPCID.SlimeRibbonRed:
				case NPCID.SlimeRibbonWhite:
				case NPCID.SlimeRibbonYellow:
				case NPCID.SlimeSpiked:
				case NPCID.SpikedJungleSlime:
				case NPCID.UmbrellaSlime:
				case NPCID.YellowSlime:
				case NPCID.ToxicSludge:
				case NPCID.Crimslime:
				case NPCID.BigCrimslime:
				case NPCID.LittleCrimslime:
				case NPCID.Gastropod:
				case NPCID.Pinky:
					VulnerableToSickness = false;
					VulnerableToHeat = true;
					break;

				// Skeletons and other armored/bone enemies that use heat-related attacks.
				case NPCID.HellArmoredBones:
				case NPCID.HellArmoredBonesMace:
				case NPCID.HellArmoredBonesSpikeShield:
				case NPCID.HellArmoredBonesSword:
				case NPCID.DiabolistRed:
				case NPCID.DiabolistWhite:
					VulnerableToHeat = false;
					VulnerableToCold = true;
					VulnerableToSickness = false;
					VulnerableToWater = true;
					break;

				// Skeletons and other armored/bone enemies that are dead or undead.
				case NPCID.SkeletronHand:
				case NPCID.SkeletronHead:
				case NPCID.AngryBones:
				case NPCID.AngryBonesBig:
				case NPCID.AngryBonesBigHelmet:
				case NPCID.AngryBonesBigMuscle:
				case NPCID.DarkCaster:
				case NPCID.CursedSkull:
				case NPCID.GiantCursedSkull:
				case NPCID.DungeonGuardian:
				case NPCID.BigBoned:
				case NPCID.BlueArmoredBones:
				case NPCID.BlueArmoredBonesMace:
				case NPCID.BlueArmoredBonesNoPants:
				case NPCID.BlueArmoredBonesSword:
				case NPCID.BoneLee:
				case NPCID.BoneSerpentBody:
				case NPCID.BoneSerpentHead:
				case NPCID.BoneSerpentTail:
				case NPCID.BoneThrowingSkeleton:
				case NPCID.BoneThrowingSkeleton2:
				case NPCID.BoneThrowingSkeleton3:
				case NPCID.BoneThrowingSkeleton4:
				case NPCID.RustyArmoredBonesAxe:
				case NPCID.RustyArmoredBonesFlail:
				case NPCID.RustyArmoredBonesSword:
				case NPCID.RustyArmoredBonesSwordNoArmor:
				case NPCID.ShortBones:
				case NPCID.Necromancer:
				case NPCID.NecromancerArmored:
				case NPCID.RaggedCaster:
				case NPCID.RaggedCasterOpenCoat:
				case NPCID.SkeletonCommando:
				case NPCID.ArmoredSkeleton:
				case NPCID.BigHeadacheSkeleton:
				case NPCID.BigMisassembledSkeleton:
				case NPCID.BigPantlessSkeleton:
				case NPCID.BigSkeleton:
				case NPCID.DD2SkeletonT1:
				case NPCID.DD2SkeletonT3:
				case NPCID.GreekSkeleton:
				case NPCID.HeadacheSkeleton:
				case NPCID.HeavySkeleton:
				case NPCID.MisassembledSkeleton:
				case NPCID.PantlessSkeleton:
				case NPCID.Skeleton:
				case NPCID.SkeletonAlien:
				case NPCID.SkeletonArcher:
				case NPCID.SkeletonAstonaut:
				case NPCID.SkeletonSniper:
				case NPCID.SkeletonTopHat:
				case NPCID.SmallHeadacheSkeleton:
				case NPCID.SmallMisassembledSkeleton:
				case NPCID.SmallPantlessSkeleton:
				case NPCID.SmallSkeleton:
				case NPCID.TacticalSkeleton:
				case NPCID.Tim:
				case NPCID.UndeadMiner:
				case NPCID.UndeadViking:
				case NPCID.ArmoredViking:
				case NPCID.GraniteFlyer:
				case NPCID.GraniteGolem:
				case NPCID.RuneWizard:
				case NPCID.Golem:
				case NPCID.GolemFistLeft:
				case NPCID.GolemFistRight:
				case NPCID.GolemHead:
				case NPCID.GolemHeadFree:
					VulnerableToSickness = false;
					VulnerableToWater = true;
					break;

				// Metal non-robotic enemies
				case NPCID.BigMimicCorruption:
				case NPCID.BigMimicCrimson:
				case NPCID.BigMimicHallow:
				case NPCID.BigMimicJungle:
				case NPCID.Paladin:
				case NPCID.Mimic:
				case NPCID.PresentMimic:
				case NPCID.PirateShipCannon:
				case NPCID.PossessedArmor:
					VulnerableToSickness = false;
					break;

				// Robotic enemies.
				case NPCID.Probe:
				case NPCID.MartianProbe:
				case NPCID.DeadlySphere:
				case NPCID.MartianDrone:
				case NPCID.MartianWalker:
				case NPCID.MartianTurret:
				case NPCID.ElfCopter:
				case NPCID.SkeletronPrime:
				case NPCID.PrimeCannon:
				case NPCID.PrimeLaser:
				case NPCID.PrimeSaw:
				case NPCID.PrimeVice:
				case NPCID.TheDestroyer:
				case NPCID.TheDestroyerBody:
				case NPCID.TheDestroyerTail:
				case NPCID.SantaNK1:
				case NPCID.MartianSaucer:
				case NPCID.MartianSaucerCannon:
				case NPCID.MartianSaucerCore:
				case NPCID.MartianSaucerTurret:
					VulnerableToElectricity = true;
					VulnerableToSickness = false;
					break;

				// Ghostly or ethereal enemies.
				case NPCID.DungeonSpirit:
				case NPCID.AncientCultistSquidhead:
				case NPCID.CultistDragonBody1:
				case NPCID.CultistDragonBody2:
				case NPCID.CultistDragonBody3:
				case NPCID.CultistDragonBody4:
				case NPCID.CultistDragonHead:
				case NPCID.CultistDragonTail:
				case NPCID.Ghost:
				case NPCID.ChaosElemental:
				case NPCID.CrimsonAxe:
				case NPCID.EnchantedSword:
				case NPCID.CursedHammer:
				case NPCID.DesertDjinn:
				case NPCID.Wraith:
				case NPCID.ShadowFlameApparition:
				case NPCID.Reaper:
				case NPCID.Poltergeist:
				case NPCID.Pixie:
					VulnerableToSickness = false;
					break;

				// Organic enemies.
				case NPCID.CultistArcherBlue:
				case NPCID.CultistArcherWhite:
				case NPCID.CultistBoss:
				case NPCID.CultistDevote:
				case NPCID.BloodCrawler:
				case NPCID.BloodCrawlerWall:
				case NPCID.CaveBat:
				case NPCID.GiantBat:
				case NPCID.CochinealBeetle:
				case NPCID.CyanBeetle:
				case NPCID.LacBeetle:
				case NPCID.AnomuraFungus:
				case NPCID.GiantFungiBulb:
				case NPCID.FungiBulb:
				case NPCID.MushiLadybug:
				case NPCID.ZombieMushroom:
				case NPCID.ZombieMushroomHat:
				case NPCID.ManEater:
				case NPCID.Snatcher:
				case NPCID.AngryTrapper:
				case NPCID.HoppinJack:
				case NPCID.Splinterling:
				case NPCID.MourningWood:
				case NPCID.Pumpking:
				case NPCID.Everscream:
				case NPCID.Crimera:
				case NPCID.BigCrimera:
				case NPCID.LittleCrimera:
				case NPCID.DemonEye:
				case NPCID.DemonEye2:
				case NPCID.DemonEyeOwl:
				case NPCID.DemonEyeSpaceship:
				case NPCID.DevourerBody:
				case NPCID.DevourerHead:
				case NPCID.DevourerTail:
				case NPCID.DoctorBones:
				case NPCID.EaterofSouls:
				case NPCID.BigEater:
				case NPCID.LittleEater:
				case NPCID.EaterofWorldsBody:
				case NPCID.EaterofWorldsHead:
				case NPCID.EaterofWorldsTail:
				case NPCID.FaceMonster:
				case NPCID.GiantShelly:
				case NPCID.GiantShelly2:
				case NPCID.GiantWormBody:
				case NPCID.GiantWormHead:
				case NPCID.GiantWormTail:
				case NPCID.GoblinScout:
				case NPCID.Harpy:
				case NPCID.JungleBat:
				case NPCID.Nymph:
				case NPCID.Raven:
				case NPCID.Salamander:
				case NPCID.Salamander2:
				case NPCID.Salamander3:
				case NPCID.Salamander4:
				case NPCID.Salamander5:
				case NPCID.Salamander6:
				case NPCID.Salamander7:
				case NPCID.Salamander8:
				case NPCID.Salamander9:
				case NPCID.Vulture:
				case NPCID.WallCreeper:
				case NPCID.WallCreeperWall:
				case NPCID.ArmedZombie:
				case NPCID.ArmedZombieCenx:
				case NPCID.ArmedZombiePincussion:
				case NPCID.ArmedZombieSwamp:
				case NPCID.ArmedZombieTwiggy:
				case NPCID.BaldZombie:
				case NPCID.BigBaldZombie:
				case NPCID.BigFemaleZombie:
				case NPCID.BigPincushionZombie:
				case NPCID.BigRainZombie:
				case NPCID.BigSwampZombie:
				case NPCID.BigTwiggyZombie:
				case NPCID.BigZombie:
				case NPCID.BloodZombie:
				case NPCID.FemaleZombie:
				case NPCID.PincushionZombie:
				case NPCID.SmallBaldZombie:
				case NPCID.SmallFemaleZombie:
				case NPCID.SmallPincushionZombie:
				case NPCID.SmallRainZombie:
				case NPCID.SmallSwampZombie:
				case NPCID.SmallTwiggyZombie:
				case NPCID.SmallZombie:
				case NPCID.SwampZombie:
				case NPCID.TwiggyZombie:
				case NPCID.Zombie:
				case NPCID.ZombieDoctor:
				case NPCID.ZombiePixie:
				case NPCID.ZombieRaincoat:
				case NPCID.ZombieSuperman:
				case NPCID.ZombieSweater:
				case NPCID.ZombieXmas:
				case NPCID.Clinger:
				case NPCID.Corruptor:
				case NPCID.Derpling:
				case NPCID.Herpling:
				case NPCID.DiggerBody:
				case NPCID.DiggerHead:
				case NPCID.DiggerTail:
				case NPCID.FloatyGross:
				case NPCID.FlyingSnake:
				case NPCID.Lihzahrd:
				case NPCID.LihzahrdCrawler:
				case NPCID.GiantFlyingFox:
				case NPCID.GiantTortoise:
				case NPCID.IchorSticker:
				case NPCID.IlluminantBat:
				case NPCID.Medusa:
				case NPCID.Moth:
				case NPCID.Unicorn:
				case NPCID.WanderingEye:
				case NPCID.Werewolf:
				case NPCID.SeekerBody:
				case NPCID.SeekerHead:
				case NPCID.SeekerTail:
				case NPCID.WyvernBody:
				case NPCID.WyvernBody2:
				case NPCID.WyvernBody3:
				case NPCID.WyvernHead:
				case NPCID.WyvernLegs:
				case NPCID.WyvernTail:
				case NPCID.Clown:
				case NPCID.CorruptBunny:
				case NPCID.CrimsonBunny:
				case NPCID.Drippler:
				case NPCID.TheGroom:
				case NPCID.TheBride:
				case NPCID.GoblinArcher:
				case NPCID.GoblinPeon:
				case NPCID.GoblinSorcerer:
				case NPCID.GoblinSummoner:
				case NPCID.GoblinThief:
				case NPCID.GoblinWarrior:
				case NPCID.DD2DarkMageT1:
				case NPCID.DD2DarkMageT3:
				case NPCID.DD2DrakinT2:
				case NPCID.DD2DrakinT3:
				case NPCID.DD2GoblinBomberT1:
				case NPCID.DD2GoblinBomberT2:
				case NPCID.DD2GoblinBomberT3:
				case NPCID.DD2GoblinT1:
				case NPCID.DD2GoblinT2:
				case NPCID.DD2GoblinT3:
				case NPCID.DD2JavelinstT1:
				case NPCID.DD2JavelinstT2:
				case NPCID.DD2JavelinstT3:
				case NPCID.DD2KoboldFlyerT2:
				case NPCID.DD2KoboldFlyerT3:
				case NPCID.DD2KoboldWalkerT2:
				case NPCID.DD2KoboldWalkerT3:
				case NPCID.DD2OgreT2:
				case NPCID.DD2OgreT3:
				case NPCID.DD2WitherBeastT2:
				case NPCID.DD2WitherBeastT3:
				case NPCID.DD2WyvernT1:
				case NPCID.DD2WyvernT2:
				case NPCID.DD2WyvernT3:
				case NPCID.Parrot:
				case NPCID.PirateCaptain:
				case NPCID.PirateCorsair:
				case NPCID.PirateCrossbower:
				case NPCID.PirateDeadeye:
				case NPCID.PirateDeckhand:
				case NPCID.Mothron:
				case NPCID.MothronEgg:
				case NPCID.MothronSpawn:
				case NPCID.Butcher:
				case NPCID.DrManFly:
				case NPCID.Eyezor:
				case NPCID.Frankenstein:
				case NPCID.Fritz:
				case NPCID.Nailhead:
				case NPCID.Psycho:
				case NPCID.SwampThing:
				case NPCID.ThePossessed:
				case NPCID.Vampire:
				case NPCID.VampireBat:
				case NPCID.BrainScrambler:
				case NPCID.GigaZapper:
				case NPCID.GrayGrunt:
				case NPCID.MartianEngineer:
				case NPCID.MartianOfficer:
				case NPCID.RayGunner:
				case NPCID.Scutlix:
				case NPCID.ScutlixRider:
				case NPCID.HeadlessHorseman:
				case NPCID.Hellhound:
				case NPCID.Scarecrow1:
				case NPCID.Scarecrow2:
				case NPCID.Scarecrow3:
				case NPCID.Scarecrow4:
				case NPCID.Scarecrow5:
				case NPCID.Scarecrow6:
				case NPCID.Scarecrow7:
				case NPCID.Scarecrow8:
				case NPCID.Scarecrow9:
				case NPCID.Scarecrow10:
				case NPCID.NebulaHeadcrab:
				case NPCID.LunarTowerNebula:
				case NPCID.NebulaBeast:
				case NPCID.NebulaBrain:
				case NPCID.NebulaSoldier:
				case NPCID.LunarTowerSolar:
				case NPCID.SolarCorite:
				case NPCID.SolarCrawltipedeTail:
				case NPCID.SolarDrakomire:
				case NPCID.SolarDrakomireRider:
				case NPCID.SolarSolenian:
				case NPCID.SolarSpearman:
				case NPCID.SolarSroller:
				case NPCID.LunarTowerStardust:
				case NPCID.StardustCellBig:
				case NPCID.StardustCellSmall:
				case NPCID.StardustJellyfishBig:
				case NPCID.StardustSoldier:
				case NPCID.StardustSpiderBig:
				case NPCID.StardustSpiderSmall:
				case NPCID.StardustWormHead:
				case NPCID.LunarTowerVortex:
				case NPCID.VortexHornet:
				case NPCID.VortexHornetQueen:
				case NPCID.VortexLarva:
				case NPCID.VortexRifleman:
				case NPCID.VortexSoldier:
				case NPCID.BrainofCthulhu:
				case NPCID.Creeper:
				case NPCID.EyeofCthulhu:
				case NPCID.ServantofCthulhu:
				case NPCID.MoonLordCore:
				case NPCID.MoonLordHand:
				case NPCID.MoonLordHead:
				case NPCID.Spazmatism: // Changes to robotic in phase 2
				case NPCID.Retinazer: // Changes to robotic in phase 2
					VulnerableToCold = true;
					VulnerableToHeat = true;
					VulnerableToSickness = true;
					break;

				// Demons and shit.
				case NPCID.WallofFlesh:
				case NPCID.WallofFleshEye:
				case NPCID.TheHungry:
				case NPCID.TheHungryII:
				case NPCID.LeechBody:
				case NPCID.LeechHead:
				case NPCID.LeechTail:
				case NPCID.Demon:
				case NPCID.VoodooDemon:
				case NPCID.RedDevil:
				case NPCID.DemonTaxCollector:
					VulnerableToCold = true;
					VulnerableToHeat = false;
					VulnerableToSickness = true;
					break;

				// Fire enemies that are also organic.
				case NPCID.FireImp:
				case NPCID.Hellbat:
				case NPCID.Lavabat:
					VulnerableToCold = true;
					VulnerableToHeat = false;
					VulnerableToSickness = true;
					VulnerableToWater = true;
					break;

				// Fire enemies that aren't organic.
				case NPCID.MeteorHead:
					VulnerableToCold = true;
					VulnerableToHeat = false;
					VulnerableToSickness = false;
					VulnerableToWater = true;
					break;

				// Lightning bug thing.
				case NPCID.DD2LightningBugT3:
					VulnerableToElectricity = false;
					VulnerableToCold = true;
					VulnerableToHeat = true;
					VulnerableToSickness = true;
					break;

				// Betsy.
				case NPCID.DD2Betsy:
					VulnerableToCold = true;
					VulnerableToHeat = false;
					VulnerableToSickness = true;
					break;

				// Nimbus
				case NPCID.AngryNimbus:
					VulnerableToCold = true;
					VulnerableToElectricity = false;
					VulnerableToWater = false;
					VulnerableToHeat = false;
					VulnerableToSickness = false;
					break;

				// Cold-themed enemies
				case NPCID.ArmedZombieEskimo:
				case NPCID.ZombieEskimo:
				case NPCID.IceBat:
				case NPCID.SnowFlinx:
				case NPCID.IceTortoise:
				case NPCID.IcyMerman:
				case NPCID.PigronCorruption:
				case NPCID.PigronCrimson:
				case NPCID.PigronHallow:
				case NPCID.Wolf:
				case NPCID.CorruptPenguin:
				case NPCID.CrimsonPenguin:
				case NPCID.ElfArcher:
				case NPCID.Krampus:
				case NPCID.Yeti:
				case NPCID.Nutcracker:
				case NPCID.NutcrackerSpinning:
				case NPCID.ZombieElf:
				case NPCID.ZombieElfBeard:
				case NPCID.ZombieElfGirl:
					VulnerableToHeat = true;
					VulnerableToCold = false;
					VulnerableToSickness = true;
					break;

				// Cold-themed enemies that aren't organic.
				case NPCID.IceElemental:
				case NPCID.IceSlime:
				case NPCID.SpikedIceSlime:
				case NPCID.IceGolem:
				case NPCID.MisterStabby:
				case NPCID.SnowBalla:
				case NPCID.SnowmanGangsta:
				case NPCID.Flocko:
				case NPCID.IceQueen:
				case NPCID.GingerbreadMan:
					VulnerableToCold = false;
					VulnerableToHeat = true;
					VulnerableToSickness = false;
					break;

				// Water-themed enemies.
				case NPCID.Crawdad:
				case NPCID.Crawdad2:
				case NPCID.BlueJellyfish:
				case NPCID.GreenJellyfish:
				case NPCID.PinkJellyfish:
				case NPCID.BloodJelly:
				case NPCID.FungoFish:
				case NPCID.Crab:
				case NPCID.Piranha:
				case NPCID.SeaSnail:
				case NPCID.Squid:
				case NPCID.Shark:
				case NPCID.AnglerFish:
				case NPCID.Arapaima:
				case NPCID.BloodFeeder:
				case NPCID.CorruptGoldfish:
				case NPCID.CrimsonGoldfish:
				case NPCID.FlyingFish:
				case NPCID.CreatureFromTheDeep:
				case NPCID.DukeFishron:
				case NPCID.Sharkron:
				case NPCID.Sharkron2:
					VulnerableToHeat = false;
					VulnerableToSickness = true;
					VulnerableToElectricity = true;
					VulnerableToWater = false;
					break;

				// Fucking bees, hornets and poisonous/toxic stuff.
				case NPCID.Bee:
				case NPCID.BeeSmall:
				case NPCID.QueenBee:
				case NPCID.BigHornetFatty:
				case NPCID.BigHornetHoney:
				case NPCID.BigHornetLeafy:
				case NPCID.BigHornetSpikey:
				case NPCID.BigHornetStingy:
				case NPCID.BigMossHornet:
				case NPCID.GiantMossHornet:
				case NPCID.Hornet:
				case NPCID.HornetFatty:
				case NPCID.HornetHoney:
				case NPCID.HornetLeafy:
				case NPCID.HornetSpikey:
				case NPCID.HornetStingy:
				case NPCID.LittleHornetFatty:
				case NPCID.LittleHornetHoney:
				case NPCID.LittleHornetLeafy:
				case NPCID.LittleHornetSpikey:
				case NPCID.LittleHornetStingy:
				case NPCID.LittleMossHornet:
				case NPCID.MossHornet:
				case NPCID.TinyMossHornet:
				case NPCID.JungleCreeper:
				case NPCID.JungleCreeperWall:
				case NPCID.BlackRecluse:
				case NPCID.BlackRecluseWall:
				case NPCID.Plantera:
				case NPCID.PlanterasTentacle:
					VulnerableToCold = true;
					VulnerableToHeat = true;
					VulnerableToSickness = false;
					break;
			}
		}
		#endregion

		#region Other Stat Changes
		private void OtherStatChanges(NPC npc)
        {
			switch (npc.type)
			{
				case NPCID.KingSlime:
				case NPCID.EyeofCthulhu:
				case NPCID.BrainofCthulhu:
				case NPCID.QueenBee:
				case NPCID.Corruptor:
				case NPCID.Crawdad:
				case NPCID.Crawdad2:
				case NPCID.ManEater:
				case NPCID.AngryTrapper:
				case NPCID.Snatcher:
				case NPCID.SpikeBall:
				case NPCID.DesertBeast:
				case NPCID.BoneLee:
				case NPCID.Paladin:
				case NPCID.BigMimicCorruption:
				case NPCID.BigMimicCrimson:
				case NPCID.BigMimicHallow:
				case NPCID.DiggerHead:
				case NPCID.SeekerHead:
				case NPCID.DuneSplicerHead:
				case NPCID.SolarCrawltipedeHead:
				case NPCID.Mimic:
				case NPCID.SandShark:
				case NPCID.SandsharkCorrupt:
				case NPCID.SandsharkCrimson:
				case NPCID.SandsharkHallow:
				case NPCID.Butcher:
				case NPCID.DeadlySphere:
				case NPCID.Mothron:
				case NPCID.Reaper:
				case NPCID.Psycho:
				case NPCID.PresentMimic:
				case NPCID.Yeti:
				case NPCID.NebulaBeast:
				case NPCID.SolarCorite:
				case NPCID.StardustWormHead:
				case NPCID.EaterofWorldsHead:
				case NPCID.SkeletronHead:
				case NPCID.SkeletronHand:
				case NPCID.WallofFlesh:
				case NPCID.TheHungry:
				case NPCID.TheHungryII:
				case NPCID.Spazmatism:
				case NPCID.Retinazer:
				case NPCID.TheDestroyer:
				case NPCID.TheDestroyerBody:
				case NPCID.TheDestroyerTail:
				case NPCID.SkeletronPrime:
				case NPCID.PrimeVice:
				case NPCID.PrimeSaw:
				case NPCID.Plantera:
				case NPCID.PlanterasTentacle:
				case NPCID.Golem:
				case NPCID.GolemFistLeft:
				case NPCID.GolemFistRight:
				case NPCID.CultistDragonHead:
				case NPCID.AncientCultistSquidhead:
				case NPCID.AncientLight:
				case NPCID.DD2OgreT2:
				case NPCID.DD2OgreT3:
				case NPCID.DD2Betsy:
				case NPCID.PumpkingBlade:
				case NPCID.SantaNK1:
				case NPCID.DukeFishron:
					canBreakPlayerDefense = true;
					break;

				// Reduce prehardmode desert enemy stats pre-Desert Scourge
				case NPCID.WalkingAntlion:

					if (!CalamityWorld.downedDesertScourge)
					{
						npc.lifeMax = (int)(npc.lifeMax * DesertEnemyStatMultiplier);
						npc.damage = (int)(npc.damage * DesertEnemyStatMultiplier);
						npc.defDamage = npc.damage;
						npc.defense /= 2;
						npc.defDefense = npc.defense;
					}
					else
						canBreakPlayerDefense = true;

					break;

				case NPCID.Antlion:
				case NPCID.FlyingAntlion:
					if (!CalamityWorld.downedDesertScourge)
					{
						npc.lifeMax = (int)(npc.lifeMax * DesertEnemyStatMultiplier);
						npc.damage = (int)(npc.damage * DesertEnemyStatMultiplier);
						npc.defDamage = npc.damage;
						npc.defense /= 2;
						npc.defDefense = npc.defense;
					}
					break;

				// Reduce Dungeon Guardian HP
				case NPCID.DungeonGuardian:
					npc.lifeMax = (int)(npc.lifeMax * 0.1);
					canBreakPlayerDefense = true;
					break;

				// Reduce Tomb Crawler stats
				case NPCID.TombCrawlerHead:

					npc.lifeMax = (int)(npc.lifeMax * (CalamityWorld.downedDesertScourge ? 0.6 : 0.45));
					if (!CalamityWorld.downedDesertScourge)
					{
						npc.damage = (int)(npc.damage * DesertEnemyStatMultiplier);
						npc.defDamage = npc.damage;
						// Tomb Crawler Head has 0 defense so there is no need to reduce it
					}
					else
						canBreakPlayerDefense = true;

					break;

				case NPCID.TombCrawlerBody:
				case NPCID.TombCrawlerTail:

					npc.lifeMax = (int)(npc.lifeMax * (CalamityWorld.downedDesertScourge ? 0.6 : 0.45));
					if (!CalamityWorld.downedDesertScourge)
					{
						npc.damage = (int)(npc.damage * DesertEnemyStatMultiplier);
						npc.defDamage = npc.damage;
						npc.defense /= 2;
						npc.defDefense = npc.defense;
					}

					break;

				// Fix Sharkron hitboxes
				case NPCID.Sharkron:
				case NPCID.Sharkron2:
					npc.width = npc.height = 36;
					canBreakPlayerDefense = true;
					break;

				// Make Core hitbox bigger
				case NPCID.MartianSaucerCore:
					npc.width *= 2;
					npc.height *= 2;
					break;

				// Nerf Green Jellyfish because they spawn in prehardmode
				case NPCID.GreenJellyfish:
					npc.damage = 40;
					npc.defDamage = npc.damage;
					npc.defense = 4;
					npc.defDefense = npc.defense;
					break;

				default:
					break;
			}

			// Reduce mech boss HP and damage depending on the new ore progression changes
			if (CalamityConfig.Instance.EarlyHardmodeProgressionRework && !BossRushEvent.BossRushActive)
			{
				if (!NPC.downedMechBossAny)
				{
					if (DestroyerIDs.Contains(npc.type) || npc.type == NPCID.Probe || SkeletronPrimeIDs.Contains(npc.type) || npc.type == NPCID.Spazmatism || npc.type == NPCID.Retinazer)
					{
						npc.lifeMax = (int)(npc.lifeMax * 0.8);
						npc.damage = (int)(npc.damage * 0.8);
						npc.defDamage = npc.damage;
					}
				}
				else if ((!NPC.downedMechBoss1 && !NPC.downedMechBoss2) || (!NPC.downedMechBoss2 && !NPC.downedMechBoss3) || (!NPC.downedMechBoss3 && !NPC.downedMechBoss1))
				{
					if (DestroyerIDs.Contains(npc.type) || npc.type == NPCID.Probe || SkeletronPrimeIDs.Contains(npc.type) || npc.type == NPCID.Spazmatism || npc.type == NPCID.Retinazer)
					{
						npc.lifeMax = (int)(npc.lifeMax * 0.9);
						npc.damage = (int)(npc.damage * 0.9);
						npc.defDamage = npc.damage;
					}
				}
			}

			if (Main.hardMode && HardmodeNPCNerfList.Contains(npc.type))
			{
				npc.damage = (int)(npc.damage * 0.75);
				npc.defDamage = npc.damage;
			}

			if (CalamityWorld.downedDoG)
            {
                if (CalamityLists.pumpkinMoonBuffList.Contains(npc.type))
                {
                    npc.lifeMax = (int)(npc.lifeMax * 3.5);
                    npc.damage += 30;
                    npc.life = npc.lifeMax;
                    npc.defDamage = npc.damage;
                }
                else if (CalamityLists.frostMoonBuffList.Contains(npc.type))
                {
                    npc.lifeMax = (int)(npc.lifeMax * 2.5);
                    npc.damage += 30;
                    npc.life = npc.lifeMax;
                    npc.defDamage = npc.damage;
                }
				else if (CalamityLists.eclipseBuffList.Contains(npc.type))
				{
					npc.lifeMax = (int)(npc.lifeMax * 5D);
					npc.damage += 30;
					npc.life = npc.lifeMax;
					npc.defDamage = npc.damage;
				}
			}

            if (NPC.downedMoonlord)
            {
                if (CalamityLists.dungeonEnemyBuffList.Contains(npc.type))
                {
                    npc.lifeMax = (int)(npc.lifeMax * 2.5);
                    npc.damage += 30;
                    npc.life = npc.lifeMax;
                    npc.defDamage = npc.damage;
                }
            }

            if (CalamityWorld.revenge)
            {
				double damageMultiplier = 1D;
				bool containsNPC = false;
                if (CalamityLists.revengeanceEnemyBuffList25Percent.Contains(npc.type))
                {
					damageMultiplier += 0.25;
					containsNPC = true;
                }
				else if (CalamityLists.revengeanceEnemyBuffList20Percent.Contains(npc.type))
				{
					damageMultiplier += 0.2;
					containsNPC = true;
				}
				else if (CalamityLists.revengeanceEnemyBuffList15Percent.Contains(npc.type))
				{
					damageMultiplier += 0.15;
					containsNPC = true;
				}
				else if (CalamityLists.revengeanceEnemyBuffList10Percent.Contains(npc.type))
				{
					damageMultiplier += 0.1;
					containsNPC = true;
				}

				if (containsNPC)
				{
					if (CalamityWorld.death)
						damageMultiplier += (damageMultiplier - 1D) * 0.6;

					npc.damage = (int)(npc.damage * damageMultiplier);
					npc.defDamage = npc.damage;
				}
			}

			if (npc.type < NPCID.Count && NPCStats.EnemyStats.ContactDamageValues.ContainsKey(npc.type))
			{
				npc.GetNPCDamage();
				npc.defDamage = npc.damage;
			}

            if ((npc.boss && npc.type != NPCID.MartianSaucerCore && npc.type < NPCID.Count) || CalamityLists.bossHPScaleList.Contains(npc.type))
            {
                double HPBoost = CalamityConfig.Instance.BossHealthBoost * 0.01;
                npc.lifeMax += (int)(npc.lifeMax * HPBoost);
            }
        }
        #endregion

        #region Special Drawing
        public static void DrawGlowmask(NPC npc, SpriteBatch spriteBatch, Texture2D texture = null, bool invertedDirection = false, Vector2 offset = default)
        {
            if (texture is null)
                texture = Main.npcTexture[npc.type];
            SpriteEffects effects = npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            if (invertedDirection)
                effects = npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            spriteBatch.Draw(texture,
                             npc.Center - Main.screenPosition + offset,
                             npc.frame,
                             Color.White,
                             npc.rotation,
                             npc.Size / 2f,
                             npc.scale,
                             effects,
                             0f);
        }

        public static void DrawAfterimage(NPC npc, SpriteBatch spriteBatch, Color startingColor, Color endingColor, Texture2D texture = null, 
            Func<NPC, int, float> rotationCalculation = null, bool directioning = false, bool invertedDirection = false)
        {
            if (NPCID.Sets.TrailingMode[npc.type] != 1)
                return;
            SpriteEffects spriteEffects = SpriteEffects.None;

            if (npc.spriteDirection == -1 && directioning)
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }

            if (invertedDirection)
            {
                spriteEffects ^= SpriteEffects.FlipHorizontally; // Same as x XOR 1, or x XOR TRUE, which inverts the bit. In this case, this reverses the horizontal flip
            }

            // Set the rotation calculation to a predefined value. The null default is solely so that 
            if (rotationCalculation is null)
            {
                rotationCalculation = (nPC, afterimageIndex) => nPC.rotation;
            }

            endingColor.A = 0;

            Color drawColor = npc.GetAlpha(startingColor);
            Texture2D npcTexture = texture ?? Main.npcTexture[npc.type];
            Vector2 origin = npc.Size * 0.5f;
            int afterimageCounter = 1;
            while (afterimageCounter < NPCID.Sets.TrailCacheLength[npc.type] && CalamityConfig.Instance.Afterimages)
            {
                Color colorToDraw = Color.Lerp(drawColor, endingColor, afterimageCounter / (float)NPCID.Sets.TrailCacheLength[npc.type]);
                colorToDraw *= afterimageCounter / (float)NPCID.Sets.TrailCacheLength[npc.type];
                spriteBatch.Draw(npcTexture,
                                 npc.oldPos[afterimageCounter] + npc.Size / 2f - Main.screenPosition + Vector2.UnitY * npc.gfxOffY,
                                 npc.frame,
                                 colorToDraw,
                                 rotationCalculation.Invoke(npc, afterimageCounter),
                                 origin,
                                 npc.scale,
                                 spriteEffects,
                                 0f);
                afterimageCounter++;
            }
        }
        #endregion

        #region Scale Expert Multiplayer Stats
        public override void ScaleExpertStats(NPC npc, int numPlayers, float bossLifeScale)
        {
            // Do absolutely nothing in single player, or in multiplayer with only one player connected.
            if (Main.netMode == NetmodeID.SinglePlayer || numPlayers <= 1)
                return;

            bool countsAsBoss = npc.boss || NPCID.Sets.TechnicallyABoss[npc.type];
            bool scalesLikeBoss = countsAsBoss || CalamityLists.bossHPScaleList.Contains(npc.type);
            bool isCalamityNPC = npc.modNPC != null && npc.modNPC.mod == CalamityMod.Instance;

            // All bosses, NPCs that are supposed to scale like bosses, and Calamity NPCs follow these rules.
            if (scalesLikeBoss || isCalamityNPC)
            {
                double scalar;
                switch (numPlayers) // Decrease HP in multiplayer before vanilla scaling
                {
                    case 1:
                        scalar = 1.0;
                        break;

                    case 2:
                        scalar = 0.82; // 1.64
                        break;

                    case 3:
                        scalar = 0.72; // 2.16
                        break;

                    case 4:
                        scalar = 0.64; // 2.56
                        break;

                    case 5:
                        scalar = 0.57; // 2.85
                        break;

                    case 6:
                        scalar = 0.52; // 3.12
                        break;

                    default:
                        scalar = 0.47; // 3.29 + 0.47 per player beyond 7
                        break;
                }

                npc.lifeMax = (int)(npc.lifeMax * scalar);
            }
        }
        #endregion

        #region Can Hit Player
        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
			if (target.Calamity().prismaticHelmet && !CalamityPlayer.areThereAnyDamnBosses)
			{
				if (npc.lifeMax < 500)
					return false;
			}

            return true;
        }
        #endregion

        #region Strike NPC
        public override bool StrikeNPC(NPC npc, ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
        {
            // Don't bother tampering with the damage is it is already zero.
            // Zero damage does not happen in the base game and is always indicative of antibutcher in Calamity.
            if (damage == 0D)
                return false;

			// Damage reduction on spawn
			bool destroyerResist = DestroyerIDs.Contains(npc.type) && (CalamityWorld.revenge || BossRushEvent.BossRushActive || CalamityWorld.malice);
			bool eaterofWorldsResist = EaterofWorldsIDs.Contains(npc.type) && BossRushEvent.BossRushActive;
			if (destroyerResist || eaterofWorldsResist || AstrumDeusIDs.Contains(npc.type))
			{
				float resistanceGateValue = (AstrumDeusIDs.Contains(npc.type) && newAI[0] != 0f) ? 300f : 600f;
				if (newAI[1] < resistanceGateValue || (newAI[2] > 0f && DestroyerIDs.Contains(npc.type)))
					damage *= 0.01;
			}

			// Large Deus worm takes reduced damage to last a long enough time
			if (AstrumDeusIDs.Contains(npc.type) && newAI[0] == 0f)
				damage *= 0.8;

            // Override hand/head eye 'death' code and use custom 'death' code instead, this is here just in case the AI code fails
            if (CalamityWorld.revenge || BossRushEvent.BossRushActive || CalamityWorld.malice)
            {
                if (npc.type == NPCID.MoonLordHand || npc.type == NPCID.MoonLordHead)
                {
                    if (npc.life - (int)damage <= 0)
                    {
                        if (newAI[0] != 1f)
                        {
                            newAI[0] = 1f;
                            npc.life = npc.lifeMax;
                            npc.netUpdate = true;
                            npc.dontTakeDamage = true;
                        }
                    }
                }
            }

            // Yellow Candle provides +5% damage which ignores both DR and defense.
            // However, armor penetration bonus damage has already been applied, so it's slightly higher than it should be.
            double yellowCandleDamage = 0.05 * damage;

			// Apply modifications to enemy's current defense based on Calamity debuffs.
			// As with defense and DR, flat reductions apply first, then multiplicative reductions.
			int effectiveDefense = npc.defense -
                    (wCleave > 0 ? WarCleave.DefenseReduction : 0) -
                    (aCrunch > 0 ? ArmorCrunch.DefenseReduction : 0) -
                    (marked > 0 && DR <= 0f ? MarkedforDeath.DefenseReduction : 0) -
					(wither > 0 ? WitherDebuff.DefenseReduction : 0) -
					Main.LocalPlayer.armorPenetration -
					miscDefenseLoss;

            // Defense can never be negative and has a minimum value of zero.
            if (effectiveDefense < 0)
                effectiveDefense = 0;

            // Apply vanilla-style defense before DR, using Calamity's reduced defense.
            damage = Main.CalculateDamage((int)damage, effectiveDefense);

            // DR applies after vanilla defense.
            damage = ApplyDR(npc, damage, yellowCandleDamage);

			// Inflict 0 damage if it's below 0.5
			damage = damage < 0.5 ? 0D : damage < 1D ? 1D : damage;

			// Disable vanilla damage method if damage is less than 0.5
			if (damage == 0D)
				return false;

			// Cancel out vanilla defense math by reversing the calculation vanilla is about to perform.
			// While roundabout, this is safer than returning false to stop vanilla damage calculations entirely.
			// Other mods will probably expect the vanilla code to run and may compensate for it themselves.
			damage = Main.CalculateDamage((int)damage, -defense);
            return true;
        }

        /// <summary>
        /// Modifies damage incoming to an NPC based on their DR (damage reduction) stat added by Calamity.<br></br>
        /// This is entirely separate from vanilla's takenDamageMultiplier.
        /// </summary>
        /// <param name="damage">Incoming damage. Has been modified by Main.DamageVar and boosted by armor penetration, but nothing else.</param>
        /// <returns></returns>
        private double ApplyDR(NPC npc, double damage, double yellowCandleDamage)
        {
            if ((DR <= 0f && KillTime == 0) || damage <= 1.0)
                return damage;

            // If the NPC currently has unbreakable DR, it cannot be reduced by any means.
            // If custom DR is enabled, use that instead of normal DR.
            float effectiveDR = unbreakableDR ? DR : (customDR ? CustomDRMath(npc, DR) : DefaultDRMath(npc, DR));

            // DR floor is 0%. Nothing can have negative DR.
            if (effectiveDR <= 0f)
                effectiveDR = 0f;

			// Add Yellow Candle damage if the NPC isn't supposed to be "near invincible"
			if (yellowCandle > 0 && DR < 0.99f && npc.takenDamageMultiplier > 0.05f)
				damage += yellowCandleDamage;

			// Boss that get higher timed DR than normal in specific circumstances
			bool nightProvi = npc.type == NPCType<Providence.Providence>() && (!Main.dayTime || CalamityWorld.malice);

			// Calculate extra DR based on kill time, similar to the Hush boss from The Binding of Isaac
			if (KillTime > 0 && AITimer < KillTime && !BossRushEvent.BossRushActive && (TimedDRScaleFactor > 0f || nightProvi))
			{
				// Set the DR scaling factor
				float DRScalar = nightProvi ? timedDR_NightProvi : TimedDRScaleFactor;

                // The limit for how much extra DR the boss can have
                float extraDRLimit = (1f - DR) * DRScalar;

				// Ranges from 1 to 0
				float currentHPRatio = npc.life / (float)npc.lifeMax;

				// Ranges from 0 to 1
				float killTimeRatio = AITimer / (float)KillTime;

				// If the player is damaging the boss too quickly
				float extraDRScalar = currentHPRatio + killTimeRatio;
				if (extraDRScalar < 1f)
				{
					// Ranges from 0 to (extraDRLimit / 2)
					effectiveDR += extraDRLimit - (extraDRLimit / (1f + (1f - extraDRScalar)));
				}
			}

			double newDamage = (1f - effectiveDR) * damage;
            return newDamage;
        }

        private float DefaultDRMath(NPC npc, float DR)
        {
            float calcDR = DR;
            if (marked > 0)
                calcDR *= 0.5f;
            if (npc.betsysCurse)
                calcDR *= 0.66f;
            if (wCleave > 0)
                calcDR *= 0.75f;
            if (npc.Calamity().kamiFlu > 0)
                calcDR *= KamiDebuff.MultiplicativeDamageReduction;
            if (npc.onFire2)
                calcDR *= 0.8f;

            return calcDR;
        }

        private float CustomDRMath(NPC npc, float DR)
        {
            void FlatEditDR(ref float theDR, bool npcHasDebuff, int buffID)
            {
                if (npcHasDebuff && flatDRReductions.TryGetValue(buffID, out float reduction))
                    theDR -= reduction;
            }
            void MultEditDR(ref float theDR, bool npcHasDebuff, int buffID)
            {
                if (npcHasDebuff && multDRReductions.TryGetValue(buffID, out float multiplier))
                    theDR *= multiplier;
            }

            float calcDR = DR;

            // Apply flat reductions first. All vanilla debuffs check their internal booleans.
            FlatEditDR(ref calcDR, npc.poisoned, BuffID.Poisoned);
            FlatEditDR(ref calcDR, npc.onFire, BuffID.OnFire);
            FlatEditDR(ref calcDR, npc.venom, BuffID.Venom);
            FlatEditDR(ref calcDR, npc.onFrostBurn, BuffID.Frostburn);
            FlatEditDR(ref calcDR, npc.shadowFlame, BuffID.ShadowFlame);
            FlatEditDR(ref calcDR, npc.daybreak, BuffID.Daybreak);
            FlatEditDR(ref calcDR, npc.betsysCurse, BuffID.BetsysCurse);
            FlatEditDR(ref calcDR, npc.onFire2, BuffID.CursedInferno);

            // Modded debuffs are handled modularly and use HasBuff.
            foreach (KeyValuePair<int, float> entry in flatDRReductions)
            {
                int buffID = entry.Key;
                if (buffID >= BuffID.Count && npc.HasBuff(buffID))
                    calcDR -= entry.Value;
            }

            // Apply multiplicative reductions second. All vanilla debuffs check their internal booleans.
            MultEditDR(ref calcDR, npc.poisoned, BuffID.Poisoned);
            MultEditDR(ref calcDR, npc.onFire, BuffID.OnFire);
            MultEditDR(ref calcDR, npc.venom, BuffID.Venom);
            MultEditDR(ref calcDR, npc.onFrostBurn, BuffID.Frostburn);
            MultEditDR(ref calcDR, npc.shadowFlame, BuffID.ShadowFlame);
            MultEditDR(ref calcDR, npc.daybreak, BuffID.Daybreak);
            MultEditDR(ref calcDR, npc.betsysCurse, BuffID.BetsysCurse);
            MultEditDR(ref calcDR, npc.onFire2, BuffID.CursedInferno);

            // Modded debuffs are handled modularly and use HasBuff.
            foreach (KeyValuePair<int, float> entry in multDRReductions)
            {
                int buffID = entry.Key;
                if (buffID >= BuffID.Count && npc.HasBuff(buffID))
                    calcDR *= entry.Value;
            }

            return calcDR;
        }
        #endregion

        #region Boss Head Slot
        public override void BossHeadSlot(NPC npc, ref int index)
        {
			if (CalamityWorld.revenge || CalamityWorld.malice)
			{
				if (npc.type == NPCID.BrainofCthulhu)
				{
					if (npc.life / (float)npc.lifeMax < ((CalamityWorld.death || CalamityWorld.malice) ? 1f : 0.2f))
						index = -1;
				}

				if (npc.type == NPCID.DukeFishron && (CalamityWorld.death || CalamityWorld.malice))
				{
					if (npc.life / (float)npc.lifeMax < 0.4f)
						index = -1;
				}
			}
        }
        #endregion

        #region Pre AI
        public override bool PreAI(NPC npc)
        {
			// Change Spaz and Ret weaknesses and resistances when phase 2 starts.
			if (npc.type == NPCID.Spazmatism || npc.type == NPCID.Retinazer)
			{
				if (npc.ai[0] >= 2f)
				{
					VulnerableToCold = null;
					VulnerableToHeat = null;
					VulnerableToSickness = false;
					VulnerableToElectricity = true;
				}
			}

			if (VulnerabilityHexFireDrawer != null)
				VulnerabilityHexFireDrawer.Update();

			CalamityGlobalTownNPC.SetPatreonTownNPCName(npc, mod);

			// Decrement each immune timer if it's greater than 0.
			for (int i = 0; i < maxPlayerImmunities; i++)
			{
				if (dashImmunityTime[i] > 0)
					dashImmunityTime[i]--;
			}

			if (CalamityPlayer.areThereAnyDamnBosses)
			{
				if (npc.velocity.Length() > maxVelocity)
					maxVelocity = npc.velocity.Length();
			}

			if (KillTime > 0 || npc.type == NPCType<Draedon>())
			{
				// If any boss NPC is active, apply Zen to nearby players to reduce spawn rate.
				if (CalamityConfig.Instance.BossZen)
				{
					if (Main.netMode != NetmodeID.Server)
					{
						if (!Main.player[Main.myPlayer].dead && Main.player[Main.myPlayer].active && Vector2.Distance(Main.player[Main.myPlayer].Center, npc.Center) < BossZenDistance)
							Main.player[Main.myPlayer].AddBuff(BuffType<BossZen>(), 2);
					}
				}

				bool DoGSentinelPhase = DevourerOfGodsIDs.Contains(npc.type) && npc.life / (float)npc.lifeMax < 0.6f && CalamityWorld.DoGSecondStageCountdown > 60;
				if (npc.type != NPCType<Draedon>() && !DoGSentinelPhase)
				{
					if (AITimer < KillTime)
						AITimer++;

					// Separate timer for aggression to avoid entering later phases too quickly
					int aggressionTimerCap = (int)(KillTime * 1.5);
					if (AIIncreasedAggressionTimer < aggressionTimerCap)
						AIIncreasedAggressionTimer++;

					// Increases aggression over time if the fight is taking too long
					killTimeRatio_IncreasedAggression = 1f - (AIIncreasedAggressionTimer / (float)aggressionTimerCap);
				}
			}

			if (npc.type == NPCID.TargetDummy || npc.type == NPCType<SuperDummyNPC>())
            {
                npc.dontTakeDamage = CalamityPlayer.areThereAnyDamnBosses;

				if (draedon != -1)
				{
					if (Main.npc[draedon].active)
						npc.dontTakeDamage = true;
				}
			}

			// Setting this in SetDefaults will disable expert mode scaling, so put it here instead
			if (ZeroContactDamageNPCList.Contains(npc.type))
				npc.damage = npc.defDamage = 0;

			// Don't do damage for 42 frames after spawning in
			if (npc.type == NPCID.Sharkron || npc.type == NPCID.Sharkron2)
				npc.damage = npc.alpha > 0 ? 0 : npc.defDamage;

			if (DestroyerIDs.Contains(npc.type) || EaterofWorldsIDs.Contains(npc.type))
                npc.buffImmune[BuffType<Enraged>()] = false;

            if (BossRushEvent.BossRushActive && !npc.friendly && !npc.townNPC && !npc.Calamity().DoesNotDisappearInBossRush)
                BossRushForceDespawnOtherNPCs(npc, mod);

			if (NPC.LunarApocalypseIsUp)
				PillarEventProgressionEdit(npc);

			// Adult Wyrm Ancient Doom
			if (npc.type == NPCID.AncientDoom)
			{
				if (Main.npc[(int)npc.ai[0]].type == NPCType<EidolonWyrmHeadHuge>())
					return CalamityGlobalAI.BuffedAncientDoomAI(npc, mod);
			}

			// Disable teleports for hardmode dungeon casters if they get hit
			if (npc.type >= NPCID.RaggedCaster && npc.type <= NPCID.DiabolistWhite && npc.justHit)
			{
				npc.ai[0] = 1f;
			}

			if (CalamityWorld.revenge || BossRushEvent.BossRushActive || CalamityWorld.malice)
            {
				switch (npc.type)
                {
                    case NPCID.KingSlime:
                        return CalamityGlobalAI.BuffedKingSlimeAI(npc, enraged > 0, mod);

                    case NPCID.EyeofCthulhu:
                        return CalamityGlobalAI.BuffedEyeofCthulhuAI(npc, enraged > 0, mod);

                    case NPCID.EaterofWorldsHead:
                    case NPCID.EaterofWorldsBody:
                    case NPCID.EaterofWorldsTail:
                        return CalamityGlobalAI.BuffedEaterofWorldsAI(npc, enraged > 0, mod);

                    case NPCID.BrainofCthulhu:
                        return CalamityGlobalAI.BuffedBrainofCthulhuAI(npc, enraged > 0, mod);
                    case NPCID.Creeper:
                        return CalamityGlobalAI.BuffedCreeperAI(npc, enraged > 0, mod);

                    case NPCID.QueenBee:
                        return CalamityGlobalAI.BuffedQueenBeeAI(npc, enraged > 0, mod);

                    case NPCID.SkeletronHand:
                        return CalamityGlobalAI.BuffedSkeletronHandAI(npc, enraged > 0, mod);
                    case NPCID.SkeletronHead:
                        return CalamityGlobalAI.BuffedSkeletronAI(npc, enraged > 0, mod);

                    case NPCID.WallofFlesh:
                        return CalamityGlobalAI.BuffedWallofFleshAI(npc, enraged > 0, mod);
                    case NPCID.WallofFleshEye:
                        return CalamityGlobalAI.BuffedWallofFleshEyeAI(npc, enraged > 0, mod);

                    case NPCID.TheDestroyer:
                    case NPCID.TheDestroyerBody:
                    case NPCID.TheDestroyerTail:
                        return CalamityGlobalAI.BuffedDestroyerAI(npc, enraged > 0, mod);

					case NPCID.Probe:
						return CalamityGlobalAI.BuffedProbeAI(npc, mod);

                    case NPCID.Retinazer:
                        return CalamityGlobalAI.BuffedRetinazerAI(npc, enraged > 0, mod);
                    case NPCID.Spazmatism:
                        return CalamityGlobalAI.BuffedSpazmatismAI(npc, enraged > 0, mod);

                    case NPCID.SkeletronPrime:
                        return CalamityGlobalAI.BuffedSkeletronPrimeAI(npc, enraged > 0, mod);
                    case NPCID.PrimeLaser:
                        return CalamityGlobalAI.BuffedPrimeLaserAI(npc, enraged > 0, mod);
                    case NPCID.PrimeCannon:
                        return CalamityGlobalAI.BuffedPrimeCannonAI(npc, enraged > 0, mod);
                    case NPCID.PrimeVice:
                        return CalamityGlobalAI.BuffedPrimeViceAI(npc, enraged > 0, mod);
                    case NPCID.PrimeSaw:
                        return CalamityGlobalAI.BuffedPrimeSawAI(npc, enraged > 0, mod);

                    case NPCID.Plantera:
                        return CalamityGlobalAI.BuffedPlanteraAI(npc, enraged > 0, mod);
                    case NPCID.PlanterasHook:
                        return CalamityGlobalAI.BuffedPlanterasHookAI(npc, mod);
                    case NPCID.PlanterasTentacle:
                        return CalamityGlobalAI.BuffedPlanterasTentacleAI(npc, mod);

                    case NPCID.Golem:
                        return CalamityGlobalAI.BuffedGolemAI(npc, enraged > 0, mod);
					case NPCID.GolemFistLeft:
					case NPCID.GolemFistRight:
						return CalamityGlobalAI.BuffedGolemFistAI(npc, enraged > 0, mod);
                    case NPCID.GolemHead:
                        return CalamityGlobalAI.BuffedGolemHeadAI(npc, enraged > 0, mod);
                    case NPCID.GolemHeadFree:
                        return CalamityGlobalAI.BuffedGolemHeadFreeAI(npc, enraged > 0, mod);

                    case NPCID.DukeFishron:
                        return CalamityGlobalAI.BuffedDukeFishronAI(npc, enraged > 0, mod);

                    case NPCID.Pumpking:
                        if (CalamityWorld.downedDoG)
                        {
                            return CalamityGlobalAI.BuffedPumpkingAI(npc);
                        }

                        break;

                    case NPCID.PumpkingBlade:
                        if (CalamityWorld.downedDoG)
                        {
                            return CalamityGlobalAI.BuffedPumpkingBladeAI(npc);
                        }

                        break;

                    case NPCID.IceQueen:
                        if (CalamityWorld.downedDoG)
                        {
                            return CalamityGlobalAI.BuffedIceQueenAI(npc);
                        }

                        break;

                    case NPCID.Mothron:
                        if (CalamityWorld.downedDoG)
                        {
                            return CalamityGlobalAI.BuffedMothronAI(npc);
                        }

                        break;

                    case NPCID.CultistBoss:
                    case NPCID.CultistBossClone:
                        return CalamityGlobalAI.BuffedCultistAI(npc, enraged > 0, mod);
					case NPCID.AncientLight:
						return CalamityGlobalAI.BuffedAncientLightAI(npc, mod);
					case NPCID.AncientDoom:
                        return CalamityGlobalAI.BuffedAncientDoomAI(npc, mod);

                    case NPCID.MoonLordCore:
                    case NPCID.MoonLordHand:
                    case NPCID.MoonLordHead:
					case NPCID.MoonLordFreeEye:
					case NPCID.MoonLordLeechBlob:
                        return CalamityGlobalAI.BuffedMoonLordAI(npc, enraged > 0, mod);

					default:
                        break;
                }
            }
			else if (npc.type == NPCID.Retinazer)
			{
				return CalamityGlobalAI.TrueMeleeRetinazerPhase2AI(npc);
			}

			if (CalamityWorld.death)
			{
				switch (npc.aiStyle)
				{
					case 1:
						if (npc.type == NPCType<BloomSlime>() || npc.type == NPCType<CharredSlime>() ||
							npc.type == NPCType<CrimulanBlightSlime>() || npc.type == NPCType<CryoSlime>() ||
							npc.type == NPCType<EbonianBlightSlime>() || npc.type == NPCType<PerennialSlime>() ||
							npc.type == NPCType<WulfrumSlime>() || npc.type == NPCType<IrradiatedSlime>() ||
							npc.type == NPCType<AstralSlime>())
						{
							return CalamityGlobalAI.BuffedSlimeAI(npc, mod);
						}
						else
						{
							switch (npc.type)
							{
								case NPCID.BlueSlime:
								case NPCID.MotherSlime:
								case NPCID.LavaSlime:
								case NPCID.DungeonSlime:
								case NPCID.CorruptSlime:
								case NPCID.IlluminantSlime:
								case NPCID.ToxicSludge:
								case NPCID.IceSlime:
								case NPCID.Crimslime:
								case NPCID.SpikedIceSlime:
								case NPCID.SpikedJungleSlime:
								case NPCID.UmbrellaSlime:
								case NPCID.RainbowSlime:
								case NPCID.SlimeMasked:
								case NPCID.HoppinJack:
								case NPCID.SlimeRibbonWhite:
								case NPCID.SlimeRibbonYellow:
								case NPCID.SlimeRibbonGreen:
								case NPCID.SlimeRibbonRed:
								case NPCID.SlimeSpiked:
								case NPCID.SandSlime:
									return CalamityGlobalAI.BuffedSlimeAI(npc, mod);
							}
						}
						break;
					case 2:
						if (npc.type == NPCType<BlightedEye>() || npc.type == NPCType<CalamityEye>())
						{
							return CalamityGlobalAI.BuffedDemonEyeAI(npc, mod);
						}
						else
						{
							switch (npc.type)
							{
								case NPCID.DemonEye:
								case NPCID.TheHungryII:
								case NPCID.WanderingEye:
								case NPCID.PigronCorruption:
								case NPCID.PigronHallow:
								case NPCID.PigronCrimson:
								case NPCID.CataractEye:
								case NPCID.SleepyEye:
								case NPCID.DialatedEye:
								case NPCID.GreenEye:
								case NPCID.PurpleEye:
								case NPCID.DemonEyeOwl:
								case NPCID.DemonEyeSpaceship:
									return CalamityGlobalAI.BuffedDemonEyeAI(npc, mod);
							}
						}
						break;
					case 3:
						if (npc.type == NPCType<StormlionCharger>() ||
							npc.type == NPCType<AstralachneaGround>() || npc.type == NPCType<CultistAssassin>())
						{
							return CalamityGlobalAI.BuffedFighterAI(npc, mod);
						}
						else
						{
							switch (npc.type)
							{
								case NPCID.Zombie:
								case NPCID.ArmedZombie:
								case NPCID.ArmedZombieEskimo:
								case NPCID.ArmedZombiePincussion:
								case NPCID.ArmedZombieSlimed:
								case NPCID.ArmedZombieSwamp:
								case NPCID.ArmedZombieTwiggy:
								case NPCID.ArmedZombieCenx:
								case NPCID.Skeleton:
								case NPCID.AngryBones:
								case NPCID.UndeadMiner:
								case NPCID.CorruptBunny:
								case NPCID.DoctorBones:
								case NPCID.TheGroom:
								case NPCID.Crab:
								case NPCID.GoblinScout:
								case NPCID.ArmoredSkeleton:
								case NPCID.Mummy:
								case NPCID.DarkMummy:
								case NPCID.LightMummy:
								case NPCID.Werewolf:
								case NPCID.Clown:
								case NPCID.SkeletonArcher:
								case NPCID.ChaosElemental:
								case NPCID.BaldZombie:
								case NPCID.PossessedArmor:
								case NPCID.ZombieEskimo:
								case NPCID.BlackRecluse:
								case NPCID.WallCreeper:
								case NPCID.UndeadViking:
								case NPCID.CorruptPenguin:
								case NPCID.FaceMonster:
								case NPCID.SnowFlinx:
								case NPCID.PincushionZombie:
								case NPCID.SlimedZombie:
								case NPCID.SwampZombie:
								case NPCID.TwiggyZombie:
								case NPCID.Nymph:
								case NPCID.ArmoredViking:
								case NPCID.Lihzahrd:
								case NPCID.LihzahrdCrawler:
								case NPCID.FemaleZombie:
								case NPCID.HeadacheSkeleton:
								case NPCID.MisassembledSkeleton:
								case NPCID.PantlessSkeleton:
								case NPCID.IcyMerman:
								case NPCID.PirateDeckhand:
								case NPCID.PirateCorsair:
								case NPCID.PirateDeadeye:
								case NPCID.PirateCrossbower:
								case NPCID.PirateCaptain:
								case NPCID.CochinealBeetle:
								case NPCID.CyanBeetle:
								case NPCID.LacBeetle:
								case NPCID.SeaSnail:
								case NPCID.ZombieRaincoat:
								case NPCID.JungleCreeper:
								case NPCID.BloodCrawler:
								case NPCID.IceGolem:
								case NPCID.Eyezor:
								case NPCID.ZombieMushroom:
								case NPCID.ZombieMushroomHat:
								case NPCID.AnomuraFungus:
								case NPCID.MushiLadybug:
								case NPCID.RustyArmoredBonesAxe:
								case NPCID.RustyArmoredBonesFlail:
								case NPCID.RustyArmoredBonesSword:
								case NPCID.RustyArmoredBonesSwordNoArmor:
								case NPCID.BlueArmoredBones:
								case NPCID.BlueArmoredBonesMace:
								case NPCID.BlueArmoredBonesNoPants:
								case NPCID.BlueArmoredBonesSword:
								case NPCID.HellArmoredBones:
								case NPCID.HellArmoredBonesSpikeShield:
								case NPCID.HellArmoredBonesMace:
								case NPCID.HellArmoredBonesSword:
								case NPCID.BoneLee:
								case NPCID.Paladin:
								case NPCID.SkeletonSniper:
								case NPCID.TacticalSkeleton:
								case NPCID.SkeletonCommando:
								case NPCID.AngryBonesBig:
								case NPCID.AngryBonesBigMuscle:
								case NPCID.AngryBonesBigHelmet:
								case NPCID.Scarecrow1:
								case NPCID.Scarecrow2:
								case NPCID.Scarecrow3:
								case NPCID.Scarecrow4:
								case NPCID.Scarecrow5:
								case NPCID.Scarecrow6:
								case NPCID.Scarecrow7:
								case NPCID.Scarecrow8:
								case NPCID.Scarecrow9:
								case NPCID.Scarecrow10:
								case NPCID.ZombieDoctor:
								case NPCID.ZombieSuperman:
								case NPCID.ZombiePixie:
								case NPCID.SkeletonTopHat:
								case NPCID.SkeletonAstonaut:
								case NPCID.SkeletonAlien:
								case NPCID.Splinterling:
								case NPCID.ZombieXmas:
								case NPCID.ZombieSweater:
								case NPCID.ZombieElf:
								case NPCID.ZombieElfBeard:
								case NPCID.ZombieElfGirl:
								case NPCID.GingerbreadMan:
								case NPCID.Yeti:
								case NPCID.Nutcracker:
								case NPCID.NutcrackerSpinning:
								case NPCID.ElfArcher:
								case NPCID.Krampus:
								case NPCID.CultistArcherBlue:
								case NPCID.CultistArcherWhite:
								case NPCID.BrainScrambler:
								case NPCID.RayGunner:
								case NPCID.MartianOfficer:
								case NPCID.GrayGrunt:
								case NPCID.MartianEngineer:
								case NPCID.GigaZapper:
								case NPCID.Scutlix:
								case NPCID.BoneThrowingSkeleton:
								case NPCID.BoneThrowingSkeleton2:
								case NPCID.BoneThrowingSkeleton3:
								case NPCID.BoneThrowingSkeleton4:
								case NPCID.CrimsonBunny:
								case NPCID.CrimsonPenguin:
								case NPCID.Medusa:
								case NPCID.GreekSkeleton:
								case NPCID.GraniteGolem:
								case NPCID.BloodZombie:
								case NPCID.Crawdad:
								case NPCID.Crawdad2:
								case NPCID.Salamander:
								case NPCID.Salamander2:
								case NPCID.Salamander3:
								case NPCID.Salamander4:
								case NPCID.Salamander5:
								case NPCID.Salamander6:
								case NPCID.Salamander7:
								case NPCID.Salamander8:
								case NPCID.Salamander9:
								case NPCID.WalkingAntlion:
								case NPCID.DesertGhoul:
								case NPCID.DesertGhoulCorruption:
								case NPCID.DesertGhoulCrimson:
								case NPCID.DesertGhoulHallow:
								case NPCID.DesertLamiaLight:
								case NPCID.DesertLamiaDark:
								case NPCID.DesertScorpionWalk:
								case NPCID.DesertBeast:
								case NPCID.StardustSoldier:
								case NPCID.StardustSpiderBig:
								case NPCID.NebulaSoldier:
								case NPCID.VortexRifleman:
								case NPCID.VortexSoldier:
								case NPCID.VortexLarva:
								case NPCID.VortexHornet:
								case NPCID.VortexHornetQueen:
								case NPCID.SolarDrakomire:
								case NPCID.SolarSpearman:
								case NPCID.SolarSolenian:
								case NPCID.Frankenstein:
								case NPCID.SwampThing:
								case NPCID.Vampire:
								case NPCID.Butcher:
								case NPCID.CreatureFromTheDeep:
								case NPCID.Fritz:
								case NPCID.Nailhead:
								case NPCID.Psycho:
								case NPCID.ThePossessed:
								case NPCID.DrManFly:
								case NPCID.GoblinPeon:
								case NPCID.GoblinThief:
								case NPCID.GoblinWarrior:
								case NPCID.GoblinArcher:
								case NPCID.GoblinSummoner:
								case NPCID.MartianWalker:
								case NPCID.DemonTaxCollector:
								case NPCID.TheBride:
									return CalamityGlobalAI.BuffedFighterAI(npc, mod);
							}
						}
						break;
					case 5:
						switch (npc.type)
						{
							case NPCID.ServantofCthulhu:
							case NPCID.EaterofSouls:
							case NPCID.MeteorHead:
							case NPCID.Corruptor:
							case NPCID.Crimera:
							case NPCID.Moth:
							case NPCID.Parrot:
								return CalamityGlobalAI.BuffedFlyingAI(npc, mod);
						}
						break;
					case 6:
						switch (npc.type)
						{
							case NPCID.DevourerHead:
							case NPCID.DevourerBody:
							case NPCID.DevourerTail:
							case NPCID.GiantWormHead:
							case NPCID.GiantWormBody:
							case NPCID.GiantWormTail:
							case NPCID.BoneSerpentHead:
							case NPCID.BoneSerpentBody:
							case NPCID.BoneSerpentTail:
							case NPCID.WyvernHead:
							case NPCID.WyvernLegs:
							case NPCID.WyvernBody:
							case NPCID.WyvernBody2:
							case NPCID.WyvernBody3:
							case NPCID.WyvernTail:
							case NPCID.DiggerHead:
							case NPCID.DiggerBody:
							case NPCID.DiggerTail:
							case NPCID.SeekerHead:
							case NPCID.SeekerBody:
							case NPCID.SeekerTail:
							case NPCID.LeechHead:
							case NPCID.LeechBody:
							case NPCID.LeechTail:
							case NPCID.TombCrawlerHead:
							case NPCID.TombCrawlerBody:
							case NPCID.TombCrawlerTail:
							case NPCID.DuneSplicerHead:
							case NPCID.DuneSplicerBody:
							case NPCID.DuneSplicerTail:
							case NPCID.StardustWormHead:
							case NPCID.SolarCrawltipedeHead:
							case NPCID.SolarCrawltipedeBody:
							case NPCID.SolarCrawltipedeTail:
								return CalamityGlobalAI.BuffedWormAI(npc, mod);
						}
						break;
					case 8:
						switch (npc.type)
						{
							case NPCID.FireImp:
							case NPCID.DarkCaster:
							case NPCID.Tim:
							case NPCID.RuneWizard:
							case NPCID.RaggedCaster:
							case NPCID.RaggedCasterOpenCoat:
							case NPCID.Necromancer:
							case NPCID.NecromancerArmored:
							case NPCID.DiabolistRed:
							case NPCID.DiabolistWhite:
							case NPCID.DesertDjinn:
							case NPCID.GoblinSorcerer:
								return CalamityGlobalAI.BuffedCasterAI(npc, mod);
						}
						break;
					case 13:
						switch (npc.type)
						{
							case NPCID.ManEater:
							case NPCID.Snatcher:
							case NPCID.Clinger:
							case NPCID.AngryTrapper:
							case NPCID.FungiBulb:
							case NPCID.GiantFungiBulb:
								return CalamityGlobalAI.BuffedPlantAI(npc, mod);
						}
						break;
					case 14:
						if (npc.type == NPCType<StellarCulex>() || npc.type == NPCType<PlaguedFlyingFox>() || npc.type == NPCType<AeroSlime>() || npc.type == NPCType<SunBat>())
						{
							return CalamityGlobalAI.BuffedBatAI(npc, mod);
						}
						else
						{
							switch (npc.type)
							{
								case NPCID.Harpy:
								case NPCID.CaveBat:
								case NPCID.JungleBat:
								case NPCID.Hellbat:
								case NPCID.Demon:
								case NPCID.VoodooDemon:
								case NPCID.GiantBat:
								case NPCID.Slimer:
								case NPCID.IlluminantBat:
								case NPCID.IceBat:
								case NPCID.Lavabat:
								case NPCID.GiantFlyingFox:
								case NPCID.RedDevil:
								case NPCID.FlyingSnake:
								case NPCID.VampireBat:
									return CalamityGlobalAI.BuffedBatAI(npc, mod);
							}
						}
						break;
					case 16:
						switch (npc.type)
						{
							case NPCID.CorruptGoldfish:
							case NPCID.Piranha:
							case NPCID.Shark:
							case NPCID.AnglerFish:
							case NPCID.Arapaima:
							case NPCID.BloodFeeder:
							case NPCID.CrimsonGoldfish:
								return CalamityGlobalAI.BuffedSwimmingAI(npc, mod);
						}
						break;
					case 18:
						switch (npc.type)
						{
							case NPCID.BlueJellyfish:
							case NPCID.PinkJellyfish:
							case NPCID.GreenJellyfish:
							case NPCID.Squid:
							case NPCID.BloodJelly:
							case NPCID.FungoFish:
								return CalamityGlobalAI.BuffedJellyfishAI(npc, mod);
						}
						break;
					case 19:
						switch (npc.type)
						{
							case NPCID.Antlion:
								return CalamityGlobalAI.BuffedAntlionAI(npc, mod);
						}
						break;
					case 20:
						switch (npc.type)
						{
							case NPCID.SpikeBall:
								return CalamityGlobalAI.BuffedSpikeBallAI(npc, mod);
						}
						break;
					case 21:
						switch (npc.type)
						{
							case NPCID.BlazingWheel:
								return CalamityGlobalAI.BuffedBlazingWheelAI(npc, mod);
						}
						break;
					case 22:
						switch (npc.type)
						{
							case NPCID.Pixie:
							case NPCID.Wraith:
							case NPCID.Gastropod:
							case NPCID.IceElemental:
							case NPCID.FloatyGross:
							case NPCID.IchorSticker:
							case NPCID.Ghost:
							case NPCID.Poltergeist:
							case NPCID.Drippler:
							case NPCID.Reaper:
								return CalamityGlobalAI.BuffedHoveringAI(npc, mod);
						}
						break;
					case 23:
						switch (npc.type)
						{
							case NPCID.CursedHammer:
							case NPCID.EnchantedSword:
							case NPCID.CrimsonAxe:
								return CalamityGlobalAI.BuffedFlyingWeaponAI(npc, mod);
						}
						break;
					case 25:
						switch (npc.type)
						{
							case NPCID.Mimic:
							case NPCID.PresentMimic:
								return CalamityGlobalAI.BuffedMimicAI(npc, mod);
						}
						break;
					case 26:
						if (npc.type == NPCType<Pitbull>())
						{
							return CalamityGlobalAI.BuffedUnicornAI(npc, mod);
						}
						else
						{
							switch (npc.type)
							{
								case NPCID.Unicorn:
								case NPCID.Wolf:
								case NPCID.HeadlessHorseman:
								case NPCID.Hellhound:
								case NPCID.StardustSpiderSmall:
								case NPCID.NebulaBeast:
								case NPCID.Tumbleweed:
									return CalamityGlobalAI.BuffedUnicornAI(npc, mod);
							}
						}
						break;
					case 39:
						if (npc.type == NPCType<PlaguedTortoise>() || npc.type == NPCType<SandTortoise>())
						{
							return CalamityGlobalAI.BuffedTortoiseAI(npc, mod);
						}
						else
						{
							switch (npc.type)
							{
								case NPCID.GiantTortoise:
								case NPCID.IceTortoise:
								case NPCID.GiantShelly:
								case NPCID.GiantShelly2:
								case NPCID.SolarSroller:
									return CalamityGlobalAI.BuffedTortoiseAI(npc, mod);
							}
						}
						break;
					case 40:
						switch (npc.type)
						{
							case NPCID.BlackRecluseWall:
							case NPCID.WallCreeperWall:
							case NPCID.JungleCreeperWall:
							case NPCID.BloodCrawlerWall:
							case NPCID.DesertScorpionWall:
								return CalamityGlobalAI.BuffedSpiderAI(npc, mod);
						}
						break;
					case 41:
						if (npc.type == NPCType<Aries>())
						{
							return CalamityGlobalAI.BuffedHerplingAI(npc, mod);
						}
						else
						{
							switch (npc.type)
							{
								case NPCID.Herpling:
								case NPCID.Derpling:
									return CalamityGlobalAI.BuffedHerplingAI(npc, mod);
							}
						}
						break;
					case 44:
						switch (npc.type)
						{
							case NPCID.FlyingFish:
							case NPCID.FlyingAntlion:
								return CalamityGlobalAI.BuffedFlyingFishAI(npc, mod);
						}
						break;
					case 49:
						switch (npc.type)
						{
							case NPCID.AngryNimbus:
								return CalamityGlobalAI.BuffedAngryNimbusAI(npc, mod);
						}
						break;
					case 50:
						switch (npc.type)
						{
							case NPCID.FungiSpore:
							case NPCID.Spore:
								return CalamityGlobalAI.BuffedSporeAI(npc, mod);
						}
						break;
					case 73:
						switch (npc.type)
						{
							case NPCID.MartianTurret:
								return CalamityGlobalAI.BuffedTeslaTurretAI(npc, mod);
						}
						break;
					case 74:
						switch (npc.type)
						{
							case NPCID.MartianDrone:
							case NPCID.SolarCorite:
								return CalamityGlobalAI.BuffedCoriteAI(npc, mod);
						}
						break;
					case 80:
						switch (npc.type)
						{
							case NPCID.MartianProbe:
								return CalamityGlobalAI.BuffedMartianProbeAI(npc, mod);
						}
						break;
					case 89:
						switch (npc.type)
						{
							case NPCID.MothronEgg:
								return CalamityGlobalAI.BuffedMothronEggAI(npc, mod);
						}
						break;
					case 91:
						if (npc.type == NPCType<CosmicElemental>())
						{
							return CalamityGlobalAI.BuffedGraniteElementalAI(npc, mod);
						}
						else
						{
							switch (npc.type)
							{
								case NPCID.GraniteFlyer:
									return CalamityGlobalAI.BuffedGraniteElementalAI(npc, mod);
							}
						}
						break;
					default:
						break;
				}
			}
			else if (Main.expertMode)
			{
				if (npc.type == NPCID.FungiSpore || npc.type == NPCID.Spore)
					return CalamityGlobalAI.BuffedSporeAI(npc, mod);
			}

            if (npc.type == NPCID.DD2LanePortal)
            {
                CalamityGlobalAI.DD2PortalAI(npc);
                return false;
            }                

            return true;
        }
        #endregion

        #region Boss Rush Force Despawn Other NPCs
        private void BossRushForceDespawnOtherNPCs(NPC npc, Mod mod)
        {
			if (BossRushEvent.BossRushStage >= BossRushEvent.Bosses.Count)
				return;

			if (!BossRushEvent.Bosses[BossRushEvent.BossRushStage].HostileNPCsToNotDelete.Contains(npc.type))
			{
				npc.active = false;
				npc.netUpdate = true;
			}
		}
		#endregion

		#region Pillar Event Progression Edit
		private void PillarEventProgressionEdit(NPC npc)
		{
			// Make pillars a bit more fun by forcing more difficult enemies based on progression.
			int solarTowerShieldStrength = (int)Math.Ceiling(NPC.ShieldStrengthTowerSolar / 25D);
			switch (solarTowerShieldStrength)
			{
				case 4:
					// Possible spawns: Drakanian, Drakomire, Drakomire Rider, Sroller
					switch (npc.type)
					{
						case NPCID.SolarCrawltipedeHead:
						case NPCID.SolarCrawltipedeBody:
						case NPCID.SolarCrawltipedeTail:
						case NPCID.SolarSolenian:
						case NPCID.SolarCorite:
							npc.active = false;
							npc.netUpdate = true;
							break;
						default:
							break;
					}
					break;
				case 3:
					// Possible spawns: Drakanian, Drakomire Rider, Sroller
					switch (npc.type)
					{
						case NPCID.SolarCrawltipedeHead:
						case NPCID.SolarCrawltipedeBody:
						case NPCID.SolarCrawltipedeTail:
						case NPCID.SolarDrakomire:
						case NPCID.SolarSolenian:
						case NPCID.SolarCorite:
							npc.active = false;
							npc.netUpdate = true;
							break;
						default:
							break;
					}
					break;
				case 2:
					// Possible spawns: Drakanian, Selenian, Sroller
					switch (npc.type)
					{
						case NPCID.SolarDrakomire:
						case NPCID.SolarCrawltipedeHead:
						case NPCID.SolarCrawltipedeBody:
						case NPCID.SolarCrawltipedeTail:
						case NPCID.SolarCorite:
						case NPCID.SolarDrakomireRider:
							npc.active = false;
							npc.netUpdate = true;
							break;
						default:
							break;
					}
					break;
				case 1:
					// Possible spawns: Corite, Selenian, Sroller, Crawltipede
					switch (npc.type)
					{
						case NPCID.SolarDrakomire:
						case NPCID.SolarSpearman:
						case NPCID.SolarDrakomireRider:
							npc.active = false;
							npc.netUpdate = true;
							break;
						default:
							break;
					}
					break;
				case 0:
					// Possible spawns: Corite, Crawltipede, Selenian
					switch (npc.type)
					{
						case NPCID.SolarDrakomire:
						case NPCID.SolarSpearman:
						case NPCID.SolarDrakomireRider:
						case NPCID.SolarSroller:
							npc.active = false;
							npc.netUpdate = true;
							break;
						default:
							break;
					}
					break;
			}

			int vortexTowerShieldStrength = (int)Math.Ceiling(NPC.ShieldStrengthTowerVortex / 25D);
			switch (vortexTowerShieldStrength)
			{
				case 4:
					// Possible spawns: Alien Larva, Alien Hornet, Alien Queen
					switch (npc.type)
					{
						case NPCID.VortexSoldier:
						case NPCID.VortexRifleman:
							npc.active = false;
							npc.netUpdate = true;
							break;
						default:
							break;
					}
					break;
				case 3:
					// Possible spawns: Alien Larva, Alien Hornet, Alien Queen, Vortexian
					if (npc.type == NPCID.VortexRifleman)
					{
						npc.active = false;
						npc.netUpdate = true;
					}
					break;
				case 2:
					// Possible spawns: Alien Larva, Alien Hornet, Alien Queen, Storm Diver
					if (npc.type == NPCID.VortexSoldier)
					{
						npc.active = false;
						npc.netUpdate = true;
					}
					break;
				case 1:
				case 0:
					// Possible spawns: Alien Larva, Alien Hornet, Alien Queen, Vortexian, Storm Diver
					break;
			}

			int nebulaTowerShieldStrength = (int)Math.Ceiling(NPC.ShieldStrengthTowerNebula / 25D);
			switch (nebulaTowerShieldStrength)
			{
				case 4:
					// Possible spawns: Brain Suckler
					switch (npc.type)
					{
						case NPCID.NebulaBeast:
						case NPCID.NebulaBrain:
						case NPCID.NebulaSoldier:
							npc.active = false;
							npc.netUpdate = true;
							break;
						default:
							break;
					}
					break;
				case 3:
					// Possible spawns: Brain Suckler, Predictor
					switch (npc.type)
					{
						case NPCID.NebulaBeast:
						case NPCID.NebulaBrain:
							npc.active = false;
							npc.netUpdate = true;
							break;
						default:
							break;
					}
					break;
				case 2:
					// Possible spawns: Brain Suckler, Predictor, Evolution Beast
					if (npc.type == NPCID.NebulaBrain)
					{
						npc.active = false;
						npc.netUpdate = true;
					}
					break;
				case 1:
				case 0:
					// Possible spawns: Predictor, Evolution Beast, Nebula Floater
					if (npc.type == NPCID.NebulaHeadcrab)
					{
						npc.active = false;
						npc.netUpdate = true;
					}
					break;
			}

			int stardustTowerShieldStrength = (int)Math.Ceiling(NPC.ShieldStrengthTowerStardust / 25D);
			switch (stardustTowerShieldStrength)
			{
				case 4:
					// Possible spawns: Milkyway Weaver, Star Cell
					switch (npc.type)
					{
						case NPCID.StardustSpiderBig:
						case NPCID.StardustSoldier:
						case NPCID.StardustJellyfishBig:
							npc.active = false;
							npc.netUpdate = true;
							break;
						default:
							break;
					}
					break;
				case 3:
					// Possible spawns: Milkyway Weaver, Stargazer, Twinkle Popper
					switch (npc.type)
					{
						case NPCID.StardustCellBig:
						case NPCID.StardustJellyfishBig:
							npc.active = false;
							npc.netUpdate = true;
							break;
						default:
							break;
					}
					break;
				case 2:
					// Possible spawns: Stargazer, Twinkle Popper, Flow Invader
					switch (npc.type)
					{
						case NPCID.StardustCellBig:
						case NPCID.StardustWormHead:
						case NPCID.StardustWormBody:
						case NPCID.StardustWormTail:
							npc.active = false;
							npc.netUpdate = true;
							break;
						default:
							break;
					}
					break;
				case 1:
				case 0:
					// Possible spawns: Twinkle Popper, Flow Invader
					switch (npc.type)
					{
						case NPCID.StardustCellBig:
						case NPCID.StardustWormHead:
						case NPCID.StardustWormBody:
						case NPCID.StardustWormTail:
						case NPCID.StardustSoldier:
							npc.active = false;
							npc.netUpdate = true;
							break;
						default:
							break;
					}
					break;
			}
		}
		#endregion

		#region AI
		public override void AI(NPC npc)
        {
			if (CalamityWorld.revenge && npc.type == NPCID.DungeonGuardian)
				CalamityGlobalAI.RevengeanceDungeonGuardianAI(npc);
        }
		#endregion

		#region Post AI
		public override void PostAI(NPC npc)
		{
			// Debuff decrements
			if (debuffResistanceTimer > 0)
				debuffResistanceTimer--;

			if (timeSlow > 0)
				timeSlow--;
			if (tesla > 0)
				tesla--;
			if (gState > 0)
				gState--;
			if (tSad > 0)
				tSad--;
			if (eFreeze > 0)
				eFreeze--;
			if (eutrophication > 0)
				eutrophication--;
			if (webbed > 0)
				webbed--;
			if (slowed > 0)
				slowed--;
			if (kamiFlu > 0)
				kamiFlu--;
			if (vaporfied > 0)
				vaporfied--;

			if (electrified > 0)
				electrified--;
			if (yellowCandle > 0)
				yellowCandle--;
			if (pearlAura > 0)
				pearlAura--;
			if (wCleave > 0)
				wCleave--;
			if (bBlood > 0)
				bBlood--;
			if (dFlames > 0)
				dFlames--;
			if (vulnerabilityHex > 0)
				vulnerabilityHex--;
			if (marked > 0)
				marked--;
			if (irradiated > 0)
				irradiated--;
			if (bFlames > 0)
				bFlames--;
			if (hFlames > 0)
				hFlames--;
			if (pFlames > 0)
				pFlames--;
			if (aFlames > 0)
				aFlames--;
			if (pShred > 0)
				pShred--;
			if (aCrunch > 0)
				aCrunch--;
			if (cDepth > 0)
				cDepth--;
			if (gsInferno > 0)
				gsInferno--;
			if (astralInfection > 0)
				astralInfection--;
			if (wDeath > 0)
				wDeath--;
			if (nightwither > 0)
				nightwither--;
			if (enraged > 0)
				enraged--;
			if (shellfishVore > 0)
				shellfishVore--;
			if (clamDebuff > 0)
				clamDebuff--;
			if (sulphurPoison > 0)
				sulphurPoison--;
            if (sagePoisonTime > 0)
                sagePoisonTime--;
            if (kamiFlu > 0)
                kamiFlu--;
            if (relicOfResilienceCooldown > 0)
                relicOfResilienceCooldown--;
            if (relicOfResilienceWeakness > 0)
                relicOfResilienceWeakness--;
            if (GaussFluxTimer > 0)
                GaussFluxTimer--;
            if (ladHearts > 0)
				ladHearts--;
            if (vulnerabilityHex > 0)
                vulnerabilityHex--;
            if (banishingFire > 0)
				banishingFire--;
            if (wither > 0)
				wither--;

			// Queen Bee is completely immune to having her movement impaired if not in a high difficulty mode.
			if (npc.type == NPCID.QueenBee && !CalamityWorld.revenge && !CalamityWorld.malice && !BossRushEvent.BossRushActive)
				return;

			if (debuffResistanceTimer <= 0 || (debuffResistanceTimer > slowingDebuffResistanceMin))
			{
				if (eFreeze <= 0 && gState <= 0 && tSad <= 0)
				{
					if (eutrophication > 0)
					{
						float velocityMult = 0.5f;
						if (VulnerableToWater.HasValue)
						{
							if (VulnerableToWater.Value)
								velocityMult = 0.25f;
							else
								velocityMult = 0.75f;
						}
						npc.velocity *= velocityMult;
					}
					else if (timeSlow > 0 || webbed > 0)
					{
						npc.velocity *= 0.85f;
					}
					else if (slowed > 0 || tesla > 0 || vaporfied > 0)
					{
						float velocityMult = 0.9f;
						if (tesla > 0)
						{
							if (VulnerableToElectricity.HasValue)
							{
								if (VulnerableToElectricity.Value)
									velocityMult = 0.8f;
								else
									velocityMult = 0.95f;
							}
						}
						npc.velocity *= velocityMult;
					}
					else if (vulnerabilityHex > 0)
					{
						npc.velocity = Vector2.Clamp(npc.velocity, new Vector2(-Calamity.MaxNPCSpeed), new Vector2(Calamity.MaxNPCSpeed, 10f));
					}
					else if (kamiFlu > 420)
					{
						npc.velocity = Vector2.Clamp(npc.velocity, new Vector2(-KamiDebuff.MaxNPCSpeed), new Vector2(KamiDebuff.MaxNPCSpeed));
					}
				}
			}

            if (!CalamityPlayer.areThereAnyDamnBosses && !CalamityLists.enemyImmunityList.Contains(npc.type))
			{
				if (pearlAura > 0)
					npc.velocity *= 0.9f;
			}

			if (npc.type == NPCID.DD2EterniaCrystal)
				CalamityGlobalAI.DD2CrystalExtraAI(npc);
		}
        #endregion

        #region On Hit Player
        public override void OnHitPlayer(NPC npc, Player target, int damage, bool crit)
        {
            if (target.Calamity().snowman)
            {
                if (npc.type == NPCID.Demon || npc.type == NPCID.VoodooDemon || npc.type == NPCID.RedDevil)
                {
                    target.AddBuff(BuffType<PopoNoselessBuff>(), 36000);
                }
            }

            switch (npc.type)
            {
                case NPCID.Golem:
                    target.AddBuff(BuffType<ArmorCrunch>(), 300);
                    break;

                case NPCID.GolemHead:
                case NPCID.GolemHeadFree:
                case NPCID.GolemFistRight:
                case NPCID.GolemFistLeft:
                    target.AddBuff(BuffType<ArmorCrunch>(), 180);
                    break;

                case NPCID.Lavabat:
                    target.AddBuff(BuffID.OnFire, 180);
                    break;

                default:
                    break;
            }

			if (Main.hardMode)
			{
				switch (npc.type)
				{
					case NPCID.BigBoned:
					case NPCID.ShortBones:
					case NPCID.AngryBones:
					case NPCID.AngryBonesBig:
					case NPCID.AngryBonesBigMuscle:
					case NPCID.AngryBonesBigHelmet:
						target.AddBuff(BuffType<WarCleave>(), 60);
						break;

					default:
						break;
				}

				if (NPC.downedPlantBoss)
				{
					switch (npc.type)
					{
						case NPCID.RustyArmoredBonesAxe:
						case NPCID.RustyArmoredBonesFlail:
						case NPCID.RustyArmoredBonesSword:
						case NPCID.RustyArmoredBonesSwordNoArmor:
						case NPCID.BlueArmoredBones:
						case NPCID.BlueArmoredBonesMace:
						case NPCID.BlueArmoredBonesNoPants:
						case NPCID.BlueArmoredBonesSword:
						case NPCID.HellArmoredBones:
						case NPCID.HellArmoredBonesSpikeShield:
						case NPCID.HellArmoredBonesMace:
						case NPCID.HellArmoredBonesSword:
							target.AddBuff(BuffType<ArmorCrunch>(), 120);
							break;

						default:
							break;
					}
				}
			}

			if (Main.expertMode)
			{
				switch (npc.type)
				{
					case NPCID.Hellbat:
						target.AddBuff(BuffID.OnFire, 120);
						break;

					default:
						break;
				}
			}
        }
		#endregion

		#region Modify Hit
		public override void ModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit)
		{
			CalamityPlayer modPlayer = player.Calamity();
			if (modPlayer.camper && !player.StandingStill())
				damage = (int)(damage * 0.1);

			// True melee resists
			if (DesertScourgeIDs.Contains(npc.type) || EaterofWorldsIDs.Contains(npc.type) || npc.type == NPCID.Creeper ||
				PerforatorIDs.Contains(npc.type) || AquaticScourgeIDs.Contains(npc.type) || DestroyerIDs.Contains(npc.type) ||
				AstrumDeusIDs.Contains(npc.type) || StormWeaverIDs.Contains(npc.type) || ThanatosIDs.Contains(npc.type) ||
				npc.type == NPCType<DarkEnergy>() || npc.type == NPCType<RavagerBody>() || AresIDs.Contains(npc.type))
			{
				double damageMult = ThanatosIDs.Contains(npc.type) ? 0.35 : 0.5;
				if (item.melee && item.type != ItemType<UltimusCleaver>() && item.type != ItemType<InfernaCutter>())
					damage = (int)(damage * damageMult);
			}
			else if (npc.type == NPCType<Polterghast.Polterghast>())
			{
				if (item.type == ItemType<GrandDad>())
					damage = (int)(damage * 0.75);
			}
			else if (npc.type == NPCType<Signus.Signus>())
			{
				if (item.type == ItemType<GrandDad>())
					damage = (int)(damage * 0.75);
			}
		}
		#endregion

		#region Modify Hit By Projectile
		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
			Player player = Main.player[projectile.owner];
			CalamityPlayer modPlayer = player.Calamity();

			CalamityGlobalTownNPC.MakeTownNPCsTakeMoreDamage(npc, projectile, mod, ref damage);

			int bullseyeType = ProjectileType<SpiritOriginBullseye>();
			Projectile bullseye = null;
			for (int i = 0; i < Main.maxProjectiles; i++)
            {
				if (Main.projectile[i].type != bullseyeType || !Main.projectile[i].active || Main.projectile[i].owner != player.whoAmI)
					continue;

				// Only choose a bullseye if it is attached to the NPC that is being hit.
				if (npc.whoAmI == (int)Main.projectile[i].ai[0])
				{
					bullseye = Main.projectile[i];
					break;
				}
			}

			// Don't allow large hitbox projectiles or explosions to "snipe" enemies.
			// Hitbox criteria were changed to allow long one dimensional projectiles so that Condemnation would work.
			bool hitBullseye = false;
			bool acceptableVelocity = projectile.velocity != Vector2.Zero;
			bool acceptableHitbox = (projectile.width < 36) || (projectile.height < 36);
			if (bullseye != null && projectile.ranged && acceptableVelocity && acceptableHitbox)
			{
				if (bullseye != null)
				{
					// Bullseyes are visually different on bosses and thus have larger hitboxes.
					float bullseyeRadius = npc.IsABoss() ? DaawnlightSpiritOrigin.BossBullseyeRadius : DaawnlightSpiritOrigin.RegularEnemyBullseyeRadius;

					// Do some geometry + trig to determine if the projectile WOULD hit the bullseye, even if it's about to be deleted on-hit.
					// This is the equivalent of drawing a laser sight from the projectile along its velocity vector and seeing if it crosses the bullseye's hitbox.
					// To do this more reliably, we back the projectile up quite a distance.
					Vector2 normVelocity = projectile.velocity.SafeNormalize(Vector2.UnitY);
					Vector2 backedUpPosition = projectile.Center - 160f * normVelocity;
					Vector2 directionToBullseyeCenter = (bullseye.Center - backedUpPosition).SafeNormalize(Vector2.UnitY);
					Vector2 perp = directionToBullseyeCenter.RotatedBy(MathHelper.PiOver2);
					// Double the radius is given so that the cosine break-even point is right at the edge of the hitbox.
					Vector2 comparisonPointOne = bullseye.Center + perp * 2f * bullseyeRadius;
					Vector2 comparisonPointTwo = bullseye.Center - perp * 2f * bullseyeRadius;
					Vector2 dirToPointOne = (comparisonPointOne - backedUpPosition).SafeNormalize(-Vector2.UnitX);
					Vector2 dirToPointTwo = (comparisonPointTwo - backedUpPosition).SafeNormalize(Vector2.UnitX);

					// Law of cosines: (A dot B) = |A| * |B| * cos(theta)
					// where theta is the angle between the two vectors A and B.
					// cos(theta) approaches one as the angle approaches zero, so an angle is smaller if the cos is bigger.
					// If the angle to the bullseye's center is smaller than the angle to both the comparison points, it's a hit.
					float dotCenter = Vector2.Dot(normVelocity, directionToBullseyeCenter);
					float dotOne = Vector2.Dot(normVelocity, dirToPointOne);
					float dotTwo = Vector2.Dot(normVelocity, dirToPointTwo);
					bool willStrikeBullseye = dotCenter > dotOne && dotCenter > dotTwo;

					// If a bullseye is triggered, set it as hit.
					if (willStrikeBullseye && bullseye.ai[1] == 0f)
					{
						crit = true;
						hitBullseye = true;
						bullseye.ai[1] = 1f; // Make the bullseye disappear immediately.
						bullseye.netUpdate = true;
					}
				}

				// TODO -- Use the 1.4 spawn context system for this.
				// This suffers from the same issues as current crit logic in vanilla, but I have no idea how to do this better without said system.
				if (modPlayer.spiritOrigin && crit)
				{
					// Crits have their damage doubled after ModifyHitNPC, in the StrikeNPC function.
					// Here, damage is divded by 2 to compensate for that.
					// This means that the bonus provided by Daawnlight Spirit Origin can be computed as a complete replacement to regular crits.
					float mult = DaawnlightSpiritOrigin.GetDamageMultiplier(player, modPlayer, hitBullseye) / 2f;
					damage = (int)(damage * mult);
				}
			}

			if (!projectile.npcProj && !projectile.trap)
			{
				if (projectile.ranged && modPlayer.plagueReaper && pFlames > 0)
					damage = (int)(damage * 1.1);
			}

			// Any weapons that shoot projectiles from anywhere other than the player's center aren't affected by point-blank shot damage boost.
			if (!Main.player[projectile.owner].ActiveItem().IsAir && Main.player[projectile.owner].ActiveItem().Calamity().canFirePointBlankShots && projectile.ranged)
			{
				if (projectile.Calamity().pointBlankShotDuration > 0)
				{
					projectile.Calamity().pointBlankShotDuration = 0;
					damage = (int)(damage * 1.5);
				}
			}

			// Nerfed because the primary fire was buffed a lot.
			if (projectile.type == ProjectileID.MonkStaffT3_AltShot || (projectile.type == ProjectileID.Electrosphere && Main.player[projectile.owner].ActiveItem().type == ItemID.MonkStaffT3))
				damage /= 4;

			// Nerfed because these are really overpowered.
			if (projectile.type == ProjectileID.CursedDartFlame)
				damage /= 2;

			// Nerf Mushroom Spear mushrooms.
			if (projectile.type == ProjectileID.Mushroom && Main.player[projectile.owner].ActiveItem().type == ItemID.MushroomSpear)
				damage /= 2;

			if (projectile.type == ProjectileID.SeedlerNut || projectile.type == ProjectileID.SeedlerThorn)
				damage /= 3;

			if (CalamityLists.pierceResistList.Contains(npc.type))
				PierceResistGlobal(projectile, npc, ref damage);

			if (AresIDs.Contains(npc.type))
			{
				// 75% resist to Dragon Rage projectiles.
				if (projectile.type == ProjectileType<DragonRageStaff>() || projectile.type == ProjectileType<DragonRageFireball>() || (projectile.type == ProjectileType<FuckYou>() && projectile.melee))
					damage = (int)(damage * 0.25);

				// 60% resist to Murasama.
				else if (projectile.type == ProjectileType<MurasamaSlash>())
					damage = (int)(damage * 0.4);

				// 50% resist to true melee and Enforcer projectiles.
				else if (projectile.Calamity().trueMelee || projectile.type == ProjectileType<EssenceFlame2>())
					damage = (int)(damage * 0.5);

				// 20% resist to Eclipse's Fall stealth strike.
				else if (projectile.type == ProjectileType<EclipsesSmol>())
					damage = (int)(damage * 0.8);
			}
			else if (npc.type == NPCType<Artemis>() || npc.type == NPCType<Apollo>())
			{
				// 10% resist to Eclipse's Fall stealth strike.
				if (projectile.type == ProjectileType<EclipsesSmol>())
					damage = (int)(damage * 0.9);
			}
			else if (ThanatosIDs.Contains(npc.type))
			{
				// 75% resist to Celestus and Chicken Cannon.
				if (projectile.type == ProjectileType<CelestusBoomerang>() || projectile.type == ProjectileType<Celestus2>())
					damage = (int)(damage * 0.25);

				// 65% resist to true melee and Hadopelagic Echo.
				else if (projectile.Calamity().trueMelee || projectile.type == ProjectileType<HadopelagicEchoSoundwave>() || projectile.type == ProjectileType<HadopelagicEcho2>())
					damage = (int)(damage * 0.35);

				// 50% resist to Chicken Cannon, Vehemence skulls and Prismatic Breaker. (why is this more than BOTH Rancor and Yharim's Crystal?)
				else if (projectile.type == ProjectileType<ChickenExplosion>() || projectile.type == ProjectileType<VehemenceSkull>() || projectile.type == ProjectileType<PrismaticBeam>())
					damage = (int)(damage * 0.5);

				// 40% resist to Wrathwing stealth strike, Rancor, and Yharim's Crystal.
				else if (projectile.type == ProjectileType<WrathwingCinder>() || projectile.type == ProjectileType<RancorLaserbeam>() || projectile.type == ProjectileType<YharimsCrystalBeam>())
					damage = (int)(damage * 0.6);

				// 30% resist to Sirius.
				else if (projectile.type == ProjectileType<SiriusExplosion>())
					damage = (int)(damage * 0.7);

				// 25% resist to God Slayer Slugs and Luminite Bullets.
				else if (projectile.type == ProjectileID.MoonlordBullet || projectile.type == ProjectileType<GodSlayerSlugProj>())
					damage = (int)(damage * 0.75);

				// 20% resist to Eradicator beams, Voltaic Climax / Void Vortex hitscan and Blood Boiler fire.
				else if (projectile.type == ProjectileType<NebulaShot>() || projectile.type == ProjectileType<ClimaxBeam>() || projectile.type == ProjectileType<BloodBoilerFire>())
					damage = (int)(damage * 0.8);

				// 15% resist to Gruesome Eminence.
				else if (projectile.type == ProjectileType<SpiritCongregation>())
					damage = (int)(damage * 0.85);

			}
			else if (npc.type == NPCType<RavagerBody>())
			{
				// 50% resist to true melee.
				if (projectile.Calamity().trueMelee)
					damage = (int)(damage * 0.5);
			}
			else if (AstrumDeusIDs.Contains(npc.type))
			{
				// 75% resist to Plaguenades.
				if (projectile.type == ProjectileType<PlaguenadeBee>() || projectile.type == ProjectileType<PlaguenadeProj>())
					damage = (int)(damage * 0.25);

				// 50% resist to true melee.
				else if (projectile.Calamity().trueMelee)
					damage = (int)(damage * 0.5);

				// 25% resist to Resurrection Butterfly.
				else if (projectile.type == ProjectileType<SakuraBullet>() || projectile.type == ProjectileType<PurpleButterfly>())
					damage = (int)(damage * 0.75);
			}
			else if (npc.type == NPCType<BrimstoneHeart>())
			{
				// 30% resist to Surge Driver's alt click comets.
				if (projectile.type == ProjectileType<PrismComet>())
					damage = (int)(damage * 0.7);

				// 20% resist to Executioner's Blade stealth strikes.
				else if (projectile.type == ProjectileType<ExecutionersBladeStealthProj>())
					damage = (int)(damage * 0.8);
			}
			else if (DevourerOfGodsIDs.Contains(npc.type))
			{
				// 15% increased damage from Time Bolt stealth strikes.
				if (projectile.type == ProjectileType<TimeBoltKnife>())
				{
					if (projectile.Calamity().stealthStrike)
						damage = (int)(damage * 1.15);
				}
			}
			else if (npc.type == NPCType<DarkEnergy>())
			{
				// 50% resist to true melee.
				if (projectile.Calamity().trueMelee)
					damage = (int)(damage * 0.5);
			}
			else if (StormWeaverIDs.Contains(npc.type))
			{
				// 25% resist to Molten Amputator blobs.
				if (projectile.type == ProjectileType<MoltenBlobThrown>())
					damage = (int)(damage * 0.75);

				// 50% resist to true melee, Elemental Axe, Dazzling Stabber Staff and Pristine Fury.
				else if (projectile.Calamity().trueMelee || projectile.type == ProjectileType<ElementalAxeMinion>() || projectile.type == ProjectileType<DazzlingStabber>() || projectile.type == ProjectileType<PristineFire>())
					damage = (int)(damage * 0.5);
			}
			else if (DestroyerIDs.Contains(npc.type))
			{
				// 50% resist to true melee and Dormant Brimseekers.
				if (projectile.Calamity().trueMelee || projectile.type == ProjectileType<DormantBrimseekerBab>())
					damage = (int)(damage * 0.5);
			}
			else if (AquaticScourgeIDs.Contains(npc.type))
			{
				// 50% resist to true melee and Dormant Brimseekers.
				if (projectile.Calamity().trueMelee || projectile.type == ProjectileType<DormantBrimseekerBab>())
					damage = (int)(damage * 0.5);
			}
			else if (PerforatorIDs.Contains(npc.type))
			{
				// 50% resist to true melee.
				if (projectile.Calamity().trueMelee)
					damage = (int)(damage * 0.5);
			}
			else if (npc.type == NPCID.Creeper)
			{
				// 50% resist to true melee and Demon Scythe.
				if (projectile.Calamity().trueMelee || projectile.type == ProjectileID.DemonScythe)
					damage = (int)(damage * 0.5);
			}
			else if (EaterofWorldsIDs.Contains(npc.type))
			{
				// 50% resist to true melee and Demon Scythe.
				if (projectile.Calamity().trueMelee || projectile.type == ProjectileID.DemonScythe)
					damage = (int)(damage * 0.5);

				// 40% resist to Sky Glaze.
				else if (projectile.type == ProjectileType<StickyFeather>())
					damage = (int)(damage * 0.6);
			}
			else if (DesertScourgeIDs.Contains(npc.type))
			{
				// 50% resist to true melee.
				if (projectile.Calamity().trueMelee)
					damage = (int)(damage * 0.5);
			}
			else if (npc.type == NPCType<OldDuke.OldDuke>())
			{
				// 20% resist to Time Bolt.
                if (projectile.type == ProjectileType<TimeBoltKnife>())
                    damage = (int)(damage * 0.8);

				// 60% resist to Last Mourning.
				else if (projectile.type == ProjectileType<MourningSkull>() || projectile.type == ProjectileID.FlamingJack)
					damage = (int)(damage * 0.4);
			}
			else if (npc.type == NPCType<SoulSeekerSupreme>())
			{
				// 85% resist to Chicken Cannon.
				if (projectile.type == ProjectileType<ChickenExplosion>())
					damage = (int)(damage * 0.15);
				
				// 30% resist to Murasama.
				else if (projectile.type == ProjectileType<MurasamaSlash>())
					damage = (int)(damage * 0.7);

				// 25% resist to Yharim's Crystal.
				else if (projectile.type == ProjectileType<YharimsCrystalBeam>())
					damage = (int)(damage * 0.75);

				// 10% resist to Surge Driver's alt click comets and Executioner's Blade stealth strikes.
				else if (projectile.type == ProjectileType<ExecutionersBladeStealthProj>() || projectile.type == ProjectileType<PrismComet>())
					damage = (int)(damage * 0.9);

				// 10% resist to Celestus.
				else if (projectile.type == ProjectileType<CelestusBoomerang>() || projectile.type == ProjectileType<Celestus2>())
					damage = (int)(damage * 0.9);

				// 15% vulnerability to the Atom Splitter (Yes, this is kinda weird, but it's what Piky asked for).
				else if (projectile.type == ProjectileType<TheAtomSplitterProjectile>() || projectile.type == ProjectileType<TheAtomSplitterDuplicate>())
					damage = (int)(damage * 1.15);
			}
			else if (npc.type == NPCID.CultistBoss)
			{
				// 25% resist to Resurrection Butterfly.
				if (projectile.type == ProjectileType<PurpleButterfly>() || projectile.type == ProjectileType<SakuraBullet>())
					damage = (int)(damage * 0.75);
			}
			else if (npc.type == NPCID.DukeFishron)
			{
				// 35% increased damage from Resurrection Butterfly.
				if (projectile.type == ProjectileType<PurpleButterfly>() || projectile.type == ProjectileType<SakuraBullet>())
					damage = (int)(damage * 1.35);
			}

			if (modPlayer.camper && !player.StandingStill())
				damage = (int)(damage * 0.1);
		}

		private void PierceResistGlobal(Projectile projectile, NPC npc, ref int damage)
		{
			if (GrenadeResistIDs.Contains(projectile.type))
			{
				if (!EaterofWorldsIDs.Contains(npc.type))
					damage = (int)(damage * 0.2);
				else if (!Main.expertMode)
					damage = (int)(damage * 0.2);
			}

			// Thanatos segments do not trigger pierce resistance if they are closed
			if (ThanatosIDs.Contains(npc.type) && unbreakableDR)
				return;

			float damageReduction = projectile.Calamity().timesPierced * projectile.Calamity().PierceResistHarshness;
			if (damageReduction > projectile.Calamity().PierceResistCap)
				damageReduction = projectile.Calamity().PierceResistCap;

			damage -= (int)(damage * damageReduction);

			if ((projectile.penetrate > 1 || projectile.penetrate == -1) && !CalamityLists.pierceResistExceptionList.Contains(projectile.type) && !projectile.IsSummon() && projectile.aiStyle != 15 && projectile.aiStyle != 39 && projectile.aiStyle != 99)
				projectile.Calamity().timesPierced++;
		}
		#endregion

        #region Check Dead
        public override bool CheckDead(NPC npc)
        {
            if (npc.lifeMax > 1000 && npc.type != NPCID.DungeonSpirit &&
                npc.type != NPCType<PhantomSpirit>() &&
                npc.type != NPCType<PhantomSpiritS>() &&
                npc.type != NPCType<PhantomSpiritM>() &&
                npc.type != NPCType<PhantomSpiritL>() &&
                npc.value > 0f && npc.HasPlayerTarget &&
                NPC.downedMoonlord &&
                Main.player[npc.target].ZoneDungeon)
            {
                int maxValue = Main.expertMode ? 4 : 6;

                if (Main.rand.NextBool(maxValue) && Main.wallDungeon[Main.tile[(int)npc.Center.X / 16, (int)npc.Center.Y / 16].wall])
                {
					int randomType = Utils.SelectRandom(Main.rand, new int[]
					{
						NPCType<PhantomSpirit>(),
						NPCType<PhantomSpiritS>(),
						NPCType<PhantomSpiritM>(),
						NPCType<PhantomSpiritL>()
					});

                    NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, randomType, 0, 0f, 0f, 0f, 0f, 255);
                }
            }

            return true;
        }
        #endregion

        #region Hit Effect
        public override void HitEffect(NPC npc, int hitDirection, double damage)
        {
            if (CalamityWorld.revenge)
            {
                switch (npc.type)
                {
                    case NPCID.MotherSlime:
                        if (npc.life <= 0)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int slimeAmt = Main.rand.Next(2) + 2; //2 to 3 extra
                                for (int s = 0; s < slimeAmt; s++)
                                {
                                    int slime = NPC.NewNPC((int)npc.Center.X, (int)(npc.position.Y + npc.height), NPCID.BlueSlime, 0, 0f, 0f, 0f, 0f, 255);
                                    NPC npc2 = Main.npc[slime];
                                    npc2.SetDefaults(NPCID.BabySlime);
                                    npc2.velocity.X = npc.velocity.X * 2f;
                                    npc2.velocity.Y = npc.velocity.Y;
                                    npc2.velocity.X += Main.rand.Next(-20, 20) * 0.1f + s * npc.direction * 0.3f;
                                    npc2.velocity.Y -= Main.rand.Next(0, 10) * 0.1f + s;
                                    npc2.ai[0] = -1000 * Main.rand.Next(3);

                                    if (Main.netMode == NetmodeID.Server && slime < Main.maxNPCs)
                                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, slime, 0f, 0f, 0f, 0, 0, 0);
                                }
                            }
                        }
                        break;

                    case NPCID.Demon:
                    case NPCID.VoodooDemon:
                        npc.ai[0] += 1f;
                        break;

                    case NPCID.CursedHammer:
                    case NPCID.EnchantedSword:
                    case NPCID.CrimsonAxe:
                        if (npc.life <= npc.lifeMax * 0.5)
                            npc.justHit = false;

                        break;

                    case NPCID.Clinger:
                    case NPCID.Gastropod:
                    case NPCID.GiantTortoise:
                    case NPCID.IceTortoise:
                    case NPCID.BlackRecluse:
                    case NPCID.BlackRecluseWall:
                        if (npc.life <= npc.lifeMax * 0.25)
                            npc.justHit = false;

                        break;

                    case NPCID.Paladin:
                        if (npc.life <= npc.lifeMax * 0.15)
                            npc.justHit = false;

                        break;

                    case NPCID.Clown:
                        if (Main.netMode != NetmodeID.MultiplayerClient && !Main.player[npc.target].dead)
                            npc.ai[2] += 29f;

                        break;
                }

                if (npc.type == NPCType<SandTortoise>() || npc.type == NPCType<PlaguedTortoise>())
                {
                    if (npc.life <= npc.lifeMax * 0.25)
                        npc.justHit = false;
                }
            }

			if (npc.life <= 0 && BossRushEvent.BossRushActive && npc.type == NPCID.WallofFlesh && BossRushEvent.CurrentlyFoughtBoss == npc.type)
            {
				// Post-Wall of Flesh teleport back to spawn.
				// This appears to only work correctly client-side in MP.
				for (int playerIndex = 0; playerIndex < Main.maxPlayers; playerIndex++)
				{
					bool appropriatePlayer = Main.myPlayer == playerIndex;
					if (Main.player[playerIndex].active && appropriatePlayer)
						Main.player[playerIndex].Spawn();
				}
			}
        }
        #endregion

        #region Edit Spawn Rate
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
			// Biomes
            if (player.Calamity().ZoneSulphur)
            {
                spawnRate = (int)(spawnRate * 1.1);
                maxSpawns = (int)(maxSpawns * 0.8f);
                if (Main.raining)
                {
                    spawnRate = (int)(spawnRate * 0.7);
                    maxSpawns = (int)(maxSpawns * 1.2f);

                    if (!player.Calamity().ZoneAbyss && CalamityWorld.rainingAcid)
                    {
                        if (AcidRainEvent.AnyRainMinibosses)
                        {
                            maxSpawns = 5;
                            spawnRate *= 2;
                        }
                        else
                        {
                            spawnRate = Main.hardMode ? 36 : 33;
                            maxSpawns = Main.hardMode ? 15 : 12;
                        }
                    }
                }
            }
            else if (player.Calamity().ZoneAbyss)
            {
                spawnRate = (int)(spawnRate * 0.7);
                maxSpawns = (int)(maxSpawns * 1.1f);
            }
            else if (player.Calamity().ZoneCalamity)
            {
                spawnRate = (int)(spawnRate * 0.9);
                maxSpawns = (int)(maxSpawns * 1.1f);
            }
            else if (player.Calamity().ZoneAstral)
            {
                spawnRate = (int)(spawnRate * 0.6);
                maxSpawns = (int)(maxSpawns * 1.2f);
            }
            else if (player.Calamity().ZoneSunkenSea)
            {
                spawnRate = (int)(spawnRate * 0.9);
                maxSpawns = (int)(maxSpawns * 1.1f);
            }

			// Boosts
			if (CalamityWorld.downedDoG && (Main.pumpkinMoon || Main.snowMoon || Main.eclipse))
			{
				spawnRate = (int)(spawnRate * 0.75);
				maxSpawns = (int)(maxSpawns * 3f);
			}

			if (player.Calamity().clamity)
			{
				spawnRate = (int)(spawnRate * 0.02);
				maxSpawns = (int)(maxSpawns * 1.5f);
			}

			if (CalamityWorld.death && Main.bloodMoon)
			{
				spawnRate = (int)(spawnRate * 0.25);
				maxSpawns = (int)(maxSpawns * 10f);
			}

			if (NPC.LunarApocalypseIsUp)
			{
				if ((player.ZoneTowerNebula && NPC.ShieldStrengthTowerNebula == 0) || (player.ZoneTowerStardust && NPC.ShieldStrengthTowerStardust == 0) ||
					(player.ZoneTowerVortex && NPC.ShieldStrengthTowerVortex == 0) || (player.ZoneTowerSolar && NPC.ShieldStrengthTowerSolar == 0))
				{
					spawnRate = (int)(spawnRate * 0.85);
					maxSpawns = (int)(maxSpawns * 1.25f);
				}
			}

			if (CalamityWorld.revenge)
				spawnRate = (int)(spawnRate * 0.85);

			if (CalamityWorld.demonMode)
				spawnRate = (int)(spawnRate * 0.75);

			if (Main.waterCandles > 0)
			{
				spawnRate = (int)(spawnRate * 0.9);
				maxSpawns = (int)(maxSpawns * 1.1f);
			}
			if (player.enemySpawns)
			{
				spawnRate = (int)(spawnRate * 0.8);
				maxSpawns = (int)(maxSpawns * 1.2f);
			}
			if (player.Calamity().chaosCandle)
			{
				spawnRate = (int)(spawnRate * 0.6);
				maxSpawns = (int)(maxSpawns * 2.5f);
			}
			if (player.Calamity().zerg)
            {
                spawnRate = (int)(spawnRate * 0.2);
                maxSpawns = (int)(maxSpawns * 5f);
            }
			if (NPC.AnyNPCs(NPCType<WulfrumPylon>()))
			{
				int otherWulfrumEnemies = NPC.CountNPCS(NPCType<WulfrumDrone>()) + NPC.CountNPCS(NPCType<WulfrumGyrator>()) + NPC.CountNPCS(NPCType<WulfrumHovercraft>()) + NPC.CountNPCS(NPCType<WulfrumRover>());
				if (otherWulfrumEnemies < 4)
				{
					spawnRate = (int)(spawnRate * 0.8);
					maxSpawns = (int)(maxSpawns * 1.2f);
				}
			}

			// Reductions
			if (Main.peaceCandles > 0)
			{
				spawnRate = (int)(spawnRate * 1.1);
				maxSpawns = (int)(maxSpawns * 0.9f);
			}
			if (player.calmed)
			{
				spawnRate = (int)(spawnRate * 1.2);
				maxSpawns = (int)(maxSpawns * 0.8f);
			}
            if (player.Calamity().tranquilityCandle)
            {
                spawnRate = (int)(spawnRate * 1.4);
                maxSpawns = (int)(maxSpawns * 0.4f);
            }
			if (player.Calamity().zen || (CalamityConfig.Instance.DisableExpertTownSpawns && player.townNPCs > 1f && Main.expertMode))
			{
				spawnRate = (int)(spawnRate * 2.5);
				maxSpawns = (int)(maxSpawns * 0.3f);
			}
			if ((player.Calamity().bossZen || CalamityWorld.DoGSecondStageCountdown > 0) && CalamityConfig.Instance.BossZen)
			{
				spawnRate *= 5;
				maxSpawns = (int)(maxSpawns * 0.001f);
			}
		}
        #endregion

        #region Edit Spawn Range
        public override void EditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY, ref int safeRangeX, ref int safeRangeY)
        {
            if (player.Calamity().ZoneAbyss)
            {
                spawnRangeX = (int)(1920 / 16 * 0.5); //0.7
                safeRangeX = (int)(1920 / 16 * 0.32); //0.52
            }
        }
		#endregion

		#region Edit Spawn Pool

		internal static readonly FieldInfo MaxSpawnsField = typeof(NPC).GetField("maxSpawns", BindingFlags.NonPublic | BindingFlags.Static);
		public static void AttemptToSpawnLabCritters(Player player)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			int spawnRate = 400;
			int maxSpawnCount = (int)MaxSpawnsField.GetValue(null);
			NPCLoader.EditSpawnRate(player, ref spawnRate, ref maxSpawnCount);

			// Enforce a limit on the amount of enemies that can appear.
			if (player.activeNPCs >= maxSpawnCount)
				return;

			for (int i = 0; i < 8; i++)
			{
				int checkPositionX = (int)(player.Center.X / 16 + Main.rand.Next(30, 54) * Main.rand.NextBool(2).ToDirectionInt());
				int checkPositionY = (int)(player.Center.Y / 16 + Main.rand.Next(24, 45) * Main.rand.NextBool(2).ToDirectionInt());
				Vector2 checkPosition = new Vector2(checkPositionX, checkPositionY);

				Tile aboveSpawnTile = CalamityUtils.ParanoidTileRetrieval(checkPositionX, checkPositionY - 1);
				bool nearLab = CalamityUtils.ManhattanDistance(checkPosition, CalamityWorld.SunkenSeaLabCenter / 16f) < 180f;
				nearLab |= CalamityUtils.ManhattanDistance(checkPosition, CalamityWorld.PlanetoidLabCenter / 16f) < 180f;
				nearLab |= CalamityUtils.ManhattanDistance(checkPosition, CalamityWorld.JungleLabCenter / 16f) < 180f;
				nearLab |= CalamityUtils.ManhattanDistance(checkPosition, CalamityWorld.HellLabCenter / 16f) < 180f;
				nearLab |= CalamityUtils.ManhattanDistance(checkPosition, CalamityWorld.IceLabCenter / 16f) < 180f;

				bool isLabWall = aboveSpawnTile.wall == WallType<HazardChevronWall>() || aboveSpawnTile.wall == WallType<LaboratoryPanelWall>() || aboveSpawnTile.wall == WallType<LaboratoryPlateBeam>();
				isLabWall |= aboveSpawnTile.wall == WallType<LaboratoryPlatePillar>() || aboveSpawnTile.wall == WallType<LaboratoryPlatingWall>() || aboveSpawnTile.wall == WallType<RustedPlateBeam>();
				if (!isLabWall || !nearLab || Collision.SolidCollision((checkPosition - new Vector2(2f, 2f)).ToWorldCoordinates(), 4, 4) || player.activeNPCs >= maxSpawnCount || !Main.rand.NextBool(spawnRate))
					continue;

				WeightedRandom<int> pool = new WeightedRandom<int>();
				pool.Add(NPCID.None, 0f);
				pool.Add(NPCType<RepairUnitCritter>(), 0.2f);

				int typeToSpawn = pool.Get();
				if (typeToSpawn != NPCID.None)
				{
					int spawnedNPC = NPCLoader.SpawnNPC(typeToSpawn, checkPositionX, checkPositionY - 1);
					if (Main.netMode == NetmodeID.Server && spawnedNPC < Main.maxNPCs)
					{
						Main.npc[spawnedNPC].position.Y -= 8f;
						NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, spawnedNPC);
						return;
					}
				}
			}
		}

        public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
        {
			bool calamityBiomeZone = spawnInfo.player.Calamity().ZoneAbyss ||
				spawnInfo.player.Calamity().ZoneCalamity ||
				spawnInfo.player.Calamity().ZoneSulphur ||
				spawnInfo.player.Calamity().ZoneSunkenSea ||
				(spawnInfo.player.Calamity().ZoneAstral && !NPC.LunarApocalypseIsUp);

			// Spawn Green Jellyfish in prehm and Blue Jellyfish in hardmode
			if (spawnInfo.player.ZoneRockLayerHeight && spawnInfo.water && !calamityBiomeZone)
			{
				if (!Main.hardMode)
					pool[NPCID.GreenJellyfish] = SpawnCondition.CaveJellyfish.Chance * 0.5f;
				else
					pool[NPCID.BlueJellyfish] = SpawnCondition.CaveJellyfish.Chance;
			}

			// Add Truffle Worm spawns to surface mushroom biome
			if (spawnInfo.player.ZoneGlowshroom && Main.hardMode && (spawnInfo.player.ZoneOverworldHeight || spawnInfo.player.ZoneSkyHeight))
			{
				if (NPC.CountNPCS(NPCID.TruffleWorm) < 2)
					pool[NPCID.TruffleWorm] = SpawnCondition.OverworldMushroom.Chance * 0.5f;
			}

			if (calamityBiomeZone)
            {
                pool[0] = 0f;
            }

            if (spawnInfo.player.Calamity().ZoneSulphur && !spawnInfo.player.Calamity().ZoneAbyss && CalamityWorld.rainingAcid)
            {
                pool.Clear();

                if (!(CalamityWorld.downedPolterghast && CalamityWorld.acidRainPoints == 1))
                {
                    Dictionary<int, AcidRainSpawnData> PossibleEnemies = AcidRainEvent.PossibleEnemiesPreHM;
                    Dictionary<int, AcidRainSpawnData> PossibleMinibosses = new Dictionary<int, AcidRainSpawnData>();
                    if (CalamityWorld.downedAquaticScourge)
                    {
                        PossibleEnemies = AcidRainEvent.PossibleEnemiesAS;
                        PossibleMinibosses = AcidRainEvent.PossibleMinibossesAS;
                        if (!PossibleEnemies.ContainsKey(NPCType<IrradiatedSlime>()))
                        {
                            PossibleEnemies.Add(NPCType<IrradiatedSlime>(), new AcidRainSpawnData(1, 0f, AcidRainSpawnRequirement.Anywhere));
                        }
                    }
                    if (CalamityWorld.downedPolterghast)
                    {
                        PossibleEnemies = AcidRainEvent.PossibleEnemiesPolter;
                        PossibleMinibosses = AcidRainEvent.PossibleMinibossesPolter;
                    }
                    foreach (int enemy in PossibleEnemies.Select(enemyType => enemyType.Key))
                    {
                        bool canSpawn = true;
                        switch (PossibleEnemies[enemy].SpawnRequirement)
                        {
                            case AcidRainSpawnRequirement.Anywhere:
                                break;
                            case AcidRainSpawnRequirement.Land:
                                canSpawn = !spawnInfo.water;
                                break;
                            case AcidRainSpawnRequirement.Water:
                                canSpawn = spawnInfo.water;
                                break;
                        }
                        if (canSpawn)
                        {
                            if (!pool.ContainsKey(enemy))
                            {
                                pool.Add(enemy, PossibleEnemies[enemy].SpawnRate);
                            }
                        }
                    }
                    if (PossibleMinibosses.Count > 0)
                    {
                        foreach (int miniboss in PossibleMinibosses.Select(miniboss => miniboss.Key).ToList())
                        {
                            bool canSpawn = true;
                            switch (PossibleMinibosses[miniboss].SpawnRequirement)
                            {
                                case AcidRainSpawnRequirement.Anywhere:
                                    break;
                                case AcidRainSpawnRequirement.Land:
                                    canSpawn = !spawnInfo.water;
                                    break;
                                case AcidRainSpawnRequirement.Water:
                                    canSpawn = spawnInfo.water;
                                    break;
                            }
                            if (canSpawn)
                            {
                                pool.Add(miniboss, PossibleMinibosses[miniboss].SpawnRate);
                            }
                        }
                    }
                    if (NPC.CountNPCS(NPCType<NuclearToad>()) >= AcidRainEvent.MaxNuclearToadCount)
                    {
                        pool.Remove(NPCType<NuclearToad>());
                    }
                }
            }

			if (spawnInfo.playerSafe)
				return;

			if (!Main.hardMode && spawnInfo.player.ZoneUnderworldHeight && !calamityBiomeZone)
			{
				if (!NPC.AnyNPCs(NPCID.VoodooDemon))
					pool[NPCID.VoodooDemon] = SpawnCondition.Underworld.Chance * 0.75f;
			}

            if (spawnInfo.player.Calamity().disableVoodooSpawns && pool.ContainsKey(NPCID.VoodooDemon))
                pool.Remove(NPCID.VoodooDemon);
		}
        #endregion

        #region Drawing
        public override void FindFrame(NPC npc, int frameHeight)
        {
            if (CalamityWorld.revenge || BossRushEvent.BossRushActive || CalamityWorld.malice)
            {
                if (npc.type == NPCID.SkeletronPrime)
                    npc.frameCounter = 0D;
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
			if (vaporfied > 0)
			{
				int dustType = Utils.SelectRandom(Main.rand, new int[]
				{
					246,
					242,
					229,
					226,
					247,
					187,
					234
				});

				if (Main.rand.Next(5) < 4)
				{
					int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, dustType, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default, 3f);
					Main.dust[dust].noGravity = true;
					Main.dust[dust].velocity *= 1.8f;
					Main.dust[dust].velocity.Y -= 0.5f;
					if (Main.rand.NextBool(4))
					{
						Main.dust[dust].noGravity = false;
						Main.dust[dust].scale *= 0.5f;
					}
				}
			}

			if (bBlood > 0)
            {
                if (Main.rand.Next(5) < 4)
                {
                    int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, 5, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default, 3f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.8f;
                    Main.dust[dust].velocity.Y -= 0.5f;
                    if (Main.rand.NextBool(4))
                    {
                        Main.dust[dust].noGravity = false;
                        Main.dust[dust].scale *= 0.5f;
                    }
                }
                Lighting.AddLight(npc.position, 0.08f, 0f, 0f);
            }

            if (bFlames > 0 || enraged > 0)
            {
                if (Main.rand.Next(5) < 4)
                {
                    int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, DustType<BrimstoneFlame>(), npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default, 3.5f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.8f;
                    Main.dust[dust].velocity.Y -= 0.5f;
                    if (Main.rand.NextBool(4))
                    {
                        Main.dust[dust].noGravity = false;
                        Main.dust[dust].scale *= 0.5f;
                    }
                }
                Lighting.AddLight(npc.position, 0.05f, 0.01f, 0.01f);
            }

            if (aFlames > 0)
            {
                if (Main.rand.Next(5) < 4)
                {
                    int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, DustType<BrimstoneFlame>(), npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default, 3.5f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.8f;
                    Main.dust[dust].velocity.Y -= 0.25f;
                    if (Main.rand.NextBool(4))
                    {
                        Main.dust[dust].noGravity = false;
                        Main.dust[dust].scale *= 0.35f;
                    }
                }
                Lighting.AddLight(npc.position, 0.025f, 0f, 0f);
            }

            if (pShred > 0)
            {
                if (Main.rand.Next(5) < 4)
                {
                    int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, 5, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default, 1.5f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.1f;
                    Main.dust[dust].velocity.Y += 0.25f;
                    if (Main.rand.NextBool(2))
                    {
                        Main.dust[dust].noGravity = false;
                        Main.dust[dust].scale *= 0.5f;
                    }
                }
            }

            if (hFlames > 0 || banishingFire > 0)
            {
                if (Main.rand.Next(5) < 4)
                {
                    int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, DustType<HolyFireDust>(), npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default, 3.5f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.8f;
                    Main.dust[dust].velocity.Y -= 0.5f;
                    if (Main.rand.NextBool(4))
                    {
                        Main.dust[dust].noGravity = false;
                        Main.dust[dust].scale *= 0.5f;
                    }
                }
                Lighting.AddLight(npc.position, 0.25f, 0.25f, 0.1f);
            }

            if (pFlames > 0)
            {
                if (Main.rand.Next(5) < 4)
                {
                    int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, 89, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default, 3.5f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.2f;
                    Main.dust[dust].velocity.Y -= 0.15f;
                    if (Main.rand.NextBool(4))
                    {
                        Main.dust[dust].noGravity = false;
                        Main.dust[dust].scale *= 0.5f;
                    }
                }
                Lighting.AddLight(npc.position, 0.07f, 0.15f, 0.01f);
            }

            if (gsInferno > 0)
            {
                if (Main.rand.Next(5) < 4)
                {
                    int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, (int)CalamityDusts.PurpleCosmilite, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default, 1.5f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.2f;
                    Main.dust[dust].velocity.Y -= 0.15f;
                    if (Main.rand.NextBool(4))
                    {
                        Main.dust[dust].noGravity = false;
                        Main.dust[dust].scale *= 0.5f;
                    }
                }
                Lighting.AddLight(npc.position, 0.1f, 0f, 0.135f);
            }

            if (astralInfection > 0)
            {
                if (Main.rand.Next(5) < 3)
                {
                    int dustType = Main.rand.NextBool(2) ? DustType<AstralOrange>() : DustType<AstralBlue>();
                    int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, dustType, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default, 0.6f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.2f;
                    Main.dust[dust].velocity.Y -= 0.15f;
                    Main.dust[dust].color = new Color(255, 255, 255, 0);
                    if (Main.rand.NextBool(4))
                    {
                        Main.dust[dust].noGravity = false;
                        Main.dust[dust].scale *= 0.5f;
                    }
                }
            }

            if (nightwither > 0)
            {
                Rectangle hitbox = npc.Hitbox;
                if (Main.rand.Next(5) < 4)
                {
                    int num3 = Utils.SelectRandom(Main.rand, new int[]
                    {
                        (int)CalamityDusts.PurpleCosmilite,
                        27,
                        234
                    });

                    int num4 = Dust.NewDust(hitbox.TopLeft(), npc.width, npc.height, num3, 0f, -2.5f, 0, default, 1f);
                    Dust dust = Main.dust[num4];
                    dust.noGravity = true;
                    dust.alpha = 200;
                    dust.velocity.Y -= 0.2f;
                    dust.velocity *= 1.2f;
                    dust.scale += Main.rand.NextFloat();
                }
            }

            if (tSad > 0 || cDepth > 0 || eutrophication > 0)
            {
                if (Main.rand.Next(6) < 3)
                {
                    int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, 33, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default, 3.5f);
                    Main.dust[dust].noGravity = false;
                    Main.dust[dust].velocity *= 1.2f;
                    Main.dust[dust].velocity.Y += 0.15f;
                    if (Main.rand.NextBool(4))
                    {
                        Main.dust[dust].noGravity = false;
                        Main.dust[dust].scale *= 0.5f;
                    }
                }
            }

            if (dFlames > 0)
            {
                if (Main.rand.Next(5) < 4)
                {
                    int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, (int)CalamityDusts.PurpleCosmilite, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default, 1.5f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.2f;
                    Main.dust[dust].velocity.Y -= 0.15f;
                    if (Main.rand.NextBool(4))
                    {
                        Main.dust[dust].noGravity = false;
                        Main.dust[dust].scale *= 0.5f;
                    }
                }
                Lighting.AddLight(npc.position, 0.1f, 0f, 0.135f);
            }

            if (sulphurPoison > 0)
            {
                if (Main.rand.Next(5) < 4)
                {
                    int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, 171, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default, 1.5f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.1f;
                    Main.dust[dust].velocity.Y += 0.25f;
                    if (Main.rand.NextBool(2))
                    {
                        Main.dust[dust].noGravity = false;
                        Main.dust[dust].scale *= 0.5f;
                    }
                }
            }
            if (webbed > 0)
            {
                if (Main.rand.Next(5) < 4)
                {
                    int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, 30, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default, 1.5f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.1f;
                    Main.dust[dust].velocity.Y += 0.25f;
                    if (Main.rand.NextBool(2))
                    {
                        Main.dust[dust].noGravity = false;
                        Main.dust[dust].scale *= 0.5f;
                    }
                }
            }
            if (slowed > 0)
            {
                if (Main.rand.Next(5) < 4)
                {
                    int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, 191, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 225, default, 3f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.1f;
                    Main.dust[dust].velocity.Y += 0.25f;
                    if (Main.rand.NextBool(2))
                    {
                        Main.dust[dust].noGravity = false;
                        Main.dust[dust].scale *= 0.5f;
                    }
                }
            }
            if (electrified > 0)
            {
                if (Main.rand.Next(5) < 4)
                {
                    int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, 132, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 0, default, 1f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.1f;
                    Main.dust[dust].velocity.Y += 0.25f;
                    if (Main.rand.NextBool(2))
                    {
                        Main.dust[dust].noGravity = false;
                        Main.dust[dust].scale *= 0.5f;
                    }
                }
            }
			if (ladHearts > 0 && !npc.loveStruck)
			{
				if (Main.rand.NextBool(5))
				{
					Vector2 velocity = CalamityUtils.RandomVelocity(10f, 1f, 1f, 0.66f);
					int heart = Gore.NewGore(npc.position + new Vector2(Main.rand.Next(npc.width + 1), Main.rand.Next(npc.height + 1)), velocity * Main.rand.Next(3, 6) * 0.33f, 331, Main.rand.Next(40, 121) * 0.01f);
					Main.gore[heart].sticky = false;
					Main.gore[heart].velocity *= 0.4f;
					Main.gore[heart].velocity.Y -= 0.6f;
				}
			}

            if (gState > 0 || eFreeze > 0)
                drawColor = Color.Cyan;

            if (marked > 0 || sulphurPoison > 0 || vaporfied > 0)
                drawColor = Color.Fuchsia;

            if (pearlAura > 0)
                drawColor = Color.White;

            if (timeSlow > 0 || tesla > 0)
                drawColor = Color.Aquamarine;
        }

        public override Color? GetAlpha(NPC npc, Color drawColor)
        {
            if (Main.LocalPlayer.Calamity().trippy)
                return new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB, npc.alpha);

            if (enraged > 0)
                return new Color(200, 50, 50, npc.alpha);

            if (npc.Calamity().kamiFlu > 0 && !CalamityLists.kamiDebuffColorImmuneList.Contains(npc.type))
                return new Color(51, 197, 108, npc.alpha);

			if (npc.type == NPCID.AncientDoom)
				return new Color(255, 255, 255, npc.alpha);

			return null;
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor)
        {
			if (npc.type != NPCID.BrainofCthulhu && (npc.type != NPCID.DukeFishron || npc.ai[0] <= 9f))
			{
				if (CalamityConfig.Instance.DebuffDisplay && (npc.boss || BossHealthBarManager.MinibossHPBarList.Contains(npc.type) || BossHealthBarManager.OneToMany.ContainsKey(npc.type) || CalamityLists.needsDebuffIconDisplayList.Contains(npc.type)))
				{
					List<Texture2D> buffTextureList = new List<Texture2D>();

					// Damage over time debuffs
					if (aFlames > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/DamageOverTime/AbyssalFlames"));
					if (astralInfection > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/DamageOverTime/AstralInfectionDebuff"));
					if (bFlames > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/DamageOverTime/BrimstoneFlames"));
					if (bBlood > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/DamageOverTime/BurningBlood"));
					if (cDepth > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/DamageOverTime/CrushDepth"));
					if (dFlames > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/DamageOverTime/DemonFlames"));
					if (gsInferno > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/DamageOverTime/GodSlayerInferno"));
					if (hFlames > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/DamageOverTime/HolyFlames"));
					if (nightwither > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/DamageOverTime/Nightwither"));
					if (pFlames > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/DamageOverTime/Plague"));
					if (shellfishVore > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/DamageOverTime/ShellfishEating"));
					if (pShred > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/DamageOverTime/Shred"));
					if (clamDebuff > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/DamageOverTime/SnapClamDebuff"));
					if (sulphurPoison > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/DamageOverTime/SulphuricPoisoning"));
                    if (sagePoisonTime > 0)
                        buffTextureList.Add(GetTexture("CalamityMod/Buffs/DamageOverTime/SagePoison"));
                    if (vaporfied > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/DamageOverTime/Vaporfied"));

					// Stat debuffs
					if (aCrunch > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/StatDebuffs/ArmorCrunch"));
					if (enraged > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/StatDebuffs/Enraged"));
					if (eutrophication > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/StatDebuffs/Eutrophication"));
					if (eFreeze > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/StatDebuffs/ExoFreeze"));
					if (gState > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/StatDebuffs/GlacialState"));
					if (irradiated > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/StatDebuffs/Irradiated"));
					if (kamiFlu > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/StatDebuffs/KamiDebuff"));
					if (marked > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/StatDebuffs/MarkedforDeath"));
					if (pearlAura > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/StatDebuffs/PearlAura"));
                    if (relicOfResilienceWeakness > 0)
                        buffTextureList.Add(GetTexture("CalamityMod/Buffs/StatDebuffs/ProfanedWeakness"));
					if (tSad > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/StatDebuffs/TemporalSadness"));
					if (tesla > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/StatDebuffs/TeslaFreeze"));
					if (timeSlow > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/StatDebuffs/TimeSlow"));
					if (wCleave > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/StatDebuffs/WarCleave"));
					if (wDeath > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/StatDebuffs/WhisperingDeath"));
					if (wither > 0)
						buffTextureList.Add(GetTexture("CalamityMod/Buffs/StatDebuffs/WitherDebuff"));

					// Vanilla damage over time debuffs
					if (electrified > 0)
						buffTextureList.Add(Main.buffTexture[BuffID.Electrified]);
					if (npc.onFire)
						buffTextureList.Add(Main.buffTexture[BuffID.OnFire]);
					if (npc.poisoned)
						buffTextureList.Add(Main.buffTexture[BuffID.Poisoned]);
					if (npc.onFire2)
						buffTextureList.Add(Main.buffTexture[BuffID.CursedInferno]);
					if (npc.onFrostBurn)
						buffTextureList.Add(Main.buffTexture[BuffID.Frostburn]);
					if (npc.venom)
						buffTextureList.Add(Main.buffTexture[BuffID.Venom]);
					if (npc.shadowFlame)
						buffTextureList.Add(Main.buffTexture[BuffID.ShadowFlame]);
					if (npc.oiled)
						buffTextureList.Add(Main.buffTexture[BuffID.Oiled]);
					if (npc.javelined)
						buffTextureList.Add(Main.buffTexture[BuffID.BoneJavelin]);
					if (npc.daybreak)
						buffTextureList.Add(Main.buffTexture[BuffID.Daybreak]);
					if (npc.celled)
						buffTextureList.Add(Main.buffTexture[BuffID.StardustMinionBleed]);
					if (npc.dryadBane)
						buffTextureList.Add(Main.buffTexture[BuffID.DryadsWardDebuff]);
					if (npc.dryadWard)
						buffTextureList.Add(Main.buffTexture[BuffID.DryadsWard]);
					if (npc.soulDrain && npc.realLife == -1)
						buffTextureList.Add(Main.buffTexture[BuffID.SoulDrain]);

					// Vanilla stat debuffs
					if (npc.confused)
						buffTextureList.Add(Main.buffTexture[BuffID.Confused]);
					if (npc.ichor)
						buffTextureList.Add(Main.buffTexture[BuffID.Ichor]);
					if (slowed > 0)
						buffTextureList.Add(Main.buffTexture[BuffID.Slow]);
					if (webbed > 0)
						buffTextureList.Add(Main.buffTexture[BuffID.Webbed]);
					if (npc.midas)
						buffTextureList.Add(Main.buffTexture[BuffID.Midas]);
					if (npc.loveStruck)
						buffTextureList.Add(Main.buffTexture[BuffID.Lovestruck]);
					if (npc.stinky)
						buffTextureList.Add(Main.buffTexture[BuffID.Stinky]);
					if (npc.betsysCurse)
						buffTextureList.Add(Main.buffTexture[BuffID.BetsysCurse]);
					if (npc.dripping)
						buffTextureList.Add(Main.buffTexture[BuffID.Wet]);
					if (npc.drippingSlime)
						buffTextureList.Add(Main.buffTexture[BuffID.Slimed]);

					// Total amount of elements in the buff list
					int buffTextureListLength = buffTextureList.Count;

					// Total length of a single row in the buff display
					int totalLength = buffTextureListLength * 14;

					// Max amount of buffs per row
					int buffDisplayRowLimit = 5;

					/* The maximum length of a single row in the buff display
					 * Limited to 80 units, because every buff drawn here is half the size of a normal buff, 16 x 16, 16 * 5 = 80 units*/
					float drawPosX = totalLength >= 80f ? 40f : (float)(totalLength / 2);

					// The height of a single frame of the npc
					float npcHeight = (float)(Main.npcTexture[npc.type].Height / Main.npcFrameCount[npc.type] / 2) * npc.scale;

					// Offset the debuff display based on the npc's graphical offset, and 16 units, to create some space between the sprite and the display
					float drawPosY = npcHeight + npc.gfxOffY + 16f;

					// The position where the display is drawn
					Vector2 drawPos = npc.Center - Main.screenPosition;

					// Iterate through the buff texture list
					for (int i = 0; i < buffTextureList.Count; i++)
					{
						// Reset the X position of the display every 5th and non-zero iteration, otherwise decrease the X draw position by 16 units
						if (i != 0)
						{
							if (i % buffDisplayRowLimit == 0)
								drawPosX = 40f;
							else
								drawPosX -= 14f;
						}

						// Offset the Y position every row after 5 iterations to limit each displayed row to 5 debuffs
						float additionalYOffset = 14f * (float)Math.Floor(i * 0.2);

						// Draw the display
						spriteBatch.Draw(buffTextureList.ElementAt(i), drawPos - new Vector2(drawPosX, drawPosY + additionalYOffset), null, Color.White, 0f, default, 0.5f, SpriteEffects.None, 0f);
					}
				}
			}

			CalamityGlobalTownNPC.TownNPCAlertSystem(npc, mod, spriteBatch);

			if (CalamityWorld.revenge || BossRushEvent.BossRushActive || CalamityWorld.malice)
            {
                if (npc.type == NPCID.SkeletronPrime || DestroyerIDs.Contains(npc.type))
                    return false;
            }

			if (npc.type == NPCID.GolemHeadFree)
			{
				// Draw the head as usual.
				Texture2D golemHeadTexture = Main.npcTexture[npc.type];
				Vector2 headDrawPosition = npc.Center + Vector2.UnitY * npc.gfxOffY - Main.screenPosition;
				spriteBatch.Draw(golemHeadTexture, headDrawPosition, npc.frame, npc.GetAlpha(drawColor), 0f, npc.frame.Size() * 0.5f, npc.scale, SpriteEffects.None, 0f);

				// Draw the eyes. The way vanilla handles this is hardcoded bullshit that cannot handle different hitboxes and thus requires rewriting.
				Color eyeColor = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, 0);
				Vector2 eyesDrawPosition = headDrawPosition - npc.scale * new Vector2(1f, 12f);
				Rectangle eyesFrame = new Rectangle(0, 0, Main.golemTexture[1].Width, Main.golemTexture[1].Height / 2);
				spriteBatch.Draw(Main.golemTexture[1], eyesDrawPosition, eyesFrame, eyeColor, 0f, eyesFrame.Size() * 0.5f, npc.scale, SpriteEffects.None, 0f);
				return false;
			}

            if (Main.LocalPlayer.Calamity().trippy)
            {
                SpriteEffects spriteEffects = SpriteEffects.None;
                if (npc.spriteDirection == 1)
                {
                    spriteEffects = SpriteEffects.FlipHorizontally;
                }

                float num66 = 0f;
                Vector2 vector11 = new Vector2(Main.npcTexture[npc.type].Width / 2, Main.npcTexture[npc.type].Height / Main.npcFrameCount[npc.type] / 2);
                Color color9 = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB, 0);
                Color alpha15 = npc.GetAlpha(color9);
                float num212 = 0.99f;
                alpha15.R = (byte)(alpha15.R * num212);
                alpha15.G = (byte)(alpha15.G * num212);
                alpha15.B = (byte)(alpha15.B * num212);
                alpha15.A = (byte)(alpha15.A * num212);
                for (int num213 = 0; num213 < 4; num213++)
                {
                    Vector2 position9 = npc.position;
                    float num214 = Math.Abs(npc.Center.X - Main.LocalPlayer.Center.X);
                    float num215 = Math.Abs(npc.Center.Y - Main.LocalPlayer.Center.Y);

                    if (num213 == 0 || num213 == 2)
                    {
                        position9.X = Main.LocalPlayer.Center.X + num214;
                    }
                    else
                    {
                        position9.X = Main.LocalPlayer.Center.X - num214;
                    }

                    position9.X -= npc.width / 2;

                    if (num213 == 0 || num213 == 1)
                    {
                        position9.Y = Main.LocalPlayer.Center.Y + num215;
                    }
                    else
                    {
                        position9.Y = Main.LocalPlayer.Center.Y - num215;
                    }

                    position9.Y -= npc.height / 2;

                    Main.spriteBatch.Draw(Main.npcTexture[npc.type], new Vector2(position9.X - Main.screenPosition.X + npc.width / 2 - Main.npcTexture[npc.type].Width * npc.scale / 2f + vector11.X * npc.scale, position9.Y - Main.screenPosition.Y + npc.height - Main.npcTexture[npc.type].Height * npc.scale / Main.npcFrameCount[npc.type] + 4f + vector11.Y * npc.scale + num66 + npc.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(npc.frame), alpha15, npc.rotation, vector11, npc.scale, spriteEffects, 0f);
                }
            }
			else
			{
				if (npc.Calamity().vulnerabilityHex > 0)
				{
					float compactness = npc.width * 0.6f;
					if (compactness < 10f)
						compactness = 10f;
					float power = npc.height / 100f;
					if (power > 2.75f)
						power = 2.75f;
					if (VulnerabilityHexFireDrawer is null || VulnerabilityHexFireDrawer.LocalTimer >= VulnerabilityHexFireDrawer.SetLifetime)
						VulnerabilityHexFireDrawer = new FireParticleSet(npc.Calamity().vulnerabilityHex, 1, Color.Red * 1.25f, Color.Red, compactness, power);
					else
						VulnerabilityHexFireDrawer.DrawSet(npc.Bottom - Vector2.UnitY * (12f - npc.gfxOffY));
				}
				else
					VulnerabilityHexFireDrawer = null;
			}

            // Draw a pillar of light and fade the background as an animation when skipping things in the DD2 event.
            if (npc.type == NPCID.DD2EterniaCrystal)
            {
                float animationTime = 120f - npc.ai[3];
                animationTime /= 120f;

                if (!Main.dedServ)
                {
                    if (!Filters.Scene["CrystalDestructionColor"].IsActive())
                        Filters.Scene.Activate("CrystalDestructionColor");

                    Filters.Scene["CrystalDestructionColor"].GetShader().UseIntensity((float)Math.Sin(animationTime * MathHelper.Pi) * 0.4f);
                }

                Vector2 drawPosition = npc.Center - Main.screenPosition + Vector2.UnitY * 60f;
                for (int i = 0; i < 4; i++)
                {
                    float intensity = MathHelper.Clamp(animationTime * 2f - i / 3f, 0f, 1f);
                    Vector2 origin = new Vector2(Main.magicPixel.Width / 2f, Main.magicPixel.Height);
                    Vector2 scale = new Vector2((float)Math.Sqrt(intensity) * 50f, intensity * 4f);
                    Color beamColor = new Color(0.4f, 0.17f, 0.4f, 0f) * (intensity * (1f - MathHelper.Clamp((animationTime - 0.8f) / 0.2f, 0f, 1f))) * 0.5f;
                    spriteBatch.Draw(Main.magicPixel, drawPosition, null, beamColor, 0f, origin, scale, SpriteEffects.None, 0f);
                }
            }
            return true;
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor)
		{
			if (CalamityWorld.revenge || BossRushEvent.BossRushActive || CalamityWorld.malice)
            {
				// His afterimages I can't get to work, so fuck it
				if (npc.type == NPCID.SkeletronPrime)
                {
                    Texture2D npcTexture = Main.npcTexture[npc.type];
                    int frameHeight = npcTexture.Height / Main.npcFrameCount[npc.type];

                    npc.frame.Y = (int)newAI[3];

                    // Floating phase
                    if (npc.ai[1] == 0f || npc.ai[1] == 4f)
                    {
                        newAI[2] += 1f;
                        if (newAI[2] >= 12f)
                        {
                            newAI[2] = 0f;
                            newAI[3] = newAI[3] + frameHeight;

                            if (newAI[3] / frameHeight >= 2f)
                                newAI[3] = 0f;
                        }
                    }

                    // Spinning probe spawn or fly over phase
                    else if (npc.ai[1] == 5f || npc.ai[1] == 6f)
                    {
                        newAI[2] = 0f;
                        newAI[3] = frameHeight;
                    }

                    // Spinning phase
                    else
                    {
                        newAI[2] = 0f;
                        newAI[3] = frameHeight * 2;
                    }

                    npc.frame.Y = (int)newAI[3];

                    SpriteEffects spriteEffects = SpriteEffects.None;
                    if (npc.spriteDirection == 1)
                        spriteEffects = SpriteEffects.FlipHorizontally;

                    spriteBatch.Draw(npcTexture, npc.Center - Main.screenPosition + new Vector2(0, npc.gfxOffY), npc.frame, npc.GetAlpha(drawColor), npc.rotation, npc.frame.Size() / 2, npc.scale, spriteEffects, 0f);

                    spriteBatch.Draw(Main.BoneEyesTexture, npc.Center - Main.screenPosition + new Vector2(0, npc.gfxOffY),
                        npc.frame, new Color(200, 200, 200, 0), npc.rotation, npc.frame.Size() / 2, npc.scale, spriteEffects, 0f);
                }
				else if (DestroyerIDs.Contains(npc.type))
				{
					if (drawColor != Color.Black)
					{
						Texture2D npcTexture = Main.npcTexture[npc.type];
						int frameHeight = npcTexture.Height / Main.npcFrameCount[npc.type];

						Vector2 halfSize = npc.frame.Size() / 2;
						SpriteEffects spriteEffects = SpriteEffects.None;
						if (npc.spriteDirection == 1)
							spriteEffects = SpriteEffects.FlipHorizontally;

						if (npc.type == NPCID.TheDestroyerBody)
						{
							if (npc.ai[2] == 0f)
								npc.frame.Y = 0;
							else
								npc.frame.Y = frameHeight;
						}

						// Draw segments
						spriteBatch.Draw(npcTexture, npc.Center - Main.screenPosition + new Vector2(0, npc.gfxOffY),
							npc.frame, npc.GetAlpha(drawColor), npc.rotation, halfSize, npc.scale, spriteEffects, 0f);

						// Draw lights
						if (npc.ai[2] == 0f)
						{
							if ((newAI[3] >= 900f && npc.life / (float)npc.lifeMax < 0.5f) || (newAI[1] < 600f && newAI[1] > 60f))
							{
								spriteBatch.Draw(Main.destTexture[npc.type - NPCID.TheDestroyer], npc.Center - Main.screenPosition + new Vector2(0, npc.gfxOffY), npc.frame,
									new Color(50, 50, 255, 0) * (1f - npc.alpha / 255f), npc.rotation, halfSize, npc.scale, spriteEffects, 0f);
							}
							else
							{
								spriteBatch.Draw(Main.destTexture[npc.type - NPCID.TheDestroyer], npc.Center - Main.screenPosition + new Vector2(0, npc.gfxOffY), npc.frame,
									new Color(255, 255, 255, 0) * (1f - npc.alpha / 255f), npc.rotation, halfSize, npc.scale, spriteEffects, 0f);
							}
						}
					}
				}
            }
		}

		public override bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position)
		{
			if (CalamityWorld.death)
			{
				switch (npc.type)
				{
					case NPCID.DiggerHead:
					case NPCID.DiggerBody:
					case NPCID.DiggerTail:
					case NPCID.SeekerHead:
					case NPCID.SeekerBody:
					case NPCID.SeekerTail:
					case NPCID.DuneSplicerHead:
					case NPCID.DuneSplicerBody:
					case NPCID.DuneSplicerTail:
						return true;
					default:
						break;
				}
			}
			return null;
		}

		public static Color buffColor(Color newColor, float R, float G, float B, float A)
		{
			newColor.R = (byte)((float)newColor.R * R);
			newColor.G = (byte)((float)newColor.G * G);
			newColor.B = (byte)((float)newColor.B * B);
			newColor.A = (byte)((float)newColor.A * A);
			return newColor;
		}
		#endregion

		#region Get Chat
		public override void OnChatButtonClicked(NPC npc, bool firstButton)
		{
			CalamityGlobalTownNPC.DisableAlert(npc, mod);
		}

		public override void GetChat(NPC npc, ref string chat)
        {
			CalamityGlobalTownNPC.NewNPCQuotes(npc, mod, ref chat);
        }
		#endregion

		#region Buff Town NPC
		public override void BuffTownNPC(ref float damageMult, ref int defense)
		{
			CalamityGlobalTownNPC.NPCStatBuffs(mod, ref damageMult, ref defense);
		}
		#endregion

		#region Shop Stuff
		public override void SetupShop(int type, Chest shop, ref int nextSlot)
        {
			CalamityGlobalTownNPC.ShopSetup(type, mod, ref shop, ref nextSlot);
        }

        public override void SetupTravelShop(int[] shop, ref int nextSlot)
        {
			CalamityGlobalTownNPC.TravelingMerchantShop(mod, ref shop, ref nextSlot);
        }
		#endregion

		#region Any Events
		public static bool AnyEvents(Player player)
		{
			if (Main.invasionType > InvasionID.None)
				return true;
			if (player.PillarZone())
				return true;
			if (DD2Event.Ongoing)
				return true;
			if ((player.ZoneOverworldHeight || player.ZoneSkyHeight) && (Main.eclipse || Main.bloodMoon || Main.pumpkinMoon || Main.snowMoon))
				return true;
			if (CalamityWorld.rainingAcid && player.InSulphur())
				return true;
			return false;
		}
		#endregion

		#region Get Downed Boss Variable
		public static bool GetDownedBossVariable(int type)
		{
			switch (type)
			{
				case NPCID.KingSlime:
					return NPC.downedSlimeKing;

				case NPCID.EyeofCthulhu:
					return NPC.downedBoss1;

				case NPCID.EaterofWorldsHead:
				case NPCID.EaterofWorldsBody:
				case NPCID.EaterofWorldsTail:
				case NPCID.BrainofCthulhu:
				case NPCID.Creeper:
					return NPC.downedBoss2;

				case NPCID.QueenBee:
					return NPC.downedQueenBee;

				case NPCID.SkeletronHead:
					return NPC.downedBoss3;

				case NPCID.WallofFlesh:
				case NPCID.WallofFleshEye:
					return Main.hardMode;

				case NPCID.TheDestroyer:
				case NPCID.TheDestroyerBody:
				case NPCID.TheDestroyerTail:
					return NPC.downedMechBoss1;

				case NPCID.Spazmatism:
				case NPCID.Retinazer:
					return NPC.downedMechBoss2;

				case NPCID.SkeletronPrime:
					return NPC.downedMechBoss3;

				case NPCID.Plantera:
					return NPC.downedPlantBoss;

				case NPCID.Golem:
				case NPCID.GolemHead:
					return NPC.downedGolemBoss;

				case NPCID.DukeFishron:
					return NPC.downedFishron;

				case NPCID.CultistBoss:
					return NPC.downedAncientCultist;

				case NPCID.MoonLordCore:
				case NPCID.MoonLordHand:
				case NPCID.MoonLordHead:
					return NPC.downedMoonlord;
			}

			if (type == NPCType<DesertScourgeHead>() || type == NPCType<DesertScourgeBody>() || type == NPCType<DesertScourgeTail>())
			{
				return CalamityWorld.downedDesertScourge;
			}
			else if (type == NPCType<CrabulonIdle>())
			{
				return CalamityWorld.downedCrabulon;
			}
			else if (type == NPCType<HiveMind.HiveMind>())
			{
				return CalamityWorld.downedHiveMind;
			}
			else if (type == NPCType<PerforatorHive>())
			{
				return CalamityWorld.downedPerforator;
			}
			else if (type == NPCType<SlimeGodCore>() || type == NPCType<SlimeGod.SlimeGod>() || type == NPCType<SlimeGodRun>() || type == NPCType<SlimeGodSplit>() || type == NPCType<SlimeGodRunSplit>())
			{
				return CalamityWorld.downedSlimeGod;
			}
			else if (type == NPCType<Cryogen.Cryogen>())
			{
				return CalamityWorld.downedCryogen;
			}
			else if (type == NPCType<AquaticScourgeHead>() || type == NPCType<AquaticScourgeBody>() || type == NPCType<AquaticScourgeBodyAlt>() || type == NPCType<AquaticScourgeTail>())
			{
				return CalamityWorld.downedAquaticScourge;
			}
			else if (type == NPCType<BrimstoneElemental.BrimstoneElemental>())
			{
				return CalamityWorld.downedBrimstoneElemental;
			}
			else if (type == NPCType<CalamitasRun3>())
			{
				return CalamityWorld.downedCalamitas;
			}
			else if (type == NPCType<Leviathan.Leviathan>() || type == NPCType<Siren>())
			{
				return CalamityWorld.downedLeviathan;
			}
			else if (type == NPCType<AstrumAureus.AstrumAureus>())
			{
				return CalamityWorld.downedAstrageldon;
			}
			else if (type == NPCType<AstrumDeusHeadSpectral>() || type == NPCType<AstrumDeusBodySpectral>() || type == NPCType<AstrumDeusTailSpectral>())
			{
				return CalamityWorld.downedStarGod;
			}
			else if (type == NPCType<PlaguebringerGoliath.PlaguebringerGoliath>())
			{
				return CalamityWorld.downedPlaguebringer;
			}
			else if (type == NPCType<RavagerBody>())
			{
				return CalamityWorld.downedScavenger;
			}
			else if (type == NPCType<ProfanedGuardianBoss>())
			{
				return CalamityWorld.downedGuardians;
			}
			else if (type == NPCType<Bumblefuck>())
			{
				return CalamityWorld.downedBumble;
			}
			else if (type == NPCType<Providence.Providence>())
			{
				return CalamityWorld.downedProvidence;
			}
			else if (type == NPCType<CeaselessVoid.CeaselessVoid>() || type == NPCType<DarkEnergy>())
			{
				return CalamityWorld.downedSentinel1;
			}
			else if (type == NPCType<StormWeaverHead>() || type == NPCType<StormWeaverBody>() || type == NPCType<StormWeaverTail>())
			{
				return CalamityWorld.downedSentinel2;
			}
			else if (type == NPCType<Signus.Signus>())
			{
				return CalamityWorld.downedSentinel3;
			}
			else if (type == NPCType<Polterghast.Polterghast>())
			{
				return CalamityWorld.downedPolterghast;
			}
			else if (type == NPCType<OldDuke.OldDuke>())
			{
				return CalamityWorld.downedBoomerDuke;
			}
			else if (type == NPCType<DevourerofGodsHead>() || type == NPCType<DevourerofGodsBody>() || type == NPCType<DevourerofGodsTail>())
			{
				return CalamityWorld.downedDoG;
			}
			else if (type == NPCType<Yharon.Yharon>())
			{
				return CalamityWorld.downedYharon;
			}
			else if (type == NPCType<Artemis>() || type == NPCType<Apollo>() || type == NPCType<AresBody>() || type == NPCType<AresGaussNuke>() || type == NPCType<AresLaserCannon>() || type == NPCType<AresPlasmaFlamethrower>() || type == NPCType<AresTeslaCannon>() || type == NPCType<ThanatosHead>() || type == NPCType<ThanatosBody1>() || type == NPCType<ThanatosBody2>() || type == NPCType<ThanatosTail>())
			{
				return CalamityWorld.downedExoMechs;
			}
			else if (type == NPCType<SupremeCalamitas.SupremeCalamitas>())
			{
				return CalamityWorld.downedSCal;
			}
			else if (type == NPCType<EidolonWyrmHeadHuge>())
			{
				return CalamityWorld.downedAdultEidolonWyrm;
			}

			return true;
		}
		#endregion

		#region Speedrun Display
		public static void SetNewBossJustDowned(NPC npc)
		{
			if (!GetDownedBossVariable(npc.type))
			{
				CalamityLists.bossTypes.TryGetValue(npc.type, out int newBossTypeJustDowned);

				for (int i = 0; i < Main.maxPlayers; i++)
				{
					Player player = Main.player[i];
					if (!player.active)
						continue;

					CalamityPlayer mp = player.Calamity();
					mp.lastSplitType = newBossTypeJustDowned;
					mp.lastSplit = mp.previousSessionTotal.Add(CalamityMod.SpeedrunTimer.Elapsed);
				}
			}
		}
		#endregion

		#region Player Counts
		public static bool AnyLivingPlayers()
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i] != null && Main.player[i].active && !Main.player[i].dead && !Main.player[i].ghost)
                {
                    return true;
                }
            }
            return false;
        }

		public static int GetActivePlayerCount()
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				return 1;
			}

			int players = 0;
			for (int i = 0; i < Main.maxPlayers; i++)
			{
				if (Main.player[i] != null && Main.player[i].active)
				{
					players++;
				}
			}
			return players;
		}
		#endregion

		#region Should Affect NPC
		public static bool ShouldAffectNPC(NPC target)
        {
			if (EaterofWorldsIDs.Contains(target.type) || DestroyerIDs.Contains(target.type))
				return false;

            if (target.damage > 0 && !target.boss && !target.friendly && !target.dontTakeDamage && target.type != NPCID.Creeper && target.type != NPCType<RavagerClawLeft>() &&
                target.type != NPCID.MourningWood && target.type != NPCID.Everscream && target.type != NPCID.SantaNK1 && target.type != NPCType<RavagerClawRight>() &&
				target.type != NPCType<Reaper>() && target.type != NPCType<Mauler>() && target.type != NPCType<EidolonWyrmHead>() && target.type != NPCID.GolemFistLeft && target.type != NPCID.GolemFistRight &&
                target.type != NPCType<EidolonWyrmHeadHuge>() && target.type != NPCType<ColossalSquid>() && target.type != NPCID.DD2Betsy && !CalamityLists.enemyImmunityList.Contains(target.type) && !AcidRainEvent.AllMinibosses.Contains(target.type))
            {
                return true;
            }
            return false;
        }
        #endregion

        #region Old Duke Spawn
        public static void OldDukeSpawn(int plr, int type, int baitType)
        {
            Player player = Main.player[plr];

            if (!player.active || player.dead)
            {
                return;
            }

            int m = 0;
            while (m < Main.maxProjectiles)
            {
                Projectile projectile = Main.projectile[m];
                if (projectile.active && projectile.bobber && projectile.owner == plr)
                {
					if (plr == Main.myPlayer && projectile.ai[0] == 0f)
					{
						for (int item = 0; item < Main.maxInventory; item++)
						{
							if (player.inventory[item].type == baitType)
							{
								player.inventory[item].stack--;
								if (player.inventory[item].stack <= 0)
								{
									player.inventory[item].SetDefaults(0, false);
								}
								break;
							}
						}

						projectile.ai[0] = 2f;
						projectile.netUpdate = true;

                        // The vanilla game uses a special packet for Duke Fishron spawning.
                        // However, this packet doesn't work on modded NPC types, so we must create
                        // a custom one.
                        // Also, you can't use Netmode != NetmodeID.MultiplayerClient in a projectile context that
                        // has an owner, hence the MyPlayer check.
                        if (Main.myPlayer == projectile.owner)
                        {
                            if (Main.netMode == NetmodeID.SinglePlayer)
                            {
                                if (!player.active || player.dead)
                                    return;

                                Projectile proj = null;
                                for (int i = 0; i < Main.maxProjectiles; i++)
                                {
                                    proj = Main.projectile[i];
                                    if (Main.projectile[i].active && Main.projectile[i].bobber && Main.projectile[i].owner == player.whoAmI)
                                    {
                                        proj = Main.projectile[i];
                                        break;
                                    }
                                }

                                if (proj is null)
                                    return;

                                int oldDuke = NPC.NewNPC((int)proj.Center.X, (int)proj.Center.Y + 100, NPCType<OldDuke.OldDuke>());
                                CalamityUtils.BossAwakenMessage(oldDuke);
                            }
							else
                            {
                                var netMessage = CalamityMod.Instance.GetPacket();
                                netMessage.Write((byte)CalamityModMessageType.ServersideSpawnOldDuke);
                                netMessage.Write((byte)player.whoAmI);
                                netMessage.Send();
                            }
                        }
                    }

                    break;
                }
                else
                {
                    m++;
                }
            }
        }
        #endregion

        #region Astral things
        public static void DoHitDust(NPC npc, int hitDirection, int dustType = 5, float xSpeedMult = 1f, int numHitDust = 5, int numDeathDust = 20)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(npc.position, npc.width, npc.height, dustType, hitDirection * xSpeedMult, -1f);
            }

            if (npc.life <= 0)
            {
                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDust(npc.position, npc.width, npc.height, dustType, hitDirection * xSpeedMult, -1f);
                }
            }
        }

        public static void DoFlyingAI(NPC npc, float maxSpeed, float acceleration, float circleTime, float minDistanceTarget = 150f, bool shouldAttackTarget = true)
        {
            //Pick a new target.
            if (npc.target < 0 || npc.target >= Main.maxPlayers || Main.player[npc.target].dead)
            {
                npc.TargetClosest(true);
            }

            Player myTarget = Main.player[npc.target];
            Vector2 toTarget = myTarget.Center - npc.Center;
            float distanceToTarget = toTarget.Length();
            Vector2 maxVelocity = toTarget;

            if (distanceToTarget < 3f)
            {
                maxVelocity = npc.velocity;
            }
            else
            {
                float magnitude = maxSpeed / distanceToTarget;
                maxVelocity *= magnitude;
            }

            //Circular motion
            npc.ai[0]++;

            //y motion
            if (npc.ai[0] > circleTime * 0.5f)
            {
                npc.velocity.Y += acceleration;
            }
            else
            {
                npc.velocity.Y -= acceleration;
            }

            //x motion
            if (npc.ai[0] < circleTime * 0.25f || npc.ai[0] > circleTime * 0.75f)
            {
                npc.velocity.X += acceleration;
            }
            else
            {
                npc.velocity.X -= acceleration;
            }

            //reset
            if (npc.ai[0] > circleTime)
            {
                npc.ai[0] = 0f;
            }

            //if close enough
            if (shouldAttackTarget && distanceToTarget < minDistanceTarget)
            {
                npc.velocity += maxVelocity * 0.007f;
            }

            if (myTarget.dead)
            {
                maxVelocity.X = npc.direction * maxSpeed / 2f;
                maxVelocity.Y = -maxSpeed / 2f;
            }

            //maximise velocity
            if (npc.velocity.X < maxVelocity.X)
            {
                npc.velocity.X += acceleration;
            }

            if (npc.velocity.X > maxVelocity.X)
            {
                npc.velocity.X -= acceleration;
            }

            if (npc.velocity.Y < maxVelocity.Y)
            {
                npc.velocity.Y += acceleration;
            }

            if (npc.velocity.Y > maxVelocity.Y)
            {
                npc.velocity.Y -= acceleration;
            }

            //rotate towards player if alive
            if (!myTarget.dead)
            {
                npc.rotation = toTarget.ToRotation();
            }
            else //don't, do velocity instead
            {
                npc.rotation = npc.velocity.ToRotation();
            }

            npc.rotation += MathHelper.Pi;

            //tile collision
            float collisionDamp = 0.7f;
            if (npc.collideX)
            {
                npc.netUpdate = true;
                npc.velocity.X = npc.oldVelocity.X * -collisionDamp;

                if (npc.direction == -1 && npc.velocity.X > 0f && npc.velocity.X < 2f)
                {
                    npc.velocity.X = 2f;
                }

                if (npc.direction == 1 && npc.velocity.X < 0f && npc.velocity.X > -2f)
                {
                    npc.velocity.X = -2f;
                }
            }
            if (npc.collideY)
            {
                npc.netUpdate = true;
                npc.velocity.Y = npc.oldVelocity.Y * -collisionDamp;

                if (npc.velocity.Y > 0f && npc.velocity.Y < 1.5f)
                {
                    npc.velocity.Y = 1.5f;
                }

                if (npc.velocity.Y < 0f && npc.velocity.Y > -1.5f)
                {
                    npc.velocity.Y = -1.5f;
                }
            }

            //water collision
            if (npc.wet)
            {
                if (npc.velocity.Y > 0f)
                {
                    npc.velocity.Y *= 0.95f;
                }

                npc.velocity.Y -= 0.3f;

                if (npc.velocity.Y < -2f)
                {
                    npc.velocity.Y = -2f;
                }
            }

            //Taken from source. Important for net?
            if (((npc.velocity.X > 0f && npc.oldVelocity.X < 0f) || (npc.velocity.X < 0f && npc.oldVelocity.X > 0f) || (npc.velocity.Y > 0f && npc.oldVelocity.Y < 0f) || (npc.velocity.Y < 0f && npc.oldVelocity.Y > 0f)) && !npc.justHit)
            {
                npc.netUpdate = true;
            }
        }

        public static void DoSpiderWallAI(NPC npc, int transformType, float chaseMaxSpeed = 2f, float chaseAcceleration = 0.08f)
        {
            //GET NEW TARGET
            if (npc.target < 0 || npc.target == 255 || Main.player[npc.target].dead)
            {
                npc.TargetClosest();
            }

            Vector2 between = Main.player[npc.target].Center - npc.Center;
            float distance = between.Length();

            //modify vector depending on distance and speed.
            if (distance == 0f)
            {
                between.X = npc.velocity.X;
                between.Y = npc.velocity.Y;
            }
            else
            {
                distance = chaseMaxSpeed / distance;
                between.X *= distance;
                between.Y *= distance;
            }

            //update if target dead.
            if (Main.player[npc.target].dead)
            {
                between.X = npc.direction * chaseMaxSpeed / 2f;
                between.Y = -chaseMaxSpeed / 2f;
            }
            npc.spriteDirection = -1;

            //If spider can't see target, circle around to attempt to find the target.
            if (!Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
            {
                //CIRCULAR MOTION, SIMILAR TO FLYING AI (Eater of Souls etc.)
                npc.ai[0]++;

                if (npc.ai[0] > 0f)
                {
                    npc.velocity.Y += 0.023f;
                }
                else
                {
                    npc.velocity.Y -= 0.023f;
                }

                if (npc.ai[0] < -100f || npc.ai[0] > 100f)
                {
                    npc.velocity.X += 0.023f;
                }
                else
                {
                    npc.velocity.X -= 0.023f;
                }

                if (npc.ai[0] > 200f)
                {
                    npc.ai[0] = -200f;
                }

                npc.velocity.X += between.X * 0.007f;
                npc.velocity.Y += between.Y * 0.007f;
                npc.rotation = npc.velocity.ToRotation();

                if (npc.velocity.X > 1.5f)
                {
                    npc.velocity.X *= 0.9f;
                }

                if (npc.velocity.X < -1.5f)
                {
                    npc.velocity.X *= 0.9f;
                }

                if (npc.velocity.Y > 1.5f)
                {
                    npc.velocity.Y *= 0.9f;
                }

                if (npc.velocity.Y < -1.5f)
                {
                    npc.velocity.Y *= 0.9f;
                }

                npc.velocity.X = MathHelper.Clamp(npc.velocity.X, -3f, 3f);
                npc.velocity.Y = MathHelper.Clamp(npc.velocity.Y, -3f, 3f);
            }
            else //CHASE TARGET
            {
                if (npc.velocity.X < between.X)
                {
                    npc.velocity.X += chaseAcceleration;

                    if (npc.velocity.X < 0f && between.X > 0f)
                    {
                        npc.velocity.X += chaseAcceleration;
                    }
                }
                else if (npc.velocity.X > between.X)
                {
                    npc.velocity.X -= chaseAcceleration;

                    if (npc.velocity.X > 0f && between.X < 0f)
                    {
                        npc.velocity.X -= chaseAcceleration;
                    }
                }
                if (npc.velocity.Y < between.Y)
                {
                    npc.velocity.Y += chaseAcceleration;

                    if (npc.velocity.Y < 0f && between.Y > 0f)
                    {
                        npc.velocity.Y += chaseAcceleration;
                    }
                }
                else if (npc.velocity.Y > between.Y)
                {
                    npc.velocity.Y -= chaseAcceleration;

                    if (npc.velocity.Y > 0f && between.Y < 0f)
                    {
                        npc.velocity.Y -= chaseAcceleration;
                    }
                }
                npc.rotation = between.ToRotation();
            }

            //DAMP COLLISIONS OFF OF WALLS
            float collisionDamp = 0.5f;
            if (npc.collideX)
            {
                npc.netUpdate = true;
                npc.velocity.X = npc.oldVelocity.X * -collisionDamp;

                if (npc.direction == -1 && npc.velocity.X > 0f && npc.velocity.X < 2f)
                {
                    npc.velocity.X = 2f;
                }

                if (npc.direction == 1 && npc.velocity.X < 0f && npc.velocity.X > -2f)
                {
                    npc.velocity.X = -2f;
                }
            }
            if (npc.collideY)
            {
                npc.netUpdate = true;
                npc.velocity.Y = npc.oldVelocity.Y * -collisionDamp;

                if (npc.velocity.Y > 0f && npc.velocity.Y < 1.5f)
                {
                    npc.velocity.Y = 2f;
                }

                if (npc.velocity.Y < 0f && npc.velocity.Y > -1.5f)
                {
                    npc.velocity.Y = -2f;
                }
            }

            if (((npc.velocity.X > 0f && npc.oldVelocity.X < 0f) || (npc.velocity.X < 0f && npc.oldVelocity.X > 0f) || (npc.velocity.Y > 0f && npc.oldVelocity.Y < 0f) || (npc.velocity.Y < 0f && npc.oldVelocity.Y > 0f)) && !npc.justHit)
            {
                npc.netUpdate = true;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int x = (int)npc.Center.X / 16;
                int y = (int)npc.Center.Y / 16;
                bool flag = false;

                for (int i = x - 1; i <= x + 1; i++)
                {
                    for (int j = y - 1; j <= y + 1; j++)
                    {
                        if (Main.tile[i, j] is null)
                        {
                            return;
                        }

                        if (Main.tile[i, j].wall > 0)
                        {
                            flag = true;
                        }
                    }
                }
                if (!flag)
                {
                    npc.Transform(transformType);
                    return;
                }
            }
        }

        public static void DoVultureAI(NPC npc, float acceleration = 0.1f, float maxSpeed = 3f, int sitWidth = 30, int flyWidth = 50, int rangeX = 100, int rangeY = 100)
        {
            npc.localAI[0]++;
            npc.noGravity = true;
            npc.TargetClosest(true);

            if (npc.ai[0] == 0f)
            {
                npc.width = sitWidth;
                npc.noGravity = false;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (npc.velocity.X != 0f || npc.velocity.Y < 0f || npc.velocity.Y > 0.3)
                    {
                        npc.ai[0] = 1f;
                        npc.netUpdate = true;
                    }
                    else
                    {
                        Rectangle playerRect = Main.player[npc.target].getRect();
                        Rectangle rangeRect = new Rectangle((int)npc.Center.X - rangeX, (int)npc.Center.Y - rangeY, rangeX * 2, rangeY * 2);
                        if (npc.localAI[0] > 20f && (rangeRect.Intersects(playerRect) || npc.life < npc.lifeMax))
                        {
                            npc.ai[0] = 1f;
                            npc.velocity.Y -= 6f;
                            npc.netUpdate = true;
                        }
                    }
                }
            }
            else if (!Main.player[npc.target].dead)
            {
                npc.width = flyWidth;

                //Collision damping
                if (npc.collideX)
                {
                    npc.velocity.X = npc.oldVelocity.X * -0.5f;

                    if (npc.direction == -1 && npc.velocity.X > 0f && npc.velocity.X < 2f)
                    {
                        npc.velocity.X = 2f;
                    }

                    if (npc.direction == 1 && npc.velocity.X < 0f && npc.velocity.X > -2f)
                    {
                        npc.velocity.X = -2f;
                    }
                }

                if (npc.collideY)
                {
                    npc.velocity.Y = npc.oldVelocity.Y * -0.5f;

                    if (npc.velocity.Y > 0f && npc.velocity.Y < 1f)
                    {
                        npc.velocity.Y = 1f;
                    }

                    if (npc.velocity.Y < 0f && npc.velocity.Y > -1f)
                    {
                        npc.velocity.Y = -1f;
                    }
                }

                if (npc.direction == -1 && npc.velocity.X > -maxSpeed)
                {
                    npc.velocity.X -= acceleration;

                    if (npc.velocity.X > maxSpeed)
                    {
                        npc.velocity.X -= acceleration;
                    }
                    else if (npc.velocity.X > 0f)
                    {
                        npc.velocity.X -= acceleration * 0.5f;
                    }

                    if (npc.velocity.X < -maxSpeed)
                    {
                        npc.velocity.X = -maxSpeed;
                    }
                }
                else if (npc.direction == 1 && npc.velocity.X < maxSpeed)
                {
                    npc.velocity.X += acceleration;

                    if (npc.velocity.X < -maxSpeed)
                    {
                        npc.velocity.X += acceleration;
                    }
                    else if (npc.velocity.X < 0f)
                    {
                        npc.velocity.X += acceleration * 0.5f;
                    }

                    if (npc.velocity.X > maxSpeed)
                    {
                        npc.velocity.X = maxSpeed;
                    }
                }

                float xDistance = Math.Abs(npc.Center.X - Main.player[npc.target].Center.X);
                float yLimiter = Main.player[npc.target].position.Y - (npc.height / 2f);
                if (xDistance > 50f)
                {
                    yLimiter -= 100f;
                }

                if (npc.position.Y < yLimiter)
                {
                    npc.velocity.Y += acceleration * 0.5f;

                    if (npc.velocity.Y < 0f)
                    {
                        npc.velocity.Y += acceleration * 0.1f;
                    }
                }
                else
                {
                    npc.velocity.Y -= acceleration * 0.5f;

                    if (npc.velocity.Y > 0f)
                    {
                        npc.velocity.Y -= acceleration * 0.1f;
                    }
                }

                if (npc.velocity.Y < -maxSpeed)
                {
                    npc.velocity.Y = -maxSpeed;
                }

                if (npc.velocity.Y > maxSpeed)
                {
                    npc.velocity.Y = maxSpeed;
                }
            }
            //Change velocity if wet.
            if (npc.wet)
            {
                if (npc.velocity.Y > 0f)
                {
                    npc.velocity.Y *= 0.95f;
                }

                npc.velocity.Y -= 0.5f;

                if (npc.velocity.Y < -4f)
                {
                    npc.velocity.Y = -4f;
                }
            }
        }

        /// <summary>
        /// Allows you to spawn dust on the NPC in a certain place. Uses the npc.position value as the base point for the rectangle.
        /// Takes direction and rotation into account.
        /// </summary>
        /// <param name="frameWidth">The width of the sheet for the NPC.</param>
        /// <param name="rect">The place to put a dust.</param>
        /// <param name="chance">The chance to spawn a dust (0.3 = 30%)</param>
        public static Dust SpawnDustOnNPC(NPC npc, int frameWidth, int frameHeight, int dustType, Rectangle rect, Vector2 velocity = default, float chance = 0.5f, bool useSpriteDirection = false)
        {
            Vector2 half = new Vector2(frameWidth / 2f, frameHeight / 2f);

            //"flip" the rectangle's position x-wise.
            if ((!useSpriteDirection && npc.direction == 1) || (useSpriteDirection && npc.spriteDirection == 1))
            {
                rect.X = frameWidth - rect.Right;
            }

            if (Main.rand.NextFloat(1f) < chance)
            {
                Vector2 offset = npc.Center - half + new Vector2(Main.rand.NextFloat(rect.Left, rect.Right), Main.rand.NextFloat(rect.Top, rect.Bottom)) - npc.Center;
                offset = offset.RotatedBy(npc.rotation);
                Dust d = Dust.NewDustPerfect(npc.Center + offset, dustType, velocity);
                return d;
            }
            return null;
        }
        #endregion
    }
}
