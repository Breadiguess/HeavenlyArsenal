global using LumUtils = Luminance.Common.Utilities.Utilities;
global using WotGUtils = NoxusBoss.Core.Utilities.Utilities;
using HeavenlyArsenal.Content.Items.Misc;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal
{
    // Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
    public class HeavenlyArsenal : Mod
    {
        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                // First, you load in your shader file.
                // You'll have to do this regardless of what kind of shader it is,
                // and you'll have to do it for every shader file.
                // This example assumes you have both armor and screen Shaders.

                Asset<Effect> dyeShader = this.Assets.Request<Effect>("Assets/AutoloadedEffects/Shaders/UncannyDye");
                //Asset<Effect> specialShader = this.Assets.Request<Effect>("Effects/MySpecials");
                //Asset<Effect> filterShader = this.Assets.Request<Effect>("Effects/MyFilters");

                // To add a dye, simply add this for every dye you want to add.
                // "PassName" should correspond to the name of your pass within the *technique*,
                // so if you get an error here, make sure you've spelled it right across your effect file.

                GameShaders.Armor.BindShader(ModContent.ItemType<UncannyDye>(), new ArmorShaderData(dyeShader, "ArmorAnimatedSine"));

                // If your dye takes specific parameters such as color, you can append them after binding the shader.
                // IntelliSense should be able to help you out here.   

                GameShaders.Armor.BindShader(ModContent.ItemType<UncannyDye>(), new ArmorShaderData(dyeShader, "ColorPass")).UseColor(10f, 100f, 1.4f);
                //GameShaders.Armor.BindShader(ModContent.ItemType<MyNoiseDyeItem>(), new ArmorShaderData(dyeShader, "NoisePass")).UseImage("Images/Misc/noise"); // Uses the default Terraria noise map.

                // To bind a miscellaneous, non-filter effect, use this.
                // If you're actually using this, you probably already know what you're doing anyway.
                // This type of shader needs an additional parameter: float4 uShaderSpecificData;
                //GameShaders.Misc["EffectName"] = new MiscShaderData(specialShader, "PassName");

                // To bind a screen shader, use this.
                // EffectPriority should be set to whatever you think is reasonable.   

                // Filters.Scene["FilterName"] = new Filter(new ScreenShaderData(filterShader, "PassName"), EffectPriority.Medium);
            }

        }
    }
}
