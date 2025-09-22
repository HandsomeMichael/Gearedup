using System;
using System.Collections.Generic;
using System.IO;
using Gearedup.Content.Items;
using Gearedup.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Gearedup.Content.Endless
{
    /// <summary>
    /// Prevent recursive craft to crash
    /// </summary>
    public class AccesoryDuplicatorMat : ModItem
    {
        public override string Texture => "Gearedup/Content/Endless/AccesoryDuplicator";
        public override void SetDefaults()
        {
            Item.width = 10;
            Item.height = 10;
            Item.rare = ItemRarityID.Expert;
            //Item.consumable = false;
            Item.consumable = true;
            // Item.expert = true;
        }
        // public override bool ConsumeItem(Player player) => false;

        public override bool CanRightClick()
        {
            return Main.mouseItem != null && !Main.mouseItem.IsAir && Main.mouseItem.accessory;
        }

        public override void RightClick(Player player)
        {
            // duplicate it
            Item acc = new Item(ModContent.ItemType<AccesoryDuplicator>());
            //((AccesoryDuplicator)acc.ModItem).duplicate = new TypeID(Main.mouseItem);
            ((AccesoryDuplicator)acc.ModItem).SetTo(Main.mouseItem);
            // ((AccesoryDuplicator)acc.ModItem).ReloadDefaults();
            player.QuickSpawnClonedItemDirect(Item.GetSource_FromThis(), acc);

            // will this get sync ? who knows dawg. who the fuck knows

            // Item.TurnToAir(true);
            // Item.SetDefaults(ModContent.ItemType<AccesoryDuplicator>());
            // ((AccesoryDuplicator)Item.ModItem).duplicate = new TypeID(Main.mouseItem);
            // ((AccesoryDuplicator)Item.ModItem).ReloadDefaults();
            // Item.Prefix(Main.mouseItem.prefix);
            //((AccesoryDuplicator)Item.ModItem).duplicate
            // Item.NetStateChanged();

            CombatText.NewText(player.Hitbox, Color.White, "Duplicated " + Main.mouseItem.Name);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "skibd", "Hold an item and right-click this to duplicates its properties\nDoes nothing on its own"));
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Bone, 25)
                .AddIngredient<WhiteStar>(3)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }

    public class AccesoryDuplicator : ModItem
    {
        public Item duplicate;
        public void SetTo(Item item)
        {
            duplicate = item.Clone();
        }
        public override ModItem Clone(Item newEntity)
        {
            AccesoryDuplicator obj = (AccesoryDuplicator)base.Clone(newEntity);
            if (duplicate != null) { obj.duplicate = duplicate.Clone(); }
            else { obj.duplicate = null; }
            return obj;
        }

        public override void LoadData(TagCompound tag)
        {
            duplicate = tag.Get<Item>("duplicate");
        }

        public override void SaveData(TagCompound tag)
        {
            tag["duplicate"] = duplicate;
        }

        public override void NetSend(BinaryWriter writer)
        {
            if (duplicate != null)
            {
                writer.Write(true);
                ItemIO.Send(duplicate, writer);
            }
            else
            {
                writer.Write(false);
            }
        }

        public override void NetReceive(BinaryReader reader)
        {
            if (reader.ReadBoolean())
            {
                duplicate = ItemIO.Receive(reader);
            }
        }

        public override void SetDefaults()
        {
            Item.width = 10;
            Item.height = 10;
            Item.rare = ItemRarityID.Expert;
            // Item.expert = true;
            Item.accessory = true;
        }

        public bool ValidDuplicate()
        {
            if (duplicate == null) return false;
            if (duplicate.IsAir) return false;
            if (duplicate.ModItem is UnloadedItem) return false;
            return true;
        }
        public override void UpdateInfoAccessory(Player player)
        {
            if (ValidDuplicate())
            {
                ItemLoader.UpdateInfoAccessory(duplicate, player);
            }
        }

        public override void UpdateEquip(Player player)
        {
            if (ValidDuplicate())
            {
                ItemLoader.UpdateEquip(duplicate, player);
            }
        }
        public override void UpdateInventory(Player player)
        {
            if (ValidDuplicate())
            {
                ItemLoader.UpdateInventory(duplicate, player);
            }
        }

        public override void UpdateVanity(Player player)
        {
            if (ValidDuplicate())
            {
                player.ApplyEquipVanity(duplicate);
            }
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (ValidDuplicate())
            {
                player.ApplyEquipFunctional(duplicate, hideVisual);
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (duplicate != null)
            {
                if (duplicate.ModItem is UnloadedItem unloaded)
                {
                    tooltips.Add(new TooltipLine(Mod, "skibd", $"Duplicated properties not found\nName : {unloaded.ModName}\nFrom : {unloaded.ItemName}"));
                }
                else
                {
                    tooltips.Add(new TooltipLine(Mod, "skibd2", $"This item is a duplicate of [i:{duplicate.type}]"));
                }
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "skibd", "A duplicate vessel emptied by unknown force\nUse extractinator to retain its material back"));
            }
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (ValidDuplicate())
            {
                float sinWave = (float)Math.Sin(Main.GameUpdateCount * 0.1);
                // The magic numbers 3.5f and 3f are used to keep the scale positive and within a visually pleasing range as the sine wave oscillates.
                // Note : stfu
                ItemSlot.DrawItemIcon(duplicate, 31, spriteBatch, position, (sinWave + 3.5f) / 3f, 32f, Main.DiscoColor * 0.4f);
                ItemSlot.DrawItemIcon(duplicate, 31, spriteBatch, position, duplicate.scale, 32f, Color.White);
                return false;
            }
            // if data is not invalid, put a question mark on - redraw the shi
            spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value,position,null,drawColor,0f,origin,1f,SpriteEffects.None,0f);
            EndlessLoader.DrawUnloaded(spriteBatch, position);
            return false;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            // if (duplicate.id is int id)
            // {
            //     var drawItem = ContentSamples.ItemsByType[id];
            //     Main.DrawItemIcon(spriteBatch, drawItem, Item.position - Main.screenPosition, lightColor, scale);
            //     return false;
            // }
            return true;
        }

        // public override void OnCraft(Recipe recipe)
        // {
        //     base.OnCraft(recipe);
        // }
    }
    // UNUSED
    // public class AccesoryDuplicator : ModItem
    // {

    //     // protected override bool CloneNewInstances => true;
    //     public TypeID duplicate;

    //     public override ModItem Clone(Item newEntity)
    //     {
    //         AccesoryDuplicator obj = (AccesoryDuplicator)base.Clone(newEntity);
    //         obj.duplicate = duplicate;
    //         // obj.ReloadDefaults();
    //         return obj;
    //     }

    //     public void ReloadDefaults()
    //     {
    //         if (duplicate.id is int id)
    //         {
    //             Item.CloneDefaults(id);
    //             SetDefaults();
    //         }
    //         // else
    //         // {
    //         //     Mod.Call("Error", $"Failed to reload defaults for duplicate type: {duplicate.mod}.{duplicate.name}");
    //         // }
    //     }

    //     public override void LoadData(TagCompound tag)
    //     {
    //         duplicate.Load(tag);
    //         duplicate.ValidateAsItem(true);
    //         if (duplicate.id.HasValue)
    //         {
    //             ReloadDefaults();
    //             return;
    //         }
    //     }

    //     public override void SaveData(TagCompound tag)
    //     {
    //         duplicate.Save(tag);
    //     }

    //     public override void NetSend(BinaryWriter writer)
    //     {
    //         duplicate.NetSend(writer);
    //     }

    //     public override void NetReceive(BinaryReader reader)
    //     {
    //         duplicate.NetReceive(reader);
    //         duplicate.ValidateAsItem(true);
    //         if (duplicate.id.HasValue)
    //         {
    //             ReloadDefaults();
    //             return;
    //         }
    //     }
    //     public override void SetDefaults()
    //     {
    //         Item.width = 10;
    //         Item.height = 10;
    //         Item.rare = ItemRarityID.Expert;
    //         // Item.expert = true;
    //         Item.accessory = true;
    //     }

    //     public override void UpdateInfoAccessory(Player player)
    //     {
    //         if (duplicate.id is int id)
    //         {
    //             var equip = ContentSamples.ItemsByType[id];
    //             ItemLoader.UpdateInfoAccessory(equip, player);
    //         }
    //     }

    //     // public override void UpdateEquip(Player player)
    //     // {
    //     //     base.UpdateEquip(player);
    //     // }
    //     public override void UpdateInventory(Player player)
    //     {
    //         if (duplicate.id is int id)
    //         {
    //             var equip = ContentSamples.ItemsByType[id];
    //             ItemLoader.UpdateInventory(equip, player);
    //         }
    //     }

    //     public override void UpdateVanity(Player player)
    //     {
    //         if (duplicate.id is int id)
    //         {
    //             var equip = ContentSamples.ItemsByType[id];
    //             player.ApplyEquipVanity(equip);
    //             // player.ApplyEquipFunctional(equip, hideVisual);
    //         }
    //     }
    //     public override void UpdateAccessory(Player player, bool hideVisual)
    //     {
    //         if (duplicate.id is int id)
    //         {
    //             var equip = ContentSamples.ItemsByType[id];
    //             player.ApplyEquipFunctional(equip, hideVisual);
    //         }
    //     }

    //     public override void ModifyTooltips(List<TooltipLine> tooltips)
    //     {
    //         if (duplicate.id is int id)
    //         {
    //             tooltips.Add(new TooltipLine(Mod, "skibd2", $"This item is a duplicate of [i:{id}]"));

    //             if (duplicate.mod == "TerraRPG")
    //             {
    //                 tooltips.Add(new TooltipLine(Mod, "skibd2", "Duplication not rejoiced fully") {OverrideColor = Color.LightPink});
    //             }
    //             //var dupe = ContentSamples.ItemsByType[id];
    //         }
    //         else
    //         {
    //             if (duplicate.mod == null || duplicate.mod == "" || duplicate.name == null || duplicate.name == "")
    //             {
    //                 tooltips.Add(new TooltipLine(Mod, "skibd", "A duplicate vessel emptied by unknown force\nUse extractinator to retain its material back"));
    //             }
    //             else
    //             {
    //                 tooltips.Add(new TooltipLine(Mod, "skibd", $"Duplicated properties not found\nName : {duplicate.name}\nFrom : {duplicate.mod}"));
    //             }
    //         }
    //     }

    //     public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    //     {
    //         if (duplicate.id is int id)
    //         {
    //             var drawItem = ContentSamples.ItemsByType[id];
    //             float sinWave = (float)Math.Sin(Main.GameUpdateCount * 0.1);
    //             // The magic numbers 3.5f and 3f are used to keep the scale positive and within a visually pleasing range as the sine wave oscillates.
    //             // Note : stfu
    //             ItemSlot.DrawItemIcon(drawItem, 31, spriteBatch, position, (sinWave + 3.5f) / 3f, 32f, Main.DiscoColor * 0.4f);
    //             ItemSlot.DrawItemIcon(drawItem, 31, spriteBatch, position, drawItem.scale, 32f, Color.White);
    //             return false;
    //         }

    //         // if data is not invalid, put a question mark on
    //         else if (!duplicate.InvalidData())
    //         {
    //             // redraw the shi
    //             spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value,position,null,drawColor,0f,origin,1f,SpriteEffects.None,0f);

    //             EndlessLoader.DrawUnloaded(spriteBatch, position);
    //             return false;
    //         }

    //         return true;
    //     }

    //     public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    //     {
    //         // if (duplicate.id is int id)
    //         // {
    //         //     var drawItem = ContentSamples.ItemsByType[id];
    //         //     Main.DrawItemIcon(spriteBatch, drawItem, Item.position - Main.screenPosition, lightColor, scale);
    //         //     return false;
    //         // }
    //         return true;
    //     }

    //     // public override void OnCraft(Recipe recipe)
    //     // {
    //     //     base.OnCraft(recipe);
    //     // }
    // }
}