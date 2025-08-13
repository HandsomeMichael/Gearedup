using System.Collections.Generic;
using System.IO;
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

		public override void RightClick(Player player)
		{
			if (Main.mouseItem != null && Main.mouseItem.dye > 0)
			{
				dyeItemID = Main.mouseItem.type;
				Item.dye = Main.mouseItem.dye;
				unloaded = false;
			}
		}

        public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			if (unloaded)
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
			UpdateAccessory(player,true);
        }
		
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			if (player == null || !player.active) return;
			player.TryGetModPlayer<GearPlayer>(out GearPlayer modPlayer);
			if (modPlayer == null) return;

			modPlayer.universalDye = (short)Item.dye;
		}

        public override void AddRecipes()
        {
            CreateRecipe()
				.AddIngredient(ItemID.IronBar,10)
				.AddIngredient(ItemID.Star,4)
				.AddTile(TileID.DyeVat)
				.Register();
        }
    }
}