namespace AAEmu.Game.Models.Game.World
{
    public class Spawner<T> where T : GameObject
    {
        public uint Id { get; set; }     // id соответствует ObjId на сервере, id из файла npc_spawns.json
        public uint ObjIdAtAAFree { get; set; }  // objId из файла npc_spawns.json для работы движения NPC и соответствует ObjId на сервере AAFree
        public uint UnitId { get; set; } // unitId соответствует templateId, unitId из файла npc_spawns.json
        public Point Position { get; set; }
        public int RespawnTime { get; set; } = 15;
        public int DespawnTime { get; set; } = 20;

        public virtual T Spawn(uint objId)
        {
            return null;
        }

        public virtual T Spawn(uint objId, ulong itemID, uint charID)
        {
            return null;
        }

        public virtual void Respawn(T obj)
        {
            Spawn(0);
        }

        public virtual void Despawn(T obj)
        {
        }
    }
}
