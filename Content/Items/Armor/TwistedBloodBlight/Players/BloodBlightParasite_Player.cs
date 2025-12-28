using CalamityMod;
using HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Mage;
using HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Melee;
using HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Ranged;
using HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Rogue;
using HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Summoner;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players
{
    public class BloodBlightParasite_Player : ModPlayer
    {
        #region Values
        private float _bloodSaturation;
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
        #endregion






        public override void PostUpdateMiscEffects()
        {
            ConstructController?.Update(Player);
            UpdateBloodSaturation();
            CheckBand();
            UpdateMorph();

            if (CrashTimer > 0)
                CrashTimer--;
            if (CombatTimer > 0)
                CombatTimer--;

            UpdateBloodState();
        }


        public override void OnHitAnything(float x, float y, Entity victim)
        {
            CombatTimer = 8 * 60;
        }

        public override void ResetEffects()
        {

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
            CombatGainRate = 0.02f;
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
            AscensionTimer++;

            if (AscensionTimer < 120) // ~2 seconds
                return false;

            if (BloodSaturation >= BloodSaturationMax && AscensionTimer > 300)
                return true;

            float lifeRatio = Player.statLife / (float)Player.statLifeMax2;
            if (lifeRatio <= 0.15f)
                return true;

            // Condition 3: Left combat while Ascended
            if (!InCombat && AscensionTimer > 10 * 60)
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
            // Force saturation down but not to zero
            BloodSaturation = 15f;

            // Notify controller to violently clean up
            ConstructController?.OnCrash();

            // Apply player penalties (keep these centralized)
            Player.AddBuff(BuffID.Weak, 300);
            Player.AddBuff(BuffID.Slow, 180);
            Player.AddBuff(BuffID.Bleeding, 240);

            // Optional: immediate HP punishment
            Player.statLife -= Player.statLifeMax2 / 10;
            Player.HealEffect(-Player.statLifeMax2 / 10, true);
        }


        private void EnterAscension()
        {
            Ascended = true;
            AscensionTimer = 0;

            CurrentState = BloodState.Ascended;

            // Notify active controller
            ConstructController?.OnAscensionStart();

            // Optional: immediate feedback
            Player.AddBuff(BuffID.Bleeding, 60 * 4);
        }



        internal int GetThrallDamage()
        {
            return 40;
        }

        #endregion
    }
}

