using HeavenlyArsenal.Core.Globals;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.GlobalInstances;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.AntishadowAssassin;

public class AntishadowBead : ModItem
{
    /// <summary>
    /// The amount of minion slots needed to summon the assassin.
    /// </summary>
    public static int MinionSlotRequirement => 5;
    /// <summary>
    /// Return a shorthand path for a given texture content prefix and name.
    /// </summary>
    public static string GetAssetPath(string prefix, string name) =>
        $"HeavenlyArsenal/{prefix}/{name}";

    public override string Texture => GetAssetPath("Content/Items/Weapons/Summon/AntishadowAssassin", Name);
    public override string LocalizationCategory => "Items.Weapons.Summon";
    public override void SetStaticDefaults()
    {
        GlobalNPCEventHandlers.ModifyNPCLootEvent += (NPC npc, NPCLoot npcLoot) =>
        {
            if (npc.type == ModContent.NPCType<AvatarOfEmptiness>())
            {
                LeadingConditionRule normalOnly = new LeadingConditionRule(new Conditions.NotExpert());
                {
                    normalOnly.OnSuccess(ItemDropRule.Common(Type));
                }
                npcLoot.Add(normalOnly);
            }
        };
        ArsenalGlobalItem.ModifyItemLootEvent += (Item item, ItemLoot loot) =>
        {
            if (item.type == AvatarOfEmptiness.TreasureBagID)
                loot.Add(ItemDropRule.Common(Type));
        };
    }

    public override void SetDefaults()
    {
        Item.width = 16;
        Item.height = 16;
        Item.damage = 2075;
        Item.mana = 19;
        Item.useTime = Item.useAnimation = 32;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.noMelee = true;
        Item.knockBack = 2f;
        Item.value = 0;
        Item.rare = ModContent.RarityType<AvatarRarity>();
        Item.UseSound = SoundID.Item44;
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<AntishadowAssassin>();
        Item.shootSpeed = 10f;
        Item.DamageType = DamageClass.Summon;
    }

    // Ensure that the player can only summon one assassin.
    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.altFunctionUse != 2)
        {
            int p = Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, type, damage, knockback, player.whoAmI);
            if (Main.projectile.IndexInRange(p))
                Main.projectile[p].originalDamage = Item.damage;
        }
        return false;
    }
}
