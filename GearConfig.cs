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
		public bool AllowAmmoPack;

		[DefaultValue(true)]
		[ReloadRequired] 
		public bool AllowEndlessThrowables;

		[DefaultValue(true)]
		[ReloadRequired] 
		public bool AllowLunarBugNet;
 
		[Header("DyeGears")]

		[DefaultValue(true)] 
		public bool AllowWeaponDye;

		[DefaultValue(false)] 
		public bool AllowDyeConsumed;

		[DefaultValue(true)] 
		public bool WaterRemoveDye;
	}

	public class GearClientConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;
		public static GearClientConfig Get => ModContent.GetInstance<GearClientConfig>();

        // save the config , this requires reflection though.
        public static void SaveConfig() => typeof(ConfigManager).GetMethod("Save", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[1] { Get });

		[Header("GearDye")]

		[DefaultValue(false)]
		public bool DyeRenderTargetsAll;

		[DefaultValue(false)]
		public bool DyeRenderTargetsModded;

		public List<ItemDefinition> DyeRenderTargetItemsList = new List<ItemDefinition>();
		public void AddItemRT(Item item)
		{
			DyeRenderTargetItemsList.Add(new ItemDefinition(item.type));
		}
		public bool IsItemRT(Item item)
		{
			if (DyeRenderTargetItemsList == null) return false;
			if (DyeRenderTargetItemsList.Count <= 0) return false;
			if (item is null || item.IsAir) return false;

			foreach (var rt in DyeRenderTargetItemsList)
			{
				if (rt != null && item.type == rt.Type)
				{
					return true;
				}
			}
			return false;
		}



		[DefaultValue(true)] 
		[ReloadRequired]
		public bool DyeProjectileDust;
		
		[DefaultValue(false)] 
		public bool DyeItemPlayerShader;

		[DefaultValue(false)] 
		public bool DyeOverlapItemLayer;

		[DefaultValue(true)] 
		public bool DyeItemDyeDye;

		[DefaultValue(true)] 
		public bool DyeItemName;
	}
}