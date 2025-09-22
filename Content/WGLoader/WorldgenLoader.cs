// This so WIP , wont even ever make it into actual stable release

// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Reflection;
// using Gearedup.Helper;
// using Microsoft.Xna.Framework;
// using Mono.Cecil.Cil;
// using Terraria;
// using Terraria.GameContent.Generation;
// using Terraria.ID;
// using Terraria.ModLoader;
// using Terraria.Utilities;
// using Terraria.WorldBuilding;

// namespace Gearedup.Content.WGLoader
// {

// 	public class ModGenPass
// 	{
// 		string potentialSource;
// 		public Mod mod;
// 		public ModSystem system;
// 		public GenPass gen;

// 		// public GenPass vanillaGenAfter;
// 		// public GenPass vanillaGenBefore;

// 		public void Apply()
// 		{

// 		}

// 		public ModGenPass(Mod mod, ModSystem system, string name, WorldGenLegacyMethod genCall)
// 		{
// 			this.mod = mod;
// 			this.system = system;
// 			this.gen = new PassLegacy(name, genCall);
// 			potentialSource = "Autoload from Mods";
// 		}

// 		public ModGenPass(Mod mod, ModSystem system, GenPass genpass, string source = "")
// 		{
// 			this.mod = mod;
// 			this.system = system;
// 			this.gen = genpass;
// 			this.potentialSource = source;
//         }
//     }

// 	public class WorldgenLoader : ModSystem
// 	{

//         public override bool IsLoadingEnabled(Mod mod)
//         {
//             return GearServerConfig.Get.WGLoader;
//         }

// 		public static List<ModGenPass> harmodeGenPasses;
// 		public static List<ModGenPass> genPasses;
// 		public static List<ModGenPass> hookPasses;

//         /// <summary>
//         /// not really performant approach
//         /// </summary>
//         public static Tilemap cachedTiles;

//         public override void Load()
//         {
//         }

//         public static GenerationProgress progress;
// 		public WorldGenerator passGenerator;
        
//         public enum SpecialSeed
//         {
//             everythingWorldGen,
//             tenthAnniversaryWorldGen,
//             getGoodWorld,
//             notTheBees,
//             noTrapsWorldGen,
//             remixWorldGen,
//             drunkWorldGen,
//             dontStarveWorldGen,
//             Crimson
//         }

//         public void StartGen_Inner(List<ModGenPass> gens, int seed)
//         {
//             passGenerator.Append(gens[0].gen);
//             passGenerator.GenerateWorld();
//         }

//         public void StartGen(List<ModGenPass> gens, int seed, List<SpecialSeed> specialSeeds = null, bool skipReset = true)
//         {
//             // evil worldgen settings
//             WorldGen.WorldGenParam_Evil = WorldGen.crimson ? 1 : 0;
//             // configure special seeds
//             if (specialSeeds != null)
//             {
//                 WorldGen.remixWorldGen = specialSeeds.Contains(SpecialSeed.remixWorldGen) ? true : Main.remixWorld;
//                 WorldGen.tenthAnniversaryWorldGen = specialSeeds.Contains(SpecialSeed.tenthAnniversaryWorldGen) ? true : Main.tenthAnniversaryWorld;
//                 WorldGen.drunkWorldGen = specialSeeds.Contains(SpecialSeed.everythingWorldGen) ? true : Main.drunkWorld;
//                 WorldGen.getGoodWorldGen = specialSeeds.Contains(SpecialSeed.getGoodWorld) ? true : Main.getGoodWorld;
//                 WorldGen.notTheBees = specialSeeds.Contains(SpecialSeed.notTheBees) ? true : Main.notTheBeesWorld;
//                 WorldGen.noTrapsWorldGen = specialSeeds.Contains(SpecialSeed.noTrapsWorldGen) ? true : Main.noTrapsWorld;
//                 WorldGen.dontStarveWorldGen = specialSeeds.Contains(SpecialSeed.dontStarveWorldGen) ? true : Main.dontStarveWorld;
//                 WorldGen.everythingWorldGen = specialSeeds.Contains(SpecialSeed.everythingWorldGen) ? true : Main.zenithWorld;
//             }

