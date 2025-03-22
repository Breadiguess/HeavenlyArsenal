using CalamityMod.Buffs.DamageOverTime;
using HeavenlyArsenal.Content.Buffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Buffs;
using NoxusBoss.Core.Utilities;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Summon
{
    public class SolynWhip_Projectile : ModProjectile
    {
        // The texture doesn't have the same name as the item, so this property points to it.
       // public override string Texture => "HeavenlyArsenal/Content/Projectiles/Weapons//summonTestWhipProjectile";

        public override void SetStaticDefaults()
        {
            // This makes the projectile use whip collision detection and allows flasks to be applied to it.
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = false; // This prevents the projectile from hitting through solid tiles.
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.WhipSettings.Segments = 9;
            Projectile.WhipSettings.RangeMultiplier = 2f;
        }

        private float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        private float ChargeTime
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        public ref Player Player => ref Main.player[Projectile.owner];


        public static readonly SoundStyle WhipCrack = new SoundStyle("HeavenlyArsenal/Assets/Sounds/Items/Summoner/SolynWhip_Crack");



        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2; // Without PiOver2, the rotation would be off by 90 degrees counterclockwise.

            Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * Timer;
            // Vanilla uses Vector2.Dot(Projectile.velocity, Vector2.UnitX) here. Dot Product returns the difference between two vectors, 0 meaning they are perpendicular.
            // However, the use of UnitX basically turns it into a more complicated way of checking if the projectile's velocity is above or equal to zero on the X axis.
            Projectile.spriteDirection = Projectile.velocity.X >= 0f ? 1 : -1;

            // remove these 3 lines if you don't want the charging mechanic
            //if (!Charge(owner))
            //{
            //    return; // timer doesn't update while charging, freezing the animation at the start.
           // }

            Timer++;

            float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;
            if (Timer >= swingTime || owner.itemAnimation <= 0)
            {
                Projectile.Kill();
                return;
            }

            owner.heldProj = Projectile.whoAmI;
            if (Timer == swingTime / 2)
            {
                // Plays a whipcrack sound at the tip of the whip.
                List<Vector2> points = Projectile.WhipPointsForCollision;
                Projectile.FillWhipControlPoints(Projectile, points);
               
            }
        }

        // This method handles a charging mechanic.
        // If you remove this, also remove Item.channel = true from the item's SetDefaults.
        // Returns true if fully charged
        private bool Charge(Player owner)
        {
            // Like other whips, this whip updates twice per frame (Projectile.extraUpdates = 1), so 120 is equal to 1 second.
            if (!owner.channel || ChargeTime >= 120)
            {
                return true; // finished charging
            }

            ChargeTime++;

            if (ChargeTime % 12 == 0) // 1 segment per 12 ticks of charge.
                Projectile.WhipSettings.Segments++;

            // Increase range up to 2x for full charge.
            Projectile.WhipSettings.RangeMultiplier += 1 / 120f;

            // Reset the animation and item timer while charging.
            owner.itemAnimation = owner.itemAnimationMax;
            owner.itemTime = owner.itemTimeMax;

            return false; // still charging
        }

        

        // This method draws a line between all points of the whip, in case there's empty space between the sprites.
        private void DrawLine(List<Vector2> list)
        {
            //Texture2D texture = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
            Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Projectiles/Weapons/Summon/SolynWhip_StringTexture").Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = new Vector2(frame.Width/2, frame.Width/2);
            
            Vector2 pos = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation();
                Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.AntiqueWhite);
                Vector2 scale = new Vector2(1, 1);

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

                pos += diff;
            }
        }



        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player.AddBuff(ModContent.BuffType<SolynWhip_Onhit_Buff>(), 
                 600); // for 5 seconds (60 ticks = 1 second)
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), 400);
            
            SoundEngine.PlaySound(WhipCrack.WithPitchOffset(Main.rand.NextFloat(-0.5f,0.5f)));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            List<Vector2> list = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, list);

            DrawLine(list);

            //Main.DrawWhip_WhipBland(Projectile, list);
            // The code below is for custom drawing.
            // If you don't want that, you can remove it all and instead call one of vanilla's DrawWhip methods, like above.
            // However, you must adhere to how they draw if you do.

            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.instance.LoadProjectile(Type);
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Vector2 pos = list[0];

            for (int i = 0; i < list.Count - 1; i++)
            {
                // These two values are set to suit this projectile's sprite, but won't necessarily work for your own.
                // You can change them if they don't!
                Rectangle frame = new Rectangle(1, 4, 34, 34);
                Vector2 origin = new Vector2(frame.Width/2,frame.Height/3);
                float scale = 1;

                // These statements determine what part of the spritesheet to draw for the current segment.
                // They can also be changed to suit your sprite.
                if (i == list.Count - 2)
                {

                //frame.Height = 18;

                // For a more impactful look, this scales the tip of the whip up when fully extended, and down when curled up.
                Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
                float t = Timer / timeToFlyOut;
                scale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));
                }
              


                if (i == 0)
                {
                    frame.Y = 0;
                }
                else if (i >= 1 && i <= 7)
                {
                    if (i % 2 == 0)
                    {
                        frame.Y = 64; 
                        
                    }
                    else
                    {
                        frame.Y = 32;
                    }
                }
               
                else if (i >=8)
                {
                    frame.Y = 102;
                }
                    Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.
                Color color = Lighting.GetColor(element.ToTileCoordinates());

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

                pos += diff;
            }
            return false;
        }
    }
}