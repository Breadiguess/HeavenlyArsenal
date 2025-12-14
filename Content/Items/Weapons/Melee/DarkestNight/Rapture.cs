using HeavenlyArsenal.Core.Globals;
using Luminance.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.GlobalInstances;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.DarkestNight;

public class Rapture : ModItem
{
    public override string LocalizationCategory => "Items.Weapons.Melee";

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        ItemID.Sets.gunProj[Type] = true;

        GlobalNPCEventHandlers.ModifyNPCLootEvent += (npc, npcLoot) =>
        {
            if (npc.type == ModContent.NPCType<NamelessDeityBoss>())
            {
                var normalOnly = new LeadingConditionRule(new Conditions.NotExpert());

                {
                    normalOnly.OnSuccess(ItemDropRule.Common(Type));
                }

                npcLoot.Add(normalOnly);
            }
        };

        ArsenalGlobalItem.ModifyItemLootEvent += (item, loot) =>
        {
            if (item.type == NamelessDeityBoss.TreasureBagID)
            {
                loot.Add(ItemDropRule.Common(Type));
            }
        };
    }

    public override void SetDefaults()
    {
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.useStyle = ItemUseStyleID.HiddenAnimation;
        Item.useTime = 7;
        Item.useAnimation = 2;
        Item.reuseDelay = 2;
        Item.crit = 10;
        Item.rare = ModContent.RarityType<NamelessDeityRarity>();
        Item.value = Item.buyPrice(5, 48, 50, 67);
        Item.DamageType = DamageClass.Melee;
        Item.shoot = ModContent.ProjectileType<BlackGlass>();
        Item.damage = 49_374;
    }

    public override bool MeleePrefix()
    {
        return true;
    }

    public override bool AltFunctionUse(Player player)
    {
        return true;
    }

    public override bool CanUseItem(Player player)
    {
        return true;
    }

    public override bool CanShoot(Player player)
    {
        return true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        return false;
    }

    public override void HoldItem(Player player)
    {
        if (player.HasBuff(BuffID.Cursed))
        {
            return;
        }

        if (player.GetModPlayer<GlassPlayer>().Empowered)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<SilentLight>()] < 1)
            {
                var a = Projectile.NewProjectileDirect(player.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<SilentLight>(), Item.damage, Item.knockBack);
                a.CritChance = Item.crit;
                var b = a.ModProjectile as SilentLight;
            }
        }
        else
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<RoaringNight>()] < 1)
            {
                var a = Projectile.NewProjectileDirect(player.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<RoaringNight>(), Item.damage, Item.knockBack);
                a.CritChance = Item.crit;
                var b = a.ModProjectile as RoaringNight;

                b.CreatorItem = player.HeldItem;
            }
        }
    }

    #region Draws

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        var tex = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Melee/DarkestNight/Rapture").Value;
        var Drawpos = position;

        Main.EntitySpriteDraw(tex, Drawpos, null, drawColor, 0f, tex.Size() * 0.5f, scale * 0.031f, SpriteEffects.None);

        return false;
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        var tex = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Melee/DarkestNight/Rapture").Value;
        var DrawPos = Item.position - Main.screenPosition;
        var Origin = tex.Size() * 0.5f;
        Main.EntitySpriteDraw(tex, DrawPos, null, lightColor, rotation, Origin, scale * 0.2f, SpriteEffects.None);

        return false; //base.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
    }

    #endregion
}