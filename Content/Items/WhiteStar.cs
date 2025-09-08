using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup.Content.Items
{
    public class WhiteStar : ModItem
	{
		public override void SetStaticDefaults()
        {
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 8));
			// ItemID.Sets.AnimatesAsSoul[Item.type] = true; // Makes the item have an animation while in world (not held.). Use in combination with RegisterItemAnimation

			// ItemID.Sets.ItemIconPulse[Item.type] = true; // The item pulses while in the player's inventory
			// ItemID.Sets.ItemNoGravity[Item.type] = true; // Makes the item have no gravity

			Item.ResearchUnlockCount = 69; // Configure the amount of this item that's needed to research it in Journey mode.
		}

		public override void SetDefaults()
        {
			Item.useStyle = ItemUseStyleID.EatFood;
			Item.UseSound = SoundID.Item119;
			Item.useAnimation = 20;
			Item.useTime = 20;
			Item.consumable = true;

			Item.width = 18;
			Item.height = 18;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = 1000;
			Item.rare = ItemRarityID.Orange;
		}

        public override bool? UseItem(Player player)
        {
			// get random buff for 1 minute
			int[] buffType = new int[] {
				// good
				BuffID.Wrath,
				BuffID.Ironskin,
				BuffID.Endurance,
				BuffID.Lucky,
				// bad
				BuffID.Chilled,
				BuffID.OnFire,
				BuffID.Ichor,
				BuffID.Bleeding
			};

			player.AddBuff(Main.rand.NextFromList(buffType),60*60);
			
			// 1/4 chance to get gold coin
			if (Main.rand.NextBool(4))
			{
				player.QuickSpawnItem(Item.GetSource_FromThis(), ItemID.GoldCoin, 1);
			}

			// 1 / 10 chance to get fallen star
			if (Main.rand.NextBool(10))
			{
				player.QuickSpawnItem(Item.GetSource_FromThis(), ItemID.FallenStar, Main.rand.Next(2,6));
			}

			// 1 / 1000 chance to trigger a rain
			if (Main.rand.NextBool(1000))
			{
				Main.StartRain();
				// player.QuickSpawnItem(Item.GetSource_FromThis(), ItemID.FallenStar, Main.rand.Next(2,6));
			}

            return true;
        }

		public override void PostUpdate()
		{
			Lighting.AddLight(Item.Center, Main.DiscoColor.ToVector3() * 0.55f * Main.essScale); // Makes this item glow when thrown out of inventory.
			if (!Item.beingGrabbed)
			{
				Item.velocity.Y = -1;
			}
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
            CreateRecipe()
                .AddIngredient(ItemID.FallenStar, 3)
                .AddIngredient(ItemID.SoulofLight, 1)
                .AddIngredient(ItemID.SoulofNight, 1)
                .AddIngredient(ItemID.SoulofFlight,1)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}