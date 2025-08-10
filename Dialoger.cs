using System;
using Gearedup.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace Gearedup
{
    public struct EntityDialog
    {
        public enum DialogType
        {
            None,
            Normal,
            Angry
        }

        /// <summary>
        /// The dialog type of this
        /// </summary>
        public DialogType type = DialogType.None;

        /// <summary>
        /// Entity to track
        /// </summary>
        public Entity entity = null;

        /// <summary>
        /// Sound to use, default to null for no sound
        /// </summary>
        public SoundStyle? soundStyle = null;

        /// <summary>
        /// the current text
        /// </summary>
        public string text = null;

        /// <summary>
        /// the text goal
        /// </summary>
        public string text_goal = null;

        /// <summary>
        /// determines how much frame require for each character to print out
        /// </summary>
        public byte framePerChar;

        /// <summary>
        /// how much time before this text dissapear, only updates after text is done printed
        /// </summary>
        public ushort timeLeft;

        public EntityDialog()
        {
        }

        public void SetTo(Entity entity, string text)
        {
        }

        public void Draw(SpriteBatch spriteBatch)
        {
        }

        public void Dispose()
        {
            entity = null;
            soundStyle = null;
            text = null;
            text_goal = null;
            timeLeft = 0;
            framePerChar = 0;
            type = DialogType.None;
        }

        public void OnSet()
        {

        }

        public void Update()
        {
            if (text.Length == text_goal.Length)
            {
                Update_TimeLeft();
                return;
            }

            if (Main.GameUpdateCount % framePerChar == 0)
            {
                Update_Inner();
            }
        }

        public void Update_TimeLeft()
        {
            timeLeft--;
            if (timeLeft <= 0)
            {
                Dispose();
            }
        }

        public float GetAlpha()
        {
            // Fade out effect
            float frame = Math.Min(timeLeft, 30f);
            return frame / 30f;
        }

        public void Update_Inner()
        {
            text += text_goal[text.Length];
        }
    }

    public class DialogSystem : ModSystem
    {
        /// <summary>
        /// Dialog entity, only loaded in world
        /// </summary>
        public EntityDialog[] dialog = null;

        public void Add(Entity entity, string text)
        {

        }

        public override void OnWorldLoad()
        {
            dialog = new EntityDialog[100];
        }

        public override void OnWorldUnload()
        {
            if (dialog == null) return;

            for (int i = 0; i < dialog.Length; i++)
            {
                dialog[i].Dispose();
            }
            dialog = null;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            for (int i = 0; i < dialog.Length; i++)
            {
                dialog[i].Update();
            }
        }

        public override void PostDrawTiles()
        {
            Main.spriteBatch.BeginNormal();

            for (int i = 0; i < dialog.Length; i++)
            {
                dialog[i].Draw(Main.spriteBatch);
            }

            Main.spriteBatch.End();
        }
    }
}