using CalamityEntropy;
using CalamityMod;
using CalamityMod.CalPlayer.Dashes;
using CalamityMod.Cooldowns;
using CalamityMod.Items.Accessories;
using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Common.Ui.Cooldowns;
using HeavenlyArsenal.Content.Buffs;
using HeavenlyArsenal.Content.Items.Weapons.Summon.AntishadowAssassin;
using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Content.Particles.Metaballs;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.Utilities.NPCUtils;

namespace HeavenlyArsenal.Content.Items.Armor.ShintoArmor
{
    public class ShintoArmorPlayer : ModPlayer
    {
        #region Values
        public float FauldRotation;
        public float FauldRotationVelocity;

        public bool SetActive;
        public int maxBarrier;
        public int barrier;
        public int timeSinceLastHit;
        public int Iframe;
        public int rechargeDelay;
        public int rechargeRate;
        public int lastHitDamage;

        public float barrierDamageReduction = 0.235f;
        public float barrierSizeInterp = 0;
        public bool ShadowShieldVisible = true;
        public bool ShadowVeil;
        internal float barrierShieldPartialRechargeProgress = 0f;

        //public float InvincibleDashRecharge;
        //public float InvincibleDashRechargeMax = 60 * 3;

        public static readonly int EnrageCooldownMax = 60 * 23;

        public float EnrageInterp = 0;
        public int EnrageTimer;
        public int enrageCooldown;

        public bool Enraged;
        #region s
        /*
            +++++++++++++++++++++++++++++++++++++*+*
            ++++++++++++++++++++++++++++++++++++++++
            ++++++++++++++++++++++++++++++++++++++++
            ++++++++++++++++++++++++++++++++++++++++
            +++++++++++++++++%@%+-...:=%@%++++++++++
            +++++++++++++*%*:.:::.:.-+=.:-=%#+++++++
            +++++++++%@@%-...:+-=-:*++-.:==.:##+++++
            ++++++#@%%@-..:=*+=%=..:=#..:==...+%++++
            ++++#@%#%%....##%@@@=..:=*:..==..*@@*+++
            +++%%##%%.......::*@*@*-=*:..===@+:+%+++
            ++@%###@:.....:=%@+-%::-+++.*%@+-..+%+++
            +@%###%#...:-=*%:+%=+#+:++...=++:.:*#+++
            #@####%*.....-::%=#+..::++:-===-...@%++*
            @#####%*.......::#-:..::++...===...@@+++
            @#####%#.....:+**#:...::=+. .--=.=@@@*++
            @#####%@...::..+-+=...::=+...--=:..#@#++
            %%#####@*.......-*=...::=+. .:==-...%*++
            +@%#####@-.......:::=*=::-...:=-=*.+@*++
            ++%@####%@-.......+#-..::++=+++==..%#+++
            +++*@%####@*........:..::-+::---===*++++
            +++++*%%###%@*..........:-+-.::-:*%+++++
            ++++++++*####%%@#=............:#@#++++++
            +++++*%###########%@@*-@%+++#@@@@%++++++
            +++#%#########################*+*%@@%+++
            +#@%##################*#*+++++++++*****#
            @%#####**++++++**++++*##################
            ++++++++++++*###########################
         */
        #endregion
        public Dictionary<string, CooldownInstance> cooldowns;

        public int ShadowVeilImmunity = 0;
        public bool ChestplateEquipped = false;

        public bool VoidBeltEquipped = false;
        #endregion
        public override void Initialize()
        {

            cooldowns = new Dictionary<string, CooldownInstance>();
            maxBarrier = ShintoArmorBreastplate.ShieldDurabilityMax;
            barrier = 0;
            timeSinceLastHit = 0;
            Iframe = 0;
            rechargeDelay = ShintoArmorBreastplate.ShieldRechargeDelay;
            rechargeRate = ShintoArmorBreastplate.ShieldRechargeRate;

            Enraged = false;
            EnrageTimer = 0;
            enrageCooldown = 0;

            ChestplateEquipped = false;
            VoidBeltEquipped = false;


        }
        public override void Load()
        {

            PlayerDashManager.TryAddDash(new ShintoArmorDash());


        }

