using HeavenlyArsenal.Content.Subworlds;
using HeavenlyArsenal.Content.Tiles.Generic;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.LightingMask;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace HeavenlyArsenal.Content.Tiles.ForgottenShrine;

public class IdolStatueData : WorldOrientedTileObject
{
    private static readonly Asset<Texture2D> bowlTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/IdolStatueBowl");

    private static readonly Asset<Texture2D> statueTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/IdolStatue");

    private static readonly Asset<Texture2D> waterTimeMapTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/IdolStatueWaterTimeMap");

    private static readonly Asset<Texture2D> waterGlowTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/IdolStatueWaterGlow");

    public IdolStatueData() { }

    public IdolStatueData(Point position) : base(position)
    { }

    /// <summary>
    /// Updates this statue.
    /// </summary>
    public override void Update()
    {
        if (Main.rand.NextBool() && IdolStatueManager.WaterFlowCutoffInterpolant < 0.5f)
        {
            Vector2 dropPosition = Position.ToVector2() + new Vector2(Main.rand.NextFloatDirection() * 10f, -32f);
            Dust drop = Dust.NewDustPerfect(dropPosition, DustID.DungeonWater);
            drop.scale *= 0.6f;
            drop.velocity = -Vector2.UnitY.RotatedByRandom(0.85f) * Main.rand.NextFloat(0.5f, 2.3f);
            drop.noGravity = true;
        }
    }

    /// <summary>
    /// Renders this statue.
    /// </summary>
    public override void Render()
    {
        if (!Main.LocalPlayer.WithinRange(Position.ToVector2(), 2350f))
            return;

        Texture2D bowl = bowlTexture.Value;
        Texture2D statue = statueTexture.Value;
        Texture2D timeMap = waterTimeMapTexture.Value;
        Vector2 bottom = Position.ToVector2() - Main.screenPosition;

        float rotation = 0f;
        PrepareLightShader();
        Main.spriteBatch.Draw(statue, bottom, null, Color.White, rotation, statue.Size() * new Vector2(0.5f, 1f), 1f, 0, 0f);

        int timeMapHeight = timeMap.Height;
        int waterSourceDescent = (int)(timeMapHeight * IdolStatueManager.WaterFlowCutoffInterpolant);
        Rectangle waterFrame = timeMap.Frame();
        waterFrame.Y += waterSourceDescent;
        waterFrame.Height -= waterSourceDescent;

        ManagedShader waterShader = ShaderManager.GetShader("HeavenlyArsenal.ShrineBowlWaterShader");
        waterShader.TrySetParameter("waterColorA", new Color(4, 167, 209).ToVector4());
        waterShader.TrySetParameter("waterColorB", new Color(150, 255, 222).ToVector4());
        waterShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
        waterShader.Apply();
        Main.spriteBatch.Draw(timeMap, bottom, waterFrame, Color.White, rotation, waterFrame.Size() * new Vector2(0.5f, 1f), 1f, 0, 0f);

        Vector2 bowlDrawPosition = bottom - Vector2.UnitY.RotatedBy(rotation) * 14f;
        PrepareLightShader();
        Main.spriteBatch.Draw(bowl, bowlDrawPosition, null, Color.White, rotation, bowl.Size() * new Vector2(0.5f, 1f), 1f, 0, 0f);

        IdolStatueManager.ExtraDrawAction?.Invoke(bottom);

        ForgottenShrineDarknessSystem.QueueGlowAction(() =>
        {
            Main.spriteBatch.Draw(waterGlowTexture.Value, bottom + Main.screenLastPosition - Main.screenPosition, waterFrame, Color.White, rotation, waterFrame.Size() * new Vector2(0.5f, 1f), 1f, 0, 0f);
        });
    }

    private static void PrepareLightShader()
    {
        ManagedShader lightShader = ShaderManager.GetShader("HeavenlyArsenal.LightingShader");
        lightShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
        lightShader.TrySetParameter("screenSize", WotGUtils.ViewportSize);
        lightShader.SetTexture(LightingMaskTargetManager.LightTarget, 1, SamplerState.LinearClamp);
        lightShader.Apply();
    }

    /// <summary>
    /// Serializes this statue data as a tag compound for world saving.
    /// </summary>
    public override TagCompound Serialize() => new TagCompound()
    {
        ["Position"] = Position,
    };

    /// <summary>
    /// Deserializes a tag compound containing data for a statue back into said statue.
    /// </summary>
    public override IdolStatueData Deserialize(TagCompound tag) => new IdolStatueData(tag.Get<Point>("Position"));
}
