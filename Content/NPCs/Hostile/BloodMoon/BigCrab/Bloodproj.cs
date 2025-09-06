using System;
using System.Collections.Generic;
using System.Linq;
    

using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using CalamityMod;
using Terraria.DataStructures;
using Microsoft.Build.Construction;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab
{
    class Bloodproj : ModProjectile
    {
        public enum BloodProjAI
        {
            Normal,
            Burrow
        }
        public Player Unfortunate;
        private Vector2 Uoffset;
        public ref float Time => ref Projectile.ai[0];
        
        public BloodProjAI CurrentState = BloodProjAI.Normal;
        
        public int BulFram;
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
            Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, Projectile.velocity, ModContent.GoreType<BloodProjGore>(), 1f);
            Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, Projectile.velocity, ModContent.GoreType<BloodProjGore2>(), 1f);
        }
        public override void AI()
        {
            //Projectile.velocity *= 0.9999f;

            StateMachine();
            Time++;
            

            if(Projectile.timeLeft == 1)
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
            float baseSpeed = Projectile.velocity.Length();

            // Time-based wave calculation using GlobalTimeWrappedHourly for pause-safe animation
            float time = Main.GlobalTimeWrappedHourly + Projectile.whoAmI * 0.15f;

            // Sine wave parameters
            float amplitude = 8f;     // How far it sways
            float frequency = 5f;     // How fast it sways

            // Perpendicular direction to velocity
            Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2);

            // Apply sine wave offset to velocity
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * baseSpeed
                + perpendicular * ((float)Math.Sin(time * frequency) * (amplitude / 60f));
        }

        private void HandleBurrow()
        {
            if (Unfortunate != null)
            {
                Projectile.spriteDirection = Unfortunate.direction;
                Uoffset.X = Math.Abs(Uoffset.X) * Unfortunate.direction;
                Projectile.Center = Unfortunate.Center + Uoffset;//new Vector2(0, -Unfortunate.height / 2);
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
            if(CurrentState == BloodProjAI.Burrow)
            {
                return false;
            }
            else
                return base.CanDamage();
        }
        public override void PostAI()
        {
            
        }
        public override bool PreDraw(ref Color lightColor)
        {
            int value = (int)(Main.GlobalTimeWrappedHourly * 10.1f) % 3;
            int FrameCount = 7;

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = new Vector2(texture.Width / 2, (texture.Height / FrameCount) / 2);
            Vector2 DrawPos = Projectile.Center - Main.screenPosition;

            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;


            Rectangle sourceRect = new Rectangle(0, value * (texture.Height / FrameCount), texture.Width, texture.Height / FrameCount);

            // Base rotation
            float rotation = Projectile.rotation + MathHelper.PiOver2;

            float time = Main.GlobalTimeWrappedHourly + Projectile.whoAmI * 0.1f + (Projectile.Center.X * 0.01f);



            Vector2 scale = Vector2.One;

            if (CurrentState == BloodProjAI.Burrow)
            {
                // Pulsate: oscillate scale between 0.9 and 1.1
                float pulsate = 1f + 0.1f * (float)Math.Sin(time * 5f);

                // Rock: oscillate rotation by +/- 5 degrees
                float rock = MathHelper.ToRadians(5f) * (float)Math.Sin(time * 3f);

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
        public override void SetStaticDefaults()
        {
            
        }
        
        public override void OnSpawn(Gore gore, IEntitySource source)
        {

            base.OnSpawn(gore, source);
        }
    }
    public class BloodProjGore2 : BloodProjGore
    {
        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/BigCrab/BloodprojGore2";
    }
}
