using System;
using CalamityMod.CalPlayer;
using CalamityMod.Cooldowns;
using CalamityMod.Items.Accessories;
using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;
using static Terraria.ModLoader.ModContent;

namespace HeavenlyArsenal.Common.Ui.Cooldowns
{

    public class BarrierRecharge : CooldownHandler
    {
        private static Color ringColorLerpStart = new Color(0, 0, 0);
        private static Color ringColorLerpEnd = new Color(220, 220, 220);

        public static new string ID => "BarrierRecharge";
        public override bool ShouldDisplay => true;
        public override LocalizedText DisplayName => Language.GetOrRegister("AntiShield Inversion");
        public override string Texture => "HeavenlyArsenal/Assets/Textures/UI/Cooldowns/BarrierRecharge_Icon";
        public override string OutlineTexture => "HeavenlyArsenal/Assets/Textures/UI/Cooldowns/BarrierRecharge_Outline";
        public override string OverlayTexture => "HeavenlyArsenal/Assets/Textures/UI/Cooldowns/BarrierRecharge_Overlay";
        public override bool SavedWithPlayer => false;
        public override bool PersistsThroughDeath => false;
        public override Color OutlineColor => new Color(133, 204, 237);
        public override Color CooldownStartColor => Color.Lerp(ringColorLerpStart, ringColorLerpEnd, instance.Completion);
        public override Color CooldownEndColor => Color.Lerp(ringColorLerpStart, ringColorLerpEnd, instance.Completion);
        public override SoundStyle? EndSound => AssetDirectory.Sounds.Items.Armor.Antishield_Regen;
        public override bool ShouldPlayEndSound => true;

        
        //public override void Tick() => instance.player.Calamity().playedSpongeShieldSound = false;
        // When the recharge period completes, grant 1 point of shielding immediately so the rest my refill normally.
        // The shield durability cooldown is added elsewhere, in Misc Effects.
        public override void OnCompleted()
        {
            SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Armor.Antishield_Regen);
        }
    }
}
