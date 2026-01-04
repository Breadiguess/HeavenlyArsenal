using HeavenlyArsenal.Content.Rarities;
using Luminance.Assets;

namespace HeavenlyArsenal.Content.Items.Materials.BloodMoon;

public class PenumbralMembrane : VariantItemBase
{
    public override int MaxVariants => 2;
    public override string LocalizationCategory => "Items.Misc";

    public override void SetDefaults()
    {
        Item.maxStack = Item.CommonMaxStack;
        Item.Size = ModContent.Request<Texture2D>(Texture).Value.Size();
        Item.rare = ModContent.RarityType<BloodMoonRarity>();
        Item.sellPrice(0, 38, 20, 5);
        
    }
}