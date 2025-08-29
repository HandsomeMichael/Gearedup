
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Gearedup.Content.Perks
{

    /// <summary>
    /// Represents the class of a gear perk, which determines what type of gear it can be applied to.
    /// </summary>
    public static class GearReqID
    {
        public const byte None = 0;
        public const byte Melee = 1;
        public const byte Ranged = 2;
        public const byte Magic = 3;
        public const byte Summon = 4;
        public const byte Bow = 5;
        public const byte Gun = 6;
        public const byte Rocket = 7;
        public const byte Axe = 8;
        public const byte Pickaxe = 9;
        public const byte Hammer = 10;
        public const byte Dartgun = 11;
        public const byte Yoyo = 12;
    }

    public class GearPerk : ModItem
    {

        public static void SpawnExample()
        {
            var item = Create(new Dictionary<string, int>{
                { "damage", 5 },{ "speed", 5 },{ "crit", -20 }
            }, "Buffalo Perk", "Thats fucked up");
        }
        
        public static Item Create(Dictionary<string, int> stats,
        string name = "Gear Perk",
        string context = "A basic gear perk",
         byte gearReq = 0)
        {
            Item item = new Item();
            item.SetDefaults(ModContent.ItemType<GearPerk>());
            var perk = item.ModItem as GearPerk;
            perk.stats = stats;
            perk.name = name;
            perk.context = context;
            perk.gearReq = gearReq;
            item.SetNameOverride(name);
            return item;
        }
        public override string Texture => "Gearedup/Content/Perks/PerkSprite";
        protected override bool CloneNewInstances => true;

        public Dictionary<string, int> stats;
        public string name;
        public string context;
        public byte gearReq;

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.rare = ItemRarityID.Blue;
        }

        public override void RightClick(Player player)
        {
            
        }

        public override void SaveData(TagCompound tag)
        {
            tag.Add("name", name);
            tag.Add("context", context);
            tag.Add("gearReq", gearReq);

            tag["statNames"] = stats.Keys.ToList();
            tag["statValues"] = stats.Values.ToList();
        }

        public override void LoadData(TagCompound tag)
        {
            name = tag.GetString("name");
            context = tag.GetString("context");
            gearReq = tag.GetByte("gearReq");

            var names = tag.Get<List<string>>("statNames");
            var values = tag.Get<List<int>>("statValues");
            stats = names.Zip(values, (k, v) => new { Key = k, Value = v }).ToDictionary(x => x.Key, x => x.Value);
        }

    }
}