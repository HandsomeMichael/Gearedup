using Terraria.ModLoader;
using Terraria;

namespace Gearedup.Commands
{
    public class DisposeRenderCommand : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "renderdispose";
        public override string Description => "Dispose render to free up memories.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            Main.NewText("Disposing render for client " + caller.Player.name);
            if (!Main.dedServ)
            {
                RenderManager.Get.DisposeAll();
                Main.NewText("Render Dispose Succesfull For U");
            }
        }
    }
    
    public class NoDust : ModCommand
    {
        public override CommandType Type => CommandType.Chat;
        public override string Command => "nodust";
        public override string Description => "Clear all dust";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            for (int i = 0; i < Main.maxDust; i++)
            {
                Main.dust[i].active = false;
            }
            Main.NewText("Cleared all dust");
        }
    }
}