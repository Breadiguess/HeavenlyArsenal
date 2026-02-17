using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
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
                int x = 0;
                for(int i = 0; i< MaxClips; i++)
                {
                    x += Clips[i].BulletCount;
                }
                return x;
            }
        }
        public int AssembleClip(Player player, int clipIndex)
        {
            if (Clips == null || clipIndex < 0 || clipIndex >= Clips.Count)
                return 0;

            Aoe_Rifle_Clip clip = Clips[clipIndex];
            clip.Bullets.Clear();

            int loaded = 0;

            for (int i = 0; i < Aoe_Rifle_HeldProj.MAX_CLIP_SIZE; i++)
            {
                Item ammo = player.ChooseAmmo(player.HeldItem);
                if (ammo == null)
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
            if(Clips == null)
            {
                Init();
            }
        }
        public override void OnCreated(Item item, ItemCreationContext context)
        {
            if (item.ModItem is Aoe_Rifle_Item)
                Init();
        }

        public override GlobalItem Clone(Item from, Item to)
        {
            var clone = (Aoe_Rifle_ClipItem)base.Clone(from, to);
            clone.Clips = new List<Aoe_Rifle_Clip>();

            foreach (var clip in Clips)
            {
                var newClip = new Aoe_Rifle_Clip(Aoe_Rifle_HeldProj.MAX_CLIP_SIZE);
                newClip.Bullets.AddRange(clip.Bullets.Select(b => b.Clone()));
                clone.Clips.Add(newClip);
            }

            return clone;
        }

        public void Init()
        {
            Clips = new List<Aoe_Rifle_Clip>(Aoe_Rifle_HeldProj.MaxClips);
            for (int i = 0; i < Aoe_Rifle_HeldProj.MaxClips; i++)
                Clips.Add(new Aoe_Rifle_Clip(Aoe_Rifle_HeldProj.MAX_CLIP_SIZE));
        }
    }

}
