using System.Collections.Generic;
using System.IO;
using Gearedup.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Gearedup.Content.Items
{
    public class UniversalDyer : ModItem
	{
		public int dyeItemID;
		bool unloaded;
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 38;
			Item.value = 1000;
			Item.accessory = true;
			Item.rare = ItemRarityID.LightRed;
			Item.dye = 0;
			Item.consumable = false;
		}

        public override void SaveData(TagCompound tag)
        {
			tag.Add("dye", dyeItemID);
        }

		public override void LoadData(TagCompound tag)
		{
			dyeItemID = tag.GetInt("dye");

			if (ContentSamples.ItemsByType.TryGetValue(dyeItemID, out Item dyeItem) && dyeItem.dye > 0)
			{
				Item.dye = dyeItem.dye;
			}
			else
			{
				unloaded = true;
			}
        }

        public override void NetSend(BinaryWriter writer)
        {
			writer.Write(dyeItemID);
        }

		public override void NetReceive(BinaryReader reader)
		{
			dyeItemID = reader.ReadInt32();

			if (ContentSamples.ItemsByType.TryGetValue(dyeItemID, out Item dyeItem) && dyeItem.dye > 0)
			{
				Item.dye = dyeItem.dye;
			}
			else
			{
				unloaded = true;
			}
        }

		// public override void RightClick(Player player)
		// {
		// 	if (Main.mouseItem != null && Main.mouseItem.dye > 0)
		// 	{
		// 		dyeItemID = Main.mouseItem.type;
		// 		Item.dye = Main.mouseItem.dye;
		// 		unloaded = false;
		// 	}
		// }
		public void CopyDyes(Item dye)
		{
			dyeItemID = dye.type;
			Item.dye = dye.dye;
			unloaded = false;
		}

        public override bool ConsumeItem(Player player)
        {
			return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			if (dyeItemID == 0)
			{
				var tt = new TooltipLine(Mod, "DyedNone", "Currently not imbued with any dyes");
				tt.OverrideColor = Color.LightPink;
				tooltips.Add(tt);
			}
			else if (unloaded)
			{
				tooltips.Add(new TooltipLine(Mod, "Dyed", "Imbued dye didnt properly load, please replace it with another one"));
			}
			else
			{
				var sample = ContentSamples.ItemsByType[dyeItemID];
				tooltips.Add(new TooltipLine(Mod, "Dyed", $"Imbued with {sample.Name} [i:{dyeItemID}]"));
			}
		}
		

        public override void UpdateVanity(Player player)
		{
			UpdateAccessory(player, true);
		}
		
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			if (player == null || !player.active) return;
			player.TryGetModPlayer<GearPlayer>(out GearPlayer modPlayer);
			if (modPlayer == null) return;

			modPlayer.universalDye = Item.dye;
		}

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			spriteBatch.BeginDyeShader(Item.dye,Item,true,true);
			spriteBatch.Draw(tex,position,frame,drawColor,0f,origin,scale, SpriteEffects.None, 0f);
			spriteBatch.BeginNormal(true,true);
			
			return false;
        }

        public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.BlackDye, 1)
				.AddIngredient(ItemID.SoulofNight, 10)
				.AddIngredient(ItemID.SoulofLight, 10)
				.AddIngredient(ItemID.FallenStar, 5)
				.AddTile(TileID.DyeVat)
				.Register();
		}
    }
}