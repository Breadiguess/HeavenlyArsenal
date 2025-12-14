using HeavenlyArsenal.Content.Rarities;

namespace HeavenlyArsenal.Content.Items.Materials.BloodMoon;

internal class ShellFragment : ModItem
{
    public override string LocalizationCategory => "Items.Misc";

    public override void SetDefaults()
    {
        Item.maxStack = Item.CommonMaxStack;
        Item.width = 38;
        Item.height = 32;
        Item.BestiaryNotes = "Crab";
        Item.rare = ModContent.RarityType<BloodMoonRarity>();
        Item.sellPrice(0, 18, 20, 5);
    }
}