// using Gearedup.Helper.WIP;
// using Microsoft.Xna.Framework;
// using Microsoft.Xna.Framework.Graphics;
// using System.Collections.Generic;
// using Terraria;
// using Terraria.ModLoader;

// namespace Gearedup.Helper
// {

//     /// <summary>
//     /// We do it just like how terraria does it
//     /// </summary>
//     public struct Metaball
//     {
//         public Vector2 position;
//         public Vector2 velocity;
//         public float scale;

//         /// <summary>
//         /// There is only just a few types
//         /// </summary>
//         public byte type;

//         /// <summary>
//         /// nullable ai, if you want your metaballs to out meta the balls
//         /// </summary>
//         public int[] ai;

//         /// <summary>
//         /// Base metaball, no values, no types, considered inactive
//         /// </summary>
//         public Metaball()
//         {
//             position = Vector2.Zero;
//             velocity = Vector2.Zero;
//             scale = 1f;
//             ai = null;
//             type = 0;
//         }

//         public void Update()
//         {

//         }
//         public void Draw(TargetInstance target)
//         {

//         }
//         public void Capture(TargetInstance target)
//         {

//         }
//         public void ShouldDraw()
//         {

//         }
//         public void Dispose()
//         {

//         }
//     }

//     public abstract class ModMetaballs : DrawRT
//     {
//         // Maximum of 
//         public virtual int MaxMetaballs => 100;
//         public Metaball[] metaballs;

//         public override void Initialize()
//         {
//             metaballs = new Metaball[MaxMetaballs];
//         }

//         public override void DrawCapture(TargetInstance target)
//         {
//             base.DrawCapture(target);
//         }

//         public override void CleanDrawCapture(TargetInstance target)
//         {
//             metaballs = new Metaball[MaxMetaballs];
//         }

//         public override void OnGetTarget(TargetInstance target)
//         {
//             base.OnGetTarget(target);
//         }

//         public override void DrawTarget(TargetInstance target)
//         {
//             base.DrawTarget(target);
//         }
//     }

//     public class MyMetaballs : ModMetaballs
//     {

//     }
// }