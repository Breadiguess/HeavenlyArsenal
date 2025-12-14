using HeavenlyArsenal.Content.Buffs;
using NoxusBoss.Content.NPCs.Bosses.Draedon.Projectiles.SolynProjectiles;
using NoxusBoss.Content.NPCs.Bosses.Draedon.SpecificEffectManagers;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.Utilities;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.SolynButterfly;

[LegacyName("SolynWhip_Item")]
public class ButterflyStaff : ModItem
{
    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults()
    {
        Item.DamageType = DamageClass.Summon;
        Item.damage = 4000;

        Item.shootSpeed = 4;
        Item.rare = ItemRarityID.Master;
        Item.useTime = 30;
        Item.useStyle = ItemUseStyleID.RaiseLamp;
        Item.useAnimation = 30;
        Item.channel = true;
        Item.noMelee = true;
        Item.mana = 60;
        Item.shootSpeed = 0;
        Item.shoot = ModContent.ProjectileType<ButterflyMinion>();
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        return base.Shoot(player, source, position, velocity, type, damage, knockback);
    }

    public override bool CanShoot(Player player)
    {
        return player.ownedProjectileCounts[Item.shoot] <= 0;
    }

    public override void HoldItem(Player player) { }
}

public enum ButterflyMinionState
{
    Shield,

    Laser
}

public class ButterflyMinionPlayer : ModPlayer
{
    public ButterflyMinionState CurrentState;

    public Projectile Butterfly;

    public bool ButterflyBarrierActive = true;

    public int ButterflyBarrierCurrentHealth;

    public int ButterflyBarrierMaxHealth = 600;

    public int butterflyBarrierRegenRate = 10;

    public int butterflyBarrierTimeSinceLastHit;

    public bool IsMinionActive => Player.ownedProjectileCounts[ModContent.ProjectileType<ButterflyMinion>()] > 0;

    public override void Initialize()
    {
        ButterflyBarrierActive = false;
    }

    public override void ResetEffects()
    {
        //ButterflyBarrierActive = false;
        //ButterflyBarrierCurrentHealth = 0;
    }

    public override void UpdateBadLifeRegen()
    {
        if (ButterflyBarrierActive)
        {
            butterflyBarrierTimeSinceLastHit++;

            if (CurrentState == ButterflyMinionState.Shield)
            {
                if (butterflyBarrierTimeSinceLastHit > 500)
                {
                    if (ButterflyBarrierCurrentHealth < ButterflyBarrierMaxHealth)
                    {
                        ButterflyBarrierCurrentHealth++;
                    }
                }
            }
        }
    }

    public override bool FreeDodge(Player.HurtInfo info)
    {
        if (ButterflyBarrierCurrentHealth > 0 && ButterflyBarrierActive)
        {
            ButterflyBarrierCurrentHealth -= info.Damage;
            butterflyBarrierTimeSinceLastHit = 0; // Reset the time since last hit
            CombatText.NewText(Player.Hitbox, Color.AliceBlue, info.Damage);

            if (ButterflyBarrierCurrentHealth <= 0)
            {
                ButterflyBarrierActive = false; // Deactivate the barrier if health is depleted
            }

            return true;
        }

        return base.FreeDodge(info);
    }
}

public class ButterflyMinion : ModProjectile
{
    //public override string Texture =>  "HeavenlyArsenal/Assets/Textures/Extra/BlackPixel";

    public enum ButterflyMinionState
    {
        Defend,

        Attack
    }

    public static int AttackCooldownMax = 120;

    public ButterflyMinionState AttackState = ButterflyMinionState.Defend;

    public ButterflyMinionState CurrentState;

    public int AttackCooldown;

    public NPC targetNPC;

    public Projectile Barrier;

    public Player Owner => Main.player[Projectile.owner];

    public ref float Time => ref Projectile.ai[0];

    public ref float AttackInterp => ref Projectile.ai[1];

    public override void SetDefaults()
    {
        Projectile.minion = true;
        Projectile.minionSlots = 4;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.hostile = false;
        Projectile.friendly = true;
        Projectile.damage = 0;
        Projectile.knockBack = 0;
        Projectile.height = Projectile.width = 20;
        Projectile.direction = 1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
    }

