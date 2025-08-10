using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;
using Terraria.Utilities;

namespace Gearedup.Helper
{
    public static class Helpme
    {
        public static GearItem Gear(this Item item)
        {
            return item.GetGlobalItem<GearItem>();
        }

		/// <summary>
		/// Uhhhhh
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty(this string text)
		{
			return text == null || text == "";
		}

		public static bool AnyNullOrEmpty(params string[] text)
		{
			foreach (var item in text)
			{
				if (item.IsNullOrEmpty())
				{
					return true;
				}
			}
			return false;
		}

        // I got 2 word warping method for no reason at all
		public static List<string> WordWrap(string text, int maxLineLength)
		{
			var list = new List<string>();

			int currentIndex;
			var lastWrap = 0;
			var whitespace = new[] { ' ', '\r', '\n', '\t' };
			do
			{
				currentIndex = lastWrap + maxLineLength > text.Length ? text.Length : (text.LastIndexOfAny(new[] { ' ', ',', '.', '?', '!', ':', ';', '-', '\n', '\r', '\t' }, Math.Min(text.Length - 1, lastWrap + maxLineLength)) + 1);
				if (currentIndex <= lastWrap)
					currentIndex = Math.Min(lastWrap + maxLineLength, text.Length);
				list.Add(text.Substring(lastWrap, currentIndex - lastWrap).Trim(whitespace));
				lastWrap = currentIndex;
			} while (currentIndex < text.Length);

			return list;
		}

		public static string WordWarpGPT(this string input, int chunkSize)
		{
			if (chunkSize <= 0) throw new ArgumentOutOfRangeException();
			var o = new StringBuilder(); int l = 0;
			foreach (var w in input.Split(' '))
			{
				if (l + w.Length > chunkSize) { if (l > 0) o.AppendLine(); l = 0; }
				if (l > 0) o.Append(' ');
				o.Append(w); l += w.Length + (l > 0 ? 1 : 0);
			}
			return o.ToString();
		}

		public static bool IsWack(this Player player , bool noItems = false)
		{
			return !player.active || player.dead || (player.noItems && noItems) || player.CCed;
		}

		/// <summary>
		/// Magically get frame count outta my ass
		/// My lawyer told me to not do this since modulo is costly but i lick ass
		/// </summary>
		/// <param name="frameCount"> the max frame</param>
		/// <param name="frameSpeed"> the time of frame changes</param>
		/// <returns></returns>
		public static int MagicallyGetFrame(int frameCount , int frameSpeed)
		{
			return (int)((Main.GameUpdateCount / frameSpeed) % frameCount);
		}

        /// <summary>
		/// resize projectile hitbox
		/// </summary>
        public static void Resize(this Projectile projectile, int newWidth = 0, int newHeight = 0){
            Vector2 oldCenter = projectile.Center;
            projectile.width = newWidth;
            projectile.height = newHeight;
            projectile.Center = oldCenter;
        }

		/// <summary>
		/// Check if the player can do heldproj amogos
		/// </summary>
		public static bool CanHeldProj(this Player player,bool checkChanneling = false) {
			return !player.active || player.dead || player.noItems || player.CCed || player.HeldItem.IsAir || (checkChanneling && !player.channel);
		}

		/// <summary>
		/// get the distance rotation of an object
		/// the point has to be radians
		/// </summary>
		public static float RotationDistance(float point,float point2) {
			return new Vector2(0,MathHelper.ToDegrees(point)).Distance(new Vector2(0,MathHelper.ToDegrees(point2)));
		}

