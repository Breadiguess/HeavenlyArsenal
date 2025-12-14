using System.Collections.Generic;
using System.Linq;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab;
using NoxusBoss.Assets;
using NoxusBoss.Core.AdvancedProjectileOwnership;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.BloodMoonWhip;

public class BlacklistedProjectiles : ModSystem
{
    public static HashSet<int> BlackListedProjectiles = new();

    public override void PostSetupContent()
    {
        for (var i = 0; i < ProjectileLoader.ProjectileCount; i++)
        {
            var name = ProjectileID.Search.GetName(i).ToLower();

            if (name.EndsWith("body") || name.EndsWith("tail"))
            {
                BlackListedProjectiles.Add(i);

                continue;
            }

            if (ProjectileID.Sets.MinionShot[i] || ProjectileID.Sets.SentryShot[i] || ProjectileID.Sets.IsAWhip[i] || ProjectileID.Sets.LightPet[i])
            {
                BlackListedProjectiles.Add(i);
            }
        }
    }
}

public class Bloodwhip_GlobalProjectile : GlobalProjectile
{
    public bool CanFire = false;

    public int DisipateTimerMAX = 8 * 60;

    public int DisipateTimer;

    public int CooldownLength = 50;

    public int Timer;

    public int NPCIndex = -1;

    public Player Owner;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
    {
        if (BlacklistedProjectiles.BlackListedProjectiles.Contains(entity.type))
        {
            return false;
        }

        if (entity.sentry)
        {
            return false;
        }

        if (entity.DamageType != DamageClass.Summon)
        {
            return false;
        }

        return lateInstantiation && true;
    }

    public override void PostAI(Projectile projectile)
    {
        Owner = Main.player[projectile.owner];

        if (Owner.GetModPlayer<BloodWhipPlayer>().hitNPCs.Count > 0)
        {
            if (DisipateTimer > 0)
            {
                doWhipLogic(projectile);
            }
            else
            {
                Timer = 0;
            }
        }
        else
        {
            Timer = 0;
        }

        if (DisipateTimer > 0)
        {
            DisipateTimer--;
        }
    }

    public void doWhipLogic(Projectile proj)
    {
        if (proj.OwnerMinionAttackTargetNPC != null)
        {
            NPCIndex = proj.OwnerMinionAttackTargetNPC.whoAmI;
        }
        else
        {
            NPCIndex = -1;

            return;
        }

        if (Main.npc[NPCIndex] == null)
        {
            NPCIndex = -1;
            Timer++;

            return;
        }

        if (Timer < 120 && Timer > 40)
        {
            var Spawnpos = proj.Center + Main.rand.NextVector2CircularEdge(20, 20);
            var a = Dust.NewDustPerfect(Spawnpos, DustID.Blood);
        }

        //Main.NewText($"Current time:{Timer}. chosen target: {Main.npc[NPCIndex].FullName} ({Main.npc[NPCIndex].whoAmI})");
        if (Timer > 120 * proj.MaxUpdates)
        {
            var toNPC = proj.AngleTo(proj.OwnerMinionAttackTargetNPC.Center).ToRotationVector2() * 30;
            var Damage = proj.originalDamage / 4 + proj.damage / 2;
            proj.NewProjectileBetter(proj.GetSource_FromThis(), proj.Center, toNPC, ModContent.ProjectileType<BloodSpit>(), Damage, 1, ai1: NPCIndex);

            Timer = -CooldownLength - 1;
            NPCIndex = -1;
        }

        Timer++;
    }

    public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (Timer < 60)
        {
            modifiers.FinalDamage *= 1.2f;
        }
    }

    public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (target.type == ModContent.NPCType<BloodMortar>())
        {
            return;
        }

        if (Timer > 0)
            //Main.NewText(Timer);
        {
            Timer = 0;
        }
    }

    public override void PostDraw(Projectile projectile, Color lightColor)
    {
        if (projectile.sentry)
        {
            return;
        }

        if (BlacklistedProjectiles.BlackListedProjectiles.Contains(projectile.type))
        {
            return;
        }

        if (projectile.DamageType != DamageClass.Summon)
        {
            return;
        }

        if (projectile.owner != Main.LocalPlayer.whoAmI)
        {
            return;
        }

        //Main.spriteBatch.End();
        //Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

        Texture2D Glow = GennedAssets.Textures.GreyscaleTextures.Corona;

        var DrawPos = projectile.Center - Main.screenPosition;
        var Origin = Glow.Size() * 0.5f;
        var Rot = MathHelper.ToRadians(Main.GlobalTimeWrappedHourly * 40);
        var scale = new Vector2(0.25f) * (Timer / 120f);

        Main.EntitySpriteDraw
        (
            Glow,
            DrawPos,
            null,
            Color.Red with
            {
                A = 0
            },
            Rot,
            Origin,
            scale,
            SpriteEffects.None
        );

        //string a = $"DisipateTimer: {DisipateTimer}\n" + $"Timer: {Timer}\n";
        //Utils.DrawBorderString(Main.spriteBatch, a, DrawPos, Color.AntiqueWhite);
        // Main.spriteBatch.End();
        //Main.spriteBatch.Begin();
    }
}

public class BloodWhipPlayer : ModPlayer
{
    public List<NPC> hitNPCs = new(Main.maxNPCs);

    public void UpdateList()
    {
        if (hitNPCs.Count > 0)
        {
            hitNPCs = hitNPCs.Where(npc => npc.active).ToList();
            hitNPCs = hitNPCs.Where(npc => npc.GetGlobalNPC<Bloodwhip_GlobalNPC>().Timer > 0).ToList();
        }
    }

    public override void PostUpdateMiscEffects()
    {
        UpdateList();
    }

    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (target.type != ModContent.NPCType<BloodMortar>())
        {
            if (proj.type != ModContent.ProjectileType<ViscousWhip_Proj>())
            {
                return;
            }
        }

        if (!hitNPCs.Contains(target))
        {
            hitNPCs.Add(target);
            target.GetGlobalNPC<Bloodwhip_GlobalNPC>().Timer = 8 * 60;
        }

        if (hitNPCs.Contains(target))
        {
            target.GetGlobalNPC<Bloodwhip_GlobalNPC>().Timer = 8 * 60;
        }

        foreach (var projectile in Main.ActiveProjectiles)
        {
            if (BlacklistedProjectiles.BlackListedProjectiles.Contains(projectile.type))
            {
                continue;
            }

            if (projectile.sentry)
            {
                continue;
            }

            if (projectile.DamageType != DamageClass.Summon)
            {
                continue;
            }

            if (projectile.owner != Player.whoAmI)
            {
                continue;
            }

            projectile.GetGlobalProjectile<Bloodwhip_GlobalProjectile>().DisipateTimer = projectile.GetGlobalProjectile<Bloodwhip_GlobalProjectile>().DisipateTimerMAX;
        }
    }
}

public class Bloodwhip_GlobalNPC : GlobalNPC
{
    public int Timer;

    public bool StruckByWhip;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        if (entity.immortal || entity.townNPC || entity.stinky)
        {
            return false;
        }

        return true;
    }

    public override void PostAI(NPC npc)
    {
        if (Timer > 0)
        {
            Timer--;
        }
    }
}