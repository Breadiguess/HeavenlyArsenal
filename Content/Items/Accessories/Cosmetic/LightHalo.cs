using HeavenlyArsenal.Common.Utilities;
using NoxusBoss.Assets;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Accessories.Cosmetic;

internal class LightHalo : ModItem
{
    public override void SetDefaults()
    {
        Item.vanity = true;
        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<LightHalo_Player>().hasHalo = true;
    }
}

internal class LightHalo_Player : ModPlayer
{
    public bool hasHalo;

    public override void ResetEffects()
    {
        hasHalo = false;
    }
}

public class LightHalo_DrawLayer : PlayerDrawLayer
{
    public override Position GetDefaultPosition()
    {
        return new BeforeParent(PlayerDrawLayers.HairBack);
    }

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return drawInfo.drawPlayer.GetModPlayer<LightHalo_Player>().hasHalo;
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        ref var player = ref drawInfo.drawPlayer;
        Texture2D texture = GennedAssets.Textures.NamelessDeity.NamelessDeityEyeFull; //ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Accessories/Cosmetic/LightHalo").Value;

        var DrawPos = drawInfo.HeadPosition() + new Vector2(10 * -player.direction, -5 + MathF.Sin(Main.GlobalTimeWrappedHourly + player.whoAmI * 10));

        var Origin = texture.Size() * 0.5f;
        var Scale = new Vector2(0.02f, 0.02f);
        var Rot = MathHelper.ToRadians(1.5f) * player.direction;
        var halo = new DrawData(texture, DrawPos, null, Color.AntiqueWhite, Rot, Origin, Scale, 0);

        drawInfo.DrawDataCache.Add(halo);
    }
}