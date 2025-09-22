using Terraria.ModLoader;
using Terraria;
using Gearedup.Content.Items;

namespace Gearedup.Commands
{
    public class GivePlayerTeleporter : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "giveplayerteleporter";
        public override string Description => "Give the ribbon fate thingy";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            var i = caller.Player.QuickSpawnItemDirect(caller.Player.GetSource_FromThis("command"), ModContent.ItemType<PlayerTeleporter>());
            var playerName = "";
            foreach (var s in args)
            {
                // add space
                if (playerName != "") playerName += " ";
                playerName += i;
            }
            ((PlayerTeleporter)i.ModItem).playerOwner = playerName;
        }
    }
    
}