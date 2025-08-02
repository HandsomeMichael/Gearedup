using System;
using System.IO;
using Microsoft.Build.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Gearedup.Helper
{

    /// <summary>
    /// Used to load any ids, good for potential unloaded items breaking the whole mod
    /// </summary>
    public struct TypeID
    {

        /// <summary>
        /// Temporary IDs , will not be saved or synced
        /// </summary>
        public int? id = null;
        public string mod;
        public string name;

        public bool IsVanilla => mod == "Terraria";

        public TypeID(Item item)
        {
            SetTo(item);
        }

        public TypeID(NPC npc)
        {
            SetTo(npc);
        }

        public TypeID(Projectile projectile)
        {
            SetTo(projectile);
        }

        public void SetTo(NPC npc, bool validate = true)
        {
            if (npc.ModNPC != null)
            {
                mod = npc.ModNPC.Mod.Name;
                name = npc.ModNPC.Name;
            }
            else
            {
                mod = "Terraria";
                name = npc.type.ToString();
            }

            if (validate) ValidateAsNPC();
        }

        public void SetTo(Projectile projectile, bool validate = true)
        {
            if (projectile.ModProjectile != null)
            {
                mod = projectile.ModProjectile.Mod.Name;
                name = projectile.ModProjectile.Name;
            }
            else
            {
                mod = "Terraria";
                name = projectile.type.ToString();
            }

            if (validate) ValidateAsProjectile();
        }

        public void SetTo(Item item, bool validate = true)
        {
            if (item.ModItem != null)
            {
                mod = item.ModItem.Mod.Name;
                name = item.ModItem.Name;
            }
            else
            {
                mod = "Terraria";
                name = item.type.ToString();
            }

            if (validate) ValidateAsItem();
        }

        public bool ValidateAsItem()
        {
            id = null;
            if (mod == null || name == null || mod == "" || name == "")
            {
                return false;
            }

            if (IsVanilla)
            {
                id = int.Parse(name);
                return true;
            }

            if (ModContent.TryFind<ModItem>(mod, name, out ModItem modEntity) && modEntity != null)
            {
                id = modEntity.Type;
                return true;
            }

            ModContent.GetInstance<Gearedup>().AddError($"Failed to find mod type : {mod}/{name}");

            // We do this our own
            // ModContent.GetInstance<Gearedup>().Logger.Error($"Failed to find mod type: {mod}.{name}");
            return false;
        }

        public bool ValidateAsNPC()
        {
            id = null;
            if (mod == null || name == null || mod == "" || name == "")
            {
                return false;
            }

            if (IsVanilla)
            {
                id = int.Parse(name);
                return true;
            }

            if (ModContent.TryFind<ModNPC>(mod, name, out ModNPC modEntity) && modEntity != null)
            {
                id = modEntity.Type;
                return true;
            }

            ModContent.GetInstance<Gearedup>().AddError($"Failed to find mod type : {mod}/{name}");
            return false;
        }

        public bool ValidateAsProjectile()
        {
            id = null;
            if (mod == null || name == null || mod == "" || name == "")
            {
                return false;
            }

            if (IsVanilla)
            {
                id = int.Parse(name);
                return true;
            }

            if (ModContent.TryFind<ModProjectile>(mod, name, out ModProjectile modEntity) && modEntity != null)
            {
                id = modEntity.Type;
                return true;
            }
            
            ModContent.GetInstance<Gearedup>().AddError($"Failed to find mod type : {mod}/{name}");
            return false;
        }

        public void NetSend(BinaryWriter writer)
        {
            writer.Write(mod);
            writer.Write(name);
        }

        public void NetReceive(BinaryReader reader)
        {
            mod = reader.ReadString();
            name = reader.ReadString();
        }

        public void Save(TagCompound tag)
        {
            tag.Add("mod", mod);
            tag.Add("name", name);
        }

        public void Load(TagCompound tag)
        {
            mod = tag.GetString("mod");
            name = tag.GetString("name");
        }
    }
}