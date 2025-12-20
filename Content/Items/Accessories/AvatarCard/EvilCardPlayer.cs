using System.Collections.Generic;

namespace HeavenlyArsenal.Content.Items.Accessories.AvatarCard;

public sealed class EvilCardPlayer : ModPlayer
{
    /// <summary>
    ///     Gets or sets whether the Evil Card effects are enabled.
    /// </summary>
    public bool Enabled { get; set; }

    public override void ResetEffects()
    {
        base.ResetEffects();
        
        Enabled = false;
    }

    public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
    {
        return inventory[slot].type == ModContent.ItemType<EvilCardItem>();
    }

    public override void ModifyLuck(ref float luck)
    {
        base.ModifyLuck(ref luck);
        
        if (!Enabled)
        {
            return;
        }

        luck -= 2230.2f;
    }

    public override bool CanSellItem(NPC vendor, Item[] shopInventory, Item item)
    {
        return item.type != ModContent.ItemType<EvilCardItem>();
    }

    public override void AnglerQuestReward(float rareMultiplier, List<Item> rewardItems)
    {
        base.AnglerQuestReward(rareMultiplier, rewardItems);
        
        if (!Enabled)
        {
            return;
        }
        
        rareMultiplier *= 0.1f;
        rewardItems.Add(new Item(ItemID.CursedFlame));
    }

    public override void PostUpdate()
    {
        base.PostUpdate();
        
        if (!Enabled)
        {
            return;
        }
        
        Player.AddBuff(BuffID.Cursed, 2);
        Player.AddBuff(BuffID.Darkness, 2);
        Player.AddBuff(BuffID.Weak, 2);
        Player.AddBuff(BuffID.Silenced, 2);
        Player.AddBuff(BuffID.BrokenArmor, 2);
    }
}