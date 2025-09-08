using System;
using System.Collections.Generic;
using Gearedup.Content.Catched;
using Gearedup.Helper;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using static Terraria.NPC;

namespace Gearedup.Content.Items
{
    
    // TO DO : Change this into Staff of Deitic
    public class DeveloGun : ModItem
    {
        int selectedType = 0;
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Handgun);
            Item.useAmmo = ModContent.ItemType<CatchedNPC>();
            Item.damage = 0;
            Item.DamageType = DamageClass.Generic;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            if (ammo.ModItem is CatchedNPC modItem)
            {
                selectedType = modItem.npcType.id.Value;
            }
            return base.CanConsumeAmmo(ammo, player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // && source.AmmoItemIdUsed == ModContent.ItemType<CatchedNPC>()
            if (selectedType != 0)
            {
                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<DeveloGunProj>(), damage, knockback, player.whoAmI, 0, 0, selectedType);
            }
            return false;
        }

    }

    public class DeveloGunProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_"+ItemID.WoodenCrate;
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.SnowBallFriendly);
            Projectile.friendly = false;
            Projectile.hostile = false;
        }
        public override void PostAI()
        {
            Projectile.rotation += Projectile.velocity.X > 0 ? -0.1f : 0.1f;
        }
        public override void OnKill(int timeLeft)
        {

            // Visuals from example mod, we're going barebones with this one

			// Play explosion sound
			SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
			// Smoke Dust spawn
			for (int i = 0; i < 50; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 2f);
				dust.velocity *= 1.4f;
			}

			// Fire Dust spawn
			for (int i = 0; i < 80; i++) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 3f);
				dust.noGravity = true;
				dust.velocity *= 5f;
				dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 2f);
				dust.velocity *= 3f;
			}

			// Large Smoke Gore spawn
			for (int g = 0; g < 2; g++) {
				var goreSpawnPosition = new Vector2(Projectile.position.X + Projectile.width / 2 - 24f, Projectile.position.Y + Projectile.height / 2 - 24f);
				Gore gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64));
				gore.scale = 1.5f;
				gore.velocity.X += 1.5f;
				gore.velocity.Y += 1.5f;
				gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64));
				gore.scale = 1.5f;
				gore.velocity.X -= 1.5f;
				gore.velocity.Y += 1.5f;
				gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64));
				gore.scale = 1.5f;
				gore.velocity.X += 1.5f;
				gore.velocity.Y -= 1.5f;
				gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64));
				gore.scale = 1.5f;
				gore.velocity.X -= 1.5f;
				gore.velocity.Y -= 1.5f;
			}

            var npc = NewNPCDirect(Projectile.GetSource_ReleaseEntity("Suprise matafaka"),(int)Projectile.Center.X,(int)Projectile.Center.Y,(int)Projectile.ai[2]);

            // Reduce Stats by 50%
            npc.SpawnedFromStatue = true; // no loot
            ReduceStats(ref npc.lifeMax,0.5f);
            ReduceStats(ref npc.life,0.5f);
            // ReduceStats(ref npc.damage,0.5f);
            npc.defense = Math.Min(5, npc.defense/2); // almost zero out defense
            npc.boss = false;

            // npc.friendly = true; // npc will be friendly but able to take damage

            // uhhh
            if (npc.type == NPCID.Vampire || npc.type == NPCID.VampireBat)
                SoundEngine.PlaySound(new SoundStyle("Gearedup/Sound/jonathanbanging"), Projectile.Center);

            if (GearServerConfig.Get.AllowDeveloGun_Brainwash && npc.TryGetGlobalNPC<BrainWashedNPC>(out var braining))
            {
                braining.ownedBy = Projectile.owner;
            }
        }

        void ReduceStats(ref int stats, float value, int max = 1)
        {
            // its either 1 or whatever
            stats = Math.Max((int)((float)stats * value),max);
        }
    }

    public class BrainWashedProj : GlobalProjectile
    {
        public int ownedBy = -1;
        public override bool InstancePerEntity => true;
        protected override bool CloneNewInstances => true;
        public override bool IsLoadingEnabled(Mod mod)
        {
            return GearServerConfig.Get.AllowDeveloGun_Brainwash;
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
                if (bw.ownedBy != - 1 && bw.ownedBy == ownedBy)
                {
                    return false;
                }
            }
            return null;
        }
    }

    public class BrainWashedNPC : GlobalNPC
    {
        public int ownedBy = -1;
        public override bool InstancePerEntity => true;
        protected override bool CloneNewInstances => true;

        public override bool IsLoadingEnabled(Mod mod)
        {
            return GearServerConfig.Get.AllowDeveloGun_Brainwash;
        }

        public static Vector2 resetPlayerPos;

        public override bool PreAI(NPC npc)
        {
            if (ownedBy != -1)
            {

                if (Main.player[ownedBy].TryGetModPlayer(out GearPlayer gr))
                {
                    gr.haveCultFollowing = true;
                }

                resetPlayerPos = Main.player[ownedBy].Center;

                float distance = 0f;
                int index = -1;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC target = Main.npc[i];
                    float newDist = target.Distance(Main.player[ownedBy].Center);
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
                    Main.player[ownedBy].Center = Main.npc[index].Center;
                }
            }
            return base.PreAI(npc);
        }

        public override void PostAI(NPC npc)
        {
            if (ownedBy != -1)
            {
                Main.player[ownedBy].Center = resetPlayerPos;
                // no fucked up defense value
                npc.defense = Math.Min(5, npc.defense);
            }
        }

        public override void AI(NPC npc)
        {
            if (ownedBy != -1)
            {
                AttackOther(npc);
            }
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (ownedBy != -1 && Main.myPlayer == ownedBy)
            {
                // Friendly
                string text = "Friendly " + npc.FullName;
                var size = Helpme.MeasureString(text);

                ChatManager.DrawColorCodedStringWithShadow(spriteBatch,
                FontAssets.MouseText.Value,
                text,
                npc.Top - Main.screenPosition + new Vector2(0, (float)(Math.Sin(Main.GameUpdateCount * 0.1) * 0.1)),
                Color.LightGreen, 0f, size / 2f, Vector2.One);
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
                    GetMeleeCollisionData(hitbox, i, ref specialHitSetter, ref damageMultiplier, ref targetRect);

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
            HitModifiers modifiers = self.GetIncomingStrikeModifiers(DamageClass.Default, num4);
            NPCLoader.ModifyHitNPC(thatNPC, self, ref modifiers);
            HitInfo strike = modifiers.ToHitInfo(thatNPC.damage, crit: false, num3, damageVariation: true);
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
            // brainwash each other yeah
            if (source is EntitySource_Parent entitySource)
            {
                if (entitySource.Entity is NPC parent)
                {
                    if (parent.TryGetGlobalNPC(out BrainWashedNPC br))
                    {
                        if (br.ownedBy != -1)
                        {
                            if (npc.TryGetGlobalNPC(out BrainWashedNPC npcBr))
                            {
                                npcBr.ownedBy = br.ownedBy;
                            }
                        }
                    }
                }
            }
        }
    }
}