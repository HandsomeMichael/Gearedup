using System;
using Gearedup.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Gearedup.Overwrite
{
    public class OverwriteSystem : ModSystem
    {
        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            // do better binoculars
            var font = FontAssets.MouseText.Value;

            if (Main.mouseItem != null && !Main.mouseItem.IsAir)
            {
                if (Main.mouseItem.type == ItemID.Stopwatch)
                {
                    BetterStopWatch.UpdateStopWatch(Main.mouseItem);
                }
            }

            if (Main.LocalPlayer.HeldItem.type == ItemID.Stopwatch)
            {
                // Use string interpolation to format the time
                string text = Helpme.GetFormatTime((int)Main.LocalPlayer.HeldItem.velocity.X);
                string text2 = Helpme.GetFormatTime((int)Main.LocalPlayer.HeldItem.velocity.Y);

                // TextSnippet[] snippets = ChatManager.ParseMessage(text, Color.LightGreen).ToArray();
                // TextSnippet[] snippets2 = ChatManager.ParseMessage(text2, Color.White).ToArray();
                // Vector2 messageSize = ChatManager.GetStringSize(font, snippets,new Vector2(2,2));

                Vector2 pos = Main.MouseWorld - Main.screenPosition + new Vector2(0,
                    40 + (float)(Math.Sin(Main.GameUpdateCount / 45f) * 8.0f));

                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, text, pos, Color.LightGreen, 0f, Vector2.One, Vector2.One);
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, text2, pos + new Vector2(0, 30), Color.White,0f,Vector2.One,Vector2.One);

                // _ = ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, snippets, pos,
                //     0f, Vector2.One, new Vector2(1.5f, 1.5f), out _);

                // _ = ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, snippets2, pos + new Vector2(0, 30),
                //     0f, Vector2.One, new Vector2(1f, 1f), out _);
            }
        }
    }
}