using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gearedup.Content.Endless;
using Gearedup.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Gearedup
{
    public class GearItem : GlobalItem
    {
        public override bool InstancePerEntity => true;

        // default to null , will initialize when actually applied
        public Dictionary<string, int> stats;

        public string customName;
        public TypeID dye;

        public bool hasStats => stats != null && stats.Count > 0;

        public static bool CanGeared(Item item)
        {
            return (item.maxStack == 1 && item.damage > 0 && item.useTime > 0 && item.useAnimation > 0) ||
            item.type == ModContent.GetInstance<EndlessThrowable>().Type; // endlless throwable can be geared up
        }

        public override bool AppliesToEntity(Item item, bool lateInstantiation) => CanGeared(item);

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (dye.id is int dyeID)
            {
                tooltips.Add(new TooltipLine(Mod, "DyeImbue", $"Imbued with {ContentSamples.ItemsByType[dyeID].Name} [i:{dyeID}]"));
                // tooltips.Add(new TooltipLine(Mod, "DyeTips",$"Press anything"));
            }
            if (!hasStats) return;
            foreach (var stat in stats)
            {
                var text = Language.GetTextValue($"Mods.Gearedup.Stat." + stat.Key, stat.Value);
                var clr = Color.Orange;
                if (stat.Value != 0)
                {
                    if (stat.Value > 0)
                    {
                        text = "+" + text;
                        clr = Color.LightGreen;
                    }
                    else
                    {
                        clr = Color.LightPink;
                    }
                }
                var tt = new TooltipLine(Mod, "GearStat_" + stat.Key, text);
                tt.OverrideColor = clr;
                tooltips.Add(tt);
            }
        }

        public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
        {
            if (hasStats && line.Name.Contains("GearStat_"))
            {
                line.BaseScale /= 2f;
                // yOffset += 4;
                // Utils.DrawInvBG(Main.spriteBatch, new Rectangle(
                //     line.X,
                //     line.Y - 2,
                //     (int)messageSize.X + 27,
                //     (int)messageSize.Y + 2), color * 0.4f);
                // Utils.DrawInvBG(Main.spriteBatch, new Rectangle(
                //     line.X - 7,
                //     line.Y - 4,
                //     34,
                //     (int)messageSize.Y + 5));
            }
            return base.PreDrawTooltipLine(item, line, ref yOffset);
        }

        public override void HoldItem(Item item, Player player)
        {

        }

        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            damage += GetStat("damage");
        }

        public float GetStat(string statName)
        {
            if (hasStats && stats.TryGetValue(statName, out int value))
            {
                return value / 100f;
            }
            return 0f;
        }

        public int GetStatFlat(string statName)
        {
            if (hasStats && stats.TryGetValue(statName, out int value))
            {
                return value;
            }
            return 0;
        }

        public override void NetSend(Item item, BinaryWriter writer)
        {
            dye.NetSend(writer);
        }

        public override void NetReceive(Item item, BinaryReader reader)
        {
            dye.NetReceive(reader);
            dye.ValidateAsItem();
        }

        public override void SaveData(Item item, TagCompound tag)
        {
            dye.Save(tag);

            if (!hasStats) return;
            tag["statNames"] = stats.Keys.ToList();
            tag["statValues"] = stats.Values.ToList();
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            dye.Load(tag);
            dye.ValidateAsItem();

            if (tag.ContainsKey("statNames") && tag.ContainsKey("statValues"))
            {
                var names = tag.Get<List<string>>("statNames");
                var values = tag.Get<List<int>>("statValues");
                stats = names.Zip(values, (k, v) => new { Key = k, Value = v }).ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (dye.id is int id)
            {
                Entity entityShader = GearClientConfig.Get.DyeItemPlayerShader ? Main.LocalPlayer : item;

                DrawData data = new DrawData
                {
                    position = position - Main.screenPosition,
                    scale = new Vector2(scale, scale),
                    sourceRect = frame,
                    texture = TextureAssets.Item[item.type].Value
                };
                spriteBatch.BeginDyeShaderByItem(id, entityShader, true, true, data);
            }
            return base.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        }

        public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            base.PostDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
            if (dye.id.HasValue) { spriteBatch.BeginNormal(true, true); }

            // if (DyeClientConfig.Get.Debug)
            // ChatManager.DrawColorCodedString(spriteBatch,FontAssets.MouseText.Value,":"+dye,position,Color.White,0f,Vector2.One,Vector2.One);
        }

        public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            if (dye.id is int id)
            {
                Entity entityShader = GearClientConfig.Get.DyeItemPlayerShader ? Main.LocalPlayer : item;
                DrawData data = new DrawData
                {
                    position = item.position - Main.screenPosition,
                    scale = new Vector2(scale, scale),
                    sourceRect = null,
                    texture = TextureAssets.Item[item.type].Value
                };

                spriteBatch.BeginDyeShaderByItem(id, entityShader, true, false, data);
            }
            return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
        }

        public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            base.PostDrawInWorld(item, spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
            if (dye.id.HasValue) { spriteBatch.BeginNormal(true); }
        }

        // this doesnt do shi my gng 💔
        public override void OnCreated(Item item, ItemCreationContext context)
        {
            // 1% chance of getting perk
            if (Main.rand.NextBool(100))
            {

            }
        }
    }

    public class ApplyDye : GlobalItem
	{
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod,"applydye","Hold weapon and right click this to dye it"));
            tooltips.Add(new TooltipLine(Mod,"applydye","Dyed item can be washed using water"));
        }

        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            // THE AMOUNT OF SHIT THIS THING CAUSE TO BUG OUT THE GAME IS HUGE
            return entity.dye > 0 && entity.type != ItemID.ColorOnlyDye;
        }

        // If not you know
        public override bool ConsumeItem(Item item, Player player)
        {
            return GearServerConfig.Get.AllowDyeConsumed;
        }
        
        // Apply Dyes
        public override void RightClick(Item item, Player player)
        {
            if (Main.mouseItem != null)
            {
                if (Main.mouseItem.TryGetGlobalItem<GearItem>(out GearItem git))
                {
                    git.dye.SetTo(item);
                    // git.dye.ValidateAsItem();

                    SoundEngine.PlaySound(SoundID.Splash);
                }
            }
        }

        public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            // if (DyeClientConfig.Get.Debug)
            // ChatManager.DrawColorCodedString(spriteBatch,FontAssets.MouseText.Value,item.type+" / "+item.dye,position,Color.White,0f,Vector2.One,Vector2.One);
        }

        public override bool CanRightClick(Item item)
        {
			if (Main.mouseItem != null && GearItem.CanGeared(Main.mouseItem))
            {
				return true;
			}

            return false;
        }
    }
}