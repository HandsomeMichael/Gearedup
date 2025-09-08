using System;
using System.Collections.Generic;
using Gearedup.Content.Items;
using Gearedup.Helper;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup
{
    public class BossBagTooltip : GlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return ItemID.Sets.BossBag[entity.type];
        }

        public override void OnConsumeItem(Item item, Player player)
        {
            if (Main.LocalPlayer.TryGetModPlayer(out GearPlayer gp))
            {
                if (gp.getBossBag)
                {
                    CombatText.NewText(new Rectangle((int)player.BottomLeft.X, (int)player.BottomLeft.Y + 50, 10, 10), Color.CornflowerBlue, $"Dropped all loot from '{item.Name}'");
                    // heal the player
                    player.Heal(10);
                }
            }
        }

        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            itemLoot.Add(new LeadingConditionRule(new Conditions.IsMasterMode())).OnSuccess(ItemDropRule.Common(ModContent.ItemType<LootOrb>(), 1000));
            // itemLoot.Add(new LeadingConditionRule(Conditions.IsMasterMode),);
            // itemLoot.Add(ItemDropRule.NormalvsExpert(ModContent.ItemType<LootOrb>(), 1000, 500));
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (Main.LocalPlayer.TryGetModPlayer(out GearPlayer gp))
            {
                if (gp.getBossBag)
                {
                    // tooltips.Add(new TooltipLine(Mod, "fullbag", "Will receive all of boss drop") { OverrideColor = Color.AntiqueWhite });

                    // var displayDrop = BossBagPatchSystem.bossBagDisplayItem[item.type];
                    
                    if (BossBagPatchSystem.bossBagDisplayItem.TryGetValue(item.type,out Dictionary<int,int> displayDrop) && displayDrop.Count > 0)
                    {
                        string displayed = "Will Always Drop : \n";
                        int i = 0;
                        foreach (var drops in displayDrop)
                        {
                            i++;
                            if (i >= 10)
                            {
                                displayed += "\n";
                                i = 0;
                            }
                            displayed += $"[i/s{drops.Value}:{drops.Key}]";
                        }
                        tooltips.Add(new TooltipLine(Mod, "items", displayed) { OverrideColor = Color.AntiqueWhite });
                    }
                    else
                    {
                        tooltips.Add(new TooltipLine(Mod, "items", "Have 1% chance to get double loot") { OverrideColor = Color.AntiqueWhite });
                    }

                    if (BossBagPatchSystem.bossBagDisplayConditionedItem.TryGetValue(item.type, out List<BossBagPatchSystem.BossBagConditionDrop> displayConditionDrop) && displayConditionDrop.Count > 0)
                    {
                        foreach (var dcp in displayConditionDrop)
                        {
                            Item displayItem = ContentSamples.ItemsByType[dcp.type];
                            if (dcp.condition != "" || dcp.condition != " ")
                            {
                                tooltips.Add(new TooltipLine(Mod, "items", $"Will receive [i/s{dcp.stack}:{dcp.type}] {displayItem.Name} when {dcp.condition}") { OverrideColor = Color.AntiqueWhite });
                            }
                            else
                            {
                                tooltips.Add(new TooltipLine(Mod, "items", $"Will receive [i/s{dcp.stack}:{dcp.type}] {displayItem.Name} for unknown condition") { OverrideColor = Color.AntiqueWhite });
                            }
                        }
                    }

                    tooltips.Add(new TooltipLine(Mod, "fullbag", $"[ [i:{ModContent.ItemType<LootOrb>()}] Loot Orb Effect ]") { OverrideColor = Color.AntiqueWhite });
                }

            }
        } 
    }

    public class BossBagPatchSystem : ModSystem
    {
        public static Dictionary<int, List<IItemDropRule>> bossBagNewRule;
        public static Dictionary<int, Dictionary<int,int>> bossBagDisplayItem;
        public static Dictionary<int, List<BossBagConditionDrop>> bossBagDisplayConditionedItem;

        public struct BossBagConditionDrop
        {
            public int type;
            public int stack;
            public string condition;

            public BossBagConditionDrop(int type, int stack, string condition)
            {
                this.type = type;
                this.stack = stack;
                this.condition = condition;
            }
        }

        public override void Load()
        {
            // already instanced in PostSetupContent
            // bossBagNewRule = new Dictionary<int, List<IItemDropRule>>();
            // bossBagDisplayItem = new Dictionary<int, Dictionary<int, int>>();
            On_Player.OpenBossBag += OpenBossPatch;
        }

        public override void Unload()
        {
            bossBagNewRule = null;
            bossBagDisplayItem = null;
            // bossBagDisplayConditionedItem = null;
            On_Player.OpenBossBag -= OpenBossPatch;
        }

        private void OpenBossPatch(On_Player.orig_OpenBossBag orig, Player self, int type)
        {

            if (self.TryGetModPlayer(out GearPlayer gp))
            {
                if (gp.getBossBag)
                {

                    DropAttemptInfo info = new()
                    {
                        player = self,
                        item = type,
                        IsExpertMode = Main.expertMode,
                        IsMasterMode = Main.masterMode,
                        IsInSimulation = false,
                        rng = Main.rand,
                    };
                    
                    var rules = bossBagNewRule[type];

                    if (rules.Count > 0)
                    {
                        for (int i = 0; i < rules.Count; i++)
                        {
                            ResolveRule(rules[i], info);
                        }
                    }
                    else
                    {
                        if (Main.rand.NextBool(100))
                        {
                            CombatText.NewText(self.Hitbox, Color.Red, "Double Loot !");
                            orig(self, type);
                        }
                        orig(self, type);
                    }

                    // Reflection didnt work for some reason

                    // var resolverType = typeof(ItemDropResolver);
                    // var resolveRuleMethod = resolverType.GetMethod("ResolveRule", System.Reflection.BindingFlags.NonPublic);

                    // for (int i = 0; i < rules.Count; i++)
                    // {
                    //     resolveRuleMethod?.Invoke(Main.ItemDropSolver, new object[] { rules[i], info });   
                    // }

                    return;
                }
            }

            orig(self, type);
        }

        private ItemDropAttemptResult ResolveRule(IItemDropRule rule, DropAttemptInfo info)
        {
            if (!rule.CanDrop(info)) {
                ItemDropAttemptResult itemDropAttemptResult = default(ItemDropAttemptResult);
                itemDropAttemptResult.State = ItemDropAttemptResultState.DoesntFillConditions;
                ItemDropAttemptResult itemDropAttemptResult2 = itemDropAttemptResult;
                ResolveRuleChains(rule, info, itemDropAttemptResult2);
                return itemDropAttemptResult2;
            }

            ItemDropAttemptResult itemDropAttemptResult3 = ((!(rule is INestedItemDropRule nestedItemDropRule)) ? rule.TryDroppingItem(info) : nestedItemDropRule.TryDroppingItem(info, ResolveRule));
            ResolveRuleChains(rule, info, itemDropAttemptResult3);
            return itemDropAttemptResult3;
        }

        private void ResolveRuleChains(IItemDropRule rule, DropAttemptInfo info, ItemDropAttemptResult parentResult)
        {
            ResolveRuleChains(ref info, ref parentResult, rule.ChainedRules);
        }

        private void ResolveRuleChains(ref DropAttemptInfo info, ref ItemDropAttemptResult parentResult, List<IItemDropRuleChainAttempt> ruleChains)
        {
            if (ruleChains == null)
                return;

            for (int i = 0; i < ruleChains.Count; i++) {
                IItemDropRuleChainAttempt itemDropRuleChainAttempt = ruleChains[i];
                if (itemDropRuleChainAttempt.CanChainIntoRule(parentResult))
                    ResolveRule(itemDropRuleChainAttempt.RuleToChain, info);
            }
        }

        // public override void PostSetupContent()
        // {
        //     UpdateDBPlease();
        // }

        public static void ResetDictionaries()
        {
            bossBagNewRule = new Dictionary<int, List<IItemDropRule>>();
            bossBagDisplayItem = new Dictionary<int, Dictionary<int, int>>();
            bossBagDisplayConditionedItem = new Dictionary<int, List<BossBagConditionDrop>>();
        }

        public static void UpdateDBSample(Item item)
        {
            if (ItemID.Sets.BossBag[item.type])
            {
                RegisterDropsDB(item);
            }
        }

        // public static void UpdateDBPlease()
        // {
        //     bossBagNewRule = new Dictionary<int, List<IItemDropRule>>();
        //     bossBagDisplayItem = new Dictionary<int, Dictionary<int, int>>();
        //     bossBagDisplayConditionedItem = new Dictionary<int, List<BossBagConditionDrop>>();

        //     foreach (var item in ContentSamples.ItemsByType)
        //     {
        //         if (ItemID.Sets.BossBag[item.Key])
        //         {
        //             RegisterDropsDB(item.Value);
        //         }
        //     }

        // }

        public static void RegisterDropsDB(Item item)
        {
            Gearedup.Log("Registering drops " + item.Name + " " + item.ModItem?.ToString(), true);

            var listDrop = Main.ItemDropsDB.GetRulesForItemID((item.type));
            int lootOrbID = ModContent.ItemType<LootOrb>();

            Dictionary<int, int> itemToDrop = new Dictionary<int, int>();
            // List<ItemDropWithConditionRule> itemToDropConditions = new List<ItemDropWithConditionRule>();

            // new LeadingConditionRule(new Conditions.NotRemixSeed())).OnSuccess(ItemDropRule.OneFromOptions(1, 2611, 2624, 2622, 2621, 2623)

            bossBagNewRule[item.type] = new List<IItemDropRule>();
            bossBagDisplayItem[item.type] = new Dictionary<int, int>();
            bossBagDisplayConditionedItem[item.type] = new List<BossBagConditionDrop>();

            Gearedup.Log("- Drops total used " + listDrop.Count, true);

            foreach (IItemDropRule drop in listDrop)
            {
                // Calamity Mod does not use proper Condition , so i dont really want to use this part of the code

                // if (drop is LeadingConditionRule leadingUpCondition)
                // {
                //     foreach (var chainedRules in drop.ChainedRules)
                //     {
                //         List<DropRateInfo> dp = new List<DropRateInfo>();
                //         DropRateInfoChainFeed ri = new DropRateInfoChainFeed(1f);
                //         chainedRules.RuleToChain.ReportDroprates(dp, ri);

                //         foreach (var leadingDrop in dp)
                //         {
                //             itemToDropConditions.Add(new ItemDropWithConditionRule(
                //                 leadingDrop.itemId,
                //                 1,
                //                 leadingDrop.stackMax,
                //                 leadingDrop.stackMax,
                //                 leadingUpCondition.condition));
                //         }
                //     }
                //     continue;
                // }

                // // If there is a condition
                // if (drop is ItemDropWithConditionRule dropCondition)
                // {
                //     itemToDropConditions.Add(dropCondition);
                //     continue;
                // }

                List<DropRateInfo> dropRateInfos = new List<DropRateInfo>();
                DropRateInfoChainFeed ratesInfo = new DropRateInfoChainFeed(1f);

                drop.ReportDroprates(dropRateInfos, ratesInfo);

                foreach (var rate in dropRateInfos)
                {
                    // Gearedup.Log("-- Adding to loot drops : " + rate.itemId, true);

                    if (rate.itemId > 0 && rate.itemId != lootOrbID)
                    {
                        if (itemToDrop.ContainsKey(rate.itemId))
                        {
                            itemToDrop[rate.itemId] += rate.stackMax;
                        }
                        else
                        {
                            itemToDrop[rate.itemId] = rate.stackMax;
                        }
                    }
                }
            }

            // add condition based rules
            // if (itemToDropConditions.Count > 0)
            // {
            //     DropAttemptInfo dropInfo = new DropAttemptInfo
            //     {
            //         player = new Player(),
            //         item = item.type,
            //         IsExpertMode = true,
            //         IsMasterMode = true,
            //         IsInSimulation = true,
            //         rng = Main.rand
            //     };

            //     foreach (var drop in itemToDropConditions)
            //     {
            //         List<DropRateInfo> dropRates = new List<DropRateInfo>();
            //         DropRateInfoChainFeed ratesInfo = new DropRateInfoChainFeed();

            //         drop.ReportDroprates(dropRates, ratesInfo);

            //         foreach (var trueDrop in dropRates)
            //         {
            //             // add to our display
            //             // if (item.type != ItemID.CopperCoin || item.type != ItemID.SilverCoin || item.type != ItemID.GoldCoin || item.type != ItemID.PlatinumCoin)
            //             // {
            //             //     // bossBagDisplayItem[item.type].Add(new Tuple<int, int>(drop.Key, drop.Value));
            //             // }
            //             bossBagDisplayConditionedItem[item.type].Add(new BossBagConditionDrop(trueDrop.itemId, trueDrop.stackMax, drop.condition.GetConditionDescription()));
            //             bossBagNewRule[item.type].Add(ItemDropRule.ByCondition(drop.condition, trueDrop.itemId, 1, trueDrop.stackMax, trueDrop.stackMax));
            //         }
            //     }
            // }

            // add common rules
            foreach (var drop in itemToDrop)
            {
                // skip calamity shit
                if (Gearedup.Get.calamityMod != null)
                {
                    // ichor and cursed flame shouldnt drop prehardmode
                    if (drop.Key == ItemID.Ichor || drop.Key == ItemID.CursedFlame)
                    {
                        if (item.type == Gearedup.Get.calamityMod.ItemType("PerforatorBag") || item.type == Gearedup.Get.calamityMod.ItemType("HiveMindBag"))
                        {
                            bossBagDisplayConditionedItem[item.type].Add(new BossBagConditionDrop(drop.Key, drop.Value, "in Hardmode"));
                            bossBagNewRule[item.type].Add(ItemDropRule.ByCondition(new Conditions.IsHardmode(), drop.Key, 1, drop.Value, drop.Value + Math.Max(drop.Value / 7, 3)));
                            continue;
                        }
                    }

                    // we shouldnt get thank you painting 
                    if (drop.Key == Gearedup.Get.calamityMod.ItemType("ThankYouPainting"))
                    {
                        bossBagNewRule[item.type].Add(ItemDropRule.Common(drop.Key, 1000, 1, 1));
                        continue;
                    }

                    // only eye of cthulhu would drop revengance stuff, otherwise rare drop chance
                    if (item.type != ItemID.EyeOfCthulhuBossBag)
                    {
                        if (drop.Key == Gearedup.Get.calamityMod.ItemType("Laudanum"))
                        {
                            bossBagNewRule[item.type].Add(ItemDropRule.Common(drop.Key, 1000, 1, 1));
                            continue;
                        }
                        else if (drop.Key == Gearedup.Get.calamityMod.ItemType("HeartofDarkness"))
                        {
                            bossBagNewRule[item.type].Add(ItemDropRule.Common(drop.Key, 1000, 1, 1));
                            continue;
                        }
                        else if (drop.Key == Gearedup.Get.calamityMod.ItemType("StressPills"))
                        {
                            bossBagNewRule[item.type].Add(ItemDropRule.Common(drop.Key, 1000, 1, 1));
                            continue;
                        }
                    }
                }

                // add to our display
                if (drop.Key != ItemID.CopperCoin && drop.Key != ItemID.SilverCoin && drop.Key != ItemID.GoldCoin && drop.Key != ItemID.PlatinumCoin)
                {
                    bossBagDisplayItem[item.type].Add(drop.Key, drop.Value);
                }

                int stack = drop.Value;
                Item itemSample = ContentSamples.ItemsByType[drop.Key];

                // add atleast 3 more item to the stack
                if (itemSample.maxStack > 10 && drop.Value > 3)
                {
                    // if you get 49 materials, you will get 7 more
                    stack += Math.Max(drop.Value / 7, 3);
                    Gearedup.Log("- Adding drop material type at stack : " + stack, true);
                }

                Gearedup.Log("- Perfectly adding rule  " + itemSample.Name, true);
                bossBagNewRule[item.type].Add(ItemDropRule.Common(drop.Key, 1, drop.Value, stack));
                // bossBagNewRule[item.type].Add(ItemDropRule.Common(ItemID.DirtBlock, 1, drop.Value, stack));
            }

            // add empress custom drop
            if (item.type == ItemID.FairyQueenBossBag)
            {
                if (!itemToDrop.ContainsKey(ItemID.EmpressBlade))
                {
                    bossBagDisplayConditionedItem[item.type].Add(new BossBagConditionDrop(ItemID.EmpressBlade, 1, "when Moonlord is Defeated"));
                    bossBagNewRule[item.type].Add(ItemDropRule.ByCondition(new IsMoonLord(), ItemID.EmpressBlade));
                }
            }
        }
    }
}