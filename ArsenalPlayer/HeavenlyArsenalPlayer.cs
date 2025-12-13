using CalamityMod;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using CalamityMod.CalPlayer.Dashes;
using HeavenlyArsenal.Content.Items.Accessories.Vambrace;







namespace HeavenlyArsenal.ArsenalPlayer
{
    public partial class HeavenlyArsenalPlayer : ModPlayer
    {
        public int AvatarRifleCounter = 7;

        public float CessationHeat = 0;
       
        public bool CessationHeld;
        public bool hasAvatarRifle { 
            get; 
            private set; 
        }

        public override void Load()
        {

        }

        public override void PostUpdate()
        { 
        }


        public override void PostUpdateMiscEffects()
        {
            //Main.NewText(Player.GetModPlayer<GenderSwapPlayer>().IsTrans.ToString());
        }
        public override void ResetEffects()
        {
            CessationHeld = false;
            hasAvatarRifle = false;
        }
    }
}