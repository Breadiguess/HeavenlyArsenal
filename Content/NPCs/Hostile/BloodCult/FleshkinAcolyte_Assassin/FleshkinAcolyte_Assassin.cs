using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodCult.FleshkinAcolyte_Assassin
{
    public partial class FleshkinAcolyte_Assassin : BaseBloodMoonNPC
    {
        public override string Texture => this.GetPath();

        public override void SetStaticDefaults2()
        {
            Main.npcFrameCount[NPC.type] = 30;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
        }
        public override BloodMoonBalanceStrength Strength => new(0,1,1);
        public override int MaxBlood =>  30;
        protected override void SetDefaults2()
        {
            NPC.Size = new Vector2(50, 50);

            NPC.friendly = false;

            NPC.lifeMax = 30_000;

            NPC.knockBackResist = 0.1f;

            NPC.defense = 90;

            NPC.damage = 400;
        }

        const int _stealthMax = 100;
        private int _stealth;
        public int StealthAmount
        {
            get => _stealth;
            set => _stealth = Math.Clamp(value, 0, _stealthMax);
        }



        public override bool PreAI()
        {
            Collision.StepUp(ref NPC.position, ref NPC.velocity, (int)NPC.Size.X, (int)NPC.Size.Y, ref NPC.stepSpeed, ref NPC.gfxOffY);


            if (StealthAmount > 25)
            {
                NPC.chaseable = false;
          
            }
            //todo: if stealth is greater than 75, check if near any players. if not, they cannot take damage. otherwise, should take damage as normal
            // If stealth is greater than 75, check if any valid player is within 100 units.
            // If no valid nearby player is found, the NPC should not take damage.
            if (StealthAmount > 75)
            {
                const float detectRadius = 125f;
                bool nearAny = false;

                for (int i = 0; i < Main.player.Length-1; i += 1)
                {
                    var player = Main.player[i];
                    if (!player.active || player.dead)
                    {
                        continue;
                    }

                    // Use player.Distance if available on the Player type; otherwise fall back to Vector2.Distance
                    float distance;
                    try
                    {
                        distance = player.Distance(this.NPC.Center);
                    }
                    catch
                    {
                        distance = Vector2.Distance(player.Center, this.NPC.Center);
                    }

                    if (distance < detectRadius)
                    {
                        nearAny = true;
                        break;
                    }
                }

                NPC.dontTakeDamage = !nearAny;
            }
            else
            {
                // Ensure damage is allowed when not in high stealth
                NPC.dontTakeDamage = false;
            }

            NPC.Opacity = Math.Clamp(1 - LumUtils.InverseLerp(0, _stealthMax, StealthAmount), 0.1f, 1);

            return base.PreAI();
        }



       


      
    }
}
