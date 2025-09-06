using CalamityMod;
using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using HeavenlyArsenal.Common.Ui;
using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;


namespace HeavenlyArsenal.Content.Items.Armor.AwakenedBloodArmor
{
    public class AwakenedBloodPlayer : ModPlayer
    {
        #region values
        public enum Form
        {
            Offsense,
            Defense
        }
        public Form CurrentForm = Form.Offsense;

        public bool AwakenedBloodSetActive;
        public bool BloodBoostActive = false;

        public int BloodBoostSink = 6 * 60;
        public int BloodBoostTotalTime = 0;
        public int BloodBoostDrainTimer = 0;

        public int blood;


        public int clot;
        public int clotDecayTimer;

        /// <summary>
        /// Timer to control the rate at which the player gains blood
        /// </summary>
        public int GainTimer = 0;

        public int CombinedResource => blood + clot;
        public int MaxResource = 100;
        #endregion

    

        public override void PreUpdate()
        {
            if (GainTimer > 0)
                GainTimer--;

            if (!AwakenedBloodSetActive)
                return;

            float bloodPercent = blood/(float) MaxResource;
            float clotPercent = clot/(float) MaxResource;
            float bloodclot = (blood + clot)/(float)MaxResource;

            WeaponBar.DisplayBar(Color.AntiqueWhite, Color.Crimson, bloodPercent, 150, 0, new Vector2(0, -20));
            WeaponBar.DisplayBar(Color.Crimson, Color.AntiqueWhite, clotPercent, 150, 0, new Vector2(0, -30));
            WeaponBar.DisplayBar(Color.HotPink, Color.Silver, bloodclot, 150, 1, new Vector2(0, -40));
            //this shit sucks

            //Main.NewText($"{bloodPercent}, clot: {clotPercent}, decay timer: {clotDecayTimer}");
            HandleForm();
            ManageBloodBoost();

            ConvertClot();
            ControlResource();
            
        }
        public override void ResetEffects()
        {
            AwakenedBloodSetActive = false;
          
        }

