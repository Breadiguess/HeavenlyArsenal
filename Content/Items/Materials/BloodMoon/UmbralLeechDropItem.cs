using HeavenlyArsenal.Content.Rarities;

namespace HeavenlyArsenal.Content.Items.Materials.BloodMoon;

public class UmbralLeechDropItem : ModItem
{
    public override string LocalizationCategory => "Items.Misc";

    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.maxStack = Item.CommonMaxStack;

        Item.width = 38;
        Item.height = 32;

        // TODO: We may want to localize this.
        Item.BestiaryNotes = "A leech that has fed on the blood of many. It is said that these leeches can drain the life force of even the strongest of beings.";

        Item.rare = ModContent.RarityType<BloodMoonRarity>();

        // TODO: We may want to adjust the price.
        Item.value = Item.sellPrice(0, 18, 20, 5);
    }
}