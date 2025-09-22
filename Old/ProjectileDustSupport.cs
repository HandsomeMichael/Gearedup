// Moved to actual detour

// using Terraria;
// using Terraria.ModLoader;

// namespace Gearedup
// {
//     class ProjectileAITrack : GlobalProjectile
// 	{
// 		public static int currentAI = -1;
//         public override bool IsLoadingEnabled(Mod mod) => GearClientConfig.Get.DyeSupport_Dust || GearClientConfig.Get.DyeSupport_Light;

//         public override bool PreAI(Projectile projectile)
//         {
// 			currentAI = projectile.whoAmI;
//             return base.PreAI(projectile);
//         }
		
//         public override void OnKill(Projectile projectile, int timeLeft) => currentAI = -1;
//         public override void PostAI(Projectile projectile) => currentAI = -1;
//     }
// }