using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup.Content.Items.Weapons
{
    public class NightThing : ModItem
    {
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Shuriken);
            Item.damage = 30;
            Item.DamageType = DamageClass.Melee;
            Item.shoot = ModContent.GetInstance<NightThingProj>().Type;
            Item.rare = ItemRarityID.LightPurple;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = -1; i < 2; i++)
            {
                Projectile.NewProjectile(source, position, velocity.RotatedBy(MathHelper.ToRadians(i*10)), type, damage, knockback);
            }

            return false;
        }
    }

    public class NightThingProj : ModProjectile
    {
        public override string Texture => "Gearedup/Content/Items/Weapons/NightThing";
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Shuriken);
            Projectile.penetrate = -1;
            Projectile.aiStyle = 0;
        }

        public override void AI()
        {
            if (Projectile.tileCollide)
            {
                Projectile.rotation += (float)Projectile.direction * 0.5f;
                Projectile.velocity.Y += 0.3f;
            }
            else
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.velocity.Y += 0.4f;
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // penetrate less
            modifiers.DefenseEffectiveness *= 2;

            modifiers.CritDamage += 2f; // Crit deals 200% more

            if (!Main.dayTime)
            {
                target.AddBuff(BuffID.Poisoned, 30);
                target.AddBuff(BuffID.OnFire, 30);
                target.AddBuff(BuffID.Bleeding, 30);
                target.AddBuff(BuffID.Slow, 30);

                // crit deals 50% less
                modifiers.CritDamage -= 2.5f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hit.Crit && target.life > 10 && target.lifeMax > 10 && target.CanBeChasedBy(Projectile))
            {
                target.lifeMax -= 1;
                CombatText.NewText(target.Hitbox, Color.LightPink, "1");
            }

            int findIndex = -1;
            float distance = 0f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var newDist = Main.npc[i].DistanceSQ(Projectile.Center);
                if (i != target.whoAmI && Main.npc[i].active && Main.npc[i].CanBeChasedBy(Projectile) && (newDist < distance || findIndex == -1))
                {
                    findIndex = i;
                    distance = newDist;
                }
            }

            if (findIndex != -1)
            {
                Projectile.velocity = Projectile.Center.DirectionTo(Main.npc[findIndex].Center + Main.npc[findIndex].velocity) * 20f;
                Projectile.damage -= 1;
                Projectile.scale -= 0.01f;

                Projectile.tileCollide = false;
                Projectile.timeLeft = 60*2; // 2 seconds before death
            }
        }
    }
}