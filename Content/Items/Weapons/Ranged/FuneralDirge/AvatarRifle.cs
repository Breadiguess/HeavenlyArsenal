using CalamityMod;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles.Metaballs;
using NoxusBoss.Content.Rarities;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.FuneralDirge;

internal class AvatarRifle2 : ModItem
{
    public const int ShootDelay = 32;

    public const int BulletsPerShot = 1;

    public const int CycleTimeDelay = 40;

    public const int CycleTime = 120;

    public const int ReloadTime = 360;

    public static int RPM = 20;

    public override string Texture => "HeavenlyArsenal/Content/Items/Weapons/Ranged/FuneralDirge/AvatarRifle";

    public override string LocalizationCategory => "Items.Weapons.Ranged";

    //public static int AmmoType = 
    public override void SetDefaults()
    {
        Item.rare = ModContent.RarityType<NamelessDeityRarity>();

        Item.damage = 9600;
        Item.DamageType = DamageClass.Ranged;
        Item.shootSpeed = 40f;
        Item.width = 40;
        Item.height = 32;
        Item.useTime = 45;
        Item.reuseDelay = 45;
        Item.useAmmo = AmmoID.Bullet;
        Item.useAnimation = 5;
        Item.noUseGraphic = true;
        Item.useTurn = true;
        Item.channel = true;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.knockBack = 6;

        //Item.UseSound = SoundID.Item1;
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<AvatarRifle_Holdout>();
        //Item.shoot = AmmoID.Bullet;
        Item.ChangePlayerDirectionOnShoot = true;
        Item.crit = 87;
        Item.noMelee = true;
        Item.Calamity().devItem = true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        return false;
    }

    private bool AvatarRifle_Out(Player player)
    {
        return player.ownedProjectileCounts[Item.shoot] > 0;
    }

    public override void HoldItem(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            if (!AvatarRifle_Out(player))
            {
                Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, Item.shoot, Item.damage, Item.knockBack, player.whoAmI);
            }
        }
    }

    public override void UpdateInventory(Player player) { }
}

public class AvatarRiflePlayer : ModPlayer
{
    public int AvatarRifleCounter = 7;

    public bool AvatarRifleEmpowered;

    public float AvatarRifleEmpoweredTimer;

    public int AvatarRifleEmpoweredMaxTime = 60 * 13; // 13 seconds

    public int RifleCharge;

    public float RifleChargeDecay;

    public override void ResetEffects()
    {
        //AvatarRifleCounter = 7;
    }

    public override void PostUpdateMiscEffects()
    {
        if (AvatarRifleEmpowered && AvatarRifleEmpoweredTimer > 0)
        {
            AvatarRifleEmpoweredTimer--;
        }
        else if (AvatarRifleEmpowered && AvatarRifleEmpoweredTimer == 0)
        {
            AvatarRifleEmpowered = false;
        }

        if (RifleCharge > 0)
        {
            RifleChargeDecay += 0.1f;

            if (RifleChargeDecay > 13)
            {
                RifleCharge--;
                RifleChargeDecay = -1;
            }
        }
    }

    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (proj.GetGlobalProjectile<AvatarRifleSuperBullet>().hasEmpowerment && proj.GetGlobalProjectile<AvatarRifleSuperBullet>().empowerment >= 0f)
        {
            if (!AvatarRifleEmpowered)
            {
                RifleCharge++;
                RifleChargeDecay = -1;
            }

            if (RifleCharge >= 8)
            {
                RifleCharge = 0;
                AvatarRifleEmpowered = true;
                AvatarRifleEmpoweredTimer = AvatarRifleEmpoweredMaxTime;

                if (Main.netMode != NetmodeID.Server)
                {
                    CombatText.NewText(proj.Hitbox, Color.Red, "EMPOWERED!");
                }
            }
        }

        base.OnHitNPCWithProj(proj, target, hit, damageDone);
    }
}

public class AvatarRifleGlobalNPC : GlobalNPC
{
    private const int maxshotcount = 4;

    private int shotcount;

    private float shredTimer;

    private readonly int MaxShredTime = 13 * 60; // 13 seconds in ticks

    private bool Shredding;

    private int originalDefense = -1; // store original defense

    // make this data per-NPC
    public override bool InstancePerEntity => true; // make this data per-NPC

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (Shredding)
        {
            var texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Tiles/ForgottenShrine/PlacedOfuda1").Value;
            var origin = new Vector2(texture.Width / 2, texture.Height / 2);

            var drawPosition = new Vector2(npc.Center.X, npc.Center.Y - npc.height * 2 + (float)Math.Sin(Main.GlobalTimeWrappedHourly) * 4f) - Main.screenPosition;
            spriteBatch.Draw(texture, drawPosition, null, Color.White, MathHelper.ToRadians(90), origin, 1f, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            Main.spriteBatch.PrepareForShaders();
            //new Texture Placeholder = GennedAssets.Textures.Extra.Code;
            var postProcessingShader = ShaderManager.GetShader("HeavenlyArsenal.FusionRifleClothPostProcessingShader");
            postProcessingShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly);
            postProcessingShader.TrySetParameter("FlameColor", new Color(208, 37, 40).ToVector4());
            postProcessingShader.SetTexture(GennedAssets.Textures.GreyscaleTextures.WhitePixel, 0, SamplerState.LinearWrap);
            postProcessingShader.Apply();
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        }

