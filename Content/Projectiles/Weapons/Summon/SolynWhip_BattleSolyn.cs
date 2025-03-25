using CalamityMod;
using HeavenlyArsenal.Content.Buffs;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles.SolynProjectiles;
using NoxusBoss.Content.NPCs.Friendly;
using NoxusBoss.Content.Particles;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;


namespace HeavenlyArsenal.Content.Projectiles.Weapons.Summon;
public class SolynWhip_BattleSolyn : ModProjectile
{
    public static int SolynHomingStarBoltDamage = 500; // Set the appropriate damage value

    public override void SetDefaults()
    {
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.width = 35;
        Projectile.height = Projectile.width;
        Projectile.aiStyle = -1;    
    }
    public override void SetStaticDefaults()
    {
        //ProjectileID.Sets.TrailingMode[Type] = Solyn.
        //ProjectileID.Sets.TrailCacheLength[Type];

    }
    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        if (!player.active || player.dead)
         {
             //Main.NewText($"I should be dead!", Color.AntiqueWhite);
             Projectile.Kill();
              return;
          }
        
         if (!CheckActive(player))
         {
                return;
         }
              


        DelegateMethods.v3_1 = new Vector3(0.3f, 0.367f, 0.45f) * 0.8f;
        Projectile.timeLeft = 24; // Keeps the minion alive
        Projectile.ai[0]++; // Increment the timer

        // Handle disengagement logic when too far from player
        bool isAttacking = HasTarget(out Vector2 targetPosition);

        if (Projectile.Distance(player.Center) > 1200f)
        {
            if (isAttacking)
            {
       
                isAttacking = false; // Stop attacking
                Projectile.ai[0] = 0;
            }
           
        }

        if (isAttacking)
        {
         
            AttackBehavior(targetPosition);
        }
        else
        {
           
            FlyNearPlayer(player);
        }

