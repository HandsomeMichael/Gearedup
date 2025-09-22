using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Serialization;

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace Gearedup
{
	public class GearServerConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;
		public static GearServerConfig Get => ModContent.GetInstance<GearServerConfig>();

		// save the config , this requires reflection though.
		public static void SaveConfig() => typeof(ConfigManager).GetMethod("Save", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[1] { Get });

		[Header("Content")]

		[DefaultValue(true)]
		[ReloadRequired]
		public bool Endless_AmmoPack;

		[DefaultValue(true)]
		[ReloadRequired]
		public bool Endless_Throwable;

		[DefaultValue(true)]
		[ReloadRequired]
		public bool Endless_DuplicateAccesory;

		[DefaultValue(true)]
		[ReloadRequired]
		public bool Catched_SuperBugNetObtainable;

		[DefaultValue(true)]
		public bool Catched_Projectile;

		[DefaultValue(true)]
		public bool Catched_ProjectileAsAmmo;

		[DefaultValue(true)]
		public bool Catched_NPCs;

		[DefaultValue(true)]
		public bool Catched_Bosses;

		[Header("DyeGears")]

		[DefaultValue(true)]
		public bool DyeItem_Toggle;

		[DefaultValue(false)]
		public bool DyeItem_ConsumeOnUse;

		[Header("Experimental")]

		[DefaultValue(false)]
		[ReloadRequired]
		public bool Content_DeityStaff;

		[DefaultValue(true)]
		[ReloadRequired]
		public bool Content_PhilosopherStone;

		[DefaultValue(true)]
		[ReloadRequired]
		public bool Content_LustruousChest;

		[DefaultValue(true)]
		public bool WGLoader;

		[Header("Debug")]
		[DefaultValue(false)]
		public bool DebugMode;

		public override void OnChanged()
		{
			// if (GearServerConfig.Get != null)
			// 	BossBagPatchSystem.UpdateDBPlease();
		}
	}

	public class GearClientConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;
		public static GearClientConfig Get => ModContent.GetInstance<GearClientConfig>();

		// save the config , this requires reflection though.
		// deprecated i think
		// public static void SaveConfig() => typeof(ConfigManager).GetMethod("Save", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[1] { Get });

		[Header("GearDye")]

		[DefaultValue(true)]
		public bool DyeRender_NPCToggle;

		[DefaultValue(DyeGraphics.Auto)]
		[DrawTicks]
		public DyeGraphics DyeRender_ProjectileVanillaGraphics;

		[DefaultValue(DyeGraphics.Auto)]
		[DrawTicks]
		public DyeGraphics DyeRender_ProjectileModGraphics;

		// [JsonDefaultListValue("{\"name\": \"GoldBar\"}")]
		public List<ItemDefinition> DyeRender_ItemForceFancyGraphics;
		//new List<ItemDefinition>();

		// // [JsonDefaultListValue("{\"name\": \"GoldBar\"}")]
		// public List<ProjectileDefinition> DyeRenderTargetProjectileList = new List<ProjectileDefinition>();

		// public void AddItemRT(Item item)
		// {
		// 	DyeRenderTargetItemsList.Add(new ItemDefinition(item.type));
		// 	SaveChanges();
		// }
		// public void AddProjectileRT(Projectile projectile)
		// {
		// 	DyeRenderTargetProjectileList.Add(new ProjectileDefinition(projectile.type));
		// }
		public bool IsItemFancyGraphics(Item item)
		{
			if (DyeRender_ItemForceFancyGraphics == null) return false;
			if (DyeRender_ItemForceFancyGraphics.Count <= 0) return false;
			if (item is null || item.IsAir) return false;

			foreach (var rt in DyeRender_ItemForceFancyGraphics)
			{
				if (rt != null && item.type == rt.Type)
				{
					return true;
				}
			}
			return false;
		}

		[DefaultValue(false)]
		[ReloadRequired]
		public bool DyeSupport_Light;

		// ts doesnt really need a setting
		
		// [DefaultValue(true)]
		// [ReloadRequired]
		// public bool DyeSupport_Dust;

		[DefaultValue(false)]
		public bool DyeItem_UsePlayerShader;

		[DefaultValue(false)]
		public bool DyeItem_OverlapLayer;

		[DefaultValue(true)]
		public bool DyeItem_ShowDyePreview;

		[DefaultValue(true)]
		public bool DyeItem_FancyName;
	}
	
	public enum DyeGraphics
	{
		Off,
		Fast,
		Auto,
		Fancy
	}
}