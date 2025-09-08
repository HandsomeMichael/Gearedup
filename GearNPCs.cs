using System;
using System.IO;
using Gearedup.Content.Catched;
using Gearedup.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Gearedup
{
    public class GearNPCs : GlobalNPC
    {
        public int dye;
        
        // public int stackDamage;
        public bool doubleLoot;
        public static int playerSpawning = -1;

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(dye);
            binaryWriter.Write(doubleLoot);
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            dye = binaryReader.ReadInt32();
            doubleLoot = binaryReader.ReadBoolean();
        }

        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            playerSpawning = player.whoAmI;
        }

        public override void Load()
        {
            playerSpawning = -1;
            On_NPC.SpawnNPC += FinishSpawnNPC;
        }

        private void FinishSpawnNPC(On_NPC.orig_SpawnNPC orig)
        {
            orig();
            playerSpawning = -1;
        }

        public override void Unload()
        {
            On_NPC.SpawnNPC -= FinishSpawnNPC;
        }

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            if (Main.CurrentFrameFlags.AnyActiveBossNPC) return;
            if (npc.townNPC || npc.friendly || npc.damage <= 0 || npc.life <= 5 || npc.immortal || npc.realLife >= 0) return;

            if (playerSpawning <= -1 || playerSpawning >= 255) return;

            var player = Main.player[playerSpawning];
            if (!player.active || player.dead) return;

            if (player.TryGetModPlayer(out GearPlayer gp))
            {
                // 1 in 500 chance to summon double loot guy
                if (gp.getBossBag && Main.rand.NextBool(500))
                {
                    npc.lifeMax = (int)((float)npc.lifeMax * 1.5);
                    npc.life = npc.lifeMax;

                    npc.damage = (int)((float)npc.damage * 1.5);
                    npc.defense = (int)((float)npc.defense * 1.2);

                    npc.scale += 0.3f;
                    npc.value *= 10f; // increase by 10x

                    doubleLoot = true;
                    npc.netUpdate = true;
                }
            }
        }

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

        // public override void ModifyGlobalLoot(GlobalLoot globalLoot)
        // {
            // globalLoot.Add(ItemDropRule.ByCondition(new Conditions.IsBloodMoonAndNotFromStatue(),1,1,1,1));
        // }

        public override void PostAI(NPC npc)
        {
            if (npc.TryGetGlobalNPC(out BrainWashedNPC globalBW))
            {
                if (globalBW.ownedBy != -1 && Main.player[globalBW.ownedBy] != null && Main.player[globalBW.ownedBy].active)
                {
                    if (Main.player[globalBW.ownedBy].TryGetModPlayer(out GearPlayer gp))
                    {
                        if (gp.universalDye != 0) { dye = gp.universalDye; }
                    }
                }
            }
            if (dye > 0 && !Main.dedServ && npc.active && npc.life > 0)
            {
                RenderManager.Get.AddTarget_NPC(dye, npc);
                // DyeRenderer.AddRender(npc, dye);
            }
        }

        public static bool TryGetDye(NPC npc, out int dyeValue)
        {
            dyeValue = 0;

            if (npc.TryGetGlobalNPC(out GearNPCs dyeEntity))
            {
                dyeValue = dyeEntity.dye;
                return dyeEntity.dye > 0;
            }


            return false;
        }
        // public override Color? GetAlpha(NPC npc, Color drawColor)
        // {
        //     if (dye > 0 && !DyeRenderer.isRenderingNPC)
        //     {
        //         return Color.White;
        //     }
        //     return null;
        // }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (dye > 0 && !RenderManager.Get.isCapturing)
            {
                return false;
            }
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }

        internal void ChangeDye(NPC npc, int newDye)
        {
            // if (dye == 0 && npc.townNPC && Main.rand.NextBool(10))
            // {
                
            //     CombatText.NewText(npc.Hitbox, Color.White, Language);
            // }

            dye = newDye;
        }
    }
}