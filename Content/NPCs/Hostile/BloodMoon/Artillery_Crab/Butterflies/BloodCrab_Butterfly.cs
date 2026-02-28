using CalamityMod;
using Luminance.Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Artillery_Crab.Butterflies
{
    internal class BloodCrab_Butterfly : BaseBloodMoonNPC
    {
        public bool OwnerExists => Owner != null;
        public NPC Owner => Main.npc.FirstOrDefault(n => n.whoAmI == ParentID && n.type == ModContent.NPCType<BloodCrab>());
        public int ParentID = -1;
        public int SocketIndex;
        public int CooldownTimeMax = 180;
        public int CooldownTime = 180;
        public bool ParentNeedsBlood = false;

        public enum ButterflyState
        {
            Attached,
            FindTarget,
            Detach,
            MoveToTarget,
            Extracting,
            Returning,
        }

        public ButterflyState State
        {
            get => (ButterflyState)NPC.ai[2];
            set => NPC.ai[2] =(float)value;
        }
        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            if (State == ButterflyState.Attached || State == ButterflyState.Extracting)
                return false;
            return base.DrawHealthBar(hbPosition, ref scale, ref position);
        }

        public override void SendExtraAI2(BinaryWriter writer)
        {
            base.SendExtraAI2(writer);
            writer.Write7BitEncodedInt(ParentID);
            writer.Write7BitEncodedInt(SocketIndex);
        }
        public override void ReceiveExtraAI2(BinaryReader reader)
        {
            base.ReceiveExtraAI2(reader);
            ParentID = reader.Read7BitEncodedInt();
            SocketIndex = reader.Read7BitEncodedInt();
        }
        public override string Texture =>  MiscTexturesRegistry.PixelPath;
        public override int MaxBlood => 50;

        public override BloodMoonBalanceStrength Strength => new(0.2f, 1.3f, 0);

        protected override void SetDefaults2()
        {
            NPC.lifeMax = 50;
            NPC.takenDamageMultiplier = 0;
            NPC.Size = new Vector2(10, 10);
            NPC.damage = 120;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.friendly = false;
        }
        public override void AI()
        {
            switch (State) 
            {
                case ButterflyState.Attached:
                    Attached();
                    break;
                case ButterflyState.FindTarget:
                    FindTarget();
                    break;
                case ButterflyState.Detach:
                    Detach();
                    break;

                case ButterflyState.MoveToTarget:
                    MoveToTarget();
                    break;
                case ButterflyState.Extracting:
                    Extract();
                    break;

                case ButterflyState.Returning:
                    Return();
                    break;
            }
            if (!Owner.active)
            {
                NPC.active = false;
            }

            Time++;
        }

      

        void Attached()
        {
            if (!OwnerExists)
                return;


            NPC.Center = Owner.As<BloodCrab>().ButterflyAttachPoints[SocketIndex].Position;
            NPC.dontCountMe = true;
            NPC.dontTakeDamage = true;


            ParentNeedsBlood = Owner.As<BloodCrab>().Blood < Owner.As<BloodCrab>().MaxBlood;

            if (ParentNeedsBlood)
            {
                if(CooldownTime--<0)
                Target = FindClosestCompatibleNPC();
                if (Target != null)
                {
                    State = ButterflyState.Detach;
                    NPC.netUpdate = true;
                }
            }

        }

        private void FindTarget()
        {
          
        }
        private void Detach()
        {
            Owner.As<BloodCrab>().ButterflyAttachPoints[SocketIndex].Filled = false;
            NPC.dontCountMe = false;
            NPC.dontTakeDamage = false;



            State = ButterflyState.MoveToTarget;
            NPC.netUpdate = true;
            Owner.ForceNetUpdate(true);

        }
        private void MoveToTarget()
        {
            //lazy rn
            NPC.velocity = NPC.Center.DirectionTo(Target.Center).RotatedBy(Cos(Time/10f)) * 10;
            //NPC.Center = Vector2.Lerp(NPC.Center, Target.Center, 0.05f);
            if(NPC.Center.Distance(Target.Center)<20)
            {
                NPC.velocity = Vector2.Zero;
                State = ButterflyState.Extracting;
                Time = -1;
                NPC.netUpdate = true;
            }
        }

        private void Extract()
        {
            NPC.Center = Target.Hitbox.Top();

            if(Target.active)
            if (Time > 120)
            {
                this.Blood += 50;
                NPC npc = Target as NPC;
                npc.SimpleStrikeNPC(NPC.damage, 0);
                State = ButterflyState.Returning;
                NPC.netUpdate = true;
                Target = null;
                return;
            }
                else
                {
                    State = ButterflyState.Returning;
                }
        }

        private void Return()
        {
            NPC.velocity = NPC.Center.DirectionTo(Owner.As<BloodCrab>().ButterflyAttachPoints[SocketIndex].Position) * 10;
          
            if(NPC.Center.Distance(Owner.As<BloodCrab>().ButterflyAttachPoints[SocketIndex].Position)<20)
            {
                NPC.velocity = Vector2.Zero;    
                Owner.As<BloodCrab>().ButterflyAttachPoints[SocketIndex].Filled = true;
                GiveCrabBlood(Owner.As<BloodCrab>(), this.Blood);
                State = ButterflyState.Attached;
                Time = -1;
                CooldownTime = CooldownTimeMax;
                NPC.netUpdate = true;
                return;
            }
        }

        private bool GiveCrabBlood(BloodCrab crab, int Amount)
        {
            if (crab.Blood < crab.MaxBlood)
            {
                this.Blood -= Amount;
                crab.Blood += Amount;
            }

            return crab.Blood < crab.MaxBlood;
        }

        private NPC FindClosestCompatibleNPC()
        {
            foreach(var npc in Main.ActiveNPCs)
            {
                if (npc.Distance(NPC.Center) < 900 && (npc.type != ModContent.NPCType<BloodCrab_Butterfly>() && npc.type != ModContent.NPCType<BloodCrab>()))
                {
                    if (npc.ModNPC is not null && npc.ModNPC is BaseBloodMoonNPC)
                        return npc;
                }
                else
                    continue;
            }
            return null;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            //Utils.DrawBorderString(spriteBatch, State.ToString(), NPC.Center - Main.screenPosition, Color.White);
            return base.PreDraw(spriteBatch, screenPos, drawColor);
        }
    }
}
