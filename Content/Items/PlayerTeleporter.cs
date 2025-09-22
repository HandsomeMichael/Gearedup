using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Gearedup.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;

namespace Gearedup.Content.Items
{
    public class PlayerTeleporter : ModItem
    {
        public string playerOwner = "";

        public override ModItem Clone(Item newEntity)
        {
            PlayerTeleporter obj = (PlayerTeleporter)base.Clone(newEntity);
            obj.playerOwner = playerOwner;
            return obj;
        }

        public override void SetDefaults()
        {
            Item.useAnimation = 2;
            Item.useTime = 2;
            Item.width = 10;
            Item.height = 10;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.rare = ItemRarityID.Blue;
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
        public override bool CanUseItem(Player player) => !player.HasBuff(BuffID.ChaosState);
        public override bool ConsumeItem(Player player) => false;
        public override bool CanRightClick() => !Main.LocalPlayer.HasBuff(BuffID.ChaosState);
        public override void RightClick(Player player)
        {
            if (CanUseItem(player)) DoAltUse(player);
        }

        public void DoAltUse(Player player)
        {
            if (playerOwner == null || playerOwner == "")
            {
                playerOwner = player.name;
                CombatText.NewText(player.Hitbox, Color.LightBlue, "Assigned to " + player.name);
            }
            else
            {
                // tp
                if (player.name != playerOwner)
                {
                    if (!Teleport(player, true))
                    {
                        CombatText.NewText(player.Hitbox, Color.LightPink, $"No player with the name '{playerOwner}'");
                    }
                }
                else
                {
                    CombatText.NewText(player.Hitbox, Color.LightPink, "Cant teleport to yourself dummy");
                }
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (playerOwner != null && playerOwner != "")
            {
                tooltips.Add(new TooltipLine(Mod, "tp", $"Use to teleport [c/FF0000:{playerOwner}] to you\nRight-click to teleport to [c/FF0000:{playerOwner}]\nRequire valid space to teleport"));
                
            }
            else
            {
                // if somehow the name is invalid
                tooltips.Add(new TooltipLine(Mod, "tp2", "Used to teleport assigned player to you or to them\nAutomatically assign to self when crafted\nRight-click to assign self "));
            }
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                DoAltUse(player);
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
                    if (!Teleport(player, false))
                    {
                        CombatText.NewText(player.Hitbox, Color.LightPink, $"No player with the name '{playerOwner}'");
                    }
                }
                else
                {
                    CombatText.NewText(player.Hitbox, Color.LightPink, "Cant teleport yourself dummy");
                }
            }
            return true;
        }

        public bool Teleport(Player player, bool reversed)
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                var target = Main.player[i];
                if (target.active && !target.dead && target.name == playerOwner)
                {
                    // actually tp the player
                    if (reversed)
                    {
                        player.Teleport(target.position, TeleportationStyleID.TeleporterTile);
                        NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, player.whoAmI, target.position.X, target.position.Y, TeleportationStyleID.TeleporterTile);
                        CombatText.NewText(target.Hitbox, Color.LightGreen, playerOwner + " teleported");

                        SoundEngine.PlaySound(new SoundStyle("Gearedup/Sounds/jonathanbanging"), target.Center);
                    }
                    else
                    {
                        target.Teleport(player.position, TeleportationStyleID.TeleporterTile);
                        NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, target.whoAmI, player.position.X, player.position.Y, TeleportationStyleID.TeleporterTile);
                        CombatText.NewText(player.Hitbox, Color.LightGreen, playerOwner + " teleported");

                        SoundEngine.PlaySound(new SoundStyle("Gearedup/Sounds/jonathanbanging"), player.Center);
                    }

                    player.AddBuff(BuffID.ChaosState, Main.CurrentFrameFlags.AnyActiveBossNPC ? 300 : 120);

