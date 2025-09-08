using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Gearedup.Helper
{
    public static class ItemHelper
    {
        public static Recipe AddModIngredient(this Recipe recipe, Mod mod, string name, int count = 1)
        {
            if (mod != null)
            {
                if (mod.TryFind(name, out ModItem modItem))
                {
                    recipe.AddIngredient(modItem.Type, count);
                }
            }
            return recipe;
        }

        public static int ItemType(this Mod mod, string name)
        {
            if (mod != null)
            {
                if (mod.TryFind(name, out ModItem modItem))
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
                if (mod.TryFind(name, out ModItem modItem))
                {
                    modItem.UpdateAccessory(player, hideVisual);
                }
            }
        }
        
        public static void Item_UpdateVanityAccessory(this Mod mod, string name, Player player)
        {
            if (mod != null)
            {
                if (mod.TryFind(name, out ModItem modItem))
                {
                    modItem.UpdateVanity(player);
                }
            }
        }
        
        public static void Item_UpdateTooltipline(this Mod mod, string name, List<TooltipLine> tooltip)
        {
            if (mod != null)
            {
                if (mod.TryFind(name, out ModItem modItem))
                {
                    tooltip.Add(new TooltipLine(mod, name, modItem.Tooltip.Value));
                }
            }
        }
    }
}