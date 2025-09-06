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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static HeavenlyArsenal.Content.Items.Weapons.CCR_Weapon.EntropicCrystal;
using static Luminance.Common.Utilities.Utilities;
using Player = Terraria.Player;

namespace HeavenlyArsenal.Content.Items.Weapons.CCR_Weapon
{
    class NoxusWeapon : ModItem
    {
        #region setup
        public static int AltDamage = 4093;
        public static HeavenlyArsenalServerConfig Config => ModContent.GetInstance<HeavenlyArsenalServerConfig>();

        public override bool IsLoadingEnabled(Mod mod)
        {
            // Check config setting
            bool enabledInConfig = ModContent.GetInstance<HeavenlyArsenalServerConfig>().EnableSpecialItems;
            bool isOtherModLoaded = ModLoader.HasMod("CalRemix");

            return enabledInConfig || isOtherModLoaded;
        }

        public override string LocalizationCategory => "Items.Weapons.Ranged";

        public override void SetDefaults()
        {
            //Item.CloneDefaults(ItemID.BloodRainBow);
            //Item.shoot = ModContent.ProjectileType<EntropicCrystal>();
            Item.shoot = ModContent.ProjectileType<TheDarkOne>();
            Item.DamageType = DamageClass.Ranged;
            Item.useAmmo = AmmoID.Arrow;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.crit = 40;
            Item.damage = 4900;
            Item.rare = ModContent.RarityType<Violet>();
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.shootSpeed = 40;
            Item.noUseGraphic = true;
            Item.UseSound = GennedAssets.Sounds.Common.TwinkleMuffled with { Volume = 0,Pitch = 0.3f, PitchVariance = 0.4f };
            if (ModLoader.TryGetMod("CalRemix", out Mod CalamityRemix))
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
        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2) 
            {
                TriggleExplosion(player);
                return true; 
            }
            else

                return base.UseItem(player);
        }
        public override void UseAnimation(Player player)
        {
            // Copy the Blood Rain Bow's use animation: raise the bow above the player and tilt it forward.
            // This mimics the vanilla Blood Rain Bow animation.
            player.itemLocation.X = player.MountedCenter.X + (player.direction * 10f);
            player.itemLocation.Y = player.MountedCenter.Y - 10f;

            // Set the item rotation to point upward and slightly forward.
            float rotation = -MathHelper.PiOver4 * player.direction;
            player.itemRotation = rotation;

            // Optionally, you can set itemAnimation and itemTime to match the Blood Rain Bow's timing.
            // These are handled by CloneDefaults(ItemID.DaedalusStormbow), but can be tweaked if needed.
        }
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {

        }
        public override void OnConsumeAmmo(Item ammo, Player player)
        {
            if(!ConsumeAmmo(player))
            {
                //prevent ammo from decreasing
                
            }
            
        }
        public override bool CanUseItem(Player player)
        {
            return base.CanUseItem(player);
        }
        public override bool AltFunctionUse(Player player){return true;}

        #endregion

        private bool BowOut(Player player) => player.ownedProjectileCounts[Item.shoot] > 0;

