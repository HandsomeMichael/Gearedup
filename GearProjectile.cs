using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Gearedup
{
    public class GearProjectile : GlobalProjectile
    {
        public short dye;
        public bool useRenderTarget;

        // public static bool ShouldRenderTarget(Projectile projectile, GearProjectile gearProjectile)
        // {
        //     return gearProjectile.useRenderTarget;
        // }

        public bool ShouldRenderTarget()
        {
            // If no other possible render target spawn then we just despawn them
            if (!GearClientConfig.Get.DyeRenderTargetsModded &&
            (GearClientConfig.Get.DyeRenderTargetItemsList == null || GearClientConfig.Get.DyeRenderTargetItemsList.Count <= 0))
            {
                return false;
            }
            return useRenderTarget;
        }
        
        public override bool InstancePerEntity => true;

        public override void PostAI(Projectile projectile)
        {
            // very safe shit
            if (projectile.owner != 255 && projectile.TryGetOwner(out Player owner))
            {
                // if (owner.TryGetModPlayer<DyePlayer>(out DyePlayer dyePlayer)) 
                // {
                //     if (dyePlayer.dye != 0) {dye = dyePlayer.dye;}
                // }
            }

            if (dye > 0 && ShouldRenderTarget() && !Main.dedServ)
            {
                DyeRenderer.AddRender(projectile, dye);
            }

        }

        public static bool TryGetDye(Projectile projectile, out int dyeValue)
        {
            dyeValue = 0;

            if (projectile.TryGetGlobalProjectile<GearProjectile>(out GearProjectile dyedProjectile))
            {
                dyeValue = dyedProjectile.dye;
                return dyedProjectile.dye > 0;
            }

            return false;
        }

        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            if (!DyeRenderer.isRendering)
            {
                if (ShouldRenderTarget())
                {
                    return false;
                }
            }
            return base.PreDraw(projectile, ref lightColor);
        }

        // public override bool PreDraw(Projectile projectile, ref Color lightColor)
        // {
        // 	if (dye > 0) {Main.spriteBatch.BeginDyeShader(dye,projectile,true);}
        //     return base.PreDraw(projectile, ref lightColor);
        // }

        // public override void PostDraw(Projectile projectile, Color lightColor)
        // {
        //     // if (dye > 0) {Main.spriteBatch.BeginNormal(true);}
        //     ChatManager.DrawColorCodedString(Main.spriteBatch,FontAssets.DeathText.Value,"dye : "+dye,projectile.Center - Main.screenPosition,Color.White,0f,Vector2.One,Vector2.One);

        // }
    }
}