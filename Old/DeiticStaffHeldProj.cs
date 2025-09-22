// using System;
// using Microsoft.Xna.Framework;
// using Terraria;
// using Terraria.Audio;
// using Terraria.DataStructures;
// using Terraria.ID;
// using Terraria.ModLoader;

// namespace Gearedup.Content.Items
// {
//     public class DeiticStaff : ModItem
//     {
//         public override void SetDefaults()
//         {
//             //Item.damage = 8;
//             //Item.knockBack = 4f;
//             Item.useStyle = ItemUseStyleID.Rapier; // Makes the player do the proper arm motion
//             Item.useAnimation = 12;
//             Item.useTime = 12;

//             Item.width = 32;
//             Item.height = 32;

//             Item.autoReuse = false;
//             Item.noUseGraphic = true; // The sword is actually a "projectile", so the item should not be visible when used
//             Item.noMelee = true; // The projectile will do the damage and not the item

//             Item.rare = ItemRarityID.White;
//             Item.value = Item.sellPrice(0, 0, 0, 10);

//             Item.shoot = ModContent.ProjectileType<DeiticStaffProj>(); // The projectile is what makes a shortsword work
//             Item.shootSpeed = 2.1f;
//         }
//     }

//     public class DeiticStaffProj : ModProjectile
//     {
//         public override void SetStaticDefaults()
//         {   
//             // Prevents jitter when stepping up and down blocks and half blocks
//             ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
//         }

//         public override void SetDefaults()
//         {
//             Projectile.width = 22;
//             Projectile.height = 22;
//             Projectile.tileCollide = false;
//             Projectile.penetrate = -1;
//             Projectile.aiStyle = -1; // Replace with 20 if you do not want custom code
//             Projectile.hide = true; // Hides the projectile, so it will draw in the player's hand when we set the player's heldProj to this one.
//         }

//         public float Charge {get => Projectile.ai[0];set => Projectile.ai[0] = value;}
// 		public float Phase {get => Projectile.ai[1];set => Projectile.ai[1] = value;}
// 		const float MaxCharge = 120f;
// 		public override void AI()
//         {
//             if (!Projectile.TryGetOwner(out Player player))
//             {
//                 Projectile.Kill();
//                 return;
//             }

// 			bool shouldDed = Phase == 1f ? false : !player.channel;
            
// 			if (!player.active || player.dead || player.noItems || player.CCed || shouldDed)
//             {
//                 Projectile.Kill();
//                 return;
//             }

// 			UpdateGraphic(player);
			
// 			Projectile.timeLeft = 16;

// 			if (Phase == 0f)
//             {
// 				Charge += 1f;
// 				if (Charge > MaxCharge)
//                 {
// 					// for (int i = -1; i < 2; i++)
//                     // {
// 					// 	Vector2 pos = Projectile.Center + Projectile.velocity;
// 					// 	Vector2 speed = (Projectile.velocity*2f).RotatedBy(MathHelper.ToRadians(i*10));
// 					// 	int a = Projectile.NewProjectile(pos,speed,ModContent.ProjectileType<starwalkerStarFriendly>(),projectile.damage,1,player.whoAmI);
// 					// 	Main.projectile[a].penetrate = 1;
// 					// 	NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, a);
// 					// }

// 					Utils.PoofOfSmoke(Projectile.Center);
// 					// player.CameraShake(10);
// 					// Main.PlaySound(Deltarune.GetSound("scytheburst"),projectile.Center);
// 					Phase = 1f;
// 				}
// 			}
// 			else
//             {
// 				Charge -= 2f;
// 				if (Charge <= 0f)
//                 {
// 					Projectile.Kill();
// 				}
// 			}

// 			float num = (Charge/(MaxCharge/2f))+1f;
// 			if (num > 1.5f) {num = 1.5f;}
// 			Projectile.scale = num;
// 		}

// 		void UpdateGraphic(Player player)
//         {
// 			player.heldProj = Projectile.whoAmI;
// 			player.SetDummyItemTime(2);
//             player.ChangeDir(Projectile.direction);
// 			Projectile.Center = player.Center;

//             // 	Projectile.spriteDirection = Projectile.direction;
//             // 	player.ChangeDir(Projectile.direction); // Change the player's direction based on the projectile's own
//             // 	player.heldProj = Projectile.whoAmI; // We tell the player that the drill is the held projectile, so it will draw in their hand
//             // 	player.SetDummyItemTime(2); // Make sure the player's item time does not change while the projectile is out
//             // 	Projectile.Center = playerCenter; // Centers the projectile on the player. Projectile.velocity will be added to this in later Terraria code causing the projectile to be held away from the player at a set distance.
//             // 	Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
//             // 	player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();
            
