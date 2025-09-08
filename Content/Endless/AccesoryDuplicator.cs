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
            // Item.expert = true;
            Item.accessory = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "skibd", "Use this to craft and duplicate accesory\nDoes nothing on its own"));
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

        // protected override bool CloneNewInstances => true;
        public TypeID duplicate;

        public override ModItem Clone(Item newEntity)
        {
            AccesoryDuplicator obj = (AccesoryDuplicator)base.Clone(newEntity);
            obj.duplicate = duplicate;
            // obj.ReloadDefaults();
            return obj;
        }

        public void ReloadDefaults()
        {
            if (duplicate.id is int id)
            {
                Item.CloneDefaults(id);
                SetDefaults();
            }
            // else
            // {
            //     Mod.Call("Error", $"Failed to reload defaults for duplicate type: {duplicate.mod}.{duplicate.name}");
            // }
        }

        public override void LoadData(TagCompound tag)
        {
            duplicate.Load(tag);
            duplicate.ValidateAsItem(true);
            if (duplicate.id.HasValue)
            {
                ReloadDefaults();
                return;
            }
        }

        public override void SaveData(TagCompound tag)
        {
            duplicate.Save(tag);
        }

        public override void NetSend(BinaryWriter writer)
        {
            duplicate.NetSend(writer);
        }

        public override void NetReceive(BinaryReader reader)
        {
            duplicate.NetReceive(reader);
            duplicate.ValidateAsItem(true);
            if (duplicate.id.HasValue)
            {
                ReloadDefaults();
                return;
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

        public override void UpdateInfoAccessory(Player player)
        {
            if (duplicate.id is int id)
            {
                var equip = ContentSamples.ItemsByType[id];
                ItemLoader.UpdateInfoAccessory(equip, player);
            }
        }

        // public override void UpdateEquip(Player player)
        // {
        //     base.UpdateEquip(player);
        // }
        public override void UpdateInventory(Player player)
        {
            if (duplicate.id is int id)
            {
                var equip = ContentSamples.ItemsByType[id];
                ItemLoader.UpdateInventory(equip, player);
            }
        }

        public override void UpdateVanity(Player player)
        {
            if (duplicate.id is int id)
            {
                var equip = ContentSamples.ItemsByType[id];
                player.ApplyEquipVanity(equip);
                // player.ApplyEquipFunctional(equip, hideVisual);
            }
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            if (duplicate.id is int id)
            {
                var equip = ContentSamples.ItemsByType[id];
                player.ApplyEquipFunctional(equip, hideVisual);
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (duplicate.id is int id)
            {
                tooltips.Add(new TooltipLine(Mod, "skibd2", $"This item is a duplicate of [i:{id}]"));
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "skibd", "Use this to craft and duplicate accesory\nDoes nothing on its own"));
            }
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (duplicate.id is int id)
            {
                var drawItem = ContentSamples.ItemsByType[id];
                float sinWave = (float)Math.Sin(Main.GameUpdateCount * 0.1);
                // The magic numbers 3.5f and 3f are used to keep the scale positive and within a visually pleasing range as the sine wave oscillates.
                // Note : stfu
                ItemSlot.DrawItemIcon(drawItem, 31, spriteBatch, position, (sinWave + 3.5f) / 3f, 32f, Main.DiscoColor * 0.4f);
                ItemSlot.DrawItemIcon(drawItem, 31, spriteBatch, position, drawItem.scale, 32f, Color.White);
                return false;
            }
            return true;
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
}