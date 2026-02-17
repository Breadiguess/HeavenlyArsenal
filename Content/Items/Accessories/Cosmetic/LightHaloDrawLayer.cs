using HeavenlyArsenal.Common.Utilities;
using NoxusBoss.Assets;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Accessories.Cosmetic;

public sealed class LightHaloDrawLayer : PlayerDrawLayer
{
    public override Position GetDefaultPosition()
    {
        return new BeforeParent(PlayerDrawLayers.HairBack);
    }

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return drawInfo.drawPlayer.GetModPlayer<LightHaloPlayer>().Enabled;
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        var player = drawInfo.drawPlayer;

        var texture = GennedAssets.Textures.NamelessDeity.NamelessDeityEyeFull.Value;
        var origin = texture.Size() / 2f;

        var position = drawInfo.HeadPosition() + new Vector2(10 * -player.direction, -5 + MathF.Sin(Main.GlobalTimeWrappedHourly + player.whoAmI * 10));
        
        var rotation = MathHelper.ToRadians(1.5f) * player.direction;
        
        var scale = new Vector2(0.2f);
        
        var data = new DrawData(texture, position, null, Color.AntiqueWhite, rotation, origin, scale, SpriteEffects.None);

        drawInfo.DrawDataCache.Add(data);
    }
}