// 			if (Phase == 0f)
//             {
//                 if (Projectile.owner == Main.myPlayer)
//                 {
//                     Projectile.rotation = Projectile.AngleTo(Main.MouseWorld);
//                     Projectile.velocity = Projectile.DirectionTo(Main.MouseWorld) * 10f;
//                     player.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * player.direction, Projectile.velocity.X * player.direction);
//                     Projectile.Center += Projectile.velocity * 2;
//                     Projectile.netUpdate = true;
//                 }
//                 if (Projectile.soundDelay <= 0)
//                 {
//                     Projectile.soundDelay = 30;
//                     //if (Charge > 1f) {Main.PlaySound(SoundID.Item15, projectile.position);}
//                 }
//             }
//             else
//             {
//                 player.itemRotation += player.direction == -1 ? 0.1f : -0.1f;
//                 Projectile.rotation += player.direction == -1 ? 0.1f : -0.1f;
//             }
// 		}
// 		public override bool ShouldUpdatePosition() => false;

// 		// This code is adapted and simplified from aiStyle 20 to use a different dust and more noises. If you want to use aiStyle 20, you do not need to do any of this.
//         // It should be noted that this projectile has no effect on mining and is mostly visual.
//         // public override void AI()
//         // {
//         // 	Player player = Main.player[Projectile.owner];

//         // 	Projectile.timeLeft = 60;

//         // 	// Animation code could go here if the projectile was animated. 

//         // 	// Plays a sound every 20 ticks. In aiStyle 20, soundDelay is set to 30 ticks.
//         // 	if (Projectile.soundDelay <= 0)
//         //     {
//         // 		SoundEngine.PlaySound(SoundID.Item22, Projectile.Center);
//         // 		Projectile.soundDelay = 20;
//         // 	}

//         // 	Vector2 playerCenter = player.RotatedRelativePoint(player.MountedCenter);

//         // 	if (Main.myPlayer == Projectile.owner)
//         //     {
//         //         // This code must only be ran on the client of the projectile owner
//         //         if (player.channel)
//         //         {
//         //             float holdoutDistance = player.HeldItem.shootSpeed * Projectile.scale;
//         //             // Calculate a normalized vector from player to mouse and multiply by holdoutDistance to determine resulting holdoutOffset
//         //             Vector2 holdoutOffset = holdoutDistance * Vector2.Normalize(Main.MouseWorld - playerCenter);
//         //             if (holdoutOffset.X != Projectile.velocity.X || holdoutOffset.Y != Projectile.velocity.Y)
//         //             {
//         //                 // This will sync the projectile, most importantly, the velocity.
//         //                 Projectile.netUpdate = true;
//         //             }

//         //             // Projectile.velocity acts as a holdoutOffset for held projectiles.
//         //             Projectile.velocity = holdoutOffset;
//         //         }
//         //         else
//         //         {
//         //             Projectile.Kill();
//         //         }
//         //     }

//         // 	if (Projectile.velocity.X > 0f)
//         //     {
//         // 		player.ChangeDir(1);
//         // 	}
//         // 	else if (Projectile.velocity.X < 0f)
//         //     {
//         // 		player.ChangeDir(-1);
//         // 	}

//         // 	Projectile.spriteDirection = Projectile.direction;
//         // 	player.ChangeDir(Projectile.direction); // Change the player's direction based on the projectile's own
//         // 	player.heldProj = Projectile.whoAmI; // We tell the player that the drill is the held projectile, so it will draw in their hand
//         // 	player.SetDummyItemTime(2); // Make sure the player's item time does not change while the projectile is out
//         // 	Projectile.Center = playerCenter; // Centers the projectile on the player. Projectile.velocity will be added to this in later Terraria code causing the projectile to be held away from the player at a set distance.
//         // 	Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
//         // 	player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();

//         // 	// Gives the drill a slight jiggle
//         // 	Projectile.velocity.X *= 1f + Main.rand.Next(-3, 4) * 0.01f;

//         // 	// Spawning dust
//         // 	// if (Main.rand.NextBool(10)) {
//         // 	// 	Dust dust = Dust.NewDustDirect(Projectile.position + Projectile.velocity * Main.rand.Next(6, 10) * 0.15f, Projectile.width, Projectile.height, ModContent.DustType<Sparkle>(), 0f, 0f, 80, Color.White, 1f);
//         // 	// 	dust.position.X -= 4f;
//         // 	// 	dust.noGravity = true;
//         // 	// 	dust.velocity.X *= 0.5f;
//         // 	// 	dust.velocity.Y = -Main.rand.Next(3, 8) * 0.1f;
//         // 	// }
//         // }
//     }
// }