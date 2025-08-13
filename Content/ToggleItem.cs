using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Gearedup.Content.Items
{
    public abstract class ToggleItem : ModItem
    {

        /// <summary>
        /// Update on inventory
        /// </summary>
        /// <param name="gearPlayer"></param>
        public virtual void UpdateEnable(GearPlayer gearPlayer)
        {
        }

        public virtual int ItemRarity => ItemRarityID.Green;

        public bool enabled;
        protected override bool CloneNewInstances => true;
        public override ModItem Clone(Item newEntity)
        {
            ToggleItem obj = (ToggleItem)base.Clone(newEntity);
            obj.enabled = enabled;
            return obj;
        }
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(enabled);
        }
        public override void NetReceive(BinaryReader reader)
        {
            enabled = reader.ReadBoolean();
        }
        public override void SaveData(TagCompound tag)
        {
            tag.Add("enabled", enabled);
        }
        public override void LoadData(TagCompound tag)
        {
            enabled = tag.GetBool("enabled");
        }
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 38;
            Item.value = 1000;
            Item.consumable = false;
            Item.rare = ItemRarity;
        }

        public override bool ConsumeItem(Player player) => false;
        public override void RightClick(Player player)
        {
            enabled = !enabled;
            CombatText.NewText(player.Hitbox, enabled ? Color.LightGreen : Color.LightPink, Item.Name + " is " + (enabled ? "enabled " : "disabled"));
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Insert(1, new TooltipLine(Mod, "pre", enabled ? "Enabled": "Disabled"));
            if (enabled)
            {
                tooltips.Add(new TooltipLine(Mod, "enable", "Right-Click to disable effect"));
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "disable", "Right-Click to enable effect"));
            }
        }

        public override bool CanRightClick() => true;
        public override void UpdateInventory(Player player)
        {
            if (!enabled) return;
            if (player == null || !player.active) return;
            player.TryGetModPlayer<GearPlayer>(out GearPlayer modPlayer);
            if (modPlayer == null) return;

            UpdateEnable(modPlayer);
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D itemText = ModContent.Request<Texture2D>(Texture + (enabled ? "_active" : "")).Value;

            spriteBatch.Draw(itemText, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            // Helpme.DrawInventory(spriteBatch, position, drawColor, extraTexture, );
            return false;
        }
    }
}