    private void HandleMinionBuffs()
    {
        Owner.AddBuff(ModContent.BuffType<ButterflyMinionBuff>(), 3);
        var hasMinion = Owner.GetValueRef<bool>("HasSolyn");

        if (Owner.dead)
        {
            hasMinion.Value = false;
        }

        if (hasMinion.Value)
        {
            Projectile.timeLeft = 2;
        }
    }

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; // This allows the minion to target enemies\
        ProjectileID.Sets.MinionShot[Projectile.type] = true; // This allows the minion to shoot projectiles
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Owner.GetModPlayer<ButterflyMinionPlayer>().Butterfly = Projectile;
    }

    public void UseStarFlyEffects()
    {
        // Release star particles.
        var starPoints = Main.rand.Next(3, 9);
        var starScaleInterpolant = Main.rand.NextFloat();
        var starLifetime = (int)float.Lerp(11f, 30f, starScaleInterpolant);
        var starScale = float.Lerp(0.2f, 0.4f, starScaleInterpolant) * Projectile.scale;
        var starColor = Color.Lerp(new Color(1f, 0.41f, 0.51f), new Color(1f, 0.85f, 0.37f), Main.rand.NextFloat());

        var starSpawnPosition = Projectile.Center + new Vector2(Projectile.spriteDirection * 10f, 8f) + Main.rand.NextVector2Circular(16f, 16f);
        var starVelocity = Main.rand.NextVector2Circular(3f, 3f) + Projectile.velocity * (1f - GameSceneSlowdownSystem.SlowdownInterpolant);
        var star = new TwinkleParticle(starSpawnPosition, starVelocity, starColor, starLifetime, starPoints, new Vector2(Main.rand.NextFloat(0.4f, 1.6f), 1f) * starScale, starColor * 0.5f);
        star.Spawn();
    }

    public Vector2 CenterWithOffset(float Multi = 1)
    {
        var Val = (float)Math.Sin(Main.GlobalTimeWrappedHourly) * Multi;
        var Offset = Projectile.Center + new Vector2(0, Val);

        return Offset;
    }

    public override void AI()
    {
        HandleMinionBuffs();
        Projectile.direction = 1;

        if (AttackCooldown > 0)
        {
            AttackCooldown--;
        }

        CurrentState = ButterflyMinionState.Attack;
        //Projectile.NewProjectileBetter(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SolynSentientStar>(),0, 0);
        UseStarFlyEffects();

        switch (CurrentState)
        {
            case ButterflyMinionState.Defend:
                Owner.GetModPlayer<ButterflyMinionPlayer>().ButterflyBarrierActive = true;
                HandleDefend();

                break;
            case ButterflyMinionState.Attack:

                Projectile.velocity = (Owner.Center - Projectile.Center) * 0.02f;

                Projectile.Center = Vector2.Lerp(Projectile.Center, Owner.Center + new Vector2(0, -120), 0.75f);

                if (Owner.velocity.Length() <= 0)
                {
                    Projectile.spriteDirection = Owner.direction;
                }
                else
                {
                    Projectile.spriteDirection = Math.Sign(Owner.Center.X - Projectile.Center.X);
                }

                //Vector2.Lerp(Projectile.Center, Owner.Center + new Vector2(0 * Owner.direction, -40), 0.1f);
                HandleAttack();

                break;
        }
    }

    private void FindTarget()
    {
        targetNPC = null;
        var maxDistance = 900f;

        foreach (var npc in Main.ActiveNPCs)
        {
            if (npc.CanBeChasedBy() && Projectile.Distance(npc.Center) < maxDistance && !npc.immortal && npc.friendly)
            {
                targetNPC = npc;
                maxDistance = Projectile.Distance(npc.Center);
            }
        }
    }

    private void HandleDefend()
    {
        if (Barrier == null || !Barrier.active || Barrier.type != ModContent.ProjectileType<SolynButterflyBarrier>())
        {
            var idx = Projectile.NewProjectile
            (
                Projectile.GetSource_FromThis(),
                Owner.Center,
                Vector2.Zero,
                ModContent.ProjectileType<SolynButterflyBarrier>(),
                0,
                0,
                Projectile.owner
            );

            Barrier = Main.projectile[idx];
        }

        var a = Owner.GetModPlayer<ButterflyMinionPlayer>().ButterflyBarrierCurrentHealth.ToString();

        Main.NewText($"{a}, {Owner.GetModPlayer<ButterflyMinionPlayer>().butterflyBarrierTimeSinceLastHit.ToString()}");
        var desiredAngle = MathHelper.Pi + MathHelper.ToRadians(5) * -Owner.velocity.X / 4;

        var Offset = new Vector2
        (
            0,
            Barrier.height / 2.7f
        ).RotatedBy(desiredAngle);

        Projectile.rotation = MathHelper.Pi + desiredAngle;
        var worldTarget = Barrier.Center + Offset;

        if (Owner.velocity.Length() <= 0)
        {
            Projectile.spriteDirection = Owner.direction;
        }
        else
        {
            Projectile.spriteDirection = Math.Sign(Owner.Center.X - Projectile.Center.X);
        }

        Projectile.velocity = (Owner.Center - Projectile.Center) * 0.04f;
        Projectile.Center = Vector2.Lerp(CenterWithOffset(10), worldTarget, 0.7f);
    }

    private void HandleAttack()
    {
        if (targetNPC == null || !targetNPC.active || targetNPC.life <= 1 || targetNPC.immortal || targetNPC.dontTakeDamage || !targetNPC.CanBeChasedBy())
        {
            AttackInterp = 0;
            targetNPC = Projectile.FindTargetWithinRange(900);
        }
        else
        {
            var directionto = Math.Sign(targetNPC.Center.X - Projectile.Center.X);
            Projectile.spriteDirection = directionto;

            if (AttackCooldown <= 0 &&
                Owner.ownedProjectileCounts[ModContent.ProjectileType<SolynButterflyChargeUp>()] <= 0 &&
                Owner.ownedProjectileCounts[ModContent.ProjectileType<SolynButterflyBeam>()] <= 0)
            {
                Projectile.NewProjectile
                (
                    Projectile.GetSource_FromThis(),
                    Projectile.Center + Projectile.velocity * 10,
                    Vector2.Zero,
                    ModContent.ProjectileType<SolynButterflyChargeUp>(),
                    Projectile.damage,
                    0,
                    default,
                    Projectile.identity,
                    -10
                );
            }

            if (AttackCooldown <= 0 && Owner.ownedProjectileCounts[ModContent.ProjectileType<SolynButterflyBeam>()] <= 0)
            {
                AttackInterp = float.Lerp(AttackInterp, 1, 0.04f);
            }
        }
    }

    public override Color? GetAlpha(Color drawColor)
    {
        //float immunityPulse = 1f - Cos01(TwoPi * ImmunityFrameCounter / ImmunityFramesGrantedOnHit * 2f);
        var baseColor = Color.Lerp(drawColor, Color.White, 0.2f);
        var immunityColor = Color.Lerp(drawColor, new Color(255, 0, 50), 0.9f);
        var color = Color.Lerp(baseColor, immunityColor, 0) * float.Lerp(1f, 0.3f, 0);

        return color * Projectile.Opacity;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var Butterfly = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Summon/SolynButterfly/ButterflyMinion").Value;
        var DrawPos = Projectile.Center - Main.screenPosition;

        //DrawPos += new Vector2(0, (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 10);
        var Origin = new Vector2(Butterfly.Width / 2, Butterfly.Height / 4 / 2 + 12);
        var Val = (float)Math.Sin(Main.GlobalTimeWrappedHourly);
        var Rot = Projectile.rotation - MathHelper.ToRadians(Val);
        DrawSpinningStar(Rot);
        var Flip = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        //DrawAfterimages(Flip);

        var fame = Butterfly.Frame(1, 4, 0, (int)(Main.GlobalTimeWrappedHourly * 10) % 4);

        //Utils.DrawBorderString(Main.spriteBatch, Projectile.direction.ToString() + ", AttackInterp: "+ AttackInterp.ToString(), DrawPos - Vector2.UnitY * -100, Color.AliceBlue);

        var value = (float)Math.Abs(Math.Sin(Main.GlobalTimeWrappedHourly / 4) + 1);

        var backglowColor = Color.Cyan with
                            {
                                A = 0
                            } *
                            value;

        for (var i = 0; i < 4; i++)
        {
            for (var j = 0; j < 4; j++)
            {
                Main.EntitySpriteDraw(Butterfly, DrawPos + (MathHelper.TwoPi * i / 4f).ToRotationVector2() * 2f, fame, backglowColor, Rot, Origin, Projectile.scale, Flip);
            }
        }

        Main.EntitySpriteDraw(Butterfly, DrawPos, fame, Color.AntiqueWhite, Rot, Origin, Projectile.scale, Flip);

        return false; // Prevent default drawing
    }

    private void DrawAfterimages(SpriteEffects effect)
    {
        var Butterfly = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Summon/SolynButterfly/ButterflyMinion").Value;

        for (var i = 0; i < Projectile.oldPos.Length - 1; i++)
        {
            var Val = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 1.1f);
            var Rot = Projectile.rotation - MathHelper.ToRadians(Val);

            var drawPos = Projectile.oldPos[i] - Main.screenPosition;
            drawPos += Butterfly.Size() / 2.5f;
            var alpha = 1f - i / (float)Projectile.oldPos.Length;
            var color = new Color(255, 255, 255, (int)(alpha * 255));
            var Origin = new Vector2(Butterfly.Width / 2, Butterfly.Height / 4 / 2);

            var fame = Butterfly.Frame(1, 4, 0, (int)(Main.GlobalTimeWrappedHourly * 10) % 4);
            Main.EntitySpriteDraw(Butterfly, drawPos, fame, color, Rot, Origin, Projectile.scale * alpha, effect);
        }
    }

    private void DrawSpinningStar(float Rotation)
    {
        var StarTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Summon/SolynButterfly/StarSpin").Value;
        var FrameCount = 13;

        var val = (int)(Main.GlobalTimeWrappedHourly * 10.1f) % 3;

        var Frame = StarTexture.Frame(1, FrameCount, 0, val); //new Rectangle(0, val * (StarTexture.Height / FrameCount), StarTexture.Width, StarTexture.Height / FrameCount);

        var Origin = new Vector2(StarTexture.Width / 2, StarTexture.Height / 11 / 2);

        var Flip = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        var Rot = Projectile.Center.AngleTo(Projectile.velocity + Projectile.Center); //Rotation - MathHelper.PiOver2 * (1 - AttackInterp);

        var DrawPos = Projectile.Center - Main.screenPosition;
        DrawPos += new Vector2(0, 20);
        Main.EntitySpriteDraw(StarTexture, DrawPos, Frame, Color.Cornsilk, Rot, Origin, 2 * (1 + AttackInterp), Flip);
    }
}