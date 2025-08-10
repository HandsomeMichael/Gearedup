using System.Collections.Generic;
using System.IO;
using Gearedup.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Gearedup.Content.Items
{
    public class PhasingDevices : ToggleItem
    {
        public override int ItemRarity => ItemRarityID.Blue;
        public override void UpdateEnable(GearPlayer gearPlayer) => gearPlayer.phasingDevice = true;
    }
}