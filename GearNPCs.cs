using Gearedup.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Gearedup
{
    public class GearNPCs : GlobalNPC
    {
        public short dye;

        public override bool InstancePerEntity => true;

        // public override void PostAI(NPC npc)
        // {
        //     // very safe shit
        //     if (projectile.owner != 255 && projectile.TryGetOwner(out Player owner))
        //     {
        //         // if (owner.TryGetModPlayer<DyePlayer>(out DyePlayer dyePlayer)) 
        //         // {
        //         //     if (dyePlayer.dye != 0) {dye = dyePlayer.dye;}
        //         // }
        //     }

        //     if (dye > 0 && ShouldRenderTarget() && !Main.dedServ)
        //     {
        //         DyeRenderer.AddRender(projectile, dye);
        //     }

        // }

        public override void PostAI(NPC npc)
        {
            if (npc.TryGetGlobalNPC<BrainWashedNPC>(out BrainWashedNPC globalBW))
            {
                if (globalBW.ownedBy != -1 && Main.player[globalBW.ownedBy] != null && Main.player[globalBW.ownedBy].active)
                {
                    if (Main.player[globalBW.ownedBy].TryGetModPlayer<GearPlayer>(out GearPlayer gp))
                    {
                        if (gp.universalDye != 0) { dye = gp.universalDye; }
                    }
                }
            }
            if (dye > 0 && !Main.dedServ && npc.active && npc.life > 0)
            {
                DyeRenderer.AddRender(npc, dye);
            }
        }

        public static bool TryGetDye(NPC npc, out int dyeValue)
        {
            dyeValue = 0;

            if (npc.TryGetGlobalNPC<GearNPCs>(out GearNPCs dyeEntity))
            {
                dyeValue = dyeEntity.dye;
                return dyeEntity.dye > 0;
            }

            return false;
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (dye > 0 && !DyeRenderer.isRenderingNPC)
            {
                return false;
            }
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }


    }
}