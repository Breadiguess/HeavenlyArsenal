using CalamityMod;
using CalamityMod.Rarities;
using HeavenlyArsenal.Core.Globals;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Core.GlobalInstances;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.GameContent.ItemDropRules;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.AvatarSpear;

public class AvatarLonginus : ModItem
{
    private string _lastApplied;

    public override string LocalizationCategory => "Items.Weapons.Melee";

    public override void SetStaticDefaults()
    {
        ItemID.Sets.Spears[Type] = true;
        ItemID.Sets.gunProj[Type] = true;
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

        GlobalNPCEventHandlers.ModifyNPCLootEvent += (npc, npcLoot) =>
        {
            if (npc.type == ModContent.NPCType<AvatarOfEmptiness>())
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
            if (item.type == AvatarOfEmptiness.TreasureBagID)
            {
                loot.Add(ItemDropRule.Common(Type));
            }
        };
    }

    public override void SetDefaults()
    {
        Item.rare = ModContent.RarityType<HotPink>();

        Item.CanBeEnchantedBySomething();
        Item.IsEnchantable();
        Item.damage = 17_537;
        Item.shootSpeed = 40f;
        Item.crit = 43;
        Item.width = 40;
        Item.height = 32;
        Item.useTime = 40;
        Item.reuseDelay = 40;

        Item.value = Item.buyPrice(5, 48, 50, 67);
        // Item.buyPrice(1, 46, 30, 2);

        Item.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>();
        Item.useAnimation = 0;
        Item.useTurn = true;
        Item.channel = true;
        Item.knockBack = 3;
        Item.autoReuse = true;
        Item.ChangePlayerDirectionOnShoot = true;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.ArmorPenetration = 4;
        Item.shoot = ModContent.ProjectileType<AvatarLonginusHeld>();
    }

    public override void UpdateInventory(Player player)
    {
        var coreName = ComputeDynamicName(player);
        var desired = coreName;

        if (_lastApplied != desired)
        {
            if (string.IsNullOrEmpty(coreName))
            {
                Item.ClearNameOverride();
            }
            else
            {
                Item.SetNameOverride(desired);
            }

            _lastApplied = desired;
        }

        if (player.GetModPlayer<AvatarSpearHeatPlayer>().Empowered)
        {
            Item.damage = (int)(Item.OriginalDamage * 1.4f);
        }
        else
        {
            Item.damage = (int)(Item.OriginalDamage * 0.96f);
        }
    }

    private string ComputeDynamicName(Player player)
    {
        var actualName = (string)this.GetLocalization("DisplayName");
        var AwakenedName = (string)this.GetLocalization("EmpoweredName");

        if (player.ownedProjectileCounts[Item.shoot] > 0)
        {
            foreach (var projectile in Main.projectile)
            {
                if (projectile.active && projectile.type == Item.shoot && projectile.owner == player.whoAmI)
                {
                    var avatarSpear = projectile.ModProjectile as AvatarLonginusHeld;

                    if (avatarSpear != null && avatarSpear.IsEmpowered)
                    {
                        return AwakenedName;
                    }

                    break;
                }
            }
        }

        return actualName;
    }

    private bool SpearOut(Player player)
    {
        return player.ownedProjectileCounts[Item.shoot] > 0;
    }

    public override void HoldItem(Player player)
    {
        if (!SpearOut(player))
        {
            var spear = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, Item.shoot, Item.damage, Item.knockBack, player.whoAmI);
            spear.rotation = -MathHelper.PiOver2 + 1f * player.direction;
        }
    }

    public override bool AltFunctionUse(Player player)
    {
        return true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        return false;
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        {
            Texture2D tex = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Melee/AvatarSpear/AvatarLonginusHeld").Value;

            frame = tex.Frame(1, 2, 0, Main.LocalPlayer.GetModPlayer<AvatarSpearHeatPlayer>().Empowered ? 1 : 0);
            Main.EntitySpriteDraw(tex, position, frame, drawColor, 0, frame.Size() / 2, scale, 0, 0);



            return false;
        }
       
    }
}