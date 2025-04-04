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

namespace HeavenlyArsenal.ArsenalPlayer
{
    class ShintoArmorPlayer : ModPlayer
    {
        public bool SetActive;
        public int maxBarrier = 560;
        public int barrier = 0;
        public int timeSinceLastHit;
        public int rechargeDelay = 30;
        public int rechargeRate = 100;
        public float barrierDamageReduction = 0.5f;
        public bool ShadowShieldVisible = false;
        public bool ShadowVeil;

        

        
        public int ShadowVeilImmunity = 0;
        public override void PostUpdateMiscEffects()
        {
            if (SetActive)
            {
                //Main.NewText($"Barrier: {barrier}, TimeSinceLastHit: {timeSinceLastHit}",Color.AntiqueWhite);
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

                                ShadowVeilImmunity = ShintoArmorHelmet.ShadowVeilIFrames;
                            }
                        }
                    }
                }
            }
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (barrier > 0)
            {
                modifiers.ModifyHurtInfo += ModifyDamage;
                timeSinceLastHit = 0;
            }
        }

        private void ModifyDamage(ref Player.HurtInfo info)
        {
            if (barrier > 0)
            {
                int incoming = info.Damage;
                CombatText.NewText(Player.Hitbox, Color.Cyan, incoming);

                // Subtract the full incoming damage from the barrier.
                barrier -= incoming;
                if (barrier < 0)
                {
                    barrier = 0;
                }

                // Cancel all damage to the player.
                info.Damage = 0;
            }
        }
        public override void PostUpdateEquips()
        {
            if (barrier > 0)
            {
                Player.statDefense += 30;
            }
        }

        public override void UpdateBadLifeRegen()
        {
            if (maxBarrier > 0)
                timeSinceLastHit++;

            if (timeSinceLastHit >= rechargeDelay && barrier < maxBarrier)
            {
                int rechargeRateWhole = rechargeRate / 60;
                barrier += Math.Min(rechargeRateWhole, maxBarrier - barrier);

                if (rechargeRate % 60 != 0)
                {
                    int rechargeSubDelay = 60 / (rechargeRate % 60);
                    if (timeSinceLastHit % rechargeSubDelay == 0 && barrier < maxBarrier)
                        barrier++;
                }
            }
        }

        public override void ResetEffects()
        {
            //barrier = 0;
            SetActive = false;
        }
    }
}
