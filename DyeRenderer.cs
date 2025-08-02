using System.Reflection;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria;
using Terraria.ModLoader.Core;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Graphics;
using static Terraria.ModLoader.Core.TmodFile;
using Gearedup.Helper;
using Terraria.ID;
using Terraria.DataStructures;

namespace Gearedup
{

    public struct ProjRenderer
    {
        public ProjRenderer(Projectile proj)
        {
            renderTarget = null;
            projectiles = new List<Projectile>(){proj};
        }

        public RenderTarget2D renderTarget;
        public List<Projectile> projectiles;

        public void Add(Projectile proj)
        {
            projectiles.Add(proj);
        }

        public bool IsActive() => projectiles.Count > 0;
        public void Clear()
        {
            projectiles.Clear();
        }

    }
    public class DyeRenderer : ModSystem
    {
        // Render Target System will always be loaded now

        // public override bool IsLoadingEnabled(Mod mod)
        // {
        //     return GearClientConfig.Get.DyeRenderTargets;
        // }

        public static bool IsCustomDrawed(Projectile projectile)
        {
            if (GearClientConfig.Get.DyeRenderTargetsAll) return true;
            if (!GearClientConfig.Get.DyeRenderTargetsModded) return false;
            return customDrawedProjectiles.Contains(projectile.type);
        }

        public static bool isRendering;

        public static Dictionary<short, ProjRenderer> renders;
        public static List<int> customDrawedProjectiles;

        public static void AddRender(Projectile projectile, short dye)
        {
            if (renders.ContainsKey(dye))
            {
                renders[dye].Add(projectile);
            }
            else
            {
                renders.Add(dye, new ProjRenderer(projectile));
            }

            Main.NewText("Add New Render Packets " + dye);
        }



        public override void Load()
        {
            renders = new Dictionary<short, ProjRenderer>();
            Terraria.On_Main.CheckMonoliths += DrawToTarget;
            // Terraria.On_Main.DrawDust += DrawDust;
            Terraria.On_Main.DrawProjectiles += DrawProjectilesPatch;
            customDrawedProjectiles = new List<int>();
        }
        
        public override void Unload()
        {
            renders = null;
            customDrawedProjectiles = null;
        }

        public override void PostSetupContent()
        {
            // automatically add potential rendertarget2D required projectiles.
            foreach (var item in ContentSamples.ProjectilesByType)
            {
                if (item.Value.ModProjectile == null) continue;

                Type type = item.Value.ModProjectile.GetType();
                MethodInfo method = type.GetMethod("PreDraw", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (method?.IsVirtual == true && !method.IsFinal && method.GetBaseDefinition().DeclaringType != type)
                {
                    customDrawedProjectiles.Add(item.Key);
                    Mod.Logger.Info($"Projectile of [{item.Value.Name}/{item.Value.type}/{type.FullName}] added to custom draw");
                }
            }
            // for (int i = ProjectileID.Count; i < ContentSamples.ProjectilesByType.Count; i++)
            // {

            // }
        }

        // WHERE DOES TERRARIA EVEN DRAW PROJECTILES IF THIS DOESNT WORK
        private void DrawProjDirect(On_Main.orig_DrawProjDirect orig, Main self)
        {
            return;
            // if (!isRendering)
            // {
            //     if (Main.projectile[i].TryGetGlobalProjectile<GearProjectile>(out GearProjectile gp))
            //     {
            //         if (gp.dye > 0)
            //         {
            //             Main.NewText("Not drawing this "+Main.projectile[i].Name);
            //             return;
            //         }
            //     }
            // }
            // orig(self, i);
        }

        private void DrawProjectilesPatch(On_Main.orig_DrawProjectiles orig, Main self)
        {
            orig(self);
            DrawRenderTargets();
        }

        private void DrawDust(On_Main.orig_DrawDust orig, Main self)
        {
            DrawRenderTargets();
            orig(self);
        }

        public void DrawRenderTargets()
        {
            GraphicsDevice gD = Main.graphics.GraphicsDevice;
            SpriteBatch spriteBatch = Main.spriteBatch;

            if (Main.dedServ || spriteBatch == null || gD == null || !HasRenderTask())
                return;

            foreach (var item in renders)
            {
                if (item.Value.IsActive())
                {
                    Main.NewText("Drawed projectile renders shader "+item.Key);
                    // DrawData data = new DrawData
                    // {
                    //     position = Vector2.Zero,
                    //     scale = Vector2.One,
                    //     sourceRect = null,
                    //     texture = item.Value.renderTarget
                    // };
                    //

                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, null,Main.GameViewMatrix.EffectMatrix);
                    GameShaders.Armor.Apply(item.Key, null, null);
                    // spriteBatch.BeginDyeShader(item.Key, Main.LocalPlayer,false,false);
                    spriteBatch.Draw(item.Value.renderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
                    spriteBatch.End();
                    item.Value.Clear();
                }
            }
        }

        private bool HasRenderTask()
        {
            foreach (var item in renders)
            {
                if (item.Value.IsActive())
                {
                    return true;
                }
            }
            return false;
        }

        private void DrawToTarget(Terraria.On_Main.orig_CheckMonoliths orig)
        {
            orig();

            if (Main.gameMenu || !HasRenderTask()) return;

            var graphics = Main.graphics.GraphicsDevice;

            int RTwidth = Main.screenWidth;
            int RTheight = Main.screenHeight;

            isRendering = true;

            foreach (var tuple in renders)
            {
                RenderTarget2D rt = tuple.Value.renderTarget;
                // Vector2 screenRes = new Vector2(RTwidth,RTheight) * Main.GameViewMatrix.Zoom;
                if (rt is null || rt.Size() != new Vector2(RTwidth, RTheight))
                // if (rt is null || rt.Size() != screenRes)
                {
                    var renderer = tuple.Value;
                    renderer.renderTarget = new RenderTarget2D(graphics, RTwidth, RTheight, default, default, default, default, RenderTargetUsage.PreserveContents);
                    // renderer.renderTarget = new RenderTarget2D(graphics, (int)screenRes.X, (int)screenRes.Y, default, default, default, default, RenderTargetUsage.PreserveContents);
                    renders[tuple.Key] = renderer;
                }

                // skip unused renderer
                if (!tuple.Value.IsActive()) continue;

                Main.NewText("Preparing render " + tuple.Key);

                graphics.SetRenderTarget(tuple.Value.renderTarget);
                graphics.Clear(Color.Transparent);

                // Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default);

                Main.spriteBatch.BeginNormal();
                //Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                foreach (var projectiles in tuple.Value.projectiles)
                {
                    Main.NewText("-- Simulating " + projectiles.Name);
                    Main.instance.DrawProj(projectiles.whoAmI);
                }

                Main.spriteBatch.End();

                graphics.SetRenderTarget(null);
            }

            isRendering = false;

        }
    }
}