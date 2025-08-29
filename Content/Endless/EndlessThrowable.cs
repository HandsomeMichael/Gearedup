using System;
using System.Collections.Generic;
using System.IO;
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
using Terraria.UI;

namespace Gearedup.Content.Endless
{
    public class EndlessThrowable : ModItem
    {
        // i didnt come up with the name, its from my cat kayy
        internal TypeID throwType;
        public override string Texture => "Gearedup/Content/Placeholder";

        public override void SetStaticDefaults()
        {
            ItemID.Sets.CanGetPrefixes[Type] = true;
            ItemID.Sets.ShimmerCountsAsItem[Type] = -1;
            ItemID.Sets.ShimmerTransformToItem[Type] = -1;
        }

        // protected override bool CloneNewInstances => true;

        public override void UpdateInventory(Player player)
        {

            if (throwType.id is int validID)
            {
                var clone = ContentSamples.ItemsByType[validID];
                ItemLoader.UpdateInventory(clone, player);
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (throwType.id is int validID)
            {
                var clone = ContentSamples.ItemsByType[validID];
                return ItemLoader.Shoot(clone, player, source, position, velocity, type, damage, knockback);
            }
            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (throwType.id is int validID)
            {
                var clone = ContentSamples.ItemsByType[validID];
                ItemLoader.ModifyShootStats(clone, player, ref position, ref velocity, ref type, ref damage, ref knockback);
            }
        }

        public override void HoldItem(Player player)
        {
            if (throwType.id is int validID)
            {
                var clone = ContentSamples.ItemsByType[validID];
                ItemLoader.HoldItem(clone, player);
            }
        }

        public override bool AltFunctionUse(Player player)
        {
            if (throwType.id is int validID)
            {
                var clone = ContentSamples.ItemsByType[validID];
                return ItemLoader.AltFunctionUse(clone, player);
            }
            return base.AltFunctionUse(player);
        }

        // try reloading defaults if shit happen
        public override void UseAnimation(Player player)
        {
            if (!(throwType.id is int) && (throwType.mod == null || throwType.mod != "") && (throwType.name == null || throwType.name != ""))
            {
                ReloadDefaults();
            }
        }

        public override void SaveData(TagCompound tag)
        {
            throwType.Save(tag);
        }

        public override void NetSend(BinaryWriter writer)
        {
            throwType.NetSend(writer);
        }

        public override void NetReceive(BinaryReader reader)
        {
            throwType.NetReceive(reader);
            throwType.ValidateAsItem();
            if (throwType.id.HasValue)
            {
                ReloadDefaults();
                return;
            }
        }

        public override void LoadData(TagCompound tag)
        {
            throwType.Load(tag);
            throwType.ValidateAsItem();
            if (throwType.id.HasValue)
            {
                ReloadDefaults();
                return;
            }
            Mod.Call("Error", $"Missing item for {throwType.mod}:{throwType.name} ");
        }

        public override ModItem Clone(Item newEntity)
        {
            EndlessThrowable obj = (EndlessThrowable)base.Clone(newEntity);
            obj.throwType = throwType;
            // obj.ReloadDefaults();
            return obj;
        }

        public override void SetDefaults()
        {
            // i should do stuff but idk too lazy
            Item.width = 10;
            Item.height = 10;
            Item.damage = 10;
            Item.shoot = ProjectileID.Seed;

            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noUseGraphic = true;

            if (throwType.id is int id && id > 0)
            {
                ReloadDefaults();
            }
            else
            {
                Item.DamageType = DamageClass.Ranged;
            }
        }

        public void ReloadDefaults()
        {
            if (throwType.id is int id)
            {
                Item.CloneDefaults(id);
                Item.maxStack = 1;
                Item.consumable = false;
            }
            else
            {
                Mod.Call("Error", $"Failed to reload defaults for throwable type: {throwType.mod}.{throwType.name}");
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (throwType.id is int id)
            {
                foreach (TooltipLine tt in tooltips)
                {
                    if (tt.Mod == "Terraria" && tt.Name == "ItemName")
                    {
                        var item = ContentSamples.ItemsByType[id];
                        tt.Text = $"Endless {item.AffixName()} {item.Name}";
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
                        tt.Text = "Unloaded Endless Throwables";
                        break;
                    }
                }

                tooltips.Insert(1, new TooltipLine(Mod, "tips", $"Unloaded item ( gone wrong ) ( gone sexual ) \n {throwType.mod}:{throwType.name}"));
            }
        }

        // public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        // {
        //     return base.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        // }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (throwType.id is int id)
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
            if (throwType.id is int id)
            {
                var drawItem = ContentSamples.ItemsByType[id];
                Main.DrawItemIcon(spriteBatch, drawItem, Item.position, lightColor, drawItem.scale);
                return false;
            }
            return true;
        }

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
                    if (accesoryDuplicator.duplicate.id is int id)
                    {
                        resetTypeIDToo = item.type;
                        resetNetID = item.netID;
                        item.type = id;
                        item.netID = id;
                    }
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
    }
}