using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using HeavenlyArsenal.Content.Projectiles.Weapons.Rogue;
using System;
using CalamityMod;
using HeavenlyArsenal.Content.Projectiles.Weapons.Magic;
using HeavenlyArsenal.ArsenalPlayer;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue;

class LifeAndCessation : ModItem
{
  

    public override void SetStaticDefaults()
    {
       
    }

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;

        
        Item.DamageType = ModContent.GetInstance<RogueDamageClass>();
        Item.damage = 50;
        Item.knockBack = 2f;
        Item.useTime = 5;
        Item.useAnimation = 5;

        // Important for channeling (charging)
        Item.channel = true;
        Item.useTurn = true;

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noUseGraphic = true; 
        Item.shoot = ModContent.ProjectileType<HeldLifeCessationProjectile>();
        Item.shootSpeed = 0f; // The “held projectile” doesn’t really move. lmao.
        Item.autoReuse = true;

        // Sound/consumable details
        Item.UseSound = SoundID.Item1;
        Item.consumable = false;


    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[ModContent.ProjectileType<HeldLifeCessationProjectile>()] <= 0;

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        int style = 1;
        Texture2D bar = AssetDirectory.Textures.Bars.Bar[style].Value;
        Texture2D barCharge = AssetDirectory.Textures.Bars.BarFill[style].Value;


        Rectangle chargeFrame = new Rectangle(0, 0, (int)(barCharge.Width * Main.LocalPlayer.GetModPlayer<HeavenlyArsenalPlayer>().CessationHeat), barCharge.Height);
        Color barColor = Color.Lerp(Color.MediumOrchid, Color.Turquoise, Utils.GetLerpValue(0.3f, 0.8f, Main.LocalPlayer.GetModPlayer<HeavenlyArsenalPlayer>().CessationHeat, true));
        barColor.A = 128;
        spriteBatch.Draw(bar, position + new Vector2(0, 35) * scale, bar.Frame(), Color.DarkSlateBlue, 0, bar.Size() * 0.5f, scale * 1.2f, 0, 0);
        spriteBatch.Draw(barCharge, position + new Vector2(0, 35) * scale, chargeFrame, barColor, 0, barCharge.Size() * 0.5f, scale * 1.2f, 0, 0);
    }

    public override void HoldItem(Player player)
    {
        
        player.GetModPlayer<HeavenlyArsenalPlayer>().CessationHeld = true;
    }
}
