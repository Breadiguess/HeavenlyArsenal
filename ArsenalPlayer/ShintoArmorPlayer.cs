using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Cooldowns;
using CalamityMod.Items.Accessories;
using CalamityMod;
using CalamityMod.CalPlayer;
using HeavenlyArsenal.Content.Items.Armor;
using System.Text;
using Terraria.Audio;
using System.Collections.Generic;
using CalamityMod.CalPlayer.Dashes;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace HeavenlyArsenal.ArsenalPlayer
{
    class ShintoArmorPlayer : ModPlayer
    {
        public bool SetActive;
        public int maxBarrier;
        public int barrier;
        public int timeSinceLastHit;
        public int Iframe;
        public int rechargeDelay;
        public int rechargeRate;
        public float barrierDamageReduction = 0.78f;
        public bool ShadowShieldVisible = true;
        public bool ShadowVeil;
        internal float barrierShieldPartialRechargeProgress = 0f;

        public bool IsDashing;
        public bool empoweredDash;


        public Dictionary<string, CooldownInstance> cooldowns;

        public int ShadowVeilImmunity = 0;

        public const int segmentCount = 7;
        public const float segmentLength = 5f;
        public Vector2[] verletPoints;
        public Vector2[] verletOldPoints;
        private bool verletInitialized = false;
        public static Asset<Texture2D> chainTexture;
        public bool ChestplateEquipped = false;

        public override void Initialize()
        {

            cooldowns = new Dictionary<string, CooldownInstance>();
            maxBarrier = ShintoArmorBreastplate.ShieldDurabilityMax;
            barrier = 0;
            timeSinceLastHit = 0;
            Iframe = 0;
            rechargeDelay = ShintoArmorBreastplate.ShieldRechargeDelay;
            rechargeRate = ShintoArmorBreastplate.ShieldRechargeRate;
            // Initialize Verlet arrays

            ChestplateEquipped = false;
            IsDashing = false;
            empoweredDash = false;

            verletPoints = new Vector2[segmentCount];
            verletOldPoints = new Vector2[segmentCount];
            verletInitialized = false; // Will initialize on first update when Player is valid.

        }
        public override void Load()
        {
           
            PlayerDashManager.TryAddDash(new ShintoArmorDash());
            PlayerDashManager.TryAddDash(new AbyssDash());
        }

        public override void PostUpdateMiscEffects()
        {
            if (SetActive)
            {
                //Main.NewText($"Barrier: {barrier}, TimeSinceLastHit: {timeSinceLastHit}", Color.AntiqueWhite);
                Player.buffImmune[BuffID.Silenced] = true;
                Player.buffImmune[BuffID.Cursed] = true;
                Player.buffImmune[BuffID.OgreSpit] = true;
                Player.buffImmune[BuffID.Frozen] = true;
                Player.buffImmune[BuffID.Webbed] = true;
                Player.buffImmune[BuffID.Stoned] = true;
                Player.buffImmune[BuffID.VortexDebuff] = true;
                Player.buffImmune[BuffID.Electrified] = true;
                Player.buffImmune[BuffID.Burning] = true;
                Player.buffImmune[BuffID.Stinky] = true;
                Player.buffImmune[BuffID.Dazed] = true;
                Player.buffImmune[BuffID.Venom] = true;
                Player.buffImmune[BuffID.CursedInferno] = true;
                Player.buffImmune[BuffID.OnFire] = true;
                Player.buffImmune[BuffID.Weak] = true;
                if (ModLoader.TryGetMod("Calamity", out Mod CalamityMod))
                {
                    Mod calamity = ModLoader.GetMod("CalamityMod");
                    Player.buffImmune[calamity.Find<ModBuff>("Clamity").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("Dragonfire").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("DoGExtremeGravity").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("FishAlert").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("GlacialState").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("GodSlayerInferno").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("HolyFlames").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("IcarusFolly").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("MiracleBlight").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("Nightwither").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("Plague").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("VulnerabilityHex").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("Warped").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("WeakPetrification").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("WhisperingDeath").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("FabsolVodkaBuff").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("FrozenLungs").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("PopoNoselessBuff").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("SearingLava").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("ShellfishClaps").Type] = true;
                    Player.buffImmune[calamity.Find<ModBuff>("BrimstoneFlames").Type] = true;
                    calamity.Call("SetWearingRogueArmor", Player, true);
                    calamity.Call("SetWearingPostMLSummonerArmor", Player, true);
                }
            }
            else
            {
                barrier = 0;
                timeSinceLastHit = 0;
                rechargeDelay = ShintoArmorBreastplate.ShieldRechargeDelay;
            }
            if (SetActive)
            {
                // Begin visual cooldown handling for shield recharge.

                // If the shield is completely discharged and not recharging, start the recharge cooldown.
                if (timeSinceLastHit == 0 && !cooldowns.ContainsKey(BarrierRecharge.ID))
                    Player.AddCooldown(BarrierRecharge.ID, ShintoArmorBreastplate.ShieldRechargeDelay);

                // Update the durability cooldown display if the shield is active.
                if (barrier > 0 && !cooldowns.ContainsKey(BarrierDurability.ID))
                {
                    var durabilityCooldown = Player.AddCooldown(BarrierDurability.ID, ShintoArmorBreastplate.ShieldDurabilityMax);
                    durabilityCooldown.timeLeft = barrier;
                }

                // Only update recharge progress visuals once the recharge delay has passed.
                if (timeSinceLastHit >= rechargeDelay && barrier > 0 && !cooldowns.ContainsKey(BarrierRecharge.ID))
                {
                    barrierShieldPartialRechargeProgress += ShintoArmorBreastplate.ShieldDurabilityMax / (float)ShintoArmorBreastplate.TotalShieldRechargeTime;
                    int pointsActuallyRecharged = (int)MathF.Floor(barrierShieldPartialRechargeProgress);
                   // durabilityCooldown.timeLeft += 
                    // This value is used only for visual display.
                    int displayBarrier = Math.Min(barrier + pointsActuallyRecharged, TheSponge.ShieldDurabilityMax);

                    barrierShieldPartialRechargeProgress -= 400 - timeSinceLastHit;

                    if (cooldowns.TryGetValue(BarrierDurability.ID, out var cdDurability))
                        cdDurability.timeLeft = displayBarrier;
                }
            }

            if (Iframe > 0)
            {
                Iframe--;
            }

            
            if (ShadowVeil)
            {
                CalamityPlayer modPlayer = Player.Calamity();
                bool wearingRogueArmor = modPlayer.wearingRogueArmor;
                float rogueStealth = modPlayer.rogueStealth;
                float rogueStealthMax = modPlayer.rogueStealthMax;
                int chaosStateDuration = 900;

                if (CalamityKeybinds.SpectralVeilHotKey.JustPressed && ShadowVeil && Main.myPlayer == Player.whoAmI && rogueStealth >= rogueStealthMax * 0.25f &&
                    wearingRogueArmor && rogueStealthMax > 0)
                {
                    if (!Player.chaosState)
                    {
                        Vector2 teleportLocation;
                        teleportLocation.X = Main.mouseX + Main.screenPosition.X;
                        if (Player.gravDir == 1f)
                            teleportLocation.Y = Main.mouseY + Main.screenPosition.Y - Player.height;
                        else
                            teleportLocation.Y = Main.screenPosition.Y + Main.screenHeight - Main.mouseY;

                        teleportLocation.X -= Player.width * 0.5f;
                        Vector2 teleportOffset = teleportLocation - Player.position;
                        if (teleportOffset.Length() > SpectralVeil.TeleportRange)
                        {
                            teleportOffset = teleportOffset.SafeNormalize(Vector2.Zero) * SpectralVeil.TeleportRange;
                            teleportLocation = Player.position + teleportOffset;
                        }
                        if (teleportLocation.X > 50f && teleportLocation.X < (float)(Main.maxTilesX * 16 - 50) && teleportLocation.Y > 50f && teleportLocation.Y < (float)(Main.maxTilesY * 16 - 50))
                        {
                            if (!Collision.SolidCollision(teleportLocation, Player.width, Player.height))
                            {
                                rogueStealth -= rogueStealthMax * 0.25f;

                                Player.Teleport(teleportLocation, 1);
                                NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, (float)Player.whoAmI, teleportLocation.X, teleportLocation.Y, 1, 0, 0);

                                int duration = chaosStateDuration;
                                Player.AddBuff(BuffID.ChaosState, duration, true);
                                Player.AddCooldown(ChaosState.ID, duration, true, "spectralveil");

                                int numDust = 40;
                                Vector2 step = teleportOffset / numDust;
                                for (int i = 0; i < numDust; i++)
                                {
                                    int dustIndex = Dust.NewDust(Player.Center - (step * i), 1, 1, DustID.VilePowder, step.X, step.Y);
                                    Main.dust[dustIndex].noGravity = true;
                                    Main.dust[dustIndex].noLight = true;
                                }

                                ShadowVeilImmunity = ShintoArmorHelmetAll.ShadowVeilIFrames;
                            }
                        }
                    }
                }
            }
           

            if (IsDashing)
            {
                Main.NewText($"Im doing things!", Color.Coral);
            }

            if (cooldowns.ContainsKey(AbyssDashCooldown.ID)) 
            {
                empoweredDash = true;
            }
            else
            {
                empoweredDash = false;
            }
        }



        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (SetActive && barrier > 0 && Iframe <= 0)
            {
                modifiers.DisableSound();
                SoundEngine.PlaySound(ShintoArmorBreastplate.ShieldHurtSound, Player.Center);
            }
            else if (SetActive && barrier <= 0)
            {
                if(timeSinceLastHit !< 0)
                {
                    SoundEngine.PlaySound(ShintoArmorBreastplate.BreakSound, Player.Center);
                    barrier = 0;
                }

                
            }
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (barrier > 0 && SetActive)
            {
                int incoming = info.Damage;

                if (Iframe <= 0)
                {
                    int actualDamage = (int)Math.Round(incoming * barrierDamageReduction, 0);
                    Iframe = Player.ComputeHitIFrames(info);
                    barrier -= actualDamage;
                    CombatText.NewText(Player.Hitbox, Color.Cyan, actualDamage);
                    Main.NewText($"Barrier: {barrier}, TimeSinceLastHit: {timeSinceLastHit}", Color.AntiqueWhite);
                    timeSinceLastHit = 0;
                }

                if (barrier < 0)
                {
                    barrier = 0;
                    return true;
                }

                // Cancel all damage to the player.
                return true;
            }
            else if(barrier== 0 && timeSinceLastHit < Iframe)
            {
                return true;
            }
            else
                return false;
        }

        public override void PostUpdateEquips()
        {
            if (barrier > 0 && SetActive)
            {
                Player.statDefense += 30;
            }
        }

        public override void UpdateBadLifeRegen()
        {
            // Increment time since last hit only when the shield is active.
            if (maxBarrier > 0 && SetActive)
                timeSinceLastHit++;

            // Only update the actual recharge if the recharge delay has passed and no recharge cooldown is active.
            if (!cooldowns.ContainsKey(BarrierRecharge.ID) && timeSinceLastHit >= rechargeDelay && barrier < maxBarrier && SetActive)
            {
                int rechargeRateWhole = rechargeRate / 60;
                barrier += Math.Min(rechargeRateWhole, maxBarrier - barrier);

                if (rechargeRate % 60 != 0)
                {
                    int rechargeSubDelay = 60 / (rechargeRate % 60);
                    
                    //starts recharging at 400 timesince
                    if (timeSinceLastHit % rechargeSubDelay == 0 && barrier < maxBarrier)
                    {
                        //Main.NewText($"Here: {timeSinceLastHit}", Color.AntiqueWhite);
                        
                        barrier++;
                    }

                }
            }
        }

        public override void ResetEffects()
        {
            IsDashing = false;
            SetActive = false;
            ChestplateEquipped = false;

        }
    }
}
