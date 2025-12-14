using HeavenlyArsenal.Content.Rarities;
using Luminance.Assets;

namespace HeavenlyArsenal.Content.Items.Materials.BloodMoon;

internal class PenumbralMembrane : ModItem
{
    public override string Texture => MiscTexturesRegistry.PixelPath;

    public override string LocalizationCategory => "Items.Misc";

    public override void SetDefaults()
    {
        Item.maxStack = Item.CommonMaxStack;
        Item.width = 38;
        Item.height = 32;
        Item.BestiaryNotes = "Crab";
        Item.rare = ModContent.RarityType<BloodMoonRarity>();
        Item.sellPrice(0, 38, 20, 5);
    }
}