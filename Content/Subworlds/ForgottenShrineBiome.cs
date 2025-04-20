using Microsoft.Xna.Framework;
using SubworldLibrary;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Subworlds;

public class ForgottenShrineBiome : ModBiome
{
    public override ModWaterStyle WaterStyle => ModContent.Find<ModWaterStyle>("HeavenlyArsenal/ForgottenShrineWater");

    public override SceneEffectPriority Priority => SceneEffectPriority.BossHigh;

    public override Color? BackgroundColor => Color.White;

    public override int Music => MusicLoader.GetMusicSlot("HeavenlyArsenal/Assets/Sounds/Music/PerfectShrineWithoutAnIdol");

    public override bool IsBiomeActive(Player player) => SubworldSystem.IsActive<ForgottenShrineSubworld>();

    public override float GetWeight(Player player) => 0.97f;
}
