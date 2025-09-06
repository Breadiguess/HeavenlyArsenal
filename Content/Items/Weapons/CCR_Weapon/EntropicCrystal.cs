using CalamityMod.Particles;
using HeavenlyArsenal.Content.Particles.Metaballs.NoxusGasMetaball;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.Items.Weapons.CCR_Weapon
{
    class EntropicCrystal : ModProjectile
    {
        #region setup
        
        public float ShatterChance;
        public float HomeResource;
        public Entity StuckNPC
        {
            get;
            set;
        }
        public Vector2 StuckOffset;
        public enum EntropicCrystalState
        {
            PreHit,
            PostHit,
            Exploding,
            DisipateHarmlessly
            //Shatter
        }
        public bool Toxic = false;
        public ref Player Owner => ref Main.player[Projectile.owner];
        public ref float Time => ref Projectile.ai[0];
        //store in ai[1] to avoid bugs
        public EntropicCrystalState CurrentState => (EntropicCrystalState)Projectile.ai[1];
        //just for fun
        public ref float StoredNPC => ref Projectile.ai[2];
        public ref float CrystalFrame => ref Projectile.localAI[1];
        public override string Texture => "HeavenlyArsenal/Content/Items/Weapons/CCR_Weapon/EntropicCrystal";
        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.width = Projectile.height = 30;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 180;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;

        }
        public override void SetStaticDefaults()
        {
        }

        public override void OnSpawn(IEntitySource source)
        {
            HomeResource = 60;
            CrystalFrame = Main.rand.Next(0, 4);
            ShatterChance = (float)Math.Round(Main.rand.NextFloat(0, 1.000001f),4);
            float gasSize = Utils.GetLerpValue(-3f, 25f, 10, true) * Projectile.width * 0.68f;
            float angularOffset = (float)Math.Sin(Time / 5f) * 0.77f;
            NoxusGasMetaball.CreateParticle(Projectile.Center + Projectile.velocity * 2f, Main.rand.NextVector2Circular(2f, 2f) + Projectile.velocity.RotatedBy(angularOffset).RotatedByRandom(0.6f) * 0.26f, gasSize);

        }
        #endregion
        public override void AI()
        {
            
            
            HandleState();
            
            Time++;


        }
        private void HandleState()
        {
            switch (CurrentState)
            {
                case EntropicCrystalState.PreHit:
                    HandlePreHit();
                    break;
                case EntropicCrystalState.PostHit:
                    HandlePostHit();
                    break;
                case EntropicCrystalState.Exploding:
                    HandlePostHit();
                    HandleExplosion();
                    break;
                case EntropicCrystalState.DisipateHarmlessly:
                    HandleDisipateHarmlessly();
                    break;
            }
        }
        /// <summary>
        /// this is the crystal before hitting anything. its just falling from the sky right now, and isn't doing all too much
        /// </summary>
        private void HandlePreHit()
        {
            
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            // Integrate HomeResource to prevent excessive homing by limiting the total amount of "turning" the projectile can do
            float homingRange = 400f;
            float homingStrength = 0.12f;
            NPC closestNPC = null;
            float closestDist = homingRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy(Projectile))
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestNPC = npc;
                    }
                }
            }
            if (closestNPC != null && HomeResource > 0f)
            {
                Vector2 desiredVelocity = (closestNPC.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * Projectile.velocity.Length();
                // Calculate the angle between current and desired velocity
                float angleDiff = Vector2.Dot(Projectile.velocity.SafeNormalize(Vector2.Zero), desiredVelocity.SafeNormalize(Vector2.Zero));
                angleDiff = Math.Clamp(angleDiff, -1f, 1f);
                float turnAmount = (float)Math.Acos(angleDiff);
                // Reduce HomeResource by the amount of turning done
                float turnCost = turnAmount * 20f; // Arbitrary scaling factor for resource usage
                if (HomeResource >= turnCost)
                {
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, homingStrength);
                    HomeResource -= turnCost;
                }
                else
                {
                    // Not enough resource to turn fully, so only turn partially
                    float partialStrength = HomeResource / turnCost * homingStrength;
                    //Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, partialStrength);
                    HomeResource = 0f;
                }
            }
        }
        /// <summary>
        /// this is the crystal after hitting something. it embeds itself in the target, and deals DOT while waiting for an explosion call.
        /// </summary>
        private void HandlePostHit()
        {
            Projectile.timeLeft = 4;
            // Stick to the hit NPC
            Projectile.velocity = Vector2.Zero;
            if (Projectile.ai[2] == 0)
            {
                if (StuckNPC != null)
                    Projectile.ai[2] = StuckNPC.whoAmI + 1;
                Projectile.netUpdate = true;
            }
            int npcIndex = (int)Projectile.ai[2] - 1;
            if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
            {
                NPC stuckNpc = Main.npc[npcIndex];
                if (stuckNpc.active && !stuckNpc.dontTakeDamage && !stuckNpc.friendly)
                {
                    Projectile.Center = StuckNPC.Center + StuckOffset;
                }
                else
                {
                    // If NPC is dead or invalid, dissipate harmlessly
                    Projectile.ai[1] = (float)EntropicCrystal.EntropicCrystalState.DisipateHarmlessly;
                    Projectile.netUpdate = true;
                }
            }


            if (Toxic)
            {
                if (Time == 60 * 3 - 1)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Color voidColor = Color.Lerp(Color.Purple, Color.Black, Main.rand.NextFloat(0.54f, 0.9f));
                        voidColor = Color.Lerp(voidColor, Color.DarkBlue, Main.rand.NextFloat(0.4f));
                        HeavySmokeParticle darkGas = new(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), Main.rand.NextVector2Circular(1f, 1f), voidColor, 11, Projectile.scale * 1.24f, Projectile.Opacity * 0.6f, Main.rand.NextFloat(0.02f), true);
                        GeneralParticleHandler.SpawnParticle(darkGas);
                    }
                }
                if (Time > 60 * 3)
                {
                    Time = 0;
                    if (Main.rand.NextBool(1))
                    {
                        float gasSize = Utils.GetLerpValue(-3f, 25f, Time, true) * Projectile.width * 0.68f;
                        float angularOffset = -Projectile.rotation;
                        NoxusGasMetaball.CreateParticle(Projectile.Center + Projectile.velocity * 2f, Main.rand.NextVector2Circular(2f, 2f) + Projectile.velocity.RotatedBy(angularOffset).RotatedByRandom(0.6f) * 0.26f, gasSize);
                    }
                    Main.npc[npcIndex].SimpleStrikeNPC(400, 0, false, 0, default, false, default, default);

                }



            }
        }
        /// <summary>
        /// ended up splitting this into two projectiles. this just creates the portal.
        /// </summary>
        private void HandleExplosion()
        {
            int Crystals = 4 * Owner.GetModPlayer<NoxusWeaponPlayer>().CrystalCount;
            //todo: manipulate the offset so that each portal is placed perfectly spaced around the StuckNPC.
            // if there are too many portals, and they'd have to be too far away from the target to hit them, create an inner ring of portals that will be placed closer to the stuck npc.
            // this can happen as many times as necessary.
            Vector2 offset = Main.rand.NextVector2CircularEdge(200 + Crystals, 200 + Crystals);
            // Only create one portal per crystal
            if (!Projectile.localAI[0].Equals(1f))
            {
                Projectile.localAI[0] = 1f;
                int portal = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center + offset,
                    Vector2.Zero,
                    ModContent.ProjectileType<EntropicBlast>(),
                    Projectile.damage,
                    0,
                    Projectile.owner
                );
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void HandleDisipateHarmlessly()
        {
            for (int i = 0; i < 2; i++)
            {
                Color voidColor = Color.Lerp(Color.Purple, Color.Black, Main.rand.NextFloat(0.54f, 0.9f));
                voidColor = Color.Lerp(voidColor, Color.DarkBlue, Main.rand.NextFloat(0.4f));
                HeavySmokeParticle darkGas = new(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), Main.rand.NextVector2Circular(1f, 1f), voidColor, 11, Projectile.scale * 1.24f, Projectile.Opacity * 0.6f, Main.rand.NextFloat(0.02f), true);
                GeneralParticleHandler.SpawnParticle(darkGas);
            }
            Projectile.alpha--;
            if (Projectile.alpha >= 0)
                Projectile.Kill();
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.Common.TwinkleMuffled with { Pitch = 0.3f, PitchVariance = 0.4f });
            Projectile.ai[1] = (float)EntropicCrystalState.PostHit;
            Projectile.timeLeft = 180;
            StuckNPC = target;
            StuckOffset = Projectile.Center -target.Center;
            Toxic = true;
            if (!Main.rand.NextBool(ShatterChance))
            {
                target.GetGlobalNPC<NoxusWeaponNPC>().AttachedCrystalCount++;
                
            }
            else
            {
                Projectile.active = false;
            }



                Projectile.netUpdate = true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            int FrameCount = 4;

            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 ScaleEffects = new Vector2(0.9f, 1);

            Rectangle Frame = new Rectangle(0, (int)CrystalFrame * texture.Height/FrameCount, texture.Width, texture.Height / FrameCount);
            Vector2 origin = new Vector2(texture.Width / 2f, (texture.Height / FrameCount) / 1.3f);


            float wind = AperiodicSin(Main.GlobalTimeWrappedHourly * 0.1f + Projectile.Center.X + Projectile.Center.Y) * 0.333f;
            if (Toxic)
            { 
                float thing = MathHelper.SmoothStep(0.8f, 1f, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 0.1f));
                ScaleEffects = new Vector2(thing, 1);
            }
            else
            {
                ScaleEffects = new Vector2 (0.9f, 1);
                wind = 0f;
            }

            Main.EntitySpriteDraw(texture, drawPos, Frame, lightColor, Projectile.rotation + wind, origin, ScaleEffects, effects);


            //attempt blur  
            
            if (Projectile.ai[1] == (float)EntropicCrystalState.PreHit)
            {
                //draw several times faded or have a shader that does the same thing
                for (int i = 0; i < 40; i++)
                {
                    Vector2 blurOffset = (Projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * (i * 2 - 10);
                    Color fadedColor = lightColor * (1f - (i * 0.2f));
                    Main.EntitySpriteDraw(texture, drawPos +  blurOffset, Frame, fadedColor, Projectile.rotation + wind, origin, ScaleEffects, effects);
                    //Utils.DrawBorderString(Main.spriteBatch, blurOffset.ToString(), drawPos + blurOffset - Vector2.UnitY * 10*i, Color.AntiqueWhite);

                }
            }

           
            //Utils.DrawBorderString(Main.spriteBatch, CrystalFrame.ToString(), drawPos - Vector2.UnitY * 100, Color.AntiqueWhite);
            return false;
        }
    }
}
