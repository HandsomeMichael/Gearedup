using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Gearedup.Helper;
using Terraria.ModLoader.IO;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Gearedup.Overwrite
{
    public class BetterStopWatch : GlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ItemID.Stopwatch;
        }

        public static void UpdateStopWatch(Item item)
        {
            if (item.beingGrabbed)
            {
                item.velocity.X += 1;
                if (item.velocity.X % 60 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Tink);
                }
            }
        }

        public override void UpdateInventory(Item item, Player player)
        {
            UpdateStopWatch(item);
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "StopwatchDesc", "Use to start and stop timer"));
            tooltips.Add(new TooltipLine(Mod, "Stopwatch1", "current  : " + Helpme.GetFormatTime((int)item.velocity.X)));
            tooltips.Add(new TooltipLine(Mod, "Stopwatch2", "previous : " + Helpme.GetFormatTime((int)item.velocity.Y)));
        }

        public override void SetDefaults(Item entity)
        {
            entity.useTime = 10;
            entity.useAnimation = 10;
            entity.useStyle = ItemUseStyleID.HoldUp;
        }
        public override bool CanUseItem(Item item, Player player) => true;

        public override bool? UseItem(Item item, Player player)
        {
            SoundEngine.PlaySound(SoundID.Camera);
            item.beingGrabbed = !item.beingGrabbed;
            if (item.beingGrabbed)
            {
                item.velocity.Y = item.velocity.X;
                item.velocity.X = 0;
            }
            return true;
        }

        public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (item.beingGrabbed)
            {
                Texture2D extraTexture = ModContent.Request<Texture2D>("StopwatchAnimation").Value;

                const int maxFrame = 8;
                int frameCount = Helpme.MagicallyGetFrame(maxFrame, 10);
                spriteBatch.Draw(extraTexture, position, extraTexture.GetVerticalFrame(frameCount, maxFrame), drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
                return false;
            }
            return true;
        }
    }
}