                    return true;
                }
            }
            return false;
        }

        public override void PostDrawTooltip(ReadOnlyCollection<DrawableTooltipLine> lines)
        {
            if (playerOwner == null || playerOwner == "") return;

            var lastLine = lines[lines.Count - 1];
            Vector2 pos = new Vector2(lastLine.X, lastLine.Y + lastLine.Font.MeasureString(lastLine.Text).Y + 24);

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                var target = Main.player[i];
                if (target.active && !target.dead && target.name == playerOwner)
                {
                    //EndlessLoader.DrawUnloaded(Main.spriteBatch, Main.MouseScreen);

                    //var panelT = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/Separator1").Value;

                    string text = $"[i:{ItemID.Heart}] {target.statLife} / {target.statLifeMax2}" +
                    $"\n  {Math.Round(target.Distance(Main.MouseWorld) / 100f)}ft away";

                    // for some weird reason, you cant draw on top of ts. thats a shame dawg
                    Utils.DrawInvBG(Main.spriteBatch, pos.X - 16, pos.Y - 12, text.MeasureString(FontAssets.MouseText.Value).X + target.width + 42, target.height + 32, Color.DarkBlue * 0.7f);

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                    Main.PlayerRenderer.DrawPlayer(Main.Camera, target, pos + Main.screenPosition, target.fullRotation, target.fullRotationOrigin);

                    Main.spriteBatch.BeginNormal(true, true);

                    ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, text,
                    pos + new Vector2(target.width + 12, 0), Color.White, 0f, Vector2.One, Vector2.One);
                    return;
                }
            }

            string textNotFound = "No player found";
            Utils.DrawInvBG(Main.spriteBatch, pos.X - 16, pos.Y - 12, textNotFound.MeasureString(FontAssets.MouseText.Value).X + 60, 40, Color.DarkBlue * 0.7f);
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, textNotFound, pos + new Vector2(12, 0), Color.White, 0f, Vector2.One, Vector2.One);
        }

        // Somehow doesnt work

        // public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        // {
        //     const int playerWidth = 40;
        //     if (playerOwner == null || playerOwner == "") return true;
        //     if (line.Name == "tp" || line.Name == "tp2") { line.X += playerWidth; }
        //     return true;
        // }

        // public override void PostDrawTooltip(ReadOnlyCollection<DrawableTooltipLine> lines)
        // {
        //     if (playerOwner == null || playerOwner == "") return;

        //     Vector2 pos = new Vector2(lines[1].X, lines[1].Y);
        //     // Vector2 pos = new Vector2(line.X, line.Y);
        //     for (int i = 0; i < Main.maxPlayers; i++)
        //     {
        //         var target = Main.player[i];
        //         if (target.active && !target.dead && target.name == playerOwner)
        //         {
        //             //EndlessLoader.DrawUnloaded(Main.spriteBatch, Main.MouseScreen);
        //             Main.PlayerRenderer.DrawPlayer(Main.Camera, (Player)target.Clone(), pos + Main.screenPosition, 0f, Vector2.One);
        //         }
        //     }
        // }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (playerOwner == null || playerOwner == "")
            {
                var asset = ModContent.Request<Texture2D>(Texture).Value;
                spriteBatch.Draw(asset, position, frame, Color.Gray, 0f, origin, scale, SpriteEffects.None, 0f);
                return false;
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.WhiteString)
                .AddIngredient(ItemID.Silk, 10)
                .AddIngredient(ItemID.FallenStar, 5)
                .AddIngredient(ItemID.WormholePotion, 30)
                .AddTile(TileID.Anvils)
                .DisableDecraft()
                .AddOnCraftCallback(ApplyCraft)
                .Register();
        }

        private void ApplyCraft(Recipe recipe, Item item, List<Item> consumedItems, Item destinationStack)
        {
            if (item.ModItem is PlayerTeleporter pt)
            {
                // should i sync ?
                pt.playerOwner = Main.LocalPlayer.name;
                // destinationStack.NetStateChanged();
            }
        }
    }
}