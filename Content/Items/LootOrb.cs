
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup.Content.Items
{
    public class LootOrb : ModItem
    {
        public void UpdateEnable(Player player)
        {
            if (player.TryGetModPlayer(out GearPlayer GP))
            {
                GP.getBossBag = true;
            }
        }

        public override void SetDefaults()
        {
            Item.width = 10;
            Item.height = 10;
            Item.rare = ItemRarityID.Expert;
            Item.master = true;
            Item.accessory = true;
        }

        public override void UpdateInventory(Player player)
        {
            UpdateEnable(player);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            UpdateEnable(player);
        }

        public override void UpdateVanity(Player player)
        {
            UpdateEnable(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.DiamondRing)
                .AddIngredient(ItemID.KingSlimeMasterTrophy)
                .AddIngredient(ItemID.EyeofCthulhuMasterTrophy)
                .AddIngredient(ItemID.QueenBeeMasterTrophy)
                .AddIngredient(ItemID.DeerclopsMasterTrophy)
                // .AddIngredient(ItemID.KingSlimeBossBag)
                // .AddIngredient(ItemID.EyeOfCthulhuBossBag)
                // .AddIngredient(ItemID.QueenBeeBossBag)
                // .AddIngredient(ItemID.DeerclopsBossBag)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
    // public class LootOrb : ToggleItem
    // {
    //     public override int ItemRarity => ItemRarityID.Expert;
    //     public override void UpdateEnable(GearPlayer gearPlayer) => gearPlayer.getBossBag = true;
    //     public override void HoldItem(Player player)
    //     {
    //         base.HoldItem(player);
    //     }
    //     public override void AddRecipes()
    //     {
    //         CreateRecipe()
    //             .AddIngredient(ItemID.DiamondRing)
    //             .AddIngredient(ItemID.KingSlimeBossBag)
    //             .AddIngredient(ItemID.EyeOfCthulhuBossBag)
    //             .AddIngredient(ItemID.QueenBeeBossBag)
    //             .AddIngredient(ItemID.DeerclopsBossBag)
    //             .AddTile(TileID.TinkerersWorkbench)
    //             .Register();
    //     }

    // }
}