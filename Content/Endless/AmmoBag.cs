using System.Collections.Generic;
using Gearedup.Helper;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria.DataStructures;

namespace Gearedup.Content.Endless
{
    // public class AmmoBag : ModItem
    // {

    //     /// <summary>
    //     /// Ammo stored as item
    //     /// </summary>
    //     public List<Item> ammo = new List<Item>();

    //     /// <summary>
    //     /// Currently selected ammo
    //     /// </summary>
    //     public byte selectedAmmo = 0;

    //     public override ModItem Clone(Item newEntity)
    //     {
    //         AmmoBag obj = (AmmoBag)base.Clone(newEntity);
    //         obj.ammo = ammo.CloneList();
    //         return obj;
    //     }

    //     public override void SaveData(TagCompound tag)
    //     {
    //         tag.Add("AmmoCount", ammo.Count);

    //         // dont save any shit if no item really exist
    //         if (ammo.Count <= 0) return;

    //         for (int i = 0; i < ammo.Count; i++)
    //         {
    //             tag.Add("Ammo_" + i, ammo[i]);
    //         }
    //     }

    //     public override void LoadData(TagCompound tag)
    //     {
    //         int ammoCount = tag.GetInt("AmmoCount");
    //         if (ammoCount > 0)
    //         {
    //             for (int i = 0; i < ammoCount; i++)
    //             {
    //                 ammo.Add(tag.Get<Item>("Ammo_" + i));
    //             }
    //         }

    //         UpdatePrice();
    //     }

    //     public override void SetDefaults()
    //     {
    //         Item.width = 10;
    //         Item.height = 10;
    //         Item.consumable = true;
    //     }

    //     public override bool CanRightClick() => true;

    //     public override void RightClick(Player player)
    //     {
    //         if (Main.mouseItem != null && Main.mouseItem.type > ItemID.None && Main.mouseItem.ammo > 0)
    //         {
    //             var cloned = Main.mouseItem.Clone();

    //             if (cloned.maxStack > 1 && ammo.Count > 0)
    //             {
    //                 bool haveStack = false;
    //                 foreach (var ammoItem in ammo)
    //                 {
    //                     if (ammoItem.type == cloned.type && !haveStack)
    //                     {
    //                         ammoItem.stack += cloned.stack;
    //                         haveStack = true;
    //                         break;
    //                     }
    //                 }
    //                 if (!haveStack)
    //                 {
    //                     ammo.Add(cloned);
    //                 }
    //             }
    //             else
    //             {
    //                 ammo.Add(cloned);
    //             }
    //             CombatText.NewText(player.Hitbox, Color.White, "Ammo added !");
    //             Main.mouseItem.TurnToAir();
    //         }
    //         else
    //         {
    //             if (ammo.Count > 0)
    //             {
    //                 foreach (var i in ammo)
    //                 {
    //                     player.QuickSpawnItem(Item.GetSource_FromThis(), i, i.stack);
    //                 }
    //                 ammo.Clear();
    //                 CombatText.NewText(player.Hitbox, Color.White, "Ammo cleared !");
    //             }
    //         }

    //         UpdatePrice();
    //     }

    //     public void UpdatePrice()
    //     {
    //         Item.value = 0;
    //         if (ammo.Count > 0)
    //         {
    //             foreach (var i in ammo)
    //             {
    //                 Item.value = i.value * i.stack;
    //             }
    //         }
    //     }

    //     public override void OnConsumedAsAmmo(Item weapon, Player player)
    //     {
    //         Item.stack = 100;
    //         ChainAmmo(weapon, player);
    //         // base.OnConsumedAsAmmo(weapon, player);
    //     }
    //     public override bool CanBeConsumedAsAmmo(Item weapon, Player player)
    //     {
    //         return base.CanBeConsumedAsAmmo(weapon, player);
    //     }
    //     public override void UpdateInventory(Player player)
    //     {
    //         // do nothin if its empty
    //         if (ammo.Count <= 0) return;

    //         // just in case
    //         if (selectedAmmo > ammo.Count)
    //         {
    //             selectedAmmo = 0;
    //         }

    //         // select ammo
    //         if (ammo[selectedAmmo] != null)
    //         {
    //             Item.ammo = ammo[selectedAmmo].ammo;
    //             Item.shoot = ammo[selectedAmmo].shoot;
    //             Item.damage = ammo[selectedAmmo].damage;
    //             Item.shootSpeed = ammo[selectedAmmo].shootSpeed;
    //         }
    //     }

    //     public override void ModifyTooltips(List<TooltipLine> tooltips)
    //     {
    //         string ammoLeft = "";
    //         if (ammo.Count > 0)
    //         {
    //             foreach (var item in ammo)
    //             {
    //                 if (item != null)
    //                 {
    //                     ammoLeft += $"[i/s{item.stack}:{item.type}] ";
    //                 }
    //             }
    //         }

    //         if (ammoLeft == "")
    //         {
    //             tooltips.Add(new TooltipLine(Mod, "AmmoLeft", "No ammo stored"));
    //         }
    //         else
    //         {
    //             tooltips.Add(new TooltipLine(Mod, "AmmoSelected", "Selected " + ammo[selectedAmmo].Name + $"[i:{ammo[selectedAmmo].type}]"));
    //             tooltips.Add(new TooltipLine(Mod, "AmmoLeft", "Ammo stored : " + ammoLeft));
    //         }
    //     }


    //     public void ChainAmmo(Item weapon, Player player)
    //     {
    //         // do nothin if its empty
    //         if (ammo.Count <= 0) return;

    //         // try decreasing stack
    //         if (ammo[selectedAmmo] != null)
    //         {
    //             // if its consumable try doing shit bout it
    //             if (ammo[selectedAmmo].consumable && ItemLoader.ConsumeItem(ammo[selectedAmmo], player))
    //             {
    //                 ammo[selectedAmmo].stack--;
    //                 ItemLoader.OnConsumeAmmo(weapon, ammo[selectedAmmo], player);

    //                 if (ammo[selectedAmmo].stack <= 0)
    //                 {
    //                     // delete
    //                     ammo[selectedAmmo].TurnToAir();
    //                     ammo.RemoveAt(selectedAmmo);

    //                     // reset ammo
    //                     if (selectedAmmo > ammo.Count) selectedAmmo = 0;
    //                 }
    //             }
    //         }

    //         // switch ammo
    //         selectedAmmo++;
    //         if (selectedAmmo > ammo.Count)
    //         {
    //             selectedAmmo = 0;
    //         }
    //     }

    //     // make the predraw think it always 1 stack
    //     public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    //     {
    //         Item.stack = 1;
    //         return base.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    //     }
    // }
}