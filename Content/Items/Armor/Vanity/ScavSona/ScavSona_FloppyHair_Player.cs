using HeavenlyArsenal.Common.utils;
using Luminance.Common.VerletIntergration;
using NoxusBoss.Core.Physics.VerletIntergration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Armor.Vanity.ScavSona
{
    public class ScavSona_FloppyHair_Player : ModPlayer
    {
        #region DrawHairToTarget;
        public override void Load()
        {
            On_Main.CheckMonoliths += CheckRenderHair;
        }
        public static RenderTarget2D ScavSona_Hair_Target;
        private void CheckRenderHair(On_Main.orig_CheckMonoliths orig)
        {

            if (ScavSona_Hair_Target == null || ScavSona_Hair_Target.IsDisposed)
                ScavSona_Hair_Target = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
            else if (ScavSona_Hair_Target.Size() != new Vector2(Main.screenWidth / 2, Main.screenHeight / 2))
            {
                Main.QueueMainThreadAction(() =>
                {
                    ScavSona_Hair_Target.Dispose();
                    ScavSona_Hair_Target = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
                });
                return;
            }
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null);

            Main.graphics.GraphicsDevice.SetRenderTarget(ScavSona_Hair_Target);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            foreach (Player player in Main.ActivePlayers)
            {
                RenderPlayerHair(player);
            }


            Main.graphics.GraphicsDevice.SetRenderTarget(null);

            Main.spriteBatch.End();

            orig();
        }

        public static void RenderPlayerHair(Player player)
        {
            
        }

        #endregion
        public const int MAX_HAIR_LENGTH = 40;
        public Vector2[] HairPos;
        public Vector2[] HairVels;
        public override void Initialize()
        {
            HairPos = new Vector2[MAX_HAIR_LENGTH];
            HairVels = new Vector2[MAX_HAIR_LENGTH];

        }

        public override void PostUpdateMiscEffects()
        {
            
        }
        
    }
}
