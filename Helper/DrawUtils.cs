using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.DataStructures;

namespace Gearedup.Helper
{
	/// <summary>
	/// Old ass funkin Utilities from the dead dead legacy 1.3 mods
	/// </summary>
	public static class DrawUtils 
	{
		/// <summary>
		/// Get vanilla projectile texture path
		/// </summary>
		/// <param name="id">vanilla projectile id</param>
		/// <returns>Desired texture</returns>
		public static string PathProj(int id)
		{
			return $"Terraria/Images/Projectile_{id}";
		}

		/// <summary>
		/// Get vanilla npc texture path
		/// </summary>
		/// <param name="id">vanilla npc id</param>
		/// <returns>Desired texture</returns>
		public static string PathNPC(int id)
		{
			return $"Terraria/Images/NPC_{id}";
		}

		/// <summary>
		/// Get vanilla item texture path
		/// </summary>
		/// <param name="id">vanilla item id</param>
		/// <returns>Desired texture</returns>
		public static string PathItem(int id)
		{
			return $"Terraria/Images/Item_{id}";
		}

		/// <summary>
		/// Get Direction from a position
		/// </summary>
		public static Vector2 DirectionFrom(this Vector2 From, Vector2 Source)
		{
			return Vector2.Normalize(From - Source);
		}

		/// <summary>
		/// Round Vector2 to an intreger , used for pixel perfect stuff
		/// </summary>
		/// <param name="pos"> the position </param>
		/// <returns></returns>
		public static Vector2 RoundToInt(this Vector2 pos){
			return new Vector2((int)pos.X,(int)pos.Y);
		}

		// SPRITE BATCH UTILITIES

		/// <summary>
		/// spriteBatch begin but apply armor gameshader afterwards
		/// </summary>
		/// <param name="id">The item id of dye.</param>
		/// <param name="entity">The entity that will get shadered</param>
		/// <param name="end">Wether or not the sprite should be Ended before started.</param>
		/// <param name="ui">Wether it scaled by ui or zoom.</param>
		public static void BeginDyeShader(this SpriteBatch spriteBatch,int id, Entity entity, bool end = false,bool ui = false,DrawData? drawData = null)
		{
			spriteBatch.BeginImmediate(end,ui);
			GameShaders.Armor.Apply(id, entity, drawData);
		}

		// im too lazy to replace the implementation
		public static void BeginDyeShaderByItem(this SpriteBatch spriteBatch,int itemid, Entity entity, bool end = false,bool ui = false,DrawData? drawData = null) {
			spriteBatch.BeginImmediate(end,ui);
			GameShaders.Armor.Apply(GameShaders.Armor.GetShaderIdFromItemId(itemid), entity, null);
		}

		// Turns out we dont need this but i still put it here just in case
		// Stolen from ShaderLib
		public static void Restart(this SpriteBatch sb, Matrix scaleMatrix, bool forShader = true, bool worldDraw = true)
		{
			Rectangle scissor = sb.GraphicsDevice.ScissorRectangle;

			sb.End();
			sb.GraphicsDevice.ScissorRectangle = scissor;
			sb.Begin(
				forShader ? SpriteSortMode.Immediate : SpriteSortMode.Deferred,
				BlendState.AlphaBlend,
				worldDraw ? SamplerState.PointClamp : SamplerState.LinearClamp,
				sb.GraphicsDevice.DepthStencilState,
				sb.GraphicsDevice.RasterizerState,
				null,
				scaleMatrix
			);
		}

		/// <summary>
		/// spriteBatch begin but using immediate SpriteSortMode (for shader applying)
		/// </summary>
		/// <param name="end">Wether or not the sprite should be Ended before started.</param>
		/// <param name="ui">Wether it scaled by ui or zoom.</param>
		public static void BeginImmediate(this SpriteBatch spriteBatch,bool end = false, bool ui = false, bool additive = false) {
			if (end) {spriteBatch.End();}
			var scale = Main.GameViewMatrix.ZoomMatrix;
			if (ui) {scale = Main.UIScaleMatrix;}
			BlendState blend = null;
			if (additive) {blend = BlendState.Additive;}
			spriteBatch.Begin(SpriteSortMode.Immediate, blend, null, null, null, null, scale);
		}
		
        /// <summary>
		/// spriteBatch begin but using normal sort mode and effects
		/// </summary>
		/// <param name="end">Wether or not the sprite should be Ended before started.</param>
		/// <param name="ui">Wether it scaled by ui or zoom.</param>
		public static void BeginNormal(this SpriteBatch spriteBatch, bool end = false, bool ui = false) {
			if (end) {spriteBatch.End();}
			//Main.GameViewMatrix.TransformationMatrix
			var scale = Main.GameViewMatrix.ZoomMatrix;
			if (ui) {scale = Main.UIScaleMatrix;}
			spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, scale);
		}

		/// <summary>
		/// spriteBatch begin but using blendstate.additive
		/// </summary>
		/// <param name="end">Wether or not the sprite should be Ended before started.</param>
		/// <param name="ui">Wether it scaled by ui or zoom.</param>
        public static void BeginGlow(this SpriteBatch spriteBatch, bool end = false,bool ui = false) {
			if (end) {spriteBatch.End();}
			var scale = Main.GameViewMatrix.ZoomMatrix;
			if (ui) {scale = Main.UIScaleMatrix;}
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, scale);
		}
	}
}