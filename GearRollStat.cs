using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Gearedup
{

    public class RollStatSystem : ModSystem
    {
        public static RollStatSystem Get => ModContent.GetInstance<RollStatSystem>();

        public enum RareID
        {
            // Always appear
            None = 0,
            NoneHardmode = 1,
            NonePrehardmode = 2,

            // Common pull
            Common = 3, 
            CommonPrehardmode = 4,
            CommonHardmode = 5,

            // Rare pull
            Rare = 10, 
            RarePrehardmode = 11,
            RareHarmode = 12,

            // Mythical pull
            Mythic = 25,
            MythicPrehardmode = 26,
            MythicHardmode = 27,


            Legend = 50,// 1 / 100
            LegendPreHarmode = 51,
            LegendHardmode = 52,
            Lunarian = 100
        }

        public struct RollStat
        {
            public string name;
            public int minValue;
            public int maxValue;

            public RareID rare = RareID.None;

            public Func<Item, bool> requirement = null;
            public Action<Item, bool, RollStat, RollStat> amplifyOther = null;
            public string[] preventRoll = null;

            public RollStat(RareID rarity, string name, int minValue, int maxValue, Func<Item, bool> requirement = null, string[] preventRoll = null)
            {
                this.rare = rarity;
                this.name = name;
                this.minValue = minValue;
                this.maxValue = maxValue;
                this.requirement = requirement;
                this.preventRoll = preventRoll;
            }

            public RollStat(RareID rarity, string name, Action<Item, bool, RollStat, RollStat> amplifyOther = null, string[] preventRoll = null)
            {
                this.rare = rarity;
                this.name = name;
                this.amplifyOther = amplifyOther;
                this.preventRoll = preventRoll;
                this.requirement = null;
            }

        }

        #region Requirements

        public bool ReqNoChannel(Item item) => !item.channel;
        public bool ReqMelee(Item item) => item.DamageType == DamageClass.Melee;
        public bool ReqNoMelee(Item item) => item.DamageType != DamageClass.Melee;
        public bool ReqCMelee(Item item) => !item.channel && item.DamageType == DamageClass.Melee;
        public bool ReqRanged(Item item) => item.DamageType == DamageClass.Ranged;
        public bool ReqCRanged(Item item) => !item.channel && item.DamageType == DamageClass.Ranged;
        public bool ReqNoSummon(Item item) => item.DamageType != DamageClass.Summon;
        public bool ReqHasKB(Item item) => item.knockBack > 2;

        public static bool ReqLessOnTools(Item item)
        {
            if (item.pick > 0 || item.hammer > 0 || item.axe > 0)
            {
                return Main.rand.NextBool(20);
            }
            return true;
        }
        public static bool ReqNoTools(Item item)
        {
            if (item.pick > 0 || item.hammer > 0 || item.axe > 0)
            {
                return false;
            }
            return true;
        }

        public static bool ReqUseAmmo(Item item) => item.useAmmo > 0;

        // progression based
        // public static bool ReqHardmode(Item item) => Main.hardMode;
        // public static bool ReqPreHardmode(Item item) => !Main.hardMode;

        #endregion

        public static void Amplify_KB(Item item, bool bad, RollStat self, RollStat otherStat)
        {
            // increase by 5% to non static prefix
            if (otherStat.maxValue != 0)
            {
                otherStat.minValue += 5;
                otherStat.maxValue += 7;
            }
        }

        public static void Amplify_Sup(Item item, bool bad, RollStat self, RollStat otherStat)
        {
            // increase by 2x to non static prefix
            if (otherStat.maxValue != 0)
            {
                otherStat.minValue *= 2;
                otherStat.maxValue *= 2;
            }
        }

        public static void Amplify_OnHardmode(Item item, bool bad, RollStat self, RollStat otherStat)
        {
            if (Main.hardMode)
            {
                self.minValue += 6;
                self.maxValue += 6;
            }
        }

        public List<RollStat> goodRollStats;
        public List<RollStat> badRollStats;

        public override void Load()
        {
            // GOOD STATS
            goodRollStats = new List<RollStat>
            {
                // always available
                new RollStat(RareID.None, "damage", 5, 10, ReqLessOnTools),
                new RollStat(RareID.None, "crit", 5, 10, ReqNoSummon),
                new RollStat(RareID.None, "critDamage", 5, 10, ReqNoSummon),
                new RollStat(RareID.None, "speed", 7, 10, ReqNoChannel),
                new RollStat(RareID.None,"moveSpeed",2,6),

                //common defense
                new RollStat(RareID.CommonPrehardmode, "defenseUsage", 1, 3),
                new RollStat(RareID.CommonHardmode, "defenseUsage", 2, 5),
                new RollStat(RareID.CommonPrehardmode, "standingDefense", 2, 5),
                new RollStat(RareID.CommonHardmode, "standingDefense", 4, 8),
                // hardmode common
                new RollStat(RareID.CommonHardmode, "debuff", 8, 20),
                new RollStat(RareID.CommonHardmode, "wet", 8, 15),
                // general common 
                new RollStat(RareID.Common, "standingSpeed", 10, 15,ReqNoChannel) {preventRoll = ["speed"]},
                new RollStat(RareID.Common, "standingDamage", 7, 12,ReqLessOnTools) {preventRoll = ["damage"]},
                new RollStat(RareID.Common, "standingCrit", 8, 15,ReqNoSummon) {preventRoll = ["crit"]},
                // rare
                new RollStat(RareID.RarePrehardmode, "armorPen", 1, 2,ReqNoSummon),
                new RollStat(RareID.CommonHardmode, "armorPen", 1, 4),
                // legend
                new RollStat(RareID.Legend, "trueExecute", 3, 10),
                new RollStat(RareID.Legend, "noAmmo") {requirement = ReqUseAmmo}
            };

            // BAD STATS
            badRollStats = new List<RollStat>
            {
                // always available
                new RollStat(RareID.None, "damage", 6, 9, ReqLessOnTools),
                new RollStat(RareID.None, "crit", 6, 9, ReqNoSummon),
                new RollStat(RareID.None, "critDamage", 6, 9, ReqNoSummon),
                new RollStat(RareID.NonePrehardmode, "speed", 8, 10, ReqNoChannel),
                new RollStat(RareID.NoneHardmode, "speed", 8, 15, ReqNoChannel),
                new RollStat(RareID.None,"moveSpeed",2,6),

                // common
                new RollStat(RareID.Common, "armorPen", 1, 2),

                // legendary
                new RollStat(RareID.Legend, "friendlyFire", Amplify_Sup){requirement = ReqNoMelee},
                new RollStat(RareID.Legend, "nokb", Amplify_KB){requirement = ReqNoSummon}
            };
        }

        public override void Unload()
        {
            goodRollStats = null;
            badRollStats = null;
        }

        internal List<RollStat> GetValidRollStats(Item item, List<RollStat> statList, List<string> preventRoll = null)
        {
            var validRoll = new List<RollStat>();

            if (statList == null)
            {
                Main.NewText("What the fuck");
                return null;
            }

            foreach (var roll in statList)
            {
                if (preventRoll != null && preventRoll.Contains(roll.name)) continue;
                if (roll.rare != 0)
                {
                    if (roll.rare == RareID.NonePrehardmode)
                    {
                        if (Main.hardMode) continue;
                    }
                    else if (roll.rare == RareID.NoneHardmode)
                    {
                        if (!Main.hardMode) continue;
                    }
                    else if (!Main.rand.NextBool((int)roll.rare)) continue;

                    switch (roll.rare)
                    {
                        case RareID.CommonHardmode: if (!Main.hardMode) continue; break;
                        case RareID.CommonPrehardmode: if (Main.hardMode) continue; break;
                        case RareID.RareHarmode: if (!Main.hardMode) continue; break;
                        case RareID.RarePrehardmode: if (Main.hardMode) continue; break;
                        case RareID.LegendHardmode: if (!Main.hardMode) continue; break;
                        case RareID.LegendPreHarmode: if (Main.hardMode) continue; break;
                        default: break;
                    }
                    // if (roll.rare == RareID.CommonHardmode && !Main.hardMode) continue;
                    // if (roll.rare == RareID.CommonPrehardmode && Main.hardMode) continue;

                    // if (roll.rare == RareID.RarePrehardmode && !Main.hardMode) continue;
                    // if (roll.rare == RareID.RareHarmode && Main.hardMode) continue;
                }

                if (roll.requirement != null)
                {
                    if (roll.requirement(item))
                    {
                        validRoll.Add(roll);
                    }
                }
                else
                {
                    validRoll.Add(roll);
                }
            }
            return validRoll;
        }

        public void TryRollStat(Item item, GearItem gearItem)
        {
            if (goodRollStats == null || badRollStats == null)
            {
                if (!Main.gameMenu)
                {
                    Main.NewText("Roll stats failed to load, reloading...");
                }

                Mod.Logger.Error("Roll stats failed to load, reloading...");
                Load();
                return;
            }

            var preventRoll = new List<string>();

            var validGoodRoll = GetValidRollStats(item, goodRollStats);
            if (validGoodRoll == null || validGoodRoll.Count <= 0)
            {
                Main.NewText("No valid good roll stat");
                return;
            }

            var goodRoll = Main.rand.NextFromCollection<RollStat>(validGoodRoll);

            preventRoll.Add(goodRoll.name);

            if (goodRoll.preventRoll != null && goodRoll.preventRoll.Length > 0)
            {
                for (int i = 0; i < goodRoll.preventRoll.Length; i++)
                {
                    preventRoll.Add(goodRoll.preventRoll[i]);
                }
            }

            var validBadRoll = GetValidRollStats(item, badRollStats, preventRoll);
            if (validBadRoll == null || validBadRoll.Count <= 0)
            {
                Main.NewText("No valid bad roll stat");
                return;
            }

            var badRoll = Main.rand.NextFromCollection<RollStat>(validBadRoll);

            if (badRoll.amplifyOther != null)
            {
                badRoll.amplifyOther(item, true, badRoll, goodRoll);
            }

            if (goodRoll.amplifyOther != null)
            {
                goodRoll.amplifyOther(item, false, goodRoll, badRoll);
            }

            gearItem.stats = new Dictionary<string, int>
            {
                { goodRoll.name, Main.rand.Next(goodRoll.minValue, goodRoll.maxValue+1) },
                { badRoll.name, Main.rand.Next(badRoll.minValue, badRoll.maxValue+1)*-1 }
            };
        }
    }
}