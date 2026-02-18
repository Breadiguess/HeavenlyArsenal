using System.Collections.Generic;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Potions;
using HeavenlyArsenal.Common;
using HeavenlyArsenal.Common.Configuration;
using HeavenlyArsenal.Content.Buffs.Stims;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;

namespace HeavenlyArsenal.Content.Items.Consumables.CombatStim;

internal class CombatStimItem : ModItem
{
    public static readonly SoundStyle TakeStim = new("HeavenlyArsenal/Assets/Sounds/Items/CombatStim/CombatStim_use");

    public override string LocalizationCategory => "Items.Consumables";

    public LocalizedText StimDuration { get; private set; }

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        
        StimDuration = this.GetLocalization("StimDuration").WithFormatArgs();
        
        ItemID.Sets.AnimatesAsSoul[Type] = true;
        
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(40 / 3, 3));
    }

    public override void SetDefaults()
    {
        base.SetDefaults();
        
        Item.maxStack = Item.CommonMaxStack;
        
        Item.consumable = true;
        
        Item.width = 46;
        Item.height = 46;
        
        Item.scale = 0.4f;
        
        Item.autoReuse = true;
        
        Item.useTime = 40;
        Item.useAnimation = 40;
        Item.useStyle = ItemUseStyleID.EatFood;

        Item.rare = ItemRarityID.Quest;
        
        Item.value = Item.buyPrice(0, 43, 10);

        Item.buffType = ModContent.BuffType<CombatStimBuff>();
    }

    private static int CalculateStimDuration(in Player player)
    {
        var count = player.GetModPlayer<StimPlayer>().stimsUsed + 1;
        var duration = Math.Abs(count - 160) * 10 / 60;

        return duration;
    }

    public override void OnConsumeItem(Player player)
    {
        base.OnConsumeItem(player);
        
        var count = player.GetModPlayer<StimPlayer>().stimsUsed;

        if (player.GetModPlayer<StimPlayer>().Addicted)
        {
            player.HealEffect(-150);
            
            player.statLife -= 150;
            
            var sound = TakeStim with
            {
                Pitch = 1f,
                PitchVariance = 0.2f,
                Volume = 0.4f
            };

            SoundEngine.PlaySound(in sound, player.Center);

            sound = GennedAssets.Sounds.Common.EarRinging with
            {
                Pitch = 0.1f + count / 10,
                Volume = 0.01f
            };
            
            SoundEngine.PlaySound(in sound, player.Center);

            NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.whoAmI);
        }
        else
        {
            player.HealEffect(-50);
            
            player.statLife -= 50;

            var sound = TakeStim with
            {
                PitchVariance = 0.2f,
                Volume = 0.4f
            };

            SoundEngine.PlaySound(in sound, player.Center);
        }

        if (ClientSideConfiguration.Instance.StimVFX)
        {
            GeneralScreenEffectSystem.RadialBlur.Start(player.Center, 1, 60);
            GeneralScreenEffectSystem.ChromaticAberration.Start(player.Center, 3f, 10);
        }

        if (player.statLife <= 0)
        {
            if (player.GetModPlayer<StimPlayer>().Addicted)
            {
                var message = Language.GetTextValue("Mods.HeavenlyArsenal.PlayerDeathMessages.CombatStimAddicted" + Main.rand.Next(1, 7 + 1), player.name);
                
                player.KillMe(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral(message)), 10000.0, 0);
            }
            else if (player.GetModPlayer<StimPlayer>().Withdrawl)
            {
                var message = Language.GetTextValue("Mods.HeavenlyArsenal.PlayerDeathMessages.CombatStim" + Main.rand.Next(1, 5 + 1), player.name);
                
                player.KillMe(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral(message)), 10000.0, 0);
            }
            else
            {
                var message = Language.GetTextValue("Mods.HeavenlyArsenal.PlayerDeathMessages.CombatStim" + Main.rand.Next(1, 5 + 1), player.name);
               
                player.KillMe(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral(message)), 10000.0, 0);
            }
        }

        var duration = CalculateStimDuration(in player);
        
        player.AddBuff(ModContent.BuffType<CombatStimBuff>(), duration * 60);
    }

    public override void UseAnimation(Player player)
    {
        base.UseAnimation(player);
        
        if (player.whoAmI != Main.myPlayer)
        {
            return;
        }
        
        player.itemRotation = MathHelper.ToRadians(45f * player.direction);
        player.itemLocation = new Vector2(player.Center.X - 40, player.Center.Y + 130);
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        base.ModifyTooltips(tooltips);
        
        var player = Main.LocalPlayer;

        var duration = CalculateStimDuration(in player);

        var line = new TooltipLine(Mod, "CombatStimTooltip", duration + " second duration")
        {
            OverrideColor = Color.White
        };

        tooltips.Add(line);
    }

    public override void AddRecipes()
    {
        CreateRecipe(20)
            .AddIngredient<YharonSoulFragment>(3)
            .AddIngredient<AstralInjection>(6)
            .AddIngredient<BloodOrb>(20)
            .AddIngredient<Bloodstone>(5)
            .AddTile(TileID.Bottles)
            .Register();
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        base.UseStyle(player, heldItemFrame);
        
        if (player.itemAnimation > 0)
        {
            return;
        }
        
        var progress = 1f - player.itemAnimation / (float)Item.useAnimation;
        var offset = new Vector2(16f * player.direction, 2f);

        if (progress < 0.2f)
        {
            player.itemLocation = player.MountedCenter + offset * (1f - progress / 0.2f);
            player.itemRotation = player.direction * MathHelper.Lerp(MathHelper.ToRadians(10f), MathHelper.ToRadians(43f), progress / 0.2f);
        }
        else
        {
            player.itemLocation = player.MountedCenter + offset * 0.225f;
        }
    }

    public override void UseItemFrame(Player player)
    {
        base.UseItemFrame(player);
        
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.itemRotation + MathHelper.TwoPi);
        player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, player.itemRotation - player.direction * MathHelper.PiOver2 * 1.5f);
    }
}