//             Main.afterPartyOfDoom = false;

//             WorldGen._lastSeed = seed;
//             passGenerator = new WorldGenerator(seed, GenVars.configuration);
//             WorldGen._genRand = new UnifiedRandom(seed);
//             //Main.rand = new UnifiedRandom(seed);

//             if (skipReset)
//             {
//                 StartGen_Inner(gens, seed);
//                 return;
//             }

//             // consistent with vanilla naming
//             #region Boring Setup
//             // Utils.LogAndConsoleInfoMessageFormat("Creating world - Seed: {0}, Width: {1}, Height: {2}, Evil: {3}, IsExpert: {4}", seed, Main.maxTilesX, Main.maxTilesY, WorldGen.WorldGenParam_Evil, Main.expertMode);
//             // There is actually world gen config ??? wtf
//             GenVars.configuration = WorldGenConfiguration.FromEmbeddedPath("Terraria.GameContent.WorldBuilding.Configuration.json");
//             WorldGen.Hooks.ProcessWorldGenConfig(ref GenVars.configuration);

//             //WorldGen.Logging.Terraria.InfoFormat("Generating World: {0}", Main.ActiveWorldFileData.Name);

//             GenVars.structures = new StructureMap();
//             GenVars.desertHiveHigh = Main.maxTilesY;
//             GenVars.desertHiveLow = 0;
//             GenVars.desertHiveLeft = Main.maxTilesX;
//             GenVars.desertHiveRight = 0;
//             GenVars.worldSurfaceLow = 0.0;
//             GenVars.worldSurface = 0.0;
//             GenVars.worldSurfaceHigh = 0.0;
//             GenVars.rockLayerLow = 0.0;
//             GenVars.rockLayer = 0.0;
//             GenVars.rockLayerHigh = 0.0;
//             GenVars.copper = 7;
//             GenVars.iron = 6;
//             GenVars.silver = 9;
//             GenVars.gold = 8;
//             GenVars.dungeonSide = 0;
//             GenVars.jungleHut = (ushort)WorldGen.genRand.Next(5);
//             GenVars.shellStartXLeft = 0;
//             GenVars.shellStartYLeft = 0;
//             GenVars.shellStartXRight = 0;
//             GenVars.shellStartYRight = 0;
//             GenVars.PyrX = null;
//             GenVars.PyrY = null;
//             GenVars.numPyr = 0;
//             GenVars.jungleMinX = -1;
//             GenVars.jungleMaxX = -1;
//             GenVars.snowMinX = new int[Main.maxTilesY];
//             GenVars.snowMaxX = new int[Main.maxTilesY];
//             GenVars.snowTop = 0;
//             GenVars.snowBottom = 0;
//             GenVars.skyLakes = 1;

//             if (Main.maxTilesX > 8000) GenVars.skyLakes++;
//             if (Main.maxTilesX > 6000) GenVars.skyLakes++;

//             GenVars.beachBordersWidth = 275;
//             GenVars.beachSandRandomCenter = GenVars.beachBordersWidth + 5 + 40;
//             GenVars.beachSandRandomWidthRange = 20;
//             GenVars.beachSandDungeonExtraWidth = 40;
//             GenVars.beachSandJungleExtraWidth = 20;
//             GenVars.oceanWaterStartRandomMin = 220;
//             GenVars.oceanWaterStartRandomMax = GenVars.oceanWaterStartRandomMin + 40;
//             GenVars.oceanWaterForcedJungleLength = 275;
//             GenVars.leftBeachEnd = 0;
//             GenVars.rightBeachStart = 0;
//             GenVars.evilBiomeBeachAvoidance = GenVars.beachSandRandomCenter + 60;
//             GenVars.evilBiomeAvoidanceMidFixer = 50;
//             GenVars.lakesBeachAvoidance = GenVars.beachSandRandomCenter + 20;
//             GenVars.smallHolesBeachAvoidance = GenVars.beachSandRandomCenter + 20;
//             GenVars.surfaceCavesBeachAvoidance = GenVars.beachSandRandomCenter + 20;
//             GenVars.surfaceCavesBeachAvoidance2 = GenVars.beachSandRandomCenter + 20;
//             GenVars.jungleOriginX = 0;
//             GenVars.snowOriginLeft = 0;
//             GenVars.snowOriginRight = 0;
//             GenVars.logX = -1;
//             GenVars.logY = -1;
//             GenVars.dungeonLocation = 0;