        Visuals();
    }

    private void FlyNearPlayer(Player player)
    {
       
        // Define the hover destination relative to the player
        Vector2 hoverDestination = player.Center + new Vector2(Projectile.HorizontalDirectionTo(player.Center) * -66f, 10f);
        Vector2 lookDestination = player.Center;

        
        // Calculate movement force toward the hover destination
        Vector2 force = Projectile.SafeDirectionTo(hoverDestination) * InverseLerp(36f, 250f, Projectile.Distance(hoverDestination)) * 0.8f;

       
        if (Vector2.Dot(Projectile.velocity, Projectile.SafeDirectionTo(hoverDestination)) < 0f)
        {
            Projectile.Center = Vector2.Lerp(Projectile.Center, hoverDestination, 0.02f);
            force *= 4f;
        }

        // Avoid flying directly into solid ground
        if (Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
        {
            Main.NewText("Avoiding collision with the ground!", Color.Red);
            force.Y -= 0.6f;
        }

        // Avoid hostile projectiles in the area
        Rectangle dangerCheckZone = Utils.CenteredRectangle(Projectile.Center, Vector2.One * 450f);
        foreach (Projectile projectile in Main.ActiveProjectiles)
        {
            bool isThreat = projectile.hostile && projectile.Colliding(projectile.Hitbox, dangerCheckZone);
            if (!isThreat)
                continue;

            float repelForceIntensity = MathHelper.Clamp(300f / (projectile.Hitbox.Distance(Projectile.Center) + 3f), 0f, 1.9f);
            force += projectile.SafeDirectionTo(Projectile.Center) * repelForceIntensity;
        }

        // Apply the calculated force to the projectile velocity
        Projectile.velocity += force;
        Projectile.velocity = Projectile.velocity.ClampLength(0f, 10f); // Cap max speed to avoid instability

        // Update visuals for rotation and sprite direction
        Projectile.spriteDirection = (int)Projectile.HorizontalDirectionTo(lookDestination);
        Projectile.rotation = Projectile.rotation.AngleLerp(0f, 0.3f);
    }
    private void AttackBehavior(Vector2 targetPosition)
    {
        
        // Behavior timing variables
        int dashPrepareTime = 10;
        int dashTime = 4;
        int waitTime = 12;
        int slowdownTime = 11;
        int wrappedTimer = (int)(Projectile.ai[0] % (dashPrepareTime + dashTime + waitTime + slowdownTime));
        Projectile.timeLeft = 400;
        // Prepare for the dash
        if (wrappedTimer <= dashPrepareTime)
        {
            if (wrappedTimer == 1)
            {

            }

            float accelerationFactor = wrappedTimer / (float)dashPrepareTime;
            Projectile.velocity += Projectile.SafeDirectionTo(targetPosition) * accelerationFactor * 8f;
            // Main.NewText($"Preparing to dash toward target. OldPos:{Projectile.oldPos[0]}", Color.Orange);
        }
        // Dash toward the target
        else if (wrappedTimer <= dashPrepareTime + dashTime)
        {
            Main.NewText($"Dashing toward target! Target Location: {targetPosition}", Color.LightBlue);
            Projectile.velocity *= 1.67f;
            Projectile.velocity = Projectile.velocity.ClampLength(0f, 50f); 
        }
        //hopes and prayers
        else if (wrappedTimer <= dashPrepareTime + dashTime + waitTime)
        {
            Projectile.velocity.Y -= 0.4f; // Apply upward motion
            if (Main.netMode != NetmodeID.MultiplayerClient && wrappedTimer % 6 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 boltVelocity = Main.rand.NextVector2Circular(16f, 16f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, boltVelocity,
                        ModContent.ProjectileType<HomingStarBolt>(), SolynHomingStarBoltDamage, 0f, Main.myPlayer);
                }
                // Main.NewText($"Fired homing star bolts!", Color.Magenta);
            }
        }
        //somewhere in here it breaks but i have NO IDEA ASIUDHFASUHFIAS

        else
        {
          


            {
                

                if (Projectile.velocity.Length() > 0)
                {
                    Main.NewText($"Slowing down after dash. {Projectile.velocity}", Color.Gray);
                    Projectile.velocity *= 0.76f; // Gradually reduce velocity
                }
            }

        }
        if (wrappedTimer <= dashPrepareTime || wrappedTimer >= dashPrepareTime + dashTime + waitTime)
            Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.velocity.X * 0.0097f, 0.21f);
        else
            Projectile.rotation += Projectile.spriteDirection * MathHelper.TwoPi * 0.18f;
        //Projectile.spriteDirection = (int)Projectile.velocity.X.NonZeroSign(); // Flip sprite based on movement direction
    }
    private bool CheckActive(Player owner)
    {
        if (owner.dead || !owner.active)
        {
            owner.ClearBuff(ModContent.BuffType<SolynWhip_Onhit_Buff>());
            return false;
        }

        if (owner.HasBuff(ModContent.BuffType<SolynWhip_Onhit_Buff>()))
        {
            Projectile.timeLeft = 2;
        }

        return true;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        return false;
    }
    private NPC currentTarget; 
    private int targetLockTimer;

    private bool HasTarget(out Vector2 targetPosition)
    {
        Player player = Main.player[Projectile.owner];

        // Maintain the current target for a fixed duration (e.g., 60 frames)
        if (currentTarget != null && currentTarget.active && !currentTarget.dontTakeDamage && Projectile.Distance(currentTarget.Center) < 700f)
        {
            targetLockTimer++;
            if (targetLockTimer >= 60)
            {
                currentTarget = null;
                targetLockTimer = 0;
            }
            else
            {
                targetPosition = currentTarget.Center;
                return true; 
            }
        }

        // Reset the current target if it becomes invalid
        if (currentTarget == null || !currentTarget.active || currentTarget.dontTakeDamage || Projectile.Distance(currentTarget.Center) >= 700f)
        {
            currentTarget = null;
            targetLockTimer = 0;
        }

        // Check for player's explicit target (priority)
        if (player.HasMinionAttackTargetNPC)
        {
            NPC target = Main.npc[player.MinionAttackTargetNPC];
            if (target.CanBeChasedBy() && Projectile.Distance(target.Center) < 700f)
            {
                currentTarget = target; // Lock on to player's designated target
                targetPosition = target.Center;
                //Main.NewText($"Locked on to player's target!", Color.Cyan);
                return true;
            }
        }

        // Check for nearest valid NPC (fallback)
        float maxDistance = 700f;
        targetPosition = Vector2.Zero;
        foreach (NPC npc in Main.npc)
        {
            if (npc.CanBeChasedBy() && Projectile.Distance(npc.Center) < maxDistance)
            {
                maxDistance = Projectile.Distance(npc.Center);
                currentTarget = npc;
                targetPosition = npc.Center;
            }
        }

        if (currentTarget != null)
        {
            //Main.NewText($"Locked on to nearest target!", Color.Cyan);
            return true; // Acquired a new target
        }

        //Main.NewText($"No valid target found.", Color.Yellow);
        return false; // No target
    }

    private void Visuals()
    {
        UseStarFlyEffects();
        if (Projectile.velocity.X > 0f)
        {
            
            Projectile.spriteDirection = -1;
        }
        else if (Projectile.velocity.X < 0f)
        {
            
            Projectile.spriteDirection = 1;
        }
    }
    public void UseStarFlyEffects()
    {
        // Release star particles.
        int starPoints = Main.rand.Next(3, 9);
        float starScaleInterpolant = Main.rand.NextFloat();
        int starLifetime = (int)MathHelper.Lerp(11f, 30f, starScaleInterpolant);
        float starScale = MathHelper.Lerp(0.2f, 0.4f, starScaleInterpolant) * Projectile.scale;
        Color starColor = Color.Lerp(new Color(1f, 0.41f, 0.51f), new Color(1f, 0.85f, 0.37f), Main.rand.NextFloat());
        
        Vector2 starSpawnPosition = Projectile.Center + new Vector2(Projectile.spriteDirection * 20f, 8f) + Main.rand.NextVector2Circular(16f, 16f);


        Vector2 starVelocity = Main.rand.NextVector2Circular(3f, 3f) + Projectile.velocity;
        TwinkleParticle star = new TwinkleParticle(starSpawnPosition, starVelocity, starColor, starLifetime, starPoints, new Vector2(Main.rand.NextFloat(0.4f, 1.6f), 1f) * starScale, starColor * 0.5f);
        star.Spawn();

    }



    public override bool MinionContactDamage()
    {
        return false;
    }
    public override void OnKill(int timeLeft)
    {
        Main.NewText($"Solyn was killed! position: {Projectile.position}, velocty: {Projectile.velocity}", Color.AntiqueWhite);
        
    }




    public override bool PreDraw(ref Color lightColor)
    {
        SpriteEffects direction = Projectile.spriteDirection.ToSpriteDirection() ^ SpriteEffects.FlipHorizontally;
        SpriteEffects spriteEffects = SpriteEffects.None;
        Color glowmaskColor = Color.White;


        Vector2 drawPosition = Projectile.Center- Main.screenPosition+ Vector2.UnitY;
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        {

            Rectangle frame = texture.Frame(1, 1, 0, Projectile.frame);
            Main.spriteBatch.PrepareForShaders();

            glowmaskColor = new(255, 178, 97);
            //drawColor = glowmaskColor;

            ManagedShader soulShader = ShaderManager.GetShader("NoxusBoss.SoulynShader");
            soulShader.TrySetParameter("outlineOnly",false);
            soulShader.TrySetParameter("imageSize", texture.Size());
           //ectangle.
            soulShader.TrySetParameter("sourceRectangle", new Vector4(0, 0, texture.Width, texture.Height));
            //Main.NewText($"Rendering ghost solyn!! as: {new Vector4(Projectile.frame, Projectile.frame, texture.Width, texture.Height)}", Color.AntiqueWhite);
            soulShader.Apply();
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,frame, lightColor, Projectile.rotation, drawPosition, Projectile.scale, spriteEffects, 0);
            //Main.EntitySpriteDraw(GennedAssets.Textures.Friendly.SolynGlow.Value, Projectile.Center, frame, Projectile.GetAlpha(glowmaskColor) * 0.26f, Projectile.rotation, frame.Size() * 0.5f, EffectiveScale, direction);

            Main.spriteBatch.ExitShaderRegion();
        } 
        return true;
    }
}