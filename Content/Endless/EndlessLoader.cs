using System;
using System.Collections.Generic;
using Gearedup.Content.Catched;
using Gearedup.Content.Items;
using Gearedup.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Gearedup.Content.Endless
{
    public class EndlessLoader : ModSystem
    {
        public override void Load()
        {
            On_Recipe.UpdateWhichItemsAreMaterials += MaterialPatch;
        }

        private void MaterialPatch(On_Recipe.orig_UpdateWhichItemsAreMaterials orig)
        {
            // Prevents the player from thinking there is more to this accesory ( there is no bru )
            int accesoryDuplicatorID = ModContent.ItemType<AccesoryDuplicator>();

            for (int i = 0; i < Recipe.numRecipes; i++)
            {
                if (Main.recipe[i].Disabled) continue;
                if (Main.recipe[i].HasResult(accesoryDuplicatorID)) continue;

                foreach (Item item in Main.recipe[i].requiredItem)
                {
                    ItemID.Sets.IsAMaterial[item.type] = true;
                }
            }

            // TODO: Check that each group is used at least once?
            foreach (RecipeGroup recipeGroup in RecipeGroup.recipeGroups.Values)
            {
                foreach (var item in recipeGroup.ValidItems)
                {
                    ItemID.Sets.IsAMaterial[item] = true;
                }
            }

            // These come from the removed Item.checkMat. Coins and Void Bag. Void Bag is removed since it would be odd to have the Material tooltip appear on the open bag and not be present on the closed bag
            ItemID.Sets.IsAMaterial[ItemID.CopperCoin] = false;
            ItemID.Sets.IsAMaterial[ItemID.SilverCoin] = false;
            ItemID.Sets.IsAMaterial[ItemID.GoldCoin] = false;
            ItemID.Sets.IsAMaterial[ItemID.PlatinumCoin] = false;

            ItemID.Sets.IsAMaterial[ItemID.VoidVault] = false;
            ItemID.Sets.IsAMaterial[ItemID.VoidLens] = false;
            ItemID.Sets.IsAMaterial[ItemID.ClosedVoidBag] = false;

            /*
            for (int i = 0; i < ItemID.Count; i++) {
                Item item = new Item();
                item.SetDefaults(i, noMatCheck: true);
                item.checkMat();
                ItemID.Sets.IsAMaterial[i] = item.material;
            }
            */
        }

        public override void AddRecipes()
        {
            foreach (Item item in ContentSamples.ItemsByType.Values)
            {
                // accesory duplicator
                if (item.accessory && item.createTile == -1 && item.type != ModContent.ItemType<AccesoryDuplicator>() && item.type != ModContent.ItemType<UniversalDyer>() )
                {
                    Recipe rec = Recipe.Create(ModContent.ItemType<AccesoryDuplicator>());

                    rec.AddIngredient(item.type);
                    rec.AddIngredient<AccesoryDuplicator>();
                    rec.AddConsumeIngredientCallback(NoConsumeAccesory);
                    rec.AddOnCraftCallback(OnCraftAccesoryDuplicator);
                    rec.AddTile(TileID.TinkerersWorkbench);
                    rec.DisableDecraft();

                    // we do shit before registering i think idk
                    var duplicator = rec.createItem.ModItem as AccesoryDuplicator;
                    duplicator.duplicate = new TypeID(item);
                    duplicator.ReloadDefaults();

                    rec.Register();
                }
                // consumable damaging item
                else if (item.damage > 0 && item.consumable &&
                     // No Infinite Coins For U
                     item.type != ItemID.GoldCoin && item.type != ItemID.SilverCoin &&
                      item.type != ItemID.PlatinumCoin && item.type != ItemID.CopperCoin &&
                      // Catched Projectile already had infinite variant
                      item.type != ModContent.GetInstance<CatchedProjectile>().Type
                      )
                {
                    if (item.ammo > 0 && !item.notAmmo)
                    {
                        Recipe rec = Recipe.Create(ModContent.ItemType<AmmoPack>());

                        // A bit more fair
                        rec.AddIngredient(item.type, Math.Min(item.maxStack, 3996) / 2);
                        rec.DisableDecraft();

                        // Hardmode ammo packs require more stuff
                        if (item.rare > ItemRarityID.Green)
                        {
                            rec.AddIngredient(ModContent.ItemType<WhiteStar>(), 1);
                        }
                        else
                        {
                            rec.AddIngredient(ItemID.FallenStar, 5);
                        }

                        rec.AddTile(TileID.Anvils);

                        // we do shit before registering i think idk
                        var ampack = rec.createItem.ModItem as AmmoPack;
                        ampack.ammoType = new TypeID(item);
                        ampack.ReloadDefaults();

                        rec.Register();
                    }
                    else if (item.useTime > 0)
                    {
                        Recipe rec = Recipe.Create(ModContent.ItemType<EndlessThrowable>());
                        // bit more fair aint it
                        rec.AddIngredient(item.type, Math.Min(item.maxStack, 3996) / 2);
                        rec.DisableDecraft();
                        rec.AddTile(TileID.Anvils);

                        // we do shit before registering i think idk
                        var throwable = rec.createItem.ModItem as EndlessThrowable;
                        throwable.throwType = new TypeID(item);
                        throwable.ReloadDefaults();

                        rec.Register();
                    }
                }
            }
        }

        private void OnCraftAccesoryDuplicator(Recipe recipe, Item item, List<Item> consumedItems, Item destinationStack)
        {
            foreach (var ingredient in consumedItems)
            {
                if (ingredient.type != ModContent.ItemType<AccesoryDuplicator>())
                {
                    destinationStack.Prefix(ingredient.prefix);
                    return;
                }
            }
        }

        private void NoConsumeAccesory(Recipe recipe, int type, ref int amount, bool isDecrafting)
        {
            if (!isDecrafting)
            {
                var item = ContentSamples.ItemsByType[type];
                if (item.accessory && item.type != ModContent.ItemType<AccesoryDuplicator>())
                {
                    amount = 0;
                }
            }
        }
    }
}