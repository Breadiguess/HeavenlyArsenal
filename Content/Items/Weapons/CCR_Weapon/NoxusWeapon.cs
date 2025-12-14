using System.Linq;
using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Particles;
using CalamityMod.Rarities;
using HeavenlyArsenal.Common;
using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Content.Particles.Metaballs.NoxusGasMetaball;
using Luminance.Assets;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using Terraria.Audio;
using Terraria.DataStructures;
using static HeavenlyArsenal.Content.Items.Weapons.CCR_Weapon.EntropicCrystal;
using Player = Terraria.Player;

namespace HeavenlyArsenal.Content.Items.Weapons.CCR_Weapon;

internal class NoxusWeapon : ModItem
{
    private bool BowOut(Player player)
    {
        return player.ownedProjectileCounts[Item.shoot] > 0;
    }

    public override void HoldItem(Player player)
    {
        if (!BowOut(player))
        {
            var Bow = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, Item.shoot, Item.damage, Item.knockBack, player.whoAmI);
            Bow.rotation = MathHelper.PiOver2 - 1f * player.direction;
        }
    }

    /// <summary>
    ///     detonates all entropic crystals owned by the player.
    ///     6/9/2025, 7:58 AM EST
    /// </summary>
    private void TriggleExplosion(Player player)
    {
        if (!player.dead)
        {
            for (var i = 0; i < player.ownedProjectileCounts[ModContent.ProjectileType<EntropicCrystal>()]; i++)

            {
                //todo: fix this code only triggering the first crystal it finds, instead of all of them.
                var crystals = Main.projectile.Where(p => p.active && p.type == ModContent.ProjectileType<EntropicCrystal>() && p.owner == player.whoAmI);

                foreach (var crystal in crystals)
                {
                    if (crystal != null && crystal.ai[1] == (float)EntropicCrystalState.PostHit)
                    {
                        crystal.ai[1] = (float)EntropicCrystalState.Exploding;
                    }
                }
            }
        }
    }

    #region setup

    public static int AltDamage = 4093;

    public static HeavenlyArsenalServerConfig Config => ModContent.GetInstance<HeavenlyArsenalServerConfig>();

    public override bool IsLoadingEnabled(Mod mod)
    {
        // Check config setting
        var enabledInConfig = ModContent.GetInstance<HeavenlyArsenalServerConfig>().EnableSpecialItems;
        var isOtherModLoaded = ModLoader.HasMod("CalRemix");

        return enabledInConfig || isOtherModLoaded;
    }

    public override string LocalizationCategory => "Items.Weapons.Ranged";

    public override void SetDefaults()
    {
        Item.shoot = ModContent.ProjectileType<TheDarkOne>();
        Item.DamageType = DamageClass.Ranged;
        Item.useAmmo = AmmoID.Arrow;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.crit = 10;
        Item.damage = 4900;
        Item.rare = ModContent.RarityType<Violet>();
        Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
        Item.useTime = 1;
        Item.useAnimation = 1;
        Item.shootSpeed = 40;
        Item.noUseGraphic = true;

        Item.UseSound = GennedAssets.Sounds.Common.TwinkleMuffled with
        {
            Volume = 0,
            Pitch = 0.3f,
            PitchVariance = 0.4f
        };

        if (ModLoader.TryGetMod("CalRemix", out var CalamityRemix))
        {
            Item.DamageType = CalamityRemix.Find<DamageClass>("StormbowDamageClass");
        }
    }

    #endregion

    #region Modplayer Integration

    public override float UseSpeedMultiplier(Player player)
    {
        return player.GetModPlayer<NoxusWeaponPlayer>().CrystalSpeedMulti;
    }

    public override void ModifyWeaponCrit(Player player, ref float crit)
    {
        crit += 1.5f * player.GetModPlayer<NoxusWeaponPlayer>().CrystalCount;
        base.ModifyWeaponCrit(player, ref crit);
    }

    #endregion

    #region UseStuff

    public override bool CanShoot(Player player)
    {
        return false;
    }

    public override bool? UseItem(Player player)
    {
        if (player.altFunctionUse == 2)
        {
            TriggleExplosion(player);

            return false;
        }

        return false;
    }

    public override bool CanConsumeAmmo(Item ammo, Player player)
    {
        return true;
    }

    public override bool CanUseItem(Player player)
    {
        return true;
    }

    public override bool AltFunctionUse(Player player)
    {
        return true;
    }

    #endregion
}

public class NoxusWeaponPlayer : ModPlayer
{
    public int CrystalCap = 100;

    public float CrystalSpeedMulti = 1;

    /// <summary>
    ///     track the amount of crystals currently lodged in NPCs for the purposes of crit scaling and
    ///     maybe some other fun things
    /// </summary>
    public int CrystalCount { get; set; }

