using System;
using Gearedup.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup.Content.NPCs
{
    /// <summary>
    /// For debugging purpose
    /// </summary>
    public class TheCat : ModNPC
    {
        public override void SetDefaults()
        {
            NPC.width = 200;
            NPC.height = 200;
            NPC.aiStyle = -1;
            NPC.damage = 30;
            NPC.value = 1000;
            NPC.boss = true;
            NPC.lifeMax = 5500;
            NPC.friendly = true;
            NPC.dontTakeDamage = true;
            NPC.knockBackResist = 0f;
            NPC.defense = 20;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath2;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.alpha = 255;
        }
        public float timer { get => NPC.ai[0]; set => NPC.ai[0] = value; }
        public override void AI()
        {
            //startup
            NPC.TargetClosest(true);
            Player player = Main.player[NPC.target];
            Vector2 dir = NPC.DirectionTo(player.Center);
            if (player.dead)
            {
                NPC.TargetClosest(true);
                player = Main.player[NPC.target];
                if (player.dead)
                {
                    timer = -1;
                }
            }
            else
            {
                NPC.Center = Vector2.Lerp(NPC.Center, player.Center, 0.1f);
            }
            //graphics
            NPC.spriteDirection = NPC.velocity.X > 0 ? -1 : 1;
            NPC.rotation = NPC.velocity.X * 0.01f;
            if (NPC.alpha > 255) { NPC.alpha = 255; }
            if (NPC.alpha < 0) { NPC.alpha = 0; }
            if (timer == -1)
            {
                NPC.scale = MathHelper.Lerp(NPC.scale, 0.5f, 0.05f);
                // NPC.alpha += 5;
                // if (NPC.alpha == 255) {
                // 	NPC.active = false;
                // }
                Main.time += 200;
                NPC.dontTakeDamage = false; // now took damage
                return;
            }
            if (timer > -1)
            {
                timer++;
                NPC.alpha -= 5;
                if (timer == 60)
                {
                    SpawnText("Look me in the eye.");
                    SoundEngine.PlaySound(new SoundStyle("Gearedup/Sound/cat1"), NPC.Center);
                }
                if (timer == 200)
                {
                    SpawnText("Do you feel it ?");
                    SoundEngine.PlaySound(new SoundStyle("Gearedup/Sound/cat2"), NPC.Center);
                }
                if (timer == 300)
                {
                    SpawnText("The Madness consuming your mind.");
                    SoundEngine.PlaySound(new SoundStyle("Gearedup/Sound/cat3"), NPC.Center);
                }
                if (timer == 480)
                {
                    SpawnText("The utter despair.");
                    SoundEngine.PlaySound(new SoundStyle("Gearedup/Sound/cat4"), NPC.Center);
                }
                if (timer == 600)
                {
                    SpawnText("In the face of the inevitable.");
                    SoundEngine.PlaySound(new SoundStyle("Gearedup/Sound/cat5"), NPC.Center);
                }
                if (timer == 760)
                {
                    timer = -1;
                }
            }
            //sound
            //for syncing projectile if i forgor
            //NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
        }

        void SpawnText(string text)
        {
            CombatText.NewText(NPC.Hitbox, Color.White, text);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var texture = ModContent.Request<Texture2D>(Texture).Value;

            Vector2 orig = texture.Size() / 2f;
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (NPC.spriteDirection == 1) { spriteEffects = SpriteEffects.FlipHorizontally; }
            spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, null, NPC.GetAlpha(Color.Black), NPC.rotation, orig, NPC.scale, spriteEffects, 0f);
            spriteBatch.BeginGlow(true);
            spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, null, NPC.GetAlpha(Color.White), NPC.rotation, orig, NPC.scale, spriteEffects, 0f);
            spriteBatch.BeginNormal(true);
            return false;
        }
        //public override void PostDraw(SpriteBatch spriteBatch, Color lightColor) {}

    }
}