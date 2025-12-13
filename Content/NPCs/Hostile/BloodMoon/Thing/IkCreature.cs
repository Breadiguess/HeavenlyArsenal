using HeavenlyArsenal.Core.Systems;
using Luminance.Assets;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Thing
{
    partial class IkCreature : ModNPC
    {
        public List<IKCreatureLeg> Legs;
        public override string Texture => MiscTexturesRegistry.PixelPath;
        public const int LimbCount = 4;
        void InitializeLegs()
        {
            Legs = new List<IKCreatureLeg>(4);
            for (int i = 0; i < 4; i++)
            {
                float baseRotation = MathHelper.Lerp(MathHelper.PiOver4 * 1.5f, -MathHelper.PiOver4 * 1.5f, i / 3f) + MathHelper.PiOver2;
                IKCreatureLeg leg = new IKCreatureLeg(NPC, i, default);// new CrabulonLeg(this, i < 1 || i > 2, i < 2, baseRotation, LegSkins[chosenSkinIndices[i + 1]]);
                Legs.Add(leg);
            }

            for (int i = 0; i < 4; i++)
            {
                int set = i < 2 ? 0 : 2;
                int otherSisterOffset = i % 2 == 0 ? 1 : 0;
                int pairedleg = i == 3 ? 0 : (i == 0 ? 3 : (i == 1 ? 2 : 1));

                //Legs[i].pairedLeg = Legs[pairedleg];
                Legs[i].Sister = Legs[set + otherSisterOffset];
            }
        }

        public override void OnSpawn(IEntitySource source)
        {
            InitializeLegs();
        }
        public override void SetDefaults()
        {
            NPC.lifeMax = 40;
            NPC.defense = 9999;
            NPC.Size = new Vector2(40, 30);
        }
        public override void AI()
        {
            NPC.velocity.X = NPC.DirectionTo(Main.MouseWorld).X;
        }
        public override void PostAI()
        {
            for (int i = 0; i < Legs.Count; i++)
            {
                var leg = Legs[i];
                leg.UpdateLimbState(ref leg, NPC.Center, 0.1f, 30f, leg.Index);
            }
        }

        #region IK leg shit
        Vector2 FindNewGrabPoint(Vector2 basePos, int index)
        {

            float maxDist = Legs[index].Skeleton._maxDistance * 0.65f;

            float sideOffset = Math.Clamp(NPC.velocity.X * 40f, -100, 100);
            //Main.NewText(sideOffset);
            Point? hit = LineAlgorithm.RaycastTo(
                NPC.Top,
                basePos + new Vector2(sideOffset * 1.25f, 115)//.RotatedBy((NPC.rotation + MathHelper.PiOver2)*0.5f)
            );

            if (!hit.HasValue)
                return FindNewGrabPoint(basePos + new Vector2(Main.rand.Next(-1, 1), 0), index); // fallback



            Vector2 world = hit.Value.ToWorldCoordinates();



            //Main.NewText(basePos - world);
            return world;
        }
        #endregion
    }
}
