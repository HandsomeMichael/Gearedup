using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.UI;

namespace Gearedup.Content.Overwrite
{
    public class FishingRodOverwrite : GlobalProjectile
    {
        // applies to bobber
        public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.bobber;

        public override void PostDraw(Projectile projectile, Color lightColor)
        {
            if (projectile.TryGetOwner(out Player player))
            {
                var playerFishingConditions = player.GetFishingConditions();
                if (playerFishingConditions.BaitItemType <= 0) return;

                var bait = ContentSamples.ItemsByType[playerFishingConditions.BaitItemType];
                ItemSlot.DrawItemIcon(bait, 31, Main.spriteBatch, projectile.Center - Main.screenPosition + new Vector2(0,projectile.height), 1f, 32f, Color.White);
            }
        }
        
    }
}