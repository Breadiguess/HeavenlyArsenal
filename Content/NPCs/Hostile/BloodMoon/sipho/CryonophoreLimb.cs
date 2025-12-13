using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.sipho
{
    partial class CryonophoreLimb : BloodMoonBaseNPC
    {
        private Vector2[] LimbSegmentPos;
        private Vector2[] LimbSegmentVels;
        public override bool canBeSacrificed => false;
        public override bool canBebuffed => false;

        public NPC Owner
        {
            get => Main.npc[OwnerIndex] != null ? Main.npc[OwnerIndex] : default;
        }

        public int OwnerIndex;
        public CryonophoreZooid self;
        public override void SetDefaults()
        {
            NPC.lifeMax = 64_000;
            NPC.aiStyle = -1;
            NPC.Size = new Vector2(30, 30);
            NPC.noGravity = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            LimbSegmentPos = new Vector2[8];
            LimbSegmentVels = new Vector2[8];
            for (int i = 0; i < LimbSegmentPos.Length; i++)
            {
                LimbSegmentPos[i] = NPC.Center;
            }
        }
        public override void AI()
        {
            //Main.NewText(NPC.Center);

            NPC.rotation = NPC.velocity.ToRotation();
            if (currentTarget == null)
            {
                Cryonophore d = Owner.ModNPC as Cryonophore;
                currentTarget = d.currentTarget;
                NPC.Center = Owner.Center;
                
            }
            else
                StateMachine();
            Time++;
        }
        public override void PostAI()
        {
            ManageLimb();
            float pushRadius = 30;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC other = Main.npc[i];
                if (other.active && other.whoAmI != NPC.whoAmI && other.type == NPC.type)
                {
                    float dist = Vector2.Distance(NPC.Center, other.Center);
                    if (dist < pushRadius && dist > 0f)
                    {
                        // Compute a small push vector away from the other NPC
                        Vector2 pushDir = (NPC.Center - other.Center).SafeNormalize(Vector2.Zero);
                        float pushAmount = (pushRadius - dist) / pushRadius; // stronger when closer
                        NPC.velocity += pushDir * 1.2f * pushAmount;
                    }
                }
            }
        }
        
        private void ManageLimb()
        {
            float segmentLength = 16;
            Vector2 BodyRot = -NPC.rotation.ToRotationVector2();
            LimbSegmentPos[0] = NPC.Center;
            for(int i = 1; i< LimbSegmentPos.Length; i++)
            {
                Vector2 targetPos = LimbSegmentPos[i - 1] + BodyRot * segmentLength;

                Vector2 alignVel = (targetPos - LimbSegmentPos[i]) * 0.5f;
                LimbSegmentVels[i] = Vector2.Lerp(LimbSegmentVels[i], alignVel, 0.5f);
                LimbSegmentPos[i] += LimbSegmentVels[i];


                if (LimbSegmentPos[i] == Vector2.Zero)
                    LimbSegmentPos[i] = NPC.Center;
            }
          
        }

        void StateMachine()
        {
            switch (self.type)
            {
                case ZooidType.basic:
                    NPC.velocity = NPC.AngleTo(currentTarget.Center).ToRotationVector2()*10;
                    break;

                case ZooidType.Ranged:
                    //ManageRanged();
                    break;
            }
        }
        
        void ManageRanged()
        {
            NPC.Center = currentTarget.Center + Main.rand.NextVector2CircularEdge(70, 70);
        }
       
    }
}
