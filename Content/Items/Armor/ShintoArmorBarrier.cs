using System.Collections.Generic;
using CalamityMod;
using CalamityMod.Cooldowns;
using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Common.Ui.Cooldowns;
using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
using HeavenlyArsenal.Content.Particles;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SpecificEffectManagers;
using Terraria.Audio;
using Terraria.Graphics.Effects;

namespace HeavenlyArsenal.Content.Items.Armor;

internal class ShintoArmorBarrier : ModPlayer
{
    public bool BarrierActive;

    public int maxBarrier;

    public int barrier;

    public int timeSinceLastHit;

    public int Iframe;

    public int rechargeDelay;

    public int rechargeRate;

    public int lastHitDamage;

    public float barrierDamageReduction = 0.235f;

    public float barrierSizeInterp;

    public Dictionary<string, CooldownInstance> cooldowns;

    internal float barrierShieldPartialRechargeProgress = 0f;

    public override void Load()
    {
        On_Main.DrawInfernoRings += On_Main_DrawInfernoRings;
    }

    private void On_Main_DrawInfernoRings(On_Main.orig_DrawInfernoRings orig, Main self)
    {
        orig(self);
        var player = Main.LocalPlayer;
        DrawDyeableShader(Main.spriteBatch);
    }

    public override void Initialize()
    {
        cooldowns = new Dictionary<string, CooldownInstance>();
        maxBarrier = ShintoArmorBreastplate.ShieldDurabilityMax;
        barrier = 0;
        timeSinceLastHit = 0;
        Iframe = 0;
        rechargeDelay = ShintoArmorBreastplate.ShieldRechargeDelay;
        rechargeRate = ShintoArmorBreastplate.ShieldRechargeRate;
    }

    public override void UpdateBadLifeRegen()
    {
        if (Iframe > 0)
        {
            Iframe--;
        }

        if (maxBarrier > 0 && BarrierActive && timeSinceLastHit < 10000)
        {
            timeSinceLastHit++;
        }

        if (!Player.Calamity().cooldowns.ContainsKey(BarrierRecharge.ID) &&
            timeSinceLastHit >= rechargeDelay &&
            barrier < maxBarrier &&
            BarrierActive)
        {
            var rechargeRateWhole = rechargeRate / 60;
            barrier += Math.Min(rechargeRateWhole, maxBarrier - barrier);

            if (rechargeRate % 60 != 0)
            {
                var rechargeSubDelay = 60 / (rechargeRate % 60);

                if (timeSinceLastHit % rechargeSubDelay == 0 && barrier < maxBarrier)
                {
                    barrier++;
                }
            }
        }
    }

    public override void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        var armorPlayer = Player.GetModPlayer<ShintoArmorPlayer>();

        if (!BarrierActive)
        {
            return;
        }

