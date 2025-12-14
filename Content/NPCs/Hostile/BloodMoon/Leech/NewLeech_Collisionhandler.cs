namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech;

partial class newLeech
{
    public void OnHitBoxCollide(int WhoAmI, Projectile origin) { }

    public override bool? CanBeHitByItem(Player player, Item item)
    {
        if (AdjHitboxes != null)
        {
            foreach (var hitbox in AdjHitboxes)
            {
                if (hitbox.Intersects(item.Hitbox))
                {
                    return true;
                }
            }
        }

        return base.CanBeHitByItem(player, item);
    }

    public override bool? CanCollideWithPlayerMeleeAttack(Player player, Item item, Rectangle meleeAttackHitbox)
    {
        if (AdjHitboxes != null)
        {
            foreach (var hitbox in AdjHitboxes)
            {
                if (hitbox.Intersects(meleeAttackHitbox))
                {
                    return true;
                }
            }
        }

        return false;

        return base.CanCollideWithPlayerMeleeAttack(player, item, meleeAttackHitbox);
    }

    public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        var a = 0;

        foreach (var r in AdjHitboxes)
        {
            if (projectile.Hitbox.IntersectsConeFastInaccurate(r.Center(), r.Width, 0, MathHelper.PiOver2)) { }

            a++;
        }

        base.ModifyHitByProjectile(projectile, ref modifiers);
    }

    public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitByProjectile(projectile, hit, damageDone);
    }

    public override bool? CanBeHitByProjectile(Projectile projectile)
    {
        return base.CanBeHitByProjectile(projectile);

        // Get the projectile's hitbox (rectangle in world coordinates)
        var projHitbox = projectile.Hitbox;

        if (Main.LocalPlayer == Main.player[projectile.owner])
        {
            if (AdjHitboxes != null && !NPC.justHit)
            {
                foreach (var hitbox in AdjHitboxes)
                {
                    if (projHitbox.IntersectsConeFastInaccurate(hitbox.Center(), hitbox.Width, 0, 360) && !NPC.justHit)
                    {
                        var a = NPC.GetIncomingStrikeModifiers(projectile.DamageType, projectile.direction);
                        var b = NPC.CalculateHitInfo(a.GetDamage(projectile.damage, false, false, Main.player[projectile.owner].luck), projectile.direction, damageType: a.DamageType);
                        //NPC.StrikeNPC(b, false, false);
                        //NetMessage.SendStrikeNPC(NPC, b);
                        Main.LocalPlayer.StrikeNPCDirect(NPC, b);
                        // NPC.SimpleStrikeNPC(projectile.damage, 
                        //    projectile.direction, crit: false, projectile.knockBack, projectile.DamageType, true, Main.player[projectile.owner].luck, false);

                        NPC.justHit = true;

                        return true;
                    }
                }
            }
        }

        // If no custom hitboxes were hit, block the hit
        return false;
    }

    public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox)
    {
        if (AdjHitboxes != null)
        {
            foreach (var i in AdjHitboxes)
            {
                npcHitbox = i;

                if (npcHitbox.IntersectsConeFastInaccurate(victimHitbox.Center(), npcHitbox.Width, 0, MathHelper.TwoPi))
                {
                    return false;
                }
            }
        }

        return base.ModifyCollisionData(victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox);
    }
}