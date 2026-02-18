using Terraria.DataStructures;

namespace HeavenlyArsenal.Utilities.Extensions;

/// <summary>
///     Provides <see cref="PlayerDrawSet" /> extension methods.
/// </summary>
public static class PlayerDrawSetExtensions
{
    /// <summary>
    ///     Calculates the screen-space position used to draw the player's head.
    /// </summary>
    /// <param name="drawInfo">
    ///     The <see cref="PlayerDrawSet" /> containing the current draw state.
    /// </param>
    /// <returns>The screen-space position used to draw the player's head.</returns>
    /// <remarks>
    ///     The calculation accounts for the player's world position, screen offset,
    ///     body frame alignment, head position, and all relevant draw offsets to ensure
    ///     correct placement relative to the player's current state and equipment.
    /// </remarks>
    public static Vector2 GetHeadDrawPosition(this PlayerDrawSet drawInfo)
    {
        var drawPlayer = drawInfo.drawPlayer;

        var position = new Vector2
        (
            (int)(drawInfo.Position.X - Main.screenPosition.X - drawPlayer.bodyFrame.Width / 2f + drawPlayer.width / 2f),
            (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.bodyFrame.Height + 4f)
        );

        return drawInfo.drawPlayer.GetHelmetDrawOffset() + position + drawPlayer.headPosition + drawInfo.headVect + drawInfo.helmetOffset;
    }

    /// <summary>
    ///     Calculates the screen-space position used to draw the player's body.
    /// </summary>
    /// <param name="drawInfo">
    ///     The <see cref="PlayerDrawSet" /> containing the current draw state.
    /// </param>
    /// <returns>The screen-space position used to draw the player's body.</returns>
    /// <remarks>
    ///     The calculation accounts for the player's world position, screen offset, body frame
    ///     alignment, body position, and all relevant draw offsets to ensure correct placement
    ///     relative to the player's current state and equipment.
    /// </remarks>
    public static Vector2 GetBodyDrawPosition(this PlayerDrawSet drawInfo)
    {
        var drawPlayer = drawInfo.drawPlayer;

        var position = new Vector2
        (
            (int)(drawInfo.Position.X - Main.screenPosition.X - drawInfo.drawPlayer.bodyFrame.Width / 2f + drawInfo.drawPlayer.width / 2f),
            (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawInfo.drawPlayer.height - drawInfo.drawPlayer.bodyFrame.Height + 4f)
        );

        return position + drawInfo.drawPlayer.bodyPosition + drawPlayer.bodyFrame.Size() / 2f;
    }

    /// <summary>
    ///     Calculates the screen-space position used to draw the player's legs.
    /// </summary>
    /// <param name="drawInfo">
    ///     The <see cref="PlayerDrawSet" /> containing the current draw state.
    /// </param>
    /// <returns>The screen-space position used to draw the player's legs.</returns>
    /// <remarks>
    ///     The calculation accounts for the player's world position, screen offset, legs frame
    ///     alignment, legs position, and all relevant draw offsets to ensure correct placement
    ///     relative to the player's current state and equipment.
    /// </remarks>
    public static Vector2 GetLegsDrawPosition(this PlayerDrawSet drawInfo)
    {
        var drawPlayer = drawInfo.drawPlayer;

        var position = new Vector2
        (
            (int)(drawInfo.Position.X - Main.screenPosition.X - drawPlayer.legFrame.Width / 2f + drawPlayer.width / 2f),
            (int)(drawInfo.Position.Y - Main.screenPosition.Y + drawPlayer.height - drawPlayer.legFrame.Height + 4f)
        );

        return position + drawPlayer.legPosition + drawInfo.legVect;
    }
}