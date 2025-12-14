namespace HeavenlyArsenal.Content.Items.Armor.Vanity.DevSet;

[AutoloadEquip(EquipType.Head)]
internal class devMask : ModItem
{
    public override void SetStaticDefaults()
    {
        //ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
        ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
    }

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;

        Item.vanity = true;
    }
}