//             if (WorldGen.genRand.Next(2) == 0) { GenVars.crimsonLeft = false; }
//             else { GenVars.crimsonLeft = true; }

//             GenVars.numOceanCaveTreasure = 0;
//             GenVars.skipDesertTileCheck = false;
//             // why is this private
//             typeof(WorldGen).GetField("growGrassUnderground").SetValue(null, false);
//             // WorldGen.growGrassUnderground = false;
//             // WorldGen.gen = true;
//             // Liquid.ReInit();
//             // WorldGen.noTileActions = true;
//             // need tinkering
//             progress.Message = "";
//             WorldGen.SetupStatueList();
//             //WorldGen.RandomizeWeather();
//             //Main.cloudAlpha = 0f;
//             //Main.maxRaining = 0f;
//             //Main.raining = false;
//             // need tinkering
//             //WorldGen.heartCount = 0;
//             typeof(WorldGen).GetField("heartCount").SetValue(null, 0);
//             GenVars.extraBastStatueCount = 0;
//             GenVars.extraBastStatueCountMax = 2;
//             Main.checkXMas();
//             Main.checkHalloween();
//             //WorldGen.ResetGenerator();

//             GenVars.numOrePatch = 0;
//             GenVars.numTunnels = 0;
//             GenVars.numLakes = 0;
//             GenVars.numMushroomBiomes = 0;
//             GenVars.numOceanCaveTreasure = 0;
//             GenVars.numOasis = 0;
//             GenVars.mudWall = false;
//             GenVars.hellChest = 0;
//             GenVars.JungleX = 0;
//             GenVars.numMCaves = 0;
//             GenVars.numIslandHouses = 0;
//             GenVars.skyIslandHouseCount = 0;
//             GenVars.dEnteranceX = 0;
//             GenVars.numDRooms = 0;
//             GenVars.numDDoors = 0;
//             GenVars.generatedShadowKey = false;
//             GenVars.numDungeonPlatforms = 0;
//             GenVars.numJChests = 0;
//             GenVars.JungleItemCount = 0;
//             GenVars.lastDungeonHall = ReLogic.Utilities.Vector2D.Zero;

//             GenVars.UndergroundDesertLocation = Rectangle.Empty;
//             GenVars.UndergroundDesertHiveLocation = Rectangle.Empty;
//             GenVars.numLarva = 0;
//             List<int> list3 = new List<int> {
//                 274,
//                 220,
//                 112,
//                 218,
//                 3019
//             };

//             // NEED TINKERING
//             if (WorldGen.remixWorldGen)
//             {
//                 list3 = new List<int> {
//                     274,
//                     220,
//                     683,
//                     218,
//                     3019
//                 };
//             }

//             List<int> list4 = new List<int>();
//             while (list3.Count > 0)
//             {
//                 int index = WorldGen.genRand.Next(list3.Count);
//                 int item = list3[index];
//                 list4.Add(item);
//                 list3.RemoveAt(index);
//             }

