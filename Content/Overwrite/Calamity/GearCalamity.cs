using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup.Content.Overwrite
{
    public class GearCalamity : ModSystem
    {
        public Mod Calamity => Gearedup.Get.calamityMod;
        public override bool IsLoadingEnabled(Mod mod)
        {
            return ((Gearedup)mod).calamityMod != null;
        }

        // edit lots of recipe lil boy
        public override void PostAddRecipes()
        {
            // that jellyfish recipe
            // bloodmoon summon -> blood orb
            // bloodmoon summon requires blood orb , gel and leather
        }
    }

    public abstract class CalItemPatch : GlobalItem
    {
        public Mod Calamity => Gearedup.Get.calamityMod;

        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.ModItem != null && entity.ModItem.Mod.Name == "CalamityMod" && ItemName == entity.ModItem.Name;
        }

        public virtual string ItemName => "cum";
        public virtual string Version => "v2.0.5";

        public override bool IsLoadingEnabled(Mod mod)
        {
            if (Calamity != null)
            {
                if (Calamity.Version.ToString() == Version)
                {
                    return true;
                }
                else
                {
                    Gearedup.Get.AddError("{ " + this.FullName + " } Dont support calamity version " + Calamity.Version.ToString() + " only " + Version);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }

    public class EatableBloodOrb : CalItemPatch
    {
        public override string ItemName => "BloodOrb";
        public override void SetDefaults(Item entity)
        {
            entity.healLife = 125;
            entity.buffType = BuffID.Bleeding;
            entity.buffTime = 60 * 60; // 1 minute of bleeding
            entity.useAnimation = 20;
            entity.useTime = 20;
            entity.useStyle = ItemUseStyleID.EatFood;

            entity.ammo = entity.type;
            entity.notAmmo = true;
            entity.consumable = true;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "battle", "Grant 'Battle' effect during Hardmode or Bloodmoon"));
        }

        public override bool? UseItem(Item item, Player player)
        {
            // during bloodmoon or hardmode would apply battle potion effect
            if (Main.hardMode || Main.bloodMoon)
            {
                player.AddBuff(BuffID.Battle, 60 * 80);
            }
            return true;
        }
    }
}