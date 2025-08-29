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
    public class GearProjectile : GlobalProjectile
    {
        public int dye;
        public bool useRenderTarget;

        public override bool InstancePerEntity => true;

        // public static bool ShouldRenderTarget(Projectile projectile, GearProjectile gearProjectile)
        // {
        //     return gearProjectile.useRenderTarget;
        // }

        public override void OnSpawn(Projectile projectile, IEntitySource spawnSource)
        {
            // dont do shit
            if (projectile == null || projectile.whoAmI <= -1 || !projectile.active) return;

			// Inherit From Dyed Item
			if (spawnSource is IEntitySource_WithStatsFromItem itemSource)
			{
				if (itemSource.Item.TryGetGlobalItem<GearItem>(out GearItem globalItem))
				{
					if (globalItem.dye.id is int dyeID && projectile.TryGetGlobalProjectile<GearProjectile>(out GearProjectile globalProj))
					{
						// Main.NewText("success applied " + dyedItem.dye);
						// globalProj.dye = (short)ContentSamples.ItemsByType[dyeID].dye;
						globalProj.dye = GameShaders.Armor.GetShaderIdFromItemId(dyeID);

						if (GearClientConfig.Get.IsItemRT(itemSource.Item))
						{
							globalProj.useRenderTarget = true;
						}
						else if (RenderManager.IsCustomDrawed(projectile))
						{
							globalProj.useRenderTarget = true;
						}
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
					if (projSource.TryGetGlobalProjectile<GearProjectile>(out GearProjectile parentDyedProjectile))
					{
						if (parentDyedProjectile.dye > 0 && projectile.TryGetGlobalProjectile<GearProjectile>(out GearProjectile GearProjectile))
						{
							GearProjectile.dye = parentDyedProjectile.dye;
							// if (DyeRenderer.IsCustomDrawed(Main.projectile[hasil]))
							if (parentDyedProjectile.ShouldRenderTarget(projSource))
							{
								GearProjectile.useRenderTarget = true;
							}
						}

					}
				}

                // Inherit Behaviour
                
                else if (entitySource.Entity is NPC npcSource)
                {
                    // Inherit Brainwash from NPC Owner
                    if (npcSource.TryGetGlobalNPC<BrainWashedNPC>(out BrainWashedNPC bw))
                    {
                        if (bw.ownedBy != -1 && projectile.TryGetGlobalProjectile<BrainWashedProj>(out BrainWashedProj bwr))
                        {
                            bwr.ownedBy = bw.ownedBy;
                            projectile.hostile = true;
                            projectile.friendly = true;
                        }
                    }

                    // Inherit Dye From NPC Owner
                    if (npcSource.TryGetGlobalNPC<GearNPCs>(out GearNPCs gearNPCs))
                    {
                        if (gearNPCs.dye > 0 && projectile.TryGetGlobalProjectile<GearProjectile>(out GearProjectile globalProj))
                        {
                            globalProj.dye = gearNPCs.dye;
                            if (RenderManager.IsCustomDrawed(projectile))
                            {
                                globalProj.useRenderTarget = true;
                            }
                        }
                    }
                }
			}

            // place our new code here
        }

        public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(dye);
            binaryWriter.Write(useRenderTarget);

            // bitWriter.WriteBit(useRenderTarget);
        }

        public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
        {
            dye = binaryReader.ReadInt32();
            useRenderTarget = binaryReader.ReadBoolean();
        }

        public bool ShouldRenderTarget(Projectile projectile)
        {
            // If no other possible render target spawn then we just despawn them
            // if (!GearClientConfig.Get.DyeRenderTargetsModded && (GearClientConfig.Get.DyeRenderTargetItemsList == null || GearClientConfig.Get.DyeRenderTargetItemsList.Count <= 0))
            // {
            //     return false;
            // }
            return useRenderTarget;
        }

        public override void PostAI(Projectile projectile)
        {
            // very safe shit
            if (projectile.owner != 255 && projectile.friendly && projectile.TryGetOwner(out Player owner))
            {
                if (owner.TryGetModPlayer<GearPlayer>(out GearPlayer gearPlayer))
                {
                    if (gearPlayer.universalDye != 0)
                    {
                        dye = gearPlayer.universalDye;
                    }
                }
            }

            // add render progress
            if (dye > 0 && ShouldRenderTarget(projectile) && !Main.dedServ)
            {
                RenderManager.Get.AddTarget_Proj(dye, projectile);
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
            // If redrawen projectile is not capturing, we hid it
            if (!RenderManager.Get.isCapturing)
            {
                if (ShouldRenderTarget(projectile)) { return false; }
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