using HeavenlyArsenal.Content.Items.Weapons.CCR_Weapon;
using Luminance.Assets;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Utilities;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Jellyfish
{
    internal class JellyExplosion : ModProjectile
    {
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override void SetStaticDefaults()
        {

        }
        public int Time
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.friendly = false;

            Projectile.Size = new Vector2(1, 1);
            Projectile.timeLeft = ExplodeTime + 60;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;

        }
        int ExplodeTime = 100;
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.scale = 0;


        }
       
        public override void AI()
        {
            if (Time % ExplodeTime == 0 && Time > 0)
                SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Weapons.AvatarRifle.FireSoundSuper, Projectile.Center).WithVolumeBoost(10);
             if (Time < ExplodeTime)
            {
                Projectile.scale = float.Lerp(Projectile.scale, 1, 0.04f);
            }
            if (Time > ExplodeTime)
            {
                Projectile.scale = float.Lerp(Projectile.scale, 0, 0.4f);
                if (Projectile.scale < 0.06f)
                    Projectile.Kill();
            }

            Time++;
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            string projName = Lang.GetProjectileName(Projectile.type).Value;
            int val = Main.rand.Next(0, 3);
            NetworkText text = NetworkText.FromKey($"Mods.{Mod.Name}.PlayerDeathMessages.JellyExplosion{val}", target.name, projName);

            modifiers = new Player.HurtModifiers
            {
                DamageSource = PlayerDeathReason.ByCustomReason(text)
            };

        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return targetHitbox.IntersectsConeFastInaccurate(projHitbox.Center(), 300, 0, MathHelper.TwoPi);
        }
        public override bool? CanDamage()
        {
            if (Time % ExplodeTime == 0)
                return true;

            else
                return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }

        BasicEffect thing;
        VertexPositionColor[] vertsTL; 

        void DrawCircle()
        {
            if (Main.netMode == NetmodeID.Server) return;

            GraphicsDevice gd = Main.graphics.GraphicsDevice;
            
            if (thing == null)
            {
                thing = new BasicEffect(gd)
                {
                    VertexColorEnabled = true,
                    Alpha = Projectile.Opacity, 
                };
            }

            const int segments = 64;                   
            float radius = 300f * Projectile.scale;    
            Color color = Color.Cyan * Projectile.Opacity * 0.5f;
            Vector2 c = Projectile.Center - Main.screenPosition;

            // TriangleList needs 3 verts per triangle
            if (vertsTL == null || vertsTL.Length != segments * 3)
                vertsTL = new VertexPositionColor[segments * 3];

            for (int i = 0; i < segments; i++)
            {
                float a0 = MathHelper.TwoPi * i / segments;
                float a1 = MathHelper.TwoPi * (i + 1) / segments;

                Vector2 p0 = c; // center
                Vector2 p1 = c + new Vector2(MathF.Cos(a0), MathF.Sin(a0)) * radius;
                Vector2 p2 = c + new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * radius;

                int k = i * 3;
                vertsTL[k + 0] = new VertexPositionColor(new Vector3(p0, 0f), color);
                vertsTL[k + 1] = new VertexPositionColor(new Vector3(p1, 0f), color);
                vertsTL[k + 2] = new VertexPositionColor(new Vector3(p2, 0f), color);
            }

            // Matrices (screen-space)
            thing.World = Matrix.Identity;
            thing.View = Main.GameViewMatrix.ZoomMatrix;
            thing.Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);

            gd.RasterizerState = RasterizerState.CullNone;
            gd.BlendState = BlendState.AlphaBlend;

            foreach (var pass in thing.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(PrimitiveType.TriangleList, vertsTL, 0, segments);
            }



        }
      

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.PrepareForShaders();
            Texture2D placeholder = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
            DrawCircle();


            //Main.EntitySpriteDraw(placeholder, Projectile.Center-Main.screenPosition, null, Color.AntiqueWhite, 0, placeholder.Size() /2, 1000, 0);
            Main.spriteBatch.ResetToDefault();
            //Utils.DrawBorderString(Main.spriteBatch, (Projectile.Distance(Main.LocalPlayer.Center)).ToString(), Projectile.Center - Main.screenPosition, Color.AliceBlue);
            return false;
        }
    }
   

}
