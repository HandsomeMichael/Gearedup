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

    public struct RenderTManager
    {
        // TO DO : HOW THE FUCK DO I OPTIMIZE THIS B
        public RenderTManager(Projectile proj)
        {
            renderTarget = null;
            entityIndexes = new List<int>() { proj.whoAmI };
        }

        public RenderTManager(NPC npc)
        {
            renderTarget = null;
            entityIndexes = new List<int>() { npc.whoAmI };
        }

        public RenderTarget2D renderTarget;
        public List<int> entityIndexes;
        public void Add(Projectile proj)
        {
            entityIndexes.Add(proj.whoAmI);
        }

        public void Add(NPC npc)
        {
            entityIndexes.Add(npc.whoAmI);
        }

        public bool IsActive() => entityIndexes.Count > 0;
        public void Clear()
        {
            entityIndexes.Clear();
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
        public static bool isRenderingNPC;

        public static Dictionary<short, RenderTManager> renders;
        public static Dictionary<short, RenderTManager> rendersNPC;
        public static List<int> customDrawedProjectiles;

        public static void AddRender(Projectile projectile, short dye)
        {
            if (renders.ContainsKey(dye))
            {
                renders[dye].Add(projectile);
            }
            else
            {
                renders.Add(dye, new RenderTManager(projectile));
            }

            Main.NewText("Add New Render Packets " + dye);
        }

        public static void AddRender(NPC npc, short dye)
        {
            if (rendersNPC.ContainsKey(dye))
            {
                rendersNPC[dye].Add(npc);
            }
            else
            {
                rendersNPC.Add(dye, new RenderTManager(npc));
            }

            Main.NewText("Add New NPC Render Packets " + dye);
        }



        public override void Load()
        {
            renders = new Dictionary<short, RenderTManager>();
            rendersNPC = new Dictionary<short, RenderTManager>();
            Terraria.On_Main.CheckMonoliths += DrawToTarget;
            // Terraria.On_Main.DrawDust += DrawDust;
            Terraria.On_Main.DrawProjectiles += DrawProjectilesPatch;
            Terraria.On_Main.DrawNPCs += DrawNPCsPatch;
            customDrawedProjectiles = new List<int>();
        }

        private void DrawNPCsPatch(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
        {
            orig(self, behindTiles);

            GraphicsDevice gD = Main.graphics.GraphicsDevice;
            SpriteBatch spriteBatch = Main.spriteBatch;

            if (Main.dedServ || spriteBatch == null || gD == null || !HasRenderTaskNPC())
                return;

            foreach (var item in rendersNPC)
            {
                if (item.Value.IsActive())
                {
                    Main.NewText("Drawed npcs renders shader " + item.Key);
                    // DrawData data = new DrawData
                    // {
                    //     position = Vector2.Zero,
                    //     scale = Vector2.One,
                    //     sourceRect = null,
                    //     texture = item.Value.renderTarget
                    // };
                    //
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.EffectMatrix);
                    GameShaders.Armor.Apply(item.Key, null, null);
                    // spriteBatch.BeginDyeShader(item.Key, Main.LocalPlayer,false,false);
                    spriteBatch.Draw(item.Value.renderTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
                    spriteBatch.End();
                    spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
                    item.Value.Clear();
                }
            }
        }

        public override void Unload()
        {
            renders = null;
            rendersNPC = null;
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
                    Main.NewText("Drawed projectile renders shader " + item.Key);
                    // DrawData data = new DrawData
                    // {
                    //     position = Vector2.Zero,
                    //     scale = Vector2.One,
                    //     sourceRect = null,
                    //     texture = item.Value.renderTarget
                    // };
                    //

                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.EffectMatrix);
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

        private bool HasRenderTaskNPC()
        {
            foreach (var item in rendersNPC)
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
            if (Main.gameMenu) return;


            var graphics = Main.graphics.GraphicsDevice;

            int RTwidth = Main.screenWidth;
            int RTheight = Main.screenHeight;

            if (HasRenderTask())
            {
                isRendering = true;

                foreach (var tuple in renders)
                {
                    RenderTarget2D rt = tuple.Value.renderTarget;
                    if (rt is null || rt.Size() != new Vector2(RTwidth, RTheight))
                    {
                        var renderer = tuple.Value;
                        renderer.renderTarget = new RenderTarget2D(graphics, RTwidth, RTheight, default, default, default, default, RenderTargetUsage.PreserveContents);
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

                    foreach (var i in tuple.Value.entityIndexes)
                    {
                        Main.NewText("-- Simulating " + Main.projectile[i].Name);
                        Main.instance.DrawProj(i);
                    }

                    Main.spriteBatch.End();

                    graphics.SetRenderTarget(null);
                }

                isRendering = false;
            }

            if (HasRenderTaskNPC())
            {
                isRenderingNPC = true;
                foreach (var tuple in rendersNPC)
                {
                    RenderTarget2D rt = tuple.Value.renderTarget;
                    if (rt is null || rt.Size() != new Vector2(RTwidth, RTheight))
                    {
                        var renderer = tuple.Value;
                        renderer.renderTarget = new RenderTarget2D(graphics, RTwidth, RTheight, default, default, default, default, RenderTargetUsage.PreserveContents);
                        rendersNPC[tuple.Key] = renderer;
                    }

                    // skip unused renderer
                    if (!tuple.Value.IsActive()) continue;

                    Main.NewText("Preparing render npc " + tuple.Key);

                    graphics.SetRenderTarget(tuple.Value.renderTarget);
                    graphics.Clear(Color.Transparent);

                    // Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default);

                    Main.spriteBatch.BeginNormal();
                    //Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.GameViewMatrix.ZoomMatrix);

                    foreach (var i in tuple.Value.entityIndexes)
                    {
                        var npc = Main.npc[i];
                        Main.NewText("-- Simulating NPC Draws " + npc.FullName);

                        // ripped straight throught heaven
                        if (npc.ModNPC != null && npc.ModNPC is ModNPC modNPC)
                        {
                            if (modNPC.PreDraw(Main.spriteBatch, Main.screenPosition, npc.GetAlpha(Color.White)))
                                Main.instance.DrawNPC(i, false);

                            modNPC.PostDraw(Main.spriteBatch, Main.screenPosition, npc.GetAlpha(Color.White));
                        }
                        else
                        {
                            Main.instance.DrawNPC(i, false);
                        }
                    }

                    Main.spriteBatch.End();

                    graphics.SetRenderTarget(null);
                }
                isRenderingNPC = false;
            }

        }
    }
}