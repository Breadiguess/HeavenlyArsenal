using HeavenlyArsenal.Content.Items.Misc;
using HeavenlyArsenal.Content.Projectiles.Misc.GoodAppleSlingFolder;
using Luminance.Common.Utilities;
using Luminance.Common.VerletIntergration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NoxusBoss.Assets;
using NoxusBoss.Content.Items;
using NoxusBoss.Core.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Misc.GoodAppleSlingFolder
{
    class GoodAppleSlingHeld : ModProjectile
    {
        // Enum representing the different states of the slingshot
        public enum GoodAppleSlingState
        {
            Idle,
            Charge,
            Fire
        }

        // The current state of the slingshot.
        // It starts in the Idle state.
        private GoodAppleSlingState currentState = GoodAppleSlingState.Idle;

        // We still use ai[0] for timing purposes
        public ref float Time => ref Projectile.ai[0];
        // You already had ai[1] for string interpolation so we leave it as is.
        public ref float StringReelBackInterpolant => ref Projectile.ai[1];

        public bool InUse => Player.controlUseItem && Player.altFunctionUse == 0;
        public ref Player Player => ref Main.player[Projectile.owner];

        // Offsets for drawing string (unchanged)
        public Vector2 topStringOffset => new Vector2(200f, +13f);
        public Vector2 bottomStringOffset => new Vector2(-200f, -20f);
        public float StringHalfHeight => (Math.Abs(topStringOffset.X) / 2 + Math.Abs(bottomStringOffset.X));
        public Vector2 StringReelbackDistance = new Vector2(0, 0);

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.width = Projectile.height = 32;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Time = 0;
            StringReelBackInterpolant = 0;
            // Ensure we start in the Idle state.
            currentState = GoodAppleSlingState.Idle;
            
        }

        public override void AI()
        {
            // Always update projectile orientation and positioning based on the player's position and mouse target.
            Projectile.velocity = Player.DirectionTo(Main.MouseWorld) * 20f;
            Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;
            Projectile.damage = (int)Player.GetTotalDamage(DamageClass.Generic).ApplyTo(Player.HeldItem.damage);
            Player.heldProj = Projectile.whoAmI;
            Projectile.Center = Player.Center;
            Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.velocity.ToRotation(), 0.9f);
            Player.ChangeDir(Projectile.direction);

            // Ensure the projectile is only active if the player holds the correct item and is not CCed or dead.
            if (Player.HeldItem.type != ModContent.ItemType<GoodAppleSling>() || Player.CCed || Player.dead)
            {
                Projectile.Kill();
                return;
            }

           
            switch (currentState)
            {
                case GoodAppleSlingState.Idle:
                    // While idle, if the player holds left click, begin charging.
                    if (Player.controlUseItem && Player.altFunctionUse == 0)
                    {
                        currentState = GoodAppleSlingState.Charge;
                        Time = 0; // reset charge time
                    }
                    break;

                case GoodAppleSlingState.Charge:
                    int totalFrames = 3;
                    int frameDuration = 30;
                    if (!Player.controlUseItem)
                    {
                        currentState = GoodAppleSlingState.Idle;
                        Time = 0;
                    }
                    else
                    {
                            
                        Projectile.frame = 0; // Start animation from the first frame
                        Projectile.frameCounter = 0; // Reset frame counter




                        Time++;
                        if (Time >= 60)
                        {
                            if (++Projectile.frameCounter > frameDuration)
                            {

                                Projectile.frameCounter = 0; // Reset the counter
                                Projectile.frame++; // Move to the next frame

                                if (Projectile.frame >= totalFrames)
                                    Projectile.frame = totalFrames;
                            }

                            currentState = GoodAppleSlingState.Fire;
                        }
                    }
                    break;

                case GoodAppleSlingState.Fire:
                    {
                        // Fire an apple projectile.
                        int ammoItemType = Player.HeldItem.useAmmo;
                        bool isGoodApple = ammoItemType == ModContent.ItemType<GoodApple>();

                        
                        SoundEngine.PlaySound(GennedAssets.Sounds.Common.TwinkleMuffled, Player.Center, null);
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            Projectile.Center,
                            Projectile.velocity * Main.rand.NextFloat(0.6f, 0.67f) + new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1)),
                            ModContent.ProjectileType<GoodAppleProj>(),
                            Projectile.damage,
                            Projectile.knockBack,
                            Player.whoAmI,
                            ai0: isGoodApple ? 1f : 0f
                        );

                        // Reset the charge timer
                        Time = 0;

                        // After firing, if the left mouse button is still held, go back to charging.
                        if (Player.controlUseItem && Player.altFunctionUse == 0)
                        {
                            currentState = GoodAppleSlingState.Charge;
                        }
                        else
                        {
                            currentState = GoodAppleSlingState.Idle;
                        }
                    }
                    break;
            }

        }

        // This method is no longer directly used in AI because we handle firing in our state machine.
        public void Shoot()
        {
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Projectile.velocity,
                ModContent.ProjectileType<GoodAppleProj>(),
                Projectile.damage,
                Projectile.knockBack,
                Player.whoAmI
            );
            // SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, 5, 0, 0);


            Vector2 topOfBow = Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation + topStringOffset.ToRotation()) * topStringOffset.Length();
            Vector2 bottomOfBow = Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation + bottomStringOffset.ToRotation()) * bottomStringOffset.Length();
            Vector2 endOfString = Projectile.Center + Projectile.rotation.ToRotationVector2();

            Vector2 origin = texture.Size() * 0.5f;
            Vector2 offset = Vector2.Zero;
            Vector2 drawPosition = (Projectile.Center - Main.screenPosition) + offset;

            Color stringColor = new Color(255, 0, 0);

            int direction = Projectile.spriteDirection;
            SpriteEffects flipEffect = direction > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;

            // Draw the bow's strings connecting the bow to the projectile direction
            //Main.spriteBatch.DrawLineBetter(topOfBow, endOfString, stringColor, 2f);
            //Main.spriteBatch.DrawLineBetter(bottomOfBow, endOfString, stringColor, 2f);

            Main.spriteBatch.Draw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, flipEffect, 0f);

            return false;
        }

        // The projectile should not hit NPCs or cut tiles.
        public override bool? CanHitNPC(NPC target) => false;
        public override bool? CanCutTiles() => false;
    }
}
