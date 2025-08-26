using Terraria;
using Terraria.GameContent.ItemDropRules;

namespace Gearedup
{
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
}