//             GenVars.hellChestItem = list4.ToArray();
//             int num1086 = 86400;
//             Main.slimeRainTime = -WorldGen.genRand.Next(num1086 * 2, num1086 * 3);
//             Main.cloudBGActive = -WorldGen.genRand.Next(8640, 86400);
//             // Need Tinkering
//             typeof(WorldGen).GetField("skipFramingDuringGen").SetValue(null, false);
//             //WorldGen.skipFramingDuringGen = false;
//             WorldGen.SavedOreTiers.Copper = 7;
//             WorldGen.SavedOreTiers.Iron = 6;
//             WorldGen.SavedOreTiers.Silver = 9;
//             WorldGen.SavedOreTiers.Gold = 8;
//             GenVars.copperBar = 20;
//             GenVars.ironBar = 22;
//             GenVars.silverBar = 21;
//             GenVars.goldBar = 19;
//             if (WorldGen.genRand.Next(2) == 0)
//             {
//                 GenVars.copper = 166;
//                 GenVars.copperBar = 703;
//                 WorldGen.SavedOreTiers.Copper = 166;
//             }

//             if ((!WorldGen.dontStarveWorldGen || WorldGen.drunkWorldGen) && WorldGen.genRand.Next(2) == 0)
//             {
//                 GenVars.iron = 167;
//                 GenVars.ironBar = 704;
//                 WorldGen.SavedOreTiers.Iron = 167;
//             }

//             if (WorldGen.genRand.Next(2) == 0)
//             {
//                 GenVars.silver = 168;
//                 GenVars.silverBar = 705;
//                 WorldGen.SavedOreTiers.Silver = 168;
//             }

//             if ((!WorldGen.dontStarveWorldGen || WorldGen.drunkWorldGen) && WorldGen.genRand.Next(2) == 0)
//             {
//                 GenVars.gold = 169;
//                 GenVars.goldBar = 706;
//                 WorldGen.SavedOreTiers.Gold = 169;
//             }

//             //WorldGen.crimson = WorldGen.genRand.Next(2) == 0;
//             // NEED TINKERING
//             if (WorldGen.WorldGenParam_Evil == 0) WorldGen.crimson = false;
//             if (WorldGen.WorldGenParam_Evil == 1) WorldGen.crimson = true;

//             if (GenVars.jungleHut == 0)
//                 GenVars.jungleHut = 119;
//             else if (GenVars.jungleHut == 1)
//                 GenVars.jungleHut = 120;
//             else if (GenVars.jungleHut == 2)
//                 GenVars.jungleHut = 158;
//             else if (GenVars.jungleHut == 3)
//                 GenVars.jungleHut = 175;
//             else if (GenVars.jungleHut == 4)
//                 GenVars.jungleHut = 45;

//             //Main.worldID = genRand.Next(int.MaxValue);
//             //WorldGen.RandomizeTreeStyle();
//             //WorldGen.RandomizeCaveBackgrounds();
//             //WorldGen.RandomizeBackgrounds(WorldGen.genRand);
//             //WorldGen.RandomizeMoonState(WorldGen.genRand);

//             WorldGen.TreeTops.CopyExistingWorldInfoForWorldGeneration();

//             // determine dungeon side
//             GenVars.dungeonSide = ((WorldGen.genRand.Next(2) != 0) ? 1 : (-1));
//             if (WorldGen.remixWorldGen)
//             {
//                 if (GenVars.dungeonSide == -1)
//                 {
//                     double num1087 = 1.0 - (double)WorldGen.genRand.Next(20, 35) * 0.01;
//                     GenVars.jungleOriginX = (int)((double)Main.maxTilesX * num1087);
//                 }
//                 else
//                 {
//                     double num1088 = (double)WorldGen.genRand.Next(20, 35) * 0.01;
//                     GenVars.jungleOriginX = (int)((double)Main.maxTilesX * num1088);
//                 }
//             }
//             else
//             {
//                 int minValue3 = 15;
//                 int maxValue12 = 30;
//                 if (WorldGen.tenthAnniversaryWorldGen && !WorldGen.remixWorldGen)
//                 {
//                     minValue3 = 25;
//                     maxValue12 = 35;
//                 }

