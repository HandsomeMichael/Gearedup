using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup.Content.Items
{
    public class OgreFeet : ModItem
    {
        public override void SetDefaults() 
		{
			Item.width = 32;
			Item.height = 38;
			Item.value = 1000;
			Item.accessory = true;
			Item.rare = ItemRarityID.Green;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) 
		{
			if (player == null || !player.active) return;
			player.TryGetModPlayer<GearPlayer>(out GearPlayer modPlayer);
			if (modPlayer == null) return;

            modPlayer.ogreFeet = true;
		}
    }
}