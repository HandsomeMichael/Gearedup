// using System.IO;
// using Terraria;
// using Terraria.ID;
// using Terraria.ModLoader;
// using Terraria.ModLoader.IO;

// namespace Gearedup.Content.Crossmod
// {
//     public class RuneProj : GlobalProjectile
//     {

//         public override void SetDefaults(Projectile entity)
//         {
//             homeTimer = homeTimerMax;
//         }
//         public override bool IsLoadingEnabled(Mod mod)
//         {
//             return ModLoader.HasMod("CalamityMod");
//         }
//         public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
//         {
//             return entity.ModProjectile != null && entity.ModProjectile.Mod.Name == "CalamityMod" && entity.ModProjectile.Name == "Runes";
//         }


//         public const byte homeTimerMax = 30;
//         public byte homeTimer;

//         public override bool InstancePerEntity => true;

//         public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
//         {
//             binaryWriter.Write(homeTimer);
//         }

//         public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
//         {
//             homeTimer = binaryReader.ReadByte();
//         }

//         public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
//         {

//             if (projectile.TryGetOwner(out Player player) && player != null && player.active && !player.dead && player.TryGetModPlayer(out GearPlayer gearPlayer) && gearPlayer.primateGift)
//             {
//                 switch (projectile.ModProjectile.Name)
//                 {
//                     case "Magic":
//                         target.AddBuff(BuffID.Poisoned, 60);
//                         break;
//                     case "Melee":
//                         target.AddBuff(BuffID.OnFire, 60);
//                         break;
//                     case "Ranged":
//                         target.AddBuff(BuffID.Frostburn, 60);
//                         break;
//                     default: break;
//                 }
//             }
//         }

//         public override void AI(Projectile projectile)
//         {
//             // primate gifts effect
//             if (projectile.TryGetOwner(out Player player) && player != null && player.active && !player.dead && player.TryGetModPlayer(out GearPlayer gearPlayer) && gearPlayer.primateGift)
//             {
//                 homeTimer++;
//                 if (homeTimer > homeTimerMax)
//                 {
//                     float distance = -1f;
//                     for (int i = 0; i < Main.maxNPCs; i++)
//                     {
//                         if (Main.npc[i].CanBeChasedBy(projectile) && (projectile.DistanceSQ(Main.npc[i].Center) < distance || distance == -1f))
//                         {
//                             projectile.velocity = projectile.velocity.Length() * projectile.DirectionTo(Main.npc[i].Center);
//                         }
//                     }
//                     homeTimer = 0;
//                 }
//             }
//         }
//     }
// }