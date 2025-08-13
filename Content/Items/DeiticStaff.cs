using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup.Content.Items
{
    public class DeiticStaff : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;

            Item.useTime = 28;
            Item.useAnimation = 28;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item20;

            
            Item.shoot = ProjectileID.PurpleLaser;
            Item.shootSpeed = 12f;
        }

        public override bool AltFunctionUse(Player player) => true;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.FallenStar, 10);
            recipe.AddIngredient(ItemID.LunarBar, 10);
            recipe.AddTile(TileID.ShimmerMonolith);
            recipe.Register();
        }
    }
}