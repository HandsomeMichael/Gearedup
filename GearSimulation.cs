using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup
{

    // Simulate stuff to check if a mod has changed how somestuff works
    
    // public class GearSimulation : ModSystem
    // {

    //     /// <summary>
    //     /// Check if a mod changes channel values of item
    //     /// </summary>
    //     public bool itemSwordChannelChanges;

    //     /// <summary>
    //     /// Check if a mod changes sword shoot values
    //     /// </summary>
    //     public bool itemSwordChanges;

    //     public bool simulated;

    //     public override void PostUpdateWorld()
    //     {
    //         if (simulated) return;

    //         Player simulatedPlayer = new Player();
    //         simulatedPlayer.active = true;
    //         simulatedPlayer.dead = false;
    //         simulatedPlayer.statLife = 100;
    //         simulatedPlayer.statLifeMax2 = 100;

    //         SimulateChannelingChanges(simulatedPlayer);


    //         simulated = true;
    //     }

    //     public override void OnWorldUnload()
    //     {
    //         simulated = false;
    //     }

    //     public void SimulateChannelingChanges(Player simulatedPlayer)
    //     {
    //         try
    //         {
    //             // simulate some stuff
    //             Item item = new Item();
    //             item.SetDefaults(ItemID.WoodenSword);
    //             ItemLoader.UpdateInventory(item, simulatedPlayer);
    //             ItemLoader.HoldItem(item, simulatedPlayer);
    //             ItemLoader.UseItem(item, simulatedPlayer);

    //             // item somehow turns into channeling
    //             if (item.channel)
    //             {
    //                 itemSwordChannelChanges = true;
    //             }
    //         }
    //         catch (Exception exc)
    //         {
    //             Mod.Logger.Error("Failed at Simulating Channeling Changes, " + exc.ToString());
    //         }
    //     }


    // }
}