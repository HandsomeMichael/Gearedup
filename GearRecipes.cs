using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup
{
    public class GearRecipe : ModSystem
    {
        public override void AddRecipes()
        {
            var recipe = Recipe.Create(ItemID.MasterBait, 1);
            recipe.AddIngredient(ItemID.JourneymanBait, 5);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();

            recipe = Recipe.Create(ItemID.JourneymanBait, 1);
            recipe.AddIngredient(ItemID.ApprenticeBait, 5);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();

            recipe = Recipe.Create(ItemID.ApprenticeBait, 2);
            recipe.AddIngredient(ItemID.RottenChunk, 4);
            recipe.AddIngredient(ItemID.DirtBlock, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();

            recipe = Recipe.Create(ItemID.ApprenticeBait, 2);
            recipe.AddIngredient(ItemID.Vertebrae, 4);
            recipe.AddIngredient(ItemID.DirtBlock, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();

            recipe = Recipe.Create(ItemID.WaterBucket);
            recipe.AddIngredient(ItemID.EmptyBucket);
            recipe.AddIngredient(ItemID.BottomlessBucket);
            recipe.DisableDecraft();
            recipe.AddConsumeIngredientCallback(BottomlessConsume);
            recipe.Register();

            recipe = Recipe.Create(ItemID.HoneyBucket);
            recipe.AddIngredient(ItemID.EmptyBucket);
            recipe.AddIngredient(ItemID.BottomlessHoneyBucket);
            recipe.DisableDecraft();
            recipe.AddConsumeIngredientCallback(BottomlessConsume);
            recipe.Register();

            recipe = Recipe.Create(ItemID.LavaBucket);
            recipe.AddIngredient(ItemID.EmptyBucket);
            recipe.AddIngredient(ItemID.BottomlessLavaBucket);
            recipe.DisableDecraft();
            recipe.AddConsumeIngredientCallback(BottomlessConsume);
            recipe.Register();

        }

        private void BottomlessConsume(Recipe recipe, int type, ref int amount, bool isDecrafting)
        {
            // bottomless bucket will not be consumed
            if (!isDecrafting)
            {
                if (type != ItemID.EmptyBucket)
                {
                    amount = 0;
                }
            }
        }
    }
}