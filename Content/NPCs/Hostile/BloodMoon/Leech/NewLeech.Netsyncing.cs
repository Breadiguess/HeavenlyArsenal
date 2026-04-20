using HeavenlyArsenal.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Terraria.GameContent.Animations.IL_Actions.NPCs;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech
{
    public partial class newLeech
    {
        public override void SendExtraAI2(BinaryWriter writer)
        {
            writer.Write(variant);
            writer.Write(SegmentCount);
            writer.Write(hasUsedEmergency);
        }

        public override void ReceiveExtraAI2(BinaryReader reader)
        {
            variant = reader.ReadInt32();
            SegmentCount = reader.ReadInt32();
            hasUsedEmergency = reader.ReadBoolean();

            EnsureSegmentDataExists();
        }
        private void EnsureSegmentDataExists()
        {
            if (SegmentCount <= 0)
                return;

            if (AdjHitboxes == null || AdjHitboxes.Length != SegmentCount)
            {
                AdjHitboxes = new Rectangle[SegmentCount];

                for (int i = 0; i < AdjHitboxes.Length; i++)
                {
                    AdjHitboxes[i].Width = 30;
                    AdjHitboxes[i].Height = 30;
                    AdjHitboxes[i].Location = NPC.Center.ToPoint();
                }
            }

            if (_ExtraHitBoxes == null || _ExtraHitBoxes.Count != SegmentCount)
            {
                _ExtraHitBoxes = new List<ExtraNPCSegment>(SegmentCount);
                for (int i = 0; i < SegmentCount; i++)
                    _ExtraHitBoxes.Add(new ExtraNPCSegment(AdjHitboxes[i], uniqueIframes: true));
            }

            Tail.Clear();

            int tailSegmentNum = (int)Utils.Remap(SegmentCount, 6, 16, 8, 34);
            int reducedNum = (int)(tailSegmentNum / 1.4f);

            for (int i = 0; i < 4; i++)
            {
                if (i < 2)
                    Tail[i] = (new Vector2[tailSegmentNum], new Vector2[tailSegmentNum]);
                else
                    Tail[i] = (new Vector2[reducedNum], new Vector2[reducedNum]);
            }
        }
    }
}
