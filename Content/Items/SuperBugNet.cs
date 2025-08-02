using System;
using System.IO;
using Gearedup.Content.Catched;
using Gearedup.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup.Content.Items
{

	public class SuperBugNet : ModItem
	{
		public int altCoolDown = 0;

		public override void SetDefaults() 
        {
			// Common Properties
			Item.width = 46;
			Item.height = 48;
			Item.value = Item.sellPrice(gold: 2, silver: 50);
			Item.rare = ItemRarityID.Quest;

			Item.useTime = 40;
			Item.useAnimation = 40;
			Item.useStyle = ItemUseStyleID.Shoot;

			Item.autoReuse = true; // This determines whether the weapon has autoswing

			Item.noMelee = true;  // This makes sure the item does not deal damage from the swinging animation
			Item.noUseGraphic = true; // This makes sure the item does not get shown when the player swings his hand

			// Projectile Properties
			Item.shoot = ModContent.ProjectileType<SuperBugNetProj>(); // The sword as a projectile
		}

        public override void UpdateInventory(Player player)
        {
            if (altCoolDown > 0) altCoolDown--;
        }

        public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player)
        {
			if (altCoolDown > 0 && player.altFunctionUse == 2 && player.HeldItem.type == Item.type)
			{
				CombatText.NewText(player.Hitbox,Color.White,$"On Cooldown {altCoolDown / 60}s");
				SoundEngine.PlaySound(SoundID.MenuClose,player.Center);
				return false;
			}
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			int attackType = 0;
			if (altCoolDown <= 0 && player.altFunctionUse == 2)
			{
				attackType = 1;
				altCoolDown = 60 * 3;
			}
			int i = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Main.myPlayer, attackType);
			if (Main.projectile[i].TryGetGlobalProjectile<GearProjectile>(out GearProjectile gr))
			{
				gr.dye = (short)ContentSamples.ItemsByType[ItemID.TwilightDye].dye;
			}
			return false; // return false to prevent original projectile from being shot
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.BugNet)
				.AddIngredient(ItemID.FallenStar,3)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}

    /// <summary>
    /// What if i make the bug net cooler fr fr
    /// </summary>
    public class SuperBugNetProj : ModProjectile
    {
		// We define some constants that determine the swing range of the sword
		// Not that we use multipliers here since that simplifies the amount of tweaks for these interactions
		// You could change the values or even replace them entirely, but they are tweaked with looks in mind
		private const float SWINGRANGE = 1.67f * (float)Math.PI; // The angle a swing attack covers (300 deg)
		private const float FIRSTHALFSWING = 0.45f; // How much of the swing happens before it reaches the target angle (in relation to swingRange)
		private const float SPINRANGE = 3.5f * (float)Math.PI; // The angle a spin attack covers (630 degrees)
		private const float WINDUP = 0.15f; // How far back the player's hand goes when winding their attack (in relation to swingRange)
		private const float UNWIND = 0.4f; // When should the sword start disappearing
		private const float SPINTIME = 2.5f; // How much longer a spin is than a swing

		private enum AttackType // Which attack is being performed
		{
			// Swings are normal sword swings that can be slightly aimed
			// Swings goes through the full cycle of animations
			Swing,
			// Spins are swings that go full circle
			// They are slower and deal more knockback
			Spin,
		}

		private enum AttackStage // What stage of the attack is being executed, see functions found in AI for description
		{
			Prepare,
			Execute,
			Unwind
		}

		// These properties wrap the usual ai and localAI arrays for cleaner and easier to understand code.
		private AttackType CurrentAttack {
			get => (AttackType)Projectile.ai[0];
			set => Projectile.ai[0] = (float)value;
		}

		private AttackStage CurrentStage {
			get => (AttackStage)Projectile.localAI[0];
			set {
				Projectile.localAI[0] = (float)value;
				Timer = 0; // reset the timer when the projectile switches states
			}
		}

		// Variables to keep track of during runtime
		private ref float InitialAngle => ref Projectile.ai[1]; // Angle aimed in (with constraints)
		private ref float Timer => ref Projectile.ai[2]; // Timer to keep track of progression of each stage
		private ref float Progress => ref Projectile.localAI[1]; // Position of sword relative to initial angle
		private ref float Size => ref Projectile.localAI[2]; // Size of sword

		// We define timing functions for each stage, taking into account melee attack speed
		// Note that you can change this to suit the need of your projectile
		private float prepTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float execTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);
		private float hideTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);

		public override string Texture => "Gearedup/Content/Items/SuperBugNet"; // Use texture of item as projectile texture
		private Player Owner => Main.player[Projectile.owner];

		public override void SetStaticDefaults() {
			ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
		}

		public override void SetDefaults() 
        {
			Projectile.width = 46; // Hitbox width of projectile
			Projectile.height = 48; // Hitbox height of projectile
			Projectile.friendly = true; // Projectile hits enemies
			Projectile.timeLeft = 10000; // Time it takes for projectile to expire
			Projectile.penetrate = -1; // Projectile pierces infinitely
			Projectile.tileCollide = false; // Projectile does not collide with tiles
			Projectile.ownerHitCheck = true; // Make sure the owner of the projectile has line of sight to the target (aka can't hit things through tile).
			Projectile.DamageType = DamageClass.Melee; // Projectile is a melee projectile
		}

		public override void OnSpawn(IEntitySource source) 
        {
			Projectile.spriteDirection = Main.MouseWorld.X > Owner.MountedCenter.X ? 1 : -1;
			float targetAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();

			if (CurrentAttack == AttackType.Spin) {
				InitialAngle = (float)(-Math.PI / 2 - Math.PI * 1 / 3 * Projectile.spriteDirection); // For the spin, starting angle is designated based on direction of hit
			}
			else {
				if (Projectile.spriteDirection == 1) {
					// However, we limit the rangle of possible directions so it does not look too ridiculous
					targetAngle = MathHelper.Clamp(targetAngle, (float)-Math.PI * 1 / 3, (float)Math.PI * 1 / 6);
				}
				else {
					if (targetAngle < 0) {
						targetAngle += 2 * (float)Math.PI; // This makes the range continuous for easier operations
					}

					targetAngle = MathHelper.Clamp(targetAngle, (float)Math.PI * 5 / 6, (float)Math.PI * 4 / 3);
				}

				InitialAngle = targetAngle - FIRSTHALFSWING * SWINGRANGE * Projectile.spriteDirection; // Otherwise, we calculate the angle
			}
		}

		public override void SendExtraAI(BinaryWriter writer) {
			// Projectile.spriteDirection for this projectile is derived from the mouse position of the owner in OnSpawn, as such it needs to be synced. spriteDirection is not one of the fields automatically synced over the network. All Projectile.ai slots are used already, so we will sync it manually. 
			writer.Write((sbyte)Projectile.spriteDirection);
		}

		public override void ReceiveExtraAI(BinaryReader reader) 
		{
			Projectile.spriteDirection = reader.ReadSByte();
		}

		void CatchNPC(NPC npc, int who = -1)
		{
            int i = npc.whoAmI;
			if (!npc.active)return;
			if (who == -1)
			{
				who = Main.myPlayer;
			}
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				npc.active = false;
				NetMessage.SendData(MessageID.BugCatching, -1, -1, null, i, who);
			}
			else
			{
				if (npc.type == 687)
				{
					TryTeleportingCaughtMysticFrog(npc);
				}
				else if (npc.SpawnedFromStatue)
				{
					Vector2 vector = npc.Center - new Vector2(20f);
					Terraria.Utils.PoofOfSmoke(vector);
					npc.active = false;
					NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, i);
					NetMessage.SendData(MessageID.PoofOfSmoke, -1, -1, null, (int)vector.X, vector.Y);
				}
				else
				{
                    var source = Main.player[who].GetSource_CatchEntity(npc);
                    int itemWhoAmI = 0;
                    if (npc.catchItem > 0)
                    {
                        itemWhoAmI = Item.NewItem(source,
                         (int)Main.player[who].Center.X,
                          (int)Main.player[who].Center.Y,
                           0, 0, Main.npc[i].catchItem, 1,
                            noBroadcast: true, 0, noGrabDelay: true);
                    }
                    else
                    {
                        itemWhoAmI = Item.NewItem(source,
                         (int)Main.player[who].Center.X,
                          (int)Main.player[who].Center.Y,
                           0, 0, ModContent.ItemType<CatchedNPC>(), 1,
                            noBroadcast: true, 0, noGrabDelay: true);

						if (Main.item[itemWhoAmI].ModItem != null && Main.item[itemWhoAmI].ModItem is CatchedNPC boeingplane)
						{
							boeingplane.npcType.SetTo(npc);
							boeingplane.npcType.ValidateAsNPC();
							boeingplane.ReloadTypes(npc);
							boeingplane.notIntended = boeingplane.IsNotIntended(npc);
							//catchType.SetTo(npc,Main.item[itemWhoAmI]);
						}
                    }
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemWhoAmI, 1f);
					npc.active = false;
					NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, i);
				}
			}
		}

        // frog
        public bool TryTeleportingCaughtMysticFrog(NPC npc)
		{
            // Check
			if (Main.netMode == NetmodeID.MultiplayerClient)return false;
			if (npc.type != 687)return false;

			Vector2 chosenTile = Vector2.Zero;
			Point point = npc.Center.ToTileCoordinates();
			if (npc.AI_AttemptToFindTeleportSpot(ref chosenTile, point.X, point.Y, 15, 8))
			{
				Vector2 newPos = new Vector2(chosenTile.X * 16f - (float)(npc.width / 2), chosenTile.Y * 16f - (float)npc.height);
				NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
				npc.Teleport(newPos, 13);
				return true;
			}
			Vector2 vector = npc.Center - new Vector2(20f);
			Utils.PoofOfSmoke(vector);
			npc.active = false;
			NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
			NetMessage.SendData(MessageID.PoofOfSmoke, -1, -1, null, (int)vector.X, vector.Y);
			return false;
		}

		void CatchProjectile(Projectile projectile)
		{
			var source = Owner.GetSource_ItemUse(Owner.HeldItem,"Catching Goth Mommy");
			int i = Item.NewItem(source,Owner.Center,0, 0, ModContent.ItemType<CatchedProjectile>(), 1,noBroadcast: true, 0, noGrabDelay: true);
			Item item = Main.item[i];
			if (item.ModItem != null && item.ModItem is CatchedProjectile boeingplane)
			{
				boeingplane.catchType.SetTo(projectile);
				boeingplane.catchType.ValidateAsNPC();
				boeingplane.ReloadTypes(projectile);
				boeingplane.notIntended = boeingplane.IsNotIntended(projectile);
				// boeingplane.isin
			}
			else
			{
				Main.NewText("You just discovered a 1 in a kajillion bajillion chance of rare Error :D");
				Mod.Logger.Error("How the fuck did this item got created then");
			}
			NetMessage.SendData(MessageID.SyncItem, -1, -1, null, i, 1f);
			projectile.active = false;
			NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile.whoAmI);
		}

		void TryCatching()
		{
			// uhh what
			if (GearServerConfig.Get.AllowSuperBugNet_NPCs)
			{
				foreach (var npc in Main.ActiveNPCs)
				{
					if (npc != null && CheckCollide(npc.Hitbox) )
					{
						if (npc.boss && !GearServerConfig.Get.AllowSuperBugNet_Bosses)
						{
							//Main.NewText("Passed on boss");
							continue;
						}
						CatchNPC(npc,Projectile.owner);
					}
				}
			}

			// might move this function around global proj
			if (GearServerConfig.Get.AllowSuperBugNet_Projectile)
			{
				foreach (var proj in Main.ActiveProjectiles)
				{
					if (proj != null && proj.type != Projectile.type && CheckCollide(proj.Hitbox))
					{
						CatchProjectile(proj);
					}
				}
			}
		}

		public override void AI() 
        {

			// Extend use animation until projectile is killed
			Owner.itemAnimation = 2;
			Owner.itemTime = 2;

			// Kill the projectile if the player dies or gets crowd controlled
			if (Owner.IsWack(true)) {
				Projectile.Kill();
				return;
			}

			// AI depends on stage and attack
			// Note that these stages are to facilitate the scaling effect at the beginning and end
			// If this is not desirable for you, feel free to simplify
			switch (CurrentStage) {
				case AttackStage.Prepare:
					PrepareStrike();
					break;
				case AttackStage.Execute:
					ExecuteStrike();
					TryCatching();
					break;
				default:
					UnwindStrike();
					TryCatching();
					break;
			}

			SetSwordPosition();
			Timer++;
		}

		public override bool PreDraw(ref Color lightColor) 
		{
			// Calculate origin of sword (hilt) based on orientation and offset sword rotation (as sword is angled in its sprite)
			Vector2 origin;
			float rotationOffset;
			SpriteEffects effects;

			if (Projectile.spriteDirection > 0) {
				origin = new Vector2(0, Projectile.height);
				rotationOffset = MathHelper.ToRadians(45f);
				effects = SpriteEffects.None;
			}
			else {
				origin = new Vector2(Projectile.width, Projectile.height);
				rotationOffset = MathHelper.ToRadians(135f);
				effects = SpriteEffects.FlipHorizontally;
			}

			Texture2D texture = TextureAssets.Projectile[Type].Value;

			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, default, lightColor * Projectile.Opacity, Projectile.rotation + rotationOffset, origin, Projectile.scale, effects, 0);

			// Since we are doing a custom draw, prevent it from normally drawing
			return false;
		}

		// Find the start and end of the sword and use a line collider to check for collision with enemies
		public bool CheckCollide(Rectangle targetHitbox) 
		{
			Vector2 start = Owner.MountedCenter;
			Vector2 end = start + Projectile.rotation.ToRotationVector2() * ((Projectile.Size.Length()) * Projectile.scale);
			float collisionPoint = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 15f * Projectile.scale, ref collisionPoint);
		}

		// Do a similar collision check for tiles
		public override void CutTiles() {
			Vector2 start = Owner.MountedCenter;
			Vector2 end = start + Projectile.rotation.ToRotationVector2() * (Projectile.Size.Length() * Projectile.scale);
			Utils.PlotTileLine(start, end, 15 * Projectile.scale, DelegateMethods.CutTiles);
		}

		public override bool? CanDamage() => false;

		// Function to easily set projectile and arm position
		public void SetSwordPosition() {
			Projectile.rotation = InitialAngle + Projectile.spriteDirection * Progress; // Set projectile rotation

			// Set composite arm allows you to set the rotation of the arm and stretch of the front and back arms independently
			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f)); // set arm position (90 degree offset since arm starts lowered)
			Vector2 armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)Math.PI / 2); // get position of hand

			armPosition.Y += Owner.gfxOffY;
			Projectile.Center = armPosition; // Set projectile to arm position
			Projectile.scale = Size * 1.2f * Owner.GetAdjustedItemScale(Owner.HeldItem); // Slightly scale up the projectile and also take into account melee size modifiers

			Owner.heldProj = Projectile.whoAmI; // set held projectile to this projectile
		}

		// Function facilitating the taking out of the sword
		private void PrepareStrike() 
		{
			Progress = WINDUP * SWINGRANGE * (1f - Timer / prepTime); // Calculates rotation from initial angle
			Size = MathHelper.SmoothStep(0, 1, Timer / prepTime); // Make sword slowly increase in size as we prepare to strike until it reaches max

			if (Timer >= prepTime) {
				SoundEngine.PlaySound(SoundID.Item1); // Play sword sound here since playing it on spawn is too early
				CurrentStage = AttackStage.Execute; // If attack is over prep time, we go to next stage
			}
		}

		// Function facilitating the first half of the swing
		private void ExecuteStrike() 
		{
			if (CurrentAttack == AttackType.Swing) {
				Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) * Timer / execTime);

				if (Timer >= execTime) {
					CurrentStage = AttackStage.Unwind;
				}
			}
			else {
				Progress = MathHelper.SmoothStep(0, SPINRANGE, (1f - UNWIND / 2) * Timer / (execTime * SPINTIME));

				if (Timer == (int)(execTime * SPINTIME * 3 / 4)) {
					SoundEngine.PlaySound(SoundID.Item1); // Play sword sound again
					Projectile.ResetLocalNPCHitImmunity(); // Reset the local npc hit immunity for second half of spin
				}

				if (Timer >= execTime * SPINTIME) {
					CurrentStage = AttackStage.Unwind;
				}
			}
		}

		// Function facilitating the latter half of the swing where the sword disappears
		private void UnwindStrike() {
			if (CurrentAttack == AttackType.Swing) {
				Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) + UNWIND * Timer / hideTime);
				Size = 1f - MathHelper.SmoothStep(0, 1, Timer / hideTime); // Make sword slowly decrease in size as we end the swing to make a smooth hiding animation

				if (Timer >= hideTime) {
					Projectile.Kill();
				}
			}
			else {
				Progress = MathHelper.SmoothStep(0, SPINRANGE, (1f - UNWIND / 2) + UNWIND / 2 * Timer / (hideTime * SPINTIME / 2));
				Size = 1f - MathHelper.SmoothStep(0, 1, Timer / (hideTime * SPINTIME / 2));

				if (Timer >= hideTime * SPINTIME / 2) {
					Projectile.Kill();
				}
			}
		}

	}
}


