using CalamityMod;
using HeavenlyArsenal.Content.Items.Armor;
using HeavenlyArsenal.Content.Items.Weapons.Summon.BloodMoonWhip;
using HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech;
//using HeavenlyArsenal.Content.Items.Accessories.VoidCrestOath;
//using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
//using HeavenlyArsenal.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Common.Players
{
    internal class DebugPlayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Torso);


        BasicEffect Cone;
        public List<VertexPositionColorTexture> ConeVerts;
        void DrawCone()
        {
            if (ConeVerts.Count < 3)
                return;

            GraphicsDevice gd = Main.graphics.GraphicsDevice;
            if(Cone == null)
            Cone = new BasicEffect(gd)
            {
                VertexColorEnabled = true,
                LightingEnabled = false,
                TextureEnabled = false
            };
            Cone.World = Matrix.Identity;
            Cone.View = Main.GameViewMatrix.ZoomMatrix;

            Cone.Projection = Matrix.CreateOrthographicOffCenter(
                0, Main.screenWidth,
                Main.screenHeight, 0,
                -1000f, 1000f);

            foreach (var pass in Cone.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(
                    PrimitiveType.TriangleStrip,
                    ConeVerts.ToArray(),
                    0,
                    ConeVerts.Count - 2
                );
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="Center">converts to world position, so all you need to do is place this in the spot you want your cone to originate from</param>
        /// <param name="rotation"></param>
        /// <param name="halfAngle"></param>
        /// <param name="length"></param>
        /// <param name="resolution"></param>
        /// <param name="color"></param>
        public static void BuildCone(List<VertexPositionColorTexture> verts, Vector2 Center, float rotation, float halfAngle, float length, int resolution, Color color)
        {
            verts.Clear();
            //offset the coordinates so they're in screen coords
            Center -= Main.screenPosition;
            // Direction the cone points in
            Vector2 dir = rotation.ToRotationVector2();

            // Create arc segment points
            for (int i = 0; i <= resolution; i++)
            {
                float t = i / (float)resolution;
                float ang = MathHelper.Lerp(-halfAngle, halfAngle, t);
                Vector2 edgeDir = dir.RotatedBy(ang);

                Vector2 p = Center + edgeDir * length;

                float radiusFade = 0.2f;
                float edgeFade = 0f;
                float sideFade = MathF.Cos((Math.Abs(ang) / halfAngle) * MathHelper.PiOver2);

                // combine fades:
                float apexAlpha = radiusFade * sideFade;
                float edgeAlpha = edgeFade * sideFade;

                Color apexColor = color * apexAlpha;
                Color edgeColor = color * edgeAlpha;

                verts.Add(new VertexPositionColorTexture(
                    new Vector3(Center, 0f),
                    apexColor,
                    new Vector2(0f, 0f)
                ));

                verts.Add(new VertexPositionColorTexture(
                    new Vector3(p, 0f),
                    edgeColor,
                    new Vector2(t, 1f)
                ));
            }
        }
        private void prepCone(Player player)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin();
            if (ConeVerts == null)
                ConeVerts = new();
            BuildCone(ConeVerts, player.Center, player.Center.AngleTo(Main.MouseWorld) + MathHelper.PiOver4/2, MathHelper.ToRadians(50), 1000, 30, Color.White);
            DrawCone();

            Main.spriteBatch.End();
            Main.spriteBatch.Begin();
        }
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {

            Player Owner = drawInfo.drawPlayer;

            //prepCone(Owner);
            //float fallSpeedInterpolant = Luminance.Common.Utilities.Utilities.InverseLerp(25f, 130f, Owner.velocity.Y);
            string msg = "";

            //msg += $"{Owner.GetModPlayer<ShintoArmorBarrier>().barrier}\n"
            //    + $"{Owner.GetModPlayer<ShintoArmorBarrier>().timeSinceLastHit}\n";
            //msg += $"{fallSpeedInterpolant}\n {Owner.maxFallSpeed}";
            /*
            msg = $"modStealth: {Owner.Calamity().modStealth} \n"
                + $"rogueStealth: {Owner.Calamity().rogueStealth}\n"
                + $"Stealth Max:{Owner.Calamity().rogueStealthMax * 100}\n"
                + $"StealthAcceleration: {Owner.Calamity().stealthAcceleration}\n"
                + $"{Owner.Calamity().stealthGenMoving}";
            */
            //msg += $"{Owner.GetModPlayer<MedusaPlayer>().MedusaStacks}\n{Owner.GetModPlayer<MedusaPlayer>().MedusaTimer}";
            // msg += $"{Owner.GetModPlayer<PlaceholderName>().blood}";
            // if(Owner.HeldItem.type == ModContent.ItemType<ViscousWhip_Item>())
            //msg += $"{Owner.Center.ToTileCoordinates()}";
            
            Utils.DrawBorderString(Main.spriteBatch, msg, Owner.Center - Main.screenPosition, Color.AntiqueWhite, 1, 0.2f, -1.2f);
           
            //Main.EntitySpriteDraw(newLeech.leechTarget, Owner.Center - Main.screenPosition, null, Color.AntiqueWhite, 0, Vector2.Zero, 1, 0);
            
        }

    }
}