		/// <summary>
		/// Get Dictionary data using Dictionary Key
		/// </summary>
		public static bool ContainsKey<TValue>(Dictionary<string,TValue> dict,string key) {
			if (dict.Count == 0) {return false;}
			foreach (var item in dict){
				if (item.Key == key) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Clone a list
		/// </summary>
		public static List<T> CloneList <T>(this List<T> clone) {
			var list = new List<T>();
			foreach (var item in clone){list.Add(item);}
			return list;
		}
		/// <summary>
		/// Shorthand for Vector2.Lerp
		/// </summary>
		public static Vector2 Lerp(this Vector2 pos,Vector2 pos2,float intensity) => Vector2.Lerp(pos,pos2,intensity);

		/// <summary>
		/// Get angle to a position
		/// </summary>
        public static float AngleTo(this Vector2 From,Vector2 Destination){
			return (float)Math.Atan2(Destination.Y - From.Y, Destination.X - From.X);
		}
		/// <summary>
		/// Get angle from a position
		/// </summary>
		public static float AngleFrom(this Vector2 From,Vector2 Source){
			return (float)Math.Atan2(From.Y - Source.Y, From.X - Source.X);
		}
		/// <summary>
		/// Distance Squared. short hand for Vector2.DistanceSquared
		/// </summary>
		public static float DistanceSQ(this Vector2 From,Vector2 Other){
			return Vector2.DistanceSquared(From, Other);
		}
		/// <summary>
		/// Get direction to a position
		/// </summary>
		public static Vector2 DirectionTo(this Vector2 From,Vector2 Destination){
			return Vector2.Normalize(Destination - From);
		}
		/// <summary>
		/// Get Direction from a position
		/// </summary>
		public static Vector2 DirectionFrom(this Vector2 From,Vector2 Source){
			return Vector2.Normalize(From - Source);
		}
		/// <summary>
		/// check if position is in range with another position
		/// </summary>
		public static bool InRange(this Vector2 From,Vector2 Target, float MaxRange){
			return Vector2.DistanceSquared(From, Target) <= MaxRange * MaxRange;
		}
		/// <summary>
		/// Resize Rectangle
		/// </summary>
		public static Rectangle Resize(this Rectangle rec, int x) {
			rec.X -= x;
			rec.Y -= x;
			rec.Width += x*2;
			rec.Height += x*2;
			return rec;
		}
		/// <summary>
		/// A quick way for getting item default. im too lazy to type
		/// </summary>
		public static Item ItemDefault(int id, int stack = 1,bool prefix = false) {
			Item item = new Item();
			item.SetDefaults(id);
			if (prefix) {
				item.Prefix(-1);
			}
			item.stack = stack;
			return item;
		}
		/// <summary>
		/// A quick way for getting npc default. im too lazy to type
		/// </summary>
		public static NPC NPCDefault(int id) {
			NPC item = new NPC();
			item.SetDefaults(id);
			return item;
		}
		/// <summary>
		/// get player lerp value. ported from 1.4 utils
		/// </summary>
		public static float GetLerpValue(float from, float to, float t, bool clamped = false){
			if (clamped){
				if (from < to){
					if (t < from){return 0f;}
					if (t > to){return 1f;}
				}
				else{
					if (t < to){return 1f;}
					if (t > from){return 0f;}
				}
			}
			return (t - from) / (to - from);
		}

		public static Vector2 MeasureString(this string text,DynamicSpriteFont font = null) {
			if (font == null) {font = FontAssets.MouseText.Value;}
			TextSnippet[] snippets = ChatManager.ParseMessage(text, Color.White).ToArray();
			return ChatManager.GetStringSize(font, snippets, Vector2.One);
		}

		/// <summary>
		/// for those who are lazy to cast int :amogus:
		/// </summary>
		public static Rectangle QuickRec(this Vector2 pos, Vector2 size) {
			return new Rectangle((int)pos.X,(int)pos.Y,(int)size.X,(int)size.Y);
		}
		/// <summary>
		/// get the rectangle of a texture
		/// this is centered
		/// </summary>
		public static Rectangle getRect(this Texture2D texture2, Vector2 pos) {
			return new Rectangle((int)pos.X - texture2.Width/2,(int)pos.Y - texture2.Width/2,texture2.Width,texture2.Height);
		}
		/// <summary>
		/// save a config
		/// </summary>
		public static void SaveConfig<T>() where T : ModConfig{
			typeof(ConfigManager).GetMethod("Save", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[1] { ModContent.GetInstance<T>()});
		}
		/// <summary>
		/// save int32 array
		/// </summary>
		public static void AddIntArray(this TagCompound tag, int[] array, string name = "array") {
			for (int i = 0; i < array.Length; i++){
				tag.Add(name+i,array[i]);
			}
		}
		/// <summary>
		/// load int32 array
		/// </summary>
		public static int[] GetIntArray(this TagCompound tag, int length, string name = "array") {
			int[] num = new int[length];
			for (int i = 0; i < length; i++){
				num[i] = tag.GetInt(name+i);
			}
			return num;
		}
		/// <summary>
		/// Read intreger array
		/// </summary>
		public static int[] ReadArrayInt32(this BinaryReader reader) {
			int length = reader.ReadInt32();
			int[] array = new int[length];
			for (int i = 0; i < length; i++)
			{
				array[i] = reader.ReadInt32();
			}
			return array;
		}
		/// <summary>
		/// Write intreger array
		/// </summary>
		public static void WriteArrayInt32(this BinaryWriter writer, int[] array) {
			writer.Write(array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				writer.Write(array[i]);
			}
		}
		/// <summary>
		/// Center a pos from entity
		/// </summary>
		public static Vector2 Center(this Entity entity, Vector2 pos) {
			return new Vector2(pos.X + (float)(entity.width / 2), pos.Y + (float)(entity.height / 2));
		}

		/// <summary>
		/// The sum of Vector2 X and Y without negative value
		/// </summary>
		public static float FloatCount(this Vector2 pos) {
			return (pos.X > 0 ? pos.X : pos.X*-1) + (pos.Y > 0 ? pos.Y : pos.Y*-1);
		}

		/// <summary>
		/// no more funny array haha
		/// </summary>
		public static string NextString(this UnifiedRandom rand,params string[] args) => rand.NextPart<string>(args);
		/// <summary>
		/// no more funny array haha
		/// </summary>
		public static T NextPart<T>(this UnifiedRandom rand,params T[] args) => rand.Next(args);

		/// <summary>
		/// Check for item stack and Delete item from inventory
		/// </summary>
		public static bool QuickRemoveItem(this Player player, int id, int amount = 1) {
			int stack = player.GetItemStack(id);
			if (stack < amount) {return false;}
			player.DeleteItem(id,amount);
			return true;
		}

		/// <summary>
		/// a shorthand for AddBuff without using ModContent
		/// </summary>
		public static void AddBuff<T> (this Player player,int time) where T : ModBuff {
			player.AddBuff(ModContent.BuffType<T>(),time);
		}
		/// <summary>
		/// a shorthand for AddBuff without using ModContent
		/// </summary>
		public static void AddBuff<T> (this NPC player,int time) where T : ModBuff {
			player.AddBuff(ModContent.BuffType<T>(),time);
		}
		
		/// <summary>
		/// Get Item stack from player inventory
		/// </summary>
		public static int GetItemStack(this Player player, int id) {
			int stack = 0;
			foreach (var item in player.inventory){if (item.type == id) {stack += item.stack;}}
			return stack;
		}

		/// <summary>
		/// a shorter short hand for calling Vector2.Distance ( i am the most lazy coder ever )
		/// </summary>
		public static float Distance(this Vector2 pos,Vector2 to) => Vector2.Distance(pos,to);

		/// <summary>
		/// Delete item from player inventory
		/// </summary>
		public static void DeleteItem(this Player player, int id, int amount = 1) {
			int deleted = 0;
			foreach(var item in player.inventory) {
				if (deleted >= amount) {
					break;
				}
				if (item != null && item.type == id) {
					if (amount == 1) {
						if (item.stack > 1) {item.stack -= 1;}
						else {item.TurnToAir();}
						deleted = amount;
						break;
					}
					else {
						if (item.stack == (amount - deleted)) {
							deleted += item.stack;
							item.TurnToAir();
							break;
						}
						else if (item.stack < (amount - deleted)) {
							deleted += item.stack;
							item.TurnToAir();
						}
						else if (item.stack > (amount - deleted)) {
							int a = amount - deleted;
							deleted += item.stack;
							item.stack -= a;
						}
					}
				}
				if (deleted >= amount) {
					break;
				}
			}
		}

        public static Rectangle GetHorizontalFrame(this Texture2D texture, int frame, int maxFrame)
		{
			int frameWidth = texture.Width / maxFrame;
			int startX = frameWidth * frame;
			return new Rectangle(startX, 0, frameWidth, texture.Height);
		}

		public static Rectangle GetVerticalFrame(this Texture2D texture, int frame, int maxFrame)
		{
			int frameHeight = texture.Height / maxFrame;
			int startY = frameHeight * frame;
			return new Rectangle(0, startY, texture.Width, frameHeight);
		}

		public static void DrawInventory(SpriteBatch spriteBatch,Vector2 position , Color drawColor,Texture2D texture, int frameCount,float scaleMult = 1f)
		{
			if (frameCount > 0)
			{
				// Pulled it outta my ass
				int animFrame = Helpme.MagicallyGetFrame(frameCount,6);
				float adjustedScale = 1f;
				// prevents 0 division error if its happen somehow
				Rectangle newFrame = new Rectangle(1,1,1,1);

				if (texture.Width > texture.Height){
					newFrame = texture.GetHorizontalFrame(animFrame,frameCount);
					adjustedScale = Math.Min(1f, 52f / newFrame.Height); 
				}
				else 
				{
					newFrame = texture.GetVerticalFrame(animFrame,frameCount);
					adjustedScale = Math.Min(1f, 52f / newFrame.Width); 
				}

				spriteBatch.Draw(texture,position,newFrame,drawColor,0f,newFrame.Size() / 2f,adjustedScale * scaleMult, SpriteEffects.None, 0f);
			}
			else 
			{
				float adjustedScale = Math.Min(1f, 52f / texture.Height);
				spriteBatch.Draw(texture,position,null,drawColor,0f,texture.Size()/2f,adjustedScale * scaleMult, SpriteEffects.None, 0f);
			}
		}



		/// <summary>
		/// Cycle between color smoothly
		/// </summary>
		public static Color CycleColor(params Color[] color) {
			float fade = Main.GameUpdateCount % 60 / 60f;
			int index = (int)(Main.GameUpdateCount / 60 % color.Length);
			return Color.Lerp(color[index], color[(index + 1) % color.Length], fade);
		}
		/// <summary>
		/// Cycle between color smoothly with update parameter
		/// </summary>
		public static Color CycleColor(int update,params Color[] color) {
			float fade = Main.GameUpdateCount % update / (float)update;
			int index = (int)(Main.GameUpdateCount / update % color.Length);
			return Color.Lerp(color[index], color[(index + 1) % color.Length], fade);
		}
		/// <summary>
		/// create a line of dust
		/// </summary>
		public static void DustLine(this Vector2 dustPos,Vector2 pos,int dust, bool gravity = true) {
			while (dustPos.Distance(pos) > 20f){
				Dust d = Dust.NewDustPerfect(dustPos, dust, Vector2.Zero);
				d.noGravity = gravity;
				dustPos += dustPos.DirectionTo(pos)*5f;
			}
		}
		/// <summary>
		/// A quick way getting tmod methodbase
		/// </summary>
		public static MethodBase GetModMethod(string loader,string method) {
			return typeof(Mod).Assembly.GetType("Terraria.ModLoader."+loader).GetMethod(method, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
		}
		/// <summary>
		/// Check if this type has an attribute
		/// </summary>
		public static bool HasAttribute<T>(this Type type) => Attribute.GetCustomAttribute(type,typeof(T)) != null;
    }
}