// using System.Collections.Generic;
// using Microsoft.Xna.Framework;
// using Microsoft.Xna.Framework.Graphics;
// using Terraria;
// using Terraria.ModLoader;

// namespace Gearedup.Helper.WIP
// {
//     /// <summary>
//     /// Very W.I.P RenderTarget performance improvement for my very fucking low vram
//     /// I would appreciate everyone that want to continue this, pls pls pls
//     /// </summary>
//     public class TargetInstance
//     {

//         /// <summary>
//         /// RenderTarget2D used, might be null or require resize so use CheckInvalid before accesing
//         /// </summary>
//         public RenderTarget2D render;

//         // will dispose unused render after designated time
//         public ushort disposeTimer;
//         public const ushort disposeMax = 60 * 60 * 15; // dispose after 15 minutes

//         /// <summary>
//         /// Check if render target is currently used
//         /// Set to true during valid capturing
//         /// Set to false after draw is complete
//         /// </summary>
//         public bool active;

//         public void Initialize()
//         {
//             disposeTimer = 0;
//             active = false;
//             render = null;
//         }

//         /// <summary>
//         /// Check if TargetInstance is Invalid
//         /// </summary>
//         /// <returns>true if renders is null or require resizing</returns>
//         public bool CheckInvalid()
//         {
//             return render == null;
//         }

//         public void UpdateDispose()
//         {
//             if (disposeTimer < disposeMax)
//             {
//                 disposeTimer++;
//             }
//             else
//             {
//                 Dispose();
//             }
//         }

//         public void SetNewRender(DrawRT drawInstance)
//         {
//             // check if we do require new render
//             if (CheckInvalid())
//             {
//                 // Try setting up new rendertarget 2d
//                 //render = new RenderTarget2D();
//             }
//             active = true;
//         }

//         public void Dispose()
//         {
//             if (!Main.gameMenu)
//             {
//                 Main.NewText("Disposed render");
//             }
//             active = false;
//             render.Dispose();
//             render = null;
//             disposeTimer = 0;
//         }
//     }

//     public class DrawRT
//     {
//         public int index;
        
//         public const int IndexID_None = -1;
//         public const int IndexID_ErrorHappen = -2;

//         public bool Valid => index >= 0;

//         public bool ShitHappens => index == IndexID_ErrorHappen;

//         /// <summary>
//         /// Called after draw capture completed, regardless if its valid or not
//         /// Used to clear unused arrays and reference to free memory
//         /// Require to call SetIndexNone at the end, it is crucial
//         /// </summary>
//         /// <param name="target"></param>
//         public virtual void CleanDrawCapture(TargetInstance target)
//         {
//             SetIndexNone();
//         }

//         /// <summary>
//         /// Initialize the draw render target
//         /// Require to call SetIndexNone at the end, it is crucial
//         /// </summary>
//         public virtual void Initialize()
//         {
//             SetIndexNone();
//         }

//         /// <summary>
//         /// Set index to none, Used in Initialize and CleanDrawCapture
//         /// </summary>
//         public void SetIndexNone()
//         {
//             index = IndexID_None;
//         }

//         /// <summary>
//         /// Set index to an error id, Used for debugging
//         /// </summary>
//         public void SetIndexError()
//         {
//             index = IndexID_ErrorHappen;
//         }

//         /// <summary>
//         /// Called when the rendertarget is capturing
//         /// Require spritebatch to Begin and End
//         /// </summary>
//         /// <param name="target">The render target used</param>
//         public virtual void DrawCapture(TargetInstance target)
//         {

//         }

//         /// <summary>
//         /// Draw the captured target
//         /// Requires begin and end
//         /// </summary>
//         /// <param name="target"></param>
//         public virtual void DrawTarget(TargetInstance target)
//         {

//         }

//         /// <summary>
//         /// 
//         /// </summary>
//         /// <param name="targets"></param>
//         /// <returns></returns>
//         public virtual bool GetAvailableTarget(TargetInstance[] targets)
//         {
//             // safe code, in case idk
//             if (targets == null) return false;
//             if (targets.Length <= 0) return false;

//             // if (ShitHappens)
//             // {
//             //     Main.NewText("Previous draw had null value, reporting as a warn");
//             // }

//             SetIndexNone();

//             for (int i = 0; i < targets.Length; i++)
//             {
//                 if (targets[i] == null)
//                 {
//                     targets[i] = new TargetInstance();
//                     targets[i].Initialize();
//                     targets[i].SetNewRender(this);
//                     OnGetTarget(targets[i]);
//                     index = i;
//                     return true;
//                 }
//                 if (!targets[i].active)
//                 {
//                     targets[i].SetNewRender(this);
//                     OnGetTarget(targets[i]);
//                     index = i;
//                     return true;
//                 }
//             }
//             return false;
//         }

//         public virtual void OnGetTarget(TargetInstance target)
//         {

//         }
//     }

//     public class DrawProjRT : DrawRT
//     {
//         public short dye;
//         public List<Projectile> projectiles;

//         public DrawProjRT(short shader, Projectile projectile)
//         {
//             dye = shader;
//             projectiles = new List<Projectile>(){projectile};
//         }

//         public override void DrawCapture(TargetInstance target)
//         {
//             foreach (var item in projectiles)
//             {
//                 throw new System.NotImplementedException();
//             }
//         }

