using Gearedup.Helper;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup.Content.Crossmod
{
    public class JumbleBullet : ModItem
    {

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.CursedBullet);
            Item.rare = ItemRarityID.Yellow;
            Item.damage = 10;
            Item.shoot = ModContent.ProjectileType<JumbleBulletProjectile>();
        }

        public override void PickAmmo(Item weapon, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback)
        {
            // double damage if player life below 15%
            if (player.statLife <= player.statLifeMax2 * 0.15f)
            {
                damage *= 2;
            }
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(150);
            recipe.AddIngredient(ItemID.CursedBullet, 50);
            recipe.AddIngredient(ItemID.IchorBullet, 50);
            recipe.AddIngredient(ItemID.VenomBullet, 50);
            recipe.AddIngredient(ItemID.BeetleHusk);
            if (Gearedup.Get.calamityMod != null)
            {
                recipe.AddModIngredient(Gearedup.Get.calamityMod, "LifeAlloy");
            }
            else
            {
                recipe.AddIngredient(ItemID.FragmentVortex);
            }
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }

    public class JumbleBulletProjectile : ModProjectile
    {
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
        }

        public override void AI()
        {
            // calamity moment
            if (Projectile.ai[0] != 1)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position, Projectile.velocity.RotatedBy(MathHelper.ToRadians(2f)), ProjectileID.IchorBullet, Projectile.damage / 4, Projectile.knockBack / 4, Projectile.owner);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position, Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(1f)), ProjectileID.VenomBullet, Projectile.damage / 4, Projectile.knockBack / 4, Projectile.owner);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position, Projectile.velocity.RotatedBy(MathHelper.ToRadians(-2f)), ProjectileID.CursedBullet, Projectile.damage / 4, Projectile.knockBack / 4, Projectile.owner);
                Projectile.ai[0] = 1;
            }
            else
            {
                Projectile.Kill();
            }
        }

        // public override void SetDefaults()
        // {
        //     Projectile.width = 4;
        //     Projectile.height = 4;
        //     Projectile.aiStyle = 1; // Bullet AI
        //     Projectile.friendly = true;
        //     Projectile.hostile = false;
        //     Projectile.DamageType = DamageClass.Ranged;
        //     Projectile.penetrate = 1;
        //     Projectile.timeLeft = 600;
        //     Projectile.ignoreWater = true;
        //     Projectile.extraUpdates = 1;
        //     AIType = ProjectileID.Bullet;
        // }
        // public override void PostAI()
        // {
        //     // if (Projectile.position.Y > Main.space)
        //     // {

        //     // }
        // }
        // public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        // {
        //     target.AddBuff(BuffID.Ichor, damageDone * 2);
        //     target.AddBuff(BuffID.CursedInferno, damageDone * 2);
        //     target.AddBuff(BuffID.Venom, damageDone * 2);
        //     target.AddBuff(BuffID.Confused, damageDone * 2);
        // }
    }
    
}