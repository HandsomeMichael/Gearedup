using System.Collections.Generic;
using Gearedup.Helper;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup.Content.Items.CalamityNoFargo
{
    public abstract class CalAccesory : ModItem
    {
        public override bool IsLoadingEnabled(Mod mod)
        {
            return Gearedup.Get.calamityMod != null;
        }

        public override void Load()
        {
            Gearedup.Log("[ Gearedup Load ] Loading CalAccesory : "+Name);
        }

        public virtual string[] Combined => [];
        public virtual string[] CombinedTooltips => null;
        public virtual bool UseTooltips => true;

        public override void SetDefaults()
        {
            Item.width = 10;
            Item.height = 10;
            Item.accessory = true;
            MoreDefaults();
        }

        public virtual void MoreDefaults()
        {

        }


        public virtual void MoreRecipes(Recipe recipe)
        {

        }

        public virtual void MoreUpdate(Player player, bool hideVisual)
        {

        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {

            if (!UseTooltips) return;

            if (CombinedTooltips != null)
            {
                foreach (var i in CombinedTooltips)
                {
                    Gearedup.Get.calamityMod.Item_UpdateTooltipline(i, tooltips);
                }
            }
            else
            {
                foreach (var i in Combined)
                {
                    Gearedup.Get.calamityMod.Item_UpdateTooltipline(i, tooltips);
                }
            }
        }
        public override void AddRecipes()
        {
            var recipe = Recipe.Create(Type);
            foreach (var i in Combined)
            {
                recipe.AddModIngredient(Gearedup.Get.calamityMod, i);
            }
            MoreRecipes(recipe);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            foreach (var i in Combined)
            {
                Gearedup.Get.calamityMod.Item_UpdateAccessory(i, player, hideVisual);
            }
            MoreUpdate(player, hideVisual);
        }
    }

    public class PrimateGifts : CalAccesory
    {
        public override string[] Combined => new string[]
        {
            "LuxorsGift","FungalSymbiote","AmidiasSpark","GladiatorsLocket","UnstableGraniteCore"
        };

        public override void MoreDefaults()
        {
            Item.defense = 2;
        }

        public override void MoreRecipes(Recipe recipe)
        {
            recipe.AddModIngredient(Gearedup.Get.calamityMod, "CryonicBar", 15);
            recipe.AddIngredient(ItemID.SoulofFright, 10);
            recipe.AddIngredient(ItemID.SoulofMight, 10);
            recipe.AddIngredient(ItemID.SoulofSight, 10);
        }
    }

    public class HeartsOfHero : CalAccesory
    {
        public override string[] Combined => new string[]
        {
            "HeartoftheElements","FungalClump","HowlsHeart"
        };
        public override void MoreRecipes(Recipe recipe)
        {
            recipe.AddIngredient(ItemID.LunarBar, 10);
        }
    }
    
    // public class PendantOfCatch : ModItem
    // {
    //     public override bool IsLoadingEnabled(Mod mod) => Gearedup.Get.calamityMod != null;

    //     public override void SetDefaults()
    //     {
    //         Item.width = 10;
    //         Item.height = 10;
    //         Item.accessory = true;
    //     }

    //     public override void ModifyTooltips(List<TooltipLine> tooltips)
    //     {
    //         Gearedup.Get.calamityMod.Item_UpdateTooltipline("EnchantedPearl", tooltips);
    //         Gearedup.Get.calamityMod.Item_UpdateTooltipline("FungalSymbiote", tooltips);
    //         Gearedup.Get.calamityMod.Item_UpdateTooltipline("AmidiasSpark", tooltips);
    //         Gearedup.Get.calamityMod.Item_UpdateTooltipline("UnstableGraniteCore", tooltips);
    //     }

    //     public override void UpdateAccessory(Player player, bool hideVisual)
    //     {
    //         Gearedup.Get.calamityMod.Item_UpdateAccessory("LuxorsGift", player, hideVisual);
    //         Gearedup.Get.calamityMod.Item_UpdateAccessory("FungalSymbiote", player, hideVisual);
    //         Gearedup.Get.calamityMod.Item_UpdateAccessory("AmidiasSpark", player, hideVisual);
    //         Gearedup.Get.calamityMod.Item_UpdateAccessory("UnstableGraniteCore", player, hideVisual);
    //     }

    //     public override void AddRecipes()
    //     {
    //         base.AddRecipes();
    //     }
    // }
}