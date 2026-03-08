using HeavenlyArsenal.Content.Items.Armor.Vanity.ScavSona;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using static Terraria.ModLoader.PlayerDrawLayer;

namespace HeavenlyArsenal.Content.Items
{
    internal class ExampleRTPlayer : ModPlayer
    {
        public static RenderTarget2D _renderTarget;
        public override void Load()
        {
            On_Main.CheckMonoliths += CheckRenderArms;
        }


        private void CheckRenderArms(On_Main.orig_CheckMonoliths orig)
        {

            if (ExampleRTPlayer._renderTarget == null || ExampleRTPlayer._renderTarget.IsDisposed)
                ExampleRTPlayer._renderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            else if (ExampleRTPlayer._renderTarget.Size() != new Vector2(Main.screenWidth, Main.screenHeight))
            {
                Main.QueueMainThreadAction(() =>
                {
                    ExampleRTPlayer._renderTarget.Dispose();
                    ExampleRTPlayer._renderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
                });
                return;
            }
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            Main.graphics.GraphicsDevice.SetRenderTarget(ExampleRTPlayer._renderTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            //for every player, check if they are a viable candidate for getting the shape of.
            foreach(Player player in Main.ActivePlayers)
            {
                GetPlayerOutline(player);
            }

            Main.graphics.GraphicsDevice.SetRenderTarget(null);

            Main.spriteBatch.End();

            orig();

        }



        private static void GetPlayerOutline(Player Player)
        {
            Matrix transformation = Main.GameViewMatrix.ZoomMatrix;
            Vector2 scale = Vector2.One / new Vector2(Main.GameViewMatrix.TransformationMatrix.M11, Main.GameViewMatrix.TransformationMatrix.M22);
          
            SpriteEffects direction = SpriteEffects.None;
            Vector2 drawPosition = Player.Center - Main.screenPosition;

            float rotation = Player.fullRotation;
            Texture2D texture = LocalPlayerDrawManager.PlayerTarget;
            Main.spriteBatch.Draw(texture, drawPosition, null, Color.Black, rotation, texture.Size() * 0.5f, scale, direction, 0f);
            //Main.spriteBatch.Draw(texture, drawPosition, null, Color.White * Projectile.Opacity * MathF.Pow(1f - pulse, 2f), rotation, texture.Size() * 0.5f, scale * (1f + pulse * 0.9f), direction, 0f);




            Main.PlayerRenderer.DrawPlayer(Main.Camera, Player, Player.position, 0f, Player.fullRotationOrigin, 0f, 1f);
        }
    }


    public class ExampleRtDrawer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition()
        {

            return new AfterParent(PlayerDrawLayers.BackAcc);
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {

            if (ExampleRTPlayer._renderTarget is null || ExampleRTPlayer._renderTarget.IsDisposed)
                return;
            //Main.EntitySpriteDraw(ExampleRTPlayer._renderTarget, Vector2.Zero, null, Color.White, 0, ExampleRTPlayer._renderTarget.Size()/4, 2, 0);
        }
    }
}
