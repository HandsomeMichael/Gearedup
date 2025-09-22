using System;
using System.Collections.Generic;
using System.ComponentModel;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;
using Terraria.Utilities;

namespace Gearedup.Optimize
{
    public class OptimizeConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;
        // public static OptimizeConfig Get => ModContent.GetInstance<OptimizeConfig>();

        [Header("BasicOptimization")]

        [DefaultValue(false)]
        public bool Opti_ShowOwnProjectileOnly;
        [DefaultValue(false)]
        public bool Opti_ReduceCombatText;
        [DefaultValue(false)]
        public bool Opti_ImproveCombatText;
        [DefaultValue(false)]
        public bool Opti_ReduceDust;

        [Header("Advanced")]

        [DefaultValue(false)]
        public bool DyeStress_Proj;
        [DefaultValue(false)]
        public bool DyeStress_NPC;
        [DefaultValue(false)]
        public bool CombatTextStress;
        [DefaultValue(false)]
        public bool DustStress;
    }

    public class Optimizator : ModSystem
    {
        // load config ref to reduce getinstance hassle
        OptimizeConfig optiConfig;

        // dont load in servers
        public override bool IsLoadingEnabled(Mod mod)
        {
            return !Main.dedServ;
        }
        

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            layers.Add(new LegacyGameInterfaceLayer("Gearedup Stress : UI", DrawStress, InterfaceScaleType.UI));
        }

        public bool DrawStress()
        {
            return true;
        }

        public override void PostSetupContent()
        {
            optiConfig = ModContent.GetInstance<OptimizeConfig>();
        }

        // public override void PostDrawInterface(SpriteBatch spriteBatch)
        // {
        //     Utils.DrawBorderString(Main.spriteBatch,"")
        // }

        public static int hiddenProj;
        public override void PostUpdateEverything()
        {
            hiddenProj = 0;
        }

        private UnifiedRandom clientRan;
        public override void Load()
        {
            clientRan = new UnifiedRandom(69);
            optiConfig = ModContent.GetInstance<OptimizeConfig>();
            On_CombatText.NewText_Rectangle_Color_int_bool_bool += CombatTextPatch;
            On_Dust.NewDust += NewDustPatch;
            On_Main.DrawProj += DrawProjPatch;
        }

        public override void Unload()
        {
            clientRan = null;
            optiConfig = null;
            // On_CombatText.NewText_Rectangle_Color_int_bool_bool -= NewCombatTextPatch;
            On_Dust.NewDust -= NewDustPatch;
            On_Main.DrawProj -= DrawProjPatch;
        }

        private int CombatTextPatch(On_CombatText.orig_NewText_Rectangle_Color_int_bool_bool orig, Rectangle location, Color color, int dmg, bool dramatic, bool dot)
        {
            // Improved combat text handling
            if (optiConfig.Opti_ReduceCombatText)
            {
                if (Main.netMode == NetmodeID.Server) { return 100; }

                var pos = location.Center();
                if (pos.DistanceSQ(Main.screenPosition) > 1000)
                {
                    Main.NewText("blocked combat text");
                    return 100;
                }

                // why tf does this gave a warning, what the fuck
                for (int i = 0; i < Main.maxCombatText; i++)
                {
                    var combatText = Main.combatText[i];
                    string text = dmg.ToString();

                    //reduce combat text clutter, instead will be merged into 1 thing
                    if (combatText.active && combatText.position.DistanceSQ(pos) < 40 && int.TryParse(combatText.text, out int origNum) && origNum > 0 && combatText.lifeTime > 30 && combatText.color == color)
                    {
                        combatText.text = (origNum + dmg).ToString();
                        Main.NewText("reduced combat text");
                        return i;
                    }

                    int fontIndex = dramatic ? 1 : 0;
                    Vector2 textSize = FontAssets.CombatText[fontIndex].Value.MeasureString(text);

                    combatText.alpha = 1f;
                    combatText.alphaDir = -1;
                    combatText.active = true;
                    combatText.scale = 0f;
                    combatText.rotation = 0f;
                    combatText.position = new Vector2(
                        location.X + location.Width * 0.5f - textSize.X * 0.5f + Main.rand.Next(-(int)(location.Width * 0.5), (int)(location.Width * 0.5) + 1),
                        location.Y + location.Height * 0.25f - textSize.Y * 0.5f + Main.rand.Next(-(int)(location.Height * 0.5), (int)(location.Height * 0.5) + 1)
                    );
                    combatText.color = color;
                    combatText.text = text;
                    combatText.velocity = new Vector2(0f, -7f);

                    if (Main.player[Main.myPlayer].gravDir == -1f)
                    {
                        combatText.velocity.Y *= -1f;
                        combatText.position.Y = location.Y + location.Height * 0.75f + textSize.Y * 0.5f;
                    }

                    combatText.lifeTime = 60;
                    combatText.crit = dramatic;
                    combatText.dot = dot;

                    if (dramatic)
                    {
                        combatText.lifeTime *= 2;
                        combatText.velocity.Y *= 2f;
                        combatText.velocity.X = Main.rand.Next(-25, 26) * 0.05f;
                        combatText.rotation = (combatText.lifeTime / 2f) * 0.002f;
                        if (combatText.velocity.X < 0f)
                            combatText.rotation *= -1f;
                    }

                    if (dot)
                    {
                        combatText.velocity.Y = -4f;
                        combatText.lifeTime = 40;
                    }

                    return i;
                }
                return 100;
            }
            return orig(location, color, dmg, dramatic, dot);

            //return 100;
        }

        private void DrawProjPatch(On_Main.orig_DrawProj orig, Main self, int i)
        {
            if (optiConfig.Opti_ShowOwnProjectileOnly)
            {
                // dont draw some projectile we dont own
                var proj = Main.projectile[i];

                if (proj != null && proj.active && !proj.hostile && proj.friendly && proj.owner != Main.myPlayer && proj.Distance(Main.LocalPlayer.Center) > 250)
                {
                    hiddenProj++;
                    return;
                }
            }
            orig(self, i);
        }

        private int NewDustPatch(On_Dust.orig_NewDust orig, Vector2 Position, int Width, int Height, int Type, float SpeedX, float SpeedY, int Alpha, Color newColor, float Scale)
        {
            if (optiConfig.Opti_ReduceDust)
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
        //     if (optiConfig.Opti_ReduceCombatText)
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