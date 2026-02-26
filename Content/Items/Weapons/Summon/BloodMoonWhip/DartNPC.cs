using System.Collections.Generic;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.BloodMoonWhip;

internal class DartNPC : GlobalNPC
{
    // Store IDs, not Projectile refs (projectile slots are recycled)
    private readonly HashSet<int> dartIds = new();

    public override bool InstancePerEntity => true;

    public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        int bloodDartType = ModContent.ProjectileType<BloodDart>();
        int stingType = ModContent.ProjectileType<BloodySting_Projectile>();

        // Track stuck darts
        if (projectile.type == bloodDartType)
        {
            if (projectile.ModProjectile is BloodDart dart &&
                dart.CurrentState == BloodDart.NeedleState.StuckInEnemy)
            {
                dartIds.Add(projectile.whoAmI);
            }
        }

        // Whip detonation logic
        if (projectile.type == stingType)
        {
            // Purge invalid/recycled entries before counting/using them
            CleanupDarts(npc, bloodDartType);

            if (dartIds.Count > 3)
            {
                Player owner = Main.player[projectile.owner];
                if (owner == null || !owner.active)
                    return;

                var hitInfo = new NPC.HitInfo
                {
                    DamageType = DamageClass.Summon,
                    Damage = dartIds.Count * 1000,
                    Knockback = 0f,
                    HitDirection = npc.Center.X < owner.Center.X ? -1 : 1
                };

                owner.StrikeNPCDirect(npc, hitInfo);

                // Dislodge/modify darts safely
                foreach (int id in dartIds)
                {
                    Projectile proj = Main.projectile[id];

                    if (!proj.active || proj.type != bloodDartType)
                        continue;

                    if (proj.ModProjectile is not BloodDart dart)
                        continue;

                    dart.Time = 0;
                    dart.CurrentState = BloodDart.NeedleState.Dislodge;
                    dart.timeOffset = Main.rand.Next(0, 30);

                    proj.damage = (int)(proj.damage * 1.4f);
                }

                dartIds.Clear();
            }

            // Tag damage
            if (projectile.minion && npc.HasBuff(ModContent.BuffType<BloodwhipBuff>()))
            {
                int extraDamage = BloodwhipBuff.TagDamage;
                npc.SimpleStrikeNPC(extraDamage, 0, noPlayerInteraction: true);
                CombatText.NewText(npc.Hitbox, Color.Red, extraDamage);
            }

            base.OnHitByProjectile(npc, projectile, hit, damageDone);
        }
    }

    private void CleanupDarts(NPC npc, int bloodDartType)
    {
        if (dartIds.Count == 0)
            return;

        // Remove entries that are dead, recycled, or no longer "stuck"
        dartIds.RemoveWhere(id =>
        {
            if (id < 0 || id >= Main.maxProjectiles)
                return true;

            Projectile p = Main.projectile[id];
            if (!p.active || p.type != bloodDartType)
                return true;

            if (p.ModProjectile is not BloodDart dart)
                return true;

            // Optional but recommended: only count darts still lodged
            if (dart.CurrentState != BloodDart.NeedleState.StuckInEnemy)
                return true;

            // Optional: if BloodDart tracks its target NPC, enforce it here:
            // return dart.StuckNpcWhoAmI != npc.whoAmI;

            return false;
        });
    }
}