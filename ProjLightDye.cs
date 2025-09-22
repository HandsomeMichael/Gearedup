using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup
{

    public class GlobalLightColor : GlobalProjectile
    {
        public override bool IsLoadingEnabled(Mod mod)
        {
            return !Main.dedServ && GearClientConfig.Get.DyeSupport_Light;
        }
        public Color? color;
        public override bool InstancePerEntity => true;
        public override void OnKill(Projectile projectile, int timeLeft)
        {
            color = null;
        }
    }
    /// <summary>
    /// referenced from
    /// https://github.com/alfuwu/DyeableLightPets/blob/master/DyeableLightPets.cs
    /// </summary>
    public class ProjectileLightDye : ModSystem
    {
        public static ProjectileLightDye Get => GearClientConfig.Get.DyeSupport_Light ? ModContent.GetInstance<ProjectileLightDye>() : null;
        public override bool IsLoadingEnabled(Mod mod) => GearClientConfig.Get.DyeSupport_Light;
        public RenderTarget2D target;
        public List<int> uColoredDye;
        public Dictionary<int, int> dyeToItemID;
        public byte updateTime;

        // update each 10 tick
        public byte GetUpdateRate() => 3;
        public float GetLerpRate() => 0.05f;

        public bool WhiteRender => true;

        public override void Load()
        {
            if (Main.dedServ) return;

            dyeToItemID = new Dictionary<int, int>();
            uColoredDye = new List<int>();
            On_Lighting.AddLight_Vector2_float_float_float += OnAddLight;
        }

        public override void Unload()
        {
            if (Main.dedServ) return;

            dyeToItemID = null;
            uColoredDye = null;
            target.Dispose();
            target = null;
        }

        public void AddUColoredDye(Item item)
        {
            if (item.dye <= 0) return;

            if (item.type >= ItemID.RedDye && item.type <= ItemID.PinkandBlackDye || item.type >= ItemID.BrightRedDye && item.type <= ItemID.BlackDye)
            {
                uColoredDye.Add(item.dye);
            }

            // dyeToItemID[GameShaders.Armor.GetShaderIdFromItemId(item.type)] = item.type;
            dyeToItemID[item.dye] = item.type;
        }

        public override void PreUpdateProjectiles()
        {
            if (Main.dedServ) return;
            updateTime++;
            if (updateTime > GetUpdateRate())
            {
                // only updates when it reached 0
                updateTime = 0;
            }
        }

        private void OnAddLight(On_Lighting.orig_AddLight_Vector2_float_float_float orig, Vector2 position, float r, float g, float b)
        {
            if (!Main.dedServ)
            {
                if (Gearedup.runned.projAI != -1 && Gearedup.runned.projAI < Main.maxProjectiles)
                {
                    // if its too far away, dont bother updating
                    if (position.Distance(Main.screenPosition) > 1000f)
                    {
                        orig(position, r, g, b);
                        return;
                    }

                    Projectile proj = Main.projectile[Gearedup.runned.projAI];
                    if (proj.active)
                    {
                        int shader = Main.GetProjectileDesiredShader(proj);
                        if (shader > 0)
                        {
                            if (updateTime == 0)
                            {
                                orig(position, r, g, b);
                                return;
                            }
                            else
                            {
                                if (proj.TryGetGlobalProjectile<GlobalLightColor>(out GlobalLightColor gb))
                                {
                                    if (gb.color is Color color)
                                    {
                                        orig(position, color.R, color.G, color.B);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }


            orig(position, r, g, b);
        }

        // S : aint no way this is the optimal way to do this
        // idk
        public Color GetDyeColor(Projectile proj, int dyeID, float r, float g, float b)
        {
            // linear interpolation for smoother colors
            Color outputColor;

            Color? prevColor = null;
            if (proj.TryGetGlobalProjectile<GlobalLightColor>(out GlobalLightColor gb))
            {
                prevColor = gb.color;
            }

            // we do a lil hardcoding
            if (uColoredDye.Contains(dyeID))
            {
                outputColor = new(GameShaders.Armor.GetShaderFromItemId(dyeToItemID[dyeID]).Shader.Parameters["uColor"].GetValueVector3());
            }
            else
            {
                target ??= new(Main.graphics.GraphicsDevice, 1, 1); // instantiate render target if it's null
                Main.graphics.GraphicsDevice.SetRenderTarget(target); // set render target
                Main.graphics.GraphicsDevice.Clear(Color.Transparent); // clear image (can i just clear with Color.white and skip the whole drawing with magic pixel? maybe idk)

                // begin spritebatch
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, null, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                GameShaders.Armor.Apply(dyeID, proj); // apply armor shader

                Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, Vector2.Zero, WhiteRender ? Color.White : new Color(r, g, b)); // draw white to the spritebatch

                Main.spriteBatch.End(); // end spritebatch
                Main.graphics.GraphicsDevice.SetRenderTarget(null); // clear render target

                Color[] pixelData = new Color[1]; // output array
                target.GetData(pixelData); // populate output array
                outputColor = pixelData[0]; // get color of the pixel we applied the armor shader to
            }

            if (gb != null)
            {
                if (gb.color is Color color)
                {
                    if (color.A > 0) outputColor = Color.Lerp(color, outputColor, GetLerpRate());
                }
                gb.color = outputColor;
            }

            return outputColor;
        }
    }
}