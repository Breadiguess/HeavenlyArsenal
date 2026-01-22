using CalamityMod;
using HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Mage;
using HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Melee;
using HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Ranged;
using HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Rogue;
using HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Summoner;
using Luminance.Core.Sounds;
using NoxusBoss.Assets;
using System.Collections.Generic;
using Terraria;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players
{
    public class BloodBlightParasite_Player : ModPlayer
    {
        #region Values

        public bool Active;
        internal float _bloodSaturation;
        /// <summary>
        /// resource representing the current saturation of the blood parasite
        /// </summary>
        public float BloodSaturation
        {
            get => _bloodSaturation;
            set => _bloodSaturation = Math.Clamp(value, 0f, BloodSaturationMax);
        }
        public float BloodSaturationMax = 100f;

        public BloodState CurrentState;       // Dormant / Bloom / Ascension / Crash
        public BloodBand CurrentBand;         // 0–30 / 30–50 / 50–70 / 70–100

        // ===== Morph Tracking =====
        public DamageClass DominantClass;
        public int DominantClassTimer;        // frames since last dominant hit
        public const int DominantClassHold = 180; // ~3 seconds

        public bool InCombat
        {
            get => CombatTimer > 0;
        }
        public int CombatTimer;

        public int AscensionTimer;
        public int CrashTimer;
        public bool IsCrashing
        {
            get => CrashTimer > 0;
        }

        public IBloodConstructController ConstructController;

        public DamageClass CurrentMorph;

        // ===== Cooldowns / Flags =====
        public int CoagulateCooldown;
        public int ActiveAbilityCooldown;


        public float OutOfCombatDecay = 0.6f;
        public float CombatGainRate = 0.01f;
        public bool Ascended { get; private set; }

        public int MaxAscentionTimer = 10 * 60;
        #endregion

        #nullable enable
        public LoopedSoundInstance? AscensionSound
        {
            get;
            private set;
        }


        public override void Load()
        {
            On_Player.ItemCheck_PayMana += On_Player_ItemCheck_PayMana;
            On_Player.GetManaCost += On_Player_GetManaCost;
        }

        private static int On_Player_GetManaCost(On_Player.orig_GetManaCost orig, Player self, Item item)
        {
            var symbiote = self.GetModPlayer<BloodBlightParasite_Player>();

            if(!symbiote.Active)
                return orig(self, item);





            return orig(self, item);
        }

        private static bool On_Player_ItemCheck_PayMana(On_Player.orig_ItemCheck_PayMana orig, Player self, Item sItem, bool canUse)
        {

            var symbiote = self.GetModPlayer<BloodBlightParasite_Player>();

            if (!symbiote.Active)
                return orig(self, sItem, canUse);


            if(symbiote.ConstructController is MagicBloodController && symbiote.Ascended)
            {
                return canUse;
            }
            return orig(self, sItem, canUse);
        }

        public override void Initialize()
        {
            CurrentMorph = DamageClass.Default;
        }

        public override void PostUpdateMiscEffects()
        {
            if(!Active)
                return;

            ConstructController?.Update(Player);
            UpdateBloodSaturation();
            CheckBand();
            UpdateMorph();

            if (CrashTimer > 0)
                CrashTimer--;
            if (CombatTimer > 0)
                CombatTimer--;

            UpdateBloodState();

            float SoundInterpolant = 1- LumUtils.InverseLerp(0, MaxAscentionTimer, AscensionTimer);
            if (AscensionSound != null)
            {
                AscensionSound.Update(Player.Center, sound =>
                {
                    sound.Pitch = -2 * SoundInterpolant;
                    sound.Volume = 2.4f * SoundInterpolant;
                });
            }





            if(Player.HeldItem.DamageType.CountsAsClass(ModContent.GetInstance<RogueDamageClass>()))
            {
                Player.Calamity().wearingRogueArmor = true;
                Player.Calamity().rogueStealthMax += 1.3f;
            }
        }


        



        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!Active)
                return;
        }
        public override void OnHitAnything(float x, float y, Entity victim)
        {
            if (!Active)
                return;
            CombatTimer = 8 * 60;
        }

        public override void ResetEffects()
        {
            Active = false;
        }


      

        public static void AttemptPurge(Player player)
        {
            player.GetModPlayer<BloodBlightParasite_Player>().ConstructController?.OnPurge();
            player.GetModPlayer<BloodBlightParasite_Player>().BloodSaturation -= 40;
        }

        #region helpers
        void UpdateMorph()
        {
            DamageClass newMorph = DetermineDominantClass();

            if (newMorph != CurrentMorph)
            {
                ConstructController?.OnCrash(); // clean up old constructs

                CurrentMorph = newMorph;
                ConstructController = CreateControllerFor(newMorph);
            }
        }

        private DamageClass DetermineDominantClass()
        {
            if (Player.HeldItem.damage > 0)
                return Player.HeldItem.DamageType;
            return DamageClass.Generic;
        }

        IBloodConstructController CreateControllerFor(DamageClass dc)
        {
            if (dc == DamageClass.Melee || dc == DamageClass.MeleeNoSpeed || dc == ModContent.GetInstance<TrueMeleeNoSpeedDamageClass>() || dc == ModContent.GetInstance<TrueMeleeDamageClass>())
                return new MeleeBloodController(this);

            if (dc == DamageClass.Ranged)
                return new RangerBloodController(this);


            if (dc == DamageClass.Magic)
                return new MagicBloodController(this);


            if (dc == DamageClass.Summon || dc == DamageClass.SummonMeleeSpeed)
                return new SummonerBloodController(this);

            if (dc == ModContent.GetInstance<RogueDamageClass>())
                return new RogueBloodController(this);

            return null;
        }
        void CheckBand()
        {
            BloodBand newBand = GetBandFromSaturation();

            if (newBand != CurrentBand)
            {
                CurrentBand = newBand;
                ConstructController?.OnBandChanged(newBand);
            }
        }

        private BloodBand GetBandFromSaturation()
        {
            if (BloodSaturation < 30)
                return 0;
            else if (BloodSaturation < 50)
                return (BloodBand)1;
            else if (BloodSaturation < 70)
                return (BloodBand)2;
            else
                return (BloodBand)3;
        }

        void UpdateBloodSaturation()
        {
            if (IsCrashing)
                return;
            CombatGainRate = 0.45f;
            if (InCombat)
                BloodSaturation += CombatGainRate;
            else
                BloodSaturation -= OutOfCombatDecay;

            if (BloodSaturation >= 90 && !Ascended)
                EnterAscension();

            if (Ascended && ShouldCrash())
                TriggerCrash();
        }

        private bool ShouldCrash()
        {
            AscensionTimer--;



            if(AscensionTimer <=0)
                return true;

            return false;
        }
        private void UpdateBloodState()
        {
            if (IsCrashing)
            {
                CurrentState = BloodState.Crashing;
                return;
            }

            if (Ascended)
            {
                CurrentState = BloodState.Ascended;
                return;
            }

            if (BloodSaturation >= 30f)
            {
                CurrentState = BloodState.Symbiotic;
                return;
            }

            CurrentState = BloodState.Dormant;
        }


        private void TriggerCrash()
        {
            Ascended = false;

            CurrentState = BloodState.Crashing;

            CrashTimer = 60 * 8;
            AscensionTimer = 0;

            BloodSaturation = 0f;

            // Notify controller to violently clean up
            ConstructController?.OnCrash();

            Player.AddBuff(BuffID.Weak, 300);
            Player.AddBuff(BuffID.Slow, 180);
            Player.AddBuff(BuffID.Bleeding, 240);

            AscensionSound?.Stop();

            Player.statLife -= Player.statLifeMax2 / 10;
            Player.HealEffect(-Player.statLifeMax2 / 10, true);
        }


        private void EnterAscension()
        {
            AscensionSound?.Stop();
            AscensionSound = LoopedSoundManager.CreateNew(
                GennedAssets.Sounds.Avatar.LilyFiringLoop,
                () => Player.dead || !Ascended);


           

            Ascended = true;
            AscensionTimer = MaxAscentionTimer;

            CurrentState = BloodState.Ascended;

            ConstructController?.OnAscensionStart();

            Player.AddBuff(BuffID.Bleeding, 60 * 4);
        }


        private static readonly Dictionary<BloodBand, float> BandDamageMultiplier = new()
        {
            [BloodBand.Low] = 0.0f,   // no thralls anyway
            [BloodBand.MidLow] = 0.75f,
            [BloodBand.MidHigh] = 1.0f,
            [BloodBand.High] = 1.35f,
        };
        public int GetThrallDamage()
        {
            // Safety: no damage when thralls are disabled
            if (CurrentBand == BloodBand.Low)
                return 0;

            const int BaseThrallDamage = 400;

            float bandMultiplier = BandDamageMultiplier.GetValueOrDefault(CurrentBand, 1f);

            // Summoner scaling (this is the important part)
            float summonMultiplier = Player.GetDamage(DamageClass.Summon).Additive;

            // Optional soft dampening so swarm counts don't spiral
            // (prevents 400% summon builds from breaking balance)
            summonMultiplier = MathHelper.Lerp(1f, summonMultiplier, 0.75f);

            float finalDamage =
                BaseThrallDamage *
                bandMultiplier *
                summonMultiplier;

            return Math.Max(1, (int)MathF.Round(finalDamage));
        }

        #endregion


     

    }
}

