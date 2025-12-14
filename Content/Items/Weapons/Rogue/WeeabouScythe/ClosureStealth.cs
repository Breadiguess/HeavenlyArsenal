using CalamityMod;
using NoxusBoss.Assets;
using Terraria.Audio;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue.WeeabouScythe;

internal class ClosureStealth : ModProjectile
{
    public ref float Timer => ref Projectile.ai[0];

    public ref Player Owner => ref Main.player[Projectile.owner];

    public override void SetDefaults()
    {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 1800;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = true;
        Projectile.usesLocalNPCImmunity = false;
        Projectile.localNPCHitCooldown = -1;
        Projectile.aiStyle = -1;
    }

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;
        ProjectileID.Sets.CanDistortWater[Type] = true;
    }

    public override void AI()
    {
        if (Timer > 20)
        {
            Projectile.velocity.X *= 0.94f;
            Projectile.velocity.Y++;
        }

        Timer++;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Owner.Center = target.Center - new Vector2(target.direction * (target.width * 2.25f), 0);

        Owner.GetModPlayer<CloseureStrikePlayer>().IsBeingEdgy = true;

        Owner.GetModPlayer<CloseureStrikePlayer>().targetedNPC = target;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var texture = ModContent.Request<Texture2D>(Texture).Value;

        var drawPosition = Projectile.Center - Main.screenPosition;
        var frame = texture.Frame(); //Projectile.frame);
        var spriteEffects = SpriteEffects.None;
        var origin = new Vector2(frame.Width / 2, frame.Height / 2);
        var Rot = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

        Main.EntitySpriteDraw(texture, drawPosition, frame, lightColor, Rot, origin, Projectile.scale, spriteEffects);

        return false;
    }
}

internal class CloseureStrikePlayer : ModPlayer
{
    public int TimerMax = 120;

    public int Timer;

    public bool IsBeingEdgy;

    public NPC targetedNPC { get; set; }

    public override void PostUpdateMiscEffects()
    {
        if (IsBeingEdgy)
        {
            Timer++;
            Player.Calamity().rogueStealth = -1;

            if (targetedNPC == null)
            {
                IsBeingEdgy = false;
                Timer = 0;

                return;
            }

            targetedNPC.GetGlobalNPC<LobotomizeTarget>().BeingLobotomized = true;

            if (Timer > 30 && Timer % 2 == 0)
            {
                // Calculate the slash's direction. This is randomized a bit but generally favors appearing to be facing towards the player, as through Nameless is
                // slicing his hands back and forth while flying towards the player.
                // If close to the player, however, this is not necessary and the direction is completely random.
                var slashDirection = targetedNPC.AngleTo(targetedNPC.Center) + Main.rand.NextFloatDirection() * 1.05f - MathHelper.PiOver2;

                // Calculate the slash draw position. This is randomized a bit to create variety.
                var randomOffset = Utils.Remap(targetedNPC.Distance(Player.Center), 100f, 250f, 10f, 90f);
                var slashSpawnPosition = targetedNPC.Center.SafeNormalize(Main.rand.NextVector2Unit());

                var SlashVelocity = Main.rand.NextVector2Unit();
                SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.SliceTelegraph, targetedNPC.Center);

                Projectile.NewProjectileDirect
                (
                    Player.GetSource_FromThis(),
                    targetedNPC.Center - slashSpawnPosition + Main.rand.NextVector2Unit() * randomOffset,
                    Vector2.Zero,
                    ModContent.ProjectileType<LightSlash>(),
                    1000,
                    0,
                    Player.whoAmI,
                    slashDirection
                );
            }

            if (Timer > TimerMax)
            {
                IsBeingEdgy = false;
                Timer = 0;
                targetedNPC.StrikeInstantKill();
            }

            //Main.NewText($"{Timer}");
        }
    }

    public override void PreUpdateMovement()
    {
        if (IsBeingEdgy)
        {
            if (targetedNPC != null)
            {
                Player.direction = Math.Sign(Player.Center.X - targetedNPC.Center.X);
            }

            Player.velocity = Vector2.Zero;
        }
        else
        {
            base.PreUpdateMovement();
        }
    }

    public override bool FreeDodge(Player.HurtInfo info)
    {
        if (IsBeingEdgy)
        {
            return true;
        }

        return base.FreeDodge(info);
    }
}

internal class LobotomizeTarget : GlobalNPC
{
    public bool BeingLobotomized;

    public override bool InstancePerEntity => true;

    public override bool PreAI(NPC npc)
    {
        if (BeingLobotomized)
        {
            return false;
        }

        return base.PreAI(npc);
    }

    public override void ResetEffects(NPC npc)
    {
        BeingLobotomized = false;
    }
}