using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using Gearedup.Content.Calamity;
using Gearedup.Content.Items;
using Gearedup.Helper;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup
{
    public class GearPlayer : ModPlayer
    {
        // public byte weight;
        // public float weightEffective; // in percent
        // public const byte maxWeight = 250;

        public int universalDye;
        public bool phasingDevice;
        public bool phasingDeviceLunar;
        public bool haveCultFollowing;
        public bool catPissed;
        public bool getBossBag;
        public bool primateGift;
        public bool heartsOfHero;
        public byte primateGiftCD;
        public float critDamage;
        public bool truthDemise;
        public bool applyTruthDemise;
        public int truthDemiseTime;

        public List<Vector2> platform = new List<Vector2>();

        public override void ResetEffects()
        {
            phasingDevice = false;
            phasingDeviceLunar = false;
            universalDye = 0;
            haveCultFollowing = false;
            catPissed = false;
            getBossBag = false;
            primateGift = false;
            heartsOfHero = false;
            critDamage = 0f;
            truthDemise = false;
            // timer
            if (primateGiftCD > 0) primateGiftCD--;
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

        // public override void ModifyMaxStats(out StatModifier health, out StatModifier mana) {
        // 	health = StatModifier.Default;
        // 	health.Base = exampleLifeFruits * ExampleLifeFruit.LifePerFruit;
        // 	// Alternatively:  health = StatModifier.Default with { Base = exampleLifeFruits * ExampleLifeFruit.LifePerFruit };
        // 	mana = StatModifier.Default;
        // 	mana.Base = exampleManaCrystals * ExampleManaCrystal.ManaPerCrystal;
        // 	// Alternatively:  mana = StatModifier.Default with { Base = exampleManaCrystals * ExampleManaCrystal.ManaPerCrystal };
        // }

        public override void PostUpdateBuffs()
        {
            // halfen max life
            if (truthDemise)
            {
                Player.statLifeMax2 /= 2;
            }
        }
        public override void OnRespawn()
        {
            if (applyTruthDemise)
            {
                Player.AddBuff<TruthDemise>(truthDemiseTime);
                Player.AddBuff(BuffID.Obstructed, 60 * 10);
                applyTruthDemise = false;
            }
        }
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            applyTruthDemise = truthDemise;
        }

        public override float UseSpeedMultiplier(Item item)
        {
            if (primateGift)
            {
                // increase 20% attack speed
                float speedIncrease = 0.2f;
                return 1f + (speedIncrease - (speedIncrease * Player.statLife / Player.statLifeMax2));
            }
            return base.UseSpeedMultiplier(item);
        }

        // public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
        // {
        //     return base.AddStartingItems(mediumCoreDeath);
        // }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // blast the spark at em
            if (primateGift && primateGiftCD <= 0 && hit.Crit && Gearedup.Get.calamityMod != null)
            {
                int sparkType = Gearedup.Get.calamityMod.Find<ModProjectile>("Spark").Type;

                var source = Player.GetSource_Accessory(ContentSamples.ItemsByType[ModContent.ItemType<PrimateGifts>()]);

                SoundEngine.PlaySound(SoundID.Item93, target.Center);

                float spread = MathHelper.ToRadians(45f);
                double startAngle = Math.Atan2(Player.velocity.X, Player.velocity.Y) - spread / 2;
                double deltaAngle = spread / 8f;

                if (Player.whoAmI == Main.myPlayer)
                {

                    int sparkDamage = (int)Player.GetTotalDamage(DamageClass.Generic).ApplyTo(6);

                    for (int i = 0; i < 4; i++)
                    {
                        double offsetAngle = startAngle + deltaAngle * (i + i * i) / 2f + 32f * i;
                        Vector2 velocity1 = new Vector2((float)Math.Sin(offsetAngle), (float)Math.Cos(offsetAngle)) * 5f;
                        Vector2 velocity2 = -velocity1;

                        Projectile.NewProjectile(source, target.Center, velocity1, sparkType, sparkDamage, 1.25f, Player.whoAmI);
                        Projectile.NewProjectile(source, target.Center, velocity2, sparkType, sparkDamage, 1.25f, Player.whoAmI);
                    }
                }

                primateGiftCD = 120;
            }

            // spawn runic projectile
            if (target.life <= 0)
            {
                if (primateGift && Gearedup.Get.calamityMod != null)
                {
                    // if (!Gearedup.Get.calamityMod.TryFind<ModProjectile>("LuxorsGiftRanged", out ModProjectile modProjectile)) 


                    float distance = 0f;
                    int index = -1;
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        var npc = Main.npc[i];
                        var curDist = npc.Distance(Player.Center);
                        if (npc.CanBeChasedBy(Player) && (curDist < distance || index == -1))
                        {
                            distance = curDist;
                            index = i;
                        }
                    }

                    if (index != -1)
                    {
                        if (hit.DamageType == DamageClass.Ranged)
                        {
                            if (Gearedup.Get.calamityMod.TryFind<ModProjectile>("LuxorsGiftRanged", out ModProjectile luxorGiftProj))
                            {
                                Projectile.NewProjectile(target.GetSource_FromAI(),
                                target.Center,
                                target.DirectionTo(Main.npc[index].Center),
                                luxorGiftProj.Type, 20, 0f, Player.whoAmI);
                            }
                        }

                        if (hit.DamageType == DamageClass.Melee)
                        {
                            if (Gearedup.Get.calamityMod.TryFind<ModProjectile>("LuxorsGiftRanged", out ModProjectile luxorGiftProj))
                            {
                                Projectile.NewProjectile(target.GetSource_FromAI(),
                                target.Center,
                                target.DirectionTo(Main.npc[index].Center),
                                luxorGiftProj.Type, 20, 0f, Player.whoAmI);
                            }
                        }

                        if (hit.DamageType == DamageClass.Magic)
                        {
                            if (Gearedup.Get.calamityMod.TryFind<ModProjectile>("LuxorsGiftRanged", out ModProjectile luxorGiftProj))
                            {
                                Projectile.NewProjectile(target.GetSource_FromAI(),
                                target.Center,
                                target.DirectionTo(Main.npc[index].Center),
                                luxorGiftProj.Type, 20, 0f, Player.whoAmI);
                            }
                        }
                    }
                }

                if (target.TryGetGlobalNPC<GearNPCs>(out GearNPCs gn))
                {
                    if (gn.doubleLoot)
                    {
                        // try dropping twice
                        DropAttemptInfo info = new()
                        {
                            player = Player,
                            npc = target,
                            IsExpertMode = Main.expertMode,
                            IsMasterMode = Main.masterMode,
                            IsInSimulation = false,
                            rng = Main.rand,
                        };
                        var prevLuck = Player.luck;
                        Player.luck = Player.luckMaximumCap * 2f; // idk if this will work or not
                        Main.ItemDropSolver.TryDropping(info);
                        Player.luck = prevLuck;
                    }
                }
            }

            if (catPissed)
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

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // increase crit damage
            modifiers.CritDamage += critDamage;

            if (heartsOfHero)
            {
                if (modifiers.DamageType != DamageClass.Summon)
                {
                    modifiers.ScalingBonusDamage += Player.slotsMinions * 0.03f; // grants 3% damage bonus foreach spawned minion
                }
                else
                {
                    float minionUsed = ((float)Player.maxMinions) - Player.slotsMinions;
                    modifiers.ScalingBonusDamage += minionUsed * 0.08f; // grants 8% damage foreach empty minion slots
                }
            }

            if (catPissed)
            {
                if (target.HasAnyBuff(BuffID.CursedInferno, BuffID.Poisoned, BuffID.Venom, BuffID.Confused, BuffID.OnFire, BuffID.Frostburn))
                {
                    modifiers.ScalingBonusDamage += 0.05f;
                }
                else
                {
                    modifiers.ScalingBonusDamage += 0.07f;
                }
            }
        }

        public override void MeleeEffects(Item item, Rectangle hitbox)
        {
            if (primateGift)
            {
                // Define animation steps and their corresponding effects
                var steps = new[]
                {
                    new { Step = 0.9, XVel = 0f, YVel = -7f, XOffset = 0f, YOffset = 0f },
                    new { Step = 0.7, XVel = 2f, YVel = -6f, XOffset = 26f, YOffset = 0f },
                    new { Step = 0.5, XVel = 4f, YVel = -4f, XOffset = 0f, YOffset = 0f },
                    new { Step = 0.3, XVel = 6f, YVel = -2f, XOffset = -4f, YOffset = -20f },
                    new { Step = 0.1, XVel = 7f, YVel = 0f, XOffset = 0f, YOffset = 6f }
                };

                foreach (var s in steps)
                {
                    int animStep = (int)(Player.itemAnimationMax * s.Step);
                    if (Player.itemAnimation == animStep)
                    {
                        float xVel = s.XVel * 1.5f * Player.direction;
                        float yVel = s.YVel * 1.5f * Player.gravDir;
                        float xOffset = s.XOffset * Player.direction;
                        float yOffset = s.YOffset * Player.gravDir;

                        // Additional offsets for direction
                        if (Player.direction == -1)
                        {
                            if (s.Step == 0.9) xOffset -= 8f;
                            if (s.Step == 0.7) xOffset -= 6f;
                        }

                        Vector2 spawnPos = new Vector2(
                            hitbox.X + hitbox.Width / 2 + xOffset,
                            hitbox.Y + hitbox.Height / 2 + yOffset
                        );

                        var proj = Projectile.NewProjectileDirect(item.GetSource_FromThis(), spawnPos, new Vector2(xVel, yVel), ProjectileID.Mushroom, 5, 0f, Player.whoAmI);
                        if (proj != null && proj.active)
                        {
                            proj.penetrate = 1;
                            proj.usesLocalNPCImmunity = true;
                            proj.localNPCHitCooldown = 20;
                        }
                    }
                }
            }
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            if (haveCultFollowing)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    if (Main.npc[i].active && Main.npc[i].TryGetGlobalNPC(out BrainWashedNPC br))
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
                    if (Main.npc[i].active && Main.npc[i].TryGetGlobalNPC(out BrainWashedNPC br))
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