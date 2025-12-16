using System.IO;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon;
using ReLogic.Content;
using Terraria.GameContent.Bestiary;
using Terraria.UI;

namespace HeavenlyArsenal.Content.Biomes;

public sealed class RiftEclipseBestiaryBackgroundProvider : IBestiaryInfoElement, IBestiaryBackgroundImagePathAndColorProvider
{
    Asset<Texture2D> IBestiaryBackgroundImagePathAndColorProvider.GetBackgroundImage()
    {
        return Main.Assets.Request<Texture2D>("Images/MapBG1");
    }

    Color? IBestiaryBackgroundImagePathAndColorProvider.GetBackgroundColor()
    {
        return Color.Black;
    }

    UIElement IBestiaryInfoElement.ProvideUIElement(BestiaryUICollectionInfo info)
    {
        return null;
    }
}