    public override void PostUpdateMiscEffects()
    {
        CrystalCount = 0;

        // filter only “live” crystals
        var liveCrystals = Main.projectile
            .Where
            (
                p =>
                    p.active &&
                    p.type == ModContent.ProjectileType<EntropicCrystal>() &&
                    p.owner == Player.whoAmI &&
                    p.ai[1] != (float)EntropicCrystalState.PreHit &&
                    p.ai[1] != (float)EntropicCrystalState.DisipateHarmlessly
            )
            .ToList();

        // how many are over-cap?
        var over = Math.Max(0, liveCrystals.Count - CrystalCap);

        /*
        if (over > 0)
        {
            var toExplode = liveCrystals.OrderBy(p => p.timeLeft).Take(over);

            foreach (var proj in toExplode)
            {
                proj.ai[1] = (float)EntropicCrystalState.Exploding;
                proj.netUpdate = true;
            }
        }
        */
        var interp = Math.Clamp(liveCrystals.Count / 16f, 0f, 2f);
        CrystalSpeedMulti = MathHelper.Lerp(CrystalSpeedMulti, 2f, interp);
    }

    public override void ResetEffects()
    {
        CrystalCount = 0;
        CrystalSpeedMulti = 1;
    }
}

public class NoxusWeaponNPC : GlobalNPC
{
    public int AttachedCrystalCount;

    public bool ChokingOnFumes { get; set; }

    public override bool InstancePerEntity => true;

    public override void UpdateLifeRegen(NPC npc, ref int damage)
    {
        if (ChokingOnFumes) { }
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (ChokingOnFumes)
        {
            for (var i = 0; i < 2; i++)
            {
                var spawnPosition = npc.Center + Main.rand.NextVector2Circular(20f, 20f);
                var velocity = Main.rand.NextVector2Circular(1f, 1f);
                var color = Color.Lerp(Color.Purple, Color.Black, Main.rand.NextFloat(0.5f));
                NoxusGasMetaball.CreateParticle(spawnPosition, velocity, Main.rand.NextFloat(10f, 30f));
            }
        }
    }
}

