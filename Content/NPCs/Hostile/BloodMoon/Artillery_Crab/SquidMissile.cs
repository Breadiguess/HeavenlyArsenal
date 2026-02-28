using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab;
using Luminance.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Artillery_Crab
{
    public class SquidMissile : ModProjectile
    {

        public const int LaunchTime = 60;

        public int Time
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public bool HasImpacted;

        private float LaunchSpeed => 19f;
        private float CruiseSpeed => 16f;
        private float TurnLerp => 0.14f;
        private float Gravity => 0.35f;
        private float MaxSpeed => 22f;
        private int StartFallingBy => LaunchTime * 4;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 30;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.friendly = false;

            Projectile.timeLeft = 600;
            Projectile.ignoreWater = true;

            Projectile.tileCollide = false;
            Projectile.damage = 200;

            Projectile.width = Projectile.height = 20;
            Projectile.maxPenetrate = -1;
            Projectile.penetrate = -1;

        }

        public override void OnSpawn(IEntitySource source)
        {
            Time = -1;
            // Fire upward by default. Add tiny sideways bias so multiple missiles don't overlap perfectly.
            float sideways = Main.rand.NextFloat(-2f, 2f);
            Projectile.velocity = new Vector2(sideways, -LaunchSpeed);

            for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Type]; i++)
            {
                Projectile.oldPos[i] = Projectile.Center - Projectile.Size/2;
            }
            Projectile.netUpdate = true;
        }

        private bool TryGetTarget(out Vector2 targetPos)
        {
            int idx = (int)Projectile.ai[1];
            if (idx >= 0 && idx < Main.maxPlayers)
            {
                Player p = Main.player[idx];
                if (p != null && p.active && !p.dead)
                {
                    targetPos = p.Center;
                    return true;
                }
            }

            targetPos = Projectile.Center + Vector2.UnitY * 200f;
            return false;
        }

        public override void AI()
        {
            if (Time < StartFallingBy)
            {
                Projectile.oldPos[0] = Projectile.position;
                float wobble = (float)Math.Sin(Time * 0.18f + Projectile.whoAmI) * 0.08f;
                Projectile.velocity = Projectile.velocity.RotatedBy(wobble);

            }

            Time++;
            if (HasImpacted)
            {
                Projectile.velocity *= 0.9f;
                return;
            }

            bool launching = Time < LaunchTime;

            if (!launching && !Projectile.tileCollide)
            {
                Projectile.tileCollide = true;
                Projectile.netUpdate = true;
            }

            // Target (player)
            TryGetTarget(out Vector2 target);

            if (launching)
            {
                // Keep going mostly up; gently drift “out”
                Projectile.velocity.Y = MathHelper.Lerp(Projectile.velocity.Y, -LaunchSpeed, 0.08f);

                // If you want it to “peel away” from the NPC, add a slight outward push based on direction to target
                Vector2 toTarget = (target - Projectile.Center);
                float outward = Math.Sign(toTarget.X); // left/right
                Projectile.velocity.X = MathHelper.Lerp(Projectile.velocity.X, outward * 4f, 0.03f);
            }
            else if(Time < StartFallingBy)
            {
                // Phase B: steer toward target, then apply gravity => arc then dive.
                Vector2 desiredDir = (target - Projectile.Center).SafeNormalize(Vector2.UnitY);
                Vector2 desiredVel = desiredDir * CruiseSpeed;

                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, TurnLerp);

                // Gravity gives the ballistic “drop”
                Projectile.velocity.Y += Gravity;

                // Clamp speed so it doesn’t go insane during long chases
                float spd = Projectile.velocity.Length();
                if (spd > MaxSpeed)
                    Projectile.velocity *= MaxSpeed / spd;
            }
            else
            {
                if(Time == StartFallingBy + 1)
                {
                    SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.ArmJutOut with { Volume = 0.2f, Pitch = -1 }, Projectile.Center);
                }
                Projectile.velocity.X *= 0.91f;
                Projectile.velocity.Y += Gravity;
            }

            if (Projectile.velocity.LengthSquared() > 0.001f)
                Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            HasImpacted = true;
            Projectile.velocity = Vector2.Zero;
            Projectile.timeLeft = 60;
            Projectile.damage = -1;

            Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, Projectile.velocity, ModContent.GoreType<BloodProjGore>());
            Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, Projectile.velocity, ModContent.GoreType<BloodProjGore2>());

            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;

            if (Projectile.oldPos == null || Projectile.oldPos.Length < 3)
                return false;


            for (int i = 0; i < Projectile.oldPos.Length - 3; i++)
            {
                NoxusBoss.Core.Utilities.Utilities.DrawLineBetter(Main.spriteBatch, Projectile.oldPos[i] + Projectile.Size/2, Projectile.oldPos[i + 1] + Projectile.Size / 2, Color.Crimson * (1 - i / (float)Projectile.oldPos.Length), 3 * (1 - i / (float)Projectile.oldPos.Length));
            }

            Rectangle frame = tex.Frame(1, 7, 0, frameY: Time%7);
            if(!HasImpacted)
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation + MathHelper.PiOver2, frame.Size() / 2f, 1, 0); 
            return false;
        }
    }
}