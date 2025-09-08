using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using Gearedup.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace Gearedup.Content.Items
{

    public class PhilosophySystem : ModSystem
    {
        public static PhilosophySystem Get => ModContent.GetInstance<PhilosophySystem>();
        public enum GodlyCondition
        {
            OnShimmer,
            Wet,
            WetLava,
            WetHoney,
            InJungle,
            InMushroom,
            InSpace,
            InOcean,
            InHell,
            InOceanOrBottomAbyss,
            AliveWallOfFlesh,
            AliveLunaticCultist,
            AliveMoonlord,
            HaveBuffIchorOrCursedFlame,
            HaveBuffCursed,
            HaveBuffPoisoned,
            HaveBuffOnFire,
            HaveNoWeapons,
            EquippedFullWoodenArmor,
            AliveBetsy,
            LowHP,
            HaveExactly1000Dirt,
            NoAngler,
            NoMerchant,
            NoGuide,
            HaveAnyPooBlocks,
            HaveBuffStink,
            HaveBuffLoveStruck,
            HaveCoppershortsword,
            Count
        }

        public ulong oweByGods;
        public List<TypeID> revealedConditions = new List<TypeID>();

        public override void LoadWorldData(TagCompound tag)
        {
            revealedConditions = new List<TypeID>();

            oweByGods = tag.Get<ulong>("oweByGods");
            int revealCount = tag.GetInt("revealedCount");

            if (revealCount <= 0) return;

            for (int i = 0; i < revealCount; i++)
            {
                // var cond = new TypeID(tag, "cond_" + i);
                // cond.ValidateAsItem();
                revealedConditions.Add(new TypeID(tag, "cond_" + i));
            }
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["oweByGods"] = oweByGods;
            tag["revealedCount"] = revealedConditions.Count;

            if (revealedConditions.Count <= 0) return;

            for (int i = 0; i < revealedConditions.Count; i++)
            {
                revealedConditions[i].Save(tag, "cond_" + i);
            }

            revealedConditions = new List<TypeID>();
        }

        public byte[] conditionsByType;
        public override void PostAddRecipes()
        {
            // random conditions
            conditionsByType = new byte[ItemLoader.ItemCount];
            UnifiedRandom random = new UnifiedRandom(69);

            for (int i = 0; i < ItemLoader.ItemCount; i++)
            {
                conditionsByType[i] = (byte)random.Next((byte)GodlyCondition.OnShimmer, (byte)GodlyCondition.Count);
            }
        }

        public override void Unload()
        {
            conditionsByType = null;
            revealedConditions = null;
        }

        public static bool ConditionRevealed(int type)
        {
            foreach (var typeID in Get.revealedConditions)
            {
                if (typeID.id is int id)
                {
                    if (id == type) return true;
                }
            }
            return false;
        }

        public static bool ConditionApprove(Item item, Player player)
        {
            byte condition = Get.conditionsByType[item.type];
            switch ((GodlyCondition)condition)
            {
                case GodlyCondition.OnShimmer: return player.ZoneShimmer;
                case GodlyCondition.Wet: return player.wet;
                case GodlyCondition.WetLava: return player.lavaWet;
                case GodlyCondition.WetHoney: return player.honeyWet;
                case GodlyCondition.InJungle: return player.ZoneJungle;
                case GodlyCondition.InMushroom: return player.ZoneGlowshroom;
                case GodlyCondition.InSpace: return player.ZoneSkyHeight;
                case GodlyCondition.InOcean: return player.ZoneBeach;
                case GodlyCondition.InHell: return player.ZoneUnderworldHeight;
                case GodlyCondition.InOceanOrBottomAbyss: return player.ZoneBeach; // Calamity 
                case GodlyCondition.AliveWallOfFlesh: return NPC.AnyNPCs(NPCID.WallofFlesh);
                case GodlyCondition.AliveLunaticCultist: return NPC.AnyNPCs(NPCID.CultistBoss);
                case GodlyCondition.AliveMoonlord: return NPC.AnyNPCs(NPCID.MoonLordCore);
                case GodlyCondition.HaveBuffIchorOrCursedFlame: return player.HasBuff(BuffID.Ichor) || player.HasBuff(BuffID.CursedInferno);
                case GodlyCondition.HaveBuffCursed: return player.HasBuff(BuffID.Cursed);
                case GodlyCondition.HaveBuffPoisoned: return player.HasBuff(BuffID.Poisoned);
                case GodlyCondition.HaveBuffOnFire: return player.HasBuff(BuffID.OnFire);
                case GodlyCondition.HaveNoWeapons: return !player.inventory.Any(i => i.damage > 0 && i.type != ItemID.None);
                case GodlyCondition.EquippedFullWoodenArmor:
                    return player.armor[0]?.type == ItemID.WoodHelmet &&
                       player.armor[1]?.type == ItemID.WoodBreastplate &&
                       player.armor[2]?.type == ItemID.WoodGreaves;
                case GodlyCondition.AliveBetsy: return NPC.AnyNPCs(NPCID.DD2Betsy);
                case GodlyCondition.LowHP: return player.statLife <= player.statLifeMax2 / 4;
                case GodlyCondition.HaveExactly1000Dirt:
                    return player.inventory.Count(i => i.type == ItemID.DirtBlock) == 1 &&
                       player.inventory.FirstOrDefault(i => i.type == ItemID.DirtBlock)?.stack == 1000;
                case GodlyCondition.NoAngler: return !NPC.AnyNPCs(NPCID.Angler);
                case GodlyCondition.NoMerchant: return !NPC.AnyNPCs(NPCID.Merchant);
                case GodlyCondition.NoGuide: return !NPC.AnyNPCs(NPCID.Guide);
                case GodlyCondition.HaveAnyPooBlocks: return player.inventory.Any(i => i.type == ItemID.PoopBlock);
                case GodlyCondition.HaveBuffStink: return player.HasBuff(BuffID.Stinky);
                case GodlyCondition.HaveBuffLoveStruck: return player.HasBuff(BuffID.Lovestruck);
                case GodlyCondition.HaveCoppershortsword: return player.inventory.Any(i => i.type == ItemID.CopperShortsword);
                default: break;
            }
            return !player.dead;
        }
        public static string ConditionDescription(int type)
        {
            byte condition = Get.conditionsByType[type];
            switch ((GodlyCondition)condition)
            {
                case GodlyCondition.OnShimmer: return "near shimmer";
                case GodlyCondition.Wet: return "is wet";
                case GodlyCondition.WetLava: return "is submerged in lava";
                case GodlyCondition.WetHoney: return "is submerged in honey";
                case GodlyCondition.InJungle: return "in jungle";
                case GodlyCondition.InMushroom: return "in mushroom biome";
                case GodlyCondition.InSpace: return "in space";
                case GodlyCondition.InOcean: return "in ocean";
                case GodlyCondition.InHell: return "in hell";
                case GodlyCondition.InOceanOrBottomAbyss: return "in the abyss";
                case GodlyCondition.AliveWallOfFlesh: return "near Wall of Flesh";
                case GodlyCondition.AliveLunaticCultist: return "near Lunatic Cultist";
                case GodlyCondition.AliveMoonlord: return "near Moon Lord";
                case GodlyCondition.HaveBuffIchorOrCursedFlame: return "has Ichor or Cursed Inferno buff";
                case GodlyCondition.HaveBuffCursed: return "has Cursed buff";
                case GodlyCondition.HaveBuffPoisoned: return "has Poisoned buff";
                case GodlyCondition.HaveBuffOnFire: return "has On Fire! buff";
                case GodlyCondition.HaveNoWeapons: return "has no weapons";
                case GodlyCondition.EquippedFullWoodenArmor: return "wearing full wooden armor";
                case GodlyCondition.AliveBetsy: return "near Betsy";
                case GodlyCondition.LowHP: return "low HP";
                case GodlyCondition.HaveExactly1000Dirt: return "has exactly 1000 dirt blocks";
                case GodlyCondition.NoAngler: return "no Angler present";
                case GodlyCondition.NoMerchant: return "no Merchant present";
                case GodlyCondition.NoGuide: return "no Guide present";
                case GodlyCondition.HaveAnyPooBlocks: return "has any poo blocks";
                case GodlyCondition.HaveBuffStink: return "has Stinky buff";
                case GodlyCondition.HaveBuffLoveStruck: return "has Lovestruck buff";
                case GodlyCondition.HaveCoppershortsword: return "has Copper Shortsword";
                default: break;
            }
            return "is alive";
        }

        public override void PreUpdateEntities()
        {
            PhilosophyTooltip.seeTooltip = false;
            base.PreUpdateEntities();
        }
        // public override void PostDrawInterface(SpriteBatch spriteBatch)
        // {
        //     PhilosophyTooltip.seeTooltip = false;
        // }
    }

    public class PhilosophyTooltip : GlobalItem
    {
        public static bool seeTooltip = false;

        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type != ModContent.ItemType<ValueManipulator>();
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (!seeTooltip) return;
            if (!ValueManipulator.IsManipulatable(item)) return;

            int valType = ModContent.ItemType<ValueManipulator>();

            tooltips.Add(new TooltipLine(Mod, "valueEnergy", $"[i:{valType}] Contains {ValueManipulator.GetEnergyText(item)} Pure Energy"){OverrideColor = Color.LightSalmon});
            if (item.stack > 1)
            {
                tooltips.Add(new TooltipLine(Mod, "valueEnergy", $"[i:{valType}] Contains {ValueManipulator.GetEnergyPerItem(item):N0} Pure Energy per stack"){OverrideColor = Color.LightSalmon});
            }

            if (PhilosophySystem.ConditionRevealed(item.type))
            {
                tooltips.Add(new TooltipLine(Mod, "duplicateCondition", $"[i:{valType}] Duplication condition : User {PhilosophySystem.ConditionDescription(item.type)}") { OverrideColor = Color.LightSalmon });
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "duplicateCondition", $"[i:{valType}] Duplication condition : Unknown") { OverrideColor = Color.LightSalmon });
            }
        }
    }
    public class ValueManipulator : ModItem
    {
        public ulong energy;

        // ulong (unsigned 64-bit integer) limit: 0 to 18,446,744,073,709,551,615
        // uint (unsigned 32-bit integer) limit: 0 to 4,294,967,295

        public override void SaveData(TagCompound tag)
        {
            tag.Add("energy", energy);
        }

        public override void LoadData(TagCompound tag)
        {
            energy = tag.Get<ulong>("energy");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(energy);
        }

        public override void NetReceive(BinaryReader reader)
        {
            energy = reader.ReadUInt64();
        }

        public override void SetDefaults()
        {
            Item.width = 10;
            Item.height = 10;
            Item.rare = ItemRarityID.Red;
            Item.consumable = false;
        }

        public override void UpdateInventory(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                PhilosophyTooltip.seeTooltip = true;
            }
        }

        public override bool ConsumeItem(Player player) => false;
        public override bool CanRightClick() => true;
        public override void RightClick(Player player)
        {
            if (Main.mouseItem != null && !Main.mouseItem.IsAir && IsManipulatable(Main.mouseItem))
            {
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                {
                    Absorb(Main.mouseItem);
                }
                else
                {
                    Duplicate(Main.mouseItem, player);
                }

            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
            {
                tooltips.Add(new TooltipLine(Mod, "tt", Language.GetTextValue("Mods.Gearedup.Items.ValueManipulator.TooltipReveal", energy.ToString("N0"))));
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "tt", Language.GetTextValue("Mods.Gearedup.Items.ValueManipulator.TooltipUnreveal", energy.ToString("N0"))));
            }
        }

        public static ulong GetEnergyPerItem(Item item)
        {
            ulong rare = (ulong)(Math.Abs(item.rare) + 1);
            return (((ulong)item.value + 1) * rare * rare) + (ulong)item.damage + ((ulong)item.pick * 10);
        }

        public static ulong GetEnergy(Item item)
        {
            return GetEnergyPerItem(item) * (ulong)item.stack;
        }
        public static string GetEnergyText(Item item)
        {
            return GetEnergy(item).ToString("N0");
        }

        public static bool IsManipulatable(Item item)
        {
            //disable valueless item
            if (item.value <= 1) return false;
            // disable duplicating useless tiles
            // if (item.createTile >= TileID.Dirt && item.value <= 1) return false;
            return true;
        }

        public void Absorb(Item item)
        {
            PhilosophySystem.Get.revealedConditions.Add(new TypeID(item));
            energy += GetEnergy(item);
            item.TurnToAir();
        }

        public void Duplicate(Item item, Player player)
        {
            ulong energyReq = GetEnergy(item);

            if (energy >= energyReq)
            {
                bool approved = PhilosophySystem.ConditionApprove(item, player);
                bool spared = Main.rand.NextBool(3) && !approved;
                if (approved || spared)
                {
                    PhilosophySystem.Get.oweByGods += energyReq / 2;
                    energy -= energyReq;

                    if (spared)
                    {
                        var typeID = new TypeID(item);
                        if (!PhilosophySystem.Get.revealedConditions.Contains(typeID))
                        {
                            CombatText.NewText(player.Hitbox, Color.AntiqueWhite, "The truth spoke and gave you its knowledge.");
                            PhilosophySystem.Get.revealedConditions.Add(typeID);
                        }
                        else
                        {
                            CombatText.NewText(player.Hitbox, Color.AntiqueWhite, "Duplication of " + item.Name + " weirdly granted.");
                        }
                    }
                    else
                    {
                        CombatText.NewText(player.Hitbox, Color.AntiqueWhite, "Duplication of " + item.Name + " granted.");
                    }

                    if (item.maxStack > 2 && item.stack < item.maxStack)
                    {
                        item.stack += 1;
                    }
                    else
                    {
                        player.QuickSpawnClonedItemDirect(Item.GetSource_GiftOrReward("philosophy stone"), item);
                    }

                    // player.QuickSpawnClonedItemDirect(Item.GetSource_GiftOrReward("philosophy stone"), item);
                }
                else
                {
                    // player.AddBuff<TruthDemise>(Math.Min((int)energyReq / 1000, 60 * 60 * 60 * 60));
                    if (player.TryGetModPlayer<GearPlayer>(out GearPlayer gp))
                    {
                        gp.applyTruthDemise = true;
                        gp.truthDemiseTime = 60 * 60 * 60 * 60;
                    }
                    CombatText.NewText(player.Hitbox, Color.Maroon, "Failed.");
                    SoundEngine.PlaySound(new SoundStyle("Gearedup/Sounds/Truth"));
                    player.DropItems();
                    player.KillMe(PlayerDeathReason.ByPlayerItem(player.whoAmI, Item), 10000, 1);
                }
            }
            else
            {
                CombatText.NewText(player.Hitbox, Color.Maroon, "Not enough pure energy, mortal.");
            }
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            string text = "";

            if (Main.mouseItem != null && !Main.mouseItem.IsAir)
            {
                if (IsManipulatable(Main.mouseItem))
                {
                    if (PhilosophySystem.ConditionRevealed(Main.mouseItem.type))
                    {
                        text = "_identify";
                        if (Main.LocalPlayer != null && Main.LocalPlayer.active && PhilosophySystem.ConditionApprove(Main.mouseItem, Main.LocalPlayer))
                        {
                            text = "_full";
                        }
                    }
                }
            }
            else
            {
                text = "_full";
            }

            Texture2D itemText = ModContent.Request<Texture2D>(Texture + text).Value;
            spriteBatch.Draw(itemText, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.PhilosophersStone)
                .AddIngredient<WhiteStar>(15)
                .AddIngredient(ItemID.SoulofMight, 20)
                .AddIngredient(ItemID.SoulofSight, 20)
                .AddIngredient(ItemID.SoulofFright, 20)
                .AddModIngredient(Gearedup.Get.calamityMod, "LifeAlloy", 5)
                .AddIngredient(ItemID.FragmentNebula, 35)
                .AddIngredient(ItemID.FragmentSolar, 35)
                .AddIngredient(ItemID.FragmentVortex, 35)
                .AddIngredient(ItemID.FragmentStardust, 35)
                .AddIngredient(ItemID.LunarBar, 50)
                .AddModIngredient(Gearedup.Get.calamityMod, "AscendantSpiritEssence", 5)
                .AddModIngredient(Gearedup.Get.calamityMod, "ShadowspecBar", 10)
                .DisableDecraft()
                .Register();
        }
    }


    public class TruthDemise : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.TryGetModPlayer<GearPlayer>(out GearPlayer gp))
            {
                gp.truthDemise = true;
                int buffTime = player.buffTime[buffIndex];
                if (buffTime > 2)
                {
                    gp.truthDemiseTime = buffTime;
                }
            }
        }
    }
}