        if (barrier > 0 && Iframe <= 0)
        {
            modifiers.DisableSound();
            SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Armor.Antishield_Hit, Player.Center);
        }
        else if (barrier <= 0 && timeSinceLastHit == 1) { }

        if (barrier <= 0 && armorPlayer.Enraged)
        {
            modifiers.FinalDamage *= 2.5f;
        }
    }

    public override void PostUpdateMiscEffects()
    {
        ManageBarrier();
        //Player.Calamity().adrenaline++;
        //Player.Calamity().adrenalineModeActive = true;
    }

    public override bool FreeDodge(Player.HurtInfo info)
    {
        if (barrier > 0 && BarrierActive)
        {
            var incoming = info.SourceDamage;

            if (Iframe <= 0)
            {
                var taken = (int)Math.Round(incoming * barrierDamageReduction);

                if (taken > 0)
                {
                    timeSinceLastHit = 0;
                }

                Iframe = (int)(Player.ComputeHitIFrames(info) * 1.25f);
                barrier -= taken;

                if (barrier <= 0)
                {
                    SoundEngine.PlaySound(AssetDirectory.Sounds.Items.Armor.Antishield_Break, Player.Center);
                }

                BarrierTakeDamageVFX();
                CombatText.NewText(Player.Hitbox, Color.Cyan, taken);
            }

            return true;
        }

        return base.FreeDodge(info);
    }

    public void ManageBarrier()
    {
        if (timeSinceLastHit == 0)
        {
            if (!Player.Calamity().cooldowns.ContainsKey(BarrierRecharge.ID))
            {
                Player.AddCooldown(BarrierRecharge.ID, ShintoArmorBreastplate.ShieldRechargeDelay);
            }
            else
            {
                Player.Calamity().cooldowns.Remove(BarrierRecharge.ID);
                Player.AddCooldown(BarrierRecharge.ID, ShintoArmorBreastplate.ShieldRechargeDelay);
            }
        }

        if (BarrierActive)
        {
            if (barrier <= 0)
            {
                Player.Calamity().cooldowns.Remove(BarrierDurability.ID);
            }

            if (barrier > 0 && !cooldowns.ContainsKey(BarrierDurability.ID))
            {
                barrierSizeInterp = float.Lerp(barrierSizeInterp, 1, 0.1f);

                var cd = Player.AddCooldown(BarrierDurability.ID, ShintoArmorBreastplate.ShieldDurabilityMax);
                cd.timeLeft = barrier;
            }
        }
        else
        {
            barrierSizeInterp = float.Lerp(barrierSizeInterp, 0, 0.2f);
            barrier = 0;
            timeSinceLastHit = 0;
            Player.Calamity().cooldowns.Remove(BarrierDurability.ID);
            Player.Calamity().cooldowns.Remove(BarrierRecharge.ID);
        }
    }

    public void DisableAllBarriers()
    {
        Player.Calamity().RoverDriveShieldDurability = -1;
        Player.Calamity().SpongeShieldDurability = -1;
        Player.Calamity().cooldowns.Remove(BarrierDurability.ID);
        Player.Calamity().cooldowns.Remove(BarrierRecharge.ID);
        cooldowns.Clear();
        barrier = 0;
        timeSinceLastHit = 0;
    }

    public void BarrierTakeDamageVFX()
    {
        for (var i = 0; i < Main.rand.Next(1, 5); i++)
        {
            var lightningPos = Player.Center + Main.rand.NextVector2Circular(24, 24);
            var particle = HeatLightning.pool.RequestParticle();
            particle.Prepare(lightningPos, Player.velocity + Main.rand.NextVector2Circular(10, 10), Main.rand.NextFloat(-2f, 2f), 10 + i * 3, Main.rand.NextFloat(0.5f, 1f));
            ParticleEngine.Particles.Add(particle);
        }
    }

    public override void ResetEffects()
    {
        if (!BarrierActive && barrier > 0)
        {
            barrier = 0;
            timeSinceLastHit = 0;
        }

        BarrierActive = false;
    }

    public static void DrawDyeableShader(SpriteBatch spriteBatch)
    {
        // TODO -- Control flow analysis indicates that this hook is not stable (as it was copied from Rover Drive).
        // Sponge shields will be drawn for each player with the Sponge equipped, yes.
        // But there is no guarantee that the shields will be in the right condition for each player.
        // Visibility is not net synced, for example.
        var alreadyDrawnShieldForPlayer = false;

        foreach (var player in Main.ActivePlayers)
        {
            if (player.GetModPlayer<ShintoArmorBarrier>().BarrierActive == false)
            {
                continue;
            }

            if (player.outOfRange || player.dead)
            {
                continue;
            }

            var modPlayer = player.GetModPlayer<ShintoArmorBarrier>();

            // Determine if the shield should be rendered
            // Use modPlayer.active (or another appropriate flag) and check that the barrier value is positive.

            var shieldExists = modPlayer.barrier > 0;

            if (!shieldExists)
            {
                continue;
            }

            // Scale the shield as drawn. The shield gently grows and shrinks; it should be largely imperceptible.
            // The "i" parameter is to desync each player's shield animation.
            var i = player.whoAmI;
            var baseScale = 1f;
            var maxExtraScale = 0.055f;
            var extraScalePulseInterpolant = MathF.Pow(4f, MathF.Sin(Main.GlobalTimeWrappedHourly * 0.791f + i) - 1);
            var scale = (baseScale + maxExtraScale * extraScalePulseInterpolant) * modPlayer.barrierSizeInterp;
            var ShieldHealthInterpolant = (float)player.GetModPlayer<ShintoArmorBarrier>().barrier / ShintoArmorBreastplate.ShieldDurabilityMax;

            if (!alreadyDrawnShieldForPlayer)
            {
                var visualShieldStrength = ShieldHealthInterpolant;

                // The scale used for the noise overlay also grows and shrinks
                var noiseScale = MathHelper.Lerp(0.28f, 0.38f, 5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 0.347f + i)) * modPlayer.barrierSizeInterp;

                var shieldEffect = Filters.Scene["CalamityMod:RoverDriveShield"].GetShader().Shader;
                shieldEffect.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly * 0.0813f);
                shieldEffect.Parameters["blowUpPower"].SetValue(3f);
                shieldEffect.Parameters["blowUpSize"].SetValue(0.56f);
                shieldEffect.Parameters["noiseScale"].SetValue(noiseScale);

                var baseShieldOpacity = 1.2f * Utils.SmoothStep(-15f, 15f, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f)) - 0.2f;
                //(float)Utils.SmoothStep(0,0.5f,Math.Sin(Main.GlobalTimeWrappedHourly * 0.45f));
                //(float)Utils.Lerp(0, 1, Math.Clamp((MathF.Sin(Main.GlobalTimeWrappedHourly * 0.95f)),0,1));//(0.2f) + 0.2f * (player.statLife / player.statLifeMax) * MathF.Sin(Main.GlobalTimeWrappedHourly * 0.76f);

                var minShieldStrengthOpacityMultiplier = 1f;
                var finalShieldOpacity = baseShieldOpacity * MathHelper.Lerp(minShieldStrengthOpacityMultiplier, 1f, visualShieldStrength);
                shieldEffect.Parameters["shieldOpacity"].SetValue(finalShieldOpacity);
                shieldEffect.Parameters["shieldEdgeBlendStrenght"].SetValue(1f);

                var shieldColor = new Color(163, 0, 41); // #189CCC
                var primaryEdgeColor = shieldColor;
                var secondaryEdgeColor = new Color(220, 20, 71); // #22E0E3                   x
                var edgeColor = CalamityUtils.MulticolorLerp(Main.GlobalTimeWrappedHourly * ShieldHealthInterpolant, primaryEdgeColor, secondaryEdgeColor);

                shieldEffect.Parameters["shieldColor"].SetValue(shieldColor.ToVector3());
                shieldEffect.Parameters["shieldEdgeColor"].SetValue(edgeColor.ToVector3());

                shieldEffect.CurrentTechnique.Passes[0].Apply();

                Main.pixelShader.CurrentTechnique.Passes[0].Apply();

                var rotation = Utils.AngleLerp(0, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 0.02f), 0.4f);
                //Texture2D ShieldNoise = AssetDirectory.Textures.VoidLake.Value;
                Texture2D glow = GennedAssets.Textures.Noise.MoltenNoise;
                Texture2D ogg = GennedAssets.Textures.Extra.Ogscule;
                Main.spriteBatch.End();

                Main.spriteBatch.Begin
                    (SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, shieldEffect, Main.GameViewMatrix.TransformationMatrix);

                // Fetch shield noise overlay texture (this is the polygon texture fed to the shader)
                var pos = player.MountedCenter + player.gfxOffY * Vector2.UnitY - Main.screenPosition;
                var color = Color.AntiqueWhite;

                if (Main.remixWorld)
                {
                    Main.EntitySpriteDraw(ogg, pos, null, Color.AntiqueWhite, rotation, ogg.Size() / 2f, 0.05f, 0);
                }
                else
                {
                    Main.EntitySpriteDraw(glow, pos, null, Color.AntiqueWhite, rotation, glow.Size() / 2f, modPlayer.barrierSizeInterp * (baseShieldOpacity / 20 + 0.1f), 0);
                }

                var suctionShader = ShaderManager.GetFilter("HeavenlyArsenal.SuctionSpiralShader");
                suctionShader.TrySetParameter("globalTime", Main.GlobalTimeWrappedHourly / 10.1f);

                suctionShader.TrySetParameter("suctionCenter", Vector2.Transform(player.Center - Main.screenPosition, Main.GameViewMatrix.TransformationMatrix));
                suctionShader.TrySetParameter("zoomedScreenSize", Main.ScreenSize.ToVector2() / Main.GameViewMatrix.Zoom);
                suctionShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom.X);
                suctionShader.TrySetParameter("suctionOpacity", 1 * (player.GetModPlayer<ShintoArmorBarrier>().barrierSizeInterp - AvatarRiftSuckVisualsManager.ZoomInInterpolant) * 0.12f);
                suctionShader.TrySetParameter("suctionBaseRange", 27f);
                suctionShader.TrySetParameter("suctionFadeOutRange", 10f);
                suctionShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.AnisotropicWrap);
                suctionShader.Activate();

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
                var drawPosition = player.Center - Main.screenPosition;

                var direction = SpriteEffects.None;
                var Gorigin = new Vector2(glow.Width / 2, glow.Height / 2);

                //Main.spriteBatch.Draw(glow, drawPosition, null, Color.Crimson, rotation, Gorigin, 0.05f, direction, 0f);
            }

            alreadyDrawnShieldForPlayer = true;
        }

        if (alreadyDrawnShieldForPlayer)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }
    }

    #region Antishield PowerCreep debuffs

    /*
    // Debuffs to allow through even if they're normally hostile
    private static readonly int[] AllowedDebuffs = new int[]
    {
        ModContent.BuffType<CombatStimBuff>(),
        ModContent.BuffType<StimWithdrawl_Debuff>(),
        ModContent.BuffType<StimAddicted_Debuff>()
    };

    public override void PreUpdateBuffs()
    {
        if(BarrierActive && barrier>0)
        for (int i = 0; i < Player.buffType.Length; i++)
        {
            int buffType = Player.buffType[i];
            if (buffType <= 0)
                continue;

            if (!Main.debuff[buffType])
                continue;

            if (IsAllowedDebuff(buffType))
                continue;

            // Otherwise, remove it — the player is "immune" to it
            //Player.buffImmune[buffType] = true;
            Player.ClearBuff(buffType);
            i--;
        }
    }
    private bool IsAllowedDebuff(int buffType)
    {
        foreach (int allowed in AllowedDebuffs)
            if (buffType == allowed)
                return true;
        return false;
    }*/

    #endregion
}