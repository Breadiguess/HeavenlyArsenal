using CalamityMod;
using NoxusBoss.Assets.Fonts;
using NoxusBoss.Content.Rarities;
using ReLogic.Graphics;
using System.Collections.ObjectModel;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI.Chat;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.ZealotsReward
{
    internal class Zealots_Item : ModItem
    {
        private readonly struct WrappedLoreLine
        {
            public readonly string Text;
            public readonly DynamicSpriteFont Font;
            public readonly Color Color;
            public readonly Vector2 Scale;
            public readonly float Height;

            public WrappedLoreLine(string text, DynamicSpriteFont font, Color color, Vector2 scale, float height)
            {
                Text = text;
                Font = font;
                Color = color;
                Scale = scale;
                Height = height;
            }
        }
        public static string Path;

        public override string LocalizationCategory => "Items.Weapons.Ranged";

        public static readonly int ConserveAmmoChance = 70;

        // Bind ConserveAmmoChance into the tooltip
        // Did I ever mention that I really hate localization?
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(ConserveAmmoChance);

        public LocalizedText LoreTip => this.GetLocalization(nameof(LoreTip));
        public const char SpecialFontMarker = '&';
        public const char NDTextMarker = '*';

        private const string LoreScrollAnchorText = "[LORE_SCROLL_ANCHOR]";
        private const int LorePanelWidth = 420;
        private const int LorePanelHeight = 190;
        private const int LorePanelPadding = 8;
        private const float LoreScrollSpeed = 28f; // pixels per second

        public override void Load()
        {
            Path = this.GetPath();
        }

        public override void SetStaticDefaults()
        {
            ItemID.Sets.gunProj[Item.type] = true;
            ItemID.Sets.BonusAttackSpeedMultiplier[Type] = 0.2f;
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;

        }

        public override void SetDefaults()
        {
            Item.damage = 10_000;
            Item.crit = 6;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 40;
            Item.height = 20;
            Item.knockBack = 4;
            Item.rare = ModContent.RarityType<AvatarRarity>();
            Item.shoot = ModContent.ProjectileType<Zealots_Held>();
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.HiddenAnimation;
            Item.useAnimation = 5;
            Item.useTime = 5;
            Item.useAmmo = ItemID.Gel;

        }
        public override void ModifyWeaponCrit(Player player, ref float crit)
        {
            float baseCrit = Item.crit;

            // Everything above the item's own crit is "bonus" crit from gear/buffs/etc.
            float bonusCrit = crit - baseCrit;
            if (bonusCrit < 0f)
                bonusCrit = 0f;

            // This weapon only gets 35% of bonus crit chance.
            crit = baseCrit + bonusCrit * 0.35f;
        }

        public override float UseSpeedMultiplier(Player player) => player.GetModPlayer<Zealots_Player>().UseSpeedMulti;



        #region OnHit
        public override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo)
        {
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            player.GetModPlayer<Zealots_Player>().UseSpeedMulti += 1 / (7 / 6f);

            Rectangle a = target.Hitbox;
            Zealots_FreezeGore.AddFreezeZone(a, 60, 40, false);
            player.GetModPlayer<Zealots_Player>().AltFireCooldown -= 2;
            if (target.TryGetGlobalNPC<Zealots_Stasis_NPC>(out var stasis))
            {
                stasis.AddStacks(target, 1, player.GetSource_OnHit(target) as Terraria.DataStructures.IEntitySource_OnHit, player.whoAmI);
            }
        }



        public override void HoldItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
                player.Calamity().rightClickListener = true;

            player.Calamity().mouseWorldListener = true;

            if (player.whoAmI != Main.myPlayer)
                return;

            if (player.dead || !player.active)
                return;

            if (player.ownedProjectileCounts[Item.shoot] < 1)
            {
                Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item),
                    player.Center,
                    Vector2.Zero,
                    Item.shoot,
                    Item.damage,
                    0,
                    player.whoAmI
                );
            }
        }
        #endregion


        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {

            bool shiftHeld = Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) ||
                             Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift);

            if (!shiftHeld)
            {
                tooltips.Add(new TooltipLine(Mod, "LoreHint", Language.GetOrRegister("Mods.HeavenlyArsenal.Items.Weapons.Zealots_Item.LoreTipOff").Value)
                {
                    OverrideColor = Color.LightGray
                });
                return;
            }

            tooltips.Clear();

            tooltips.Add(new TooltipLine(Mod, "LoreHeader", "--- MEMORY TRACE ---")
            {
                OverrideColor = Color.Cyan
            });
            
            // The extra newlines make the tooltip box tall enough for the scroll panel.
            tooltips.Add(new TooltipLine(Mod, "LoreScrollAnchor", LoreScrollAnchorText + "\n \n \n \n \n \n \n \n \n \n")
            {
                OverrideColor = Color.White
            });
        }
     

        public override bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                TooltipLine line = lines[i];

                if (line.Name.StartsWith("Terraria"))
                    continue;

                if (line.Name == "LoreHint")
                    continue;

                if (line.Name == "LoreScrollAnchor")
                {
                    // Reserve vertical space, but do not draw the anchor text.
                    y += LorePanelHeight;
                    continue;
                }

                if (line.Name == "LoreHeader")
                {
                    DynamicSpriteFont font = FontAssets.MouseText.Value;
                    Color color = line.OverrideColor ?? Color.Cyan;
                    Vector2 drawPos = new Vector2(x, y);

                    ChatManager.DrawColorCodedStringWithShadow(
                        Main.spriteBatch,
                        font,
                        line.Text,
                        drawPos,
                        color,
                        0f,
                        Vector2.Zero,
                        Vector2.One
                    );

                    y += (int)font.MeasureString(line.Text).Y;
                    continue;
                }
            }

            return true;
        }
        public override void PostDrawTooltip(ReadOnlyCollection<DrawableTooltipLine> lines)
        {
            bool shiftHeld = Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) ||
                             Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift);

            if (!shiftHeld)
                return;

            DrawableTooltipLine? anchor = null;

            foreach (DrawableTooltipLine line in lines)
            {
                if (line.Name == "LoreScrollAnchor")
                {
                    anchor = line;
                    break;
                }
            }

            if (anchor is null)
                return;

            string fullLore = LoreTip.Value ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fullLore))
                return;

            Rectangle panelArea = new Rectangle(anchor.X, anchor.Y, LorePanelWidth, LorePanelHeight);
            DrawLorePanel(panelArea);

            List<WrappedLoreLine> wrappedLines = BuildWrappedLoreLines(fullLore, LorePanelWidth - LorePanelPadding * 2);
            if (wrappedLines.Count == 0)
                return;

            float totalHeight = 0f;
            for (int i = 0; i < wrappedLines.Count; i++)
                totalHeight += wrappedLines[i].Height;

            if (totalHeight <= 0f)
                return;

            float scrollOffset = (float)(Main.GlobalTimeWrappedHourly * LoreScrollSpeed) % totalHeight;
            float baseX = panelArea.X + LorePanelPadding;
            float startY = panelArea.Y + LorePanelPadding - scrollOffset;

            for (int pass = 0; pass < 3; pass++)
            {
                float drawY = startY + totalHeight * pass;

                for (int i = 0; i < wrappedLines.Count; i++)
                {
                    WrappedLoreLine wrapped = wrappedLines[i];
                    float currentY = drawY;

                    drawY += wrapped.Height;

                    float val = LumUtils.InverseLerpBump(panelArea.Bottom, panelArea.Bottom - 50, panelArea.Top + 50, panelArea.Top, currentY);
                    Main.NewText(val);

                    ChatManager.DrawColorCodedStringWithShadow(
                        Main.spriteBatch,
                        wrapped.Font,
                        wrapped.Text,
                        new Vector2(baseX, currentY),
                        wrapped.Color * val,
                        0f,
                        Vector2.Zero,
                        wrapped.Scale
                    );
                }
            }
        }
        private List<WrappedLoreLine> BuildWrappedLoreLines(string fullLore, int maxWidth)
        {
            List<WrappedLoreLine> result = new();

            string[] sourceLines = fullLore.Replace("\r\n", "\n").Split('\n');

            for (int i = 0; i < sourceLines.Length; i++)
            {
                string rawLine = sourceLines[i];

                if (string.IsNullOrWhiteSpace(rawLine))
                {
                    result.Add(new WrappedLoreLine(" ", FontAssets.MouseText.Value, Color.White, Vector2.One, 14f));
                    continue;
                }

                DynamicSpriteFont font = GetSpecialFontForRawText(rawLine);
                Vector2 scale = GetFontScaleForRawText(rawLine);
                Color color = GetLineColor(rawLine, i);

                string cleanedLine = StripMarker(rawLine);

                foreach (string wrapped in WrapString(cleanedLine, font, scale, maxWidth))
                {
                    float height = font.MeasureString(wrapped).Y * scale.Y;

                    if (rawLine.StartsWith(SpecialFontMarker))
                        height += 2f;
                    else
                        height *= 0.55f;

                    result.Add(new WrappedLoreLine(wrapped, font, color, scale, height));
                }
            }

            return result;
        }

        private List<string> WrapString(string text, DynamicSpriteFont font, Vector2 scale, int maxWidth)
        {
            List<string> lines = new();

            if (string.IsNullOrWhiteSpace(text))
            {
                lines.Add(" ");
                return lines;
            }

            string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string current = "";

            for (int i = 0; i < words.Length; i++)
            {
                string candidate = string.IsNullOrEmpty(current) ? words[i] : current + " " + words[i];
                float width = font.MeasureString(candidate).X * scale.X;

                if (width <= maxWidth)
                {
                    current = candidate;
                    continue;
                }

                if (!string.IsNullOrEmpty(current))
                    lines.Add(current);

                current = words[i];
            }

            if (!string.IsNullOrEmpty(current))
                lines.Add(current);

            return lines;
        }

        private string StripMarker(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (text.StartsWith(SpecialFontMarker) || text.StartsWith(NDTextMarker)) 
                return text.Substring(1);

            return text;
        }

        private DynamicSpriteFont GetSpecialFontForRawText(string text)
        {
            if (text.StartsWith(NDTextMarker))
                return FontRegistry.Instance.NamelessDeityText;

            if (text.StartsWith(SpecialFontMarker))
                return FontRegistry.Instance.AvatarPoemText;

            return FontAssets.MouseText.Value;
        }

        private Vector2 GetFontScaleForRawText(string text)
        {
            if (text.StartsWith(NDTextMarker))
                return new Vector2(0.4f);

            if (text.StartsWith(SpecialFontMarker))
                return new Vector2(0.407f, 0.405f) * 0.8f;

            return Vector2.One;
        }

        private Color GetLineColor(string text, int index)
        {
            float colorInterpolant = Sin01(index * 0.3f - Main.GlobalTimeWrappedHourly * 3.4f);
            float hue = (0.9f + colorInterpolant * 0.19f) % 1f;

            if (text.StartsWith(SpecialFontMarker))
                return new Color(252, 37, 74);

            if (text.StartsWith(NDTextMarker))
                return Main.hslToRgb(hue, 1f, 0.74f);

            return Color.White;
        }

        private void DrawLorePanel(Rectangle area)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            Main.spriteBatch.Draw(pixel, area, new Color(10, 10, 18, 220));

            int border = 2;
            Color borderColor = Color.Cyan * 0.8f;

            Utils.DrawRect(Main.spriteBatch, area, borderColor);
        }
        private DynamicSpriteFont GetSpecialFontForLine(TooltipLine line)
        {
            if (line.Text.StartsWith(NDTextMarker))
                return FontRegistry.Instance.NamelessDeityText;

            if (line.Text.Contains(SpecialFontMarker))
                return FontRegistry.Instance.AvatarPoemText;

            return FontAssets.MouseText.Value;
        }

        private Vector2 GetFontScale(TooltipLine line)
        {
            if (line.Text.StartsWith(NDTextMarker))
                return new Vector2(0.4f);
            else if (line.Text.StartsWith(SpecialFontMarker))
            {
                return new Vector2(0.407f, 0.405f) * 0.8f;
            }
            else
                return Vector2.One;
        }
        public override bool AltFunctionUse(Player player) => true;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return false;
        }
        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            return !Main.rand.NextBool(ConserveAmmoChance, 100);
        }

    }
}