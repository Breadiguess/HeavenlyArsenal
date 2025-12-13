using CalamityMod.Items.Materials;
using Luminance.Assets;
using NoxusBoss.Content.NPCs.Bosses.Draedon;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Content.Tiles.GenesisComponents;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.World.WorldSaving;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

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

    [JITWhenModsEnabled(CalamityCompatibility.ModName)]
    public override void AddRecipes()
    {
        CreateRecipe(1).
            AddTile(TileID.WorkBenches).
            AddIngredient<ExoPrism>(5).
            AddIngredient(ItemID.Seed).
            AddCondition(Language.GetText("Mods.NoxusBoss.Conditions.ObtainedBefore"), BossDownedSaveSystem.HasDefeated<MarsBody>).
            Register();
    }
}
