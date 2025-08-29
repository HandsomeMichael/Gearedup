using System.IO;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;

namespace Gearedup
{
    public partial class Gearedup
    {
        internal enum MessageType : byte
        {
            GearPlayerSync,
            GearProjectileSync
		}

		// Override this method to handle network packets sent for this mod.
		//TODO: Introduce OOP packets into tML, to avoid this god-class level hardcode.
		public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
			MessageType msgType = (MessageType)reader.ReadByte();

			switch (msgType)
            {
				// Sync the mod player pls
                // case MessageType.GearPlayerSync:

                // 	byte index = reader.ReadByte();
                // 	GearPlayer gp = Main.player[index].GetModPlayer<GearPlayer>();
                // 	gp.ReceivePlayerSync(reader);

                // 	if (Main.netMode == NetmodeID.Server) {
                // 		// Forward the changes to the other clients
                // 		gp.SyncPlayer(-1, whoAmI, false);
                // 	}

                // break;
                // case MessageType.ExampleTeleportToStatue:
                // 	if (Main.npc[reader.ReadByte()].ModNPC is ExamplePerson person && person.NPC.active) {
                // 		person.StatueTeleport();
                // 	}

                // 	break;
                // case MessageType.ExampleDodge:
                // 	ExampleDamageModificationPlayer.HandleExampleDodgeMessage(reader, whoAmI);
                // 	break;
                default:
					Logger.WarnFormat("ExampleMod: Unknown Message type: {0}", msgType);
					break;
			}
		}
    }
}