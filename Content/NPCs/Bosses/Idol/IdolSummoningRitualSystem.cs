using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
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
        if (State == IdolSummoningRitualState.Inactive)
            return;

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