// Unused code

// // wacky way to code part 12
// // use weird instancing mechanism for all the wacky people

// public const int Instances_Max = 255;
// public static int[] Instances;
// public static bool CheckAnyHitting(Rectangle hitbox)
// {
//     for (int i = 0; i < Instances.Length; i++)
//     {
//         if (Instances[i] != -1)
//         {
//             int index = Instances[i];
//             if (Main.projectile[index] != null && Main.projectile[index].ModProjectile != null)
//             {
//                 var proj = Main.projectile[index].ModProjectile as SuperBugNetProj;
//             }
//         }
//     }
//     return false;
// }

// public static void UnloadInstances()
// {
//     Instances = null;
// }

// public static void ResetInstances()
// {
//     Instances = new int[Instances_Max];
//     for (int i = 0; i < Instances.Length; i++)
//     {
//         Instances[i] = -1;
//     }
// }

// public int instanceKey 
// {
//     get { return (int)Projectile.ai[1]; }
//     set { Projectile.ai[1] = value;}
// }

// public void DeleteKey()
// {
//     if (instanceKey == -1)
//     {
//         Main.NewText("Error , key already not existed in the first place");
//         return;
//     }

//     Instances[instanceKey] = -1;
//     instanceKey = -1;
// }

// public void TryAssignKey()
// {
//     if (instanceKey == -1)
//     {
//         for (int i = 0; i < Instances.Length; i++)
//         {
//             if (Instances[i] == -1)
//             {
//                 Instances[i] = Projectile.whoAmI;
//                 instanceKey = i;
//                 Main.NewText("Sucessfully assigned key to "+i);
//             }
//         }
//     }
// }

// public override void OnKill(int timeLeft)
// {
//     DeleteKey();
// }