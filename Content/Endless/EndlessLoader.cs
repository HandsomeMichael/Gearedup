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
            On_Main.MouseText_DrawItemTooltip_GetLinesInfo += PatchTooltip;
        }

        private void PatchTooltip(On_Main.orig_MouseText_DrawItemTooltip_GetLinesInfo orig, Item item, ref int yoyoLogo, ref int researchLine, float oldKB, ref int numLines, string[] toolTipLine, bool[] preFixLine, bool[] badPreFixLine, string[] toolTipNames, out int prefixlineIndex)
        {
            int resetNetID = 0;
            int resetTypeIDToo = 0;
            if (item.ModItem != null)
            {
                if (item.ModItem is EndlessThrowable throwable)
                {
                    if (throwable.throwType.id is int id)
                    {
                        resetNetID = item.netID;
                        item.netID = id;
                    }
                }
                else if (item.ModItem is AccesoryDuplicator accesoryDuplicator)
                {
                    if (accesoryDuplicator.ValidDuplicate())
                    {
                        resetTypeIDToo = item.type;
                        resetNetID = item.netID;
                        item.type = accesoryDuplicator.duplicate.type;
                        item.netID = accesoryDuplicator.duplicate.netID;    
                    }
                    // if (accesoryDuplicator.duplicate.id is int id)
                    // {
                    //     resetTypeIDToo = item.type;
                    //     resetNetID = item.netID;
                    //     item.type = id;
                    //     item.netID = id;
                    // }
                }
            }

            orig(item, ref yoyoLogo, ref researchLine, oldKB, ref numLines, toolTipLine, preFixLine, badPreFixLine, toolTipNames, out prefixlineIndex);

            if (resetNetID != 0)
            {
                item.netID = resetNetID;
            }
            if (resetTypeIDToo != 0)
            {
                item.type = resetTypeIDToo;
            }
        }
        
        // Might be needed later

        // public override void Load()
        // {
        //     On_Recipe.UpdateWhichItemsAreMaterials += MaterialPatch;
        // }

        // private void MaterialPatch(On_Recipe.orig_UpdateWhichItemsAreMaterials orig)
        // {
        //     // Prevents the player from thinking there is more to this accesory ( there is no bru )
        //     int accesoryDuplicatorID = ModContent.ItemType<AccesoryDuplicator>();

        //     for (int i = 0; i < Recipe.numRecipes; i++)
        //     {
        //         if (Main.recipe[i].Disabled) continue;
        //         if (Main.recipe[i].HasResult(accesoryDuplicatorID)) continue;

        //         foreach (Item item in Main.recipe[i].requiredItem)
        //         {
        //             ItemID.Sets.IsAMaterial[item.type] = true;
        //         }
        //     }

        //     // TODO: Check that each group is used at least once?
        //     foreach (RecipeGroup recipeGroup in RecipeGroup.recipeGroups.Values)
        //     {
        //         foreach (var item in recipeGroup.ValidItems)
        //         {
        //             ItemID.Sets.IsAMaterial[item] = true;
        //         }
        //     }

        //     // These come from the removed Item.checkMat. Coins and Void Bag. Void Bag is removed since it would be odd to have the Material tooltip appear on the open bag and not be present on the closed bag
        //     ItemID.Sets.IsAMaterial[ItemID.CopperCoin] = false;
        //     ItemID.Sets.IsAMaterial[ItemID.SilverCoin] = false;
        //     ItemID.Sets.IsAMaterial[ItemID.GoldCoin] = false;
        //     ItemID.Sets.IsAMaterial[ItemID.PlatinumCoin] = false;

        //     ItemID.Sets.IsAMaterial[ItemID.VoidVault] = false;
        //     ItemID.Sets.IsAMaterial[ItemID.VoidLens] = false;
        //     ItemID.Sets.IsAMaterial[ItemID.ClosedVoidBag] = false;

        //     /*
        //     for (int i = 0; i < ItemID.Count; i++) {
        //         Item item = new Item();
        //         item.SetDefaults(i, noMatCheck: true);
        //         item.checkMat();
        //         ItemID.Sets.IsAMaterial[i] = item.material;
        //     }
        //     */
        // }

        public override void AddRecipes()
        {
            foreach (Item item in ContentSamples.ItemsByType.Values)
            {
                // consumable damaging item
                if (item.damage > 0 && item.consumable &&
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
                        rec.AddIngredient(AmmoPack.GetReqStack(item));
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
                        rec.AddOnCraftCallback(CraftEndlessAmmo);

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
                        rec.AddOnCraftCallback(CraftEndlessThrow);
                        // we do shit before registering i think idk
                        var throwable = rec.createItem.ModItem as EndlessThrowable;
                        throwable.throwType = new TypeID(item);
                        throwable.ReloadDefaults();


                        rec.Register();
                    }
                }
            }
        }

        private void CraftEndlessThrow(Recipe recipe, Item item, List<Item> consumedItems, Item destinationStack)
        {
            if (consumedItems != null && consumedItems.Count > 0 && consumedItems[0].useTime > 0 && destinationStack.ModItem is EndlessThrowable et)
            {   
                if (et.throwType.InvalidData())
                {
                    et.throwType.SetTo(consumedItems[0]);
                    Main.NewText("Invalid data, magic storage issue fix");
                }
            }
        }
        
        private void CraftEndlessAmmo(Recipe recipe, Item item, List<Item> consumedItems, Item destinationStack)
        {
            if ( consumedItems != null && consumedItems.Count > 0 && consumedItems[0].ammo > 0 && destinationStack.ModItem is AmmoPack et)
            {
                if (et.ammoType.InvalidData())
                {
                    et.ammoType.SetTo(consumedItems[0]);
                    Main.NewText("Invalid data, magic storage issue fix");
                }
            }
        }

        public static void DrawUnloaded(SpriteBatch spriteBatch, Vector2 pos)
        {
            const float scale = 0.8f;
            Texture2D extraTexture = ModContent.Request<Texture2D>("Gearedup/Content/Endless/Unloaded").Value;
            Vector2 orig = extraTexture.Size() * scale / 2f;
            spriteBatch.Draw(extraTexture, pos, null, Color.White, 0f, orig, scale, SpriteEffects.None, 0f);
        }

        // private void OnCraftAccesoryDuplicator(Recipe recipe, Item item, List<Item> consumedItems, Item destinationStack)
        // {
        //     foreach (var ingredient in consumedItems)
        //     {
        //         if (ingredient.type != ModContent.ItemType<AccesoryDuplicator>())
        //         {
        //             destinationStack.Prefix(ingredient.prefix);
        //             return;
        //         }
        //     }
        // }

        // private void NoConsumeAccesory(Recipe recipe, int type, ref int amount, bool isDecrafting)
        // {
        //     if (!isDecrafting)
        //     {
        //         var item = ContentSamples.ItemsByType[type];
        //         if (item.accessory && item.type != ModContent.ItemType<AccesoryDuplicator>())
        //         {
        //             amount = 0;
        //         }
        //     }
        // }
    }
}