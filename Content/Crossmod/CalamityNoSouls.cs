using Gearedup.Helper;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup.Content.Crossmod
{
    public abstract class CalAccesory : ModItem
    {
        // public override bool IsLoadingEnabled(Mod mod)
        // {
        //     // return Gearedup.Get.calamityMod != null;
        // }

        public override void Load()
        {
            Gearedup.Log("[ Cal Accesory Module ] Loading CalAccesory : "+Name);
        }

        public override bool IsLoadingEnabled(Mod mod)
        {
            return ModLoader.HasMod("CalamityMod");
        }

        public virtual string[] Combined => [];
        // public virtual bool UseTooltips => true;

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

        public override void AddRecipes()
        {
            if (Gearedup.Get.calamityMod == null) return;
            // if (Gearedup.Get.fargoSoul != null) return;

            var recipe = Recipe.Create(Type);
            foreach (var i in Combined)
            {
                recipe.AddModIngredient(Gearedup.Get.calamityMod, i);
            }
            MoreRecipes(recipe);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }

        public override void UpdateVanity(Player player)
        {
            foreach (var i in Combined)
            {
                Gearedup.Get.calamityMod.Item_UpdateVanityAccessory(i, player);
            }
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
            "LuxorsGift","AmidiasSpark","GladiatorsLocket","UnstableGraniteCore","TrinketofChi"
        };

        public override void MoreUpdate(Player player, bool hideVisual)
        {
            if (player.TryGetModPlayer(out GearPlayer gp))
            {
                gp.primateGift = true;
            }
        }
        public override void MoreDefaults()
        {
            Item.rare = ItemRarityID.Master;
            Item.defense = 2;
            Item.master = true;
        }

        public override void MoreRecipes(Recipe recipe)
        {
            recipe.AddModIngredient(Gearedup.Get.calamityMod, "FungalSymbiote");
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
            "HeartoftheElements","ProfanedSoulArtifact","DaawnlightSpiritOrigin","FungalClump","HowlsHeart","WulfrumBattery"
        };
        public override void MoreDefaults()
        {
            Item.rare = ItemRarityID.Expert;
        }
        public override void MoreUpdate(Player player, bool hideVisual)
        {
            if (player.TryGetModPlayer(out GearPlayer gp))
            {
                gp.heartsOfHero = true;
            }

            // duplicate bone helm because i dont fucking know how it works
            // player.ApplyEquipFunctional(ContentSamples.ItemsByType[ItemID.BoneHelm], hideVisual);
        }
        public override void MoreRecipes(Recipe recipe)
        {
            // recipe.AddIngredient(ItemID.BoneHelm);
            recipe.AddModIngredient(Gearedup.Get.calamityMod, "RuinousSoul", 5);
            recipe.AddModIngredient(Gearedup.Get.calamityMod, "GalacticaSingularity", 5);
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