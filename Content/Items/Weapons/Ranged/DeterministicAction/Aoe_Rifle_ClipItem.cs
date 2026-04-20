using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction.Aoe_Rifle_HeldProj;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction
{
    public class Aoe_Rifle_ClipItem : GlobalItem
    {
        public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return entity.type == ModContent.ItemType<Aoe_Rifle_Item>();
        }

        public List<Aoe_Rifle_Clip> Clips;

        public int TotalBullets
        {
            get
            {
                EnsureInitialized();

                int total = 0;
                for (int i = 0; i < Clips.Count; i++)
                    total += Clips[i].BulletCount;

                return total;
            }
        }

        public void EnsureInitialized()
        {
            if (Clips != null && Clips.Count == Aoe_Rifle_HeldProj.MaxClips)
                return;

            Init();
        }

        public void Init()
        {
            Clips = new List<Aoe_Rifle_Clip>(Aoe_Rifle_HeldProj.MaxClips);
            for (int i = 0; i < Aoe_Rifle_HeldProj.MaxClips; i++)
                Clips.Add(new Aoe_Rifle_Clip(Aoe_Rifle_HeldProj.MAX_CLIP_SIZE));
        }

        public int AssembleClip(Player player, int clipIndex)
        {
            EnsureInitialized();

            if (clipIndex < 0 || clipIndex >= Clips.Count)
                return 0;

            Aoe_Rifle_Clip clip = Clips[clipIndex];
            clip.Bullets.Clear();

            int loaded = 0;

            for (int i = 0; i < Aoe_Rifle_HeldProj.MAX_CLIP_SIZE; i++)
            {
                Item ammo = player.ChooseAmmo(player.HeldItem);
                if (ammo == null || ammo.IsAir)
                    break;

                Item stored = ammo.Clone();
                stored.stack = 1;
                clip.Bullets.Add(stored);

                if (ammo.consumable)
                    player.ConsumeItem(ammo.type);

                loaded++;
            }

            return loaded;
        }

        public override void UpdateInventory(Item item, Player player)
        {
            EnsureInitialized();
        }

        public override void OnCreated(Item item, ItemCreationContext context)
        {
            EnsureInitialized();
        }

        public override GlobalItem Clone(Item from, Item to)
        {
            var clone = (Aoe_Rifle_ClipItem)base.Clone(from, to);
            clone.Init();

            if (Clips == null)
                return clone;

            clone.Clips.Clear();

            foreach (var clip in Clips)
            {
                var newClip = new Aoe_Rifle_Clip(Aoe_Rifle_HeldProj.MAX_CLIP_SIZE);

                if (clip?.Bullets != null)
                {
                    foreach (var bullet in clip.Bullets)
                    {
                        if (bullet != null && !bullet.IsAir)
                            newClip.Bullets.Add(bullet.Clone());
                    }
                }

                clone.Clips.Add(newClip);
            }

            return clone;
        }

        public override void SaveData(Item item, TagCompound tag)
        {
            EnsureInitialized();

            // Save as primitive data so it persists cleanly.
            tag["ClipCount"] = Clips.Count;

            for (int i = 0; i < Clips.Count; i++)
            {
                var clip = Clips[i];
                tag[$"Clip_{i}_BulletCount"] = clip.Bullets.Count;

                for (int j = 0; j < clip.Bullets.Count; j++)
                {
                    Item bullet = clip.Bullets[j];
                    if (bullet == null || bullet.IsAir)
                        continue;

                    tag[$"Clip_{i}_Bullet_{j}_Type"] = bullet.type;
                    tag[$"Clip_{i}_Bullet_{j}_Prefix"] = (int)bullet.prefix;
                    tag[$"Clip_{i}_Bullet_{j}_Stack"] = bullet.stack;
                }
            }
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            Init();

            int clipCount = tag.ContainsKey("ClipCount")
                ? tag.GetInt("ClipCount")
                : Aoe_Rifle_HeldProj.MaxClips;

            Clips.Clear();

            for (int i = 0; i < clipCount; i++)
            {
                var clip = new Aoe_Rifle_Clip(Aoe_Rifle_HeldProj.MAX_CLIP_SIZE);

                int bulletCount = tag.ContainsKey($"Clip_{i}_BulletCount")
                    ? tag.GetInt($"Clip_{i}_BulletCount")
                    : 0;

                for (int j = 0; j < bulletCount; j++)
                {
                    string typeKey = $"Clip_{i}_Bullet_{j}_Type";
                    if (!tag.ContainsKey(typeKey))
                        continue;

                    int bulletType = tag.GetInt(typeKey);
                    int prefix = tag.ContainsKey($"Clip_{i}_Bullet_{j}_Prefix")
                        ? tag.GetInt($"Clip_{i}_Bullet_{j}_Prefix")
                        : 0;
                    int stack = tag.ContainsKey($"Clip_{i}_Bullet_{j}_Stack")
                        ? tag.GetInt($"Clip_{i}_Bullet_{j}_Stack")
                        : 1;

                    Item bullet = new Item();
                    bullet.SetDefaults(bulletType);
                    bullet.stack = stack;
                    if (prefix > 0)
                        bullet.Prefix(prefix);

                    clip.Bullets.Add(bullet);
                }

                Clips.Add(clip);
            }

            // Defensive padding/truncation
            while (Clips.Count < Aoe_Rifle_HeldProj.MaxClips)
                Clips.Add(new Aoe_Rifle_Clip(Aoe_Rifle_HeldProj.MAX_CLIP_SIZE));

            if (Clips.Count > Aoe_Rifle_HeldProj.MaxClips)
                Clips.RemoveRange(Aoe_Rifle_HeldProj.MaxClips, Clips.Count - Aoe_Rifle_HeldProj.MaxClips);
        }

        public override void NetSend(Item item, BinaryWriter writer)
        {
            EnsureInitialized();

            writer.Write((byte)Clips.Count);

            for (int i = 0; i < Clips.Count; i++)
            {
                var clip = Clips[i];
                writer.Write((byte)clip.Bullets.Count);

                for (int j = 0; j < clip.Bullets.Count; j++)
                {
                    Item bullet = clip.Bullets[j];

                    bool valid = bullet != null && !bullet.IsAir;
                    writer.Write(valid);

                    if (!valid)
                        continue;

                    writer.Write(bullet.type);
                    writer.Write((byte)bullet.prefix);
                    writer.Write((short)bullet.stack);
                }
            }
        }

        public override void NetReceive(Item item, BinaryReader reader)
        {
            Init();

            int clipCount = reader.ReadByte();
            Clips.Clear();

            for (int i = 0; i < clipCount; i++)
            {
                var clip = new Aoe_Rifle_Clip(Aoe_Rifle_HeldProj.MAX_CLIP_SIZE);

                int bulletCount = reader.ReadByte();

                for (int j = 0; j < bulletCount; j++)
                {
                    bool valid = reader.ReadBoolean();
                    if (!valid)
                        continue;

                    int bulletType = reader.ReadInt32();
                    byte prefix = reader.ReadByte();
                    short stack = reader.ReadInt16();

                    Item bullet = new Item();
                    bullet.SetDefaults(bulletType);
                    bullet.stack = stack;
                    if (prefix > 0)
                        bullet.Prefix(prefix);

                    clip.Bullets.Add(bullet);
                }

                Clips.Add(clip);
            }

            while (Clips.Count < Aoe_Rifle_HeldProj.MaxClips)
                Clips.Add(new Aoe_Rifle_Clip(Aoe_Rifle_HeldProj.MAX_CLIP_SIZE));

            if (Clips.Count > Aoe_Rifle_HeldProj.MaxClips)
                Clips.RemoveRange(Aoe_Rifle_HeldProj.MaxClips, Clips.Count - Aoe_Rifle_HeldProj.MaxClips);
        }
    }
}