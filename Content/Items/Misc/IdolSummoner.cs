using HeavenlyArsenal.Content.NPCs.Bosses.Idol;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Misc
{
    public class IdolSummoner : ModItem
    {
        public override string Texture => "NoxusBoss/Assets/Textures/Content/Items/Debugging/DebugItem";

        public override void SetStaticDefaults() => Item.ResearchUnlockCount = 0;

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 36;
            Item.useAnimation = 40;
            Item.useTime = 40;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.UseSound = null;
            Item.rare = ItemRarityID.Blue;
            Item.value = 0;
        }

        public override bool? UseItem(Player p)
        {
            if (Main.myPlayer == NetmodeID.MultiplayerClient || p.itemAnimation != p.itemAnimationMax - 1)
                return false;

            if (ModContent.GetInstance<IdolSummoningRitualSystem>().IsActive)
                ModContent.GetInstance<IdolSummoningRitualSystem>().Reset();
            else
                ModContent.GetInstance<IdolSummoningRitualSystem>().Start();
            return null;
        }
    }
}