        public override void ArmorSetBonusActivated()
        {
            if (!AwakenedBloodSetActive)
                return;

            int value = 76;
            if(value > 75 && !BloodBoostActive)
            {
                BloodBoostDrainTimer = 0;
                BloodBoostActive = true;
                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.BloodCry);
            }
                
        }

        
        #region Hit NPC

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if(AwakenedBloodSetActive && GainTimer <= 0)
            {
               
                GainBlood();
                ControlResource();
            }
            base.OnHitNPC(target, hit, damageDone);
        }
        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (AwakenedBloodSetActive && GainTimer <= 0)
            {
                GainTimer = 20;
                GainBlood();
                ControlResource();
            }
        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (AwakenedBloodSetActive && GainTimer <= 0)
            {
                
                GainBlood();
                ControlResource();
            }
        }
        #endregion
        #region hitByNPC or Projectile
        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            if(AwakenedBloodSetActive)
                BloodLoss(hurtInfo);   
        }
        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            if (AwakenedBloodSetActive)
                BloodLoss(hurtInfo);
        }
        #endregion
        #region helpers
        private void HandleForm()
        {
            switch (CurrentForm)
            {
                case Form.Offsense:
                    ManageOffense();
                    break;

                case Form.Defense:
                    ManageDefense();
                    break;
            }
        }
        private void ManageOffense()
        {
            int TendrilCount = 2;
            int TendrilBaseDamage = 300;
            if (Player.ownedProjectileCounts[ModContent.ProjectileType<BloodNeedle>()] < TendrilCount)
            {
                for(int i = 0; i < TendrilCount; i++)
                {
                    Projectile a = Projectile.NewProjectileDirect(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<BloodNeedle>(), TendrilBaseDamage, 1);
                    a.localAI[0] = i+1;
                }
                
            }


            Player.statDefense -= 75;
            /*
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
                        Player.Calamity();
                        int damage = (int)Player.GetBestClassDamage().ApplyTo(30);
                        
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
            }*/
        }
        private void ManageDefense()
        {
            Player.statDefense += 25;
            PurgeClot();
        }
        public void ControlResource()
        {
            
            blood = Utils.Clamp(blood, 0, 100);
            clot = Utils.Clamp(clot, 0, 100);
            if(CombinedResource > MaxResource)
            {
                int difference = MaxResource - blood;
                

            }
        }
        public void GainBlood()
        {
            if(GainTimer <=0 && CombinedResource < MaxResource)
            {
                blood += 5;
                clotDecayTimer++;
            }
            GainTimer = 20;
        }
        public void ConvertClot()
        {
            clotDecayTimer++;

            int ClotMax = 0;
            ClotMax = CurrentForm == Form.Defense? 60 : 180;

            if(clotDecayTimer !< ClotMax)
            {
                return;
            }

            if (CombinedResource >= MaxResource)
                return;
            int value = (int)Math.Round((MaxResource * (blood / (float)MaxResource)));

            value /= 4;
            blood -= value;
            
            clot += value;
            clotDecayTimer = 0;
        }
        public void BloodLoss(Player.HurtInfo hurtInfo)
        {
            int bloodLoss = (int)(hurtInfo.Damage * 0.1f);
            blood -= bloodLoss;
            if (blood < 0)
            {
                blood = 0;
            }
            //Main.NewText($"Lost {bloodLoss} blood. Total Blood: {blood}");
        }
        public void PurgeClot()
        {
            if (clot <= 0 || Player.statLife >= Player.statLifeMax2)
                return; 

            
            Player.HealEffect(clot, true);
            Player.statLife += clot;
            clot = 0;
        }

        public void ManageBloodBoost()
        {
            if (!BloodBoostActive) return;

            int DrainGate = BloodBoostTotalTime < BloodBoostSink ? 4: 2;

            Player.GetDamage(DamageClass.Generic) += 0.55f;
            Player.GetArmorPenetration(DamageClass.Generic) += 15;
            Player.GetCritChance(DamageClass.Generic) += 10;

            if (blood > 0 && BloodBoostDrainTimer > DrainGate)
            {
                blood--;
                BloodBoostDrainTimer = 0;
            }
            if (blood <= 0)
                BloodBoostActive = false;
            BloodBoostDrainTimer++;

            BloodBoostTotalTime++;
        }
     
        #endregion

    }

    public class AwakenedBloodDraw : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);
        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => true;// drawInfo.drawPlayer.head == EquipLoader.GetEquipSlot(Mod, nameof(ShintoArmorHelmetAll), EquipType.Head);
       
        public override bool IsHeadLayer => false;

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            DrawDebug(ref drawInfo);

            ManageBloodBoostVFX(drawInfo.drawPlayer);
        }
        public void ManageBloodBoostVFX(Player player)
        {

            var bloodplayer = player.GetModPlayer<AwakenedBloodPlayer>();

            if (!bloodplayer.BloodBoostActive)
                return;
            float Wane =(float) Math.Clamp(Math.Tanh(bloodplayer.blood),0,0.2f);
            ManagedScreenFilter suctionShader = ShaderManager.GetFilter("HeavenlyArsenal.ColdShader");

            suctionShader.TrySetParameter("globalTime", Main.GlobalTimeWrappedHourly);
            suctionShader.TrySetParameter("intensityFactor", Wane);
            suctionShader.TrySetParameter("psychadelicExponent", 1);
            suctionShader.SetTexture(GennedAssets.Textures.Noise.VoronoiNoise, 1, SamplerState.AnisotropicWrap);
            suctionShader.TrySetParameter("psychedelicColorTint", new Vector4(1, 0, 0.1f, 100   ));
            suctionShader.TrySetParameter("colorAccentuationFactor",0.1f);
            //suctionShader.TrySetParameter("noiseTexture", GennedAssets.Textures.Noise.TechyNoise);

            suctionShader.TrySetParameter("opacity", 0);
            suctionShader.Activate();
            
            //sampler baseTexture : register(s0);
            //sampler psychedelicTexture : register(s1);
            //sampler noiseTexture : register(s2);

            //float globalTime;
            //float opacity;
            // float intensityFactor;
            // float psychedelicExponent;
            // float colorAccentuationFactor;
            // float3 colorToAccentuate;
            // float4 goldColor;
            //  float4 psychedelicColorTint;

        }
        protected void DrawDebug(ref PlayerDrawSet drawInfo)
        {
            var bloodplayer = drawInfo.drawPlayer.GetModPlayer<AwakenedBloodPlayer>();

            string blood = $"Blood: {bloodplayer.blood}";
            string gaincooldown = $"Blood gain timer: {bloodplayer.GainTimer}";
            string clot = $"clot: {bloodplayer.clot}";
            string Decaytimer = $"blood decay Timer: {bloodplayer.clotDecayTimer}";

            string Bloodboostactive = $"Bloodboost Active?: {bloodplayer.BloodBoostActive}";
            string combinedString = blood + ", " + gaincooldown + ", " + clot + ", " + Decaytimer + ", " + Bloodboostactive; 
            Vector2 DrawPos = drawInfo.drawPlayer.Center - Main.screenPosition;
            Vector2 Offset = Vector2.UnitY * -120 + Vector2.UnitX * -120;
            Utils.DrawBorderString(Main.spriteBatch, combinedString, DrawPos + Offset, Color.AntiqueWhite);
        }

    }
}
