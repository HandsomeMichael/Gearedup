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
using Gearedup.Utils;

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
        // Loads render targets
        public override bool IsLoadingEnabled(Mod mod)
        {
            return GearClientConfig.Get.DyeRenderTargets;
        }

        public Dictionary<short, ProjRenderer> renders;

        public void AddRender(Projectile projectile, short dye)
        {
            if (renders.ContainsKey(dye))
            {
                renders[dye].Add(projectile);
            }
            else
            {
                renders.Add(dye, new ProjRenderer(projectile));
            }

            Main.NewText("Add New Render Packets " +dye);
        }

        public override void Load()
        {
            renders = new Dictionary<short, ProjRenderer>();
            Terraria.On_Main.CheckMonoliths += DrawToTarget;
            // Terraria.On_Main.DrawDust += DrawDust;
            Terraria.On_Main.DrawProjectiles += DrawProjectilesPatch;
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

        public override void Unload()
        {
            renders = null;
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
                    spriteBatch.BeginDyeShader(item.Key, Main.LocalPlayer);
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

            foreach (var tuple in renders)
            {
                RenderTarget2D rt = tuple.Value.renderTarget;
                if (rt is null || rt.Size() != new Vector2(RTwidth, RTheight))
                {
                    var renderer = tuple.Value;
                    renderer.renderTarget = new RenderTarget2D(graphics, RTwidth, RTheight, default, default, default, default, RenderTargetUsage.PreserveContents);
                    renders[tuple.Key] = renderer;
                }

                Main.NewText("Preparing render " + tuple.Key);

                graphics.SetRenderTarget(tuple.Value.renderTarget);

                graphics.Clear(Color.Transparent);
                // Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default);
                Main.spriteBatch.BeginNormal();

                foreach (var projectiles in tuple.Value.projectiles)
                {
                    Main.NewText("-- Simulating " + projectiles.Name);
                    Main.instance.DrawProj(projectiles.whoAmI);
                }

                Main.spriteBatch.End();

                graphics.SetRenderTarget(null);
            }

        }
    }
}