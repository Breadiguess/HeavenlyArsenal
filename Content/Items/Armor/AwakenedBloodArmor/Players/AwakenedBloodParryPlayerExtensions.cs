namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor.Players;

public static class AwakenedBloodParryPlayerExtensions
{
    public static void Parry(this Player player, int time)
    {
        var awakenedBloodPlayer = player.GetModPlayer<AwakenedBloodPlayer>();
        
        if (awakenedBloodPlayer.Form != AwakenedBloodForm.Defense)
        {
            return;
        }
        
        var awakenedBloodParryPlayer = player.GetModPlayer<AwakenedBloodParryPlayer>();

        awakenedBloodParryPlayer.Time = time;

        awakenedBloodParryPlayer.Parry();
    }
}