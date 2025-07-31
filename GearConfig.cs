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
		[ReloadRequired] 
		public bool DyeRenderTargets;

		[DefaultValue(true)] 
		[ReloadRequired]
		public bool DyeProjectileDust;
		
		[DefaultValue(false)] 
		public bool DyeItemPlayerShader;

		[DefaultValue(false)] 
		public bool DyeOverlapItemLayer;

		// [DefaultValue(true)] 
		// public bool DyeItemDyeDye;

		[DefaultValue(true)] 
		public bool DyeItemName;

		[DefaultValue(true)] 
		public bool DyeItemPrefix;
	}
}