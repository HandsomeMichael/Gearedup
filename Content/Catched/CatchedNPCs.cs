using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Gearedup.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Gearedup.Content.Catched
{
    public struct NPCStats
    {
        public int life;
        public int damage;
        public int defense;
        public float[] ai;
    }
    public class CatchedNPC : ModItem
    {
        public override string Texture => "Gearedup/Content/Placeholder";
        public TypeID npcType;
        public bool notIntended;

        public bool IsNotIntended(NPC npc)
        {
            return (npc.realLife != -1 && npc.realLife != npc.whoAmI) || npc.type == NPCID.TargetDummy;
        }


        public void ReloadTypes(NPC npc = null)
        {
            Item.SetNameOverride(Lang.GetNPCName(npcType.id.Value).Value);
            Item.makeNPC = npcType.id.Value;
            if (npc != null)
            {
                Item.color = npc.color;
                Item.value = (int)npc.value;
                // Item.rare = DetermineNPCRarity(npc);
            }
        }

        public override void NetSend(BinaryWriter writer)
        {
            npcType.NetSend(writer);
            writer.Write(notIntended);
            writer.Write(Item.value);
        }
        public override void NetReceive(BinaryReader reader)
        {
            npcType.NetReceive(reader);
            notIntended = reader.ReadBoolean();
            Item.value = reader.ReadInt32();

            if (npcType.ValidateAsNPC())
            {
                ReloadTypes();
            }
        }
        public override void SaveData(TagCompound tag)
        {
            npcType.Save(tag);

            tag.Add("value", Item.value);
            tag.Add("intended", notIntended);
        }
        public override void LoadData(TagCompound tag)
        {
            npcType.Load(tag);

            Item.value = tag.GetInt("value");
            notIntended = tag.GetBool("notIntended");

            if (npcType.ValidateAsNPC())
            {
                ReloadTypes();
            }
        }

        // Stacking
        public override bool CanStack(Item source)
        {
            if (source.ModItem != null && source.ModItem is CatchedNPC target)
            {
                if (target.npcType.id == npcType.id)
                {
                    return true;
                }
            }
            return false;
        }

        public override void SetDefaults()
        {
            // By defaults create a bunny looking ass
            Item.CloneDefaults(ItemID.Bunny);
            Item.rare = ItemRarityID.Blue;
            Item.ammo = ModContent.ItemType<CatchedNPC>();
            Item.notAmmo = true;
        }

        internal static FieldInfo bestiaryKeyField;

        public override void Load()
        {
            bestiaryKeyField = typeof(FlavorTextBestiaryInfoElement).GetField("_key", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public override void Unload()
        {
            bestiaryKeyField = null;
        }

        public bool TryBestiaryDescription(List<TooltipLine> tooltips)
        {
            if (npcType.id is int npcID)
            {
                var npc = ContentSamples.NpcsByNetId[npcID];
                tooltips.Add(new TooltipLine(Mod, "info_damage",$"{npc.damage} damage"));
                tooltips.Add(new TooltipLine(Mod, "info_damage",$"{npc.lifeMax} life"));
                tooltips.Add(new TooltipLine(Mod, "info_damage",$"{npc.defense} defense"));

                // now this is the tricky part , idk if the npc will find its entry correctly without shooting itself
                var bestiaryEntry = Main.BestiaryDB.FindEntryByNPCID(npcID);
                if (bestiaryEntry == null || bestiaryEntry.Info == null) return false;

                foreach (var infoNode in bestiaryEntry.Info)
                {
                    // somehow we need reflection just for this... wow
                    if (infoNode is FlavorTextBestiaryInfoElement element)
                    {
                        string keyValue = (string)bestiaryKeyField.GetValue(element);

                        // we word wrap this chud
                        var list = Helpme.WordWrap(Language.GetText(keyValue).Value, 50);
                        for (int i = 0; i < list.Count; i++)
                        {
                            tooltips.Add(new TooltipLine(Mod, "Bestiary_" + i, list[i]));
                        }
                        return true;
                    }
                }
            }

            return false;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (!TryBestiaryDescription(tooltips))
            {
                tooltips.Add(new TooltipLine(Mod, "BestiaryNone", "No description found for this NPC"));
            }

            if (npcType.id is int validID)
            {
                if (validID == NPCID.BestiaryGirl)
                {
                    tooltips.Add(new TooltipLine(Mod, "Warning", "'No putting in jar'"));
                }

                if (validID == NPCID.Painter)
                {
                    tooltips.Add(new TooltipLine(Mod, "Warning", "'God yes , put me in a jar please'"));
                }

                // if (notIntended(ContentSamples.NpcsByNetId[validID]))
                if (notIntended)
                {
                    tooltips.Add(new TooltipLine(Mod, "Intented", "Might not be intended to catch") { OverrideColor = Color.LightYellow });
                }
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "Unloaded", "This item didnt loaded properly \n" + $"ID : [{npcType.mod} : {npcType.name}] ") { OverrideColor = Color.Red });
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {

            if (npcType.id is int npcID)
            {
                // Check texture
                Main.instance.LoadNPC(npcID);
                Texture2D texture = Terraria.GameContent.TextureAssets.Npc[npcID].Value;
                if (texture == null) return true;

                int frameCount = Main.npcFrameCount[npcID];

                Helpme.DrawInventory(spriteBatch, Item.Center - Main.screenPosition, lightColor, texture, frameCount);
                return false;
            }

            // Helpme.DrawInvalid(spriteBatch,Item.Center - Main.screenPosition,rotation);
            return true;
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            // Check id
            if (npcType.id is int validID)
            {
                // Check texture
                Main.instance.LoadNPC(validID);
                Texture2D texture = Terraria.GameContent.TextureAssets.Npc[validID].Value;
                if (texture == null) return true;

                int frameCount = Main.npcFrameCount[validID];

                Helpme.DrawInventory(spriteBatch, position, drawColor, texture, frameCount);

                return false;
            }

            // Helpme.DrawInvalid(spriteBatch,position,0f);

            return true;
        }
    }

}