public class EntropicBlast : ModProjectile
{
    public override bool? CanDamage()
    {
        if (Projectile.ai[1] == 0)
        {
            return false;
        }

        return true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Crystal.ai[1] = (float)EntropicCrystalState.DisipateHarmlessly;
        HasHit = true;
    }

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
    }

    public override void OnSpawn(IEntitySource source)
    {
        portalInterp = 0;

        if (source is EntitySource_Parent parentSource && parentSource.Entity is Projectile parentProj)
        {
            if (parentProj.type == ModContent.ProjectileType<EntropicCrystal>())
            {
                Crystal = parentProj;

                if (Crystal != null && Crystal.active && Crystal.ModProjectile is EntropicCrystal entropicCrystal)
                {
                    SpawnPos = Projectile.Center;
                    Offset = Projectile.Center - Crystal.Center;
                }
            }
        }
    }

    public override void AI()
    {
        ManageState();
        portalInterp = (float)Math.Clamp(Math.Round(MathHelper.Lerp(portalInterp, PortalOpen ? 1.1f : -0.1f, PortalOpen ? 0.3f : 0.05f), 2), 0, 1);

        if (Crystal.active == false)
        {
            PortalOpen = false;
        }

        //todo: set the spawn pos and normal velocity to be offest in comparison to the crystal, while still allowing the projectile to move under its own power.
        if (portalInterp == 0.5f) { }

        if (portalInterp == 0 && PortalOpen == false)
        {
            Projectile.Kill();
        }

        Time++;
    }

    private void ManageState()
    {
        switch (CurrentStage)
        {
            case BlastStage.portal:
                HandlePortal();

                break;
            case BlastStage.Bolt:
                HandleBolt();

                break;
        }
    }

    private void HandleBolt()
    {
        if (Time == 1)
        {
            for (var i = 0; i < 20; i++)
            {
                NoxusGasMetaball.CreateParticle
                    (Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), Projectile.velocity.RotatedByRandom(0.68f) * Main.rand.NextFloat(0.9f), Main.rand.NextFloat(13f, 56f));
            }

            SoundEngine.PlaySound(SoundID.Item103, Projectile.Center);

            SoundEngine.PlaySound
            (
                GennedAssets.Sounds.Common.Glitch with
                {
                    MaxInstances = 0,
                    Volume = 0.1f,
                    PitchVariance = 0.4f
                }
            );

            //SoundEngine.PlaySound(GennedAssets.Sounds.Common.EarRinging with { MaxInstances = 0, Volume = 0.05f });
            float screenshakePower = 1; //MathHelper.Lerp(Owner.GetModPlayer<NoxusWeaponPlayer>().CrystalCount, 10, 3);
            ScreenShakeSystem.SetUniversalRumble(screenshakePower, MathHelper.TwoPi, null, 0.2f);
            //Main.NewText($"screenshakePower: {screenshakePower}, {Owner.GetModPlayer<NoxusWeaponPlayer>().CrystalCount}");
        }

        if (Time <= 10 || !HasHit)
        {
            //todo: make the projectile always stay relative to the crystal,
            //allowing for the boss to move around but still get hit by the projectile
            //this will preventing stray crystals that never explode due to never being deactivated.
            Projectile.velocity = Projectile.rotation.ToRotationVector2() * 60;

            // Add a mild amount of slithering movement.
            var slitherOffset = (float)Math.Sin(Time / 6.4f + Projectile.identity) * Utils.GetLerpValue(10f, 25f, Time, true) * 6.2f;
            var perpendicularDirection = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
            Projectile.Center += perpendicularDirection * slitherOffset;

            Projectile.rotation = Projectile.velocity.ToRotation();

            for (var i = 0; i < 4; i++)
            {
                var gasSize = Utils.GetLerpValue(-3f, 25f, Time, true) * Projectile.width * 0.68f;
                var angularOffset = (float)Math.Sin(Time / 5f) * 0.77f;

                NoxusGasMetaball.CreateParticle
                    (Projectile.Center + Projectile.velocity * 2f, Main.rand.NextVector2Circular(2f, 2f) + Projectile.velocity.RotatedBy(angularOffset).RotatedByRandom(0.6f) * 0.26f, gasSize);
            }

            // Spawn particles.
            for (var i = 0; i < 20; i++)
            {
                var voidColor = Color.Lerp(Color.Purple, Color.Black, Main.rand.NextFloat(0.54f, 0.9f));
                voidColor = Color.Lerp(voidColor, Color.DarkBlue, Main.rand.NextFloat(0.4f));

                HeavySmokeParticle darkGas = new
                (
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    Main.rand.NextVector2Circular(1f, 1f),
                    voidColor,
                    11,
                    Projectile.scale * 1.24f,
                    Projectile.Opacity * 0.6f,
                    Main.rand.NextFloat(0.02f),
                    true
                );

                GeneralParticleHandler.SpawnParticle(darkGas);
            }

            FadeInterp = 1;
        }
        else
        {
            FadeInterp -= 0.1f;
            Projectile.velocity *= 0.8f;
            Utils.SmoothStep(0, 255, FadeInterp);
            Fadeout = (byte)Math.Clamp(Utils.Lerp(0, byte.MaxValue, FadeInterp), 0, 255);

            if (Fadeout <= 3 && !Projectile.hide)
            {
                Projectile.hide = true;
                Projectile.timeLeft = 60;
            }
        }
    }

    private void HandlePortal()
    {
        Projectile.Opacity = (float)Math.Pow(Projectile.scale, 2.6f);

        if (Crystal != null && Crystal.ai[2] > 0)
        {
            var npcIndex = (int)Crystal.ai[2] - 1;

            if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
            {
                var targetNpc = Main.npc[npcIndex];

                if (targetNpc != null && targetNpc.active)
                {
                    Projectile.rotation = (targetNpc.Center - Projectile.Center).ToRotation();
                    //SpawnPos = Offset + targetNpc.Center;
                    Projectile.Center = SpawnPos;

                    thing = npcIndex;
                }
            }
        }

        /*
        for (int i = 0; i < 2; i++)
        {

            Vector2 lightAimPosition = SpawnPos + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloatDirection() * Projectile.scale * 50f + Main.rand.NextVector2Circular(10f, 10f);
            Vector2 lightSpawnPosition = SpawnPos + Projectile.velocity * 75f + Projectile.velocity.RotatedByRandom(2.83f) * Main.rand.NextFloat(700f);
            Vector2 lightVelocity = (lightAimPosition - lightSpawnPosition) * 0.06f;
            SquishyLightParticle light = new(lightSpawnPosition, lightVelocity, 0.33f * portalInterp, Color.Pink, 19, 0.04f, 3f, 8f);
            GeneralParticleHandler.SpawnParticle(light);
        }*/
        if (Time == 0)
        {
            var darkParticle = NoxusPortal.pool.RequestParticle();
            darkParticle.Prepare(SpawnPos, Vector2.Zero, Color.AntiqueWhite, Projectile.rotation, Projectile.timeLeft, portalInterp, Projectile);
            ParticleEngine.BehindProjectiles.Add(darkParticle);
        }

        if (Time > 60)
        {
            for (var i = 0; i < 2; i++)
            {
                var voidColor = Color.Lerp(Color.Purple, Color.Black, Main.rand.NextFloat(0.54f, 0.9f));
                voidColor = Color.Lerp(voidColor, Color.DarkBlue, Main.rand.NextFloat(0.4f));

                HeavySmokeParticle darkGas = new
                (
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    Main.rand.NextVector2Circular(1f, 1f),
                    voidColor,
                    11,
                    Projectile.scale * 1.24f,
                    Projectile.Opacity * 0.6f,
                    Main.rand.NextFloat(0.02f),
                    true
                );

                GeneralParticleHandler.SpawnParticle(darkGas);
            }

            Projectile.ai[1] = (float)BlastStage.Bolt;
            Time = 0;
        }
    }

    public void DrawPortal(Vector2 DrawPos)
    {
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

        var PortalShader = ShaderManager.GetShader("HeavenlyArsenal.PortalShader");

        PortalShader.TrySetParameter("circleStretchInterpolant", portalInterp);
        PortalShader.TrySetParameter("transformation", Matrix.CreateScale(10f, 2f, 2f));

        //PortalShader.TrySetParameter("aimDirection", Projectile.rotation + MathHelper.PiOver2);
        PortalShader.TrySetParameter
        (
            "uColor",
            Color.MediumPurple with
            {
                A = 0
            }
        );

        //PortalShader.TrySetParameter("uSecondaryColor", Color.White);
        PortalShader.TrySetParameter("edgeFadeInSharpness", 2.3f);
        PortalShader.TrySetParameter("aheadCircleMoveBackFactor", 0.67f);
        PortalShader.TrySetParameter("aheadCircleZoomFsctor", 0.09f);
        //PortalShader.TrySetParameter("uProgress", portalInterp * Main.GlobalTimeWrappedHourly);
        PortalShader.TrySetParameter("uTime", Main.GlobalTimeWrappedHourly);

        PortalShader.SetTexture(GennedAssets.Textures.GreyscaleTextures.StarDistanceLookup, 0);
        PortalShader.SetTexture(GennedAssets.Textures.Noise.TurbulentNoise, 1);
        PortalShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 2);
        PortalShader.SetTexture(GennedAssets.Textures.Extra.Void, 3);
        PortalShader.SetTexture(GennedAssets.Textures.GreyscaleTextures.Spikes, 4);

        PortalShader.Apply();
        Texture2D pixel = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
        var maxScale = 5f;
        var textureArea = Projectile.Size / pixel.Size() * maxScale;
        var scaleMod = 1f + (float)(Math.Cos(Main.GlobalTimeWrappedHourly * 15f + Projectile.identity) * 0.012f);
        textureArea *= scaleMod;

        Main.spriteBatch.Draw(pixel, DrawPos, null, Projectile.GetAlpha(Color.MediumPurple), Projectile.rotation, pixel.Size() * 0.5f, textureArea, 0, 0f);
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
    }

    public void DrawBolt(Vector2 DrawPos)
    {
        var Drawcolor = Color.Purple with
        {
            A = Fadeout
        };

        var texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/DarkComet").Value;
        Main.EntitySpriteDraw(texture, DrawPos, null, Drawcolor * Projectile.Opacity, Projectile.rotation, texture.Size() * 0.5f, 1f, SpriteEffects.None);
        CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Drawcolor * Projectile.Opacity);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var PortalDraw = SpawnPos - Main.screenPosition;
        // if(portalInterp > 0)
        // DrawPortal(PortalDraw);

        //Utils.DrawBorderString(Main.spriteBatch, "Interp: " + portalInterp.ToString() + " | Pos: " + SpawnPos.ToString(), PortalDraw - Vector2.UnitY * 110, Color.AntiqueWhite, 1);

        if (Projectile.ai[1] == (float)BlastStage.Bolt)
        {
            DrawBolt(Projectile.Center - Main.screenPosition);
        }

        return false;
    }

    #region setup

    public ref Player Owner => ref Main.player[Projectile.owner];

    public Vector2 SpawnPos;

    private Vector2 Offset;

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    private enum BlastStage
    {
        portal,

        Bolt
    }

    private BlastStage CurrentStage => (BlastStage)Projectile.ai[1];

    public ref float Time => ref Projectile.ai[0];

    public ref float thing => ref Projectile.ai[2];

    private Projectile Crystal;

    public float FadeInterp;

    public byte Fadeout = 255;

    private bool PortalOpen = true;

    public float portalInterp;

    private bool HasHit;

    public override void SetDefaults()
    {
        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.hostile = false;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.timeLeft = 240;
        Projectile.damage = NoxusWeapon.AltDamage;
        Projectile.width = Projectile.height = 60;
    }

    #endregion
}