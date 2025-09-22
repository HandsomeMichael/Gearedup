// Unused

using System;
using System.Collections.Generic;
using Gearedup.Content.Items;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;

namespace Gearedup
{
	public class GearDrops : ModSystem
	{
        public override void Load()
        {
			On_NPC.NPCLoot_DropItems += DropItemPatch;
        }

        private void DropItemPatch(On_NPC.orig_NPCLoot_DropItems orig, NPC self, Player closestPlayer)
        {
			// DropAttemptInfo dropAttemptInfo = default(DropAttemptInfo);
			// dropAttemptInfo.player = closestPlayer;
			// dropAttemptInfo.npc = self;
			// dropAttemptInfo.IsExpertMode = Main.expertMode;
			// dropAttemptInfo.IsMasterMode = Main.masterMode;
			// dropAttemptInfo.IsInSimulation = false;
			// dropAttemptInfo.rng = Main.rand;
			// DropAttemptInfo info = dropAttemptInfo;
			// Main.ItemDropSolver.TryDropping(info);

			if (GearServerConfig.Get.Content_DeityStaff && BrainWashedNPC.CanBeBrainwashed(self))
            {
                if (self.TryGetGlobalNPC(out BrainWashedNPC br))
                {
                    if (br.ownedBy != -1)
                    {
                        // dont call orig, dont drop nothin
                        return;
                    }
                }
            }

			if (self.TryGetGlobalNPC(out GearNPCs gnpcs))
			{
				if (gnpcs.doubleLoot)
				{
					// call twice
					orig(self, closestPlayer);
				}
			}

			orig(self, closestPlayer);
        }
    }
	public class IsMoonLord : IItemDropRuleCondition, IProvideItemConditionDescription
	{
		public bool CanDrop(DropAttemptInfo info)
		{
			return NPC.downedMoonlord;
		}

		public bool CanShowItemDropInUI()
		{
			return true;
		}

		public string GetConditionDescription()
		{
			return "Drop if moonlord is defeated";
		}
	}

	public class DropLuckBasedStack : IItemDropRule
	{
		public int itemId;
		public int amountDroppedMinimum;
		public int amountExtraDroppedMinimum;
		public float luckMultiplier;

		public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }

		public DropLuckBasedStack(int itemId, int amountDroppedMinimum = 1, int amountExtraDroppedMinimum = 3,float luckMultiplier = 1f)
		{
			// if (amountDroppedMinimum > amountDroppedMaximum) {
			// 	throw new ArgumentOutOfRangeException(nameof(amountDroppedMinimum), $"{nameof(amountDroppedMinimum)} must be lesser or equal to {nameof(amountDroppedMaximum)}.");
			// }

			this.itemId = itemId;
			this.amountDroppedMinimum = amountDroppedMinimum;
			this.amountExtraDroppedMinimum = amountExtraDroppedMinimum;
			this.luckMultiplier = luckMultiplier;
			ChainedRules = new List<IItemDropRuleChainAttempt>();
		}

		public virtual bool CanDrop(DropAttemptInfo info) => true;

		public virtual ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
		{
			ItemDropAttemptResult result;
			int extraDrop = 0;
			if (info.player.luck > 0f)
			{
				extraDrop = (int)((float)amountDroppedMinimum * info.player.luck * luckMultiplier) + amountExtraDroppedMinimum; // atleast 1 extra drop
			}
			CommonCode.DropItem(info, itemId, amountDroppedMinimum + extraDrop);
			result = default(ItemDropAttemptResult);
			result.State = ItemDropAttemptResultState.Success;
			return result;
		}

		public virtual void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
		{
			// always drop
			float num = 1f;  //(float)chanceNumerator / (float)chanceDenominator;
			float dropRate = 1f * ratesInfo.parentDroprateChance;
			drops.Add(new DropRateInfo(itemId, amountDroppedMinimum + amountExtraDroppedMinimum, (int)((float)amountDroppedMinimum*luckMultiplier) + amountExtraDroppedMinimum, dropRate, ratesInfo.conditions));
			Chains.ReportDroprates(ChainedRules, num, drops, ratesInfo);
		}
	}

}