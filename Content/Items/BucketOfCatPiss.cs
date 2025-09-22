using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup.Content.Items
{
    public class BucketOfCatPiss : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 10;
            Item.height = 10;
            Item.value = 10000;
            Item.rare = ItemRarityID.Expert;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // increased damage by 8%
            player.GetDamage(DamageClass.Generic) += 0.8f;

            if (player.TryGetModPlayer(out GearPlayer gp))
            {
                gp.catPissed = true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.EmptyBucket)
                .AddIngredient(ItemID.BottomlessHoneyBucket)
                .AddIngredient(ItemID.LightShard, 2)
                .AddIngredient(ItemID.DarkShard, 2)
                .AddIngredient(ItemID.AvengerEmblem)
                .AddIngredient(ItemID.PoopBlock, 15)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
}