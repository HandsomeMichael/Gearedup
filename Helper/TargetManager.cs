using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace Gearedup.Helper.WIP
{
    /// <summary>
    /// Very W.I.P RenderTarget performance improvement for my very fucking low vram
    /// I would appreciate everyone that want to continue this, pls pls pls
    /// </summary>
    public class TargetInstance
    {
        public RenderTarget2D render;

        // will dispose unused render after designated time
        public ushort disposeTimer;
        public const ushort disposeMax = 60 * 60 * 15; // dispose after 15 minutes
        public bool active;

        public void UpdateDispose()
        {

            if (disposeTimer < disposeMax)
            {
                disposeTimer++;
            }
            else
            {
                Dispose();
            }
        }
        public void SetNewRender()
        {

            active = true;
        }

        public void Dispose()
        {
            if (!Main.gameMenu)
            {
                Main.NewText("Disposed render");
            }
            active = false;
            render.Dispose();
            render = null;
        }
    }

    public class DrawRT
    {
        public int index;
        public virtual void DrawCapture(TargetInstance target)
        {

        }
        public virtual void DrawTarget(TargetInstance target)
        {

        }

        public virtual void GetAvailableTarget(List<TargetInstance> targets)
        {
            if (targets == null) return;
            if (targets.Count <= 0) return;

            for (int i = 0; i < targets.Count; i++)
            {
                var item = targets[i];
                if (!item.active)
                {
                    OnGetTarget(item);
                    item.SetNewRender();
                    index = i;
                }
            }
        }

        public virtual void OnGetTarget(TargetInstance target)
        {

        }
    }

    public class DrawProjRT : DrawRT
    {
        public short dye;
        public List<Projectile> projectiles;

        public override void DrawCapture(TargetInstance target)
        {
            foreach (var item in projectiles)
            {
                throw new System.NotImplementedException();
            }
        }

        public override void DrawTarget(TargetInstance target)
        {
            Main.spriteBatch.Draw(target.render, new Rectangle(), Color.White);
        }
    }

    public class ExampleManager
    {
        /// <summary>
        /// The actual targets thread
        /// </summary>
        public List<TargetInstance> targets;

        /// <summary>
        /// Save on where to draw the render target
        /// </summary>
        public Dictionary<string, List<TargetInstance>> targetsDrawLayer;
        public Dictionary<string, List<DrawRT>> drawTargets;

        public void Load_DrawTargets()
        {
            drawTargets = new Dictionary<string, List<DrawRT>>
            {
                { "proj", new List<DrawRT>() },
                { "npc", new List<DrawRT>() }
            };

            drawTargets["proj"].Add(new DrawProjRT());
        }

        public void DisposeAll()
        {
            foreach (var item in targets)
            {
                item.Dispose();
            }
        }

        public void Draw()
        {
            
        }

        public void Capture()
        {

        }

    }
}