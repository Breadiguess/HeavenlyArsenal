using System.Collections.Generic;
using NoxusBoss.Content.Rarities;

namespace HeavenlyArsenal.Content.Items.Accessories.AvatarCard;

public class EvilCard : ModItem
{
    public override string LocalizationCategory => "Items.Accessories";

    public override void SetDefaults()
    {
        Item.DefaultToAccessory();
        Item.rare = ModContent.RarityType<AvatarRarity>();
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<EvilCardPlayer>().evilCard = true;
    }
}

public class EvilCardPlayer : ModPlayer
{
    public bool evilCard;

    public override void ResetEffects()
    {
        evilCard = false;
    }

    public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
    {
        if (inventory[slot].type == ModContent.ItemType<EvilCard>())
        {
            return true;
        }

        return base.ShiftClickSlot(inventory, context, slot);
    }

    public override void ModifyLuck(ref float luck)
    {
        if (evilCard)
        {
            luck -= 2230.2f;
        }

        base.ModifyLuck(ref luck);
    }

    public override bool CanSellItem(NPC vendor, Item[] shopInventory, Item item)
    {
        if (item.type == ModContent.ItemType<EvilCard>())
        {
            //no selling!
            return false;
        }

        return base.CanSellItem(vendor, shopInventory, item);
    }

    public override void AnglerQuestReward(float rareMultiplier, List<Item> rewardItems)
    {
        if (evilCard)
        {
            rareMultiplier *= 0.1f;
            rewardItems.Add(new Item(ItemID.CursedFlame));
        }
    }

    public override void PostUpdate()
    {
        if (evilCard)
        {
            Player.AddBuff(BuffID.Cursed, 2);
            Player.AddBuff(BuffID.Darkness, 2);
            Player.AddBuff(BuffID.Weak, 2);
            Player.AddBuff(BuffID.Silenced, 2);
            Player.AddBuff(BuffID.BrokenArmor, 2);
        }
    }
}