// using System.Collections.Generic;
// using Terraria;
// using Terraria.Audio;
// using Terraria.ID;
// using Terraria.ModLoader;
// using Gearedup.Helper;
// using Microsoft.Xna.Framework.Graphics;
// using Microsoft.Xna.Framework;
// using Terraria.DataStructures;
// using Microsoft.CodeAnalysis.CSharp.Syntax;

// namespace Gearedup.Overwrite
// {
//     public class DepthMeterRewrite : GlobalItem
//     {
//         public override bool AppliesToEntity(Item entity, bool lateInstantiation)
//         {
//             return entity.type == ItemID.DepthMeter;
//         }
//         public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
//         {
//             tooltips.Add(new TooltipLine(Mod, "depthMeterOverwrite", "Place it to see how far a fall is"));
//         }
//         public override void SetDefaults(Item entity)
//         {
//             entity.useTime = 10;
//             entity.useAnimation = 10;
//             entity.useStyle = ItemUseStyleID.Swing;
//             entity.noMelee = true;
//             entity.noUseGraphic = true;
//             entity.shoot = ModContent.ProjectileType<DepthMeterProjectile>();
//         }
//         public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
//         {
//             var proj = Projectile.NewProjectileDirect(source,Main.MouseWorld,Vector2.Zero,type,0,0f,player.whoAmI);
//             if (proj != null)
//             {
//                 proj.netUpdate = true;
//             }

//             return false;
//         }
//     }

//     public class DepthMeterProjectile : ModProjectile
//     {
//         public override string Texture => DrawUtils.PathItem(ItemID.DepthMeter);
//         public override void SetDefaults()
//         {
//             Projectile.width = 5;
//             Projectile.height = 5;
//         }

//         public override void OnKill(int timeLeft)
//         {   
//         }

//         public override void AI()
//         {
//         }
//     }
// }