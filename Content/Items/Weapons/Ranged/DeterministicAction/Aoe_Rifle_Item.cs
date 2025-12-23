using CalamityMod.Projectiles.Ranged;
using HeavenlyArsenal.Core.Globals;
using Luminance.Assets;
using Microsoft.Xna.Framework.Input;
using NoxusBoss.Assets.Fonts;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.GlobalInstances;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria.GameContent.ItemDropRules;
using Terraria.UI.Chat;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction
{
    public class Aoe_Rifle_Item : ModItem
    {
        // IDEAS:
        // 1. hitscan rifle, with chinese kanji and multiple other languages for "finality" or "The end" or some other shtick on impact with something 
        // 2. authority?
        // 3. WOUND THE WORLD

        public override string LocalizationCategory => "Items.Weapons.Ranged";


        public override void SetStaticDefaults()
        {
            ItemID.Sets.gunProj[Type] = true;
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
            Item.value = Terraria.Item.buyPrice(4, 20, 10, 4);
            Item.damage = 13_000;
            Item.crit = -3;
            Item.rare = ModContent.RarityType<AvatarRarity>();
            
            Item.DamageType = DamageClass.Ranged;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.useAmmo = AmmoID.Bullet;
            Item.shootSpeed = 40;    
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.shoot = ModContent.ProjectileType<Aoe_Rifle_HeldProj>();
            Item.noUseGraphic = true;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.knockBack = 7;
        }

        public override void ModifyWeaponCrit(Player player, ref float crit)
        {
            crit = 1;
        }

        public override bool AltFunctionUse(Player player) => true;
        public override void HoldItem(Player player)
        {
            if (player.ownedProjectileCounts[Item.shoot] <1)
            {
                Projectile.NewProjectileDirect(player.GetSource_FromThis(), player.Center, Vector2.Zero, Item.shoot, 0, 0);

            }
        }
        public override bool CanUseItem(Player player) => false;
        public override bool CanShoot(Player player) => false;
        public override void UpdateInventory(Player player)
        {
            if (!Main.keyState.IsKeyDown(Keys.LeftShift))
                return;

            
        }
        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Mod != "HeavenlyArsenal")
                return true;
            Color poemColor = Color.Lerp(Color.Crimson, Color.Red, MathF.Sin(Main.GlobalTimeWrappedHourly*10.1f));
            Vector2 drawPosition = new Vector2(line.X, line.Y);

            // Use a special font.
            line.Font = FontRegistry.Instance.AvatarPoemText;
            line.BaseScale *= new Vector2(0.407f, 0.405f);
            /* if (FontRegistry.RussianGameCulture.IsActive)
                line.BaseScale *= 1f; */ 

            // Draw lines.
            List<string> lines =
            [
                line.Text.Replace("\t", "       ")
            ];

            for (int i = 0; i < lines.Count; i++)
            {
                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, line.Font, lines[i], drawPosition, poemColor, Color.Black, line.Rotation, line.Origin, line.BaseScale, line.MaxWidth, line.Spread * 0.6f);
                drawPosition.X += line.Font.MeasureString(lines[i]).X * line.BaseScale.X;
            }
            return false;
        }
        public override bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
        {
          
            return base.PreDrawTooltip(lines, ref x, ref y);
        }


        internal static void DrawHeldShiftTooltip(List<TooltipLine> tooltips, TooltipLine[] holdShiftTooltips, bool hideNormalTooltip = false)
        {
            //yes, this is literally just the lore item. don't sue me this time.
            // Do not override anything if the Left Shift key is not being held.
            if (!Main.keyState.IsKeyDown(Keys.LeftShift))
                return;

            // Acquire base tooltip data.
            int firstTooltipIndex = -1;
            int lastTooltipIndex = -1;
            int standardTooltipCount = 0;
            for (int i = 0; i < tooltips.Count; i++)
            {
                if (tooltips[i].Name.StartsWith("Tooltip"))
                {
                    if (firstTooltipIndex == -1)
                    {
                        firstTooltipIndex = i;
                    }
                    lastTooltipIndex = i;
                    standardTooltipCount++;
                }
            }

            // Replace tooltips.
            if (firstTooltipIndex != -1)
            {
                if (hideNormalTooltip)
                {
                    tooltips.RemoveRange(firstTooltipIndex, standardTooltipCount);
                    lastTooltipIndex -= standardTooltipCount;
                }
                tooltips.InsertRange(lastTooltipIndex + 1, holdShiftTooltips);
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine fullLore = new TooltipLine(Mod, "thing", this.GetLocalizedValue("Lore"))
            {
                OverrideColor = Color.Crimson
            };
            
            DrawHeldShiftTooltip(tooltips, new TooltipLine[]
            {
            fullLore
            }, true);
        }
    }
}
