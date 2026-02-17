using Terraria.ModLoader.IO;

namespace HeavenlyArsenal.Common.Players;

public sealed class TransgenderPlayer : ModPlayer
{
    /// <summary>
    ///     Gets or sets whether the player is transgender.
    /// </summary>
    public bool Enabled { get; set; }

    public override void SaveData(TagCompound tag)
    {
        base.SaveData(tag);

        // TODO: Breaking change warning: Changed tag name to a constant.
        tag["HasUsedGenderSwapPotion"] = Enabled;
    }

    public override void LoadData(TagCompound tag)
    {
        base.LoadData(tag);

        // TODO: Breaking change warning: Changed tag name to a constant.
        Enabled = tag.GetBool("HasUsedGenderSwapPotion");
    }

    public override void CopyClientState(ModPlayer targetCopy)
    {
        base.CopyClientState(targetCopy);

        if (targetCopy is not TransgenderPlayer clone)
        {
            return;
        }

        clone.Enabled = Enabled;
    }

    public override void SendClientChanges(ModPlayer clientPlayer)
    {
        base.SendClientChanges(clientPlayer);

        if (clientPlayer is not TransgenderPlayer clone || clone.Enabled == Enabled)
        {
            return;
        }

        SyncPlayer(-1, Main.myPlayer, false);
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        base.SyncPlayer(toWho, fromWho, newPlayer);
        
        // TODO: Use a ModPacket to synchronize player properties.
    }
}