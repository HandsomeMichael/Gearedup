using System.IO;
using Gearedup.Helper;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Gearedup.Content.Items
{
    public class PlayerTeleporter : ModItem
    {
        public string playerOwner;

        public override ModItem Clone(Item newEntity)
        {
            PlayerTeleporter obj = (PlayerTeleporter)base.Clone(newEntity);
            obj.playerOwner = playerOwner;
            return obj;
        }

        public override void SetDefaults()
        {
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.width = 10;
            Item.height = 10;
            Item.useStyle = ItemUseStyleID.HoldUp;
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(playerOwner);
        }

        public override void NetReceive(BinaryReader reader)
        {
            playerOwner = reader.ReadString();
        }

        public override void SaveData(TagCompound tag)
        {
            tag.Add("playerOwner", playerOwner);
        }

        public override void LoadData(TagCompound tag)
        {
            playerOwner = tag.GetString("playerOwner");
        }

        public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player) => true;

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                playerOwner = player.name;
                CombatText.NewText(player.Hitbox, Color.LightBlue, "Assigned to "+player.name);
            }
            else
            {
                // no owner
                if (playerOwner == null || playerOwner == "")
                {
                    CombatText.NewText(player.Hitbox, Color.LightPink, "No player assigned");
                    return true;
                }

                // tp
                if (player.name != playerOwner)
                {
                    for (int i = 0; i < Main.maxPlayers; i++)
                    {
                        var target = Main.player[i];
                        if (target.active && !target.dead && target.name == playerOwner)
                        {
                            // actually tp the player
                            target.Teleport(player.position,TeleportationStyleID.TeleportationPotion);
                            NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, target.whoAmI, player.position.X, player.position.Y, TeleportationStyleID.TeleportationPotion);
                            CombatText.NewText(player.Hitbox, Color.LightGreen, playerOwner+" teleported");
                            return true;
                        }
                    }

                    CombatText.NewText(player.Hitbox, Color.LightGreen, $"No player with the name '{playerOwner}'");
                }
                else
                {
                    CombatText.NewText(player.Hitbox, Color.LightPink, "Cant teleport to self dummy");
                }
            }
            return true;
        }
    }
}