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
        public override void AddRecipes()
        {
            foreach (Item item in ContentSamples.ItemsByType.Values)
            {
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
                        rec.AddIngredient(item.type, Math.Min(item.maxStack, 3996) / 2);

                        // Hardmode ammo packs require more stuff
                        if (item.rare > ItemRarityID.Blue)
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
    }
}