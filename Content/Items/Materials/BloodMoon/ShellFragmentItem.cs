using HeavenlyArsenal.Content.Rarities;

namespace HeavenlyArsenal.Content.Items.Materials.BloodMoon;

internal class ShellFragmentItem : ModItem
{
    public override string LocalizationCategory => "Items.Misc";

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.maxStack = Item.CommonMaxStack;

        Item.width = 38;
        Item.height = 32;

        // TODO: We may want to localize this.
        Item.BestiaryNotes = "Crab";

        Item.rare = ModContent.RarityType<BloodMoonRarity>();

        // TODO: We may want to adjust the price.
        Item.value = Item.sellPrice(0, 18, 20, 5);
    }
}