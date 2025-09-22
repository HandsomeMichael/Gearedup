using System;
using System.IO;
using Gearedup.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.CompilerServices.SymbolWriter;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;

namespace Gearedup.Content.Items
{
    public class BrainWashedProj : GlobalProjectile
    {
        public short ownedBy = -1;
        public override bool InstancePerEntity => true;
        protected override bool CloneNewInstances => true;
        public override bool IsLoadingEnabled(Mod mod)
        {
            return GearServerConfig.Get.Content_DeityStaff;
        }

        public override bool CanHitPlayer(Projectile projectile, Player target)
        {
            if (target.whoAmI == ownedBy)
            {
                return false;
            }
            return base.CanHitPlayer(projectile, target);
        }

        public override bool? CanHitNPC(Projectile projectile, NPC target)
        {
            if (target.TryGetGlobalNPC(out BrainWashedNPC bw))
            {
                if (bw.ownedBy != -1 && bw.ownedBy == ownedBy)
                {
                    return false;
                }
            }
            return null;
        }

        // inherit brain wash
        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (source is EntitySource_Parent entitySource)
            {
                // inherit from npc
                if (entitySource.Entity is NPC npcSource)
                {
                    if (npcSource.TryGetGlobalNPC(out BrainWashedNPC bw))
                    {
                        if (bw.ownedBy != -1 && projectile.TryGetGlobalProjectile(out BrainWashedProj bwr))
                        {
                            bwr.ownedBy = bw.ownedBy;
                            projectile.hostile = true;
                            projectile.friendly = true;
                        }
                    }
                }
                // inherit from projectiles
                else if (entitySource.Entity is Projectile proj)
                {
                    if (proj.TryGetGlobalProjectile(out BrainWashedProj bp))
                    {
                        if (bp.ownedBy != -1 && projectile.TryGetGlobalProjectile(out BrainWashedProj bpr))
                        {
                            bpr.ownedBy = bp.ownedBy;
                            projectile.hostile = true;
                            projectile.friendly = true;
                        }
                    }
                }
            }
        }
    }

    public class BrainWashedNPC : GlobalNPC
    {
        /// <summary>
        /// Pull it outta my ass
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Color GetRarityColorByNPCValue(int value)
        {
            if (value < 1000) return new Color(180, 180, 180); // Common (Gray)
            if (value < 5000) return new Color(0, 255, 0); // Uncommon (Green)
            if (value < 15000) return new Color(0, 112, 221); // Rare (Blue)
            if (value < 50000) return new Color(163, 53, 238); // Epic (Purple)
            return new Color(255, 128, 0); // Legendary (Orange)
        }

        public static bool CanBeBrainwashed(NPC npc)
        {
            return !npc.friendly && npc.type != NPCID.TargetDummy;
        }

        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return CanBeBrainwashed(entity);
        }

        public short ownedBy = -1;
        public bool foundTarget;
        public override bool InstancePerEntity => true;
        protected override bool CloneNewInstances => true;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            if (ownedBy != -1)
            {
                binaryWriter.Write(true);
                binaryWriter.Write(ownedBy);
                binaryWriter.Write(foundTarget);
            }
            else
            {
                binaryWriter.Write(false);
            }
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            // should read more data
            if (binaryReader.ReadBoolean())
            {
                ownedBy = binaryReader.ReadInt16();
                foundTarget = binaryReader.ReadBoolean(); 
            }
        }

        public override bool IsLoadingEnabled(Mod mod)
        {
            return GearServerConfig.Get.Content_DeityStaff;
        }

        public static Vector2? resetPlayerPos;
        //public static bool? resetNoGravity;

        public override bool PreAI(NPC npc)
        {
            // prevent random value change if AI fails
            resetPlayerPos = null;
            //resetNoGravity = null;

            if (ownedBy != -1)
            {
                var player = Main.player[ownedBy];
                // instantly kill if the guy die
                if (!player.active || player.dead)
                {
                    npc.StrikeInstantKill();
                    return base.PreAI(npc);
                }

                if (player.TryGetModPlayer(out GearPlayer gr))
                {
                    gr.haveCultFollowing = true;
                }

                // discourage despawn
                npc.DiscourageDespawn(600);

                // always target you
                npc.target = ownedBy;
                foundTarget = false;

                resetPlayerPos = player.Center;
                //resetNoGravity = npc.noGravity;
                // npc.noGravity = true;

                float distance = 0f;
                int index = -1;

                // call each frame, might not be performant but whatevs
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC target = Main.npc[i];
                    float newDist = target.Distance(player.Center);
                    bool maiEnemi = true;
                    if (target.TryGetGlobalNPC(out BrainWashedNPC brainWashed))
                    {
                        if (brainWashed.ownedBy == ownedBy)
                        {
                            maiEnemi = false;
                        }
                    }
                    if (i != npc.whoAmI && maiEnemi && target.active && target.CanBeChasedBy(npc) && newDist < 2000f && ((newDist < distance) || index == -1))
                    {
                        index = i;
                        distance = newDist;
                    }
                }

                if (index != -1)
                {
                    Dust.NewDust(Main.npc[index].Center, 10, 10, DustID.BatScepter);
                    player.Center = Main.npc[index].Center;
                    foundTarget = true;
                }
                else
                {
                    // dont run ai, instead it will just follow the player
                    if (npc.Distance(player.Center) > 500f)
                    {
                        npc.Center = npc.Center.Lerp(player.Center, 0.1f);
                    }
                    npc.velocity = Vector2.Zero;
                    return false;
                }
            }
            return base.PreAI(npc);
        }

        public override bool CheckActive(NPC npc)
        {

            if (ownedBy != -1)
            {
                // reset gravity
                //if (resetNoGravity.HasValue) npc.noGravity = resetNoGravity.Value;
            }
            return base.CheckActive(npc);
        }

        public override void PostAI(NPC npc)
        {
            if (ownedBy != -1)
            {
                if (resetPlayerPos.HasValue) Main.player[ownedBy].Center = resetPlayerPos.Value;

                // no fucked up defense value
                npc.defense = Math.Min(5, npc.defense);
                npc.value = 0; // no value
                npc.SpawnedFromStatue = true;

                if (!foundTarget)
                {
                    npc.velocity = Vector2.Zero;
                }
            }
        }

        public override void AI(NPC npc)
        {
            if (ownedBy != -1 && foundTarget)
            {
                AttackOther(npc);
            }
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (ownedBy != -1 && !foundTarget)
            {
                var thatTexture = Main.Assets.Request<Texture2D>("Images/Extra_55").Value;
                int frame = Helpme.MagicallyGetFrame(4, 10);
                var rect = thatTexture.GetVerticalFrame(frame, 4);
                var size = new Vector2(npc.width, npc.height) * npc.scale;
                var scale = Math.Min(size.X / rect.Width, size.Y / rect.Height) * 2f;

                Main.spriteBatch.Draw(thatTexture, npc.Center - Main.screenPosition, rect, Color.White, 0f, rect.Size() / 2f * scale, scale, SpriteEffects.None, 0f);

                return false;
            }
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (ownedBy != -1 && Main.myPlayer == ownedBy)
            {
                // Friendly
                const float scale = 0.8f;
                string text = npc.FullName;
                var size = Helpme.MeasureString(text);

                ChatManager.DrawColorCodedStringWithShadow(spriteBatch,
                FontAssets.MouseText.Value,
                text,
                npc.Top - Main.screenPosition + new Vector2(0, (float)(Math.Sin(Main.GameUpdateCount * 0.1) * 0.1)),
                Color.MediumPurple, 0f, size / 2f, Vector2.One * scale);
            }
        }

        // no hitting hitting
        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            if (ownedBy != -1)
            {
                return target.whoAmI != ownedBy;
            }
            return base.CanHitPlayer(npc, target, ref cooldownSlot);
        }

        public void AttackOther(NPC npc)
        {
            int specialHitSetter = 1;
            float damageMultiplier = 1f;
            Rectangle hitbox = npc.Hitbox;

            for (int i = 0; i < 200; i++)
            {
                NPC target = Main.npc[i];
                bool maiEnemi = true;
                if (i != npc.whoAmI && target.TryGetGlobalNPC(out BrainWashedNPC br))
                {
                    if (br.ownedBy == ownedBy)
                    {
                        maiEnemi = false;
                    }
                }
                if (i != npc.whoAmI && maiEnemi && target.active && (!target.friendly || NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[target.type]))
                {
                    Rectangle targetRect = target.Hitbox;
                    NPC.GetMeleeCollisionData(hitbox, i, ref specialHitSetter, ref damageMultiplier, ref targetRect);

                    if (hitbox.Intersects(targetRect))
                    {
                        if (target.immune[255] <= 0) { BeHurtByOtherNPC(target, npc); }
                        if (target.damage > 0 && npc.immune[255] <= 0) { BeHurtByOtherNPC(npc, target); }
                        break;
                    }
                }
            }
        }

        private void BeHurtByOtherNPC(NPC self, NPC thatNPC)
        {
            int num = 30;
            if (self.type == NPCID.DD2EterniaCrystal)
            {
                num = 20;
            }
            int num3 = 6;
            int num4 = ((!(thatNPC.Center.X > self.Center.X)) ? 1 : (-1));
            NPC.HitModifiers modifiers = self.GetIncomingStrikeModifiers(DamageClass.Default, num4);
            NPCLoader.ModifyHitNPC(thatNPC, self, ref modifiers);
            NPC.HitInfo strike = modifiers.ToHitInfo(thatNPC.damage, crit: false, num3, damageVariation: true);
            double num5 = self.StrikeNPC(strike, fromNet: false, noPlayerInteraction: true);

            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                NetMessage.SendStrikeNPC(self, in strike);
            }

            self.netUpdate = true;
            self.immune[255] = num;
            NPCLoader.OnHitNPC(thatNPC, self, in strike);
            int num2 = strike.SourceDamage;
            if (self.dryadWard)
            {
                num2 = (int)num5 / 3;
                num3 = 6;
                num4 *= -1;
                thatNPC.SimpleStrikeNPC(num2, num4, false, num3);
                // thatNPC.StrikeNPCNoInteraction(num2, num3, num4);
                if (Main.netMode != NetmodeID.SinglePlayer)
                {
                    NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, thatNPC.whoAmI, num2, num3, num4);
                }
                thatNPC.netUpdate = true;
                thatNPC.immune[255] = num;
            }
            if (NPCID.Sets.HurtingBees[thatNPC.type])
            {
                num2 = self.damage;
                num3 = 6;
                num4 *= -1;

                thatNPC.SimpleStrikeNPC(num2, num4, false, num3);

                if (Main.netMode != NetmodeID.SinglePlayer)
                {
                    NetMessage.SendData(MessageID.DamageNPC, -1, -1, null, thatNPC.whoAmI, num2, num3, num4);
                }
                thatNPC.netUpdate = true;
                thatNPC.immune[255] = num;
            }
        }

        public override bool? CanBeHitByItem(NPC npc, Player player, Item item)
        {
            if (ownedBy != -1 && player.whoAmI == ownedBy)
            {
                return false;
            }
            return null;
        }

        public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
        {
            if (ownedBy != -1)
            {
                // can be hit by anyother projectile except owner projectile
                if (projectile.owner == ownedBy) { return false; }
                return true;
            }
            return null;
        }

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            //NPCLoader.SpawnNPC
            // brainwash each other yeah
            if (source is EntitySource_Parent entitySource)
            {
                if (entitySource.Entity is NPC parent)
                {
                    if (parent.TryGetGlobalNPC(out BrainWashedNPC br))
                    {
                        if (br.ownedBy != -1)
                        {
                            // idk if this work lets see
                            ownedBy = br.ownedBy;
                            // if (npc.TryGetGlobalNPC(out BrainWashedNPC npcBr))
                            // {
                            //     npcBr.ownedBy = br.ownedBy;
                            // }
                        }
                    }
                }

                if (entitySource.Entity is Projectile proj)
                {
                    if (proj.TryGetGlobalProjectile<BrainWashedProj>(out BrainWashedProj br))
                    {
                        if (br.ownedBy != -1)
                        {
                            ownedBy = br.ownedBy;
                        }
                    }
                }
            }
        }
    }
}