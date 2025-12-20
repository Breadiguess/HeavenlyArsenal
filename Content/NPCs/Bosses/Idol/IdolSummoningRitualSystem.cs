using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Core.SoundSystems;
using NoxusBoss.Core.Utilities;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Idol;

public partial class IdolSummoningRitualSystem : ModSystem
{
    /// <summary>
    /// How long it's been since the summoning ritual began.
    /// </summary>
    public int Timer
    {
        get;
        set;
    }

    /// <summary>
    /// The state of the animation.
    /// </summary>
    public IdolSummoningRitualState State
    {
        get;
        set;
    }

    /// <summary>
    /// Whether the summoning ritual is ongoing or not.
    /// </summary>
    public bool IsActive => Timer >= 1;

    /// <summary>
    /// The volume of the base wind sound.
    /// </summary>
    public float BaseWindSoundVolume
    {
        get;
        set;
    }

    /// <summary>
    /// The volume of the harsh wind sound.
    /// </summary>
    public float HarshWindSoundVolume
    {
        get;
        set;
    }

    /// <summary>
    /// The sound instance for the basic wind.
    /// </summary>
    public static LoopedSoundInstance BaseWindSoundInstance
    {
        get;
        private set;
    }

    /// <summary>
    /// The sound instance for the harsh wind.
    /// </summary>
    public static LoopedSoundInstance HarshWindSoundInstance
    {
        get;
        private set;
    }

    /// <summary>
    /// Handles the natural updating of sounds, making them crossfade and dissipate as necessary.
    /// </summary>
    private void UpdateSounds()
    {
        const float cutoffVolume = 0.001f;
        if ((BaseWindSoundInstance is null || BaseWindSoundInstance.HasBeenStopped) && BaseWindSoundVolume > cutoffVolume)
            BaseWindSoundInstance = LoopedSoundManager.CreateNew(new SoundStyle("HeavenlyArsenal/Assets/Sounds/Environment/IdolSummonWind"), () => BaseWindSoundVolume <= cutoffVolume);
        if ((HarshWindSoundInstance is null || HarshWindSoundInstance.HasBeenStopped) && HarshWindSoundVolume > cutoffVolume)
            HarshWindSoundInstance = LoopedSoundManager.CreateNew(new SoundStyle("HeavenlyArsenal/Assets/Sounds/Environment/IdolSummonWindHarsh"), () => HarshWindSoundVolume <= cutoffVolume);
        UpdateLoopSound(BaseWindSoundInstance, BaseWindSoundVolume);
        UpdateLoopSound(HarshWindSoundInstance, HarshWindSoundVolume);
    }

    private static void UpdateLoopSound(LoopedSoundInstance sound, float volume)
    {
        if (sound is null)
            return;

        bool wasInactive = !sound.HasLoopSoundBeenStarted;
        sound.Update(Main.LocalPlayer.Center, s =>
        {
            s.Volume = volume;
        });

        // Ensure that the first frame of the sound's playing isn't with a volume of 1.
        if (sound.HasLoopSoundBeenStarted && wasInactive && SoundEngine.TryGetActiveSound(sound.LoopingSoundSlot, out ActiveSound? soundInstance) && soundInstance is not null)
            soundInstance.Sound.Volume = 0f;
    }

    /// <summary>
    /// Starts the summoning ritual.
    /// </summary>
    public void Start()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        State = IdolSummoningRitualState.WorldRumble;
        NetMessage.SendData(MessageID.WorldData);
    }

    /// <summary>
    /// Transitions the current state of this summoning ritual to something else, resetting old data in the process.
    /// </summary>
    public void SwitchState(IdolSummoningRitualState newState)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        Timer = 0;
        State = newState;
        NetMessage.SendData(MessageID.WorldData);
    }

    /// <summary>
    /// Resets the overall state of this summoning ritual.
    /// </summary>
    public void Reset()
    {
        Timer = 0;
        State = IdolSummoningRitualState.Inactive;
        RumbleInterpolant = 0f;
    }

    public override void PostUpdateWorld()
    {
        UpdateSounds();
        if (State == IdolSummoningRitualState.Inactive)
        {
            BaseWindSoundVolume = (BaseWindSoundVolume * 0.98f).StepTowards(0f, 0.004f);
            HarshWindSoundVolume = (HarshWindSoundVolume * 0.98f).StepTowards(0f, 0.004f);
            return;
        }

        switch (State)
        {
            case IdolSummoningRitualState.WorldRumble:
                Perform_WorldRumble();
                break;
            case IdolSummoningRitualState.OpenStatueEye:
                Perform_OpenStatueEye();
                break;
            case IdolSummoningRitualState.BatheWorldInCrimson:
                Perform_BatheWorldInCrimson();
                break;
        }
        Timer++;

        ScreenShakeSystem.StartShake(RumbleInterpolant * 2f, MathHelper.Pi / 2.1f, Vector2.UnitX * Main.rand.NextFromList(-1f, 1f));
    }

    public override void ClearWorld() => Reset();

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(Timer);
        writer.Write((byte)State);
    }

    public override void NetReceive(BinaryReader reader)
    {
        Timer = reader.ReadInt32();
        State = (IdolSummoningRitualState)reader.ReadByte();
    }
}
