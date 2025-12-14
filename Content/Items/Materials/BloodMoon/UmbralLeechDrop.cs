//using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech;
using HeavenlyArsenal.Content.Rarities;

namespace HeavenlyArsenal.Content.Items.Materials.BloodMoon;

internal class UmbralLeechDrop : ModItem
{
    public override string LocalizationCategory => "Items.Misc";

    public override void SetDefaults()
    {
        Item.maxStack = Item.CommonMaxStack;
        Item.width = 38;
        Item.height = 32;
        Item.BestiaryNotes = "A leech that has fed on the blood of many. It is said that these leeches can drain the life force of even the strongest of beings.";
        Item.rare = ModContent.RarityType<BloodMoonRarity>();
        Item.sellPrice(0, 18, 20, 5);
    }
}