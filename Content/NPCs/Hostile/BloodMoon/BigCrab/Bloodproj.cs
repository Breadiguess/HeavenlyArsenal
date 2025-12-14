using System.Collections.Generic;
using CalamityMod;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab;

internal class Bloodproj : ModProjectile
{
    public enum BloodProjAI
    {
        Normal,

        Burrow
    }

    public Player Unfortunate;

    public BloodProjAI CurrentState = BloodProjAI.Normal;

    public int BulFram;

    private Vector2 Uoffset;

    public ref float Time => ref Projectile.ai[0];

    public override void SetDefaults()
    {
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.timeLeft = 400;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.damage = 100;
        Projectile.width = Projectile.height = 20;
    }

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.CanHitPastShimmer[Projectile.type] = true;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        overPlayers.Add(index);
        //base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
    }

    public override void OnKill(int timeLeft)
    {
        Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, Projectile.velocity, ModContent.GoreType<BloodProjGore>());
        Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, Projectile.velocity, ModContent.GoreType<BloodProjGore2>());
    }

    public override void AI()
    {
        //Projectile.velocity *= 0.9999f;

        StateMachine();
        Time++;

        if (Projectile.timeLeft == 1)
        {
            Projectile.Kill();
        }
    }

    private void StateMachine()
    {
        switch (CurrentState)
        {
            case BloodProjAI.Normal:
                HandleNormal();

                break;
            case BloodProjAI.Burrow:
                HandleBurrow();

                break;
        }
    }

    private void HandleNormal()
    {
        // Face the direction of travel
        Projectile.rotation = Projectile.velocity.ToRotation();

        // Base speed
        var baseSpeed = Projectile.velocity.Length();

        // Time-based wave calculation using GlobalTimeWrappedHourly for pause-safe animation
        var time = Main.GlobalTimeWrappedHourly + Projectile.whoAmI * 0.15f;

        // Sine wave parameters
        var amplitude = 8f; // How far it sways
        var frequency = 5f; // How fast it sways

        // Perpendicular direction to velocity
        var perpendicular = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);

        // Apply sine wave offset to velocity
        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * baseSpeed + perpendicular * ((float)Math.Sin(time * frequency) * (amplitude / 60f));
    }

    private void HandleBurrow()
    {
        if (Unfortunate != null)
        {
            Projectile.spriteDirection = Unfortunate.direction;
            Uoffset.X = Math.Abs(Uoffset.X) * Unfortunate.direction;
            Projectile.Center = Unfortunate.Center + Uoffset; //new Vector2(0, -Unfortunate.height / 2);
            Uoffset = Vector2.Lerp(Uoffset, Vector2.Zero, 0.006f);
            Projectile.rotation = Uoffset.ToRotation();
        }

        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity.X, Projectile.velocity.Y, 100, default, 1.5f);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        if (CurrentState == BloodProjAI.Normal)
        {
            CurrentState = BloodProjAI.Burrow;
            Time = 0;
            Unfortunate = target;
            Unfortunate.Calamity().DealDefenseDamage(info, 20);
            Uoffset = Projectile.Center - target.Center;
            info.Knockback = 0;
            target.RemoveAllIFrames();
        }
    }

    public override bool? CanDamage()
    {
        if (CurrentState == BloodProjAI.Burrow)
        {
            return false;
        }

        return base.CanDamage();
    }

    public override void PostAI() { }

    public override bool PreDraw(ref Color lightColor)
    {
        var value = (int)(Main.GlobalTimeWrappedHourly * 10.1f) % 3;
        var FrameCount = 7;

        var texture = TextureAssets.Projectile[Projectile.type].Value;
        var origin = new Vector2(texture.Width / 2, texture.Height / FrameCount / 2);
        var DrawPos = Projectile.Center - Main.screenPosition;

        var effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

        var sourceRect = new Rectangle(0, value * (texture.Height / FrameCount), texture.Width, texture.Height / FrameCount);

        // Base rotation
        var rotation = Projectile.rotation + MathHelper.PiOver2;

        var time = Main.GlobalTimeWrappedHourly + Projectile.whoAmI * 0.1f + Projectile.Center.X * 0.01f;

        var scale = Vector2.One;

        if (CurrentState == BloodProjAI.Burrow)
        {
            // Pulsate: oscillate scale between 0.9 and 1.1
            var pulsate = 1f + 0.1f * (float)Math.Sin(time * 5f);

            // Rock: oscillate rotation by +/- 5 degrees
            var rock = MathHelper.ToRadians(5f) * (float)Math.Sin(time * 3f);

            scale = new Vector2(pulsate, pulsate);
            rotation += rock;
        }

        Main.EntitySpriteDraw(texture, DrawPos, sourceRect, lightColor, rotation, origin, scale, effects);

        return false;
    }
}

public class SquidDebuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        //todo: lower stats and randomly take some damage
    }
}

public class BloodProjGore : ModGore
{
    public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/BigCrab/BloodprojGore1";

    public override void SetStaticDefaults() { }

    public override void OnSpawn(Gore gore, IEntitySource source)
    {
        base.OnSpawn(gore, source);
    }
}

public class BloodProjGore2 : BloodProjGore
{
    public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/BigCrab/BloodprojGore2";
}