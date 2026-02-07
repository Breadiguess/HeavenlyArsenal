using System.Collections.Generic;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Potions;
using HeavenlyArsenal.Common;
using HeavenlyArsenal.Content.Buffs.Stims;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;
using Player = Terraria.Player;

namespace HeavenlyArsenal.Content.Items.Consumables.CombatStim;

internal class CombatStim : ModItem
{
    public static readonly SoundStyle TakeStim = new("HeavenlyArsenal/Assets/Sounds/Items/CombatStim/CombatStim_use");

    public override string LocalizationCategory => "Items.Consumables";

    public LocalizedText StimDuration { get; private set; }

    public override void SetStaticDefaults()
    {
        StimDuration = this.GetLocalization("StimDuration").WithFormatArgs();
        //Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 8));
        ItemID.Sets.AnimatesAsSoul[Type] = true;
        //ItemID.Sets.ItemNoGravity[Type] = true;
        //ItemID.Sets.ItemNoGravity[Type] = true;
        //ItemID.Sets.ItemIconPulse[Type] = true;
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(40 / 3, 3));
    }

    public override void SetDefaults()
    {
        Item.width = 10;
        Item.height = 10;
        Item.useStyle = ItemUseStyleID.EatFood;
        Item.useAnimation = 40;
        Item.useTime = 40;

        Item.consumable = true;
        Item.maxStack = 9999;
        Item.value = Item.buyPrice(0, 43, 10);
        Item.rare = ItemRarityID.Quest;
        Item.scale = 0.4f;
        Item.autoReuse = true;

        Item.buffType = ModContent.BuffType<CombatStimBuff>();
    }

    private int CalculateStimDuration(Player player)
    {
        var Stimsused = player.GetModPlayer<StimPlayer>().stimsUsed + 1;
        var a = Math.Abs(Stimsused - 160) * 10 / 60;

        return a;
    }

    public override void OnConsumeItem(Player player)
    {
        var Stimsused = player.GetModPlayer<StimPlayer>().stimsUsed;

        if (player.GetModPlayer<StimPlayer>().Addicted)
        {
            player.HealEffect(-150);
            player.statLife -= 150;
            //TODO: call net update

            SoundEngine.PlaySound
            (
                TakeStim with
                {
                    Pitch = 1f,
                    PitchVariance = 0.2f,
                    Volume = 0.4f
                },
                player.Center
            );

            SoundEngine.PlaySound
            (
                GennedAssets.Sounds.Common.EarRinging with
                {
                    Pitch = 0.1f + Stimsused / 10,
                    Volume = 0.01f
                },
                player.Center
            );

            NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.whoAmI);
        }
        else
        {
            player.HealEffect(-50);
            player.statLife -= 50;

            SoundEngine.PlaySound
            (
                TakeStim with
                {
                    PitchVariance = 0.2f,
                    Volume = 0.4f
                },
                player.Center
            );
        }

        if (HeavenlyArsenalClientConfig.Instance.StimVFX)
        {
            GeneralScreenEffectSystem.ChromaticAberration.Start(player.Center, 3f, 10);
            GeneralScreenEffectSystem.RadialBlur.Start(player.Center, 1, 60);
        }

        //player.GetModPlayer<StimPlayer>().UseStim();

        if (player.statLife <= 0)
        {
            if (player.GetModPlayer<StimPlayer>().Addicted)
            {
                var deathMessage = Language.GetTextValue("Mods.HeavenlyArsenal.PlayerDeathMessages.CombatStimAddicted" + Main.rand.Next(1, 7 + 1), player.name);
                player.KillMe(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral(deathMessage)), 10000.0, 0);
            }
            else if (player.GetModPlayer<StimPlayer>().Withdrawl)
            {
                var deathMessage = Language.GetTextValue("Mods.HeavenlyArsenal.PlayerDeathMessages.CombatStim" + Main.rand.Next(1, 5 + 1), player.name);
                player.KillMe(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral(deathMessage)), 10000.0, 0);
            }
            else
            {
                var deathMessage = Language.GetTextValue("Mods.HeavenlyArsenal.PlayerDeathMessages.CombatStim" + Main.rand.Next(1, 5 + 1), player.name);
                player.KillMe(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral(deathMessage)), 10000.0, 0);
            }
        }

        var StimDuration = CalculateStimDuration(player);
        player.AddBuff(ModContent.BuffType<CombatStimBuff>(), StimDuration * 60);
    }

    public override void UseAnimation(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            player.itemLocation = new Vector2(player.Center.X - 40, player.Center.Y + 130);
            player.itemRotation = MathHelper.ToRadians(45f * player.direction);
        }
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        var player = Main.LocalPlayer;

        var stimDuration = CalculateStimDuration(player);

        var line = new TooltipLine(Mod, "CombatStimTooltip", stimDuration + " second duration")
        {
            OverrideColor = Color.White
        };

        tooltips.Add(line);
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        var texture = TextureAssets.Item[Type].Value;
        var StimFrame = texture.Frame(1, 3); //new Rectangle(1, 3, 46, 46);

        Main.EntitySpriteDraw(texture, position, StimFrame, drawColor, 0f, origin, scale, SpriteEffects.None);

        return false;
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        var texture = TextureAssets.Item[Type].Value;
        var StimFrame = texture.Frame(1, 3);

        var DrawPos = Item.position - Main.screenPosition;
        DrawPos += new Vector2(0, -12);
        var Origin = new Vector2(texture.Width / 2, StimFrame.Height / 2);
        Main.EntitySpriteDraw(texture, DrawPos, StimFrame, lightColor, 0f, Origin, scale, SpriteEffects.None);

        return false;
    }

    public override void AddRecipes()
    {
        CreateRecipe(20)
            .AddIngredient<YharonSoulFragment>(3)
            .AddIngredient<AstralInjection>(6)
            // .AddIngredient(ItemID.BottledWater)
            .AddIngredient<BloodOrb>(20)
            .AddIngredient<Bloodstone>(5)
            .AddTile(TileID.Bottles)
            .Register();
    }

    #region Firing Animation

    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        //if (player.ItemAnimationJustStarted)
        //  Main.NewText($"<player>: stim started", Color.Green);
        if (player.itemAnimation > 0)
        {
            var progress = 1f - player.itemAnimation / (float)Item.useAnimation;
            var injectionOffset = new Vector2(16 * player.direction, 2);

            if (progress < 0.2f)
            {
                player.itemLocation = player.MountedCenter + injectionOffset * (1 - progress / 0.2f);

                player.itemRotation = player.direction *
                                      MathHelper.Lerp
                                      (
                                          MathHelper.ToRadians(10), // start angle
                                          MathHelper.ToRadians(43), // end angle (pointing in)
                                          progress / 0.2f
                                      );
            }
            else
            {
                player.itemLocation = player.MountedCenter + injectionOffset * 0.225f;
            }
        }
    }

    public override void UseItemFrame(Player player)
    {
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.itemRotation + MathHelper.TwoPi);
        player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, player.itemRotation - player.direction * MathHelper.PiOver2 * 1.5f);
    }

    #endregion
}