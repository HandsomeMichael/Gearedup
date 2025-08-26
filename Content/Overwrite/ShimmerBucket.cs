using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup.Content.Overwrite
{
    public class ShimmerBucket : GlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ItemID.BottomlessShimmerBucket;
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "shim", "Hold item and right click this to shimmer it"));
        }
        public override bool ConsumeItem(Item item, Player player) => false;
        public override bool CanRightClick(Item item) => true;
        public override void RightClick(Item item, Player player)
        {
            if (Main.mouseItem != null)
            {
                if (Main.mouseItem.CanShimmer())
                {
                    Vector2 lastPos = Main.mouseItem.Center;
                    Main.mouseItem.Center = player.Center;

                    typeof(Item).GetMethod("GetShimmered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                        ?.Invoke(Main.mouseItem, null);

                    Main.mouseItem.Center = lastPos;
                }
            }
        }
    }
}