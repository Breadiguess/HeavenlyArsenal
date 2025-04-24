using System.Collections.Generic;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace HeavenlyArsenal.Content.Tiles.Generic;

public abstract class WorldOrientedTileObjectManager<TTileObject> : ModSystem where TTileObject : WorldOrientedTileObject, new()
{
    /// <summary>
    /// The set of tile objects maintained by this manager.
    /// </summary>
    public List<TTileObject> TileObjects
    {
        get;
        protected set;
    } = new List<TTileObject>(32);

    /// <summary>
    /// Registers a new tile object into the set maintained by the world.
    /// </summary>
    public virtual void Register(TTileObject tileObject) => TileObjects.Add(tileObject);

    /// <summary>
    /// Removes a given existing tile object from the set maintained by the world.
    /// </summary>
    public void Remove(TTileObject tileObject) => TileObjects.Remove(tileObject);

    public override void SaveWorldData(TagCompound tag)
    {
        tag["ObjectCount"] = TileObjects.Count;

        TagCompound objectsTag = new TagCompound();
        for (int i = 0; i < TileObjects.Count; i++)
            objectsTag.Add($"Object{i}", TileObjects[i].Serialize());

        tag["Objects"] = objectsTag;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        TileObjects.Clear();

        if (!tag.TryGet("ObjectCount", out int objectCount) || !tag.TryGet("Objects", out TagCompound objectsTag))
            return;

        TTileObject scuffed = new();
        for (int i = 0; i < objectCount; i++)
            TileObjects.Add((TTileObject)scuffed.Deserialize(objectsTag.GetCompound($"Object{i}")));
    }

    public sealed override void PostUpdatePlayers()
    {
        Parallel.For(0, TileObjects.Count, i =>
        {
            TTileObject tileObject = TileObjects[i];
            tileObject.Update();
        });
    }
}
