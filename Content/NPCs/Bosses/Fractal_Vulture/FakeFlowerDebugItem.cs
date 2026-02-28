using CalamityMod.Items.Materials;
using Luminance.Assets;
using NoxusBoss.Content.NPCs.Bosses.Draedon;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.World.WorldSaving;
using Terraria.Localization;

namespace HeavenlyArsenal.Content.NPCs.Bosses.FractalVulture;

public class FakeFlowerDebugItem : ModItem
{
    public override string Texture => MiscTexturesRegistry.PixelPath;

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;
        ItemID.Sets.ItemNoGravity[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.width = 36;
        Item.height = 50;
        Item.value = 0;
        Item.rare = ModContent.RarityType<GenesisComponentRarity>();
        Item.DefaultToPlaceableTile(ModContent.TileType<FakeFlowerTile>());
        Item.Wrath().GenesisComponent = true;
    }
}