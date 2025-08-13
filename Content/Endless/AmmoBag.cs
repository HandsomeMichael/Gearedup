using System.Collections.Generic;
using Gearedup.Helper;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Gearedup.Content.Endless
{
    public class AmmoBag : ModItem
    {

        /// <summary>
        /// Ammo stored as item
        /// </summary>
        public List<Item> ammo = new List<Item>();

        /// <summary>
        /// Currently selected ammo
        /// </summary>
        public byte selectedAmmo = 0;

        public override ModItem Clone(Item newEntity)
        {
            AmmoBag obj = (AmmoBag)base.Clone(newEntity);
            obj.ammo = ammo.CloneList();
            return obj;
        }

        public override void SaveData(TagCompound tag)
        {
            tag.Add("AmmoCount", ammo.Count);

            // dont save any shit if no item really exist
            if (ammo.Count <= 0) return;

            for (int i = 0; i < ammo.Count; i++)
            {
                tag.Add("Ammo_" + i, ammo[i]);
            }
        }

        public override void LoadData(TagCompound tag)
        {
            int ammoCount = tag.GetInt("AmmoCount");
            if (ammoCount > 0)
            {
                for (int i = 0; i < ammoCount; i++)
                {
                    ammo.Add(tag.Get<Item>("Ammo_" + i));
                }
            }
        }

        public override void SetDefaults()
        {
            Item.width = 10;
            Item.height = 10;
            Item.consumable = false;
        }

        public override void RightClick(Player player)
        {
            if (player.HeldItem != null && player.HeldItem.ammo > 0)
            {
                var cloned = player.HeldItem.Clone();
                if (cloned.maxStack > 1)
                {
                    bool haveStack = false;
                    foreach (var ammoItem in ammo)
                    {
                        if (ammoItem.type == cloned.type && !haveStack)
                        {
                            ammoItem.stack += cloned.stack;
                            haveStack = true;
                            break;
                        }
                    }
                    if (!haveStack)
                    {
                        ammo.Add(cloned);
                    }
                }
                else
                {
                    ammo.Add(cloned);
                }
                player.HeldItem.TurnToAir();
            }
        }

        public override void UpdateInventory(Player player)
        {
            // just in case
            if (selectedAmmo > ammo.Count)
            {
                selectedAmmo = 0;
            }

            // select ammo
            if (ammo[selectedAmmo] != null)
            {
                Item.ammo = ammo[selectedAmmo].ammo;
                Item.shoot = ammo[selectedAmmo].shoot;
                Item.damage = ammo[selectedAmmo].damage;
                Item.shootSpeed = ammo[selectedAmmo].shootSpeed;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string ammoLeft = "";
            foreach (var item in ammo)
            {
                if (item != null)
                {
                    ammoLeft += $"[i:{item.type}]";
                }
            }

            if (ammoLeft == "")
            {
                tooltips.Add(new TooltipLine(Mod, "AmmoLeft", "No ammo stored"));
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "AmmoSelected", "Selected "+ammo[selectedAmmo].Name + $"[i:{ammo[selectedAmmo].type}]"));
                tooltips.Add(new TooltipLine(Mod, "AmmoLeft", "Ammo stored : " + ammoLeft));
            }
        }

        public override bool ConsumeItem(Player player) => false;

        public override void OnConsumedAsAmmo(Item weapon, Player player)
        {

            // try decreasing stack
            if (ammo[selectedAmmo] != null)
            {
                // if its consumable try doing shit bout it
                if (ammo[selectedAmmo].consumable && ItemLoader.ConsumeItem(ammo[selectedAmmo], player))
                {
                    ammo[selectedAmmo].stack--;
                    if (ammo[selectedAmmo].stack <= 0)
                    {
                        // delete
                        ammo[selectedAmmo].TurnToAir();
                        ammo.RemoveAt(selectedAmmo);

                        // reset ammo
                        if (selectedAmmo > ammo.Count) selectedAmmo = 0;
                    }
                }
            }

            // switch ammo
            selectedAmmo++;
            if (selectedAmmo > ammo.Count)
            {
                selectedAmmo = 0;
            }
        }
        
    }
}