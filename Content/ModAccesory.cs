using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Gearedup.Content.Items
{
    public abstract class ModAccesory : ModItem
    {
        public byte accesoryID = 0;

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 38;
            Item.value = 1000;
            Item.consumable = false;
            // Item.rare = ItemRarity;

        }
    }
}