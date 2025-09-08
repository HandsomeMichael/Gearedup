using System.Collections.Generic;
using System.Linq;
using Gearedup.Helper;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup.Content.Calamity
{
    public class GearCalamity : ModSystem
    {
        public override bool IsLoadingEnabled(Mod mod)
        {
            return ModLoader.HasMod("CalamityMod");
        }

        // edit lots of recipe lil boy
        public override void PostAddRecipes()
        {
            for (int i = 0; i < Recipe.numRecipes; i++)
            {
                Recipe recipe = Main.recipe[i];

                if (recipe.TryGetResult(Gearedup.Get.calamityMod.ItemType("GrandGelatin"), out _))
                {
                    recipe.RemoveIngredient(ItemID.SoulofLight);
                    recipe.RemoveIngredient(ItemID.SoulofNight);

                    recipe.AddModIngredient(Gearedup.Get.calamityMod, "PurifiedGel", 15);
                }

                if (recipe.TryGetResult(Gearedup.Get.calamityMod.ItemType("TracersCelestial"), out _))
                {
                    recipe.AddIngredient(ItemID.HellfireTreads);
                }

                if (recipe.TryGetResult(Gearedup.Get.calamityMod.ItemType("AngelTreads"), out _))
                {
                    recipe.AddIngredient(ItemID.Magiluminescence);
                }


                // All recipes that require wood will now need 100% more
                // if (recipe.TryGetIngredient(ItemID.Wood, out Item ingredient)) {
                // 	ingredient.stack *= 2;
                // }
            }
            // that jellyfish recipe
            // bloodmoon summon -> blood orb
            // bloodmoon summon requires blood orb , gel and leather
        }
    }

    public abstract class ModItemPatch : GlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.ModItem != null && entity.ModItem.Mod.Name == ModName && ItemName == entity.ModItem.Name;
        }
        public virtual string ItemName => "cum";
        public virtual string ModName => "CalamityMod";

        public override bool IsLoadingEnabled(Mod mod)
        {
            return ModLoader.HasMod(ModName);
        }
    }

    public abstract class ModItemPatchMultiple : GlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.ModItem != null && entity.ModItem.Mod.Name == ModName && ItemName.Contains(entity.ModItem.Name);
        }
        public virtual string[] ItemName => new string[] { "cum" };
        public virtual string ModName => "CalamityMod";

        public override bool IsLoadingEnabled(Mod mod)
        {
            return ModLoader.HasMod(ModName);
        }
    }

    public class EatableBloodOrb : ModItemPatch
    {
        public override string ItemName => "BloodOrb";
        public override string ModName => "CalamityMod";
        public override void SetDefaults(Item entity)
        {
            entity.healLife = 125;
            entity.buffType = BuffID.Bleeding;
            entity.buffTime = 60 * 60; // 1 minute of bleeding
            entity.useAnimation = 20;
            entity.useTime = 20;
            entity.useStyle = ItemUseStyleID.EatFood;

            entity.ammo = entity.type;
            entity.notAmmo = true;
            entity.consumable = true;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "battle", "Grant 'Battle' effect during Hardmode or Bloodmoon"));
        }

        public override bool? UseItem(Item item, Player player)
        {
            // during bloodmoon or hardmode would apply battle potion effect
            if (Main.hardMode || Main.bloodMoon)
            {
                player.AddBuff(BuffID.Battle, 60 * 80);
            }
            return true;
        }
    }

    public class TracersCelestialPatch : ModItemPatchMultiple
    {
        public override string[] ItemName => ["TracersElysian", "TracersCelestial", "TracersSeraph",];
        public override string ModName => "CalamityMod";

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "hellfireBonus", "Effect of Hellfire Threads & Magiluminescence"));
        }

        public override void UpdateVanity(Item item, Player player)
        {
            player.ApplyEquipVanity(ContentSamples.ItemsByType[ItemID.HellfireTreads]);
        }
        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            player.hasMagiluminescence = true;
            player.ApplyEquipFunctional(ContentSamples.ItemsByType[ItemID.HellfireTreads], hideVisual);
        }
    }

    public class AngelTreads : ModItemPatch
    {
        public override string ItemName => "AngelTreads";
        public override string ModName => "CalamityMod";

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "magilumenEff", "Effect of Magiluminescence"));
        }
        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            player.hasMagiluminescence = true;
        }
    }
    
    public class DodgeIncrease : ModItemPatch
    {
        public override bool IsLoadingEnabled(Mod mod)
        {
            return ModLoader.HasMod("CalamityMod") && ModLoader.HasMod("Dodgeroll");
        }
        public override string ItemName => "StatisNinjaBelt";
        public override string ModName => "CalamityMod";

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "Dodgeroll", "Multiply dodge time by 2x"));
        }
        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            player.hasMagiluminescence = true;
        }
    }
}