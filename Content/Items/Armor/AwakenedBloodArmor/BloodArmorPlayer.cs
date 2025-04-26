using CalamityMod;
using CalamityMod.Buffs.Potions;
using CalamityMod.Particles;
using HeavenlyArsenal.Common.Ui;
using HeavenlyArsenal.Content.Buffs;
using HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.NewFolder
{
    public enum BloodArmorForm
    {
        Offense,
        Defense
    }

    public class BloodUnit
    {
        public float Amount;
        public float Age; // seconds
    }
    public class ClotUnit
    {
        public float Amount;
        public float Age;
    }
    public class BloodArmorPlayer : ModPlayer
    {
        public bool BloodArmorEquipped;
        public bool Frenzy;

        public List<BloodUnit> bloodUnits = new List<BloodUnit>();
        public float Clot;

        public BloodArmorForm CurrentForm = BloodArmorForm.Offense;

        private float MaxResource = 1f;
        private float AgingThreshold = 7f;      // how long until one unit turns to clot
        private float ClotDrainInterval = 0.4f;   // defense form healing
        private float ClotHealingRate = 0.015f; // 5.5% max health per clot unit

        private float clotDrainTimer = 0f;

        private float frenzyTimer = 0f;
        public float CurrentBlood => bloodUnits.Sum(u => u.Amount);
        public float TotalResource => Math.Clamp(CurrentBlood + Clot, 0f, MaxResource);
        public override void Initialize()
        {
            Frenzy = false;
            Clot = 0f;
            bloodUnits.Clear();
            clotDrainTimer = 0f;
            frenzyTimer = 0f;
            CurrentForm = BloodArmorForm.Offense;
        }
        
        public override void UpdateEquips()
        {
            if (BloodArmorEquipped)
            {
                if (CurrentForm != BloodArmorForm.Offense)
                {

                }
            }


        }
        public override void PostUpdateMiscEffects()
        {
            //Main.NewText($"equipped: {BloodArmorEquipped} | Total: {TotalResource:F2} | Blood: {CurrentBlood:F2} | Clot: {Clot:F2} | frenzyTimer: {frenzyTimer:F2} | Frenzy: {Frenzy} | form : {CurrentForm}");

            if (BloodArmorEquipped)
            {
                /*
                if (CalamityKeybinds.SpectralVeilHotKey.JustPressed)
                {
                    CurrentForm = CurrentForm == BloodArmorForm.Offense ? BloodArmorForm.Defense : BloodArmorForm.Offense;
                    
                }
                */
                // Main.NewText($"Total: {TotalResource:F2} | Blood: {CurrentBlood:F2} | Clot: {Clot:F2} | frenzyTimer: {frenzyTimer:F2}| Frenzy: {Frenzy} | form : {CurrentForm}");
                WeaponBar.DisplayBar(Color.AntiqueWhite, Color.Crimson, CurrentBlood, 120, 0, new Vector2(0, -30));
                WeaponBar.DisplayBar(Color.Crimson, Color.AntiqueWhite, Clot, 120, 0, new Vector2(0, -20));
                WeaponBar.DisplayBar(Color.HotPink, Color.Silver, TotalResource, 120, 1, new Vector2(0, -40));
                // ——— Age & convert at most one unit per tick ——— \\
                bool convertedThisTick = false;
                for (int i = 0; i < bloodUnits.Count && !convertedThisTick; i++)
                {
                    BloodUnit unit = bloodUnits[i];
                    unit.Age += 1f / 60f;

                    if (unit.Age >= AgingThreshold)
                    {
                        // convert only this one
                        Clot = Math.Clamp(Clot + unit.Amount, 0f, MaxResource);
                        bloodUnits.RemoveAt(i);
                        convertedThisTick = true;
                    }
                }

                // ——— Offense Form & Frenzy ——— \\
                if (CurrentForm == BloodArmorForm.Offense)
                {
                    Player.statDefense -= 50;
                    if (Player.ownedProjectileCounts[ModContent.ProjectileType<BloodNeedle>()] <= 1 && Main.myPlayer == Player.whoAmI)
                    {
                        bool[] tentaclesPresent = new bool[2];
                        foreach (Projectile projectile in Main.ActiveProjectiles)
                        {
                            if (projectile.type == ModContent.ProjectileType<BloodNeedle>() && projectile.owner == Main.myPlayer && projectile.ai[1] >= 0f && projectile.ai[1] < 6f)
                                tentaclesPresent[(int)projectile.ai[1]] = true;
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            if (!tentaclesPresent[i])
                            {
                                int damage = (int)Player.GetBestClassDamage().ApplyTo(600);
                                damage = Player.ApplyArmorAccDamageBonusesTo(damage);

                                var source = Player.GetSource_FromThis(AwakenedBloodHelm.TentacleEntitySourceContext);
                                Vector2 vel = new Vector2(Main.rand.Next(-13, 14), Main.rand.Next(-13, 14)) * 0.25f;
                                Projectile.NewProjectile(source, Player.Center, vel, ModContent.ProjectileType<BloodNeedle>(), damage, 8f, Main.myPlayer, Main.rand.Next(120), i);
                            }
                        }

                        float damageUp = 0.1f;
                        int critUp = 10;


                        damageUp *= 2f;
                        critUp *= 2;

                        Player.GetDamage<GenericDamageClass>() += damageUp;
                        Player.GetCritChance<GenericDamageClass>() += critUp;
                    }
                    if (TotalResource >= MaxResource && CurrentBlood != 0 && !Frenzy && CurrentForm == BloodArmorForm.Offense)
                    {
                        Frenzy = true;
                        frenzyTimer = 1f + TotalResource * 9f;
                    }
                    if (Frenzy) 
                        { 
                        Player.AddBuff(ModContent.BuffType<BloodArmorFrenzy>(), (int)frenzyTimer, true);
                        ApplyFrenzyEffects();
                        }
                    }
                    // ——— Defense Form ——— \\
                    if (CurrentForm == BloodArmorForm.Defense)
                    {
                    Player.moveSpeed *= 0.76f;
                    Player.statDefense += 50;
                    clotDrainTimer += 1f / 60f;

                    if (Player.statLifeMax2 != Player.statLife)
                    {
                        if (clotDrainTimer >= ClotDrainInterval && Clot > 0f)
                        {
                            EatClot();
                            clotDrainTimer = 0f;
                        }
                    }

                }
            }

            // ——— Frenzy Timer ———
            if (Frenzy)
            {
                frenzyTimer -= 1f / 60f;
                if (frenzyTimer <= 0f)
                    Frenzy = false;
            }
        }

        public void EatClot()
        {
            Player.statLife += (int)(Player.statLifeMax2 * ClotHealingRate);
            Player.AddBuff(ModContent.BuffType<BloodfinBoost>(), 360, true, true);
            CombatText.NewText(Player.Hitbox, Color.Green, (int)(Player.statLifeMax2 * ClotHealingRate), true, false);
            Clot = Math.Max(Clot - 0.1f, 0f);
        }
        public override void ArmorSetBonusActivated()
        {
            if (BloodArmorEquipped)
            {
                if (CurrentForm == BloodArmorForm.Offense)
                {

                }
                else
                {
                    Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<BloodHarpoon>(), 3000, 0f, Main.myPlayer);
                }
            }

        }

        private void ApplyFrenzyEffects()
        {
            Player.GetDamage(DamageClass.Generic) *= 2f;
            Player.GetAttackSpeed(DamageClass.Default) += 1f;
            DrainBloodRapidly();
        }

        //shh emoji
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
        public override void FrameEffects()
        {
            if (BloodArmorEquipped)
            {
                if (CurrentForm == BloodArmorForm.Offense)
                {
                    // big stupid, i am
                    Player.head = EquipLoader.GetEquipSlot(Mod, "AwakenedBloodHelm", EquipType.Head);
                    Player.body = EquipLoader.GetEquipSlot(Mod, "AwakenedBloodplateDefense", EquipType.Body);
                }
                else if (CurrentForm == BloodArmorForm.Defense)
                {
                    Player.head = EquipLoader.GetEquipSlot(Mod, "AwakenedBloodHelmDefense", EquipType.Head);
                    Player.body = EquipLoader.GetEquipSlot(Mod, "AwakenedBloodplate", EquipType.Body);
                }
            }
            if (BloodArmorEquipped && Frenzy)
            {

                if (Main.rand.NextBool(3))
                {
                    int dust = Dust.NewDust(Player.TopLeft - new Vector2(2f), Player.width + 4, Player.height + 4, Main.rand.NextBool(8) ? 296 : 5, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 100, default, 1.25f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1.3f;
                    Main.dust[dust].velocity.Y -= 0.5f;
                   

                }
                if (Main.rand.NextBool(5))
                {
                    Particle Plus = new HealingPlus(Player.Center - new Vector2(Main.rand.NextFloat(-16,16), 0), Main.rand.NextFloat(1.4f, 1.8f), new Vector2(0, Main.rand.NextFloat(-2f, -3.5f)) + Player.velocity, Color.Red, Color.DarkRed, Main.rand.Next(50));
                    GeneralParticleHandler.SpawnParticle(Plus);
                }
            }
        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            AddBloodUnit(0);
            base.OnHitNPC(target, hit, damageDone);
        }

        public void AddBloodUnit(float age = 0f )
        {
            if (BloodArmorEquipped && !Frenzy)
            {
                // ——— Respect clot capacity when adding blood ———
                float space = MaxResource - Clot - CurrentBlood;
                float toAdd = Math.Min(0.005f, space);
                if (toAdd > 0f)
                    bloodUnits.Add(new BloodUnit { Amount = toAdd, Age = 0f });
            }
        }
        public override void ResetEffects()
        {
            if (!BloodArmorEquipped)
            {
                
                for (int i = bloodUnits.Count - 1; i >= 0; i--)
                {
                    bloodUnits[i].Amount = Math.Max(bloodUnits[i].Amount - 0.05f, 0f);
                    if (bloodUnits[i].Amount <= 0f)
                        bloodUnits.RemoveAt(i);
                }
                
            }
            BloodArmorEquipped = false;
        }

        public override void UpdateDead()
        {
            BloodArmorEquipped = false;
            bloodUnits.Clear();
            Clot = 0f;
            Frenzy = false;
        }
    }
}