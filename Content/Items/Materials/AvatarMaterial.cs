using HeavenlyArsenal.Core.Globals;
using HeavenlyArsenal.Core.Graphics;
using Luminance.Assets;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.GlobalInstances;
using Terraria.GameContent.ItemDropRules;

namespace HeavenlyArsenal.Content.Items.Materials;

public class AvatarMaterial : ModItem, ILocalizedModType
{
    public override string LocalizationCategory => "Items.Misc";

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        
        ItemID.Sets.ItemNoGravity[Type] = true;
    }

    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.maxStack = Item.CommonMaxStack;
        
        Item.width = 32;
        Item.height = 32;
        
        Item.value = Item.buyPrice(0, 0, 0, 3);
        
        Item.rare = ModContent.RarityType<AvatarRarity>();
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        var batch = Main.spriteBatch;
        var parameters = batch.Capture();
        
        batch.End();
        batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);

        var texture = GennedAssets.Textures.GreyscaleTextures.WhitePixel.Value;

        var shader = ShaderManager.GetShader("HeavenlyArsenal.avatarMaterial");

        shader.TrySetParameter("Time", Main.GlobalTimeWrappedHourly);
        shader.TrySetParameter("Color", Color.Red.ToVector4());
        
        // Try between 0.15f and 0.5f
        shader.TrySetParameter("MorphSpeed", 0.5f);
        shader.TrySetParameter("Threshold", 0.20f);
        
        // Try between 0.06f and 0.12f for smoother edges
        shader.TrySetParameter("EdgeWidth", 0.08f);
        shader.TrySetParameter("NoiseScale", new Vector2(2f, 2f));
        
        // Keep small to avoid clamping
        shader.TrySetParameter("WarpStrength", 0.05f);
        shader.TrySetParameter("NoiseSpeed", 0.15f);
        
        shader.SetTexture(GennedAssets.Textures.FirstPhaseForm.AvatarRift, 0);
        shader.SetTexture(GennedAssets.Textures.Noise.DendriticNoiseZoomedOut, 1, SamplerState.AnisotropicWrap);
        shader.SetTexture(GennedAssets.Textures.GoodAppleHearts.BarsLifeOverlay_Fill, 2);

        shader.SetTexture(GennedAssets.Textures.SecondPhaseForm.Beads2, 3);
        shader.Apply();

        batch.Draw(texture, position, null, Color.White, 0, texture.Size() / 2f, new Vector2(20f), SpriteEffects.None, 0f);

        batch.End();
        batch.Begin(in parameters);

        return true;
    }
}