using System;
using System.Collections.Generic;
using Gearedup.Content.Items;
using Gearedup.Utils;
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
    public class AmmoPack : ModItem
    {

        // i didnt come up with the name, its from my cat kayy
        internal TypeID ammoType;

        public override void SaveData(TagCompound tag)
        {
            ammoType.Save(tag);
        }

        public override void LoadData(TagCompound tag)
        {
            ammoType.Load(tag);
            ammoType.ValidateAsItem();
            if (ammoType.id.HasValue)
            {
                ReloadDefaults();
                return;
            }
            Mod.Call("Error",$"Missing item for {ammoType.mod}:{ammoType.name} ");
        }

        public override ModItem Clone(Item newEntity)
        {
            AmmoPack obj = (AmmoPack)base.Clone(newEntity);
            obj.ammoType = ammoType;
            return obj;
        }

        public override void SetDefaults()
        {
            // i should do stuff but idk too lazy
            Item.width = 10;
            Item.height = 10;
        }

        public void ReloadDefaults()
        {
            if (ammoType.id is int id)
            {
                Item.CloneDefaults(id);
                Item.maxStack = 1;
                Item.consumable = false;
            }
            else
            {
                Mod.Call("Error", $"Failed to reload defaults for ammo type: {ammoType.mod}.{ammoType.name}");
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ammoType.id is int id)
            {
                foreach (TooltipLine tt in tooltips)
                {
                    if (tt.Mod == "Terraria" && tt.Name == "ItemName")
                    {
                        tt.Text = "Magical " + ContentSamples.ItemsByType[id].Name + " Pack";
                        break;
                    }
                }
            }
            else
            {
                foreach (TooltipLine tt in tooltips)
                {
                    if (tt.Mod == "Terraria" && tt.Name == "ItemName")
                    {
                        tt.Text = "Empty Magical Pack";
                        break;
                    }
                }

                tooltips.Insert(1, new TooltipLine(Mod, "tips", "Does nothing on its own..."));
                //tooltips.Add(new TooltipLine(Mod, "tips", "Does nothing on its own..."));
            }
        }

        // public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        // {
        //     return base.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        // }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Item.color = ItemRarity.GetColor(Item.rare);
            if (ammoType.id is int id)
            {
                Main.instance.LoadItem(id);
                var texture = Terraria.GameContent.TextureAssets.Item[id].Value;
                if (texture == null) return;

                // if it did have some sort of animation
                if (Main.itemAnimations[id] != null)
                {
                    // i found no other drawanimation 
                    if (Main.itemAnimations[id] is DrawAnimationVertical vertical)
                    {
                        Helpme.DrawInventory(spriteBatch, position, drawColor, texture, vertical.FrameCount, scale);
                        return;
                    }
                }

                Helpme.DrawInventory(spriteBatch, position, drawColor, texture, 0, scale);
            }
        }
    }

    public class EndlessLoader : ModSystem
    {
        public override void AddRecipes()
        {
            foreach (Item item in ContentSamples.ItemsByType.Values)
            {
                if (item.ammo > 0 && item.damage > 0 && !item.notAmmo && item.maxStack > 10)
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
            }
        }
    }
}