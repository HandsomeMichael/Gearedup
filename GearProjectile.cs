using System.IO;
using Gearedup.Content.Catched;
using Gearedup.Content.Items;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Gearedup
{
    // public class PetGlobal : GlobalProjectile
    // {
    //     public bool resetStats;
    //     public override bool InstancePerEntity => true;

    //     public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
    //     {
    //         binaryWriter.Write(resetStats);
    //     }

    //     public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
    //     {
    //         resetStats = binaryReader.ReadBoolean();
    //     }

    //     public override void PostAI(Projectile projectile)
    //     {
    //         if (projectile.owner != 255 && projectile.TryGetOwner(out Player owner))
    //         {
    //             if (owner.TryGetModPlayer(out GearPlayer gearPlayer))
    //             {
    //                 if (gearPlayer.heartsOfHero)
    //                 {
    //                     projectile.damage = 50;
    //                     projectile.DamageType = DamageClass.Generic;
    //                     projectile.penetrate = -1;
    //                     // use local npc immune
    //                     projectile.usesLocalNPCImmunity = true;
    //                     projectile.localNPCHitCooldown = 20;
    //                     projectile.netUpdate = true;
    //                     resetStats = true;
    //                 }
    //                 else if (resetStats)
    //                 {
    //                     // reset all stats to normal
    //                     resetStats = false;
    //                     projectile.damage = 0;
    //                     projectile.DamageType = DamageClass.Default;
    //                     projectile.usesLocalNPCImmunity = false;
    //                     projectile.netUpdate = true;
    //                 }
    //             }
    //         }
    //     }

    //     public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
    //     {
    //         return Main.projPet[entity.type];
    //     }
    // }
    public class GearProjectile : GlobalProjectile
    {
        public int dye;
        //public bool useRenderTarget;

        public override bool InstancePerEntity => true;

        // public static bool ShouldRenderTarget(Projectile projectile, GearProjectile gearProjectile)
        // {
        //     return gearProjectile.useRenderTarget;
        // }

        public override void OnSpawn(Projectile projectile, IEntitySource spawnSource)
        {
            // dont do shit
            if (projectile == null || projectile.whoAmI <= -1 || !projectile.active) return;

            // Modded stuff
            if (projectile.ModProjectile != null)
            {
                // fungal heal increased life steal
                if (projectile.ModProjectile.Mod.Name == "CalamityMod" && projectile.ModProjectile.Name == "FungalHeal")
                {
                    if (projectile.TryGetOwner(out Player player) && player.TryGetModPlayer<GearPlayer>(out GearPlayer gp) && gp.heartsOfHero)
                    {
                        projectile.ai[1] += 5;
                    }
                }
            }

            // Inherit From Dyed Item
            if (spawnSource is IEntitySource_WithStatsFromItem itemSource)
            {
                if (itemSource.Item.TryGetGlobalItem(out GearItem globalItem))
                {
                    if (globalItem.dye.id is int dyeID && projectile.TryGetGlobalProjectile(out GearProjectile globalProj))
                    {
                        // Main.NewText("success applied " + dyedItem.dye);
                        // globalProj.dye = (short)ContentSamples.ItemsByType[dyeID].dye;
                        globalProj.dye = GameShaders.Armor.GetShaderIdFromItemId(dyeID);

                        if (GearClientConfig.Get.IsItemFancyGraphics(itemSource.Item))
                        {
                            RenderManager.projectileForceFancy[projectile.type] = true;
                        }
                        // Legacy Code
                        // if (GearClientConfig.Get.IsItemRT(itemSource.Item))
                        // {
                        //     globalProj.useRenderTarget = true;
                        // }
                        // else if (RenderManager.IsCustomDrawed(projectile))
                        // {
                        //     globalProj.useRenderTarget = true;
                        // }
                    }
                }
            }

            // Catched Projectile Behaviour
            if (spawnSource is EntitySource_ItemUse_WithAmmo ammoSource)
            {
                if (ammoSource.AmmoItemIdUsed == ModContent.ItemType<CatchedProjectile>())
                {
                    projectile.hostile = false;
                    projectile.friendly = true;
                }
            }

            // Inherit Parents dye
            if (spawnSource is EntitySource_Parent entitySource)
            {
                // Projectile owner
                if (entitySource.Entity is Projectile projSource)
                {
                    if (projSource.TryGetGlobalProjectile(out GearProjectile parentDyedProjectile))
                    {
                        if (parentDyedProjectile.dye > 0 && projectile.TryGetGlobalProjectile(out GearProjectile GearProjectile))
                        {
                            GearProjectile.dye = parentDyedProjectile.dye;
                            // if (DyeRenderer.IsCustomDrawed(Main.projectile[hasil]))

                            // inherit fancy graphics
                            if (RenderManager.projectileForceFancy[projSource.type])
                            {
                                RenderManager.projectileForceFancy[projectile.type] = true;
                            }
                            // if (parentDyedProjectile.ShouldRenderTarget(projSource))
                            // {
                            //     GearProjectile.useRenderTarget = true;
                            // }
                        }

                    }
                }

                // Inherit Behaviour

                else if (entitySource.Entity is NPC npcSource)
                {
                    // Inherit Dye From NPC Owner
                    if (npcSource.TryGetGlobalNPC(out GearNPCs gearNPCs))
                    {
                        if (gearNPCs.dye > 0 && projectile.TryGetGlobalProjectile(out GearProjectile globalProj))
                        {
                            globalProj.dye = gearNPCs.dye;
                            // if (RenderManager.IsCustomDrawed(projectile))
                            // {
                            //     globalProj.useRenderTarget = true;
                            // }
                        }
                    }
                }
            }

            // place our new code here
        }

        public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(dye);
            //binaryWriter.Write(useRenderTarget);
        }

        public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
        {
            dye = binaryReader.ReadInt32();
            //useRenderTarget = binaryReader.ReadBoolean();
        }

        // public bool ShouldRenderTarget(Projectile projectile)
        // {
        //     // If no other possible render target spawn then we just despawn them
        //     // if (!GearClientConfig.Get.DyeRenderTargetsModded && (GearClientConfig.Get.DyeRenderTargetItemsList == null || GearClientConfig.Get.DyeRenderTargetItemsList.Count <= 0))
        //     // {
        //     //     return false;
        //     // }
        //     return useRenderTarget;
        // }

        public override void PostAI(Projectile projectile)
        {
            // very safe shit
            if (projectile.owner != 255 && projectile.friendly && projectile.TryGetOwner(out Player owner))
            {
                if (owner.TryGetModPlayer(out GearPlayer gearPlayer))
                {
                    if (gearPlayer.universalDye != 0)
                    {
                        dye = gearPlayer.universalDye;
                    }
                }
            }

            // add render progress
            //if (dye > 0 && ShouldRenderTarget(projectile) && !Main.dedServ)
            if (dye > 0 && RenderManager.IsCustomDrawed(projectile) && !Main.dedServ)
            {
                RenderManager.Get.AddTarget_Proj(dye, projectile);
            }
        }

        public static bool TryGetDye(Projectile projectile, out int dyeValue)
        {
            dyeValue = 0;

            if (projectile.TryGetGlobalProjectile(out GearProjectile dyedProjectile))
            {
                dyeValue = dyedProjectile.dye;
                return dyedProjectile.dye > 0;
            }

            return false;
        }

        // Should be moved to optimal detour later
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            // If redrawen projectile is not capturing, we hid it
            if (!RenderManager.Get.isCapturing)
            {
                if (RenderManager.IsCustomDrawed(projectile)) { return false; }
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