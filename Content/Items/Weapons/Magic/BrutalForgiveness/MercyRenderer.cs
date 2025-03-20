using Luminance.Common.Utilities;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace HeavenlyArsenal.Content.Items.Weapons.Magic.BrutalForgiveness;

public class MercyRenderer : ModSystem
{
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        layers.Insert(0, new LegacyGameInterfaceLayer("Heavenly Arsenal: Mercy", () =>
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            int mercyID = ModContent.ProjectileType<Mercy>();
            foreach (Projectile mercy in Main.ActiveProjectiles)
            {
                if (mercy.type == mercyID)
                    mercy.As<Mercy>().RenderSelf();
            }

            Main.spriteBatch.ResetToDefault();
            return true;
        }));
    }
}