        public override void HoldItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                if (!BowOut(player))
                {
                    Projectile Bow = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, Item.shoot, Item.damage, Item.knockBack, player.whoAmI);
                    Bow.rotation = MathHelper.PiOver2 - 1f * player.direction;
                }
            }
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return false;
            if (player.altFunctionUse == 2)
                return false;

            int CrystalAmount = 3;
            float spawnHeight = 600f;
            float horizSpread = 200f;
            int delayPerCrystal = 3;  // ticks between each

            // center X of spawn is player.Center.X, not adding MouseWorld.X twice
            for (int i = 0; i < CrystalAmount; i++)
            {
                // random X offset
                float offsetX = Main.rand.NextFloat(-horizSpread, horizSpread);
                Vector2 spawnPos = new Vector2(Main.MouseWorld.X + offsetX, (Main.MouseWorld.Y + player.Center.Y)/2 - spawnHeight);

                
                Vector2 aimDir = (Main.MouseWorld - spawnPos).SafeNormalize(Vector2.UnitY);
                aimDir = Vector2.Lerp(Vector2.UnitY, aimDir, 0.5f);
                Vector2 projVel = aimDir * Item.shootSpeed;

               
                float startDelay = i * delayPerCrystal;

                Projectile.NewProjectile(
                    source,
                    spawnPos,
                    projVel,
                    ModContent.ProjectileType<EntropicCrystal>(),
                    damage,
                    knockback,
                    player.whoAmI,
                    ai0: startDelay,
                    ai1: 0f
                );

            }

            // we handled the spawning ourselves
            return false;
        }


        // Prevent ammo from being consumed if the player uses the alt function
        public bool ConsumeAmmo(Player player)
        {
            // If using alt function (right-click), do not consume ammo
            if (player.altFunctionUse == 2)
                return false;
            return true;
        }
        
        /// <summary>
        /// detonates all entropic crystals owned by the player.
        /// 6/9/2025, 7:58 AM EST
        /// </summary>
        private void TriggleExplosion(Player player)
        {
            
            if (!player.dead)
            {
                for (int i = 0; i < player.ownedProjectileCounts[ModContent.ProjectileType<EntropicCrystal>()]; i++)
                    
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

    }
    public class NoxusWeaponPlayer: ModPlayer
    {
        /// <summary>
        /// track the amount of crystals currently lodged in NPCs for the purposes of crit scaling and maybe some other fun things
        /// </summary>
        public int CrystalCount
        {
            get;
            set;
        }

      
        public int CrystalCap = 100;
        public float CrystalSpeedMulti = 1;


        public override void PostUpdateMiscEffects()
        {
            CrystalCount = 0;

            // filter only “live” crystals
            var liveCrystals = Main.projectile
                .Where(p =>
                    p.active &&
                    p.type == ModContent.ProjectileType<EntropicCrystal>() &&
                    p.owner == Player.whoAmI &&
                    p.ai[1] != (float)EntropicCrystalState.PreHit &&
                    p.ai[1] != (float)EntropicCrystalState.DisipateHarmlessly)
                .ToList();

            // how many are over-cap?
            int over = Math.Max(0, liveCrystals.Count - CrystalCap);

           
            if (over > 0)
            {
                var toExplode = liveCrystals.OrderBy(p => p.timeLeft).Take(over);

                foreach (var proj in toExplode)
                {
                    proj.ai[1] = (float)EntropicCrystalState.Exploding;
                    proj.netUpdate = true;
                }
            }

            float interp = Math.Clamp(liveCrystals.Count / 16f, 0f, 2f);
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
        public bool ChokingOnFumes
        {
            get;
            set;
        }
        public override bool InstancePerEntity => true;
        
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (ChokingOnFumes)
            {

            }
        }
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (ChokingOnFumes)
            {               
                for (int i = 0; i < 2; i++)
                {
                    Vector2 spawnPosition = npc.Center + Main.rand.NextVector2Circular(20f, 20f);
                    Vector2 velocity = Main.rand.NextVector2Circular(1f, 1f);
                    Color color = Color.Lerp(Color.Purple, Color.Black, Main.rand.NextFloat(0.5f));
                    NoxusGasMetaball.CreateParticle(spawnPosition, velocity, Main.rand.NextFloat(10f, 30f));
                }
            }
        }
    }
    
    class EntropicBlast : ModProjectile
    {
        #region setup
        public ref Player Owner => ref Main.player[Projectile.owner];
        private Vector2 SpawnPos;
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

        private float FadeInterp;
        public byte Fadeout = 255;

        private bool PortalOpen = true;
        private float portalInterp = 0f;
        private bool HasHit;
        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.tileCollide = false;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.penetrate = - 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = 240;
            Projectile.damage = NoxusWeapon.AltDamage;
            Projectile.width = Projectile.height = 60;
            
            
        }
        #endregion
        public override bool? CanDamage() {
            if (Projectile.ai[1] == 0) 
                return false; 
            else 
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
            portalInterp =(float)Math.Clamp(Math.Round(MathHelper.Lerp(portalInterp, PortalOpen ? 1.1f : -0.1f, PortalOpen ? 0.3f : 0.05f),2),0,1);
            if (Crystal.active == false)
            {
                PortalOpen = false;
            }
            else
            {
                //todo: set the spawn pos and normal velocity to be offest in comparison to the crystal, while still allowing the projectile to move under its own power.
                

            }
            if(portalInterp == 0.5f)
            {
                
            }
            if (portalInterp == 0 && PortalOpen == false)
            {
                Projectile.Kill();
            }
            //Main.NewText($"Portal: {PortalOpen}, Interpolant: {portalInterp}");
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
            /*
            if(Time== 1) 
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), SpawnPos,
                Projectile.rotation.ToRotationVector2() * 40, ModContent.ProjectileType<EntropicComet>(),
                NoxusWeapon.AltDamage,0,default, default,default, Crystal.whoAmI);*/

            if (Time == 1)
            {
                for (int i = 0; i < 20; i++)
                    NoxusGasMetaball.CreateParticle(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), Projectile.velocity.RotatedByRandom(0.68f) * Main.rand.NextFloat(0.9f), Main.rand.NextFloat(13f, 56f));

                SoundEngine.PlaySound(SoundID.Item103, Projectile.Center);
                SoundEngine.PlaySound(GennedAssets.Sounds.Common.Glitch with { MaxInstances = 0, Volume = 0.1f, PitchVariance = 0.4f });
                //SoundEngine.PlaySound(GennedAssets.Sounds.Common.EarRinging with { MaxInstances = 0, Volume = 0.05f });
                float screenshakePower = 1;//MathHelper.Lerp(Owner.GetModPlayer<NoxusWeaponPlayer>().CrystalCount, 10, 3);
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
                float slitherOffset = (float)Math.Sin(Time / 6.4f + Projectile.identity) * Utils.GetLerpValue(10f, 25f, Time, true) * 6.2f;
                Vector2 perpendicularDirection = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
                Projectile.Center += perpendicularDirection * slitherOffset;

                Projectile.rotation = Projectile.velocity.ToRotation();


                for (int i = 0; i < 4; i++)
                {
                    float gasSize = Utils.GetLerpValue(-3f, 25f, Time, true) * Projectile.width * 0.68f;
                    float angularOffset = (float)Math.Sin(Time / 5f) * 0.77f;
                    NoxusGasMetaball.CreateParticle(Projectile.Center + Projectile.velocity * 2f, Main.rand.NextVector2Circular(2f, 2f) + Projectile.velocity.RotatedBy(angularOffset).RotatedByRandom(0.6f) * 0.26f, gasSize);
                }
                // Spawn particles.
                for (int i = 0; i < 20; i++)
                {
                    Color voidColor = Color.Lerp(Color.Purple, Color.Black, Main.rand.NextFloat(0.54f, 0.9f));
                    voidColor = Color.Lerp(voidColor, Color.DarkBlue, Main.rand.NextFloat(0.4f));
                    HeavySmokeParticle darkGas = new(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), Main.rand.NextVector2Circular(1f, 1f), voidColor, 11, Projectile.scale * 1.24f, Projectile.Opacity * 0.6f, Main.rand.NextFloat(0.02f), true);
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
                int npcIndex = (int)Crystal.ai[2] - 1;
                if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
                {
                    NPC targetNpc = Main.npc[npcIndex];
                    if (targetNpc != null && targetNpc.active)
                    {
                        Projectile.rotation = (targetNpc.Center - Projectile.Center).ToRotation();
                        SpawnPos = Offset + targetNpc.Center;
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
            if(Time == 1)
            {
                NoxusPortal darkParticle = NoxusPortal.pool.RequestParticle();
                darkParticle.Prepare(SpawnPos, Projectile.velocity, Color.AntiqueWhite, Projectile.rotation, Projectile.timeLeft, portalInterp,Projectile);
                ParticleEngine.Particles.Add(darkParticle);
            }
            if (Time > 60)
            {
                for (int i = 0; i < 2; i++)
                {
                    Color voidColor = Color.Lerp(Color.Purple, Color.Black, Main.rand.NextFloat(0.54f, 0.9f));
                    voidColor = Color.Lerp(voidColor, Color.DarkBlue, Main.rand.NextFloat(0.4f));
                    HeavySmokeParticle darkGas = new(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), Main.rand.NextVector2Circular(1f, 1f), voidColor, 11, Projectile.scale * 1.24f, Projectile.Opacity * 0.6f, Main.rand.NextFloat(0.02f), true);
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

            ManagedShader PortalShader = ShaderManager.GetShader("HeavenlyArsenal.PortalShader");

            PortalShader.TrySetParameter("circleStretchInterpolant", portalInterp);
            PortalShader.TrySetParameter("transformation", (Matrix.CreateScale(10f, 2f, 2f)));
            //PortalShader.TrySetParameter("aimDirection", Projectile.rotation + MathHelper.PiOver2);
            PortalShader.TrySetParameter("uColor", Color.MediumPurple with { A = 255});
            //PortalShader.TrySetParameter("uSecondaryColor", Color.White);
            PortalShader.TrySetParameter("edgeFadeInSharpness", 20.3f);
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
            float maxScale = 5f;
            Vector2 textureArea = Projectile.Size / pixel.Size() * maxScale;
            float scaleMod = 1f + (float)(Math.Cos(Main.GlobalTimeWrappedHourly * 15f + Projectile.identity) * 0.012f);
            textureArea *= scaleMod;

            Main.spriteBatch.Draw(pixel, DrawPos, null, Projectile.GetAlpha(Color.MediumPurple), Projectile.rotation, pixel.Size() * 0.5f, textureArea, 0, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            

        }
        public void DrawBolt(Vector2 DrawPos)
        {
            Color Drawcolor = Color.Purple with { A = Fadeout };
            Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/DarkComet").Value;
            Main.EntitySpriteDraw(texture, DrawPos, null, Drawcolor, Projectile.rotation, texture.Size() * 0.5f, 1f, SpriteEffects.None);
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Drawcolor);

        }
        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 PortalDraw = SpawnPos - Main.screenPosition; 
           // if(portalInterp > 0)
           // DrawPortal(PortalDraw);

            //Utils.DrawBorderString(Main.spriteBatch, "Interp: " + portalInterp.ToString() + " | Pos: " + SpawnPos.ToString(), PortalDraw - Vector2.UnitY * 110, Color.AntiqueWhite, 1);
            
            if(Projectile.ai[1] == (float)BlastStage.Bolt) 
                DrawBolt(Projectile.Center - Main.screenPosition);

            return false;
        }
    }
   
}