        //Utils.DrawBorderString(spriteBatch, "| ShotCount: " + shotcount.ToString(), npc.Center - Vector2.UnitY * 150 - Main.screenPosition, Color.White);
        //Utils.DrawBorderString(spriteBatch, "| Defense: " + npc.defense.ToString() + " | DefDefense: "+ npc.defDefense.ToString(), npc.Center - Vector2.UnitY * 130 - Main.screenPosition, Color.White);
        //Utils.DrawBorderString(spriteBatch, "| Shredding: " + Shredding.ToString(), npc.Center - Vector2.UnitY * 110 - Main.screenPosition, Color.White);

        base.PostDraw(npc, spriteBatch, screenPos, drawColor);
    }

    public override void PostAI(NPC npc)
    {
        if (Shredding)
        {
            if (Main.rand.NextBool(1))
            {
                var metalball = ModContent.GetInstance<PaleAvatarBlobMetaball>();

                for (var i = 0; i < 10; i++)
                {
                    var gasSize = 4f; // * Main.rand.NextFloat(0.32f, 1.6f);
                    metalball.CreateParticle(npc.Bottom + npc.velocity * 0.1f, new Vector2(Main.rand.NextFloat(-10, 10), -10), gasSize);
                }
            }
        }

        base.PostAI(npc);
    }

    public override bool PreAI(NPC npc)
    {
        //CombatText.NewText(npc.Hitbox, Color.Gray, $"Shotcount: {shotcount}");
        if (Shredding)
        {
            if (shredTimer > 0)
            {
                if (shredTimer % 4 == 0)
                {
                    var radius = 600f;
                    var player = Main.player[Player.FindClosest(npc.Center, npc.width, npc.height)];

                    var damage = GetPlayerStrongestDamage(player) * 200f;

                    foreach (var target in Main.npc)
                    {
                        if (target.active && !target.friendly && !target.dontTakeDamage && target.whoAmI != npc.whoAmI)
                        {
                            if (Vector2.Distance(npc.Center, target.Center) <= radius)
                            {
                                target.SimpleStrikeNPC((int)damage, 0, true, 0, DamageClass.Generic, true, 50);
                            }
                        }
                    }
                }

                shredTimer--;
            }
            else
            {
                EndShredEffect(npc);
            }
        }

        return base.PreAI(npc);
    }

    public void TriggerShredEffect(NPC npc, int time)
    {
        if (!Shredding)
        {
            originalDefense = npc.defDefense;
            npc.defense = (int)(npc.defDefense * 0.25f); // reduce defense by 75%
            shredTimer = time;
            Shredding = true;

            // Radial damage to nearby NPCs (600px radius)
            var radius = 600f;
            var player = Main.player[Player.FindClosest(npc.Center, npc.width, npc.height)];

            var damage = GetPlayerStrongestDamage(player) * 200f;

            foreach (var target in Main.npc)
            {
                if (target.active && !target.friendly && !target.dontTakeDamage) //&& target.whoAmI != npc.whoAmI)
                {
                    if (Vector2.Distance(npc.Center, target.Center) <= radius)
                    {
                        target.SimpleStrikeNPC((int)damage, 0, true, 0, DamageClass.Generic, true, 50); // apply damage with no knockback
                    }
                }
            }

            if (Main.netMode != NetmodeID.Server)
            {
                CombatText.NewText(npc.Hitbox, Color.Red, "SHREDDED!");
            }

            //Main.NewText($"{npc.FullName} is shredded! Defense reduced by 45% for {time / 60f:F1} seconds.", Color.Orange);
        }
    }

    public void EndShredEffect(NPC npc)
    {
        if (originalDefense != -1)
        {
            npc.defense = originalDefense;
            originalDefense = -1;
        }

        Shredding = false;

        if (Main.netMode != NetmodeID.Server)
        {
            CombatText.NewText(npc.Hitbox, Color.Gray, "Shred faded");
        }
    }

    public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
    {
        if (projectile.GetGlobalProjectile<AvatarRifleSuperBullet>().hasEmpowerment && !Shredding)
        {
            shotcount++;

            if (shotcount >= maxshotcount)
            {
                shotcount = 0;
                TriggerShredEffect(npc, MaxShredTime);
            }
        }

        base.OnHitByProjectile(npc, projectile, hit, damageDone);
    }

    private float GetPlayerStrongestDamage(Player player)
    {
        var maxDamage = player.GetTotalDamage(DamageClass.Magic).ApplyTo(1f);
        maxDamage = Math.Max(maxDamage, player.GetTotalDamage(DamageClass.Melee).ApplyTo(1f));
        maxDamage = Math.Max(maxDamage, player.GetTotalDamage(DamageClass.Ranged).ApplyTo(1f));
        maxDamage = Math.Max(maxDamage, player.GetTotalDamage(DamageClass.Summon).ApplyTo(1f));

        return maxDamage * 40; // base scaling factor for AoE damage
    }
}