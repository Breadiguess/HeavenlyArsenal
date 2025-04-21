using System;
using System.Collections.Generic;
using System.Linq;
using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.Graphics.Primitives;
using CalamityMod.World;
using HeavenlyArsenal.Content.Buffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Ranged
{
    public class PossibilitySeed_Laser : ModProjectile, ILocalizedModType
    {

        public Player Owner => Main.player[Projectile.owner];

        public ref float LaserLength => ref Projectile.ai[1];
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public const float MaxLaserLength = 3330f;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.hide = true;
            Projectile.timeLeft = 230;
        }

        public ref float OwnerIndex => ref Projectile.ai[0];

        public Vector2 rot
        {
            get
            {
                // Smoothly interpolate the rotation vector to avoid snapping
                Vector2 currentRot = Projectile.rotation.ToRotationVector2();
                Vector2 targetRot = Main.projectile[(int)(OwnerIndex)].rotation.ToRotationVector2();
                return Vector2.Lerp(currentRot, targetRot, 0.1f); // Adjust the lerp factor (0.1f) for desired smoothness
            }
        }
        //todo: lerp this so that it doesnt instantly snap to a location
        
        public override void OnSpawn(IEntitySource source)
        {
            if (source is EntitySource_Parent parentSource && parentSource.Entity is Projectile parentProjectile && parentProjectile.type == ModContent.ProjectileType<PossibilitySeed>())
            {
                OwnerIndex = parentProjectile.whoAmI;
                Main.NewText($"i was made by a possibility seed!");
            }
            base.OnSpawn(source);
        }
        public override void OnKill(int timeLeft)
        {
            Main.NewText($"Laser {Projectile.whoAmI} was killed. time left: {timeLeft}");
            base.OnKill(timeLeft);
        }
        public override void AI()
        {
            // If the owner is no longer able to cast the beam, kill it.
            //Projectile.rotation += 10;
            Projectile.rotation = rot.ToRotation();
            
            //WHY NOT
            Main.NewText($"{Projectile.whoAmI} rotation: {Projectile.rotation}");
            // Grow bigger up to a point.
            Projectile.scale = MathHelper.Clamp(Projectile.scale + 0.15f, 0.05f, 2f);

           
            float[] laserLengthSamplePoints = new float[24];
            Collision.LaserScan(Projectile.Center, rot, Projectile.scale * 8f, MaxLaserLength, laserLengthSamplePoints);
            LaserLength = laserLengthSamplePoints.Average();

            // Update aim.
            // UpdateAim();

            // Adjust damage every frame. This is necessary to ensure that mana sickness and such are applied.
            //Projectile.damage = (int)Owner.GetTotalDamage<MagicDamageClass>().ApplyTo(MagicCircle.damage);

            // Create arms on surfaces.
            if (Main.myPlayer == Projectile.owner && Main.rand.NextBool(8))
                CreateDustOnSurfaces();

            // Create hit effects at the end of the beam.
            /*
            if (Main.myPlayer == Projectile.owner)
                CreateTileHitEffects();
            */
            // Make the beam cast light along its length. The brightness of the light is reliant on the scale of the beam.
            
            DelegateMethods.v3_1 = Color.DarkViolet.ToVector3() * Projectile.scale * 0.4f;
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + rot * LaserLength, Projectile.width * Projectile.scale, DelegateMethods.CastLight);
        }

        //this will be pointless later on. 
        public void UpdateAim()
        {
            // Only execute the aiming code for the owner.
            if (Main.myPlayer != Projectile.owner)
                return;

            Vector2 newAimDirection = Projectile.velocity.SafeNormalize(Vector2.UnitY);

            // Sync if the direction is different from the old one.
            // Spam caps are ignored due to the frequency of this happening.
            if (newAimDirection != Projectile.velocity)
            {
                Projectile.netUpdate = true;
                Projectile.netSpam = 0;
            }

            Projectile.velocity = newAimDirection;
        }

        public void CreateDustOnSurfaces()
        {
            Vector2 rot = Projectile.rotation.ToRotationVector2();
            Vector2 endOfLaser = Projectile.Center + rot * LaserLength + Main.rand.NextVector2Circular(80f, 8f);
            Vector2 idealCenter = endOfLaser;
            if (WorldUtils.Find(idealCenter.ToTileCoordinates(), Searches.Chain(new Searches.Down(5), new CustomConditions.SolidOrPlatform()), out Point result))
            {
                idealCenter = result.ToWorldCoordinates();
            }
            Point endOfLaserTileCoords = idealCenter.ToTileCoordinates();
            Tile endTile = CalamityUtils.ParanoidTileRetrieval(endOfLaserTileCoords.X, endOfLaserTileCoords.Y);

            if (endTile.HasUnactuatedTile && (Main.tileSolid[endTile.TileType] || Main.tileSolidTop[endTile.TileType]) && !endTile.IsHalfBlock && endTile.Slope == 0)
            {
                Vector2 armSpawnPosition = endOfLaserTileCoords.ToWorldCoordinates();
                //Projectile.NewProjectile(Projectile.GetSource_FromThis(), armSpawnPosition, Vector2.Zero, ModContent.ProjectileType<RancorArm>(), Projectile.damage * 2 / 3, 0f, Projectile.owner);
            }
        }


        private float PrimitiveWidthFunction(float completionRatio) => Projectile.scale * 65;

        private Color PrimitiveColorFunction(float completionRatio)
        {
            Color vibrantColor = Color.Lerp(Color.DarkGoldenrod, Color.AntiqueWhite, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 0.67f - completionRatio / LaserLength * 29f) * 0.5f + 0.5f);
            float opacity = Projectile.Opacity * Utils.GetLerpValue(0.97f, 0.9f, completionRatio, true) *
                Utils.GetLerpValue(0f, MathHelper.Clamp(15f / LaserLength, 0f, 0.5f), completionRatio, true) *
                (float)Math.Pow(Utils.GetLerpValue(60f, 270f, LaserLength, true), 3D);
            return Color.Lerp(vibrantColor, Color.Bisque, 0.5f) * opacity * 2f;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            //GameShaders.Misc["CalamityMod:Flame"].UseImage0("Assets/Textures/Extra/Iridescence");
            GameShaders.Misc["CalamityMod:Flame"].UseImage1("Images/Misc/Perlin");

            Vector2[] basePoints = new Vector2[24];
            for (int i = 0; i < basePoints.Length; i++)
                basePoints[i] = Projectile.Center + rot * i / (basePoints.Length - 1f) * LaserLength;

            PrimitiveRenderer.RenderTrail(basePoints, new(PrimitiveWidthFunction, PrimitiveColorFunction, shader: GameShaders.Misc["CalamityMod:Flame"]), 92);
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + rot * LaserLength);
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overWiresUI.Add(index);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<RancorBurn>(), 150);
            target.AddBuff(ModContent.BuffType<AntimatterAnnihilationAll_Buff>(), 150);        
        }

        public override bool ShouldUpdatePosition() => false;
    }
}