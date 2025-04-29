using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles
{
    public class globalHomingAI : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        /// <summary>
        /// Whether or not the projectile will home in
        /// </summary>
        public bool enabled = false;
        /// <summary>
        /// How fast the projectile will turn to home in on the target. Recommended 0.5f - 5f
        /// </summary>
        public float agility = 0;
        /// <summary>
        /// How far away the projectile can acquire new targets. Measured in tiles. -1 is infinite range
        /// </summary>
        public float range = -1;
        /// <summary>
        /// How close the projectile will stop homing. Do not have the projectile be 'annoying' to enemies. Measured in tiles ( * 16 applied in code)
        /// </summary>
        public float slack = 0;
        /// <summary>
        /// Can it home in through walls or does it require LOS to acquire a target
        /// </summary>
        public bool wallHack = false;
        /// <summary>
        /// Slows the projectile down by this value 60 times/sec. Can create more accurate, agile, and stable homing. Recommended 1.01f - 1.1f
        /// </summary>
        public float decel = 1;
        public bool hasTarget;
        /// <summary>
        /// If the projectile is close enough to the target position, stop homing AI
        /// </summary>
        public bool slacking;
        public int targetID = -1;
        public Vector2 targetPos = Vector2.Zero;
        /// <summary>
        /// If the projectile is less than [range] from [range center], home in
        /// </summary>
        public Vector2 rangeCenter = Vector2.Zero;

        public override bool PreAI(Projectile proj)
        {
            rangeCenter = proj.Center;
            return true;
        }
        public override void AI(Projectile proj)
        {
            if (enabled)
            {
                hasTarget = false;
                Player player = Main.player[proj.owner];
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npcScan = Main.npc[i];
                    if (npcScan.CanBeChasedBy() && (rangeCenter.Distance(npcScan.Center) < range * 16 || range == -1) && (Collision.CanHitLine(proj.Center, 0, 0, npcScan.position, npcScan.width, npcScan.height) || wallHack))
                    {
                        hasTarget = true;
                        targetID = npcScan.whoAmI;
                        targetPos = npcScan.Center;
                    }
                }
                if (player.HasMinionAttackTargetNPC)
                {
                    hasTarget = true;
                    targetID = player.MinionAttackTargetNPC;
                    targetPos = Main.npc[player.MinionAttackTargetNPC].Center;
                }
                if (hasTarget)
                {
                    if (proj.Distance(targetPos) > slack * 16)
                    {
                        slacking = false;
                        proj.velocity /= decel;
                        proj.velocity += proj.DirectionTo(targetPos) * agility;
                    }
                    else
                    {
                        slacking = true;
                    }
                }
            }
        }
    }
}