//                 if (GenVars.dungeonSide == -1)
//                 {
//                     double num1089 = 1.0 - (double)WorldGen.genRand.Next(minValue3, maxValue12) * 0.01;
//                     GenVars.jungleOriginX = (int)((double)Main.maxTilesX * num1089);
//                 }
//                 else
//                 {
//                     double num1090 = (double)WorldGen.genRand.Next(minValue3, maxValue12) * 0.01;
//                     GenVars.jungleOriginX = (int)((double)Main.maxTilesX * num1090);
//                 }
//             }

//             int num1091 = WorldGen.genRand.Next(Main.maxTilesX);
//             if (WorldGen.drunkWorldGen)
//                 GenVars.dungeonSide *= -1;

//             if (GenVars.dungeonSide == 1)
//             {
//                 while ((double)num1091 < (double)Main.maxTilesX * 0.6 || (double)num1091 > (double)Main.maxTilesX * 0.75)
//                 {
//                     num1091 = WorldGen.genRand.Next(Main.maxTilesX);
//                 }
//             }
//             else
//             {
//                 while ((double)num1091 < (double)Main.maxTilesX * 0.25 || (double)num1091 > (double)Main.maxTilesX * 0.4)
//                 {
//                     num1091 = WorldGen.genRand.Next(Main.maxTilesX);
//                 }
//             }

//             if (WorldGen.drunkWorldGen)
//                 GenVars.dungeonSide *= -1;

//             int num1092 = WorldGen.genRand.Next(50, 90);
//             double num1093 = (double)Main.maxTilesX / 4200.0;
//             num1092 += (int)((double)WorldGen.genRand.Next(20, 40) * num1093);
//             num1092 += (int)((double)WorldGen.genRand.Next(20, 40) * num1093);
//             int num1094 = num1091 - num1092;
//             num1092 = WorldGen.genRand.Next(50, 90);
//             num1092 += (int)((double)WorldGen.genRand.Next(20, 40) * num1093);
//             num1092 += (int)((double)WorldGen.genRand.Next(20, 40) * num1093);
//             int num1095 = num1091 + num1092;
//             if (num1094 < 0)
//                 num1094 = 0;

//             if (num1095 > Main.maxTilesX)
//                 num1095 = Main.maxTilesX;

//             GenVars.snowOriginLeft = num1094;
//             GenVars.snowOriginRight = num1095;
//             GenVars.leftBeachEnd = WorldGen.genRand.Next(GenVars.beachSandRandomCenter - GenVars.beachSandRandomWidthRange, GenVars.beachSandRandomCenter + GenVars.beachSandRandomWidthRange);
//             if (WorldGen.tenthAnniversaryWorldGen && !WorldGen.remixWorldGen)
//                 GenVars.leftBeachEnd = GenVars.beachSandRandomCenter + GenVars.beachSandRandomWidthRange;

//             if (GenVars.dungeonSide == 1)
//                 GenVars.leftBeachEnd += GenVars.beachSandDungeonExtraWidth;
//             else
//                 GenVars.leftBeachEnd += GenVars.beachSandJungleExtraWidth;

//             GenVars.rightBeachStart = Main.maxTilesX - WorldGen.genRand.Next(GenVars.beachSandRandomCenter - GenVars.beachSandRandomWidthRange, GenVars.beachSandRandomCenter + GenVars.beachSandRandomWidthRange);
//             if (WorldGen.tenthAnniversaryWorldGen && !WorldGen.remixWorldGen)
//                 GenVars.rightBeachStart = Main.maxTilesX - (GenVars.beachSandRandomCenter + GenVars.beachSandRandomWidthRange);

//             if (GenVars.dungeonSide == -1)
//                 GenVars.rightBeachStart -= GenVars.beachSandDungeonExtraWidth;
//             else
//                 GenVars.rightBeachStart -= GenVars.beachSandJungleExtraWidth;

