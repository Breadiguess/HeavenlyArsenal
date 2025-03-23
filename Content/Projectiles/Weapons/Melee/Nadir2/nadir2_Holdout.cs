using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using rail;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Melee.Nadir2;
public class nadir2_Holdout : ModProjectile
{
    private int attackState = 0; // Tracks the current attack state
    private int attackCooldown = 0; // Cooldown between attacks
    private bool isThrusting = false; // Tracks whether the spear is thrusting
    private float thrustProgress = 0f; 
    private const float thrustDistance = 120f; // Distance the spear moves forward during thrust
    private const float thrustRecoverySpeed = 0.1f; // Speed of thrust recovery
    private const float maxIdleDistance = 15f; // Maximum distance from the player when not stabbing
    private const float rotationEaseSpeed = 0.45f; // Speed of rotation easing (lower = slower)

    public override void SetDefaults()
    {
        Projectile.width = 64;
        Projectile.height = 64;
        Projectile.penetrate = -1; // Unlimited penetration
        Projectile.timeLeft = 2; // Keeps the projectile alive
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.friendly = true;
        Projectile.hostile = false;
    }

    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        Projectile.timeLeft = 2; // Prevent despawning

        // Create dust at the sprite's origin point
        CreateDustAtOrigin();

        // Update spear position and clamp distance when not thrusting
        UpdateProjectileHeldVariables(player.Center);

        // Manipulate player animation and arm positions
        ManipulatePlayerVariables(player);

        // Handle attack logic
        if (Main.mouseLeft && attackCooldown == 0)
        {
            PerformAttack(player);
            attackCooldown = 30; // Set cooldown for attacks
        }

        // Reduce attack cooldown each tick
        if (attackCooldown > 0)
        {
            attackCooldown--;
        }