//         public override void DrawTarget(TargetInstance target)
//         {
//             if (target == null)
//             {
//                 Main.NewText("Target instance somehow null, what the fuck mike");
//                 target = new TargetInstance();
//                 return;
//             }
//             if (target.CheckInvalid())
//             {
//                 // we create new render and dont draw
//                 target = new TargetInstance();
//                 target.SetNewRender(this);
//                 SetIndexError();
//                 return;
//             }
//             Main.spriteBatch.Draw(target.render, new Rectangle(), Color.White);
//         }

//         public override void CleanDrawCapture(TargetInstance target)
//         {
//             index = IndexID_None;
//             projectiles.Clear();
//         }
//     }

//     public class RenderManager : ModSystem
//     {
//         public override void Load()
//         {
//             base.Load();
//         }

//         public override void Unload()
//         {
//             base.Unload();
//         }

//         public override void OnWorldLoad()
//         {
//             Load();
//         }

//         public override void OnWorldUnload()
//         {
//             Unload();
//         }

//         public override void PostDrawTiles()
//         {
//             base.PostDrawTiles();
//         }

//         /// <summary>
//         /// The instances of rendertarget2d compiled in 1 array
//         /// Used by all other DrawRT Objects using DrawRT.index 
//         /// </summary>
//         public TargetInstance[] targets;

//         /// <summary>
//         /// Max Render Target in the screen, around 300 sounds fine tbh 
//         /// </summary>
//         public const short MaxTargets = 300;

//         public List<DrawRT> drawTargetsProj;
//         public List<DrawRT> drawTargetsNPCs;

//         /// <summary>
//         /// Load important properties
//         /// </summary>
//         public void Load_DrawTargets()
//         {
//             targets = new TargetInstance[MaxTargets];

//             drawTargetsProj = new List<DrawRT>();
//             drawTargetsNPCs = new List<DrawRT>();
//         }
        
//         /// <summary>
//         /// Create a new target for dyed projectiles
//         /// </summary>
//         /// <param name="dye">The Shader ID</param>
//         /// <param name="projectile">The projectile to be added, must be active</param>
//         public void AddTarget_Proj(short dye, Projectile projectile)
//         {
//             // if some shit happen
//             if ( projectile == null || !projectile.active || projectile.hide) return;

//             // if drawTargetsPorj has no shit, we just make one
//             if (drawTargetsProj.Count <= 0)
//             {
//                 drawTargetsProj.Add(new DrawProjRT(dye, projectile));
//             }

//             // adds a new draw target
//             foreach (DrawProjRT item in drawTargetsProj)
//             {
//                 if (item == null) continue;
//                 if (item.dye == dye)
//                 {
//                     if (item.projectiles == null)
//                     {
//                         item.projectiles = new List<Projectile>();
//                     }
//                     item.projectiles.Add(projectile);
//                 }
//             }

//             // if there is no matching draw targets, we make one
//             drawTargetsProj.Add(new DrawProjRT(dye, projectile));
//         }

//         /// <summary>
//         /// Dispose all target instance and drawrt
//         /// </summary>
//         public void DisposeAll()
//         {
//             // foreach (var item in targets)
//             // {
//             //     item.Dispose();
//             // }

//             for (int i = 0; i < targets.Length; i++)
//             {
//                 targets[i].Dispose();
//             }

//         }

//         public void Draw_NPC()
//         {

//         }

//         public void Draw_Tiles()
//         {

//         }

//         public void Draw_PreTiles()
//         {

//         }

//         public void Draw_Player()
//         {
//         }

//         public void Draw_Proj()
//         {
//             TryDraw(ref drawTargetsProj);
//         }

//         public bool TryDraw(ref List<DrawRT> draw)
//         {
//             foreach (var item in draw)
//             {
//                 // only draw valid targets
//                 if (item.Valid)
//                 {
//                     item.DrawTarget(targets[item.index]);
//                 }

//                 // would still clean draw capture otherwise
//                 item.CleanDrawCapture(targets[item.index]);
//             }
//             return false;
//         }

//         public void Capture()
//         {
//             // Prepare targets
//             if (!Capture_PrepareTargets()) return;

//             // capture our draw targets
//             Capture_Projectile();
//             Capture_NPCs();

//             // After done draw capturing , we update unused targetinstance
//             foreach (var item in targets)
//             {
//                 item.UpdateDispose();
//             }
//         }

//         /// <summary>say no
//         /// Prepare targets, if targets somehow didnt initialize we 
//         /// </summary>
//         /// <returns></returns>
//         public bool Capture_PrepareTargets()
//         {
//             // try reloading properties
//             if (targets == null || targets.Length <= 0)
//             {
//                 Load_DrawTargets();
//                 return false;
//             }
//             else if (drawTargetsNPCs == null || drawTargetsProj == null)
//             {
//                 Load_DrawTargets();
//                 return false;
//             }
//             return true;
//         }

//         /// <summary>
//         /// Capture NPC Drawing
//         /// </summary>
//         public void Capture_NPCs()
//         {

//         }

//         /// <summary>
//         /// Capture Projectile Drawing
//         /// </summary>
//         public void Capture_Projectile()
//         {
//             foreach (var item in drawTargetsProj)
//             {
//                 if (item.GetAvailableTarget(targets))
//                 {
//                     item.DrawCapture(targets[item.index]);
//                 }
//             }
//         }

//     }
// }