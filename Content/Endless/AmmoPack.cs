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

namespace Gearedup.Content.Endless
{
    public class AmmoPack : ModItem
    {
        public static int GetReqStack(Item item)
        {
            return Math.Min(item.maxStack, 3996) / 2;
        }

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
            Mod.Call("Error", $"Missing item for {ammoType.mod}:{ammoType.name} ");
        }

        public override void NetSend(BinaryWriter writer)
        {
            ammoType.NetSend(writer);
        }

        public override void NetReceive(BinaryReader reader)
        {
            ammoType.NetReceive(reader);
            ammoType.ValidateAsItem();
        }

        // protected override bool CloneNewInstances => true;
        public override ModItem Clone(Item newEntity)
        {
            AmmoPack obj = (AmmoPack)base.Clone(newEntity);
            obj.ammoType = ammoType;
            obj.ReloadDefaults();
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
        public override bool CanRightClick() => true;
        public override void RightClick(Player player)
        {
            if (ammoType.id is int id)
            {
                player.QuickSpawnItem(player.GetSource_FromThis(), id, GetReqStack(ContentSamples.ItemsByType[id]));
                Item.TurnToAir();
            }
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (ammoType.id is int id)
            {
                //tooltips.Add(new TooltipLine(Mod, "extract", $"Can be [i:{ItemID.Extractinator}] extracted into [i:{id}] ammo"));
                tooltips.Add(new TooltipLine(Mod, "extract", $"Right-click this to convert it back to [i/s{GetReqStack(ContentSamples.ItemsByType[id])}:{id}] ammo"));
            }
            else
            {
                if (ammoType.InvalidData())
                {
                    tooltips.Add(new TooltipLine(Mod, "noValue", $"No duplicated ammo properties found\nExtract this for some [i:{ItemID.FallenStar}] fallen star"));
                }
                else
                {
                    tooltips.Add(new TooltipLine(Mod, "noMods", $"No matching ammo properties\nName : {ammoType.name}\nFrom : {ammoType.mod}"));
                }
            }

            //     foreach (TooltipLine tt in tooltips)
            //     {
            //         if (tt.Mod == "Terraria" && tt.Name == "ItemName")
            //         {
            //             tt.Text = "Endless " + ContentSamples.ItemsByType[id].Name + " Pack";
            //             return;
            //         }
            //     }
            // }
            // else
            // {
            //     foreach (TooltipLine tt in tooltips)
            //     {
            //         if (tt.Mod == "Terraria" && tt.Name == "ItemName")
            //         {
            //             tt.Text = "Empty Magical Pack";
            //             break;
            //         }
            //     }

            //     tooltips.Insert(1, new TooltipLine(Mod, "tips", "Does nothing on its own..."));
            //     //tooltips.Add(new TooltipLine(Mod, "tips", "Does nothing on its own..."));
            // }
        }

        // public override void ExtractinatorUse(int extractinatorBlockType, ref int resultType, ref int resultStack)
        // {
        //     if (ammoType.id is int id)
        //     {
        //         resultStack = Math.Min(ContentSamples.ItemsByType[id].maxStack, 3996) / 2;
        //         resultType = id;
        //     }
        // }

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
            else if (!ammoType.InvalidData())
            {
                // redraw the shi
                // spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value,position,null,drawColor,0f,origin,1f,SpriteEffects.None,0f);
                EndlessLoader.DrawUnloaded(spriteBatch, position);
            }
        }
    }
}