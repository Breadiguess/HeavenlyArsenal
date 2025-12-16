using HeavenlyArsenal.Content.Rarities;
using Luminance.Assets;

namespace HeavenlyArsenal.Content.Items.Materials.BloodMoon;

public class PenumbralMembraneItem : ModItem
{
    public override string Texture => MiscTexturesRegistry.PixelPath;

    public override string LocalizationCategory => "Items.Misc";

    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.maxStack = Item.CommonMaxStack;

        Item.width = 32;
        Item.height = 32;

        // TODO: We may want to localize this.
        Item.BestiaryNotes = "Crab";

        Item.value = Item.sellPrice(0, 38, 20, 5);

        Item.rare = ModContent.RarityType<BloodMoonRarity>();
    }
}