//             int num1096 = 50;
//             if (GenVars.dungeonSide == -1)
//                 GenVars.dungeonLocation = WorldGen.genRand.Next(GenVars.leftBeachEnd + num1096, (int)((double)Main.maxTilesX * 0.2));
//             else
//                 GenVars.dungeonLocation = WorldGen.genRand.Next((int)((double)Main.maxTilesX * 0.8), GenVars.rightBeachStart - num1096);
//             int num1097 = 0;
//             if (Main.maxTilesX >= 8400)
//                 num1097 = 2;
//             else if (Main.maxTilesX >= 6400)
//                 num1097 = 1;
//             GenVars.extraBastStatueCountMax = 2 + num1097;
//             // so fucking random ???
//             // Main.tileSolid[659] = false;
//             #endregion
//         }

		

// 		public override void Unload()
// 		{
// 			harmodeGenPasses = null;
// 			genPasses = null;
// 			hookPasses = null;
// 		}

// 		public override void PostAddRecipes()
// 		{
// 			// petition for tmod to make hook for PostPostSetupContent
// 			LoadModdedGenPass();
// 		}

// 		public void LoadModdedGenPass()
// 		{
// 			Mod.Logger.Info("[World Gen] Preparing to load passes");

// 			// Reset modded gen pass
// 			genPasses = new List<ModGenPass>();
// 			hookPasses = new List<ModGenPass>();

// 			// load mod pass
// 			List<GenPass> fakeGenPass = new List<GenPass>();
// 			double totalWeight = 0f;
// 			double vanillaWeight = 0f;

// 			// vanilla gen pass always loaded
// 			foreach (var item in WorldGen.VanillaGenPasses)
// 			{
// 				fakeGenPass.Add(item.Value);
// 				totalWeight += item.Value.Weight;
// 				vanillaWeight += item.Value.Weight;
// 			}

// 			// trick the mod to modify world gen
// 			FieldInfo field = typeof(Terraria.ModLoader.SystemLoader).GetField("Systems", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
// 			ModSystem[] modsystems = (ModSystem[])field.GetValue(null);

// 			foreach (var system in modsystems)
// 			{
// 				system.ModifyWorldGenTasks(fakeGenPass, ref totalWeight);

// 				if (system.GetType().IsMethodOverridden("PreWorldGen"))
// 				{
// 					hookPasses.Add(new ModGenPass(system.Mod, system, system.Name + " Pre Worldgen", delegate { system.PreWorldGen(); }));	
// 				}

// 				if (system.GetType().IsMethodOverridden("PostWorldGen"))
// 				{
// 					hookPasses.Add(new ModGenPass(system.Mod, system, system.Name + " Post Worldgen", delegate { system.PostWorldGen(); }));	
// 				}

// 				for (int i = 0; i < fakeGenPass.Count; i++)
// 				{
// 					var pass = fakeGenPass[i];
// 					if (!WorldGen.VanillaGenPasses.ContainsKey(pass.Name))
// 					{
// 						string source = "";
						
// 						if (i > 0)
// 						{
// 							source += $"Happens after {fakeGenPass[i - 1]} in step {i}\n";
// 						}

// 						if (i < fakeGenPass.Count - 1)
// 						{
// 							source += $"Happens before {fakeGenPass[i+1]} in step {i}\n";
// 						}

// 						source += "Loaded on ModifyWorldGenTasks";

// 						Gearedup.Get.Logger.Info("[World Gen] Loading pass : " + pass.Name + " from " + system.Mod.Name);
// 						genPasses.Add(new ModGenPass(system.Mod, system, pass,source));
// 					}
// 				}
// 			}

// 			Mod.Logger.Info($"[World Gen] succesfully passed {totalWeight - vanillaWeight} weight [ {genPasses.Count} | {hookPasses.Count} ]");
// 		}
//     }

//     // public class WorldgenCatalyst : ModItem
//     // {
//     //     public override void SetDefaults()
//     //     {
//     //         base.SetDefaults();
//     //     }
//     // }
// }