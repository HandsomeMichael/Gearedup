
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Gearedup.Content.Perks
{
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