using System;
using System.Collections.Generic;
using Gearedup.Content.Items;
using Gearedup.Content.Perks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Gearedup
{
	public partial class Gearedup : Mod
	{
		// get instances
		public static Gearedup Get => ModContent.GetInstance<Gearedup>();
		public static string DotTexture => "Gearedup/Content/Dot";
		public static string PlaceholderTexture => "Gearedup/Content/Placeholder";

		public List<string> errors;

		// cross mod ig
		public Mod calamityMod;
		public Mod fargoSoul;

		public static void Log(string context, bool error = false)
		{
			// dont log anything if debugmode is disabled
			if (!GearServerConfig.Get.DebugMode) return;

			if (!Main.gameMenu)
			{
				Main.NewText(context);
			}

			if (error)
			{
				Get.errors.Add(context);
				Get.Logger.Info(context);
			}
		}

		public override object Call(params object[] args)
		{
			switch (args[0])
			{
				case "GetLogs":
					return errors;
				case "Log":
					Log("[call] " + args[1]);
					break;
				case "NotSupported":
					throw new Exception("This mod is not supported using " + args[1] + ", please disable " + DisplayNameClean + " for the time being");
				default:
					errors.Add($"Unknown call: {args[0]}");
					break;
			}
			return null;
		}

		public override void Load()
		{
			errors = new List<string>();

			On_ItemSlot.DrawItemIcon += ItemSlot_DrawItemIcon;
			On_Item.NewItem_Inner += Item_NewItem_Inner;
			On_Main.GetProjectileDesiredShader += ShaderPatch;
			// Terraria.On_Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float += ProjPatch;
			On_PlayerDrawLayers.DrawPlayer_27_HeldItem += ShittyPatch;

			if (GearClientConfig.Get.DyeProjectileDust) { On_Dust.NewDust += DustPatch; }

			calamityMod = LoadMod("CalamityMod", "Recipes, npcs , projectiles patches");
			fargoSoul = LoadMod("Fargowiltasoul", "Recipes patches i guess idk");

			// thoriumMod = ModLoader.GetMod("ThoriumMod"); 
		}

		public override void PostSetupContent()
		{
			// calamityMod = LoadMod("CalamityMod", "Recipes, npcs , projectiles patches");
			// fargoSoul = LoadMod("Fargowiltasoul", "Recipes patches I guess idk");
			// base.PostSetupContent();
		}

		public Mod LoadMod(string name, string fuck)
		{
			if (ModLoader.TryGetMod(name, out Mod res))
			{
				Logger.Info("Adding crossmod shit to " + name + " for " + fuck);
				return res;
			}
			Logger.Info("Failed to integrate " + name + " , couldnt find the mod dumbass");
			return null;
		}

		public override void Unload()
		{
			errors = null;
		}

		private int Item_NewItem_Inner(On_Item.orig_NewItem_Inner orig, IEntitySource source, int x, int y, int width, int height, Item itemToClone, int type, int stack, bool noBroadcast, int pfix, bool noGrabDelay, bool reverseLookup)
		{
			// do fun stuff in hardmode
			if (Main.hardMode && source is EntitySource_DropAsItem projSource)
			{
				if (projSource.Entity is Projectile projectile)
				{
					if (projectile.type == ProjectileID.FallingStar)
					{
						// 1/15 chance to turn into a white star
						if (Main.rand.NextBool(15))
						{
							return orig(source, x, y, width, height, itemToClone, ModContent.ItemType<WhiteStar>(), stack, noBroadcast, pfix, noGrabDelay, reverseLookup);
						}
					}
				}
			}
			return orig(source, x, y, width, height, itemToClone, type, stack, noBroadcast, pfix, noGrabDelay, reverseLookup);
		}
		private float ItemSlot_DrawItemIcon(On_ItemSlot.orig_DrawItemIcon orig, Item item, int context, SpriteBatch spriteBatch, Vector2 screenPositionForItemCenter, float scale, float sizeLimit, Color environmentColor)
		{
			if (Main.mouseItem != null)
			{
				if (Main.mouseItem.ModItem is GearPerk)
				{
					if (GearItem.CanGeared(item))
					{
						// scale += (float)(Math.Sin(Main.GameUpdateCount * 0.1) * 0.1);
						return orig(item, context, spriteBatch, screenPositionForItemCenter,
						scale + (float)(Math.Sin(Main.GameUpdateCount * 0.1) * 0.1),
						sizeLimit,
						environmentColor);
					}
				}
				// else if (Main.mouseItem.accessory)
				// {

				// }
				else if (GearItem.CanGeared(Main.mouseItem) || Main.mouseItem.type == ModContent.GetInstance<UniversalDyer>().Type)
				{
					if (item.dye > 0 && item.type != ModContent.GetInstance<UniversalDyer>().Type)
					{
						return orig(item, context, spriteBatch, screenPositionForItemCenter,
						scale + (float)(Math.Sin(Main.GameUpdateCount * 0.1) * 0.1),
						sizeLimit,
						environmentColor);
					}
				}
			}
			return orig(item, context, spriteBatch, screenPositionForItemCenter, scale, sizeLimit, environmentColor);
		}

		// We patch creation of dust
		private int DustPatch(On_Dust.orig_NewDust orig, Vector2 position, int width, int height, int type, float speedX, float speedY, int alpha, Color newColor, float scale)
		{

			// new method

			int i = orig(position, width, height, type, speedX, speedY, alpha, newColor, scale);
			if (ProjectileAITrack.currentAI != -1)
			{
				var projectile = Main.projectile[ProjectileAITrack.currentAI];

				if (projectile != null && projectile.active && projectile.timeLeft >= 1)
				{
					var gp = projectile.GetGlobalProjectile<GearProjectile>();
					if (gp.dye > 0)
					{
						// Main.dust[i].shader = GameShaders.Armor.GetSecondaryShader(gp.dye, Main.LocalPlayer);
						Main.dust[i].shader = GameShaders.Armor.GetSecondaryShader(gp.dye, Main.LocalPlayer);
					}
				}
			}
			return i;
		}

		private void ShittyPatch(On_PlayerDrawLayers.orig_DrawPlayer_27_HeldItem orig, ref PlayerDrawSet drawinfo)
		{

			int oldCount = drawinfo.DrawDataCache.Count;

			orig(ref drawinfo);

			int newCount = drawinfo.DrawDataCache.Count;

			if (drawinfo.drawPlayer.JustDroppedAnItem) return;

			Item heldItem = drawinfo.heldItem;
			if (heldItem == null || heldItem.IsAir) return;

			// int? shaderNum = heldItem.GetGlobalItem<GearItem>().dye.id;//GetShader(heldItem);

			if (heldItem.TryGetGlobalItem(out GearItem gi))
			{
				if (gi.dye.id is int dyeID)
				{
					int shader = GameShaders.Armor.GetShaderIdFromItemId(dyeID);
					if (shader <= 0)
					{
						Log("[drawlayer helditem] shader is zero for " + heldItem.Name, true);
						return;
					}

					for (int i = oldCount; i < newCount; i++)
					{
						DrawData cloneData = drawinfo.DrawDataCache[i];
						cloneData.shader = shader;

						if (GearClientConfig.Get.DyeOverlapItemLayer)
						{
							drawinfo.DrawDataCache.Add(cloneData);
						}
						else
						{
							drawinfo.DrawDataCache[i] = cloneData;
						}
					}
				}
			}
		}

		private int ShaderPatch(On_Main.orig_GetProjectileDesiredShader orig, Projectile proj)
		{

			if (proj.active && proj.TryGetGlobalProjectile(out GearProjectile dyedProjectile))
			{
				// Don't use shader again if it tried to use render target methods
				if (dyedProjectile.dye > 0 && !dyedProjectile.ShouldRenderTarget(proj))
				{
					return dyedProjectile.dye;
				}
			}
			return orig(proj);
		}
	}


	public class SlimShady : ModSystem
	{
		public override void OnWorldLoad()
		{
			var errors = (List<string>)Mod.Call("GetErrors");

			if (errors != null && errors.Count > 0)
			{
				Main.NewText($"Found {errors.Count} registered logs. skibidi toilet haktuah", Color.LightPink);
			}
		}

		public override void PostAddRecipes()
		{
			BossBagPatchSystem.ResetDictionaries();

			foreach (var item in ContentSamples.ItemsByType)
			{
				//ProjectileLightDye.Get?.AddUColoredDye(item.Value);
				BossBagPatchSystem.RegisterDropsDB(item.Value);
			}   
        }

		public override void PostDrawInterface(SpriteBatch spriteBatch)
		{
			if (Main.mouseItem != null && Main.HoverItem != null && !Main.HoverItem.IsAir)
			{
				if (Main.HoverItem.dye > 0 && GearItem.CanGeared(Main.mouseItem))
				{
					ItemSlot.DrawItemIcon(Main.HoverItem, 1, spriteBatch, new Vector2(Main.mouseX, Main.mouseY), 2f, 1f, Color.White);
				}

				// else if (GearItem.CanGeared(Main.HoverItem) && Main.mouseItem.ModItem is GearPerk)
				// {

				// }
			}
		}
		// public override void UpdateUI(GameTime gameTime)
		// {
		// 	if (Main.mouseItem != null && Main.mouseItem.ModItem is GearPerk)
		// 	{
		// 		if (Main.HoverItem != null && !Main.HoverItem.IsAir && GearItem.CanGeared(Main.HoverItem))
		// 		{
		// 			Main.NewText("Geared Item", Color.Cyan);
		// 			Main.LocalPlayer.cursorItemIconText = "Geared Item";
		// 		}
		// 	}
		// }
	}
}
