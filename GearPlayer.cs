using System;
using System.Collections.Generic;
using System.IO;
using Gearedup.Content.Items;
using Gearedup.Helper;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup
{
    public class GearPlayer : ModPlayer
    {
        public short universalDye;
        public bool phasingDevice;
        public bool phasingDeviceLunar;
        public bool haveCultFollowing;
        public bool catPissed;
        public bool getBossBag;

        public List<Vector2> platform = new List<Vector2>();

        public override void ResetEffects()
        {
            phasingDevice = false;
            phasingDeviceLunar = false;
            universalDye = 0;
            haveCultFollowing = false;
            catPissed = false;
            getBossBag = false;
        }

        // public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        // {
        //     ModPacket packet = Mod.GetPacket();
        //     packet.Write((byte)Gearedup.MessageType.GearPlayerSync);
        //     packet.Write((byte)Player.whoAmI);

        //     SendPlayerSync(packet);

        //     packet.Send(toWho, fromWho);
        // }

        // public void SendPlayerSync(ModPacket packet)
        // {
        //     packet.Write((byte)lunarDamage);
        // }

        // public void ReceivePlayerSync(BinaryReader reader)
        // {
        //     lunarDamage = reader.ReadByte();
        // }

        // public override void CopyClientState(ModPlayer targetCopy) {
        // 	GearPlayer clone = (GearPlayer)targetCopy;
        // 	clone.lunarDamage = lunarDamage;
        // }

        // public override void SendClientChanges(ModPlayer clientPlayer) {
        // 	GearPlayer clone = (GearPlayer)clientPlayer;

        // 	if (lunarDamage != clone.lunarDamage)
        // 		SyncPlayer(toWho: -1, fromWho: Main.myPlayer, newPlayer: false);
        // }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (catPissed && proj.DamageType == DamageClass.Ranged)
            {
                if (Main.rand.NextBool(5))
                {
                    target.AddBuff(BuffID.CursedInferno, hit.Damage * 2);
                }
                if (Main.rand.NextBool(6))
                {
                    target.AddBuff(BuffID.Ichor, hit.Damage * 2);
                }
            }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (catPissed)
            {
                if (proj.DamageType == DamageClass.Ranged &&
                target.HasAnyBuff(BuffID.CursedInferno, BuffID.Poisoned, BuffID.Venom, BuffID.Confused, BuffID.OnFire, BuffID.Frostburn))
                {
                    modifiers.ScalingBonusDamage += 0.05f;
                }
            }
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            if (haveCultFollowing)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && Main.npc[i].TryGetGlobalNPC<BrainWashedNPC>(out BrainWashedNPC br))
                    {
                        if (br.ownedBy != -1 && br.ownedBy == Player.whoAmI)
                        {
                            Main.npc[i].SimpleStrikeNPC(hurtInfo.Damage, hurtInfo.HitDirection, false, hurtInfo.Knockback);
                        }
                    }
                }
            }
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            if (haveCultFollowing)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && Main.npc[i].TryGetGlobalNPC<BrainWashedNPC>(out BrainWashedNPC br))
                    {
                        if (br.ownedBy != -1 && br.ownedBy == Player.whoAmI)
                        {
                            Main.npc[i].SimpleStrikeNPC(hurtInfo.Damage, hurtInfo.HitDirection, false, hurtInfo.Knockback);
                        }
                    }
                }
            }
        }

        public override void PostUpdateMiscEffects()
        {
            if (phasingDevice && Player.holdDownCardinalTimer[0] > 0)
            {
                // safe code
                if (platform == null) { platform = new List<Vector2>(); }

                // credit to fargowiltas mod for this code
                int width = 2;
                if (Player.mount.Active)
                {
                    // offset by 2
                    width = ((int)Player.mount._data.textureWidth / 16);
                }
                for (int i = -width; i <= width; i++)
                {
                    Vector2 pos = Player.Center;
                    pos.X += i * 16;
                    pos.Y += Player.height / 2;
                    pos.Y += 8;

                    // foreach (var item in offset) { pos += item(Player, i); }

                    Tile tile = Framing.GetTileSafely((int)(pos.X / 16), (int)(pos.Y / 16));

                    if (!tile.IsActuated && IsPlatform(tile.TileType))
                    {
                        // Main.NewText("Actuated");
                        tile.IsActuated = true;
                        platform.Add(pos);
                    }
                }

                Player.waterWalk = false;
                Player.waterWalk2 = false;

                if (platform.Count > 0 && phasingDeviceLunar)
                {
                    Player.wingTime = Player.wingTimeMax; // reset wing time
                    Player.velocity *= 1.2f; // increase player velocity by 20% during this
                    Player.endurance += 0.01f; // have slight dr
                }

                // Legacy way

                // Tile thisTile = Framing.GetTileSafely(Player.Bottom);
                // Tile bottomTile = Framing.GetTileSafely(Player.Bottom + Vector2.UnitY * 8);

                // if (!Collision.SolidCollision(Player.BottomLeft, Player.width, 16))
                // {
                //     if (Player.velocity.Y >= 0 && (IsPlatform(thisTile.TileType) || IsPlatform(bottomTile.TileType)))
                //     {
                //         // i added +1 just incase idk
                //         Player.position.Y += 3;
                //     }
                //     if (Player.velocity.Y == 0)
                //     {
                //         Player.position.Y += 16;
                //     }

                // }

                static bool IsPlatform(int tileType)
                {
                    return TileID.Sets.Platforms[tileType] || tileType == TileID.Platforms || tileType == TileID.PlanterBox;
                }
            }
        }

        public override void PostUpdate()
        {
            foreach (var pos in platform)
            {
                var tile = Framing.GetTileSafely((int)(pos.X / 16), (int)(pos.Y / 16));
                tile.IsActuated = false;
            }
            platform.Clear();
        }
    }
}