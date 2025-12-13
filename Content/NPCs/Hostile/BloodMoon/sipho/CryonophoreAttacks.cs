using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.sipho
{
    partial class Cryonophore
    {
        public enum Behavior
        {
            debug,
            findTarget,
            Attack,
            DetachLimb
        }
        public Behavior CurrentState;

        public void StateMachine()
        {
            switch (CurrentState)
            {
                case Behavior.debug:
                    CurrentState = Behavior.findTarget;
                    break;
                case Behavior.findTarget:
                    findTarget();
                    break;
                case Behavior.Attack:
                    ManageAttack();
                    break;
                case Behavior.DetachLimb:
                    DetachLimb();
                    break;
            }
        }
        void findTarget()
        {
            if (currentTarget == null || !currentTarget.active)
            {
                HashSet<Player> temp = new HashSet<Player>(Main.player.Length);
                foreach (Player player in Main.ActivePlayers)
                {
                    temp.Add(player);
                }
                List<Player> temp2 = temp.ToList();
                temp2.Sort((a, b) => a.Distance(NPC.Center).CompareTo(b.Distance(NPC.Center)));
                //placeholder override for  now
                currentTarget = temp2[0];
            }
            else
            {
                CurrentState = Behavior.Attack;
            }



        }
        void ManageAttack()
        {
            if (NPC.Distance(currentTarget.Center) < 100)
            {
                Time = 0;
                CurrentState = Behavior.DetachLimb;
                return;
            }
            NPC.velocity = NPC.AngleTo(currentTarget.Center).ToRotationVector2();
        }
        void DetachLimb()
        {
            if (Time == 1)
                foreach (var zooid in OwnedZooids)
                {
                    if (!Main.rand.NextBool(6))
                        continue;
                    //else if (blood <= 0)
                    //    continue;

                    var id = zooid.Value.Item1.id;
                    var type = zooid.Value.Item1.type;
                    if (zooid.Value.Item2 != null)
                        Main.NewText(id + ", " + type);
                    SpawnZooid(id);
                }
            NPC.velocity = NPC.AngleTo(currentTarget.Center).ToRotationVector2() * -10;
            if (Time > 60)
                CurrentState = Behavior.Attack;
        }
    }
}
