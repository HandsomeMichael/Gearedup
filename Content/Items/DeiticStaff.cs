using System;
using Gearedup.Content.Catched;
using Gearedup.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup.Content.Items
{
    public class DeiticStaff : ModItem
    {
        int selectedType = 0;

        public override bool IsLoadingEnabled(Mod mod)
        {
            return GearServerConfig.Get.Content_DeityStaff;
        }

        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.Shoot; // Makes the player do the proper arm motion
            Item.useAnimation = 12;
            Item.useTime = 12;

            Item.width = 32;
            Item.height = 32;

            Item.autoReuse = false;
            Item.noMelee = true; // The projectile will do the damage and not the item

            Item.rare = ItemRarityID.Master;
            Item.value = Item.sellPrice(0, 15, 0, 0);

            Item.shoot = ModContent.ProjectileType<DeiticStaffProj>();
            Item.shootSpeed = 2.1f;
            Item.useAmmo = ModContent.ItemType<CatchedNPC>();
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            if (ammo.ModItem is CatchedNPC modItem)
            {
                selectedType = modItem.npcType.id.Value;
            }
            return base.CanConsumeAmmo(ammo, player);
        }
        
        public override bool AltFunctionUse(Player player) => true;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                // not work correctly in multiplayer
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    var npc = Main.npc[i];
                    if (npc.active && npc.TryGetGlobalNPC<BrainWashedNPC>(out BrainWashedNPC br))
                    {
                        if (br.ownedBy == player.whoAmI)
                        {
                            npc.life = 0;
                            npc.timeLeft = 0;
                            npc.despawnEncouraged = true;
                            npc.checkDead();
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, i);

                            // npc.life = 0;
                            // npc.checkDead();
                            var item = player.QuickSpawnItemDirect(npc.GetSource_CatchEntity(player), ModContent.ItemType<CatchedNPC>());
                            ((CatchedNPC)item.ModItem).npcType = new TypeID(npc);
                            ((CatchedNPC)item.ModItem).ReloadTypes(npc);
                            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item.whoAmI, 1f);
                        }
                    }
                }
                return false;
            }
            if (selectedType != 0)
            {
                Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, ModContent.ProjectileType<DeiticStaffProj>(), damage, knockback, player.whoAmI, selectedType, 0, 0);
            }
            player.AddBuff(BuffID.ChaosState, 60 * 60); // 1 minute cooldown to prevent spam
            return false;
        }
    }

    public class DeiticStaffProj : ModProjectile
    {
        public override string Texture => Gearedup.DotTexture;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.Opacity = 0f;
        }

        public override bool IsLoadingEnabled(Mod mod)
        {
            return GearServerConfig.Get.Content_DeityStaff;
        }

        public int NPCType { get => (int)Projectile.ai[0]; set => Projectile.ai[0] = value; }
        public float Timer { get => Projectile.ai[1]; set => Projectile.ai[1] = value; }
        public float Phase { get => Projectile.ai[2]; set => Projectile.ai[2] = value; }

        public const int maxTimerInit = 60;
        public const int maxTimerSpawn = 30;

        public override bool PreDraw(ref Color lightColor)
        {
            var npc = ContentSamples.NpcsByNetId[NPCType];
            var size = new Vector2(npc.width, npc.height) * npc.scale;
            var cultistTexture = Main.Assets.Request<Texture2D>("Images/Extra_34").Value;

            // Calculate scale so cultistTexture fits the NPC size
            var scale = Math.Min(size.X / cultistTexture.Width, size.Y / cultistTexture.Height) * 2f; // times 50%

            Main.spriteBatch.Draw(cultistTexture, Projectile.Center - Main.screenPosition,null, Color.White * Projectile.Opacity, Projectile.rotation, cultistTexture.Size() / 2f , scale, SpriteEffects.None, 0f);

            return false;
        }

        public override void AI()
        {
            Timer++;
            Projectile.rotation += 0.08f ;

            if (Projectile.Opacity < 1f)
            {
                Projectile.Opacity += 0.06f;
            }

            if (Timer > maxTimerInit)
            {
                if (Phase == 1f)
                {
                    Projectile.Kill();
                }
                else
                {
                    Phase = 1f;
                    SpawnHim();
                }
            }
        }

        public void SpawnHim()
        {
            var npc = NPC.NewNPCDirect(Projectile.GetSource_ReleaseEntity(), (int)Projectile.Center.X, (int)Projectile.Center.Y, NPCType);

            // Reduce Stats by 50%
            npc.SpawnedFromStatue = true; // no loot
            ReduceStats(ref npc.lifeMax, 0.5f);
            ReduceStats(ref npc.life, 0.5f);
            // ReduceStats(ref npc.damage,0.5f);
            npc.defense = Math.Min(5, npc.defense / 2); // almost zero out defense
            npc.boss = false;
            npc.value = 0;
            // npc.friendly = true; // npc will be friendly but able to take damage

            // removed jonathan banging sound effect
            if (npc.type == NPCID.Vampire || npc.type == NPCID.VampireBat) SoundEngine.PlaySound(new SoundStyle("Gearedup/Sounds/jonathanbanging"), Projectile.Center);

            if (npc.TryGetGlobalNPC<BrainWashedNPC>(out var braining))
            {
                braining.ownedBy = (short)Projectile.owner;
            }

            // sync hell yeah
            npc.netUpdate = true;
        }

        void ReduceStats(ref int stats, float value, int max = 1)
        {
            // its either 1 or whatever
            stats = Math.Max((int)((float)stats * value),max);
        }
    }
}