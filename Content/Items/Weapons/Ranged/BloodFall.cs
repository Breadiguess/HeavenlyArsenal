using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles;
using CalamityMod.Projectiles.Ranged;
using Luminance.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Player = Terraria.Player;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged
{
    #region sins
    //i must confess my sins- the base code for this item was stolen from calamity community remix.
    // as i have only ever hated on calamity community remix, this is to  my deep shame. 
    #endregion
    public class BloodFall : ModItem, ILocalizedModType
    {
        public override string LocalizationCategory => "Items.Ranged";
        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Ranged;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3.5f;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.shootSpeed = 12f;
            Item.channel = true;
            Item.width = 20;
            Item.height = 46;
            Item.damage = 84;
            Item.crit = 16;
            Item.useTime = 2;
            Item.useAnimation = 28;

            Item.useAmmo = AmmoID.Arrow;
            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.rare = ItemRarityID.Yellow;
            Item.shoot = ModContent.ProjectileType<BloodFallTelegraph>();
            if (ModLoader.TryGetMod("CalRemix", out Mod CalamityRemix))
            {

                Item.DamageType = CalamityRemix.Find<DamageClass>("StormbowDamageClass");
            }
        }
        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            // Only consume ammo if BloodFallTelegraph exists and its WindupInterpolant is greater than 0
            foreach (Projectile projectile in Main.projectile)
            {
                if (projectile.type == ModContent.ProjectileType<BloodFallTelegraph>())
                {
                    BloodFallTelegraph telegraph = (BloodFallTelegraph)projectile.ModProjectile;
                    if (telegraph.WindupInterpolant >= 0.999f)
                    {
                        return true;
                    }
                }
                
                
            }
            return false;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<BloodFallTelegraph>()] <= 0)
            {
                if (player.altFunctionUse == 0)
                {
                    Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<BloodFallTelegraph>(), damage, 0, player.whoAmI, 0, 0);
                }
            }
            return false;
        }


        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.BloodRainBow).
                AddIngredient(ItemID.Cobweb, 15).
                AddIngredient<LivingShard>(12).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }


    public class BloodFallTelegraph : ModProjectile
    {
        public ref Player Owner => ref Main.player[Projectile.owner];
        public ref float Time => ref Projectile.ai[0];
        public ref float WindupInterpolant => ref Projectile.ai[1];

        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 36;
            Projectile.timeLeft = 180;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            // Keep projectile alive as long as channeling or windup > 0
            Projectile.timeLeft = 3;
            if (!Owner.channel && !Owner.controlUseItem && WindupInterpolant <= 0f)
            {
                Projectile.Kill();
                return;
            }

            // Update windup
            if (WindupInterpolant < 1f && Owner.channel)
            {
                WindupInterpolant = Math.Min(1f, WindupInterpolant + 0.025f);
            }
            else if (!Owner.channel && WindupInterpolant > 0f)
            {
                WindupInterpolant = Math.Max(0f, WindupInterpolant - 0.025f);
            }

            // Telegraph follows mouse
            Vector2 center = Projectile.Center;
            center = Main.MouseWorld;
            Projectile.Center = center;

            // Determine drop angle based on horizontal offset
            float dx = Projectile.Center.X - Owner.Center.X;
            float t = MathHelper.Clamp(dx / 800f, -1f, 1f);
            float angle = MathHelper.Lerp(MathHelper.PiOver2, MathHelper.PiOver4, t);
            Projectile.rotation = MathHelper.PiOver2 + angle;

            // Smoothly interpolate speed based on windup
            float minSpeed = 15f;
            float maxSpeed = 45f;
            float currentSpeed = MathHelper.Lerp(minSpeed, maxSpeed, WindupInterpolant);
            float speedX = (float)Math.Cos(angle) * currentSpeed;
            float speedY = (float)Math.Sin(angle) * currentSpeed;

            // Prevent clipping into ground
            int tileX = (int)(Projectile.Center.X / 16f);
            int tileY = (int)(Projectile.Center.Y / 16f);
            int maxTilesDown = 200;
            int groundTileY = -1;
            for (int i = 0; i < maxTilesDown; i++)
            {
                if (WorldGen.SolidTile(tileX, tileY + i))
                {
                    groundTileY = tileY + i;
                    break;
                }
            }
            if (groundTileY > 0)
            {
                float groundYWorld = groundTileY * 16f;
                // Clamp telegraph just above ground to avoid sinking
                if (Projectile.Center.Y > groundYWorld - 10f)
                {
                    Vector2 updatedCenter = Projectile.Center;
                    updatedCenter.Y = groundYWorld - 10f;
                    Projectile.Center = updatedCenter;
                }
            }

            // Spawn arrows periodically
            if (Time % 5 == 0 && WindupInterpolant >= 1f)
            {
                for (int i = 1; i < 10 + 1; i++)
                {
                    Vector2 spawnOffset = new Vector2(
                        Main.rand.NextFloat(-100f, 100f),
                        Main.rand.NextFloat(-100f, 300f) + 500f
                    ).RotatedBy(Projectile.rotation);

                    Vector2 spawnPos = Projectile.Center + spawnOffset;
                    // Clamp arrow spawn above ground
                    if (groundTileY > 0)
                    {
                        float groundYWorld = groundTileY * 16f;
                        spawnPos.Y = Math.Min(spawnPos.Y, groundYWorld - 20f);
                    }
                    /*
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), spawnPos,  new Vector2(speedX + Main.rand.NextFloat(-5f, 5f), speedY),
                        ModContent.ProjectileType<BloodfireArrowProj>(),
                        Projectile.damage,
                        Projectile.knockBack,
                        Owner.whoAmI
                    );
                    */
                    if(Main.rand.NextBool(4))
                        Owner.statLife += 1;
                    int shotArrow = Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, new Vector2(speedX + Main.rand.NextFloat(-5f, 5f), speedY), ModContent.ProjectileType<BloodfireArrowProj>(), Projectile.damage, Projectile.knockBack, Owner.whoAmI);
                    CalamityGlobalProjectile cgp = Main.projectile[shotArrow].Calamity();
                    cgp.allProjectilesHome = true;
                }
            }

            Time++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
            Vector2 origin = pixel.Size() / 2f;

            // Draw laser line from sky to ground
            float beamHeight = 2000f;
            Vector2 beamStart = Projectile.Center - Vector2.UnitY * beamHeight;

            // Determine ground Y for beam end
            int tileX = (int)(Projectile.Center.X / 16f);
            int tileY = (int)(Projectile.Center.Y / 16f);
            int groundTileY = -1;
            for (int i = 0; i < 200; i++)
            {
                if (WorldGen.SolidTile(tileX, tileY + i))
                {
                    groundTileY = tileY + i;
                    break;
                }
            }
            float groundYWorld = (groundTileY > 0 ? groundTileY * 16f : Projectile.Center.Y);
            Vector2 beamEnd = new Vector2(Projectile.Center.X, groundYWorld);

            Vector2 diff = beamEnd - beamStart;
            float rot = diff.ToRotation() - MathHelper.PiOver2;
            float len = diff.Length();

            sb.Draw(pixel, beamStart - Main.screenPosition, null, Color.Red * 0.3f, rot, origin, new Vector2(40f, len), SpriteEffects.None, 0f);

            // Draw windup indicator
            Utils.DrawBorderString(sb, $"Windup: {WindupInterpolant:0.00}", Projectile.Center - Vector2.UnitY * 60f - Main.screenPosition, Color.White);

            // Draw telegraph area
            Vector2 scale = new Vector2(400f * WindupInterpolant, 1000f);
            sb.Draw(pixel, Projectile.Center - Main.screenPosition, null, Color.White * 0.5f, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);

            return false;
        }
    }
}