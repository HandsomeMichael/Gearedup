using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Gearedup
{
    public class Optimizator : ModSystem
    {
        private UnifiedRandom clientRan;
        public override void Load()
        {
            clientRan = new UnifiedRandom(69);
            // On_CombatText.NewText_Rectangle_Color_int_bool_bool += NewCombatTextPatch;
            On_Dust.NewDust += NewDustPatch;
            On_Main.DrawProj += DrawProjPatch;
        }

        private void DrawProjPatch(On_Main.orig_DrawProj orig, Main self, int i)
        {
            if (GearClientConfig.Get.Opti_ShowOwnProjectileOnly)
            {
                // dont draw some projectile we dont own
                var proj = Main.projectile[i];

                if (proj != null && proj.active && !proj.hostile && proj.friendly && proj.owner != Main.myPlayer && proj.Distance(Main.LocalPlayer.Center) > 250)
                {
                    return;
                }
            }
            orig(self, i);
        }

        public override void Unload()
        {
            clientRan = null;
            // On_CombatText.NewText_Rectangle_Color_int_bool_bool -= NewCombatTextPatch;
            On_Dust.NewDust -= NewDustPatch;
            On_Main.DrawProj -= DrawProjPatch;
        }

        private int NewDustPatch(On_Dust.orig_NewDust orig, Vector2 Position, int Width, int Height, int Type, float SpeedX, float SpeedY, int Alpha, Color newColor, float Scale)
        {
            if (GearClientConfig.Get.Opti_ReduceDust)
            {
                if (Position.DistanceSQ(Main.LocalPlayer.Center) > 1000)
                {
                    return 0;
                }
                else if (clientRan.NextBool(30))
                {
                    return 0;
                }
            }
            return orig(Position, Width, Height, Type, SpeedX, SpeedY, Alpha, newColor, Scale);
        }

        // private int NewCombatTextPatch(On_CombatText.orig_NewText_Rectangle_Color_int_bool_bool orig, Rectangle location, Color color, int amount, bool dramatic, bool dot)
        // {
        //     if (GearClientConfig.Get.Opti_ReduceCombatText)
        //     {
        //         var pos = new Vector2(location.X, location.Y);

        //         if (pos.DistanceSQ(Main.LocalPlayer.Center) > 1000)
        //         {
        //             return 0;
        //         }
                
        //         for (int i = 0; i < Main.maxCombatText; i++)
        //         {
        //             var combatText = Main.combatText[i];
        //             int parsed = int.Parse(combatText.text);
        //             if (combatText.active && parsed > 0)
        //             {
        //                 combatText.text = (amount + parsed).ToString();
        //                 return i;
        //                 // float distance = combatText.position.DistanceSQ(pos);
        //                 // if (distance < 300)
        //                 // {
        //                 //     combatText.text = (amount + parsed).ToString();
        //                 //     return i;
        //                 // }
        //             }
        //         }
        //     }
        //     return orig(location, color, amount, dramatic, dot);
        // }
    }
}