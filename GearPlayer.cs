using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Gearedup
{
    public class GearPlayer : ModPlayer
    {
        public short universalDye;
        public bool ogreFeet;
        public bool phasingDevice;

        public List<Vector2> platform = new List<Vector2>();

        public override void ResetEffects()
        {
            ogreFeet = false;
            phasingDevice = false;
            universalDye = 0;
        }

        public override void PostUpdateMiscEffects()
        {
            if (phasingDevice && Player.holdDownCardinalTimer[0] > 0)
            {
                // safe code
                if (platform == null) { platform = new List<Vector2>(); }

                // credit to fargowiltas mod for this code
                int width = 2;
                if (Player.mount.Active)
                {
                    // offset by 2
                    width = ((int)Player.mount._data.textureWidth / 16);
                }
                for (int i = -width; i <= width; i++)
                {
                    Vector2 pos = Player.Center;
                    pos.X += width * 16;
                    pos.Y += Player.height / 2;
                    pos.Y += 8;

                    // foreach (var item in offset) { pos += item(Player, i); }

                    Tile tile = Framing.GetTileSafely((int)(pos.X / 16), (int)(pos.Y / 16));
                    if (!tile.IsActuated && IsPlatform(tile.TileType))
                    {
                        Main.NewText("Actuated");
                        tile.IsActuated = true;
                        platform.Add(pos);
                    }
                }

                Player.waterWalk = false;
                Player.waterWalk2 = false;

                // Legacy way

                // Tile thisTile = Framing.GetTileSafely(Player.Bottom);
                // Tile bottomTile = Framing.GetTileSafely(Player.Bottom + Vector2.UnitY * 8);

                // if (!Collision.SolidCollision(Player.BottomLeft, Player.width, 16))
                // {
                //     if (Player.velocity.Y >= 0 && (IsPlatform(thisTile.TileType) || IsPlatform(bottomTile.TileType)))
                //     {
                //         // i added +1 just incase idk
                //         Player.position.Y += 3;
                //     }
                //     if (Player.velocity.Y == 0)
                //     {
                //         Player.position.Y += 16;
                //     }

                // }

                static bool IsPlatform(int tileType)
                {
                    return TileID.Sets.Platforms[tileType] || tileType == TileID.Platforms || tileType == TileID.PlanterBox;
                }
            }
        }

        public override void PostUpdate()
        {
            foreach (var pos in platform) {
                var tile = Framing.GetTileSafely((int)(pos.X / 16), (int)(pos.Y / 16));
                tile.IsActuated = false;
            }
            platform.Clear();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < Player.buffTime.Length; i++)
            {
                if (Player.buffTime[i] > 0)
                {
                    target.AddBuff(Player.buffType[i], Player.buffTime[i] * 2);
                }
            }
        }

        public override void PreUpdateMovement()
        {
            if (ogreFeet && Player.immuneTime <= 0)
            {
                Player.velocity.X *= 0.8f;
            }
        }
    }
}