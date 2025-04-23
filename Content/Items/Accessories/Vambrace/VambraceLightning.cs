using System;
using System.Collections.Generic;
using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Accessories.Vambrace
{
    public class VambraceLightning : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        private const int MaxJumps = 5;
        private const float JumpRange = 400f;

        // Tracks which NPCs have been hit by this instance
        public List<int> hitNPCs;
        // Tracks the projectile index of the parent that spawned this instance
        public int ParentProjID => (int)Projectile.localAI[1];

        public ref float Time => ref Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.AllowsContactDamageFromJellyfish[Type] = true;
            ProjectileID.Sets.TrailingMode[Type] = 1;
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.DontApplyParryDamageBuff[Type] = true;
            ProjectileID.Sets.NoMeleeSpeedVelocityScaling[Type] = true;
            
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            // Initialize lists
            hitNPCs = new List<int>();
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            // On first tick, ensure hit list exists
            if (hitNPCs == null && Time == 0)
            {
                hitNPCs = new List<int>();
                
            }
               

            // Add some electric dust
            Dust.NewDustPerfect(Projectile.Center, DustID.Electric, new Vector2(0, 0), 100, Color.AntiqueWhite, 1f);
            //Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, 0f, 0f, 100, default, 1.5f);
            Lighting.AddLight(Projectile.Center, 0.1f, 0.4f, 0.7f);
            Projectile.velocity = Vector2.Zero;

            NPC nextTarget = null;
            float closestDist = JumpRange;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.life > 0)
                {
                    float dist = Vector2.Distance(npc.Center, Projectile.Center);
                    if (dist < closestDist && !hitNPCs.Contains(npc.whoAmI))
                    {
                        closestDist = dist;
                        nextTarget = npc;
                    }
                    else
                    {
                        Projectile.Kill();
                    }
                }

            }
            Time++;
        }


        public override void OnSpawn(IEntitySource source)
        {
            Main.NewText($"Lightning spawned: {Projectile.whoAmI}, by:{Projectile.GetSource_FromThis}");
            base.OnSpawn(source);
        }
        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);
        }


        public override bool? CanCutTiles() => false;

        public override bool? CanHitNPC(NPC target)
        {
            if (hitNPCs.Contains(target.whoAmI))
                return false;
            return true ;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int currentJumps = (int)Projectile.ai[0];

            // Record hit
            if (!hitNPCs.Contains(target.whoAmI))
                hitNPCs.Add(target.whoAmI);
            if (currentJumps >= MaxJumps)
            {
                Projectile.Kill();
                return;
            }

            // Find next target
            NPC nextTarget = null;
            float closestDist = JumpRange;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.life > 0)
                {
                    float dist = Vector2.Distance(npc.Center, Projectile.Center);
                    if (dist < closestDist && !hitNPCs.Contains(npc.whoAmI))
                    {
                        closestDist = dist;
                        nextTarget = npc;
                    }
                }
            }
            if (nextTarget == null)
            {
                Projectile.Kill();
                return;
            }

            // Otherwise, chain to next target
            Projectile.ai[0] = currentJumps + 1;

            Vector2 direction = nextTarget.Center - Projectile.Center;
            direction.Normalize();
            direction *= 12f;
            int newProj = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                direction,
                Type,
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner,
                Projectile.ai[0], 
                Projectile.whoAmI
            );
            if (Main.projectile[newProj].ModProjectile is VambraceLightning vd)
            {
                vd.hitNPCs = new List<int>(hitNPCs);
            }
            Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D BloomCircleSmall = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Extra/GreyscaleTextures/BloomCircleSmall").Value;
            float scaleFactor = Projectile.width / 50f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + Projectile.velocity;
            Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, Projectile.GetAlpha(Color.AntiqueWhite) with { A = 0 } * 0.2f, 0f, BloomCircleSmall.Size() * 0.5f, scaleFactor * 1.2f, 0, 0f);
            return false;
        }
    }
}
