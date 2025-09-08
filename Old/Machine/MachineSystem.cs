using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Gearedup.Content.Machine
{
    public class MachineSystem : ModSystem
    {
        public List<MachineBehaviour> machineBehaviours;
        public List<MachineEntity> machines;

        public override void NetSend(BinaryWriter writer)
        {
            base.NetSend(writer);
        }

        public override void NetReceive(BinaryReader reader)
        {
            base.NetReceive(reader);
        }

        public override void SaveWorldData(TagCompound tag)
        {
            base.SaveWorldData(tag);
        }

        public override void LoadWorldData(TagCompound tag)
        {
            base.LoadWorldData(tag);
        }

        public override void PostDrawTiles()
        {
            base.PostDrawTiles();
        }

        public override void PostUpdateWorld()
        {
            // only updates on server or singleplayer
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            foreach (MachineEntity item in machines)
            {
                machineBehaviours[item.behaviourMap].Update(item);
            }
        }
    }
    public struct MachineEntity
    {
        public Vector2 position;
        public int width;
        public int height;
        public int behaviourMap;
        public bool requireSync;
    }

    public abstract class MachineBehaviour
    {
        public int index;
        public virtual void Update(MachineEntity entity)
        {
            entity.position.X += 1;
        }
    }
}