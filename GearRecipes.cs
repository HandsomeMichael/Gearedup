using System;
using Gearedup.Content.Items;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup
{
    public class GearRecipe : ModSystem
    {
        public override void AddRecipes()
        {

            // Craftable baits
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

            // Buckets

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

            // Create bottomless stuff

            recipe = Recipe.Create(ItemID.BottomlessBucket);
            recipe.AddIngredient(ItemID.WaterBucket, 99);
            recipe.AddIngredient(ItemID.HallowedBar, 10);
            recipe.AddIngredient(ItemID.ChlorophyteBar, 8);
            recipe.AddIngredient<WhiteStar>(5);
            recipe.AddCondition(Condition.InBeach);
            recipe.DisableDecraft();
            recipe.Register();

            recipe = Recipe.Create(ItemID.BottomlessHoneyBucket);
            recipe.AddIngredient(ItemID.HoneyBucket, 99);
            recipe.AddIngredient(ItemID.HallowedBar, 10);
            recipe.AddIngredient(ItemID.ChlorophyteBar, 8);
            recipe.AddIngredient<WhiteStar>(5);
            recipe.AddCondition(Condition.InJungle);
            recipe.DisableDecraft();
            recipe.Register();

            recipe = Recipe.Create(ItemID.BottomlessLavaBucket);
            recipe.AddIngredient(ItemID.LavaBucket, 99);
            recipe.AddIngredient(ItemID.HallowedBar, 10);
            recipe.AddIngredient(ItemID.ChlorophyteBar, 8);
            recipe.AddIngredient<WhiteStar>(5);
            recipe.AddCondition(Condition.InUnderworld);
            recipe.DisableDecraft();
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