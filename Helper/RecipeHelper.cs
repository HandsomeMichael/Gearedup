using Terraria;
using Terraria.ModLoader;

namespace Gearedup.Helper
{
    public static class RecipeHelper
    {
        public static void AddModIngredient(this Recipe recipe, Mod mod, string name, int count = 1)
        {
            if (mod != null)
            {
                if (mod.TryFind<ModItem>(name, out ModItem modItem))
                {
                    recipe.AddIngredient(modItem.Type, count);
                }
            }
        }
    }
}