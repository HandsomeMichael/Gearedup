using System;
using System.Collections.Generic;
using Gearedup.Content.Catched;
using Gearedup.Content.Items;
using Gearedup.Content.Perks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content.Sources;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Gearedup
{
	public class Gearedup : Mod
	{
		public List<string> errors;

		public void AddError(string context)
		{

		}

		public override object Call(params object[] args)
		{
			switch (args[0])
			{
				case "GetErrors":
					return errors;
				case "Error":
					errors.Add(args[1].ToString());

					break;
				case "NotSupported":
					throw new Exception("This mod is not supported using one of your mods, please remove this shit bruh");
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
			Terraria.On_Item.NewItem_Inner += Item_NewItem_Inner;
			Terraria.On_Main.GetProjectileDesiredShader += ShaderPatch;
			Terraria.On_Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float += ProjPatch;
			Terraria.DataStructures.On_PlayerDrawLayers.DrawPlayer_27_HeldItem += ShittyPatch;

			if (GearClientConfig.Get.DyeProjectileDust) { Terraria.On_Dust.NewDust += DustPatch; }
		}

		public override void Unload()
		{
			errors = null;
		}

		private int Item_NewItem_Inner(On_Item.orig_NewItem_Inner orig, IEntitySource source, int X, int Y, int Width, int Height, Item itemToClone, int Type, int Stack, bool noBroadcast, int pfix, bool noGrabDelay, bool reverseLookup)
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
							return orig(source, X, Y, Width, Height, itemToClone, ModContent.ItemType<WhiteStar>(), Stack, noBroadcast, pfix, noGrabDelay, reverseLookup);
						}
					}
				}
			}
			return orig(source, X, Y, Width, Height, itemToClone, Type, Stack, noBroadcast, pfix, noGrabDelay, reverseLookup);
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
			}
			return orig(item, context, spriteBatch, screenPositionForItemCenter, scale, sizeLimit, environmentColor);
		}

		// We patch creation of dust
		private int DustPatch(On_Dust.orig_NewDust orig, Vector2 Position, int Width, int Height, int Type, float SpeedX, float SpeedY, int Alpha, Color newColor, float Scale)
		{

			// new method

			int i = orig(Position, Width, Height, Type, SpeedX, SpeedY, Alpha, newColor, Scale);
			if (ProjectileDustSupport.currentAI != -1)
			{
				var projectile = Main.projectile[ProjectileDustSupport.currentAI];

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

			if (heldItem.TryGetGlobalItem<GearItem>(out GearItem gi))
			{
				if (gi.dye.id is int dyeID)
				{
					int shader = ContentSamples.ItemsByType[dyeID].dye;
					if (shader <= 0)
					{
						Main.NewText("Error, shader is zero");
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
		
		// TO DO : Move this to OnSpawn Hook
		private int ProjPatch(On_Projectile.orig_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float orig,
		IEntitySource spawnSource, float X, float Y, float SpeedX, float SpeedY, int Type, int Damage, float KnockBack, int Owner, float ai0, float ai1, float ai2)
		{
			int hasil = orig(spawnSource, X, Y, SpeedX, SpeedY, Type, Damage, KnockBack, Owner, ai0, ai1, ai2);

			// if (!GearServerConfig.Get.ProjectileFollowParentDye) return hasil;

			// get from item
			if (hasil >= 0 && spawnSource is IEntitySource_WithStatsFromItem itemSource)
			{
				// Main.NewText("from item");
				if (itemSource.Item.TryGetGlobalItem<GearItem>(out GearItem globalItem))
				{
					// Main.NewText("got moditem");
					if (globalItem.dye.id is int dyeID && Main.projectile[hasil].TryGetGlobalProjectile<GearProjectile>(out GearProjectile globalProj))
					{
						// Main.NewText("success applied " + dyedItem.dye);
						globalProj.dye = (short)ContentSamples.ItemsByType[dyeID].dye;

						if (GearClientConfig.Get.IsItemRT(itemSource.Item))
						{
							globalProj.useRenderTarget = true;
						}
						else if (DyeRenderer.IsCustomDrawed(Main.projectile[hasil]))
						{
							globalProj.useRenderTarget = true;
						}
					}
				}
			}

			// We could check player ammo for this but im lazy ahh mf

			// if (hasil >= 0 && spawnSource is EntitySource_ItemUse_WithAmmo ammoSource)
			// {
			//     if (ammoSource.AmmoItemIdUsed != 0) 
			//     {
			//     }
			// }
			if (hasil >= 0 && spawnSource is EntitySource_ItemUse_WithAmmo ammoSource)
			{
				if (ammoSource.AmmoItemIdUsed == ModContent.ItemType<CatchedProjectile>())
				{
					Main.projectile[hasil].hostile = false;
					Main.projectile[hasil].friendly = true;
				}
			}

			// get from parents
			if (hasil >= 0 && spawnSource is EntitySource_Parent entitySource)
			{
				// projectile
				if (entitySource.Entity is Projectile projSource)
				{
					if (projSource.TryGetGlobalProjectile<GearProjectile>(out GearProjectile parentDyedProjectile))
					{
						if (parentDyedProjectile.dye > 0 && Main.projectile[hasil].TryGetGlobalProjectile<GearProjectile>(out GearProjectile GearProjectile))
						{
							GearProjectile.dye = parentDyedProjectile.dye;
							// if (DyeRenderer.IsCustomDrawed(Main.projectile[hasil]))
							if (parentDyedProjectile.ShouldRenderTarget())
							{
								GearProjectile.useRenderTarget = true;
							}
						}

					}
				}
				// NPC TO DO :

				else if (entitySource.Entity is NPC npcSource)
				{
					if (npcSource.TryGetGlobalNPC<BrainWashedNPC>(out BrainWashedNPC bw))
					{
						if (bw.ownedBy != -1 && Main.projectile[hasil].TryGetGlobalProjectile<BrainWashedProj>(out BrainWashedProj bwr))
						{
							bwr.ownedBy = bw.ownedBy;
							Main.projectile[hasil].hostile = true;
							Main.projectile[hasil].friendly = true;
						}
					}
					// if (npcSource.TryGetGlobalNPC<DyedNPC>(out DyedNPC dyedNPC))
					// {
					//     if (dyedNPC.dye > 0 && Main.projectile[hasil].TryGetGlobalProjectile<GearProjectile>(out GearProjectile GearProjectile)) 
					//     {
					//         GearProjectile.dye = dyedNPC.dye;
					//     }
					// }
				}
			}
			return hasil;
		}

		private int ShaderPatch(On_Main.orig_GetProjectileDesiredShader orig, Projectile proj)
		{
			// if (proj != null && proj.active && proj.owner != 255)
			// {
			//     if (proj.TryGetOwner(out Player player)) 
			//     {
			//         if (player.TryGetModPlayer<DyePlayer>(out DyePlayer dyePlayer)) 
			//         {
			//             return dyePlayer.dye;
			//         }
			//     }
			// }
			if (proj.active && proj.TryGetGlobalProjectile<GearProjectile>(out GearProjectile dyedProjectile))
			{
				// Dont use shader again if it tried to use render target methods
				if (dyedProjectile.dye > 0 && !dyedProjectile.ShouldRenderTarget())
				{
					return dyedProjectile.dye;
				}
			}
			return orig(proj);
		}
	
	}
	
	/// <summary>
	/// Think about worse coding implementation and this problaby it bruh
	/// </summary>
	public class SlimShady : ModSystem
	{

		public override void OnWorldLoad()
		{
			var errors = (List<string>)Mod.Call("GetErrors");
			if (errors != null && errors.Count > 0)
			{
				Main.NewText($"There is {errors.Count} errors found during loading, check the log for more details.", Color.Red);
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
