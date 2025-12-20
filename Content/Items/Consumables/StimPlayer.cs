using HeavenlyArsenal.Common;
using HeavenlyArsenal.Common.Configuration;
using HeavenlyArsenal.Content.Buffs.Stims;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using Terraria.Audio;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Consumables.CombatStim;

public sealed class StimPlayer : ModPlayer
{
    public int stimsUsed;

    public int MAX_STIMS_USED = 10;

    public bool Withdrawl;

    public bool Addicted;

    public float addictionChance;

    public int LoseStimTimer;

    public int timeSinceLastStim;

    private readonly int WithdrawlDuration = 32 * 60;

    private int WithdrawlTime;

    private readonly int AddictionCheckInterval = 20 * 60;

    public override void Initialize()
    {
        timeSinceLastStim = 0;
    }

    public override void PostUpdate()
    {
        LoseStimTimer++;
        //Main.NewText($"Stims Used: {stimsUsed}, Addiction chance: {addictionChance}, Widthdrawl: {Withdrawl}, Addicted {Addicted}, Time since last Stim: {timeSinceLastStim}, addictionCheckInterval = {AddictionCheckInterval}, withdrawltime: {WithdrawlTime}, LoseStimTimer: {LoseStimTimer }", Color.AntiqueWhite);

        addictionChance = (float)Math.Clamp(Math.Pow(stimsUsed, stimsUsed / 2), 0, 100);

        // MathHelper.Clamp(10*stimsUsed/3+stimsUsed,0,100);
        if (Withdrawl)
        {
            if (Player.HasBuff(ModContent.BuffType<StimAddicted_Debuff>()))
            {
                Player.ClearBuff(ModContent.BuffType<StimAddicted_Debuff>());
            }

            //YOU CAN'T HAVE ADDICTED AND WITHDRAWL AT THE SAME TIME STOP IT
            if (Addicted)
            {
                Withdrawl = false;

                return;
            }

            if (!Player.HasBuff(ModContent.BuffType<StimWithdrawl_Debuff>()))
            {
                WithdrawlTime = 1;
                Player.AddBuff(ModContent.BuffType<StimWithdrawl_Debuff>(), WithdrawlDuration, false); // Apply debuff
            }

            if (WithdrawlTime >= WithdrawlDuration)
            {
                Withdrawl = false;
            }

            //some extra stuff for psychosis
            if (ClientSideConfiguration.Instance.StimVFX)
            {
                //only you can hear it, so you look like a freak
                if (Main.netMode != NetmodeID.Server)
                {
                    if (WithdrawlTime % Main.rand.Next(50, 100) == 0 && !Player.HasBuff<CombatStimBuff>())
                    {
                        ScreenShakeSystem.StartShake(1f, shakeStrengthDissipationIncrement: 1f);

                        var VoiceSpawnOffset = Player.Center + Main.rand.NextVector2CircularEdge(400, 460);
                        var a = AssetDirectory.Sounds.Items.CombatStim.PsychosisWhisper;
                        var sound = Main.soundVolume;

                        SoundEngine.PlaySound
                        (
                            a with
                            {
                                Volume = 0.2f,
                                PitchVariance = 0.5f,
                                MaxInstances = 0,
                                PlayOnlyIfFocused = true,
                                Type = SoundType.Sound
                            },
                            VoiceSpawnOffset
                        );
                    }
                }
            }

            WithdrawlTime++;
        }

        else if (Addicted)
        {
            timeSinceLastStim++;

            if (Player.HasBuff(ModContent.BuffType<StimWithdrawl_Debuff>()))
            {
                Player.ClearBuff(ModContent.BuffType<StimWithdrawl_Debuff>());
            }

            if (!Player.HasBuff(ModContent.BuffType<StimAddicted_Debuff>()))
            {
                Player.AddBuff(ModContent.BuffType<StimAddicted_Debuff>(), AddictionCheckInterval); // Apply debuff
            }

            if (timeSinceLastStim >= AddictionCheckInterval)
            {
                Withdrawl = true;
                Addicted = false;
            }
        }

        if (LoseStimTimer == 2200 && stimsUsed > 0)
        {
            stimsUsed -= 1;
            LoseStimTimer = -1;
        }
    }

    public void UseStim()
    {
        stimsUsed++;
        timeSinceLastStim = 0;

        // Update addiction chance
        LoseStimTimer = 0;
        WithdrawlTime = 0;

        // Check for addiction
        if (Main.rand.NextFloat() < addictionChance / 100f)
        {
            Addicted = true;
            Withdrawl = false;
        }

        if (Addicted)
        {
            Player.ClearBuff(ModContent.BuffType<StimAddicted_Debuff>());
        }
    }

    public void EndWithdrawl()
    {
        Addicted = false;
        Withdrawl = false;
        stimsUsed = 0;
        addictionChance = 0f;
    }

    public override void UpdateDead()
    {
        Addicted = false;
        Withdrawl = false;
        stimsUsed = 0;
        addictionChance = 0f;
        timeSinceLastStim = 0;
    }
}

internal class StimDraw : PlayerDrawLayer
{
    public float Wane;

    public float d;

    public override bool IsHeadLayer => false;

    public override Position GetDefaultPosition()
    {
        return new AfterParent(PlayerDrawLayers.Head);
    }

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        return true;
        // drawInfo.drawPlayer.head == EquipLoader.GetEquipSlot(Mod, nameof(ShintoArmorHelmetAll), EquipType.Head);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        // DrawDebug(ref drawInfo);

        ManageStimVFX(drawInfo.drawPlayer);
    }

    public void ManageStimVFX(Player player)
    {
        //prevent horrid visual effects if the right config option is selected
        if (!ClientSideConfiguration.Instance.StimVFX)
        {
            return;
        }

        var stimPlayer = player.GetModPlayer<StimPlayer>();

        if (player.HasBuff(ModContent.BuffType<CombatStimBuff>()))
        {
            Wane = Math.Clamp(2 * stimPlayer.stimsUsed / stimPlayer.MAX_STIMS_USED, 0.1f, 2);
        }
        else
        {
            // Smoothly return to 0
            Wane = MathHelper.Lerp(Wane, 0f, 0.1f); // adjust 0.1f for smoothness
        }

        d = MathHelper.Lerp(d, Math.Clamp(stimPlayer.stimsUsed, 0, 20), 0.001f);

        var suctionShader = ShaderManager.GetFilter("HeavenlyArsenal.StimAddicted");

        suctionShader.TrySetParameter("globalTime", Main.GlobalTimeWrappedHourly);
        suctionShader.TrySetParameter("intensityFactor", Wane);
        suctionShader.TrySetParameter("psychadelicExponent", 0);
        suctionShader.TrySetParameter("psychedelicColorTint", Color.Crimson.ToVector4());
        suctionShader.TrySetParameter("colorAccentuationFactor", d);
        suctionShader.SetTexture(GennedAssets.Textures.Noise.VoronoiNoise, 0, SamplerState.AnisotropicWrap);

        suctionShader.SetTexture(GennedAssets.Textures.Noise.VoronoiNoise, 1, SamplerState.AnisotropicWrap);
        suctionShader.SetTexture(GennedAssets.Textures.Noise.VoronoiNoise, 2);

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
}