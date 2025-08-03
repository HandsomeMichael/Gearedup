using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup.Content.Items
{
    public class UniversalDyer : ModItem
	{
		public override void SetDefaults() 
		{
			Item.width = 32;
			Item.height = 38;
			Item.value = 1000;
			Item.accessory = true;
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) 
		{
			if (player == null || !player.active) return;
			player.TryGetModPlayer<GearPlayer>(out GearPlayer modPlayer);
			if (modPlayer == null) return;

			int b = 0;
			for (int i = 0; i < player.armor.Length; i++)
			{
				if (player.armor[i] == Item) 
				{
					b = i;
					break;
				}
			}
			modPlayer.universalDye = (short)player.dye[b].dye;
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