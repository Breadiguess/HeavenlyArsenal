using SubworldLibrary;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Subworlds;

public class ForgottenShrineSystem : ModSystem
{
    public override void PreUpdateEntities()
    {
        if (!SubworldSystem.IsActive<ForgottenShrineSubworld>())
            return;

        ModContent.GetInstance<ForgottenShrineBackground>().ShouldBeActive = true;
    }
}