        // Update thrust progress and transition out of thrust state
        if (isThrusting)
        {
            
            thrustProgress += thrustRecoverySpeed;
            if (thrustProgress >= 1f)
            {
                thrustProgress = 0f;
                isThrusting = false;
            }
        }
    }
     
    private void CreateDustAtOrigin()
    {
        // Create dust at the center of the sprite
        Vector2 dustPosition = Projectile.Center;
        Dust dust = Dust.NewDustDirect(dustPosition, 1, 1, DustID.Smoke, 0f, 0f, 150, Color.White, 1f);
        dust.velocity *= 0.3f; // Slow the dust movement
        dust.noGravity = true; // Make the dust float
    }

    public void UpdateProjectileHeldVariables(Vector2 armPosition)
    {
        // Manually calculate the normalized direction to the mouse
        Vector2 directionToMouse = Main.MouseWorld - Projectile.Center;
        if (directionToMouse.Length() > 0f) // Avoid division by zero
        {
            directionToMouse.Normalize(); // Converts to a unit vector
        }
        else
        {
            directionToMouse = Vector2.UnitX; // Default direction if the length is zero
        }

        // Adjust spear velocity why is it adjusting velocity, shouldn't it be rotation?
        if (Main.myPlayer == Projectile.owner)
        {
            float aimInterpolant = Utils.GetLerpValue(10f, 40f, Projectile.Distance(Main.MouseWorld), true);
            Vector2 oldVelocity = Projectile.velocity;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, directionToMouse, aimInterpolant);
            if (Projectile.velocity != oldVelocity)
            {
                Projectile.netSpam = 0;
                Projectile.netUpdate = true;
            }
        }

        // Calculate thrust offset to simulate thrust mechanics
        Vector2 thrustOffset = Vector2.Zero;

        
        if (isThrusting)
        {
            //  Projectile.damage = Projectile.GetSource_FromThis(Item.dam;
            Projectile.damage = 10000;
            thrustOffset = Projectile.velocity * thrustDistance * thrustProgress;
        }
        else
        {
            Projectile.damage = 0;
        }
        //Main.player = Player
            // Adjust target position with clamping
            Vector2 targetPosition = armPosition - Projectile.Size * 0.4f + Projectile.velocity * 34f + thrustOffset;
        //Projectile.velocity = Vector2.Lerp(Projectile.velocity, Player.MountedCenter.DirectionTo(Main.MouseWorld), 0.1f);
        Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;
        // Clamp distance when not stabbing
        if (!isThrusting)
        {
            float distanceToPlayer = Vector2.Distance(armPosition, targetPosition);
            if (distanceToPlayer > maxIdleDistance)
            {
                Vector2 clampedOffset = Vector2.Normalize(targetPosition - armPosition) * maxIdleDistance;
                targetPosition = armPosition + clampedOffset;
            }
        }

        // Smoothly adjust the spear's position

        Projectile.Center = Vector2.Lerp(Projectile.Center, targetPosition + new Vector2( MathHelper.ToRadians(45f)), 0.9f);

        // Ease the rotation toward the mouse using Utils.AngleLerp
        float targetRotation = (Main.MouseWorld - Projectile.Center).ToRotation(); 
        Projectile.rotation = Projectile.rotation.AngleLerp(targetRotation, rotationEaseSpeed);

        // Update sprite direction
        Projectile.spriteDirection = Projectile.direction;
    }

    public void ManipulatePlayerVariables(Player player)
    {
        player.ChangeDir(Projectile.spriteDirection);
        player.heldProj = Projectile.whoAmI;

        // Adjust player's front and back arm rotations
        float frontArmRotation = Projectile.rotation;
        if (player.direction == -1)
        {
            frontArmRotation += MathHelper.PiOver2+frontArmRotation;
        }
        else
        {
            frontArmRotation = MathHelper.PiOver2 - frontArmRotation;
        }

        frontArmRotation += Projectile.rotation + MathHelper.Pi + player.direction * MathHelper.PiOver2 + 0.12f;
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, frontArmRotation);
        player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
        CreateLantern();
    }




    private Vector2 GetLanternAnchor()
    {
        //base offset (30, -30) then rotate it by the projectile's rotation.
      
        Vector2 baseOffset = new Vector2(50, 0);
        baseOffset = baseOffset.RotatedBy(Projectile.rotation);
        return Projectile.Center + baseOffset;
    }

    public void CreateLantern()
    {
        int lanternType = ModContent.ProjectileType<nadir2Lantern>();
        Vector2 specifiedAnchor = GetLanternAnchor();
        bool lanternExists = false;

        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile proj = Main.projectile[i];
            if (proj.active && proj.owner == Projectile.owner && proj.type == lanternType)
            {
                // Update Anchor
                proj.ai[0] = specifiedAnchor.X;
                proj.ai[1] = specifiedAnchor.Y;
                proj.netUpdate = true;
                lanternExists = true;
                break;
            }
        }

        if (!lanternExists)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(),
            specifiedAnchor,
            Vector2.Zero,
            lanternType,
            -1,
            -1,
            Projectile.owner,
            specifiedAnchor.X,
            specifiedAnchor.Y);
        }
    }

    public bool IsEmpowered = false;


    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        IsEmpowered = true;
        base.OnHitNPC(target, hit, damageDone);
    }
    public override bool PreDraw(ref Color lightColor)
    {


        Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        Texture2D Empowered = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Projectiles/Weapons/Melee/Nadir2/Nadir2Upgrade_Empowered").Value;  



        // Declare the origin variable outside the if-else scope
        Vector2 origin;

        float rotation = Projectile.rotation;
        SpriteEffects direction = SpriteEffects.None;

        // Adjust origin and flipping based on spriteDirection
        if (Projectile.spriteDirection < 0f)
        {
            direction = SpriteEffects.FlipVertically;
            rotation -= MathHelper.PiOver4; // Adjust rotation if flipped
            origin = new Vector2(77, 77); // Origin for flipped direction
        }
        else
        {
            origin = new Vector2(76, 122); // Origin for normal direction
            rotation += MathHelper.PiOver4;
        }

        Vector2 drawPosition = Projectile.Center - new Vector2(0,-10) - Main.screenPosition;
        if(IsEmpowered)
        {
            Main.spriteBatch.Draw(
                Empowered,
                drawPosition,
                null,
                Projectile.GetAlpha(lightColor),
                rotation,
                origin,
                Projectile.scale,
                direction,
                0f
            );
        }
        else
        {

        
        // Draw the projectile
        Main.spriteBatch.Draw(
            texture,
            drawPosition,
            null,
            Projectile.GetAlpha(lightColor),
            rotation,
            origin,
            Projectile.scale,
            direction,
            0f
        );
    }

        return false;
    }


    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        // find dimensions of texture
        Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

        // find blades
        Vector2 topRightOffset = new Vector2(texture.Width * 0.5f, -texture.Height * 0.5f);
        topRightOffset = topRightOffset.RotatedBy(Projectile.rotation+MathHelper.ToRadians(45f)); // Rotate the offset based on the projectile's rotation

        // Set the new hitbox position
        Vector2 newHitboxPosition = Projectile.Center + topRightOffset;

        // Update the hitbox rectangle
        hitbox.X = (int)newHitboxPosition.X - hitbox.Width / 2; // Center the hitbox
        hitbox.Y = (int)newHitboxPosition.Y - hitbox.Height / 2;
    }



    private void PerformAttack(Player player)
    {
        switch (attackState)
        {
            case 0: // Thrust attack
                PerformThrust();
                break;
            case 1: // Crescent swing
                PerformCrescentSwing();
                break;
            case 2: // Large slash
                PerformLargeSlash();
                break;
        }

        if (attackState > 2)
        {
            attackState = 0;
        }
    }

    private void PerformThrust()
    {
        isThrusting = true; // Enable thrusting state
        thrustProgress = 0f; // Reset thrust progress
    }

    private void PerformCrescentSwing()
    {
        float swingAmount = MathHelper.ToRadians(45f);
        Projectile.rotation += swingAmount;
    }

    private void PerformLargeSlash()
    {
        float slashAmount = MathHelper.ToRadians(90f);
        Projectile.rotation += slashAmount; 
    }


    public override void OnKill(int timeLeft)
    {
        int lanternType = ModContent.ProjectileType<nadir2Lantern>();
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile proj = Main.projectile[i];
            if (proj.active && proj.owner == Projectile.owner && proj.type == lanternType)
            {
                proj.Kill();
            }
        }
    }






}