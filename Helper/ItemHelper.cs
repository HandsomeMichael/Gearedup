using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Gearedup.Helper
{
    public static class ItemHelper
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

        public static int ItemType(this Mod mod, string name)
        {
            if (mod != null)
            {
                if (mod.TryFind<ModItem>(name, out ModItem modItem))
                {
                    return modItem.Type;
                }
            }
            return 0;
        }

        public static void Item_UpdateAccessory(this Mod mod, string name, Player player, bool hideVisual)
        {
            if (mod != null)
            {
                if (mod.TryFind<ModItem>(name, out ModItem modItem))
                {
                    modItem.UpdateAccessory(player, hideVisual);
                }
            }
        }
        
        public static void Item_UpdateTooltipline(this Mod mod, string name , List<TooltipLine> tooltip)
        {
            if (mod != null)
            {
                if (mod.TryFind<ModItem>(name, out ModItem modItem))
                {
                    tooltip.Add(new TooltipLine(mod, name, modItem.Tooltip.Value));
                }
            }
        }
    }
}