using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework; // Using another one library
using NoxusBoss.Content.Buffs;
using NoxusBoss.Content.Rarities;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee;// Where is your code locates

public class StupidFuckYouSword : ModItem
{
    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1; // How many items need for research in Journey Mode
    }

    public override void SetDefaults()
    {
        // Visual properties
        Item.width = 40; // Width of an item sprite
        Item.height = 40; // Height of an item sprite
        Item.scale = 0f; // Multiplicator of item size, for example is you set this to 2f our sword will be biger twice. IMPORTANT: If you are using numbers with floating point, write "f" in their end, like 1.5f, 3.14f, 2.1278495f etc.
        Item.rare = ItemRarityID.Blue; // The color of item's name in game. See https://terraria.wiki.gg/wiki/Rarity
        //Item
        // Combat properties
        Item.damage = 5090; // Item damage
        Item.DamageType = DamageClass.Melee; // What type of damage item is deals, Melee, Ranged, Magic, Summon, Generic (takes bonuses from all damage multipliers), Default (doesn't take bonuses from any damage multipliers)
        // useTime and useAnimation often use the same value, but we'll see examples where they don't use the same values
        Item.useTime = 120; // How long the swing lasts in ticks (60 ticks = 1 second)
        Item.useAnimation = 120; // How long the swing animation lasts in ticks (60 ticks = 1 second)
        Item.knockBack = 6f; // How far the sword punches enemies, 20 is maximal value
        Item.autoReuse = true; // Can the item auto swing by holding the attack button
        Item.noUseGraphic = false;


        Item.rare = ModContent.RarityType<NamelessDeityRarity>();
        // Other properties
        Item.value = 10000; // Item sell price in copper coins
        Item.useStyle = ItemUseStyleID.Swing; // This is how you're holding the weapon, visit https://terraria.wiki.gg/wiki/Use_Style_IDs for list of possible use styles
        Item.UseSound = SoundID.Item1; // What sound is played when using the item, all sounds can be found here - https://terraria.wiki.gg/wiki/Sound_IDs
    }

    public override void MeleeEffects(Player player, Rectangle hitbox)
    {
        if (Main.rand.NextBool(3)) // With 1/3 chance per tick (60 ticks = 1 second)...
        {
            // ...spawning dust
            Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), // Position to spawn
            hitbox.Width, hitbox.Height, // Width and Height
            DustID.Poisoned, // Dust type. Check https://terraria.wiki.gg/wiki/Dust_IDs
            0, 0, // Speed X and Speed Y of dust, it have some randomization
            125); // Dust transparency, 0 - full visibility, 255 - full transparency

        }
    }

    // What is happening on hitting living entity
    public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (Main.rand.NextBool(1)) // 1/4 chance, or 25% in other words
        {
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), // Adding Poisoned to target
                 600); // for 5 seconds (60 ticks = 1 second)
            //target.AddBuff(ModContent.BuffType<AntimatterAnnihilation>(), // Adding Poisoned to target
            //     6000); // for 5 seconds (60 ticks = 1 second)
            
        }
    }

    // Creating item craft
    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
         // We are using custom material for the craft, 7 Steel Shards
        recipe.AddIngredient(ItemID.Wood, 3); // Also, we are using vanilla material to craft, 3 Wood
        recipe.AddIngredient(ItemID.JungleSpores, 5); // I've added some Jungle Spores to craft
        recipe.AddTile(TileID.Anvils); // Crafting station we need for craft, WorkBenches, Anvils etc. You can find them here - https://terraria.wiki.gg/wiki/Tile_IDs
        recipe.Register();
    }
}