        public override void UpdateLifeRegen()
        {
            if (!SetActive)
                return;

            AntishadowHealing();
        }
        public override void PostUpdateMiscEffects()
        {

            if (!SetActive)
                return;

            ManageImmunity();
            ManageBarrier();
            ManageEnraged();
            AntishadowHealing();
            if (Iframe > 0)
            {
                Iframe--;
            }
            if(isShadeTeleporting)
                Player.SetDummyItemTime(2);



        }
        public override void ArmorSetBonusActivated()
        {
            if (SetActive)
                if (enrageCooldown <= 0)
                {

                    Enraged = Enraged == true ? false : true;
                    if (!Enraged)
                    {
                        enrageCooldown = EnrageCooldownMax;
                        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.ErasureRiftClose with { PitchVariance = 0.6f });
                    }
                    else
                    {
                        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.RiftOpen with { PitchVariance = 0.6f });
                        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.ErasureRiftOpen with { PitchVariance = 0.6f });

                    }

                }
                else
                    if (Main.rand.NextBool(250))
                    // Rare chance for ND to laugh at you for being silly
                    SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.Chuckle with { Volume = 1.1f, MaxInstances = 0 });
                else
                {
                    SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.DeadStarTelegraph with { MaxInstances = 0, PitchVariance = 0.25f });
                    SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.ArcticWindGust);
                }


            if (Enraged)
            {
                EnrageTimer = barrier > 0 ? Math.Max(60 * 10, barrier * 10) : 60 * 8;
                if (EnrageTimer > EnrageCooldownMax)
                    EnrageTimer = EnrageCooldownMax;
            }
        }
        private void ManageEnraged()
        {
            if (SetActive)
            {
                if (Enraged)
                {
                    Player.GetDamage<GenericDamageClass>() *= 3;
                    EnrageInterp = float.Lerp(EnrageInterp, 1, 0.4f);
                    DisableAllBarriers();

                    EnrageTimer--;
                    if (EnrageTimer <= 0)
                    {
                        enrageCooldown = EnrageCooldownMax;
                        Enraged = false;
                        EnrageTimer = 0;
                        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.DisgustingStarSever with { PitchVariance = 0.24f, Volume = 0.5f });
                        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.FogRelease with { PitchVariance = 0.5f });
                    }
                    if (Player.Calamity().cooldowns.ContainsKey(EnrageCooldown.ID))
                    {
                        var Salty = Player.Calamity().cooldowns[EnrageTimerVisual.ID];
                        Salty.timeLeft = EnrageTimer;
                    }
                    else
                    {
                        Player.AddCooldown(EnrageTimerVisual.ID, EnrageTimer);

                    }

                }
                else
                {
                    //Main.NewText($"{EnrageTimer}, {enrageCooldown}");
                    EnrageTimer = 0;
                    if (Player.Calamity().cooldowns.ContainsKey(EnrageTimerVisual.ID))
                    {
                        Player.Calamity().cooldowns.Remove(EnrageTimerVisual.ID);

                    }

                    if (timeSinceLastHit == 1 && EnrageInterp > 0.9)
                    {
                        barrier = 0;
                        Player.AddCooldown(BarrierRecharge.ID, ShintoArmorBreastplate.ShieldRechargeDelay);
                        if (Player.Calamity().HasAnyEnergyShield)
                        {

                        }
                    }
                    EnrageInterp = float.Lerp(EnrageInterp, 0, 0.1f);

                    if (enrageCooldown > 0)
                    {
                        if (Player.Calamity().cooldowns.ContainsKey(EnrageCooldown.ID))
                        {
                            var Enraget = Player.Calamity().cooldowns[EnrageCooldown.ID];
                            Enraget.timeLeft = enrageCooldown;
                        }
                        else
                        {
                            Player.AddCooldown(EnrageCooldown.ID, enrageCooldown);

                        }

                        enrageCooldown--;
                    }
                }
            }
            else
            {
                Enraged = false;
                EnrageTimer = 0;
                enrageCooldown = 0;
                EnrageTimer = 0;
            }
        }
        /// <summary>
        /// Prevents the player from being affected by certain debuffs while the Shinto Armor is active.
        /// </summary>
        private void ManageImmunity()
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
                Player.buffImmune[BuffID.BrokenArmor] = true;
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
                //no

                if (ModLoader.TryGetMod("CalamityEntropy", out Mod Entropy))
                {
                    Player.buffImmune[Entropy.Find<ModBuff>("VoidTouch").Type] = true;
                    Player.buffImmune[Entropy.Find<ModBuff>("VoidVirus").Type] = true;
                    Player.buffImmune[Entropy.Find<ModBuff>("Deceive").Type] = true;
                    Player.buffImmune[Entropy.Find<ModBuff>("MaliciousCode").Type] = true;
                }

            }
        }

        /// <summary>
        /// Some barrier logic. 
        /// Not all of it can be in this method, but this is the main logic for the barrier.
        /// </summary>
        private void ManageBarrier()
        {
            if (timeSinceLastHit == 0 && !cooldowns.ContainsKey(BarrierRecharge.ID))
                Player.AddCooldown(BarrierRecharge.ID, ShintoArmorBreastplate.ShieldRechargeDelay);
            if (SetActive)
            {



                if (barrier <= 0)
                    Player.Calamity().cooldowns.Remove(BarrierDurability.ID);
                // Only update recharge progress visuals once the recharge delay has passed.
                if (timeSinceLastHit >= rechargeDelay && barrier > 0 && !cooldowns.ContainsKey(BarrierRecharge.ID))
                {
                    barrierShieldPartialRechargeProgress += ShintoArmorBreastplate.ShieldDurabilityMax / (float)ShintoArmorBreastplate.TotalShieldRechargeTime;
                    int pointsActuallyRecharged = (int)MathF.Floor(barrierShieldPartialRechargeProgress);
                    // durabilityCooldown.timeLeft += 
                    // This value is used only for visual display.
                    int displayBarrier = Math.Min(barrier + pointsActuallyRecharged, TheSponge.ShieldDurabilityMax);

                    barrierShieldPartialRechargeProgress -= 400 - timeSinceLastHit;

                    if (Player.Calamity().cooldowns.TryGetValue(BarrierDurability.ID, out var cdDurability))
                        cdDurability.timeLeft = displayBarrier;
                }

                // Update the durability cooldown display if the shield is active.
                if (barrier > 0 && !cooldowns.ContainsKey(BarrierDurability.ID))
                {
                    if (barrierSizeInterp < 1)
                    {
                        barrierSizeInterp = float.Lerp(barrierSizeInterp, 1, 0.1f);
                    }
                    var durabilityCooldown = Player.AddCooldown(BarrierDurability.ID, ShintoArmorBreastplate.ShieldDurabilityMax);
                    durabilityCooldown.timeLeft = barrier;
                }
            }
            if (!SetActive)
            {
                barrierSizeInterp = float.Lerp(barrierSizeInterp, 0, 0.2f);
                barrier = 0;
                timeSinceLastHit = 0;
                rechargeDelay = ShintoArmorBreastplate.ShieldRechargeDelay;

                Player.Calamity().cooldowns.Remove(BarrierDurability.ID);

                Player.Calamity().cooldowns.Remove(BarrierRecharge.ID);
            }


        }

        private void DisableAllBarriers()
        {
            //there's gotta be a better way to do it than this.

            Player.Calamity().RoverDriveShieldDurability = (int)float.Lerp(Player.Calamity().RoverDriveShieldDurability, -1, 0.4f);
            Player.Calamity().cooldowns.Remove(WulfrumRoverDriveDurability.ID);
            Player.Calamity().cooldowns.Remove(WulfrumRoverDriveRecharge.ID);

            Player.Calamity().pSoulShieldDurability = (int)float.Lerp(Player.Calamity().pSoulShieldDurability, -1, 0.4f);
            Player.Calamity().cooldowns.Remove(ProfanedSoulShield.ID);
            Player.Calamity().cooldowns.Remove(ProfanedSoulShieldRecharge.ID);

            Player.Calamity().SpongeShieldDurability = (int)float.Lerp(Player.Calamity().SpongeShieldDurability, -1, 0.4f);

            Player.Calamity().cooldowns.Remove(SpongeRecharge.ID);
            Player.Calamity().cooldowns.Remove(SpongeDurability.ID);


            cooldowns.Clear();
            barrier = (int)float.Lerp(barrier, -1, 0.14f);
            if (barrier <= 0)
                Player.Calamity().cooldowns.Remove(BarrierDurability.ID);
            barrierSizeInterp = float.Lerp(barrierSizeInterp, -1, 0.14f);
            if (barrierSizeInterp <= 0)
                barrierSizeInterp = 0;
            timeSinceLastHit = 0;
            Player.Calamity().cooldowns.Remove(BarrierRecharge.ID);

          
            
        }
        /// <summary>
        /// Manages the Production of the Antishadow Metaball, as well as the healing rate.
        /// </summary>
        private void AntishadowHealing()
        {

            if (!SetActive)
                return;
            if (Player.StandingStill() && Player.velocity.Length() == 0 && Player.itemAnimation == 0)
            {

                // Actually apply "standing still" regeneration (the stats are granted even at full health)
                float regenTimeNeededForTurboRegen = 40;
                int turboRegenPower = 15 * (1 + (int)Player.lifeRegenTime / 1000);
                // Main.NewText(turboRegenPower);
                if (turboRegenPower > 0)
                {

                    if (Player.lifeRegenTime > regenTimeNeededForTurboRegen && Player.lifeRegenTime < 1800f)
                        Player.lifeRegenTime = 1800f;
                    Player.lifeRegenCount += turboRegenPower;
                    Player.lifeRegenTime += turboRegenPower;
                }

                if (Player.lifeRegen > 0 && Player.statLife < Player.Calamity().actualMaxLife)
                {
                    if (Main.rand.NextBool(1))
                    {
                        AntishadowBlob Blob = ModContent.GetInstance<AntishadowBlob>();
                        for (int i = 0; i < 1; i++)

                        {
                            float randomoffset = Main.rand.Next(-4, 4);
                            Vector2 bloodSpawnPosition = Player.Center + Main.rand.NextVector2CircularEdge(120 + randomoffset, 120 + randomoffset);

                            //var dust = Dust.NewDustPerfect(bloodSpawnPosition, DustID.AncientLight, Vector2.Zero, default, Color.Red);
                            //dust.noGravity = true;
                            Blob.player = Player;

                            Blob.CreateParticle(bloodSpawnPosition, Vector2.Zero, 0, 0);
                        }
                    }


                }



            }

        }

        #region Barrier stuff
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (SetActive)
            {
                if (barrier > 0 && Iframe <= 0)
                {
                    modifiers.DisableSound();

                    if (lastHitDamage > maxBarrier / 3)
                    {
                        SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Armor.Antishield_Hit with { Pitch = 0.2f }, Player.Center);
                    }
                    else
                        SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Armor.Antishield_Hit, Player.Center);
                }
                else if (barrier <= 0)
                {
                    if (timeSinceLastHit! < 0)
                    {
                        if (lastHitDamage >= maxBarrier)
                        {
                            SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Armor.Antishield_Break with { Volume = 1, Pitch = 1.1f }, Player.Center);

                        }
                        else
                            SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Armor.Antishield_Break, Player.Center);
                        barrier = 0;

                    }


                }

                if (barrier <= 0 && Enraged)
                {
                    modifiers.FinalDamage *= 2.5f;
                }
            }

        }

        public void BarrierTakeDamageVFX()
        {
            for (int i = 0; i < Main.rand.Next(1, 5); i++)
            {
                Vector2 lightningPos = Player.Center + Main.rand.NextVector2Circular(24, 24);

                HeatLightning particle = HeatLightning.pool.RequestParticle();
                particle.Prepare(lightningPos, Player.velocity + Main.rand.NextVector2Circular(10, 10), Main.rand.NextFloat(-2f, 2f), 10 + i * 3, Main.rand.NextFloat(0.5f, 1f));
                ParticleEngine.Particles.Add(particle);
            }
        }





        #endregion
        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (VoidBeltEquipped && Player.GetModPlayer<ShintoArmorBarrier>().barrier <= 0 && Main.rand.NextBool(8))
            {
                VoidBelt();
                return true;
            }


            if (barrier > 0 && SetActive)
            {

                int incoming = info.SourceDamage;

                if (Iframe <= 0)
                {

                    int TakenDamage = (int)Math.Round(incoming * barrierDamageReduction, 0);

                    if (TakenDamage > 0)
                    {
                        timeSinceLastHit = 0;
                    }
                    if (TakenDamage > ShintoArmorBreastplate.ShieldDurabilityMax && barrier == ShintoArmorBreastplate.ShieldDurabilityMax)
                    {
                        GeneralScreenEffectSystem.ChromaticAberration.Start(Player.Center, 3f, 90);
                        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.AngryDistant with { Volume = 0.05f }, Player.Center);

                    }
                    Iframe = (int)(Player.ComputeHitIFrames(info) * 1.25f);
                    if (info.DamageSource.SourceProjectileType.Equals(ModContent.ProjectileType<StolenPlanetoid>()))
                    {


                        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Clap);
                        barrier = 0;
                        TakenDamage = 999999;
                    }
                    else

                        barrier -= TakenDamage;
                    BarrierTakeDamageVFX();
                    CombatText.NewText(Player.Hitbox, Color.Cyan, TakenDamage);


                }



                // Cancel all damage to the player.
                return true;
            }
            else if (barrier <= 0 && timeSinceLastHit < Iframe)
            {
                return true;
            }

            else
                return false;
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

        public void VoidBelt()
        {

            GeneralScreenEffectSystem.RadialBlur.Start(Player.Center, 3f, 90);
            Player.SetImmuneTimeForAllTypes(120);
            Player.AddBuff(ModContent.BuffType<AntishadowRetaliation>(), 6 * 65, false);
            for (int i = 0; i < 40; i++)
            {
                Dust.NewDust(Player.Center, Player.width, Player.height, DustID.BunnySlime, 0, 0, 100, Color.Crimson, 1);

            }
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.AngryDistant with { Volume = 0.4f, PitchVariance = 0.3f }, Player.Center);
            for (int i = 0; i < 70; i++)
            {
                int fireBrightness = Main.rand.Next(100);
                Color fireColor = new Color(fireBrightness, fireBrightness, fireBrightness);
                if (Main.rand.NextBool(3) && Player.velocity.X > 20)
                    fireColor = new Color(220, 20, Main.rand.Next(16), 255);

                Vector2 position = Player.Center + Main.rand.NextVector2Circular(30f, 30f);
                AntishadowFireParticleSystemManager.CreateNew(Player.whoAmI, false, position, Main.rand.NextVector2Circular(30f, Player.velocity.X * .96f), Vector2.One * Main.rand.NextFloat(30f, 50f), fireColor);
            }

        }

        
        public override void PostUpdateEquips()
        {
            if (!SetActive)
                return;

            if (barrier > 0)
            {
                Player.statDefense += 30;
            }

            int wingSlot = EquipLoader.GetEquipSlot(Mod, "ShintoArmorWings", EquipType.Wings);
            bool thing = Player.equippedWings == null;

            if (thing)
            {
                Player.wings = ShintoArmorWings.WingSlotID;
                //Player.wings = wingSlot;
                Player.wingsLogic = wingSlot;


                Player.wingTime = 1000;
                Player.wingTimeMax = 1000;
               // Player.equippedWings = Player.armor[1];

                
                if (ModLoader.HasMod("CalamityMod"))
                {
                    ModLoader.GetMod("CalamityMod").Call("ToggleInfiniteFlight", Player, true);
                }
                Player.noFallDmg = true;
            }



        }

   
        public bool JustTeleported;
        public bool isStillInShadow;
        public bool isShadeTeleporting;
        public float ShadeTeleportInterpolant;

        public override void PreUpdateMovement()
        {
            if (!SetActive)
                return;


            if (JustTeleported)
            {
                float thing = 0.2f;
                ShadeTeleportInterpolant = MathF.Round(float.Lerp(ShadeTeleportInterpolant, 0, thing),5);
                if(ShadeTeleportInterpolant <= 0.01f)
                {
                    JustTeleported = false;
                }
                Player.velocity = Vector2.Zero;
                return;
            }
            if (isShadeTeleporting || ShadeTeleportInterpolant !< 0)
            {
              
                Player.velocity = Vector2.Zero;
                ShadeTeleport();
            }



        }
        public void ShadeTeleport()
        {
            if (!SetActive)
            {
                isShadeTeleporting = false;
                return;
            }
           
            int chaosStateDuration = 900;

            if (!Player.chaosState)
            {
                Vector2 startPos = Player.Center;
                Point startTile = startPos.ToTileCoordinates();

                // Look down 4 tiles for solid ground
                bool groundBelow = false;
                for (int y = 0; y <= 4; y++)
                {
                    if (WorldGen.SolidTile(startTile.X, startTile.Y + y))
                    {
                        groundBelow = true;
                        break;
                    }
                }

                if (groundBelow)
                {
                    offset = Player.height*1.2f;
                }
                else
                {
                    offset = 0f;
                }
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
                Point targetTile = teleportLocation.ToTileCoordinates();
                for (int y = 0; y < 10; y++)
                {
                    if (WorldGen.SolidTile(targetTile.X, targetTile.Y + y))
                    {
                        teleportLocation.Y = (targetTile.Y + y - 3) * 16f + 6;
                        break;
                    }
                }
                if (teleportLocation.X > 50f && teleportLocation.X < Main.maxTilesX * 16 - 50 && teleportLocation.Y > 50f && teleportLocation.Y < Main.maxTilesY * 16 - 50)
                {
                    if (ShadeTeleportInterpolant >= 0.99f && isShadeTeleporting)
                        if (!Collision.SolidCollision(teleportLocation, Player.width, Player.height))
                        {
                            

                            Player.Teleport(teleportLocation, 20);
                            if (!WorldGen.SolidTile(targetTile.X, targetTile.Y + 4))
                                offset = 0;
                            isShadeTeleporting = false;
                            JustTeleported = true;
                            NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, Player.whoAmI, teleportLocation.X, teleportLocation.Y, 1, 0, 0);

                            int duration = chaosStateDuration;
                            
                            int numDust = 40;
                            Vector2 step = teleportOffset / numDust;




                            ShadowVeilImmunity = ShintoArmorHelmetAll.ShadowVeilIFrames;
                        }
                }

            }
            if (isShadeTeleporting)
                ShadeTeleportInterpolant = float.Lerp(ShadeTeleportInterpolant, 1, 0.2f);

        }
        public override void FrameEffects()
        {
            if (Player.HasBuff<AntishadowRetaliation>())
            {

            }

        }
        public float offset = 45;
        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            /*
            if (isShadeTeleporting || JustTeleported)
            {
                drawInfo.cWings = 0;
                drawInfo.drawsBackHairWithoutHeadgear = false;
                drawInfo.drawPlayer.armorEffectDrawOutlines = false;
                drawInfo.torsoOffset = offset * ShadeTeleportInterpolant;
                drawInfo.mountOffSet = offset * ShadeTeleportInterpolant;
                drawInfo.seatYOffset = offset * ShadeTeleportInterpolant;
                drawInfo.legsOffset = new Vector2(0, offset * ShadeTeleportInterpolant);
               
                drawInfo.itemColor = Color.Transparent;


                drawInfo.colorBodySkin = Color.Black;
            }
            */
        }
        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            /*

            if (isShadeTeleporting || JustTeleported)
            {
                r = float.Lerp(r, 0, MathF.Round(ShadeTeleportInterpolant, 3));
                g = r;
                b = r;
                fullBright = false;
                //a = float.Lerp(a, 0, MathF.Round(ShadeTeleportInterpolant, 3));
            }*/
        }
        public override void ResetEffects()
        {
            if (!SetActive && barrier > 0)
            {
                barrier = 0;
            }
            SetActive = false;
            ChestplateEquipped = false;
            VoidBeltEquipped = false;
            if (Player.statLife <= 0)
            {
                Enraged = false;
                enrageCooldown = EnrageCooldownMax;
            }

        }

    }
}
