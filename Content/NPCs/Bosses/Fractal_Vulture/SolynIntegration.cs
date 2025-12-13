using CalamityMod;
using HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Projectiles;
using HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Solyn;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.FirstPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles.SolynProjectiles;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm.AvatarOfEmptiness;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture
{
    partial class voidVulture
    {
        public Action<BattleSolynBird> SolynAction
        {
            get;
            set;
        }

        public void DecideSolynBehavior()
        {

                SolynAction = s => StandardSolynBehavior_FlyNearPlayer(s, NPC);

            if (currentState == Behavior.VomitCone)
            {

                SolynAction = s => ShieldPlayer_Behavior(s, NPC);
            }
            if (currentState == Behavior.CollidingCommet)
            {
                //SolynAction = s => EatCommets_WithFace_Behavior(s, currentTarget as Player, NPC);
            }
            if(currentState == Behavior.placeholder2)
            {
                SolynAction = s => GetImpaled_Behavior(s, currentTarget as Player, NPC);
            }
        }
        /// <summary>
        /// The amount of damage homing star bolts from Solyn do to enemies.
        /// </summary>
        public static int SolynHomingStarBoltDamage => GetAIInt("SolynHomingStarBoltDamage");

        // NOTE -- I at one point complained about this method and the one below needing an explicit static NPC instance, rather than just implicitly accessing the NPC argument for simpler syntax.
        // Sorry, past me, you didn't do anything wrong. This is necessary because this method is not just used by the AvatarOfEmptiness (Avatar's phase 2) ModNPC.
        // It is also used by the AvatarRift (Avatar's phase 1) ModNPC. Consequently, it must be static and require an explicit argument to account for both use cases across the different NPCs.

        // Could thereotically fuse the two mod NPCs like Calamity did a while ago, but I don't really see the point. It'd be a bunch of work for little tangible return.

        /// <summary>
        /// Instructs Solyn to fly near the player.
        /// </summary>
        public static void StandardSolynBehavior_FlyNearPlayer(BattleSolynBird solyn, NPC avatar)
        {
            NPC solynNPC = solyn.NPC;
            Player playerToFollow = solyn.Player;
            Vector2 lookDestination = playerToFollow.Center;
            Vector2 hoverDestination = playerToFollow.Center + new Vector2(solynNPC.HorizontalDirectionTo(playerToFollow.Center) * -66f, -10f);


            Vector2 force = solynNPC.SafeDirectionTo(hoverDestination) * LumUtils.InverseLerp(36f, 250f, solynNPC.Distance(hoverDestination)) * 0.8f;
            if (Vector2.Dot(solynNPC.velocity, solynNPC.SafeDirectionTo(hoverDestination)) < 0f)
            {
                solynNPC.Center = Vector2.Lerp(solynNPC.Center, hoverDestination, 0.02f);
                force *= 4f;
            }

            // Try to not fly directly into the ground.
            if (Collision.SolidCollision(solynNPC.TopLeft, solynNPC.width, solynNPC.height))
                force.Y -= 0.6f;

            // Try to avoid dangerous projectiles.
            Rectangle dangerCheckZone = Utils.CenteredRectangle(solynNPC.Center, Vector2.One * 450f);
            foreach (Projectile projectile in Main.ActiveProjectiles)
            {
                bool isThreat = projectile.hostile && projectile.Colliding(projectile.Hitbox, dangerCheckZone);
                if (!isThreat)
                    continue;

                float repelForceIntensity = Math.Clamp(300f / (projectile.Hitbox.Distance(solynNPC.Center) + 3f), 0f, 1.9f);
                force += projectile.SafeDirectionTo(solynNPC.Center) * repelForceIntensity;
            }

            solynNPC.velocity += force;

            solyn.UseStarFlyEffects();
            solynNPC.spriteDirection = (int)solynNPC.HorizontalDirectionTo(lookDestination);
            solynNPC.rotation = solynNPC.rotation.AngleLerp(0f, 0.3f);
        }

        /// <summary>
        /// Instructs Solyn to attack the Avatar.
        /// </summary>
        public static void StandardSolynBehavior_AttackAvatar(BattleSolynBird solyn)
        {
            NPC solynNPC = solyn.NPC;
            solyn.UseStarFlyEffects();

            int dashPrepareTime = 10;
            int dashTime = 4;
            int waitTime = 12;
            int slowdownTime = 11;
            int wrappedTimer = solyn.AITimer % (dashPrepareTime + dashTime + waitTime + slowdownTime);

            NPC target = Myself is null ? AvatarRift.Myself : Myself;

            if (target is null)
            {
                StandardSolynBehavior_FlyNearPlayer(solyn, target);
                return;
            }

            float accelerationFactor = 1f;
            Vector2 destination = target.type == ModContent.NPCType<voidVulture>() ? target.As<voidVulture>().HeadPos : target.Center;

            // Prepare for the dash, drifting towards the Avatar at an accelerating pace.
            if (wrappedTimer <= dashPrepareTime)
            {
                if (wrappedTimer == 1)
                    solynNPC.oldPos = new Vector2[solynNPC.oldPos.Length];

                solynNPC.velocity += solynNPC.SafeDirectionTo(destination) * wrappedTimer * accelerationFactor / dashPrepareTime * 8f;
            }

            // Initiate the dash.
            else if (wrappedTimer <= dashPrepareTime + dashTime)
            {
                if (Vector2.Dot(solynNPC.velocity, solynNPC.SafeDirectionTo(destination)) < 0f)
                    solynNPC.velocity *= 0.75f;
                else
                    solynNPC.velocity *= 1.67f;
                solynNPC.velocity = solynNPC.velocity.ClampLength(0f, 100f);
            }

            // Fly upward after the dash has reached its maximum speed, releasing homing star bolts.
            else if (wrappedTimer <= dashPrepareTime + dashTime + waitTime)
            {
                solynNPC.velocity.Y -= 4f;

                if (Main.netMode != NetmodeID.MultiplayerClient && !solyn.IsMultiplayerClone)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 boltVelocity = Main.rand.NextVector2Circular(16f, 16f);
                        Projectile.NewProjectile(solynNPC.GetSource_FromAI(), solynNPC.Center, boltVelocity, ModContent.ProjectileType<HomingStarBolt>(), SolynHomingStarBoltDamage, 0f, -1, 0f, 0f, 1f);
                    }
                }
            }

            // Slow down after the dash.
            else
                solynNPC.velocity *= 0.76f;

            // SPIN
            if (wrappedTimer <= dashPrepareTime || wrappedTimer >= dashPrepareTime + dashTime + waitTime)
                solynNPC.rotation = solynNPC.rotation.AngleLerp(solynNPC.velocity.X * 0.0097f, 0.21f);
            else
                solynNPC.rotation += solynNPC.spriteDirection * MathHelper.TwoPi * 0.18f;

            // Decide Solyn's direction.
            if (Math.Abs(solynNPC.velocity.X) >= 1.3f)
                solynNPC.spriteDirection = solynNPC.velocity.X.NonZeroSign();

            // Make afterimages stronger than usual.
            solyn.AfterimageCount = 14;
            solyn.AfterimageClumpInterpolant = 0.5f;
            solyn.AfterimageGlowInterpolant = 1f;
        }




        public static void ShieldPlayer_Behavior(BattleSolynBird solyn, NPC npc)
        {
            NPC solynNPC = solyn.NPC;
            Player playerToProtect = solyn.Player;


            if (playerToProtect is null)
            {
                // Fallback if player reference is missing
                StandardSolynBehavior_FlyNearPlayer(solyn, npc);
                return;
            }

            solyn.UseStarFlyEffects();

            // 3. Compute direction from player to boss (npc)
            Vector2 toBoss = npc.Center - solynNPC.Center;
            Vector2 dirToBoss;
            if (toBoss.LengthSquared() < 0.0001f)
                dirToBoss = new Vector2(1f, 0f);
            else
                dirToBoss = Vector2.Normalize(toBoss);
            if (voidVulture.Myself.As<voidVulture>().Time == 0)
                npc.As<voidVulture>().SolynChosenShield = dirToBoss.RotatedByRandom(MathHelper.TwoPi)*100;
            // 4. Hover destination in front of the player, slightly above
            float desiredDistance = 86f;
            Vector2 hoverDestination = playerToProtect.Center + dirToBoss * desiredDistance + new Vector2(0f, -12f);

            // 5. Smooth steering force toward hover destination
            Vector2 toDest = solynNPC.SafeDirectionTo(hoverDestination);
            float distanceToDest = solynNPC.Distance(hoverDestination);
            Vector2 force = toDest * LumUtils.InverseLerp(24f, 300f, distanceToDest) * 0.9f;

            if (Vector2.Dot(solynNPC.velocity, solynNPC.SafeDirectionTo(hoverDestination)) < 0f)
            {
                // Nudge Solyn toward destination if moving the wrong way
                //solynNPC.Center = Vector2.Lerp(solynNPC.Center, hoverDestination, 0.02f);
                //force *= 3.5f;
            }

            // Avoid sinking into solids
            if (Collision.SolidCollision(solynNPC.TopLeft, solynNPC.width, solynNPC.height))
                force.Y -= 0.6f;

            //if (voidVulture.Myself.As<voidVulture>().Time <= voidVulture.VomitCone_ShootStart)
            solyn.NPC.Center = npc.Center - npc.As<voidVulture>().SolynChosenShield;
            //solynNPC.velocity += force;
            solyn.NPC.velocity = npc.velocity;

            // Visual alignment
            solynNPC.spriteDirection = (int)solynNPC.HorizontalDirectionTo(npc.Center);
            solynNPC.rotation = solynNPC.rotation.AngleLerp(dirToBoss.ToRotation(), 0.22f);

            bool shouldSpawnShield = playerToProtect.ownedProjectileCounts[ModContent.ProjectileType<DirectionalSolynForcefield3>()] < 1 && npc.As<voidVulture>().Time > 40;
            if (Main.netMode != NetmodeID.MultiplayerClient && !solyn.IsMultiplayerClone)
            {
                // Spawn at a single tick in the cooldown window to avoid duplicates
                if (shouldSpawnShield)
                {
                    Vector2 spawnPosition = solynNPC.Center + dirToBoss * 18f;
                    float projectileRotation = dirToBoss.ToRotation();

                    int projType = ModContent.ProjectileType<DirectionalSolynForcefield3>();
                    // Damage 0, knockback 0, owner -1 (NPC-sourced). Pass rotation in ai0 and target npc.whoAmI in ai1.
                    Projectile Shield = Projectile.NewProjectileDirect(
                        solynNPC.GetSource_FromAI(),
                        spawnPosition,
                        Vector2.Zero,
                        projType,
                        0,
                        0f,
                        -1,
                        projectileRotation,
                        npc.whoAmI
                    );
                    Shield.As<DirectionalSolynForcefield3>().Solyn = solyn.NPC;
                }
            }

            // 8. Visual tuning for protective stance
            solyn.AfterimageCount = 8;
            solyn.AfterimageClumpInterpolant = 0.6f;
            solyn.AfterimageGlowInterpolant = 0.9f;
            solyn.BackglowScale = 0.9f;
        }

        public static void GetImpaled_Behavior(BattleSolynBird solyn,Player player, NPC npc)
        {
            if (npc.As<voidVulture>().Time < 40)
            {
                npc.As<voidVulture>().StoredSolynPos = solyn.NPC.Center;
            }
            foreach (var proj in Main.ActiveProjectiles)
            {
                if (proj.type == ModContent.ProjectileType<SeekingEnergy>())
                {
                   
                    int val = (int)(40 * (1 - LumUtils.InverseLerp(0, 400, npc.As<voidVulture>().Time))) + 4;
                    if (solyn.AITimer % val == 0)
                    {
                        if(Main.rand.NextBool(2))
                        SoundEngine.PlaySound(GennedAssets.Sounds.Solyn.Speak with { PitchVariance = 0.2f});

                        SoundEngine.PlaySound(GennedAssets.Sounds.Solyn.ForcefieldHit);
                    }
                    solyn.NPC.Center = Vector2.Lerp(npc.As<voidVulture>().StoredSolynPos, npc.As<voidVulture>().HeadPos, npc.As<voidVulture>().ReelSolynInterpolant);
                    solyn.NPC.Center += Main.rand.NextVector2Unit();
                    if (player.ownedProjectileCounts[ModContent.ProjectileType<SolynBarrier>()] < 1)
                    {
                        Projectile a = Projectile.NewProjectileDirect(solyn.NPC.GetSource_FromThis(), solyn.NPC.Center, Vector2.Zero, ModContent.ProjectileType<SolynBarrier>(), 0, 0);
                        a.As<SolynBarrier>().Solyn = solyn.NPC;
                    }
                    break;
                }
             
            }

            if(npc.As<voidVulture>().Time > 400)
            {
                solyn.NPC.velocity = Vector2.Zero;
                solyn.NPC.Center = npc.As<voidVulture>().HeadPos;
            }

        }
       
    }
}
