using static Luminance.Common.Utilities.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Core.GlobalInstances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using NoxusBoss.Assets;

namespace HeavenlyArsenal.Content.Rarities
{
    public class BloodMoonRarity : ModRarity
    {
        public override Color RarityColor => new Color(220, 20, 70);
       // public override void Load() => GlobalItemEventHandlers.PreDrawTooltipLineEvent += RenderRarityWithShader;

        private bool RenderRarityWithShader(Item item, DrawableTooltipLine line, ref int yOffset)
        {
            return false;
            if (item.rare == Type && line.Name == "ItemName" && line.Mod == "Terraria")
            {
                Main.spriteBatch.PrepareForShaders(null, true);

                ManagedShader barShader = ShaderManager.GetShader("HeavenlyArsenal.NamelessBossBarShader");
                
                barShader.TrySetParameter("textureSize", Vector2.One * 560f );
                barShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly);
                barShader.TrySetParameter("dropletFrequency", 5.0f);
                barShader.TrySetParameter("dropletThreshold", 0.8f);
                barShader.TrySetParameter("bleedSpeed", 0.2f);
                barShader.TrySetParameter("minDropletRadius", 0.01f);
                barShader.TrySetParameter("maxDropletRadius", 0.03f);
               
                 barShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
                
                barShader.Apply();
                Vector2 drawPosition = new Vector2(line.X, line.Y);
                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, line.Font, line.Text, drawPosition, Color.White, line.Rotation, line.Origin, line.BaseScale, line.MaxWidth, line.Spread);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
                return false;
            }

            return true;
        }
    }
}
