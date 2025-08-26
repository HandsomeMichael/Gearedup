using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup.Content.Items
{
    public class DevEyesGlobal : GlobalItem
    {
        public override bool IsLoadingEnabled(Mod mod)
        {
            return GearClientConfig.Get.Debug_ItemID;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (GearClientConfig.Get.Debug_ItemID) tooltips.Add(new TooltipLine(Mod, "shitass", item.ModItem == null ? $"Vanilla : {item.type}" : $"{item.ModItem.Mod} : {item.ModItem.Name} / {item.type}" ));
        }
    }
    // public class DevEyes : ModItem
    // {
    //     public static bool DevEyesEnable = false;
    //     public override void SetDefaults()
    //     {
    //         Item.useAnimation = 10;
    //         Item.useTime = 10;
    //         Item.useStyle = ItemUseStyleID.HoldUp;
    //         Item.width = 10;
    //         Item.height = 10;
    //         Item.accessory = true;
    //     }

    //     public override void ModifyTooltips(List<TooltipLine> tooltips)
    //     {
    //         tooltips.Add(new TooltipLine(Mod, "gming", DevEyesEnable ? " The All Seeing Eyes Will See Shit\nRight click to disable" : "The All Seeing Eyes Will Not See Shit\nRight click to enable"));
    //     }
    //     public override bool? UseItem(Player player)
    //     {
    //         if (player.whoAmI == Main.myPlayer)
    //         {
    //             DevEyesEnable = !DevEyesEnable;
    //         }
    //         return true;
    //     }
    // }
}