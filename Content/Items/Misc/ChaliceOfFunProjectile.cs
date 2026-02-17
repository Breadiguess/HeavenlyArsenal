using CalamityMod;
using CalamityMod.Items.Weapons.Magic;
using HeavenlyArsenal.Content.Buffs.Stims;
using HeavenlyArsenal.Content.Items.Consumables.CombatStim;
using HeavenlyArsenal.Content.Items.Misc;
using Terraria.GameContent;
using Terraria.Localization;

namespace HeavenlyArsenal.Content.Projectiles.Misc;

internal class ChaliceOfFunProjectile : ModProjectile
{
    public bool isDraining;

    public override LocalizedText DisplayName => CalamityUtils.GetItemName<Rancor>();

    public Player Owner => Main.player[Projectile.owner];

    public bool InUse => Owner.controlUseItem && Owner.altFunctionUse == 0;

    public ref float Time => ref Projectile.ai[0];

    public ref float drinkProgress => ref Projectile.ai[1];

    public override void SetDefaults()
    {
        Projectile.DamageType = DamageClass.Generic;
        
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.friendly = true;
        
        Projectile.hide = true;
        
        Projectile.width = 32;
        Projectile.height = 32;
        
        Projectile.penetrate = -1;
        
        Projectile.timeLeft = 90000;

        Projectile.noEnchantmentVisuals = true;
        Projectile.manualDirectionChange = true;
    }

    public override void AI()
    {
        AdjustPlayerHoldValues();

        var rot = (float.Pi - MathHelper.PiOver4) * drinkProgress * -Owner.direction;
        Projectile.rotation = rot + MathHelper.PiOver2;
        var arm = new Player.CompositeArmData(true, Player.CompositeArmStretchAmount.Full, rot);
        var armPosition = Owner.GetFrontHandPositionImproved(arm);
        Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (float.Pi - MathHelper.PiOver4) * drinkProgress * -Owner.direction);
        Projectile.Center = armPosition + new Vector2(0, 10 * -Owner.direction).RotatedBy(Projectile.rotation); // + Vector2.UnitX * Owner.direction * 8f;
        Projectile.velocity = Vector2.Zero;

        Owner.heldProj = Projectile.whoAmI;

        if (Owner.HeldItem?.type != ModContent.ItemType<ChaliceOfFunItem>())
        {
            Projectile.Kill();

            return;
        }

        Lighting.AddLight(Projectile.Center, Color.Crimson.ToVector3());
        Time++;

        if (InUse)
        {
            Drink(Owner);
        }

        if (!InUse)
        {
            drinkProgress = 0;
        }
    }

    public void Drink(Player player)
    {
        drinkProgress = float.Lerp(drinkProgress, 1, 0.2f);

        if (drinkProgress >= 0.99f)
        {
            player.GetModPlayer<StimPlayer>().Addicted = false;
            player.GetModPlayer<StimPlayer>().Withdrawl = false;
            player.GetModPlayer<StimPlayer>().stimsUsed = 0;
            player.ClearBuff(ModContent.BuffType<StimAddicted_Debuff>());

            //Main.NewText($"Dust: {dustLocation}", Color.AntiqueWhite);
            //drained();
        }
    }

    public void drained()
    {
        drinkProgress = 0;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var texture = TextureAssets.Projectile[Type].Value;
        var glow = AssetDirectory.Textures.BigGlowball.Value;
        var Juice = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Projectiles/Misc/ChaliceOfFun_Juice").Value;

        var scale = 0.75f;
        var offset = new Vector2(0, 0);

        var origin = texture.Size() * 0.5f;
        var Gorigin = new Vector2(glow.Width / 2 + 125 * -Owner.direction, glow.Height - 1.6f * glow.Height / 4);

        var drawPosition = Projectile.Center - Main.screenPosition;

        var rotation = Projectile.rotation;

        var direction = Projectile.spriteDirection == 1 ? 0 : SpriteEffects.FlipVertically;
        Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, scale, direction, 0f);

        var glowPosition = Projectile.Center - Main.screenPosition;
        //Main.spriteBatch.Draw(glow, drawPosition, null, Projectile.GetAlpha(lightColor).MultiplyRGB(Color.Crimson), rotation, Gorigin, 0.1f, direction, 0f);

        Main.spriteBatch.Draw(Juice, drawPosition, null, Projectile.GetAlpha(lightColor).MultiplyRGB(Color.Crimson), rotation, origin, scale, direction, 0f);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);

        return false;
    }

    public void AdjustPlayerHoldValues()
    {
        Projectile.spriteDirection = Owner.direction;
        Projectile.timeLeft = 2;
        Owner.heldProj = Projectile.whoAmI;
    }

    public override bool? CanDamage()
    {
        return false;
    }
}