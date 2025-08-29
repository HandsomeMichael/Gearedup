using System.Collections.Generic;
using System.IO;
using Gearedup.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Gearedup.Content.Items
{
    public class PhasingDevice : ToggleItem
    {
        public override int ItemRarity => ItemRarityID.Blue;
        public override void UpdateEnable(GearPlayer gearPlayer) => gearPlayer.phasingDevice = true;
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.GravitationPotion, 5)
                .AddIngredient(ItemID.IronBar, 15)
                .AddIngredient(ItemID.SoulofFlight, 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
            
            CreateRecipe()
                .AddIngredient(ItemID.GravitationPotion, 5)
                .AddIngredient(ItemID.LeadBar, 15)
                .AddIngredient(ItemID.SoulofFlight, 10)
				.AddTile(TileID.MythrilAnvil)
				.Register();
        }
    }

    public class LunarPhasingDevice : ToggleItem
    {
        public override int ItemRarity => ItemRarityID.Red;

        public override void UpdateEnable(GearPlayer gearPlayer)
        {
            gearPlayer.phasingDevice = true;
            gearPlayer.phasingDeviceLunar = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<PhasingDevice>())
                .AddIngredient(ItemID.FragmentNebula, 10)
                .AddIngredient(ItemID.FragmentSolar, 10)
                .AddIngredient(ItemID.FragmentStardust, 10)
                .AddIngredient(ItemID.FragmentVortex, 10)
				.AddTile(TileID.MythrilAnvil)
				.Register();
        }
    }
}