using HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor.Projectiles;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor.Players
{
    internal class AwakenedBloodPlayer_Parry : ModPlayer
    {
        public void HandleParry()
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.Common.MediumBloodSpill, Player.Center);
            for(int i = 0; i< 6; i++)
            {

                Vector2 Velocity = new Vector2(1, 0).RotatedBy(i / 6f * MathHelper.TwoPi).RotatedByRandom(MathHelper.ToRadians(12));
                Projectile.NewProjectileDirect(Player.GetSource_FromThis(), Player.Center, Velocity, ModContent.ProjectileType<AwakenedBlood_ParryThorn>(), 100, 0);
            }
        }
        public int ParryTime { get; set; }
        internal const int bloodThornParry = 30;
        public bool IsParrying
        {
            get => ParryTime > 0;
        }
        public override void PostUpdateMiscEffects()
        {
            if(ParryTime>0)
                ParryTime--;
            
        }
        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            if(IsParrying)
            {
                HandleParry();
            }
        }
        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            if(IsParrying)
            {
                HandleParry();
            }
        }
        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if(IsParrying)
            {
                modifiers.FinalDamage *= 0f;
            }
        }

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if(IsParrying)
            {
                modifiers.FinalDamage *= 0f;
            }
        }

        public static void AttemptParry(Player player)
        {
            if(player.GetModPlayer<AwakenedBloodPlayer>().CurrentForm != AwakenedBloodPlayer.Form.Defense)
                return;
            player.GetModPlayer<AwakenedBloodPlayer_Parry>().ParryTime = bloodThornParry;

            player.GetModPlayer<AwakenedBloodPlayer_Parry>().HandleParry();
        }
    }
}
