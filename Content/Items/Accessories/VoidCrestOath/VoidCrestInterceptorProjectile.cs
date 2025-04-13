using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles
{
    public class VoidCrestInterceptorProjectile : ModProjectile
    {
        public override string Texture => "Calamitymod/Projectiles/InvisibleProj"; // Placeholder texture
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 6000; // Lifetime in ticks
                                      // Additional settings as needed
        }

        public override void AI()
        {
            // Basic AI example: homing in on the nearest hostile projectile, or just continuing
            // along its velocity. A more sophisticated AI might search out the target, etc.
            // For now, just do some dust or visuals:
            int dustIndex = Dust.NewDust(Projectile.Center, 4, 4, DustID.GoldCoin);
            Main.dust[dustIndex].noGravity = true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // If it hits an NPC, it might vanish:
            Projectile.Kill();
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // If it hits a player (in PvP), also vanish:
            Projectile.Kill();
        }

        public ref float Time => ref Projectile.ai[0];
        public int NPCToAttachTo => (int)Projectile.ai[1];

        public ref float VisualScale => ref Projectile.localAI[0];

        private VoidLakeShadowHandData[] shadowHands;
        private int handCount;

        public struct VoidLakeShadowHandData
        {
            public VoidLakeShadowHandData(float targetLength)
            {
                Length = targetLength * Main.rand.NextFloat(0.8f, 1.2f);
                ArmStyle = Main.rand.Next(3);
                HandStyle = Main.rand.Next(3);
            }

            private int ArmStyle;
            private int HandStyle;

            public float Length;
            public float Rotation;

            public void Draw(Vector2 center, float extensionAmount, float scale, float rotation, int direction = 1)
            {
                const float armLength = 215;

                Texture2D armTexture = AssetDirectory.Textures.VoidLakeShadowArm.Value;
                Texture2D handTexture = AssetDirectory.Textures.VoidLakeShadowHand.Value;
                Rectangle armFrame = armTexture.Frame(3, 1, ArmStyle);
                Rectangle handFrame = handTexture.Frame(3, 1, ArmStyle);
                SpriteEffects flip = direction > 0 ? 0 : SpriteEffects.FlipHorizontally;

                float scalingUp = 0.5f * scale * MathF.Sqrt(Utils.GetLerpValue(0, 0.5f, extensionAmount, true));
                Vector2 armScale = new Vector2(scalingUp, extensionAmount * (Length / armLength));

                Vector2 handPosition = center + rotation.ToRotationVector2() * extensionAmount * Length;
                Main.EntitySpriteDraw(handTexture, handPosition, handFrame, Color.Red, rotation + MathHelper.PiOver2, new Vector2(handFrame.Width / 2, handFrame.Height - 60), scalingUp * 1.15f, flip, 0);

                Main.EntitySpriteDraw(armTexture, center, armFrame, Color.Red, rotation + MathHelper.PiOver2, new Vector2(armFrame.Width / 2, armFrame.Height - 16), armScale, flip, 0);
            }
        }

        public const float DistanceFromTarget = 160;
    }
    

}
