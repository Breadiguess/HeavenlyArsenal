using CalamityMod;
using CalamityMod.Buffs.Potions;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using HeavenlyArsenal.Common.Ui;
using HeavenlyArsenal.Content.Buffs;
using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
using Luminance.Core.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using ReLogic.Content;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor
{
    #region helper classes
    /// <summary>
    /// Represents which form the Blood Armor is currently in (Offense or Defense).
    /// </summary>
    public enum BloodArmorForm
    {
        Neutral, // only used first time you enter the armor.
        Offense,
        Defense
    }

    /// <summary>
    /// A single unit of “blood” resource held by the Blood Armor.
    /// </summary>
    public class BloodUnit
    {
        /// <summary>
        /// How much “blood” this unit currently contains (0 to 1).
        /// </summary>
        public float Amount;

        /// <summary>
        /// How many seconds this unit has existed since being generated.
        /// </summary>
        public float Age;
    }

    /// <summary>
    /// A single unit of “clot” resource held by the Blood Armor.
    /// </summary>
    public class ClotUnit
    {
        /// <summary>
        /// How much “clot” this unit currently contains (0 to 1).
        /// </summary>
        public float Amount;

        /// <summary>
        /// How many seconds this unit has existed since being converted from blood.
        /// </summary>
        public float Age;
    }
    #endregion

    /// <summary>
    /// Handles the Blood Armor’s custom resource logic, toggling between Offense/Defense forms,
    /// and driving visual/sound effects for the player wearing the set.
    /// </summary>
    public class BloodArmorPlayer : ModPlayer
    {
        #region Fields

        private readonly List<SlotId> attachedSounds = new List<SlotId>();

       public LoopedSoundInstance AmbientLoop { get; set; }

        public LoopedSoundInstance FrenzyLoop { get; set; }
        public bool BloodArmorEquipped;
        public bool Frenzy;

        /// <summary>
        /// How many tendril projectiles to spawn when in Offense form.
        /// </summary>
        public int TendrilCount = 2;

        /// <summary>
        /// List of individual BloodUnits currently held.
        /// </summary>
        public List<BloodUnit> bloodUnits = new List<BloodUnit>();

        /// <summary>
        /// How much “clot” resource is currently stored (0 to 1).
        /// </summary>
        public float Clot;

        /// <summary>
        /// Tracks if the player is currently in Offense or Defense form.
        /// </summary>
        public BloodArmorForm CurrentForm;
        /// <summary>
        /// Cached string representation of CurrentForm. Updated every tick in PreUpdate().
        /// </summary>
        private string actualFormString = BloodArmorForm.Defense.ToString();


        private float MaxResource = 1f;
        private float AgingThreshold = 7f;      // seconds until a blood unit converts to clot
        private float ClotDrainInterval = 0.4f; // seconds between each healing tick from clot
        private float ClotHealingRate = 0.015f; // fraction of max health healed per clot unit

        private float clotDrainTimer = 0f;

        /// <summary>
        /// How long (in seconds) until Frenzy ends once triggered.
        /// </summary>
        public float frenzyTimer = 0f;

        private int frenzyCooldown;

        /// <summary>
        /// Total of all “blood” across every BloodUnit.
        /// </summary>
        public float CurrentBlood => bloodUnits.Sum(u => u.Amount);

        /// <summary>
        /// Combined “blood” + “clot” clamped between 0 and MaxResource.
        /// </summary>
        public float TotalResource => Math.Clamp(CurrentBlood + Clot, 0f, MaxResource);

        /// <summary>
        /// Base damage dealt by a single tendril in Offense form.
        /// </summary>
        public int TendrilBaseDamage = 400;

        #endregion

        #region Initialization and Loading

        /// <summary>
        /// Reset all Blood Armor state when the player is newly initialized or when leaving a world.
        /// </summary>
        public override void Initialize()
        {
            Frenzy = false;
            Clot = 0f;
            bloodUnits.Clear();
            clotDrainTimer = 0f;
            frenzyTimer = 0f;
           
        }
        public override void Load()
        {
            On_Player.UpdateVisibleAccessory += CorrectVisuals;
        }
        public override void Unload()
        {
            On_Player.UpdateVisibleAccessory -= CorrectVisuals;
        }

       

       

        /// <summary>
        /// Forces the game to continue using the correct head/body equip textures
        /// even if the game is paused, alt-tabbed, or autopaused.
        /// Uses <see cref="ActualForm"/> to pick the proper “Offense” or “Defense” sprite suffix.
        /// </summary>
        private void CorrectVisuals(On_Player.orig_UpdateVisibleAccessory orig, Player self, int itemSlot, Item item, bool modded)
        {
            orig(self, itemSlot, item, modded);

            // Check if the player is wearing the helmet and chestplate in their respective slots
            bool hasVanityHead = !self.armor[10].IsAir;
            bool hasArmorHead = self.armor[0].type == ModContent.ItemType<AwakenedBloodHelm>();
            bool hasVanityBody = !self.armor[11].IsAir;
            bool hasArmorBody = self.armor[1].type == ModContent.ItemType<AwakenedBloodplate>();

            // Only override visuals if the player truly has the set equipped (vanity-free)
            if (!hasVanityHead && hasArmorHead)
            {
                string headTexture = $"AwakenedBloodHelm{ActualForm}";
                self.head = EquipLoader.GetEquipSlot(Mod, headTexture, EquipType.Head);
               
            }

            if (!hasVanityBody && hasArmorBody)
            {
                string bodyTexture = $"AwakenedBloodplate{ActualForm}";
                self.body = EquipLoader.GetEquipSlot(Mod, bodyTexture, EquipType.Body);
            }
        }

        #endregion

        #region Main Logic: Resource Aging, Form Effects, and Frenzy Timer

        /// <summary>
        /// Called every tick before the player’s update loop. 
        /// Handles drawing the resource bars, aging blood into clot, 
        /// applying Offense/Defense bonuses, and managing Frenzy.
        /// </summary>
        public override void PreUpdate()
        {
          
            actualFormString = CurrentForm.ToString();
            if (!BloodArmorEquipped)
                return;

            
            
            WeaponBar.DisplayBar(Color.AntiqueWhite, Color.Crimson, CurrentBlood, 120, 0, new Vector2(0, -30));
            WeaponBar.DisplayBar(Color.Crimson, Color.AntiqueWhite, Clot, 120, 0, new Vector2(0, -20));
            WeaponBar.DisplayBar(Color.HotPink, Color.Silver, TotalResource, 120, 1, new Vector2(0, -40));

            

            // ——— Offense Form Logic & Frenzy Behavior ——— \\ 
            if (CurrentForm == BloodArmorForm.Offense)
            {
                // Massive defense drop while in Offense form
                Player.statDefense -= 75;

                
                if (Player.ownedProjectileCounts[ModContent.ProjectileType<BloodNeedle>()] <= TendrilCount - 1
                    && Main.myPlayer == Player.whoAmI)
                {
                    bool[] tentaclesPresent = new bool[TendrilCount];
                    foreach (Projectile proj in Main.projectile)
                    {
                        if (proj.active
                            && proj.type == ModContent.ProjectileType<BloodNeedle>()
                            && proj.owner == Main.myPlayer
                            && proj.ai[1] >= 0f
                            && proj.ai[1] < TendrilCount)
                        {
                            tentaclesPresent[(int)proj.ai[1]] = true;
                        }
                    }

                    for (int i = 0; i < TendrilCount; i++)
                    {
                        if (!tentaclesPresent[i])
                        {
                            int damage = (int)Player.GetBestClassDamage().ApplyTo(TendrilBaseDamage);
                            if (Frenzy)
                                damage *= 2;
                            damage = Player.ApplyArmorAccDamageBonusesTo(damage);

                            var source = Player.GetSource_FromThis(AwakenedBloodHelm.TentacleEntitySourceContext);
                            Vector2 vel = new Vector2(Main.rand.Next(-13, 14), Main.rand.Next(-13, 14)) * 0.25f;
                            Projectile.NewProjectile(source,
                                Player.Center,
                                vel,
                                ModContent.ProjectileType<BloodNeedle>(),
                                damage,
                                8f,
                                Main.myPlayer,
                                Main.rand.Next(120),
                                i);
                        }
                    }
                }

                // Trigger Frenzy once resource is maxed
                if (TotalResource >= MaxResource && CurrentBlood > 0f && !Frenzy)
                {
                    Frenzy = true;
                    frenzyTimer = 1f + TotalResource * 9f;
                }

                // Base Offense buffs
                float damageUp = 0.4f;
                int critUp = 9;

                if (Frenzy)
                {
                    // While Frenzy is active, double the damage/crit buff and drain blood rapidly
                    if (!Player.HasBuff<BloodArmorFrenzy>())
                    {
                        int buffDurationTicks = (int)(frenzyTimer * 60);
                        Player.AddBuff(ModContent.BuffType<BloodArmorFrenzy>(), buffDurationTicks, true);
                    }

                    damageUp *= 2f;
                    critUp *= 2;
                    DrainBloodRapidly();
                }

                Player.GetDamage<GenericDamageClass>() += damageUp;
                Player.GetCritChance<GenericDamageClass>() += critUp;
            }

            // ——— Defense Form Logic ——— \\ 
            if (CurrentForm == BloodArmorForm.Defense)
            {
                // Cancel any ongoing frenzy
                if (Frenzy)
                    frenzyTimer = 0f;

                Player.moveSpeed *= 0.76f;
                Player.statDefense += 50;

                // Drain clot into healing over time
                clotDrainTimer += 1f / 60f;
                if (Player.statLife < Player.statLifeMax2 && clotDrainTimer >= ClotDrainInterval && Clot > 0f)
                {
                    EatClot();
                    clotDrainTimer = 0f;
                }
            }

            // ——— Manage Frenzy Timer ——— \\ 
            if (Frenzy)
            {
                frenzyTimer -= 1f / 60f;
                if (frenzyTimer <= 0f)
                {
                    Frenzy = false;
                    SoundEngine.PlaySound(
                        GennedAssets.Sounds.Common.MediumBloodSpill with
                        { Volume = 0.5f, Pitch = 1f, PitchVariance = 0.5f, MaxInstances = 4 });
                }
            }
        }

        public override void PostUpdateMiscEffects()
        {
            if(BloodArmorEquipped)
                AgeBlood();
        }
        private void AgeBlood()
        {
            // ——— Age & Convert At Most One BloodUnit Per Tick ——— \\ 
            bool convertedThisTick = false;
            for (int i = 0; i < bloodUnits.Count && !convertedThisTick; i++)
            {
                BloodUnit unit = bloodUnits[i];
                unit.Age += 1f / 60f; // each tick is ~1/60th of a second

                if (unit.Age >= AgingThreshold)
                {
                    Clot = Math.Clamp(Clot + unit.Amount, 0f, MaxResource);
                    bloodUnits.RemoveAt(i);
                    convertedThisTick = true;
                }
            }
        }
        #endregion

        #region attempt get the actual form (rather than pre armor set update form, which defaults to 
        /// <summary>
        /// Backing field for <see cref="ActualForm"/>. Overwritten at the start of each PreUpdate.
        /// </summary>
        private string actualFormStr = BloodArmorForm.Defense.ToString();
        /// <summary>
        /// Returns the current armor form (“Offense” or “Defense”) as a string.
        /// Used by <see cref="CorrectVisuals"/> to choose the correct equip texture suffix.
        /// </summary>
        public string ActualForm => CurrentForm.ToString();

        #endregion

        #region Clot Consumption & Healing

        /// <summary>
        /// Consumes one “clot” unit to heal the player over time and spawn a healing VFX.
        /// </summary>
        public void EatClot()
        {
            int healingAmount = (int)(Player.statLifeMax2 * ClotHealingRate);
            Player.statLife += healingAmount;
            Player.AddBuff(ModContent.BuffType<BloodfinBoost>(), 360, true, true);
            CombatText.NewText(Player.Hitbox, Color.Green, healingAmount, true, false);
            Clot = Math.Max(Clot - 0.1f, 0f);
        }

        #endregion

        #region Armor Set Bonus Trigger

        /// <summary>
        /// Called when the full armor set bonus is activated. If in Defense form,
        /// spawns a powerful Blood Harpoon projectile.
        /// </summary>
        public override void ArmorSetBonusActivated()
        {
            if (!BloodArmorEquipped)
                return;

            if (CurrentForm == BloodArmorForm.Offense)
            {
                // (Offense form has no special set-bonus effect here.)
            }
            else
            {
                Projectile.NewProjectile(
                    Player.GetSource_FromThis(),
                    Player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<BloodHarpoon>(),
                    3000,
                    0f,
                    Main.myPlayer);
            }
        }
        public override void FrameEffects()
        {
            if (BloodArmorEquipped)
            {



                /*
                if (CurrentForm == BloodArmorForm.Offense)
                {
                    if (!hasVanityHead)
                        Player.head = EquipLoader.GetEquipSlot(Mod, "AwakenedBloodHelm", EquipType.Head);
                    if (!hasVanityBody)
                        Player.body = EquipLoader.GetEquipSlot(Mod, "AwakenedBloodplateOffense", EquipType.Body);
                }
                else if (CurrentForm == BloodArmorForm.Defense)
                {
                    if (!hasVanityHead)
                        Player.head = EquipLoader.GetEquipSlot(Mod, "AwakenedBloodHelmDefense", EquipType.Head);
                    if (!hasVanityBody)
                        Player.body = EquipLoader.GetEquipSlot(Mod, "AwakenedBloodplateDefense", EquipType.Body);
                }
                */
            }

            if (BloodArmorEquipped && Frenzy)
            {
                FrenzyVFX(3);
                if (Main.rand.NextBool(5))
                {
                    Particle plus = new HealingPlus(
                        Player.Center + new Vector2(Main.rand.NextFloat(-16, 16), 0),
                        Main.rand.NextFloat(1.4f, 1.8f),
                        new Vector2(0, Main.rand.NextFloat(-2f, -3.5f)) + Player.velocity,
                        Color.Red, Color.DarkRed,
                        Main.rand.Next(50)
                    );
                    GeneralParticleHandler.SpawnParticle(plus);
                }
            }
        }
        private void FrenzyVFX(int Rand)
        {
            if (Main.rand.NextBool(Rand))
            {
                int dust = Dust.NewDust(
                        Player.TopLeft - new Vector2(2f),
                        Player.width + 4, Player.height + 4,
                        Main.rand.NextBool(8) ? 296 : 5,
                        Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f,
                        100, default, 1.25f
                    );
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 1.3f;
                Main.dust[dust].velocity.Y -= 0.5f;
            }
        }
        #endregion

        #region Blood Drain During Frenzy

        /// <summary>
        /// Rapidly drains all BloodUnits by a small amount each tick while Frenzy is active.
        /// </summary>
        private void DrainBloodRapidly()
        {
            float drainPerFrame = 0.02f;
            for (int i = bloodUnits.Count - 1; i >= 0; i--)
            {
                var unit = bloodUnits[i];
                unit.Amount = Math.Max(unit.Amount - drainPerFrame, 0f);
                if (unit.Amount <= 0f)
                    bloodUnits.RemoveAt(i);
            }
        }

        #endregion

        #region On-Hit Effects & Blood Generation

        /// <summary>
        /// Called when any projectile hits an NPC. Currently present for future hair-triggered effects.
        /// </summary>
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Future code can go here (e.g., synergy with DragonRageFireball, etc.).
            base.OnHitNPCWithProj(proj, target, hit, damageDone);
        }

        /// <summary>
        /// Called when the player’s melee hit registers. 
        /// Adds a BloodUnit each time the player hits an NPC (unless in Frenzy).
        /// </summary>
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            AddBloodUnit();
            base.OnHitNPC(target, hit, damageDone);
        }

        /// <summary>
        /// Adds one small BloodUnit (0.005f) if there is room within the MaxResource limit.
        /// </summary>
        /// <param name="age">Optional starting age (in seconds) for the new unit; default is zero.</param>
        public void AddBloodUnit(float age = 0f)
        {
            if (BloodArmorEquipped && !Frenzy)
            {
                float space = MaxResource - Clot - CurrentBlood;
                float toAdd = Math.Min(0.005f, space);
                if (toAdd > 0f)
                    bloodUnits.Add(new BloodUnit { Amount = toAdd, Age = age });
            }
        }

        #endregion

        #region Reset & Death Handling

        /// <summary>
        /// If the Blood Armor is not equipped anymore, decay all BloodUnits over time and reset Clot.
        /// Resets Frenzy if it was active.
        /// </summary>
        public override void ResetEffects()
        {
            if (!BloodArmorEquipped)
            {
                if (Frenzy)
                {
                    frenzyTimer = 0f;
                    Frenzy = false;
                }

                for (int i = bloodUnits.Count - 1; i >= 0; i--)
                {
                    bloodUnits[i].Amount = Math.Max(bloodUnits[i].Amount - 0.05f, 0f);
                    if (bloodUnits[i].Amount <= 0f)
                        bloodUnits.RemoveAt(i);
                }

                Clot = 0f;
            }

            BloodArmorEquipped = false;
        }

        /// <summary>
        /// Fully resets Blood Armor resources/flags when the player dies.
        /// </summary>
        public override void UpdateDead()
        {
            BloodArmorEquipped = false;
            bloodUnits.Clear();
            Clot = 0f;
            Frenzy = false;
            frenzyTimer = 0f;
        }

        #endregion
    }

    /// <summary>
    /// A custom player draw layer that displays a pulsing crimson glow
    /// around the player’s sprite while Frenzy is active (or fading out as the timer ends).
    /// </summary>
    public class FrenzyGlowLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.ElectrifiedDebuffBack);
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            var modPlayer = drawInfo.drawPlayer.GetModPlayer<BloodArmorPlayer>();
            return modPlayer.BloodArmorEquipped && (modPlayer.Frenzy || modPlayer.frenzyTimer > 0f);
        }
        public override bool IsHeadLayer => false;
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            var modPlayer = drawInfo.drawPlayer.GetModPlayer<BloodArmorPlayer>();
            Asset<Texture2D> coronaTexture = GennedAssets.Textures.GreyscaleTextures.Corona.Asset;

            Vector2 playerCenterOnScreen = drawInfo.drawPlayer.Center - Main.screenPosition;
            float baseScale = 0.25f;
            float rotation = Main.GlobalTimeWrappedHourly * 2f;

            // Fade in to full alpha at the start of Frenzy, then fade out as the timer depletes
            float alpha;
            if (modPlayer.Frenzy)
            {
                alpha = 1f;
            }
            else
            {
                // frenzyTimer counts down (in seconds); dividing by 60 gives fraction
                alpha = MathHelper.Clamp(modPlayer.frenzyTimer / 60f, 0f, 1f);
            }

            Color glowColor = Color.Crimson * alpha;
            byte alphaByte = (byte)(255 * alpha);
            glowColor.A = alphaByte;

            // Pulsing effect
            float pulseSpeed = 5f;
            float pulse = (float)Math.Sin(Main.GlobalTimeWrappedHourly * pulseSpeed) * 0.1f;
            Vector2 finalScale = new Vector2(baseScale * (1f + pulse), baseScale * (1f + pulse));

            Main.spriteBatch.Draw(
                coronaTexture.Value,
                playerCenterOnScreen,
                null,
                glowColor with { A = 0 },
                rotation + MathHelper.PiOver4,
                Utils.Size(coronaTexture) * 0.5f,
                finalScale,
                SpriteEffects.None,
                0f);
        }
    }
}
